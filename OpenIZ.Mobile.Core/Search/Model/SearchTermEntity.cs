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
using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenIZ.Mobile.Core.Search.Model
{
    /// <summary>
    /// Identifies a link between a search term and an entity
    /// </summary>
    [Table("search_term_entity")]
    public class SearchTermEntity
    {
        /// <summary>
        /// Constructs a new instances of the search term entity link
        /// </summary>
        public SearchTermEntity()
        {
            this.Key = Guid.NewGuid().ToByteArray();
        }

        /// <summary>
        /// Key of the association
        /// </summary>
        [Column("key"), PrimaryKey, MaxLength(16)]
        public byte[] Key { get; set; }

        /// <summary>
        /// Entity identifier
        /// </summary>
        [Column("entity"), NotNull, Indexed, MaxLength(16)]
        public byte[] EntityId { get; set; }

        /// <summary>
        /// Gets or sets the term identifier
        /// </summary>
        [Column("term"), NotNull, Indexed, MaxLength(16)]
        public byte[] TermId { get; set; }

    }
}
