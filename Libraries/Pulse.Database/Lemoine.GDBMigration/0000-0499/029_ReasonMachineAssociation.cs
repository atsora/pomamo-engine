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
  /// Migration 029: add the following tables
  /// <item>reasongroup</item>
  /// <item>reason</item>
  /// <item>machinemodedefaultreason</item>
  /// <item>reasonselection</item>
  /// <item>reasonmachineassociation</item>
  /// <item>machinereasonslot</item>
  /// <item>machinestatus</item>
  /// <item>machinereasonsummary</item>
  /// <item></item>
  /// </summary>
  [Migration(29)]
  public class ReasonMachineAssociation: MigrationExt
  {
    static readonly string REASON_GROUP_NAME = "ReasonGroupName";
    static readonly string REASON_GROUP_TRANSLATION_KEY = "ReasonGroupTranslationKey";
    static readonly string REASON_GROUP_COLOR = "ReasonGroupColor";

    static readonly string REASON_NAME = "ReasonName";
    static readonly string REASON_CODE = "ReasonCode";
    static readonly string REASON_TRANSLATION_KEY = "ReasonTranslationKey";
    static readonly string REASON_COLOR = "ReasonColor";
    
    static readonly string DEFAULT_REASON_MAXIMUM_DURATION = "DefaultReasonMaximumDuration";
    
    static readonly string REASON_MACHINE_ASSOCIATION_END = "ReasonMachineAssociationEnd";
    
    static readonly string CNC_MACHINE_MODE_ID = "CncMachineModeId";
    static readonly string REASON_SLOT_END_DATETIME = "ReasonSlotEndDateTime";
    
    static readonly ILog log = LogManager.GetLogger(typeof (ReasonMachineAssociation).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery ("DROP VIEW IF EXISTS reasongroup CASCADE;");
      if (!Database.TableExists (TableName.REASON_GROUP)) {
        AddReasonGroupTable ();
      }
      
      Database.ExecuteNonQuery ("DROP VIEW IF EXISTS reason CASCADE;");
      if (!Database.TableExists (TableName.REASON)) {
        AddReasonTable ();
      }
      
      if (!Database.TableExists (TableName.MACHINE_MODE_DEFAULT_REASON)) {
        AddMachineModeDefaultReasonTable ();
      }
      
      if (!Database.TableExists (TableName.REASON_SELECTION)) {
        AddReasonSelectionTable ();
      }
      
      if (!Database.TableExists (TableName.REASON_MACHINE_ASSOCIATION)) {
        AddReasonMachineAssociationTable ();
      }
      
      Database.ExecuteNonQuery ("DROP VIEW IF EXISTS reasonslot CASCADE;");
      if (!Database.TableExists (TableName.REASON_SLOT)) {
        AddReasonSlotTable ();
      }
      
      Database.ExecuteNonQuery ("DROP VIEW IF EXISTS machinestatus CASCADE;");
      if (!Database.TableExists (TableName.MACHINE_STATUS)) {
        AddMachineStatusTable ();
      }
      
      if (!Database.TableExists (TableName.REASON_SUMMARY)) {
        AddReasonSummaryTable ();
      }
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      if (Database.TableExists (TableName.REASON_SUMMARY)) {
        RemoveReasonSummaryTable ();
      }
      
      if (Database.TableExists (TableName.MACHINE_STATUS)) {
        RemoveMachineStatusTable ();
      }
      
      if (Database.TableExists (TableName.REASON_SLOT)) {
        RemoveReasonSlotTable ();
      }
      
      if (Database.TableExists (TableName.REASON_MACHINE_ASSOCIATION)) {
        RemoveReasonMachineAssociationTable ();
      }
      
      if (Database.TableExists (TableName.REASON_GROUP)) {
        RemoveReasonGroupTable ();
      }
    }
    
    void AddReasonGroupTable ()
    {
      Database.AddTable (TableName.REASON_GROUP,
                         new Column (ColumnName.REASON_GROUP_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (REASON_GROUP_NAME, DbType.String, ColumnProperty.Unique),
                         new Column (REASON_GROUP_TRANSLATION_KEY, DbType.String, ColumnProperty.Unique),
                         new Column ("ReasonGroupDescription", DbType.String),
                         new Column ("ReasonGroupDescriptionTranslationKey", DbType.String),
                         new Column (REASON_GROUP_COLOR, DbType.String, 7, ColumnProperty.NotNull));
      MakeColumnCaseInsensitive (TableName.REASON_GROUP,
                                 REASON_GROUP_NAME);
      Database.ExecuteNonQuery (@"ALTER TABLE reasongroup
ALTER reasongroupcolor SET DEFAULT '#FFFF00'"); // Default is yellow
      AddConstraintNameTranslationKey (TableName.REASON_GROUP,
                                       REASON_GROUP_NAME,
                                       REASON_GROUP_TRANSLATION_KEY);
      AddConstraintColor (TableName.REASON_GROUP,
                          REASON_GROUP_COLOR);
      AddUniqueIndex (TableName.REASON_GROUP,
                      REASON_GROUP_NAME,
                      REASON_GROUP_TRANSLATION_KEY);
      Database.Insert (TableName.TRANSLATION,
                       new string [] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string [] {"", "ReasonGroupDefault", "Unclassified reasons"});
      Database.Insert (TableName.REASON_GROUP,
                       new string [] {ColumnName.REASON_GROUP_ID, REASON_GROUP_TRANSLATION_KEY, REASON_GROUP_COLOR},
                       new string [] {"1", "ReasonGroupDefault", "#FFFF00"}); // id = 1
      Database.Insert (TableName.TRANSLATION,
                       new string [] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string [] {"", "ReasonGroupMotion", "Motion"});
      Database.Insert (TableName.REASON_GROUP,
                       new string [] {ColumnName.REASON_GROUP_ID, REASON_GROUP_TRANSLATION_KEY, REASON_GROUP_COLOR},
                       new string [] {"2", "ReasonGroupMotion", "#008000"}); // id = 2 (green)
      Database.Insert (TableName.TRANSLATION,
                       new string [] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string [] {"", "ReasonGroupShort", "Short idle time"});
      Database.Insert (TableName.REASON_GROUP,
                       new string [] {ColumnName.REASON_GROUP_ID, REASON_GROUP_TRANSLATION_KEY, REASON_GROUP_COLOR},
                       new string [] {"3", "ReasonGroupShort", "#FFA500"}); // id = 3 (orange)
      Database.Insert (TableName.TRANSLATION,
                       new string [] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string [] {"", "ReasonGroupUnanswered", "Unanswered"});
      Database.Insert (TableName.REASON_GROUP,
                       new string [] {ColumnName.REASON_GROUP_ID, REASON_GROUP_TRANSLATION_KEY, REASON_GROUP_COLOR},
                       new string [] {"4", "ReasonGroupUnanswered", "#FFFF00"}); // id = 4 (yellow)
      Database.Insert (TableName.TRANSLATION,
                       new string [] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string [] {"", "ReasonGroupAuto", "Auto reasons"});
      Database.Insert (TableName.REASON_GROUP,
                       new string [] {ColumnName.REASON_GROUP_ID, REASON_GROUP_TRANSLATION_KEY, REASON_GROUP_COLOR},
                       new string [] {"5", "ReasonGroupAuto", "#FFFF00"}); // id = 5 (yellow)
      Database.Insert (TableName.TRANSLATION,
                       new string [] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string [] {"", "ReasonGroupIdle", "Idle"});
      Database.Insert (TableName.REASON_GROUP,
                       new string [] {ColumnName.REASON_GROUP_ID, REASON_GROUP_TRANSLATION_KEY, REASON_GROUP_COLOR},
                       new string [] {"6", "ReasonGroupIdle", "#FFFF00"}); // id = 6 (yellow)
      ResetSequence (TableName.REASON_GROUP,
                     ColumnName.REASON_GROUP_ID);
    }

    void RemoveReasonGroupTable ()
    {
      Database.RemoveTable (TableName.REASON_GROUP);
      // Remove the translations
      Database.Delete (TableName.TRANSLATION,
                       ColumnName.TRANSLATION_KEY,
                       "ReasonGroupDefault");
      Database.Delete (TableName.TRANSLATION,
                       ColumnName.TRANSLATION_KEY,
                       "ReasonGroupMotion");
      Database.Delete (TableName.TRANSLATION,
                       ColumnName.TRANSLATION_KEY,
                       "ReasonGroupShort");
      Database.Delete (TableName.TRANSLATION,
                       ColumnName.TRANSLATION_KEY,
                       "ReasonGroupUnanswered");
      Database.Delete (TableName.TRANSLATION,
                       ColumnName.TRANSLATION_KEY,
                       "ReasonGroupAuto");
    }

    void AddReasonTable ()
    {
      Database.AddTable (TableName.REASON,
                         new Column (ColumnName.REASON_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (REASON_NAME, DbType.String, ColumnProperty.Unique),
                         new Column (REASON_TRANSLATION_KEY, DbType.String, ColumnProperty.Unique),
                         new Column (REASON_CODE, DbType.String, ColumnProperty.Unique),
                         new Column ("ReasonDescription", DbType.String),
                         new Column ("ReasonDescriptionTranslationKey", DbType.String),
                         new Column (REASON_COLOR, DbType.String, 7, ColumnProperty.NotNull),
                         new Column ("ReasonLinkOperationDirection", DbType.Int32, ColumnProperty.NotNull, 0),
                         new Column (ColumnName.REASON_GROUP_ID, DbType.Int32, ColumnProperty.NotNull, 1));
      MakeColumnCaseInsensitive (TableName.REASON,
                                 REASON_NAME);
      MakeColumnCaseInsensitive (TableName.REASON,
                                 REASON_CODE);
      Database.ExecuteNonQuery (@"ALTER TABLE reason
ALTER reasoncolor SET DEFAULT '#FFFF00'"); // Default is yellow
      AddConstraintNameTranslationKey (TableName.REASON,
                                       REASON_NAME,
                                       REASON_TRANSLATION_KEY);
      AddConstraintColor (TableName.REASON,
                          REASON_COLOR);
      Database.GenerateForeignKey (TableName.REASON, ColumnName.REASON_GROUP_ID,
                                   TableName.REASON_GROUP, ColumnName.REASON_GROUP_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetDefault);
      AddIndex (TableName.REASON,
                ColumnName.REASON_GROUP_ID);
      AddUniqueIndex (TableName.REASON,
                      REASON_NAME,
                      REASON_TRANSLATION_KEY);
      AddUniqueIndexCondition (TableName.REASON,
                               string.Format ("{0} IS NOT NULL", REASON_CODE),
                               REASON_CODE);
      // Default reasons
      Database.Insert (TableName.TRANSLATION,
                       new string [] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string [] {"", "ReasonMotion", "Motion"});
      Database.Insert (TableName.REASON,
                       new string [] {ColumnName.REASON_ID, REASON_TRANSLATION_KEY, REASON_COLOR, ColumnName.REASON_GROUP_ID},
                       new string [] {"2", "ReasonMotion", "#008000", "2"}); // id = 2 (green)
      Database.Insert (TableName.TRANSLATION,
                       new string [] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string [] {"", "ReasonShort", "Short idle time"});
      Database.Insert (TableName.REASON,
                       new string [] {ColumnName.REASON_ID, REASON_TRANSLATION_KEY, REASON_COLOR, ColumnName.REASON_GROUP_ID},
                       new string [] {"3", "ReasonShort", "#FFA500", "3"}); // id = 3 (orange)
      Database.Insert (TableName.TRANSLATION,
                       new string [] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string [] {"", "ReasonUnanswered", "Unanswered"});
      Database.Insert (TableName.REASON,
                       new string [] {ColumnName.REASON_ID, REASON_TRANSLATION_KEY, REASON_COLOR, ColumnName.REASON_GROUP_ID},
                       new string [] {"4", "ReasonUnanswered", "#FFFF00", "6"}); // id = 4 (yellow)
      Database.Insert (TableName.TRANSLATION,
                       new string [] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string [] {"", "ReasonUnattended", "Unattended"});
      Database.Insert (TableName.REASON,
                       new string [] {ColumnName.REASON_ID, REASON_TRANSLATION_KEY, REASON_COLOR, ColumnName.REASON_GROUP_ID},
                       new string [] {"5", "ReasonUnattended", "#FFFF00", "6"}); // id = 5 (yellow)
      Database.Insert (TableName.TRANSLATION,
                       new string [] {ColumnName.LOCALE, ColumnName.TRANSLATION_KEY, ColumnName.TRANSLATION_VALUE},
                       new string [] {"", "ReasonOff", "Machine Off"});
      Database.Insert (TableName.REASON,
                       new string [] {ColumnName.REASON_ID, REASON_TRANSLATION_KEY, REASON_COLOR, ColumnName.REASON_GROUP_ID},
                       new string [] {"6", "ReasonOff", "#FFFF00", "6"}); // id = 6 (yellow)
      ResetSequence (TableName.REASON,
                     ColumnName.REASON_ID);
    }
    
    void AddMachineModeDefaultReasonTable ()
    {
      Database.AddTable (TableName.MACHINE_MODE_DEFAULT_REASON,
                         new Column ("MachineModeDefaultReasonId", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (ColumnName.MACHINE_MODE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.MACHINE_OBSERVATION_STATE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (DEFAULT_REASON_MAXIMUM_DURATION, DbType.Int32),
                         new Column (ColumnName.REASON_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column ("DefaultReasonOverwriteRequired", DbType.Boolean, ColumnProperty.NotNull));
      Database.GenerateForeignKey (TableName.MACHINE_MODE_DEFAULT_REASON, ColumnName.MACHINE_MODE_ID,
                                   TableName.MACHINE_MODE, ColumnName.MACHINE_MODE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.MACHINE_MODE_DEFAULT_REASON, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   TableName.MACHINE_OBSERVATION_STATE, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.MACHINE_MODE_DEFAULT_REASON, ColumnName.REASON_ID,
                                   TableName.REASON, ColumnName.REASON_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddNamedUniqueConstraint ("MachineModeDefaultReason_SecondaryKey",
                             TableName.MACHINE_MODE_DEFAULT_REASON,
                             new string [] { ColumnName.MACHINE_MODE_ID,
                               ColumnName.MACHINE_OBSERVATION_STATE_ID,
                               DEFAULT_REASON_MAXIMUM_DURATION});
      AddIndex (TableName.MACHINE_MODE_DEFAULT_REASON,
                ColumnName.MACHINE_MODE_ID,
                ColumnName.MACHINE_OBSERVATION_STATE_ID);
    }
    
    void AddReasonSelectionTable ()
    {
      Database.AddTable (TableName.REASON_SELECTION,
                         new Column ("ReasonSelectionId", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (ColumnName.MACHINE_MODE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.MACHINE_OBSERVATION_STATE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.REASON_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column ("ReasonSelectionSelectable", DbType.Boolean, ColumnProperty.NotNull, true),
                         new Column ("ReasonSelectionDetailsRequired", DbType.Boolean, ColumnProperty.NotNull, false));
      Database.GenerateForeignKey (TableName.REASON_SELECTION, ColumnName.MACHINE_MODE_ID,
                                   TableName.MACHINE_MODE, ColumnName.MACHINE_MODE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.REASON_SELECTION, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   TableName.MACHINE_OBSERVATION_STATE, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.REASON_SELECTION, ColumnName.REASON_ID,
                                   TableName.REASON, ColumnName.REASON_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddNamedUniqueConstraint ("ReasonSelection_SecondaryKey",
                             TableName.REASON_SELECTION,
                             new string [] { ColumnName.MACHINE_MODE_ID,
                               ColumnName.MACHINE_OBSERVATION_STATE_ID,
                               ColumnName.REASON_ID});
      AddIndex (TableName.REASON_SELECTION,
                ColumnName.MACHINE_MODE_ID,
                ColumnName.MACHINE_OBSERVATION_STATE_ID);
    }
    
    void AddReasonMachineAssociationTable ()
    {
      Database.AddTable (TableName.REASON_MACHINE_ASSOCIATION,
                         new Column (ColumnName.MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.REASON_ID, DbType.Int32),
                         new Column ("ReasonMachineAssociationBegin", DbType.DateTime, ColumnProperty.NotNull),
                         new Column (REASON_MACHINE_ASSOCIATION_END, DbType.DateTime),
                         new Column (ColumnName.REASON_DETAILS, DbType.String));
      Database.GenerateForeignKey (TableName.REASON_MACHINE_ASSOCIATION, ColumnName.MODIFICATION_ID,
                                   TableName.MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.REASON_MACHINE_ASSOCIATION, ColumnName.MACHINE_ID,
                                   TableName.MONITORED_MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.REASON_MACHINE_ASSOCIATION, ColumnName.REASON_ID,
                                   TableName.REASON, ColumnName.REASON_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      SetModificationTable (TableName.REASON_MACHINE_ASSOCIATION);
      AddIndex (TableName.REASON_MACHINE_ASSOCIATION,
                ColumnName.MACHINE_ID);
    }
    
    void RemoveReasonMachineAssociationTable ()
    {
      Database.ExecuteNonQuery (@"DELETE FROM modification
WHERE modificationreferencedtable='ReasonMachineAssociation'");
      Database.RemoveTable (TableName.REASON_MACHINE_ASSOCIATION);
    }
    
    void AddReasonSlotTable ()
    {
      Database.AddTable (TableName.REASON_SLOT,
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column ("ReasonSlotBeginDateTime", DbType.DateTime, ColumnProperty.PrimaryKey),
                         new Column ("ReasonSlotBeginDay", DbType.Date, ColumnProperty.NotNull),
                         new Column (REASON_SLOT_END_DATETIME, DbType.DateTime, ColumnProperty.NotNull),
                         new Column ("ReasonSlotEndDay", DbType.Date, ColumnProperty.NotNull),
                         new Column (ColumnName.MACHINE_MODE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.REASON_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.MACHINE_OBSERVATION_STATE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column ("ReasonSlotDefaultReason", DbType.Boolean, ColumnProperty.NotNull),
                         new Column ("ReasonSlotOverwriteRequired", DbType.Boolean, ColumnProperty.NotNull),
                         new Column (ColumnName.REASON_DETAILS, DbType.String));
      Database.GenerateForeignKey (TableName.REASON_SLOT, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.REASON_SLOT, ColumnName.MACHINE_MODE_ID,
                                   TableName.MACHINE_MODE, ColumnName.MACHINE_MODE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.REASON_SLOT, ColumnName.REASON_ID,
                                   TableName.REASON, ColumnName.REASON_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.REASON_SLOT, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   TableName.MACHINE_OBSERVATION_STATE, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddIndex (TableName.REASON_SLOT,
                ColumnName.MACHINE_ID);
      AddIndex (TableName.REASON_SLOT,
                ColumnName.MACHINE_ID,
                "ReasonSlotBeginDay");
    }
    
    void RemoveReasonSlotTable ()
    {
      Database.RemoveTable (TableName.REASON_SLOT);
    }
    
    void AddMachineStatusTable ()
    {
      Database.AddTable (TableName.MACHINE_STATUS,
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (CNC_MACHINE_MODE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.MACHINE_MODE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.MACHINE_OBSERVATION_STATE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.REASON_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (REASON_SLOT_END_DATETIME, DbType.DateTime, ColumnProperty.NotNull),
                         new Column ("MachineStatusDefaultReason", DbType.Boolean, ColumnProperty.NotNull),
                         new Column (REASON_MACHINE_ASSOCIATION_END, DbType.DateTime),
                         new Column ("MachineStatusManualActivity", DbType.Boolean, ColumnProperty.NotNull, false),
                         new Column ("activityenddatetime", DbType.DateTime));
      Database.GenerateForeignKey (TableName.MACHINE_STATUS, ColumnName.MACHINE_ID,
                                   TableName.MONITORED_MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.MACHINE_STATUS, CNC_MACHINE_MODE_ID,
                                   TableName.MACHINE_MODE, ColumnName.MACHINE_MODE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.MACHINE_STATUS, ColumnName.MACHINE_MODE_ID,
                                   TableName.MACHINE_MODE, ColumnName.MACHINE_MODE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.MACHINE_STATUS, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   TableName.MACHINE_OBSERVATION_STATE, ColumnName.MACHINE_OBSERVATION_STATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.MACHINE_STATUS, ColumnName.REASON_ID,
                                   TableName.REASON, ColumnName.REASON_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
    }
    
    void RemoveMachineStatusTable ()
    {
      Database.RemoveTable (TableName.MACHINE_STATUS);
    }
    
    void AddReasonSummaryTable ()
    {
      Database.AddTable (TableName.REASON_SUMMARY,
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column ("ReasonSummaryDay", DbType.Date, ColumnProperty.PrimaryKey),
                         new Column (ColumnName.MACHINE_OBSERVATION_STATE_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (ColumnName.REASON_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column ("reasonsummarytime", DbType.Double, ColumnProperty.NotNull));
      Database.GenerateForeignKey (TableName.REASON_SUMMARY, ColumnName.MACHINE_ID,
                                   TableName.MONITORED_MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.REASON_SUMMARY, ColumnName.REASON_ID,
                                   TableName.REASON, ColumnName.REASON_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddIndex (TableName.REASON_SUMMARY,
                ColumnName.MACHINE_ID,
                "ReasonSummaryDay");
      AddIndex (TableName.REASON_SUMMARY,
                ColumnName.REASON_ID,
                "ReasonSummaryDay");
    }
    
    void RemoveReasonSummaryTable ()
    {
      Database.RemoveTable (TableName.REASON_SUMMARY);
    }
  }
}
