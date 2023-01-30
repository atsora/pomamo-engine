// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;

using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Business.Config;
using System.Threading;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table DaySlot
  /// </summary>
  [Serializable]
  public class DaySlot : GenericRangeSlot, ISlot, IDaySlot, IWithTemplate
  {
    #region Members
    IDayTemplate m_dayTemplate;
    DateTime? m_day;
    int? m_weekYear;
    int? m_weekNumber;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (DaySlot).FullName);

    #region Constructors and factory methods
    /// <summary>
    /// The default constructor is forbidden
    /// and is only used by NHibernate
    /// </summary>
    protected DaySlot ()
      : base (false)
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dayTemplate">not null</param>
    /// <param name="range"></param>
    public DaySlot (IDayTemplate dayTemplate,
                     UtcDateTimeRange range)
      : base (false, range)
    {
      Debug.Assert (null != dayTemplate);
      if (null == dayTemplate) {
        GetLogger ().Error ("DaySlot: " +
                            "dayTemplate argument was null");
        throw new ArgumentNullException ("dayTemplate");
      }
      m_dayTemplate = dayTemplate;
    }
    #endregion // Constructors and factory methods

    #region Getters / Setters
    /// <summary>
    /// Reference to the DayTemplate
    /// 
    /// not null
    /// </summary>
    // disable once ConvertToAutoProperty
    public virtual IDayTemplate DayTemplate
    {
      get { return m_dayTemplate; }
      set {
        Debug.Assert (null != value);
        if (null == value) {
          GetLogger ().Error ("DaySlot: " +
                              "argument was null");
          throw new ArgumentNullException ();
        }
        m_dayTemplate = value;
      }
    }

    /// <summary>
    /// Year associated to the week number
    /// </summary>
    public virtual int? WeekYear { get { return m_weekYear; } }

    /// <summary>
    /// Week number (in association with the WeekYear property)
    /// </summary>
    public virtual int? WeekNumber { get { return m_weekNumber; } }

    /// <summary>
    /// Reference to a shift
    /// 
    /// Always null
    /// </summary>
    public virtual IShift Shift
    {
      get { return null; }
      // disable once ValueParameterNotUsed
      set { }
    }

    /// <summary>
    /// Reference to the day
    /// </summary>
    public virtual DateTime? Day
    {
      get { return m_day; }
      set {
        Debug.Assert (!value.HasValue || (DateTimeKind.Utc != value.Value.Kind));
        Debug.Assert (!value.HasValue || (0 == value.Value.TimeOfDay.Ticks));

        if (object.Equals (m_day, value)) {
          return;
        }

        m_day = value;
        ComputeWeekNumber ();
      }
    }

    /// <summary>
    /// Compute the week number
    /// </summary>
    public virtual void ComputeWeekNumber ()
    {
      if (!m_day.HasValue) {
        if (log.IsDebugEnabled) {
          log.Debug ("ComputeWeekNumber: day is not defined");
        }
        return;
      }

      var day = m_day.Value;
      var firstDayOfWeek = Lemoine.Info.ConfigSet.LoadAndGet<DayOfWeek> (ConfigKeys.GetCalendarConfigKey (CalendarConfigKey.FirstDayOfWeek), DayOfWeek.Monday);
      var calendarWeekRuleString = Lemoine.Info.ConfigSet.LoadAndGet<string> (ConfigKeys.GetCalendarConfigKey (CalendarConfigKey.CalendarWeekRule), "Iso");
      var currentCulture = System.Globalization.CultureInfo.CurrentCulture;
      var calendar = currentCulture.Calendar;
      System.Globalization.CalendarWeekRule calendarWeekRule;
      if (!Enum.TryParse<System.Globalization.CalendarWeekRule> (calendarWeekRuleString, out calendarWeekRule)) {
        if (calendarWeekRuleString.Equals ("Iso", StringComparison.InvariantCultureIgnoreCase)) {
          calendarWeekRule = System.Globalization.CalendarWeekRule.FirstFourDayWeek;
        }
        else {
          log.ErrorFormat ("ComputeWeekNumber: invalid week rule {0} => use default instead", calendarWeekRuleString);
          calendarWeekRule = System.Globalization.CalendarWeekRule.FirstFourDayWeek;
        }
        // With .NET Core 3, use the ISOWeek API
        // Else, cheat (see https://blogs.msdn.microsoft.com/shawnste/2006/01/24/iso-8601-week-of-year-format-in-microsoft-net/):
        // If its Monday, Tuesday or Wednesday, then it'll
        // be the same week# as whatever Thursday, Friday or Saturday are,
        // and we always get those right
        var d = day;
        if ((DayOfWeek.Monday <= d.DayOfWeek) && (d.DayOfWeek <= DayOfWeek.Wednesday)) {
          d = d.AddDays (3);
        }
        m_weekNumber = calendar.GetWeekOfYear (d, calendarWeekRule, firstDayOfWeek);
      }
      else {
        m_weekNumber = calendar.GetWeekOfYear (day, calendarWeekRule, firstDayOfWeek);
      }
      if ((1 == day.Month) && (52 <= m_weekNumber)) {
        m_weekYear = day.Year - 1;
      }
      else if ((12 == day.Month) && (1 == m_weekNumber)) {
        m_weekYear = day.Year + 1;
      }
      else {
        m_weekYear = day.Year;
      }
      if (log.IsDebugEnabled) {
        log.DebugFormat ("ComputeWeekNumber: for {0}, week#={1} year={2}",
          day, m_weekNumber, m_weekYear);
      }
    }

    /// <summary>
    /// Day range of the slot
    /// 
    /// If Day is not defined, [,) is returned
    /// </summary>
    public override DayRange DayRange
    {
      get {
        if (!this.Day.HasValue) {
          return new DayRange (new LowerBound<DateTime> (null),
                               new UpperBound<DateTime> (null));
        }
        else {
          return new DayRange (this.Day.Value, this.Day.Value);
        }
      }
    }

    /// <summary>
    /// Begin day (from cut-off time) of the slot
    /// </summary>
    public override LowerBound<DateTime> BeginDay
    {
      get {
        return this.DayRange.Lower;
      }
    }

    /// <summary>
    /// Optionally end day of the slot
    /// </summary>
    public override UpperBound<DateTime> EndDay
    {
      get {
        return this.DayRange.Upper;
      }
    }
    #endregion // Getters / Setters

    #region Slot implementation
    /// <summary>
    /// <see cref="Slot.GetLogger" />
    /// </summary>
    /// <returns></returns>
    protected override ILog GetLogger ()
    {
      return log;
    }

    /// <summary>
    /// IComparable implementation
    /// <see cref="IComparable.CompareTo" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override int CompareTo (object obj)
    {
      if (obj is IDaySlot) {
        var other = (IDaySlot)obj;
        return this.BeginDateTime.CompareTo (other.BeginDateTime);
      }

      GetLogger ().ErrorFormat ("CompareTo: " +
                                "object {0} of invalid type",
                                obj);
      throw new ArgumentException ("object is not a DaySlot");
    }

    /// <summary>
    /// IComparable implementation
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual int CompareTo (IDaySlot other)
    {
      return this.BeginDateTime.CompareTo (other.BeginDateTime);
    }


    /// <summary>
    /// <see cref="Slot.ReferenceDataEquals" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool ReferenceDataEquals (ISlot obj)
    {
      IDaySlot other = obj as IDaySlot;
      if (other == null) {
        return false;
      }

      return NHibernateHelper.EqualsNullable (this.DayTemplate, other.DayTemplate, (a, b) => a.Id == b.Id) // Not to initialize the proxy if not required
        && object.Equals (this.Day, other.Day);
    }

    /// <summary>
    /// <see cref="Slot.IsEmpty" />
    /// </summary>
    /// <returns></returns>
    public override bool IsEmpty ()
    {
      // No gap is wished
      // Return false even when the day is null
      return false;
    }

    /// <summary>
    /// <see cref="Slot.HandleAddedSlot" />
    /// </summary>
    public override void HandleAddedSlot ()
    {
      // There is nothing to do for this slot
    }

    /// <summary>
    /// <see cref="Slot.HandleRemovedSlot" />
    /// </summary>
    public override void HandleRemovedSlot ()
    {
      // There is nothing to do for this slot
    }

    /// <summary>
    /// <see cref="Slot.HandleModifiedSlot" />
    /// </summary>
    /// <param name="oldSlot"></param>
    public override void HandleModifiedSlot (ISlot oldSlot)
    {
      // There is nothing to do for this slot
    }
    #endregion // Slot implementation

    #region IWithTemplate implementation
    /// <summary>
    /// Process the template when it has not been processed yet
    /// 
    /// until must be after slot.BeginDateTime
    /// </summary>
    /// <param name="range"></param>
    /// <param name="checkedThread"></param>
    /// <returns>true if completed, else false</returns>
    public virtual bool ProcessTemplate (CancellationToken cancellationToken, UtcDateTimeRange range,
                                         Lemoine.Threading.IChecked checkedThread)
    {
      return ProcessTemplate (cancellationToken, range,
                              null,
                              false,
                              checkedThread,
                              null);
    }

    /// <summary>
    /// Process the template when the day is unknown
    /// 
    /// until must be after slot.BeginDateTime
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="applicableRange">in UTC</param>
    /// <param name="mainModification"></param>
    /// <param name="partOfDetectionAnalysis"></param>
    /// <param name="checkedThread"></param>
    /// <param name="maxAnalysisDateTime">return false if not completed at maxAnalysisDateTime</param>
    /// <returns>true if completed, else false</returns>
    public virtual bool ProcessTemplate (CancellationToken cancellationToken, UtcDateTimeRange applicableRange,
                                         IModification mainModification,
                                         bool partOfDetectionAnalysis,
                                         Lemoine.Threading.IChecked checkedThread,
                                         DateTime? maxAnalysisDateTime)
    {
      // TODO: cancellationToken
      Debug.Assert (!this.Day.HasValue);
      Debug.Assert (null != this.DayTemplate);
      Debug.Assert (applicableRange.Overlaps (this.DateTimeRange));

      if (this.Day.HasValue) {
        GetLogger ().InfoFormat ("ProcessTemplate: " +
                                 "day is already known {0} " +
                                 "=> do nothing",
                                 this.Day.Value);
        return true;
      }

      if (!applicableRange.Overlaps (this.DateTimeRange)) { // Nothing to process, no overlap
        GetLogger ().InfoFormat ("ProcessTemplate: " +
                                 "application range {0} does not overlap {1} " +
                                 "=> nothing to process",
                                 applicableRange, this.DateTimeRange);
        return true;
      }

      // Define the applicable process range
      var range = new UtcDateTimeRange (applicableRange.Intersects (this.DateTimeRange));
      Debug.Assert (range.Lower.HasValue);
      Debug.Assert (range.Upper.HasValue);
      Debug.Assert (!range.IsEmpty ()); // Because of the check above
      Debug.Assert (this.DateTimeRange.ContainsRange (range));
      var initialSlotRange = this.DateTimeRange;

      if (range.Lower.Value < AnalysisConfigHelper.MinTemplateProcessDateTime) {
        // The lower value is too small to allow a correct process, increase it
        GetLogger ().WarnFormat ("ProcessTemplate: " +
                                 "the range lower value {0} is too small to allow a correct process " +
                                 "=> increase it to {1}",
                                 range.Lower.Value,
                                 AnalysisConfigHelper.MinTemplateProcessDateTime);
        range = new UtcDateTimeRange (AnalysisConfigHelper.MinTemplateProcessDateTime,
                                      range.Upper,
                                      range.LowerInclusive,
                                      range.UpperInclusive);
        if (range.IsEmpty ()) {
          // Nothing to do then
          return true;
        }
      }

      using (var daySlotCacheSuspend = new DaySlotCacheSuspend ())
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        // Process it one day after the other one
        DateTime currentDay = range.Lower.Value.ToLocalTime ().AddDays (-1).Date;
        DateTime lastDay = range.Upper.Value.ToLocalTime ().AddDays (1).Date;
        while (currentDay <= lastDay) { // Loop on days
          using (IDAOTransaction transaction = session.BeginTransaction ("DaySlot.ProcessTemplate")) { // Process the item one after each other
            // until 'end' only
            foreach (IDayTemplateItem item in this.DayTemplate.Items) {
              if (item.WeekDays.HasFlagDayOfWeek (currentDay.DayOfWeek)) { // Day of week is ok
                TimeSpan correctedCutOff = item.CutOff;
                if (TimeSpan.FromHours (12) < correctedCutOff) {
                  correctedCutOff = correctedCutOff - TimeSpan.FromHours (24);
                }
                ApplyForDay (initialSlotRange,
                             correctedCutOff,
                             currentDay,
                             mainModification,
                             partOfDetectionAnalysis,
                             checkedThread);
                break; // Applying the first matching item is sufficient
              }
            }
            transaction.Commit ();
          } // Transaction

          currentDay = currentDay.AddDays (1);

          if (null != checkedThread) {
            checkedThread.SetActive ();
          }

          if (maxAnalysisDateTime.HasValue && (maxAnalysisDateTime.Value <= DateTime.UtcNow)) {
            GetLogger ().WarnFormat ("ProcessTemplate: " +
                                     "maxAnalysisDateTime {0} is reached " +
                                     "=> return false",
                                     maxAnalysisDateTime);
            return false;
          }
        } // End loop on days
      } // Session

      return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="initialSlotRange">initial slot range</param>
    /// <param name="cutOff">Between -12h and +12h</param>
    /// <param name="day"></param>
    /// <param name="mainModification"></param>
    /// <param name="partOfDetectionAnalysis"></param>
    /// <param name="checkedThread"></param>
    void ApplyForDay (UtcDateTimeRange initialSlotRange,
                      TimeSpan cutOff,
                      DateTime day,
                      IModification mainModification,
                      bool partOfDetectionAnalysis,
                      Lemoine.Threading.IChecked checkedThread)
    {
      Debug.Assert (TimeSpan.FromHours (-12) < cutOff);
      Debug.Assert (cutOff <= TimeSpan.FromHours (12));
      Debug.Assert (DateTimeKind.Utc != day.Kind);
      Debug.Assert (day.Equals (day.Date));

      DateTime dayLocalBegin = day.Add (cutOff);
      DateTime dayLocalEnd;

      { // - Get the dayEnd which is the begin time of the next day, else consider the same cut-off
        DateTime nextDay = day.AddDays (1);
        IDaySlot nextDaySlot = ModelDAOHelper.DAOFactory.DaySlotDAO.FindByDay (nextDay);
        if (null != nextDaySlot) {
          Debug.Assert (nextDaySlot.BeginDateTime.HasValue);
          dayLocalEnd = nextDaySlot.BeginDateTime.Value;
        }
        else {
          dayLocalEnd = nextDay.Add (cutOff);
        }
      }
      UtcDateTimeRange dayUtcRange = new UtcDateTimeRange (dayLocalBegin, dayLocalEnd);
      UtcDateTimeRange applicableRange = new UtcDateTimeRange (initialSlotRange.Intersects (dayUtcRange));
      if (applicableRange.IsEmpty ()) { // Nothing to do
        return;
      }

      log.DebugFormat ("ApplyForDay: " +
                       "day={0} ApplicableRange={1} LocalRange={2}-{3} cutOff={4}",
                       day, applicableRange, dayLocalBegin, dayLocalEnd, cutOff);

      { // - Apply the range to day
        ApplyDayRange (day, applicableRange, mainModification, partOfDetectionAnalysis, checkedThread);
      }

      { // - Extend the previous day if needed
        DateTime previousDay = day.AddDays (-1);
        IDaySlot previousDaySlot = ModelDAOHelper.DAOFactory.DaySlotDAO.FindByDay (previousDay);
        if (null != previousDaySlot) {
          Debug.Assert (previousDaySlot.EndDateTime.HasValue);
          Debug.Assert (Bound.Compare<DateTime> (previousDaySlot.EndDateTime, applicableRange.Lower) <= 0);
          if (Bound.Compare<DateTime> (applicableRange.Lower, previousDaySlot.EndDateTime) < 0) { // - abnormal case
            GetLogger ().FatalFormat ("ApplyForDay: " +
                                      "previous day slot end {0} is after current day begin {1}",
                                      previousDaySlot.EndDateTime, applicableRange.Lower);
          }
          else if (Bound.Compare<DateTime> (previousDaySlot.EndDateTime, applicableRange.Lower) < 0) { // extend the day
            UtcDateTimeRange applicablePreviousDayRange =
              new UtcDateTimeRange (previousDaySlot.BeginDateTime.HasValue
                                    ? previousDaySlot.BeginDateTime.Value
                                    : previousDaySlot.EndDateTime.Value,
                                    applicableRange.Lower.Value);
            ApplyDayRange (previousDay,
                           applicablePreviousDayRange,
                           mainModification, partOfDetectionAnalysis, checkedThread);
          }
        }
      }
    }

    /// <summary>
    /// Apply a range [dayBegin, dayEnd) to a day
    /// </summary>
    /// <param name="day"></param>
    /// <param name="range"></param>
    /// <param name="mainModification"></param>
    /// <param name="partOfDetectionAnalysis"></param>
    /// <param name="checkedThread"></param>
    void ApplyDayRange (DateTime day, UtcDateTimeRange range, IModification mainModification, bool partOfDetectionAnalysis,
      Lemoine.Threading.IChecked checkedThread)
    {
      var dayChange = new DayChange (this.DayTemplate, day, range, mainModification);
      dayChange.Caller = checkedThread;
      dayChange.Apply ();
    }
    #endregion // IWithTemplate implementation

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[DaySlot {this.Id} Range={this.DateTimeRange} {this.DayTemplate?.ToStringIfInitialized ()} Day={m_day}]";
      }
      else {
        return $"[DaySlot {this.Id}]";
      }
    }
  }
}
