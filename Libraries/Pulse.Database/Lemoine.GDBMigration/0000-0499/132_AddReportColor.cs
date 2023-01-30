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
  /// Migration 132: add columns in database to store some colors that are specific to the reports
  /// </summary>
  [Migration(132)]
  public class AddReportColor: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddReportColor).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddReasonReportColor ();
      AddReasonGroupReportColor ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveReasonGroupReportColor ();
      RemoveReasonReportColor ();
    }
    
    void AddReasonReportColor ()
    {
      Database.AddColumn (TableName.REASON,
                          new Column (TableName.REASON + "reportcolor", DbType.String, 7, ColumnProperty.NotNull, "'#FFFF00'"));
      AddConstraintColor (TableName.REASON,
                          TableName.REASON + "reportcolor");
      Database.ExecuteNonQuery (@"UPDATE reason
SET reasonreportcolor=reasoncolor");
    }
    
    void RemoveReasonReportColor ()
    {
      Database.ExecuteNonQuery (@"ALTER TABLE reason 
DROP COLUMN IF EXISTS reasonreportcolor CASCADE");
    }
    
    void AddReasonGroupReportColor ()
    {
      Database.AddColumn (TableName.REASON_GROUP,
                          new Column (TableName.REASON_GROUP + "reportcolor", DbType.String, 7, ColumnProperty.NotNull, "'#FFFF00'"));
      AddConstraintColor (TableName.REASON_GROUP,
                          TableName.REASON_GROUP + "reportcolor");
      Database.ExecuteNonQuery (@"UPDATE reasongroup
SET reasongroupreportcolor=reasongroupcolor");
    }
    
    void RemoveReasonGroupReportColor ()
    {
      Database.ExecuteNonQuery (@"ALTER TABLE reasongroup 
DROP COLUMN IF EXISTS reasongroupreportcolor CASCADE");
    }
  }
}
