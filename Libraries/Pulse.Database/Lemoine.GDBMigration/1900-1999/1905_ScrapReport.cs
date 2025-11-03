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
  /// Migration 1905: Scrap report
  /// </summary>
  [Migration (1905)]
  public class ScrapReport : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ScrapReport).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (!Database.TableExists (TableName.SCRAP_REPORT)) {
        CreateScrapReportTable ();
        PartitionTable (TableName.SCRAP_REPORT, TableName.MACHINE);
      }
      if (!Database.TableExists (TableName.SCRAP_REASON_REPORT)) {
        CreateScrapReasonReportTable ();
        PartitionTable (TableName.SCRAP_REASON_REPORT, TableName.MACHINE);
      }
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      if (Database.TableExists (TableName.SCRAP_REASON_REPORT)) {
        if (IsPartitioned (TableName.SCRAP_REASON_REPORT)) {
          UnpartitionTable (TableName.SCRAP_REASON_REPORT);
        }
        Database.RemoveTable (TableName.SCRAP_REASON_REPORT);
      }
      if (Database.TableExists (TableName.SCRAP_REPORT)) {
        if (IsPartitioned (TableName.SCRAP_REPORT)) {
          UnpartitionTable (TableName.SCRAP_REPORT);
        }
        RemoveMachineModificationTable (TableName.SCRAP_REPORT);
        Database.RemoveTable (TableName.SCRAP_REPORT);
      }
    }

    void CreateScrapReportTable ()
    {
      Database.AddTable (TableName.SCRAP_REPORT,
                        new Column (ColumnName.MODIFICATION_ID, DbType.Int64, ColumnProperty.PrimaryKey),
                        new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                        new Column (TableName.SCRAP_REPORT + "datetimerange", DbType.Int32, ColumnProperty.NotNull),
                        new Column (TableName.SCRAP_REPORT + "day", DbType.Date, ColumnProperty.Null),
                        new Column (ColumnName.SHIFT_ID, DbType.Int32),
                        new Column (ColumnName.OPERATION_ID, DbType.Int32, ColumnProperty.NotNull),
                        new Column (ColumnName.COMPONENT_ID, DbType.Int32),
                        new Column (ColumnName.WORK_ORDER_ID, DbType.Int32),
                        new Column (ColumnName.MANUFACTURING_ORDER_ID, DbType.Int32),
                        new Column ("scrapreportcycles", DbType.Int32, ColumnProperty.NotNull),
                        new Column ("scrapreportparts", DbType.Int32, ColumnProperty.NotNull),
                        new Column ("scrapreportvalid", DbType.Int32, ColumnProperty.NotNull),
                        new Column ("scrapreportsetup", DbType.Int32, ColumnProperty.NotNull),
                        new Column ("scrapreportscrap", DbType.Int32, ColumnProperty.NotNull),
                        new Column ("scrapreportfixable", DbType.Int32, ColumnProperty.NotNull),
                        new Column ("scrapdetails", DbType.String),
                        new Column (TableName.SCRAP_REPORT + "update", DbType.Int64, ColumnProperty.Null));
      MakeColumnTsRange (TableName.SCRAP_REPORT, TableName.SCRAP_REPORT + "datetimerange");

      Database.GenerateForeignKey (TableName.SCRAP_REPORT, ColumnName.MODIFICATION_ID,
                                  TableName.MACHINE_MODIFICATION, ColumnName.MODIFICATION_ID,
                                  Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.SCRAP_REPORT, ColumnName.MACHINE_ID,
                                  TableName.MACHINE, ColumnName.MACHINE_ID,
                                  Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.SCRAP_REPORT, ColumnName.SHIFT_ID,
                                   TableName.SHIFT, ColumnName.SHIFT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      Database.GenerateForeignKey (TableName.SCRAP_REPORT, ColumnName.OPERATION_ID,
                                  TableName.OPERATION, ColumnName.OPERATION_ID,
                                  Migrator.Framework.ForeignKeyConstraint.Restrict);
      Database.GenerateForeignKey (TableName.SCRAP_REPORT, ColumnName.COMPONENT_ID,
                                  TableName.COMPONENT, ColumnName.COMPONENT_ID,
                                  Migrator.Framework.ForeignKeyConstraint.SetNull);
      Database.GenerateForeignKey (TableName.SCRAP_REPORT, ColumnName.WORK_ORDER_ID,
                                  TableName.WORK_ORDER, ColumnName.WORK_ORDER_ID,
                                  Migrator.Framework.ForeignKeyConstraint.SetNull);
      // Note: manuforder may be a view => no foreign key
      Database.GenerateForeignKey (TableName.SCRAP_REPORT, TableName.SCRAP_REPORT + "update",
                                  TableName.SCRAP_REPORT, ColumnName.MODIFICATION_ID,
                                  Migrator.Framework.ForeignKeyConstraint.Restrict);

      // Constraints
      AddNoOverlapConstraintCondition (TableName.SCRAP_REPORT, "scrapreportupdate = NULL", TableName.SCRAP_REPORT + "datetimerange", ColumnName.MACHINE_ID);

      SetMachineModificationTable (TableName.SCRAP_REPORT);

      AddIndex (TableName.SCRAP_REPORT,
                ColumnName.MACHINE_ID,
                TableName.SCRAP_REPORT + "day",
                ColumnName.SHIFT_ID);
      AddGistIndex (TableName.SCRAP_REPORT,
                    ColumnName.MACHINE_ID,
                    TableName.SCRAP_REPORT + "datetimerange");
      AddIndex (TableName.SCRAP_REPORT,
                ColumnName.MACHINE_ID,
                ColumnName.OPERATION_ID);
      AddIndex (TableName.SCRAP_REPORT,
                ColumnName.MACHINE_ID,
                ColumnName.MANUFACTURING_ORDER_ID);
    }

    void CreateScrapReasonReportTable ()
    {
      Database.AddTable (TableName.SCRAP_REASON_REPORT,
                        new Column (TableName.SCRAP_REASON_REPORT + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                        new Column (ColumnName.MODIFICATION_ID, DbType.Int64, ColumnProperty.NotNull),
                        new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                        new Column (ColumnName.NON_CONFORMANCE_REASON_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                        new Column ("scrapreasonquantity", DbType.Int32, ColumnProperty.NotNull));
      Database.GenerateForeignKey (TableName.SCRAP_REASON_REPORT, ColumnName.MODIFICATION_ID,
                                  TableName.SCRAP_REPORT, ColumnName.MODIFICATION_ID,
                                  Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.SCRAP_REASON_REPORT, ColumnName.MACHINE_ID,
                                  TableName.MACHINE, ColumnName.MACHINE_ID,
                                  Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.SCRAP_REASON_REPORT, ColumnName.NON_CONFORMANCE_REASON_ID,
                                  TableName.NON_CONFORMANCE_REASON, ColumnName.NON_CONFORMANCE_REASON_ID,
                                  Migrator.Framework.ForeignKeyConstraint.Restrict);
      AddIndex (TableName.SCRAP_REASON_REPORT,
                ColumnName.MACHINE_ID,
                ColumnName.MODIFICATION_ID);
    }
  }
}
