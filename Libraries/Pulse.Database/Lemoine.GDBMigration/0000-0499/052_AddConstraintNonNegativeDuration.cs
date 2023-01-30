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
  /// Migration 052: Add a constraint to check durations are not negative
  /// 
  /// It is done in the following tables:
  /// <item>OperationSlot</item>
  /// <item>MachineActivitySummary</item>
  /// <item>ReasonSummary</item>
  /// <item>EventLongPeriod</item>
  /// <item>EventLongPeriodConfig</item>
  /// <item>MachineModeDefaultReason</item>
  /// </summary>
  [Migration(52)]
  public class AddConstraintNonNegativeDuration: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddConstraintNonNegativeDuration).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      CorrectDatabase ();
      
      AddNonNegativeConstraint (TableName.OPERATION_SLOT,
                                "operationslotduration");
      AddNonNegativeConstraint (TableName.OPERATION_SLOT,
                                "operationslotruntime");
      AddNonNegativeConstraint (TableName.MACHINE_ACTIVITY_SUMMARY,
                                "machineactivitytime");
      AddNonNegativeConstraint (TableName.REASON_SUMMARY,
                                "reasonsummarytime");
      AddNonNegativeConstraint (TableName.EVENT_LONG_PERIOD,
                                "eventtriggerduration");
      AddNonNegativeConstraint (TableName.EVENT_LONG_PERIOD_CONFIG,
                                "eventtriggerduration");
      AddNonNegativeConstraint (TableName.MACHINE_MODE_DEFAULT_REASON,
                                "defaultreasonmaximumduration");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
    
    void CorrectDatabase ()
    {
      Database.ExecuteNonQuery (@"UPDATE operationslot
SET operationslotduration=0
WHERE operationslotduration<0");
      Database.ExecuteNonQuery (@"UPDATE operationslot
SET operationslotruntime=0
WHERE operationslotruntime<0");
      Database.ExecuteNonQuery (@"UPDATE machineactivitysummary
SET machineactivitytime=0
WHERE machineactivitytime<0");
      Database.ExecuteNonQuery (@"UPDATE reasonsummary
SET reasonsummarytime=0
WHERE reasonsummarytime<0");
    }
  }
}
