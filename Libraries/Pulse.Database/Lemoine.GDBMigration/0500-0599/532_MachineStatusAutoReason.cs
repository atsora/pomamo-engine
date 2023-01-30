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
  /// Migration 532: add new columns for auto-reason in machine status
  /// </summary>
  [Migration (532)]
  public class AddMachineStatusAutoReason : MigrationExt
  {
    static readonly string MACHINE_STATUS_REASON_SCORE = "machinestatusreasonscore";
    static readonly string MACHINE_STATUS_REASON_SOURCE = "machinestatusreasonsource";
    static readonly string MACHINE_STATUS_AUTO_REASON_NUMBER = "machinestatusautoreasonnumber";
    static readonly string MACHINE_STATUS_DEFAULT_REASON = "machinestatusdefaultreason";
    static readonly string MACHINE_STATUS_OVERWRITE_REQUIRED = "machinestatusoverwriterequired";
    static readonly string MACHINE_STATUS_CONSOLIDATION_LIMIT = "machinestatusconsolidationlimit";
    static readonly string REASON_MACHINE_ASSOCIATION_END = "reasonmachineassociationend";

    static readonly ILog log = LogManager.GetLogger (typeof (AddMachineStatusAutoReason).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddConsolidationLimit ();
      AddReasonScore ();
      AddReasonSource ();
      AddAutoReasonNumber ();
      RemoveMachineStatusDefaultReason ();
      AddOverwriteRequired ();
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveOverwriteRequired ();
      RestoreDefaultReason ();
      RemoveReasonSource ();
      RemoveReasonScore ();
      RemoveAutoReasonNumber ();
      RemoveConsolidationLimit ();
    }

    void AddConsolidationLimit ()
    {
      Database.AddColumn (TableName.MACHINE_STATUS, new Column (MACHINE_STATUS_CONSOLIDATION_LIMIT, DbType.DateTime));
      Database.ExecuteNonQuery (@"
UPDATE machinestatus
SET machinestatusconsolidationlimit=reasonslotenddatetime
WHERE machinestatusdefaultreason=TRUE");
      Database.ExecuteNonQuery (@"
UPDATE machinestatus
SET machinestatusconsolidationlimit=reasonmachineassociationend
WHERE machinestatusdefaultreason=FALSE");
      Database.RemoveColumn (TableName.MACHINE_STATUS, REASON_MACHINE_ASSOCIATION_END);
      string constraintName = "machinestatus_consolidationlimit";
      Database.AddCheckConstraint (constraintName, TableName.MACHINE_STATUS,
        "machinestatusconsolidationlimit IS NULL OR (reasonslotenddatetime <= machinestatusconsolidationlimit)");
    }

    void RemoveConsolidationLimit ()
    {
      Database.RemoveConstraint (TableName.MACHINE_STATUS, "machinestatus_consolidationlimit");
      Database.AddColumn (TableName.MACHINE_STATUS, new Column (REASON_MACHINE_ASSOCIATION_END, DbType.DateTime));
      Database.ExecuteNonQuery (@"
UPDATE machinestatus
SET reasonmachineassociationend=machinestatusconsolidationlimit
WHERE machinestatusdefaultreason=FALSE
");
      Database.RemoveColumn (TableName.MACHINE_STATUS, MACHINE_STATUS_CONSOLIDATION_LIMIT);
    }

    void AddReasonScore ()
    {
      Database.AddColumn (TableName.MACHINE_STATUS, new Column (MACHINE_STATUS_REASON_SCORE, DbType.Double));
      Database.ExecuteNonQuery (@"
UPDATE machinestatus
SET machinestatusreasonscore=100
WHERE machinestatusdefaultreason=FALSE");
      Database.ExecuteNonQuery (@"
UPDATE machinestatus
SET machinestatusreasonscore=10
WHERE machinestatusdefaultreason=TRUE");
      SetNotNull (TableName.MACHINE_STATUS, MACHINE_STATUS_REASON_SCORE);
    }

    void RemoveReasonScore ()
    {
      Database.RemoveColumn (TableName.MACHINE_STATUS, MACHINE_STATUS_REASON_SCORE);
    }

    void AddReasonSource ()
    {
      Database.AddColumn (TableName.MACHINE_STATUS, new Column (MACHINE_STATUS_REASON_SOURCE, DbType.Int32));
      Database.ExecuteNonQuery (@"
UPDATE machinestatus
SET machinestatusreasonsource=4
WHERE machinestatusdefaultreason=FALSE");
      Database.ExecuteNonQuery (@"
UPDATE machinestatus
SET machinestatusreasonsource=1
WHERE machinestatusdefaultreason=TRUE");
      SetNotNull (TableName.MACHINE_STATUS, MACHINE_STATUS_REASON_SOURCE);
    }

    void RemoveReasonSource ()
    {
      Database.RemoveColumn (TableName.MACHINE_STATUS, MACHINE_STATUS_REASON_SOURCE);
    }

    void AddAutoReasonNumber ()
    {
      Database.AddColumn (TableName.MACHINE_STATUS, new Column (MACHINE_STATUS_AUTO_REASON_NUMBER, DbType.Int32));
      Database.ExecuteNonQuery (@"UPDATE machinestatus SET machinestatusautoreasonnumber=0;");
      SetNotNull (TableName.MACHINE_STATUS, MACHINE_STATUS_AUTO_REASON_NUMBER);
    }

    void RemoveAutoReasonNumber ()
    {
      Database.RemoveColumn (TableName.MACHINE_STATUS, MACHINE_STATUS_AUTO_REASON_NUMBER);
    }

    void RemoveMachineStatusDefaultReason ()
    {
      RemoveColumnCascade (TableName.MACHINE_STATUS, MACHINE_STATUS_DEFAULT_REASON);
      AddVirtualColumn (TableName.MACHINE_STATUS, MACHINE_STATUS_DEFAULT_REASON, "boolean", "SELECT $1.machinestatusreasonsource = 0");
    }

    void RestoreDefaultReason ()
    {
      DropVirtualColumn (TableName.MACHINE_STATUS, MACHINE_STATUS_DEFAULT_REASON);
      Database.AddColumn (TableName.MACHINE_STATUS, new Column (MACHINE_STATUS_DEFAULT_REASON, DbType.Boolean));
      Database.ExecuteNonQuery (@"
UPDATE machinestatus
SET machinestatusdefaultreason=TRUE
WHERE machinestatusreasonsource IN (1,3)
");
      Database.ExecuteNonQuery (@"
UPDATE machinestatus
SET machinestatusdefaultreason=FALSE
WHERE machinestatusreasonsource NOT IN (1,3)
");
      SetNotNull (TableName.MACHINE_STATUS, MACHINE_STATUS_DEFAULT_REASON);
    }

    void AddOverwriteRequired ()
    {
      Database.AddColumn (TableName.MACHINE_STATUS, new Column (MACHINE_STATUS_OVERWRITE_REQUIRED, DbType.Boolean));
      Database.ExecuteNonQuery (@"UPDATE machinestatus SET machinestatusoverwriterequired=FALSE;");
      SetNotNull (TableName.MACHINE_STATUS, MACHINE_STATUS_OVERWRITE_REQUIRED);
    }

    void RemoveOverwriteRequired ()
    {
      Database.RemoveColumn (TableName.MACHINE_STATUS, MACHINE_STATUS_OVERWRITE_REQUIRED);
    }
  }
}
