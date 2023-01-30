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
  /// Migration 184: Add a completion order to the modificationstatus table
  /// </summary>
  [Migration(184)]
  public class AddModificationCompletionOrder: MigrationExt
  {
    static readonly string ANALYSIS_COMPLETION_ORDER = "analysiscompletionorder";
    
    static readonly ILog log = LogManager.GetLogger(typeof (AddModificationCompletionOrder).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.MODIFICATION_STATUS,
                          new Column (ANALYSIS_COMPLETION_ORDER, DbType.Int32, ColumnProperty.Null));
      AddUniqueIndexCondition (TableName.MODIFICATION_STATUS,
                               string.Format ("{0} IS NOT NULL",
                                              ANALYSIS_COMPLETION_ORDER),
                               ANALYSIS_COMPLETION_ORDER);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveIndex (TableName.MODIFICATION_STATUS, ANALYSIS_COMPLETION_ORDER);
      Database.RemoveColumn (TableName.MODIFICATION_STATUS, ANALYSIS_COMPLETION_ORDER);
    }
  }
}
