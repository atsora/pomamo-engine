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
	/// Migration 227: deprecated
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
