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
  /// Migration 032:
  /// <item>fix the sfkprocess view</item>
  /// <item>Add the reasondetails columns</item>
  /// <item>Add the machine mode color</item>
  /// <item>Update the foreign keys of the processdetection table</item>
  /// <item>Add an index for modificationdatetime / modificationid in the modification table</item>
  /// <item>Update the indexes in Fact</item>
  /// <item>Add the undefined reason</item>
  /// <item>Fix sfkmachcomm_inserter</item>
  /// <item>Make the color column of the reason table not null (default is yellow)</item>
  /// <item>Make the color column of the reasongroup table not null (default is yellow)</item>
  /// <item>Remove the last four columns of sfkmachcomm</item>
  /// </summary>
  [Migration(32)]
  public class UpdateFixComponentProcessReason: MigrationExt
  {
    static readonly string MACHINE_MODE_COLOR = "MachineModeColor";
    static readonly string REASON_TRANSLATION_KEY = "ReasonTranslationKey";
    static readonly string REASON_COLOR = "ReasonColor";
        
    static readonly ILog log = LogManager.GetLogger(typeof (UpdateFixComponentProcessReason).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      FixSfkProcessView ();
      FixReasonDetailsColumns ();
      AddMachineModeColor ();
      FixProcessDetectionForeignKeys ();
      AddIndexModificationTable ();
      AddUndefinedReason ();
      AddReasonColorMandatory ();
      AddReasonGroupColorMandatory ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveUndefinedReason ();
      UndoAddIndexModificationTable ();
    }
    
    void FixSfkProcessView ()
    {
      Database.ExecuteNonQuery (@"CREATE OR REPLACE VIEW sfkprocess AS
 SELECT operation.operationid AS id, operation.operationname AS name, operation.operationtypeid - 1 AS processtypeid,
 CASE WHEN componentintermediateworkpiece.componentid IS NULL THEN 0 ELSE componentintermediateworkpiece.componentid END AS componentid,
 1 AS ordernb, operation.operationestimatedmachininghours AS hours, 0 AS completed
   FROM operation
NATURAL JOIN intermediateworkpiece
LEFT OUTER JOIN componentintermediateworkpiece USING (intermediateworkpieceid);");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE FUNCTION sfkprocess_inserter()
  RETURNS trigger AS
$BODY$
DECLARE
varintermediateworkpieceid int8;
BEGIN
  INSERT INTO operation (operationname, operationtypeid,
    operationestimatedmachininghours)
  VALUES (NEW.name, NEW.processtypeid+1, NEW.hours)
  RETURNING operationid AS id INTO STRICT NEW.id;
  INSERT INTO intermediateworkpiece (operationid)
  VALUES (NEW.id)
  RETURNING intermediateworkpieceid
  INTO STRICT varintermediateworkpieceid;
  IF NEW.componentid<>0 AND NEW.componentid IS NOT NULL THEN
    INSERT INTO componentintermediateworkpiece (componentid, intermediateworkpieceid)
    VALUES (NEW.componentid, varintermediateworkpieceid);
  END IF;
  RETURN NEW;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE COST 100;");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE FUNCTION sfkprocess_updater()
  RETURNS trigger AS
$BODY$
BEGIN
  UPDATE operation
  SET operationname=NEW.name, operationtypeid=NEW.processtypeid+1,
    operationestimatedmachininghours=NEW.hours
  WHERE operationid = OLD.id;
  UPDATE componentintermediateworkpiece
  SET componentid=NEW.componentid
  WHERE OLD.componentid<>0 AND OLD.componentid IS NOT NULL
    AND componentid=OLD.componentid AND intermediateworkpieceid= (SELECT intermediateworkpieceid FROM intermediateworkpiece WHERE operationid=OLD.id);
  RETURN NEW;
END;
$BODY$
  LANGUAGE plpgsql VOLATILE COST 100;");
    }

    void FixReasonDetailsColumns ()
    {
      if (!Database.ColumnExists (TableName.REASON_MACHINE_ASSOCIATION,
                                  ColumnName.REASON_DETAILS)) {
        Database.AddColumn (TableName.REASON_MACHINE_ASSOCIATION,
                            ColumnName.REASON_DETAILS,
                            DbType.String);
      }
      if (!Database.ColumnExists (TableName.REASON_SLOT,
                                  ColumnName.REASON_DETAILS)) {
        Database.AddColumn (TableName.REASON_SLOT,
                            ColumnName.REASON_DETAILS,
                            DbType.String);
      }
    }
    
    void AddMachineModeColor ()
    {
      if (!Database.ColumnExists (TableName.MACHINE_MODE,
                                  MACHINE_MODE_COLOR)) {
        Database.AddColumn (TableName.MACHINE_MODE,
                            MACHINE_MODE_COLOR,
                            DbType.String,
                            7);
        // Default colors:
        Database.ExecuteNonQuery (@"UPDATE machinemode
SET machinemodecolor='#008000'
WHERE machinemoderunning=TRUE"); // running => green
        Database.ExecuteNonQuery (@"UPDATE machinemode
SET machinemodecolor='#FFFF00'
WHERE machinemodetranslationkey='MachineModeInactive'"); // inactive => yellow
        Database.ExecuteNonQuery (@"UPDATE machinemode
SET machinemodecolor='#FFFF00'
WHERE machinemodetranslationkey='MachineModeNoData'"); // no data => yellow
        Database.ExecuteNonQuery (@"UPDATE machinemode
SET machinemodecolor='#736F6E'
WHERE machinemodetranslationkey='MachineModeUnavailable'"); // unavailable => grey
        Database.ExecuteNonQuery (@"UPDATE machinemode
SET machinemodecolor='#FF0000'
WHERE machinemodetranslationkey='MachineModeError'"); // error => red
      }
    }
    
    void FixProcessDetectionForeignKeys ()
    {
      Database.GenerateForeignKey (TableName.OLD_SEQUENCE_DETECTION, ColumnName.OLD_SEQUENCE_ID,
                                   TableName.OLD_SEQUENCE, ColumnName.OLD_SEQUENCE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
    }
    
    void AddIndexModificationTable ()
    {
      AddUniqueIndex (TableName.MODIFICATION,
                      ColumnName.MODIFICATION_DATETIME,
                      ColumnName.MODIFICATION_ID);
    }
    
    void UndoAddIndexModificationTable ()
    {
      Database.ExecuteNonQuery ("DROP INDEX IF EXISTS modification_modificationdatetime_modificationid_idx");
    }
    
    void AddUndefinedReason ()
    {
      Database.Insert (TableName.REASON,
                       new string [] {ColumnName.REASON_ID, REASON_TRANSLATION_KEY, REASON_COLOR},
                       new string [] {"1", "UndefinedValue", "#FFC0CB"}); // id = 1 (pink)
    }
    
    void RemoveUndefinedReason ()
    {
      Database.Delete (TableName.REASON,
                       new string [] {ColumnName.REASON_ID},
                       new string [] {"1"});
    }
    
    void AddReasonColorMandatory ()
    {
      Database.ExecuteNonQuery (@"UPDATE reason
SET reasoncolor='#FFFF00'
WHERE reasoncolor IS NULL"); // yellow
      Database.ExecuteNonQuery (@"ALTER TABLE reason
ALTER reasoncolor SET NOT NULL"); // reasoncolor NOT NULL
      Database.ExecuteNonQuery (@"ALTER TABLE reason
ALTER reasoncolor SET DEFAULT '#FFFF00'"); // Default is yellow
    }
    
    void AddReasonGroupColorMandatory ()
    {
      Database.ExecuteNonQuery (@"UPDATE reasongroup
SET reasongroupcolor='#FFFF00'
WHERE reasongroupcolor IS NULL"); // yellow
      Database.ExecuteNonQuery (@"ALTER TABLE reasongroup
ALTER reasongroupcolor SET NOT NULL"); // reasongroupcolor NOT NULL
      Database.ExecuteNonQuery (@"ALTER TABLE reasongroup
ALTER reasongroupcolor SET DEFAULT '#FFFF00'"); // Default is yellow
    }
  }
}
