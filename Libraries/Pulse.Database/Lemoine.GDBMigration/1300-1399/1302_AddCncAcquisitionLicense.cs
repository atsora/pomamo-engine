// Copyright (c) 2023 Nicolas Relange

using Lemoine.Core.Log;
using Migrator.Framework;
using System;
using System.Data;
using System.Diagnostics;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 1302: 
  /// </summary>
  [Migration (1302)]
  public class AddCncAcquisitionLicense : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddCncAcquisitionLicense).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      var columnName = $"{TableName.CNC_ACQUISITION}license";
      Database.AddColumn (TableName.CNC_ACQUISITION,
        new Column (columnName, DbType.Int32));
      Database.ExecuteNonQuery ($"update {TableName.CNC_ACQUISITION} set {columnName}=0");
      SetNotNull (TableName.CNC_ACQUISITION, columnName);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.CNC_ACQUISITION, $"{TableName.CNC_ACQUISITION}license");
    }
  }
}
