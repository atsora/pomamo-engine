// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

using System.Diagnostics;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;
using Lemoine.Business.Config;
using System.Threading.Tasks;
using Lemoine.Database.Persistent;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Common DAO methods for all the Slots
  /// </summary>
  public class BeginEndSlotDAO<TSlot, I>
    : SlotDAO<TSlot, I>, Lemoine.Threading.IChecked
    where TSlot: BeginEndSlot, I, Lemoine.Threading.IChecked
    where I: ISlot
  {
    readonly ILog log = LogManager.GetLogger(typeof (BeginEndSlotDAO<TSlot, I>).FullName);

    static readonly string MIN_TEMPLATE_PROCESS_DATE_TIME_KEY = "Analysis.MinTemplateProcessDateTime";
    static readonly DateTime MIN_TEMPLATE_PROCESS_DATE_TIME_DEFAULT = new DateTime (2019, 01, 01, 00, 00, 00, DateTimeKind.Utc);

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dayColumn"></param>
    protected BeginEndSlotDAO (bool dayColumn)
      : base (dayColumn)
    {
    }
    #endregion // Constructors

    /// <summary>
    /// Find all the slots order by begin date/time
    /// </summary>
    /// <returns></returns>
    public override IList<I> FindAll ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<TSlot> ()
        .AddOrder (Order.Asc ("BeginDateTime"))
        .List<I> ();
    }

    /// <summary>
    /// Find all the slots order by begin date/time
    /// </summary>
    /// <returns></returns>
    public override async Task<IList<I>> FindAllAsync ()
    {
      return await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<TSlot> ()
        .AddOrder (Order.Asc ("BeginDateTime"))
        .ListAsync<I> ();
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
        .AddOrder (Order.Asc ("BeginDateTime"))
        .List<I> ();
    }

    /// <summary>
    /// Get the list of impacted slots for the analysis
    /// </summary>
    /// <param name="additionalCriterion">additional criterion</param>
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
        .AddOrder (Order.Asc ("BeginDateTime"))
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
      return GetImpactedSlotsForAnalysis (Restrictions.Eq ("Machine.Id", machine.Id), range, dateTime, pastOnly, leftMerge, rightMerge);
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
    
    #region Range criterion methods
    /// <summary>
    /// Criterion to find the time slot at a given UTC date/time
    /// </summary>
    /// <param name="utcDateTime">UTC date/time</param>
    /// <returns></returns>
    protected virtual AbstractCriterion AtUtcDateTime (DateTime utcDateTime)
    {
      if (this.DayColumn) {
        IDaySlot daySlot = ModelDAOHelper.DAOFactory.DaySlotDAO
          .FindAt (utcDateTime);
        if ( (null != daySlot) && daySlot.Day.HasValue) {
          // From constraint
          Junction result = Restrictions.Conjunction ();
          result = result
            .Add (Restrictions.Disjunction ()
                  .Add (Restrictions.IsNull ("EndDay"))
                  .Add (Restrictions.Conjunction ()
                        .Add (Restrictions.Ge ("EndDay",
                                               (UpperBound<DateTime>)daySlot.Day.Value))
                        .Add (Restrictions.Gt ("EndDateTime",
                                               (UpperBound<DateTime>)utcDateTime))));
          
          // To constraint
          result = result
            .Add (Restrictions.Conjunction ()
                  .Add (Restrictions.Le ("BeginDay",
                                         (LowerBound<DateTime>)daySlot.Day.Value))
                  .Add (Restrictions.Le ("BeginDateTime",
                                         (LowerBound<DateTime>)utcDateTime)));
          
          return result;
        }
        else { // Fallback: the day slot is not processed yet, use the date/time
          // From constraint
          Junction result = Restrictions.Conjunction ();
          result = result
            .Add (Restrictions.Disjunction ()
                  .Add (Restrictions.IsNull ("EndDay"))
                  .Add (Restrictions.Gt ("EndDateTime",
                                         (UpperBound<DateTime>)utcDateTime)));
          
          // To constraint
          result = result
            .Add (Restrictions.Le ("BeginDateTime",
                                   (LowerBound<DateTime>)utcDateTime));
          
          return result;
        }
      }
      else { // !m_dayColumn
        return Restrictions.Conjunction ()
          .Add (Restrictions.Disjunction ()
                .Add (Restrictions.IsNull ("EndDateTime"))
                .Add (Restrictions.Gt ("EndDateTime",
                                       (UpperBound<DateTime>)utcDateTime)))
          .Add (Restrictions.Le ("BeginDateTime",
                                 (LowerBound<DateTime>)utcDateTime));
      }
    }
    
    /// <summary>
    /// Range criterion with UTC date/times
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    protected virtual AbstractCriterion InUtcRange (UtcDateTimeRange range)
    {
      if ((null == range) || (range.IsEmpty ())) {
        return Expression.Sql ("FALSE");
      }
      
      if (!range.Lower.HasValue && !range.Upper.HasValue) {
        return Restrictions.IsNotNull ("BeginDateTime");
      }
      
      // From constraint
      Junction result = Restrictions.Conjunction ();
      if (range.Lower.HasValue) {
        if (this.DayColumn) {
          var minTemplateProcessDateTime = Lemoine.Info.ConfigSet
            .LoadAndGet (MIN_TEMPLATE_PROCESS_DATE_TIME_KEY, MIN_TEMPLATE_PROCESS_DATE_TIME_DEFAULT);
          if (Bound.Compare<DateTime> (range.Lower.Value, minTemplateProcessDateTime) <= 0) {
            // Very old day, not processed, use only enddatetime
            result = result
              .Add (Restrictions.Disjunction ()
                    .Add (Restrictions.IsNull ("EndDateTime"))
                    .Add (Restrictions.Gt ("EndDateTime",
                                           (UpperBound<DateTime>)range.Lower.Value)));
          }
          else {
            IDaySlot fromDaySlot = ModelDAOHelper.DAOFactory.DaySlotDAO
              .FindAt (range.Lower.Value, true);
            if ( (null != fromDaySlot) && fromDaySlot.Day.HasValue) {
              result = result
                .Add (Restrictions.Disjunction ()
                      .Add (Restrictions.IsNull ("EndDay"))
                      .Add (Restrictions.Conjunction ()
                            .Add (Restrictions.Ge ("EndDay",
                                                   (UpperBound<DateTime>)fromDaySlot.Day.Value))
                            .Add (Restrictions.Gt ("EndDateTime",
                                                   (UpperBound<DateTime>)range.Lower.Value))));
            }
            else { // Fallback: the day is not processed yet, use the date/time only instead
              result = result
                .Add (Restrictions.Disjunction ()
                      .Add (Restrictions.IsNull ("EndDateTime"))
                      .Add (Restrictions.Gt ("EndDateTime",
                                             (UpperBound<DateTime>)range.Lower.Value)));
            }
          }
        }
        else { // !m_dayColumn
          result = result
            .Add (Restrictions.Disjunction ()
                  .Add (Restrictions.IsNull ("EndDateTime"))
                  .Add (Restrictions.Gt ("EndDateTime",
                                         (UpperBound<DateTime>)range.Lower.Value)));
        }
      }
      
      // To constraint
      if (range.Upper.HasValue) {
        if (this.DayColumn) {
          IDaySlot toDaySlot = ModelDAOHelper.DAOFactory.DaySlotDAO
            .FindAt (range.Upper.Value, true);
          if ( (null != toDaySlot) && toDaySlot.Day.HasValue) {
            result = result
              .Add (Restrictions.And (Restrictions.Le ("BeginDay",
                                                       (LowerBound<DateTime>)toDaySlot.Day.Value),
                                      Restrictions.Lt ("BeginDateTime",
                                                       (LowerBound<DateTime>)range.Upper.Value)));
          }
          else { // Fallback: the slot is not processed yet: use the date/time only instead
            result = result
              .Add (Restrictions.Lt ("BeginDateTime",
                                     (LowerBound<DateTime>)range.Upper.Value));
          }
        }
        else {
          result = result
            .Add (Restrictions.Lt ("BeginDateTime",
                                   (LowerBound<DateTime>)range.Upper.Value));
        }
      }
      
      return result;
    }
    
    /// <summary>
    /// Range criterion with UTC date/times
    /// 
    /// At least utcFrom or utcTo must be not null
    /// </summary>
    /// <param name="utcFrom">UTC from nullable date/time</param>
    /// <param name="utcTo">UTC to nullable date/time</param>
    /// <returns></returns>
    protected virtual AbstractCriterion InUtcRange (LowerBound<DateTime> utcFrom, UpperBound<DateTime> utcTo)
    {
      if (!utcFrom.HasValue && !utcTo.HasValue) {
        return Expression.Sql ("FALSE");
      }
      
      if (Bound.Compare<DateTime> (utcTo, utcFrom) < 0) {
        return Expression.Sql ("FALSE");
      }
      
      // From constraint
      Junction result = Restrictions.Conjunction ();
      if (utcFrom.HasValue) {
        if (this.DayColumn) {
          IDaySlot fromDaySlot = ModelDAOHelper.DAOFactory.DaySlotDAO
            .FindAt (utcFrom.Value, true);
          if ( (null != fromDaySlot) && fromDaySlot.Day.HasValue) {
            result = result
              .Add (Restrictions.Disjunction ()
                    .Add (Restrictions.IsNull ("EndDay"))
                    .Add (Restrictions.Conjunction ()
                          .Add (Restrictions.Ge ("EndDay",
                                                 (UpperBound<DateTime>)fromDaySlot.Day.Value))
                          .Add (Restrictions.Gt ("EndDateTime",
                                                 (UpperBound<DateTime>)utcFrom.Value))));
          }
          else { // Fallback: the day is not processed yet, use the date/time only instead
            result = result
              .Add (Restrictions.Disjunction ()
                    .Add (Restrictions.IsNull ("EndDay"))
                    .Add (Restrictions.Gt ("EndDateTime",
                                           (UpperBound<DateTime>)utcFrom.Value)));
          }
        }
        else { // !m_dayColumn
          result = result
            .Add (Restrictions.Disjunction ()
                  .Add (Restrictions.IsNull ("EndDateTime"))
                  .Add (Restrictions.Gt ("EndDateTime",
                                         (UpperBound<DateTime>)utcFrom.Value)));
        }
      }
      
      // To constraint
      if (utcTo.HasValue) {
        if (this.DayColumn) {
          IDaySlot toDaySlot = ModelDAOHelper.DAOFactory.DaySlotDAO
            .FindAt (utcTo.Value, true);
          if ( (null != toDaySlot) && toDaySlot.Day.HasValue) {
            result = result
              .Add (Restrictions.And (Restrictions.Le ("BeginDay",
                                                       (LowerBound<DateTime>)toDaySlot.Day.Value),
                                      Restrictions.Lt ("BeginDateTime",
                                                       (LowerBound<DateTime>)utcTo.Value)));
          }
          else { // Fallback: the day slot is not processed yet, use the date/time only instead
            result = result
              .Add (Restrictions.Lt ("BeginDateTime",
                                     (LowerBound<DateTime>)utcTo.Value));
          }
        }
        else {
          result = result
            .Add (Restrictions.Lt ("BeginDateTime",
                                   (LowerBound<DateTime>)utcTo.Value));
        }
      }
      
      return result;
    }
    
    /// <summary>
    /// Range criterion with days
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    protected virtual AbstractCriterion InDayRange (DayRange range)
    {
      if ((null == range) || (range.IsEmpty ())) {
        return Expression.Sql ("FALSE");
      }
      
      if (!range.Lower.HasValue && !range.Upper.HasValue) {
        return Expression.Sql ("TRUE");
      }
      
      // From constraint
      Junction result = Restrictions.Conjunction ();
      if (range.Lower.HasValue) {
        if (this.DayColumn) {
          result = result
            .Add (Restrictions.Or (Restrictions.IsNull ("EndDay"),
                                   Restrictions.Ge ("EndDay",
                                                    (UpperBound<DateTime>)range.Lower.Value)));
        }
        else { // !m_dayColumn
          DateTime utcFrom = ModelDAOHelper.DAOFactory.TimeConfigDAO.GetDayBeginUtcDateTime (range.Lower.Value);
          result = result
            .Add (Restrictions.Or (Restrictions.IsNull ("EndDateTime"),
                                   Restrictions.Gt ("EndDateTime",
                                                    (UpperBound<DateTime>)utcFrom)));
        }
      }
      
      // To constraint
      if (range.Upper.HasValue) {
        if (this.DayColumn) {
          result = result
            .Add (Restrictions.Le ("BeginDay",
                                   (LowerBound<DateTime>)range.Upper.Value));
        }
        else {
          DateTime utcTo = ModelDAOHelper.DAOFactory.TimeConfigDAO.GetDayEndUtcDateTime (range.Upper.Value);
          result = result
            .Add (Restrictions.Lt ("BeginDateTime",
                                   (LowerBound<DateTime>)utcTo));
        }
      }
      
      return result;
    }
    
    /// <summary>
    /// Range criterion with days
    /// 
    /// At least dayFrom or dayTo must be not null
    /// </summary>
    /// <param name="dayFrom">nullable day</param>
    /// <param name="dayTo">nullable day</param>
    /// <returns></returns>
    protected virtual AbstractCriterion InDayRange (LowerBound<DateTime> dayFrom, UpperBound<DateTime> dayTo)
    {
      if (!dayFrom.HasValue && !dayTo.HasValue) {
        return Expression.Sql ("FALSE");
      }
      
      if (dayFrom.HasValue
          && NullableDateTime.Compare (dayTo, dayFrom) < 0) {
        return Expression.Sql ("FALSE");
      }
      
      // From constraint
      Junction result = Restrictions.Conjunction ();
      if (dayFrom.HasValue) {
        if (this.DayColumn) {
          result = result
            .Add (Restrictions.Or (Restrictions.IsNull ("EndDay"),
                                   Restrictions.Ge ("EndDay",
                                                    (UpperBound<DateTime>)dayFrom.Value)));
        }
        else { // !m_dayColumn
          DateTime utcFrom = ModelDAOHelper.DAOFactory.TimeConfigDAO.GetDayBeginUtcDateTime (dayFrom.Value);
          result = result
            .Add (Restrictions.Or (Restrictions.IsNull ("EndDateTime"),
                                   Restrictions.Gt ("EndDateTime",
                                                    (UpperBound<DateTime>)utcFrom)));
        }
      }
      
      // To constraint
      if (dayTo.HasValue) {
        if (this.DayColumn) {
          result = result
            .Add (Restrictions.Le ("BeginDay",
                                   (LowerBound<DateTime>)dayTo.Value));
        }
        else {
          DateTime utcTo = ModelDAOHelper.DAOFactory.TimeConfigDAO.GetDayEndUtcDateTime (dayTo.Value);
          result = result
            .Add (Restrictions.Lt ("BeginDateTime",
                                   (LowerBound<DateTime>)utcTo));
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
    /// <param name="range">UTC from nullable date/time</param>
    /// <returns></returns>
    protected virtual AbstractCriterion InUtcRangeWithBounds (UtcDateTimeRange range)
    {
      if ((null == range) || range.IsEmpty ()) {
        return Expression.Sql ("FALSE");
      }
      
      if (!range.Lower.HasValue && !range.Upper.HasValue) {
        return Restrictions.IsNotNull ("BeginDateTime");
      }
      
      LowerBound<DateTime> utcFrom = range.Lower;
      UpperBound<DateTime> utcTo = range.Upper;
      
      // From constraint
      Junction result = Restrictions.Conjunction ();
      if (utcFrom.HasValue) {
        if (this.DayColumn) {
          IDaySlot fromDaySlot = ModelDAOHelper.DAOFactory.DaySlotDAO
            .FindAt (utcFrom.Value, true);
          if ((null != fromDaySlot) && fromDaySlot.Day.HasValue) {
            result = result
              .Add (Restrictions.Disjunction ()
                    .Add (Restrictions.IsNull ("EndDay"))
                    .Add (Restrictions.Conjunction ()
                          .Add (Restrictions.Ge ("EndDay",
                                                 (UpperBound<DateTime>)fromDaySlot.Day.Value))
                          .Add (Restrictions.Ge ("EndDateTime",
                                                 (UpperBound<DateTime>)utcFrom))));
          }
          else { // Fallback: the day slot is not processed yet, use the date/time only instead
            result = result
              .Add (Restrictions.Disjunction ()
                    .Add (Restrictions.IsNull ("EndDay"))
                    .Add (Restrictions.Ge ("EndDateTime",
                                           (UpperBound<DateTime>)utcFrom)));
          }
        }
        else { // !m_dayColumn
          result = result
            .Add (Restrictions.Disjunction ()
                  .Add (Restrictions.IsNull ("EndDateTime"))
                  .Add (Restrictions.Ge ("EndDateTime",
                                         (UpperBound<DateTime>)utcFrom)));
        }
      }
      
      // To constraint
      if (utcTo.HasValue) {
        if (this.DayColumn) {
          IDaySlot toDaySlot = ModelDAOHelper.DAOFactory.DaySlotDAO
            .FindAt (utcTo.Value, true);
          if ( (null != toDaySlot) && toDaySlot.Day.HasValue) {
            result = result
              .Add (Restrictions.Conjunction ()
                    .Add (Restrictions.Le ("BeginDay",
                                           (LowerBound<DateTime>) toDaySlot.Day.Value))
                    .Add (Restrictions.Le ("BeginDateTime",
                                           (LowerBound<DateTime>) utcTo)));
          }
          else { // Fallback: the day slot is not processed yet, use the date/time only instead
            result = result
              .Add (Restrictions.Le ("BeginDateTime",
                                     (LowerBound<DateTime>) utcTo));
          }
        }
        else { // !m_dayColumn
          result = result
            .Add (Restrictions.Le ("BeginDateTime",
                                   (LowerBound<DateTime>)utcTo));
        }
      }
      
      return result;
    }
    
    /// <summary>
    /// Range criterion with UTC date/times
    /// 
    /// The criterion includes the periods that have one common date/time on the left,
    /// even though it does not intersect the given period
    /// </summary>
    /// <param name="range">UTC from nullable date/time</param>
    /// <returns></returns>
    protected virtual AbstractCriterion InUtcRangeWithLeftBound (UtcDateTimeRange range)
    {
      if ((null == range) || range.IsEmpty ()) {
        return Expression.Sql ("FALSE");
      }
      
      if (!range.Lower.HasValue && !range.Upper.HasValue) {
        return Restrictions.IsNotNull ("BeginDateTime");
      }
      
      LowerBound<DateTime> utcFrom = range.Lower;
      UpperBound<DateTime> utcTo = range.Upper;
      
      // From constraint
      Junction result = Restrictions.Conjunction ();
      if (utcFrom.HasValue) {
        if (this.DayColumn) {
          IDaySlot fromDaySlot = ModelDAOHelper.DAOFactory.DaySlotDAO
            .FindAt (utcFrom.Value, true);
          if ( (null != fromDaySlot) && fromDaySlot.Day.HasValue) {
            result = result
              .Add (Restrictions.Disjunction ()
                    .Add (Restrictions.IsNull ("EndDay"))
                    .Add (Restrictions.Conjunction ()
                          .Add (Restrictions.Ge ("EndDay",
                                                 (UpperBound<DateTime>)fromDaySlot.Day.Value))
                          .Add (Restrictions.Ge ("EndDateTime",
                                                 (UpperBound<DateTime>)utcFrom))));
          }
          else { // Fallback: the day is not processed yet, use only the date/time
            result = result
              .Add (Restrictions.Disjunction ()
                    .Add (Restrictions.IsNull ("EndDay"))
                    .Add (Restrictions.Conjunction ()
                          .Add (Restrictions.Ge ("EndDateTime",
                                                 (UpperBound<DateTime>)utcFrom))));
          }
        }
        else { // !m_dayColumn
          result = result
            .Add (Restrictions.Disjunction ()
                  .Add (Restrictions.IsNull ("EndDateTime"))
                  .Add (Restrictions.Ge ("EndDateTime",
                                         (UpperBound<DateTime>)utcFrom)));
        }
      }
      
      // To constraint
      if (utcTo.HasValue) {
        if (this.DayColumn) {
          IDaySlot toDaySlot = ModelDAOHelper.DAOFactory.DaySlotDAO
            .FindAt (utcTo.Value, true);
          if ( (null != toDaySlot) && toDaySlot.Day.HasValue) {
            result = result
              .Add (Restrictions.Conjunction ()
                    .Add (Restrictions.Le ("BeginDay",
                                           (LowerBound<DateTime>) toDaySlot.Day.Value))
                    .Add (Restrictions.Lt ("BeginDateTime",
                                           (LowerBound<DateTime>) utcTo)));
          }
          else { // Fallback: the day is not processed yet, use only the date/time
            result = result
              .Add (Restrictions.Lt ("BeginDateTime",
                                     (LowerBound<DateTime>) utcTo));
          }
        }
        else { // !m_dayColumn
          result = result
            .Add (Restrictions.Lt ("BeginDateTime",
                                   (LowerBound<DateTime>)utcTo));
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
    /// <param name="range">UTC from nullable date/time</param>
    /// <returns></returns>
    protected virtual AbstractCriterion InUtcRangeWithRightBound (UtcDateTimeRange range)
    {
      if ((null == range) || range.IsEmpty ()) {
        return Expression.Sql ("FALSE");
      }
      
      if (!range.Lower.HasValue && !range.Upper.HasValue) {
        return Restrictions.IsNotNull ("BeginDateTime");
      }
      
      LowerBound<DateTime> utcFrom = range.Lower;
      UpperBound<DateTime> utcTo = range.Upper;
      
      // From constraint
      Junction result = Restrictions.Conjunction ();
      if (utcFrom.HasValue) {
        if (this.DayColumn) {
          IDaySlot fromDaySlot = ModelDAOHelper.DAOFactory.DaySlotDAO
            .FindAt (utcFrom.Value, true);
          if ( (null != fromDaySlot) && fromDaySlot.Day.HasValue) {
            result = result
              .Add (Restrictions.Disjunction ()
                    .Add (Restrictions.IsNull ("EndDay"))
                    .Add (Restrictions.Conjunction ()
                          .Add (Restrictions.Ge ("EndDay",
                                                 (UpperBound<DateTime>)fromDaySlot.Day.Value))
                          .Add (Restrictions.Gt ("EndDateTime",
                                                 (UpperBound<DateTime>)utcFrom))));
          }
          else {
            result = result
              .Add (Restrictions.Disjunction ()
                    .Add (Restrictions.IsNull ("EndDay"))
                    .Add (Restrictions.Gt ("EndDateTime",
                                           (UpperBound<DateTime>)utcFrom)));
          }
        }
        else { // !m_dayColumn
          result = result
            .Add (Restrictions.Disjunction ()
                  .Add (Restrictions.IsNull ("EndDateTime"))
                  .Add (Restrictions.Gt ("EndDateTime",
                                         (UpperBound<DateTime>)utcFrom)));
        }
      }
      
      // To constraint
      if (utcTo.HasValue) {
        if (this.DayColumn) {
          IDaySlot toDaySlot = ModelDAOHelper.DAOFactory.DaySlotDAO
            .FindAt (utcTo.Value, true);
          if ( (null != toDaySlot) && toDaySlot.Day.HasValue) {
            result = result
              .Add (Restrictions.Conjunction ()
                    .Add (Restrictions.Le ("BeginDay",
                                           (LowerBound<DateTime>) toDaySlot.Day.Value))
                    .Add (Restrictions.Le ("BeginDateTime",
                                           (LowerBound<DateTime>) utcTo)));
          }
          else { // Fallback: the day slot is not processed yet, use the date/time only instead
            result = result
              .Add (Restrictions.Le ("BeginDateTime",
                                     (LowerBound<DateTime>)utcTo));
          }
        }
        else { // !m_dayColumn
          result = result
            .Add (Restrictions.Le ("BeginDateTime",
                                   (LowerBound<DateTime>)utcTo));
        }
      }
      
      return result;
    }
    
    /// <summary>
    /// Criterion to get the machine slots that begin before
    /// the given UTC date/time
    /// </summary>
    /// <param name="utcDateTime">UTC date/time</param>
    /// <returns></returns>
    protected virtual AbstractCriterion BeginBeforeUtcDateTime (DateTime utcDateTime)
    {
      if (this.DayColumn) {
        IDaySlot daySlot = ModelDAOHelper.DAOFactory.DaySlotDAO.FindAt (utcDateTime, true);
        if ( (null != daySlot) && daySlot.Day.HasValue) {
          return Restrictions.Conjunction ()
            .Add (Restrictions.Le ("BeginDay",
                                   (LowerBound<DateTime>)daySlot.Day.Value))
            .Add (Restrictions.Le ("BeginDateTime",
                                   (LowerBound<DateTime>)utcDateTime));
        }
        else { // Fallback: the day slot is not processed yet, use the date/time only instead
          return Restrictions.Le ("BeginDateTime",
                                  (LowerBound<DateTime>)utcDateTime);
        }
      }
      else { // !m_dayColumn
        return Restrictions.Le ("BeginDateTime",
                                (LowerBound<DateTime>)utcDateTime);
      }
    }
    
    /// <summary>
    /// Criterion to get the machine slots that end after (not strictly)
    /// the given UTC date/time
    /// </summary>
    /// <param name="utcDateTime">UTC date/time</param>
    /// <returns></returns>
    protected virtual AbstractCriterion EndAfterUtcDateTime (DateTime utcDateTime)
    {
      if (this.DayColumn) {
        IDaySlot daySlot = ModelDAOHelper.DAOFactory.DaySlotDAO
          .FindAt (utcDateTime, true);
        if ( (null != daySlot) && daySlot.Day.HasValue) {
          return Restrictions.Disjunction ()
            .Add (Restrictions.IsNull ("EndDay"))
            .Add (Restrictions.Conjunction ()
                  .Add (Restrictions.Ge ("EndDay",
                                         (UpperBound<DateTime>)daySlot.Day.Value))
                  .Add (Restrictions.Ge ("EndDateTime",
                                         (UpperBound<DateTime>)utcDateTime)));
        }
        else { // Fallback: the day is not processed yet, use the date/time only instead
          return Restrictions.Disjunction ()
            .Add (Restrictions.IsNull ("EndDay"))
            .Add (Restrictions.Ge ("EndDateTime",
                                   (UpperBound<DateTime>)utcDateTime));
        }
      }
      else {
        return Restrictions.Disjunction ()
          .Add (Restrictions.IsNull ("EndDateTime"))
          .Add (Restrictions.Ge ("EndDateTime",
                                 (UpperBound<DateTime>)utcDateTime));
      }
    }
    #endregion // Range criterion methods
  }
}
