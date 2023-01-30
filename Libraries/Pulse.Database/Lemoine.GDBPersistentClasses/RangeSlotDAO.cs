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
using System.Threading.Tasks;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Common DAO methods for all the Slots
  /// </summary>
  public class RangeSlotDAO<TSlot, I>
    : SlotDAO<TSlot, I>, Lemoine.Threading.IChecked
    where TSlot : RangeSlot, I, Lemoine.Threading.IChecked
    where I : ISlot
  {
    readonly bool m_lowerInfPossible = false;
    readonly bool m_upperInfPossible = false;
    readonly bool m_lowerIncPossible = false;
    readonly bool m_upperIncPossible = false;

    ILog log = LogManager.GetLogger (typeof (RangeSlotDAO<TSlot, I>).FullName);

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dayColumn"></param>
    /// <param name="lowerInfPossible"></param>
    /// <param name="upperInfPossible"></param>
    /// <param name="lowerIncPossible"></param>
    /// <param name="upperIncPossible"></param>
    protected RangeSlotDAO (bool dayColumn, bool lowerInfPossible, bool upperInfPossible, bool lowerIncPossible, bool upperIncPossible)
      : base (dayColumn)
    {
      m_lowerInfPossible = lowerInfPossible;
      m_upperInfPossible = upperInfPossible;
      m_lowerIncPossible = lowerIncPossible;
      m_upperIncPossible = upperIncPossible;
    }
    #endregion // Constructors

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected virtual bool IsLowerInfPossible ()
    {
      return m_lowerInfPossible;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected virtual bool IsUpperInfPossible ()
    {
      return m_upperInfPossible;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected virtual bool IsLowerIncPossible ()
    {
      return m_lowerIncPossible;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected virtual bool IsUpperIncPossible ()
    {
      return m_upperIncPossible;
    }

    /// <summary>
    /// Find all the slots order by the range
    /// </summary>
    /// <returns></returns>
    public override IList<I> FindAll ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<TSlot> ()
        // Note: Gist indexes do not support sorting, so the upper index is used instead
        .AddOrder (Order.Asc (GetUpperProjection ())) // NULLS LAST is the default
        .List<I> ();
    }

    /// <summary>
    /// Find all the slots order by the range
    /// </summary>
    /// <returns></returns>
    public override async Task<IList<I>> FindAllAsync ()
    {
      return await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<TSlot> ()
        // Note: Gist indexes do not support sorting, so the upper index is used instead
        .AddOrder (Order.Asc (GetUpperProjection ())) // NULLS LAST is the default
        .ListAsync<I> ();
    }

    /// <summary>
    /// Find all the slots that overlap the specified range
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public virtual IList<I> FindOverlapsRange (UtcDateTimeRange range)
    {
      // Note: several tests were done and using the upper index is not more effective
      //       unless limit is used
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<TSlot> ()
        .Add (OverlapRange (range))
        .AddOrder (Order.Asc ("DateTimeRange"))
        .List<I> ();
    }

    /// <summary>
    /// Find all the slots that overlap the specified range asynchronously
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public virtual async System.Threading.Tasks.Task<IList<I>> FindOverlapsRangeAsync (UtcDateTimeRange range)
    {
      // Note: several tests were done and using the upper index is not more effective
      //       unless limit is used
      return await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<TSlot> ()
        .Add (OverlapRange (range))
        .AddOrder (Order.Asc ("DateTimeRange"))
        .ListAsync<I> ();
    }

    /// <summary>
    /// Find the unique slot at the specified UTC date/time
    /// 
    /// Be careful ! This can be used only on global slots
    /// </summary>
    /// <param name="at">in UTC</param>
    /// <returns></returns>
    public virtual I FindAt (Bound<DateTime> at)
    {
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
          .CreateCriteria<TSlot> ()
          // Note: new SimpleExpression ("DateTimeRange", dateTime, "@>") does not work because it compares object of different types
          // The returned error is: Type mismatch in NHibernate.Criterion.SimpleExpression: DateTimeRange expected type Lemoine.Model.UtcDateTimeRange, actual type System.DateTime
          .Add (new SimpleTypedExpression ("DateTimeRange", new Lemoine.NHibernateTypes.UTCDateTimeFullType (), utc, "@>"))
          .UniqueResult<I> ();
      }
      else { // !at.HasValue
        switch (at.BoundType) {
          case BoundType.Lower:
            return NHibernateHelper.GetCurrentSession ()
              .CreateCriteria<TSlot> ()
              .Add (IsLowerInfinite ())
              .UniqueResult<I> ();
          case BoundType.Upper:
            return NHibernateHelper.GetCurrentSession ()
              .CreateCriteria<TSlot> ()
              .Add (IsUpperInfinite ())
              .UniqueResult<I> ();
          default:
            log.FatalFormat ("Unexpected BoundType");
            throw new InvalidOperationException ("Invalid BoundType");
        }
      }
    }

    /// <summary>
    /// Find the unique slot at the specified UTC date/time
    /// 
    /// Be careful ! This can be used only on global slots
    /// </summary>
    /// <param name="at">in UTC</param>
    /// <returns></returns>
    public virtual async System.Threading.Tasks.Task<I> FindAtAsync (Bound<DateTime> at)
    {
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
        return await NHibernateHelper.GetCurrentSession ()
          .CreateCriteria<TSlot> ()
          // Note: new SimpleExpression ("DateTimeRange", dateTime, "@>") does not work because it compares object of different types
          // The returned error is: Type mismatch in NHibernate.Criterion.SimpleExpression: DateTimeRange expected type Lemoine.Model.UtcDateTimeRange, actual type System.DateTime
          .Add (new SimpleTypedExpression ("DateTimeRange", new Lemoine.NHibernateTypes.UTCDateTimeFullType (), utc, "@>"))
          .UniqueResultAsync<I> ();
      }
      else { // !at.HasValue
        switch (at.BoundType) {
        case BoundType.Lower:
          return await NHibernateHelper.GetCurrentSession ()
            .CreateCriteria<TSlot> ()
            .Add (IsLowerInfinite ())
            .UniqueResultAsync<I> ();
        case BoundType.Upper:
          return await NHibernateHelper.GetCurrentSession ()
            .CreateCriteria<TSlot> ()
            .Add (IsUpperInfinite ())
            .UniqueResultAsync<I> ();
        default:
          log.FatalFormat ("Unexpected BoundType");
          throw new InvalidOperationException ("Invalid BoundType");
        }
      }
    }

    /// <summary>
    /// Range criterion with UTC date/times
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    protected virtual AbstractCriterion InUtcRange (UtcDateTimeRange range)
    {
      return OverlapRange (range);
    }

    /// <summary>
    /// Range criterion with UTC date/times using directly the range
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    protected virtual AbstractCriterion OverlapRange (UtcDateTimeRange range)
    {
      return new SimpleExpression ("DateTimeRange", range, "&&");
    }

    /// <summary>
    /// Range criterion with UTC date/times using the lower/upper bounds
    /// to be able to use some other indexes
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    [Obsolete("This is pretty inefficient", true)]
    protected virtual AbstractCriterion OverlapRangeUsingBounds (UtcDateTimeRange range)
    {
      if ((null == range) || (range.IsEmpty ())) {
        return Expression.Sql ("FALSE");
      }

      var lowerProjection = GetLowerProjection ();
      var upperProjection = GetUpperProjection ();

      // From constraint
      Junction result = Restrictions.Conjunction ();
      if (range.Lower.HasValue) {
        if (range.LowerInclusive) {
          result = result
            .Add (Restrictions.Disjunction ()
                  .Add (IsUpperInfinite ())
                  .Add (Restrictions.Gt (upperProjection, range.Lower.Value))
                  .Add (Restrictions.Conjunction ()
                        .Add (IsUpperInclusive ())
                        .Add (Restrictions.Ge (upperProjection, range.Lower.Value)))
                 );
        }
        else { // Not inclusive
          result = result
            .Add (Restrictions.Disjunction ()
                  .Add (IsUpperInfinite ())
                  .Add (Restrictions.Gt (upperProjection,
                                         range.Lower.Value)));
        }
      }

      // To constraint
      if (range.Upper.HasValue) {
        if (range.UpperInclusive) {
          result = result
            .Add (Restrictions.Disjunction ()
                  .Add (IsLowerInfinite ())
                  .Add (Restrictions.Lt (lowerProjection, range.Upper.Value))
                  .Add (Restrictions.Conjunction ()
                        .Add (IsLowerInclusive ())
                        .Add (Restrictions.Le (lowerProjection, range.Upper.Value)))
                 );
        }
        else {
          result = result
            .Add (Restrictions.Disjunction ()
                  .Add (IsLowerInfinite ())
                  .Add (Restrictions.Lt (lowerProjection, range.Upper.Value)));
        }
      }

      return result;
    }

    /// <summary>
    /// Range criterion with UTC date/times
    /// 
    /// The criterion includes the periods that have one common date/time,
    /// even though it does not intersect the given period
    /// </summary>
    /// <param name="range">UTC range</param>
    /// <returns></returns>
    protected virtual AbstractCriterion InUtcRangeWithBounds (UtcDateTimeRange range)
    {
      return new Disjunction ()
        .Add (new SimpleExpression ("DateTimeRange", range, "&&")) // Overlap
        .Add (new SimpleExpression ("DateTimeRange", range, "-|-")); // Adjacent to
    }

    /// <summary>
    /// Range criterion with UTC date/times
    /// 
    /// The criterion includes the periods that have one common date/time,
    /// even though it does not intersect the given period
    /// 
    /// This method is using the lower and upper bounds
    /// </summary>
    /// <param name="range">UTC range</param>
    /// <returns></returns>
    [Obsolete ("This is pretty inefficient", true)]
    protected virtual AbstractCriterion InUtcRangeWithBoundsUsingBounds (UtcDateTimeRange range)
    {
      if ((null == range) || (range.IsEmpty ())) {
        return Expression.Sql ("FALSE");
      }

      var lowerProjection = GetLowerProjection ();
      var upperProjection = GetUpperProjection ();

      // From constraint
      Junction result = Restrictions.Conjunction ();
      if (range.Lower.HasValue) {
        result = result
          .Add (Restrictions.Disjunction ()
                .Add (IsUpperInfinite ())
                .Add (Restrictions.Ge (upperProjection, range.Lower.Value))
               );
      }

      // To constraint
      if (range.Upper.HasValue) {
        result = result
          .Add (Restrictions.Disjunction ()
                .Add (IsLowerInfinite ())
                .Add (Restrictions.Le (lowerProjection, range.Upper.Value))
               );
      }

      return result;
    }

    /// <summary>
    /// Range criterion with UTC date/times
    /// 
    /// The criterion includes the periods that have one common date/time,
    /// even though it does not intersect the given period
    /// </summary>
    /// <param name="range">UTC range</param>
    /// <returns></returns>
    protected virtual AbstractCriterion InUtcRangeWithLeftBound (UtcDateTimeRange range)
    {
      return new Disjunction ()
        .Add (new SimpleExpression ("DateTimeRange", range, "&&")) // Overlap
        .Add (new Conjunction ()
              .Add (new SimpleExpression ("DateTimeRange", range, "-|-"))
              .Add (new SimpleExpression ("DateTimeRange", range, "<<"))); // Adjacent to on the left
    }

    /// <summary>
    /// Range criterion with UTC date/times
    /// 
    /// The criterion includes the periods that have one common date/time,
    /// even though it does not intersect the given period
    /// </summary>
    /// <param name="range">UTC range</param>
    /// <returns></returns>
    protected virtual AbstractCriterion InUtcRangeWithRightBound (UtcDateTimeRange range)
    {
      return new Disjunction ()
        .Add (new SimpleExpression ("DateTimeRange", range, "&&")) // Overlap
        .Add (new Conjunction ()
              .Add (new SimpleExpression ("DateTimeRange", range, "-|-"))
              .Add (new SimpleExpression ("DateTimeRange", range, ">>"))); // Adjacent to on the right
    }

    /// <summary>
    /// Range criterion with days
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    protected virtual AbstractCriterion InDayRange (DayRange range)
    {
      return new SimpleExpression ("DayRange", range, "&&");
    }

    protected virtual AbstractCriterion AtDay (DateTime day)
    {
      return new SimpleTypedExpression ("DayRange", new Lemoine.NHibernateTypes.DayType (), day, "@>");
    }

    /// <summary>
    /// Criterion to get the machine slots that begin (not strictly) before
    /// the given UTC date/time
    /// </summary>
    /// <param name="utcDateTime">UTC date/time</param>
    /// <returns></returns>
    protected virtual AbstractCriterion BeginBeforeUtcDateTime (DateTime utcDateTime)
    {
      // Not strictly on the right of utcDateTime
      return new NotExpression (new SimpleExpression ("DateTimeRange",
                                                      new UtcDateTimeRange (utcDateTime, utcDateTime, "[]"),
                                                      ">>"));
    }

    /// <summary>
    /// Criterion to get the machine slots that begin (not strictly) before
    /// the given UTC date/time using the lower bound
    /// </summary>
    /// <param name="utcDateTime">UTC date/time</param>
    /// <returns></returns>
    [Obsolete ("Risk to create inefficient requests")]
    protected virtual AbstractCriterion BeginBeforeUtcDateTimeUsingLower (DateTime utcDateTime)
    {
      // Lower <= utcDateTime
      return Restrictions.Disjunction ()
        .Add (IsLowerInfinite ())
        .Add (Restrictions.Le (GetLowerProjection (), utcDateTime));
    }

    /// <summary>
    /// Criterion to get the machine slots that end after (not strictly)
    /// the given UTC date/time
    /// </summary>
    /// <param name="utcDateTime">UTC date/time</param>
    /// <returns></returns>
    protected virtual AbstractCriterion EndAfterUtcDateTime (DateTime utcDateTime)
    {
      return InUtcRangeWithLeftBound (new UtcDateTimeRange (utcDateTime));
    }

    /// <summary>
    /// Criterion to get the machine slots that end after (not strictly)
    /// the given UTC date/time
    /// 
    /// This method is using directly the range
    /// </summary>
    /// <param name="utcDateTime">UTC date/time</param>
    /// <returns></returns>
    protected virtual AbstractCriterion EndAfterUtcDateTimeUsingRange (DateTime utcDateTime)
    {
      return InUtcRangeWithLeftBound (new UtcDateTimeRange (utcDateTime));
    }

    /// <summary>
    /// Criterion to get the machine slots that end after (not strictly)
    /// the given UTC date/time using the upper bound
    /// </summary>
    /// <param name="utcDateTime">UTC date/time</param>
    /// <returns></returns>
    [Obsolete("Risk to create inefficient requests")]
    protected virtual AbstractCriterion EndAfterUtcDateTimeUsingUpper (DateTime utcDateTime)
    {
      return Restrictions.Disjunction ()
        .Add (IsUpperInfinite ())
        .Add (Restrictions.Ge (GetUpperProjection (), utcDateTime));
    }

    /// <summary>
    /// Get a projection of the lower bound of the date/time range
    /// </summary>
    /// <returns></returns>
    protected virtual IProjection GetLowerProjection ()
    {
      return Projections
        .SqlFunction ("lower",
          new NHibernateTypes.UTCDateTimeFullType (),
          Projections.Property ("DateTimeRange"));
    }

    /// <summary>
    /// Get a projection of the upper bound of the date/time range
    /// </summary>
    /// <returns></returns>
    protected virtual IProjection GetUpperProjection ()
    {
      return Projections
        .SqlFunction ("upper",
          new NHibernateTypes.UTCDateTimeFullType (),
          Projections.Property ("DateTimeRange"));
    }

    /// <summary>
    /// Is the lower bound infinite ?
    /// </summary>
    /// <returns></returns>
    protected virtual AbstractCriterion IsLowerInfinite ()
    {
      return new Conjunction ()
        .Add (new FunctionExpression ("DateTimeRange", "lower_inf"))
        // Or: return Restrictions.Eq (GetLowerInfProjection (), true);
        .Add (OverlapRange (new UtcDateTimeRange (new LowerBound<DateTime> (null), new DateTime (1980, 01, 01))));
    }

    /// <summary>
    /// Projection to get the lower_inf property of the date/time range
    /// </summary>
    /// <returns></returns>
    protected virtual IProjection GetLowerInfProjection ()
    {
      return Projections
        .SqlFunction ("lower_inf",
        NHibernateUtil.Boolean,
        Projections.Property ("DateTimeRange"));
    }

    /// <summary>
    /// Is the upper bound infinite ?
    /// </summary>
    /// <returns></returns>
    protected virtual AbstractCriterion IsUpperInfinite ()
    {
      return new Conjunction ()
        .Add (new FunctionExpression ("DateTimeRange", "upper_inf"))
        // Or: return Restrictions.Eq (GetUpperInfProjection (), true);
        .Add (OverlapRange (new UtcDateTimeRange (new DateTime (2200, 01, 01))));
    }

    /// <summary>
    /// Projection to get the upper_inf property of the date/time range
    /// </summary>
    /// <returns></returns>
    protected virtual IProjection GetUpperInfProjection ()
    {
      return Projections
        .SqlFunction ("upper_inf",
        NHibernateUtil.Boolean,
        Projections.Property ("DateTimeRange"));
    }

    /// <summary>
    /// Is the lower bound inclusive ?
    /// </summary>
    /// <returns></returns>
    protected virtual AbstractCriterion IsLowerInclusive ()
    {
      return new FunctionExpression ("DateTimeRange", "lower_inc");
      // Or: return Restrictions.Eq (GetLowerIncProjection (), true);
    }

    /// <summary>
    /// Projection to get the lower_inc property of the date/time range
    /// </summary>
    /// <returns></returns>
    protected virtual IProjection GetLowerIncProjection ()
    {
      return Projections
        .SqlFunction ("lower_inc",
        NHibernateUtil.Boolean,
        Projections.Property ("DateTimeRange"));
    }

    /// <summary>
    /// Is the upper bound inclusive ?
    /// </summary>
    /// <returns></returns>
    protected virtual AbstractCriterion IsUpperInclusive ()
    {
      return new FunctionExpression ("DateTimeRange", "upper_inc");
      // Or: return Restrictions.Eq (GetUpperIncProjection (), true);
    }

    /// <summary>
    /// Projection to get the upper_inc property of the date/time range
    /// </summary>
    /// <returns></returns>
    protected virtual IProjection GetUpperIncProjection ()
    {
      return Projections
        .SqlFunction ("upper_inc",
        NHibernateUtil.Boolean,
        Projections.Property ("DateTimeRange"));
    }

    /// <summary>
    /// Check if the range is empty
    /// </summary>
    /// <returns></returns>
    protected virtual AbstractCriterion IsEmptyRange ()
    {
      return new FunctionExpression ("DateTimeRange", "isempty");
      // Or: return Restrictions.Eq (GetIsEmptyProjection (), true);
    }

    /// <summary>
    /// Projection to get the isempty property of the date/time range
    /// </summary>
    /// <returns></returns>
    protected virtual IProjection GetIsEmptyProjection ()
    {
      return Projections
        .SqlFunction ("isempty",
        NHibernateUtil.Boolean,
        Projections.Property ("DateTimeRange"));
    }

    /// <summary>
    /// Get the list of impacted slots for the analysis
    /// </summary>
    /// <param name="range"></param>
    /// <param name="dateTime"></param>
    /// <param name="pastOnly"></param>
    /// <param name="leftMerge">Get the data to try to merge on the left too</param>
    /// <param name="rightMerge">Get the data to try to merge on the right too</param>
    /// <returns></returns>
    public override IList<I> GetImpactedSlotsForAnalysis (UtcDateTimeRange range,
                                                          DateTime dateTime,
                                                          bool pastOnly,
                                                          bool leftMerge,
                                                          bool rightMerge)
    {
      ICriteria criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<TSlot> ();
      if (leftMerge && rightMerge) {
        criteria.Add (InUtcRangeWithBounds (range));
      }
      else if (leftMerge) {
        criteria.Add (InUtcRangeWithLeftBound (range));
      }
      else if (rightMerge) {
        criteria.Add (InUtcRangeWithRightBound (range));
      }
      else {
        criteria.Add (InUtcRange (range));
      }
      if (pastOnly) {
        // In some cases the slot begin must be before modificationdatetime
        // to avoid processing slots that are in the future
        criteria.Add (BeginBeforeUtcDateTime (dateTime));
      }
      return criteria
        .AddOrder (Order.Asc (GetUpperProjection ()))
        .List<I> ();
    }

    /// <summary>
    /// Get the list of impacted slots for the analysis
    /// </summary>
    /// <param name="additionalCriterion"></param>
    /// <param name="range"></param>
    /// <param name="dateTime"></param>
    /// <param name="pastOnly"></param>
    /// <param name="leftMerge">Get the data to try to merge on the left too</param>
    /// <param name="rightMerge">Get the data to try to merge on the right too</param>
    /// <returns></returns>
    internal IList<I> GetImpactedSlotsForAnalysis (ICriterion additionalCriterion,
      UtcDateTimeRange range,
      DateTime dateTime,
      bool pastOnly,
      bool leftMerge,
      bool rightMerge)
    {
      ICriteria criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<TSlot> ()
        .Add (additionalCriterion);
      if (leftMerge && rightMerge) {
        criteria.Add (InUtcRangeWithBounds (range));
      }
      else if (leftMerge) {
        criteria.Add (InUtcRangeWithLeftBound (range));
      }
      else if (rightMerge) {
        criteria.Add (InUtcRangeWithRightBound (range));
      }
      else {
        criteria.Add (InUtcRange (range));
      }
      if (pastOnly) {
        // In some cases the slot begin must be before modificationdatetime
        // to avoid processing slots that are in the future
        criteria.Add (BeginBeforeUtcDateTime (dateTime));
      }
      return criteria
        .AddOrder (Order.Asc (GetUpperProjection ()))
        .List<I> ();
    }

    /// <summary>
    /// Get the list of impacted machine slots for the analysis
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="dateTime"></param>
    /// <param name="pastOnly"></param>
    /// <param name="leftMerge">Get the data to try to merge on the left too</param>
    /// <param name="rightMerge">Get the data to try to merge on the right too</param>
    /// <returns></returns>
    public override IList<I> GetImpactedMachineSlotsForAnalysis (IMachine machine,
                                                                   UtcDateTimeRange range,
                                                                   DateTime dateTime,
                                                                   bool pastOnly,
                                                                   bool leftMerge,
                                                                   bool rightMerge)
    {
      return GetImpactedSlotsForAnalysis (Restrictions.Eq ("Machine.Id", machine.Id), range, dateTime,pastOnly, leftMerge, rightMerge);
    }

    /// <summary>
    /// Get the list of impacted machine module slots for the analysis
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="range"></param>
    /// <param name="dateTime"></param>
    /// <param name="pastOnly"></param>
    /// <param name="leftMerge">Get the data to try to merge on the left too</param>
    /// <param name="rightMerge">Get the data to try to merge on the right too</param>
    /// <returns></returns>
    public override IList<I> GetImpactedMachineModuleSlotsForAnalysis (IMachineModule machineModule,
                                                                         UtcDateTimeRange range,
                                                                         DateTime dateTime,
                                                                         bool pastOnly,
                                                                         bool leftMerge,
                                                                         bool rightMerge)
    {
      return GetImpactedSlotsForAnalysis (Restrictions.Eq ("MachineModule.Id", machineModule.Id), range, dateTime, pastOnly, leftMerge, rightMerge);
    }

    /// <summary>
    /// Get the list of impacted line slots for the analysis
    /// </summary>
    /// <param name="line"></param>
    /// <param name="range"></param>
    /// <param name="dateTime"></param>
    /// <param name="pastOnly"></param>
    /// <param name="leftMerge">Get the data to try to merge on the left too</param>
    /// <param name="rightMerge">Get the data to try to merge on the right too</param>
    /// <returns></returns>
    public override IList<I> GetImpactedLineSlotsForAnalysis (ILine line,
                                                                UtcDateTimeRange range,
                                                                DateTime dateTime,
                                                                bool pastOnly,
                                                                bool leftMerge,
                                                                bool rightMerge)
    {
      return GetImpactedSlotsForAnalysis (Restrictions.Eq ("Line.Id", line.Id), range, dateTime, pastOnly, leftMerge, rightMerge);
    }

    /// <summary>
    /// Get the list of impacted user slots for the analysis
    /// </summary>
    /// <param name="user"></param>
    /// <param name="range"></param>
    /// <param name="dateTime"></param>
    /// <param name="pastOnly"></param>
    /// <param name="leftMerge">Get the data to try to merge on the left too</param>
    /// <param name="rightMerge">Get the data to try to merge on the right too</param>
    /// <returns></returns>
    public override IList<I> GetImpactedUserSlotsForAnalysis (IUser user,
                                                       UtcDateTimeRange range,
                                                       DateTime dateTime,
                                                       bool pastOnly,
                                                       bool leftMerge,
                                                       bool rightMerge)
    {
      return GetImpactedSlotsForAnalysis (Restrictions.Eq ("User.Id", user.Id), range, dateTime, pastOnly, leftMerge, rightMerge);
    }

  }
}
