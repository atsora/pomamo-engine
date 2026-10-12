// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="ICncDataPartitionDAO"/>
  /// with the PostgreSQL functions of migration 2001 (cncdatacreatepartition)
  /// </summary>
  public class CncDataPartitionDAO : ICncDataPartitionDAO
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CncDataPartitionDAO).FullName);

    // Error raised by PostgreSQL when no partition matches a row:
    // check_violation in routine ExecFindPartition (independent of the language of the server messages)
    static readonly string MISSING_PARTITION_SQL_STATE = "23514";
    static readonly string MISSING_PARTITION_ROUTINE = "ExecFindPartition";

    /// <summary>
    /// <see cref="ICncDataPartitionDAO"/>
    /// </summary>
    /// <returns></returns>
    public int GetWeekCutOffDay ()
    {
      return (int)Lemoine.Info.ConfigSet
        .LoadAndGet<DayOfWeek> (ConfigKeys.GetCncConfigKey (CncConfigKey.DataPartitionWeekCutOffDay), DayOfWeek.Sunday);
    }

    /// <summary>
    /// <see cref="ICncDataPartitionDAO"/>
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public bool IsMissingPartition (Exception ex)
    {
      for (var e = ex; e is not null; e = e.InnerException) {
        if (e is Npgsql.PostgresException postgresException) {
          return string.Equals (postgresException.SqlState, MISSING_PARTITION_SQL_STATE, StringComparison.InvariantCulture)
            && string.Equals (postgresException.Routine, MISSING_PARTITION_ROUTINE, StringComparison.InvariantCulture);
        }
      }
      return false;
    }

    /// <summary>
    /// <see cref="ICncDataPartitionDAO"/>
    /// </summary>
    /// <param name="table"></param>
    /// <param name="machineModuleId"></param>
    /// <param name="dateTime">UTC date/time</param>
    public void CreatePartition (CncDataTable table, int machineModuleId, DateTime dateTime)
    {
      ExecuteInTransaction ("CncDataPartition.CreatePartition",
        "SELECT public.cncdatacreatepartition (@table, @machinemoduleid, @datetime, @cutoffday)",
        command => {
          AddParameter (command, "table", DbType.String, GetTableName (table));
          AddParameter (command, "machinemoduleid", DbType.Int32, machineModuleId);
          AddParameter (command, "datetime", DbType.DateTime, ToTimestamp (dateTime));
          AddParameter (command, "cutoffday", DbType.Int32, GetWeekCutOffDay ());
        });
      if (log.IsDebugEnabled) {
        log.Debug ($"CreatePartition: partition of {table} for machine module {machineModuleId} at {dateTime} checked");
      }
    }

    /// <summary>
    /// <see cref="ICncDataPartitionDAO"/>
    /// </summary>
    /// <param name="machineModuleId"></param>
    /// <param name="fromDateTime">UTC date/time</param>
    /// <param name="days"></param>
    public void CreateWeekPartitions (int machineModuleId, DateTime fromDateTime, int days)
    {
      ExecuteInTransaction ("CncDataPartition.CreateWeekPartitions", $@"
SELECT public.cncdatacreatepartition (t, @machinemoduleid, d, @cutoffday)
  FROM unnest (ARRAY['{GetTableName (CncDataTable.Sample)}', '{GetTableName (CncDataTable.Period)}']) AS t,
       generate_series (date_trunc ('day', @from), date_trunc ('day', @from) + make_interval (days => @days), interval '1 day') AS d",
        command => {
          AddParameter (command, "machinemoduleid", DbType.Int32, machineModuleId);
          AddParameter (command, "cutoffday", DbType.Int32, GetWeekCutOffDay ());
          AddParameter (command, "from", DbType.DateTime, ToTimestamp (fromDateTime));
          AddParameter (command, "days", DbType.Int32, days);
        });
    }

    /// <summary>
    /// <see cref="ICncDataPartitionDAO"/>
    /// </summary>
    /// <param name="table"></param>
    /// <param name="machineModuleId"></param>
    /// <param name="dateTime">UTC date/time</param>
    public void EnsurePartition (CncDataTable table, int machineModuleId, DateTime dateTime)
    {
      var timestamp = ToTimestamp (dateTime);
      if (s_knownPartitions.TryGetValue ((table, machineModuleId), out var knownPartitions)
        && knownPartitions.Exists (p => (p.Lower <= timestamp) && (timestamp < p.Upper))) {
        return;
      }

      using (var command = CreateCommand ()) {
        // createdhere: the partition was created (attached) by the current transaction,
        // that may be rolled back => not cached
        command.CommandText = @"
SELECT p.lowerbound, p.upperbound,
       coalesce (c.xmin::text = (pg_current_xact_id_if_assigned ()::text::bigint % 4294967296)::text, FALSE) AS createdhere
  FROM public.cncdatapartitions p
  JOIN pg_catalog.pg_class c ON c.oid = to_regclass (format ('public.%I', p.partitionname))
 WHERE p.tablename = @table AND p.machinemoduleid = @machinemoduleid
   AND p.lowerbound <= @datetime AND @datetime < p.upperbound";
        AddParameter (command, "table", DbType.String, GetTableName (table));
        AddParameter (command, "machinemoduleid", DbType.Int32, machineModuleId);
        AddParameter (command, "datetime", DbType.DateTime, timestamp);
        using (var reader = command.ExecuteReader ()) {
          if (reader.Read ()) {
            if (reader.GetBoolean (2)) {
              return;
            }
            var partition = (Lower: reader.GetDateTime (0), Upper: reader.GetDateTime (1));
            s_knownPartitions.AddOrUpdate ((table, machineModuleId),
              _ => new List<(DateTime Lower, DateTime Upper)> { partition },
              (_, l) => new List<(DateTime Lower, DateTime Upper)> (l) { partition });
            return;
          }
        }
      }

      // Not cached: the creation may be rolled back with the current transaction
      log.Warn ($"EnsurePartition: missing partition of {table} for machine module {machineModuleId} at {dateTime} => create it in the current transaction");
      using (var command = CreateCommand ()) {
        command.CommandText = "SELECT public.cncdatacreatepartition (@table, @machinemoduleid, @datetime, @cutoffday)";
        AddParameter (command, "table", DbType.String, GetTableName (table));
        AddParameter (command, "machinemoduleid", DbType.Int32, machineModuleId);
        AddParameter (command, "datetime", DbType.DateTime, timestamp);
        AddParameter (command, "cutoffday", DbType.Int32, GetWeekCutOffDay ());
        command.ExecuteNonQuery ();
      }
    }

    // Known partitions by table and machine module, bounds as timestamps without time zone
    static readonly ConcurrentDictionary<(CncDataTable, int), List<(DateTime Lower, DateTime Upper)>> s_knownPartitions = new ();

    /// <summary>
    /// Forget the known partitions of a table and a machine module
    /// (after a partition is dropped or when a partition is unexpectedly missing)
    /// </summary>
    /// <param name="table"></param>
    /// <param name="machineModuleId"></param>
    internal static void ForgetPartitions (CncDataTable table, int machineModuleId)
    {
      s_knownPartitions.TryRemove ((table, machineModuleId), out _);
    }

    /// <summary>
    /// <see cref="ICncDataPartitionDAO"/>
    /// </summary>
    public T RunWithPartition<T> (CncDataTable table, int machineModuleId, DateTime dateTime, Func<T> unitOfWork)
    {
      try {
        return unitOfWork ();
      }
      catch (Exception ex) when (IsMissingPartition (ex)) {
        log.Warn ($"RunWithPartition: missing partition of {table} for machine module {machineModuleId} at {dateTime} => create it and try again", ex);
        CreatePartition (table, machineModuleId, dateTime);
        return unitOfWork ();
      }
    }

    /// <summary>
    /// <see cref="ICncDataPartitionDAO"/>
    /// </summary>
    public void RunWithPartition (CncDataTable table, int machineModuleId, DateTime dateTime, Action unitOfWork)
    {
      RunWithPartition<bool> (table, machineModuleId, dateTime, () => { unitOfWork (); return true; });
    }

    /// <summary>
    /// <see cref="ICncDataPartitionDAO"/>
    /// </summary>
    public IList<ICncDataPartition> FindPartitions (CncDataTable table)
    {
      var partitions = new List<ICncDataPartition> ();
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginReadOnlyTransaction ("CncDataPartition.FindPartitions")) {
        using (var command = CreateCommand ()) {
          command.CommandText = @"
SELECT machinemoduleid, partitionname, lowerbound, upperbound
  FROM public.cncdatapartitions
 WHERE tablename = @table
 ORDER BY lowerbound, machinemoduleid";
          AddParameter (command, "table", DbType.String, GetTableName (table));
          using (var reader = command.ExecuteReader ()) {
            while (reader.Read ()) {
              partitions.Add (new CncDataPartition (table, reader.GetInt32 (0), reader.GetString (1),
                DateTime.SpecifyKind (reader.GetDateTime (2), DateTimeKind.Utc),
                DateTime.SpecifyKind (reader.GetDateTime (3), DateTimeKind.Utc)));
            }
          }
        }
        transaction.Commit ();
      }
      return partitions;
    }

    /// <summary>
    /// <see cref="ICncDataPartitionDAO"/>
    /// </summary>
    public long ArchivePartition (ICncDataPartition partition, TimeSpan maxDuration)
    {
      var (archiveTable, archiveSql, countSql, archivedCountSql) = partition.Table switch {
        CncDataTable.Sample => (CncDataTable.SampleArchive, GetSampleArchiveSql (partition.Name), GetSampleCountSql (partition.Name), SAMPLE_ARCHIVED_COUNT_SQL),
        CncDataTable.Period => (CncDataTable.PeriodArchive, GetPeriodArchiveSql (partition.Name), GetPeriodCountSql (partition.Name), PERIOD_ARCHIVED_COUNT_SQL),
        _ => throw new ArgumentException ($"Partition {partition.Name} of {partition.Table} can't be archived", nameof (partition)),
      };
      var deadline = DateTime.UtcNow.Add (maxDuration);

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ("CncDataPartition.ArchivePartition")) {
        // Archive partitions (quarters) that contain the week
        Execute (deadline, "SELECT public.cncdatacreatepartition (@table, @machinemoduleid, @lower, @cutoffday), public.cncdatacreatepartition (@table, @machinemoduleid, @last, @cutoffday)",
          command => {
            AddParameter (command, "table", DbType.String, GetTableName (archiveTable));
            AddParameter (command, "machinemoduleid", DbType.Int32, partition.MachineModuleId);
            AddParameter (command, "lower", DbType.DateTime, ToTimestamp (partition.LowerBound));
            AddParameter (command, "last", DbType.DateTime, ToTimestamp (partition.UpperBound.AddMilliseconds (-1)));
            AddParameter (command, "cutoffday", DbType.Int32, GetWeekCutOffDay ());
          });

        var count = Scalar (deadline, countSql, command => { });
        Execute (deadline, archiveSql, command => { });
        var archivedCount = Scalar (deadline, archivedCountSql,
          command => {
            AddParameter (command, "machinemoduleid", DbType.Int32, partition.MachineModuleId);
            AddParameter (command, "lower", DbType.DateTime, ToTimestamp (partition.LowerBound));
            AddParameter (command, "upper", DbType.DateTime, ToTimestamp (partition.UpperBound));
          });
        if (count != archivedCount) {
          log.Error ($"ArchivePartition: {archivedCount} archived elements instead of {count} for partition {partition.Name} => rollback");
          throw new InvalidOperationException ($"Archive of partition {partition.Name}: {archivedCount} archived elements instead of {count}");
        }

        Execute (deadline, $"DROP TABLE public.{QuoteIdentifier (partition.Name)}", command => { });
        transaction.Commit ();
        ForgetPartitions (partition.Table, partition.MachineModuleId);
        if (log.IsInfoEnabled) {
          log.Info ($"ArchivePartition: {count} elements of partition {partition.Name} archived");
        }
        return count;
      }
    }

    /// <summary>
    /// <see cref="ICncDataPartitionDAO"/>
    /// </summary>
    public void DropPartition (ICncDataPartition partition, TimeSpan maxDuration)
    {
      var deadline = DateTime.UtcNow.Add (maxDuration);
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ("CncDataPartition.DropPartition")) {
        Execute (deadline, $"DROP TABLE public.{QuoteIdentifier (partition.Name)}", command => { });
        transaction.Commit ();
      }
      ForgetPartitions (partition.Table, partition.MachineModuleId);
      if (log.IsInfoEnabled) {
        log.Info ($"DropPartition: partition {partition.Name} dropped");
      }
    }

    static readonly TimeSpan LOCK_TIMEOUT = TimeSpan.FromMinutes (1);

    // Samples: regrouped by field and by minute of their own date/time, so that an archive row lasts less than 1 minute.
    // Note: the values could be quantified here (integer values and a scale per field, columns intvalues and scale)
    // to compress them better, once a precision is defined by field
    static string GetSampleArchiveSql (string partitionName) => $@"
WITH s AS (
  SELECT h.machinemoduleid, h.fieldid,
         h.cncdatasamplebegindatetime + u.delta * interval '1 millisecond' AS t,
         u.doublevalue, u.jsonvalue
    FROM public.{QuoteIdentifier (partitionName)} h
    CROSS JOIN LATERAL ROWS FROM (unnest (h.cncdatasampledeltas),
                                  unnest (h.cncdatasampledoublevalues),
                                  jsonb_array_elements (h.cncdatasamplejsonvalues)) AS u (delta, doublevalue, jsonvalue)
), b AS (
  SELECT s.*, date_trunc ('minute', s.t) AS bucket,
         min (s.t) OVER (PARTITION BY s.machinemoduleid, s.fieldid, date_trunc ('minute', s.t)) AS first
    FROM s
)
INSERT INTO public.cncdatasamplearchive (machinemoduleid, fieldid,
  cncdatasamplearchivebegindatetime, cncdatasamplearchiveenddatetime, cncdatasamplearchivedeltas,
  cncdatasamplearchivedoublevalues, cncdatasamplearchivejsonvalues)
SELECT b.machinemoduleid, b.fieldid, b.first, max (b.t),
       array_agg (round (extract (epoch FROM (b.t - b.first)) * 1000)::integer ORDER BY b.t),
       CASE WHEN count (b.doublevalue) > 0 THEN array_agg (b.doublevalue ORDER BY b.t) END,
       CASE WHEN count (b.jsonvalue) > 0 THEN jsonb_agg (b.jsonvalue ORDER BY b.t) END
  FROM b
 GROUP BY b.machinemoduleid, b.fieldid, b.bucket, b.first";

    static string GetSampleCountSql (string partitionName) =>
      $"SELECT coalesce (sum (cardinality (cncdatasampledeltas)), 0) FROM public.{QuoteIdentifier (partitionName)}";

    static readonly string SAMPLE_ARCHIVED_COUNT_SQL = @"
SELECT coalesce (sum (cardinality (cncdatasamplearchivedeltas)), 0)
  FROM public.cncdatasamplearchive
 WHERE machinemoduleid = @machinemoduleid
   AND cncdatasamplearchivebegindatetime >= @lower AND cncdatasamplearchivebegindatetime < @upper";

    // Periods: regrouped by field and by day of their begin, without any loss (ids, stopped and statistics are kept)
    static string GetPeriodArchiveSql (string partitionName) => $@"
WITH p AS (
  SELECT h.*, date_trunc ('day', h.cncdataperiodbegindatetime) AS bucket,
         min (h.cncdataperiodbegindatetime) OVER (PARTITION BY h.machinemoduleid, h.fieldid, date_trunc ('day', h.cncdataperiodbegindatetime)) AS first
    FROM public.{QuoteIdentifier (partitionName)} h
)
INSERT INTO public.cncdataperiodarchive (machinemoduleid, fieldid,
  cncdataperiodarchivebegindatetime, cncdataperiodarchiveenddatetime,
  cncdataperiodarchiveids, cncdataperiodarchivebegindeltas, cncdataperiodarchivedurations, cncdataperiodarchivestoppeds,
  cncdataperiodarchivecounts, cncdataperiodarchiveaverages, cncdataperiodarchivemins, cncdataperiodarchivemaxs,
  cncdataperiodarchivestddevs, cncdataperiodarchivelastvalues, cncdataperiodarchivestringvalues, cncdataperiodarchivejsonvalues)
SELECT p.machinemoduleid, p.fieldid, p.first, max (p.cncdataperiodenddatetime),
       array_agg (p.cncdataperiodid ORDER BY p.cncdataperiodbegindatetime),
       array_agg (round (extract (epoch FROM (p.cncdataperiodbegindatetime - p.first)) * 1000)::integer ORDER BY p.cncdataperiodbegindatetime),
       array_agg (round (extract (epoch FROM (p.cncdataperiodenddatetime - p.cncdataperiodbegindatetime)) * 1000)::integer ORDER BY p.cncdataperiodbegindatetime),
       array_agg (p.cncdataperiodstopped ORDER BY p.cncdataperiodbegindatetime),
       CASE WHEN count (p.cncdataperiodcount) > 0 THEN array_agg (p.cncdataperiodcount ORDER BY p.cncdataperiodbegindatetime) END,
       CASE WHEN count (p.cncdataperiodaverage) > 0 THEN array_agg (p.cncdataperiodaverage ORDER BY p.cncdataperiodbegindatetime) END,
       CASE WHEN count (p.cncdataperiodmin) > 0 THEN array_agg (p.cncdataperiodmin ORDER BY p.cncdataperiodbegindatetime) END,
       CASE WHEN count (p.cncdataperiodmax) > 0 THEN array_agg (p.cncdataperiodmax ORDER BY p.cncdataperiodbegindatetime) END,
       CASE WHEN count (p.cncdataperiodstddev) > 0 THEN array_agg (p.cncdataperiodstddev ORDER BY p.cncdataperiodbegindatetime) END,
       CASE WHEN count (p.cncdataperiodlastvalue) > 0 THEN array_agg (p.cncdataperiodlastvalue ORDER BY p.cncdataperiodbegindatetime) END,
       CASE WHEN count (p.cncdataperiodstringvalue) > 0 THEN array_agg (p.cncdataperiodstringvalue ORDER BY p.cncdataperiodbegindatetime) END,
       CASE WHEN count (p.cncdataperiodjsonvalue) > 0 THEN jsonb_agg (p.cncdataperiodjsonvalue ORDER BY p.cncdataperiodbegindatetime) END
  FROM p
 GROUP BY p.machinemoduleid, p.fieldid, p.bucket, p.first";

    static string GetPeriodCountSql (string partitionName) =>
      $"SELECT count (*) FROM public.{QuoteIdentifier (partitionName)}";

    static readonly string PERIOD_ARCHIVED_COUNT_SQL = @"
SELECT coalesce (sum (cardinality (cncdataperiodarchiveids)), 0)
  FROM public.cncdataperiodarchive
 WHERE machinemoduleid = @machinemoduleid
   AND cncdataperiodarchivebegindatetime >= @lower AND cncdataperiodarchivebegindatetime < @upper";

    static string QuoteIdentifier (string identifier) => "\"" + identifier.Replace ("\"", "\"\"") + "\"";

    internal static IDbCommand CreateCommand () => NHibernateHelper.GetCurrentSession ().Connection.CreateCommand ();

    /// <summary>
    /// Limit the next statement to the time left before the deadline (and the time to wait for a lock)
    /// </summary>
    static void SetTimeouts (DateTime deadline)
    {
      var left = deadline.Subtract (DateTime.UtcNow);
      if (left <= TimeSpan.Zero) {
        throw new TimeoutException ("No time left before the deadline");
      }
      var statementTimeout = (long)Math.Max (1, left.TotalMilliseconds);
      var lockTimeout = (long)Math.Max (1, Math.Min (left.TotalMilliseconds, LOCK_TIMEOUT.TotalMilliseconds));
      using (var command = CreateCommand ()) {
        command.CommandText = $"SET LOCAL statement_timeout = {statementTimeout}; SET LOCAL lock_timeout = {lockTimeout}";
        command.ExecuteNonQuery ();
      }
    }

    static void Execute (DateTime deadline, string sql, Action<IDbCommand> addParameters)
    {
      SetTimeouts (deadline);
      using (var command = CreateCommand ()) {
        command.CommandText = sql;
        addParameters (command);
        command.ExecuteNonQuery ();
      }
    }

    static long Scalar (DateTime deadline, string sql, Action<IDbCommand> addParameters)
    {
      SetTimeouts (deadline);
      using (var command = CreateCommand ()) {
        command.CommandText = sql;
        addParameters (command);
        return Convert.ToInt64 (command.ExecuteScalar (), System.Globalization.CultureInfo.InvariantCulture);
      }
    }

    static string GetTableName (CncDataTable table) => table switch {
      CncDataTable.Sample => "cncdatasample",
      CncDataTable.Period => "cncdataperiod",
      CncDataTable.SampleArchive => "cncdatasamplearchive",
      CncDataTable.PeriodArchive => "cncdataperiodarchive",
      _ => throw new ArgumentOutOfRangeException (nameof (table), table, "Unknown cncdata table"),
    };

    /// <summary>
    /// UTC date/time as a timestamp without time zone (Kind Unspecified, so that no time zone conversion is made)
    /// </summary>
    internal static DateTime ToTimestamp (DateTime dateTime)
    {
      var utc = (dateTime.Kind == DateTimeKind.Local) ? dateTime.ToUniversalTime () : dateTime;
      return DateTime.SpecifyKind (utc, DateTimeKind.Unspecified);
    }

    internal static void AddParameter (IDbCommand command, string name, DbType dbType, object value)
    {
      var parameter = command.CreateParameter ();
      parameter.ParameterName = name;
      parameter.DbType = dbType;
      parameter.Value = value;
      command.Parameters.Add (parameter);
    }

    static void ExecuteInTransaction (string transactionName, string sql, Action<IDbCommand> addParameters)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction (transactionName)) {
        using (var command = CreateCommand ()) {
          command.CommandText = sql;
          addParameters (command);
          command.ExecuteNonQuery ();
        }
        transaction.Commit ();
      }
    }
  }

  /// <summary>
  /// Implementation of <see cref="ICncDataPartition"/>
  /// </summary>
  internal class CncDataPartition : ICncDataPartition
  {
    public CncDataTable Table { get; }
    public int MachineModuleId { get; }
    public string Name { get; }
    public DateTime LowerBound { get; }
    public DateTime UpperBound { get; }

    public CncDataPartition (CncDataTable table, int machineModuleId, string name, DateTime lowerBound, DateTime upperBound)
    {
      this.Table = table;
      this.MachineModuleId = machineModuleId;
      this.Name = name;
      this.LowerBound = lowerBound;
      this.UpperBound = upperBound;
    }

    public override string ToString () => $"{this.Name} [{this.LowerBound:o}, {this.UpperBound:o})";
  }
}
