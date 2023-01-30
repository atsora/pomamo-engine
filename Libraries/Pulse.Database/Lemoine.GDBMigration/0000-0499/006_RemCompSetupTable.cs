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
  /// Migration 006: Remove the TABLE sfkcompsetup
  /// </summary>
  [Migration(06)]
  public class RemCompSetupTable: Migration
  {
    static readonly ILog log = LogManager.GetLogger(typeof (RemCompSetupTable).FullName);
    static readonly string TABLE_NAME = "sfkcompsetup"; 
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if ( Database.TableExists(TABLE_NAME) ) {
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
