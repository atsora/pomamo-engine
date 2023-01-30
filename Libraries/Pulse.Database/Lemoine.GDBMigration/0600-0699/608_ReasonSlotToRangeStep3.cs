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
  /// Migration 608:
  /// </summary>
  [Migration(608)]
  public class ReasonSlotToRangeStep3: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ReasonSlotToRangeStep3).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      RemoveReasonSlotColumn (TableName.REASON_SLOT + "begindatetime");
      RemoveReasonSlotColumn (TableName.REASON_SLOT + "enddatetime");
      RemoveReasonSlotColumn (TableName.REASON_SLOT + "beginday");
      RemoveReasonSlotColumn (TableName.REASON_SLOT + "endday");
      
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {0}{1} SET NOT NULL",
                                               TableName.REASON_SLOT,
                                               "datetimerange"));
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {0}{1} SET NOT NULL",
                                               TableName.REASON_SLOT,
                                               "dayrange"));
      AddConstraintRangeNotEmpty (TableName.REASON_SLOT, TableName.REASON_SLOT + "dayrange");
      AddConstraintRangeStrictlyPositiveDuration (TableName.REASON_SLOT, TableName.REASON_SLOT + "datetimerange");
      AddGistIndex (TableName.REASON_SLOT,
                    ColumnName.MACHINE_ID,
                    TableName.REASON_SLOT + "dayrange");
      
      CreateVirtualColumns ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      RemoveVirtualColumns ();
      
      RemoveIndex(TableName.REASON_SLOT, ColumnName.MACHINE_ID, TableName.REASON_SLOT + "dayrange");
      
      Database.AddColumn (TableName.REASON_SLOT,
                          new Column (TableName.REASON_SLOT + "begindatetime", DbType.DateTime, ColumnProperty.Null));
      Database.AddColumn (TableName.REASON_SLOT,
                          new Column (TableName.REASON_SLOT + "enddatetime", DbType.DateTime));
      Database.AddColumn (TableName.REASON_SLOT,
                          new Column (TableName.REASON_SLOT + "beginday", DbType.DateTime, ColumnProperty.Null));
      Database.AddColumn (TableName.REASON_SLOT,
                          new Column (TableName.REASON_SLOT + "endday", DbType.DateTime));
      
    }
    
    void CreateVirtualColumns ()
    {
      Database.ExecuteNonQuery (@"CREATE OR REPLACE FUNCTION public.reasonslotbegindatetime(reasonslot)
  RETURNS timestamp without time zone AS
$BODY$SELECT lower($1.reasonslotdatetimerange)$BODY$
  LANGUAGE sql IMMUTABLE COST 100;");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE FUNCTION public.reasonslotenddatetime(reasonslot)
  RETURNS timestamp without time zone AS
$BODY$SELECT upper($1.reasonslotdatetimerange)$BODY$
  LANGUAGE sql IMMUTABLE COST 100;");

      Database.ExecuteNonQuery (@"CREATE OR REPLACE FUNCTION public.reasonslotbeginday(reasonslot)
  RETURNS date AS
$BODY$SELECT lower($1.reasonslotdayrange)$BODY$
  LANGUAGE sql IMMUTABLE COST 100;");
      Database.ExecuteNonQuery (@"CREATE OR REPLACE FUNCTION public.reasonslotendday(reasonslot)
  RETURNS date AS
$BODY$SELECT upper($1.reasonslotdayrange)$BODY$
  LANGUAGE sql IMMUTABLE COST 100;");
    }
    
    void RemoveVirtualColumns ()
    {
      Database.ExecuteNonQuery (@"DROP FUNCTION public.reasonslotbegindatetime(reasonslot);");
      Database.ExecuteNonQuery (@"DROP FUNCTION public.reasonslotenddatetime(reasonslot);");
      Database.ExecuteNonQuery (@"DROP FUNCTION public.reasonslotbeginday(reasonslot);");
      Database.ExecuteNonQuery (@"DROP FUNCTION public.reasonslotendday(reasonslot);");
    }
    
    void RemoveReasonSlotColumn (string columnName)
    { // In cascade because of some views in reportv2
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} DROP COLUMN {1} CASCADE",
                                               TableName.REASON_SLOT, columnName));
    }
  }
}
