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
  /// Migration 285: the tool life management was based on machines instead of machine modules
  /// </summary>
  [Migration(285)]
  public class ToolLifeWithMachineModules: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ToolLifeWithMachineModules).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up()
    {
      RemoveEventToolLifeTable();
      RemoveToolLifeTable();
      RemoveToolPositionTable();
      
      AddToolPositionTable();
      AddToolLifeTable();
      AddEventToolLifeTable();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down()
    {
      // Nothing, the tables can stay updated in the database
    }
    
    void AddToolPositionTable()
    {
      Database.AddTable(TableName.TOOL_POSITION,
                        new Column(ColumnName.TOOL_POSITION_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                        new Column(TableName.TOOL_POSITION + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                        new Column(ColumnName.MACHINE_MODULE_ID, DbType.Int32, ColumnProperty.NotNull),
                        new Column(TableName.TOOL_POSITION + "magazine", DbType.Int32, null),
                        new Column(TableName.TOOL_POSITION + "pot", DbType.Int32, null),
                        new Column(TableName.TOOL_POSITION + "toolnumber", DbType.Int32, ColumnProperty.NotNull),
                        new Column(TableName.TOOL_POSITION + "stateid", DbType.Int32, ColumnProperty.NotNull));
      
      // Foreign key
      Database.GenerateForeignKey(TableName.TOOL_POSITION, ColumnName.MACHINE_MODULE_ID,
                                  TableName.MACHINE_MODULE, ColumnName.MACHINE_MODULE_ID,
                                  Migrator.Framework.ForeignKeyConstraint.Cascade);
    }
    
    void AddToolLifeTable()
    {
      Database.AddTable(TableName.TOOL_LIFE,
                        new Column(ColumnName.TOOL_LIFE_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                        new Column(TableName.TOOL_LIFE + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                        new Column(ColumnName.MACHINE_MODULE_ID, DbType.Int32, ColumnProperty.NotNull),
                        new Column(ColumnName.TOOL_POSITION_ID, DbType.Int32, null),
                        new Column(TableName.TOOL_LIFE + "typeid", DbType.Int32, ColumnProperty.NotNull),
                        new Column(TableName.TOOL_LIFE + "direction", DbType.Int32, ColumnProperty.NotNull),
                        new Column(TableName.TOOL_LIFE + "value", DbType.Double, ColumnProperty.NotNull),
                        new Column(TableName.TOOL_LIFE + "warning", DbType.Double),
                        new Column(TableName.TOOL_LIFE + "limit", DbType.Double),
                        new Column(TableName.TOOL_LIFE + "initial", DbType.Double),
                        new Column(ColumnName.UNIT_ID, DbType.Int32));
      
      // Foreign keys
      Database.GenerateForeignKey(TableName.TOOL_LIFE, ColumnName.MACHINE_MODULE_ID,
                                  TableName.MACHINE_MODULE, ColumnName.MACHINE_MODULE_ID,
                                  Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey(TableName.TOOL_LIFE, ColumnName.TOOL_POSITION_ID,
                                  TableName.TOOL_POSITION, ColumnName.TOOL_POSITION_ID,
                                  Migrator.Framework.ForeignKeyConstraint.SetNull);
      Database.GenerateForeignKey(TableName.TOOL_LIFE, ColumnName.UNIT_ID,
                                  TableName.UNIT, ColumnName.UNIT_ID,
                                  Migrator.Framework.ForeignKeyConstraint.SetNull);
    }
    
    void AddEventToolLifeTable()
    {
      Database.AddTable(TableName.EVENT_TOOL_LIFE,
                        new Column(ColumnName.EVENT_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                        new Column(ColumnName.EVENT_LEVEL_ID, DbType.Int32, ColumnProperty.NotNull),
                        new Column("eventdatetime", DbType.DateTime, ColumnProperty.NotNull),
                        // Reason of the event
                        new Column(TableName.EVENT_TOOL_LIFE + "typeid", DbType.Int32, ColumnProperty.NotNull, 0),
                        new Column("eventmessage", DbType.String, ColumnProperty.NotNull),
                        // Localisation
                        new Column(ColumnName.MACHINE_MODULE_ID, DbType.Int32, ColumnProperty.NotNull),
                        new Column(TableName.EVENT_TOOL_LIFE + "magazine", DbType.Int32),
                        new Column(TableName.EVENT_TOOL_LIFE + "pot", DbType.Int32),
                        // Description of the tool / states
                        new Column(TableName.EVENT_TOOL_LIFE + "oldstate", DbType.Int32, ColumnProperty.NotNull, 0),
                        new Column(TableName.EVENT_TOOL_LIFE + "newstate", DbType.Int32, ColumnProperty.NotNull, 0),
                        new Column(TableName.EVENT_TOOL_LIFE + "oldtoolnumber", DbType.Int32, ColumnProperty.NotNull),
                        new Column(TableName.EVENT_TOOL_LIFE + "newtoolnumber", DbType.Int32, ColumnProperty.NotNull),
                        // Description of the life
                        new Column(TableName.EVENT_TOOL_LIFE + "lifetype", DbType.Int32, ColumnProperty.NotNull, 0),
                        new Column(TableName.EVENT_TOOL_LIFE + "lifedirection", DbType.Int32, ColumnProperty.NotNull, 0),
                        new Column(ColumnName.UNIT_ID, DbType.Int32),
                        new Column(TableName.EVENT_TOOL_LIFE + "oldvalue", DbType.Double),
                        new Column(TableName.EVENT_TOOL_LIFE + "newvalue", DbType.Double),
                        new Column(TableName.EVENT_TOOL_LIFE + "oldwarning", DbType.Double),
                        new Column(TableName.EVENT_TOOL_LIFE + "newwarning", DbType.Double),
                        new Column(TableName.EVENT_TOOL_LIFE + "oldlimit", DbType.Double),
                        new Column(TableName.EVENT_TOOL_LIFE + "newlimit", DbType.Double),
                        new Column(TableName.EVENT_TOOL_LIFE + "oldinitial", DbType.Double),
                        new Column(TableName.EVENT_TOOL_LIFE + "newinitial", DbType.Double)
                       );
      
      // Sequence
      SetSequence (TableName.EVENT_TOOL_LIFE, ColumnName.EVENT_ID, SequenceName.EVENT_ID);
      
      // Foreign keys
      Database.GenerateForeignKey(TableName.EVENT_TOOL_LIFE, ColumnName.EVENT_LEVEL_ID,
                                  TableName.EVENT_LEVEL, ColumnName.EVENT_LEVEL_ID,
                                  Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey(TableName.EVENT_TOOL_LIFE, ColumnName.MACHINE_MODULE_ID,
                                  TableName.MACHINE_MODULE, ColumnName.MACHINE_MODULE_ID,
                                  Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey(TableName.EVENT_TOOL_LIFE, ColumnName.UNIT_ID,
                                  TableName.UNIT, ColumnName.UNIT_ID,
                                  Migrator.Framework.ForeignKeyConstraint.SetNull);
    }
    
    void RemoveToolPositionTable()
    {
      Database.RemoveTable(TableName.TOOL_POSITION);
    }
    
    void RemoveToolLifeTable()
    {
      Database.RemoveTable(TableName.TOOL_LIFE);
    }
    
    void RemoveEventToolLifeTable()
    {
      Database.RemoveTable(TableName.EVENT_TOOL_LIFE);
    }
  }
}
