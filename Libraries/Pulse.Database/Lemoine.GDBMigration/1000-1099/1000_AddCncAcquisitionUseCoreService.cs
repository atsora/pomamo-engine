// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using System.Diagnostics;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 1000: 
  /// </summary>
  [Migration (1000)]
  public class AddCncAcquisitionUseCoreService : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddCncAcquisitionUseCoreService).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.CNC_ACQUISITION, new Column (TableName.CNC_ACQUISITION + "UseCoreService", DbType.Boolean, ColumnProperty.Null, "FALSE"));
      Database.ExecuteNonQuery ($"UPDATE {TableName.CNC_ACQUISITION} SET {TableName.CNC_ACQUISITION}usecoreservice=FALSE;");
      SetNotNull (TableName.CNC_ACQUISITION, TableName.CNC_ACQUISITION + "UseCoreService");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.CNC_ACQUISITION, TableName.CNC_ACQUISITION + "UseCoreService");
    }
  }
}
