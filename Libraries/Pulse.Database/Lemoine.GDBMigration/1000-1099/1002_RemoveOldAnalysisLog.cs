// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Data;
using System.Diagnostics;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 1002: 
  /// </summary>
  [Migration (1002)]
  public class RemoveOldAnalysisLog : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (RemoveOldAnalysisLog).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE VIEW public.analysislog AS 
 SELECT globalmodificationlog.logid,
    globalmodificationlog.datetime,
    globalmodificationlog.level,
    globalmodificationlog.message,
    globalmodificationlog.module,
    globalmodificationlog.modificationid
   FROM globalmodificationlog
UNION
 SELECT machinemodificationlog.logid,
    machinemodificationlog.datetime,
    machinemodificationlog.level,
    machinemodificationlog.message,
    machinemodificationlog.module,
    machinemodificationlog.modificationid
   FROM machinemodificationlog
;");
      RemoveTable ("oldanalysislog");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
    }
  }
}
