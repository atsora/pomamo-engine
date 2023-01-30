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
  /// Migration 028: add the following tables
  /// <item>machineobservationstate</item>
  /// <item></item>
  /// </summary>
  [Migration(28)]
  public class MachineObservationState: MigrationExt
  {
    static readonly string MACHINE_OBSERVATION_STATE_NAME = "MachineObservationStateName";
    static readonly string MACHINE_OBSERVATION_STATE_TRANSLATION_KEY = "MachineObservationStateTranslationKey";
    static readonly string MACHINE_OBSERVATION_STATE_USER_REQUIRED = "MachineObservationStateUserRequired";
    static readonly string MACHINE_OBSERVATION_STATE_ON_SITE = "MachineObservationStateOnSite";
    static readonly string MACHINE_OBSERVATION_STATE_ID_SITE_ATTENDANCE_CHANGE = "MachineObservationStateIdSiteAttendanceChange";
    
    static readonly ILog log = LogManager.GetLogger(typeof (MachineObservationState).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (!Database.TableExists (TableName.USER_ATTENDANCE)) {
        AddUserAttendanceTable ();
      }
      
      if (!Database.TableExists (TableName.USER_SLOT)) {
        AddUserSlotTable ();
      }
      
      if (!Database.TableExists (TableName.MACHINE_OBSERVATION_STATE)) {
        AddMachineObservationStateTable ();
      }
      
      // Foreign key for table machinemoduleactivitysummary
      Database.GenerateForeignKey (TableName.MACHINE_ACTIVITY_SUMMARY, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   TableName.MACHINE_OBSERVATION_STATE, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      
      if (!Database.TableExists (TableName.MACHINE_OBSERVATION_STATE_ASSOCIATION)) {
        AddMachineObservationStateAssociationTable ();
      }
      
      if (!Database.TableExists (TableName.OBSERVATION_STATE_SLOT)) {
        AddObservationStateSlotTable ();
      }
      
      if (Database.TableExists ("usermachineassociation")) {
        // Table replaced by machineobservationstateassociation
        Database.ExecuteNonQuery (@"DELETE FROM modification
WHERE modificationreferencedtable='UserMachineAssociation'");
        Database.RemoveTable ("usermachineassociation");
      }
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      if (Database.TableExists (TableName.OBSERVATION_STATE_SLOT)){
        RemoveObservationStateSlotTable ();
      }
      
      if (Database.TableExists (TableName.MACHINE_OBSERVATION_STATE_ASSOCIATION)) {
        RemoveMachineObservationStateAssociationTable ();
      }
      
      if (Database.TableExists (TableName.MACHINE_OBSERVATION_STATE)) {
        RemoveMachineObservationStateTable ();
      }
      
      if (Database.TableExists (TableName.USER_SLOT)) {
        RemoveUserSlotTable ();
      }
      
      if (Database.TableExists (TableName.USER_ATTENDANCE)) {
        RemoveUserAttendanceTable ();
      }
    }
    
    void AddUserAttendanceTable ()
    {
      Database.AddTable (TableName.USER_ATTENDANCE,
                         new Column (ColumnName.MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (ColumnName.USER_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column ("UserAttendanceBegin", DbType.DateTime),
                         new Column ("UserAttendanceEnd", DbType.DateTime));
      AddOneNotNullConstraint (TableName.USER_ATTENDANCE,
                               "userattendancebegin",
                               "userattendanceend");
      Database.GenerateForeignKey (TableName.USER_ATTENDANCE, ColumnName.MODIFICATION_ID,
                                   TableName.MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.USER_ATTENDANCE, ColumnName.USER_ID,
                                   TableName.USER, ColumnName.USER_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      SetModificationTable (TableName.USER_ATTENDANCE);
      AddIndex (TableName.USER_ATTENDANCE,
                ColumnName.USER_ID);
    }
    
    void AddUserSlotTable ()
    {
      Database.AddTable (TableName.USER_SLOT,
                         new Column (ColumnName.USER_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column ("UserSlotBeginDateTime", DbType.DateTime, ColumnProperty.PrimaryKey),
                         new Column ("UserSlotBeginDay", DbType.Date, ColumnProperty.NotNull),
                         new Column ("UserSlotEndDateTime", DbType.DateTime),
                         new Column ("UserSlotEndDay", DbType.Date));
      Database.GenerateForeignKey (TableName.USER_SLOT, ColumnName.USER_ID,
                                   TableName.USER, ColumnName.USER_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddIndex (TableName.USER_SLOT,
                ColumnName.USER_ID);
      AddIndex (TableName.USER_SLOT,
                ColumnName.USER_ID,
                "UserSlotBeginDay");
    }
    
    void AddMachineObservationStateTable ()
    {
      Database.AddTable (TableName.MACHINE_OBSERVATION_STATE,
                         new Column (ColumnName.MACHINE_OBSERVATION_STATE_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (MACHINE_OBSERVATION_STATE_NAME, DbType.String, ColumnProperty.Unique),
                         new Column (MACHINE_OBSERVATION_STATE_TRANSLATION_KEY, DbType.String, ColumnProperty.Unique),
                         new Column (MACHINE_OBSERVATION_STATE_USER_REQUIRED, DbType.Boolean, ColumnProperty.NotNull),
                         new Column (MACHINE_OBSERVATION_STATE_ON_SITE, DbType.Boolean),
                         new Column (MACHINE_OBSERVATION_STATE_ID_SITE_ATTENDANCE_CHANGE, DbType.Int32));
      MakeColumnCaseInsensitive (TableName.MACHINE_OBSERVATION_STATE,
                                 MACHINE_OBSERVATION_STATE_NAME);
      AddConstraintNameTranslationKey (TableName.MACHINE_OBSERVATION_STATE,
                                       MACHINE_OBSERVATION_STATE_NAME,
                                       MACHINE_OBSERVATION_STATE_TRANSLATION_KEY);
      Database.GenerateForeignKey (TableName.MACHINE_OBSERVATION_STATE, MACHINE_OBSERVATION_STATE_ID_SITE_ATTENDANCE_CHANGE,
                                   TableName.MACHINE_OBSERVATION_STATE, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      AddUniqueIndex (TableName.MACHINE_OBSERVATION_STATE,
                      MACHINE_OBSERVATION_STATE_NAME,
                      MACHINE_OBSERVATION_STATE_TRANSLATION_KEY);
      // List of machine work types
      Database.Insert (TableName.TRANSLATION,
                       new string [] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string [] {"", "MachineObservationStateAttended", "Machine ON with operator (attended)"});
      Database.Insert (TableName.MACHINE_OBSERVATION_STATE,
                       new string [] {ColumnName.MACHINE_OBSERVATION_STATE_ID, MACHINE_OBSERVATION_STATE_TRANSLATION_KEY, MACHINE_OBSERVATION_STATE_USER_REQUIRED, MACHINE_OBSERVATION_STATE_ON_SITE},
                       new string [] {"1", "MachineObservationStateAttended", "1", "1"}); // id = 1 => attended
      Database.Insert (TableName.TRANSLATION,
                       new string [] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string [] {"", "MachineObservationStateUnattended", "Machine ON without operator (unattended)"});
      Database.Insert (TableName.MACHINE_OBSERVATION_STATE,
                       new string [] {ColumnName.MACHINE_OBSERVATION_STATE_ID, MACHINE_OBSERVATION_STATE_TRANSLATION_KEY, MACHINE_OBSERVATION_STATE_USER_REQUIRED},
                       new string [] {"2", "MachineObservationStateUnattended", "0"}); // id = 2 => unattended
      Database.Insert (TableName.TRANSLATION,
                       new string [] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string [] {"", "MachineObservationStateOnSite", "Machine ON with operator (on-site)"});
      Database.Insert (TableName.MACHINE_OBSERVATION_STATE,
                       new string [] {ColumnName.MACHINE_OBSERVATION_STATE_ID, MACHINE_OBSERVATION_STATE_TRANSLATION_KEY, MACHINE_OBSERVATION_STATE_USER_REQUIRED, MACHINE_OBSERVATION_STATE_ON_SITE},
                       new string [] {"3", "MachineObservationStateOnSite", "1", "1"}); // id = 3 => on-site
      Database.Insert (TableName.TRANSLATION,
                       new string [] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string [] {"", "MachineObservationStateOnCall", "Machine ON with on call operator (off-site)"});
      Database.Insert (TableName.MACHINE_OBSERVATION_STATE,
                       new string [] {ColumnName.MACHINE_OBSERVATION_STATE_ID, MACHINE_OBSERVATION_STATE_TRANSLATION_KEY, MACHINE_OBSERVATION_STATE_USER_REQUIRED, MACHINE_OBSERVATION_STATE_ON_SITE},
                       new string [] {"4", "MachineObservationStateOnCall", "1", "0"}); // id = 4 => on-call
      Database.Insert (TableName.TRANSLATION,
                       new string [] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string [] {"", "MachineObservationStateOff", "Machine OFF"});
      Database.Insert (TableName.MACHINE_OBSERVATION_STATE,
                       new string [] {ColumnName.MACHINE_OBSERVATION_STATE_ID, MACHINE_OBSERVATION_STATE_TRANSLATION_KEY, MACHINE_OBSERVATION_STATE_USER_REQUIRED},
                       new string [] {"5", "MachineObservationStateOff", "0"}); // id = 5 => off
      Database.Insert (TableName.TRANSLATION,
                       new string [] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string [] {"", "MachineObservationStateUnknown", "Unknown"});
      Database.Insert (TableName.MACHINE_OBSERVATION_STATE,
                       new string [] {ColumnName.MACHINE_OBSERVATION_STATE_ID, MACHINE_OBSERVATION_STATE_TRANSLATION_KEY, MACHINE_OBSERVATION_STATE_USER_REQUIRED},
                       new string [] {"6", "MachineObservationStateUnknown", "0"}); // id = 6 => unknown
      ResetSequence (TableName.MACHINE_OBSERVATION_STATE,
                     ColumnName.MACHINE_OBSERVATION_STATE_ID);
      // Site attendance change
      // - 1 attended => 2 unattended
      Database.Update (TableName.MACHINE_OBSERVATION_STATE,
                       new string [] {MACHINE_OBSERVATION_STATE_ID_SITE_ATTENDANCE_CHANGE},
                       new string [] {"2"},
                       string.Format ("{0}={1}",
                                      ColumnName.MACHINE_OBSERVATION_STATE_ID, 1));
      // - 3 on-site => 2 unattended
      Database.Update (TableName.MACHINE_OBSERVATION_STATE,
                       new string [] {MACHINE_OBSERVATION_STATE_ID_SITE_ATTENDANCE_CHANGE},
                       new string [] {"2"},
                       string.Format ("{0}={1}",
                                      ColumnName.MACHINE_OBSERVATION_STATE_ID, 3));
      // - 4 on-call => 3 on-site
      Database.Update (TableName.MACHINE_OBSERVATION_STATE,
                       new string [] {MACHINE_OBSERVATION_STATE_ID_SITE_ATTENDANCE_CHANGE},
                       new string [] {"3"},
                       string.Format ("{0}={1}",
                                      ColumnName.MACHINE_OBSERVATION_STATE_ID, 4));
    }
    
    void AddMachineObservationStateAssociationTable ()
    {
      Database.AddTable (TableName.MACHINE_OBSERVATION_STATE_ASSOCIATION,
                         new Column (ColumnName.MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.MACHINE_OBSERVATION_STATE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.USER_ID, DbType.Int32),
                         new Column ("MachineObservationStateAssociationBegin", DbType.DateTime, ColumnProperty.NotNull),
                         new Column ("MachineObservationStateAssociationEnd", DbType.DateTime));
      Database.GenerateForeignKey (TableName.MACHINE_OBSERVATION_STATE_ASSOCIATION, ColumnName.MODIFICATION_ID,
                                   TableName.MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.MACHINE_OBSERVATION_STATE_ASSOCIATION, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.MACHINE_OBSERVATION_STATE_ASSOCIATION, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   TableName.MACHINE_OBSERVATION_STATE, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.MACHINE_OBSERVATION_STATE_ASSOCIATION, ColumnName.USER_ID,
                                   TableName.USER, ColumnName.USER_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      SetModificationTable (TableName.MACHINE_OBSERVATION_STATE_ASSOCIATION);
      AddIndex (TableName.MACHINE_OBSERVATION_STATE_ASSOCIATION,
                ColumnName.MACHINE_ID);
      AddIndexCondition (TableName.MACHINE_OBSERVATION_STATE_ASSOCIATION,
                         string.Format ("{0} IS NOT NULL", ColumnName.USER_ID),
                         ColumnName.USER_ID);
    }
    
    void AddObservationStateSlotTable ()
    {
      Database.AddTable (TableName.OBSERVATION_STATE_SLOT,
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column ("ObservationStateSlotBeginDateTime", DbType.DateTime, ColumnProperty.PrimaryKey),
                         new Column ("ObservationStateSlotBeginDay", DbType.Date, ColumnProperty.NotNull),
                         new Column ("ObservationStateSlotEndDateTime", DbType.DateTime),
                         new Column ("ObservationStateSlotEndDay", DbType.Date),
                         new Column (ColumnName.MACHINE_OBSERVATION_STATE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.USER_ID, DbType.Int32));
      Database.GenerateForeignKey (TableName.OBSERVATION_STATE_SLOT, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.OBSERVATION_STATE_SLOT, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   TableName.MACHINE_OBSERVATION_STATE, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.OBSERVATION_STATE_SLOT, ColumnName.USER_ID,
                                   TableName.USER, ColumnName.USER_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddIndex (TableName.OBSERVATION_STATE_SLOT,
                ColumnName.MACHINE_ID);
      AddIndex (TableName.OBSERVATION_STATE_SLOT,
                ColumnName.MACHINE_ID,
                "ObservationStateSlotBeginDay");
      AddIndexCondition (TableName.OBSERVATION_STATE_SLOT,
                         string.Format ("{0} IS NOT NULL", ColumnName.USER_ID),
                         ColumnName.USER_ID,
                         "ObservationStateSlotBeginDay");
      // Initialize the table (default is 6: unknown)
      Database.ExecuteNonQuery (@"INSERT INTO observationstateslot (machineid, observationstateslotbegindatetime, observationstateslotbeginday, machineobservationstateid)
SELECT machineid, '1970-01-01 00:00:00', '1970-01-01', 6
FROM machine");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE FUNCTION machine_observationstateslot_inserter()
  RETURNS trigger AS
$BODY$
BEGIN
  INSERT INTO observationstateslot (machineid, observationstateslotbegindatetime, observationstateslotbeginday, machineobservationstateid)
    VALUES (NEW.machineid, '1970-01-01 00:00:00', '1970-01-01', 6);
  RETURN NEW;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100");
      Database.ExecuteNonQuery (@"CREATE TRIGGER machine_observationstateslot_inserter_trigger
AFTER INSERT ON machine
FOR EACH ROW
EXECUTE PROCEDURE machine_observationstateslot_inserter()");
    }
    
    void RemoveObservationStateSlotTable ()
    {
      Database.ExecuteNonQuery (@"DROP TRIGGER IF EXISTS machine_observationstateslot_inserter_trigger ON machine");
      Database.ExecuteNonQuery (@"DROP FUNCTION IF EXISTS machine_observationstateslot_inserter()");
      Database.RemoveTable (TableName.OBSERVATION_STATE_SLOT);
    }

    void RemoveMachineObservationStateAssociationTable ()
    {
      Database.ExecuteNonQuery (@"DELETE FROM modification
WHERE modificationreferencedtable='MachineObservationStateAssociation'");
      Database.RemoveTable (TableName.MACHINE_OBSERVATION_STATE_ASSOCIATION);
    }
    
    void RemoveMachineObservationStateTable ()
    {
      Database.RemoveTable (TableName.MACHINE_OBSERVATION_STATE);
      // Remove the translations
      Database.Delete (TableName.TRANSLATION,
                       ColumnName.TRANSLATION_KEY,
                       "MachineObservationStateAttended");
      Database.Delete (TableName.TRANSLATION,
                       ColumnName.TRANSLATION_KEY,
                       "MachineObservationStateUnattended");
      Database.Delete (TableName.TRANSLATION,
                       ColumnName.TRANSLATION_KEY,
                       "MachineObservationStateOnSite");
      Database.Delete (TableName.TRANSLATION,
                       ColumnName.TRANSLATION_KEY,
                       "MachineObservationStateOnCall");
      Database.Delete (TableName.TRANSLATION,
                       ColumnName.TRANSLATION_KEY,
                       "MachineObservationStateOff");
    }
    
    void RemoveUserSlotTable ()
    {
      Database.RemoveTable (TableName.USER_SLOT);
    }
    
    void RemoveUserAttendanceTable ()
    {
      RemoveModificationTable ("UserAttendance");
    }
  }
}
