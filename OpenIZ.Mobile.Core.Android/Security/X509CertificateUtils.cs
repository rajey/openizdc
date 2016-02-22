﻿using System;
using System.Security.Cryptography.X509Certificates;
using OpenIZ.Mobile.Core.Configuration;
using OpenIZ.Mobile.Core.Diagnostics;

namespace OpenIZ.Mobile.Core.Android.Security
{
	/// <summary>
	/// Utilities for x509 certificates
	/// </summary>
	public static class X509CertificateUtils
	{

		private static Tracer s_tracer = TracerHelper.GetTracer(typeof(X509CertificateUtils));

		/// <summary>
		/// Find configuration value from configuration values
		/// </summary>
		public static X509Certificate2 FindCertificate(ServiceCertificateConfiguration config)
		{
			return FindCertificate (config.FindType, config.StoreLocation, config.StoreName, config.FindValue);
		}

		/// <summary>
		/// Find a certiifcate from string values
		/// </summary>
		/// <returns>The certificate.</returns>
		/// <param name="findType">Find type.</param>
		/// <param name="storeLocation">Store location.</param>
		/// <param name="storeName">Store name.</param>
		/// <param name="findValue">Find value.</param>
		public static X509Certificate2 FindCertificate(
			String findType,
			String storeLocation,
			String storeName,
			String findValue)
		{
			X509FindType eFindType = X509FindType.FindByThumbprint;
			StoreLocation eStoreLocation = StoreLocation.CurrentUser;
			StoreName eStoreName = StoreName.My;

			if (!Enum.TryParse (findType, out eFindType))
				s_tracer.TraceWarning ("{0} not valid value for {1}, using {2} as default", findType, eFindType.GetType().Name, eFindType);
			
			if(!Enum.TryParse (storeLocation, out eStoreLocation))
				s_tracer.TraceWarning ("{0} not valid value for {1}, using {2} as default", storeLocation, eStoreLocation.GetType().Name, eStoreLocation);

			if(!Enum.TryParse (storeName, out eStoreName))
				s_tracer.TraceWarning ("{0} not valid value for {1}, using {2} as default", storeName, eStoreName.GetType().Name, eStoreName);

			return FindCertificate (eFindType, eStoreLocation, eStoreName, findValue);
		}

		/// <summary>
		/// Find the specified certificate
		/// </summary>
		/// <returns>The certificate.</returns>
		/// <param name="findType">Find type.</param>
		/// <param name="storeLocation">Store location.</param>
		/// <param name="storeName">Store name.</param>
		/// <param name="findValue">Find value.</param>
		public static X509Certificate2 FindCertificate(
			X509FindType findType,
			StoreLocation storeLocation,
			StoreName storeName,
			String findValue
		)
		{
			X509Store store = new X509Store(storeName, storeLocation);
			try {
				store.Open(OpenFlags.ReadOnly);
				var matches = store.Certificates.Find(findType, findValue, true);
				if(matches.Count == 0)
					throw new InvalidOperationException("Certificate not found");
				else if(matches.Count > 1)
					throw new InvalidOperationException("Too many candidate certificates found");
				else
					return matches[0];
			} catch (Exception ex) {
				s_tracer.TraceError (ex.ToString ());
				throw;
			}
			finally {
				store.Close ();
			}
		}

	}
}

