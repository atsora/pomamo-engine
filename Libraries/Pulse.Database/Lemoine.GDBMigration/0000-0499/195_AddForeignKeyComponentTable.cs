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
  /// Migration 195: Use to repair forgotten foreign key on table Component
  /// </summary>
  [Migration(195)]
  public class AddForeignKeyComponentTable: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddForeignKeyComponentTable).FullName);
    private const string FK_NAME = "fk_component_intermediateworkpiece";
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      // component.finalworkpieceid
      Database.AddForeignKey(FK_NAME,
                             TableName.COMPONENT, "finalworkpieceid",
                             TableName.INTERMEDIATE_WORK_PIECE, ColumnName.INTERMEDIATE_WORK_PIECE_ID, 
                             Migrator.Framework.ForeignKeyConstraint.SetNull);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveForeignKey(TableName.COMPONENT, FK_NAME);
    }
  }
}
