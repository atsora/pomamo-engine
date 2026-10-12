// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NpgsqlTypes;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="ICncDataSampleDAO"/>
  /// </summary>
  public class CncDataSampleDAO : ICncDataSampleDAO
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CncDataSampleDAO).FullName);

    /// <summary>
    /// Maximum duration of a row (smallint deltas in ms)
    /// </summary>
    static readonly int MAX_ROW_DURATION_MS = short.MaxValue;

    /// <summary>
    /// Upper bound of the requests when the range has no upper bound
    /// </summary>
    static readonly DateTime NO_UPPER_BOUND = new DateTime (9999, 12, 31, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Row of table cncdatasample
    /// </summary>
    internal sealed class SampleRow
    {
      public int FieldId { get; }
      public DateTime Begin { get; }
      public DateTime End { get; private set; }
      public bool IsJson { get; }
      public List<short> Deltas { get; } = new List<short> ();
      public List<double> DoubleValues { get; } = new List<double> ();
      public List<string> JsonValues { get; } = new List<string> ();

      public SampleRow (int fieldId, CncDataSampleValue first)
      {
        this.FieldId = fieldId;
        this.Begin = first.DateTime;
        this.IsJson = !first.DoubleValue.HasValue;
        Add (first);
      }

      public void Add (CncDataSampleValue value)
      {
        this.Deltas.Add ((short)(value.DateTime - this.Begin).TotalMilliseconds);
        if (this.IsJson) {
          this.JsonValues.Add (value.JsonValue);
        }
        else {
          this.DoubleValues.Add (value.DoubleValue.Value);
        }
        this.End = value.DateTime;
      }
    }

    /// <summary>
    /// Split the values of a field into rows:
    /// at most <see cref="MAX_ROW_DURATION_MS"/> ms, never across a week bound, one kind of value by row
    /// </summary>
    /// <param name="fieldId"></param>
    /// <param name="values"></param>
    /// <param name="cutOffDay"></param>
    /// <returns></returns>
    internal static IEnumerable<SampleRow> Pack (int fieldId, IEnumerable<CncDataSampleValue> values, DayOfWeek cutOffDay)
    {
      SampleRow row = null;
      DateTime weekEnd = DateTime.MinValue;
      foreach (var value in values.OrderBy (v => v.DateTime)) {
        if ((null != row)
          && (row.IsJson == !value.DoubleValue.HasValue)
          && (value.DateTime < weekEnd)
          && ((value.DateTime - row.Begin).TotalMilliseconds <= MAX_ROW_DURATION_MS)) {
          row.Add (value);
        }
        else {
          if (null != row) {
            yield return row;
          }
          row = new SampleRow (fieldId, value);
          weekEnd = CncDataWeek.GetWeekEnd (value.DateTime, cutOffDay);
        }
      }
      if (null != row) {
        yield return row;
      }
    }

    /// <summary>
    /// <see cref="ICncDataSampleDAO"/>
    /// </summary>
    public int Insert (IMachineModule machineModule, IEnumerable<CncDataSampleSeries> series)
    {
      return Insert (machineModule, series, false);
    }

    /// <summary>
    /// <see cref="ICncDataSampleDAO"/>
    /// </summary>
    public int InsertIgnoringDuplicates (IMachineModule machineModule, IEnumerable<CncDataSampleSeries> series)
    {
      return Insert (machineModule, series, true);
    }

    static readonly string COLUMNS = @"machinemoduleid, fieldid, cncdatasamplebegindatetime, cncdatasampleenddatetime,
  cncdatasampledeltas, cncdatasampledoublevalues, cncdatasamplejsonvalues";

    int Insert (IMachineModule machineModule, IEnumerable<CncDataSampleSeries> series, bool ignoreDuplicates)
    {
      Debug.Assert (null != machineModule);
      Debug.Assert (null != series);

      var partitionDAO = ModelDAOHelper.DAOFactory.CncDataPartitionDAO;
      var cutOffDay = (DayOfWeek)partitionDAO.GetWeekCutOffDay ();
      var rows = series
        .SelectMany (s => Pack (s.Field.Id, s.Values, cutOffDay))
        .ToList ();
      if (0 == rows.Count) {
        return 0;
      }

      foreach (var weekRow in rows.GroupBy (r => CncDataWeek.GetWeekStart (r.Begin, cutOffDay)).Select (g => g.First ())) {
        partitionDAO.EnsurePartition (CncDataTable.Sample, machineModule.Id, weekRow.Begin);
      }

      if (!(NHibernateHelper.GetCurrentSession ().Connection is Npgsql.NpgsqlConnection connection)) {
        log.Fatal ("Insert: the connection is not a Npgsql connection");
        throw new InvalidOperationException ("Not a Npgsql connection");
      }
      var insertedRows = rows.Count;
      try {
        string table;
        if (ignoreDuplicates) {
          table = "pg_temp.cncdatasamplereplay";
          using (var command = connection.CreateCommand ()) {
            command.CommandText = $@"
CREATE TEMPORARY TABLE IF NOT EXISTS cncdatasamplereplay (
  machinemoduleid integer NOT NULL,
  fieldid integer NOT NULL,
  cncdatasamplebegindatetime timestamp(3) without time zone NOT NULL,
  cncdatasampleenddatetime timestamp(3) without time zone NOT NULL,
  cncdatasampledeltas smallint[] NOT NULL,
  cncdatasampledoublevalues double precision[],
  cncdatasamplejsonvalues jsonb
) ON COMMIT DELETE ROWS;
TRUNCATE pg_temp.cncdatasamplereplay";
            command.ExecuteNonQuery ();
          }
        }
        else {
          table = "public.cncdatasample";
        }
        using (var writer = connection.BeginBinaryImport ($"COPY {table} ({COLUMNS}) FROM STDIN (FORMAT BINARY)")) {
          foreach (var row in rows) {
            writer.StartRow ();
            writer.Write (machineModule.Id, NpgsqlDbType.Integer);
            writer.Write (row.FieldId, NpgsqlDbType.Integer);
            writer.Write (CncDataPartitionDAO.ToTimestamp (row.Begin), NpgsqlDbType.Timestamp);
            writer.Write (CncDataPartitionDAO.ToTimestamp (row.End), NpgsqlDbType.Timestamp);
            writer.Write (row.Deltas.ToArray (), NpgsqlDbType.Array | NpgsqlDbType.Smallint);
            if (row.IsJson) {
              writer.WriteNull ();
              writer.Write ("[" + string.Join (",", row.JsonValues) + "]", NpgsqlDbType.Jsonb);
            }
            else {
              writer.Write (row.DoubleValues.ToArray (), NpgsqlDbType.Array | NpgsqlDbType.Double);
              writer.WriteNull ();
            }
          }
          writer.Complete ();
        }
        if (ignoreDuplicates) {
          using (var command = connection.CreateCommand ()) {
            command.CommandText = $@"
INSERT INTO public.cncdatasample ({COLUMNS})
SELECT {COLUMNS} FROM pg_temp.cncdatasamplereplay
ON CONFLICT DO NOTHING";
            insertedRows = command.ExecuteNonQuery ();
          }
          if (insertedRows < rows.Count) {
            log.Info ($"Insert: {rows.Count - insertedRows} rows of machine module {machineModule.Id} were already inserted");
          }
        }
      }
      catch (Exception ex) when (partitionDAO.IsMissingPartition (ex)) {
        log.Error ($"Insert: unexpected missing partition for machine module {machineModule.Id}", ex);
        CncDataPartitionDAO.ForgetPartitions (CncDataTable.Sample, machineModule.Id);
        throw;
      }
      if (log.IsDebugEnabled) {
        log.Debug ($"Insert: {insertedRows} rows inserted for machine module {machineModule.Id}");
      }
      return insertedRows;
    }

    /// <summary>
    /// <see cref="ICncDataSampleDAO"/>
    /// </summary>
    public IList<CncDataSampleValue> FindInRange (IMachineModule machineModule, IField field, UtcDateTimeRange range)
    {
      return Find (machineModule, field, range, null);
    }

    /// <summary>
    /// <see cref="ICncDataSampleDAO"/>
    /// </summary>
    public IList<CncDataSampleValue> FindInRange (IMachineModule machineModule, IField field, UtcDateTimeRange range, int maxCount)
    {
      if (maxCount <= 0) {
        throw new ArgumentOutOfRangeException (nameof (maxCount), maxCount, "The maximum number of samples must be strictly positive");
      }
      return Find (machineModule, field, range, maxCount);
    }

    IList<CncDataSampleValue> Find (IMachineModule machineModule, IField field, UtcDateTimeRange range, int? maxCount)
    {
      Debug.Assert (null != machineModule);
      Debug.Assert (null != field);

      var result = new List<CncDataSampleValue> ();
      if (range.IsEmpty ()) {
        return result;
      }
      var (from, to) = GetFromTo (range);
      using (var command = CncDataPartitionDAO.CreateCommand ()) {
        command.CommandText = @"
SELECT datetime, doublevalue, jsonvalue
  FROM public.cncdatasamplerange (@machinemoduleid, @fieldid, @from, @to)"
          + (maxCount.HasValue ? " LIMIT @limit" : "");
        CncDataPartitionDAO.AddParameter (command, "machinemoduleid", DbType.Int32, machineModule.Id);
        CncDataPartitionDAO.AddParameter (command, "fieldid", DbType.Int32, field.Id);
        CncDataPartitionDAO.AddParameter (command, "from", DbType.DateTime, CncDataPartitionDAO.ToTimestamp (from));
        CncDataPartitionDAO.AddParameter (command, "to", DbType.DateTime, CncDataPartitionDAO.ToTimestamp (to));
        if (maxCount.HasValue) {
          CncDataPartitionDAO.AddParameter (command, "limit", DbType.Int32, maxCount.Value);
        }
        using (var reader = command.ExecuteReader ()) {
          while (reader.Read ()) {
            result.Add (ReadValue (reader));
          }
        }
      }
      return result;
    }

    /// <summary>
    /// <see cref="ICncDataSampleDAO"/>
    /// </summary>
    public CncDataSampleValue? FindLast (IMachineModule machineModule, IField field)
    {
      Debug.Assert (null != machineModule);
      Debug.Assert (null != field);

      using (var command = CncDataPartitionDAO.CreateCommand ()) {
        // The deltas are ordered: the last element of the last row is the last sample
        command.CommandText = @"
SELECT s.cncdatasamplebegindatetime + s.cncdatasampledeltas[cardinality (s.cncdatasampledeltas)] * interval '1 millisecond',
       s.cncdatasampledoublevalues[cardinality (s.cncdatasampledeltas)],
       s.cncdatasamplejsonvalues -> (cardinality (s.cncdatasampledeltas) - 1)
  FROM public.cncdatasample s
 WHERE s.machinemoduleid = @machinemoduleid AND s.fieldid = @fieldid
 ORDER BY s.cncdatasamplebegindatetime DESC
 LIMIT 1";
        CncDataPartitionDAO.AddParameter (command, "machinemoduleid", DbType.Int32, machineModule.Id);
        CncDataPartitionDAO.AddParameter (command, "fieldid", DbType.Int32, field.Id);
        using (var reader = command.ExecuteReader ()) {
          if (reader.Read ()) {
            return ReadValue (reader);
          }
          else {
            return null;
          }
        }
      }
    }

    static CncDataSampleValue ReadValue (IDataReader reader)
    {
      var dateTime = DateTime.SpecifyKind (reader.GetDateTime (0), DateTimeKind.Utc);
      return reader.IsDBNull (1)
        ? new CncDataSampleValue (dateTime, reader.GetString (2))
        : new CncDataSampleValue (dateTime, reader.GetDouble (1));
    }

    /// <summary>
    /// Convert a range into the [from, to) parameters of the functions cncdatasamplerange and cncdataperiodrange
    /// </summary>
    /// <param name="range">with a lower bound</param>
    /// <returns></returns>
    internal static (DateTime from, DateTime to) GetFromTo (UtcDateTimeRange range)
    {
      if (!range.Lower.HasValue) {
        log.Error ($"GetFromTo: range {range} without any lower bound");
        throw new ArgumentException ("The range must have a lower bound", nameof (range));
      }
      var from = CncDataWeek.TruncateToMilliseconds (range.Lower.Value);
      if (!range.LowerInclusive) {
        from = from.AddMilliseconds (1);
      }
      DateTime to;
      if (range.Upper.HasValue) {
        to = CncDataWeek.TruncateToMilliseconds (range.Upper.Value);
        if (range.UpperInclusive) {
          to = to.AddMilliseconds (1);
        }
      }
      else {
        to = NO_UPPER_BOUND;
      }
      return (from, to);
    }
  }
}
