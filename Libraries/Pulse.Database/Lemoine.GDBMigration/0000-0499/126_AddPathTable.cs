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
  /// Migration 126: add path table (in the sense of operation with multi-paths),
  /// and links between:
  ///  sequence * --> 1 path (pathid column in sequence)
  ///  path * -> 1 operation (operationid column in path)
  /// 
  /// A sequence is contained in a unique not-null path.
  /// A path is contained in a unique not-null operation.
  /// An operation can have 0, 1 (single-path) or more than one paths (multi-paths).
  /// Paths have a number which is unique in their operation (numbering usually starts at 1).
  /// 
  /// Migration creates a single path for operation having at least one sequence migration
  /// (with number 1). The order of sequences in the path follows the previous order in the operation.
  /// 
  /// Note that an operation can be retrieved from a sequence either directly or through path.
  /// The resulting operation must be unique (a database integrity constraint forces it).
  /// </summary>
  [Migration(126)]
  public class AddPathTable: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddPathTable).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // 1. create path table
      
      Database.AddTable (TableName.PATH_OLD,
                         new Column (ColumnName.PATH_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (ColumnName.PATH_VERSION, DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ColumnName.PATH_NUMBER, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.OPERATION_ID, DbType.Int32, ColumnProperty.NotNull));
      
      Database.GenerateForeignKey (TableName.PATH_OLD, ColumnName.OPERATION_ID,
                                   TableName.OPERATION, ColumnName.OPERATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      
      // add index on (operation) on path table
      AddIndex(TableName.PATH_OLD, ColumnName.OPERATION_ID);
      
      // add constraint on uniqueness of path number in operation
      Database.ExecuteNonQuery(
        "ALTER TABLE path " +
        "ADD CONSTRAINT path_operationid_pathnumber_key UNIQUE (operationid, pathnumber);"
       );
      
      // 2. modify sequence table
      
      // constraints on this column added after migration has been performed,
      // since the SEQUENCE table preexists
      Database.AddColumn(TableName.SEQUENCE,
                         new Column (ColumnName.PATH_ID, DbType.Int32));
      
      // drop sequence order uniqueness w.r.t. operation
      // and recreate it w.r.t. path
      Database.ExecuteNonQuery ("ALTER TABLE sequence " +
                                "DROP CONSTRAINT sequence_operationid_sequenceorder_key");

      Database.ExecuteNonQuery(
        "ALTER TABLE sequence " +
        "ADD CONSTRAINT sequence_pathid_sequenceorder_key UNIQUE (pathid, sequenceorder)" +
        "DEFERRABLE INITIALLY DEFERRED;"
       );
      
      // 3. create paths and migrate sequences into path
      Database.ExecuteNonQuery(String.Format(
        @"CREATE OR REPLACE FUNCTION migrate_sequence_paths() RETURNS void AS $$
DECLARE operationrecord RECORD;
DECLARE sequencerecord RECORD;
DECLARE numberOfSequences INT;
DECLARE pathrecordid INT;
BEGIN
  FOR operationrecord in SELECT * from operation LOOP
    numberOfSequences = (SELECT COUNT(*) FROM sequence WHERE operationid = operationrecord.operationid);
    IF (numberOfSequences > 0) THEN
      INSERT INTO path (pathnumber, operationid) VALUES ('1', operationrecord.operationid)
      RETURNING {0} INTO pathrecordid;
      FOR sequencerecord in SELECT * from sequence WHERE operationid = operationrecord.operationid LOOP
        UPDATE sequence SET {0} = pathrecordid WHERE sequenceid = sequencerecord.sequenceid;
      END LOOP;
    END IF;
  END LOOP;
END;

$$ LANGUAGE plpgsql;

SELECT migrate_sequence_paths();
        ", ColumnName.PATH_ID));
      
      // add not-null / foreign key constraints on sequence.pathid
      Database.ExecuteNonQuery(String.Format(
        @"alter table sequence
alter column {0} SET NOT NULL;", ColumnName.PATH_ID));
      
      Database.GenerateForeignKey (TableName.SEQUENCE, ColumnName.PATH_ID,
                                   TableName.PATH_OLD, ColumnName.PATH_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      
      // add index on (operation,path) on sequence table
      AddIndex(TableName.SEQUENCE, ColumnName.OPERATION_ID, ColumnName.PATH_ID);
      
      // add integrity constraint: operation retrieved through any path of a sequence should be
      // equal to operation of sequence
      
      /* not added: NH will make it impossible to maintain
       * 
      Database.ExecuteNonQuery(
        @"CREATE OR REPLACE FUNCTION same_operation_path(operationidIn int, pathidIn int) returns boolean as $$
DECLARE pathidoperationid INT;
BEGIN
  pathidoperationid = (SELECT operationid FROM path WHERE pathid = pathidIn);
  return pathidoperationid = operationidIn;
END;
$$ LANGUAGE plpgsql;

ALTER TABLE sequence
ADD CONSTRAINT check_same_operation_path CHECK (same_operation_path (operationid, pathid));");
       */
    }
    
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      // remove multiple paths: beware this destroys all corresponding sequences
      Database.ExecuteNonQuery(@"
      CREATE OR REPLACE FUNCTION unmigrate_sequence_paths() RETURNS void AS $$
        DECLARE operationrecord RECORD;
      DECLARE numberofpaths INT;
      DECLARE minpathnumber INT;
      BEGIN
        FOR operationrecord in SELECT * FROM operation LOOP
        numberofpaths = (SELECT COUNT(*) FROM path WHERE operationid = operationrecord.operationid);
      IF (numberofpaths > 0) THEN
        minpathnumber = (SELECT MIN(pathnumber) FROM path WHERE operationid = operationrecord.operationid);
      DELETE FROM path WHERE pathnumber <> minpathnumber AND operationid = operationrecord.operationid;
      END IF;
      END LOOP;
      END;

      $$ LANGUAGE plpgsql;

      SELECT unmigrate_sequence_paths();");

      Database.RemoveColumn(TableName.SEQUENCE, ColumnName.PATH_ID);
      Database.RemoveTable(TableName.PATH_OLD);
      
      // this would fail if multiple paths for a single operation weren't deleted
      Database.ExecuteNonQuery(
        @"ALTER TABLE sequence
        ADD CONSTRAINT sequence_operationid_sequenceorder_key UNIQUE (operationid, sequenceorder)");
    }
  }
}
