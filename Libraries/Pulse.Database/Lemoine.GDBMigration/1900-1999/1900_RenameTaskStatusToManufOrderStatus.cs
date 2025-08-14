// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using System.Net.NetworkInformation;
using Lemoine.Core.Log;
using Migrator.Framework;
using NHibernate.Mapping;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 1900: rename task into manufacturing order in table workordermachineassociation
  /// </summary>
  [Migration (1900)]
  public class RenameTaskStatusToManufOrderStatus : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (RenameTaskToManufOrder).FullName);

    static readonly string USE_DEPRECATED_TASK_STATUS_KEY = "Migration.UseDeprecatedTaskStatus";
    static readonly bool USE_DEPRECATED_TASK_STATUS_DEFAULT = false;

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (Database.TableExists (TableName.TASK_STATUS)) {
        // Remove some constraints
        RemoveConstraint (TableName.TASK_STATUS, $"{TableName.TASK_STATUS}_name_translationkey");
        RemoveConstraintColor (TableName.TASK_STATUS, $"{TableName.TASK_STATUS}color");

        // Rename table
        Database.RenameTable (TableName.TASK_STATUS, TableName.MANUFACTURING_ORDER_STATUS);
        foreach (var suffix in new string[] { "id", "version", "name", "translationkey", "color" }) {
          Database.RenameColumn (TableName.MANUFACTURING_ORDER_STATUS,
            $"{TableName.TASK_STATUS}{suffix}",
            $"{TableName.MANUFACTURING_ORDER_STATUS}{suffix}");
        }

        // Values
        foreach (var suffix in new string[] { "New", "Ready", "Running", "Complete", "Hold" }) {
          Database.ExecuteNonQuery ($"""
            UPDATE {TableName.MANUFACTURING_ORDER_STATUS}
            SET {TableName.MANUFACTURING_ORDER_STATUS}translationkey='ManufacturingOrderStatus{suffix}'
            WHERE {TableName.MANUFACTURING_ORDER_STATUS}translationkey='TaskStatus{suffix}';
            """);
          Database.ExecuteNonQuery ($"""
            UPDATE {TableName.TRANSLATION}
            SET translationkey='ManufacturingOrderStatus{suffix}'
            WHERE translationkey='TaskStatus{suffix}';
            """);
        }

        // Indexes
        Database.ExecuteNonQuery ($"""
          ALTER INDEX taskstatus_pkey
          RENAME TO manuforderstatus_pkey;
          """);
        Database.ExecuteNonQuery ($"""
          ALTER INDEX taskstatus_taskstatusname_key
          RENAME TO manuforderstatus_manuforderstatusname_key;
          """);
        Database.ExecuteNonQuery ($"""
          ALTER INDEX taskstatus_taskstatustranslationkey_key
          RENAME TO manuforderstatus_manuforderstatustranslationkey_key;
          """);

        // Constraints
        AddConstraintColor (TableName.MANUFACTURING_ORDER_STATUS,
                            TableName.MANUFACTURING_ORDER_STATUS + "color");
        AddConstraintNameTranslationKey (TableName.MANUFACTURING_ORDER_STATUS,
                                         TableName.MANUFACTURING_ORDER_STATUS + "name",
                                         TableName.MANUFACTURING_ORDER_STATUS + "translationkey");

        // Sequence
        Database.ExecuteNonQuery ($"""
          ALTER SEQUENCE taskstatus_taskstatusid_seq
          RENAME TO manuforderstatus_manuforderstatusid_seq;
          """);
      }
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      if (Lemoine.Info.ConfigSet.LoadAndGet (USE_DEPRECATED_TASK_STATUS_KEY, USE_DEPRECATED_TASK_STATUS_DEFAULT)) {
        // Sequence
        Database.ExecuteNonQuery ($"""
          ALTER SEQUENCE manuforderstatus_manuforderstatusid_seq
          RENAME TO taskstatus_taskstatusid_seq;
          """);

        // Remove some constraints
        RemoveConstraint (TableName.MANUFACTURING_ORDER_STATUS, $"{TableName.MANUFACTURING_ORDER_STATUS}_name_translationkey");
        RemoveConstraintColor (TableName.MANUFACTURING_ORDER_STATUS, $"{TableName.MANUFACTURING_ORDER_STATUS}color");

        // Values
        foreach (var suffix in new string[] { "New", "Ready", "Running", "Complete", "Hold" }) {
          Database.ExecuteNonQuery ($"""
            UPDATE {TableName.MANUFACTURING_ORDER_STATUS}
            SET {TableName.MANUFACTURING_ORDER_STATUS}translationkey='TaskStatus{suffix}'
            WHERE {TableName.MANUFACTURING_ORDER_STATUS}translationkey='ManufacturingOrderStatus{suffix}';
            """);
          Database.ExecuteNonQuery ($"""
            UPDATE {TableName.TRANSLATION}
            SET translationkey='TaskStatus{suffix}'
            WHERE translationkey='ManufacturingOrderStatus{suffix}';
            """);
        }

        foreach (var suffix in new string[] { "id", "version", "name", "translationkey", "color" }) {
          Database.RenameColumn (TableName.MANUFACTURING_ORDER_STATUS,
            $"{TableName.MANUFACTURING_ORDER_STATUS}{suffix}",
          $"{TableName.TASK_STATUS}{suffix}");
        }
        Database.RenameTable (TableName.MANUFACTURING_ORDER_STATUS, TableName.TASK_STATUS);

        // Indexes
        Database.ExecuteNonQuery ($"""
          ALTER INDEX manuforderstatus_pkey
          RENAME TO taskstatus_pkey;
          """);
        Database.ExecuteNonQuery ($"""
          ALTER INDEX manuforderstatus_manuforderstatusname_key
          RENAME TO taskstatus_taskstatusname_key;
          """);
        Database.ExecuteNonQuery ($"""
          ALTER INDEX manuforderstatus_manuforderstatustranslationkey_key
          RENAME TO taskstatus_taskstatustranslationkey_key;
          """);

        // Constraints
        AddConstraintColor (TableName.TASK_STATUS,
                            TableName.TASK_STATUS + "color");
        AddConstraintNameTranslationKey (TableName.TASK_STATUS,
                                         TableName.TASK_STATUS + "name",
                                         TableName.TASK_STATUS + "translationkey");
      }
    }

  }
}
