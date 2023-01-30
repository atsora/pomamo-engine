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
  /// Migration 286: add the table EventToolLifeConfig, which defines the rules to assign
  ///                a level to an tool life event. Tool life events are also linked to
  ///                the rules having generated them.
  /// </summary>
  [Migration(286)]
  public class EventToolLifeConfig: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (EventToolLifeConfig).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // Add table eventtoollifeconfig
      Database.AddTable (TableName.EVENT_TOOL_LIFE_CONFIG,
                         new Column (ColumnName.EVENT_TOOL_LIFE_CONFIG_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.EVENT_TOOL_LIFE_CONFIG + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ColumnName.MACHINE_FILTER_ID, DbType.Int32),
                         new Column (ColumnName.MACHINE_OBSERVATION_STATE_ID, DbType.Int32),
                         new Column (TableName.EVENT_TOOL_LIFE_CONFIG + "typeid", DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.EVENT_LEVEL_ID, DbType.Int32, ColumnProperty.NotNull));
      
      // Foreign keys
      Database.GenerateForeignKey(TableName.EVENT_TOOL_LIFE_CONFIG, ColumnName.MACHINE_FILTER_ID,
                                  TableName.MACHINE_FILTER, ColumnName.MACHINE_FILTER_ID,
                                  Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey(TableName.EVENT_TOOL_LIFE_CONFIG, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                  TableName.MACHINE_OBSERVATION_STATE, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                  Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey(TableName.EVENT_TOOL_LIFE_CONFIG, ColumnName.EVENT_LEVEL_ID,
                                  TableName.EVENT_LEVEL, ColumnName.EVENT_LEVEL_ID,
                                  Migrator.Framework.ForeignKeyConstraint.Restrict);
      
      // Unique constraint
      AddNamedUniqueConstraint("uniquekey", TableName.EVENT_TOOL_LIFE_CONFIG,
                            ColumnName.MACHINE_FILTER_ID,
                            ColumnName.MACHINE_OBSERVATION_STATE_ID,
                            TableName.EVENT_TOOL_LIFE_CONFIG + "typeid");
      
      // Add a link from the table eventtoollife to the table eventtoollifeconfig
      Database.AddColumn(TableName.EVENT_TOOL_LIFE, ColumnName.EVENT_TOOL_LIFE_CONFIG_ID, DbType.Int32);
      Database.GenerateForeignKey(TableName.EVENT_TOOL_LIFE, ColumnName.EVENT_TOOL_LIFE_CONFIG_ID,
                                  TableName.EVENT_TOOL_LIFE_CONFIG, ColumnName.EVENT_TOOL_LIFE_CONFIG_ID,
                                  Migrator.Framework.ForeignKeyConstraint.SetNull);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      // Remove column
      Database.RemoveColumn(TableName.EVENT_TOOL_LIFE, ColumnName.EVENT_TOOL_LIFE_CONFIG_ID);
      
      // Remove table
      Database.RemoveTable(TableName.EVENT_TOOL_LIFE_CONFIG);
    }
  }
}
