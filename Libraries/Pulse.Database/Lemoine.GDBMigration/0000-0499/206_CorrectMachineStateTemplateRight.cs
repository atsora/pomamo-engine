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
  /// Migration 206: correct the table machinestatetemplateright:
  /// <item>rename machinestatetemplaterightversion to rightversion</item>
  /// <item>make roleid nullable</item>
  /// </summary>
  [Migration(206)]
  public class CorrectMachineStateTemplateRight: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (CorrectMachineStateTemplateRight).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0}
ALTER COLUMN {1} DROP NOT NULL",
                                               TableName.MACHINE_STATE_TEMPLATE_RIGHT,
                                               ColumnName.ROLE_ID));
      if (Database.ColumnExists (TableName.MACHINE_STATE_TEMPLATE_RIGHT,
                                 TableName.MACHINE_STATE_TEMPLATE_RIGHT + "version")) {
        Database.AddColumn (TableName.MACHINE_STATE_TEMPLATE_RIGHT,
                            new Column ("rightversion", DbType.Int32, ColumnProperty.NotNull, 1));
        Database.RemoveColumn (TableName.MACHINE_STATE_TEMPLATE_RIGHT,
                               TableName.MACHINE_STATE_TEMPLATE_RIGHT + "version");
      }
      AddUniqueConstraint (TableName.MACHINE_STATE_TEMPLATE_RIGHT,
                           ColumnName.MACHINE_STATE_TEMPLATE_ID,
                           ColumnName.ROLE_ID,
                           "rightaccessprivilege");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveUniqueConstraint (TableName.MACHINE_STATE_TEMPLATE_RIGHT,
                              ColumnName.MACHINE_STATE_TEMPLATE_ID,
                              ColumnName.ROLE_ID,
                              "rightaccessprivilege");
    }
  }
}
