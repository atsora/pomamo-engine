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
  /// Migration 154: Correct the index operationcycle_partialcycles
  /// </summary>
  [Migration(154)]
  public class CorrectIndexOperationCyclePartial: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (CorrectIndexOperationCyclePartial).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery ("DROP INDEX IF EXISTS operationcycle_partialcycles");
      Database.ExecuteNonQuery (@"CREATE INDEX operationcycle_partialcycles
  ON operationcycle
  USING btree
  (machineid, operationcyclebegin)
  WHERE operationcycleend IS NULL OR operationcyclestatus IN (2, 3);");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery ("DROP INDEX IF EXISTS operationcycle_partialcycles");
      Database.ExecuteNonQuery (@"CREATE INDEX operationcycle_partialcycles
  ON operationcycle
  USING btree
  (machineid, operationcyclebegin)
  WHERE operationcycleend IS NULL;");
    }
  }
}
