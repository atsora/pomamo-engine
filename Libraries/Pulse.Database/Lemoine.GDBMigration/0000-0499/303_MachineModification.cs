// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Data;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 303: create machinemodification, globalmodification and linemodification tables
  /// </summary>
  [Migration(303)]
  public class MachineModification: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MachineModification).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.RenameTable (TableName.MODIFICATION, "old" + TableName.MODIFICATION);
      Database.RenameTable (TableName.MODIFICATION_STATUS, "old" + TableName.MODIFICATION_STATUS);
      
      // modification
      AddModificationStep1 ("global");
      AddModificationStep1 ("machine");
      Database.AddColumn ("machine" + TableName.MODIFICATION,
                          new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull));
      Database.GenerateForeignKey ("machine" + TableName.MODIFICATION, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddModificationStep2 ("global");
      AddModificationStep2 ("machine");
      
      // modificationstatus
      AddModificationStatus ("global");
      AddModificationStatus ("machine");
      Database.AddColumn ("machine" + TableName.MODIFICATION_STATUS,
                          new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull));
      Database.GenerateForeignKey ("machine" + TableName.MODIFICATION_STATUS, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      
      AddModificationView ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {      
      Database.ExecuteNonQuery (@"DROP VIEW IF EXISTS modification");
      
      Database.RemoveTable ("global" + TableName.MODIFICATION);
      Database.RemoveTable ("machine" + TableName.MODIFICATION);
      Database.RemoveTable ("global" + TableName.MODIFICATION_STATUS);
      Database.RemoveTable ("machine" + TableName.MODIFICATION_STATUS);

      Database.RenameTable ("old" + TableName.MODIFICATION, TableName.MODIFICATION);
      Database.RenameTable ("old" + TableName.MODIFICATION_STATUS, TableName.MODIFICATION_STATUS);
    }
    
    void AddModificationStep1 (string prefix)
    {
      Database.AddTable (prefix + TableName.MODIFICATION,
                         new Column (ColumnName.MODIFICATION_ID, DbType.Int64, ColumnProperty.PrimaryKey),
                         new Column (ColumnName.REVISION_ID, DbType.Int32, ColumnProperty.Null),
                         new Column (ColumnName.MODIFICATION_DATETIME, DbType.DateTime, ColumnProperty.NotNull),
                         new Column ("ModificationReferencedTable", DbType.String, ColumnProperty.NotNull),
                         new Column (ColumnName.MODIFICATION_PRIORITY, DbType.Int32, ColumnProperty.NotNull, 100),
                         new Column ("parentglobal" + ColumnName.MODIFICATION_ID, DbType.Int64, ColumnProperty.Null),
                         new Column ("parentmachine" + ColumnName.MODIFICATION_ID, DbType.Int64, ColumnProperty.Null),
                         new Column (TableName.MODIFICATION + "auto", DbType.Boolean, ColumnProperty.NotNull, "FALSE"));
      MakeColumnCaseInsensitive (prefix + TableName.MODIFICATION,
                                 "modificationreferencedtable");
      SetSequence (prefix + TableName.MODIFICATION,
                   ColumnName.MODIFICATION_ID,
                   "modification_modificationid_seq");
      Database.ExecuteNonQuery (string.Format (@"
ALTER TABLE {0}modification
ALTER COLUMN modificationdatetime
SET DEFAULT now() AT TIME ZONE 'UTC';",
                                               prefix));
      Database.AddCheckConstraint ("parentmodification", prefix + TableName.MODIFICATION,
                                   "parentglobalmodificationid IS NULL OR parentmachinemodificationid IS NULL");
    }
    
    void AddModificationStep2 (string prefix)
    {
      Database.GenerateForeignKey (prefix + TableName.MODIFICATION, ColumnName.REVISION_ID,
                                   TableName.REVISION, ColumnName.REVISION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      Database.GenerateForeignKey (prefix + TableName.MODIFICATION, "parentglobal" + ColumnName.MODIFICATION_ID,
                                   "global" + TableName.MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      Database.GenerateForeignKey (prefix + TableName.MODIFICATION, "parentmachine" + ColumnName.MODIFICATION_ID,
                                   "machine" + TableName.MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      
      AddIndex (prefix + TableName.MODIFICATION,
                ColumnName.REVISION_ID);
      AddIndexCondition (prefix + TableName.MODIFICATION,
                         "parentglobalmodificationid IS NOT NULL",
                         "parentglobal" + ColumnName.MODIFICATION_ID);
      AddIndexCondition (prefix + TableName.MODIFICATION,
                         "parentmachinemodificationid IS NOT NULL",
                         "parentmachine" + ColumnName.MODIFICATION_ID);
      AddIndexCondition (prefix + TableName.MODIFICATION,
                         "parentglobalmodificationid IS NULL AND parentmachinemodificationid IS NULL",
                         ColumnName.MODIFICATION_ID,
                         ColumnName.MODIFICATION_PRIORITY);
      AddIndexCondition (prefix + TableName.MODIFICATION,
                         "parentglobalmodificationid IS NULL AND parentmachinemodificationid IS NULL",
                         ColumnName.MODIFICATION_DATETIME,
                         ColumnName.MODIFICATION_PRIORITY);
    }
    
    void AddModificationStatus (string prefix)
    {
      Database.AddTable (prefix + TableName.MODIFICATION_STATUS,
                         new Column (ColumnName.MODIFICATION_ID, DbType.Int64, ColumnProperty.PrimaryKey),
                         new Column (TableName.ANALYSIS_STATUS + "Id", DbType.Int32, ColumnProperty.NotNull, 0),
                         new Column ("AnalysisAppliedDateTime", DbType.DateTime, ColumnProperty.Null),
                         new Column ("analysisbegin", DbType.DateTime),
                         new Column ("analysisend", DbType.DateTime),
                         new Column ("analysisiterations", DbType.Int32),
                         new Column ("analysistotalduration", DbType.Double),
                         new Column ("analysislastduration", DbType.Double),
                         new Column ("analysiscompletionorder", DbType.Int64),
                         new Column ("analysisstepspan", DbType.Int32),
                         new Column ("analysissubglobalmodifications", DbType.Boolean, ColumnProperty.NotNull, "FALSE"),
                         new Column ("analysissubmachinemodifications", DbType.Boolean, ColumnProperty.NotNull, "FALSE"));
      Database.GenerateForeignKey (prefix + TableName.MODIFICATION_STATUS, ColumnName.MODIFICATION_ID,
                                   prefix + TableName.MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (prefix + TableName.MODIFICATION_STATUS, TableName.ANALYSIS_STATUS + "id",
                                   TableName.ANALYSIS_STATUS, TableName.ANALYSIS_STATUS + "id",
                                   Migrator.Framework.ForeignKeyConstraint.Restrict);
      Database.AddCheckConstraint (prefix + "modificationstatus_analysisstatusids_applieddatetime",
                                   prefix + TableName.MODIFICATION_STATUS,
                                   "(analysisstatusid <> 0) OR (analysisapplieddatetime IS NULL)");
      AddIndex (prefix + TableName.MODIFICATION_STATUS,
                TableName.ANALYSIS_STATUS + "id");
    }
    
    void AddModificationView ()
    {
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE VIEW modification AS
SELECT modificationid, revisionid, modificationdatetime, modificationreferencedtable, modificationpriority,
  CASE WHEN parentmachinemodificationid IS NOT NULL THEN parentmachinemodificationid
       WHEN parentglobalmodificationid IS NOT NULL THEN parentglobalmodificationid
       ELSE NULL END AS parentmodificationid
FROM globalmodification
UNION
SELECT modificationid, revisionid, modificationdatetime, modificationreferencedtable, modificationpriority,
  CASE WHEN parentmachinemodificationid IS NOT NULL THEN parentmachinemodificationid
       WHEN parentglobalmodificationid IS NOT NULL THEN parentglobalmodificationid
       ELSE NULL END AS parentmodificationid
FROM machinemodification
;");
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE VIEW modificationstatus AS
SELECT modificationid, analysisstatusid, analysisapplieddatetime, analysisbegin, analysisend,
  analysisiterations, analysistotalduration, analysislastduration, analysiscompletionorder,
  analysisstepspan, (analysissubglobalmodifications OR analysissubmachinemodifications) AS analysissubmodifications
FROM globalmodificationstatus
UNION
SELECT modificationid, analysisstatusid, analysisapplieddatetime, analysisbegin, analysisend,
  analysisiterations, analysistotalduration, analysislastduration, analysiscompletionorder,
  analysisstepspan, (analysissubglobalmodifications OR analysissubmachinemodifications) AS analysissubmodifications
FROM machinemodificationstatus
;");
    }
    
  }
}
