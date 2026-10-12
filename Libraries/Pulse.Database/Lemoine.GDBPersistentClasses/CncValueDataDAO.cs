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

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="ICncValueDataDAO"/> with the functions cncvalueat and cncvaluerange (migration 2004)
  ///
  /// The SQL functions return a few more values (they include the values that end at the requested date/time):
  /// the exact semantics of <see cref="ICncValueDAO"/> are applied here
  /// </summary>
  public class CncValueDataDAO : ICncValueDataDAO
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CncValueDataDAO).FullName);

    /// <summary>
    /// Bounds of the requests when a range has no bound
    /// </summary>
    static readonly DateTime NO_LOWER_BOUND = new DateTime (1900, 01, 01, 0, 0, 0, DateTimeKind.Utc);
    static readonly DateTime NO_UPPER_BOUND = new DateTime (9999, 12, 31, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// <see cref="ICncValueDataDAO"/>
    /// </summary>
    public IList<ICncValueData> FindAt (IMachineModule machineModule, DateTime at)
    {
      Debug.Assert (null != machineModule);

      var fields = new Dictionary<int, IField> ();
      return Read ("SELECT * FROM public.cncvalueat (@machinemoduleid, @at)",
        command => {
          CncDataPartitionDAO.AddParameter (command, "machinemoduleid", DbType.Int32, machineModule.Id);
          CncDataPartitionDAO.AddParameter (command, "at", DbType.DateTime, CncDataPartitionDAO.ToTimestamp (at));
        },
        machineModule,
        fieldId => {
          if (!fields.TryGetValue (fieldId, out var field)) {
            field = ModelDAOHelper.DAOFactory.FieldDAO.FindById (fieldId);
            fields[fieldId] = field;
          }
          return field;
        })
        .Where (v => IsAt (v, at))
        .ToList ();
    }

    /// <summary>
    /// <see cref="ICncValueDataDAO"/>
    /// </summary>
    public ICncValueData FindAt (IMachineModule machineModule, IField field, DateTime at)
    {
      return ReadRange (machineModule, field, at, at.AddMilliseconds (1))
        .LastOrDefault (v => IsAt (v, at));
    }

    /// <summary>
    /// <see cref="ICncValueDataDAO"/>
    /// </summary>
    public ICncValueData FindWithEnd (IMachineModule machineModule, IField field, DateTime end)
    {
      return ReadRange (machineModule, field, end, end.AddMilliseconds (1))
        .FirstOrDefault (v => (v.Begin < end) && (v.End == end));
    }

    /// <summary>
    /// <see cref="ICncValueDataDAO"/>
    /// </summary>
    public IList<ICncValueData> FindOverlapsRange (IMachineModule machineModule, IField field, UtcDateTimeRange range)
    {
      if (range.IsEmpty ()) {
        return new List<ICncValueData> ();
      }
      var from = range.Lower.HasValue ? range.Lower.Value : NO_LOWER_BOUND;
      DateTime to;
      if (range.Upper.HasValue) {
        to = range.UpperInclusive ? range.Upper.Value.AddMilliseconds (1) : range.Upper.Value;
      }
      else {
        to = NO_UPPER_BOUND;
      }
      return ReadRange (machineModule, field, from, to)
        .Where (v => IsInRange (v, range))
        .ToList ();
    }

    /// <summary>
    /// <see cref="ICncValueDataDAO"/>
    /// </summary>
    public IList<ICncValueData> FindNext (IMachineModule machineModule, IField field, DateTime dateTime, int maxNbFetched)
    {
      return FindFirst (machineModule, field, dateTime, NO_UPPER_BOUND, false, v => dateTime <= v.Begin, maxNbFetched)
        .ToList ();
    }

    /// <summary>
    /// <see cref="ICncValueDataDAO"/>
    /// </summary>
    public IEnumerable<ICncValueData> FindFirstOverlapsRange (IMachineModule machineModule, IField field, UtcDateTimeRange range, int n, bool descending)
    {
      if (range.IsEmpty ()) {
        return new List<ICncValueData> ();
      }
      var lower = range.Lower.HasValue ? range.Lower.Value : NO_LOWER_BOUND;
      DateTime upper;
      if (range.Upper.HasValue) {
        upper = range.UpperInclusive ? range.Upper.Value.AddMilliseconds (1) : range.Upper.Value;
      }
      else {
        upper = NO_UPPER_BOUND;
      }
      return FindFirst (machineModule, field, lower, upper, descending, v => IsInRange (v, range), n);
    }

    /// <summary>
    /// <see cref="ICncValueDataDAO"/>
    /// </summary>
    public IEnumerable<ICncValueData> FindOverlapsRangeAscending (IMachineModule machineModule, IField field, UtcDateTimeRange range, TimeSpan step)
    {
      var logger = LogManager.GetLogger ($"{typeof (CncValueDataDAO).FullName}.{machineModule.Id}.{field.Id}");
      var strategy = new CncValueDataOverlapsRangeByStepStrategy (this, field, step, false);
      return new FindOverlapsRangeEnumerable<ICncValueData, IMachineModule> (strategy, machineModule, range, logger);
    }

    /// <summary>
    /// <see cref="ICncValueDataDAO"/>
    /// </summary>
    public IEnumerable<ICncValueData> FindOverlapsRangeDescending (IMachineModule machineModule, IField field, UtcDateTimeRange range, TimeSpan step)
    {
      var logger = LogManager.GetLogger ($"{typeof (CncValueDataDAO).FullName}.{machineModule.Id}.{field.Id}");
      var strategy = new CncValueDataOverlapsRangeByStepStrategy (this, field, step, true);
      return new FindOverlapsRangeEnumerable<ICncValueData, IMachineModule> (strategy, machineModule, range, logger);
    }

    /// <summary>
    /// First values in [lower, upper) that match a filter, in the given order of their begin
    ///
    /// The range is read by windows of increasing size from its start (or its end if descending),
    /// so that an open range does not read the whole archive.
    /// The values of a field do not overlap: the order is kept from one window to the other
    /// </summary>
    IEnumerable<ICncValueData> FindFirst (IMachineModule machineModule, IField field, DateTime lower, DateTime upper,
      bool descending, Func<ICncValueData, bool> filter, int n)
    {
      var result = new List<ICncValueData> ();
      var seen = new HashSet<(CncValueSource, long)> ();
      var window = INITIAL_WINDOW;
      var cursor = descending ? upper : lower;
      while ((result.Count < n) && (descending ? (lower < cursor) : (cursor < upper))) {
        DateTime from, to;
        if (descending) {
          to = cursor;
          from = (cursor.Subtract (lower) <= window) ? lower : cursor.Subtract (window);
          cursor = from;
        }
        else {
          from = cursor;
          to = (upper.Subtract (cursor) <= window) ? upper : cursor.Add (window);
          cursor = to;
        }
        var values = ReadRange (machineModule, field, from, to);
        foreach (var v in descending ? values.Reverse () : values) {
          if (seen.Add ((v.Source, v.Id)) && filter (v)) {
            result.Add (v);
            if (n <= result.Count) {
              break;
            }
          }
        }
        window = TimeSpan.FromTicks (window.Ticks * 2); // About 22 windows between 1900 and 9999
      }
      return result;
    }

    static readonly TimeSpan INITIAL_WINDOW = TimeSpan.FromDays (1);

    /// <summary>
    /// Same semantics as CncValueDAO.FindAt: begin &lt;= at &lt; end, or begin = end = at
    /// </summary>
    static bool IsAt (ICncValueData v, DateTime at) =>
      (v.Begin <= at) && ((at < v.End) || ((v.End == at) && (v.Begin == v.End)));

    /// <summary>
    /// Same semantics as CncValueDAO.FindOverlapsRange
    /// </summary>
    static bool IsInRange (ICncValueData v, UtcDateTimeRange range)
    {
      if (range.Lower.HasValue) {
        var lower = range.Lower.Value;
        if (!((lower < v.End) || ((v.End == lower) && (v.Begin == v.End) && range.LowerInclusive))) {
          return false;
        }
      }
      if (range.Upper.HasValue) {
        var upper = range.Upper.Value;
        if (range.UpperInclusive ? (upper < v.Begin) : (upper <= v.Begin)) {
          return false;
        }
      }
      return true;
    }

    IList<ICncValueData> ReadRange (IMachineModule machineModule, IField field, DateTime from, DateTime to)
    {
      Debug.Assert (null != machineModule);
      Debug.Assert (null != field);

      return Read ("SELECT * FROM public.cncvaluerange (@machinemoduleid, @fieldid, @from, @to)",
        command => {
          CncDataPartitionDAO.AddParameter (command, "machinemoduleid", DbType.Int32, machineModule.Id);
          CncDataPartitionDAO.AddParameter (command, "fieldid", DbType.Int32, field.Id);
          CncDataPartitionDAO.AddParameter (command, "from", DbType.DateTime, CncDataPartitionDAO.ToTimestamp (from));
          CncDataPartitionDAO.AddParameter (command, "to", DbType.DateTime, CncDataPartitionDAO.ToTimestamp (to));
        },
        machineModule,
        _ => field);
    }

    static IList<ICncValueData> Read (string sql, Action<IDbCommand> addParameters, IMachineModule machineModule, Func<int, IField> getField)
    {
      // As the auto flush of NHibernate before a query: the cnc values that are created or modified in the session
      // are visible in the SQL request
      var session = NHibernateHelper.GetCurrentSession ();
      if ((NHibernate.FlushMode.Manual != session.FlushMode) && session.IsDirty ()) {
        session.Flush ();
      }

      var result = new List<ICncValueData> ();
      using (var command = CncDataPartitionDAO.CreateCommand ()) {
        command.CommandText = sql;
        addParameters (command);
        using (var reader = command.ExecuteReader ()) {
          while (reader.Read ()) {
            var fieldId = reader.GetInt32 (reader.GetOrdinal ("fieldid"));
            var field = getField (fieldId);
            if (null == field) {
              log.Error ($"Read: unknown field id {fieldId} => skip the value");
              continue;
            }
            result.Add (new CncValueData (
              reader.GetInt64 (reader.GetOrdinal ("cncvalueid")),
              ParseSource (GetValue<string> (reader, "cncvaluesource")),
              machineModule,
              field,
              DateTime.SpecifyKind (reader.GetDateTime (reader.GetOrdinal ("cncvaluebegindatetime")), DateTimeKind.Utc),
              DateTime.SpecifyKind (reader.GetDateTime (reader.GetOrdinal ("cncvalueenddatetime")), DateTimeKind.Utc),
              GetValue<string> (reader, "cncvaluestring"),
              GetValue<int?> (reader, "cncvalueint"),
              GetValue<double?> (reader, "cncvaluedouble"),
              GetValue<double?> (reader, "cncvaluedeviation"),
              reader.GetBoolean (reader.GetOrdinal ("cncvaluestopped")),
              GetValue<int?> (reader, "cncvaluecount"),
              GetValue<double?> (reader, "cncvaluemin"),
              GetValue<double?> (reader, "cncvaluemax"),
              GetValue<string> (reader, "cncvaluejson")));
          }
        }
      }
      return result;
    }

    static T GetValue<T> (IDataReader reader, string column)
    {
      var i = reader.GetOrdinal (column);
      return reader.IsDBNull (i) ? default : (T)reader.GetValue (i);
    }

    static CncValueSource ParseSource (string source)
    {
      switch (source) {
      case "cncdataperiod":
        return CncValueSource.CncDataPeriod;
      case "cncdataperiodarchive":
        return CncValueSource.CncDataPeriodArchive;
      default:
        return CncValueSource.CncValue;
      }
    }
  }

  /// <summary>
  /// Strategy to read the cnc values by step (same configuration as the one of the cnc values)
  /// </summary>
  internal sealed class CncValueDataOverlapsRangeByStepStrategy
    : IFindOverlapsRangeByStepStrategy<ICncValueData, IMachineModule>
  {
    static readonly string INITIAL_STEP_NUMBER_KEY = "Database.CncValueDAO.FindOverlapsRangeStep.InitialStepNumber";
    static readonly int INITIAL_STEP_NUMBER_DEFAULT = 4;
    static readonly string LOWER_LIMIT_KEY = "Database.CncValueDAO.FindOverlapsRangeStep.LowerLimit";
    static readonly LowerBound<DateTime> LOWER_LIMIT_DEFAULT = new DateTime (2016, 01, 01, 00, 00, 00, DateTimeKind.Utc);
    static readonly string MAX_GAP_KEY = "Database.CncValueDAO.FindOverlapsRangeStep.MaxGap";
    static readonly TimeSpan? MAX_GAP_DEFAULT = TimeSpan.FromDays (90);

    readonly CncValueDataDAO m_dao;
    readonly IField m_field;

    public CncValueDataOverlapsRangeByStepStrategy (CncValueDataDAO dao, IField field, TimeSpan step, bool descending)
    {
      m_dao = dao;
      m_field = field;
      this.Step = step;
      this.Descending = descending;
    }

    public bool Descending { get; }

    public TimeSpan Step { get; }

    public UtcDateTimeRange LimitRange =>
      new UtcDateTimeRange (Lemoine.Info.ConfigSet.LoadAndGet (LOWER_LIMIT_KEY, LOWER_LIMIT_DEFAULT), DateTime.UtcNow);

    public TimeSpan? MaxGap => Lemoine.Info.ConfigSet.LoadAndGet (MAX_GAP_KEY, MAX_GAP_DEFAULT);

    public IEnumerable<ICncValueData> InitialRequest (IMachineModule machineModule, UtcDateTimeRange range) =>
      m_dao.FindFirstOverlapsRange (machineModule, m_field, range,
        Lemoine.Info.ConfigSet.LoadAndGet (INITIAL_STEP_NUMBER_KEY, INITIAL_STEP_NUMBER_DEFAULT), this.Descending);

    public bool MaxResultsInitial => true;

    public bool LimitRangeInitial => false;

    public IEnumerable<ICncValueData> NextRequest (IMachineModule machineModule, UtcDateTimeRange range)
    {
      var values = m_dao.FindOverlapsRange (machineModule, m_field, range);
      return this.Descending ? values.Reverse () : values;
    }

    public bool MaxResultsNext => false;

    public bool LimitRangeNext => true;

    public bool ReadWrite => false;
  }

  /// <summary>
  /// Implementation of <see cref="ICncValueData"/> for a row of cncvalueall
  /// </summary>
  internal sealed class CncValueData : ICncValueData
  {
    public long Id { get; }
    public CncValueSource Source { get; }
    public IMachineModule MachineModule { get; }
    public IField Field { get; }
    public DateTime Begin { get; }
    public DateTime End { get; }
    public string String { get; }
    public int? Int { get; }
    public double? Double { get; }
    public double? Deviation { get; }
    public bool Stopped { get; }
    public int? Count { get; }
    public double? Min { get; }
    public double? Max { get; }
    public string JsonValue { get; }

    public TimeSpan Length => this.End.Subtract (this.Begin);

    public UtcDateTimeRange DateTimeRange => new UtcDateTimeRange (this.Begin, this.End);

    /// <summary>
    /// Same as CncValue.Value
    /// </summary>
    public object Value
    {
      get {
        switch (this.Field.Type) {
        case FieldType.Boolean:
          return this.Int.HasValue ? (object)(0 != this.Int.Value) : null;
        case FieldType.String:
          return this.String;
        case FieldType.Int32:
          return this.Int;
        case FieldType.Double:
          return this.Double;
        default:
          return this.String;
        }
      }
    }

    public CncValueData (long id, CncValueSource source, IMachineModule machineModule, IField field, DateTime begin, DateTime end,
      string s, int? i, double? d, double? deviation, bool stopped, int? count, double? min, double? max, string jsonValue)
    {
      this.Id = id;
      this.Source = source;
      this.MachineModule = machineModule;
      this.Field = field;
      this.Begin = begin;
      this.End = end;
      this.String = s;
      this.Int = i;
      this.Double = d;
      this.Deviation = deviation;
      this.Stopped = stopped;
      this.Count = count;
      this.Min = min;
      this.Max = max;
      this.JsonValue = jsonValue;
    }

    public bool IsEmpty () => null == this.Value;

    public override string ToString () => $"[CncValueData {this.Source} {this.Id} field={this.Field?.Id} {this.Begin:o}-{this.End:o}]";
  }
}
