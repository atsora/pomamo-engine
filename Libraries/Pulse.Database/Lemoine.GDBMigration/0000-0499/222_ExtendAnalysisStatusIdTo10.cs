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
  /// Migration 222: Extend AnalysisStatusId up to 10
  /// The 9 and 10 are reserved for futur usage.
  /// Fix at the same time the constraint who block when id > 7
  /// </summary>
  [Migration(222)]
  public class ExtendAnalysisStatusIdTo10: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ExtendAnalysisStatusIdTo10).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.RemoveConstraint (TableName.MODIFICATION_STATUS,
                                 "modificationstatus_analysisstatusids");
      Database.AddCheckConstraint ("modificationstatus_analysisstatusids",
                                   TableName.MODIFICATION_STATUS,
                                   "analysisstatusid IN (0, 1, 3, 4, 5, 6, 7, 8, 9, 10)"); // 0: New, 1: Pending, 3: Done, 4: Error, 5: Obsolete, 6: Delete, 7: Timeout, 8: Integrity Violation
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      // 8, 9 or 10 can be present
//      Database.RemoveConstraint (TableName.MODIFICATION_STATUS,
//                                 "modificationstatus_analysisstatusids");
//      Database.AddCheckConstraint ("modificationstatus_analysisstatusids",
//                                   TableName.MODIFICATION_STATUS,
//                                   "analysisstatusid IN (0, 1, 3, 4, 5, 6, 7)"); // 0: New, 1: Pending, 3: Done, 4: Error, 5: Obsolete, 6: Delete
    }
  }
}
