// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration.Properties
{
  /// <summary>
  /// Migration 069: add the DeliverablePiece table
  /// </summary>
  [Migration(69)]
  public class DeliverablePiece: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (DeliverablePiece).FullName);

    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddDeliverablePiece();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveDeliverablePiece();
    }

    void AddDeliverablePiece ()
    {
      // column COMPONENT_ID may be null
      // since one does not necessarily know it 
      // when creating Deliverable Piece 
      Database.AddTable (TableName.DELIVERABLE_PIECE,
                         new Column (ColumnName.DELIVERABLE_PIECE_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (ColumnName.DELIVERABLE_PIECE_VERSION, DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ColumnName.DELIVERABLE_PIECE_CODE, DbType.String, ColumnProperty.NotNull),
                         new Column (ColumnName.COMPONENT_ID, DbType.Int32));

      Database.GenerateForeignKey (TableName.DELIVERABLE_PIECE, ColumnName.COMPONENT_ID,
                                   TableName.COMPONENT, ColumnName.COMPONENT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      
      ResetSequence (TableName.DELIVERABLE_PIECE, ColumnName.DELIVERABLE_PIECE_ID);

    }

    void RemoveDeliverablePiece()
    {
      Database.RemoveTable (TableName.DELIVERABLE_PIECE);
    }
    
  }
}
