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
  /// Migration 036: Add the Long Period event
  /// <item>Add the EventLevel table</item>
  /// <item>Add the EventLongPeriodConfig table</item>
  /// <item>Add the EventLongPeriod table</item>
  /// <item>Add the Event view</item>
  /// <item>Add an index in ReasonSlot</item>
  /// </summary>
  [Migration(36)]
  public class EventLongPeriod: MigrationExt
  {
    static readonly string EVENT_LEVEL_NAME = "EventLevelName";
    static readonly string EVENT_LEVEL_TRANSLATION_KEY = "EventLevelTranslationKey";
    static readonly string EVENT_LEVEL_PRIORITY = "EventLevelPriority";
    
    static readonly ILog log = LogManager.GetLogger(typeof (EventLongPeriod).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (!Database.TableExists (TableName.EVENT_LEVEL)) {
        AddEventLevelTable ();
      }
      if (!Database.TableExists (TableName.EVENT_LONG_PERIOD_CONFIG)) {
        AddEventLongPeriodConfigTable ();
      }
      if (!Database.TableExists (TableName.EVENT_LONG_PERIOD)) {
        AddEventLongPeriodTable ();
      }
      AddEventView ();
      AddReasonSlotIndex ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveReasonSlotIndex ();
      RemoveEventView ();
      if (Database.TableExists (TableName.EVENT_LONG_PERIOD)) {
        RemoveEventLongPeriodTable ();
      }
      if (Database.TableExists (TableName.EVENT_LONG_PERIOD_CONFIG)) {
        RemoveEventLongPeriodConfigTable ();
      }
      if (Database.TableExists (TableName.EVENT_LEVEL)) {
        RemoveEventLevelTable ();
      }
    }
    
    void AddEventLevelTable ()
    {
      Database.AddTable (TableName.EVENT_LEVEL,
                         new Column (ColumnName.EVENT_LEVEL_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column ("EventLevelVersion", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (EVENT_LEVEL_NAME, DbType.String, ColumnProperty.Unique),
                         new Column (EVENT_LEVEL_TRANSLATION_KEY, DbType.String, ColumnProperty.Unique),
                         new Column (EVENT_LEVEL_PRIORITY, DbType.Int32, ColumnProperty.NotNull));
      MakeColumnCaseInsensitive (TableName.EVENT_LEVEL,
                                 EVENT_LEVEL_NAME);
      AddConstraintNameTranslationKey (TableName.EVENT_LEVEL,
                                       EVENT_LEVEL_NAME,
                                       EVENT_LEVEL_TRANSLATION_KEY);
      AddUniqueIndex (TableName.EVENT_LEVEL,
                      EVENT_LEVEL_NAME,
                      EVENT_LEVEL_TRANSLATION_KEY);
      AddIndex (TableName.EVENT_LEVEL,
                EVENT_LEVEL_PRIORITY);
      // List of event levels
      Database.Insert (TableName.TRANSLATION,
                       new string [] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string [] {"", "EventLevelAlert", "Alert"});
      Database.Insert (TableName.EVENT_LEVEL,
                       new string [] {ColumnName.EVENT_LEVEL_ID, EVENT_LEVEL_TRANSLATION_KEY, EVENT_LEVEL_PRIORITY},
                       new string [] {"1", "EventLevelAlert", "100"}); // id = 1 => Alert
      Database.Insert (TableName.TRANSLATION,
                       new string [] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string [] {"", "EventLevelError", "Error"});
      Database.Insert (TableName.EVENT_LEVEL,
                       new string [] {ColumnName.EVENT_LEVEL_ID, EVENT_LEVEL_TRANSLATION_KEY, EVENT_LEVEL_PRIORITY},
                       new string [] {"2", "EventLevelError", "300"}); // id = 2 => Error
      Database.Insert (TableName.TRANSLATION,
                       new string [] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string [] {"", "EventLevelWarn", "Warning"});
      Database.Insert (TableName.EVENT_LEVEL,
                       new string [] {ColumnName.EVENT_LEVEL_ID, EVENT_LEVEL_TRANSLATION_KEY, EVENT_LEVEL_PRIORITY},
                       new string [] {"3", "EventLevelWarn", "400"}); // id = 3 => Warning
      Database.Insert (TableName.TRANSLATION,
                       new string [] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string [] {"", "EventLevelNotice", "Notice"});
      Database.Insert (TableName.EVENT_LEVEL,
                       new string [] {ColumnName.EVENT_LEVEL_ID, EVENT_LEVEL_TRANSLATION_KEY, EVENT_LEVEL_PRIORITY},
                       new string [] {"4", "EventLevelNotice", "500"}); // id = 4 => Notice
      Database.Insert (TableName.TRANSLATION,
                       new string [] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string [] {"", "EventLevelInfo", "Info"});
      Database.Insert (TableName.EVENT_LEVEL,
                       new string [] {ColumnName.EVENT_LEVEL_ID, EVENT_LEVEL_TRANSLATION_KEY, EVENT_LEVEL_PRIORITY},
                       new string [] {"5", "EventLevelInfo", "600"}); // id = 5 => Info
      ResetSequence (TableName.EVENT_LEVEL,
                     ColumnName.EVENT_LEVEL_ID);
    }
    
    void RemoveEventLevelTable ()
    {
      Database.RemoveTable (TableName.EVENT_LEVEL);
      // Remove the translations
      Database.Delete (TableName.TRANSLATION,
                       ColumnName.TRANSLATION_KEY,
                       "EventLevelAlert");
      Database.Delete (TableName.TRANSLATION,
                       ColumnName.TRANSLATION_KEY,
                       "EventLevelError");
      Database.Delete (TableName.TRANSLATION,
                       ColumnName.TRANSLATION_KEY,
                       "EventLevelWarn");
      Database.Delete (TableName.TRANSLATION,
                       ColumnName.TRANSLATION_KEY,
                       "EventLevelNotice");
      Database.Delete (TableName.TRANSLATION,
                       ColumnName.TRANSLATION_KEY,
                       "EventLevelInfo");
    }
    
    void AddEventLongPeriodConfigTable ()
    {
      Database.AddTable (TableName.EVENT_LONG_PERIOD_CONFIG,
                         new Column (ColumnName.EVENT_LONG_PERIOD_CONFIG_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column ("EventLongPeriodConfigVersion", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ColumnName.MACHINE_ID, DbType.Int32),
                         new Column (ColumnName.MACHINE_MODE_ID, DbType.Int32),
                         new Column (ColumnName.MACHINE_OBSERVATION_STATE_ID, DbType.Int32),
                         new Column (ColumnName.EVENT_TRIGGER_DURATION, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.EVENT_LEVEL_ID, DbType.Int32, ColumnProperty.NotNull));
      AddNamedUniqueConstraint (TableName.EVENT_LONG_PERIOD_CONFIG + "_unique",
                             TableName.EVENT_LONG_PERIOD_CONFIG,
                             new string [] {ColumnName.MACHINE_ID, ColumnName.MACHINE_MODE_ID, ColumnName.MACHINE_OBSERVATION_STATE_ID, ColumnName.EVENT_TRIGGER_DURATION} );
      AddIndex (TableName.EVENT_LONG_PERIOD_CONFIG,
                ColumnName.MACHINE_ID);
      Database.GenerateForeignKey (TableName.EVENT_LONG_PERIOD_CONFIG, ColumnName.MACHINE_ID,
                                   TableName.MONITORED_MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.EVENT_LONG_PERIOD_CONFIG, ColumnName.MACHINE_MODE_ID,
                                   TableName.MACHINE_MODE, ColumnName.MACHINE_MODE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.EVENT_LONG_PERIOD_CONFIG, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   TableName.MACHINE_OBSERVATION_STATE, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.EVENT_LONG_PERIOD_CONFIG, ColumnName.EVENT_LEVEL_ID,
                                   TableName.EVENT_LEVEL, ColumnName.EVENT_LEVEL_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Restrict);
    }
    
    void RemoveEventLongPeriodConfigTable ()
    {
      Database.RemoveTable (TableName.EVENT_LONG_PERIOD_CONFIG);
    }
    
    void AddEventLongPeriodTable ()
    {
      Database.AddTable (TableName.EVENT_LONG_PERIOD,
                         new Column (ColumnName.EVENT_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (ColumnName.EVENT_LEVEL_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.EVENT_DATETIME, DbType.DateTime, ColumnProperty.NotNull),
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.MACHINE_MODE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.MACHINE_OBSERVATION_STATE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.EVENT_TRIGGER_DURATION, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.EVENT_LONG_PERIOD_CONFIG_ID, DbType.Int32));
      AddSequence (SequenceName.EVENT_ID);
      SetSequence (TableName.EVENT_LONG_PERIOD,
                   ColumnName.EVENT_ID,
                   SequenceName.EVENT_ID);
      AddIndex (TableName.EVENT_LONG_PERIOD,
                ColumnName.MACHINE_ID);
      Database.GenerateForeignKey (TableName.EVENT_LONG_PERIOD, ColumnName.MACHINE_ID,
                                   TableName.MONITORED_MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.EVENT_LONG_PERIOD, ColumnName.MACHINE_MODE_ID,
                                   TableName.MACHINE_MODE, ColumnName.MACHINE_MODE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.EVENT_LONG_PERIOD, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   TableName.MACHINE_OBSERVATION_STATE, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.EVENT_LONG_PERIOD, ColumnName.EVENT_LONG_PERIOD_CONFIG_ID,
                                   TableName.EVENT_LONG_PERIOD_CONFIG, ColumnName.EVENT_LONG_PERIOD_CONFIG_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      Database.GenerateForeignKey (TableName.EVENT_LONG_PERIOD, ColumnName.EVENT_LEVEL_ID,
                                   TableName.EVENT_LEVEL, ColumnName.EVENT_LEVEL_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
    }
    
    void RemoveEventLongPeriodTable ()
    {
      Database.RemoveTable (TableName.EVENT_LONG_PERIOD);
      RemoveSequence (SequenceName.EVENT_ID);
    }
    
    void AddEventView ()
    {
      Database.ExecuteNonQuery (@"CREATE OR REPLACE VIEW event AS
SELECT eventid, eventlevelid, eventdatetime, 'EventLongPeriod', machineid
FROM eventlongperiod");
    }
    
    void RemoveEventView ()
    {
      Database.ExecuteNonQuery (@"DROP VIEW IF EXISTS event");
    }
    
    void AddReasonSlotIndex ()
    {
      AddIndex (TableName.REASON_SLOT,
                ColumnName.MACHINE_ID,
                "reasonslotenddatetime");
      AddUniqueConstraint (TableName.REASON_SLOT,
                           ColumnName.MACHINE_ID,
                           "reasonslotenddatetime");
    }
    
    void RemoveReasonSlotIndex ()
    {
      RemoveUniqueConstraint (TableName.REASON_SLOT,
                              ColumnName.MACHINE_ID,
                              "reasonslotenddatetime");
      RemoveIndex (TableName.REASON_SLOT,
                   ColumnName.MACHINE_ID,
                   "reasonslotenddatetime");
    }
  }
}
