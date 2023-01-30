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
  /// Migration 150: Change the secondary key of componentintermediateworkpiece to include the order and the code
  /// </summary>
  [Migration(150)]
  public class ChangeComponentIntermediateWorkPieceSecondaryKey: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ChangeComponentIntermediateWorkPieceSecondaryKey).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      RemoveIndex (TableName.COMPONENT_INTERMEDIATE_WORK_PIECE,
                   ColumnName.COMPONENT_ID);
      Database.RemoveConstraint (TableName.COMPONENT_INTERMEDIATE_WORK_PIECE,
                                 "ComponentIntermediateWorkPiece_SecondaryKey");
      AddNamedUniqueConstraint ("ComponentIntermediateWorkPiece_SecondaryKey",
                             TableName.COMPONENT_INTERMEDIATE_WORK_PIECE,
                             new string [] { ColumnName.COMPONENT_ID,
                               ColumnName.INTERMEDIATE_WORK_PIECE_ID,
                               "intermediateworkpieceorderforcomponent",
                               "intermediateworkpiececodeforcomponent"});
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveConstraint (TableName.COMPONENT_INTERMEDIATE_WORK_PIECE,
                                 "ComponentIntermediateWorkPiece_SecondaryKey");
      AddNamedUniqueConstraint ("ComponentIntermediateWorkPiece_SecondaryKey",
                             TableName.COMPONENT_INTERMEDIATE_WORK_PIECE,
                             new string [] { ColumnName.COMPONENT_ID,
                               ColumnName.INTERMEDIATE_WORK_PIECE_ID});
      AddIndex (TableName.COMPONENT_INTERMEDIATE_WORK_PIECE,
                ColumnName.COMPONENT_ID);
    }
  }
}
