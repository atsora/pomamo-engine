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
  /// Migration 119: Remove the boolean views for C++ that are not needed any more
  /// </summary>
  [Migration(119)]
  public class RemoveBooleanViews: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (RemoveBooleanViews).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"DROP VIEW IF EXISTS datastructureoptionboolean");
      Database.ExecuteNonQuery (@"DROP VIEW IF EXISTS stampboolean CASCADE");
      Database.ExecuteNonQuery (@"DROP VIEW IF EXISTS machineobservationstateboolean;");
      Database.ExecuteNonQuery (@"DROP VIEW IF EXISTS machinemodeboolean;");
      Database.ExecuteNonQuery (@"DROP VIEW IF EXISTS reasonslotboolean;");
      Database.ExecuteNonQuery (@"DROP VIEW IF EXISTS reasonselectionboolean CASCADE;");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
