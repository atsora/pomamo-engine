// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;
using System.Linq;
using Lemoine.Extensions.Database;
using System.Threading.Tasks;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IReasonSlotDAO">IReasonSlotDAO</see>
  /// </summary>
  public sealed class ReasonSlotDAO
    : GenericMachineRangeSlotDAO<ReasonSlot, IReasonSlot>
    , IReasonSlotDAO
  {
    readonly ILog log = LogManager.GetLogger (typeof (ReasonSlotDAO).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    internal ReasonSlotDAO ()
      : base (true, false, false, true, false)
    {
    }

    /// <summary>
    /// Override MakePersistent not to store reason slots with an empty range
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override IReasonSlot MakePersistent (IReasonSlot entity)
    {
      Debug.Assert (!entity.DateTimeRange.IsEmpty ());
      if (entity.DateTimeRange.IsEmpty ()) {
        log.FatalFormat ("MakePersistent: " +
                         "reason slot with an empty date/time range => make it transient. " +
                         "StackTrace: {0}",
                         System.Environment.StackTrace);
        MakeTransient (entity);
        return entity;
      }

      ((ReasonSlot)entity).FlagRemoved = false;
      return base.MakePersistent (entity);
    }

    /// <summary>
    /// Override MakeTransient to update the ReasonSlotChange flag
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override void MakeTransient (IReasonSlot entity)
    {
      ((ReasonSlot)entity).FlagRemoved = true;
      base.MakeTransient (entity);
    }

    /// <summary>
    /// Override MakePersistent not to store reason slots with an empty range
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override async Task<IReasonSlot> MakePersistentAsync (IReasonSlot entity)
    {
      Debug.Assert (!entity.DateTimeRange.IsEmpty ());
      if (entity.DateTimeRange.IsEmpty ()) {
        log.FatalFormat ("MakePersistent: " +
                         "reason slot with an empty date/time range => make it transient. " +
                         "StackTrace: {0}",
                         System.Environment.StackTrace);
        await MakeTransientAsync (entity);
        return entity;
      }

      ((ReasonSlot)entity).FlagRemoved = false;
      return await base.MakePersistentAsync (entity);
    }

    /// <summary>
    /// Override MakeTransientAsync to update the ReasonSlotChange flag
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override async System.Threading.Tasks.Task MakeTransientAsync (IReasonSlot entity)
    {
      ((ReasonSlot)entity).FlagRemoved = true;
      await base.MakeTransientAsync (entity);
    }

    /// <summary>
    /// Find the unique slot at the specified UTC date/time
    /// with an early fetch of the reason
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="at">in UTC</param>
    /// <returns></returns>
    public IReasonSlot FindAtWithReason (IMachine machine, Bound<DateTime> at)
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
          log.Error ($"FindAtWithReason: date/time {at} is of kind Unspecified");
          Debug.Assert (DateTimeKind.Unspecified != at.Value.Kind);
          utc = at.Value;
          break;
        }
        return NHibernateHelper.GetCurrentSession ()
          .CreateCriteria<ReasonSlot> ()
          .Add (Restrictions.Eq ("Machine.Id", machine.Id))
          // Note: new SimpleExpression ("DateTimeRange", dateTime, "@>") does not work because it compares object of different types
          // The returned error is: Type mismatch in NHibernate.Criterion.SimpleExpression: DateTimeRange expected type Lemoine.Model.UtcDateTimeRange, actual type System.DateTime
          .Add (new SimpleTypedExpression ("DateTimeRange", new Lemoine.NHibernateTypes.UTCDateTimeFullType (), utc, "@>"))
          .Fetch (SelectMode.Fetch, "Reason")
          .UniqueResult<IReasonSlot> ();
      }
      else { // !at.HasValue
        switch (at.BoundType) {
        case BoundType.Lower:
          return NHibernateHelper.GetCurrentSession ()
            .CreateCriteria<ReasonSlot> ()
            .Add (Restrictions.Eq ("Machine.Id", machine.Id))
            .Add (new FunctionExpression ("DateTimeRange", "lower_inf"))
            .Fetch (SelectMode.Fetch, "Reason")
            .UniqueResult<IReasonSlot> ();
        case BoundType.Upper:
          return NHibernateHelper.GetCurrentSession ()
            .CreateCriteria<ReasonSlot> ()
            .Add (Restrictions.Eq ("Machine.Id", machine.Id))
            .Add (new FunctionExpression ("DateTimeRange", "upper_inf"))
            .Fetch (SelectMode.Fetch, "Reason")
            .UniqueResult<IReasonSlot> ();
        default:
          log.FatalFormat ("Unexpected BoundType");
          throw new InvalidOperationException ("Invalid BoundType");
        }
      }
    }

    /// <summary>
    /// Find all the reason slots in a specified UTC date/time range.
    /// Include the reason slots that are in the limit
    /// (they must be excluded programmatically then)
    /// 
    /// Order them by date/time range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range">not empty</param>
    /// <returns></returns>
    public IList<IReasonSlot> FindAllInUtcRangeWithLimit (IMonitoredMachine machine,
                                                          UtcDateTimeRange range)
    {
      return FindOverlapsRangeWithLimit (machine, range);
    }

    /// <summary>
    /// Get the last reason slot for the machine
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    public IReasonSlot GetLast (IMachine machine)
    {
      Debug.Assert (null != machine);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ReasonSlot> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .AddOrder (Order.Desc (GetUpperProjection ()))
        .SetMaxResults (1)
        .UniqueResult<IReasonSlot> ();
    }

    /// <summary>
    /// Find the reason slot with the specified end date/time
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public IReasonSlot FindWithEnd (IMachine machine, DateTime end)
    {
      Debug.Assert (null != machine);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ReasonSlot> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        // Note: .Add (new FunctionExpression ("DateTimeRange", "upper_inf") is not required
        //       because ReasonSlotEndDateTime is never null
        .Add (new FunctionExpression ("DateTimeRange", "upper", NHibernateUtil.DateTime, end))
        .UniqueResult<IReasonSlot> ();
    }

    /// <summary>
    /// Find the reason slot with the specified begin date/time
    /// 
    /// Note: FindAt could have beed used instead although that the performance of this method is may be a little bit higher
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="begin"></param>
    /// <returns></returns>
    public IReasonSlot FindWithBegin (IMachine machine, DateTime begin)
    {
      Debug.Assert (null != machine);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<IReasonSlot> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (new FunctionExpression ("DateTimeRange", "lower", NHibernateUtil.DateTime, begin))
        .UniqueResult<IReasonSlot> ();
    }

    /// <summary>
    /// Find the reason slot with the specified end date/time
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="machineMode">not null</param>
    /// <param name="machineObservationState">not null</param>
    /// <param name="end"></param>
    /// <returns></returns>
    public IReasonSlot FindWithEnd (IMachine machine, IMachineMode machineMode,
                                      IMachineObservationState machineObservationState,
                                      DateTime end)
    {
      Debug.Assert (null != machine);
      Debug.Assert (null != machineMode);
      Debug.Assert (null != machineObservationState);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<IReasonSlot> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (Restrictions.Eq ("MachineMode.Id", machineMode.Id))
        .Add (Restrictions.Eq ("MachineObservationState.Id", machineObservationState.Id))
        // Note: .Add (new FunctionExpression ("DateTimeRange", "upper_inf") is not required
        //       because ReasonSlotEndDateTime is never null
        .Add (new FunctionExpression ("DateTimeRange", "upper", NHibernateUtil.DateTime, end))
        .UniqueResult<IReasonSlot> ();
    }

    /// <summary>
    /// Find all the reason slots in a specified UTC date/time range
    /// with an early fetch of the machine mode
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IList<IReasonSlot> FindAllInUtcRangeWithMachineMode (IMachine machine,
                                                                UtcDateTimeRange range)
    {
      Debug.Assert (null != machine);

      Debug.Assert (!range.IsEmpty ());
      if (range.IsEmpty ()) {
        log.ErrorFormat ("FindAllInUtcRangeWithMachineMode: " +
                         "empty range " +
                         "=> return an empty list. " +
                         "StackTrace: {0}",
                         System.Environment.StackTrace);
        return new List<IReasonSlot> ();
      }

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ReasonSlot> ()
        .Fetch (SelectMode.Fetch, "MachineMode")
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (OverlapRange (range))
        .AddOrder (Order.Asc ("DateTimeRange"))
        .List<IReasonSlot> ();
    }

    /// <summary>
    /// Find all the reason slots in a specified UTC date/time range
    /// with an early fetch of the machine mode
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IList<IReasonSlot> FindAllInUtcRangeWithMachineModeReason (IMachine machine,
                                                                      UtcDateTimeRange range)
    {
      Debug.Assert (null != machine);

      Debug.Assert (!range.IsEmpty ());
      if (range.IsEmpty ()) {
        log.ErrorFormat ("FindAllInUtcRangeWithMachineModeReason: " +
                         "empty range " +
                         "=> return an empty list. " +
                         "StackTrace: {0}",
                         System.Environment.StackTrace);
        return new List<IReasonSlot> ();
      }

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ReasonSlot> ()
        .Fetch (SelectMode.Fetch, "MachineMode")
        .Fetch (SelectMode.Fetch, "Reason")
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (OverlapRange (range))
        .AddOrder (Order.Asc ("DateTimeRange"))
        .List<IReasonSlot> ();
    }

    /// <summary>
    /// Find all the reason slots in a specified UTC date/time range
    /// with an early fetch of the reason, the machine observation state and the machine mode
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IList<IReasonSlot> FindAllInUtcRangeWithReasonMachineObservationStateMachineMode (IMonitoredMachine machine,
                                                                                             UtcDateTimeRange range)
    {
      Debug.Assert (null != machine);

      Debug.Assert (!range.IsEmpty ());
      if (range.IsEmpty ()) {
        log.ErrorFormat ("FindAllInUtcRangeWithReasonMachineObservationStateMachineMode: " +
                         "empty range " +
                         "=> return an empty list. " +
                         "StackTrace: {0}",
                         System.Environment.StackTrace);
        return new List<IReasonSlot> ();
      }

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ReasonSlot> ()
        .Fetch (SelectMode.Fetch, "Reason")
        .Fetch (SelectMode.Fetch, "MachineMode")
        .Fetch (SelectMode.Fetch, "MachineObservationState")
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (OverlapRange (range))
        .AddOrder (Order.Asc ("DateTimeRange"))
        .List<IReasonSlot> ();
    }

    /// <summary>
    /// Find the first reasonslot with a reason Processing
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="n"></param>
    /// <returns></returns>
    public IEnumerable<IReasonSlot> FindFirstProcessingInRangeDescending (IMonitoredMachine machine, UtcDateTimeRange range, int n)
    {
      Debug.Assert (null != machine);

      if (range.IsEmpty ()) {
        return new List<IReasonSlot> ();
      }

      var lowerProjection = GetLowerProjection ();
      var criteria = NHibernateHelper.GetCurrentSession ()
      .CreateCriteria<ReasonSlot> ()
      .Add (Restrictions.Eq ("Machine.Id", machine.Id))
      .Add (Restrictions.Eq ("Reason.Id", (int)ReasonId.Processing));
      if (range.Upper.HasValue) {
        if (range.UpperInclusive && IsLowerIncPossible ()) {
          criteria = criteria
            .Add (Restrictions.Le (lowerProjection, range.Upper.Value));
        }
        else { // !range.LowerInclusive || !IsUpperIncPossible
          criteria = criteria
            .Add (Restrictions.Lt (lowerProjection, range.Upper.Value));
        }
      }
      return criteria
        .Add (OverlapRange (range))
        .AddOrder (Order.Desc (lowerProjection))
        .SetMaxResults (n)
        .List<IReasonSlot> ();
    }

    class FindProcessingDescendingStrategy
      : IFindOverlapsRangeByStepStrategy<IReasonSlot, IMonitoredMachine>
    {
      static readonly string LOWER_LIMIT_AGE_KEY = "ReasonSlotDAO.FindProcessing.LowerLimit";
      static readonly TimeSpan LOWER_LIMIT_AGE_DEFAULT = TimeSpan.FromDays (365);

      static readonly string STEP_KEY = "ReasonSlotDAO.FindProcessing.Step";
      static readonly TimeSpan STEP_DEFAULT = TimeSpan.FromDays (1);

      readonly int m_stepNumber;
      readonly Action<UtcDateTimeRange> m_preLoad;

      public FindProcessingDescendingStrategy (int stepNumber, Action<UtcDateTimeRange> preLoad)
      {
        m_stepNumber = stepNumber;
        m_preLoad = preLoad;
      }

      public bool Descending
      {
        get { return true; }
      }

      public UtcDateTimeRange LimitRange
      {
        get
        {
          var age = Lemoine.Info.ConfigSet
            .LoadAndGet<TimeSpan> (LOWER_LIMIT_AGE_KEY, LOWER_LIMIT_AGE_DEFAULT);
          var lower = DateTime.UtcNow.Subtract (age);
          return new UtcDateTimeRange (lower);
        }
      }

      public TimeSpan? MaxGap
      {
        get { return null; }
      }

      // Note: normally not used
      public TimeSpan Step
      {
        get { return Lemoine.Info.ConfigSet.LoadAndGet (STEP_KEY, STEP_DEFAULT); }
      }

      IEnumerable<IReasonSlot> Request (IMonitoredMachine machine, UtcDateTimeRange range)
      {
        var reasonSlots = new ReasonSlotDAO ()
          .FindFirstProcessingInRangeDescending (machine, range, m_stepNumber);
        if (reasonSlots.Any ()) {
          var last = reasonSlots.Last ();
          var first = reasonSlots.First ();
          var resultRange = new UtcDateTimeRange (last.DateTimeRange.Lower,
            first.DateTimeRange.Upper, last.DateTimeRange.LowerInclusive, first.DateTimeRange.UpperInclusive);
          m_preLoad (resultRange);
        }
        return reasonSlots;
      }

      public IEnumerable<IReasonSlot> InitialRequest (IMonitoredMachine machine, UtcDateTimeRange range)
      {
        return Request (machine, range);
      }

      public bool MaxResultsInitial { get { return true; } }

      public bool LimitRangeInitial { get { return false; } }

      public IEnumerable<IReasonSlot> NextRequest (IMonitoredMachine machine, UtcDateTimeRange range)
      {
        return Request (machine, range);
      }

      public bool MaxResultsNext { get { return true; } }

      public bool LimitRangeNext { get { return false; } }

      public bool ReadWrite { get { return false; } }
    }

    /// <summary>
    /// Find the processing reason slots in the specified range (by step number)
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <param name="stepNumber">number of reason slots to retrieve in the same time</param>
    /// <param name="preLoad">Pre-load method to call</param>
    /// <returns></returns>
    public IEnumerable<IReasonSlot> FindProcessingDescending (IMonitoredMachine machine,
      UtcDateTimeRange range, int stepNumber, Action<UtcDateTimeRange> preLoad)
    {
      var logger = LogManager.GetLogger (typeof (ReasonSlotDAO).FullName + "." + machine.Id);
      var strategy = new FindProcessingDescendingStrategy (stepNumber, preLoad);
      return new FindOverlapsRangeEnumerable<IReasonSlot, IMonitoredMachine> (strategy, machine, range, logger);
    }

    /// <summary>
    /// Find all the reason slots (on different machines)
    /// at a specific date/time with the specified production state
    /// 
    /// They are order by ascending begin date/time
    /// </summary>
    /// <param name="productionState"></param>
    /// <param name="at"></param>
    /// <returns></returns>
    public IEnumerable<IReasonSlot> FindAt (IProductionState productionState, DateTime at)
    {
      Debug.Assert (DateTimeKind.Utc == at.Kind);

      ICriteria criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ReasonSlot> ();
      if (null == productionState) {
        criteria.Add (Restrictions.IsNull ("ProductionState"));
      }
      else {
        criteria.Add (Restrictions.Eq ("ProductionState.Id", productionState.Id));
      }
      // Note: new SimpleExpression ("DateTimeRange", dateTime, "@>") does not work because it compares object of different types
      // The returned error is: Type mismatch in NHibernate.Criterion.SimpleExpression: DateTimeRange expected type Lemoine.Model.UtcDateTimeRange, actual type System.DateTime
      criteria.Add (new SimpleTypedExpression ("DateTimeRange", new Lemoine.NHibernateTypes.UTCDateTimeFullType (), at, "@>"));
      criteria.AddOrder (Order.Asc ("DateTimeRange"));
      return criteria.List<IReasonSlot> ();
    }

    /// <summary>
    /// Find all the reason slots that match a production state in a specified range
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="productionState"></param>
    /// <returns></returns>
    public IEnumerable<IReasonSlot> FindOverlapsRangeMatchingProductionState (IMachine machine,
                                                                              UtcDateTimeRange range,
                                                                              IProductionState productionState)
    {
      Debug.Assert (null != machine);

      NHibernate.ICriteria criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ReasonSlot> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (OverlapRange (range));
      if (null == productionState) {
        criteria.Add (Restrictions.IsNull ("ProductionState"));
      }
      else {
        criteria.Add (Restrictions.Eq ("ProductionState.Id", productionState.Id));
      }
      return criteria.AddOrder (Order.Asc ("DateTimeRange"))
        .List<IReasonSlot> ();
    }

    /// <summary>
    /// Find the first reason slot with a different production state that is strictly on the left of the specified range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="productionState"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IReasonSlot FindFirstStrictlyLeftDifferentProductionState (IMachine machine,
                                                                      IProductionState productionState,
                                                                      UtcDateTimeRange range)
    {
      Debug.Assert (null != machine);

      if (range.IsEmpty ()) {
        log.Error ("FindFirstStrictlyLeftDifferentProductionState: empty range");
        return null;
      }

      if (!range.Lower.HasValue) {
        log.Error ("FindFirstStrictlyLeftDifferentProductionState: no lower value, there is no possible slot to return");
        return null;
      }

      var upperProjection = GetUpperProjection ();
      ICriteria criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ReasonSlot> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (new SimpleExpression ("DateTimeRange", range, "<<")) // Strictly left
        .Add (Restrictions.Le (upperProjection, range.Lower.Value));
      if (null == productionState) {
        criteria.Add (Restrictions.IsNotNull ("ProductionState"));
      }
      else { // null != machineStateTemplate
        criteria.Add (Restrictions.Disjunction ()
                      .Add (Restrictions.IsNull ("ProductionState"))
                      .Add (Restrictions.Not (Restrictions.Eq ("ProductionState.Id", productionState.Id))));
      }
      return criteria
        .AddOrder (Order.Desc (upperProjection))
        .SetMaxResults (1)
        .UniqueResult<IReasonSlot> ();
    }

    /// <summary>
    /// Find the first reason slot with a different production state that is strictly on the right of the specified range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="productionState"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IReasonSlot FindFirstStrictlyRightDifferentProductionState (IMachine machine,
                                                                       IProductionState productionState,
                                                                       UtcDateTimeRange range)
    {
      Debug.Assert (null != machine);

      if (range.IsEmpty ()) {
        log.ErrorFormat ("FindFirstStrictlyRightDifferentProductionState: empty range");
        return null;
      }

      if (!range.Upper.HasValue) {
        log.ErrorFormat ("FindFirstStrictlyRightDifferentProductionState: no upper value, there is no possible slot to return");
        return null;
      }

      var lowerProjection = GetLowerProjection ();
      ICriteria criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ReasonSlot> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
      // Note: new SimpleExpression ("DateTimeRange", dateTime, ">>") does not work because it compares object of different types
      // The returned error is: Type mismatch in NHibernate.Criterion.SimpleExpression: DateTimeRange expected type Lemoine.Model.UtcDateTimeRange, actual type System.DateTime
        .Add (new SimpleExpression ("DateTimeRange", range, ">>")) // Strictly right
        .Add (Restrictions.Ge (lowerProjection, range.Upper.Value));
      if (null == productionState) {
        criteria.Add (Restrictions.IsNotNull ("ProductionState"));
      }
      else { // null != machineStateTemplate
        criteria.Add (Restrictions.Disjunction ()
                      .Add (Restrictions.IsNull ("ProductionState"))
                      .Add (Restrictions.Not (Restrictions.Eq ("ProductionState.Id", productionState.Id))));
      }
      return criteria
        .AddOrder (Order.Asc (lowerProjection))
        .SetMaxResults (1)
        .UniqueResult<IReasonSlot> ();
    }

    /// <summary>
    /// Find all the reason slots that match a production state
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <param name="productionState"></param>
    /// <returns></returns>
    public IList<IReasonSlot> FindMatchingProductionState (IMachine machine,
                                                           UtcDateTimeRange range,
                                                           IProductionState productionState)
    {
      Debug.Assert (null != machine);

      ICriteria criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ObservationStateSlot> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (InUtcRange (range));
      if (null == productionState) {
        criteria.Add (Restrictions.IsNull ("ProductionState"));
      }
      else {
        criteria.Add (Restrictions.Eq ("ProductionState", productionState));
      }
      return criteria.AddOrder (Order.Asc ("DateTimeRange"))
        .List<IReasonSlot> ();
    }
  }
}
