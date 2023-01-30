// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 603:
  /// </summary>
  [Migration(603)]
  public class AddOperationCycleDateTime: MigrationExt
  {
    static readonly string INDEX_NAME = TableName.OPERATION_CYCLE + "_datetime";

    static readonly ILog log = LogManager.GetLogger(typeof (AddOperationCycleDateTime).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery (@"CREATE OR REPLACE FUNCTION public.operationcycledatetime(operationcyclebegin timestamp without time zone, operationcycleend timestamp without time zone, operationcyclestatus integer)
  RETURNS timestamp without time zone AS
$BODY$
  SELECT CASE WHEN operationcyclestatus <> 2 AND operationcycleend IS NOT NULL THEN operationcycleend ELSE operationcyclebegin END;
$BODY$
  LANGUAGE SQL IMMUTABLE
  COST 100;");
      
      AddVirtualColumn (TableName.OPERATION_CYCLE, TableName.OPERATION_CYCLE + "datetime",
                        "timestamp without time zone", @"
SELECT public.operationcycledatetime ($1.operationcyclebegin, $1.operationcycleend, $1.operationcyclestatus)
");
      
      RemoveIndex (TableName.OPERATION_CYCLE, "datetime");
      
      AddNamedIndex (INDEX_NAME,
                     TableName.OPERATION_CYCLE,
                     ColumnName.MACHINE_ID,
                     "operationcycledatetime(operationcyclebegin, operationcycleend, operationcyclestatus)");
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveNamedIndex (INDEX_NAME, TableName.OPERATION_CYCLE);

      RemoveIndex (TableName.OPERATION_CYCLE, "datetime");

      AddNamedIndex (INDEX_NAME,
                     TableName.OPERATION_CYCLE,
                     "firstorsecond (operationcycleend, operationcyclebegin)",
                     ColumnName.OPERATION_CYCLE_ID);

      DropVirtualColumn (TableName.OPERATION_CYCLE, TableName.OPERATION_CYCLE + "datetime");
      Database.ExecuteNonQuery (@"
DROP FUNCTION IF EXISTS public.operationcycledatetime(timestamp without time zone, timestamp without time zone, integer);
");
    }
  }
}
