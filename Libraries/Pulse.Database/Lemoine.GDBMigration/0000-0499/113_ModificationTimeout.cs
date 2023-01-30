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
  /// Migration 113: Add a Timeout status to the modificationstatus table
  /// </summary>
  [Migration(113)]
  public class ModificationTimeout: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ModificationTimeout).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.RemoveConstraint (TableName.MODIFICATION_STATUS,
                                 "modificationstatus_analysisstatusids");
      Database.AddCheckConstraint ("modificationstatus_analysisstatusids",
                                   TableName.MODIFICATION_STATUS,
                                   "analysisstatusid IN (0, 1, 3, 4, 5, 6, 7)"); // 0: New, 1: Pending, 3: Done, 4: Error, 5: Obsolete, 6: Delete, 7: Timeout
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveConstraint (TableName.MODIFICATION_STATUS,
                                 "modificationstatus_analysisstatusids");
      Database.AddCheckConstraint ("modificationstatus_analysisstatusids",
                                   TableName.MODIFICATION_STATUS,
                                   "analysisstatusid IN (0, 1, 3, 4, 5, 6)"); // 0: New, 1: Pending, 3: Done, 4: Error, 5: Obsolete, 6: Delete
    }
  }
}
