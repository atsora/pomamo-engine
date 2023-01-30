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
  /// Migration 131: Add the intermediateworkpiecesummary and iwpbymachinesummary table
  /// </summary>
  [Migration(131)]
  public class AddIntermediateWorkPieceSummary: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddIntermediateWorkPieceSummary).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // Now part of plugin Lemoine.Plugin.IntermediateWorkPieceSummary
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
