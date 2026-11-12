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

        // The table may be partitioned by pgfkpart (migration 508 or the partitioning script):
        // its partitions, its pgfkpart catalog entries and its insert trigger functions are renamed with it
        RenamePartitionedTable (TableName.TASK_MACHINE_ASSOCIATION, TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION);
        foreach (var suffix in new string[] { "range", "option" }) {
          RenamePartitionedColumn (TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION,
            $"{TableName.TASK_MACHINE_ASSOCIATION}{suffix}",
            $"{TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION}{suffix}");
        }
        RenamePartitionedColumn (TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION, ColumnName.TASK_ID, ColumnName.MANUFACTURING_ORDER_ID);
        Database.ExecuteNonQuery ("""
          UPDATE machinemodification
          SET modificationreferencedtable='ManufacturingOrderMachineAssociation'
          WHERE modificationreferencedtable='TaskMachineAssociation';
          """);

        // Indexes: on a partitioned table, the machineid index is not on the parent table any more
        // (it is in pgfkpart.parentindex, already renamed with the table)
        RenameIndex (TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION,
          $"{TableName.TASK_MACHINE_ASSOCIATION}_pkey",
          $"{TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION}_pkey");
        RenameIndex (TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION,
          $"{TableName.TASK_MACHINE_ASSOCIATION}_machineid_idx",
          $"{TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION}_machineid_idx");

        // On a partitioned table, the foreign key to machinemodification is kept by pgfkpart in the partitions
        if (!IsPartitioned (TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION)) {
          Database.GenerateForeignKey (TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION, ColumnName.MODIFICATION_ID,
                                       TableName.MACHINE_MODIFICATION, ColumnName.MODIFICATION_ID,
                                       Migrator.Framework.ForeignKeyConstraint.Cascade);
        }
        Database.GenerateForeignKey (TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION, ColumnName.MACHINE_ID,
                                     TableName.MACHINE, ColumnName.MACHINE_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);

        // Rules
        RenameDeleteRule ();
      }
    }

    /// <summary>
    /// Rename the delete rule of the machine modification table, if it has still its old name
    /// </summary>
    void RenameDeleteRule ()
    {
      Database.ExecuteNonQuery ($$"""
        DO $renamerule$
        BEGIN
          IF EXISTS (SELECT 1 FROM pg_rules
                     WHERE schemaname='public' AND tablename='{{TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION}}'
                       AND rulename='{{TableName.TASK_MACHINE_ASSOCIATION}}_delete') THEN
            ALTER RULE {{TableName.TASK_MACHINE_ASSOCIATION}}_delete
            ON {{TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION}}
            RENAME TO {{TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION}}_delete;
          END IF;
        END
        $renamerule$;
        """);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      if (Lemoine.Info.ConfigSet.LoadAndGet (USE_DEPRECATED_TASK_KEY, USE_DEPRECATED_TASK_DEFAULT)) {
        Database.RemoveForeignKey (TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION, $"fk_{TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION}_machine");
        Database.RemoveForeignKey (TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION, $"fk_{TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION}_machinemodification");

        Database.ExecuteNonQuery ("""
          UPDATE machinemodification
          SET modificationreferencedtable='TaskMachineAssociation'
          WHERE modificationreferencedtable='ManufacturingOrderMachineAssociation';
          """);
        RenamePartitionedColumn (TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION, "manuforderid", "taskid");
        foreach (var suffix in new string[] { "range", "option" }) {
          RenamePartitionedColumn (TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION,
            $"{TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION}{suffix}",
            $"{TableName.TASK_MACHINE_ASSOCIATION}{suffix}");
        }
        RenamePartitionedTable (TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION, TableName.TASK_MACHINE_ASSOCIATION);

        // Indexes
        RenameIndex (TableName.TASK_MACHINE_ASSOCIATION,
          $"{TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION}_pkey",
          $"{TableName.TASK_MACHINE_ASSOCIATION}_pkey");
        RenameIndex (TableName.TASK_MACHINE_ASSOCIATION,
          $"{TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION}_machineid_idx",
          $"{TableName.TASK_MACHINE_ASSOCIATION}_machineid_idx");

        if (!IsPartitioned (TableName.TASK_MACHINE_ASSOCIATION)) {
          Database.GenerateForeignKey (TableName.TASK_MACHINE_ASSOCIATION, ColumnName.MODIFICATION_ID,
                                       TableName.MACHINE_MODIFICATION, ColumnName.MODIFICATION_ID,
                                       Migrator.Framework.ForeignKeyConstraint.Cascade);
        }
        Database.GenerateForeignKey (TableName.TASK_MACHINE_ASSOCIATION, ColumnName.MACHINE_ID,
                                     TableName.MACHINE, ColumnName.MACHINE_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
      }
    }

  }
}
