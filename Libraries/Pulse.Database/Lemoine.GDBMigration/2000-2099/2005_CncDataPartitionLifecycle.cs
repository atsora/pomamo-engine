// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 2005: life cycle of the cncdata partitions with the machine modules and the fields
  /// <item>cncdatacreatemachinepartition: the machine module level partition of a table is prepared on its own, then attached
  ///       (ATTACH PARTITION locks the parent table in SHARE UPDATE EXCLUSIVE mode: the reads and the writes are not blocked,
  ///       while CREATE TABLE ... PARTITION OF locked it in ACCESS EXCLUSIVE mode until the end of the transaction)</item>
  /// <item>cncdatacreatepartition: uses it; a lock is waited at most 10 s (lock_timeout of the function)</item>
  /// <item>cncdatadropmachinemodule: drop the partitions of a machine module (instantaneous, no row by row delete),
  ///       called by a trigger before a machine module is deleted: the ON DELETE CASCADE foreign keys have nothing to delete any more</item>
  /// <item>cncdatadroporphanpartitions: drop the partitions of the machine modules that do not exist any more
  ///       (used by the database maintenance)</item>
  /// <item>the foreign keys to field are ON DELETE RESTRICT: a field with some cnc data can't be deleted
  ///       (the cascade would delete row by row in all the partitions of all the machine modules); deactivate it instead</item>
  /// </summary>
  [Migration (2005)]
  public class CncDataPartitionLifecycle : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CncDataPartitionLifecycle).FullName);

    static readonly string TABLES = "ARRAY['cncdatasample', 'cncdatasamplearchive', 'cncdataperiod', 'cncdataperiodarchive']";
    static readonly string LOCK_TIMEOUT = "10s";

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      AddCreateMachinePartitionFunction ();
      Database.ExecuteNonQuery (BuildCreatePartitionFunction (true));
      SetFieldForeignKeys ("RESTRICT");
      AddDropMachineModuleFunction ();
      AddDropOrphanPartitionsFunction ();
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery ("DROP FUNCTION IF EXISTS public.cncdatadroporphanpartitions ()");
      Database.ExecuteNonQuery ("DROP TRIGGER IF EXISTS cncdata_machinemodule_beforedelete ON public.machinemodule");
      Database.ExecuteNonQuery ("DROP FUNCTION IF EXISTS public.cncdatamachinemodulebeforedelete ()");
      Database.ExecuteNonQuery ("DROP FUNCTION IF EXISTS public.cncdatadropmachinemodule (integer)");
      SetFieldForeignKeys ("CASCADE");
      Database.ExecuteNonQuery (BuildCreatePartitionFunction (false));
      Database.ExecuteNonQuery ("DROP FUNCTION IF EXISTS public.cncdatacreatemachinepartition (text, integer)");
    }

    void AddCreateMachinePartitionFunction ()
    {
      Database.ExecuteNonQuery ($@"
CREATE OR REPLACE FUNCTION public.cncdatacreatemachinepartition (p_table text, p_machinemoduleid integer)
RETURNS text
LANGUAGE plpgsql
SET lock_timeout = '{LOCK_TIMEOUT}'
AS
$$
DECLARE
  v_machinetable text := p_table || '_mm' || p_machinemoduleid;
BEGIN
  IF p_table NOT IN ('cncdatasample', 'cncdatasamplearchive', 'cncdataperiod', 'cncdataperiodarchive') THEN
    RAISE EXCEPTION 'cncdatacreatemachinepartition: unknown table %', p_table;
  END IF;

  -- One partition creation at a time for a table
  PERFORM pg_advisory_xact_lock (hashtext ('cncdatacreatepartition.' || p_table));

  IF to_regclass ('public.' || v_machinetable) IS NULL THEN
    -- Prepared on its own, then attached: the parent table is not locked in ACCESS EXCLUSIVE mode.
    -- No CHECK constraint on the machine module (it would be inherited by the leaf partitions):
    -- the new table is empty, so its validation by ATTACH is immediate
    EXECUTE format ('CREATE TABLE public.%I (LIKE public.%I INCLUDING ALL EXCLUDING IDENTITY) '
                    'PARTITION BY RANGE (%I)',
                    v_machinetable, p_table,
                    p_table || 'begindatetime');
    EXECUTE format ('ALTER TABLE public.%I ATTACH PARTITION public.%I FOR VALUES IN (%s)',
                    p_table, v_machinetable, p_machinemoduleid);
  END IF;
  RETURN v_machinetable;
END
$$");
      Database.ExecuteNonQuery (@"
COMMENT ON FUNCTION public.cncdatacreatemachinepartition (text, integer) IS
'Create if needed the machine module level partition of a cncdata table, prepared on its own then attached
(the parent table is not locked in ACCESS EXCLUSIVE mode). Return the partition name.'");
    }

    /// <summary>
    /// Function cncdatacreatepartition
    /// </summary>
    /// <param name="v2005">true: version 2005 (machine level partition attached, foreign key to field ON DELETE RESTRICT, lock_timeout),
    /// false: version 2001</param>
    static string BuildCreatePartitionFunction (bool v2005)
    {
      var settings = v2005 ? $"SET lock_timeout = '{LOCK_TIMEOUT}'" : "";
      var machineLevel = v2005
        ? @"  PERFORM public.cncdatacreatemachinepartition (p_table, p_machinemoduleid);"
        : @"  IF to_regclass ('public.' || v_machinetable) IS NULL THEN
    EXECUTE format ('CREATE TABLE public.%I PARTITION OF public.%I FOR VALUES IN (%s) PARTITION BY RANGE (%I)',
                    v_machinetable, p_table, p_machinemoduleid, v_begin);
  END IF;";
      var fieldDeleteAction = v2005 ? "RESTRICT" : "CASCADE";
      return $@"
CREATE OR REPLACE FUNCTION public.cncdatacreatepartition (p_table text, p_machinemoduleid integer,
                                                          p_datetime timestamp without time zone, p_cutoffdow integer DEFAULT 0)
RETURNS text
LANGUAGE plpgsql
{settings}
AS
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
{machineLevel}

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
                  'CONSTRAINT %I FOREIGN KEY (fieldid) REFERENCES public.field (fieldid) ON DELETE {fieldDeleteAction}'
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
$$";
    }

    /// <summary>
    /// Foreign keys to field of the cncdata tables, propagated to all the partitions
    /// </summary>
    /// <param name="deleteAction">RESTRICT or CASCADE</param>
    void SetFieldForeignKeys (string deleteAction)
    {
      var oppositeAction = deleteAction.Equals ("RESTRICT") ? "c" : "r";
      Database.ExecuteNonQuery ($@"
DO $$
DECLARE
  v_table text;
  r record;
BEGIN
  FOREACH v_table IN ARRAY {TABLES} LOOP
    EXECUTE format ('ALTER TABLE public.%I DROP CONSTRAINT IF EXISTS %I', v_table, v_table || '_fieldid_fkey');
    EXECUTE format ('ALTER TABLE public.%I ADD CONSTRAINT %I FOREIGN KEY (fieldid) REFERENCES public.field (fieldid) ON DELETE {deleteAction}',
                    v_table, v_table || '_fieldid_fkey');
  END LOOP;
  -- Foreign keys of the partitions that were not attached to the ones of the parent tables
  FOR r IN
    SELECT c.conrelid::regclass AS rel, c.conname
      FROM pg_catalog.pg_constraint c
      JOIN pg_catalog.pg_class k ON k.oid = c.conrelid
     WHERE c.contype = 'f' AND c.confrelid = 'public.field'::regclass
       AND c.confdeltype = '{oppositeAction}' AND c.conparentid = 0
       AND k.relnamespace = 'public'::regnamespace
       AND k.relname ~ '^cncdata(sample|period)(archive)?_mm[0-9]+'
  LOOP
    EXECUTE format ('ALTER TABLE %s DROP CONSTRAINT %I', r.rel, r.conname);
  END LOOP;
END
$$");
    }

    void AddDropMachineModuleFunction ()
    {
      Database.ExecuteNonQuery ($@"
CREATE OR REPLACE FUNCTION public.cncdatadropmachinemodule (p_machinemoduleid integer)
RETURNS integer
LANGUAGE plpgsql
SET lock_timeout = '{LOCK_TIMEOUT}'
AS
$$
DECLARE
  v_table text;
  v_count integer := 0;
BEGIN
  FOREACH v_table IN ARRAY {TABLES} LOOP
    IF to_regclass ('public.' || v_table || '_mm' || p_machinemoduleid) IS NOT NULL THEN
      EXECUTE format ('DROP TABLE public.%I', v_table || '_mm' || p_machinemoduleid);
      v_count := v_count + 1;
    END IF;
  END LOOP;
  RETURN v_count;
END
$$");
      Database.ExecuteNonQuery (@"
COMMENT ON FUNCTION public.cncdatadropmachinemodule (integer) IS
'Drop the partitions (machine module level, with their leaf partitions) of a machine module in all the cncdata tables.
Return the number of dropped machine module level partitions.'");

      Database.ExecuteNonQuery (@"
CREATE OR REPLACE FUNCTION public.cncdatamachinemodulebeforedelete ()
RETURNS trigger
LANGUAGE plpgsql AS
$$
BEGIN
  PERFORM public.cncdatadropmachinemodule (OLD.machinemoduleid);
  RETURN OLD;
END
$$");
      Database.ExecuteNonQuery (@"
CREATE TRIGGER cncdata_machinemodule_beforedelete
  BEFORE DELETE ON public.machinemodule
  FOR EACH ROW EXECUTE FUNCTION public.cncdatamachinemodulebeforedelete ()");
    }

    void AddDropOrphanPartitionsFunction ()
    {
      Database.ExecuteNonQuery ($@"
CREATE OR REPLACE FUNCTION public.cncdatadroporphanpartitions ()
RETURNS integer
LANGUAGE plpgsql
SET lock_timeout = '{LOCK_TIMEOUT}'
AS
$$
DECLARE
  r record;
  v_count integer := 0;
BEGIN
  FOR r IN
    SELECT c.relname, (regexp_match (c.relname, '_mm([0-9]+)$'))[1]::integer AS machinemoduleid
      FROM pg_catalog.pg_inherits i
      JOIN pg_catalog.pg_class c ON c.oid = i.inhrelid
      JOIN pg_catalog.pg_class p ON p.oid = i.inhparent
     WHERE p.relnamespace = 'public'::regnamespace
       AND p.relname = ANY ({TABLES})
  LOOP
    IF (r.machinemoduleid IS NOT NULL)
      AND NOT EXISTS (SELECT 1 FROM public.machinemodule m WHERE m.machinemoduleid = r.machinemoduleid) THEN
      EXECUTE format ('DROP TABLE public.%I', r.relname);
      v_count := v_count + 1;
    END IF;
  END LOOP;
  RETURN v_count;
END
$$");
      Database.ExecuteNonQuery (@"
COMMENT ON FUNCTION public.cncdatadroporphanpartitions () IS
'Drop the machine module level partitions of the cncdata tables whose machine module does not exist any more.
Return the number of dropped partitions.'");
    }
  }
}
