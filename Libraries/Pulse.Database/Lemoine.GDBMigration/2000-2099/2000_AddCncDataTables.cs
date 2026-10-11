// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Migrator.Framework;

namespace Lemoine.GDBMigration
{
  /// <summary>
  /// Migration 2000: add the tables to store the cnc data more precisely and more efficiently:
  /// <item>cncdatasample: time-stamped samples (high frequency), one row by batch and by field, values in arrays</item>
  /// <item>cncdataperiod: aggregated data on periods, with combinable statistics, to replace cncvalue</item>
  /// <item>cncdatasamplearchive / cncdataperiodarchive: compressed archives</item>
  ///
  /// All the tables are partitioned by machine module (LIST), then by time (RANGE on the begin date/time):
  /// by week for cncdatasample and cncdataperiod, by quarter for the archives.
  /// The partitions are created by the functions of migration 2001.
  /// A row never crosses a partition bound: a CHECK on the end date/time is added in each leaf partition.
  /// </summary>
  [Migration (2000)]
  public class AddCncDataTables : MigrationExt
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AddCncDataTables).FullName);

    static readonly int POSTGRESQL_18 = 18000000;

    /// <summary>
    /// Update the database
    /// </summary>
    override public void Up ()
    {
      Database.ExecuteNonQuery ("CREATE EXTENSION IF NOT EXISTS btree_gist");
      var virtualColumns = IsVirtualColumnSupported ();
      AddCncDataSample (virtualColumns);
      AddCncDataSampleArchive ();
      AddCncDataPeriod (virtualColumns);
      AddCncDataPeriodArchive ();
    }

    /// <summary>
    /// Downgrade the database
    /// </summary>
    override public void Down ()
    {
      Database.ExecuteNonQuery ($"DROP TABLE IF EXISTS public.{TableName.CNC_DATA_PERIOD_ARCHIVE} CASCADE");
      Database.ExecuteNonQuery ($"DROP TABLE IF EXISTS public.{TableName.CNC_DATA_PERIOD} CASCADE");
      Database.ExecuteNonQuery ($"DROP TABLE IF EXISTS public.{TableName.CNC_DATA_SAMPLE_ARCHIVE} CASCADE");
      Database.ExecuteNonQuery ($"DROP TABLE IF EXISTS public.{TableName.CNC_DATA_SAMPLE} CASCADE");
    }

    /// <summary>
    /// PostgreSQL 18 is required by Atsora Tracking 20, but the virtual columns are only a convenience:
    /// they are not added on an older version (for example a development database), so that the migration does not fail
    /// </summary>
    /// <returns></returns>
    bool IsVirtualColumnSupported ()
    {
      var version = GetPostgresqlVersion ();
      if (version < POSTGRESQL_18) {
        log.Warn ($"IsVirtualColumnSupported: PostgreSQL version {version} < 18 => the virtual range columns are not added. PostgreSQL 18 is required");
        return false;
      }
      return true;
    }

    void AddCncDataSample (bool virtualColumns)
    {
      var t = TableName.CNC_DATA_SAMPLE;
      var rangeColumn = virtualColumns
        ? $"{t}range tsrange GENERATED ALWAYS AS (tsrange ({t}begindatetime, {t}enddatetime, '[]')) VIRTUAL,"
        : "";
      Database.ExecuteNonQuery ($@"
CREATE TABLE public.{t} (
  {ColumnName.MACHINE_MODULE_ID} integer NOT NULL
    REFERENCES public.{TableName.MACHINE_MODULE} ({ColumnName.MACHINE_MODULE_ID}) ON DELETE CASCADE,
  {ColumnName.FIELD_ID} integer NOT NULL
    REFERENCES public.{TableName.FIELD} ({ColumnName.FIELD_ID}) ON DELETE CASCADE,
  {t}begindatetime timestamp(3) without time zone NOT NULL,
  {t}enddatetime timestamp(3) without time zone NOT NULL,
  {rangeColumn}
  {t}deltas smallint[] NOT NULL,
  {t}doublevalues double precision[],
  {t}jsonvalues jsonb,
  CONSTRAINT {t}_begin_end CHECK ({t}begindatetime <= {t}enddatetime),
  CONSTRAINT {t}_max_duration CHECK ({t}enddatetime - {t}begindatetime <= interval '32767 milliseconds'),
  CONSTRAINT {t}_one_value_column CHECK (num_nonnulls ({t}doublevalues, {t}jsonvalues) = 1),
  CONSTRAINT {t}_pkey PRIMARY KEY ({ColumnName.MACHINE_MODULE_ID}, {ColumnName.FIELD_ID}, {t}begindatetime)
) PARTITION BY LIST ({ColumnName.MACHINE_MODULE_ID})");
      Database.ExecuteNonQuery ($@"
COMMENT ON TABLE public.{t} IS
'Time-stamped samples of cnc data (high frequency). One row by batch and by field, the values in arrays.
Partitioned by machine module, then by week. A row never crosses a week bound.'");
      Database.ExecuteNonQuery ($@"
COMMENT ON COLUMN public.{t}.{t}deltas IS 'Time of each sample in ms after {t}begindatetime'");
    }

    void AddCncDataSampleArchive ()
    {
      var t = TableName.CNC_DATA_SAMPLE_ARCHIVE;
      Database.ExecuteNonQuery ($@"
CREATE TABLE public.{t} (
  {ColumnName.MACHINE_MODULE_ID} integer NOT NULL
    REFERENCES public.{TableName.MACHINE_MODULE} ({ColumnName.MACHINE_MODULE_ID}) ON DELETE CASCADE,
  {ColumnName.FIELD_ID} integer NOT NULL
    REFERENCES public.{TableName.FIELD} ({ColumnName.FIELD_ID}) ON DELETE CASCADE,
  {t}begindatetime timestamp(3) without time zone NOT NULL,
  {t}enddatetime timestamp(3) without time zone NOT NULL,
  {t}deltas integer[] NOT NULL,
  {t}intvalues integer[],
  {t}scale double precision,
  {t}doublevalues double precision[],
  {t}jsonvalues jsonb,
  CONSTRAINT {t}_begin_end CHECK ({t}begindatetime <= {t}enddatetime),
  CONSTRAINT {t}_max_duration CHECK ({t}enddatetime - {t}begindatetime <= interval '1 minute'),
  CONSTRAINT {t}_one_value_column CHECK (num_nonnulls ({t}intvalues, {t}doublevalues, {t}jsonvalues) = 1),
  CONSTRAINT {t}_scale CHECK (({t}intvalues IS NULL) OR ({t}scale IS NOT NULL)),
  CONSTRAINT {t}_pkey PRIMARY KEY ({ColumnName.MACHINE_MODULE_ID}, {ColumnName.FIELD_ID}, {t}begindatetime)
) PARTITION BY LIST ({ColumnName.MACHINE_MODULE_ID})");
      SetLz4Compression (t, "deltas", "intvalues", "doublevalues", "jsonvalues");
      Database.ExecuteNonQuery ($@"
COMMENT ON TABLE public.{t} IS
'Archived cncdatasample: batches of at most 1 minute, quantified values (value = intvalue * scale), lz4 compression.
Partitioned by machine module, then by quarter (whole weeks).'");
    }

    void AddCncDataPeriod (bool virtualColumns)
    {
      var t = TableName.CNC_DATA_PERIOD;
      var rangeColumn = virtualColumns
        ? $"{t}range tsrange GENERATED ALWAYS AS (tsrange ({t}begindatetime, {t}enddatetime)) VIRTUAL,"
        : "";
      Database.ExecuteNonQuery ($@"
CREATE TABLE public.{t} (
  {t}id bigint GENERATED BY DEFAULT AS IDENTITY,
  {t}version integer NOT NULL DEFAULT 1,
  {ColumnName.MACHINE_MODULE_ID} integer NOT NULL
    REFERENCES public.{TableName.MACHINE_MODULE} ({ColumnName.MACHINE_MODULE_ID}) ON DELETE CASCADE,
  {ColumnName.FIELD_ID} integer NOT NULL
    REFERENCES public.{TableName.FIELD} ({ColumnName.FIELD_ID}) ON DELETE CASCADE,
  {t}begindatetime timestamp(3) without time zone NOT NULL,
  {t}enddatetime timestamp(3) without time zone NOT NULL,
  {rangeColumn}
  {t}stopped boolean NOT NULL DEFAULT FALSE,
  {t}count integer,
  {t}average double precision,
  {t}min double precision,
  {t}max double precision,
  {t}stddev double precision,
  {t}lastvalue double precision,
  {t}stringvalue text,
  {t}jsonvalue jsonb,
  CONSTRAINT {t}_begin_end CHECK ({t}begindatetime <= {t}enddatetime),
  CONSTRAINT {t}_pkey PRIMARY KEY ({ColumnName.MACHINE_MODULE_ID}, {t}id, {t}begindatetime)
) PARTITION BY LIST ({ColumnName.MACHINE_MODULE_ID})");
      Database.ExecuteNonQuery ($@"
CREATE INDEX {t}_machinemoduleid_fieldid_begindatetime_idx
  ON public.{t} ({ColumnName.MACHINE_MODULE_ID}, {ColumnName.FIELD_ID}, {t}begindatetime)");
      Database.ExecuteNonQuery ($@"
COMMENT ON TABLE public.{t} IS
'Aggregated cnc data on periods, with combinable statistics (count, average, min, max, stddev). To replace cncvalue.
Partitioned by machine module, then by week. A period never crosses a week bound.
No overlap: exclusion constraint in each leaf partition.'");
      Database.ExecuteNonQuery ($@"
COMMENT ON COLUMN public.{t}.{t}stopped IS 'The period was ended by a stop: do not aggregate it with the next one'");
    }

    void AddCncDataPeriodArchive ()
    {
      var t = TableName.CNC_DATA_PERIOD_ARCHIVE;
      Database.ExecuteNonQuery ($@"
CREATE TABLE public.{t} (
  {ColumnName.MACHINE_MODULE_ID} integer NOT NULL
    REFERENCES public.{TableName.MACHINE_MODULE} ({ColumnName.MACHINE_MODULE_ID}) ON DELETE CASCADE,
  {ColumnName.FIELD_ID} integer NOT NULL
    REFERENCES public.{TableName.FIELD} ({ColumnName.FIELD_ID}) ON DELETE CASCADE,
  {t}begindatetime timestamp(3) without time zone NOT NULL,
  {t}enddatetime timestamp(3) without time zone NOT NULL,
  {t}ids bigint[] NOT NULL,
  {t}begindeltas integer[] NOT NULL,
  {t}durations integer[] NOT NULL,
  {t}stoppeds boolean[] NOT NULL,
  {t}counts integer[],
  {t}averages double precision[],
  {t}mins double precision[],
  {t}maxs double precision[],
  {t}stddevs double precision[],
  {t}lastvalues double precision[],
  {t}stringvalues text[],
  {t}jsonvalues jsonb,
  CONSTRAINT {t}_begin_end CHECK ({t}begindatetime <= {t}enddatetime),
  CONSTRAINT {t}_max_duration CHECK ({t}enddatetime - {t}begindatetime <= interval '7 days'),
  CONSTRAINT {t}_pkey PRIMARY KEY ({ColumnName.MACHINE_MODULE_ID}, {ColumnName.FIELD_ID}, {t}begindatetime)
) PARTITION BY LIST ({ColumnName.MACHINE_MODULE_ID})");
      SetLz4Compression (t, "ids", "begindeltas", "durations", "stoppeds", "counts", "averages", "mins", "maxs",
        "stddevs", "lastvalues", "stringvalues", "jsonvalues");
      Database.ExecuteNonQuery ($@"
COMMENT ON TABLE public.{t} IS
'Archived cncdataperiod, without any loss: the periods of a field (at most one week) in arrays, lz4 compression.
Partitioned by machine module, then by quarter (whole weeks).'");
    }

    void SetLz4Compression (string table, params string[] columnSuffixes)
    {
      foreach (var columnSuffix in columnSuffixes) {
        Database.ExecuteNonQuery ($"ALTER TABLE public.{table} ALTER COLUMN {table}{columnSuffix} SET COMPRESSION lz4");
      }
    }
  }
}
