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
	/// Migration 227:
	/// </summary>
	[Migration(227)]
	public class FillUserEmail: MigrationExt
	{
		static readonly ILog log = LogManager.GetLogger(typeof (FillUserEmail).FullName);
		
		/// <summary>
		/// Update the database
		/// </summary>
		override public void Up ()
		{
      if (Database.TableExists ("sfkaddr")) {
        // Subquery to retrieve valid emails from the old user table
        String regExpEmail = "^email:([A-Za-z0-9._%-]+@[A-Za-z0-9.-]+\\.[A-Za-z]+)$";
        String subQuery = "SELECT login, REGEXP_REPLACE(dest, '" + regExpEmail + "', '\\1') As dest " +
          "FROM sfkaddr " +
          "WHERE dest LIKE 'email:%'";

        // Add tableuser.usermail based on sfkaddr.dest
        Database.ExecuteQuery ("UPDATE " + TableName.USER +
                              " SET useremail = tmp.dest " +
                              " FROM (" + subQuery + ") AS tmp" +
                              " WHERE " + TableName.USER + ".userlogin=tmp.login"
                             );
      }
		}
		
		/// <summary>
		/// Downgrade the database
		/// </summary>
		override public void Down ()
		{
			// Nothing to do
		}
	}
}
