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
  /// Migration 1936: scrap counters in operationslot
  ///
  /// <item>operationslot: sum of the scrap reports whose range ends in the slot (six integer columns),
  /// replacing the unused json column operationslotscrap (migration 1907)</item>
  /// <item>scrapreport: index to find the reports that are updated by another one</item>
  /// <item>scrapreport: fix the condition of the no-overlap constraint (scrapreportupdate = NULL was never true)</item>
  /// </summary>
  [Migration (1936)]
  public class AddOperationSlotScrap : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddOperationSlotScrap).FullName);

    static readonly string[] OPERATION_SLOT_COUNTERS = new string[] {
      TableName.OPERATION_SLOT + "reportedcycles",
      TableName.OPERATION_SLOT + "reportedparts",
      TableName.OPERATION_SLOT + "validparts",
      TableName.OPERATION_SLOT + "setupparts",
      TableName.OPERATION_SLOT + "scrapparts",
      TableName.OPERATION_SLOT + "fixableparts",
    };

    static readonly string OPERATION_SLOT_SCRAP_JSON = TableName.OPERATION_SLOT + "scrap";
    static readonly string SCRAP_REPORT_RANGE = TableName.SCRAP_REPORT + "datetimerange";
    static readonly string SCRAP_REPORT_UPDATE = TableName.SCRAP_REPORT + "update";

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      foreach (var column in OPERATION_SLOT_COUNTERS) {
        if (!Database.ColumnExists (TableName.OPERATION_SLOT, column)) {
          Database.AddColumn (TableName.OPERATION_SLOT,
                              new Column (column, DbType.Int32, ColumnProperty.NotNull, 0));
        }
      }
      if (Database.ColumnExists (TableName.OPERATION_SLOT, OPERATION_SLOT_SCRAP_JSON)) {
        Database.RemoveColumn (TableName.OPERATION_SLOT, OPERATION_SLOT_SCRAP_JSON);
      }

      // Note: a scrap report whose range ends in an operation slot also overlaps it,
      // so the existing gist index on (machineid, scrapreportdatetimerange) is used to get them.
      // This index is for the reports that are updated by another one
      AddIndex (TableName.SCRAP_REPORT, ColumnName.MACHINE_ID, SCRAP_REPORT_UPDATE);

      FixNoOverlapConstraint ();
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveIndex (TableName.SCRAP_REPORT, ColumnName.MACHINE_ID, SCRAP_REPORT_UPDATE);
      if (!Database.ColumnExists (TableName.OPERATION_SLOT, OPERATION_SLOT_SCRAP_JSON)) {
        Database.AddColumn (TableName.OPERATION_SLOT,
                            new Column (OPERATION_SLOT_SCRAP_JSON, DbType.String));
        MakeColumnJson (TableName.OPERATION_SLOT, OPERATION_SLOT_SCRAP_JSON);
      }
      foreach (var column in OPERATION_SLOT_COUNTERS) {
        if (Database.ColumnExists (TableName.OPERATION_SLOT, column)) {
          Database.RemoveColumn (TableName.OPERATION_SLOT, column);
        }
      }
    }

    void FixNoOverlapConstraint ()
    {
      // Two initial reports (that do not update another one) must not overlap.
      // If some rows already violate it, keep the previous constraint, else the migration would fail
      var nbOverlaps = Convert.ToInt64 (Database.ExecuteScalar ($"""
        SELECT COUNT(*)
        FROM {TableName.SCRAP_REPORT} a
        JOIN {TableName.SCRAP_REPORT} b
          ON a.{ColumnName.MACHINE_ID} = b.{ColumnName.MACHINE_ID}
          AND a.{ColumnName.MODIFICATION_ID} < b.{ColumnName.MODIFICATION_ID}
          AND a.{SCRAP_REPORT_RANGE} && b.{SCRAP_REPORT_RANGE}
        WHERE a.{SCRAP_REPORT_UPDATE} IS NULL
          AND b.{SCRAP_REPORT_UPDATE} IS NULL
        """));
      if (0 < nbOverlaps) {
        log.Error ($"FixNoOverlapConstraint: {nbOverlaps} overlapping initial scrap reports => the no-overlap constraint is not fixed");
        return;
      }
      AddNoOverlapConstraintCondition (TableName.SCRAP_REPORT, $"{SCRAP_REPORT_UPDATE} IS NULL", SCRAP_REPORT_RANGE, ColumnName.MACHINE_ID);
    }
  }
}
