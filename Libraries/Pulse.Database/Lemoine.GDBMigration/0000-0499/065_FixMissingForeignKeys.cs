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
  /// Migration 065: Fix the missing foreign keys in the following tables:
  /// <item>workorderproject</item>
  /// <item>componentintermediateworkpiece</item>
  /// </summary>
  [Migration(65)]
  public class FixMissingForeignKeys: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (FixMissingForeignKeys).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      FixWorkOrderProject ();
      FixComponentIntermediateWorkPiece ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
    
    void FixWorkOrderProject ()
    {
      if (!Database.ConstraintExists (TableName.WORK_ORDER_PROJECT,
                                      string.Format ("fk_{0}_{1}",
                                                     TableName.WORK_ORDER_PROJECT,
                                                     TableName.WORK_ORDER))) {
        Database.ExecuteNonQuery (@"DELETE FROM workorderproject 
WHERE NOT EXISTS (SELECT 1 FROM workorder WHERE workorder.workorderid = workorderproject.workorderid)");
        Database.GenerateForeignKey (TableName.WORK_ORDER_PROJECT, ColumnName.WORK_ORDER_ID,
                                     TableName.WORK_ORDER, ColumnName.WORK_ORDER_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
      }
      if (!Database.ConstraintExists (TableName.WORK_ORDER_PROJECT,
                                      string.Format ("fk_{0}_{1}",
                                                     TableName.WORK_ORDER_PROJECT,
                                                     TableName.PROJECT))) {
        Database.ExecuteNonQuery (@"DELETE FROM workorderproject 
WHERE NOT EXISTS (SELECT 1 FROM project WHERE project.projectid = workorderproject.projectid)");
        Database.GenerateForeignKey (TableName.WORK_ORDER_PROJECT, ColumnName.PROJECT_ID,
                                     TableName.PROJECT, ColumnName.PROJECT_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
      }
    }
    
    void FixComponentIntermediateWorkPiece ()
    {
      if (!Database.ConstraintExists (TableName.COMPONENT_INTERMEDIATE_WORK_PIECE,
                                      string.Format ("fk_{0}_{1}",
                                                     TableName.COMPONENT_INTERMEDIATE_WORK_PIECE,
                                                     TableName.COMPONENT))) {
        Database.ExecuteNonQuery (@"DELETE FROM componentintermediateworkpiece 
WHERE NOT EXISTS (SELECT 1 FROM component WHERE component.componentid = componentintermediateworkpiece.componentid)");
        Database.GenerateForeignKey (TableName.COMPONENT_INTERMEDIATE_WORK_PIECE, ColumnName.COMPONENT_ID,
                                     TableName.COMPONENT, ColumnName.COMPONENT_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
      }
      if (!Database.ConstraintExists (TableName.COMPONENT_INTERMEDIATE_WORK_PIECE,
                                      string.Format ("fk_{0}_{1}",
                                                     TableName.COMPONENT_INTERMEDIATE_WORK_PIECE,
                                                     TableName.INTERMEDIATE_WORK_PIECE))) {
        Database.ExecuteNonQuery (@"DELETE FROM componentintermediateworkpiece 
WHERE NOT EXISTS (SELECT 1 FROM intermediateworkpiece WHERE intermediateworkpiece.intermediateworkpieceid = componentintermediateworkpiece.intermediateworkpieceid)");
        Database.GenerateForeignKey (TableName.COMPONENT_INTERMEDIATE_WORK_PIECE, ColumnName.INTERMEDIATE_WORK_PIECE_ID,
                                     TableName.INTERMEDIATE_WORK_PIECE, ColumnName.INTERMEDIATE_WORK_PIECE_ID,
                                     Migrator.Framework.ForeignKeyConstraint.Cascade);
      }
    }
  }
}
