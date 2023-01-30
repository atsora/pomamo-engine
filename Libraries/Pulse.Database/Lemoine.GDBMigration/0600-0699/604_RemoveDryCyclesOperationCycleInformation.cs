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
  /// Migration 604: Remove the drycycles column in operationslot and the operationcycleinformation table
  /// </summary>
  [Migration(604)]
  public class RemoveDryCyclesOperationCycleInformation: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (RemoveDryCyclesOperationCycleInformation).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      DryCyclesUp ();
      OperationCycleInformationUp ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      OperationCycleInformationDown ();
      DryCyclesDown ();
    }
    
    void DryCyclesUp ()
    {
      Database.RemoveColumn (TableName.OPERATION_SLOT,
                             TableName.OPERATION_SLOT + "DryCycles");
    }
    
    void DryCyclesDown ()
    {
      Database.AddColumn (TableName.OPERATION_SLOT,
                          new Column ("OperationSlotDryCycles", DbType.Int32, ColumnProperty.NotNull, 0));
    }
    
    void OperationCycleInformationUp ()
    {
      if (Database.TableExists (TableName.OPERATION_CYCLE_INFORMATION)) {
        if (IsPartitioned (TableName.OPERATION_CYCLE_INFORMATION)) {
          UnpartitionTable (TableName.OPERATION_CYCLE_INFORMATION);
        }
        Database.RemoveTable (TableName.OPERATION_CYCLE_INFORMATION);
      }
    }
    
    void OperationCycleInformationDown ()
    {
      Database.AddTable (TableName.OPERATION_CYCLE_INFORMATION,
                         new Column (ColumnName.MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.OPERATION_ID, DbType.Int32),
                         new Column ("OperationCycleApplicationDateTime", DbType.DateTime),
                         new Column ("DryCycleNumber", DbType.Int32),
                         new Column ("DryCycleNumberOffset", DbType.Int32));
      Database.GenerateForeignKey (TableName.OPERATION_CYCLE_INFORMATION, ColumnName.MODIFICATION_ID,
                                   TableName.MACHINE_MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.OPERATION_CYCLE_INFORMATION, ColumnName.OPERATION_ID,
                                   TableName.OPERATION, ColumnName.OPERATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.OPERATION_CYCLE_INFORMATION, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.ExecuteNonQuery (@"CREATE INDEX
operationcycleinformation_machineid_idx
ON operationcycleinformation
USING btree (machineid)");
      Database.ExecuteNonQuery (@"CREATE INDEX
operationcycleinformation_machineid_operationid_idx
ON operationcycleinformation
USING btree (machineid, operationid)");
      SetMachineModificationTable (TableName.OPERATION_CYCLE_INFORMATION);
      if (IsPartitioned (TableName.MACHINE_MODIFICATION) && !IsPartitioned (TableName.OPERATION_CYCLE_INFORMATION)) {
        PartitionTable (TableName.OPERATION_CYCLE_INFORMATION, TableName.MACHINE);
      }
    }
  }
}
