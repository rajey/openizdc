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
 * Date: 2016-6-28
 */
using OpenIZ.Core.Model.Entities;
using OpenIZ.Mobile.Core.Data.Model;
using OpenIZ.Mobile.Core.Data.Model.Entities;
using SQLite.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenIZ.Mobile.Core.Data.Persistence
{
    /// <summary>
    /// Represents an organization persistence service
    /// </summary>
    public class OrganizationPersistenceService : EntityDerivedPersistenceService<Organization, DbOrganization>
    {

       
        /// <summary>
        /// Model instance
        /// </summary>
        public override Organization ToModelInstance(object dataInstance, SQLiteConnectionWithLock context, bool loadFast)
        {
            var iddat = dataInstance as DbVersionedData;
            var organization = dataInstance as DbOrganization ?? context.Table<DbOrganization>().Where(o => o.Uuid == iddat.Uuid).First();
            var dbe = dataInstance as DbEntity ?? context.Table<DbEntity>().Where(o => o.Uuid == organization.Uuid).First();
            var retVal = m_entityPersister.ToModelInstance<Organization>(dbe, context, loadFast);
            retVal.IndustryConceptKey = organization.IndustryConceptUuid  != null ? (Guid?)new Guid(organization.IndustryConceptUuid) : null;
            return retVal;
        }

        /// <summary>
        /// Insert the organization
        /// </summary>
        public override Organization Insert(SQLiteConnectionWithLock context, Organization data)
        {
            // ensure industry concept exists
            data.IndustryConcept?.EnsureExists(context);
            data.IndustryConceptKey = data.IndustryConcept?.Key ?? data.IndustryConceptKey;

            return base.Insert(context, data);
        }

        /// <summary>
        /// Update the organization
        /// </summary>
        public override Organization Update(SQLiteConnectionWithLock context, Organization data)
        {
            data.IndustryConcept?.EnsureExists(context);
            data.IndustryConceptKey = data.IndustryConcept?.Key ?? data.IndustryConceptKey;
            return base.Update(context, data);
        }

    }
}
