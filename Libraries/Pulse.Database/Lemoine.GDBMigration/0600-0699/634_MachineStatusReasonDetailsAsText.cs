// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using System.Diagnostics;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration MachineStatusReasonDetailsAsText: 
  /// </summary>
  [Migration (634)]
  public class MachineStatusReasonDetailsAsText : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (MachineStatusReasonDetailsAsText).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      MakeColumnText (TableName.MACHINE_STATUS, "reasondetails");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0}
ALTER COLUMN {1}
SET DATA TYPE CHARACTER VARYING(255)",
                                               TableName.MACHINE_STATUS, "reasondetails"));
    }
  }
}
