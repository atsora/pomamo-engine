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
  /// Migration 306:
  /// </summary>
  [Migration(306)]
  public class AddModificationCompletionOrderSequence: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddModificationCompletionOrderSequence).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"ALTER SEQUENCE modification_modificationid_seq
NO MAXVALUE");
      
      Database.ExecuteNonQuery (@"CREATE SEQUENCE modification_completionorder_seq
  INCREMENT 1
  MINVALUE 1
  NO MAXVALUE
  START 1
  CACHE 1;");
      Database.ExecuteNonQuery (@"
WITH maxcompletionorders AS (
  SELECT MAX(analysiscompletionorder) AS maxcompletionorder
  FROM globalmodificationstatus
  UNION
  SELECT MAX(analysiscompletionorder) AS maxcompletionorder
  FROM machinemodificationstatus
)
SELECT SETVAL('modification_completionorder_seq', (SELECT MAX(maxcompletionorder) FROM maxcompletionorders) + 1)");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (@"DROP SEQUENCE IF EXISTS modification_completionorder_seq");
    }
  }
}
