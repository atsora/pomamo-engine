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
  /// Migration 204:
  /// </summary>
  [Migration(204)]
  public class AddUserShiftColumnsToMachineStateTemplate: MigrationExt
  {
    static readonly string MACHINE_STATE_TEMPLATE_NAME = TableName.MACHINE_STATE_TEMPLATE + "name";
    static readonly string MACHINE_STATE_TEMPLATE_TRANSLATION_KEY = TableName.MACHINE_STATE_TEMPLATE + "translationkey";
    static readonly string MACHINE_STATE_TEMPLATE_USER_REQUIRED = TableName.MACHINE_STATE_TEMPLATE + "userrequired";
    static readonly string MACHINE_STATE_TEMPLATE_ON_SITE = TableName.MACHINE_STATE_TEMPLATE + "onsite";
    static readonly string MACHINE_STATE_TEMPLATE_ID_SITE_ATTENDANCE_CHANGE = ColumnName.MACHINE_STATE_TEMPLATE_ID + "siteattendancechange";
    static readonly string MACHINE_STATE_TEMPLATE_SHIFT_REQUIRED = TableName.MACHINE_STATE_TEMPLATE + "shiftrequired";
    
    static readonly ILog log = LogManager.GetLogger(typeof (AddUserShiftColumnsToMachineStateTemplate).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      RemoveIndex (TableName.MACHINE_STATE_TEMPLATE,
                   MACHINE_STATE_TEMPLATE_NAME);
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0}
ALTER COLUMN {1} DROP NOT NULL",
                                               TableName.MACHINE_STATE_TEMPLATE,
                                               MACHINE_STATE_TEMPLATE_NAME));
      Database.AddColumn (TableName.MACHINE_STATE_TEMPLATE,
                          new Column (MACHINE_STATE_TEMPLATE_TRANSLATION_KEY, DbType.String, ColumnProperty.Unique));
      Database.AddColumn (TableName.MACHINE_STATE_TEMPLATE,
                          new Column (MACHINE_STATE_TEMPLATE_USER_REQUIRED, DbType.Boolean, ColumnProperty.NotNull, false));
      Database.AddColumn (TableName.MACHINE_STATE_TEMPLATE,
                          new Column (MACHINE_STATE_TEMPLATE_ON_SITE, DbType.Boolean));
      Database.AddColumn (TableName.MACHINE_STATE_TEMPLATE,
                          new Column (MACHINE_STATE_TEMPLATE_ID_SITE_ATTENDANCE_CHANGE, DbType.Int32));
      Database.AddColumn (TableName.MACHINE_STATE_TEMPLATE,
                          new Column (MACHINE_STATE_TEMPLATE_SHIFT_REQUIRED, DbType.Boolean, ColumnProperty.NotNull, false));
      AddConstraintNameTranslationKey (TableName.MACHINE_STATE_TEMPLATE,
                                       MACHINE_STATE_TEMPLATE_NAME,
                                       MACHINE_STATE_TEMPLATE_TRANSLATION_KEY);
      Database.GenerateForeignKey (TableName.MACHINE_STATE_TEMPLATE, MACHINE_STATE_TEMPLATE_ID_SITE_ATTENDANCE_CHANGE,
                                   TableName.MACHINE_STATE_TEMPLATE, ColumnName.MACHINE_STATE_TEMPLATE_ID,
                                   Migrator.Framework.ForeignKeyConstraint.SetNull);
      AddUniqueIndex (TableName.MACHINE_STATE_TEMPLATE,
                      MACHINE_STATE_TEMPLATE_NAME,
                      MACHINE_STATE_TEMPLATE_TRANSLATION_KEY);
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0}
ALTER COLUMN {1} DROP DEFAULT",
                                               TableName.MACHINE_STATE_TEMPLATE,
                                               MACHINE_STATE_TEMPLATE_USER_REQUIRED));
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0}
ALTER COLUMN {1} DROP DEFAULT",
                                               TableName.MACHINE_STATE_TEMPLATE,
                                               MACHINE_STATE_TEMPLATE_SHIFT_REQUIRED));
      SetSequence (TableName.MACHINE_STATE_TEMPLATE,
                   ColumnName.MACHINE_STATE_TEMPLATE_ID,
                   100); // Leave 100 IDs for the default values
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveIndex (TableName.MACHINE_STATE_TEMPLATE,
                   MACHINE_STATE_TEMPLATE_NAME,
                   MACHINE_STATE_TEMPLATE_TRANSLATION_KEY);
      Database.RemoveColumn (TableName.MACHINE_STATE_TEMPLATE,
                             MACHINE_STATE_TEMPLATE_SHIFT_REQUIRED);
      Database.RemoveColumn (TableName.MACHINE_STATE_TEMPLATE,
                             MACHINE_STATE_TEMPLATE_ID_SITE_ATTENDANCE_CHANGE);
      Database.RemoveColumn (TableName.MACHINE_STATE_TEMPLATE,
                             MACHINE_STATE_TEMPLATE_ON_SITE);
      Database.RemoveColumn (TableName.MACHINE_STATE_TEMPLATE,
                             MACHINE_STATE_TEMPLATE_USER_REQUIRED);
      Database.RemoveColumn (TableName.MACHINE_STATE_TEMPLATE,
                             MACHINE_STATE_TEMPLATE_TRANSLATION_KEY);
      AddUniqueIndex (TableName.MACHINE_STATE_TEMPLATE,
                      MACHINE_STATE_TEMPLATE_NAME);
      ResetSequence (TableName.MACHINE_STATE_TEMPLATE,
                     ColumnName.MACHINE_STATE_TEMPLATE_ID);
    }
  }
}
