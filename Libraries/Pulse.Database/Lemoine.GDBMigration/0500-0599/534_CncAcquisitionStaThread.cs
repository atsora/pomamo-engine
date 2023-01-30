// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System.Data;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 534: add a new property "sta thread" for a cnc acquisition
  /// </summary>
  [Migration(534)]
  public class CncAcquisitionStaThread : MigrationExt
  {
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up()
    {
      Database.AddColumn(TableName.CNC_ACQUISITION, new Column(TableName.CNC_ACQUISITION + "stathread", DbType.Boolean, false));
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down()
    {
      Database.RemoveColumn(TableName.CNC_ACQUISITION, TableName.CNC_ACQUISITION + "stathread");
    }
  }
}
