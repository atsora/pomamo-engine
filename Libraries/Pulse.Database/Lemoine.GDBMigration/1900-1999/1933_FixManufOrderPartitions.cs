// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;

using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 1933: complete the rename of task into manuforder (migrations 1902 to 1904) in the tables partitioned by pgfkpart
  ///
  /// The first versions of the migrations 1902 to 1904 did not update the pgfkpart objects:
  /// - the partitions, the catalog entries and the insert trigger functions of a partitioned taskmachineassociation table
  ///   kept the old table name, so that any insert in manufordermachineassociation failed
  /// - pgfkpart.parentindex kept the old column names (for example taskid in the index of operationslot),
  ///   so that the creation of the partition of a new machine failed
  ///
  /// This migration also renames the index operationslot_machineid_taskid_idx into operationslot_machineid_manuforderid_idx,
  /// the name it gets in a new database. On a database with up-to-date migrations, it changes nothing else
  /// </summary>
  [Migration (1933)]
  public class FixManufOrderPartitions : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (FixManufOrderPartitions).FullName);

    static readonly string OLD_OPERATION_SLOT_INDEX = $"{TableName.OPERATION_SLOT}_{ColumnName.MACHINE_ID}_{ColumnName.TASK_ID}_idx";
    static readonly string NEW_OPERATION_SLOT_INDEX = $"{TableName.OPERATION_SLOT}_{ColumnName.MACHINE_ID}_{ColumnName.MANUFACTURING_ORDER_ID}_idx";

    /// <summary>
    /// Columns renamed from task to manuforder: table, old column, new column
    ///
    /// The tables of the plugins are included: if a plugin already renamed its column, its pgfkpart catalog entries are fixed here.
    /// Else the plugin renames the column later with TransformationProviderExt.RenameColumn, that also updates the catalog
    /// </summary>
    static readonly (string table, string oldColumn, string newColumn)[] RENAMED_COLUMNS = new[] {
      (TableName.OPERATION_SLOT, ColumnName.TASK_ID, ColumnName.MANUFACTURING_ORDER_ID),
      (TableName.OPERATION_SLOT, $"{TableName.OPERATION_SLOT}autotask", $"{TableName.OPERATION_SLOT}automanuforder"),
      (TableName.WORKORDER_MACHINE_ASSOCIATION, $"{TableName.WORKORDER_MACHINE_ASSOCIATION}resettask", $"{TableName.WORKORDER_MACHINE_ASSOCIATION}resetmanuforder"),
      (TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION, ColumnName.TASK_ID, ColumnName.MANUFACTURING_ORDER_ID),
      ("cyclecountsummary", ColumnName.TASK_ID, ColumnName.MANUFACTURING_ORDER_ID),
      ("cycledurationsummary", ColumnName.TASK_ID, ColumnName.MANUFACTURING_ORDER_ID),
      ("hourlyoperationsummary", ColumnName.TASK_ID, ColumnName.MANUFACTURING_ORDER_ID),
      ("iwpbymachinesummary", ColumnName.TASK_ID, ColumnName.MANUFACTURING_ORDER_ID),
    };

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // Partitions, catalog entries and insert trigger functions of manufordermachineassociation,
      // if they still have the name taskmachineassociation
      RenamePartitionedTable (TableName.TASK_MACHINE_ASSOCIATION, TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION);
      if (Database.TableExists (TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION)) {
        RenameIndex (TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION,
          $"{TableName.TASK_MACHINE_ASSOCIATION}_pkey",
          $"{TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION}_pkey");
        RenameIndex (TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION,
          $"{TableName.TASK_MACHINE_ASSOCIATION}_machineid_idx",
          $"{TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION}_machineid_idx");
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

      // Column names in the pgfkpart catalog, once the column is renamed
      foreach (var (table, oldColumn, newColumn) in RENAMED_COLUMNS) {
        if (Database.ColumnExists (table, newColumn) && !Database.ColumnExists (table, oldColumn)) {
          RenameColumnInPartitionMetadata (table, oldColumn, newColumn);
        }
      }

      // Index of migration 284, on the parent table or in the partitions
      RenameIndex (TableName.OPERATION_SLOT, OLD_OPERATION_SLOT_INDEX, NEW_OPERATION_SLOT_INDEX);
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      // Nothing to do: the fixed objects have the names the migrations 284 and 1902 to 1904 give them in a new database
    }
  }
}
