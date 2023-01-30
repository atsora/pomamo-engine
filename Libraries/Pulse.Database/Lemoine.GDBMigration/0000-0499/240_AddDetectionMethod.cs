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
  /// Migration 240:
  /// <item>add three columns for detection method in table machinemodule</item>
  /// <item>add columns to machinemoduledetection</item>
  /// </summary>
  [Migration(240)]
  public class AddDetectionMethod: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddDetectionMethod).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      MachineModuleUp ();
      MachineModuleDetectionUp ();
      
      // Minimum stampid is 10
      Database.ExecuteNonQuery (@"SELECT setval ('stamp_stampid_seq', 10)
WHERE nextval('stamp_stampid_seq') < 10;");
      Database.ExecuteNonQuery (@"ALTER SEQUENCE stamp_stampid_seq MINVALUE 10 START 10;");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      MachineModuleDetectionDown ();
      MachineModuleDown ();
    }
    
    void MachineModuleUp ()
    {
      Database.AddColumn (TableName.MACHINE_MODULE,
                          new Column (TableName.MACHINE_MODULE + "sequencedetectionmethod",
                                      DbType.Int32, ColumnProperty.NotNull, 1024));
      Database.AddColumn (TableName.MACHINE_MODULE,
                          new Column (TableName.MACHINE_MODULE + "cycledetectionmethod",
                                      DbType.Int32, ColumnProperty.NotNull, 8));
      Database.AddColumn (TableName.MACHINE_MODULE,
                          new Column (TableName.MACHINE_MODULE + "startcycledetectionmethod",
                                      DbType.Int32, ColumnProperty.NotNull, 1));
      Database.AddColumn (TableName.MACHINE_MODULE,
                          new Column (TableName.MACHINE_MODULE + "detectionmethodvariable",
                                      DbType.String, ColumnProperty.Null));
    }
    
    void MachineModuleDown ()
    {
      Database.RemoveColumn (TableName.MACHINE_MODULE,
                             TableName.MACHINE_MODULE + "detectionmethodvariable");
      Database.RemoveColumn (TableName.MACHINE_MODULE,
                             TableName.MACHINE_MODULE + "startcycledetectionmethod");
      Database.RemoveColumn (TableName.MACHINE_MODULE,
                             TableName.MACHINE_MODULE + "cycledetectionmethod");
      Database.RemoveColumn (TableName.MACHINE_MODULE,
                             TableName.MACHINE_MODULE + "sequencedetectionmethod");
    }
    
    void MachineModuleDetectionUp ()
    {
      Database.AddColumn (TableName.MACHINE_MODULE_DETECTION,
                          new Column (TableName.MACHINE_MODULE_DETECTION + "quantity",
                                      DbType.Int32));
      Database.AddColumn (TableName.MACHINE_MODULE_DETECTION,
                          new Column ("operationcode",
                                      DbType.String)); // With a 'StartCycle' / 'EndCycle'
    }
    
    void MachineModuleDetectionDown ()
    {
      Database.RemoveColumn (TableName.MACHINE_MODULE_DETECTION,
                             "operationcode");
      Database.RemoveColumn (TableName.MACHINE_MODULE_DETECTION,
                             TableName.MACHINE_MODULE_DETECTION + "quantity");
    }
  }
}
