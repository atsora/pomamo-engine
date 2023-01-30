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
  /// Migration 261:
  /// </summary>
  [Migration(261)]
  public class AddNonConformanceReasonDetails: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddNonConformanceReasonDetails).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.NON_CONFORMANCE_REASON,
                          new Column (TableName.NON_CONFORMANCE_REASON+"detailsrequired",
                                      DbType.Boolean, ColumnProperty.NotNull, false));
      Database.AddColumn (TableName.NON_CONFORMANCE_REPORT,
                          new Column ("nonconformancedetails", DbType.String));
      Database.AddColumn (TableName.OPERATION_CYCLE_DELIVERABLE_PIECE,
                          new Column ("nonconformancedetails", DbType.String));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.NON_CONFORMANCE_REASON,
                             TableName.NON_CONFORMANCE_REASON + "detailsrequired");
      Database.RemoveColumn (TableName.NON_CONFORMANCE_REPORT,
                             "nonconformancedetails");
      Database.RemoveColumn (TableName.OPERATION_CYCLE_DELIVERABLE_PIECE,
                             "nonconformancedetails");
    }
  }
}
