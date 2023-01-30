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
  /// Migration 351:
  /// </summary>
  [Migration(351)]
  public class FieldAggregationMax: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (FieldAggregationMax).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.RenameColumn (TableName.FIELD,
                             TableName.FIELD + "averagemintime",
                             TableName.FIELD + "mintime");
      Database.RenameColumn (TableName.FIELD,
                             TableName.FIELD + "averagemaxdeviation",
                             TableName.FIELD + "limitdeviation");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RenameColumn (TableName.FIELD,
                             TableName.FIELD + "mintime",
                             TableName.FIELD + "averagemintime");
      Database.RenameColumn (TableName.FIELD,
                             TableName.FIELD + "limitdeviation",
                             TableName.FIELD + "averagemaxdeviation");
    }
  }
}
