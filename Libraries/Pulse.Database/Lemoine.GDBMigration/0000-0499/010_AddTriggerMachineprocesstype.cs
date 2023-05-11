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
  /// Migration 010: deprecated
  /// </summary>
  [Migration(10)]
  public class AddTriggerMachineprocesstype: Migration
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddTriggerMachineprocesstype).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // Note: machineprocesstype is removed in migration 17. There is no need any more to create it
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
