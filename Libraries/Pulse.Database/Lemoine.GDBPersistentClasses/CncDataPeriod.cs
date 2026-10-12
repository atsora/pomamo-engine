// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table cncdataperiod
  ///
  /// MachineModule and Begin are the secondary keys of the partitions: they can't be changed
  /// </summary>
  public class CncDataPeriod : ICncDataPeriod
  {
    long m_id = 0;
    int m_version = 0;
    IMachineModule m_machineModule;
    IField m_field;
    DateTime m_begin;
    DateTime m_end;
    DateTime? m_maxEnd = null; // Not persistent
    bool m_stopped = false;
    int? m_count = null;
    double? m_average = null;
    double? m_min = null;
    double? m_max = null;
    double? m_stdDev = null;
    double? m_lastValue = null;
    string m_stringValue = null;
    string m_jsonValue = null;

    ILog log = LogManager.GetLogger (typeof (CncDataPeriod).FullName);

    /// <summary>
    /// <see cref="ICncDataPeriodValue"/>
    /// </summary>
    public virtual long Id => m_id;

    /// <summary>
    /// <see cref="IDataWithVersion"/>
    /// </summary>
    public virtual int Version => m_version;

    /// <summary>
    /// <see cref="IPartitionedByMachineModule"/>
    /// </summary>
    public virtual IMachineModule MachineModule => m_machineModule;

    /// <summary>
    /// <see cref="ICncDataPeriod"/>
    /// </summary>
    public virtual IField Field => m_field;

    /// <summary>
    /// <see cref="ICncDataPeriodValue"/>
    /// </summary>
    public virtual DateTime Begin => m_begin;

    /// <summary>
    /// <see cref="ICncDataPeriod"/>
    /// </summary>
    public virtual DateTime End
    {
      get => m_end;
      set {
        var end = CncDataWeek.TruncateToMilliseconds (value);
        if (end < m_begin) {
          log.Error ($"End.set: end {end} before begin {m_begin}");
          throw new ArgumentOutOfRangeException (nameof (value), end, $"End before Begin {m_begin}");
        }
        if (this.MaxEnd < end) {
          log.Error ($"End.set: end {end} after the week bound {this.MaxEnd}");
          throw new ArgumentOutOfRangeException (nameof (value), end, $"End after the week bound {this.MaxEnd}");
        }
        m_end = end;
      }
    }

    /// <summary>
    /// <see cref="ICncDataPeriod"/>
    /// </summary>
    public virtual DateTime MaxEnd
    {
      get {
        if (!m_maxEnd.HasValue) {
          var cutOffDay = (DayOfWeek)ModelDAOHelper.DAOFactory.CncDataPartitionDAO.GetWeekCutOffDay ();
          m_maxEnd = CncDataWeek.GetWeekEnd (m_begin, cutOffDay);
        }
        return m_maxEnd.Value;
      }
    }

    /// <summary>
    /// <see cref="IWithDateTimeRange"/>
    /// </summary>
    public virtual UtcDateTimeRange DateTimeRange => new UtcDateTimeRange (m_begin, m_end);

    /// <summary>
    /// <see cref="ICncDataPeriod"/>
    /// </summary>
    public virtual bool Stopped
    {
      get => m_stopped;
      set => m_stopped = value;
    }

    /// <summary>
    /// <see cref="ICncDataPeriod"/>
    /// </summary>
    public virtual int? Count
    {
      get => m_count;
      set => m_count = value;
    }

    /// <summary>
    /// <see cref="ICncDataPeriod"/>
    /// </summary>
    public virtual double? Average
    {
      get => m_average;
      set => m_average = value;
    }

    /// <summary>
    /// <see cref="ICncDataPeriod"/>
    /// </summary>
    public virtual double? Min
    {
      get => m_min;
      set => m_min = value;
    }

    /// <summary>
    /// <see cref="ICncDataPeriod"/>
    /// </summary>
    public virtual double? Max
    {
      get => m_max;
      set => m_max = value;
    }

    /// <summary>
    /// <see cref="ICncDataPeriod"/>
    /// </summary>
    public virtual double? StdDev
    {
      get => m_stdDev;
      set => m_stdDev = value;
    }

    /// <summary>
    /// <see cref="ICncDataPeriod"/>
    /// </summary>
    public virtual double? LastValue
    {
      get => m_lastValue;
      set => m_lastValue = value;
    }

    /// <summary>
    /// <see cref="ICncDataPeriod"/>
    /// </summary>
    public virtual string StringValue
    {
      get => m_stringValue;
      set => m_stringValue = value;
    }

    /// <summary>
    /// <see cref="ICncDataPeriod"/>
    /// </summary>
    public virtual string JsonValue
    {
      get => m_jsonValue;
      set => m_jsonValue = value;
    }

    /// <summary>
    /// <see cref="ICncDataPeriodValue"/>: a persistent period is never archived
    /// </summary>
    public virtual bool Archived => false;

    /// <summary>
    /// <see cref="ICncDataPeriod"/>
    /// </summary>
    public virtual CncDataStatistics? Statistics
    {
      get {
        if (m_count.HasValue && (0 < m_count.Value) && m_average.HasValue && m_min.HasValue && m_max.HasValue && m_lastValue.HasValue) {
          return new CncDataStatistics (m_count.Value, m_average.Value, m_min.Value, m_max.Value, m_stdDev ?? 0.0, m_lastValue.Value);
        }
        else {
          return null;
        }
      }
      set {
        m_count = value?.Count;
        m_average = value?.Average;
        m_min = value?.Min;
        m_max = value?.Max;
        m_stdDev = value?.StdDev;
        m_lastValue = value?.LastValue;
      }
    }

    /// <summary>
    /// Default constructor for NHibernate
    /// </summary>
    protected CncDataPeriod ()
    { }

    /// <summary>
    /// Constructor (End = Begin)
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="field">not null</param>
    /// <param name="begin">UTC date/time, truncated to the millisecond</param>
    internal protected CncDataPeriod (IMachineModule machineModule, IField field, DateTime begin)
    {
      m_machineModule = machineModule ?? throw new ArgumentNullException (nameof (machineModule));
      m_field = field ?? throw new ArgumentNullException (nameof (field));
      m_begin = CncDataWeek.TruncateToMilliseconds (begin);
      m_end = m_begin;
      log = LogManager.GetLogger ($"{typeof (CncDataPeriod).FullName}.{machineModule.Id}.{field.Id}");
    }

    /// <summary>
    /// <see cref="ICncDataPeriod"/>
    /// </summary>
    /// <param name="end"></param>
    /// <param name="statistics"></param>
    public virtual void Extend (DateTime end, CncDataStatistics statistics)
    {
      var truncatedEnd = CncDataWeek.TruncateToMilliseconds (end);
      if (truncatedEnd < m_end) {
        log.Error ($"Extend: end {truncatedEnd} before the current end {m_end}");
        throw new ArgumentOutOfRangeException (nameof (end), truncatedEnd, $"End before the current end {m_end}");
      }
      this.End = truncatedEnd;
      var current = this.Statistics;
      this.Statistics = current.HasValue
        ? CncDataStatistics.Combine (current.Value, statistics)
        : statistics;
    }

    /// <summary>
    /// <see cref="object.ToString"/>
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[CncDataPeriod {this.Id} {this.MachineModule?.ToStringIfInitialized ()} {this.Field?.ToStringIfInitialized ()} {this.Begin:o}-{this.End:o}]";
      }
      else {
        return $"[CncDataPeriod {this.Id}]";
      }
    }
  }
}
