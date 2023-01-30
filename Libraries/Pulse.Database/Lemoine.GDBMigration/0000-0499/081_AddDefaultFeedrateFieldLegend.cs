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
  /// Migration 081: make fieldlegendcolor case insensitive
  /// 
  /// Note: no default value is set here any more in FieldLegend for the field Feedrate
  /// </summary>
  [Migration(81)]
  public class AddDefaultFeedrateFieldLegend: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddDefaultFeedrateFieldLegend).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // - Add a forgotten request in migration 080
      Database.ExecuteNonQuery ("ALTER TABLE fieldlegend " +
                                "ALTER COLUMN fieldlegendcolor " +
                                "SET DATA TYPE CITEXT;");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
