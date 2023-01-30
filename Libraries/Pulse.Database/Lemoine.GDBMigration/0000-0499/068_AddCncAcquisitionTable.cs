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
  /// Migration 068: add the new Cnc Acquisition table
  /// <item>migrate sfkmachcomm to cncacquisition</item>
  /// <item>update machinemodule</item>
  /// <item>make machine module point to the new cncacquisition table</item>
  /// <item>trigger to update sfkmach from monitored machine</item>
  /// </summary>
  [Migration(68)]
  public class AddCncAcquisitionTable: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddCncAcquisitionTable).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (Database.TableExists (TableName.SFK_MACH)) {
        FixSfkmach ();
      }
      AddCncAcquisition ();
      UpgradeMachineModuleTable ();
      UpgradeTriggers ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      DowngradeTriggers ();
      DowngradeMachineModuleTable ();
      RemoveCncAcquisition ();
    }
    
    void FixSfkmach ()
    {
      if (Database.ColumnExists (TableName.SFK_MACH, "jitterchannelwitdh")) {
        Database.RenameColumn (TableName.SFK_MACH,
                               "jitterchannelwitdh",
                               "jitterchannelwidth");
      }
    }
    
    void AddCncAcquisition ()
    {
      Database.AddTable (TableName.CNC_ACQUISITION,
                         new Column (ColumnName.CNC_ACQUISITION_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column ("cncacquisitionversion", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column ("cncacquisitionname", DbType.String),
                         new Column ("cncacquisitionconfigfile", DbType.String),
                         new Column ("cncacquisitionconfigprefix", DbType.String, ColumnProperty.NotNull, "''"),
                         new Column ("cncacquisitionconfigparameters", DbType.String),
                         new Column ("cncacquisitionuseprocess", DbType.Boolean, ColumnProperty.NotNull, false),
                         new Column ("computerid", DbType.Int32, ColumnProperty.NotNull),
                         new Column ("cncacquisitionevery", DbType.Double, ColumnProperty.NotNull, 2),
                         new Column ("cncacquisitionnotrespondingtimeout", DbType.Double, ColumnProperty.NotNull, 120),
                         new Column ("cncacquisitionsleepbeforerestart", DbType.Double, ColumnProperty.NotNull, 10));
      AddUniqueConstraint (TableName.CNC_ACQUISITION,
                           "cncacquisitionname");
      Database.GenerateForeignKey (TableName.CNC_ACQUISITION, ColumnName.COMPUTER_ID,
                                   TableName.COMPUTER, ColumnName.COMPUTER_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
    }
    
    void RemoveCncAcquisition ()
    {
      Database.RemoveTable (TableName.CNC_ACQUISITION);      
    }
    
    void UpgradeMachineModuleTable ()
    {
      Database.RemoveColumn (TableName.MACHINE_MODULE,
                             "machinemoduleconfiguration");
      Database.RemoveColumn (TableName.MACHINE_MODULE,
                             "machinemoduleparameters");
      Database.AddColumn (TableName.MACHINE_MODULE,
                          new Column (ColumnName.CNC_ACQUISITION_ID, DbType.Int32));
      Database.GenerateForeignKey (TableName.MACHINE_MODULE, ColumnName.CNC_ACQUISITION_ID,
                                   TableName.CNC_ACQUISITION, ColumnName.CNC_ACQUISITION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      Database.AddColumn (TableName.MACHINE_MODULE,
                          new Column ("machinemoduleconfigprefix", DbType.String, ColumnProperty.NotNull, "''"));
      Database.AddColumn (TableName.MACHINE_MODULE,
                          new Column ("machinemoduleconfigparameters", DbType.String));
      AddUniqueConstraint (TableName.MACHINE_MODULE,
                           ColumnName.CNC_ACQUISITION_ID,
                           "machinemoduleconfigprefix");
      Database.ExecuteNonQuery (@"
UPDATE machinemodule
SET cncacquisitionid=cncacquisition.cncacquisitionid
FROM cncacquisition
WHERE cncacquisition.cncacquisitionid=machinemoduleid");
    }
    
    void DowngradeMachineModuleTable ()
    {
      Database.RemoveColumn (TableName.MACHINE_MODULE,
                             "machinemoduleconfigparameters");
      Database.RemoveColumn (TableName.MACHINE_MODULE,
                             "machinemoduleconfigprefix");
      Database.RemoveColumn (TableName.MACHINE_MODULE,
                             ColumnName.CNC_ACQUISITION_ID);
    }
    
    void UpgradeTriggers ()
    {
      Database.ExecuteNonQuery (@"DROP FUNCTION IF EXISTS sfkmach_insert_updater() CASCADE");

      if (Database.TableExists (TableName.SFK_MACH)) {
        Database.ExecuteNonQuery (@"CREATE OR REPLACE FUNCTION monitoredmachine_insert_updater()
  RETURNS trigger AS
$BODY$
BEGIN
INSERT INTO sfkmach (machid, machname, disp_prio,
 postid, machpostid, guyid, machserenc, machirpm, machmetric, machobsolete, machlogpc, sfkspvid,
 sfkmode, sfkthresh, mclassid, firstevt, sfktype, sfkcost, activeflag, manualflag, active_negate,
 manual_negate, first_feed_evt, rpm_below, rpm_threshold, no_chupchick, g0g1_thresh, opreset_time,
 machtypeid, tpfilter, montype, jitterchannelwidth, spindleloadbelow, spindleloadthreshold, cncignorenoconnect,
 rotthreshold, lightactivitycheckforfull, spindleloadindex)
 SELECT machineid, machinename, CASE WHEN machinedisplaypriority IS NULL THEN 1 ELSE machinedisplaypriority END,
 0, 0, 0, 0, 4294967295, 1, 0, '', 0,
 0, 0.25, 0, now(), 0, 1, 0, -1, 0,
 0, now(), 0, 0, 1, 0, 600, 0, 8, 1, 0.1, 0, 0, 1,
 0.25, 0, -1
 FROM machine
 WHERE machineid=NEW.machineid;
RETURN NEW;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;");
        Database.ExecuteNonQuery (@"CREATE TRIGGER monitoredmachine_inserter
  AFTER INSERT
  ON monitoredmachine
  FOR EACH ROW
  EXECUTE PROCEDURE monitoredmachine_insert_updater();");

        Database.ExecuteNonQuery (@"CREATE OR REPLACE FUNCTION machine_update_rename()
  RETURNS trigger AS
$BODY$
BEGIN
UPDATE sfkmach
SET machname=NEW.machinename
WHERE machid=NEW.machineid;
RETURN NEW;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;");
        Database.ExecuteNonQuery (@"CREATE TRIGGER machine_update
  AFTER UPDATE
  ON machine
  FOR EACH ROW
  EXECUTE PROCEDURE machine_update_rename();");
      }
    }
    
    void DowngradeTriggers ()
    {
      Database.ExecuteNonQuery (@"DROP FUNCTION IF EXISTS monitoredmachine_insert_updater() CASCADE");
      Database.ExecuteNonQuery (@"DROP FUNCTION IF EXISTS machine_update_rename() CASCADE");
      
      Database.ExecuteNonQuery (@"CREATE OR REPLACE FUNCTION sfkmach_insert_updater()
  RETURNS trigger AS
$BODY$
BEGIN
INSERT INTO machine (machineid, machinename, machinemonitoringtypeid, machinedisplaypriority)
 VALUES (DEFAULT, NEW.machname, 2, NEW.disp_prio)
 RETURNING machineid INTO NEW.machid;
INSERT INTO monitoredmachine (machineid)
 VALUES (NEW.machid);
RETURN NEW;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE
  COST 100;");
      Database.ExecuteNonQuery (@"CREATE TRIGGER sfkmach_updater
  BEFORE INSERT
  ON sfkmach
  FOR EACH ROW
  EXECUTE PROCEDURE sfkmach_insert_updater();");
    }
  }
}
