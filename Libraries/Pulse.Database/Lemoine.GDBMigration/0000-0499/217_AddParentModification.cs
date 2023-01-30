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
  /// Migration 217: allow to associate a modification to a parent modification 
  /// to create differred modifications from a modification
  /// </summary>
  [Migration(217)]
  public class AddParentModification: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddParentModification).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.MODIFICATION,
                          new Column ("parentmodificationid", DbType.Int32));
      Database.GenerateForeignKey (TableName.MODIFICATION, "parentmodificationid",
                                   TableName.MODIFICATION, ColumnName.MODIFICATION_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.MODIFICATION,
                             "parentmodificationid");
    }
  }
}
