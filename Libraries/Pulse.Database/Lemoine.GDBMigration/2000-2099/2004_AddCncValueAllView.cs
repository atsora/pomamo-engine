// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 2004: reading facade of the cnc data, compatible with table cncvalue
  /// <item>cncvalueall: union of cncvalue, cncdataperiod and cncdataperiodarchive (view cncdataperiodall)</item>
  /// <item>cncvaluerange: the same rows for a machine module, a field and a period, with the partition pruning</item>
  /// <item>cncvalueat: the same rows for all the fields of a machine module at a date/time, with the partition pruning</item>
  ///
  /// A field stores its data in a single table at a time (field.fieldcncdatastorage): the historical data may be in cncvalue
  /// and the new ones in cncdataperiod, without any overlap, so a simple union is enough.
  ///
  /// The periods are translated into the columns of cncvalue according to field.cncdataaggregationtype:
  /// <item>cncvaluedouble: NewValue (3) last value, Max (4) maximum, Sum (2) average * count, Average (1) average,
  ///       Counter (5) cumulated total at the end, Integral (6) average rate (the cumulated totals are in cncvaluemin / cncvaluemax)</item>
  /// <item>cncvaluedeviation: Average standard deviation, Max max - min (as in cncvalue), else null</item>
  /// <item>cncvalueint: integer part of cncvaluedouble (not for a String field)</item>
  /// <item>cncvaluestring: string value, else json value (except for Counter and Integral, where it is a state of the import),
  ///       else True/False for a Boolean field, else cncvaluedouble as text</item>
  /// Additional columns: cncvaluesource (cncvalue, cncdataperiod, cncdataperiodarchive), cncvaluecount, cncvaluemin, cncvaluemax, cncvaluejson,
  /// rowbegindatetime / rowenddatetime (stored row, to prune the partitions).
  ///
  /// Note: cncvalueid is unique only with cncvaluesource. The averages of the periods give the same weight to each value
  /// (the averages of cncvalue are weighted by the duration)
  /// </summary>
  [Migration (2004)]
  public class AddCncValueAllView : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddCncValueAllView).FullName);

    // Columns of a period: p (cncdataperiodall or cncdataperiodrange), f (field), x.d (the translated double)
    static readonly string DOUBLE = @"
CASE f.cncdataaggregationtype
  WHEN 3 THEN p.lastvalue
  WHEN 4 THEN p.max
  WHEN 2 THEN p.average * p.count
  WHEN 1 THEN p.average
  WHEN 5 THEN p.lastvalue
  WHEN 6 THEN p.average
  ELSE coalesce (p.average, p.lastvalue)
END";
    static readonly string STRING = @"
CASE
  WHEN p.stringvalue IS NOT NULL THEN p.stringvalue
  WHEN p.jsonvalue IS NOT NULL AND coalesce (f.cncdataaggregationtype, 0) NOT IN (5, 6) THEN p.jsonvalue::text
  WHEN x.d IS NULL THEN NULL
  WHEN f.fieldtype = 'Boolean' THEN CASE WHEN x.d <> 0 THEN 'True' ELSE 'False' END
  ELSE x.d::text
END::character varying";
    static readonly string INT = @"
CASE
  WHEN f.fieldtype = 'String' THEN NULL
  WHEN abs (x.d) < 2147483648 THEN trunc (x.d)::integer
END";
    static readonly string DEVIATION = @"
CASE f.cncdataaggregationtype
  WHEN 1 THEN p.stddev
  WHEN 4 THEN p.max - p.min
END";

    static readonly string CNC_VALUE_COLUMNS = @"
v.cncvalueid, v.machinemoduleid, v.fieldid, v.cncvaluebegindatetime, v.cncvalueenddatetime,
v.cncvaluestring, v.cncvalueint, v.cncvaluedouble, v.cncvaluedeviation, v.cncvaluestopped,
'cncvalue'::text AS cncvaluesource,
NULL::integer AS cncvaluecount, NULL::double precision AS cncvaluemin, NULL::double precision AS cncvaluemax, NULL::jsonb AS cncvaluejson,
v.cncvaluebegindatetime AS rowbegindatetime, v.cncvalueenddatetime AS rowenddatetime";

    static string PeriodColumns (string machineModuleId, string fieldId, string rowBegin, string rowEnd) => $@"
p.id, {machineModuleId}, {fieldId}, p.begindatetime, p.enddatetime,
{STRING}, {INT}, x.d, {DEVIATION}, p.stopped,
CASE WHEN p.archived THEN 'cncdataperiodarchive' ELSE 'cncdataperiod' END,
p.count, p.min, p.max, p.jsonvalue,
{rowBegin}, {rowEnd}";

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery ($@"
CREATE OR REPLACE VIEW public.cncvalueall AS
SELECT {CNC_VALUE_COLUMNS}
  FROM public.cncvalue v
UNION ALL
SELECT {PeriodColumns ("p.machinemoduleid", "p.fieldid", "p.rowbegindatetime", "p.rowenddatetime")}
  FROM public.cncdataperiodall p
  JOIN public.field f ON f.fieldid = p.fieldid
  CROSS JOIN LATERAL (SELECT {DOUBLE} AS d) x");
      Database.ExecuteNonQuery (@"
COMMENT ON VIEW public.cncvalueall IS
'Cnc values from cncvalue, cncdataperiod and cncdataperiodarchive with the columns of cncvalue (a field is stored in a single table at a time).
cncvalueid is unique only with cncvaluesource. To read a period of a machine module and a field, prefer cncvaluerange (partition pruning)'");

      Database.ExecuteNonQuery ($@"
CREATE OR REPLACE FUNCTION public.cncvaluerange (p_machinemoduleid integer, p_fieldid integer,
                                                 p_from timestamp without time zone, p_to timestamp without time zone)
RETURNS SETOF public.cncvalueall
LANGUAGE sql STABLE AS
$$
  SELECT u.*
    FROM (
      SELECT {CNC_VALUE_COLUMNS}
        FROM public.cncvalue v
       WHERE v.machinemoduleid = p_machinemoduleid
         AND v.fieldid = p_fieldid
         AND v.cncvaluebegindatetime < p_to
         AND v.cncvalueenddatetime >= p_from
      UNION ALL
      SELECT {PeriodColumns ("p_machinemoduleid", "p_fieldid", "p.begindatetime", "p.enddatetime")}
        FROM public.cncdataperiodrange (p_machinemoduleid, p_fieldid, p_from, p_to) p
        JOIN public.field f ON f.fieldid = p_fieldid
        CROSS JOIN LATERAL (SELECT {DOUBLE} AS d) x
    ) u
   ORDER BY u.cncvaluebegindatetime
$$");
      Database.ExecuteNonQuery (@"
COMMENT ON FUNCTION public.cncvaluerange (integer, integer, timestamp without time zone, timestamp without time zone) IS
'Cnc values of a machine module and a field that intersect [p_from, p_to) (UTC) from cncvalue, cncdataperiod and cncdataperiodarchive,
with the columns of the view cncvalueall, ordered by begin'");

      // All the fields of a machine module at a date/time.
      // A period begins at most 7 days before (it never crosses a week bound): this prunes the partitions
      Database.ExecuteNonQuery ($@"
CREATE OR REPLACE FUNCTION public.cncvalueat (p_machinemoduleid integer, p_at timestamp without time zone)
RETURNS SETOF public.cncvalueall
LANGUAGE sql STABLE AS
$$
  SELECT {CNC_VALUE_COLUMNS}
    FROM public.cncvalue v
   WHERE v.machinemoduleid = p_machinemoduleid
     AND v.cncvaluebegindatetime <= p_at
     AND v.cncvalueenddatetime >= p_at
  UNION ALL
  SELECT {PeriodColumns ("p.machinemoduleid", "p.fieldid", "p.rowbegindatetime", "p.rowenddatetime")}
    FROM public.cncdataperiodall p
    JOIN public.field f ON f.fieldid = p.fieldid
    CROSS JOIN LATERAL (SELECT {DOUBLE} AS d) x
   WHERE p.machinemoduleid = p_machinemoduleid
     AND p.rowbegindatetime <= p_at
     AND p.rowbegindatetime >= p_at - interval '7 days'
     AND p.rowenddatetime >= p_at
     AND p.begindatetime <= p_at
     AND p.enddatetime >= p_at
$$");
      Database.ExecuteNonQuery (@"
COMMENT ON FUNCTION public.cncvalueat (integer, timestamp without time zone) IS
'Cnc values of all the fields of a machine module that contain p_at (UTC, end included) from cncvalue, cncdataperiod and cncdataperiodarchive,
with the columns of the view cncvalueall'");
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery ("DROP FUNCTION IF EXISTS public.cncvalueat (integer, timestamp without time zone)");
      Database.ExecuteNonQuery ("DROP FUNCTION IF EXISTS public.cncvaluerange (integer, integer, timestamp without time zone, timestamp without time zone)");
      Database.ExecuteNonQuery ("DROP VIEW IF EXISTS public.cncvalueall");
    }
  }
}
