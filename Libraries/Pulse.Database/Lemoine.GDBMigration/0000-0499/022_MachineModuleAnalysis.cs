// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using System.Diagnostics;

using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 022: Add the following tables
  /// <item>MonitoredMachine (only the base columns)</item>
  /// <item>MachineModule</item>
  /// <item>MachineMode</item>
  /// <item>MachineModuleStatus</item>
  /// <item>ProcessDetection</item>
  /// </summary>
  [Migration(22)]
  public class MachineModuleAnalysis: Migration
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MachineModuleAnalysis).FullName);

    static readonly string MAIN_MACHINE_MODULE_ID = "mainmachinemoduleid";
    static readonly string MACHINE_MODULE_NAME = "machinemodulename";
    static readonly string MACHINE_MODULE_CODE = "machinemodulecode";
    static readonly string MACHINE_MODULE_EXTERNALCODE = "machinemoduleexternalcode";
    static readonly string MACHINE_MODULE_CONFIGURATION = "machinemoduleconfiguration";
    static readonly string MACHINE_MODULE_PARAMETERS = "machinemoduleparameters";
    
    static readonly string MACHINE_MODE_NAME = "machinemodename";
    static readonly string MACHINE_MODE_TRANSLATION_KEY = "machinemodetranslationkey";
    static readonly string MACHINE_MODE_RUNNING = "machinemoderunning";
    static readonly string MACHINE_MODE_AUTO = "machinemodeauto";
    static readonly string MACHINE_MODE_MANUAL = "machinemodemanual";
    static readonly string MACHINE_MODE_AUTOPROCESS = "machinemodeautoprocess";
    
    static readonly string MACHINE_MODULE_STATUS_AUTO_PROCESS = "autoprocessid";
    static readonly string MACHINE_MODULE_STATUS_AUTO_PROCESS_DATETIME = "autoprocessdatetime";
    static readonly string MACHINE_MODULE_STATUS_AUTO_PROCESS_BEGIN = "autoprocessbegin";
    static readonly string MACHINE_MODULE_STATUS_AUTO_PROCESS_END = "autoprocessend";
    
    /// <summary>
    /// Add the new MonitoredMachine table (only the base columns)
    /// </summary>
    private void AddMonitoredMachineTable ()
    {
      Database.AddTable (TableName.MONITORED_MACHINE,
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (MAIN_MACHINE_MODULE_ID, DbType.Int32));
      Database.GenerateForeignKey (TableName.MONITORED_MACHINE, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.ExecuteNonQuery ("CREATE UNIQUE INDEX monitoredmachine_mainmachinemoduleid_idx " +
                                "ON monitoredmachine (mainmachinemoduleid) " +
                                "WHERE mainmachinemoduleid IS NOT NULL;");
    }
    
    /// <summary>
    /// Add the new MachineModule table
    /// </summary>
    private void AddMachineModuleTable ()
    {
      Database.AddTable (TableName.MACHINE_MODULE,
                         new Column (ColumnName.MACHINE_MODULE_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (MACHINE_MODULE_NAME, DbType.String),
                         new Column (MACHINE_MODULE_CODE, DbType.String),
                         new Column (MACHINE_MODULE_EXTERNALCODE, DbType.String, ColumnProperty.Unique),
                         new Column (MACHINE_MODULE_CONFIGURATION, DbType.String),
                         new Column (MACHINE_MODULE_PARAMETERS, DbType.String));
      Database.ExecuteNonQuery ("ALTER TABLE machinemodule " +
                                "ALTER COLUMN machinemodulename " +
                                "SET DATA TYPE CITEXT;");
      Database.ExecuteNonQuery ("ALTER TABLE machinemodule " +
                                "ALTER COLUMN machinemodulecode " +
                                "SET DATA TYPE CITEXT;");
      Database.AddUniqueConstraint ("machinemodule_machineid_name_key",
                                    TableName.MACHINE_MODULE,
                                    new string[] {ColumnName.MACHINE_ID,
                                      MACHINE_MODULE_NAME});
      Database.AddUniqueConstraint ("machinemodule_machineid_code_key",
                                    TableName.MACHINE_MODULE,
                                    new string[] {ColumnName.MACHINE_ID,
                                      MACHINE_MODULE_CODE});
      Database.AddCheckConstraint ("machinemodule_name_code",
                                   TableName.MACHINE_MODULE,
                                   "((machinemodulename IS NOT NULL) OR (machinemodulecode IS NOT NULL))");
      Database.GenerateForeignKey (TableName.MACHINE_MODULE, ColumnName.MACHINE_ID,
                                   TableName.MONITORED_MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.MONITORED_MACHINE, MAIN_MACHINE_MODULE_ID,
                                   TableName.MACHINE_MODULE, ColumnName.MACHINE_MODULE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      Database.ExecuteNonQuery ("CREATE UNIQUE INDEX machinemodule_machineid_name_idx " +
                                "ON machinemodule (machineid, machinemodulename) " +
                                "WHERE machinemodulename IS NOT NULL;");
      Database.ExecuteNonQuery ("CREATE UNIQUE INDEX machinemodule_machineid_code_idx " +
                                "ON machinemodule (machineid, machinemodulecode) " +
                                "WHERE machinemodulecode IS NOT NULL;");
      Database.ExecuteNonQuery ("CREATE INDEX machinemodule_machineid_idx " +
                                "ON machinemodule (machineid)");
      Database.ExecuteNonQuery (@"UPDATE monitoredmachine
SET mainmachinemoduleid=machinemodule.machinemoduleid
FROM machinemodule
WHERE monitoredmachine.machineid=machinemodule.machineid");
    }
    
    /// <summary>
    /// Add the new MachineMode table
    /// </summary>
    private void AddMachineModeTable ()
    {
      Database.AddTable (TableName.MACHINE_MODE,
                         new Column (ColumnName.MACHINE_MODE_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (MACHINE_MODE_NAME, DbType.String, ColumnProperty.Unique),
                         new Column (MACHINE_MODE_TRANSLATION_KEY, DbType.String, ColumnProperty.Unique),
                         new Column (MACHINE_MODE_RUNNING, DbType.Boolean, ColumnProperty.NotNull),
                         new Column (MACHINE_MODE_AUTO, DbType.Boolean),
                         new Column (MACHINE_MODE_MANUAL, DbType.Boolean),
                         new Column (MACHINE_MODE_AUTOPROCESS, DbType.Boolean, ColumnProperty.NotNull, false));
      Database.ExecuteNonQuery ("ALTER TABLE machinemode " +
                                "ALTER COLUMN machinemodename " +
                                "SET DATA TYPE CITEXT;");
      Database.AddCheckConstraint ("machinemode_name_translationkey",
                                   TableName.MACHINE_MODE,
                                   "((machinemodename IS NOT NULL) OR (machinemodetranslationkey IS NOT NULL))");
      Database.Insert (TableName.MACHINE_MODE,
                       new string [] {MACHINE_MODE_TRANSLATION_KEY, MACHINE_MODE_RUNNING},
                       new string [] {"MachineModeInactive", "0"}); // id = 1
      Database.Insert (TableName.TRANSLATION,
                       new string [] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string [] {"", "MachineModeInactive", "Inactive"});
      Database.Insert (TableName.MACHINE_MODE,
                       new string [] {MACHINE_MODE_TRANSLATION_KEY, MACHINE_MODE_RUNNING, MACHINE_MODE_AUTOPROCESS},
                       new string [] {"MachineModeActive", "1", "1"}); // id = 2, corresponds to light activity, or general activity
      Database.Insert (TableName.TRANSLATION,
                       new string [] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string [] {"", "MachineModeActive", "Active"});
      Database.Insert (TableName.MACHINE_MODE,
                       new string [] {MACHINE_MODE_TRANSLATION_KEY, MACHINE_MODE_RUNNING, MACHINE_MODE_AUTO, MACHINE_MODE_AUTOPROCESS},
                       new string [] {"MachineModeAuto", "1", "1", "1"}); // id = 3
      Database.Insert (TableName.TRANSLATION,
                       new string [] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string [] {"", "MachineModeAuto", "Auto"});
      Database.Insert (TableName.MACHINE_MODE,
                       new string [] {MACHINE_MODE_TRANSLATION_KEY, MACHINE_MODE_RUNNING, MACHINE_MODE_MANUAL},
                       new string [] {"MachineModeManual", "1", "1"}); // id = 4, generic manual mode
      Database.Insert (TableName.TRANSLATION,
                       new string [] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string [] {"", "MachineModeManual", "Manual"});
      Database.Insert (TableName.MACHINE_MODE,
                       new string [] {MACHINE_MODE_TRANSLATION_KEY, MACHINE_MODE_RUNNING, MACHINE_MODE_MANUAL},
                       new string [] {"MachineModeJog", "1", "1"}); // id = 5
      Database.Insert (TableName.TRANSLATION,
                       new string [] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string [] {"", "MachineModeJog", "Jog mode"});
      Database.Insert (TableName.MACHINE_MODE,
                       new string [] {MACHINE_MODE_TRANSLATION_KEY, MACHINE_MODE_RUNNING, MACHINE_MODE_MANUAL},
                       new string [] {"MachineModeMDI", "1", "1"}); // id = 6
      Database.Insert (TableName.TRANSLATION,
                       new string [] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string [] {"", "MachineModeMDI", "MDI mode"});
      Database.Insert (TableName.MACHINE_MODE,
                       new string [] {MACHINE_MODE_TRANSLATION_KEY, MACHINE_MODE_RUNNING, MACHINE_MODE_MANUAL, MACHINE_MODE_AUTOPROCESS},
                       new string [] {"MachineModeSingleBlock", "1", "1", "1"}); // id = 7
      Database.Insert (TableName.TRANSLATION,
                       new string [] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string [] {"", "MachineModeSingleBlock", "Single block mode"});
    }
    
    /// <summary>
    /// Add the new MachineModuleStatus table
    /// </summary>
    private void AddMachineModuleStatusTable ()
    {
      Database.AddTable (TableName.OLD_MACHINE_MODULE_STATUS,
                         new Column (ColumnName.MACHINE_MODULE_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (MACHINE_MODULE_STATUS_AUTO_PROCESS, DbType.Int32),
                         new Column (MACHINE_MODULE_STATUS_AUTO_PROCESS_DATETIME, DbType.DateTime),
                         new Column (MACHINE_MODULE_STATUS_AUTO_PROCESS_BEGIN, DbType.DateTime),
                         new Column (MACHINE_MODULE_STATUS_AUTO_PROCESS_END, DbType.DateTime));
      Database.GenerateForeignKey (TableName.OLD_MACHINE_MODULE_STATUS, ColumnName.MACHINE_MODULE_ID,
                                   TableName.MACHINE_MODULE, ColumnName.MACHINE_MODULE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      // Set the foreign key once the process table exists. Note: the column has been removed since (to be replaced by the autoprocess table)
      // so there is no use to add the foreign key now
      /*      Database.GenerateForeignKey (TableName.MACHINE_MODULE_STATUS, MACHINE_MODULE_STATUS_AUTO_PROCESS,
                                   TableName.PROCESS, ColumnName.PROCESS_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);*/
      Database.AddCheckConstraint ("machinemodulestatus_autoprocess_datetime",
                                   TableName.OLD_MACHINE_MODULE_STATUS,
                                   @"(autoprocessid IS NULL)
OR (autoprocessid IS NOT NULL
    AND autoprocessdatetime IS NOT NULL)");
    }
    
    /// <summary>
    /// Add the new (temporary) ProcessMachineModuleAssociation table
    /// </summary>
    private void AddProcessDetectionTable ()
    {
      Database.AddTable (TableName.OLD_SEQUENCE_DETECTION,
                         new Column (ColumnName.MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (ColumnName.OLD_SEQUENCE_ID, DbType.Int32),
                         new Column (ColumnName.MACHINE_MODULE_ID, DbType.Int32, ColumnProperty.NotNull));
      Database.GenerateForeignKey (TableName.OLD_SEQUENCE_DETECTION, ColumnName.MODIFICATION_ID,
                                   TableName.MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.OLD_SEQUENCE_DETECTION, ColumnName.MACHINE_MODULE_ID,
                                   TableName.MACHINE_MODULE, ColumnName.MACHINE_MODULE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.ExecuteNonQuery (string.Format (@"CREATE RULE {0}_delete AS
ON DELETE TO {0}
DO ALSO DELETE FROM modification
  WHERE modificationid = OLD.modificationid;",
                                               TableName.OLD_SEQUENCE_DETECTION));
    }
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // 1. MonitoredMachine
      if (!Database.TableExists (TableName.MONITORED_MACHINE)) {
        AddMonitoredMachineTable ();
      }
      
      // 2. MachineModule
      if (!Database.TableExists (TableName.MACHINE_MODULE)) {
        AddMachineModuleTable ();
      }

      // 4. MachineMode
      Database.ExecuteNonQuery ("DROP VIEW IF EXISTS machinemode CASCADE;");
      if (!Database.TableExists (TableName.MACHINE_MODE)) {
        AddMachineModeTable ();
      }
      
      // 5. create the table MachineModuleStatus
      if (!Database.TableExists (TableName.OLD_MACHINE_MODULE_STATUS)) {
        AddMachineModuleStatusTable ();
      }
      
      // 6. Create the table processdetection
      if (!Database.TableExists (TableName.OLD_SEQUENCE_DETECTION)) {
        AddProcessDetectionTable ();
      }
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      if (Database.TableExists ("processmachinemoduleasscoation")) {
        Database.ExecuteNonQuery (@"DELETE FROM modification
WHERE modificationreferencedtable='ProcessMachineModuleAssociation'");
        Database.RemoveTable ("processmachinemoduleassociation");
      }
      if (Database.TableExists (TableName.OLD_SEQUENCE_DETECTION)) {
        Database.ExecuteNonQuery (@"DELETE FROM modification
WHERE modificationreferencedtable='ProcessDetection'");
        Database.RemoveTable (TableName.OLD_SEQUENCE_DETECTION);
      }

      if (Database.TableExists (TableName.OLD_MACHINE_MODULE_STATUS)) {
        Database.RemoveTable (TableName.OLD_MACHINE_MODULE_STATUS);
      }
      if (Database.TableExists (TableName.MACHINE_MODE)) {
        Database.RemoveTable (TableName.MACHINE_MODE);
      }
      if (Database.TableExists (TableName.MACHINE_MODULE)) {
        Database.RemoveTable (TableName.MACHINE_MODULE);
      }
      if (Database.TableExists (TableName.MONITORED_MACHINE)) {
        Database.RemoveTable (TableName.MONITORED_MACHINE);
      }
      
      // Obsolete translation data
      Database.Delete (TableName.TRANSLATION,
                       ColumnName.TRANSLATION_KEY,
                       "MachineModeInactive");
      Database.Delete (TableName.TRANSLATION,
                       ColumnName.TRANSLATION_KEY,
                       "MachineModeActive");
      Database.Delete (TableName.TRANSLATION,
                       ColumnName.TRANSLATION_KEY,
                       "MachineModeAuto");
      Database.Delete (TableName.TRANSLATION,
                       ColumnName.TRANSLATION_KEY,
                       "MachineModeManual");
      Database.Delete (TableName.TRANSLATION,
                       ColumnName.TRANSLATION_KEY,
                       "MachineModeJog");
      Database.Delete (TableName.TRANSLATION,
                       ColumnName.TRANSLATION_KEY,
                       "MachineModeMDI");
      Database.Delete (TableName.TRANSLATION,
                       ColumnName.TRANSLATION_KEY,
                       "MachineModeSingleBlock");
    }
  }
}
