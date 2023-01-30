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
using Lemoine.Business.Config;
using System.Linq;
using System.Threading;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table ShiftSlot
  /// 
  /// Analysis table where are stored all
  /// the shifts of a given machine.
  /// </summary>
  [Serializable]
  public class ShiftSlot : GenericRangeSlot, ISlot, IShiftSlot, IWithTemplate
  {
    /// <summary>
    /// Machine modification priority to use for shift machine association
    /// </summary>
    static readonly string SHIFT_MACHINE_ASSOCIATION_PRIORITY = "Analysis.ShiftSlot.ShiftAssociation.Priority";

    #region Members
    IShiftTemplate m_shiftTemplate;
    IShift m_shift;
    DateTime? m_day;
    ISet<IShiftSlotBreak> m_breaks = new HashSet<IShiftSlotBreak> ();
    bool m_templateProcessed = false;
    TimeSpan? m_effectiveDuration = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (ShiftSlot).FullName);

    #region Constructors and factory methods
    /// <summary>
    /// The default constructor is forbidden
    /// and is only used by NHibernate
    /// </summary>
    protected ShiftSlot ()
      : base (false)
    {
    }

    /// <summary>
    /// Constructor to be used by GenericRangeSlot.Create
    /// </summary>
    /// <param name="range"></param>
    internal ShiftSlot (UtcDateTimeRange range)
      : base (false, range)
    { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="shiftTemplate"></param>
    /// <param name="range"></param>
    public ShiftSlot (IShiftTemplate shiftTemplate, UtcDateTimeRange range)
      : base (false, range)
    {
      m_shiftTemplate = shiftTemplate;
    }
    #endregion // Constructors and factory methods

    #region Getters / Setters
    /// <summary>
    /// Reference to the shift template
    /// </summary>
    // disable once ConvertToAutoProperty
    public virtual IShiftTemplate ShiftTemplate
    {
      get { return m_shiftTemplate; }
      set { m_shiftTemplate = value; }
    }

    /// <summary>
    /// Reference to the Shift
    /// </summary>
    // disable once ConvertToAutoProperty
    public virtual IShift Shift
    {
      get { return m_shift; }
      set { m_shift = value; }
    }

    /// <summary>
    /// Begin date/time of the slot
    /// </summary>
    public override LowerBound<DateTime> BeginDateTime
    {
      get { return base.BeginDateTime; }
      protected set {
        base.BeginDateTime = value;
        m_day = (DateTime?)base.BeginDay;
      }
    }

    /// <summary>
    /// Shift slot day
    /// </summary>
    public virtual DateTime? Day
    {
      get { return m_day; }
      set {
        Debug.Assert (!value.HasValue || value.Equals (value.Value.Date));
        Debug.Assert (!value.HasValue || DateTimeKind.Utc != value.Value.Kind);

        m_day = value;
      }
    }

    /// <summary>
    /// Set of breaks
    /// </summary>
    public virtual ISet<IShiftSlotBreak> Breaks
    {
      get { return m_breaks; }
    }

    /// <summary>
    /// Was the template processed ?
    /// </summary>
    public virtual bool TemplateProcessed
    {
      get { return m_templateProcessed; }
      set { m_templateProcessed = value; }
    }

    /// <summary>
    /// Effective duration of the shift slot considering the breaks
    /// </summary>
    public virtual TimeSpan? EffectiveDuration
    {
      get { return m_effectiveDuration; }
      protected set { m_effectiveDuration = value; }
    }

    /// <summary>
    /// Elapsed effective duration of the shift slot considering the breaks
    /// </summary>
    public virtual TimeSpan? ElapsedEffectiveDuration
    {
      get { return GetElapsedEffectiveDuration (DateTime.UtcNow); }
    }
    #endregion // Getters / Setters

    /// <summary>
    /// Get the elapsed effective duration at the specified UTC date/time
    /// </summary>
    /// <param name="utcNow"></param>
    /// <returns></returns>
    public virtual TimeSpan? GetElapsedEffectiveDuration (DateTime utcNow)
    {
      utcNow = new DateTime (utcNow.Year, utcNow.Month, utcNow.Day, utcNow.Hour, utcNow.Minute, utcNow.Second, DateTimeKind.Utc); // Just keep the seconds
      UtcDateTimeRange nowRange = new UtcDateTimeRange (utcNow, utcNow, "[]");

      // - In the future ?
      if (nowRange.IsStrictlyLeftOf (this.DateTimeRange)) { // In the future => return 0s
        return TimeSpan.FromTicks (0);
      }
      if (this.DateTimeRange.Lower.Equals (utcNow)) {
        return TimeSpan.FromTicks (0);
      }

      // - In the past ?
      if (this.DateTimeRange.IsStrictlyLeftOf (nowRange)) {
        return this.EffectiveDuration;
      }
      if (this.DateTimeRange.Upper.Equals (utcNow)) {
        return this.EffectiveDuration;
      }

      // - Consider the breaks in case of intersection
      if (!this.DateTimeRange.Lower.HasValue) {
        return null;
      }
      Debug.Assert (this.DateTimeRange.ContainsElement (utcNow));
      TimeSpan duration = utcNow.Subtract (this.DateTimeRange.Lower.Value);
      foreach (IShiftSlotBreak b in this.Breaks) {
        Debug.Assert (b.Range.Lower.HasValue);
        Debug.Assert (b.Range.Upper.HasValue);
        if (b.Range.IsStrictlyLeftOf (nowRange)) {
          Debug.Assert (b.Range.Duration.HasValue);
          duration = duration.Subtract (b.Range.Duration.Value);
        }
        else if (b.Range.ContainsElement (utcNow)) { // Intersection
          TimeSpan elapsedBreakTime = utcNow.Subtract (b.Range.Lower.Value);
          duration = duration.Subtract (elapsedBreakTime);
        }
        Debug.Assert (0 <= duration.Ticks);
      }
      return duration;
    }

    /// <summary>
    /// Add a break with the specified range
    /// 
    /// If the range is empty, null is returned
    /// </summary>
    /// <param name="range">range</param>
    /// <returns></returns>
    public virtual IShiftSlotBreak AddBreak (UtcDateTimeRange range)
    {
      if (range.IsEmpty ()) {
        log.WarnFormat ("AddBreak: " +
                        "empty range");
        return null;
      }

      IShiftSlotBreak newBreak = new ShiftSlotBreak (range);
      m_breaks.Add (newBreak);

      Debug.Assert (range.Duration.HasValue);
      if (this.EffectiveDuration.HasValue) {
        this.EffectiveDuration = this.EffectiveDuration.Value
          .Subtract (range.Duration.Value);
      }

      return newBreak;
    }

    #region Slot implementation
    /// <summary>
    /// Set a new date/time range
    /// </summary>
    /// <param name="newRange"></param>
    public override void UpdateDateTimeRange (UtcDateTimeRange newRange)
    {
      if (!this.DateTimeRange.Equals (newRange)) {
        base.UpdateDateTimeRange (newRange);
        if (!newRange.Lower.HasValue) {
          this.Shift = null;
          this.Day = null;
          this.TemplateProcessed = false;
        }
        else { // newRange.Lower.HasValue
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
            IDaySlot daySlot = ModelDAOHelper.DAOFactory.DaySlotDAO
              .FindAt (newRange.Lower.Value, true);
            Debug.Assert (null != daySlot);
            if (daySlot.Day.HasValue) {
              if (daySlot.DateTimeRange.ContainsRange (newRange)) {
                // Ok to just to update the day
                if (!daySlot.Day.Value.Equals (this.Day)) {
                  GetLogger ().DebugFormat ("UpdateDateTimeRange: " +
                                            "update from day {0} to {1}",
                                            this.Day, daySlot.Day);
                  this.Day = daySlot.Day;
                }
              }
              else { // newRange overlaps several day range => reset template processed
                GetLogger ().DebugFormat ("UpdateDateTimeRange: " +
                                          "day slot id={0} does not contain newRange {1} " +
                                          "=> reset TemplateProcessed",
                                          daySlot.Id, newRange);
                this.Shift = null;
                this.Day = null;
                this.TemplateProcessed = false;
              }
            }
            else { // !daySlot.Day.HasValue
              // The day is now known yet => reset TemplateProcessed
              this.Shift = null;
              this.Day = null;
              this.TemplateProcessed = false;
            }
          }
        }
      }
    }

    /// <summary>
    /// IComparable implementation
    /// <see cref="IComparable.CompareTo" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override int CompareTo (object obj)
    {
      if (obj is IShiftSlot other) {
        return this.BeginDateTime.CompareTo (other.BeginDateTime);
      }

      log.ErrorFormat ("CompareTo: " +
                       "object {0} of invalid type",
                       obj);
      throw new ArgumentException ("object is not a ShiftSlot");
    }

    /// <summary>
    /// IComparable implementation
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual int CompareTo (IShiftSlot other)
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
      if (!(obj is IShiftSlot other)) {
        return false;
      }

      return object.Equals (this.Day, other.Day)
        && NHibernateHelper.EqualsNullable (this.ShiftTemplate, other.ShiftTemplate, (a, b) => a.Id == b.Id)
        && NHibernateHelper.EqualsNullable (this.Shift, other.Shift, (a, b) => a.Id == b.Id)
        && object.Equals (this.TemplateProcessed, other.TemplateProcessed);
    }

    /// <summary>
    /// <see cref="Slot.IsEmpty" />
    /// </summary>
    /// <returns></returns>
    public override bool IsEmpty ()
    {
      // No gap is wished
      // Return false even when the shift is null
      return false;
    }

    /// <summary>
    /// Implementation of MachineSlot
    /// <see cref="Slot.Consolidate()" />
    /// </summary>
    protected override void Consolidate ()
    {
      this.EffectiveDuration = this.Duration;

      // Break implementation
      if (this.Day.HasValue && (null != this.Shift) && (null != this.ShiftTemplate)) {
        this.Breaks.Clear ();

        LocalDateTimeRange localRange = this.DateTimeRange.ToLocalTime ();
        Debug.Assert (localRange.Lower.HasValue);
        Debug.Assert (localRange.Upper.HasValue);
        for (DateTime date = localRange.Lower.Value.Date;
             date <= localRange.Upper.Value.Date;
             date = date.AddDays (1).Date) {
          // Loop first on breaks that may match a specific date
          bool specificDate = false;
          foreach (IShiftTemplateBreak templateBreak in this.ShiftTemplate.Breaks) {
            // Day matches ?
            if (templateBreak.Day.HasValue && (templateBreak.Day.Value.Equals (date))) {
              specificDate = true;
              log.DebugFormat ("Consolidate: " +
                               "break {0} matches a specific date {1}",
                               templateBreak, date);
              LocalDateTimeRange localBreakRange =
                localRange.Intersects (date, templateBreak.TimePeriod);
              if (!localBreakRange.IsEmpty ()) {
                UtcDateTimeRange utcBreakRange = localBreakRange.ToUniversalTime ();
                this.AddBreak (utcBreakRange);
              }
            }
          }

          // Loop on breaks that do not consider a specific date
          if (!specificDate) {
            foreach (IShiftTemplateBreak templateBreak in this.ShiftTemplate.Breaks) {
              // DayOfWeek matches ?
              if (!templateBreak.Day.HasValue && templateBreak.WeekDays.HasFlagDayOfWeek (date.DayOfWeek)) {
                log.DebugFormat ("Consolidate: " +
                                 "break {0} matches the day of week",
                                 templateBreak);
                LocalDateTimeRange localBreakRange =
                  localRange.Intersects (date, templateBreak.TimePeriod);
                if (!localBreakRange.IsEmpty ()) {
                  this.AddBreak (localBreakRange.ToUniversalTime ());
                }
              }
            }
          } // Loop on breaks
        } // Loop on dates
      } // if (Day, Shift, ShiftTemplate)
    }

    IEnumerable<IMachine> GetMachines ()
    {
      return ModelDAOHelper.DAOFactory.MachineDAO
        .FindAll (true)
        .Where (m => m.MonitoringType.Id != (int)MachineMonitoringTypeId.Obsolete);
    }

    /// <summary>
    /// Check if propagating the shift to the operation slots is required for a specified machine
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    bool IsShiftMachineAssociationRequired (IMachine machine, UtcDateTimeRange range)
    {
      if (Bound.Compare<DateTime> (range.Lower, DateTime.UtcNow) <= 0) {
        return true;
      }

      // Compare range.Lower with machineStatus.OperationSlotSplitEnd
      IOperationSlotSplit operationSlotSplit = ModelDAOHelper.DAOFactory.OperationSlotSplitDAO
        .FindById (machine.Id);
      if (null == operationSlotSplit) {
        log.WarnFormat ("IsShiftMachineAssociationRequired: " +
                         "operationslotsplit did not exist for machine {0} " +
                         "=> return false",
                         machine.Id);
        return false;
      }
      else {
        return Bound.Compare<DateTime> (range.Lower, operationSlotSplit.End) <= 0;
      }
    }

    /// <summary>
    /// <see cref="Slot.HandleAddedSlot" />
    /// </summary>
    public override void HandleAddedSlot ()
    {
      HandleAddedSlot (this.DateTimeRange);
    }

    /// <summary>
    /// Action to take after the slot is inserted
    /// </summary>
    /// <param name="range"></param>
    void HandleAddedSlot (UtcDateTimeRange range)
    {
      if ((null != this.ShiftTemplate) && this.TemplateProcessed
          && (AnalysisConfigHelper.OperationSlotSplitOption.HasFlag (OperationSlotSplitOption.ByGlobalShift)
              || AnalysisConfigHelper.OperationSlotSplitOption.Equals (OperationSlotSplitOption.ByDay))) {
        Debug.Assert (this.TemplateProcessed);
        Debug.Assert (this.Day.HasValue); // Because template processed
        var priority = Lemoine.Info.ConfigSet
          .LoadAndGet (SHIFT_MACHINE_ASSOCIATION_PRIORITY, AnalysisConfigHelper.AutoModificationPriority);
        // Add the association to all the machines
        foreach (IMachine machine in GetMachines ()) {
          if (IsShiftMachineAssociationRequired (machine, range)) {
            ShiftMachineAssociation association =
              new ShiftMachineAssociation (machine, this.Day.Value, this.Shift, range);
            association.Auto = true;
            association.Priority = priority;
            (new ShiftMachineAssociationDAO ()).MakePersistent (association);
          }
        }
      }
    }

    /// <summary>
    /// <see cref="Slot.HandleRemovedSlot" />
    /// </summary>
    public override void HandleRemovedSlot ()
    {
      HandleRemovedSlot (this.DateTimeRange);
    }

    void HandleRemovedSlot (UtcDateTimeRange range)
    {
      if ((null != this.ShiftTemplate) && this.TemplateProcessed
          && (AnalysisConfigHelper.OperationSlotSplitOption.HasFlag (OperationSlotSplitOption.ByGlobalShift)
              || AnalysisConfigHelper.OperationSlotSplitOption.Equals (OperationSlotSplitOption.ByDay))) {
        Debug.Assert (this.TemplateProcessed);
        Debug.Assert (this.Day.HasValue); // Because template processed
        var priority = Lemoine.Info.ConfigSet
          .LoadAndGet (SHIFT_MACHINE_ASSOCIATION_PRIORITY, AnalysisConfigHelper.AutoModificationPriority);
        // Remove the association at all the machines
        foreach (IMachine machine in GetMachines ()) {
          if (IsShiftMachineAssociationRequired (machine, range)) {
            ShiftMachineAssociation association =
              new ShiftMachineAssociation (machine, this.Day.Value, null, range);
            association.Auto = true;
            association.Priority = priority;
            (new ShiftMachineAssociationDAO ()).MakePersistent (association);
          }
        }
      }
    }

    /// <summary>
    /// <see cref="Slot.HandleModifiedSlot" />
    /// </summary>
    /// <param name="oldSlot"></param>
    public override void HandleModifiedSlot (ISlot oldSlot)
    {
      if ((null != this.ShiftTemplate) && this.TemplateProcessed
          && (AnalysisConfigHelper.OperationSlotSplitOption.HasFlag (OperationSlotSplitOption.ByGlobalShift)
              || AnalysisConfigHelper.OperationSlotSplitOption.Equals (OperationSlotSplitOption.ByDay))) {
        if (oldSlot is ShiftSlot) {
          ShiftSlot oldShiftSlot = oldSlot as ShiftSlot;
          Debug.Assert (null != oldShiftSlot);

          if ((null != oldShiftSlot.ShiftTemplate) && oldShiftSlot.TemplateProcessed) {
            if (object.Equals (oldShiftSlot.Day, this.Day)
                && object.Equals (oldShiftSlot.Shift, this.Shift)) { // only the date range changed
              // this:    xx.....
              // old:         xx......
              // process: xx..
              if (Bound.Compare<DateTime> (this.BeginDateTime, oldSlot.BeginDateTime) < 0) {
                UpperBound<DateTime> end = UpperBound.GetMinimum<DateTime> (this.EndDateTime, oldSlot.BeginDateTime.Value);
                HandleAddedSlot (new UtcDateTimeRange (this.BeginDateTime, end));
              }

              // this:        xxx....
              // old:     xx......
              // process: xx..
              if (Bound.Compare<DateTime> (oldSlot.BeginDateTime, this.BeginDateTime) < 0) {
                Debug.Assert (this.BeginDateTime.HasValue);
                UpperBound<DateTime> end = UpperBound.GetMinimum<DateTime> (oldSlot.EndDateTime,
                                                                            this.BeginDateTime.Value);
                HandleRemovedSlot (new UtcDateTimeRange (oldSlot.BeginDateTime, end));
              }

              // Intersection case
              // this:    xxxx    xx
              // old:      xx    xxxxx
              // process:  xx     xx
              if (this.DateTimeRange.Overlaps (oldSlot.DateTimeRange)) {
                // No change in the intersection
              }

              // this:    ........xxxx
              // old:         xx
              // process:       ..xxxx
              if (NullableDateTime.Compare (oldSlot.EndDateTime, this.EndDateTime) < 0) {
                Debug.Assert (oldSlot.EndDateTime.HasValue);
                LowerBound<DateTime> begin = LowerBound.GetMaximum<DateTime> (this.BeginDateTime, oldSlot.EndDateTime.Value);
                HandleAddedSlot (new UtcDateTimeRange (begin, this.EndDateTime));
              }

              // this:     xx
              // old:     .....xx
              // process:    ..xx
              if (NullableDateTime.Compare (this.EndDateTime, oldSlot.EndDateTime) < 0) {
                Debug.Assert (this.EndDateTime.HasValue);
                LowerBound<DateTime> begin = LowerBound.GetMaximum<DateTime> (oldSlot.BeginDateTime,
                                                                              this.EndDateTime.Value);
                HandleRemovedSlot (new UtcDateTimeRange (begin, oldSlot.EndDateTime));
              }
            }
            else { // The day or the shift changed
              Debug.Assert (oldShiftSlot.Day.HasValue); // Because template processed

              // Remove old
              if (!object.Equals (this.DateTimeRange, oldShiftSlot.DateTimeRange)) {
                var priority = Lemoine.Info.ConfigSet
                  .LoadAndGet (SHIFT_MACHINE_ASSOCIATION_PRIORITY, AnalysisConfigHelper.AutoModificationPriority);
                foreach (IMachine machine in GetMachines ()) {
                  if (IsShiftMachineAssociationRequired (machine, oldShiftSlot.DateTimeRange)) {
                    var removeOld = new ShiftMachineAssociation (machine, oldShiftSlot.Day.Value, null, oldShiftSlot.DateTimeRange);
                    removeOld.Auto = true;
                    removeOld.Priority = priority;
                    (new ShiftMachineAssociationDAO ()).MakePersistent (removeOld);
                  }
                }
              }

              // Add new
              if ((null != this.ShiftTemplate) && this.TemplateProcessed) {
                Debug.Assert (this.Day.HasValue);
                var priority = Lemoine.Info.ConfigSet
                  .LoadAndGet (SHIFT_MACHINE_ASSOCIATION_PRIORITY, AnalysisConfigHelper.AutoModificationPriority);
                foreach (IMachine machine in GetMachines ()) {
                  if (IsShiftMachineAssociationRequired (machine, this.DateTimeRange)) {
                    var addNew = new ShiftMachineAssociation (machine, this.Day.Value, this.Shift, this.DateTimeRange);
                    addNew.Auto = true;
                    addNew.Priority = priority;
                    (new ShiftMachineAssociationDAO ()).MakePersistent (addNew);
                  }
                }
              }
            }
          }
          else { // !oldShiftSlot.TemplateProcessed
            HandleAddedSlot (this.DateTimeRange);
          }
        }
      }
    }
    #endregion // Slot implementation

    #region IWithTemplate implementation
    /// <summary>
    /// Process the template when it has not been processed yet
    /// 
    /// until must be after slot.BeginDateTime
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="range"></param>
    /// <returns>true if completed, else false</returns>
    public virtual bool ProcessTemplate (CancellationToken cancellationToken, UtcDateTimeRange range)
    {
      return ProcessTemplate (cancellationToken, range, null);
    }

    /// <summary>
    /// Process the template when it has not been processed yet
    /// 
    /// until must be after slot.BeginDateTime
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="range"></param>
    /// <param name="checkedThread"></param>
    /// <returns>true if completed, else false</returns>
    public virtual bool ProcessTemplate (CancellationToken cancellationToken, UtcDateTimeRange range, Lemoine.Threading.IChecked checkedThread)
    {
      return ProcessTemplate (cancellationToken, range,
                              null,
                              false,
                              checkedThread,
                              null);
    }

    /// <summary>
    /// Process the template when it has not been processed yet
    /// 
    /// until must be after slot.BeginDateTime
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="applicableRange"></param>
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
      return ProcessTemplate (cancellationToken, applicableRange,
                              mainModification,
                              partOfDetectionAnalysis,
                              checkedThread,
                              maxAnalysisDateTime,
                              0);
    }

    /// <summary>
    /// Process the template when it has not been processed yet
    /// 
    /// until must be after slot.BeginDateTime
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="applicableRange"></param>
    /// <param name="mainModification"></param>
    /// <param name="partOfDetectionAnalysis"></param>
    /// <param name="checkedThread"></param>
    /// <param name="maxAnalysisDateTime">return false if not completed at maxAnalysisDateTime</param>
    /// <param name="attempt">attempt number in case of recursivity</param>
    /// <returns>true if completed, else false</returns>
    bool ProcessTemplate (CancellationToken cancellationToken, UtcDateTimeRange applicableRange,
                          IModification mainModification,
                          bool partOfDetectionAnalysis,
                          Lemoine.Threading.IChecked checkedThread,
                          DateTime? maxAnalysisDateTime,
                          int attempt)
    {
      const int MAX_ATTEMPT = 10;

      if (this.TemplateProcessed) {
        GetLogger ().DebugFormat ("ProcessTemplate: " +
                                  "template is already processed");
        return true;
      }

      GetLogger ().DebugFormat ("ProcessTemplate: " +
                                "attempt {0}",
                                attempt);

      if (MAX_ATTEMPT <= attempt) {
        GetLogger ().ErrorFormat ("ProcessTemplate: " +
                                  "max attempt {0} is reached " +
                                  "=> give up",
                                  attempt);
        return false;
      }

      Debug.Assert (applicableRange.Upper.HasValue);
      Debug.Assert (false == this.TemplateProcessed);

      if (!applicableRange.Overlaps (this.DateTimeRange)) { // Nothing to process, the ranges do not overlap
        GetLogger ().InfoFormat ("ProcessTemplate: " +
                                 "applicationRange {0} does not overlap {1} " +
                                 "=> nothing to process",
                                 applicableRange, this.DateTimeRange);
        return true;
      }

      // Define the applicable process range
      var range = new UtcDateTimeRange (applicableRange.Intersects (this.DateTimeRange));
      Debug.Assert (!range.IsEmpty ()); // Because of the check above

      bool retry;
      if (ProcessTemplate (cancellationToken, range,
                           mainModification,
                           partOfDetectionAnalysis,
                           checkedThread,
                           maxAnalysisDateTime,
                           out retry)) {
        GetLogger ().Debug ("ProcessTemplate: " +
                            "completed");
        return true;
      }
      else if (retry) {
        GetLogger ().Info ("ProcessTemplate: " +
                           "retry once again because some day templates were not processed");
        // Try again on the newly created shifts
        IList<IShiftSlot> shiftSlots;
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("ShiftSlot.ProcessTemplate.ReloadShiftSlots")) {
            shiftSlots = ModelDAOHelper.DAOFactory.ShiftSlotDAO
              .FindOverlapsRange (range);
          }
          foreach (IShiftSlot shiftSlot in shiftSlots) {
            bool result = ((ShiftSlot)shiftSlot).ProcessTemplate (cancellationToken, range,
                                                                  mainModification,
                                                                  partOfDetectionAnalysis, checkedThread, maxAnalysisDateTime, attempt + 1);
            if (!result) {
              GetLogger ().WarnFormat ("ProcessTemplate: " +
                                       "give up because ProcessTemplate on ShiftSlot {0} returned false",
                                       shiftSlot);
              return false;
            }

            if (null != checkedThread) {
              checkedThread.SetActive ();
            }

            // Is it still ok to run it ?
            if (maxAnalysisDateTime.HasValue && (maxAnalysisDateTime.Value <= DateTime.UtcNow)) {
              log.WarnFormat ("ProcessTemplate: " +
                              "maxAnalysisDateTime {0} is reached " +
                              "=> return false",
                              maxAnalysisDateTime);
              return false;
            }
          }
        }

        log.DebugFormat ("ProcessTemplate: " +
                         "all the newly created shift slots were processed");
        return true;
      }
      else {
        GetLogger ().Debug ("ProcessTemplate: " +
                            "do not retry once more because maxAnalysisDateTime was reached");
        return false;
      }
    }

    /// <summary>
    /// Process the template when MachineObservationState is null
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="applicationRange">Upper has a value</param>
    /// <param name="mainModification"></param>
    /// <param name="partOfDetectionAnalysis"></param>
    /// <param name="checkedThread"></param>
    /// <param name="maxAnalysisDateTime">return false if not completed at maxAnalysisDateTime</param>
    /// <param name="retry"></param>
    /// <returns>true if completed, else false</returns>
    bool ProcessTemplate (CancellationToken cancellationToken, UtcDateTimeRange applicationRange,
                          IModification mainModification,
                          bool partOfDetectionAnalysis,
                          Lemoine.Threading.IChecked checkedThread,
                          DateTime? maxAnalysisDateTime,
                          out bool retry)
    {
      retry = false;

      Debug.Assert (applicationRange.Upper.HasValue);
      Debug.Assert (false == this.TemplateProcessed);
      Debug.Assert (this.DateTimeRange.Overlaps (applicationRange));

      if (!applicationRange.Overlaps (this.DateTimeRange)) { // Nothing to process, the ranges do not overlap
        GetLogger ().InfoFormat ("ProcessTemplate: " +
                                 "applicationRange {0} does not overlap {1} " +
                                 "=> nothing to process",
                                 applicationRange, this.DateTimeRange);
        return true;
      }

      // Define the applicable process range
      var range = new UtcDateTimeRange (applicationRange.Intersects (this.DateTimeRange));
      Debug.Assert (!range.IsEmpty ()); // Because of the check above

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginTransaction ("ShiftSlot.ProcessTemplateItems")) {
          // Get the day slots and process them if needed
          IList<IDaySlot> daySlots = ModelDAOHelper.DAOFactory.DaySlotDAO
            .FindOverlapsRange (range);
          foreach (IDaySlot daySlot in daySlots) { // Loop on days
            // Is it still ok to run it ?
            if (maxAnalysisDateTime.HasValue && (maxAnalysisDateTime.Value <= DateTime.UtcNow)) {
              log.WarnFormat ("ProcessTemplate: " +
                              "maxAnalysisDateTime {0} is reached " +
                              "=> return false",
                              maxAnalysisDateTime);
              transaction.Commit (); // Because it is ok to return a commit here
              return false;
            }
            if (null != checkedThread) {
              checkedThread.SetActive ();
            }

            // Check the day slot was processed
            if (!daySlot.Day.HasValue) {
              GetLogger ().InfoFormat ("ProcessTemplate: " +
                                       "daySlot {0} was not processed, do it now",
                                       daySlot);
              if (!((DaySlot)daySlot).ProcessTemplate (cancellationToken, range, mainModification, partOfDetectionAnalysis,
                                                       checkedThread, maxAnalysisDateTime)) {
                GetLogger ().WarnFormat ("ProcessDayTemplates: " +
                                         "process of daySlot {0} was not completed because maxAnalysisDateTime {1} was reached",
                                         daySlot, maxAnalysisDateTime);
              }
              else { // ProcessTemplate ok, retry
                retry = true;
              }
              transaction.Commit (); // Because it is ok to return Commit here
              return false;
            } // Day slot process

            if (null != checkedThread) {
              checkedThread.SetActive ();
            }

            // Is it still ok to run it ?
            if (maxAnalysisDateTime.HasValue && (maxAnalysisDateTime.Value <= DateTime.UtcNow)) {
              log.WarnFormat ("ProcessTemplate: " +
                              "maxAnalysisDateTime {0} is reached " +
                              "=> return false",
                              maxAnalysisDateTime);
              transaction.Commit (); // Because it is ok to return a commit here
              return false;
            }

            // Applied range for this day (intersection)
            UtcDateTimeRange adjustedRange = new UtcDateTimeRange (daySlot.DateTimeRange.Intersects (range));
            if (adjustedRange.IsEmpty ()) {
              Debug.Assert (false);
              log.FatalFormat ("ProcessTemplate: " +
                               "adjustedRange is empty, " +
                               "{0} and {1} do not intersect with each other",
                               daySlot, range);
            }
            Debug.Assert (adjustedRange.Lower.HasValue);
            Debug.Assert (adjustedRange.Upper.HasValue);

            if ((null == this.ShiftTemplate)
                || (null == this.ShiftTemplate.Items)
                || (0 == this.ShiftTemplate.Items.Count)) { // Consider a 24h time period
              ApplyShift (daySlot.Day.Value,
                          (null == this.ShiftTemplate) ? this.Shift : null,
                          adjustedRange,
                          mainModification, partOfDetectionAnalysis, checkedThread);
            }
            else { // null != this.ShiftTemplate
              // Check there is not only a 24h time period
              IShiftTemplateItem fullDayItem = null;
              bool onlyFullDayItem = true;
              foreach (IShiftTemplateItem item in this.ShiftTemplate.Items) {
                if (item.Day.HasValue || !item.TimePeriod.IsFullDay ()) {
                  onlyFullDayItem = false;
                }
                else {
                  if (null != fullDayItem) {
                    GetLogger ().WarnFormat ("ProcessTemplate: " +
                                             "in template {0} several full day items",
                                             this.ShiftTemplate);
                  }
                  fullDayItem = item;
                }
              }

              // Apply the full day item or cancel first any existing slot if such full day item does not exist
              ApplyShift (daySlot.Day.Value,
                          (null == fullDayItem) ? null : fullDayItem.Shift,
                          adjustedRange,
                          mainModification, partOfDetectionAnalysis, checkedThread);
              if (null != checkedThread) {
                checkedThread.SetActive ();
              }

              if (!onlyFullDayItem) {
                // Process then the item one after each other
                // until 'end' only
                LocalDateTimeRange localRange = adjustedRange.ToLocalTime ();
                for (DateTime date = localRange.Lower.Value.Date;
                     date <= localRange.Upper.Value.Date;
                     date = date.AddDays (1).Date) {
                  // Loop on items with a specific date
                  bool specificDate = false;
                  foreach (IShiftTemplateItem item in this.ShiftTemplate.Items) { // Loop on specific date items
                    if (item.Day.HasValue && item.Day.Value.Equals (date)) {
                      specificDate = true;
                      // Do not take into account here item.WeekDays because item.Day is specified,
                      // and normally WeekDays should be AllDays here
                      Debug.Assert (item.WeekDays.HasFlag (WeekDay.AllDays));
                      ApplyShift (daySlot.Day.Value,
                                  date, item.TimePeriod, item.Shift,
                                  localRange,
                                  mainModification, partOfDetectionAnalysis, checkedThread);
                      if (null != checkedThread) {
                        checkedThread.SetActive ();
                      }
                    }
                  } // End loop on specific date items

                  // Loop on items with no specific date
                  if (!specificDate) { // No specific date item match, check the day of week
                    foreach (IShiftTemplateItem item in this.ShiftTemplate.Items) { // Loop on items not considering the specific dates
                      if (!item.Day.HasValue && item.WeekDays.HasFlagDayOfWeek (date.DayOfWeek)) { // Day of week is ok
                        ApplyShift (daySlot.Day.Value,
                                    date, item.TimePeriod, item.Shift,
                                    localRange,
                                    mainModification, partOfDetectionAnalysis, checkedThread);
                      } // If day of week ok
                    } // End loop on items not considering the specific dates
                  } // If no specific date item match
                } // End loop on dates
              } // If !onlyFullDayItem
            } // If null != this.ShiftTemplate

          } // End loop on days
          transaction.Commit ();
        }
      }

      return true;
    }

    void ApplyShift (DateTime day,
                     DateTime date,
                     TimePeriodOfDay timePeriodOfDay,
                     IShift shift,
                     LocalDateTimeRange localGlobalRange,
                     IModification mainModification, bool partOfDetectionAnalysis,
                     Lemoine.Threading.IChecked checkedThread)
    {
      Debug.Assert (DateTimeKind.Local == date.Kind);
      Debug.Assert (date.Equals (date.Date));
      Debug.Assert (DateTimeKind.Utc != day.Kind);
      Debug.Assert (day.Equals (day.Date));
      Debug.Assert (localGlobalRange.Lower.HasValue);
      Debug.Assert (localGlobalRange.Upper.HasValue);

      LocalDateTimeRange localRange = localGlobalRange.Intersects (date, timePeriodOfDay);
      if (localRange.IsEmpty ()) {
        log.DebugFormat ("ApplyForDay: " +
                         "nothing to do because the time period is empty");
        return;
      }

      { // Apply the change
        ShiftChange shiftChange =
          new ShiftChange (day, shift, localRange.ToUniversalTime (), mainModification);
        shiftChange.Caller = checkedThread;
        shiftChange.Apply ();
      }
    }

    /// <summary>
    /// Apply a shift for a 24h timePeriod
    /// </summary>
    /// <param name="day"></param>
    /// <param name="shift"></param>
    /// <param name="range"></param>
    /// <param name="mainModification"></param>
    /// <param name="partOfDetectionAnalysis"></param>
    /// <param name="checkedThread"></param>
    void ApplyShift (DateTime day,
                     IShift shift,
                     UtcDateTimeRange range,
                     IModification mainModification, bool partOfDetectionAnalysis,
                     Lemoine.Threading.IChecked checkedThread)
    {
      Debug.Assert (DateTimeKind.Utc != day.Kind);
      Debug.Assert (day.Equals (day.Date));
      Debug.Assert (range.Lower.HasValue);
      Debug.Assert (range.Upper.HasValue);

      { // Apply the change
        ShiftChange shiftChange =
          new ShiftChange (day, shift, range, mainModification);
        shiftChange.Caller = checkedThread;
        shiftChange.Apply ();
      }
    }
    #endregion // IWithTemplate implementation

    #region ICloneable implementation
    /// <summary>
    /// Make a shallow copy
    /// <see cref="ICloneable.Clone" />
    /// </summary>
    /// <returns></returns>
    public override object Clone ()
    {
      var clone = (ShiftSlot)base.Clone ();
      clone.m_breaks = new HashSet<IShiftSlotBreak> ();
      // Note: the breaks are re-created during the consolidation phase
      return clone;
    }
    #endregion // ICloneable implementation

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[ShiftSlot {this.Id} Range={this.DateTimeRange}]";
      }
      else {
        return $"[ShiftSlot {this.Id}]";
      }
    }
  }
}
