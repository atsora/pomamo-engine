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
  /// Migration 225:
  /// </summary>
  [Migration(225)]
  public class AddEmailConfigValidityPeriods: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddEmailConfigValidityPeriods).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.EMAIL_CONFIG,
                          new Column (TableName.EMAIL_CONFIG + "weekdays", DbType.Int32, ColumnProperty.NotNull, Int32.MaxValue.ToString ()));
      Database.AddColumn (TableName.EMAIL_CONFIG,
                          new Column (TableName.EMAIL_CONFIG + "timeperiod", DbType.String, 17, ColumnProperty.Null));
      Database.AddColumn (TableName.EMAIL_CONFIG,
                          new Column (TableName.EMAIL_CONFIG + "begindatetime", DbType.DateTime, ColumnProperty.Null));
      Database.AddColumn (TableName.EMAIL_CONFIG,
                          new Column (TableName.EMAIL_CONFIG + "enddatetime", DbType.DateTime, ColumnProperty.Null));
      Database.AddColumn (TableName.EMAIL_CONFIG,
                          new Column (TableName.EMAIL_CONFIG + "autopurge", DbType.Boolean, ColumnProperty.NotNull, "FALSE"));
      
      Database.AddColumn (TableName.EMAIL_CONFIG,
                          new Column (TableName.EMAIL_CONFIG + "timestamp", DbType.DateTime, ColumnProperty.NotNull, "(CURRENT_TIMESTAMP AT TIME ZONE 'GMT')"));
      AddTimeStampTrigger (TableName.EMAIL_CONFIG);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveTimeStampTrigger (TableName.EMAIL_CONFIG);
      Database.RemoveColumn (TableName.EMAIL_CONFIG,
                             TableName.MACHINE_STATE_TEMPLATE_ITEM + "timestamp");
      Database.RemoveColumn (TableName.EMAIL_CONFIG,
                             TableName.EMAIL_CONFIG + "timestamp");
      
      Database.RemoveColumn (TableName.EMAIL_CONFIG,
                             TableName.EMAIL_CONFIG + "autopurge");
      Database.RemoveColumn (TableName.EMAIL_CONFIG,
                             TableName.EMAIL_CONFIG + "enddatetime");
      Database.RemoveColumn (TableName.EMAIL_CONFIG,
                             TableName.EMAIL_CONFIG + "begindatetime");
      Database.RemoveColumn (TableName.EMAIL_CONFIG,
                             TableName.EMAIL_CONFIG + "timeperiod");
      Database.RemoveColumn (TableName.EMAIL_CONFIG,
                             TableName.EMAIL_CONFIG + "weekdays");
    }
  }
}
