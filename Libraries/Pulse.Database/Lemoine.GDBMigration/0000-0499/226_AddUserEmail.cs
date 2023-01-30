// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
	/// <summary>
	/// Migration 226: create the field useremail in usertable
	/// </summary>
	[Migration(226)]
	public class AddUserEmail: MigrationExt
	{
		static readonly ILog log = LogManager.GetLogger(typeof (AddUserEmail).FullName);
		
		/// <summary>
		/// Update the database
		/// </summary>
		override public void Up ()
		{
			// Creation of the column "useremail", with a syntaxical constraint
			Database.AddColumn(TableName.USER, "useremail", DbType.String);
			Database.AddCheckConstraint("proper_email", TableName.USER,
			                            "useremail ~ '^[A-Za-z0-9._%-]+@[A-Za-z0-9.-]+\\.[A-Za-z]+$'");
			MakeColumnCaseInsensitive(TableName.USER, "useremail");
		}
		
		/// <summary>
		/// Downgrade the database
		/// </summary>
		override public void Down ()
		{
			Database.RemoveColumn(TableName.USER, "useremail");
		}
	}
}
