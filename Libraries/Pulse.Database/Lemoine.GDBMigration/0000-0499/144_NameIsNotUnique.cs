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
  /// Migration 144: Remove the unicity on names in WorkOrder and Project tables
  /// </summary>
  [Migration(144)]
  public class NameIsNotUnique: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (NameIsNotUnique).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      RemoveUniqueConstraint (TableName.PROJECT,
                              TableName.PROJECT + "name");
      RemoveUniqueConstraint (TableName.WORK_ORDER,
                              TableName.WORK_ORDER + "name");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      AddUniqueConstraint (TableName.WORK_ORDER,
                           TableName.WORK_ORDER + "name");      
      AddUniqueConstraint (TableName.PROJECT,
                           TableName.PROJECT + "name");
    }
  }
}
