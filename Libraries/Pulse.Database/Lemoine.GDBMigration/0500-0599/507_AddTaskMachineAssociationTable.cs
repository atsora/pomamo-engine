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
  /// Migration 507:
  /// </summary>
  [Migration (507)]
  public class AddTaskMachineAssociationTable : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddTaskMachineAssociationTable).FullName);

    // Keep the options until all customers were upgraded to version >= 19.0.0
    // And after version >= 19.0.0 was installed at new customers
    static readonly string USE_DEPRECATED_TASK_MACHINE_ASSOCIATION_KEY = "Migration.UseDeprecatedTaskMachineAssociation";
    static readonly bool USE_DEPRECATED_TASK_MACHINE_ASSOCIATION_DEFAULT = false;

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (Lemoine.Info.ConfigSet.LoadAndGet (USE_DEPRECATED_TASK_MACHINE_ASSOCIATION_KEY, USE_DEPRECATED_TASK_MACHINE_ASSOCIATION_DEFAULT)) {
        Database.AddTable (TableName.TASK_MACHINE_ASSOCIATION,
                           new Column (ColumnName.MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                           new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                           new Column (TableName.TASK_MACHINE_ASSOCIATION + "range", DbType.Int32, ColumnProperty.NotNull),
                           new Column (ColumnName.TASK_ID, DbType.Int32),
                           new Column (TableName.TASK_MACHINE_ASSOCIATION + "option", DbType.Int32));
        MakeColumnTsRange (TableName.TASK_MACHINE_ASSOCIATION, TableName.TASK_MACHINE_ASSOCIATION + "range");
        Database.GenerateForeignKey (TableName.TASK_MACHINE_ASSOCIATION, ColumnName.MODIFICATION_ID,
                                     TableName.MACHINE_MODIFICATION, ColumnName.MODIFICATION_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
        Database.GenerateForeignKey (TableName.TASK_MACHINE_ASSOCIATION, ColumnName.MACHINE_ID,
                                     TableName.MACHINE, ColumnName.MACHINE_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
        SetMachineModificationTable (TableName.TASK_MACHINE_ASSOCIATION);
        AddIndex (TableName.TASK_MACHINE_ASSOCIATION,
                  ColumnName.MACHINE_ID);
      }
      else { // manufordermachineassociation
        Database.AddTable (TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION,
                           new Column (ColumnName.MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                           new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                           new Column (TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION + "range", DbType.Int32, ColumnProperty.NotNull),
                           new Column (ColumnName.MANUFACTURING_ORDER_ID, DbType.Int32),
                           new Column (TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION + "option", DbType.Int32));
        MakeColumnTsRange (TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION, TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION + "range");
        Database.GenerateForeignKey (TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION, ColumnName.MODIFICATION_ID,
                                     TableName.MACHINE_MODIFICATION, ColumnName.MODIFICATION_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
        Database.GenerateForeignKey (TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION, ColumnName.MACHINE_ID,
                                     TableName.MACHINE, ColumnName.MACHINE_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
        SetMachineModificationTable (TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION);
        AddIndex (TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION,
                  ColumnName.MACHINE_ID);
      }
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      if (Database.TableExists (TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION)) {
        Database.ExecuteNonQuery ($"""
          DELETE FROM machinemodification
          WHERE modificationreferencedtable='{TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION}'
          """);
        Database.RemoveTable (TableName.MANUFACTURING_ORDER_MACHINE_ASSOCIATION);
      }

      if (Database.TableExists (TableName.TASK_MACHINE_ASSOCIATION)) {
        Database.ExecuteNonQuery ($"""
          DELETE FROM machinemodification
          WHERE modificationreferencedtable='{TableName.TASK_MACHINE_ASSOCIATION}'
          """);
        Database.RemoveTable (TableName.TASK_MACHINE_ASSOCIATION);
      }
    }
  }
}
