// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 1923: make the local time of a machine state template stop not null
  ///
  /// A null local time was already processed as local midnight in
  /// MachineStateTemplateAssociation.GetStop, so replacing the null values by 0:00:00
  /// does not change the behavior
  /// </summary>
  [Migration (1923)]
  public class MachineStateTemplateStopLocalTimeNotNull : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (MachineStateTemplateStopLocalTimeNotNull).FullName);

    static readonly string LOCAL_TIME = TableName.MACHINE_STATE_TEMPLATE_STOP + "localtime";

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery ($"""
        UPDATE {TableName.MACHINE_STATE_TEMPLATE_STOP}
        SET {LOCAL_TIME} = '00:00:00'
        WHERE {LOCAL_TIME} IS NULL
        """);
      Database.ExecuteNonQuery ($"""
        ALTER TABLE {TableName.MACHINE_STATE_TEMPLATE_STOP}
        ALTER COLUMN {LOCAL_TIME} SET DEFAULT '00:00:00'
        """);
      SetNotNull (TableName.MACHINE_STATE_TEMPLATE_STOP, LOCAL_TIME);
    }

    /// <summary>
    /// Downgrade the database
    ///
    /// Note: the local times that were null before the migration are not restored,
    /// they keep the 0:00:00 value, which is equivalent
    /// </summary>
    override public void Down ()
    {
      DropNotNull (TableName.MACHINE_STATE_TEMPLATE_STOP, LOCAL_TIME);
      DropDefault (TableName.MACHINE_STATE_TEMPLATE_STOP, LOCAL_TIME);
    }
  }
}
