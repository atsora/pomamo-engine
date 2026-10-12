// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="ICncDataPeriodDAO"/>
  ///
  /// A period never crosses a week bound, so a period that contains a date/time t
  /// begins after t minus one week: this criterion restricts the requests to at most two week partitions
  /// </summary>
  public class CncDataPeriodDAO
    : VersionableByMachineModuleNHibernateDAO<CncDataPeriod, ICncDataPeriod, long>
    , ICncDataPeriodDAO
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CncDataPeriodDAO).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public CncDataPeriodDAO ()
      : base ("MachineModule")
    { }

    /// <summary>
    /// Create the missing partition of a new period, then save or update it
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override ICncDataPeriod MakePersistent (ICncDataPeriod entity)
    {
      Debug.Assert (null != entity);
      if (0 == entity.Id) {
        ModelDAOHelper.DAOFactory.CncDataPartitionDAO.EnsurePartition (CncDataTable.Period, entity.MachineModule.Id, entity.Begin);
      }
      try {
        return base.MakePersistent (entity);
      }
      catch (Exception ex) when (ModelDAOHelper.DAOFactory.CncDataPartitionDAO.IsMissingPartition (ex)) {
        log.Error ($"MakePersistent: unexpected missing partition for {entity}", ex);
        CncDataPartitionDAO.ForgetPartitions (CncDataTable.Period, entity.MachineModule.Id);
        throw;
      }
    }

    /// <summary>
    /// Create the missing partition of a new period, then save or update it
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override async Task<ICncDataPeriod> MakePersistentAsync (ICncDataPeriod entity)
    {
      Debug.Assert (null != entity);
      if (0 == entity.Id) {
        ModelDAOHelper.DAOFactory.CncDataPartitionDAO.EnsurePartition (CncDataTable.Period, entity.MachineModule.Id, entity.Begin);
      }
      try {
        return await base.MakePersistentAsync (entity);
      }
      catch (Exception ex) when (ModelDAOHelper.DAOFactory.CncDataPartitionDAO.IsMissingPartition (ex)) {
        log.Error ($"MakePersistentAsync: unexpected missing partition for {entity}", ex);
        CncDataPartitionDAO.ForgetPartitions (CncDataTable.Period, entity.MachineModule.Id);
        throw;
      }
    }

    /// <summary>
    /// <see cref="ICncDataPeriodDAO"/>
    /// </summary>
    public ICncDataPeriod FindById (long id, IMachineModule machineModule, DateTime begin)
    {
      Debug.Assert (null != machineModule);

      if (TryGetInCache (id, out var result)) {
        return result;
      }
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CncDataPeriod> ()
        .Add (Restrictions.Eq ("Id", id))
        .Add (Restrictions.Eq ("MachineModule.Id", machineModule.Id))
        .Add (Restrictions.Eq ("Begin", CncDataWeek.TruncateToMilliseconds (begin)))
        .UniqueResult<ICncDataPeriod> ();
    }

    /// <summary>
    /// <see cref="ICncDataPeriodDAO"/>
    ///
    /// If there is also an empty period at end, the period that begins before end is returned
    /// </summary>
    public ICncDataPeriod FindWithEnd (IMachineModule machineModule, IField field, DateTime end)
    {
      Debug.Assert (null != machineModule);
      Debug.Assert (null != field);

      var truncatedEnd = CncDataWeek.TruncateToMilliseconds (end);
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CncDataPeriod> ()
        .Add (Restrictions.Eq ("MachineModule.Id", machineModule.Id))
        .Add (Restrictions.Eq ("Field.Id", field.Id))
        .Add (Restrictions.Ge ("Begin", truncatedEnd.Subtract (CncDataWeek.Duration)))
        .Add (Restrictions.Le ("Begin", truncatedEnd))
        .Add (Restrictions.Eq ("End", truncatedEnd))
        .AddOrder (Order.Asc ("Begin"))
        .SetMaxResults (1)
        // Not cacheable because of the partitioned table:
        // there is a risk to make NHibernate use a FindById without the secondary keys
        .UniqueResult<ICncDataPeriod> ();
    }

    /// <summary>
    /// <see cref="ICncDataPeriodDAO"/>
    /// </summary>
    public ICncDataPeriod FindLast (IMachineModule machineModule, IField field)
    {
      Debug.Assert (null != machineModule);
      Debug.Assert (null != field);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CncDataPeriod> ()
        .Add (Restrictions.Eq ("MachineModule.Id", machineModule.Id))
        .Add (Restrictions.Eq ("Field.Id", field.Id))
        .AddOrder (Order.Desc ("Begin"))
        .SetMaxResults (1)
        .UniqueResult<ICncDataPeriod> ();
    }

    /// <summary>
    /// <see cref="ICncDataPeriodDAO"/>
    /// </summary>
    public ICncDataPeriod FindAt (IMachineModule machineModule, IField field, DateTime at)
    {
      Debug.Assert (null != machineModule);
      Debug.Assert (null != field);

      var truncatedAt = CncDataWeek.TruncateToMilliseconds (at);
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CncDataPeriod> ()
        .Add (Restrictions.Eq ("MachineModule.Id", machineModule.Id))
        .Add (Restrictions.Eq ("Field.Id", field.Id))
        .Add (Restrictions.Ge ("Begin", truncatedAt.Subtract (CncDataWeek.Duration)))
        .Add (Restrictions.Le ("Begin", truncatedAt))
        .Add (Restrictions.Disjunction ()
              .Add (Restrictions.Gt ("End", truncatedAt))
              .Add (Restrictions.Conjunction ()
                    .Add (Restrictions.Eq ("End", truncatedAt))
                    .Add (Restrictions.EqProperty ("Begin", "End"))))
        .AddOrder (Order.Desc ("Begin"))
        .SetMaxResults (1)
        .UniqueResult<ICncDataPeriod> ();
    }

    /// <summary>
    /// <see cref="ICncDataPeriodDAO"/>
    /// </summary>
    public IList<ICncDataPeriod> FindOverlapsRange (IMachineModule machineModule, IField field, UtcDateTimeRange range)
    {
      Debug.Assert (null != machineModule);
      Debug.Assert (null != field);

      if (range.IsEmpty ()) {
        return new List<ICncDataPeriod> ();
      }
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CncDataPeriod> ()
        .Add (Restrictions.Eq ("MachineModule.Id", machineModule.Id))
        .Add (Restrictions.Eq ("Field.Id", field.Id))
        .Add (InUtcRange (range))
        .AddOrder (Order.Asc ("Begin"))
        // Not cacheable because of the partitioned table
        .List<ICncDataPeriod> ();
    }

    static AbstractCriterion InUtcRange (UtcDateTimeRange range)
    {
      var result = Restrictions.Conjunction ();
      if (range.Lower.HasValue) {
        var lower = CncDataWeek.TruncateToMilliseconds (range.Lower.Value);
        if (!range.LowerInclusive) {
          lower = lower.AddMilliseconds (1);
        }
        result
          .Add (Restrictions.Ge ("Begin", lower.Subtract (CncDataWeek.Duration))) // Partition pruning
          .Add (Restrictions.Disjunction ()
            .Add (Restrictions.Gt ("End", lower))
            .Add (Restrictions.Conjunction ()
                  .Add (Restrictions.Eq ("End", lower))
                  .Add (Restrictions.EqProperty ("Begin", "End"))));
      }
      if (range.Upper.HasValue) {
        var upper = CncDataWeek.TruncateToMilliseconds (range.Upper.Value);
        if (range.UpperInclusive) {
          result.Add (Restrictions.Le ("Begin", upper));
        }
        else {
          result.Add (Restrictions.Lt ("Begin", upper));
        }
      }
      return result;
    }

    /// <summary>
    /// <see cref="ICncDataPeriodDAO"/>
    /// </summary>
    public IList<ICncDataPeriodValue> FindValuesOverlapsRange (IMachineModule machineModule, IField field, UtcDateTimeRange range)
    {
      Debug.Assert (null != machineModule);
      Debug.Assert (null != field);

      var result = new List<ICncDataPeriodValue> ();
      if (range.IsEmpty ()) {
        return result;
      }
      var (from, to) = CncDataSampleDAO.GetFromTo (range);
      using (var command = CncDataPartitionDAO.CreateCommand ()) {
        command.CommandText = @"
SELECT id, begindatetime, enddatetime, stopped, count, average, min, max, stddev, lastvalue, stringvalue, jsonvalue, archived
  FROM public.cncdataperiodrange (@machinemoduleid, @fieldid, @from, @to)";
        CncDataPartitionDAO.AddParameter (command, "machinemoduleid", DbType.Int32, machineModule.Id);
        CncDataPartitionDAO.AddParameter (command, "fieldid", DbType.Int32, field.Id);
        CncDataPartitionDAO.AddParameter (command, "from", DbType.DateTime, CncDataPartitionDAO.ToTimestamp (from));
        CncDataPartitionDAO.AddParameter (command, "to", DbType.DateTime, CncDataPartitionDAO.ToTimestamp (to));
        using (var reader = command.ExecuteReader ()) {
          while (reader.Read ()) {
            result.Add (new CncDataPeriodValue (
              reader.GetInt64 (0),
              DateTime.SpecifyKind (reader.GetDateTime (1), DateTimeKind.Utc),
              DateTime.SpecifyKind (reader.GetDateTime (2), DateTimeKind.Utc),
              reader.GetBoolean (3),
              reader.IsDBNull (4) ? null : reader.GetInt32 (4),
              reader.IsDBNull (5) ? null : reader.GetDouble (5),
              reader.IsDBNull (6) ? null : reader.GetDouble (6),
              reader.IsDBNull (7) ? null : reader.GetDouble (7),
              reader.IsDBNull (8) ? null : reader.GetDouble (8),
              reader.IsDBNull (9) ? null : reader.GetDouble (9),
              reader.IsDBNull (10) ? null : reader.GetString (10),
              reader.IsDBNull (11) ? null : reader.GetString (11),
              reader.GetBoolean (12)));
          }
        }
      }
      return result;
    }
  }

  /// <summary>
  /// Implementation of <see cref="ICncDataPeriodValue"/> for the read-only periods (recent or archived)
  /// </summary>
  internal sealed class CncDataPeriodValue : ICncDataPeriodValue
  {
    public long Id { get; }
    public DateTime Begin { get; }
    public DateTime End { get; }
    public bool Stopped { get; }
    public int? Count { get; }
    public double? Average { get; }
    public double? Min { get; }
    public double? Max { get; }
    public double? StdDev { get; }
    public double? LastValue { get; }
    public string StringValue { get; }
    public string JsonValue { get; }
    public bool Archived { get; }
    public UtcDateTimeRange DateTimeRange => new UtcDateTimeRange (this.Begin, this.End);

    public CncDataPeriodValue (long id, DateTime begin, DateTime end, bool stopped, int? count, double? average,
      double? min, double? max, double? stdDev, double? lastValue, string stringValue, string jsonValue, bool archived)
    {
      this.Id = id;
      this.Begin = begin;
      this.End = end;
      this.Stopped = stopped;
      this.Count = count;
      this.Average = average;
      this.Min = min;
      this.Max = max;
      this.StdDev = stdDev;
      this.LastValue = lastValue;
      this.StringValue = stringValue;
      this.JsonValue = jsonValue;
      this.Archived = archived;
    }

    public override string ToString () => $"[CncDataPeriodValue {this.Id} {this.Begin:o}-{this.End:o}{(this.Archived ? " archived" : "")}]";
  }
}
