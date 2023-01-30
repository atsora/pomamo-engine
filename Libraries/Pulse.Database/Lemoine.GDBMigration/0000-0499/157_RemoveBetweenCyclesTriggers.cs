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
  /// Migration 157: Because it is too complex to debug and too risky, give up the idea of using triggers for between cycles
  /// </summary>
  [Migration(157)]
  public class RemoveBetweenCyclesTriggers: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (RemoveBetweenCyclesTriggers).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"BEGIN;
      
DROP TRIGGER IF EXISTS betweencycles_insert ON betweencycles CASCADE;
DROP TRIGGER IF EXISTS betweencycles_cycle_update ON betweencycles CASCADE;

DROP TRIGGER IF EXISTS operationslot_operation_update ON operationslot CASCADE;

DROP TRIGGER IF EXISTS operationcycle_operationslot_update ON operationcycle CASCADE;

DROP FUNCTION IF EXISTS betweencycles_offsetduration(integer, integer) CASCADE;
DROP FUNCTION IF EXISTS betweencycles_offsetduration(integer, integer, integer) CASCADE;
DROP FUNCTION IF EXISTS betweencycles_offsetduration(integer, timestamp without time zone, timestamp without time zone, integer, integer) CASCADE;

COMMIT;");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      // Do nothing
    }
  }
}
