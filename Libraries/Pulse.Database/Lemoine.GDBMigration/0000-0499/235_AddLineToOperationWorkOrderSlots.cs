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
  /// Migration 235: Add a reference to the line in OperationSlot and WorkOrderSlot
  /// </summary>
  [Migration(235)]
  public class AddLineToOperationWorkOrderSlots: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddLineToOperationWorkOrderSlots).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      WorkOrderMachineAssociationUp ();
      WorkOrderSlotUp ();
      OperationSlotUp ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      OperationSlotDown ();
      WorkOrderSlotDown ();
      WorkOrderMachineAssociationDown ();
    }

    void OperationSlotUp ()
    {
      Database.AddColumn (TableName.OPERATION_SLOT,
                          new Column (ColumnName.LINE_ID, DbType.Int32, ColumnProperty.Null));
      Database.GenerateForeignKey (TableName.OPERATION_SLOT, ColumnName.LINE_ID,
                                   TableName.LINE_OLD, ColumnName.LINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
    }
    
    void OperationSlotDown ()
    {
      Database.RemoveColumn (TableName.OPERATION_SLOT,
                             ColumnName.LINE_ID);
    }
    
    void WorkOrderSlotUp ()
    {
      Database.AddColumn (TableName.WORK_ORDER_SLOT,
                          new Column (ColumnName.LINE_ID, DbType.Int32, ColumnProperty.Null));
      Database.GenerateForeignKey (TableName.WORK_ORDER_SLOT, ColumnName.LINE_ID,
                                   TableName.LINE_OLD, ColumnName.LINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
    }
    
    void WorkOrderSlotDown ()
    {
      Database.RemoveColumn (TableName.WORK_ORDER_SLOT,
                             ColumnName.LINE_ID);
    }
    
    void WorkOrderMachineAssociationUp ()
    {
      Database.AddColumn (TableName.WORKORDER_MACHINE_ASSOCIATION,
                          new Column (ColumnName.LINE_ID, DbType.Int32, ColumnProperty.Null));
      Database.GenerateForeignKey (TableName.WORKORDER_MACHINE_ASSOCIATION, ColumnName.LINE_ID,
                                   TableName.LINE_OLD, ColumnName.LINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
    }
    
    void WorkOrderMachineAssociationDown ()
    {
      Database.RemoveColumn (TableName.WORKORDER_MACHINE_ASSOCIATION,
                             ColumnName.LINE_ID);
    }
    
  }
}
