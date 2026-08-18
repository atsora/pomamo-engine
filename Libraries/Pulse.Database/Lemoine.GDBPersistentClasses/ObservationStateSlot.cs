// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System.Threading;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table ObservationStateSlot
  /// 
  /// Analysis table where are stored all
  /// the Machine Observation State periods of a given machine.
  /// </summary>
  [Serializable]
  public class ObservationStateSlot: GenericMachineRangeSlot, IObservationStateSlot, IWithTemplate
  {
    static readonly string MACHINE_STATE_TEMPLATE_PROCESS_MAX_RANGE_KEY = "MachineStateTemplate.Process.MaxRange";
    static readonly TimeSpan MACHINE_STATE_TEMPLATE_PROCESS_MAX_RANGE_DEFAULT = TimeSpan.FromDays (1);

    /// <summary>
    /// Maximum number of nested machine state templates that may be applied recursively
    /// </summary>
    static readonly string MACHINE_STATE_TEMPLATE_MAX_RECURSION_DEPTH_KEY = "MachineStateTemplate.Process.MaxRecursionDepth";
    static readonly int MACHINE_STATE_TEMPLATE_MAX_RECURSION_DEPTH_DEFAULT = 10;


    IMachineObservationState m_machineObservationState;
    IMachineStateTemplate m_machineStateTemplate;
    IUser m_user;
    IShift m_shift;
    bool? m_production;

    ILog log = LogManager.GetLogger(typeof (ObservationStateSlot).FullName);

    /// <summary>
    /// The default constructor is forbidden
    /// </summary>
    protected ObservationStateSlot ()
      : base (true)
    {
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    public ObservationStateSlot (IMachine machine,
                                 UtcDateTimeRange range)
      : base (true, machine, range)
    {
    }
    
    /// <summary>
    /// Reference to the Machine Observation State
    /// </summary>
    public virtual IMachineObservationState MachineObservationState {
      get { return m_machineObservationState; }
      set { m_machineObservationState = value; }
    }
    
    /// <summary>
    /// Reference to the Machine State Template
    /// </summary>
    public virtual IMachineStateTemplate MachineStateTemplate {
      get { return m_machineStateTemplate; }
      set { m_machineStateTemplate = value; }
    }
    
    /// <summary>
    /// Reference to the User
    /// </summary>
    public virtual IUser User {
      get { return m_user; }
      set { m_user = value; }
    }
    
    /// <summary>
    /// Does this slot correspond to a production ?
    /// </summary>
    public virtual bool? Production {
      get { return m_production; }
      set { m_production = value; }
    }

    /// <summary>
    /// Reference to the Shift if known
    /// </summary>
    public virtual IShift Shift {
      get { return m_shift; }
      set { m_shift = value; }
    }
    
    /// <summary>
    /// Reference to a day
    /// 
    /// Always null
    /// </summary>
    public virtual DateTime? Day {
      get { return null; }
      // disable once ValueParameterNotUsed
      set { }
    }
    
    /// <summary>
    /// <see cref="Slot.Consolidated" />
    /// </summary>
    public override bool Consolidated {
      get { return true; }
      set { }
    }
    
    #region Slot implementation
    /// <summary>
    /// IComparable implementation
    /// <see cref="IComparable.CompareTo" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override int CompareTo(object obj)
    {
      if (obj is ObservationStateSlot) {
        IObservationStateSlot other = (IObservationStateSlot) obj;
        if (other.Machine.Equals (this.Machine)) {
          return this.BeginDateTime.CompareTo (other.BeginDateTime);
        }
        else {
          log.ErrorFormat ("CompareTo: " +
                           "trying to compare slots " +
                           "for different machines {0} {1}",
                           this, other);
          throw new ArgumentException ("Comparison of slots from different machines");
        }
      }
      
      log.ErrorFormat ("CompareTo: " +
                       "object {0} of invalid type",
                       obj);
      throw new ArgumentException ("object is not the right slot");
    }
    
    /// <summary>
    /// IComparable implementation
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual int CompareTo(IObservationStateSlot other)
    {
      if (other.Machine.Equals (this.Machine)) {
        return this.BeginDateTime.CompareTo (other.BeginDateTime);
      }

      log.ErrorFormat ("CompareTo: " +
                       "trying to compare ObservationStateSlots " +
                       "for different machines {0} {1}",
                       this, other);
      throw new ArgumentException ("Comparison of ObservationStateSlots from different machines");
    }
    
    /// <summary>
    /// Slot implementation
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool ReferenceDataEquals (ISlot obj)
    {
      IObservationStateSlot other = obj as IObservationStateSlot;
      if (other == null) {
        return false;
      }

      if ( (this.IsProduction () || (other.Production.HasValue && other.Production.Value))
          && (!object.Equals (this.Production, other.Production))) {
        // Do not merge the slots if only one of the slot is a production slot
        return false;
      }
      return object.Equals(this.Machine, other.Machine)
        && object.Equals(this.MachineObservationState, other.MachineObservationState)
        && object.Equals(this.MachineStateTemplate, other.MachineStateTemplate)
        && object.Equals(this.User, other.User)
        && object.Equals(this.Shift, other.Shift);
    }
    
    /// <summary>
    /// <see cref="Slot.IsEmpty" />
    /// </summary>
    /// <returns></returns>
    public override bool IsEmpty ()
    {
      Debug.Assert ( (null != this.MachineObservationState)
                    || (null != this.MachineStateTemplate));
      
      return false;
    }
    
    /// <summary>
    /// <see cref="Slot.HandleAddedSlot" />
    /// </summary>
    public override void HandleAddedSlot ()
    {
      AnalysisAccumulator.AddObservationStateSlotPeriod (this, this.DateTimeRange);
    }
    
    /// <summary>
    /// <see cref="Slot.HandleRemovedSlot" />
    /// </summary>
    public override void HandleRemovedSlot ()
    {
      AnalysisAccumulator.RemoveObservationStateSlotPeriod (this, this.DateTimeRange);
    }

    /// <summary>
    /// <see cref="Slot.HandleModifiedSlot" />
    /// </summary>
    /// <param name="oldSlot"></param>
    public override void HandleModifiedSlot (ISlot oldSlot)
    {
      if (oldSlot is ObservationStateSlot) {
        ObservationStateSlot oldObservationStateSlot = oldSlot as ObservationStateSlot;
        Debug.Assert (null != oldObservationStateSlot);
        
        if (ReferenceDataEquals (oldSlot)) {
          // only the period of time changed => optimization
          
          // this:    xx.....
          // old:         xx......
          // process: xx..
          if (Bound.Compare<DateTime> (this.BeginDateTime, oldObservationStateSlot.BeginDateTime) < 0) {
            UpperBound<DateTime> end = UpperBound.GetMinimum<DateTime> (this.EndDateTime, oldObservationStateSlot.BeginDateTime.Value);
            // Add this period to the impacted tables
            AnalysisAccumulator.AddObservationStateSlotPeriod (this, new UtcDateTimeRange (this.BeginDateTime, end));
          }

          // this:        xxx....
          // old:     xx......
          // process: xx..
          if (Bound.Compare<DateTime> (oldObservationStateSlot.BeginDateTime, this.BeginDateTime) < 0) {
            Debug.Assert (this.BeginDateTime.HasValue);
            UpperBound<DateTime> end = UpperBound
              .GetMinimum<DateTime> (oldObservationStateSlot.EndDateTime,
                                     this.BeginDateTime.Value);
            // Remove this period to the impacted summary tables
            AnalysisAccumulator.RemoveObservationStateSlotPeriod (oldObservationStateSlot,
                                                                  new UtcDateTimeRange (oldObservationStateSlot.BeginDateTime, end));
          }
          
          // Intersection case
          // this:    xxxx    xx
          // old:      xx    xxxxx
          // process:  xx     xx
          // The reference data is equal => nothing to do
          
          // this:    ........xxxx
          // old:         xx
          // process:       ..xxxx
          if (NullableDateTime.Compare (oldObservationStateSlot.EndDateTime, this.EndDateTime) < 0) {
            Debug.Assert (oldObservationStateSlot.EndDateTime.HasValue);
            LowerBound<DateTime> begin = LowerBound.GetMaximum<DateTime> (this.BeginDateTime, oldObservationStateSlot.EndDateTime.Value);
            AnalysisAccumulator.AddObservationStateSlotPeriod (this,
                                                               new UtcDateTimeRange (begin, this.EndDateTime));
          }
          
          // this:     xx
          // old:     .....xx
          // process:    ..xx
          if (NullableDateTime.Compare (this.EndDateTime, oldObservationStateSlot.EndDateTime) < 0) {
            Debug.Assert (this.EndDateTime.HasValue);
            LowerBound<DateTime> begin = LowerBound.GetMaximum<DateTime> (oldObservationStateSlot.BeginDateTime,
                                                                          this.EndDateTime.Value);
            // Remove this period from the impacted summary tables
            AnalysisAccumulator.RemoveObservationStateSlotPeriod (oldObservationStateSlot,
                                                                  new UtcDateTimeRange (begin, oldObservationStateSlot.EndDateTime));
          }
        }
        else { // More changes: do not try to optimize it here (done in the accumulator)
          AnalysisAccumulator.RemoveObservationStateSlotPeriod (oldObservationStateSlot,
                                                                oldObservationStateSlot.DateTimeRange);
          AnalysisAccumulator.AddObservationStateSlotPeriod (this,
                                                             this.DateTimeRange);
        }
      }
      else {
        Debug.Assert (false);
        log.FatalFormat ("HandleModifiedSlot: " +
                         "unexpected slot type {0}",
                         oldSlot.GetType());
        throw new ArgumentException ("Not supported slot");
      }
    }
    #endregion // Slot implementation
    
    /// <summary>
    /// Process the template when MachineObservationState is null
    ///
    /// applicableRange must overlaps the date/time range of the slot
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="applicableRange">Upper must have a value</param>
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
      Debug.Assert (applicableRange.Upper.HasValue);
      Debug.Assert (null == this.MachineObservationState);
      Debug.Assert (null != this.MachineStateTemplate);
      Debug.Assert (this.DateTimeRange.Overlaps (applicableRange));

      if (!this.DateTimeRange.Overlaps (applicableRange)) {
        log.Fatal ($"ProcessTemplate: DateTimeRange {this.DateTimeRange} does not overlap applicableRange {applicableRange} => fallback, return true");
        return true;
      }

      UtcDateTimeRange correctedRange = new UtcDateTimeRange (applicableRange.Intersects (this.DateTimeRange));
      Debug.Assert (!correctedRange.IsEmpty ()); // Because of the pre-condition above: this.DateTimeRange.Overlaps (applicableRange)
      Debug.Assert (correctedRange.Upper.HasValue); // because applicableRange.Upper.HasValue

      bool result = ProcessTemplate (this.Machine,
                                     this.MachineStateTemplate,
                                     this.User,
                                     this.Shift,
                                     correctedRange,
                                     mainModification,
                                     partOfDetectionAnalysis,
                                     checkedThread,
                                     maxAnalysisDateTime,
                                     log);
      AnalysisAccumulator.PushMessage ("Cache/ClearDomainByMachine/MachineObservationStateAssociation/" + this.Machine.Id
                                       + "?Broadcast=true");
      return result;
    }


    /// <summary>
    /// Process the template when MachineObservationState is null
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="machineStateTemplate">not null</param>
    /// <param name="user"></param>
    /// <param name="currentShift">nullable</param>
    /// <param name="applicableRange">not empty and Upper must have a value</param>
    /// <param name="mainModification"></param>
    /// <param name="partOfDetectionAnalysis"></param>
    /// <param name="checkedThread"></param>
    /// <param name="maxAnalysisDateTime">return false if not completed at maxAnalysisDateTime</param>
    /// <param name="log"></param>
    /// <returns>true if completed, else false</returns>
    static bool ProcessTemplate (IMachine machine,
                                 IMachineStateTemplate machineStateTemplate,
                                 IUser user,
                                 IShift currentShift,
                                 UtcDateTimeRange applicableRange,
                                 IModification mainModification,
                                 bool partOfDetectionAnalysis,
                                 Lemoine.Threading.IChecked checkedThread,
                                 DateTime? maxAnalysisDateTime,
                                 ILog log)
    {
      Debug.Assert (null != machine);
      Debug.Assert (null != machineStateTemplate);
      Debug.Assert (!applicableRange.IsEmpty ()); // Because of the pre-condition
      Debug.Assert (applicableRange.Upper.HasValue); // because applicableRange.Upper.HasValue

      Bound<DateTime> utcBeginDateTime = applicableRange.Lower;

      // The machine state templates that are being applied, from the root one to the current one.
      // It is used to detect the cycles when an item applies recursively another machine state template
      var ancestorTemplateIds = new List<int> { machineStateTemplate.Id };

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        using (IDAOTransaction transaction = session.BeginTransaction ("ObservationStateSlot.ProcessTemplateLongPeriod"))
        { // - Process first a long period item, if any
          UpperBound<DateTime> limitSpecifiedDateTime;
          IMachineStateTemplateItem itemWithLongPeriod =
            IsItemForLongPeriod (machineStateTemplate, applicableRange.Lower, out limitSpecifiedDateTime, log);
          if (null != itemWithLongPeriod) {
            if (log.IsDebugEnabled) {
              log.Debug ($"ProcessTemplate: process item for long period {itemWithLongPeriod}");
            }
            checkedThread?.SetActive ();
            UpperBound<DateTime> endDateTime = UpperBound.GetMinimum<DateTime> (applicableRange.Upper, limitSpecifiedDateTime);
            IShift shift = itemWithLongPeriod.Shift ?? currentShift;
            Debug.Assert (BoundType.Lower == utcBeginDateTime.BoundType);
            UtcDateTimeRange range = new UtcDateTimeRange ((LowerBound<DateTime>)utcBeginDateTime, endDateTime);
            if (!range.IsEmpty ()) {
              if (maxAnalysisDateTime.HasValue) {
                // Do not process a range that would be too large in one step
                TimeSpan limitTimeSpan = Lemoine.Info.ConfigSet.LoadAndGet<TimeSpan> (MACHINE_STATE_TEMPLATE_PROCESS_MAX_RANGE_KEY,
                                                                                      MACHINE_STATE_TEMPLATE_PROCESS_MAX_RANGE_DEFAULT);
                var limitMax = new UpperBound<DateTime> (null);
                var upperOrNow = UpperBound.GetMinimum<DateTime> (range.Upper,
                                                                  DateTime.UtcNow).Value;
                for (var limitMin = upperOrNow.Subtract (limitTimeSpan);
                     Bound.Compare<DateTime> (range.Lower, limitMax) < 0;
                     limitMin = limitMin.Subtract (limitTimeSpan) ) {
                  checkedThread?.SetActive ();
                  UtcDateTimeRange limitedRange =
                    new UtcDateTimeRange (range.Intersects (new UtcDateTimeRange (limitMin, limitMax)));
                  if (limitedRange.IsEmpty ()) {
                    log.Error ("ProcessTemplate: empty limitedRange");
                  }
                  Debug.Assert (!limitedRange.IsEmpty ());
                  ApplyMachineObservationState (machine,
                                                machineStateTemplate,
                                                itemWithLongPeriod.MachineObservationState,
                                                user,
                                                shift,
                                                limitedRange,
                                                mainModification, partOfDetectionAnalysis, log, checkedThread);
                  limitMax = limitMin;
                  if (maxAnalysisDateTime.HasValue && (maxAnalysisDateTime.Value <= DateTime.UtcNow)) {
                    log.Warn ($"ProcessTemplate: maxAnalysisDateTime {maxAnalysisDateTime} is reached, return false but the analysis is completed in range {limitMin}-{utcBeginDateTime} => return false");
                    transaction.Commit ();
                    return false;
                  }
                } // for
              }
              else { // !maxAnalysisDateTime.HasValue => this can be done in one step
                ApplyMachineObservationState (machine,
                                              machineStateTemplate,
                                              itemWithLongPeriod.MachineObservationState,
                                              user,
                                              shift,
                                              range,
                                              mainModification, partOfDetectionAnalysis, log, checkedThread);
              }
              if (Bound.Compare<DateTime> (endDateTime, applicableRange.Upper) == 0) {
                // - The process is completed
                transaction.Commit ();
                return true;
              }
              else { // - There is potentially still a period to process
                Debug.Assert (endDateTime.HasValue);
                utcBeginDateTime = endDateTime.Value.ToUniversalTime ();
              }
            }
          }
          transaction.Commit ();
        } // The process is completed now until beginDateTime
        checkedThread?.SetActive ();
        if (maxAnalysisDateTime.HasValue && (maxAnalysisDateTime.Value <= DateTime.UtcNow)) {
          log.Warn ($"ProcessTemplate: maxAnalysisDateTime {maxAnalysisDateTime} is reached, return false but the analysis is completed until {utcBeginDateTime} => return false");
          return false;
        }

        if (new UtcDateTimeRange (new LowerBound<DateTime> (null), applicableRange.Upper)
            .ContainsElement (utcBeginDateTime)) { // There is still something to process
          Debug.Assert (utcBeginDateTime.HasValue);
          Debug.Assert (Bound.Compare<DateTime> (utcBeginDateTime, applicableRange.Upper) < 0);
          using (IDAOTransaction transaction = session.BeginTransaction ("ObservationStateSlot.ProcessTemplateItems"))
          { // Process the item one after each other
            // until 'maxEndDateTime' only
            foreach (IMachineStateTemplateItem item in machineStateTemplate.Items) {
              ProcessTemplateItem (machine, machineStateTemplate, ancestorTemplateIds, user, currentShift, item,
                                   utcBeginDateTime.Value, applicableRange.Upper.Value,
                                   mainModification, partOfDetectionAnalysis, checkedThread, log);
              checkedThread?.SetActive ();
              if (maxAnalysisDateTime.HasValue && (maxAnalysisDateTime.Value <= DateTime.UtcNow)) {
                log.Warn ($"ProcessTemplate: maxAnalysisDateTime {maxAnalysisDateTime} is reached, return false, the analysis is completed for some items");
                transaction.Commit ();
                return false;
              }
            }
            transaction.Commit ();
          } // transaction
        }
      }

      return true;
    }

    /// <summary>
    /// Add a machine observation state / shift during a specified period
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="machineStateTemplate"></param>
    /// <param name="machineObservationState"></param>
    /// <param name="user"></param>
    /// <param name="shift"></param>
    /// <param name="range">range in UTC</param>
    /// <param name="mainModification"></param>
    /// <param name="partOfDetectionAnalysis"></param>
    /// <param name="log"></param>
    /// <param name="checkedThread"></param>
    static void ApplyMachineObservationState (IMachine machine,
                                              IMachineStateTemplate machineStateTemplate,
                                              IMachineObservationState machineObservationState,
                                              IUser user,
                                              IShift shift,
                                              UtcDateTimeRange range,
                                              IModification mainModification,
                                              bool partOfDetectionAnalysis,
                                              ILog log,
                                              Lemoine.Threading.IChecked checkedThread)
    {
      Debug.Assert (null != machine);
      if (range.IsEmpty ()) {
        log.Fatal ($"ApplyMachineObservationState: empty range. StackTrace: {System.Environment.StackTrace}");
      }
      Debug.Assert (!range.IsEmpty ());

      if (log.IsDebugEnabled) {
        log.Debug ($"ApplyMachineObservationState: apply {machineObservationState} / {shift} in range {range}");
      }

      MachineObservationStateAssociation association =
        new MachineObservationStateAssociation (machine, range, mainModification, partOfDetectionAnalysis);
      association.MachineObservationState = machineObservationState;
      association.MachineStateTemplate = machineStateTemplate;
      association.User = user;
      association.Shift = shift;
      association.Caller = checkedThread;
      association.Apply ();
    }

    /// <summary>
    /// Process a machine state template item until a specified date/time
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="machineStateTemplate">machine state template that is associated to the machine (the root one in case of recursion)</param>
    /// <param name="ancestorTemplateIds">ids of the machine state templates that are being applied, from the root one to the one that owns the item</param>
    /// <param name="user"></param>
    /// <param name="currentShift"></param>
    /// <param name="item"></param>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    /// <param name="mainModification"></param>
    /// <param name="partOfDetectionAnalysis"></param>
    /// <param name="checkedThread"></param>
    /// <param name="log"></param>
    static void ProcessTemplateItem (IMachine machine,
                                     IMachineStateTemplate machineStateTemplate,
                                     IEnumerable<int> ancestorTemplateIds,
                                     IUser user,
                                     IShift currentShift,
                                     IMachineStateTemplateItem item,
                                     DateTime begin,
                                     DateTime end,
                                     IModification mainModification,
                                     bool partOfDetectionAnalysis,
                                     Lemoine.Threading.IChecked checkedThread,
                                     ILog log)
    {
      Debug.Assert (begin < end);

      if (log.IsDebugEnabled) {
        log.Debug ($"ProcessTemplateItem: process item {item} between {begin} and {end}");
      }

      IShift shift = item.Shift ?? currentShift;

      if (item.Day.HasValue && !item.YearlyRepeat) { // A unique specific day
        // Do not take into account here item.WeekDays because item.Day is specified,
        // and normally WeekDays should be AllDays here
        Debug.Assert (item.WeekDays.HasFlag (WeekDay.AllDays));
        Debug.Assert (DateTimeKind.Local == item.Day.Value.Kind);
        if (item.IsWeekApplicable (item.Day.Value)) {
          ApplyItemForDate (machine, machineStateTemplate, ancestorTemplateIds, item,
                            user, shift, item.Day.Value, begin, end,
                            mainModification, partOfDetectionAnalysis, checkedThread, log);
        }
      }
      else if (item.Day.HasValue) { // item.YearlyRepeat: the same day every year, mainly for the public holidays
        Debug.Assert (DateTimeKind.Local == item.Day.Value.Kind);
        var firstYear = begin.ToLocalTime ().Year;
        var lastYear = end.ToLocalTime ().Year;
        for (var year = firstYear; year <= lastYear; ++year) { // Loop on years
          var day = item.GetYearlyDay (year);
          if (day.HasValue && item.IsWeekApplicable (day.Value)) {
            ApplyItemForDate (machine, machineStateTemplate, ancestorTemplateIds, item,
                              user, shift, day.Value, begin, end,
                              mainModification, partOfDetectionAnalysis, checkedThread, log);
          }
          checkedThread?.SetActive ();
        }
      }
      else if (item.HasDayRestriction () || !item.TimePeriod.IsFullDay ()) {
        // Process it one day after the other one
        DateTime currentDay = begin.ToLocalTime ().Date;
        DateTime lastDay = end.ToLocalTime ().Date;
        while (currentDay <= lastDay) { // Loop on days
          if (item.IsDayApplicable (currentDay)) { // Day of week and week number are ok
            ApplyItemForDate (machine, machineStateTemplate, ancestorTemplateIds, item,
                              user, shift, currentDay, begin, end,
                              mainModification, partOfDetectionAnalysis, checkedThread, log);
          }
          currentDay = currentDay.AddDays (1);
          checkedThread?.SetActive ();
        }
      }
      else { // long period
        ApplyItem (machine, machineStateTemplate, ancestorTemplateIds, item,
                   user, shift, new UtcDateTimeRange (begin, end),
                   mainModification, partOfDetectionAnalysis, checkedThread, log);
      }
    }

    /// <summary>
    /// Apply a machine state template item on a specific day, considering its time period of day
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="machineStateTemplate"></param>
    /// <param name="ancestorTemplateIds"></param>
    /// <param name="item"></param>
    /// <param name="user"></param>
    /// <param name="shift"></param>
    /// <param name="date">local day</param>
    /// <param name="minBeginDateTime"></param>
    /// <param name="maxEndDateTime"></param>
    /// <param name="mainModification"></param>
    /// <param name="partOfDetectionAnalysis"></param>
    /// <param name="checkedThread"></param>
    /// <param name="log"></param>
    static void ApplyItemForDate (IMachine machine,
                                  IMachineStateTemplate machineStateTemplate,
                                  IEnumerable<int> ancestorTemplateIds,
                                  IMachineStateTemplateItem item,
                                  IUser user,
                                  IShift shift,
                                  DateTime date,
                                  DateTime minBeginDateTime,
                                  DateTime maxEndDateTime,
                                  IModification mainModification,
                                  bool partOfDetectionAnalysis,
                                  Lemoine.Threading.IChecked checkedThread,
                                  ILog log)
    {
      Debug.Assert (DateTimeKind.Local == date.Kind);
      Debug.Assert (date.Equals (date.Date));
      Debug.Assert (DateTimeKind.Unspecified != minBeginDateTime.Kind);
      Debug.Assert (DateTimeKind.Unspecified != maxEndDateTime.Kind);

      LocalDateTimeRange range = new LocalDateTimeRange (minBeginDateTime, maxEndDateTime);
      Debug.Assert (!range.IsEmpty ());

      { // - Consider day
        LocalDateTimeRange dateRange = new LocalDateTimeRange (date, date.AddDays (1));
        range = new LocalDateTimeRange (range.Intersects (dateRange));
        if (range.IsEmpty ()) {
          if (log.IsDebugEnabled) {
            log.Debug ($"ApplyItemForDate: nothing to do because the day {date} is not in range {minBeginDateTime}-{maxEndDateTime}");
          }
          return;
        }
        Debug.Assert (!range.IsEmpty ());
      }

      Debug.Assert (range.Lower.HasValue); // Because of the intersection with the day
      Debug.Assert (range.Upper.HasValue); // Because of the intersection with the day
      Debug.Assert (range.Duration.HasValue); // Because of the two asserts above
      Debug.Assert (range.Duration.Value <= TimeSpan.FromHours (25)); // A day is maximum 25 hours because of DST

      if (false == item.TimePeriod.IsFullDay ()) { // - Consider timePeriod
        range = range.Intersects (date, item.TimePeriod);
        if (range.IsEmpty ()) {
          if (log.IsDebugEnabled) {
            log.Debug ($"ApplyItemForDate: nothing to do because the time period {item.TimePeriod} is not between {minBeginDateTime} and {maxEndDateTime}");
          }
          return;
        }
        Debug.Assert (!range.IsEmpty ());
      }

      ApplyItem (machine, machineStateTemplate, ancestorTemplateIds, item,
                 user, shift, range.ToUniversalTime (),
                 mainModification, partOfDetectionAnalysis, checkedThread, log);
    }

    /// <summary>
    /// Apply a machine state template item on a specified range:
    /// either apply its machine observation state,
    /// or apply recursively the machine state template it references
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="machineStateTemplate"></param>
    /// <param name="ancestorTemplateIds"></param>
    /// <param name="item"></param>
    /// <param name="user"></param>
    /// <param name="shift"></param>
    /// <param name="range">not empty range in UTC</param>
    /// <param name="mainModification"></param>
    /// <param name="partOfDetectionAnalysis"></param>
    /// <param name="checkedThread"></param>
    /// <param name="log"></param>
    static void ApplyItem (IMachine machine,
                           IMachineStateTemplate machineStateTemplate,
                           IEnumerable<int> ancestorTemplateIds,
                           IMachineStateTemplateItem item,
                           IUser user,
                           IShift shift,
                           UtcDateTimeRange range,
                           IModification mainModification,
                           bool partOfDetectionAnalysis,
                           Lemoine.Threading.IChecked checkedThread,
                           ILog log)
    {
      Debug.Assert (!range.IsEmpty ());

      if (null != item.SubMachineStateTemplate) { // Recursive application of another machine state template
        ProcessSubMachineStateTemplate (machine, machineStateTemplate, ancestorTemplateIds,
                                        item.SubMachineStateTemplate, user, shift, range,
                                        mainModification, partOfDetectionAnalysis, checkedThread, log);
        return;
      }

      if (null == item.MachineObservationState) {
        log.Error ($"ApplyItem: item {item} references neither a machine observation state nor a machine state template => skip it");
        return;
      }

      ApplyMachineObservationState (machine, machineStateTemplate, item.MachineObservationState,
                                    user, shift, range,
                                    mainModification, partOfDetectionAnalysis, log, checkedThread);
    }

    /// <summary>
    /// Apply recursively the items of a sub machine state template on the specified range
    ///
    /// Note: the resulting observation state slots keep a reference to the root machine state template,
    /// the one that is associated to the machine
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="machineStateTemplate">root machine state template</param>
    /// <param name="ancestorTemplateIds">not null</param>
    /// <param name="subMachineStateTemplate">not null</param>
    /// <param name="user"></param>
    /// <param name="currentShift"></param>
    /// <param name="range">not empty and bounded range in UTC</param>
    /// <param name="mainModification"></param>
    /// <param name="partOfDetectionAnalysis"></param>
    /// <param name="checkedThread"></param>
    /// <param name="log"></param>
    static void ProcessSubMachineStateTemplate (IMachine machine,
                                                IMachineStateTemplate machineStateTemplate,
                                                IEnumerable<int> ancestorTemplateIds,
                                                IMachineStateTemplate subMachineStateTemplate,
                                                IUser user,
                                                IShift currentShift,
                                                UtcDateTimeRange range,
                                                IModification mainModification,
                                                bool partOfDetectionAnalysis,
                                                Lemoine.Threading.IChecked checkedThread,
                                                ILog log)
    {
      Debug.Assert (null != subMachineStateTemplate);
      Debug.Assert (!range.IsEmpty ());
      Debug.Assert (range.Lower.HasValue);
      Debug.Assert (range.Upper.HasValue);

      if (ancestorTemplateIds.Contains (subMachineStateTemplate.Id)) {
        log.Error ($"ProcessSubMachineStateTemplate: {subMachineStateTemplate.ToStringIfInitialized ()} is already being applied, there is a cycle in the machine state templates => skip it");
        return;
      }

      var maxRecursionDepth = Lemoine.Info.ConfigSet
        .LoadAndGet<int> (MACHINE_STATE_TEMPLATE_MAX_RECURSION_DEPTH_KEY, MACHINE_STATE_TEMPLATE_MAX_RECURSION_DEPTH_DEFAULT);
      if (maxRecursionDepth <= ancestorTemplateIds.Count ()) {
        log.Error ($"ProcessSubMachineStateTemplate: the maximum recursion depth {maxRecursionDepth} is reached with {subMachineStateTemplate.ToStringIfInitialized ()} => skip it");
        return;
      }

      if (!range.Lower.HasValue || !range.Upper.HasValue) {
        log.Error ($"ProcessSubMachineStateTemplate: unbounded range {range} => skip it");
        return;
      }

      if (log.IsDebugEnabled) {
        log.Debug ($"ProcessSubMachineStateTemplate: apply {subMachineStateTemplate.ToStringIfInitialized ()} in range {range}");
      }

      var subAncestorTemplateIds = ancestorTemplateIds
        .Concat (new int[] { subMachineStateTemplate.Id })
        .ToList ();
      foreach (var subItem in subMachineStateTemplate.Items) {
        ProcessTemplateItem (machine, machineStateTemplate, subAncestorTemplateIds, user, currentShift, subItem,
                             range.Lower.Value, range.Upper.Value,
                             mainModification, partOfDetectionAnalysis, checkedThread, log);
        checkedThread?.SetActive ();
      }
    }

    /// <summary>
    /// Is there an item that is applicable a long period of time
    /// (more that one week)
    /// since the specified begin date/time
    /// </summary>
    /// <param name="machineStateTemplate"></param>
    /// <param name="begin"></param>
    /// <param name="limitSpecifiedDateTime">Limit UTC or Local date/time</param>
    /// <param name="log"></param>
    /// <returns>MachineStateTemplate item that is available during a long period of time</returns>
    static IMachineStateTemplateItem IsItemForLongPeriod (IMachineStateTemplate machineStateTemplate,
                                                          LowerBound<DateTime> begin,
                                                          out UpperBound<DateTime> limitSpecifiedDateTime,
                                                          ILog log)
    {
      IMachineStateTemplateItem itemWithLongPeriod = null;
      limitSpecifiedDateTime = new UpperBound<DateTime> (null); // No limit local date/time

      // - If there is stop, the period can't be longer than one week, return false
      if (0 < machineStateTemplate.Stops.Count) {
        if (log.IsDebugEnabled) {
          log.Debug ("IsItemForLongPeriod: return null because there is a stop");
        }
        return null;
      }

      foreach (IMachineStateTemplateItem item in machineStateTemplate.Items) {
        if (null != item.SubMachineStateTemplate) {
          // - The items of the referenced machine state template must be applied one after the other,
          // this optimization can't be used
          if (log.IsDebugEnabled) {
            log.Debug ("IsItemForLongPeriod: return null because an item applies recursively another machine state template");
          }
          return null;
        }
        else if (item.WeekNumber.HasValue || item.YearlyRepeat) {
          // - The applicable periods are not contiguous, this optimization can't be used
          if (log.IsDebugEnabled) {
            log.Debug ("IsItemForLongPeriod: return null because an item is restricted to some weeks or is repeated every year");
          }
          return null;
        }
        else if (item.Day.HasValue) {
          Debug.Assert (DateTimeKind.Utc != item.Day.Value.Kind);
          DateTime dayBegin = new DateTime (item.Day.Value.Ticks, DateTimeKind.Local);
          // Note: The cut-off is not taken into account for the moment here
          dayBegin = dayBegin.Add (item.TimePeriod.Begin);
          if (NullableDateTime.Compare (dayBegin, limitSpecifiedDateTime) < 0) {
            limitSpecifiedDateTime = dayBegin;
          }
          Debug.Assert (limitSpecifiedDateTime.HasValue);
          if (log.IsDebugEnabled) {
            log.Debug ($"IsItemForLongPeriod: day begin {dayBegin} identified, adjust limitLocalDateTime to {limitSpecifiedDateTime} and continue");
          }
          continue;
        }
        else if (false == item.TimePeriod.IsFullDay ()) {
          // - If there is a defined time period (and the time period is not 0:00-0:00),
          // without a day,
          // the period can't be longer than one day, return false
          if (log.IsDebugEnabled) {
            log.Debug ("IsItemForLongPeriod: return null because there is a time period");
          }
          return null;
        }
        else if (!item.WeekDays.HasFlag (WeekDay.AllDays)) { // - Not the whole week,
          // return false
          if (log.IsDebugEnabled) {
            log.Debug ("IsItemForLongPeriod: not the whole week is considered here, return null");
          }
          return null;
        }
        else { // This is an item that is applicable for all times and days
          if (null != itemWithLongPeriod) {
            log.Warn ($"IsItemForLongPeriod: applicable item {itemWithLongPeriod} will be overriden by {item}");
          }
          itemWithLongPeriod = item;
        }
      }

      if (log.IsDebugEnabled) {
        log.Debug ($"IsItemForLongPeriod: return {itemWithLongPeriod} with limitLocalDateTime {limitSpecifiedDateTime}");
      }
      Debug.Assert (!limitSpecifiedDateTime.HasValue || (DateTimeKind.Unspecified != limitSpecifiedDateTime.Value.Kind));
      return itemWithLongPeriod;
    }


    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals(IObservationStateSlot other)
    {
      return this.Equals ((object) other);
    }
    
    /// <summary>
    ///   Determines whether the specified Object
    ///   is equal to the current Object
    /// </summary>
    /// <param name="obj">The object to compare with the current object</param>
    /// <returns>true if the specified Object is equal to the current Object; otherwise, false</returns>
    public override bool Equals(object obj)
    {
      if (object.ReferenceEquals(this,obj)) {
        return true;
      }

      if (obj == null) {
        return false;
      }
      // Note: do not use here this.GetType () != obj.GetType
      //       because a Xxx may be compared with a XxxProxy
      //       which may return false although true might be returned
      ObservationStateSlot other = obj as ObservationStateSlot;
      if (null == other) {
        return false;
      }
      if (this.Id != 0) {
        return (other.Id == this.Id) && (other.Version == this.Version);
      }
      return false;
    }


    /// <summary>
    ///   Serves as a hash function for a particular type
    /// </summary>
    /// <returns>A hash code for the current Object</returns>
    public override int GetHashCode()
    {
      if (0 != Id) {
        int hashCode = 0;
        unchecked {
          hashCode += 1000000007 * Id.GetHashCode();
          hashCode += 1000000009 * Version.GetHashCode();
        }
        return hashCode;
      }
      else {
        return base.GetHashCode ();
      }
    }

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[ObservationStateSlot {this.Id} {this.Machine?.ToStringIfInitialized ()} Range={this.DateTimeRange}]";
      }
      else {
        return $"[ObservationStateSlot {this.Id}]";
      }
    }
  }
}
