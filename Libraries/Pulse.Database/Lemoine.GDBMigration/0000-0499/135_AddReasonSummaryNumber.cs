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
  /// Migration 135: add a column reasonsummarynumber to the reasonsummary table
  /// </summary>
  [Migration(135)]
  public class AddReasonSummaryNumber: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddReasonSummaryNumber).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.AddColumn (TableName.REASON_SUMMARY,
                          new Column (TableName.REASON_SUMMARY + "number", DbType.Int32, ColumnProperty.NotNull, 0));
      Database.ExecuteNonQuery (@"UPDATE reasonsummary 
SET reasonsummarynumber=(SELECT COUNT(*) FROM reasonslot
  WHERE reasonslot.machineid=reasonsummary.machineid
    AND reasonsummaryday>=reasonslotbeginday
    AND reasonsummaryday<=reasonslotendday
    AND reasonslot.reasonid=reasonsummary.reasonid
    AND reasonslot.machineobservationstateid=reasonsummary.machineobservationstateid)
WHERE reasonsummarynumber=0;");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.RemoveColumn (TableName.REASON_SUMMARY,
                            TableName.REASON_SUMMARY + "number");
    }
  }
}
