// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 2001: functions to manage the partitions of the cncdata tables (see migration 2000)
  /// <item>cncdataweekstart: start of the week of a date/time, 00:00 UTC of the cut-off day
  /// (0 = Sunday ... 6 = Saturday, as DayOfWeek and extract (dow), config Cnc.DataPartitionWeekCutOffDay)</item>
  /// <item>cncdatapartitions: view of the leaf partitions with their bounds, read from the catalog</item>
  /// <item>cncdatacreatepartition: create (if needed) the partition of a table that contains a date/time</item>
  /// <item>cncdatacreatepartitions: create the week partitions of all the machine modules on a period</item>
  ///
  /// A leaf partition is prepared on its own, then attached without any scan:
  /// it gets a CHECK on the machine module (the machine level partition constraint),
  /// CHECK constraints on the bounds, the foreign keys (not copied by LIKE) and its indexes (LIKE ... INCLUDING ALL).
  /// The CHECK on the end date/time prevents a row from crossing the partition bound.
  /// The partitions of cncdataperiod and of the archives get an exclusion constraint (no overlap by field).
  /// </summary>
  [Migration (2001)]
  public class AddCncDataPartitionFunctions : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddCncDataPartitionFunctions).FullName);

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddWeekStartFunction ();
      AddPartitionsView ();
      AddCreatePartitionFunction ();
      AddCreatePartitionsFunction ();
      CreateInitialPartitions ();
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery ("DROP FUNCTION IF EXISTS public.cncdatacreatepartitions (timestamp without time zone, timestamp without time zone, integer)");
      Database.ExecuteNonQuery ("DROP FUNCTION IF EXISTS public.cncdatacreatepartition (text, integer, timestamp without time zone, integer)");
      Database.ExecuteNonQuery ("DROP VIEW IF EXISTS public.cncdatapartitions");
      Database.ExecuteNonQuery ("DROP FUNCTION IF EXISTS public.cncdataweekstart (timestamp without time zone, integer)");
    }

    void AddWeekStartFunction ()
    {
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE FUNCTION public.cncdataweekstart (p_datetime timestamp without time zone, p_cutoffdow integer DEFAULT 0)
RETURNS timestamp without time zone
LANGUAGE sql IMMUTABLE PARALLEL SAFE AS
$$
  SELECT date_trunc ('day', p_datetime)
       - make_interval (days => ((extract (dow FROM p_datetime)::integer - p_cutoffdow + 7) % 7))
$$");
      Database.ExecuteNonQuery (@"
COMMENT ON FUNCTION public.cncdataweekstart (timestamp without time zone, integer) IS
'Start of the week of a UTC date/time: 00:00 of the cut-off day (0 = Sunday ... 6 = Saturday)'");
    }

    void AddPartitionsView ()
    {
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE VIEW public.cncdatapartitions AS
SELECT topc.relname::text AS tablename,
       substring (pg_get_expr (machinec.relpartbound, machinec.oid) FROM 'IN \((\d+)\)')::integer AS machinemoduleid,
       leafc.relname::text AS partitionname,
       (regexp_match (pg_get_expr (leafc.relpartbound, leafc.oid), $re$FROM \('([^']+)'\) TO \('([^']+)'\)$re$))[1]::timestamp without time zone AS lowerbound,
       (regexp_match (pg_get_expr (leafc.relpartbound, leafc.oid), $re$FROM \('([^']+)'\) TO \('([^']+)'\)$re$))[2]::timestamp without time zone AS upperbound
  FROM pg_class topc
  JOIN pg_namespace n ON n.oid = topc.relnamespace AND n.nspname = 'public'
  JOIN pg_inherits machinei ON machinei.inhparent = topc.oid
  JOIN pg_class machinec ON machinec.oid = machinei.inhrelid
  JOIN pg_inherits leafi ON leafi.inhparent = machinec.oid
  JOIN pg_class leafc ON leafc.oid = leafi.inhrelid
 WHERE topc.relname IN ('cncdatasample', 'cncdatasamplearchive', 'cncdataperiod', 'cncdataperiodarchive')");
      Database.ExecuteNonQuery (@"
COMMENT ON VIEW public.cncdatapartitions IS
'Leaf partitions of the cncdata tables, with their machine module and their bounds [lowerbound, upperbound)'");
    }

    void AddCreatePartitionFunction ()
    {
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE FUNCTION public.cncdatacreatepartition (p_table text, p_machinemoduleid integer,
                                                          p_datetime timestamp without time zone, p_cutoffdow integer DEFAULT 0)
RETURNS text
LANGUAGE plpgsql AS
$$
DECLARE
  v_machinetable text := p_table || '_mm' || p_machinemoduleid;
  v_begin text := p_table || 'begindatetime';
  v_end text := p_table || 'enddatetime';
  v_leaf text;
  v_lower timestamp without time zone;
  v_upper timestamp without time zone;
  v_quarter timestamp without time zone;
  v_exclusion text := '';
BEGIN
  IF p_table NOT IN ('cncdatasample', 'cncdatasamplearchive', 'cncdataperiod', 'cncdataperiodarchive') THEN
    RAISE EXCEPTION 'cncdatacreatepartition: unknown table %', p_table;
  END IF;

  -- One partition creation at a time for a table
  PERFORM pg_advisory_xact_lock (hashtext ('cncdatacreatepartition.' || p_table));

  -- Machine level
  IF to_regclass ('public.' || v_machinetable) IS NULL THEN
    EXECUTE format ('CREATE TABLE public.%I PARTITION OF public.%I FOR VALUES IN (%s) PARTITION BY RANGE (%I)',
                    v_machinetable, p_table, p_machinemoduleid, v_begin);
  END IF;

  -- Existing partition
  SELECT partitionname INTO v_leaf
    FROM public.cncdatapartitions
   WHERE tablename = p_table AND machinemoduleid = p_machinemoduleid
     AND lowerbound <= p_datetime AND p_datetime < upperbound;
  IF FOUND THEN
    RETURN v_leaf;
  END IF;

  -- Bounds: a week, or a quarter of whole weeks for the archives
  IF p_table LIKE '%archive' THEN
    v_quarter := date_trunc ('quarter', p_datetime);
    v_lower := public.cncdataweekstart (v_quarter, p_cutoffdow);
    v_upper := public.cncdataweekstart (v_quarter + interval '3 months', p_cutoffdow);
    IF v_upper <= p_datetime THEN
      v_lower := v_upper;
      v_upper := public.cncdataweekstart (v_quarter + interval '6 months', p_cutoffdow);
    END IF;
  ELSE
    v_lower := public.cncdataweekstart (p_datetime, p_cutoffdow);
    v_upper := v_lower + interval '7 days';
  END IF;

  -- Not to overlap the existing partitions (for example after a change of the cut-off day)
  SELECT greatest (v_lower, max (upperbound)) INTO v_lower
    FROM public.cncdatapartitions
   WHERE tablename = p_table AND machinemoduleid = p_machinemoduleid AND upperbound <= p_datetime;
  SELECT least (v_upper, min (lowerbound)) INTO v_upper
    FROM public.cncdatapartitions
   WHERE tablename = p_table AND machinemoduleid = p_machinemoduleid AND p_datetime < lowerbound;

  v_leaf := v_machinetable || '_' || to_char (v_lower, 'YYYYMMDD');
  IF p_table <> 'cncdatasample' THEN
    v_exclusion := format (', CONSTRAINT %I EXCLUDE USING gist (fieldid WITH =, tsrange (%I, %I) WITH &&)',
                           v_leaf || '_nooverlap', v_begin, v_end);
  END IF;

  -- Prepared on its own, so that it is attached without any scan
  EXECUTE format ('CREATE TABLE public.%I (LIKE public.%I INCLUDING ALL EXCLUDING IDENTITY, '
                  'CONSTRAINT %I CHECK (machinemoduleid = %s), '
                  'CONSTRAINT %I CHECK (%I >= %L AND %I < %L), '
                  'CONSTRAINT %I CHECK (%I <= %L), '
                  'CONSTRAINT %I FOREIGN KEY (machinemoduleid) REFERENCES public.machinemodule (machinemoduleid) ON DELETE CASCADE, '
                  'CONSTRAINT %I FOREIGN KEY (fieldid) REFERENCES public.field (fieldid) ON DELETE CASCADE'
                  '%s)',
                  v_leaf, p_table,
                  v_leaf || '_machinemodule', p_machinemoduleid,
                  v_leaf || '_bounds', v_begin, v_lower, v_begin, v_upper,
                  v_leaf || '_nocrossing', v_end, v_upper,
                  v_leaf || '_machinemoduleid_fkey',
                  v_leaf || '_fieldid_fkey',
                  v_exclusion);
  EXECUTE format ('ALTER TABLE public.%I ATTACH PARTITION public.%I FOR VALUES FROM (%L) TO (%L)',
                  v_machinetable, v_leaf, v_lower, v_upper);
  RETURN v_leaf;
END
$$");
      Database.ExecuteNonQuery (@"
COMMENT ON FUNCTION public.cncdatacreatepartition (text, integer, timestamp without time zone, integer) IS
'Create if needed the partition of a cncdata table (and the machine level one) that contains a UTC date/time.
Weeks for cncdatasample and cncdataperiod, quarters of whole weeks for the archives. Return the partition name.'");
    }

    void AddCreatePartitionsFunction ()
    {
      Database.ExecuteNonQuery (@"
CREATE OR REPLACE FUNCTION public.cncdatacreatepartitions (p_from timestamp without time zone, p_to timestamp without time zone,
                                                           p_cutoffdow integer DEFAULT 0)
RETURNS integer
LANGUAGE plpgsql AS
$$
DECLARE
  v_machinemoduleid integer;
  v_table text;
  v_datetime timestamp without time zone;
  v_before integer;
  v_after integer;
BEGIN
  SELECT count (*) INTO v_before FROM public.cncdatapartitions;
  FOR v_machinemoduleid IN SELECT machinemoduleid FROM public.machinemodule ORDER BY machinemoduleid LOOP
    FOREACH v_table IN ARRAY ARRAY['cncdatasample', 'cncdataperiod'] LOOP
      -- Day by day: an existing partition is just returned, and the step works with irregular bounds
      v_datetime := p_from;
      WHILE v_datetime <= p_to LOOP
        PERFORM public.cncdatacreatepartition (v_table, v_machinemoduleid, v_datetime, p_cutoffdow);
        v_datetime := v_datetime + interval '1 day';
      END LOOP;
    END LOOP;
  END LOOP;
  SELECT count (*) INTO v_after FROM public.cncdatapartitions;
  RETURN v_after - v_before;
END
$$");
      Database.ExecuteNonQuery (@"
COMMENT ON FUNCTION public.cncdatacreatepartitions (timestamp without time zone, timestamp without time zone, integer) IS
'Create the week partitions of cncdatasample and cncdataperiod for all the machine modules from p_from to p_to (UTC).
Return the number of created partitions.'");
    }

    void CreateInitialPartitions ()
    {
      // Current week and the next two ones, with the default cut-off day (Sunday)
      Database.ExecuteNonQuery (@"
SELECT public.cncdatacreatepartitions (now () AT TIME ZONE 'UTC', now () AT TIME ZONE 'UTC' + interval '14 days', 0)");
    }
  }
}
