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
  /// Deprecated: Migration 051: Add the view machineobservationstateboolean
  /// for Borland C++ code
  /// </summary>
  [Migration (51)]
  public class MachineObservationStateBoolean : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (MachineObservationStateBoolean).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // Not necessary any more
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (@"DROP VIEW IF EXISTS machineobservationstateboolean;");
    }
  }
}
