// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    
    #region MachineStateTemplate
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
        log.FatalFormat ("ProcessTemplate: " +
                         "DateTimeRange {0} does not overlap applicableRange {1} " +
                         "=> fallback, return true");
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
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        using (IDAOTransaction transaction = session.BeginTransaction ("ObservationStateSlot.ProcessTemplateLongPeriod"))
        { // - Process first a long period item, if any
          UpperBound<DateTime> limitSpecifiedDateTime;
          IMachineStateTemplateItem itemWithLongPeriod =
            IsItemForLongPeriod (machineStateTemplate, applicableRange.Lower, out limitSpecifiedDateTime, log);
          if (null != itemWithLongPeriod) {
            log.DebugFormat ("ProcessTemplate: " +
                             "process item for long period {0}",
                             itemWithLongPeriod);
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
                    log.ErrorFormat ("ProcessTemplate: " +
                                     "empty limitedRange");
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
                    log.WarnFormat ("ProcessTemplate: " +
                                    "maxAnalysisDateTime {0} is reached, return false " +
                                    "but the analysis is completed in range {1}-{2} " +
                                    "=> return false",
                                    maxAnalysisDateTime, limitMin, utcBeginDateTime);
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
          log.WarnFormat ("ProcessTemplate: " +
                          "maxAnalysisDateTime {0} is reached, return false " +
                          "but the analysis is completed until {1} " +
                          "=> return false",
                          maxAnalysisDateTime, utcBeginDateTime);
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
              ProcessTemplateItem (machine, machineStateTemplate, user, currentShift, item,
                                   utcBeginDateTime.Value, applicableRange.Upper.Value,
                                   mainModification, partOfDetectionAnalysis, checkedThread, log);
              checkedThread?.SetActive ();
              if (maxAnalysisDateTime.HasValue && (maxAnalysisDateTime.Value <= DateTime.UtcNow)) {
                log.WarnFormat ("ProcessTemplate: " +
                                "maxAnalysisDateTime {0} is reached, return false, " +
                                "the analysis is completed for some items",
                                maxAnalysisDateTime);
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
        log.FatalFormat ("ApplyMachineObservationState: " +
                         "empty range. " +
                         "StackTrace: {0}",
                         System.Environment.StackTrace);
      }
      Debug.Assert (!range.IsEmpty ());
      
      log.DebugFormat ("ApplyMachineObservationState: " +
                       "apply {0} / {1} in range {2}",
                       machineObservationState, shift, range);
      
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
    /// <param name="machineStateTemplate"></param>
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
      
      log.DebugFormat ("ProcessTemplateItem: " +
                       "process item {0} between {1} and {2}",
                       item, begin, end);
      
      IShift shift = item.Shift ?? currentShift;
      if (item.Day.HasValue) {
        // Do not take into account here item.WeekDays because item.Day is specified,
        // and normally WeekDays should be AllDays here
        Debug.Assert (item.WeekDays.HasFlag (WeekDay.AllDays));
        Debug.Assert (DateTimeKind.Local == item.Day.Value.Kind);
        ApplyForDate (machine,
                      machineStateTemplate,
                      item.MachineObservationState,
                      user,
                      shift,
                      item.Day.Value,
                      item.TimePeriod,
                      begin, end,
                      mainModification, partOfDetectionAnalysis, log, checkedThread);
      }
      else if ((false == item.TimePeriod.IsFullDay ())
               || (false == item.WeekDays.HasFlag (WeekDay.AllDays))) {
        // Process it one day after the other one
        DateTime currentDay = begin.ToLocalTime ().Date;
        DateTime lastDay = end.ToLocalTime ().Date;
        while (currentDay <= lastDay) { // Loop on days
          if (item.WeekDays.HasFlagDayOfWeek (currentDay.DayOfWeek)) { // Day of week is ok
            ApplyForDate (machine,
                          machineStateTemplate,
                          item.MachineObservationState,
                          user,
                          shift,
                          currentDay,
                          item.TimePeriod,
                          begin, end,
                          mainModification, partOfDetectionAnalysis, log, checkedThread);
          }
          currentDay = currentDay.AddDays (1);
          checkedThread?.SetActive ();
        }
      }
      else { // long period
        ApplyMachineObservationState (machine,
                                      machineStateTemplate,
                                      item.MachineObservationState,
                                      user,
                                      shift,
                                      new UtcDateTimeRange (begin, end),
                                      mainModification, partOfDetectionAnalysis, log, checkedThread);
      }
    }
    
    static void ApplyForDate (IMachine machine,
                              IMachineStateTemplate machineStateTemplate,
                              IMachineObservationState machineObservationState,
                              IUser user,
                              IShift shift,
                              DateTime date,
                              TimePeriodOfDay timePeriod,
                              DateTime minBeginDateTime,
                              DateTime maxEndDateTime,
                              IModification mainModification,
                              bool partOfDetectionAnalysis,
                              ILog log,
                              Lemoine.Threading.IChecked checkedThread)
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
          log.DebugFormat ("ApplyForDay: " +
                           "nothing to do because the day {0} is not in range {1}-{2}",
                           date, minBeginDateTime, maxEndDateTime);
          return;
        }
        Debug.Assert (!range.IsEmpty ());
      }
      
      Debug.Assert (range.Lower.HasValue); // Because of the intersection with the day
      Debug.Assert (range.Upper.HasValue); // Because of the intersection with the day
      Debug.Assert (range.Duration.HasValue); // Because of the two asserts above
      Debug.Assert (range.Duration.Value <= TimeSpan.FromHours (25)); // A day is maximum 25 hours because of DST
      
      if (false == timePeriod.IsFullDay ()) { // - Consider timePeriod
        range = range.Intersects (date, timePeriod);
        if (range.IsEmpty ()) {
          log.DebugFormat ("ApplyForDay: " +
                           "nothing to do because the time period {0} is not between {1} and {2}",
                           timePeriod, minBeginDateTime, maxEndDateTime);
          return;
        }
        Debug.Assert (!range.IsEmpty ());
      }
      
      ApplyMachineObservationState (machine,
                                    machineStateTemplate,
                                    machineObservationState,
                                    user,
                                    shift,
                                    range.ToUniversalTime (),
                                    mainModification, partOfDetectionAnalysis, log, checkedThread);
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
        log.DebugFormat ("IsItemForLongPeriod: " +
                         "return false because there is a stop");
        return null;
      }
      
      foreach (IMachineStateTemplateItem item in machineStateTemplate.Items) {
        if (item.Day.HasValue) {
          Debug.Assert (DateTimeKind.Utc != item.Day.Value.Kind);
          DateTime dayBegin = new DateTime (item.Day.Value.Ticks, DateTimeKind.Local);
          // Note: The cut-off is not taken into account for the moment here
          dayBegin = dayBegin.Add (item.TimePeriod.Begin);
          if (NullableDateTime.Compare (dayBegin, limitSpecifiedDateTime) < 0) {
            limitSpecifiedDateTime = dayBegin;
          }
          Debug.Assert (limitSpecifiedDateTime.HasValue);
          log.DebugFormat ("IsItemForLongPeriod: " +
                           "day begin {0} identified, " +
                           "adjust limitLocalDateTime to {1} " +
                           "and continue",
                           dayBegin, limitSpecifiedDateTime);
          continue;
        }
        else if (false == item.TimePeriod.IsFullDay ()) {
          // - If there is a defined time period (and the time period is not 0:00-0:00),
          // without a day,
          // the period can't be longer than one day, return false
          log.DebugFormat ("IsItemForLongPeriod: " +
                           "return false because there is a time period");
          return null;
        }
        else if (!item.WeekDays.HasFlag (WeekDay.AllDays)) { // - Not the whole week,
          // return false
          log.DebugFormat ("IsItemForLongPeriod: " +
                           "not the whole week is considered here, return false");
          return null;
        }
        else { // This is an item that is applicable for all times and days
          if (null != itemWithLongPeriod) {
            log.WarnFormat ("IsItemForLongPeriod: " +
                            "applicable item {0} will be overriden by {1}",
                            itemWithLongPeriod, item);
          }
          itemWithLongPeriod = item;
        }
      }
      
      log.DebugFormat ("IsItemForLongPeriod: " +
                       "return {0} with limitLocalDateTime {1}",
                       itemWithLongPeriod,
                       limitSpecifiedDateTime);
      Debug.Assert (!limitSpecifiedDateTime.HasValue || (DateTimeKind.Unspecified != limitSpecifiedDateTime.Value.Kind));
      return itemWithLongPeriod;
    }
    #endregion // MachineStateTemplate

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
