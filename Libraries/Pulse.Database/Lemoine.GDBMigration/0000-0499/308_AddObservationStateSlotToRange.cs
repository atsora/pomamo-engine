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
  /// Migration 308: convert observationstateslot to tsrange
  /// </summary>
  [Migration(308)]
  public class AddObservationStateSlotToRange: MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AddObservationStateSlotToRange).FullName);
    
    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      RemoveIndex(TableName.OBSERVATION_STATE_SLOT,
                  ColumnName.MACHINE_ID,
                  TableName.OBSERVATION_STATE_SLOT + "endday",
                  TableName.OBSERVATION_STATE_SLOT + "beginday");
      RemoveIndex(TableName.OBSERVATION_STATE_SLOT,
                  ColumnName.MACHINE_ID,
                  TableName.OBSERVATION_STATE_SLOT + "enddatetime",
                  TableName.OBSERVATION_STATE_SLOT + "begindatetime");
      RemoveUniqueConstraint(TableName.OBSERVATION_STATE_SLOT,
                             ColumnName.MACHINE_ID,
                             TableName.OBSERVATION_STATE_SLOT + "begindatetime");
      RemoveConstraint (TableName.OBSERVATION_STATE_SLOT,
                        BuildName (TableName.OBSERVATION_STATE_SLOT, "idx",
                                   ColumnName.MACHINE_ID,
                                   TableName.OBSERVATION_STATE_SLOT + "begindatetime"));
      RemoveIndex(TableName.OBSERVATION_STATE_SLOT, "nomos");

      Database.AddColumn (TableName.OBSERVATION_STATE_SLOT,
                          new Column (TableName.OBSERVATION_STATE_SLOT + "datetimerange", DbType.Int32, ColumnProperty.Null));
      MakeColumnTsRange (TableName.OBSERVATION_STATE_SLOT, TableName.OBSERVATION_STATE_SLOT + "datetimerange");
      Database.AddColumn (TableName.OBSERVATION_STATE_SLOT,
                          new Column (TableName.OBSERVATION_STATE_SLOT + "dayrange", DbType.Int32, ColumnProperty.Null));
      MakeColumnTsRange (TableName.OBSERVATION_STATE_SLOT, TableName.OBSERVATION_STATE_SLOT + "dayrange");

      Database.ExecuteNonQuery (string.Format (@"UPDATE {0} SET {0}datetimerange=tsrange({0}begindatetime,{0}enddatetime)",
                                               TableName.OBSERVATION_STATE_SLOT));
      Database.ExecuteNonQuery (string.Format (@"UPDATE {0} SET {0}datetimerange=tsrange(NULL,upper({0}datetimerange))
WHERE lower({0}datetimerange)='1970-01-01 00:00:00'",
                                               TableName.OBSERVATION_STATE_SLOT));
      Database.RemoveColumn (TableName.OBSERVATION_STATE_SLOT, TableName.OBSERVATION_STATE_SLOT + "begindatetime");
      Database.RemoveColumn (TableName.OBSERVATION_STATE_SLOT, TableName.OBSERVATION_STATE_SLOT + "enddatetime");
      
      Database.ExecuteNonQuery (string.Format (@"UPDATE {0} SET {0}dayrange=tsrange({0}beginday,{0}endday,'[]')",
                                               TableName.OBSERVATION_STATE_SLOT));
      Database.ExecuteNonQuery (string.Format (@"UPDATE {0} SET {0}dayrange=tsrange(NULL,upper({0}dayrange),'(]')
WHERE lower({0}dayrange)='1970-01-01 00:00:00'",
                                               TableName.OBSERVATION_STATE_SLOT));
      Database.RemoveColumn (TableName.OBSERVATION_STATE_SLOT, TableName.OBSERVATION_STATE_SLOT + "beginday");
      Database.RemoveColumn (TableName.OBSERVATION_STATE_SLOT, TableName.OBSERVATION_STATE_SLOT + "endday");
      
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {0}{1} SET NOT NULL",
                                               TableName.OBSERVATION_STATE_SLOT,
                                               "datetimerange"));
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {0}{1} SET NOT NULL",
                                               TableName.OBSERVATION_STATE_SLOT,
                                               "dayrange"));
      // TODO: Replace it by AddNoOverlapConstraint once pg_fkpart and dispatch_index supports it
      /*
      AddNoOverlapConstraint (TableName.OBSERVATION_STATE_SLOT,
                              ColumnName.MACHINE_ID,
                              TableName.OBSERVATION_STATE_SLOT + "datetimerange");
       */
      AddGistIndex (TableName.OBSERVATION_STATE_SLOT,
                    ColumnName.MACHINE_ID,
                    TableName.OBSERVATION_STATE_SLOT + "datetimerange");
      AddGistIndex (TableName.OBSERVATION_STATE_SLOT,
                    ColumnName.MACHINE_ID,
                    TableName.OBSERVATION_STATE_SLOT + "dayrange");
      AddNamedGistIndexCondition (TableName.OBSERVATION_STATE_SLOT + "_nomosrange",
                                  TableName.OBSERVATION_STATE_SLOT,
                                  "machineobservationstateid IS NULL",
                                  ColumnName.MACHINE_ID,
                                  TableName.OBSERVATION_STATE_SLOT + "datetimerange");
      
      CreateView ();
    }
    
    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery (@"DROP VIEW IF EXISTS observationstatebeginendslot");
      Database.ExecuteNonQuery (@"DROP VIEW IF EXISTS observationstateslotbor");
      
      // Once the nooverlap constraint is set when pg_fkpart supports it, remove it
      /*
          RemoveNoOverlapConstraint (TableName.OBSERVATION_STATE_SLOT, ColumnName.MACHINE_ID,
                                     TableName.OBSERVATION_STATE_SLOT + "datetimerange");
    }
       */
      
      RemoveIndex(TableName.OBSERVATION_STATE_SLOT,
                  ColumnName.MACHINE_ID,
                  TableName.OBSERVATION_STATE_SLOT + "datetimerange");
      RemoveIndex(TableName.OBSERVATION_STATE_SLOT,
                  ColumnName.MACHINE_ID,
                  TableName.OBSERVATION_STATE_SLOT + "dayrange");
      RemoveNamedIndex (TableName.OBSERVATION_STATE_SLOT + "_nomosrange",
        TableName.OBSERVATION_STATE_SLOT);
      
      Database.AddColumn (TableName.OBSERVATION_STATE_SLOT,
                          new Column (TableName.OBSERVATION_STATE_SLOT + "begindatetime", DbType.DateTime, ColumnProperty.Null));
      Database.AddColumn (TableName.OBSERVATION_STATE_SLOT,
                          new Column (TableName.OBSERVATION_STATE_SLOT + "enddatetime", DbType.DateTime));
      Database.AddColumn (TableName.OBSERVATION_STATE_SLOT,
                          new Column (TableName.OBSERVATION_STATE_SLOT + "beginday", DbType.DateTime, ColumnProperty.Null));
      Database.AddColumn (TableName.OBSERVATION_STATE_SLOT,
                          new Column (TableName.OBSERVATION_STATE_SLOT + "endday", DbType.DateTime));
      
      Database.ExecuteNonQuery (string.Format (@"UPDATE {0} SET {0}begindatetime=lower({0}datetimerange)
WHERE NOT lower_inf({0}datetimerange)",
                                               TableName.OBSERVATION_STATE_SLOT));
      Database.ExecuteNonQuery (string.Format (@"UPDATE {0} SET {0}begindatetime='1970-01-01 00:00:00'
WHERE lower_inf({0}datetimerange)",
                                               TableName.OBSERVATION_STATE_SLOT));
      Database.ExecuteNonQuery (string.Format (@"UPDATE {0} SET {0}enddatetime=upper({0}datetimerange)
WHERE NOT upper_inf({0}datetimerange)",
                                               TableName.OBSERVATION_STATE_SLOT));
      Database.RemoveColumn (TableName.OBSERVATION_STATE_SLOT, TableName.OBSERVATION_STATE_SLOT + "datetimerange");
      
      Database.ExecuteNonQuery (string.Format (@"UPDATE {0} SET {0}beginday=lower({0}dayrange)
WHERE NOT lower_inf({0}dayrange)",
                                               TableName.OBSERVATION_STATE_SLOT));
      Database.ExecuteNonQuery (string.Format (@"UPDATE {0} SET {0}beginday='1970-01-01 00:00:00'
WHERE lower_inf({0}dayrange)",
                                               TableName.OBSERVATION_STATE_SLOT));
      Database.ExecuteNonQuery (string.Format (@"UPDATE {0} SET {0}endday=upper({0}dayrange)
WHERE NOT upper_inf({0}dayrange)",
                                               TableName.OBSERVATION_STATE_SLOT));
      Database.RemoveColumn (TableName.OBSERVATION_STATE_SLOT, TableName.OBSERVATION_STATE_SLOT + "dayrange");
      
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {0}{1} SET NOT NULL",
                                               TableName.OBSERVATION_STATE_SLOT,
                                               "begindatetime"));
      Database.ExecuteNonQuery (string.Format (@"ALTER TABLE {0} ALTER COLUMN {0}{1} SET NOT NULL",
                                               TableName.OBSERVATION_STATE_SLOT,
                                               "beginday"));
      
      AddUniqueConstraint (TableName.OBSERVATION_STATE_SLOT,
                           ColumnName.MACHINE_ID,
                           TableName.OBSERVATION_STATE_SLOT + "begindatetime");
      AddIndex (TableName.OBSERVATION_STATE_SLOT,
                ColumnName.MACHINE_ID,
                TableName.OBSERVATION_STATE_SLOT + "enddatetime",
                TableName.OBSERVATION_STATE_SLOT + "begindatetime");
      AddIndex (TableName.OBSERVATION_STATE_SLOT,
                ColumnName.MACHINE_ID,
                TableName.OBSERVATION_STATE_SLOT + "endday",
                TableName.OBSERVATION_STATE_SLOT + "beginday");
      AddNamedIndexCondition (TableName.OBSERVATION_STATE_SLOT + "_nomos",
                              TableName.OBSERVATION_STATE_SLOT,
                              "machineobservationstateid IS NULL",
                              ColumnName.MACHINE_ID,
                              TableName.OBSERVATION_STATE_SLOT + "enddatetime",
                              TableName.OBSERVATION_STATE_SLOT + "begindatetime");
    }
    
    void CreateView ()
    {
      Database.ExecuteNonQuery (@"CREATE OR REPLACE VIEW observationstatebeginendslot AS
SELECT observationstateslotid, observationstateslotversion, machineid, userid, shiftid,
  machineobservationstateid, machinestatetemplateid, observationstateslotproduction,
  CASE WHEN lower_inf(observationstateslotdatetimerange)
    THEN '1970-01-01 00:00:00'::timestamp without time zone
    ELSE lower(observationstateslotdatetimerange)
  END AS observationstateslotbegindatetime,
  CASE WHEN upper_inf(observationstateslotdatetimerange)
    THEN NULL::timestamp without time zone
    ELSE upper(observationstateslotdatetimerange)
  END AS observationstateslotenddatetime,
  CASE WHEN lower_inf(observationstateslotdayrange)
    THEN '1970-01-01 00:00:00'::timestamp without time zone
    ELSE lower(observationstateslotdayrange)
  END AS observationstateslotbeginday,
  CASE WHEN upper_inf(observationstateslotdayrange)
    THEN NULL::timestamp without time zone
    ELSE upper(observationstateslotdayrange)
  END AS observationstateslotendday
FROM observationstateslot
");

      // For Borland C++
      Database.ExecuteNonQuery (@"CREATE OR REPLACE VIEW observationstateslotbor AS
SELECT observationstateslotid, observationstateslotversion, machineid,
  CASE WHEN lower_inf(observationstateslotdatetimerange)
    THEN '1970-01-01 00:00:00'::timestamp without time zone
    ELSE lower(observationstateslotdatetimerange)
  END AS observationstateslotbegindatetime,
  CASE WHEN lower_inf(observationstateslotdayrange)
    THEN '1970-01-01 00:00:00'::timestamp without time zone
    ELSE lower(observationstateslotdayrange)
  END AS observationstateslotbeginday,
  CASE WHEN upper_inf(observationstateslotdatetimerange)
    THEN NULL::timestamp without time zone
    ELSE upper(observationstateslotdatetimerange)
  END AS observationstateslotenddatetime,
  CASE WHEN upper_inf(observationstateslotdayrange)
    THEN NULL::timestamp without time zone
    ELSE upper(observationstateslotdayrange)
  END AS observationstateslotendday,
  machineobservationstateid,
  userid, shiftid
FROM observationstateslot
");
    }
  }
}
