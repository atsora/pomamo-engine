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
  /// Migration 082: Add the RapidTraverseOverride field that can be found on Fanuc controls
  /// </summary>
  [Migration(82)]
  public class AddRapidTraverseOverrideField: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddRapidTraverseOverrideField).FullName);

    static readonly string FIELD_CODE = TableName.FIELD + "Code";
    static readonly string FIELD_NAME = TableName.FIELD + "Name";
    static readonly string FIELD_TRANSLATION_KEY = TableName.FIELD + "TranslationKey";
    static readonly string FIELD_DESCRIPTION = TableName.FIELD + "Description";
    static readonly string FIELD_TYPE = TableName.FIELD + "Type";
    static readonly string CNC_DATA_AGGREGATION_TYPE = "CncDataAggregationType";
    static readonly string FIELD_CUSTOM = TableName.FIELD + "Custom";
    static readonly string FIELD_AVERAGE_MIN_TIME = TableName.FIELD + "averagemintime";
    static readonly string FIELD_AVERAGE_MAX_DEVIATION = TableName.FIELD + "averagemaxdeviation";
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // RapidTraverseOverride
      Database.Insert (TableName.TRANSLATION,
                       new string[] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string[] {"", "FieldRapidTraverseOverride", "Rapid traverse override"});
      Database.Insert (TableName.FIELD,
                       new string[] {ColumnName.FIELD_ID, FIELD_CODE, FIELD_TRANSLATION_KEY, FIELD_TYPE, ColumnName.UNIT_ID, CNC_DATA_AGGREGATION_TYPE, FIELD_AVERAGE_MIN_TIME, FIELD_AVERAGE_MAX_DEVIATION, FIELD_CUSTOM},
                       new string[] {"105", "RapidTraverseOverride", "FieldRapidTraverseOverride", "Double", "4", "1", "10", "5", "0"});
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.Delete (TableName.FIELD,
                       ColumnName.FIELD_ID,
                       "105");
      Database.Delete (TableName.TRANSLATION,
                       ColumnName.TRANSLATION_KEY,
                       "FieldRapidTraverseOverride");
    }
  }
}
