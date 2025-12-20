// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 1906: Add additional properties to nonconformancereason
  /// </summary>
  [Migration (1906)]
  public class NonConformanceReasonProperties : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (NonConformanceReasonProperties).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.NON_CONFORMANCE_REASON,
        new Column (TableName.NON_CONFORMANCE_REASON + "color", DbType.String, "'#FF8000'"));
      Database.ExecuteNonQuery ($"""
        UPDATE {TableName.NON_CONFORMANCE_REASON}
        SET {TableName.NON_CONFORMANCE_REASON}color='#FF8000';
        """);
      SetNotNull (TableName.NON_CONFORMANCE_REASON, TableName.NON_CONFORMANCE_REASON + "color");
      AddConstraintColor (TableName.NON_CONFORMANCE_REASON, TableName.NON_CONFORMANCE_REASON + "color");

      Database.AddColumn (TableName.NON_CONFORMANCE_REASON,
        new Column (TableName.NON_CONFORMANCE_REASON + "displaypriority", DbType.Double));

      Database.AddColumn (TableName.NON_CONFORMANCE_REASON,
        new Column (TableName.NON_CONFORMANCE_REASON + "category", DbType.Int32, 1));
      Database.ExecuteNonQuery ($"""
        UPDATE {TableName.NON_CONFORMANCE_REASON}
        SET {TableName.NON_CONFORMANCE_REASON}category=1
        """);
      SetNotNull (TableName.NON_CONFORMANCE_REASON, TableName.NON_CONFORMANCE_REASON + "category");
      // 1 is Scrap
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.NON_CONFORMANCE_REASON, TableName.NON_CONFORMANCE_REASON + "category");
      Database.RemoveColumn (TableName.NON_CONFORMANCE_REASON, TableName.NON_CONFORMANCE_REASON + "displaypriority");
      Database.RemoveColumn (TableName.NON_CONFORMANCE_REASON, TableName.NON_CONFORMANCE_REASON + "color");
    }
  }
}
