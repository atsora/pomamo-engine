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
  /// Migration 074:
  /// </summary>
  [Migration(74)]
  public class DeliverablePieceMachineAssociation: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (DeliverablePieceMachineAssociation).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddDeliverablePieceMachineAssociation();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveDeliverablePieceMachineAssociation();
    }

    void AddDeliverablePieceMachineAssociation ()
    {
      // column COMPONENT_ID may be null
      // since one does not necessarily know it
      // when creating Deliverable Piece
      Database.AddTable (TableName.DELIVERABLE_PIECE_MACHINE_ASSOCIATION,
                         new Column (ColumnName.MODIFICATION_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column (ColumnName.DELIVERABLE_PIECE_ID, DbType.Int32),
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column (TableName.DELIVERABLE_PIECE_MACHINE_ASSOCIATION + "begindatetime", DbType.DateTime, ColumnProperty.NotNull),
                         new Column (TableName.DELIVERABLE_PIECE_MACHINE_ASSOCIATION + "enddatetime", DbType.DateTime));
      
      Database.GenerateForeignKey (TableName.DELIVERABLE_PIECE_MACHINE_ASSOCIATION, ColumnName.MODIFICATION_ID,
                                   TableName.MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.DELIVERABLE_PIECE_MACHINE_ASSOCIATION, ColumnName.DELIVERABLE_PIECE_ID,
                                   TableName.DELIVERABLE_PIECE, ColumnName.DELIVERABLE_PIECE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.DELIVERABLE_PIECE_MACHINE_ASSOCIATION, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      
      Database.ExecuteNonQuery (string.Format (@"CREATE RULE {0}_delete AS
ON DELETE TO {0}
DO ALSO DELETE FROM modification
  WHERE modificationid = OLD.modificationid;",
                                               TableName.DELIVERABLE_PIECE_MACHINE_ASSOCIATION));


    }

    void RemoveDeliverablePieceMachineAssociation()
    {

      Database.ExecuteNonQuery (@"DELETE FROM modification
WHERE modificationreferencedtable='" + TableName.DELIVERABLE_PIECE_MACHINE_ASSOCIATION + "'");

      Database.RemoveTable (TableName.DELIVERABLE_PIECE_MACHINE_ASSOCIATION);
    }

  }

}