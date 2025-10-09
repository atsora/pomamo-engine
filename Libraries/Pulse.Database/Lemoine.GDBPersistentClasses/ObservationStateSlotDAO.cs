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
using NHibernate;
using NHibernate.Criterion;
using Lemoine.Core.Log;
using System.Threading.Tasks;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IObservationStateSlotDAO">IObservationStateSlotDAO</see>
  /// </summary>
  public sealed class ObservationStateSlotDAO
    : GenericMachineRangeSlotDAO<ObservationStateSlot, IObservationStateSlot>
    , IObservationStateSlotDAO
  {
    readonly ILog log = LogManager.GetLogger (typeof (ObservationStateSlotDAO).FullName);
    
    /// <summary>
    /// Constructor
    /// </summary>
    internal ObservationStateSlotDAO ()
      : base (true, true, true, true, false)
    {
    }

    /// <summary>
    /// <see cref="IObservationStateSlotDAO"/>
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="day"></param>
    /// <param name="shift"></param>
    /// <returns></returns>
    public IList<IObservationStateSlot> FindWith (IMachine machine, DateTime day, IShift shift)
    {
      Debug.Assert (null != machine);
      Debug.Assert (day.Kind == DateTimeKind.Unspecified);
      Debug.Assert (null != shift);

      var dayRange = new DayRange (day, day, true, true);
      var result = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ObservationStateSlot> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (Restrictions.IsNotNull ("Shift"))
        .Add (Restrictions.Eq ("Shift.Id", shift.Id))
        .Add (new SimpleExpression ("DayRange", dayRange, "&&"))
        .AddOrder (Order.Asc ("DateTimeRange"))
        .List<IObservationStateSlot> ();
      return result;
    }

    /// <summary>
    /// <see cref="IObservationStateSlotDAO"/>
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="day"></param>
    /// <param name="shift"></param>
    /// <returns></returns>
    public async Task<IList<IObservationStateSlot>> FindWithAsync (IMachine machine, DateTime day, IShift shift)
    {
      Debug.Assert (null != machine);
      Debug.Assert (day.Kind == DateTimeKind.Unspecified);
      Debug.Assert (null != shift);

      var dayRange = new DayRange (day, day, true, true);
      var result = await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ObservationStateSlot> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (Restrictions.IsNotNull ("Shift"))
        .Add (Restrictions.Eq ("Shift.Id", shift.Id))
        .Add (new SimpleExpression ("DayRange", dayRange, "&&"))
        .AddOrder (Order.Asc ("DateTimeRange"))
        .ListAsync<IObservationStateSlot> ();
      return result;
    }

    /// <summary>
    /// Implements <see cref="Lemoine.ModelDAO.IObservationStateSlotDAO.GetListInRange" />
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public IList<IObservationStateSlot> GetListInRange(IMachine machine, DateTime begin, DateTime end)
    {
      return FindOverlapsRange (machine, new UtcDateTimeRange (begin, end));
    }
    
    /// <summary>
    /// Find all the machine observation state slots in a specified UTC date/time range
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    [Obsolete("Use FindOverlapsRange instead")]
    public IList<IObservationStateSlot> FindAllInUtcRange (IMachine machine,
                                                           UtcDateTimeRange range)
    {
      return FindOverlapsRange (machine, range);
    }

    /// <summary>
    /// Find all the machine observation state slots in a specified UTC date/time range
    /// with the specified properties.
    /// 
    /// If one property is null, it must be null in the database as well
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="machineStateTemplate"></param>
    /// <param name="machineObservationState"></param>
    /// <param name="shift"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public IList<IObservationStateSlot> FindAllInUtcRangeWith (IMachine machine,
                                                               UtcDateTimeRange range,
                                                               IMachineStateTemplate machineStateTemplate,
                                                               IMachineObservationState machineObservationState,
                                                               IShift shift,
                                                               IUser user)
    {
      ICriteria criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ObservationStateSlot> ()
        .Add (Restrictions.Eq ("Machine", machine))
        .Add (InUtcRange (range));
      if (null == machineStateTemplate) {
        criteria.Add (Restrictions.IsNull ("MachineStateTemplate"));
      }
      else {
        criteria.Add (Restrictions.Eq ("MachineStateTemplate", machineStateTemplate));
      }
      if (null == machineObservationState) {
        criteria.Add (Restrictions.IsNull ("MachineObservationState"));
      }
      else {
        criteria.Add (Restrictions.Eq ("MachineObservationState", machineObservationState));
      }
      if (null == shift) {
        criteria.Add (Restrictions.IsNull ("Shift"));
      }
      else {
        criteria.Add (Restrictions.Eq ("Shift", shift));
      }
      if (null == user) {
        criteria.Add (Restrictions.IsNull ("User"));
      }
      else {
        criteria.Add (Restrictions.Eq ("User", user));
      }
      return criteria.AddOrder (Order.Asc ("DateTimeRange"))
        .List<IObservationStateSlot> ();
    }

    /// <summary>
    /// Find all the machine observation state slots that match a machine state template
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="machineStateTemplate"></param>
    /// <returns></returns>
    public IList<IObservationStateSlot> FindMatchingMachineStateTemplate (IMachine machine,
                                                                          UtcDateTimeRange range,
                                                                          IMachineStateTemplate machineStateTemplate)
    {
      ICriteria criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ObservationStateSlot> ()
        .Add (Restrictions.Eq ("Machine", machine))
        .Add (InUtcRange (range));
      if (null == machineStateTemplate) {
        criteria.Add (Restrictions.IsNull ("MachineStateTemplate"));
      }
      else {
        criteria.Add (Restrictions.Eq ("MachineStateTemplate", machineStateTemplate));
      }
      return criteria.AddOrder (Order.Asc ("DateTimeRange"))
        .List<IObservationStateSlot> ();
    }
    
    /// <summary>
    /// Find all the machine observation state slots that match a machine state template association.
    /// It means:
    /// <item>same machine state template</item>
    /// <item>if the shift or user is set, it must match</item>
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="machineStateTemplate"></param>
    /// <param name="shift"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public IList<IObservationStateSlot> FindMatchingMachineStateTemplateAssociation (IMachine machine,
                                                                                     UtcDateTimeRange range,
                                                                                     IMachineStateTemplate machineStateTemplate,
                                                                                     IShift shift,
                                                                                     IUser user)
    {
      ICriteria criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ObservationStateSlot> ()
        .Add (Restrictions.Eq ("Machine", machine))
        .Add (InUtcRange (range));
      if (null == machineStateTemplate) {
        criteria.Add (Restrictions.IsNull ("MachineStateTemplate"));
      }
      else {
        criteria.Add (Restrictions.Eq ("MachineStateTemplate", machineStateTemplate));
      }
      if (null != shift) {
        criteria.Add (Restrictions.Eq ("Shift", shift));
      }
      if (null != user) {
        criteria.Add (Restrictions.Eq ("User", user));
      }
      return criteria.AddOrder (Order.Asc ("DateTimeRange"))
        .List<IObservationStateSlot> ();
    }

    /// <summary>
    /// Find all the machine observation state slots in a specified day range
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IList<IObservationStateSlot> FindAllInDayRange (IMachine machine,
                                                           DayRange range)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ObservationStateSlot> ()
        .Add (Restrictions.Eq ("Machine", machine))
        .Add (new SimpleExpression ("DayRange", range, "&&")) // Overlap
        .AddOrder (Order.Asc ("DateTimeRange"))
        .List<IObservationStateSlot> ();
    }

    /// <summary>
    /// Find all the observation state slots that matches a given user in a specified UTC date/time range
    /// </summary>
    /// <param name="user"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IList<IObservationStateSlot> FindByUserInRange (IUser user,
                                                           UtcDateTimeRange range)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ObservationStateSlot> ()
        .Add (Restrictions.Eq ("User", user))
        .Add (InUtcRange (range))
        .AddOrder (Order.Asc ("Machine"))
        .AddOrder (Order.Asc ("DateTimeRange"))
        .List<IObservationStateSlot> ();
    }
    
    /// <summary>
    /// Get the impacted observation state slots by a user site attendance change
    /// </summary>
    /// <param name="user"></param>
    /// <param name="newSiteAttendance"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IList<IObservationStateSlot> GetAttendanceChangeImpacts (IUser user, bool newSiteAttendance,
                                                                    UtcDateTimeRange range)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ObservationStateSlot> ()
        .CreateAlias ("MachineStateTemplate", "MachineStateTemplate")
        .Add (Restrictions.Eq ("User", user))
        .Add (Restrictions.IsNotNull ("MachineStateTemplate"))
        .Add (Restrictions.Eq ("MachineStateTemplate.OnSite", !newSiteAttendance))
        .Add (InUtcRange (range))
        .AddOrder (Order.Asc ("DateTimeRange"))
        .List<IObservationStateSlot> ();
    }
    
    /// <summary>
    /// Find all the observation state slots with no machine observation state
    /// that intersect the specified date/range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IList<IObservationStateSlot> FindWithNoMachineObservationState (IMachine machine,
                                                                           UtcDateTimeRange range)
    {
      Debug.Assert (null != machine);
      
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ObservationStateSlot> ()
        .Add (Restrictions.Eq ("Machine", machine))
        .Add (Restrictions.IsNull ("MachineObservationState"))
        .Add (InUtcRange (range))
        .AddOrder (Order.Asc ("DateTimeRange"))
        .List<IObservationStateSlot> ();
    }

    /// <summary>
    /// Find all the observation state slots with no machine observation state
    /// that intersect the specified date/range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <param name="n">maximum number of items to take</param>
    /// <returns></returns>
    public IEnumerable<IObservationStateSlot> FindWithNoMachineObservationStateInRange (IMachine machine,
      UtcDateTimeRange range, int n)
    {
      Debug.Assert (null != machine);

      var upperProjection = GetUpperProjection ();
      var criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ObservationStateSlot> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (Restrictions.IsNull ("MachineObservationState"));
      if (range.Lower.HasValue) {
        criteria = criteria
          .Add (Restrictions.Gt (upperProjection, range.Lower.Value));
      }
      IEnumerable<IObservationStateSlot> result = criteria
        .Add (OverlapRange (range))
        .AddOrder (Order.Asc (upperProjection))
        .SetMaxResults (n)
        .List<IObservationStateSlot> ();
      if (IsUpperInfPossible () && result.Count () < n) {
        IObservationStateSlot withUpperInf = NHibernateHelper.GetCurrentSession ()
          .CreateCriteria<ObservationStateSlot> ()
          .Add (Restrictions.Eq ("Machine.Id", machine.Id))
          .Add (Restrictions.IsNull ("MachineObservationState"))
          .Add (IsUpperInfinite ())
          .Add (OverlapRange (range))
          .UniqueResult<IObservationStateSlot> ();
        if (null != withUpperInf) {
          result = result
            .Concat (new List<IObservationStateSlot> { withUpperInf });
        }
      }
      return result;
    }

    /// <summary>
    /// Find all the observation state slots with no machine observation state
    /// that intersect the specified date/range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="utcBegin">in UTC</param>
    /// <param name="utcEnd">in UTC</param>
    /// <param name="n">maximum number of items to take</param>
    /// <returns></returns>
    public IEnumerable<IObservationStateSlot> FindWithNoMachineObservationStateInRange (IMachine machine,
                                                                                  DateTime utcBegin,
                                                                                  DateTime utcEnd,
                                                                                  int n)
    {
      Debug.Assert (null != machine);

      var range = new UtcDateTimeRange (utcBegin, utcEnd);
      return FindWithNoMachineObservationStateInRange (machine, range, n);
    }

    /// <summary>
    /// Find all the observation state slots with no machine observation state
    /// that begin before the specified date/time
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="until">in UTC</param>
    /// <param name="limit">maximum number of items to take</param>
    /// <returns></returns>
    public IEnumerable<IObservationStateSlot> FindWithNoMachineObservationState (IMachine machine,
                                                                           DateTime until,
                                                                           int limit)
    {
      Debug.Assert (null != machine);
      UtcDateTimeRange range = new UtcDateTimeRange (new LowerBound<DateTime>(null), until);
      return FindWithNoMachineObservationStateInRange (machine, range, limit);
    }
    
    /// <summary>
    /// Get last MachineObservationStateSlot for a given machine in a date range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public IObservationStateSlot GetLastIntersectWithRange(IMachine machine, DateTime begin, DateTime end)
    {
      Debug.Assert (null != machine);

      var range = new UtcDateTimeRange (begin, end);
      var result = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ObservationStateSlot> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (IsUpperInfinite ())
        .Add (OverlapRange (range))
        .UniqueResult<IObservationStateSlot> ();
      if (null != result) {
        return result;
      }
      else { // with upper_inf, not valid
        var lowerProjection = GetLowerProjection ();
        return NHibernateHelper.GetCurrentSession ()
          .CreateCriteria<ObservationStateSlot> ()
          .Add (Restrictions.Eq ("Machine", machine))
          .Add (OverlapRange (range))
          .Add (Restrictions.Lt (lowerProjection, end))
          .AddOrder (Order.Desc (lowerProjection)) // ok because never null
          .SetMaxResults (1)
          .UniqueResult<IObservationStateSlot> ();
      }
    }

    /// <summary>
    /// Get the production duration in a specific date/time range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public TimeSpan GetProductionDuration (IMachine machine, UtcDateTimeRange range)
    {
      Debug.Assert (null != machine);
      
      IList<IObservationStateSlot> observationStateSlots = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ObservationStateSlot> ()
        .Add (Restrictions.Eq ("Machine", machine))
        .Add (InUtcRange (range))
        .Add (Restrictions.IsNotNull ("Production"))
        .Add (Restrictions.Eq ("Production", true))
        .List<IObservationStateSlot> ();
      IEnumerable<UtcDateTimeRange> ranges = observationStateSlots
        .Select (slot => new UtcDateTimeRange (slot.DateTimeRange.Intersects (range)));
      
      // - If any of the slot has an undefined duration (because of +oo or -oo),
      //   return an exception
      var slotsWithNoDuration = ranges.Where (slot => !slot.Duration.HasValue);
      if (slotsWithNoDuration.Any ()) {
        log.ErrorFormat ("GetProductionDuration: " +
                         "at least one slot has no duration");
        throw new Exception ("Slot with no duration");
      }
      
      return ranges.Aggregate (TimeSpan.FromSeconds (0), (total, next) => total.Add (next.Duration.Value));
    }
    
    /// <summary>
    /// Find all the observation state slots (on different machines)
    /// at a specific date/time with the specified machine state template
    /// 
    /// They are order by ascending begin date/time
    /// </summary>
    /// <param name="machineStateTemplate"></param>
    /// <param name="at"></param>
    /// <returns></returns>
    public IList<IObservationStateSlot> FindAt (IMachineStateTemplate machineStateTemplate, DateTime at)
    {
      Debug.Assert (DateTimeKind.Utc == at.Kind);
      
      ICriteria criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ObservationStateSlot> ();
      if (null == machineStateTemplate) {
        criteria.Add (Restrictions.IsNull ("MachineStateTemplate"));
      }
      else {
        criteria.Add (Restrictions.Eq ("MachineStateTemplate", machineStateTemplate));
      }
      // Note: new SimpleExpression ("DateTimeRange", dateTime, "@>") does not work because it compares object of different types
      // The returned error is: Type mismatch in NHibernate.Criterion.SimpleExpression: DateTimeRange expected type Lemoine.Model.UtcDateTimeRange, actual type System.DateTime
      criteria.Add (new SimpleTypedExpression ("DateTimeRange", new Lemoine.NHibernateTypes.UTCDateTimeFullType (), at, "@>"));
      criteria.AddOrder (Order.Asc ("DateTimeRange"));
      return criteria.List<IObservationStateSlot> ();
    }
    
    /// <summary>
    /// Find the first observation state slot with a different machine state template that is strictly on the left of the specified range
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="machineStateTemplate"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    internal IObservationStateSlot FindFirstStrictlyLeftDifferentMachineStateTemplate (IMachine machine,
                                                                                       IMachineStateTemplate machineStateTemplate,
                                                                                       UtcDateTimeRange range)
    {
      if (range.IsEmpty ()) {
        log.ErrorFormat ("FindFirstStrictlyLeftDifferentMachineStateTemplate: empty range");
        return null;
      }

      if (!range.Lower.HasValue) {
        log.ErrorFormat ("FindFirstStrictlyLeftDifferentMachineStateTemplate: no lower value, there is no possible slot to return");
        return null;
      }

      var upperProjection = GetUpperProjection ();
      ICriteria criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ObservationStateSlot> ()
        .Add (Restrictions.Eq ("Machine", machine))
        .Add (new SimpleExpression ("DateTimeRange", range, "<<")) // Strictly left
        .Add (Restrictions.Le (upperProjection, range.Lower.Value));
      if (null == machineStateTemplate) {
        criteria.Add (Restrictions.IsNotNull ("MachineStateTemplate"));
      }
      else { // null != machineStateTemplate
        criteria.Add (Restrictions.Disjunction ()
                      .Add (Restrictions.IsNull ("MachineStateTemplate"))
                      .Add (Restrictions.Not (Restrictions.Eq ("MachineStateTemplate.Id", machineStateTemplate.Id))));
      }
      return criteria
        .AddOrder (Order.Desc (upperProjection))
        .SetMaxResults (1)
        .UniqueResult<IObservationStateSlot> ();
    }

    /// <summary>
    /// Find the first observation state slot with a different machine state template that is strictly on the right of the specified range
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="machineStateTemplate"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    internal IObservationStateSlot FindFirstStrictlyRightDifferentMachineStateTemplate (IMachine machine,
                                                                                        IMachineStateTemplate machineStateTemplate,
                                                                                        UtcDateTimeRange range)
    {
      if (range.IsEmpty ()) {
        log.ErrorFormat ("FindFirstStrictlyRightDifferentMachineStateTemplate: empty range");
        return null;
      }

      if (!range.Upper.HasValue) {
        log.ErrorFormat ("FindFirstStrictlyRightDifferentMachineStateTemplate: no upper value, there is no possible slot to return");
        return null;
      }

      var lowerProjection = GetLowerProjection ();
      ICriteria criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ObservationStateSlot> ()
        .Add (Restrictions.Eq ("Machine", machine))
      // Note: new SimpleExpression ("DateTimeRange", dateTime, ">>") does not work because it compares object of different types
      // The returned error is: Type mismatch in NHibernate.Criterion.SimpleExpression: DateTimeRange expected type Lemoine.Model.UtcDateTimeRange, actual type System.DateTime
        .Add (new SimpleExpression ("DateTimeRange", range, ">>")) // Strictly right
        .Add (Restrictions.Ge (lowerProjection, range.Upper.Value));
      if (null == machineStateTemplate) {
        criteria.Add (Restrictions.IsNotNull ("MachineStateTemplate"));
      }
      else { // null != machineStateTemplate
        criteria.Add (Restrictions.Disjunction ()
                      .Add (Restrictions.IsNull ("MachineStateTemplate"))
                      .Add (Restrictions.Not (Restrictions.Eq ("MachineStateTemplate.Id", machineStateTemplate.Id))));
      }
      return criteria
        .AddOrder (Order.Asc (lowerProjection))
        .SetMaxResults (1)
        .UniqueResult<IObservationStateSlot> ();
    }
    
    /// <summary>
    /// Find all the observation state slots that match a machine state template in a specified range
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="machineStateTemplate"></param>
    /// <returns></returns>
    public IEnumerable<IObservationStateSlot> FindOverlapsRangeMatchingMachineStateTemplate (IMachine machine,
                                                                                             UtcDateTimeRange range,
                                                                                             IMachineStateTemplate machineStateTemplate)
    {
      Debug.Assert (null != machine);
      
      NHibernate.ICriteria criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ObservationStateSlot> ()
        .Add (Restrictions.Eq ("Machine", machine))
        .Add (OverlapRange (range));
      if (null == machineStateTemplate) {
        criteria.Add (Restrictions.IsNull ("MachineStateTemplate"));
      }
      else {
        criteria.Add (Restrictions.Eq ("MachineStateTemplate.Id", machineStateTemplate.Id));
      }
      return criteria.AddOrder (Order.Asc ("DateTimeRange"))
        .List<IObservationStateSlot> ();
    }

    /// <summary>
    /// Find the slot with the specified end date/time
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public IObservationStateSlot FindWithEnd (IMachine machine,
                                              DateTime end)
    {
      Debug.Assert (null != machine);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ObservationStateSlot> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        // Note: .Add (new FunctionExpression ("DateTimeRange", "upper_inf") is not required here
        // since end is specified
        .Add (new FunctionExpression ("DateTimeRange", "upper", NHibernateUtil.DateTime, end))
        .UniqueResult<IObservationStateSlot> ();
    }

    /// <summary>
    /// Find the slot with the specified end date/time
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<IObservationStateSlot> FindWithEndAsync (IMachine machine,
                                              DateTime end)
    {
      Debug.Assert (null != machine);

      return await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ObservationStateSlot> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        // Note: .Add (new FunctionExpression ("DateTimeRange", "upper_inf") is not required here
        // since end is specified
        .Add (new FunctionExpression ("DateTimeRange", "upper", NHibernateUtil.DateTime, end))
        .UniqueResultAsync<IObservationStateSlot> ();
    }
  }
}
