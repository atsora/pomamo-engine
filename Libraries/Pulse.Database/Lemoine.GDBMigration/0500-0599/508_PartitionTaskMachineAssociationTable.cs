// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 508:
  /// </summary>
  [Migration(508)]
  public class PartitionTaskMachineAssociationTable: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (PartitionTaskMachineAssociationTable).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (IsPartitioned (TableName.MACHINE_MODIFICATION)) {
        if (Database.TableExists (TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION) && !IsPartitioned (TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION)) {
          PartitionTable (TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION, TableName.MACHINE);
        }

        if (Database.TableExists (TableName.TASK_MACHINE_ASSOCIATION) && !IsPartitioned (TableName.TASK_MACHINE_ASSOCIATION)) {
          PartitionTable (TableName.TASK_MACHINE_ASSOCIATION, TableName.MACHINE);
        }
      }
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      if (Database.TableExists (TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION) && IsPartitioned (TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION)) {
        UnpartitionTable (TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION);
      }

      if (Database.TableExists (TableName.TASK_MACHINE_ASSOCIATION) && IsPartitioned (TableName.TASK_MACHINE_ASSOCIATION)) {
        UnpartitionTable (TableName.TASK_MACHINE_ASSOCIATION);
      }
    }
  }
}
