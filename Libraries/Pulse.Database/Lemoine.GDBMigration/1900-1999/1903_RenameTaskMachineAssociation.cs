// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Migrator.Framework;
using NHibernate.Mapping;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 1903: rename taskmachineassociation into manufordermachineassociation
  /// </summary>
  [Migration (1903)]
  public class RenameTaskMachineAssociation : MigrationExt
  {
    static readonly string USE_DEPRECATED_TASK_KEY = "Migration.UseDeprecatedTask";
    static readonly bool USE_DEPRECATED_TASK_DEFAULT = false;

    static readonly ILog log = LogManager.GetLogger (typeof (RenameTaskMachineAssociation).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (Database.TableExists (TableName.TASK_MACHINE_ASSOCIATION)) {
        Database.RemoveForeignKey (TableName.TASK_MACHINE_ASSOCIATION, $"fk_{TableName.TASK_MACHINE_ASSOCIATION}_machine");
        Database.RemoveForeignKey (TableName.TASK_MACHINE_ASSOCIATION, $"fk_{TableName.TASK_MACHINE_ASSOCIATION}_machinemodification");

        Database.RenameTable (TableName.TASK_MACHINE_ASSOCIATION, TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION);
        foreach (var suffix in new string[] { "range", "option" }) {
          Database.RenameColumn (TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION,
            $"{TableName.TASK_MACHINE_ASSOCIATION}{suffix}",
            $"{TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION}{suffix}");
        }
        Database.RenameColumn (TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION, ColumnName.TASK_ID, ColumnName.MANUFACTURING_ORDER_ID);
        Database.ExecuteNonQuery ("""
          UPDATE machinemodification
          SET modificationreferencedtable='ManufacturingOrderMachineAssociation'
          WHERE modificationreferencedtable='TaskMachineAssociation';
          """);

        // Indexes
        Database.ExecuteNonQuery ($"""
          ALTER INDEX {TableName.TASK_MACHINE_ASSOCIATION}_pkey
          RENAME TO {TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION}_pkey;
          """);
        Database.ExecuteNonQuery ($"""
          ALTER INDEX {TableName.TASK_MACHINE_ASSOCIATION}_machineid_idx
          RENAME TO {TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION}_machineid_idx;
          """);

        Database.GenerateForeignKey (TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION, ColumnName.MODIFICATION_ID,
                                     TableName.MACHINE_MODIFICATION, ColumnName.MODIFICATION_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
        Database.GenerateForeignKey (TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION, ColumnName.MACHINE_ID,
                                     TableName.MACHINE, ColumnName.MACHINE_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);

        // Rules
        Database.ExecuteNonQuery ($"""
          ALTER RULE {TableName.TASK_MACHINE_ASSOCIATION}_delete
          ON {TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION}
          RENAME TO {TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION}_delete;
          """);
      }
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      if (Lemoine.Info.ConfigSet.LoadAndGet (USE_DEPRECATED_TASK_KEY, USE_DEPRECATED_TASK_DEFAULT)) {
        Database.RemoveForeignKey (TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION, $"fk_{TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION}_machine");
        Database.RemoveForeignKey (TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION, $"fk_{TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION}_machinemodification");

        // Indexes
        Database.ExecuteNonQuery ($"""
          ALTER INDEX {TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION}_pkey
          RENAME TO {TableName.TASK_MACHINE_ASSOCIATION}_pkey;
          """);
        Database.ExecuteNonQuery ($"""
          ALTER INDEX {TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION}_machineid_idx
          RENAME TO {TableName.TASK_MACHINE_ASSOCIATION}_machineid_idx;
          """);

        Database.ExecuteNonQuery ("""
          UPDATE machinemodification
          SET modificationreferencedtable='TaskMachineAssociation'
          WHERE modificationreferencedtable='ManufacturingOrderMachineAssociation';
          """);
        Database.RenameColumn (TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION, "manuforderid", "taskid");
        foreach (var suffix in new string[] { "range", "option" }) {
          Database.RenameColumn (TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION,
            $"{TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION}{suffix}",
            $"{TableName.TASK_MACHINE_ASSOCIATION}{suffix}");
        }
        Database.RenameTable (TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION, TableName.TASK_MACHINE_ASSOCIATION);

        Database.GenerateForeignKey (TableName.TASK_MACHINE_ASSOCIATION, ColumnName.MODIFICATION_ID,
                                     TableName.MACHINE_MODIFICATION, ColumnName.MODIFICATION_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
        Database.GenerateForeignKey (TableName.TASK_MACHINE_ASSOCIATION, ColumnName.MACHINE_ID,
                                     TableName.MACHINE, ColumnName.MACHINE_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
      }
    }

  }
}
