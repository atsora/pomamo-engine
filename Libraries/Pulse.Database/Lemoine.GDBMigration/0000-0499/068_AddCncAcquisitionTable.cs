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
  /// Migration 068: add the new Cnc Acquisition table
  /// <item>update machinemodule</item>
  /// <item>make machine module point to the new cncacquisition table</item>
  /// </summary>
  [Migration(68)]
  public class AddCncAcquisitionTable: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddCncAcquisitionTable).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddCncAcquisition ();
      UpgradeMachineModuleTable ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      DowngradeMachineModuleTable ();
      RemoveCncAcquisition ();
    }
    
    void AddCncAcquisition ()
    {
      Database.AddTable (TableName.CNC_ACQUISITION,
                         new Column (ColumnName.CNC_ACQUISITION_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column ("cncacquisitionversion", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column ("cncacquisitionname", DbType.String),
                         new Column ("cncacquisitionconfigfile", DbType.String),
                         new Column ("cncacquisitionconfigprefix", DbType.String, ColumnProperty.NotNull, "''"),
                         new Column ("cncacquisitionconfigparameters", DbType.String),
                         new Column ("cncacquisitionuseprocess", DbType.Boolean, ColumnProperty.NotNull, false),
                         new Column ("computerid", DbType.Int32, ColumnProperty.NotNull),
                         new Column ("cncacquisitionevery", DbType.Double, ColumnProperty.NotNull, 2),
                         new Column ("cncacquisitionnotrespondingtimeout", DbType.Double, ColumnProperty.NotNull, 120),
                         new Column ("cncacquisitionsleepbeforerestart", DbType.Double, ColumnProperty.NotNull, 10));
      AddUniqueConstraint (TableName.CNC_ACQUISITION,
                           "cncacquisitionname");
      Database.GenerateForeignKey (TableName.CNC_ACQUISITION, ColumnName.COMPUTER_ID,
                                   TableName.COMPUTER, ColumnName.COMPUTER_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
    }
    
    void RemoveCncAcquisition ()
    {
      Database.RemoveTable (TableName.CNC_ACQUISITION);      
    }
    
    void UpgradeMachineModuleTable ()
    {
      Database.RemoveColumn (TableName.MACHINE_MODULE,
                             "machinemoduleconfiguration");
      Database.RemoveColumn (TableName.MACHINE_MODULE,
                             "machinemoduleparameters");
      Database.AddColumn (TableName.MACHINE_MODULE,
                          new Column (ColumnName.CNC_ACQUISITION_ID, DbType.Int32));
      Database.GenerateForeignKey (TableName.MACHINE_MODULE, ColumnName.CNC_ACQUISITION_ID,
                                   TableName.CNC_ACQUISITION, ColumnName.CNC_ACQUISITION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      Database.AddColumn (TableName.MACHINE_MODULE,
                          new Column ("machinemoduleconfigprefix", DbType.String, ColumnProperty.NotNull, "''"));
      Database.AddColumn (TableName.MACHINE_MODULE,
                          new Column ("machinemoduleconfigparameters", DbType.String));
      AddUniqueConstraint (TableName.MACHINE_MODULE,
                           ColumnName.CNC_ACQUISITION_ID,
                           "machinemoduleconfigprefix");
      Database.ExecuteNonQuery (@"
UPDATE machinemodule
SET cncacquisitionid=cncacquisition.cncacquisitionid
FROM cncacquisition
WHERE cncacquisition.cncacquisitionid=machinemoduleid");
    }
    
    void DowngradeMachineModuleTable ()
    {
      Database.RemoveColumn (TableName.MACHINE_MODULE,
                             "machinemoduleconfigparameters");
      Database.RemoveColumn (TableName.MACHINE_MODULE,
                             "machinemoduleconfigprefix");
      Database.RemoveColumn (TableName.MACHINE_MODULE,
                             ColumnName.CNC_ACQUISITION_ID);
    }
  }
}
