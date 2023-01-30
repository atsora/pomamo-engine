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
  /// Migration 525: add a new column machinemoduledetectioncustom to machinemoduledetection that will be used by plugins
  /// </summary>
  [Migration(525)]
  public class AddMachineModuleDetectionCustom: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddMachineModuleDetectionCustom).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.MACHINE_MODULE_DETECTION,
                          new Column (TableName.MACHINE_MODULE_DETECTION + "customtype", DbType.String));
      MakeColumnCaseInsensitive (TableName.MACHINE_MODULE_DETECTION,
                                 TableName.MACHINE_MODULE_DETECTION + "customtype");
      Database.AddColumn (TableName.MACHINE_MODULE_DETECTION,
                          new Column (TableName.MACHINE_MODULE_DETECTION + "customvalue", DbType.String));
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.MACHINE_MODULE_DETECTION,
                            TableName.MACHINE_MODULE_DETECTION + "customvalue");
      Database.RemoveColumn (TableName.MACHINE_MODULE_DETECTION,
                            TableName.MACHINE_MODULE_DETECTION + "customtype");
    }
  }
}
