﻿using OpenIZ.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenIZ.Core.Data.Warehouse;
using System.Runtime.CompilerServices;
using Mono.Data.Sqlite;
using System.IO;
using OpenIZ.Core.Diagnostics;
using OpenIZ.Mobile.Core;
using System.Dynamic;
using Newtonsoft.Json;
using OpenIZ.Mobile.Core.Services;
using System.Reflection;
using System.Collections;
using OpenIZ.Core.Model.Query;
using System.Linq.Expressions;

namespace OpenIZ.Mobile.Core.Data.Warehouse
{
    /// <summary>
    /// Represents a simple SQLite ad-hoc data warehouse
    /// </summary>
    public class SQLiteDatawarehouse : IAdHocDatawarehouseService, IDaemonService, IDisposable
    {

        // Lock 
        private Object m_lock = new object();

        // Disposed
        private bool m_disposed = false;

        // Tracer
        private Tracer m_tracer = Tracer.GetTracer(typeof(SQLiteDatawarehouse));

        // Connection to the data-warehouse
        private SqliteConnection m_connection;

        public event EventHandler Starting;
        public event EventHandler Started;
        public event EventHandler Stopping;
        public event EventHandler Stopped;

        /// <summary>
        /// Gets the name of the data provider so callers can determine how to create stored queries
        /// </summary>
        public string DataProvider
        {
            get
            {

                return "sqlite";
            }
        }

        /// <summary>
        /// True if this is running
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return this.m_connection != null;
            }
        }

        /// <summary>
        /// Constructs the SQLite data warehouse file
        /// </summary>
        public SQLiteDatawarehouse()
        {
        }

        /// <summary>
        /// Initialize the database connection
        /// </summary>
        private void InitializeConnection(String dbFile)
        {
            try
            {
                this.m_tracer.TraceInfo("Connecting datawarehouse to {0}", dbFile);
                this.m_connection = new SqliteConnection(String.Format("Data Source={0}", dbFile));
                this.m_connection.Open();
            }
            catch (Exception e)
            {
                this.m_tracer.TraceError("Error starting data warehouse: {0}", e);
            }
        }

        /// <summary>
        /// Initialize the specified warehouse database
        /// </summary>
        private void InitializeDatabase(string dbFile)
        {
            lock (this.m_lock)
            {
                this.InitializeConnection(dbFile);

                // Initialize the warehouse
                using (var initStream = typeof(SQLiteDatawarehouse).GetTypeInfo().Assembly.GetManifestResourceStream("OpenIZ.Mobile.Core.Data.Warehouse.InitWarehouse.sql"))
                using (var sr = new StreamReader(initStream))
                {
                    var stmts = sr.ReadToEnd().Split(';').Select(o => o.Trim()).ToArray();
                    for (int i = 0; i < stmts.Length; i++)
                    {
                        var stmt = stmts[i];
                        if (String.IsNullOrEmpty(stmt)) continue;
                        this.m_tracer.TraceVerbose("EXECUTE: {0}", stmt);
                        using (var cmd = this.m_connection.CreateCommand())
                        {
                            cmd.CommandText = stmt;
                            cmd.CommandType = System.Data.CommandType.Text;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Throw an exception if the object is disposed
        /// </summary>
        private void ThrowIfDisposed()
        {
            if (this.m_disposed)
                throw new ObjectDisposedException(nameof(SQLiteDatawarehouse));
        }

        /// <summary>
        /// Add an object to the data
        /// </summary>
        /// <param name="datamartId"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public Guid Add(Guid datamartId, dynamic obj)
        {
            this.ThrowIfDisposed();
            lock (this.m_lock)
            {
                using (var tx = this.m_connection.BeginTransaction())
                    try
                    {
                        // Get the datamart
                        var mart = this.GetDatamart(datamartId);

                        Guid retVal = Guid.Empty;

                        if (obj is IEnumerable<dynamic>)
                            foreach (var itm in obj as IEnumerable<dynamic>)
                                retVal = this.InsertObject(tx, mart.Schema.Name, mart.Schema, itm);
                        else
                            retVal = this.InsertObject(tx, mart.Schema.Name, mart.Schema, obj);

                        tx.Commit();

                        return retVal;
                    }
                    catch (Exception e)
                    {
                        this.m_tracer.TraceError("Error while adding data to {0}: {1}", datamartId, e);
                        tx.Rollback();
                        throw;
                    }
            }
        }

        /// <summary>
        /// Insert specified object
        /// </summary>
        private Guid InsertObject(SqliteTransaction tx, String path, IDatamartSchemaPropertyContainer pcontainer, dynamic obj, Guid? scope = null)
        {
            // Conver to expando
            IDictionary<String, Object> tuple = new ExpandoObject();
            foreach (var pi in obj.GetType().GetProperties())
                tuple.Add(pi.Name, pi.GetValue(obj, null));
                 
            tuple.Add("uuid", Guid.NewGuid());
            tuple.Add("cont_id", scope);
            tuple.Add("extraction_time", DateTime.Now);

            // Create the properties
            List<Object> parameters = new List<object>() { tuple["uuid"] };
            if (scope != null)
                parameters.Add(scope);
            parameters.Add(tuple["extraction_time"]);

            foreach (var p in pcontainer.Properties.Where(o => o.Type != SchemaPropertyType.Object))
                parameters.Add(tuple[p.Name]);

            // Now time to store
            StringBuilder sbQuery = new StringBuilder("INSERT INTO ");
            sbQuery.Append(path);
            sbQuery.Append(" VALUES (");
            foreach (var itm in parameters)
                sbQuery.Append("?, ");
            sbQuery.Remove(sbQuery.Length - 2, 2);
            sbQuery.Append(")");

            // Database command
            using (var dbc = this.CreateCommand(tx, sbQuery.ToString(), parameters.ToArray()))
                dbc.ExecuteNonQuery();

            // Sub-properties
            foreach (var p in pcontainer.Properties.Where(o => o.Type == SchemaPropertyType.Object))
                this.InsertObject(tx, String.Format("{0}_{1}", path, p.Name), p, obj[p.Name], (Guid)tuple["uuid"]);

            return (Guid)tuple["uuid"];
        }

        /// <summary>
        /// Query against the specified object
        /// </summary>
        public IEnumerable<dynamic> AdhocQuery(Guid datamartId, dynamic queryParameters)
        {
            this.ThrowIfDisposed();
            int tr = 0;
            return this.AdhocQuery(datamartId, queryParameters, 0, 100, out tr);
        }

        public IEnumerable<dynamic> AdhocQuery(Guid datamartId, dynamic queryParameters, int offset, int count, out int totalResults)
        {
            this.ThrowIfDisposed();
            lock (this.m_lock)
            {
                try
                {
                    var mart = this.GetDatamart(datamartId);
                    if (mart == null)
                        throw new KeyNotFoundException(datamartId.ToString());

                    // Query paremeters
                    // Query paremeters
                    IDictionary<String, Object> parms = queryParameters as ExpandoObject;
                    if (queryParameters is String)
                        queryParameters = NameValueCollection.ParseQueryString(queryParameters as String);
                    if (queryParameters is NameValueCollection)
                    {
                        parms = new ExpandoObject();
                        foreach (var itm in (queryParameters as NameValueCollection))
                            parms.Add(itm.Key, itm.Value);
                    }
                    else
                    {
                        parms = new ExpandoObject();
                        foreach (var itm in queryParameters.GetType().GetProperties())
                            parms.Add(itm.Name, itm.GetValue(queryParameters, null));
                    }

                    return this.QueryInternal(mart.Schema.Name, parms, offset, count, out totalResults);
                }
                catch (Exception e)
                {
                    this.m_tracer.TraceError("Error executing {0} : {1}", queryParameters, e);
                    throw;
                }
            }
        }

        /// <summary>
        /// Create a datamart
        /// </summary>
        public DatamartDefinition CreateDatamart(string name, object schema)
        {
            this.ThrowIfDisposed();

            DatamartSchema dmSchema = schema as DatamartSchema;
            if (schema is ExpandoObject)
                dmSchema = JsonConvert.DeserializeObject<DatamartSchema>(JsonConvert.SerializeObject(schema));

            // Now create / register the data schema
            lock (this.m_lock)
            {
                using (var tx = this.m_connection.BeginTransaction())
                {
                    try
                    {
                        this.m_tracer.TraceInfo("Creating datamart {0}", name);

                        // Register the schema
                        dmSchema.Id = dmSchema.Id == default(Guid) ? Guid.NewGuid() : dmSchema.Id;
                        using (var dbc = this.CreateCommand(tx, "INSERT INTO dw_schemas VALUES (?, ?)", dmSchema.Id.ToByteArray(), dmSchema.Name))
                            dbc.ExecuteNonQuery();

                        this.CreateProperties(name, tx, dmSchema);

                        // Create mart
                        var retVal = new DatamartDefinition()
                        {
                            Id = Guid.NewGuid(),
                            Name = name,
                            CreationTime = DateTime.Now,
                            Schema = dmSchema
                        };
                        using (var dbc = this.CreateCommand(tx, "INSERT INTO dw_datamarts VALUES (?, ?, ?, ?)", retVal.Id.ToByteArray(), name, retVal.CreationTime, retVal.Schema.Id))
                            dbc.ExecuteNonQuery();

                        foreach (var itm in dmSchema.Queries)
                            this.CreateStoredQuery(retVal.Id, itm);

                        tx.Commit();
                        return retVal;
                    }
                    catch (Exception e)
                    {
                        this.m_tracer.TraceError("Error creating data mart {0} : {1}", name, e);
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Create properties for the specified container
        /// </summary>
        private void CreateProperties(String pathName, SqliteTransaction tx, IDatamartSchemaPropertyContainer container)
        {
            List<String> indexes = new List<string>();

            // Create the property container table
            StringBuilder createSql = new StringBuilder();
            createSql.AppendFormat("CREATE TABLE {0} (", pathName);
            createSql.Append("uuid blob(16) not null primary key, extraction_time datetime not null default current_timestamp, ");
            if (container is DatamartSchemaProperty)
                createSql.Append("entity_uuid blob(16) not null, ");

            // Create the specified dm_<<name>>_table
            foreach (var itm in container.Properties)
            {
                itm.Id = itm.Id == default(Guid) ? Guid.NewGuid() : itm.Id;
                using (var dbc = this.CreateCommand(tx, "INSERT INTO dw_properties VALUES (?, ?, ?, ?, ?)", itm.Id.ToByteArray(), container.Id.ToByteArray(), itm.Name, (int)itm.Type, (int)itm.Attributes))
                    dbc.ExecuteNonQuery();
                if (itm.Properties?.Count > 0)
                    this.CreateProperties(String.Format("{0}_{1}", pathName, itm.Name), tx, itm);

                var typeString = "";
                switch (itm.Type)
                {
                    case SchemaPropertyType.Binary:
                        typeString = "blob";
                        break;
                    case SchemaPropertyType.Boolean:
                        typeString = "boolean";
                        break;
                    case SchemaPropertyType.Date:
                        typeString = "datetime";
                        break;
                    case SchemaPropertyType.Decimal:
                        typeString = "decimal";
                        break;
                    case SchemaPropertyType.Float:
                        typeString = "float";
                        break;
                    case SchemaPropertyType.Integer:
                        typeString = "integer";
                        break;
                    case SchemaPropertyType.String:
                        typeString = "varchar(128)";
                        break;
                    case SchemaPropertyType.Uuid:
                        typeString = "blob(16)";
                        break;
                }
                // Add property to the table
                if (!String.IsNullOrEmpty(typeString))
                {
                    String attributeString = "";
                    if (itm.Attributes.HasFlag(SchemaPropertyAttributes.NotNull))
                        attributeString += "not null ";
                    if (itm.Attributes.HasFlag(SchemaPropertyAttributes.Unique))
                        attributeString += "unique ";
                    createSql.AppendFormat("\r\n\t{0} {1} {2},", itm.Name, typeString, attributeString);

                    if (itm.Attributes.HasFlag(SchemaPropertyAttributes.Indexed))
                        indexes.Add(String.Format("CREATE INDEX {0}_{1}_idx ON {0}({1})", pathName, itm.Name));
                }
            }

            createSql.Remove(createSql.Length - 1, 1);
            createSql.AppendFormat(")");

            // Now execute SQL create statement
            using (var dbc = this.CreateCommand(tx, createSql.ToString())) dbc.ExecuteNonQuery();
            foreach (var idx in indexes)
                using (var dbc = this.CreateCommand(tx, idx)) dbc.ExecuteNonQuery();

        }

        /// <summary>
        /// Create a command with the specified parameters
        /// </summary>
        private SqliteCommand CreateCommand(SqliteTransaction tx, string sql, params object[] parameters)
        {
            
            var retVal = this.m_connection.CreateCommand();
            retVal.Transaction = tx;
            retVal.CommandType = System.Data.CommandType.Text;
            retVal.CommandText = sql;
            foreach (var itm in parameters)
            {
                var parm = retVal.CreateParameter();
                parm.Value = itm;
                if (itm is String) parm.DbType = System.Data.DbType.String;
                else if (itm is DateTime || itm is DateTimeOffset) parm.DbType = System.Data.DbType.DateTime;
                else if (itm is Int32) parm.DbType = System.Data.DbType.Int32;
                else if (itm is Boolean) parm.DbType = System.Data.DbType.Boolean;
                else if (itm is byte[])
                {
                    parm.DbType = System.Data.DbType.Binary;
                    parm.Value = itm;
                }
                else if(itm is Guid || itm is Guid?)
                {
                    parm.DbType = System.Data.DbType.Binary;
                    if (itm != null)
                        parm.Value = ((Guid)itm).ToByteArray();
                    else parm.Value = DBNull.Value;
                }
                else if (itm is float || itm is double) parm.DbType = System.Data.DbType.Double;
                else if (itm is Decimal) parm.DbType = System.Data.DbType.Decimal;
                else if (itm == null)
                {
                    parm.Value = DBNull.Value;
                }
                retVal.Parameters.Add(parm);
            }
#if DEBUG
            this.m_tracer.TraceVerbose("Created SQL statement: {0}", retVal.CommandText);
            foreach (SqliteParameter v in retVal.Parameters)
                this.m_tracer.TraceVerbose(" --> [{0}] {1}", v.DbType, v.Value is byte[] ? BitConverter.ToString(v.Value as Byte[]).Replace("-", "") : v.Value);
#endif
            return retVal;
        }

        /// <summary>
        /// Delete the specified tuple from the datamart object
        /// </summary>
        /// <param name="datamartId"></param>
        /// <param name="tupleId"></param>
        public void Delete(Guid datamartId, dynamic deleteQuery)
        {

            this.ThrowIfDisposed();

            lock (this.m_lock)
            {
                using (var tx = this.m_connection.BeginTransaction())
                    try
                    {
                        var mart = this.GetDatamart(datamartId);
                        if (mart == null)
                            throw new KeyNotFoundException(datamartId.ToString());

                        // Query paremeters
                        IDictionary<String, Object> parms = null;
                        if (deleteQuery is String)
                            deleteQuery = NameValueCollection.ParseQueryString(deleteQuery as String);
                        if (deleteQuery is NameValueCollection)
                        {
                            parms = new ExpandoObject();
                            foreach (var itm in (deleteQuery as NameValueCollection))
                                parms.Add(itm.Key, itm.Value);
                        }
                        else 
                        {
                            parms = new ExpandoObject();
                            foreach (var itm in deleteQuery.GetType().GetProperties())
                                parms.Add(itm.Name, itm.GetValue(deleteQuery, null));
                        }

                        // First, we delete the record
                        List<object> vals = new List<object>();
                        using (var dbc = this.CreateCommand(tx, String.Format("DELETE FROM {0} WHERE uuid IN ({1})", mart.Schema.Name, this.ParseFilterDictionary(mart.Schema.Name, parms, vals)), vals.ToArray()))
                            dbc.ExecuteNonQuery();
                        this.DeleteProperties(tx, mart.Schema.Name, mart.Schema);
                        
                        tx.Commit();
                    }
                    catch (Exception e)
                    {
                        this.m_tracer.TraceError("Error deleting {0} : {1}", datamartId, e);
                        tx.Rollback();
                        throw;
                    }
            }
        }

        /// <summary>
        /// Delete property values
        /// </summary>
        private void DeleteProperties(SqliteTransaction tx, String path, IDatamartSchemaPropertyContainer container)
        {
            foreach (var p in container.Properties.Where(o => o.Type == SchemaPropertyType.Object))
            {
                using (var dbc = this.CreateCommand(tx, string.Format("DELETE FROM {0}_{1} WHERE entity_uuid NOT IN (SELECT uuid FROM {0})", path, p.Name)))
                    dbc.ExecuteNonQuery();
                this.DeleteProperties(tx, String.Format("{0}_{1}", path, p.Name), p);
            }
        }

        public void DeleteDatamart(Guid datamartId)
        {
            this.ThrowIfDisposed();

            throw new NotImplementedException();
        }

        /// <summary>
        /// Dispose the 
        /// </summary>
        public void Dispose()
        {
            this.m_disposed = true;
            this.m_connection.Close();
            this.m_connection.Dispose();
        }

        public dynamic Get(Guid datamartId, Guid tupleId)
        {
            this.ThrowIfDisposed();

            throw new NotImplementedException();
        }

        /// <summary>
        /// Get the specified data mart
        /// </summary>
        public DatamartDefinition GetDatamart(String name)
        {
            DatamartDefinition retVal = null;
            using (var cmd = this.CreateCommand(null, "SELECT * FROM dw_datamarts WHERE name = ?", name))
            using (var rdr = cmd.ExecuteReader())
                if (rdr.Read())
                {
                    retVal = new DatamartDefinition()
                    {
                        Id = new Guid((byte[])rdr["uuid"]),
                        Name = (string)rdr["name"],
                        CreationTime = (DateTime)rdr["creation_time"],
                        Schema = new DatamartSchema() { Id = new Guid((byte[])rdr["schema_id"]) }
                    };
                }
                else return null;

            retVal.Schema = this.LoadSchema(retVal.Schema.Id);
            return retVal;
        }

        /// <summary>
        /// Get data marts
        /// </summary>
        public List<DatamartDefinition> GetDatamarts()
        {
            this.ThrowIfDisposed();

            lock (this.m_lock)
            {
                try
                {
                    List<DatamartDefinition> retVal = new List<DatamartDefinition>();
                    using (var cmd = this.CreateCommand(null, "SELECT * FROM dw_datamarts"))
                    using (var rdr = cmd.ExecuteReader())
                        while (rdr.Read())
                        {
                            retVal.Add(new DatamartDefinition()
                            {
                                Id = new Guid((byte[])rdr["uuid"]),
                                Name = (string)rdr["name"],
                                CreationTime = (DateTime)rdr["creation_time"],
                                Schema = new DatamartSchema() { Id = new Guid((byte[])rdr["schema_id"]) }
                            });
                        }

                    foreach (var itm in retVal)
                        itm.Schema = this.LoadSchema(itm.Schema.Id);
                    return retVal;
                }
                catch (Exception e)
                {
                    this.m_tracer.TraceError("Error getting data marts: {0}", e);
                    throw;
                }
            }
        }

        /// <summary>
        /// Execute a stored query
        /// </summary>
        public IEnumerable<dynamic> StoredQuery(Guid datamartId, string queryId, dynamic queryParameters)
        {
            this.ThrowIfDisposed();

            lock (this.m_lock)
            {
                try
                {
                    var mart = this.GetDatamart(datamartId);
                    if (mart == null)
                        throw new KeyNotFoundException(datamartId.ToString());

                    // Query paremeters
                    IDictionary<String, Object> parms = queryParameters as ExpandoObject;
                    if (queryParameters is String)
                        queryParameters = NameValueCollection.ParseQueryString(queryParameters as String);
                    if (queryParameters is NameValueCollection)
                    {
                        parms = new ExpandoObject();
                        foreach (var itm in (queryParameters as NameValueCollection))
                            parms.Add(itm.Key, itm.Value);
                    }
                    else
                    {
                        parms = new ExpandoObject();
                        foreach (var itm in queryParameters.GetType().GetProperties())
                            parms.Add(itm.Name, itm.GetValue(queryParameters, null));
                    }

                    int tr = 0;
                    return this.QueryInternal(String.Format("sqp_{0}_{1}", mart.Schema.Name, queryId), parms, 0, 100, out tr);
                }
                catch (Exception e)
                {
                    this.m_tracer.TraceError("Error executing {0} : {1}", queryId, e);
                    throw;
                }
            }

        }

        /// <summary>
        /// Query the specified object with the specified parameters
        /// </summary>
        private IEnumerable<dynamic> QueryInternal(string objectName, IDictionary<string, object> parms, int offset, int count, out int totalResults)
        {
            // Build a query
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("SELECT DISTINCT * FROM {0} WHERE uuid IN (", objectName);
            List<Object> vals = new List<Object>();
            sb.Append(this.ParseFilterDictionary(objectName, parms, vals));

            sb.Append(") ");

            // Construct the result set
            List<dynamic> retVal = new List<dynamic>();


            using (var dbc = this.CreateCommand(null, String.Format("SELECT COUNT(*) FROM ({0})", sb), vals.ToArray()))
                totalResults = (int)dbc.ExecuteScalar();

            sb.AppendFormat(" OFFSET {0} LIMIT {1}", offset, count);

            using (var dbc = this.CreateCommand(null, sb.ToString(), vals))
            using (var rdr = dbc.ExecuteReader())
                while (rdr.Read())
                    retVal.Add(this.CreateDynamic(rdr));
            return retVal;
        }

        /// <summary>
        /// Parses a filter dictionary and creates the necessary SQL
        /// </summary>
        private String ParseFilterDictionary(String objectName, IDictionary<string, object> parms, List<object> vals)
        {
            StringBuilder sb = new StringBuilder();
            if (parms.Count() > 0)
            {
                sb.AppendFormat("SELECT uuid FROM {0} WHERE ", objectName);

                foreach (var s in parms)
                {

                    object rValue = s.Value;
                    if (!(rValue is IList))
                        rValue = new List<Object>() { rValue };

                    sb.Append("(");

                    string key = s.Key.Replace(".", "_"),
                        scopedQuery = objectName + ".";

                    foreach (var itm in rValue as IList)
                    {
                        var value = itm;

                        if (value is String)
                        {
                            var sValue = itm as String;
                            switch (sValue[0])
                            {
                                case '<':
                                    if (sValue[1] == '=')
                                    {
                                        sb.AppendFormat(" {0} <= ? AND ", key);
                                        value = sValue.Substring(2);
                                    }
                                    else
                                    {
                                        sb.AppendFormat(" {0} < ? AND ", key);
                                        value = sValue.Substring(1);
                                    }
                                    break;
                                case '>':
                                    if (sValue[1] == '=')
                                    {
                                        sb.AppendFormat(" {0} >= ? AND ", key);
                                        value = sValue.Substring(2);
                                    }
                                    else
                                    {
                                        sb.AppendFormat(" {0} > ? AND ", key);
                                        value = sValue.Substring(1);
                                    }
                                    break;
                                case '!':
                                    if (sValue.Equals("!null"))
                                    {
                                        sb.AppendFormat(" {0} IS NOT NULL AND ", key);
                                        value = sValue = null;
                                    }
                                    else
                                    {
                                        sb.AppendFormat(" {0} <> ? AND ", key);
                                        value = sValue.Substring(1);
                                    }
                                    break;
                                case '~':
                                    sb.AppendFormat(" {0} LIKE '%' || ? || '%'  OR ", key);
                                    value = sValue.Substring(1);
                                    break;
                                default:
                                    if (sValue.Equals("null"))
                                    {
                                        sb.AppendFormat("{0} IS NULL OR ", key);
                                        value = sValue = null;
                                    }
                                    else
                                        sb.AppendFormat(" {0} = ?  OR ", key);
                                    break;
                            }
                        }
                        else
                            sb.AppendFormat(" {0} = ?  OR ", key);

                        // Value correction
                        DateTime tdateTime = default(DateTime);
                        Guid gValue = Guid.Empty;
                        if (value == null)
                            ;
                        else if (value is Guid)
                            vals.Add(((Guid)value).ToByteArray());
                        else if (Guid.TryParse(value.ToString(), out gValue))
                            vals.Add(gValue.ToByteArray());
                        else if (DateTime.TryParse(value.ToString(), out tdateTime))
                            vals.Add(tdateTime);
                        else
                            vals.Add(value);
                    }
                    sb.Remove(sb.Length - 4, 4);
                } // exist or value

                sb.Append(") AND ");

            }
            else
                sb.Append("SELECT uuid FROM sqp_{0}_{1}    ");
            sb.Remove(sb.Length - 4, 4);
            return sb.ToString();
        }

        /// <summary>
        /// Create a dynamic object
        /// </summary>
        private dynamic CreateDynamic(SqliteDataReader rdr)
        {
            dynamic retVal = new ExpandoObject();
            for (int i = 0; i < rdr.FieldCount; i++)
                retVal[rdr.GetName(i)] = rdr[i];
            return retVal;
        }

        /// <summary>
        /// Create a stored query
        /// </summary>
        public void CreateStoredQuery(Guid datamartId, object queryDefinition)
        {
            this.ThrowIfDisposed();
            
            var dmQuery = queryDefinition as DatamartStoredQuery;
            if (queryDefinition is ExpandoObject)
                dmQuery = JsonConvert.DeserializeObject<DatamartStoredQuery>(JsonConvert.SerializeObject(queryDefinition));

            // Not interested
            if (dmQuery == null) throw new ArgumentOutOfRangeException(nameof(queryDefinition));
            else if (dmQuery.ProviderId != this.DataProvider) return;
            else if (!dmQuery.Definition.Trim().StartsWith("select", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Only SELECT allowed for stored queries");

            // Now create / register the data schema
            lock (this.m_lock)
            {
                using (var tx = this.m_connection.BeginTransaction())
                {
                    try
                    {
                        this.m_tracer.TraceInfo("Creating stored query {0}", dmQuery.Name);

                        var mart = this.GetDatamart(datamartId);
                        if (mart == null) throw new KeyNotFoundException(datamartId.ToString());

                        using (var dbc = this.CreateCommand(tx, String.Format("DROP VIEW IF EXISTS sqp_{0}_{1}", mart.Schema.Name, dmQuery.Name))) dbc.ExecuteNonQuery();
                        // Create the data in the DMART
                        StringBuilder queryBuilder = new StringBuilder("CREATE VIEW IF NOT EXISTS ");
                        queryBuilder.AppendFormat("sqp_{0}_{1} AS SELECT * FROM ({2})", mart.Schema.Name, dmQuery.Name, dmQuery.Definition);

                        using (var dbc = this.CreateCommand(tx, queryBuilder.ToString())) dbc.ExecuteNonQuery();
                        tx.Commit();
                    }
                    catch (Exception e)
                    {
                        this.m_tracer.TraceError("Error creating stored query {0} : {1}", dmQuery.Name, e);
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Get the specified data mart
        /// </summary>
        private DatamartDefinition GetDatamart(Guid datamartId)
        {
            lock (this.m_lock)
            {
                try
                {
                    DatamartDefinition retVal = new DatamartDefinition();
                    using (var cmd = this.CreateCommand(null, "SELECT * FROM dw_datamarts WHERE uuid = ?", datamartId.ToByteArray()))
                    using (var rdr = cmd.ExecuteReader())
                        if (rdr.Read())
                        {
                            retVal = new DatamartDefinition()
                            {
                                Id = new Guid((byte[])rdr["uuid"]),
                                Name = (string)rdr["name"],
                                CreationTime = (DateTime)rdr["creation_time"],
                                Schema = new DatamartSchema() { Id = new Guid((byte[])rdr["schema_id"]) }
                            };
                        }
                        else return retVal;

                    retVal.Schema = this.LoadSchema(retVal.Schema.Id);
                    return retVal;
                }
                catch (Exception e)
                {
                    this.m_tracer.TraceError("Error getting data marts: {0}", e);
                    throw;
                }
            }
        }

        /// <summary>
        /// Load schema for the specified item
        /// </summary>
        private DatamartSchema LoadSchema(Guid id)
        {
            DatamartSchema retVal = null;
            using (var dbc = this.CreateCommand(null, "SELECT * FROM dw_schemas WHERE uuid = ?", id.ToByteArray()))
            using (var rdr = dbc.ExecuteReader())
                if (rdr.Read())
                {
                    retVal = new DatamartSchema()
                    {
                        Id = id,
                        Name = (string)rdr["name"]
                    };
                }
                else
                    return null;

            retVal.Properties = this.LoadProperties(id);
            // TODO: load schema
            return retVal;
        }

        /// <summary>
        /// Load properties for the specified container id
        /// </summary>
        private List<DatamartSchemaProperty> LoadProperties(Guid containerId)
        {
            List<DatamartSchemaProperty> retVal = new List<DatamartSchemaProperty>();
            using (var dbc = this.CreateCommand(null, "SELECT * FROM dw_properties WHERE cont_id = ?", containerId.ToByteArray()))
            using (var rdr = dbc.ExecuteReader())
                while (rdr.Read())
                {
                    retVal.Add(new DatamartSchemaProperty()
                    {
                        Id = new Guid((byte[])rdr["uuid"]),
                        Name = (string)rdr["name"],
                        Type = (SchemaPropertyType)(int)rdr["type"],
                        Attributes = (SchemaPropertyAttributes)(int)rdr["attr"]
                    });
                }

            foreach (var itm in retVal)
                itm.Properties = this.LoadProperties(itm.Id);
            // TODO: load schema
            return retVal;
        }

        /// <summary>
        /// Start the warehouse service
        /// </summary>
        public bool Start()
        {
            this.Starting?.Invoke(this, EventArgs.Empty);

            var connectionString = ApplicationContext.Current.Configuration.GetConnectionString("openIzWarehouse");

            this.InitializeDatabase(connectionString.Value);

            this.Started?.Invoke(this, EventArgs.Empty);
            return true;
        }

        public bool Stop()
        {
            this.Stopping?.Invoke(this, EventArgs.Empty);

            this.m_connection.Close();
            this.m_connection.Dispose();

            this.Stopped?.Invoke(this, EventArgs.Empty);
            return true;
        }
    }
}