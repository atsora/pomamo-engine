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
  /// Migration 089:
  /// add a not-null contraint on column component
  /// add a unique constraint on pair of columns (code, component)
  /// also remove DeliverablePieceMachineAssociation table (not used)
  /// </summary>
  [Migration(89)]
  public class ConstrainDeliverablePiece: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ConstrainDeliverablePiece).FullName);
    
    static readonly string MIG_SUFFIX = "Mig";
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery(String.Format("DROP TABLE IF EXISTS {0}", TableName.DELIVERABLE_PIECE_MACHINE_ASSOCIATION));
      
      Database.RenameTable (TableName.DELIVERABLE_PIECE,
                            TableName.DELIVERABLE_PIECE + MIG_SUFFIX);
      
      RemoveSequence("deliverablepiece_deliverablepieceid_seq");
      
      // change w.r.t Migration 69: column COMPONENT_ID may not be null
      // also a pair (code,component) is unique
      Database.AddTable (TableName.DELIVERABLE_PIECE,
                         new Column (ColumnName.DELIVERABLE_PIECE_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (ColumnName.DELIVERABLE_PIECE_VERSION, DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ColumnName.DELIVERABLE_PIECE_CODE, DbType.String, ColumnProperty.NotNull),
                         new Column (ColumnName.COMPONENT_ID, DbType.Int32, ColumnProperty.NotNull));

      Database.GenerateForeignKey (TableName.DELIVERABLE_PIECE, ColumnName.COMPONENT_ID,
                                   TableName.COMPONENT, ColumnName.COMPONENT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      
      AddNamedUniqueConstraint("deliverablepiece_unique_code_componentid",
                            TableName.DELIVERABLE_PIECE,
                            new string[] {ColumnName.DELIVERABLE_PIECE_CODE,ColumnName.COMPONENT_ID});

      ResetSequence (TableName.DELIVERABLE_PIECE, ColumnName.DELIVERABLE_PIECE_ID);
      
      Database.ExecuteNonQuery (string.Format (@"INSERT INTO {0}
(deliverablepiececode, componentid)
SELECT deliverablepiececode, componentid
FROM {0}{1} WHERE componentid IS NOT NULL",
                                               TableName.DELIVERABLE_PIECE,
                                               MIG_SUFFIX));
      
      Database.RemoveTable (TableName.DELIVERABLE_PIECE + MIG_SUFFIX);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      
      // does not reestablish deliverablepiece/machine association
      
      Database.RenameTable (TableName.DELIVERABLE_PIECE,
                            TableName.DELIVERABLE_PIECE + MIG_SUFFIX);
      
      RemoveSequence("deliverablepiece_deliverablepieceid_seq");
      
      Database.AddTable (TableName.DELIVERABLE_PIECE,
                         new Column (ColumnName.DELIVERABLE_PIECE_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (ColumnName.DELIVERABLE_PIECE_VERSION, DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ColumnName.DELIVERABLE_PIECE_CODE, DbType.String, ColumnProperty.NotNull),
                         new Column (ColumnName.COMPONENT_ID, DbType.Int32));

      Database.GenerateForeignKey (TableName.DELIVERABLE_PIECE, ColumnName.COMPONENT_ID,
                                   TableName.COMPONENT, ColumnName.COMPONENT_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      
      ResetSequence (TableName.DELIVERABLE_PIECE, ColumnName.DELIVERABLE_PIECE_ID);
      
      Database.ExecuteNonQuery (string.Format (@"INSERT INTO {0}
(deliverablepiececode, componentid)
SELECT deliverablepiececode, componentid
FROM {0}{1}",
                                               TableName.DELIVERABLE_PIECE,
                                               MIG_SUFFIX));
      
      Database.RemoveTable (TableName.DELIVERABLE_PIECE + MIG_SUFFIX);

    }
  }
}
