﻿/*
 * Copyright 2015-2016 Mohawk College of Applied Arts and Technology
 * 
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 * User: justi
 * Date: 2016-6-14
 */
using System;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using OpenIZ.Mobile.Core.Configuration;
using OpenIZ.Mobile.Core.Diagnostics;
using OpenIZ.Mobile.Core.Xamarin.Security;
using System.Security;
using System.IO.Compression;
using OpenIZ.Core.Http;
using System.IO;
using OpenIZ.Core.Model;
using OpenIZ.Core.Model.EntityLoader;
using OpenIZ.Mobile.Core.Xamarin.Resources;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace OpenIZ.Mobile.Core.Xamarin.Http
{
	/// <summary>
	/// Represents an android enabled rest client
	/// </summary>
	public class RestClient : RestClientBase
	{

		// Config section
		private ServiceClientConfigurationSection m_configurationSection;
		// Tracer
		private Tracer m_tracer;

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIZ.Mobile.Core.Xamarin.Http.RestClient"/> class.
		/// </summary>
		public RestClient () : base()
		{
			this.m_tracer = Tracer.GetTracer (this.GetType ());
			this.m_configurationSection = ApplicationContext.Current.Configuration.GetSection<ServiceClientConfigurationSection> ();

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIZ.Mobile.Core.Xamarin.Http.RestClient"/> class.
		/// </summary>
		public RestClient (ServiceClientDescription config) : base(config)
		{
			this.m_configurationSection = ApplicationContext.Current.Configuration.GetSection<ServiceClientConfigurationSection> ();
			this.m_tracer = Tracer.GetTracer (this.GetType ());
			// Find the specified certificate
			if (config.Binding.Security.ClientCertificate != null) {
				this.ClientCertificates = new X509Certificate2Collection ();
				var cert = X509CertificateUtils.FindCertificate (config.Binding.Security.ClientCertificate.FindType, 
					          config.Binding.Security.ClientCertificate.StoreLocation,
					          config.Binding.Security.ClientCertificate.StoreName,
					          config.Binding.Security.ClientCertificate.FindValue);
				if (cert == null)
					throw new SecurityException (String.Format("Certificate described by {0} could not be found in {1}/{2}", 
						config.Binding.Security.ClientCertificate.FindValue,
						config.Binding.Security.ClientCertificate.StoreLocation,
						config.Binding.Security.ClientCertificate.StoreName));
				this.ClientCertificates.Add (cert);
			}
		}

		/// <summary>
		/// Create HTTP Request object
		/// </summary>
		protected override WebRequest CreateHttpRequest (string url, params KeyValuePair<string, object>[] query)
		{
			var retVal = (HttpWebRequest)base.CreateHttpRequest (url, query);

			// Certs?
			if(this.ClientCertificates != null)
				retVal.ClientCertificates.AddRange(this.ClientCertificates);

			// Compress?
			if(this.Description.Binding.Optimize)
				retVal.Headers.Add (HttpRequestHeader.AcceptEncoding, "deflate,gzip");

            // Proxy?
            if (!String.IsNullOrEmpty(this.m_configurationSection.ProxyAddress))
                retVal.Proxy = new WebProxy(this.m_configurationSection.ProxyAddress);
			return retVal;
		}

        /// <summary>
        /// Invokes the specified method against the url provided
        /// </summary>
        /// <param name="method">Method.</param>
        /// <param name="url">URL.</param>
        /// <param name="contentType">Content type.</param>
        /// <param name="body">Body.</param>
        /// <param name="query">Query.</param>
        /// <typeparam name="TBody">The 1st type parameter.</typeparam>
        /// <typeparam name="TResult">The 2nd type parameter.</typeparam>
        protected override TResult InvokeInternal<TBody, TResult>(string method, string url, string contentType, Dictionary<HttpRequestHeader, String> additionalHeaders, TBody body, params KeyValuePair<string, object>[] query)
        {

            if (String.IsNullOrEmpty(method))
                throw new ArgumentNullException(nameof(method));
            if (String.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));

            // Three times:
            // 1. With provided credential
            // 2. With challenge
            // 3. With challenge again
            for (int i = 0; i < 2; i++)
            {
                // Credentials provided ?
                HttpWebRequest requestObj = this.CreateHttpRequest(url, query) as HttpWebRequest;
                if (!String.IsNullOrEmpty(contentType))
                    requestObj.ContentType = contentType;
                requestObj.Method = method;

                // Additional headers
                if (additionalHeaders != null)
                    foreach (var hdr in additionalHeaders)
                    {
                        if (hdr.Key == HttpRequestHeader.IfModifiedSince)
                            requestObj.IfModifiedSince = DateTime.Parse(hdr.Value);
                        else
                            requestObj.Headers.Add(hdr.Key, hdr.Value);
                    }
                
                // Body was provided?
                try
                {

                    // Try assigned credentials
                    IBodySerializer serializer = null;
                    if (body != null)
                    {
                        // GET Stream, 
                        Stream requestStream = null;
                        Exception requestException = null;
                        
                        //requestStream = requestObj.GetRequestStream();
                        var requestTask = requestObj.GetRequestStreamAsync().ContinueWith(r =>
                        {
                            if (r.IsFaulted)
                                requestException = r.Exception.InnerExceptions.First();
                            else
                                requestStream = r.Result;
                        });

                        if (!requestTask.Wait(this.Description.Endpoint[0].Timeout))
                        {
                            throw new TimeoutException();
                        }
                        else if (requestException != null) throw requestException;

                        if (contentType == null)
                            throw new ArgumentNullException(nameof(contentType));

                        serializer = this.Description.Binding.ContentTypeMapper.GetSerializer(contentType, typeof(TBody));
                        (body as IdentifiedData)?.SetDelayLoad(false);
                        // Serialize and compress with deflate
                        if (this.Description.Binding.Optimize)
                        {
                            requestObj.Headers.Add("Content-Encoding", "deflate");
                            using (var df = new DeflateStream(requestStream, CompressionMode.Compress))
                                serializer.Serialize(df, body);
                        }
                        else
                            serializer.Serialize(requestStream, body);

                    }

                    // Response
                    HttpWebResponse response = null;
                    Exception responseError = null;

                    var responseTask = requestObj.GetResponseAsync().ContinueWith(r => {
                        if (r.IsFaulted)
                            responseError = r.Exception.InnerExceptions.First();
                        else
                            response = r.Result as HttpWebResponse;
                    });
                    if (!responseTask.Wait(this.Description.Endpoint[0].Timeout))
                        throw new TimeoutException();
                    else if (responseError != null)
                    {
                        if (((responseError as WebException)?.Response as HttpWebResponse).StatusCode == HttpStatusCode.NotModified)
                            return default(TResult);
                        else
                            throw responseError;
                    }

                    var validationResult = this.ValidateResponse(response);
                    if (validationResult != ServiceClientErrorType.Valid)
                    {
                        this.m_tracer.TraceError("Response failed validation : {0}", validationResult);
                        throw new WebException(Strings.err_response_failed_validation, null, WebExceptionStatus.Success, response);
                    }
                    // De-serialize
                    var responseContentType = response.ContentType;
                    if (responseContentType.Contains(";"))
                        responseContentType = responseContentType.Substring(0, responseContentType.IndexOf(";"));

                    if (response.StatusCode == HttpStatusCode.NotModified)
                        return default(TResult);
                    serializer = this.Description.Binding.ContentTypeMapper.GetSerializer(responseContentType, typeof(TResult));

                    TResult retVal = default(TResult);
                    // Compression?
                    switch (response.Headers[HttpResponseHeader.ContentEncoding])
                    {
                        case "deflate":
                            using (DeflateStream df = new DeflateStream(response.GetResponseStream(), CompressionMode.Decompress))
                                retVal = (TResult)serializer.DeSerialize(df);
                            break;
                        case "gzip":
                            using (GZipStream df = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress))
                                retVal = (TResult)serializer.DeSerialize(df);
                            break;
                        default:
                            retVal = (TResult)serializer.DeSerialize(response.GetResponseStream());
                            break;
                    }

                    (retVal as IdentifiedData)?.SetDelayLoad(true);
                    return retVal;
                }
                catch(TimeoutException e)
                {
                    this.m_tracer.TraceError("Request timed out:{0}", e);
                    throw;
                }
                catch (WebException e)
                {

                    this.m_tracer.TraceError(e.ToString());

                    // status
                    switch (e.Status)
                    {
                        case WebExceptionStatus.ProtocolError:

                            // Deserialize
                            TResult result = default(TResult);
                            var errorResponse = (e.Response as HttpWebResponse);
                            var responseContentType = errorResponse.ContentType;
                            if (responseContentType.Contains(";"))
                                responseContentType = responseContentType.Substring(0, responseContentType.IndexOf(";"));
                            var serializer = this.Description.Binding.ContentTypeMapper.GetSerializer(responseContentType, typeof(TResult));
                            try
                            {
                                switch (errorResponse.Headers[HttpResponseHeader.ContentEncoding])
                                {
                                    case "deflate":
                                        using (DeflateStream df = new DeflateStream(errorResponse.GetResponseStream(), CompressionMode.Decompress))
                                            result = (TResult)serializer.DeSerialize(df);
                                        break;
                                    case "gzip":
                                        using (GZipStream df = new GZipStream(errorResponse.GetResponseStream(), CompressionMode.Decompress))
                                            result = (TResult)serializer.DeSerialize(df);
                                        break;
                                    default:
                                        result = (TResult)serializer.DeSerialize(errorResponse.GetResponseStream());
                                        break;
                                }
                            }
                            catch(Exception dse)
                            {
                                this.m_tracer.TraceError("Could not de-serialize error response! {0}", dse);
                            }

                            switch (errorResponse.StatusCode)
                            {
                                case HttpStatusCode.Unauthorized: // Validate the response
                                    if (this.ValidateResponse(errorResponse) != ServiceClientErrorType.Valid)
                                        throw new RestClientException<TResult>(
                                            result,
                                            e,
                                            e.Status,
                                            e.Response);

                                    break;
                                default:
                                    throw new RestClientException<TResult>(
                                        result,
                                        e,
                                        e.Status,
                                        e.Response);
                            }
                            break;
                        default:
                            throw;
                    }
                }
               
            }


            return default(TResult);
        }

		/// <summary>
		/// Gets or sets the client certificate
		/// </summary>
		/// <value>The client certificate.</value>
		public X509Certificate2Collection ClientCertificates {
			get;
			set;
		}
	}
}
