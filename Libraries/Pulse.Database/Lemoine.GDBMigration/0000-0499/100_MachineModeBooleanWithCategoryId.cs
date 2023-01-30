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
  /// Migration 100 (deprecated): Add the machinemodecategoryid to the view machinemodeboolean
  /// </summary>
  [Migration(100)]
  public class MachineModeBooleanWithCategoryId: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MachineModeBooleanWithCategoryId).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
