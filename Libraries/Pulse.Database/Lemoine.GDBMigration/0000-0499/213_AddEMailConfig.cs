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
  /// Migration 213: Add a table EmailConfig where are stored which recipient to set in an e-mail
  /// </summary>
  [Migration(213)]
  public class AddEmailConfig: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddEmailConfig).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddTable (TableName.EMAIL_CONFIG,
                         new Column (TableName.EMAIL_CONFIG + "id", DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.EMAIL_CONFIG + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         // Filter part
                         new Column (TableName.EMAIL_CONFIG + "name", DbType.String, ColumnProperty.Unique),
                         new Column (TableName.EMAIL_CONFIG + "datatype", DbType.String),
                         new Column (TableName.EMAIL_CONFIG + "freefilter", DbType.String),
                         new Column ("maxlevelpriority", DbType.Int32, ColumnProperty.NotNull, 1000), // 1000: all
                         new Column (ColumnName.EVENT_LEVEL_ID, DbType.Int32),
                         new Column (ColumnName.MACHINE_ID, DbType.Int32),
                         new Column (ColumnName.MACHINE_FILTER_ID, DbType.Int32),
                         // Recipients part
                         new Column (TableName.EMAIL_CONFIG + "to", DbType.String, 2048),
                         new Column (TableName.EMAIL_CONFIG + "cc", DbType.String, 2048),
                         new Column (TableName.EMAIL_CONFIG + "bcc", DbType.String, 2048));
      MakeColumnCaseInsensitive (TableName.EMAIL_CONFIG,
                                 TableName.EMAIL_CONFIG + "name");
      Database.GenerateForeignKey (TableName.EMAIL_CONFIG, ColumnName.EVENT_LEVEL_ID,
                                   TableName.EVENT_LEVEL, ColumnName.EVENT_LEVEL_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.EMAIL_CONFIG, ColumnName.MACHINE_ID,
                                   TableName.MACHINE, ColumnName.MACHINE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.EMAIL_CONFIG, ColumnName.MACHINE_FILTER_ID,
                                   TableName.MACHINE_FILTER, ColumnName.MACHINE_FILTER_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveTable (TableName.EMAIL_CONFIG);
    }
  }
}
