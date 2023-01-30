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
  /// Migration 009 (deprecated): add some columns to get the services installed on the controls
  /// </summary>
  [Migration(09)]
  public class AddColumnsMachineCnc: Migration
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddColumnsMachineCnc).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // Do nothing now: these columns are not required any more
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
