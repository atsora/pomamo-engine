// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Data;

using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 072: Add the new Fact table
  /// </summary>
  [Migration(72)]
  public class AddNewFactTable: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddNewFactTable).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      CreateFactTable ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveFactTable ();
    }
    
    void CreateFactTable ()
    {
      Database.AddTable (TableName.FACT,
                         new Column (ColumnName.FACT_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column ("factversion", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ColumnName.MACHINE_ID, DbType.Int32, ColumnProperty.NotNull),
                         new Column ("factbegindatetime", DbType.DateTime, ColumnProperty.NotNull),
                         new Column ("factenddatetime", DbType.DateTime, ColumnProperty.NotNull),
                         new Column (ColumnName.MACHINE_MODE_ID, DbType.Int32, ColumnProperty.NotNull));
      Database.GenerateForeignKey (TableName.FACT, ColumnName.MACHINE_ID,
                                   TableName.MONITORED_MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.FACT, ColumnName.MACHINE_MODE_ID,
                                   TableName.MACHINE_MODE, ColumnName.MACHINE_MODE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddUniqueConstraint (TableName.FACT, ColumnName.MACHINE_ID, "factbegindatetime");
      Database.AddCheckConstraint ("fact_factbegindatetime_factenddatetime", TableName.FACT,
                                   @"factbegindatetime < factenddatetime");
      AddIndex (TableName.FACT,
                ColumnName.MACHINE_ID);
      AddIndex (TableName.FACT,
                ColumnName.MACHINE_ID,
                "factbegindatetime");
      AddIndex (TableName.FACT,
                ColumnName.MACHINE_ID,
                "factenddatetime");
    }
    
    void RemoveFactTable ()
    {
      Database.RemoveTable (TableName.FACT);
    }
  }
}
