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
  /// Migration 158: Between cycles item with begin=end are allowed
  /// </summary>
  [Migration(158)]
  public class BetweenCyclesNullLength: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (BetweenCyclesNullLength).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // begin <= end
      Database.RemoveConstraint (TableName.BETWEEN_CYCLES, TableName.BETWEEN_CYCLES + "period");
      Database.AddCheckConstraint (TableName.BETWEEN_CYCLES + "period",
                                   TableName.BETWEEN_CYCLES,
                                   string.Format ("{0} <= {1}",
                                                  TableName.BETWEEN_CYCLES + "begin",
                                                  TableName.BETWEEN_CYCLES + "end"));
      
      // previouscycle <> nextcycle
      Database.AddCheckConstraint (TableName.BETWEEN_CYCLES + "differentcycles",
                                   TableName.BETWEEN_CYCLES,
                                   "previouscycleid <> nextcycleid");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      // previouscycle <> nextcycle
      Database.RemoveConstraint (TableName.BETWEEN_CYCLES,
                                 TableName.BETWEEN_CYCLES + "differentcycles");

      // begin < end
      Database.RemoveConstraint (TableName.BETWEEN_CYCLES, TableName.BETWEEN_CYCLES + "period");
      Database.AddCheckConstraint (TableName.BETWEEN_CYCLES + "period",
                                   TableName.BETWEEN_CYCLES,
                                   string.Format ("{0} < {1}",
                                                  TableName.BETWEEN_CYCLES + "begin",
                                                  TableName.BETWEEN_CYCLES + "end"));
    }
  }
}
