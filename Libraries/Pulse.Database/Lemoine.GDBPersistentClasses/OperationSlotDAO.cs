// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
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
using Lemoine.Business.Config;
using System.Threading.Tasks;
using Lemoine.Collections;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IOperationSlotDAO">IOperationSlotDAO</see>
  /// </summary>
  public sealed class OperationSlotDAO
    : GenericMachineRangeSlotDAO<OperationSlot, IOperationSlot>
    , IOperationSlotDAO
  {
    static readonly string CURRENT_OPERATION_SLOT_MARGIN = "CurrentOperationSlotMargin";
    static readonly TimeSpan CURRENT_OPERATION_SLOT_DEFAULT = TimeSpan.FromSeconds (30);

    static readonly string FIND_BY_MO_STRICTLY_BEFORE_MAX_RANGE_DURATION_KEY = "OperationSlotDAO.FindByManufacturingOrderStrictlyBefore.MaxRangeDuration";
    static readonly TimeSpan FIND_BY_MO_STRICTLY_BEFORE_MAX_RANGE_DURATION_DEFAULT = TimeSpan.FromDays (14);

    readonly ILog log = LogManager.GetLogger (typeof (OperationSlotDAO).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    internal OperationSlotDAO ()
      : base (true, false, true, true, false)
    {
    }

    /// <summary>
    /// Override MakePersistent to track its transient status change
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override IOperationSlot MakePersistent (IOperationSlot entity)
    {
      OperationSlot operationSlot = (OperationSlot)entity;
      IOperationSlot result;
      using (var changeTracker = new OperationSlot.ChangeTracker (operationSlot)) {
        result = base.MakePersistent (entity);
        operationSlot.SetPersistent ();
      }
      return result;
    }

    /// <summary>
    /// Override MakePersistent to track its transient status change
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override async Task<IOperationSlot> MakePersistentAsync (IOperationSlot entity)
    {
      OperationSlot operationSlot = (OperationSlot)entity;
      IOperationSlot result;
      using (var changeTracker = new OperationSlot.ChangeTracker (operationSlot)) {
        result = await base.MakePersistentAsync (entity);
        operationSlot.SetPersistent ();
      }
      return result;
    }

    /// <summary>
    /// Override MakeTransient to track its transient status change
    /// </summary>
    /// <param name="entity"></param>
    public override void MakeTransient (IOperationSlot entity)
    {
      OperationSlot operationSlot = (OperationSlot)entity;
      using (var changeTracker = new OperationSlot.ChangeTracker (operationSlot)) {
        base.MakeTransient (entity);
        operationSlot.SetTransient ();
      }
    }

    /// <summary>
    /// Override MakeTransient to track its transient status change
    /// </summary>
    /// <param name="entity"></param>
    public override async System.Threading.Tasks.Task MakeTransientAsync (IOperationSlot entity)
    {
      OperationSlot operationSlot = (OperationSlot)entity;
      using (var changeTracker = new OperationSlot.ChangeTracker (operationSlot)) {
        await base.MakeTransientAsync (entity);
        operationSlot.SetTransient ();
      }
    }

    /// <summary>
    /// Find all operation slots in a given time range with an eager fetch of the work order, component, operation
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IList<IOperationSlot> FindOverlapsRangeWithEagerFetch (IMachine machine,
                                                                  UtcDateTimeRange range)
    {
      Debug.Assert (null != machine);

      if (range.IsEmpty ()) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("FindOverlapsRangeWithEagerFetch: " +
                           "empty range " +
                           "=> return an empty list. " +
                           "StackTrace: {0}",
                           System.Environment.StackTrace);
        }
        return new List<IOperationSlot> ();
      }

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<OperationSlot> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (OverlapRange (range))
        .Fetch (SelectMode.Fetch, "WorkOrder")
        .Fetch (SelectMode.Fetch, "Component")
        .Fetch (SelectMode.Fetch, "Operation")
        .AddOrder (Order.Asc ("DateTimeRange"))
        .List<IOperationSlot> ();
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IOperationSlotDAO.GetOperationSlotAtConsideringEnd" />
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="at"></param>
    /// <returns></returns>
    public IOperationSlot GetOperationSlotAtConsideringEnd (IMachine machine,
                                                            Bound<DateTime> at)
    {
      Debug.Assert (null != machine);

      if (at.HasValue) {
        DateTime utc;
        switch (at.Value.Kind) {
        case DateTimeKind.Utc:
          utc = at.Value;
          break;
        case DateTimeKind.Local:
          utc = at.Value.ToUniversalTime ();
          break;
        case DateTimeKind.Unspecified:
        default:
          log.ErrorFormat ("FindAt: " +
                           "date/time {0} is of kind Unspecified",
                           at);
          Debug.Assert (DateTimeKind.Unspecified != at.Value.Kind);
          utc = at.Value;
          break;
        }
        return NHibernateHelper.GetCurrentSession ()
          .CreateCriteria<OperationSlot> ()
          .Add (Restrictions.Eq ("Machine.Id", machine.Id))
          // Note: new SimpleExpression ("DateTimeRange", dateTime, "@>") does not work because it compares object of different types
          // The returned error is: Type mismatch in NHibernate.Criterion.SimpleExpression: DateTimeRange expected type Lemoine.Model.UtcDateTimeRange, actual type System.DateTime
          .Add (new SimpleTypedExpression ("DateTimeRange", new Lemoine.NHibernateTypes.UTCDateTimeFullType (), utc.AddSeconds (-1), "@>"))
          .UniqueResult<IOperationSlot> ();
      }
      else { // !at.HasValue
        switch (at.BoundType) {
        case BoundType.Lower:
          return NHibernateHelper.GetCurrentSession ()
            .CreateCriteria<OperationSlot> ()
            .Add (Restrictions.Eq ("Machine", machine))
            .Add (new FunctionExpression ("DateTimeRange", "lower_inf"))
            .UniqueResult<IOperationSlot> ();
        case BoundType.Upper:
          return NHibernateHelper.GetCurrentSession ()
            .CreateCriteria<OperationSlot> ()
            .Add (Restrictions.Eq ("Machine", machine))
            .Add (new FunctionExpression ("DateTimeRange", "upper_inf"))
            .UniqueResult<IOperationSlot> ();
        default:
          log.FatalFormat ("Unexpected BoundType");
          throw new InvalidOperationException ("Invalid BoundType");
        }
      }
    }

    /// <summary>
    /// Get the first operation slot with a different operation after the specified date/time
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="dateTime"></param>
    /// <param name="operation"></param>
    /// <returns></returns>
    public IOperationSlot GetFirstDifferentOperationAfter (IMachine machine,
                                                           DateTime dateTime,
                                                           IOperation operation)
    {
      Debug.Assert (null != machine);
      Debug.Assert (null != operation);

      var upperProjection = GetUpperProjection ();
      var result = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<OperationSlot> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (Restrictions.Not (Restrictions.Eq ("Operation.Id", ((IDataWithId)operation).Id)))
        .Add (Restrictions.Gt (upperProjection, dateTime))
        .Add (OverlapRange (new UtcDateTimeRange (dateTime)))
        .AddOrder (Order.Asc (upperProjection))
        .SetMaxResults (1)
        .UniqueResult<IOperationSlot> ();
      if (null != result) {
        return result;
      }
      else { // null == result => with upper_inf
        return NHibernateHelper.GetCurrentSession ()
          .CreateCriteria<OperationSlot> ()
          .Add (Restrictions.Eq ("Machine.Id", machine.Id))
          .Add (Restrictions.Not (Restrictions.Eq ("Operation.Id", ((IDataWithId)operation).Id)))
          .Add (IsUpperInfinite ())
          .Add (OverlapRange (new UtcDateTimeRange (dateTime)))
          .UniqueResult<IOperationSlot> ();
      }
    }

    /// <summary>
    /// Get the last operation slot, which operation is not null
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    public IOperationSlot GetLastOperationNotNull (IMachine machine)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<OperationSlot> ()
        .Add (Restrictions.Eq ("Machine", machine))
        .Add (Restrictions.IsNotNull ("Operation"))
        .AddOrder (Order.Desc (GetUpperProjection ()))
        .SetMaxResults (1)
        .UniqueResult<IOperationSlot> ();
    }

    /// <summary>
    /// Get the last operation slot strictly before a specified date/time, which operation is not null
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public IOperationSlot GetLastOperationNotNullBefore (IMachine machine, DateTime dateTime)
    {
      var range = new UtcDateTimeRange (new LowerBound<DateTime> (null), dateTime);
      var result = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<OperationSlot> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (Restrictions.IsNotNull ("Operation"))
        .Add (IsUpperInfinite ())
        .Add (OverlapRange (range))
        .UniqueResult<IOperationSlot> ();
      if (null != result) {
        return result;
      }
      else { // with upper_inf, not valid
        var lowerProjection = GetLowerProjection ();
        return NHibernateHelper.GetCurrentSession ()
          .CreateCriteria<OperationSlot> ()
          .Add (Restrictions.Eq ("Machine", machine))
          .Add (Restrictions.IsNotNull ("Operation"))
          .Add (OverlapRange (range))
          .Add (Restrictions.Lt (lowerProjection, dateTime))
          .AddOrder (Order.Desc (lowerProjection)) // ok because never null
          .SetMaxResults (1)
          .UniqueResult<IOperationSlot> ();
      }
    }

    /// <summary>
    /// Get the last operation slot whose begin date/time is strictly before a specified date/time
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public IOperationSlot GetLastStrictlyBefore (IMachine machine, DateTime dateTime)
    {
      var range = new UtcDateTimeRange (new LowerBound<DateTime> (null), dateTime);
      var result = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<OperationSlot> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (IsUpperInfinite ())
        .Add (OverlapRange (range))
        .UniqueResult<IOperationSlot> ();
      if (null != result) {
        return result;
      }
      else { // with upper_inf, not valid
        var lowerProjection = GetLowerProjection ();
        return NHibernateHelper.GetCurrentSession ()
          .CreateCriteria<OperationSlot> ()
          .Add (Restrictions.Eq ("Machine", machine))
          .Add (OverlapRange (range))
          .Add (Restrictions.Lt (lowerProjection, dateTime))
          .AddOrder (Order.Desc (lowerProjection)) // ok because never null
          .SetMaxResults (1)
          .UniqueResult<IOperationSlot> ();
      }
    }

    /// <summary>
    /// Get the last operation slot whose begin date/time is (not strictly) before a specified date/time
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    internal IOperationSlot GetLastBefore (IMachine machine, DateTime dateTime)
    {
      var range = new UtcDateTimeRange (new LowerBound<DateTime> (null), dateTime, "(]");
      var result = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<OperationSlot> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (IsUpperInfinite ())
        .Add (OverlapRange (range))
        .UniqueResult<IOperationSlot> ();
      if (null != result) {
        return result;
      }
      else { // with upper_inf, not valid
        var lowerProjection = GetLowerProjection ();
        return NHibernateHelper.GetCurrentSession ()
          .CreateCriteria<OperationSlot> ()
          .Add (Restrictions.Eq ("Machine", machine))
          .Add (OverlapRange (range))
          .Add (Restrictions.Le (lowerProjection, dateTime))
          .AddOrder (Order.Desc (lowerProjection)) // ok because never null
          .SetMaxResults (1)
          .UniqueResult<IOperationSlot> ();
      }
    }

    /// <summary>
    /// Get the next operation slot with a not null operation
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    internal IOperationSlot GetNextNotNullOperation (IMachine machine, DateTime dateTime)
    {
      Debug.Assert (null != machine);

      var range = new UtcDateTimeRange (dateTime);
      var upperProjection = GetUpperProjection ();
      var result = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<OperationSlot> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (Restrictions.IsNotNull ("Operation"))
        .Add (Restrictions.Gt (upperProjection, dateTime))
        .Add (OverlapRange (range))
        .AddOrder (Order.Asc (upperProjection))
        .SetMaxResults (1)
        .UniqueResult<IOperationSlot> ();
      if (null != result) {
        return result;
      }
      else { // null == result => with upper_inf
        return NHibernateHelper.GetCurrentSession ()
          .CreateCriteria<OperationSlot> ()
          .Add (Restrictions.Eq ("Machine.Id", machine.Id))
          .Add (Restrictions.IsNotNull ("Operation"))
          .Add (IsUpperInfinite ())
          .Add (OverlapRange (range))
          .UniqueResult<IOperationSlot> ();
      }
    }

    /// <summary>
    /// Does an operation slot exist in a given time range ?
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public bool ExistsInRange (IMachine machine,
                               UtcDateTimeRange range)
    {
      return null != NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<OperationSlot> ()
        .Add (Restrictions.Eq ("Machine", machine))
        .Add (OverlapRange (range))
        .SetMaxResults (1)
        .UniqueResult<IOperationSlot> ();
    }

    /// <summary>
    /// Find all operation slots in a given time range, ordered by ascending begin datetime.
    /// with the specified properties.
    /// 
    /// If one property is null, it must be null in the database as well
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="operation"></param>
    /// <returns></returns>
    public IList<IOperationSlot> FindAllInRangeWith (IMachine machine,
                                                     UtcDateTimeRange range,
                                                     IOperation operation)
    {
      Debug.Assert (null != machine);

      if (range.IsEmpty ()) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("FindAllInRangeWith: " +
                           "empty range " +
                           "=> return an empty list. " +
                           "StackTrace: {0}",
                           System.Environment.StackTrace);
        }
        return new List<IOperationSlot> ();
      }

      AbstractCriterion operationCriterion;
      if (null == operation) {
        operationCriterion = Restrictions.IsNull ("Operation");
      }
      else {
        operationCriterion = Restrictions.Eq ("Operation.Id", ((IDataWithId)operation).Id);
      }

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<OperationSlot> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (OverlapRange (range))
        .Add (operationCriterion)
        .AddOrder (Order.Asc ("DateTimeRange"))
        .List<IOperationSlot> ();
    }

    /// <summary>
    /// Find all the operation slots for the specified day and shift
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="day"></param>
    /// <param name="shift"></param>
    /// <returns></returns>
    public IList<IOperationSlot> FindByDayShift (IMachine machine,
                                                 DateTime day,
                                                 IShift shift)
    {
      ICriteria criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<OperationSlot> ()
        .Add (Restrictions.Eq ("Machine", machine))
        .Add (InDayRange (new DayRange (day, day)))
        .Add (Restrictions.Eq ("Day", day));
      if (null == shift) {
        criteria = criteria.Add (Restrictions.IsNull ("Shift"));
      }
      else {
        criteria = criteria.Add (Restrictions.Eq ("Shift", shift));
      }
      criteria = criteria
        .AddOrder (Order.Asc ("DateTimeRange"));
      return criteria.List<IOperationSlot> ();
    }

    /// <summary>
    /// Find all the operation slots for the specified day and shift
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="day"></param>
    /// <param name="shift"></param>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<IList<IOperationSlot>> FindByDayShiftAsync (IMachine machine,
                                                 DateTime day,
                                                 IShift shift)
    {
      ICriteria criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<OperationSlot> ()
        .Add (Restrictions.Eq ("Machine", machine))
        .Add (InDayRange (new DayRange (day, day)))
        .Add (Restrictions.Eq ("Day", day));
      if (null == shift) {
        criteria = criteria.Add (Restrictions.IsNull ("Shift"));
      }
      else {
        criteria = criteria.Add (Restrictions.Eq ("Shift", shift));
      }
      criteria = criteria
        .AddOrder (Order.Asc ("DateTimeRange"));
      return await criteria.ListAsync<IOperationSlot> ();
    }

    /// <summary>
    /// Get the first operation slot whose begin date/time in strictly the specified period (begin is excluded)
    /// 
    /// null is returned is no such operation slot was found
    /// 
    /// The returned operation slot must start after begin
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="begin">in UTC</param>
    /// <param name="end">in UTC</param>
    /// <returns></returns>
    public IOperationSlot GetFirstBeginStrictlyBetween (IMachine machine,
                                                        DateTime begin,
                                                        DateTime end)
    {
      if (end <= begin) { // Empty period
        return null;
      }

      var lowerProjection = GetLowerProjection ();
      var result = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<OperationSlot> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (Restrictions.Gt (lowerProjection, begin))
        .Add (Restrictions.Lt (lowerProjection, end))
        .AddOrder (Order.Asc (lowerProjection))
        .SetMaxResults (1)
        .UniqueResult<IOperationSlot> ();
      return result;
    }

    /// <summary>
    /// Check if it exists an operation slot with a different operation
    /// in the specified period
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="begin">in UTC</param>
    /// <param name="end">in UTC</param>
    /// <param name="operation">null or not null. If null, return true if there is a not null operation in the specified range</param>
    /// <returns></returns>
    public bool ExistsDifferentOperationBetween (IMachine machine,
                                                 DateTime begin,
                                                 DateTime end,
                                                 IOperation operation)
    {
      return ExistsDifferentOperationBetween (machine, new UtcDateTimeRange (begin, end), operation);
    }

    /// <summary>
    /// Check if it exists an operation slot with a different operation
    /// in the specified period
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range">in UTC</param>
    /// <param name="operation">null or not null. If null, return true if there is a not null operation in the specified range</param>
    /// <returns></returns>
    public bool ExistsDifferentOperationBetween (IMachine machine,
                                                 UtcDateTimeRange range,
                                                 IOperation operation)
    {
      if (range.IsEmpty ()) { // Empty period
        return false;
      }

      var criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<OperationSlot> ()
        .Add (Restrictions.Eq ("Machine", machine));
      if (null == operation) {
        criteria = criteria.Add (Restrictions.IsNotNull ("Operation"));
      }
      else {
        criteria = criteria.Add (Restrictions.Not (Restrictions.Eq ("Operation", operation)));
      }
      IOperationSlot anyOperationSlot = criteria
        .Add (OverlapRange (range))
        .SetMaxResults (1)
        .UniqueResult<IOperationSlot> ();
      return (null != anyOperationSlot);
    }

    /// <summary>
    /// Check if it exists a period of time with a different operation or component or work order
    /// in the specified period
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="begin">in UTC</param>
    /// <param name="end">in UTC</param>
    /// <param name="operation"></param>
    /// <param name="component"></param>
    /// <param name="workOrder"></param>
    /// <returns></returns>
    public bool ExistsDifferentWorkOrderComponentOperationBetween (IMachine machine,
                                                                   DateTime begin,
                                                                   DateTime end,
                                                                   IOperation operation,
                                                                   IComponent component,
                                                                   IWorkOrder workOrder)
    {
      if (end <= begin) { // Empty period
        return false;
      }

      Bound<DateTime> currentDateTime = begin;
      IList<IOperationSlot> slots = FindOverlapsRange (machine, new UtcDateTimeRange (begin, end));
      foreach (IOperationSlot slot in slots) {
        if (currentDateTime.HasValue && (Bound.Compare<DateTime> (currentDateTime, slot.BeginDateTime) < 0)) { // Gap
          log.DebugFormat ("ExistsDifferentWorkOrderComponentOperationBetween: " +
                           "gap in operation slots => return true");
          return true;
        }
        currentDateTime = slot.EndDateTime;
        if (!object.Equals (slot.Operation, operation)
            || !object.Equals (slot.Component, component)
            || !object.Equals (slot.WorkOrder, workOrder)) {
          log.DebugFormat ("ExistsDifferentWorkOrderComponentOperationBetween: " +
                           "operation or component or work order differs in slot {0}",
                           slot);
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Is there the same specified operation in whole specified range
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="operation"></param>
    /// <returns></returns>
    public bool IsContinuousOperationInRange (IMachine machine,
                                              UtcDateTimeRange range,
                                              IOperation operation)
    {
      if (range.IsEmpty ()) {
        return true;
      }

      bool existsDifferentOperation = ExistsDifferentOperationBetween (machine, range, operation);
      if (existsDifferentOperation) {
        log.DebugFormat ("IsContinuousOperationInRange: " +
                         "there is a different operation => return false");
        return false;
      }
      if (null != operation) { // Check it is continous
        var slots = FindAllInRangeWith (machine, range, operation);
        if (!slots.Any ()) {
          return false;
        }
        Bound<DateTime> currentBound = range.Lower;
        foreach (var slot in slots) {
          if (!slot.DateTimeRange.ContainsElement (currentBound)) {
            log.DebugFormat ("IsContinousOperationInRange: " +
                             "discontinuation");
            return false;
          }
          currentBound = slot.DateTimeRange.Upper;
        }
      }
      return true;
    }

    /// <summary>
    /// Find all the operation slots for the specified operation
    /// </summary>
    /// <param name="operation"></param>
    /// <returns></returns>
    public IList<IOperationSlot> FindByOperation (IOperation operation)
    {
      IList<IOperationSlot> slots = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<OperationSlot> ()
        .Add (Restrictions.Eq ("Operation", operation))
        .List<IOperationSlot> ();
      return slots;
    }

    /// <summary>
    /// Find all the operation slots for the specified component
    /// </summary>
    /// <param name="component"></param>
    /// <returns></returns>
    public IList<IOperationSlot> FindByComponent (IComponent component)
    {
      IList<IOperationSlot> slots = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<OperationSlot> ()
        .Add (Restrictions.Eq ("Component", component))
        .List<IOperationSlot> ();
      return slots;
    }

    /// <summary>
    /// Find all the operation slots for the specified workOrder
    /// </summary>
    /// <param name="workOrder"></param>
    /// <returns></returns>
    public IList<IOperationSlot> FindByWorkOrder (IWorkOrder workOrder)
    {
      IList<IOperationSlot> slots = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<OperationSlot> ()
        .Add (Restrictions.Eq ("WorkOrder", workOrder))
        .List<IOperationSlot> ();
      return slots;
    }

    /// <summary>
    /// Find all the operation slots for the specified manufacturing order that are strictly before a specified date/time
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="manfacturingOrder">not null</param>
    /// <param name="before">in UTC</param>
    /// <returns></returns>
    public IList<IOperationSlot> FindByManufacturingOrderStrictlyBefore (IMachine machine, IManufacturingOrder manfacturingOrder, DateTime before)
    {
      Debug.Assert (null != machine);
      Debug.Assert (null != manfacturingOrder);

      var maxRangeDuration = Lemoine.Info.ConfigSet.LoadAndGet (FIND_BY_MO_STRICTLY_BEFORE_MAX_RANGE_DURATION_KEY, FIND_BY_MO_STRICTLY_BEFORE_MAX_RANGE_DURATION_DEFAULT);
      var range = new UtcDateTimeRange (before.Subtract (maxRangeDuration), before);
      var slots = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<OperationSlot> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (Restrictions.Eq ("ManufacturingOrder.Id", manfacturingOrder.Id))
        .Add (OverlapRange (range))
        .AddOrder (Order.Desc ("DateTimeRange"))
        .List<IOperationSlot> ();
      return slots;
    }

    /// <summary>
    /// Find all the operation slots for the specified manufacturing order
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="manufacturingOrder"></param>
    /// <returns></returns>
    public IList<IOperationSlot> FindByManufacturingOrder (IMachine machine, IManufacturingOrder manufacturingOrder)
    {
      IList<IOperationSlot> slots = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<OperationSlot> ()
        .Add (Restrictions.Eq ("Machine", machine))
        .Add (Restrictions.Eq ("ManufacturingOrder", manufacturingOrder))
        .List<IOperationSlot> ();
      return slots;
    }

    /// <summary>
    /// Find all the operation slots for the specified manufacturing order
    /// </summary>
    /// <param name="manufacturingOrder"></param>
    /// <returns></returns>
    public IList<IOperationSlot> FindByManufacturingOrder (IManufacturingOrder manufacturingOrder)
    {
      IList<IOperationSlot> slots = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<OperationSlot> ()
        .Add (Restrictions.Eq ("ManufacturingOrder", manufacturingOrder))
        .List<IOperationSlot> ();
      return slots;
    }

    /// <summary>
    /// Find all the operation slots for the specified work order and component
    /// </summary>
    /// <param name="workOrder"></param>
    /// <param name="component"></param>
    /// <returns></returns>
    public IList<IOperationSlot> FindByWorkOrderComponent (IWorkOrder workOrder, IComponent component)
    {
      IList<IOperationSlot> slots = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<OperationSlot> ()
        .Add (Restrictions.Eq ("WorkOrder", workOrder))
        .Add (Restrictions.Eq ("Component", component))
        .List<IOperationSlot> ();
      return slots;
    }

    /// <summary>
    /// Find all the operation slots for the specified component and operation
    /// </summary>
    /// <param name="component"></param>
    /// <param name="operation"></param>
    /// <returns></returns>
    public IList<IOperationSlot> FindByComponentOperation (IComponent component, IOperation operation)
    {
      IList<IOperationSlot> slots = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<OperationSlot> ()
        .Add (Restrictions.Eq ("Component", component))
        .Add (Restrictions.Eq ("Operation", operation))
        .List<IOperationSlot> ();
      return slots;
    }

    /// <summary>
    /// Get the last effective operation slot (that is effective now)
    /// </summary>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    public IOperationSlot GetLastEffective (IMachine machine)
    {
      return GetLastEffective (machine, new LowerBound<DateTime> (null));
    }

    /// <summary>
    /// Get the last effective operation slot (that is effective now)
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="lastPeriodDuration">Period before now that is taken into account</param>
    /// <returns></returns>
    public IOperationSlot GetLastEffective (IMachine machine, TimeSpan lastPeriodDuration)
    {
      return GetLastEffective (machine, DateTime.UtcNow.Subtract (lastPeriodDuration));
    }

    /// <summary>
    /// Get the last effective operation slot (that is effective now)
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="after">after a specific date/time. Default: -oo</param>
    /// <returns></returns>
    public IOperationSlot GetLastEffective (IMachine machine, LowerBound<DateTime> after)
    {
      Debug.Assert (null != machine);

      IOperationSlot virtualOperationSlot;
      var effectiveOperationSlots = GetEffective (machine, out virtualOperationSlot, after, true, false,
                                                  DateTime.UtcNow);
      return effectiveOperationSlots.LastOrDefault ();
    }

    /// <summary>
    /// Get the effective operation slots (that are effective now)
    /// </summary>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    public IList<IOperationSlot> GetEffective (IMachine machine)
    {
      return GetEffective (machine, new LowerBound<DateTime> (null));
    }

    /// <summary>
    /// Get the effective operation slots (that are effective now)
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="after">after a specific date/time. Default: -oo</param>
    /// <returns></returns>
    public IList<IOperationSlot> GetEffective (IMachine machine,
                                               LowerBound<DateTime> after)
    {
      IOperationSlot virtualOperationSlot;
      return GetEffective (machine, out virtualOperationSlot, after);
    }

    /// <summary>
    /// Get the effective operation slots (that are effective now)
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="virtualOperationSlot">with no day and no shift</param>
    /// <returns></returns>
    public IList<IOperationSlot> GetEffective (IMachine machine, out IOperationSlot virtualOperationSlot)
    {
      return GetEffective (machine, out virtualOperationSlot, new LowerBound<DateTime> (null), false, false,
                           DateTime.UtcNow);
    }

    /// <summary>
    /// Get the effective operation slots (that are effective now)
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="virtualOperationSlot">with no day and no shift</param>
    /// <param name="after">after a specific date/time. Default: -oo</param>
    /// <returns></returns>
    public IList<IOperationSlot> GetEffective (IMachine machine, out IOperationSlot virtualOperationSlot,
                                               LowerBound<DateTime> after)
    {
      return GetEffective (machine, out virtualOperationSlot, after, false, false,
                           DateTime.UtcNow);
    }

    /// <summary>
    /// Get the effective operation of the current shift
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="virtualOperationSlot">with the day and shift if found</param>
    /// <returns></returns>
    public IList<IOperationSlot> GetEffectiveCurrentShift (IMachine machine, out IOperationSlot virtualOperationSlot)
    {
      return GetEffectiveCurrentShift (machine, out virtualOperationSlot, DateTime.UtcNow);
    }

    /// <summary>
    /// Get the effective operation of the current shift
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="virtualOperationSlot">with the day and shift if found</param>
    /// <param name="dateTime">at this specific date/time</param>
    /// <returns></returns>
    public IList<IOperationSlot> GetEffectiveCurrentShift (IMachine machine, out IOperationSlot virtualOperationSlot, DateTime dateTime)
    {
      DateTime after = dateTime.Subtract (TimeSpan.FromHours (12));
      IList<IOperationSlot> operationSlots = GetEffective (machine, out virtualOperationSlot, after, false, true,
                                                           dateTime);
      if (operationSlots.Any ()) {
        if (operationSlots.Any () && operationSlots.Last ().DateTimeRange.ContainsElement (dateTime)) {
          return operationSlots;
        }
        else { // Check the current day / shift
          var day = virtualOperationSlot.Day;
          var shift = virtualOperationSlot.Shift;

          if (!day.HasValue) {
            log.Debug ("GetEffectiveCurrentShift: no day is defined in operation slot => return an empty list");
            virtualOperationSlot = null;
            return new List<IOperationSlot> ();
          }

          if (shift is null) {
            log.Debug ("GetEffectiveCurrentShift: no shift is defined in operation slot => return an empty list");
            virtualOperationSlot = null;
            return new List<IOperationSlot> ();
          }

          Debug.Assert (null != shift);
          if (AnalysisConfigHelper.OperationSlotSplitOption
              .HasFlag (OperationSlotSplitOption.ByGlobalShift)) {
            IShiftSlot shiftSlot = ModelDAOHelper.DAOFactory.ShiftSlotDAO
              .FindAt (dateTime);
            if ((null == shiftSlot) || !shiftSlot.Day.HasValue || (null == shiftSlot.Shift)
                || !object.Equals (shiftSlot.Shift, shift) || !object.Equals (shiftSlot.Day, day)) {
              log.WarnFormat ("GetEffectiveCurrentShift: " +
                              "current shift slot does not match retrieved shift and day, processing delay ? " +
                              "machineid={0} dateTime={1}", machine.Id, dateTime);
              virtualOperationSlot = null;
              return new List<IOperationSlot> ();
            }
          }
          else if (AnalysisConfigHelper.OperationSlotSplitOption
                   .HasFlag (OperationSlotSplitOption.ByMachineShift)) {
            var observationStateSlot = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
              .FindAt (machine, dateTime);
            if ((null == observationStateSlot) || (null == observationStateSlot.Shift)
                || !object.Equals (observationStateSlot.Shift, shift)) {
              log.WarnFormat ("GetEffectiveCurrentShift: " +
                              "operation shift does not match observation state slot shift, processing delay ? " +
                              "machineid={0} dateTime={1}", machine.Id, dateTime);
              virtualOperationSlot = null;
              return new List<IOperationSlot> ();
            }

            Debug.Assert (day.HasValue);
            IDaySlot daySlot = ModelDAOHelper.DAOFactory.DaySlotDAO.FindProcessedByDay (day.Value);
            if (!daySlot.DateTimeRange.ContainsElement (dateTime)) {
              log.WarnFormat ("GetEffectiveCurrentShift: " +
                              "day {0} is not the current day, processing delay ?" +
                              "=> return an empty list, machineid={1} dateTime={2}",
                              day.Value, machine.Id, dateTime);
              virtualOperationSlot = null;
              return new List<IOperationSlot> ();
            }
          }
          else if (AnalysisConfigHelper.OperationSlotSplitOption.Equals (OperationSlotSplitOption.ByDay)) {
            Debug.Assert (day.HasValue);
            var daySlot = ModelDAOHelper.DAOFactory.DaySlotDAO
              .FindProcessedByDay (day.Value);
            if (!daySlot.DateTimeRange.ContainsElement (dateTime)) {
              log.Warn ($"GetEffectiveCurrentShift: day {day.Value} is not the current day, processing delay ? => return an empty list, machineid={machine.Id} dateTime={dateTime}");
              virtualOperationSlot = null;
              return new List<IOperationSlot> ();
            }
          }
          else {
            log.Error ("GetEffectiveCurrentShift: no split by shift or day");
            virtualOperationSlot = null;
            return new List<IOperationSlot> ();
          }

          log.Debug ("GetEffectiveCurrentShift: retrieved operation slots are ok");
          return operationSlots;
        }
      }
      else { // No operation slots: the day/shift must be retrieved separately
        return operationSlots;
      }
    }

    /// <summary>
    /// Get the effective operation slots (that are effective now)
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="virtualOperationSlot">with no day and no shift if restrictLastShift is false</param>
    /// <param name="after">after a specific date/time. Default: -oo</param>
    /// <param name="onlyLast">Get only the last operation slot</param>
    /// <param name="restrictSingleShift">restrict the data to a single day/shift</param>
    /// <param name="dateTime">at this specific date/time</param>
    /// <returns></returns>
    IList<IOperationSlot> GetEffective (IMachine machine, out IOperationSlot virtualOperationSlot,
                                        LowerBound<DateTime> after,
                                        bool onlyLast,
                                        bool restrictSingleShift,
                                        DateTime dateTime)
    {
      Debug.Assert (null != machine);

      DateTime? day = null;
      IShift shift = null;
      IManufacturingOrder manufacturingOrder = null;
      ILine line = null;
      IWorkOrder workOrder = null;
      IComponent component = null;
      IOperation operation = null;
      IList<IOperationSlot> effectiveSlots = new List<IOperationSlot> ();

      log.DebugFormat ("GetEffective: " +
                       "after={0} onlyLast={1} restructSingleShift={2} dateTime={3}",
                       after, onlyLast, restrictSingleShift, dateTime);

      UtcDateTimeRange range = new UtcDateTimeRange (after, dateTime);
      bool first = true;
      foreach (var operationSlot in FindOverlapsRangeDescending (machine, range, TimeSpan.FromDays (1))) {
        log.DebugFormat ("GetEffective: " +
                         "check the validity of operation slot id={0}",
                         operationSlot.Id);
        // - day/shift
        if (restrictSingleShift) {
          if (first) {
            day = operationSlot.Day;
            shift = operationSlot.Shift;
            first = false;
          }
          else { // Not first
            if (!object.Equals (day, operationSlot.Day) || !object.Equals (shift, operationSlot.Shift)) {
              log.Debug ("GetEffective: " +
                         "another day or shift, stop here");
              break; // Another day or shift => stop here
            }
          }
        }
        // - manufacturingOrder
        if ((null != operationSlot.ManufacturingOrder)
            && (null != operationSlot.Operation) // To avoid any problem when a manufacturing order is not associated to an operation
            && operationSlot.DateTimeRange.ContainsElement (dateTime)) { // the manufacturing orders must be defined until now
          manufacturingOrder = operationSlot.ManufacturingOrder;
          workOrder = operationSlot.WorkOrder;
          component = operationSlot.Component;
          operation = operationSlot.Operation;
          log.DebugFormat ("GetEffective: " +
                           "one manufcaturing order is defined, include this operation slot");
          effectiveSlots.Insert (0, operationSlot);
          if (onlyLast) {
            log.Debug ("GetEffective: " +
                       "only the last one, stop here");
            break;
          }
          else {
            continue;
          }
        }
        if (null != manufacturingOrder) {
          if (!object.Equals (manufacturingOrder, operationSlot.ManufacturingOrder)) {
            log.Debug ("GetEffective: " +
                       "another manufacturing order, stop here");
            break; // Another manufacturing order => stop here
          }
        }
        // - operation
        if ((null == operation) && (null != operationSlot.Operation)) { // First identified operation
          // Can this operation be considered as valid ?
          if (operationSlot.DateTimeRange.Upper.HasValue
              && IsUnidentifiedOperationAfter (machine, operationSlot.DateTimeRange.Upper.Value)) {
            log.DebugFormat ("GetEffective: " +
                             "IsUnidentifiedOperationAfter after={0} returned true",
                             operationSlot.DateTimeRange.Upper.Value);
            break;
          }
          log.DebugFormat ("GetEffective: " +
                           "new operation detected {0}, reset the stored values",
                           ((IDataWithId<int>)operationSlot.Operation).Id);
          component = null;
          workOrder = null;
          line = null;
          effectiveSlots.Clear ();
          operation = operationSlot.Operation;
        }
        if ((null != operationSlot.Operation) && !object.Equals (operation, operationSlot.Operation)) {
          log.Debug ("GetEffective: " +
                     "another operation, stop here");
          break; // Another operation => stop here
        }
        // - Component
        if (null != operationSlot.Component) {
          if (null == component) {
            component = operationSlot.Component;
          }
          if (!object.Equals (component, operationSlot.Component)) {
            log.Debug ("GetEffective: " +
                       "another component, stop here");
            break; // Another component => stop here
          }
        }
        // - WorkOrder
        if (null != operationSlot.WorkOrder) {
          if (null == workOrder) {
            workOrder = operationSlot.WorkOrder;
          }
          if (!object.Equals (workOrder, operationSlot.WorkOrder)) {
            log.Debug ("GetEffective: " +
                       "another work order, stop here");
            break; // Another work order => stop here
          }
        }
        // - Line
        if (null != operationSlot.Line) {
          if (null == line) {
            line = operationSlot.Line;
          }
          if (!object.Equals (line, operationSlot.Line)) {
            log.Debug ("GetEffective: " +
                       "another line, stop here");
            break; // Another line => stop here
          }
        }
        log.DebugFormat ("GetEffective: " +
                         "consider the operation slot id={0}",
                         operationSlot.Id);
        effectiveSlots.Insert (0, operationSlot);
        if (onlyLast && (null != operation)) { // (null != manufacturing order) is already processed
          log.Debug ("GetEffective: " +
                     "only last, stop here");
          break;
        }
      } // Loop on operationSlots

      if (effectiveSlots.Any ()) {
        UtcDateTimeRange effectiveRange = new UtcDateTimeRange (effectiveSlots.First ().DateTimeRange.Lower,
                                                                effectiveSlots.Last ().DateTimeRange.Upper);
        log.DebugFormat ("GetEffective: " +
                         "virtual operation slot has range {0}",
                         effectiveRange);
        virtualOperationSlot = ModelDAOHelper.ModelFactory.CreateOperationSlot (machine,
                                                                                operation,
                                                                                component,
                                                                                workOrder,
                                                                                line,
                                                                                manufacturingOrder,
                                                                                day,
                                                                                shift,
                                                                                effectiveRange);
      }
      else {
        log.DebugFormat ("GetEffective: " +
                         "no effective slot => virtualOperationSlot is null");
        virtualOperationSlot = null;
      }

      return effectiveSlots;
    }

    bool IsUnidentifiedOperationAfter (IMachine machine, DateTime after)
    {
      // Note: this margin gives some flexibility because the machine may run a few seconds
      //       after the end of the last sequence if the sequence is not reset at the very end
      TimeSpan currentOperationSlotMargin = Lemoine.Info.ConfigSet
        .LoadAndGet<TimeSpan> (CURRENT_OPERATION_SLOT_MARGIN,
                               CURRENT_OPERATION_SLOT_DEFAULT);
      UtcDateTimeRange range = new UtcDateTimeRange (after.Add (currentOperationSlotMargin),
                                                     DateTime.UtcNow);
      if (range.IsEmpty ()) {
        log.DebugFormat ("IsUnidentifiedOperationAfter: " +
                         "return false because the range is empty");
        return false;
      }

      IMonitoredMachine monitoredMachine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
        .FindById (machine.Id);
      if (null == monitoredMachine) {
        log.DebugFormat ("IsUnidenfiedOperationAfter: " +
                         "return false because the machine is not monitored");
        return false;
      }

      bool existsNoSequence = ModelDAOHelper.DAOFactory.SequenceSlotDAO
        .ExistsNoSequenceBetween (monitoredMachine,
                                  range.Lower.Value,
                                  range.Upper.Value);
      return existsNoSequence;
    }

    /// <summary>
    /// Find the slot with the specified end date/time
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public IOperationSlot FindWithEnd (IMachine machine,
                                       DateTime end)
    {
      Debug.Assert (null != machine);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<OperationSlot> ()
        .Add (Restrictions.Eq ("Machine", machine))
        .Add (new FunctionExpression ("DateTimeRange", "upper", NHibernateUtil.DateTime, end))
        .UniqueResult<IOperationSlot> ();
    }

    /// <summary>
    /// Find all the slots that overlap the specified range
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="workOrder">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IList<IOperationSlot> FindOverlapsRangeWithWorkOrder (IMachine machine,
                                                                 IWorkOrder workOrder,
                                                                 UtcDateTimeRange range)
    {
      Debug.Assert (null != machine);
      Debug.Assert (null != workOrder);

      Debug.Assert (null != machine);

      if (range.IsEmpty ()) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("FindOverlapsRangeWithWorkOrder: " +
                           "empty range " +
                           "=> return an empty list. " +
                           "StackTrace: {0}",
                           System.Environment.StackTrace);
        }
        return new List<IOperationSlot> ();
      }

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<OperationSlot> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (Restrictions.Eq ("WorkOrder.Id", ((IDataWithId)workOrder).Id))
        .Add (OverlapRange (range))
        .AddOrder (Order.Asc ("DateTimeRange"))
        .List<IOperationSlot> ();
    }
  }
}
