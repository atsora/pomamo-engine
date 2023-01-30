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
  /// Migration 199: Add the tables EventCncValue and EventCncValueConfig
  /// </summary>
  [Migration(199)]
  public class AddEventCncValue: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddEventCncValue).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (!Database.TableExists (TableName.EVENT_CNC_VALUE_CONFIG)) {
        AddEventCncValueConfigTable ();
      }
      if (!Database.TableExists (TableName.EVENT_CNC_VALUE)) {
        AddEventCncValueTable ();
      }
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      if (Database.TableExists (TableName.EVENT_CNC_VALUE)) {
        RemoveEventCncValueTable ();
      }
      if (Database.TableExists (TableName.EVENT_CNC_VALUE_CONFIG)) {
        RemoveEventCncValueConfigTable ();
      }
    }
    
    void AddEventCncValueConfigTable ()
    {
      Database.AddTable (TableName.EVENT_CNC_VALUE_CONFIG,
                         new Column (ColumnName.EVENT_CNC_VALUE_CONFIG_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column ("EventCncValueConfigVersion", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (TableName.EVENT_CNC_VALUE_CONFIG + "name", DbType.String, ColumnProperty.NotNull),
                         new Column ("EventMessage", DbType.String, ColumnProperty.NotNull, "''"),
                         new Column (ColumnName.FIELD_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.MACHINE_FILTER_ID, DbType.Int32),
                         new Column ("EventCncValueCondition", DbType.String, ColumnProperty.NotNull),
                         new Column ("EventCncValueMinDuration", DbType.Int32, ColumnProperty.NotNull, 0),
                         new Column (ColumnName.EVENT_LEVEL_ID, DbType.Int32, ColumnProperty.NotNull));
      AddNonNegativeConstraint (TableName.EVENT_CNC_VALUE_CONFIG,
                                "EventCncValueMinDuration");
      AddUniqueIndex (TableName.EVENT_CNC_VALUE_CONFIG,
                      TableName.EVENT_CNC_VALUE_CONFIG + "name");
      AddIndex (TableName.EVENT_CNC_VALUE_CONFIG,
                ColumnName.FIELD_ID);
      Database.GenerateForeignKey (TableName.EVENT_CNC_VALUE_CONFIG, ColumnName.FIELD_ID,
                                   TableName.FIELD, ColumnName.FIELD_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.EVENT_CNC_VALUE_CONFIG, ColumnName.MACHINE_FILTER_ID,
                                   TableName.MACHINE_FILTER, ColumnName.MACHINE_FILTER_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.EVENT_CNC_VALUE_CONFIG, ColumnName.EVENT_LEVEL_ID,
                                   TableName.EVENT_LEVEL, ColumnName.EVENT_LEVEL_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Restrict);
    }
    
    void RemoveEventCncValueConfigTable ()
    {
      Database.RemoveTable (TableName.EVENT_CNC_VALUE_CONFIG);
    }
    
    void AddEventCncValueTable ()
    {
      Database.AddTable (TableName.EVENT_CNC_VALUE,
                         new Column (ColumnName.EVENT_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (ColumnName.EVENT_LEVEL_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.EVENT_DATETIME, DbType.DateTime, ColumnProperty.NotNull),
                         new Column ("EventMessage", DbType.String, ColumnProperty.NotNull),
                         new Column (ColumnName.MACHINE_MODULE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.FIELD_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (TableName.CNC_VALUE + "string", DbType.String),
                         new Column (TableName.CNC_VALUE + "int", DbType.Int32),
                         new Column (TableName.CNC_VALUE + "double", DbType.Double),
                         new Column (TableName.EVENT_CNC_VALUE + "duration", DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.EVENT_CNC_VALUE_CONFIG_ID, DbType.Int32));
      SetSequence (TableName.EVENT_CNC_VALUE,
                   ColumnName.EVENT_ID,
                   SequenceName.EVENT_ID);
      AddNonNegativeConstraint (TableName.EVENT_CNC_VALUE,
                                "EventCncValueDuration");
      AddIndex (TableName.EVENT_CNC_VALUE,
                ColumnName.FIELD_ID);
      AddIndex (TableName.EVENT_CNC_VALUE,
                ColumnName.MACHINE_MODULE_ID);
      Database.GenerateForeignKey (TableName.EVENT_CNC_VALUE, ColumnName.MACHINE_MODULE_ID,
                                   TableName.MACHINE_MODULE, ColumnName.MACHINE_MODULE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.EVENT_CNC_VALUE, ColumnName.FIELD_ID,
                                   TableName.FIELD, ColumnName.FIELD_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.EVENT_CNC_VALUE, ColumnName.EVENT_CNC_VALUE_CONFIG_ID,
                                   TableName.EVENT_CNC_VALUE_CONFIG, ColumnName.EVENT_CNC_VALUE_CONFIG_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      Database.GenerateForeignKey (TableName.EVENT_CNC_VALUE, ColumnName.EVENT_LEVEL_ID,
                                   TableName.EVENT_LEVEL, ColumnName.EVENT_LEVEL_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
    }
    
    void RemoveEventCncValueTable ()
    {
      Database.RemoveTable (TableName.EVENT_CNC_VALUE);
    }
    
    void UpgradeEventView ()
    {
      // Note: this method is never called
      Database.ExecuteNonQuery (@"CREATE OR REPLACE VIEW event AS
SELECT eventid, eventlevelid, eventdatetime, 'EventLongPeriod', machineid, '' AS eventmessage
FROM eventlongperiod
UNION
SELECT eventid, eventlevelid, eventdatetime, 'EventCncValue', machineid, eventmessage
FROM eventcncvalue
NATURAL JOIN machinemodule
");
    }
    
    void DowngradeEventView ()
    {
      Database.ExecuteNonQuery (@"CREATE OR REPLACE VIEW event AS
SELECT eventid, eventlevelid, eventdatetime, 'EventLongPeriod', machineid
FROM eventlongperiod");
    }
  }
}
