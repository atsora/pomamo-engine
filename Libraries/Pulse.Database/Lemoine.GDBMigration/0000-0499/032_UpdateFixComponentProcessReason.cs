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
  /// <item>Add the reasondetails columns</item>
  /// <item>Add the machine mode color</item>
  /// <item>Update the foreign keys of the processdetection table</item>
  /// <item>Add an index for modificationdatetime / modificationid in the modification table</item>
  /// <item>Update the indexes in Fact</item>
  /// <item>Add the undefined reason</item>
  /// <item>Make the color column of the reason table not null (default is yellow)</item>
  /// <item>Make the color column of the reasongroup table not null (default is yellow)</item>
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
