﻿using OpenIZ.Mobile.Core.Diagnostics;
using SQLite.Net;
using SQLite.Net.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenIZ.Mobile.Core.Data.Connection
{
    /// <summary>
    /// SQLiteConnectionManager
    /// </summary>
    public class SQLiteConnectionManager
    {


        // connections
        private Dictionary<String, SQLiteConnectionWithLock> m_connections = new Dictionary<string, SQLiteConnectionWithLock>();

        // Tracer
        private Tracer m_tracer = Tracer.GetTracer(typeof(SQLiteConnectionManager));
        // lock
        private static object s_lockObject = new object();
        // instance singleton
        private static SQLiteConnectionManager s_instance = null;

        /// <summary>
        /// Gets the current connection manager
        /// </summary>
        public static SQLiteConnectionManager Current
        {
            get
            {
                if (s_instance== null)
                    lock (s_lockObject)
                        if (s_instance == null)
                            s_instance = new SQLiteConnectionManager();
                return s_instance;
            }
        }

        ///// <summary>
        ///// Release the connection
        ///// </summary>
        ///// <param name="databasePath"></param>
        //public void ReleaseConnection(string databasePath)
        //{
        //    Object lockObject = null;
        //    if (!this.m_locks.TryGetValue(databasePath, out lockObject))
        //        return;
        //    else
        //        Monitor.Exit(lockObject);
        //}

        /// <summary>
        /// SQLLiteConnection manager
        /// </summary>
        private SQLiteConnectionManager()
        {

        }

        /// <summary>
        /// Get connection to the datafile
        /// </summary>
        public SQLiteConnectionWithLock GetConnection(String dataSource)
        {
            SQLiteConnectionWithLock conn = null;

            try
            {
                if (!this.m_connections.TryGetValue(dataSource, out conn))
                    lock (s_lockObject)
                        if (!this.m_connections.TryGetValue(dataSource, out conn))
                        {

                            ISQLitePlatform platform = ApplicationContext.Current.GetService<ISQLitePlatform>();
                            conn = new SQLiteConnectionWithLock(platform, new SQLiteConnectionString( dataSource,true));
                            this.m_connections.Add(dataSource, conn);
                            this.m_tracer.TraceInfo("Connection to {0} established, {1} active connections", dataSource, this.m_connections.Count);
#if DEBUG_SQL
                conn.TraceListener = new TracerTraceListener();
#endif
                        }
                return conn;
            }
            catch(Exception e)
            {
                this.m_tracer.TraceError("Error getting connection: {0}", e);
                if (conn != null)
                    Monitor.Exit(conn);
                throw;
            }
        }
    }

    /// <summary>
    /// Tracer trace listener
    /// </summary>
    internal class TracerTraceListener : ITraceListener
    {
        // Tracer
        private Tracer m_tracer = Tracer.GetTracer(typeof(SQLiteConnectionWithLock) );

        /// <summary>
        /// Trace info to console
        /// </summary>
        public void Receive(string message)
        {
            this.m_tracer.TraceVerbose(message);
        }
    }
}