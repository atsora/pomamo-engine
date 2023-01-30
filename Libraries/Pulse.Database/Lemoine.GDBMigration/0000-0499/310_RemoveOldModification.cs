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
  /// Migration 310:
  /// </summary>
  [Migration(310)]
  public class RemoveOldModification: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (RemoveOldModification).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.RemoveTable ("oldmodification");
      Database.RemoveTable ("oldmodificationstatus");
      
      RestoreModificationIdSequence ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      const string prefix = "old";
      
      Database.AddTable ("old" + TableName.MODIFICATION,
                         new Column (ColumnName.MODIFICATION_ID, DbType.Int64, ColumnProperty.PrimaryKey),
                         new Column (ColumnName.REVISION_ID, DbType.Int32, ColumnProperty.Null),
                         new Column (ColumnName.MODIFICATION_DATETIME, DbType.DateTime, ColumnProperty.NotNull),
                         new Column ("ModificationReferencedTable", DbType.String, ColumnProperty.NotNull),
                         new Column (ColumnName.MODIFICATION_PRIORITY, DbType.Int32, ColumnProperty.NotNull, 100),
                         new Column ("parent" + ColumnName.MODIFICATION_ID, DbType.Int64, ColumnProperty.Null));
      MakeColumnCaseInsensitive ("old" + TableName.MODIFICATION,
                                 "modificationreferencedtable");
      SetSequence ("old" + TableName.MODIFICATION,
                   ColumnName.MODIFICATION_ID,
                   "modification_modificationid_seq");
      Database.ExecuteNonQuery (string.Format (@"
ALTER TABLE {0}modification
ALTER COLUMN modificationdatetime
SET DEFAULT now() AT TIME ZONE 'UTC';",
                                               "old"));
      
      Database.GenerateForeignKey (prefix + TableName.MODIFICATION, ColumnName.REVISION_ID,
                                   TableName.REVISION, ColumnName.REVISION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      Database.GenerateForeignKey (prefix + TableName.MODIFICATION, "parent" + ColumnName.MODIFICATION_ID,
                                   prefix + TableName.MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      
      AddIndex (prefix + TableName.MODIFICATION,
                ColumnName.REVISION_ID);
      AddIndexCondition (prefix + TableName.MODIFICATION,
                         "parentmodificationid IS NOT NULL",
                         "parent" + ColumnName.MODIFICATION_ID);
      AddIndexCondition (prefix + TableName.MODIFICATION,
                         "parentmodificationid IS NULL",
                         ColumnName.MODIFICATION_ID,
                         ColumnName.MODIFICATION_PRIORITY);
      AddIndexCondition (prefix + TableName.MODIFICATION,
                         "parentmodificationid IS NULL",
                         ColumnName.MODIFICATION_DATETIME,
                         ColumnName.MODIFICATION_PRIORITY);
      
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
                         new Column ("analysissubmodifications", DbType.Boolean, ColumnProperty.NotNull, "FALSE"));
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
    
    void RestoreModificationIdSequence ()
    {
      long isSequence = (long)Database.ExecuteScalar (@"SELECT COUNT(*) FROM information_schema.sequences
    WHERE sequence_schema = 'public' AND sequence_name = 'modification_modificationid_seq'");
      if (0 == isSequence) {
        Database.ExecuteNonQuery (@"CREATE SEQUENCE modification_modificationid_seq
  INCREMENT 1
  MINVALUE 1
  NO MAXVALUE
  START 1
  CACHE 1;");
        Database.ExecuteNonQuery (@"
WITH maxmodifications AS (
  SELECT MAX(modificationid) AS maxmodificationid
  FROM globalmodification
  UNION
  SELECT MAX(modificationid) AS maxmodificationid
  FROM machinemodification
)
SELECT SETVAL('modification_modificationid_seq', (SELECT MAX(maxmodificationid) FROM maxmodifications) + 1)");
        SetSequence ("global" + TableName.MODIFICATION,
                     ColumnName.MODIFICATION_ID,
                     "modification_modificationid_seq");
        SetSequence ("machine" + TableName.MODIFICATION,
                     ColumnName.MODIFICATION_ID,
                     "modification_modificationid_seq");
      }
    }
  }
}
