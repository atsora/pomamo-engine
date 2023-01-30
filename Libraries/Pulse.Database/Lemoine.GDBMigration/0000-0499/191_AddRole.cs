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
  /// Migration 191: Add role
  /// 
  /// (skip the user/role association for the moment)
  /// </summary>
  [Migration(191)]
  public class AddRole: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddRole).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddRoleTable ();
      AddMachineStateTemplateRight ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveTable (TableName.MACHINE_STATE_TEMPLATE_RIGHT);
      Database.RemoveTable (TableName.ROLE);
      RemoveSequence (SequenceName.RIGHT_ID);
    }
    
    void AddRoleTable ()
    {
      Database.AddTable (TableName.ROLE,
                         new Column (ColumnName.ROLE_ID, DbType.Int32, ColumnProperty.PrimaryKeyWithIdentity),
                         new Column (TableName.ROLE + "version", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (TableName.ROLE + "name", DbType.String, ColumnProperty.Unique),
                         new Column (TableName.ROLE + "translationkey", DbType.String, ColumnProperty.Unique));
      MakeColumnCaseInsensitive (TableName.ROLE,
                                 TableName.ROLE + "name");
      AddConstraintNameTranslationKey (TableName.ROLE,
                                       TableName.ROLE + "name",
                                       TableName.ROLE + "translationkey");
      AddUniqueIndex (TableName.ROLE,
                      TableName.ROLE + "name",
                      TableName.ROLE + "translationkey");
    }
    
    void AddMachineStateTemplateRight ()
    {
      Database.AddTable (TableName.MACHINE_STATE_TEMPLATE_RIGHT,
                         new Column (ColumnName.RIGHT_ID, DbType.Int32, ColumnProperty.PrimaryKey),
                         new Column ("rightversion", DbType.Int32, ColumnProperty.NotNull, 1),
                         new Column (ColumnName.ROLE_ID, DbType.Int32, ColumnProperty.Null),
                         new Column (ColumnName.RIGHT_ACCESS_PRIVILEGE, DbType.Int32, ColumnProperty.NotNull),
                         new Column (ColumnName.MACHINE_STATE_TEMPLATE_ID, DbType.Int32, ColumnProperty.Null));
      AddSequence (SequenceName.RIGHT_ID);
      SetSequence (TableName.MACHINE_STATE_TEMPLATE_RIGHT,
                   ColumnName.RIGHT_ID,
                   SequenceName.RIGHT_ID);
      Database.GenerateForeignKey (TableName.MACHINE_STATE_TEMPLATE_RIGHT, ColumnName.ROLE_ID,
                                   TableName.ROLE, ColumnName.ROLE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      Database.GenerateForeignKey (TableName.MACHINE_STATE_TEMPLATE_RIGHT, ColumnName.MACHINE_STATE_TEMPLATE_ID,
                                   TableName.MACHINE_STATE_TEMPLATE, ColumnName.MACHINE_STATE_TEMPLATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.Cascade);
      AddIndex (TableName.MACHINE_STATE_TEMPLATE_RIGHT,
                ColumnName.ROLE_ID);
      AddIndex (TableName.MACHINE_STATE_TEMPLATE_RIGHT,
                ColumnName.MACHINE_STATE_TEMPLATE_ID);
    }
  }
}
