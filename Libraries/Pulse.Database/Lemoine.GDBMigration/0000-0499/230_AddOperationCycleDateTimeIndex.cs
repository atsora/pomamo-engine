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
  /// Migration 230:
  /// </summary>
  [Migration(230)]
  public class AddOperationCycleDateTimeIndex: MigrationExt
  {
    static readonly string INDEX_NAME = TableName.OPERATION_CYCLE + "_datetime";
    
    static readonly ILog log = LogManager.GetLogger(typeof (AddOperationCycleDateTimeIndex).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"CREATE OR REPLACE FUNCTION firstorsecond(first timestamp without time zone, second timestamp without time zone)
  RETURNS timestamp without time zone AS
$BODY$
begin
  IF first IS NOT NULL THEN
    return first;
  ELSE
    return second;
  END IF;
end;
$BODY$
  LANGUAGE plpgsql IMMUTABLE
  COST 100;");
      
      AddNamedIndex (INDEX_NAME,
                     TableName.OPERATION_CYCLE,
                     "firstorsecond (operationcycleend, operationcyclebegin)",
                     ColumnName.OPERATION_CYCLE_ID);
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveNamedIndex (INDEX_NAME, TableName.OPERATION_CYCLE);
      
      Database.ExecuteNonQuery (@"DROP FUNCTION firstorsecond(timestamp without time zone, timestamp without time zone)");
    }
  }
}
