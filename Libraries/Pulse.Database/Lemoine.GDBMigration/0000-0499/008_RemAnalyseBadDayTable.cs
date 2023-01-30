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
  /// Migration 008: remove sfkanlbadday - Aim : rework compactor access
  /// </summary>
  [Migration(08)]
  public class RemAnalyseBadDayTable: Migration
  {
    static readonly ILog log = LogManager.GetLogger(typeof (RemAnalyseBadDayTable).FullName);
    static readonly string TABLE_NAME = "sfkanlbadday"; 
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (Database.TableExists (TABLE_NAME)) {
        Database.RemoveTable (TABLE_NAME);
      }
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
