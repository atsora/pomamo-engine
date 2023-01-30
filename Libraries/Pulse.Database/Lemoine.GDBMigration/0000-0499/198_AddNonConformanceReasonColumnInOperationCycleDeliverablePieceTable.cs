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
  /// Migration 198: Add column nonconformancereasonid in operationcycledeliverablepiece
  /// </summary>
  [Migration(198)]
  public class AddNonConformanceReasonColumnInOperationCycleDeliverablePieceTable: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddNonConformanceReasonColumnInOperationCycleDeliverablePieceTable).FullName);
    private const string FK_NAME = "fk_operationcycledeliverablepiece_nonconformancereason";
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn(TableName.OPERATION_CYCLE_DELIVERABLE_PIECE, ColumnName.NON_CONFORMANCE_REASON_ID, DbType.Int32);
      
      Database.GenerateForeignKey(
                             TableName.OPERATION_CYCLE_DELIVERABLE_PIECE, ColumnName.NON_CONFORMANCE_REASON_ID, 
                             TableName.NON_CONFORMANCE_REASON, ColumnName.NON_CONFORMANCE_REASON_ID, 
                             Migrator.Framework.ForeignKeyConstraint.SetNull);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn(TableName.OPERATION_CYCLE_DELIVERABLE_PIECE, ColumnName.NON_CONFORMANCE_REASON_ID);
    }
  }
}
