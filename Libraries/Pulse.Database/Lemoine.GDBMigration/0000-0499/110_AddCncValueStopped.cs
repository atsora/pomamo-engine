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
  /// Migration 110: Add a column in the CncValue table to tell a given value has been interrupted
  /// <item>Add the new CncValueStopped column</item>
  /// <item>Remove the two unused factid and autosequenceid columns</item>
  /// </summary>
  [Migration(110)]
  public class AddCncValueStopped: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddCncValueStopped).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.RemoveColumn (TableName.CNC_VALUE,
                             ColumnName.FACT_ID);
      Database.RemoveColumn (TableName.CNC_VALUE,
                             ColumnName.AUTO_SEQUENCE_ID);
      
      Database.AddColumn (TableName.CNC_VALUE,
                          new Column (TableName.CNC_VALUE + "Stopped", DbType.Boolean, ColumnProperty.NotNull, false));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.CNC_VALUE,
                             TableName.CNC_VALUE + "Stopped");
      
      Database.AddColumn (TableName.CNC_VALUE,
                          new Column (ColumnName.FACT_ID, DbType.Int32));
      Database.AddColumn (TableName.CNC_VALUE,
                          new Column (ColumnName.AUTO_SEQUENCE_ID, DbType.Int32));
      Database.GenerateForeignKey (TableName.CNC_VALUE, ColumnName.FACT_ID,
                                   TableName.FACT, ColumnName.FACT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      Database.GenerateForeignKey (TableName.CNC_VALUE, ColumnName.AUTO_SEQUENCE_ID,
                                   TableName.AUTO_SEQUENCE, ColumnName.AUTO_SEQUENCE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
    }
  }
}
