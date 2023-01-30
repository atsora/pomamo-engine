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
  /// Migration 194: Use to repair forgotten foreign key on table DeliverablePiece
  /// </summary>
  [Migration(194)]
  public class AddForeignKeyDeliverablePieceTable: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddForeignKeyDeliverablePieceTable).FullName);
    private const string FK_NAME = "fk_deliverablepiece_component";
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // deliverablepiece.componentid
      Database.AddForeignKey(FK_NAME,
                             TableName.DELIVERABLE_PIECE, ColumnName.COMPONENT_ID,
                             TableName.COMPONENT, ColumnName.COMPONENT_ID, 
                             Migrator.Framework.ForeignKeyConstraint.Cascade);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveForeignKey(TableName.DELIVERABLE_PIECE, FK_NAME);
    }
  }
}
