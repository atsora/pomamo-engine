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
  /// Migration 075: New CncValue table
  /// <item>Add the new table CncValue</item>
  /// <item>Add two columns in the Field table</item>
  /// <item>Add new default values in the Field and Unit tables</item>
  /// </summary>
  [Migration(75)]
  public class CncValue: MigrationExt
  {
    static readonly string UNIT_NAME = TableName.UNIT + "Name";
    static readonly string UNIT_TRANSLATION_KEY = TableName.UNIT + "TranslationKey";
    static readonly string UNIT_DESCRIPTION = TableName.UNIT + "Description";
    
    static readonly string FIELD_CODE = TableName.FIELD + "Code";
    static readonly string FIELD_NAME = TableName.FIELD + "Name";
    static readonly string FIELD_TRANSLATION_KEY = TableName.FIELD + "TranslationKey";
    static readonly string FIELD_DESCRIPTION = TableName.FIELD + "Description";
    static readonly string FIELD_TYPE = TableName.FIELD + "Type";
    static readonly string CNC_DATA_AGGREGATION_TYPE = "CncDataAggregationType";
    static readonly string FIELD_CUSTOM = TableName.FIELD + "Custom";
    static readonly string FIELD_AVERAGE_MIN_TIME = TableName.FIELD + "averagemintime";
    static readonly string FIELD_AVERAGE_MAX_DEVIATION = TableName.FIELD + "averagemaxdeviation";
    
    static readonly ILog log = LogManager.GetLogger(typeof (CncValue).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddCncValueTable ();
      UpgradeFieldTable ();
      AddDefaultValues ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveDefaultValues ();
      DowngradeFieldTable ();
      RemoveCncValueTable ();
    }
    
    void AddCncValueTable ()
    {
      Database.AddTable (TableName.CNC_VALUE,
                         new Column (TableName.CNC_VALUE + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.CNC_VALUE + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ColumnName.MACHINE_MODULE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.FIELD_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (TableName.CNC_VALUE + "begindatetime", DbType.DateTime, ColumnProperty.NotNull),
                         new Column (TableName.CNC_VALUE + "enddatetime", DbType.DateTime, ColumnProperty.NotNull),
                         new Column (TableName.CNC_VALUE + "string", DbType.String),
                         new Column (TableName.CNC_VALUE + "int", DbType.Int32),
                         new Column (TableName.CNC_VALUE + "double", DbType.Double),
                         new Column (TableName.CNC_VALUE + "deviation", DbType.Double),
                         new Column (ColumnName.FACT_ID, DbType.Int32),
                         new Column (ColumnName.AUTO_SEQUENCE_ID, DbType.Int32));
      Database.GenerateForeignKey (TableName.CNC_VALUE, ColumnName.MACHINE_MODULE_ID,
                                   TableName.MACHINE_MODULE, ColumnName.MACHINE_MODULE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.CNC_VALUE, ColumnName.FIELD_ID,
                                   TableName.FIELD, ColumnName.FIELD_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.CNC_VALUE, ColumnName.FACT_ID,
                                   TableName.FACT, ColumnName.FACT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      Database.GenerateForeignKey (TableName.CNC_VALUE, ColumnName.AUTO_SEQUENCE_ID,
                                   TableName.AUTO_SEQUENCE, ColumnName.AUTO_SEQUENCE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      AddUniqueConstraint (TableName.CNC_VALUE,
                           ColumnName.MACHINE_MODULE_ID, ColumnName.FIELD_ID, TableName.CNC_VALUE + "begindatetime");
      Database.AddCheckConstraint ("cncvalue_begindatetime_enddatetime", TableName.CNC_VALUE,
                                   @"cncvaluebegindatetime <= cncvalueenddatetime");
      AddIndex (TableName.CNC_VALUE,
                ColumnName.MACHINE_MODULE_ID,
                ColumnName.FIELD_ID);
      AddIndex (TableName.CNC_VALUE,
                ColumnName.MACHINE_MODULE_ID,
                ColumnName.FIELD_ID,
                TableName.CNC_VALUE + "begindatetime");
      AddIndex (TableName.CNC_VALUE,
                ColumnName.MACHINE_MODULE_ID,
                ColumnName.FIELD_ID,
                TableName.CNC_VALUE + "enddatetime");
    }
    
    void RemoveCncValueTable ()
    {
      Database.RemoveTable (TableName.CNC_VALUE);
    }
    
    void UpgradeFieldTable ()
    {
      Database.AddColumn (TableName.FIELD,
                          new Column (FIELD_AVERAGE_MIN_TIME, DbType.Double));
      Database.AddColumn (TableName.FIELD,
                          new Column (FIELD_AVERAGE_MAX_DEVIATION, DbType.Double));
    }
    
    void DowngradeFieldTable ()
    {
      Database.RemoveColumn (TableName.FIELD, FIELD_AVERAGE_MAX_DEVIATION);
      Database.RemoveColumn (TableName.FIELD, FIELD_AVERAGE_MIN_TIME);
    }
    
    void AddDefaultValues ()
    {
      // Unit: feedrate (mm/min)
      Database.Insert (TableName.TRANSLATION,
                       new string[] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string[] {"", "UnitFeedrate", "mm/min"});
      Database.Insert (TableName.UNIT,
                       new string[] {ColumnName.UNIT_ID, UNIT_TRANSLATION_KEY, UNIT_DESCRIPTION},
                       new string[] {"1", "UnitFeedrate", "Feedrate unit (mm/min)"});
      // Unit: US feedrate (in/min)
      Database.Insert (TableName.TRANSLATION,
                       new string[] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string[] {"", "UnitFeedrateUS", "IPM"});
      Database.Insert (TableName.UNIT,
                       new string[] {ColumnName.UNIT_ID, UNIT_TRANSLATION_KEY, UNIT_DESCRIPTION},
                       new string[] {"2", "UnitFeedrateUS", "Feedrate US unit (IPM)"});
      // Unit: rotation speed (rpm = rotation per minute)
      Database.Insert (TableName.TRANSLATION,
                       new string[] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string[] {"", "UnitRotationSpeed", "RPM"});
      Database.Insert (TableName.UNIT,
                       new string[] {ColumnName.UNIT_ID, UNIT_TRANSLATION_KEY, UNIT_DESCRIPTION},
                       new string[] {"3", "UnitRotationSpeed", "Rotation speed unit (RPM)"});
      // Unit: percent (%)
      Database.Insert (TableName.TRANSLATION,
                       new string[] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string[] {"", "UnitPercent", "%"});
      Database.Insert (TableName.UNIT,
                       new string[] {ColumnName.UNIT_ID, UNIT_TRANSLATION_KEY, UNIT_DESCRIPTION},
                       new string[] {"4", "UnitPercent", "Percent (%)"});
      SetSequence (TableName.UNIT, ColumnName.UNIT_ID, 100);
      
      // Note: Although the fields are inserted by the DefaultValue,
      //       insert some of them not to block the migration

      // Feedrate
      Database.Insert (TableName.TRANSLATION,
                       new string[] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string[] {"", "FieldFeedrate", "Feedrate"});
      Database.Insert (TableName.FIELD,
                       new string[] {ColumnName.FIELD_ID, FIELD_CODE, FIELD_TRANSLATION_KEY, FIELD_TYPE, ColumnName.UNIT_ID, CNC_DATA_AGGREGATION_TYPE, FIELD_AVERAGE_MIN_TIME, FIELD_AVERAGE_MAX_DEVIATION, FIELD_CUSTOM},
                       new string[] {"100", "Feedrate", "FieldFeedrate", "Double", "1", "1", "10", "200", "0"});
      // SpindleSpeed
      Database.Insert (TableName.TRANSLATION,
                       new string[] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string[] {"", "FieldSpindleSpeed", "Spindle speed"});
      Database.Insert (TableName.FIELD,
                       new string[] {ColumnName.FIELD_ID, FIELD_CODE, FIELD_TRANSLATION_KEY, FIELD_TYPE, ColumnName.UNIT_ID, CNC_DATA_AGGREGATION_TYPE, FIELD_AVERAGE_MIN_TIME, FIELD_AVERAGE_MAX_DEVIATION, FIELD_CUSTOM},
                       new string[] {"101", "SpindleSpeed", "FieldSpindleSpeed", "Double", "3", "1", "10", "20",  "0"});
      // SpindleLoad
      Database.Insert (TableName.TRANSLATION,
                       new string[] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string[] {"", "FieldSpindleLoad", "Spindle load"});
      Database.Insert (TableName.FIELD,
                       new string[] {ColumnName.FIELD_ID, FIELD_CODE, FIELD_TRANSLATION_KEY, FIELD_TYPE, CNC_DATA_AGGREGATION_TYPE, FIELD_AVERAGE_MIN_TIME, FIELD_AVERAGE_MAX_DEVIATION, FIELD_CUSTOM},
                       new string[] {"102", "SpindleLoad", "FieldSpindleLoad", "Double", "1", "10", "20", "0"});
      // FeedrateOverride
      Database.Insert (TableName.TRANSLATION,
                       new string[] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string[] {"", "FieldFeedrateOverride", "Feedrate override"});
      Database.Insert (TableName.FIELD,
                       new string[] {ColumnName.FIELD_ID, FIELD_CODE, FIELD_TRANSLATION_KEY, FIELD_TYPE, ColumnName.UNIT_ID, CNC_DATA_AGGREGATION_TYPE, FIELD_AVERAGE_MIN_TIME, FIELD_AVERAGE_MAX_DEVIATION, FIELD_CUSTOM},
                       new string[] {"103", "FeedrateOverride", "FieldFeedrateOverride", "Double", "4", "1", "10", "5", "0"});
      // SpindleSpeedOverride
      Database.Insert (TableName.TRANSLATION,
                       new string[] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string[] {"", "FieldSpindleSpeedOverride", "SpindleSpeed override"});
      Database.Insert (TableName.FIELD,
                       new string[] {ColumnName.FIELD_ID, FIELD_CODE, FIELD_TRANSLATION_KEY, FIELD_TYPE, ColumnName.UNIT_ID, CNC_DATA_AGGREGATION_TYPE, FIELD_AVERAGE_MIN_TIME, FIELD_AVERAGE_MAX_DEVIATION, FIELD_CUSTOM},
                       new string[] {"104", "SpindleSpeedOverride", "FieldSpindleSpeedOverride", "Double", "4", "1", "10", "5", "0"});

      SetSequence (TableName.FIELD, ColumnName.FIELD_ID, 200);
    }
    
    void RemoveDefaultValues ()
    {
      Database.ExecuteNonQuery (string.Format (@"DELETE FROM {0} WHERE {1} IS NOT NULL",
                                               TableName.FIELD, CNC_DATA_AGGREGATION_TYPE));
      Database.ExecuteNonQuery (string.Format (@"DELETE FROM {0}",
                                               TableName.UNIT));
      string[] translationKeys = new string [] {"UnitFeedrate", "UnitFeedrateUS", "UnitRotationSpeed", "UnitPercent",
        "FieldFeedrate", "FieldSpindleSpeed", "FieldSpindleLoad", "FieldFeedrateOverride", "FieldSpindleSpeedOverride"};
      foreach (string translationKey in translationKeys) {
        Database.Delete (TableName.TRANSLATION,
                         ColumnName.TRANSLATION_KEY,
                         translationKey);
      }
    }
  }
}
