// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;
using System.Collections;
using System.Threading.Tasks;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IOperationCycleDAO">IOperationCycleDAO</see>
  /// 
  /// Note that for some DAO methods that returns an IOperationCycle,
  /// an eager fetch of the associated OperationSlot is done.
  /// This gives a little more chance to take profit of the partitioning
  /// </summary>
  public sealed class OperationCycleDAO
    : VersionableByMachineNHibernateDAO<OperationCycle, IOperationCycle, int>
    , IOperationCycleDAO
  {
    readonly ILog log = LogManager.GetLogger (typeof (OperationCycleDAO).FullName);

    static readonly string MIN_DATE_TIME_KEY = "Database.OperationCycleDAO.MinDateTime";
    static readonly DateTime MIN_DATE_TIME_DEFAULT = new DateTime (2019, 01, 01, 00, 00, 00, DateTimeKind.Utc);

    /// <summary>
    /// Constructor
    /// </summary>
    public OperationCycleDAO ()
      : base ("Machine")
    { }

    /// <summary>
    /// Override MakePersistent to track its transient status change
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override IOperationCycle MakePersistent (IOperationCycle entity)
    {
      OperationCycle operationCycle = (OperationCycle)entity;
      IOperationCycle result;
      using (var changeTracker = new OperationCycle.ChangeTracker (operationCycle)) {
        result = base.MakePersistent (entity);
        operationCycle.SetPersistent ();
      }
      return result;
    }

    /// <summary>
    /// Override MakePersistent to track its transient status change
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override async Task<IOperationCycle> MakePersistentAsync (IOperationCycle entity)
    {
      OperationCycle operationCycle = (OperationCycle)entity;
      IOperationCycle result;
      using (var changeTracker = new OperationCycle.ChangeTracker (operationCycle)) {
        result = await base.MakePersistentAsync (entity);
        operationCycle.SetPersistent ();
      }
      return result;
    }

    /// <summary>
    /// Override MakeTransient to track its transient status change
    /// </summary>
    /// <param name="entity"></param>
    public override void MakeTransient (IOperationCycle entity)
    {
      OperationCycle operationCycle = (OperationCycle)entity;
      using (var changeTracker = new OperationCycle.ChangeTracker (operationCycle)) {
        base.MakeTransient (entity);
        operationCycle.SetTransient ();
      }
    }

    /// <summary>
    /// Override MakeTransient to track its transient status change
    /// </summary>
    /// <param name="entity"></param>
    public override async System.Threading.Tasks.Task MakeTransientAsync (IOperationCycle entity)
    {
      OperationCycle operationCycle = (OperationCycle)entity;
      using (var changeTracker = new OperationCycle.ChangeTracker (operationCycle)) {
        await base.MakeTransientAsync (entity);
        operationCycle.SetTransient ();
      }
    }

    /// <summary>
    /// Find all the operation cycles, returned by ascending Date/Time
    /// </summary>
    /// <returns></returns>
    public override IList<IOperationCycle> FindAll ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<OperationCycle> ()
        .AddOrder (Order.Asc ("DateTime"))
        .AddOrder (Order.Asc ("End"))
        .List<IOperationCycle> ();
    }

    /// <summary>
    /// Find all the operation cycles, returned by ascending Date/Time
    /// </summary>
    /// <returns></returns>
    public override async Task<IList<IOperationCycle>> FindAllAsync ()
    {
      return await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<OperationCycle> ()
        .AddOrder (Order.Asc ("DateTime"))
        .AddOrder (Order.Asc ("End"))
        .ListAsync<IOperationCycle> ();
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IOperationCycleDAO.CheckNoOperationCycleStrictlyAfter" />
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public bool CheckNoOperationCycleStrictlyAfter (IMachine machine,
                                                    DateTime dateTime)
    {
      IQuery query = NHibernateHelper.GetCurrentSession ()
        .GetNamedQuery ("AnyOperationCycleStrictlyAfter");
      query.SetParameter ("Machine", machine);
      query.SetParameter ("DateTime", dateTime);
      query.SetMaxResults (1);
      return (null == query.UniqueResult<IOperationCycle> ());
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IOperationCycleDAO.GetLast" />
    /// </summary>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    public IOperationCycle GetLast (IMachine machine)
    {
      Debug.Assert (null != machine);

      IQuery query = NHibernateHelper.GetCurrentSession ()
        .GetNamedQuery ("OperationCycleForMachineDesc");
      query.SetParameter ("MachineId", machine.Id);
      query.SetMaxResults (1);
      return query
        .UniqueResult<IOperationCycle> ()
        .LoadOperationSlot ();
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IOperationCycleDAO.GetLast" />
    /// </summary>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    public IOperationCycle GetLastFullCycle (IMachine machine)
    {
      Debug.Assert (null != machine);

      IQuery query = NHibernateHelper.GetCurrentSession ()
        .GetNamedQuery ("FullOperationCycleForMachineDesc");
      query.SetParameter ("MachineId", machine.Id);
      query.SetMaxResults (1);
      return query
        .UniqueResult<IOperationCycle> ()
        .LoadOperationSlot ();
    }

    /// <summary>
    /// Get the n last full operation cycles for the specified machine before a specified date/time
    /// 
    /// This is sorted by descending date/time
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="before"></param>
    /// <param name="quantity"></param>
    /// <returns></returns>
    public IList<IOperationCycle> GetLastFullCyclesBefore (IMachine machine, DateTime before, int quantity)
    {
      Debug.Assert (null != machine);

      IQuery query = NHibernateHelper.GetCurrentSession ()
        .GetNamedQuery ("FullOperationCycleForMachineDescBefore");
      query.SetParameter ("MachineId", machine.Id);
      query.SetParameter ("Before", before);
      query.SetMaxResults (quantity);
      return query.List<IOperationCycle> ();
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IOperationCycleDAO.FindAllWithOperationSlot" />
    /// </summary>
    /// <param name="operationSlot">not null</param>
    /// <returns></returns>
    public IList<IOperationCycle> FindAllWithOperationSlot (IOperationSlot operationSlot)
    {
      Debug.Assert (null != operationSlot);
      Debug.Assert (null != operationSlot.Machine);

      IQuery query = NHibernateHelper.GetCurrentSession ()
        .GetNamedQuery ("OperationCycleWithOperationSlot");
      query.SetParameter ("MachineId", operationSlot.Machine.Id);
      query.SetParameter ("OperationSlot", operationSlot);
      return query.List<IOperationCycle> ();
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IOperationCycleDAO.FindAllWithOperation" />
    /// </summary>
    /// <param name="operation">not null</param>
    /// <returns></returns>
    public IList<IOperationCycle> FindAllWithOperation (IOperation operation)
    {
      Debug.Assert (null != operation);

      IQuery query = NHibernateHelper.GetCurrentSession ()
        .GetNamedQuery ("OperationCycleWithOperation");
      query.SetParameter ("Operation", operation);
      return query.List<IOperationCycle> ();
    }


    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IOperationCycleDAO.FindAllInRangeExceptInSlot" />
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="rangeBegin"></param>
    /// <param name="rangeEnd"></param>
    /// <param name="operationSlot"></param>
    /// <returns></returns>
    public IList<IOperationCycle> FindAllInRangeExceptInSlot (IMachine machine,
                                                              DateTime rangeBegin,
                                                              DateTime? rangeEnd,
                                                              IOperationSlot operationSlot)
    {
      IQuery query = NHibernateHelper.GetCurrentSession ()
        .GetNamedQuery ("OperationCycleInRangeExceptInSlot");
      query.SetParameter ("Machine", machine);
      query.SetParameter ("RangeBegin", rangeBegin);
      query.SetParameter ("RangeEnd", rangeEnd);
      query.SetParameter ("OperationSlot", operationSlot);
      return query.List<IOperationCycle> ();
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IOperationCycleDAO.FindAllInRange" />
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IList<IOperationCycle> FindAllInRange (IMachine machine,
                                                  UtcDateTimeRange range)
    {
      Debug.Assert (null != machine);

      DateTime? rangeBegin = range.Lower.NullableValue;
      DateTime? rangeEnd = range.Upper.NullableValue;
      IQuery query = NHibernateHelper.GetCurrentSession ()
        .GetNamedQuery ("OperationCycleInRange");
      query.SetParameter ("MachineId", machine.Id);
      query.SetParameter ("RangeBegin", rangeBegin);
      query.SetParameter ("RangeEnd", rangeEnd);
      var operationCycles = query.List<IOperationCycle> ();
      LoadOperationSlots (operationCycles);
      return operationCycles;
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IOperationCycleDAO.FindOverlapsRange" />
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range">not null</param>
    /// <returns></returns>
    public IList<IOperationCycle> FindOverlapsRange (IMachine machine, UtcDateTimeRange range)
    {
      Debug.Assert (null != machine);
      Debug.Assert (null != range);

      DateTime? rangeBegin = range.Lower.NullableValue;
      DateTime? rangeEnd = range.Upper.NullableValue;
      IQuery query = NHibernateHelper.GetCurrentSession ()
        .GetNamedQuery ("OperationCycleInRange");
      query.SetParameter ("MachineId", machine.Id);
      query.SetParameter ("RangeBegin", rangeBegin);
      query.SetParameter ("RangeEnd", rangeEnd);
      var operationCycles = query.List<IOperationCycle> ();
      LoadOperationSlots (operationCycles);
      return operationCycles;
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IOperationCycleDAO.FindOverlapsRangeAsync" />
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range">not null</param>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<IList<IOperationCycle>> FindOverlapsRangeAsync (IMachine machine, UtcDateTimeRange range)
    {
      Debug.Assert (null != machine);
      Debug.Assert (null != range);

      DateTime? rangeBegin = range.Lower.NullableValue;
      DateTime? rangeEnd = range.Upper.NullableValue;
      IQuery query = NHibernateHelper.GetCurrentSession ()
        .GetNamedQuery ("OperationCycleInRange");
      query.SetParameter ("MachineId", machine.Id);
      query.SetParameter ("RangeBegin", rangeBegin);
      query.SetParameter ("RangeEnd", rangeEnd);
      var operationCycles = await query.ListAsync<IOperationCycle> ();
      LoadOperationSlots (operationCycles);
      return operationCycles;
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IOperationCycleDAO.GetMinCycleEnd" />
    /// </summary>
    /// <param name="operationSlot">not null</param>
    /// <returns></returns>
    public DateTime? GetMinCycleEnd (IOperationSlot operationSlot)
    {
      Debug.Assert (null != operationSlot);

      // min cycle end should not consider partial cycles:
      // that is why we only keep cycles with no estimated end
      DateTime? minDateTime = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<IOperationCycle> ()
        .Add (Restrictions.Eq ("Machine.Id", operationSlot.Machine.Id)) // To take profit of the partitioning
        .Add (Restrictions.Disjunction ()
              .Add (Restrictions.IsNull ("Status"))
              .Add (Restrictions.Eq ("Status", Lemoine.Model.OperationCycleStatus.BeginEstimated)))
        .Add (Restrictions.Eq ("OperationSlot", operationSlot))
        .SetProjection (Projections.Min ("End"))
        .UniqueResult<DateTime?> ();

      return minDateTime;
    }

    /// <summary>
    /// For a given operation slot, find its last full operation cycle
    /// with no associated deliverable piece
    /// </summary>
    public IOperationCycle FindLastFullNotAssociated (IOperationSlot operationSlot,
                                                     DateTime date)
    {
      IQuery query = NHibernateHelper.GetCurrentSession ()
        .GetNamedQuery ("LastFullOperationCyclesNotAssociated");
      query.SetParameter ("MachineId", operationSlot.Machine.Id); // To take profit of the partitioning
      query.SetParameter ("OperationSlot", operationSlot);
      query.SetParameter ("Date", date);
      query.SetMaxResults (1);

      IOperationCycle result = query.UniqueResult<IOperationCycle> ();
      return result;
    }


    /// <summary>
    /// For a given operation slot, finds its operation cycles
    /// with no associated deliverable piece (sorted by descending begin date times)
    /// </summary>
    public IList<IOperationCycle> FindAllNotAssociated (IOperationSlot operationSlot,
                                                       DateTime date)
    {
      IQuery query = NHibernateHelper.GetCurrentSession ()
        .GetNamedQuery ("OperationCyclesNotAssociated");
      query.SetParameter ("MachineId", operationSlot.Machine.Id); // To take profit of the partitioning
      query.SetParameter ("OperationSlot", operationSlot);
      query.SetParameter ("Date", date);

      IList<IOperationCycle> listReturn =
        query.List<IOperationCycle> ();

      return listReturn;
    }

    /// <summary>
    /// Find all the operation cycles on given machine
    /// intersecting interval [dateTime-timeSpan, dateTime+timeSpan]
    /// </summary>
    public IList<IOperationCycle> FindAllAroundDate (IMachine machine,
                                                     DateTime dateTime,
                                                     TimeSpan timeSpan)
    {
      DateTime rangeBegin = dateTime.Subtract (timeSpan);
      DateTime rangeEnd = dateTime.Add (timeSpan);

      IQuery query = NHibernateHelper.GetCurrentSession ()
        .GetNamedQuery ("OperationCyclesAroundDate");
      query.SetParameter ("Machine", machine);
      query.SetParameter ("RangeBegin", rangeBegin);
      query.SetParameter ("RangeEnd", rangeEnd);

      IList<IOperationCycle> listReturn =
        query.List<IOperationCycle> ();

      return listReturn;
    }

    /// <summary>
    /// Get the period duration for the specified operation slot
    /// to compute the average cycle time
    /// </summary>
    /// <param name="operationSlot">Not null</param>
    /// <returns></returns>
    public TimeSpan? GetSlotPeriodDuration (IOperationSlot operationSlot)
    {
      Debug.Assert (null != operationSlot);

      object[] slotPeriod =
        NHibernateHelper.GetCurrentSession ()
        .GetNamedQuery ("OperationCycleSlotPeriod")
        .SetParameter ("MachineId", operationSlot.Machine.Id) // To take profit of the partitioning
        .SetParameter ("OperationSlot", operationSlot)
        .UniqueResult<object[]> ();
      if (null != slotPeriod) {
        Debug.Assert (2 == slotPeriod.Length);
        if ((null == slotPeriod[0]) || (null == slotPeriod[1])) {
          log.DebugFormat ("GetSlotPeriodDuration: " +
                           "operation slot with no min/max cycle end " +
                           "=> no period found, return null");
          return null;
        }
        DateTime begin = (DateTime)slotPeriod[0];
        DateTime end = (DateTime)slotPeriod[1];
        log.DebugFormat ("GetSlotPeriodDuration: " +
                         "period is {0}-{1}",
                         begin, end);
        Debug.Assert (begin <= end);
        return end.Subtract (begin);
      }
      else {
        log.DebugFormat ("GetSlotPeriodDuration: " +
                         "no period found");
        return null;
      }
    }

    /// <summary>
    /// Get the operation cycle matching a dateTime on a given machine:
    /// operation cycle has a begin which is lower or equal to dateTime
    /// and its end (if any) is greater than dateTime
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public IOperationCycle FindAt (IMachine machine, DateTime dateTime)
    {
      IQuery query = NHibernateHelper.GetCurrentSession ()
        .GetNamedQuery ("OperationCycleAtDate");
      query.SetParameter ("Machine", machine);
      query.SetParameter ("Date", dateTime);
      query.SetMaxResults (1);
      var operationCycle = query.UniqueResult<IOperationCycle> ();
      if ((null != operationCycle) && !operationCycle.End.HasValue) {
        // Make sure there is no operation cycle between the start date/time and dateTime
        Debug.Assert (operationCycle.Begin.HasValue);
        var range = new UtcDateTimeRange (operationCycle.Begin.Value, dateTime, "[]");
        var cycles = FindOverlapsRangeAscending (machine, range, 2);
        if (cycles.Any (x => x.Id != operationCycle.Id)) {
          return null;
        }
      }
      return operationCycle;
    }

    /// <summary>
    /// Get the operation cycle matching a dateTime on a given machine:
    /// operation cycle has a begin which is lower or equal to dateTime
    /// and its end (if any) is greater than dateTime
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<IOperationCycle> FindAtAsync (IMachine machine, DateTime dateTime)
    {
      IQuery query = NHibernateHelper.GetCurrentSession ()
        .GetNamedQuery ("OperationCycleAtDate");
      query.SetParameter ("Machine", machine);
      query.SetParameter ("Date", dateTime);
      query.SetMaxResults (1);
      var operationCycle = await query.UniqueResultAsync<IOperationCycle> ();
      if ((null != operationCycle) && !operationCycle.End.HasValue) {
        // Make sure there is no operation cycle between the start date/time and dateTime
        Debug.Assert (operationCycle.Begin.HasValue);
        var range = new UtcDateTimeRange (operationCycle.Begin.Value, dateTime, "[]");
        var cycles = FindOverlapsRangeAscending (machine, range, 2);
        if (cycles.Any (x => x.Id != operationCycle.Id)) {
          return null;
        }
      }
      return operationCycle;
    }

    /// <summary>
    /// Get the operation cycle matching a dateTime on a given machine:
    /// operation cycle has an end which is greater or equal to dateTime
    /// and its begin (if any) is lower than dateTime
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public IOperationCycle FindAtEnd (IMachine machine, DateTime dateTime)
    {
      IQuery query = NHibernateHelper.GetCurrentSession ()
        .GetNamedQuery ("OperationCycleAtDateEnd");
      query.SetParameter ("Machine", machine);
      query.SetParameter ("Date", dateTime);
      query.SetMaxResults (1);
      return query.UniqueResult<IOperationCycle> ();
    }

    /// <summary>
    /// Get the operation cycle matching a precise begin datetime
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public IOperationCycle FindWithBeginEqualTo (IMachine machine, DateTime dateTime)
    {
      IQuery query = NHibernateHelper.GetCurrentSession ()
        .GetNamedQuery ("OperationCycleWithBeginEqualTo");
      query.SetParameter ("Machine", machine);
      query.SetParameter ("Date", dateTime);
      query.SetMaxResults (1);
      return query.UniqueResult<IOperationCycle> ();
    }

    /// <summary>
    /// Get the operation cycle matching a precise end datetime
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public IOperationCycle FindWithEndEqualTo (IMachine machine, DateTime dateTime)
    {
      IQuery query = NHibernateHelper.GetCurrentSession ()
        .GetNamedQuery ("OperationCycleWithEndEqualTo");
      query.SetParameter ("Machine", machine);
      query.SetParameter ("Date", dateTime);
      query.SetMaxResults (1);
      return query
        .UniqueResult<IOperationCycle> ()
        .LoadOperationSlot ();
    }

    /// <summary>
    /// Get the first cycle of an operation slot
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    public IOperationCycle GetFirstCycle (IOperationSlot slot)
    {
      Debug.Assert (null != slot);

      IOperationCycle cycle = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<OperationCycle> ()
        .Add (Restrictions.Eq ("Machine", slot.Machine))
        .Add (Restrictions.Eq ("OperationSlot", slot))
        .AddOrder (Order.Asc ("DateTime"))
        .AddOrder (Order.Asc ("Id"))
        .SetMaxResults (1)
        .UniqueResult<IOperationCycle> ();
      return cycle;
    }

    /// <summary>
    /// Get the last cycle of an operation slot
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    public IOperationCycle GetLastCycle (IOperationSlot slot)
    {
      Debug.Assert (null != slot);

      IOperationCycle cycle = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<OperationCycle> ()
        .Add (Restrictions.Eq ("Machine", slot.Machine))
        .Add (Restrictions.Eq ("OperationSlot", slot))
        .AddOrder (Order.Desc ("DateTime"))
        .AddOrder (Order.Desc ("Id"))
        .SetMaxResults (1)
        .UniqueResult<IOperationCycle> ();
      return cycle;
    }

    /// <summary>
    /// Get the first cycle whose begin is before the specified date/time
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public IOperationCycle GetFirstBeginBefore (IMachine machine, Bound<DateTime> dateTime)
    {
      Debug.Assert (null != machine);

      if (dateTime.HasValue) {
        IOperationCycle cycle = NHibernateHelper.GetCurrentSession ()
          .CreateCriteria<OperationCycle> ()
          .Add (Restrictions.Eq ("Machine", machine))
          .Add (Restrictions.IsNotNull ("Begin"))
          .Add (Restrictions.Lt ("Begin", dateTime.Value))
          .AddOrder (Order.Desc ("Begin"))
          .SetMaxResults (1)
          .UniqueResult<IOperationCycle> ();
        return cycle;
      }
      else if (dateTime.BoundType.Equals (BoundType.Lower)) {
        return null;
      }
      else if (dateTime.BoundType.Equals (BoundType.Upper)) {
        return GetLast (machine);
      }
      else {
        Debug.Assert (false);
        throw new InvalidOperationException ();
      }
    }

    /// <summary>
    /// Get the first cycle whose beginning is strictly after the specified date/time
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public IOperationCycle GetFirstBeginAfter (IMachine machine, Bound<DateTime> dateTime)
    {
      Debug.Assert (null != machine);

      if (dateTime.HasValue) {
        IOperationCycle cycle = NHibernateHelper.GetCurrentSession ()
          .CreateCriteria<OperationCycle> ()
          .Add (Restrictions.Eq ("Machine", machine))
          .Add (Restrictions.IsNotNull ("Begin"))
          .Add (Restrictions.Gt ("Begin", dateTime.Value))
          .AddOrder (Order.Asc ("Begin"))
          .SetMaxResults (1)
          .UniqueResult<IOperationCycle> ();
        return cycle;
      }
      else if (dateTime.BoundType.Equals (BoundType.Lower)) {
        throw new NotImplementedException ("To be implemented if returning the first cycle of a machine is needed");
      }
      else if (dateTime.BoundType.Equals (BoundType.Upper)) {
        return null;
      }
      else {
        Debug.Assert (false);
        throw new InvalidOperationException ();
      }
    }

    /// <summary>
    /// Get the first operation cycle strictly before the specified date/time
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public IOperationCycle GetFirstStrictlyBefore (IMachine machine, Bound<DateTime> dateTime)
    {
      Debug.Assert (null != machine);

      if (dateTime.HasValue) {
        IOperationCycle cycle = NHibernateHelper.GetCurrentSession ()
          .CreateCriteria<OperationCycle> ()
          .Add (Restrictions.Eq ("Machine", machine))
          .Add (Restrictions.Lt ("DateTime", dateTime.Value))
          .AddOrder (Order.Desc ("DateTime"))
          .SetMaxResults (1)
          .UniqueResult<IOperationCycle> ();
        return cycle;
      }
      else if (dateTime.BoundType.Equals (BoundType.Lower)) {
        return null;
      }
      else if (dateTime.BoundType.Equals (BoundType.Upper)) {
        return GetLast (machine);
      }
      else {
        Debug.Assert (false);
        throw new InvalidOperationException ();
      }
    }

    /// <summary>
    /// Get the first operation cycle strictly after the specified date/time
    /// with an eager fetch of the operation slot
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public IOperationCycle GetFirstStrictlyAfter (IMachine machine, Bound<DateTime> dateTime)
    {
      Debug.Assert (null != machine);

      IOperationCycle operationCycle;
      if (dateTime.HasValue) {
        operationCycle = NHibernateHelper.GetCurrentSession ()
          .CreateCriteria<OperationCycle> ()
          .Add (Restrictions.Eq ("Machine", machine))
          .Add (Restrictions.Gt ("DateTime", dateTime.Value))
          .AddOrder (Order.Asc ("DateTime"))
          .SetMaxResults (1)
          .UniqueResult<IOperationCycle> ();
      }
      else if (dateTime.BoundType.Equals (BoundType.Lower)) {
        operationCycle = NHibernateHelper.GetCurrentSession ()
          .CreateCriteria<OperationCycle> ()
          .Add (Restrictions.Eq ("Machine", machine))
          .AddOrder (Order.Asc ("DateTime"))
          .SetMaxResults (1)
          .UniqueResult<IOperationCycle> ();
      }
      else if (dateTime.BoundType.Equals (BoundType.Upper)) {
        return null;
      }
      else {
        Debug.Assert (false);
        throw new InvalidOperationException ();
      }

      if (null != operationCycle) {
        if (!NHibernateUtil.IsInitialized (operationCycle.OperationSlot)) {
          if (log.IsDebugEnabled) {
            log.Debug ("GetFirstStrictlyAfter: " +
                       "operation slot was not loaded " +
                       "=> load it manually");
          }
          // If not initialized => not null
          // but do not check it with an assert because else it will be initialized
          IOperationSlot operationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
            .FindById (operationCycle.OperationSlot.Id, machine);
          NHibernateUtil.Initialize (operationCycle.OperationSlot);
        }
      }
      return operationCycle;
    }

    /// <summary>
    /// Find the first n full operation cycle in the range in the specified order
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="n">maximum number elements to retrieve</param>
    /// <param name="descending">False: ascending order (not implemented), True: descending order</param>
    /// <returns></returns>
    public IEnumerable<IOperationCycle> FindFirstFullOverlapsRange (IMachine machine, UtcDateTimeRange range, int n, bool descending)
    {
      Debug.Assert (null != machine);

      if (descending) {
        var before = range.Upper.HasValue ? range.Upper.Value : DateTime.UtcNow;
        var operationCycles = GetLastFullCyclesBefore (machine, before, n);
        if (range.Lower.HasValue) {
          return operationCycles.Where (c => range.ContainsElement (c.DateTime));
        }
        else {
          return operationCycles;
        }
      }
      else { // ascending
        throw new NotImplementedException ();
      }
    }

    /// <summary>
    /// Find all the slots that overlap the specified range
    /// in a descending order
    /// and with the specified step between two requests
    /// 
    /// Then the request may be interrupted without reaching the begin of the range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <param name="step"></param>
    /// <returns></returns>
    public IEnumerable<IOperationCycle> FindFullOverlapsRangeDescending (IMachine machine, UtcDateTimeRange range, int step)
    {
      return new FindFullOverlapsRangeEnumerable (this, machine, range, step, true, log);
    }

    /// <summary>
    /// FindOverlapsRangeEnumerable implementation
    /// </summary>
    class FindFullOverlapsRangeEnumerable : IEnumerable<IOperationCycle>, IEnumerable
    {
      readonly ILog m_log = LogManager.GetLogger (typeof (FindFullOverlapsRangeEnumerable).FullName);

      readonly FindFullOverlapsRangeEnumerator m_enumerator;

      public FindFullOverlapsRangeEnumerable (IOperationCycleDAO dao,
        IMachine machine,
        UtcDateTimeRange range,
        int step,
        bool descending,
        ILog logger)
      {
        Debug.Assert (null != machine);

        m_log = logger;
        m_log.DebugFormat ("FindFullOverlapsRangeEnumerable: " +
                           "range={0}",
                           range);

        m_enumerator = new FindFullOverlapsRangeEnumerator (dao,
          machine,
          range,
          step,
          descending,
          logger);
      }

      #region IEnumerable implementation
      IEnumerator<IOperationCycle> IEnumerable<IOperationCycle>.GetEnumerator ()
      {
        return m_enumerator;
      }
      #endregion
      #region IEnumerable implementation
      IEnumerator IEnumerable.GetEnumerator ()
      {
        return m_enumerator;
      }
      #endregion
    }

    class FindFullOverlapsRangeEnumerator : IEnumerator<IOperationCycle>, IEnumerator
    {
      readonly string LOWER_LIMIT_KEY = "Database.OperationCycleDAO.FindFullOverlapsRangeStep.LowerLimit";
      readonly LowerBound<DateTime> LOWER_LIMIT_DEFAULT = new DateTime (2014, 01, 01, 00, 00, 00, DateTimeKind.Utc);

      readonly ILog m_log = LogManager.GetLogger (typeof (FindFullOverlapsRangeEnumerator).FullName);

      readonly IOperationCycleDAO m_dao;
      readonly IMachine m_machine;
      readonly UtcDateTimeRange m_range;
      UtcDateTimeRange m_futureRange;
      readonly int m_step;
      readonly bool m_descending;
      bool m_initialStep = true;

      IEnumerator<IOperationCycle> m_currentEnumerator;

      public FindFullOverlapsRangeEnumerator (IOperationCycleDAO dao,
                                          IMachine machine,
                                          UtcDateTimeRange range,
                                          int step,
                                          bool descending,
                                          ILog logger)
      {
        Debug.Assert (null != dao);
        Debug.Assert (null != machine);

        m_log = logger;

        m_dao = dao;
        m_machine = machine;
        var limitRange =
          new UtcDateTimeRange (Lemoine.Info.ConfigSet.LoadAndGet<LowerBound<DateTime>> (LOWER_LIMIT_KEY,
                                                                                         LOWER_LIMIT_DEFAULT),
                                DateTime.UtcNow); // Do not use here ConfigSet, else it drives to a bug after some time
        m_range = new UtcDateTimeRange (range.Intersects (limitRange));
        m_futureRange = m_range;
        m_step = step;
        m_descending = descending;

        m_log.DebugFormat ("FindFullOverlapsRangeEnumerator: " +
                           "corrected range={0} = {1}*{2}",
                           m_range, range, limitRange);
      }

      #region IEnumerator implementation
      bool IEnumerator.MoveNext ()
      {
        if (m_futureRange.IsEmpty ()) {
          m_log.DebugFormat ("MoveNext: " +
                             "empty range => return false");
          return false;
        }

        if (!m_initialStep) {
          if (null == m_currentEnumerator) {
            m_log.WarnFormat ("MoveNext: " +
                              "current numerator is null => return false (second call)");
            return false;
          }
          else if (m_currentEnumerator.MoveNext ()) {
            return true;
          }
        }

        m_initialStep = false;
        IEnumerable<IOperationCycle> cycles = m_dao
          .FindFirstFullOverlapsRange (m_machine, m_futureRange, m_step, m_descending);
        if (cycles.Any ()) {
          m_log.DebugFormat ("MoveNext: " +
                             "{0} first slots in range {1}",
                             cycles.Count (), m_range);
          if (m_descending) {
            IOperationCycle oldestCycle = cycles.Last ();
            var oldestCycleStart = oldestCycle.Begin.HasValue ? oldestCycle.Begin.Value : oldestCycle.DateTime;
            m_futureRange = new UtcDateTimeRange (m_range.Lower, oldestCycleStart, m_range.LowerInclusive, false);
          }
          else { // Ascending
            IOperationCycle newestCycle = cycles.Last ();
            var newestCycleEnd = newestCycle.End.HasValue ? newestCycle.End.Value : newestCycle.DateTime;
            m_futureRange = new UtcDateTimeRange (newestCycleEnd, m_range.Upper, false, m_range.UpperInclusive);
          }
          m_currentEnumerator = cycles
            .GetEnumerator ();
          var moveNextResult = m_currentEnumerator.MoveNext ();
          Debug.Assert (true == moveNextResult);
          return true;
        }
        else { // No slot: return false, there is no item
          m_log.DebugFormat ("MoveNext: " +
                             "no cycle in range {0} (first) => return false",
                             m_range);
          m_currentEnumerator = null;
          return false;
        }
      }
      void IEnumerator.Reset ()
      {
        m_initialStep = true;
        m_futureRange = m_range;
      }
      object System.Collections.IEnumerator.Current
      {
        get
        {
          return m_currentEnumerator.Current;
        }
      }
      #endregion
      #region IDisposable implementation
      void IDisposable.Dispose ()
      {
        if (null != m_currentEnumerator) {
          m_currentEnumerator.Dispose ();
          m_currentEnumerator = null;
        }
      }
      #endregion
      #region IEnumerator implementation
      IOperationCycle IEnumerator<IOperationCycle>.Current
      {
        get
        {
          return m_currentEnumerator.Current;
        }
      }
      #endregion
    }

    /// <summary>
    /// Get the n next operation cycles for the specified machine after a specified date/time (including)
    /// 
    /// This is sorted by descending date/time
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="after"></param>
    /// <param name="quantity"></param>
    /// <returns></returns>
    public IList<IOperationCycle> GetNextCyclesAfter (IMachine machine, DateTime after, int quantity)
    {
      Debug.Assert (null != machine);

      IQuery query = NHibernateHelper.GetCurrentSession ()
        .GetNamedQuery ("OperationCycleForMachineAscAfter");
      query.SetParameter ("MachineId", machine.Id);
      query.SetParameter ("After", after);
      query.SetMaxResults (quantity);
      return query.List<IOperationCycle> ();
    }

    /// <summary>
    /// Find the first n operation cycle in the range in the specified order
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="n">maximum number elements to retrieve</param>
    /// <param name="descending">False: ascending order, True: descending order (not implemented)</param>
    /// <returns></returns>
    public IEnumerable<IOperationCycle> FindFirstOverlapsRange (IMachine machine, UtcDateTimeRange range, int n, bool descending)
    {
      Debug.Assert (null != machine);

      if (descending) {
        throw new NotImplementedException ();
      }
      else { // ascending
        var after = range.Lower.HasValue
          ? range.Lower.Value
          : Lemoine.Info.ConfigSet.LoadAndGet (MIN_DATE_TIME_KEY, MIN_DATE_TIME_DEFAULT);
        var operationCycles = GetNextCyclesAfter (machine, after, n);
        if (range.Upper.HasValue) {
          return operationCycles.Where (c => range.ContainsElement (c.DateTime));
        }
        else {
          return operationCycles;
        }
      }
    }

    /// <summary>
    /// Find all the cycles that overlap the specified range
    /// in a ascending order
    /// and with the specified step between two requests
    /// 
    /// Then the request may be interrupted without reaching the begin of the range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <param name="step"></param>
    /// <returns></returns>
    public IEnumerable<IOperationCycle> FindOverlapsRangeAscending (IMachine machine, UtcDateTimeRange range, int step)
    {
      return new FindOverlapsRangeEnumerable (this, machine, range, step, false, log);
    }

    /// <summary>
    /// FindOverlapsRangeEnumerable implementation
    /// </summary>
    class FindOverlapsRangeEnumerable : IEnumerable<IOperationCycle>, IEnumerable
    {
      readonly ILog m_log = LogManager.GetLogger (typeof (FindOverlapsRangeEnumerable).FullName);

      readonly FindOverlapsRangeEnumerator m_enumerator;

      public FindOverlapsRangeEnumerable (IOperationCycleDAO dao,
        IMachine machine,
        UtcDateTimeRange range,
        int step,
        bool descending,
        ILog logger)
      {
        Debug.Assert (null != machine);

        m_log = logger;
        m_log.DebugFormat ("FindOverlapsRangeEnumerable: " +
                           "range={0}",
                           range);

        m_enumerator = new FindOverlapsRangeEnumerator (dao,
          machine,
          range,
          step,
          descending,
          logger);
      }

      #region IEnumerable implementation
      IEnumerator<IOperationCycle> IEnumerable<IOperationCycle>.GetEnumerator ()
      {
        return m_enumerator;
      }
      #endregion
      #region IEnumerable implementation
      IEnumerator IEnumerable.GetEnumerator ()
      {
        return m_enumerator;
      }
      #endregion
    }

    class FindOverlapsRangeEnumerator : IEnumerator<IOperationCycle>, IEnumerator
    {
      readonly string LOWER_LIMIT_KEY = "Database.OperationCycleDAO.FindOverlapsRangeStep.LowerLimit";
      readonly LowerBound<DateTime> LOWER_LIMIT_DEFAULT = new DateTime (2014, 01, 01, 00, 00, 00, DateTimeKind.Utc);

      readonly ILog m_log = LogManager.GetLogger (typeof (FindOverlapsRangeEnumerator).FullName);

      readonly IOperationCycleDAO m_dao;
      readonly IMachine m_machine;
      readonly UtcDateTimeRange m_range;
      UtcDateTimeRange m_futureRange;
      readonly int m_step;
      readonly bool m_descending;
      bool m_initialStep = true;

      IEnumerator<IOperationCycle> m_currentEnumerator;

      public FindOverlapsRangeEnumerator (IOperationCycleDAO dao,
                                          IMachine machine,
                                          UtcDateTimeRange range,
                                          int step,
                                          bool descending,
                                          ILog logger)
      {
        Debug.Assert (null != dao);
        Debug.Assert (null != machine);

        m_log = logger;

        m_dao = dao;
        m_machine = machine;
        var limitRange =
          new UtcDateTimeRange (Lemoine.Info.ConfigSet.LoadAndGet<LowerBound<DateTime>> (LOWER_LIMIT_KEY,
                                                                                         LOWER_LIMIT_DEFAULT),
                                DateTime.UtcNow); // Do not use here ConfigSet, else it drives to a bug after some time
        m_range = new UtcDateTimeRange (range.Intersects (limitRange));
        m_futureRange = m_range;
        m_step = step;
        m_descending = descending;

        m_log.DebugFormat ("FindOverlapsRangeEnumerator: " +
                           "corrected range={0} = {1}*{2}",
                           m_range, range, limitRange);
      }

      #region IEnumerator implementation
      bool IEnumerator.MoveNext ()
      {
        if (m_futureRange.IsEmpty ()) {
          m_log.DebugFormat ("MoveNext: " +
                             "empty range => return false");
          return false;
        }

        if (!m_initialStep) {
          if (null == m_currentEnumerator) {
            m_log.WarnFormat ("MoveNext: " +
                              "current numerator is null => return false (second call)");
            return false;
          }
          else if (m_currentEnumerator.MoveNext ()) {
            return true;
          }
        }

        m_initialStep = false;
        IEnumerable<IOperationCycle> cycles = m_dao
          .FindFirstOverlapsRange (m_machine, m_futureRange, m_step, m_descending);
        if (cycles.Any ()) {
          m_log.DebugFormat ("MoveNext: " +
                             "{0} first slots in range {1}",
                             cycles.Count (), m_range);
          if (m_descending) {
            IOperationCycle oldestCycle = cycles.Last ();
            var oldestCycleStart = oldestCycle.Begin.HasValue ? oldestCycle.Begin.Value : oldestCycle.DateTime;
            m_futureRange = new UtcDateTimeRange (m_range.Lower, oldestCycleStart, m_range.LowerInclusive, false);
          }
          else { // Ascending
            IOperationCycle newestCycle = cycles.Last ();
            var newestCycleEnd = newestCycle.End.HasValue ? newestCycle.End.Value : newestCycle.DateTime;
            m_futureRange = new UtcDateTimeRange (newestCycleEnd, m_range.Upper, false, m_range.UpperInclusive);
          }
          m_currentEnumerator = cycles
            .GetEnumerator ();
          var moveNextResult = m_currentEnumerator.MoveNext ();
          Debug.Assert (true == moveNextResult);
          return true;
        }
        else { // No slot: return false, there is no item
          m_log.DebugFormat ("MoveNext: " +
                             "no cycle in range {0} (first) => return false",
                             m_range);
          m_currentEnumerator = null;
          return false;
        }
      }
      void IEnumerator.Reset ()
      {
        m_initialStep = true;
        m_futureRange = m_range;
      }
      object System.Collections.IEnumerator.Current
      {
        get
        {
          return m_currentEnumerator.Current;
        }
      }
      #endregion
      #region IDisposable implementation
      void IDisposable.Dispose ()
      {
        if (null != m_currentEnumerator) {
          m_currentEnumerator.Dispose ();
          m_currentEnumerator = null;
        }
      }
      #endregion
      #region IEnumerator implementation
      IOperationCycle IEnumerator<IOperationCycle>.Current
      {
        get
        {
          return m_currentEnumerator.Current;
        }
      }
      #endregion
    }

    /// <summary>
    /// <see cref="IOperationCycleDAO"/>
    /// </summary>
    /// <param name="operationCycle">not null</param>
    /// <returns></returns>
    public IOperationCycle LoadOperationSlot (IOperationCycle operationCycle)
    {
      IOperationSlot operationSlot = null;
      return LoadOperationSlot (operationCycle, ref operationSlot);
    }

    IOperationCycle LoadOperationSlot (IOperationCycle operationCycle, ref IOperationSlot operationSlot)
    {
      Debug.Assert (null != operationCycle);

      if ((null != operationCycle.OperationSlot) && !NHibernateUtil.IsInitialized (operationCycle.OperationSlot)) {
        if (log.IsInfoEnabled) {
          log.InfoFormat ("LoadOperationSlot: operation slot id={0} is not loaded (e.g. left join not effective) => fallback: load it manually", operationCycle.OperationSlot.Id);
        }
        // If not initialized => not null
        // but do not check it with an assert because else it will be initialized
        if ((null == operationSlot) || (operationSlot.Id != operationCycle.OperationSlot.Id)) {
          operationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
            .FindById (operationCycle.OperationSlot.Id, operationCycle.Machine);
          if (null == operationSlot) {
            log.FatalFormat ("LoadOperationSlot: operation slot is={0} was not found for machine={1}", operationCycle.OperationSlot.Id, operationCycle.Machine.Id);
          }
          else { // null != operationSlot
            if (log.IsDebugEnabled) {
              log.DebugFormat ("LoadOperationSlot: operationSlot id={0} was loaded. Initialize now operationCycle.OperationSlot={1}",
                operationCycle.OperationSlot.Id, operationCycle.Id);
            }
          }
          NHibernateUtil.Initialize (operationCycle.OperationSlot);
          if (log.IsDebugEnabled) {
            log.DebugFormat ("LoadOperationSlot: operationCycle.OperationSlot initialized");
          }
        }
        Debug.Assert (NHibernate.NHibernateUtil.IsInitialized (operationCycle.OperationSlot),
                      "operationCycle.OperationSlot not initialized");
      }
      return operationCycle;
    }

    /// <summary>
    /// <see cref="IOperationCycleDAO"/>
    /// </summary>
    /// <param name="operationCycles"></param>
    /// <returns></returns>
    public IEnumerable<IOperationCycle> LoadOperationSlots (IEnumerable<IOperationCycle> operationCycles)
    {
      IOperationSlot operationSlot = null;
      foreach (var operationCycle in operationCycles) {
        yield return LoadOperationSlot (operationCycle, ref operationSlot);
      }
    }
  }
}
