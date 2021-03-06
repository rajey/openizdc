﻿/*
 * Copyright 2015-2018 Mohawk College of Applied Arts and Technology
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
 * User: fyfej
 * Date: 2017-9-1
 */
using OpenIZ.Core.Data.QueryBuilder.Attributes;
using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenIZ.Mobile.Core.Data.Model.Acts
{
    /// <summary>
    /// Represents an act protocol
    /// </summary>
    [Table("act_protocol")]
    public class DbActProtocol : DbIdentified
    {

        /// <summary>
        /// Gets or sets the act UUID
        /// </summary>
        [Column("act_uuid"), MaxLength(16), Indexed, ForeignKey(typeof(DbAct), nameof(DbAct.Uuid))]
        public byte[] SourceUuid { get; set; }

        /// <summary>
        /// Gets or sets the protocol uuid
        /// </summary>
        [Column("proto_uuid"), MaxLength(16)]
        public byte[] ProtocolUuid { get; set; }

        /// <summary>
        /// Represents the sequence of the item
        /// </summary>
        [Column("sequence")]
        public int Sequence { get; set; }
    }
}
