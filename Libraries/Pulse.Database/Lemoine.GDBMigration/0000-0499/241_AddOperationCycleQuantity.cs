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
  /// Migration 241: add a column quantity to operationcycle table
  /// </summary>
  [Migration(241)]
  public class AddOperationCycleQuantity: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddOperationCycleQuantity).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      OperationCycleUp ();
      OperationSlotUp ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      OperationSlotDown ();
      OperationCycleDown ();
    }

    void OperationCycleUp ()
    {
      Database.AddColumn (TableName.OPERATION_CYCLE,
                          new Column (TableName.OPERATION_CYCLE + "quantity", DbType.Int32, ColumnProperty.Null));
    }
    
    void OperationCycleDown ()
    {
      Database.RemoveColumn (TableName.OPERATION_CYCLE,
                             TableName.OPERATION_CYCLE + "quantity");
    }
    
    void OperationSlotUp ()
    {
      Database.AddColumn (TableName.OPERATION_SLOT,
                          new Column (TableName.OPERATION_SLOT + "adjustedcycles", DbType.Int32, ColumnProperty.NotNull, 0));
      Database.AddColumn (TableName.OPERATION_SLOT,
                          new Column (TableName.OPERATION_SLOT + "adjustedquantity", DbType.Int32, ColumnProperty.NotNull, 0));
    }
    
    void OperationSlotDown ()
    {
      Database.RemoveColumn (TableName.OPERATION_SLOT,
                             TableName.OPERATION_SLOT + "adjustedquantity");
      Database.RemoveColumn (TableName.OPERATION_SLOT,
                             TableName.OPERATION_SLOT + "adjustedcycles");
    }
  }
}
