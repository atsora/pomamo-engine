// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 605:
  /// </summary>
  [Migration(605)]
  public class CncvalueidToBigserial: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (CncvalueidToBigserial).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0}
ALTER COLUMN {1}
SET DATA TYPE bigint",
                                               TableName.CNC_VALUE, TableName.CNC_VALUE + "id"));
      Database.ExecuteNonQuery (@"ALTER SEQUENCE public.cncvalue_cncvalueid_seq
NO MAXVALUE");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0}
ALTER COLUMN {1}
SET DATA TYPE integer",
                                               TableName.CNC_VALUE, TableName.CNC_VALUE + "id"));
      Database.ExecuteNonQuery (@"ALTER SEQUENCE public.cncvalue_cncvalueid_seq 
MAXVALUE 2147483647");
    }
  }
}
