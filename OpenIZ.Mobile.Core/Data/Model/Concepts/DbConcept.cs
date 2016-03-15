﻿using System;
using SQLite;

namespace OpenIZ.Mobile.Core.Data.Concepts
{
	/// <summary>
	/// Physical data layer implemntation of concept
	/// </summary>
	[Table("concept")]
	public class DbConcept : DbVersionedData
	{

		/// <summary>
		/// Gets or sets whether the object is a system concept or not
		/// </summary>
		[Column("is_system")]
		public bool IsSystemConcept {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the object mnemonic
		/// </summary>
		/// <value>The mnemonic.</value>
		[Column("mnemonic"), Unique, NotNull]
		public string Mnemonic {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the status concept id
		/// </summary>
		[Column("status_id"), Indexed, NotNull]
		public int StatusId {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the concept classification
		/// </summary>
		[Column("class_id"), NotNull]
		public int ClassId {
			get;
			set;
		}
	}
}

