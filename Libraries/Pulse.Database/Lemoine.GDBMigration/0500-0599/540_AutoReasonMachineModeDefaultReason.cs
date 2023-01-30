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
  /// Migration 540: add the columns for auto-reason in table machinemodedefaultreason
  /// </summary>
  [Migration (540)]
  public class AutoReasonMachineModeDefaultReason : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AutoReasonMachineModeDefaultReason).FullName);

    static readonly string DEFAULT_REASON_AUTO = "machinemodedefaultreasonauto";
    static readonly string DEFAULT_REASON_SCORE = "machinemodedefaultreasonscore";

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      if (!Database.ColumnExists (TableName.MACHINE_MODE_DEFAULT_REASON, DEFAULT_REASON_AUTO)) {
        AddMachineModeDefaultReasonAuto ();
      }
      if (!Database.ColumnExists (TableName.MACHINE_MODE_DEFAULT_REASON, DEFAULT_REASON_SCORE)) {
        AddMachineModeDefaultReasonScore ();
      }
    }


    void AddMachineModeDefaultReasonAuto ()
    {
      Database.AddColumn (TableName.MACHINE_MODE_DEFAULT_REASON, new Column (DEFAULT_REASON_AUTO, DbType.Boolean));
      Database.ExecuteNonQuery (@"
UPDATE machinemodedefaultreason
SET machinemodedefaultreasonauto=FALSE
");
      Database.ExecuteNonQuery (@"
UPDATE machinemodedefaultreason
SET machinemodedefaultreasonauto=TRUE
WHERE defaultreasonmaximumduration IS NULL
  AND defaultreasonoverwriterequired=FALSE");
      SetNotNull (TableName.MACHINE_MODE_DEFAULT_REASON, DEFAULT_REASON_AUTO);
    }

    void AddMachineModeDefaultReasonScore ()
    {
      Database.AddColumn (TableName.MACHINE_MODE_DEFAULT_REASON, new Column (DEFAULT_REASON_SCORE, DbType.Double));
      Database.ExecuteNonQuery (@"
UPDATE machinemodedefaultreason
SET machinemodedefaultreasonscore=90
WHERE machinemodedefaultreasonauto=TRUE
");
      Database.ExecuteNonQuery (@"
UPDATE machinemodedefaultreason
SET machinemodedefaultreasonscore=10
WHERE machinemodedefaultreasonauto=FALSE
");
      SetNotNull (TableName.MACHINE_MODE_DEFAULT_REASON, DEFAULT_REASON_SCORE);
    }


    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveMachineModeDefaultReasonScore ();
      RemoveMachineModeDefaultReasonAuto ();
    }

    void RemoveMachineModeDefaultReasonScore ()
    {
      Database.RemoveColumn (TableName.MACHINE_MODE_DEFAULT_REASON, DEFAULT_REASON_SCORE);
    }

    void RemoveMachineModeDefaultReasonAuto ()
    {
      Database.RemoveColumn (TableName.MACHINE_MODE_DEFAULT_REASON, DEFAULT_REASON_AUTO);
    }
  }
}
