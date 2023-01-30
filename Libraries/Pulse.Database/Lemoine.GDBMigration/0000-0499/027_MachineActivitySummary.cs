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
  /// Migration 027: adapt the tables to fill the table machinemoduleactivity summary in
  /// <item>change the factoverwriteactivity column to overridenmachinemode</item>
  /// <item>create the table machinemoduleactivitysummary</item>
  /// </summary>
  [Migration(27)]
  public class MachineModuleActivitySummary: MigrationExt
  {
    static readonly string MACHINE_MODE_TRANSLATION_KEY = "machinemodetranslationkey";
    static readonly string MACHINE_MODE_RUNNING = "machinemoderunning";    
    
    static readonly ILog log = LogManager.GetLogger(typeof (MachineModuleActivitySummary).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddNewMachineModes ();

      Database.ExecuteNonQuery ("DROP VIEW IF EXISTS currentdayutilization CASCADE;");
      Database.ExecuteNonQuery ("DROP VIEW IF EXISTS machineactivitysummary CASCADE;");
      AddMachineActivitySummary ();

      UpdateActivityManual ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveMachineActivitySummary ();
      RemoveNewMachineModes ();
    }
    
    void AddNewMachineModes ()
    {
      Database.Insert (TableName.MACHINE_MODE,
                       new string [] {MACHINE_MODE_TRANSLATION_KEY, MACHINE_MODE_RUNNING},
                       new string [] {"MachineModeNoData", "0"});
      Database.Insert (TableName.TRANSLATION,
                       new string [] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string [] {"", "MachineModeNoData", "No data"});
      Database.Insert (TableName.MACHINE_MODE,
                       new string [] {MACHINE_MODE_TRANSLATION_KEY, MACHINE_MODE_RUNNING},
                       new string [] {"MachineModeUnavailable", "0"});
      Database.Insert (TableName.TRANSLATION,
                       new string [] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string [] {"", "MachineModeUnavailable", "Machine unavailable"});
      Database.Insert (TableName.MACHINE_MODE,
                       new string [] {MACHINE_MODE_TRANSLATION_KEY, MACHINE_MODE_RUNNING},
                       new string [] {"MachineModeError", "0"});
      Database.Insert (TableName.TRANSLATION,
                       new string [] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string [] {"", "MachineModeError", "Error"});
    }
    
    void RemoveNewMachineModes ()
    {
      Database.Delete (TableName.MACHINE_MODE,
                       MACHINE_MODE_TRANSLATION_KEY,
                       "MachineModeNoData");
      Database.Delete (TableName.MACHINE_MODE,
                       MACHINE_MODE_TRANSLATION_KEY,
                       "MachineModeUnavailable");
      Database.Delete (TableName.MACHINE_MODE,
                       MACHINE_MODE_TRANSLATION_KEY,
                       "MachineModeError");
      
      Database.Delete (TableName.TRANSLATION,
                       ColumnName.TRANSLATION_KEY,
                       "MachineModeNoData");
      Database.Delete (TableName.TRANSLATION,
                       ColumnName.TRANSLATION_KEY,
                       "MachineModeUnavailable");
      Database.Delete (TableName.TRANSLATION,
                       ColumnName.TRANSLATION_KEY,
                       "MachineModeError");
    }
        
    void AddMachineActivitySummary ()
    {
      Database.AddTable (TableName.MACHINE_ACTIVITY_SUMMARY,
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column ("machineactivityday", DbType.Date, ColumnProperty.PrimaryKey),
                         new Column (ColumnName.MACHINE_OBSERVATION_STATE_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (ColumnName.MACHINE_MODE_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column ("machineactivitytime", DbType.Double, ColumnProperty.NotNull));
      Database.GenerateForeignKey (TableName.MACHINE_ACTIVITY_SUMMARY, ColumnName.MACHINE_ID,
                                   TableName.MONITORED_MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.MACHINE_ACTIVITY_SUMMARY, ColumnName.MACHINE_MODE_ID,
                                   TableName.MACHINE_MODE, ColumnName.MACHINE_MODE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
    }
    
    void RemoveMachineActivitySummary ()
    {
      if (Database.TableExists (TableName.MACHINE_ACTIVITY_SUMMARY)) {
        Database.RemoveTable (TableName.MACHINE_ACTIVITY_SUMMARY);
      }
    }
    
    void UpdateActivityManual ()
    {
      // Remove the not-null constraint
      // Not necessary any more
      DropNotNull (TableName.ACTIVITY_MANUAL, "activityenddatetime");
    }
  }
}
