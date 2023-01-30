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
  /// Migration 612:
  /// </summary>
  [Migration(612)]
  public class OperationSlotToRangeStep3: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (OperationSlotToRangeStep3).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      RemoveOperationSlotColumn (TableName.OPERATION_SLOT + "begindatetime");
      RemoveOperationSlotColumn (TableName.OPERATION_SLOT + "enddatetime");
      RemoveOperationSlotColumn (TableName.OPERATION_SLOT + "beginday");
      RemoveOperationSlotColumn (TableName.OPERATION_SLOT + "endday");
      
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {0}{1} SET NOT NULL",
                                               TableName.OPERATION_SLOT,
                                               "datetimerange"));
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {0}{1} SET NOT NULL",
                                               TableName.OPERATION_SLOT,
                                               "dayrange"));
      AddConstraintRangeNotEmpty (TableName.OPERATION_SLOT, TableName.OPERATION_SLOT + "dayrange");
      AddConstraintRangeStrictlyPositiveDuration (TableName.OPERATION_SLOT, TableName.OPERATION_SLOT + "datetimerange");
      AddGistIndex (TableName.OPERATION_SLOT,
                    ColumnName.MACHINE_ID,
                    TableName.OPERATION_SLOT + "dayrange");
      
      CreateVirtualColumns ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveVirtualColumns ();

      RemoveIndex (TableName.OPERATION_SLOT, ColumnName.MACHINE_ID, TableName.OPERATION_SLOT + "dayrange");
      
      Database.AddColumn (TableName.OPERATION_SLOT,
                          new Column (TableName.OPERATION_SLOT + "begindatetime", DbType.DateTime, ColumnProperty.Null));
      Database.AddColumn (TableName.OPERATION_SLOT,
                          new Column (TableName.OPERATION_SLOT + "enddatetime", DbType.DateTime));
      Database.AddColumn (TableName.OPERATION_SLOT,
                          new Column (TableName.OPERATION_SLOT + "beginday", DbType.DateTime, ColumnProperty.Null));
      Database.AddColumn (TableName.OPERATION_SLOT,
                          new Column (TableName.OPERATION_SLOT + "endday", DbType.DateTime));
      
    }
    
    void CreateVirtualColumns ()
    {
      Database.ExecuteNonQuery (@"CREATE OR REPLACE FUNCTION public.operationslotbegindatetime(operationslot)
  RETURNS timestamp without time zone AS
$BODY$SELECT lower($1.operationslotdatetimerange)$BODY$
  LANGUAGE sql IMMUTABLE COST 100;");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE FUNCTION public.operationslotenddatetime(operationslot)
  RETURNS timestamp without time zone AS
$BODY$SELECT upper($1.operationslotdatetimerange)$BODY$
  LANGUAGE sql IMMUTABLE COST 100;");

      Database.ExecuteNonQuery (@"CREATE OR REPLACE FUNCTION public.operationslotbeginday(operationslot)
  RETURNS date AS
$BODY$SELECT lower($1.operationslotdayrange)$BODY$
  LANGUAGE sql IMMUTABLE COST 100;");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE FUNCTION public.operationslotendday(operationslot)
  RETURNS date AS
$BODY$SELECT upper($1.operationslotdayrange)$BODY$
  LANGUAGE sql IMMUTABLE COST 100;");
    }
    
    void RemoveVirtualColumns ()
    {
      Database.ExecuteNonQuery (@"DROP FUNCTION public.operationslotbegindatetime(operationslot);");
      Database.ExecuteNonQuery (@"DROP FUNCTION public.operationslotenddatetime(operationslot);");
      Database.ExecuteNonQuery (@"DROP FUNCTION public.operationslotbeginday(operationslot);");
      Database.ExecuteNonQuery (@"DROP FUNCTION public.operationslotendday(operationslot);");
    }
    
    void RemoveOperationSlotColumn (string columnName)
    { // In cascade because of some views in reportv2
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} DROP COLUMN {1} CASCADE",
                                               TableName.OPERATION_SLOT, columnName));
    }
  }
}
