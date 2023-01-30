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
  /// Migration 614: deprecated. New number is 517
  /// </summary>
  [Migration(614)]
  public class SequenceToolNumberOldNumber: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (SequenceToolNumberOldNumber).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // This is done now on Migration 517
      // So do nothing here
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
