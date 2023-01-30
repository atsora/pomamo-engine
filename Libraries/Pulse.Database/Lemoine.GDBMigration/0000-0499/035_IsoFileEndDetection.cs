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
  /// Migration 035: ISO File End Detection
  /// <item>Add a column isofileend in stamp table</item>
  /// <item>Add a table isofileenddetection</item>
  /// <item>Make the stamp column of stampdetection not null</item>
  /// </summary>
  [Migration(35)]
  public class IsoFileEndDetection: MigrationExt
  {
    static readonly string STAMP_ISO_FILE_END = "stampisofileend";
    
    static readonly ILog log = LogManager.GetLogger(typeof (IsoFileEndDetection).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (!Database.TableExists (TableName.ISO_FILE_END_DETECTION)) {
        AddIsoFileEndDetectionTable ();
      }
      AddStampIsoFileEndColumn ();
      MakeStampDetectionStampIdNotNull ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      if (Database.TableExists (TableName.ISO_FILE_END_DETECTION)) {
        RemoveIsoFileEndDetectionTable ();
      }
    }
    
    void AddIsoFileEndDetectionTable ()
    {
      Database.AddTable (TableName.ISO_FILE_END_DETECTION,
                         new Column (ColumnName.MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (ColumnName.MACHINE_MODULE_ID, DbType.Int32, ColumnProperty.NotNull));
      Database.GenerateForeignKey (TableName.ISO_FILE_END_DETECTION, ColumnName.MODIFICATION_ID,
                                   TableName.MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.ISO_FILE_END_DETECTION, ColumnName.MACHINE_MODULE_ID,
                                   TableName.MACHINE_MODULE, ColumnName.MACHINE_MODULE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.ExecuteNonQuery (string.Format (@"CREATE RULE {0}_delete AS
ON DELETE TO {0}
DO ALSO DELETE FROM modification
  WHERE modificationid = OLD.modificationid;",
                                               TableName.ISO_FILE_END_DETECTION));
    }
    
    void RemoveIsoFileEndDetectionTable ()
    {
      Database.RemoveTable (TableName.ISO_FILE_END_DETECTION);
    }
    
    void AddStampIsoFileEndColumn ()
    {
      if (!Database.ColumnExists (TableName.STAMP,
                                  STAMP_ISO_FILE_END)) {
        Database.AddColumn (TableName.STAMP,
                            new Column (STAMP_ISO_FILE_END,
                                        DbType.Boolean,
                                        ColumnProperty.NotNull,
                                        false));
      }
      
      Database.ExecuteNonQuery (@"CREATE OR REPLACE VIEW stampboolean AS
SELECT stampid, isofileid, stampposition, processid, operationid, componentid,
  CASE WHEN operationcyclebegin THEN 1 ELSE 0 END AS operationcyclebegin,
  CASE WHEN operationcycleend THEN 1 ELSE 0 END AS operationcycleend,
  CASE WHEN stampisofileend THEN 1 ELSE 0 END AS stampisofileend
FROM stamp");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE RULE stampboolean_insert AS
ON INSERT TO stampboolean DO INSTEAD
INSERT INTO stamp (isofileid, stampposition, processid, operationid, componentid, operationcyclebegin, operationcycleend, stampisofileend)
VALUES (NEW.isofileid, NEW.stampposition, NEW.processid, NEW.operationid, NEW.componentid,
 (NEW.operationcyclebegin=1), (NEW.operationcycleend=1), (NEW.stampisofileend=1))
RETURNING stamp.stampid AS stampid,
 stamp.isofileid AS isofileid,
 stamp.stampposition AS stampposition,
 stamp.processid AS processid, stamp.operationid AS operationid,
 stamp.componentid AS componentid,
 CASE WHEN stamp.operationcyclebegin THEN 1 ELSE 0 END AS operationcyclebegin,
 CASE WHEN stamp.operationcycleend THEN 1 ELSE 0 END AS operationcycleend,
 CASE WHEN stamp.stampisofileend THEN 1 ELSE 0 END AS stampisofileend;");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE RULE stampboolean_update AS
ON UPDATE TO stampboolean DO INSTEAD
UPDATE stamp SET stampposition=NEW.stampposition,
 processid=NEW.processid, operationid=NEW.operationid,
 componentid=NEW.componentid,
 operationcyclebegin=(NEW.operationcyclebegin=1),
 operationcycleend=(NEW.operationcycleend=1),
 stampisofileend=(NEW.stampisofileend=1)
WHERE stampid=OLD.stampid;");
    }
    
    void MakeStampDetectionStampIdNotNull ()
    {
      Database.ExecuteNonQuery (@"DELETE FROM stampdetection
WHERE stampid IS NULL");
      SetNotNull (TableName.STAMP_DETECTION, ColumnName.STAMP_ID);
    }
  }
}
