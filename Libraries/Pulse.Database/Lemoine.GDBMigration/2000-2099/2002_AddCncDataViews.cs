// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 2002: views and functions to read the cncdata tables (see migration 2000)
  /// <item>cncdatasampleall: one row by sample, from cncdatasample and cncdatasamplearchive</item>
  /// <item>cncdataperiodall: one row by period, from cncdataperiod and cncdataperiodarchive</item>
  /// <item>cncdatasamplerange / cncdataperiodrange: the same rows for a machine module, a field and a period</item>
  ///
  /// A filter on a computed date/time (after the unnest) does not prune any partition:
  /// the range functions filter first on the date/times of the stored rows,
  /// with a margin of the longest row (1 minute for the samples, 7 days for the periods)
  /// </summary>
  [Migration (2002)]
  public class AddCncDataViews : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddCncDataViews).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddSampleView ();
      AddSampleRangeFunction ();
      AddPeriodView ();
      AddPeriodRangeFunction ();
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery ("DROP FUNCTION IF EXISTS public.cncdataperiodrange (integer, integer, timestamp without time zone, timestamp without time zone)");
      Database.ExecuteNonQuery ("DROP VIEW IF EXISTS public.cncdataperiodall");
      Database.ExecuteNonQuery ("DROP FUNCTION IF EXISTS public.cncdatasamplerange (integer, integer, timestamp without time zone, timestamp without time zone)");
      Database.ExecuteNonQuery ("DROP VIEW IF EXISTS public.cncdatasampleall");
    }

    void AddSampleView ()
    {
      // Multi-argument unnest (ROWS FROM): each array is detoasted once
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE VIEW public.cncdatasampleall AS
SELECT s.machinemoduleid,
       s.fieldid,
       s.cncdatasamplebegindatetime AS begindatetime,
       s.cncdatasampleenddatetime AS enddatetime,
       s.cncdatasamplebegindatetime + u.delta * interval '1 millisecond' AS datetime,
       u.doublevalue,
       u.jsonvalue,
       FALSE AS archived
  FROM public.cncdatasample s
  CROSS JOIN LATERAL ROWS FROM (unnest (s.cncdatasampledeltas),
                                unnest (s.cncdatasampledoublevalues),
                                jsonb_array_elements (s.cncdatasamplejsonvalues)) AS u (delta, doublevalue, jsonvalue)
UNION ALL
SELECT a.machinemoduleid,
       a.fieldid,
       a.cncdatasamplearchivebegindatetime,
       a.cncdatasamplearchiveenddatetime,
       a.cncdatasamplearchivebegindatetime + u.delta * interval '1 millisecond',
       coalesce (u.intvalue * a.cncdatasamplearchivescale, u.doublevalue),
       u.jsonvalue,
       TRUE
  FROM public.cncdatasamplearchive a
  CROSS JOIN LATERAL ROWS FROM (unnest (a.cncdatasamplearchivedeltas),
                                unnest (a.cncdatasamplearchiveintvalues),
                                unnest (a.cncdatasamplearchivedoublevalues),
                                jsonb_array_elements (a.cncdatasamplearchivejsonvalues)) AS u (delta, intvalue, doublevalue, jsonvalue)");
      Database.ExecuteNonQuery (@"
COMMENT ON VIEW public.cncdatasampleall IS
'One row by cnc data sample, from cncdatasample and cncdatasamplearchive.
begindatetime/enddatetime are those of the stored batch: filter on them to prune the partitions, or use cncdatasamplerange'");
    }

    void AddSampleRangeFunction ()
    {
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE FUNCTION public.cncdatasamplerange (p_machinemoduleid integer, p_fieldid integer,
                                                      p_from timestamp without time zone, p_to timestamp without time zone)
RETURNS TABLE (datetime timestamp without time zone, doublevalue double precision, jsonvalue jsonb)
LANGUAGE sql STABLE AS
$$
  SELECT v.datetime, v.doublevalue, v.jsonvalue
    FROM public.cncdatasampleall v
   WHERE v.machinemoduleid = p_machinemoduleid
     AND v.fieldid = p_fieldid
     AND v.begindatetime < p_to
     AND v.enddatetime >= p_from
     AND v.begindatetime >= p_from - interval '1 minute' -- longest batch (archive)
     AND v.datetime >= p_from
     AND v.datetime < p_to
   ORDER BY v.datetime
$$");
      Database.ExecuteNonQuery (@"
COMMENT ON FUNCTION public.cncdatasamplerange (integer, integer, timestamp without time zone, timestamp without time zone) IS
'Cnc data samples of a machine module and a field in [p_from, p_to) (UTC), hot and archived data'");
    }

    void AddPeriodView ()
    {
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE VIEW public.cncdataperiodall AS
SELECT p.cncdataperiodid AS id,
       p.machinemoduleid,
       p.fieldid,
       p.cncdataperiodbegindatetime AS begindatetime,
       p.cncdataperiodenddatetime AS enddatetime,
       p.cncdataperiodstopped AS stopped,
       p.cncdataperiodcount AS count,
       p.cncdataperiodaverage AS average,
       p.cncdataperiodmin AS min,
       p.cncdataperiodmax AS max,
       p.cncdataperiodstddev AS stddev,
       p.cncdataperiodlastvalue AS lastvalue,
       p.cncdataperiodstringvalue AS stringvalue,
       p.cncdataperiodjsonvalue AS jsonvalue,
       p.cncdataperiodbegindatetime AS rowbegindatetime,
       p.cncdataperiodenddatetime AS rowenddatetime,
       FALSE AS archived
  FROM public.cncdataperiod p
UNION ALL
SELECT u.id,
       a.machinemoduleid,
       a.fieldid,
       a.cncdataperiodarchivebegindatetime + u.begindelta * interval '1 millisecond',
       a.cncdataperiodarchivebegindatetime + (u.begindelta + u.duration) * interval '1 millisecond',
       u.stopped,
       u.count,
       u.average,
       u.min,
       u.max,
       u.stddev,
       u.lastvalue,
       u.stringvalue,
       u.jsonvalue,
       a.cncdataperiodarchivebegindatetime,
       a.cncdataperiodarchiveenddatetime,
       TRUE
  FROM public.cncdataperiodarchive a
  CROSS JOIN LATERAL ROWS FROM (unnest (a.cncdataperiodarchiveids),
                                unnest (a.cncdataperiodarchivebegindeltas),
                                unnest (a.cncdataperiodarchivedurations),
                                unnest (a.cncdataperiodarchivestoppeds),
                                unnest (a.cncdataperiodarchivecounts),
                                unnest (a.cncdataperiodarchiveaverages),
                                unnest (a.cncdataperiodarchivemins),
                                unnest (a.cncdataperiodarchivemaxs),
                                unnest (a.cncdataperiodarchivestddevs),
                                unnest (a.cncdataperiodarchivelastvalues),
                                unnest (a.cncdataperiodarchivestringvalues),
                                jsonb_array_elements (a.cncdataperiodarchivejsonvalues))
    AS u (id, begindelta, duration, stopped, count, average, min, max, stddev, lastvalue, stringvalue, jsonvalue)");
      Database.ExecuteNonQuery (@"
COMMENT ON VIEW public.cncdataperiodall IS
'One row by cnc data period, from cncdataperiod and cncdataperiodarchive.
rowbegindatetime/rowenddatetime are those of the stored row: filter on them to prune the partitions, or use cncdataperiodrange'");
    }

    void AddPeriodRangeFunction ()
    {
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE FUNCTION public.cncdataperiodrange (p_machinemoduleid integer, p_fieldid integer,
                                                      p_from timestamp without time zone, p_to timestamp without time zone)
RETURNS TABLE (id bigint, begindatetime timestamp without time zone, enddatetime timestamp without time zone,
               stopped boolean, count integer, average double precision, min double precision, max double precision,
               stddev double precision, lastvalue double precision, stringvalue text, jsonvalue jsonb, archived boolean)
LANGUAGE sql STABLE AS
$$
  SELECT v.id, v.begindatetime, v.enddatetime, v.stopped, v.count, v.average, v.min, v.max,
         v.stddev, v.lastvalue, v.stringvalue, v.jsonvalue, v.archived
    FROM public.cncdataperiodall v
   WHERE v.machinemoduleid = p_machinemoduleid
     AND v.fieldid = p_fieldid
     AND v.rowbegindatetime < p_to
     AND v.rowenddatetime >= p_from
     AND v.rowbegindatetime >= p_from - interval '7 days' -- longest row: a period or an archive row never crosses a week
     AND v.begindatetime < p_to
     AND v.enddatetime >= p_from
   ORDER BY v.begindatetime
$$");
      Database.ExecuteNonQuery (@"
COMMENT ON FUNCTION public.cncdataperiodrange (integer, integer, timestamp without time zone, timestamp without time zone) IS
'Cnc data periods of a machine module and a field that intersect [p_from, p_to) (UTC), hot and archived data'");
    }
  }
}
