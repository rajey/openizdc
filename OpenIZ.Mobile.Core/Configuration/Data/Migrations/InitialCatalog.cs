﻿using System;
using System.Reflection;
using System.Linq;
using SQLite;
using OpenIZ.Mobile.Core.Data.Model.Concepts;
using OpenIZ.Mobile.Core.Diagnostics;
using OpenIZ.Mobile.Core.Data.Model.DataType;
using OpenIZ.Mobile.Core.Data.Model.Extensibility;
using OpenIZ.Mobile.Core.Data.Model.Security;
using OpenIZ.Mobile.Core.Data.Model.Entities;
using System.IO;
using OpenIZ.Mobile.Core.Data.Model.Roles;
using OpenIZ.Mobile.Core.Data.Model.Acts;

namespace OpenIZ.Mobile.Core.Configuration.Data.Migrations
{
	/// <summary>
	/// This class is responsible for setting up an initial catalog of items in the SQL Lite database
	/// </summary>
	internal class InitialCatalog : IDbMigration
	{
		
		#region IDbMigration implementation

		/// <summary>
		/// Install the initial catalog
		/// </summary>
		public bool Install ()
		{

			var tracer = Tracer.GetTracer (this.GetType ());

			// Database for the SQL Lite connection
			using (var db = new SQLiteConnection (ApplicationContext.Current?.Configuration.GetConnectionString(ApplicationContext.Current?.Configuration.GetSection<DataConfigurationSection>().MainDataSourceConnectionStringName).Value)) {

                try
                {
                    db.BeginTransaction();
                    db.TableChanged += (s, e) => tracer.TraceInfo("Updating {0}", e.Table.TableName);
                    // Create tables
                    tracer.TraceInfo("Installing Concept Tables...");
                    db.CreateTable<DbConcept>(CreateFlags.None);
                    db.CreateTable<DbConceptName>(CreateFlags.None);
                    db.CreateTable<DbConceptClass>(CreateFlags.None);
                    db.CreateTable<DbConceptRelationship>(CreateFlags.None);
                    db.CreateTable<DbConceptRelationshipType>(CreateFlags.None);
                    db.CreateTable<DbConceptSet>(CreateFlags.None);
                    db.CreateTable<DbConceptSetConceptAssociation>(CreateFlags.None);

                    tracer.TraceInfo("Installing Identiifers Tables...");
                    db.CreateTable<DbEntityIdentifier>(CreateFlags.None);
                    db.CreateTable<DbActIdentifier>(CreateFlags.None);
                    db.CreateTable<DbIdentifierType>(CreateFlags.None);
                    db.CreateTable<DbAssigningAuthority>(CreateFlags.None);

                    tracer.TraceInfo("Installing Extensibility Tables...");
                    db.CreateTable<DbActExtension>(CreateFlags.None);
                    db.CreateTable<DbActNote>(CreateFlags.None);
                    db.CreateTable<DbEntityExtension>(CreateFlags.None);
                    db.CreateTable<DbEntityNote>(CreateFlags.None);
                    db.CreateTable<DbExtensionType>(CreateFlags.None);

                    tracer.TraceInfo("Installing Security Tables...");
                    db.CreateTable<DbSecurityApplication>(CreateFlags.None);
                    db.CreateTable<DbSecurityDevice>(CreateFlags.None);
                    db.CreateTable<DbSecurityPolicy>(CreateFlags.None);
                    db.CreateTable<DbSecurityDevicePolicy>(CreateFlags.None);
                    db.CreateTable<DbSecurityRolePolicy>(CreateFlags.None);
                    db.CreateTable<DbActSecurityPolicy>(CreateFlags.None);
                    db.CreateTable<DbEntitySecurityPolicy>(CreateFlags.None);
                    db.CreateTable<DbSecurityRole>(CreateFlags.None);
                    db.CreateTable<DbSecurityUser>(CreateFlags.None);
                    // Anonymous user
                    db.Insert(new DbSecurityUser()
                    {
                        Key = Guid.Parse("C96859F0-043C-4480-8DAB-F69D6E86696C"),
                        PasswordHash = "XXX",
                        SecurityHash = "XXX",
                        Lockout = DateTime.MaxValue,
                        UserName = "ANONYMOUS",
                        CreationTime = DateTime.Now,
                        CreatedByKey = Guid.Empty
                    });
                    // System user
                    db.Insert(new DbSecurityUser()
                    {
                        Key = Guid.Parse("fadca076-3690-4a6e-af9e-f1cd68e8c7e8"),
                        PasswordHash = "XXXX",
                        SecurityHash = "XXXX",
                        Lockout = DateTime.MaxValue,
                        UserName = "SYSTEM",
                        CreationTime = DateTime.Now,
                        CreatedByKey = Guid.Empty
                    });
                    db.CreateTable<DbSecurityUserRole>(CreateFlags.None);

                    tracer.TraceInfo("Installing Entity Tables...");
                    db.CreateTable<DbEntity>();
                    db.CreateTable<DbApplicationEntity>();
                    db.CreateTable<DbDeviceEntity>();
                    db.CreateTable<DbEntityAddress>();
                    db.CreateTable<DbEntityAddressComponent>();
                    db.CreateTable<DbEntityName>();
                    db.CreateTable<DbEntityNameComponent>();
                    db.CreateTable<DbEntityRelationship>();
                    db.CreateTable<DbMaterial>();
                    db.CreateTable<DbManufacturedMaterial>();
                    db.CreateTable<DbOrganization>();
                    db.CreateTable<DbPerson>();
                    db.CreateTable<DbPlace>();
                    db.CreateTable<DbTelecomAddress>();
                    db.CreateTable<DbEntityTag>();
                    db.CreateTable<DbUserEntity>();
                    db.CreateTable<DbPlaceService>();
                    db.CreateTable<DbPersonLanguageCommunication>();

                    tracer.TraceInfo("Installing Role Tables...");
                    db.CreateTable<DbPatient>();
                    db.CreateTable<DbProvider>();

                    tracer.TraceInfo("Installing Act Tables...");
                    db.CreateTable<DbAct>();
                    db.CreateTable<DbObervation>();
                    db.CreateTable<DbCodedObservation>();
                    db.CreateTable<DbQuantityObservation>();
                    db.CreateTable<DbTextObservation>();
                    db.CreateTable<DbSubstanceAdministration>();
                    db.CreateTable<DbPatientEncounter>();

                    tracer.TraceInfo("Initializing Data & Views...");

                    // Run SQL Script
                    string[] resourceSql = {
                    "OpenIZ.Mobile.Core.Data.Sql.000_init_openiz_algonquin.sql",
                    "OpenIZ.Mobile.Core.Data.Sql.001_init_openiz_core_data.sql"
                };


                    foreach (var sql in resourceSql)
                    {
                        tracer.TraceInfo("Deploying {0}", sql);
                        using (StreamReader sr = new StreamReader(typeof(InitialCatalog).GetTypeInfo().Assembly.GetManifestResourceStream(sql)))
                        {
                            foreach (var stmt in sr.ReadToEnd().Split(';').Select(o => o.Trim()))
                            {
                                if (String.IsNullOrEmpty(stmt)) continue;
                                tracer.TraceVerbose("EXECUTE: {0}", stmt);
                                db.Execute(stmt);
                            }
                        }
                    }
                    db.Commit();
                }
                catch(Exception e)
                {
                    db.Rollback();
                    tracer.TraceError("Error deploying InitialCatalog: {0}", e);
                    throw;
                }
			}
			return true;
		}


		/// <summary>
		/// Configuration identifier
		/// </summary>
		/// <value>The identifier.</value>
		public string Id {
			get {
				return "000-init-openiz-algonquin";
			}
		}

		/// <summary>
		/// A human readable description of the migration
		/// </summary>
		/// <value>The description.</value>
		public string Description {
			get {
				return "OpenIZ Mobile Algonquin (0.1.0.0) data model";
			}
		}


		#endregion
	}
}

