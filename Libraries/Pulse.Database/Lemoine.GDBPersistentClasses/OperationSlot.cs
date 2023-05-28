// Copyright (C) 2009-2023 Lemoine Automation Technologies, 2023 Nicolas Relange
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Business.Config;

namespace Lemoine.GDBPersistentClasses
{
  class ConsolidationRange : ICloneable
  {
    bool m_valid = false;
    UtcDateTimeRange m_range;

    public bool Valid
    {
      get { return m_valid; }
      set { m_valid = value; }
    }

    public UtcDateTimeRange Range
    {
      get { return m_range; }
      set { m_range = value; }
    }

    public object Clone ()
    {
      return MemberwiseClone ();
    }
  }

  /// <summary>
  /// Persistent class of the
  /// analysis table operationslot
  /// that keeps a track of all the Operation Slot periods
  /// </summary>
  [Serializable]
  public class OperationSlot : GenericMachineRangeSlot, IOperationSlot, IComparable<IOperationSlot>
  {
    /// <summary>
    /// Disposable class to call only once before the change AnalysisAccumulator.RemoveCycle
    /// and only once after the change AnalysisAccumulator.AddCycle
    /// </summary>
    internal class ChangeTracker : IDisposable
    {
      readonly OperationSlot m_operationSlot;
      readonly OperationSlot m_previous;
      readonly bool m_active;

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="operationSlot"></param>
      public ChangeTracker (OperationSlot operationSlot)
      {
        Debug.Assert (null != operationSlot);
        if (!operationSlot.m_changeTrackerActive) {
          operationSlot.m_changeTrackerActive = true;
          m_operationSlot = operationSlot;
          if ((0 != operationSlot.Id) && !operationSlot.m_deleted) {
            m_previous = (OperationSlot)operationSlot.CopyForChangeTracker ();
            if (!object.Equals (operationSlot.Shift, m_previous.Shift)) {
              operationSlot.GetLogger ().Fatal ($"ChangeTracker: CopyForChangeTracker did not keep the shift: {operationSlot.Shift} VS {m_previous.Shift}");
            }
          }
          else {
            m_previous = null; // Transient
          }
          m_active = true;
        }
      }

      /// <summary>
      /// Implements IDisposable
      /// </summary>
      public void Dispose ()
      {
        if (m_active) {
          if ((0 != m_operationSlot.Id) && !m_operationSlot.m_deleted) {
            if (m_previous is null) {
              using (var analysisAccumulatorCallerHolder2 = new AnalysisAccumulatorCallerHolder (m_operationSlot)) {
                AnalysisAccumulator.OperationSlotUpdated (null,
                                                          m_operationSlot);
              }
            }
            else { // m_previous is not null
              using (var analysisAccumulatorCallerHolder1 = new AnalysisAccumulatorCallerHolder (m_previous)) {
                using (var analysisAccumulatorCallerHolder2 = new AnalysisAccumulatorCallerHolder (m_operationSlot)) {
                  AnalysisAccumulator.OperationSlotUpdated (m_previous,
                                                            m_operationSlot);
                }
              }
            }
          }
          else if (null != m_previous) {
            Debug.Assert (null != m_operationSlot);
            using (var analysisAccumulatorCallerHolder1 = new AnalysisAccumulatorCallerHolder (m_previous)) {
              using (var analysisAccumulatorCallerHolder2 = new AnalysisAccumulatorCallerHolder (m_operationSlot)) {
                AnalysisAccumulator.OperationSlotRemoved (m_operationSlot,
                                                          m_previous);
              }
            }
          }
          m_operationSlot.m_changeTrackerActive = false;
        }
      }
    }

    static readonly ILog slog = LogManager.GetLogger (typeof (OperationSlot).FullName);

    #region Members
    IOperation m_operation;
    IComponent m_component;
    IWorkOrder m_workOrder;
    ILine m_line;
    ITask m_task;
    IShift m_shift;
    DateTime? m_day;
    TimeSpan? m_runTime = null;
    int m_totalCycles = 0;
    int m_adjustedCycles = 0;
    int m_adjustedQuantity = 0;
    int m_partialCycles = 0;
    TimeSpan? m_averageCycleTime; // in s
    bool? m_autoTask;
    TimeSpan? m_productionDuration = null;

    bool m_activityConsolidated = true;
    [NonSerialized]
    ConsolidationRange m_activityConsolidationRange = new ConsolidationRange ();
    bool m_productionConsolidated = true; // It does not necessary means ProductionDuration.HasValue
    [NonSerialized]
    ConsolidationRange m_productionConsolidationRange = new ConsolidationRange (); // Only valid if ProductionDuration.HasValue;
    bool m_endDateTimeInitialized = false;

    bool m_deleted = false;
    bool m_changeTrackerActive = false;
    #endregion // Members

    #region Constructors
    /// <summary>
    /// The default constructor is forbidden
    /// </summary>
    protected OperationSlot ()
  : base (true)
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    public OperationSlot (IMachine machine,
                          UtcDateTimeRange range)
  : base (true, machine, range)
    {
    }

    /// <summary>
    /// Constructor
    /// 
    /// If shift is not null, then day must be not null as well
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="operation"></param>
    /// <param name="component"></param>
    /// <param name="workOrder"></param>
    /// <param name="line"></param>
    /// <param name="task"></param>
    /// <param name="day"></param>
    /// <param name="shift"></param>
    /// <param name="range"></param>
    internal protected OperationSlot (IMachine machine,
                                      IOperation operation,
                                      IComponent component,
                                      IWorkOrder workOrder,
                                      ILine line,
                                      ITask task,
                                      DateTime? day,
                                      IShift shift,
                                      UtcDateTimeRange range)
  : base (true, machine, range)
    {
      Debug.Assert ((null == shift) || day.HasValue);

      m_operation = operation;
      m_component = component;
      m_workOrder = workOrder;
      m_line = line;
      m_task = task;
      m_day = day;
      m_shift = shift;
      if (Bound.Compare<DateTime> (range.Lower, DateTime.UtcNow) <= 0) {
        // Invalid the consolidation only if the slot is not only in the future
        m_activityConsolidated = false;
        m_activityConsolidationRange.Valid = false;
        m_productionConsolidated = false;
        m_productionConsolidationRange.Valid = false;
      }
      else { // In the future only
        m_activityConsolidated = true;
        m_runTime = TimeSpan.FromSeconds (0);
        m_productionConsolidated = true;
        m_productionDuration = TimeSpan.FromSeconds (0);
      }
    }
    #endregion // Constructors

    #region Getters / Setters
    /// <summary>
    /// Display name that is retrieved with a display function
    /// </summary>
    public virtual string Display
    {
      get; set;
    }

    /// <summary>
    /// Reference to the operation
    /// </summary>
    public virtual IOperation Operation
    {
      get { return m_operation; }
      set {
        if (object.Equals (value, m_operation)) {
          // No change
          return;
        }

        using (ChangeTracker changeTracker = new ChangeTracker (this)) {
          m_operation = value;
        }
      }
    }

    /// <summary>
    /// Reference to the associated component or null if unknown
    /// </summary>
    public virtual IComponent Component
    {
      get { return m_component; }
      set {
        if (object.Equals (m_component, value)) {
          // Nothing to do
          return;
        }

        using (ChangeTracker changeTracker = new ChangeTracker (this)) {
          m_component = value;
        }
      }
    }

    /// <summary>
    /// Reference to the work order if known.
    /// 
    /// null if the work order could not be identified yet.
    /// </summary>
    public virtual IWorkOrder WorkOrder
    {
      get { return m_workOrder; }
      set {
        if (object.Equals (m_workOrder, value)) {
          // Nothing to do
          return;
        }

        using (ChangeTracker changeTracker = new ChangeTracker (this)) {
          m_workOrder = value;
        }
      }
    }

    /// <summary>
    /// Reference to the line if known.
    /// 
    /// null if no line was identified
    /// </summary>
    public virtual ILine Line
    {
      get { return m_line; }
      set {
        if (object.Equals (m_line, value)) {
          // Nothing to do
          return;
        }

        using (ChangeTracker changeTracker = new ChangeTracker (this)) {
          m_line = value;
        }
      }
    }

    /// <summary>
    /// Reference to the task if known.
    /// 
    /// null if no task was identified
    /// </summary>
    public virtual ITask Task
    {
      get { return m_task; }
      set {
        if (object.Equals (m_task, value)) {
          // Nothing to do
          return;
        }

        using (ChangeTracker changeTracker = new ChangeTracker (this)) {
          m_task = value;
        }
      }
    }

    /// <summary>
    /// If the option to split the operation slots by day is set,
    /// reference to the day.
    /// 
    /// null if the option to split the operation slot by day is not set
    /// </summary>
    public virtual DateTime? Day
    {
      get { return m_day; }
      set {
        if (!object.Equals (m_day, value)) {
          using (ChangeTracker changeTracker = new ChangeTracker (this)) {
            m_day = value;
          }
        }
      }
    }

    /// <summary>
    /// If the corresponding option is selected,
    /// reference to the shift.
    /// 
    /// null if there is no shift
    /// or if the option to split the operation slot by shift is not set
    /// </summary>
    public virtual IShift Shift
    {
      get { return m_shift; }
      set {
        if (!object.Equals (m_shift, value)) {
          using (ChangeTracker changeTracker = new ChangeTracker (this)) {
            m_shift = value;
          }
        }
      }
    }

    /// <summary>
    /// Optionally UTC end date/time of the slot
    /// </summary>
    public override UpperBound<DateTime> EndDateTime
    {
      get { return base.EndDateTime; }
      set {
        if (object.Equals (value, base.EndDateTime)) {
          // No change
          m_endDateTimeInitialized = true;
          return;
        }

        if (m_endDateTimeInitialized) {
          // A change of the end date/time invalids (not after initialization)
          // the Runtime and OperationCycle values
          // but only if the slot is not in the future,
          // because there is no activity and no cycle in the future
          if (DateTime.UtcNow <= this.BeginDateTime) {
            GetLogger ().DebugFormat ("EndDateTime.set: " +
                                      "do not invalid the Runtime and OperationCycle values " +
                                      "because the slot is in the future");
          }
          else { // Slot in present / past
                 // If the activity is consolidated, keep the consolidated range, to avoid too much process in the future
            if (m_activityConsolidated && AnalysisConfigHelper.OperationSlotRunTime) {
              Debug.Assert (this.RunTime.HasValue); // activityConsolidated => RunTime.HasValue
              GetLogger ().DebugFormat ("EndDateTime.set: " +
                                        "consider the activity consolidation period {0} runTime={1} for {2}, " +
                                        "before updating the end date/time to {3}",
                                        this.DateTimeRange, this.RunTime, this,
                                        value);
              m_activityConsolidationRange.Range = this.DateTimeRange;
              m_activityConsolidationRange.Valid = true;
              m_activityConsolidated = false;
            }

            // If the production is consolidated, keep the consolidated range, to avoid too much process in the future
            if (m_productionConsolidated && this.ProductionDuration.HasValue && AnalysisConfigHelper.OperationSlotProductionDuration) {
              GetLogger ().DebugFormat ("EndDateTime.set: " +
                                        "consider the production consolidation period {0} productionDuration={1} for {2}, " +
                                        "before updating the end date/time to {3}",
                                        this.DateTimeRange, this.ProductionDuration, this,
                                        value);
              m_productionConsolidationRange.Range = this.DateTimeRange;
              m_productionConsolidationRange.Valid = true; // Ok because this.ProductionDuration.HasValue
              m_productionConsolidated = false;
            }
          }
        }
        else {
          m_endDateTimeInitialized = true;
        }

        using (ChangeTracker changeTracker = new ChangeTracker (this)) {
          base.EndDateTime = value;
        }
      }
    }

    /// <summary>
    /// Run time in seconds of this operation slot
    /// </summary>
    public virtual TimeSpan? RunTime
    {
      get { return m_runTime; }
      set {
        Debug.Assert (!value.HasValue || (0 <= value.Value.Ticks));
        if (value.HasValue && (value.Value.Ticks < 0)) {
          Debug.Assert (false);
          GetLogger ().FatalFormat ("RunTime.set: " +
                                    "New run time {0} can't be negative on operation slot id {1} range {2}",
                                    value.Value, this.Id, this.DateTimeRange);
          GetLogger ().FatalFormat ("RunTime.set: " +
                                    "fallback: set 0 for the run time instead of {0} on operation slot id {1} range {2}",
                                    value.Value, this.Id, this.DateTimeRange);
          m_runTime = TimeSpan.FromTicks (0);
          return;
        }
        m_runTime = value;
      }
    }

    /// <summary>
    /// Number of run full cycles (from begin to end) during the slot
    /// 
    /// Use the setter very carefully because it does not update the number of intermediate work pieces
    /// Use the IncrementTotalCycles and SetTotalCycles methods instead
    /// </summary>
    public virtual int TotalCycles
    {
      get { return m_totalCycles; }
      set {
        if (object.Equals (m_totalCycles, value)) {
          // Nothing to do
          return;
        }

        using (ChangeTracker changeTracker = new ChangeTracker (this)) {
          m_totalCycles = value;
        }
      }
    }

    /// <summary>
    /// Number of full cycles for which there is an adjusted number of intermediate work pieces.
    /// 
    /// Use the setter very carefully because it does not update the number of intermediate work pieces
    /// Use the IncrementTotalCycles and SetTotalCycles methods instead
    /// </summary>
    public virtual int AdjustedCycles
    {
      get { return m_adjustedCycles; }
      set {
        if (object.Equals (m_adjustedCycles, value)) {
          // Nothing to do
          return;
        }

        using (ChangeTracker changeTracker = new ChangeTracker (this)) {
          m_adjustedCycles = value;
        }
      }
    }

    /// <summary>
    /// Adjusted quantity of intermediate work pieces
    /// 
    /// Use the setter very carefully because it does not update the number of intermediate work pieces
    /// Use the IncrementTotalCycles and SetTotalCycles methods instead
    /// </summary>
    public virtual int AdjustedQuantity
    {
      get { return m_adjustedQuantity; }
      set {
        if (object.Equals (m_adjustedQuantity, value)) {
          // Nothing to do
          return;
        }

        using (ChangeTracker changeTracker = new ChangeTracker (this)) {
          m_adjustedQuantity = value;
        }
      }
    }

    /// <summary>
    /// Number of partial cycles (a begin but no end) during the slot
    /// </summary>
    public virtual int PartialCycles
    {
      get { return m_partialCycles; }
      set {
        if (object.Equals (m_partialCycles, value)) {
          // Nothing to do
          return;
        }

        using (ChangeTracker changeTracker = new ChangeTracker (this)) {
          m_partialCycles = value;
        }
      }
    }

    /// <summary>
    /// Average cycle time in seconds of the full cycles during the slot
    /// </summary>
    // disable once ConvertToAutoProperty
    public virtual TimeSpan? AverageCycleTime
    {
      get { return m_averageCycleTime; }
      set { m_averageCycleTime = value; }
    }

    /// <summary>
    /// The task was automatically determined
    /// </summary>
    public virtual bool? AutoTask
    {
      get { return m_autoTask; }
      set { m_autoTask = value; }
    }

    /// <summary>
    /// Production duration
    /// </summary>
    public virtual TimeSpan? ProductionDuration
    {
      get { return m_productionDuration; }
      set { m_productionDuration = value; }
    }

    /// <summary>
    /// <see cref="Slot.Consolidated" />
    /// </summary>
    public override bool Consolidated
    {
      get { return m_activityConsolidated && m_productionConsolidated; }
      set {
        GetLogger ().DebugFormat ("Consolidated.set: " +
                                  "set {0} to Consolidated for {1}",
                                  value, this);
        m_activityConsolidated = value;
        m_productionConsolidated = value;
      }
    }
    #endregion // Getters / Setters

    #region ICheckedCaller
    /// <summary>
    /// <see cref="Lemoine.Threading.ICheckedCaller"/>
    /// </summary>
    /// <param name="caller"></param>
    public virtual void SetCheckedCaller (Lemoine.Threading.IChecked caller)
    {
      this.Caller = caller;
    }
    #endregion ICheckedCaller

    #region Methods
    /// <summary>
    /// IDisplay implementation
    /// </summary>
    /// <param name="variant"></param>
    /// <returns></returns>
    public virtual string GetDisplay (string variant)
    {
      if (string.IsNullOrEmpty (variant)) {
        return this.Display;
      }
      else {
        GetLogger ().FatalFormat ("GetDisplay: not implemented for variant {0}",
                                  variant);
        throw new NotImplementedException ("OperationSlot.GetDisplay with variant " + variant);
      }
    }

    /// <summary>
    /// Set the operation cycle as persistent
    /// </summary>
    internal protected virtual void SetPersistent ()
    {
      m_deleted = false;
    }

    /// <summary>
    /// Set the operation cycle as transient
    /// </summary>
    internal protected virtual void SetTransient ()
    {
      m_deleted = true;
    }

    /// <summary>
    /// Make a shallow copy, but with unconsolidated operation cycles
    /// 
    /// Reset the cycles there because the association cycle => slot is lost
    /// <see cref="ICloneable.Clone" />
    /// </summary>
    /// <returns></returns>
    public override object Clone ()
    {
      var clone = base.Clone () as OperationSlot;

      // But keep for performance reason the consolidation values on the run time and production time
      clone.m_activityConsolidationRange = (ConsolidationRange)this.m_activityConsolidationRange.Clone ();
      clone.m_productionConsolidationRange = (ConsolidationRange)this.m_productionConsolidationRange.Clone ();

      clone.m_totalCycles = 0;
      clone.m_partialCycles = 0;
      clone.m_averageCycleTime = null;
      clone.m_adjustedCycles = 0;
      clone.m_adjustedQuantity = 0;
      return clone;
    }

    /// <summary>
    /// Make a copy for the ChangeTracker
    /// </summary>
    /// <returns></returns>
    object CopyForChangeTracker ()
    {
      var copy = base.Clone () as OperationSlot;
      Debug.Assert (copy.m_totalCycles == this.m_totalCycles);
      return copy;
    }

    /// <summary>
    /// <see cref="Slot.UpdateDateTimeRange" />
    /// </summary>
    /// <param name="newRange">not empty</param>
    public override void UpdateDateTimeRange (UtcDateTimeRange newRange)
    {
      Debug.Assert (!newRange.IsEmpty ()); // For the moment, this case is not coded

      // A change of the begin date/time does not invalid
      // the Runtime and OperationCycle values
      // if the slot is in the future,
      // because there is no activity and no cycle in the future
      if ((Bound.Compare<DateTime> (DateTime.UtcNow, newRange.Lower) <= 0)
          && (Bound.Compare<DateTime> (DateTime.UtcNow, this.DateTimeRange.Lower) <= 0)) {
        GetLogger ().DebugFormat ("UpdateDateTimeRange: " +
                                  "do not invalid the Runtime and OperationCycle values " +
                                  "because the slot is in the future");
      }
      else { // Slot in present / past
        if (!newRange.Equals (this.DateTimeRange)) { // New range
                                                     // If the operation cycles or activity or production are consolidated, keep the consolidated range, to avoid too much process in the future
          if (m_activityConsolidated && AnalysisConfigHelper.OperationSlotRunTime) {
            Debug.Assert (this.RunTime.HasValue); // m_activityConsolidated => this.RunTime.HasValue
            GetLogger ().DebugFormat ("UpdateDateTimeRange: " +
                                      "consider the activity consolidation period {0} runTime={1} for {2}, " +
                                      "before updating the date/time range to {3}",
                                      this.DateTimeRange, this.RunTime, this,
                                      newRange);
            m_activityConsolidationRange.Range = this.DateTimeRange;
            m_activityConsolidationRange.Valid = true;
            m_activityConsolidated = false;
          }
          if (m_productionConsolidated && this.ProductionDuration.HasValue && AnalysisConfigHelper.OperationSlotProductionDuration) {
            GetLogger ().DebugFormat ("UpdateDateTimeRange: " +
                                      "consider the production consolidation period {0} productionDuration={1} for {2}, " +
                                      "before updating the date/time range to {3}",
                                      this.DateTimeRange, this.ProductionDuration, this,
                                      newRange);
            m_productionConsolidationRange.Range = this.DateTimeRange;
            m_productionConsolidationRange.Valid = true; // Ok because this.ProductionDuration.HasValue
            m_productionConsolidated = false;
          }
        } // End if new begin date/time
      } // End Future VS present / past

      // Apply in all the cases the parent method
      using (ChangeTracker changeTracker = new ChangeTracker (this)) {
        base.UpdateDateTimeRange (newRange);
      }

      Debug.Assert (this.DateTimeRange.Equals (newRange));
    }

    /// <summary>
    /// IComparable implementation
    /// <see cref="IComparable.CompareTo" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override int CompareTo (object obj)
    {
      if (obj is OperationSlot) {
        var other = (IOperationSlot)obj;
        if (other.Machine.Equals (this.Machine)) {
          return this.BeginDateTime.CompareTo (other.BeginDateTime);
        }
        else {
          GetLogger ().ErrorFormat ("CompareTo: " +
                                    "trying to compare operation slots " +
                                    "for different machines {0} {1}",
                                    this, other);
          throw new ArgumentException ("Comparison of OperationSlots from different machines");
        }
      }

      GetLogger ().ErrorFormat ("CompareTo: " +
                                "object {0} of invalid type",
                                obj);
      throw new ArgumentException ("object is not an OperationSlot");
    }

    /// <summary>
    /// IComparable implementation
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual int CompareTo (IOperationSlot other)
    {
      if (other.Machine.Equals (this.Machine)) {
        return this.BeginDateTime.CompareTo (other.BeginDateTime);
      }

      GetLogger ().ErrorFormat ("CompareTo: " +
                                "trying to compare operation slots " +
                                "for different machines {0} {1}",
                                this, other);
      throw new ArgumentException ("Comparison of OperationSlots from different machines");
    }

    /// <summary>
    /// Slot implementation
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool ReferenceDataEquals (ISlot obj)
    {
      var other = obj as IOperationSlot;
      if (other == null) {
        return false;
      }

      return object.Equals (this.Operation, other.Operation)
        && object.Equals (this.Component, other.Component)
        && object.Equals (this.WorkOrder, other.WorkOrder)
        && object.Equals (this.Line, other.Line)
        && object.Equals (this.Task, other.Task)
        && object.Equals (this.Machine, other.Machine)
        && object.Equals (this.Day, other.Day)
        && object.Equals (this.Shift, other.Shift);
    }

    /// <summary>
    /// <see cref="Slot.IsEmpty" />
    /// </summary>
    /// <returns></returns>
    public override bool IsEmpty ()
    {
      bool result = ((null == this.WorkOrder)
                     && (null == this.Component)
                     && (null == this.Operation)
                     && (null == this.Line)
                     && (null == this.Task))
        || this.DateTimeRange.IsEmpty ();
      GetLogger ().DebugFormat ("IsEmpty: " +
                                "return {0} for {1}",
                                result, this);
      return result;
    }

    /// <summary>
    /// Implementation of the IMachineSlot interface
    /// 
    /// This updates:
    /// <item>the Runtime property from the Fact table</item>
    /// <item>the Production duration</item>
    /// <see cref="Slot.Consolidate()" />
    /// </summary>
    protected override void Consolidate ()
    {
      GetLogger ().Debug ("Consolidate");
      SetActive ();

      using (var analysisAccumulatorCallerHolder = new AnalysisAccumulatorCallerHolder (this)) {

        if (false == m_activityConsolidated) {
          GetLogger ().Debug ("Consolidate: " +
                              "consolidate the activity properties");
          ConsolidateRunTime ();
        }

        if (false == m_productionConsolidated) {
          GetLogger ().Debug ("Consolidate: " +
                              "consolidate the production properties");
          ConsolidateProduction ();
        }
      }

      GetLogger ().Debug ("Consolidate: " +
                          "done");
      SetActive ();
    }

    /// <summary>
    /// <see cref="Slot.HandleAddedSlot" />
    /// </summary>
    public override void HandleAddedSlot ()
    {
      // There is nothing to do for this slot
      // Everything is managed by Consolidate
    }

    /// <summary>
    /// <see cref="Slot.HandleRemovedSlot" />
    /// </summary>
    public override void HandleRemovedSlot ()
    {
      // Remove the cycles from the summary
      if ((0 == this.TotalCycles) && (0 == this.PartialCycles)) {
        Debug.Assert (0 == this.AdjustedCycles);
        Debug.Assert (0 == this.AdjustedQuantity);
        GetLogger ().DebugFormat ("HandleRemovedSlot: " +
                                  "no cycle in the removed slot");
      }
      else { // 0 != this.TotalCycles
        GetLogger ().DebugFormat ("HandleRemovedSlot: " +
                                  "remove {0} existing cycles and {1} partial cycles",
                                  this.TotalCycles, this.PartialCycles);
        IList<IOperationCycle> cycles = ModelDAOHelper.DAOFactory.OperationCycleDAO.FindAllWithOperationSlot (this);
        foreach (IOperationCycle cycle in cycles) {
          cycle.OperationSlot = null; // For the moment it looks to be sufficient
        }
      }
    }

    /// <summary>
    /// <see cref="Slot.HandleModifiedSlot" />
    /// </summary>
    /// <param name="oldSlot"></param>
    public override void HandleModifiedSlot (ISlot oldSlot)
    {
      // There is nothing to do for this slot
      // because already is already managed by Consolidate, especially the cycles in summaries
    }

    /// <summary>
    /// Consolidate the run time
    /// </summary>
    public virtual void ConsolidateRunTime ()
    {
      if (!AnalysisConfigHelper.OperationSlotRunTime) { // Option is not active, do nothing
        GetLogger ().DebugFormat ("ConsolidateRunTime: " +
                                  "return immediately because the option is not active");
        return;
      }

      TimeSpan runTime;

      // Shortcut for slots in the future
      if (Bound.Compare<DateTime> (DateTime.UtcNow, this.BeginDateTime) <= 0) {
        GetLogger ().DebugFormat ("ConsolidateRunTime: " +
                                  "slot in the future " +
                                  "=> no possible run time, set it to 0");
        runTime = TimeSpan.FromTicks (0);
      }
      else {
        IMonitoredMachine monitoredMachine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (this.Machine.Id);
        if (null == monitoredMachine) {
          GetLogger ().InfoFormat ("ConsolidateRunTime: " +
                                   "machine {0} is not a monitored one " +
                                   "=> skip ConsolidateRunTime",
                                   this.Machine.Id);
          return;
        }

        if (!m_activityConsolidationRange.Valid
            || !this.DateTimeRange.Overlaps (m_activityConsolidationRange.Range)
            || !this.RunTime.HasValue) { // No intersection or the option has been activated since => recompute the runTime completely
          GetLogger ().Debug ("ConsolidateRunTime: " +
                              "re-compute completely the run time");
          runTime = GetRunTimeBetween (monitoredMachine, this.BeginDateTime, this.EndDateTime);
        }
        else { // Intersection...
          runTime = this.RunTime.Value;
          GetLogger ().DebugFormat ("ConsolidateRunTime: " +
                                    "intersection with activityConsolidationRange {0}, " +
                                    "vs OperationSlot {1} {2}",
                                    m_activityConsolidationRange.Range,
                                    this, this.DateTimeRange);
          // Begin
          Debug.Assert (m_activityConsolidationRange.Range.Lower.HasValue);
          if (m_activityConsolidationRange.Range.Lower.HasValue
              && (Bound.Compare<DateTime> (this.BeginDateTime, m_activityConsolidationRange.Range.Lower.Value) < 0)) { // Add a period
            runTime += GetRunTimeBetween (monitoredMachine, this.BeginDateTime, m_activityConsolidationRange.Range.Lower.Value);
          }
          else if (m_activityConsolidationRange.Range.Lower.HasValue
                   && (Bound.Compare<DateTime> (m_activityConsolidationRange.Range.Lower.Value, this.BeginDateTime) < 0)) { // Remove a period
            Debug.Assert (this.BeginDateTime.HasValue);
            runTime -= GetRunTimeBetween (monitoredMachine, m_activityConsolidationRange.Range.Lower.Value, this.BeginDateTime.Value);
          }
          else { // Begin unchanged
            Debug.Assert (this.BeginDateTime.Equals (m_activityConsolidationRange.Range.Lower.Value));
          }
          // End
          if (NullableDateTime.Compare (m_activityConsolidationRange.Range.Upper, this.EndDateTime) < 0) { // Add a period
            Debug.Assert (m_activityConsolidationRange.Range.Upper.HasValue);
            runTime += GetRunTimeBetween (monitoredMachine, m_activityConsolidationRange.Range.Upper.Value, this.EndDateTime);
          }
          else if (NullableDateTime.Compare (this.EndDateTime, m_activityConsolidationRange.Range.Upper) < 0) { // Remove a period
            Debug.Assert (this.EndDateTime.HasValue);
            runTime -= GetRunTimeBetween (monitoredMachine, this.EndDateTime.Value, m_activityConsolidationRange.Range.Upper);
          }
          else { // End unchanged
            Debug.Assert (Bound.Equals<DateTime> (this.EndDateTime, m_activityConsolidationRange.Range.Upper));
          }
        }

        if (runTime.Ticks < 0) {
          GetLogger ().Fatal ($"ConsolidateRunTime: re-compute completely the run time because it is {runTime} (negative) for operationslot id={this.Id} range={this.DateTimeRange}");
          runTime = GetRunTimeBetween (monitoredMachine, this.BeginDateTime, this.EndDateTime);
          GetLogger ().Fatal ($"ConsolidateRunTime: the re-computed run time is {runTime} for range {this.DateTimeRange}");
        }
      }

      if (!object.Equals (RunTime, runTime)) {
        GetLogger ().DebugFormat ("ConsolidateRunTime: " +
                                  "update run time from {0} to {1} " +
                                  "for operation slot {2}",
                                  this.RunTime, runTime,
                                  this);
        RunTime = runTime;
      }

      // Consolidation is done !
      Debug.Assert (this.RunTime.HasValue);
      m_activityConsolidated = true;
      m_activityConsolidationRange.Valid = false;
    }

    /// <summary>
    /// Get the run time in the specified range
    /// </summary>
    /// <param name="monitoredMachine"></param>
    /// <param name="lower"></param>
    /// <param name="upper"></param>
    /// <returns></returns>
    TimeSpan GetRunTimeBetween (IMonitoredMachine monitoredMachine,
                                LowerBound<DateTime> lower,
                                UpperBound<DateTime> upper)
    {
      return GetRunTimeBetween (monitoredMachine, new UtcDateTimeRange (lower, upper));
    }

    /// <summary>
    /// Get the run time in the specified range
    /// </summary>
    /// <param name="monitoredMachine"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    TimeSpan GetRunTimeBetween (IMonitoredMachine monitoredMachine,
                                UtcDateTimeRange range)
    {
      TimeSpan runTime = TimeSpan.FromTicks (0);
      SetActive ();

      if (Bound.Compare<DateTime> (DateTime.UtcNow, range.Lower) <= 0) { // Shortcut for periods in the future
        GetLogger ().DebugFormat ("GetRunTimeBetween: " +
                                  "period {0} in the future " +
                                  "=> return 0",
                                  range);
      }
      else {
        // Use the ReasonSlots here, so that the data here is synchronized with the activities analysis
        IList<IReasonSlot> reasonSlots =
          ModelDAOHelper.DAOFactory.ReasonSlotDAO
          .FindOverlapsRange (monitoredMachine,
                              range);
        SetActive ();
        LowerBound<DateTime> lastBegin = DateTime.MinValue;
        foreach (IReasonSlot reasonSlot in reasonSlots) {
          if (!reasonSlot.Running) {
            continue;
          }
          if (reasonSlot.DateTimeRange.IsEmpty ()) {
            continue;
          }
          if (Bound.Equals<DateTime> (lastBegin, reasonSlot.BeginDateTime)) {
            continue; // To avoid duplicated facts (although there should not be any)
          }
          lastBegin = reasonSlot.BeginDateTime;
          UtcDateTimeRange intersection = new UtcDateTimeRange (reasonSlot.DateTimeRange.Intersects (range));
          Debug.Assert (!intersection.IsEmpty ()); // Because slots in range
          Debug.Assert (intersection.Lower.HasValue);
          Debug.Assert (intersection.Upper.HasValue);
          Debug.Assert (intersection.Duration.HasValue);
          runTime += intersection.Duration.Value;
        }
      }

      GetLogger ().DebugFormat ("GetRunTimeBetween: " +
                                "run time in period {0} is {1}",
                                range, runTime);
      return runTime;
    }

    /// <summary>
    /// Consolidate the production duration
    /// </summary>
    public virtual void ConsolidateProduction ()
    {
      if (!AnalysisConfigHelper.OperationSlotProductionDuration) { // Option is not active, do nothing
        GetLogger ().DebugFormat ("ConsolidateProduction: " +
                                  "return immediately because the option is not active");
        return;
      }

      TimeSpan productionDuration;

      // Shortcut for slots in the future
      if (Bound.Compare<DateTime> (DateTime.UtcNow, this.BeginDateTime) <= 0) {
        GetLogger ().DebugFormat ("ConsolidateProduction: " +
                                  "slot in the future " +
                                  "=> no production yet set it to 0");
        productionDuration = TimeSpan.FromTicks (0);
      }
      else {
        // Restrict the range to process to (,ProductionAnalysisDateTime)
        IProductionAnalysisStatus productionAnalysisStatus = ModelDAOHelper.DAOFactory.ProductionAnalysisStatusDAO
          .FindById (this.Machine.Id);
        SetActive ();
        if (null == productionAnalysisStatus) {
          GetLogger ().ErrorFormat ("ConsolidateProduction: " +
                                    "the row in productionanalysisstatus has not been intialized yet " +
                                    "=> return");
          return;
        }
        if (Bound.Compare<DateTime> (productionAnalysisStatus.AnalysisDateTime, this.BeginDateTime) <= 0) {
          GetLogger ().DebugFormat ("ConsolidateProduction: " +
                                    "slot after the analysis date/time " +
                                    "=> no production yet, set it to 0");
          productionDuration = TimeSpan.FromTicks (0);
        }
        else {
          UtcDateTimeRange range = new UtcDateTimeRange (new UtcDateTimeRange (new LowerBound<DateTime> (null),
                                                                               productionAnalysisStatus.AnalysisDateTime)
                                                         .Intersects (this.DateTimeRange));
          Debug.Assert (!range.IsEmpty ());
          Debug.Assert (Bound.Compare<DateTime> (range.Upper, productionAnalysisStatus.AnalysisDateTime) <= 0);

          if (!m_productionConsolidationRange.Valid
              || !range.ContainsRange (m_productionConsolidationRange.Range)) { // Production in cache does not contain the new range => recompute the productionDuration completely
            GetLogger ().Debug ("ConsolidateProduction: " +
                                "re-compute completely the production time");
            productionDuration = GetProductionDurationInRange (this.Machine, range);
          }
          else if (!this.ProductionDuration.HasValue) {
            Debug.Assert (this.ProductionDuration.HasValue);
            GetLogger ().FatalFormat ("ConsolidateProduction: " +
                                      "ConsolidationRange ok but productionDuration is null");
            productionDuration = GetProductionDurationInRange (this.Machine, range);
          }
          else { // The new range is an extension of the production in cache
            Debug.Assert (this.ProductionDuration.HasValue); // m_productionConsolidationRange.Valid => ProductionDuration.HasValue
            productionDuration = this.ProductionDuration.Value;
            GetLogger ().DebugFormat ("ConsolidateProduction: " +
                                      "intersection with productionConsolidationRange {0}, " +
                                      "vs OperationSlot {1} {2}",
                                      m_productionConsolidationRange.Range,
                                      this, range);
            // Begin
            Debug.Assert (Bound.Compare<DateTime> (range.Lower, m_productionConsolidationRange.Range.Lower) <= 0);
            Debug.Assert (m_productionConsolidationRange.Range.Lower.HasValue);
            productionDuration = productionDuration
              .Add (GetProductionDurationInRange (this.Machine,
                                                  new UtcDateTimeRange (range.Lower,
                                                                        m_productionConsolidationRange.Range.Lower.Value)));
            // End
            Debug.Assert (m_productionConsolidationRange.Range.Upper.HasValue);
            Debug.Assert (Bound.Compare<DateTime> (m_productionConsolidationRange.Range.Upper, range.Upper) <= 0);
            productionDuration = productionDuration
              .Add (GetProductionDurationInRange (this.Machine,
                                                  new UtcDateTimeRange (m_productionConsolidationRange.Range.Upper.Value,
                                                                        range.Upper)));
          }
        }
      }

      if (!this.ProductionDuration.HasValue
          || !object.Equals (this.ProductionDuration.Value, productionDuration)) {
        GetLogger ().DebugFormat ("ConsolidateProduction: " +
                                  "update production duration from {0} to {1} " +
                                  "for operation slot {2}",
                                  this.ProductionDuration, productionDuration,
                                  this);
        this.ProductionDuration = productionDuration;
      }

      // Consolidation is done !
      Debug.Assert (this.ProductionDuration.HasValue);
      m_productionConsolidated = true;
      m_productionConsolidationRange.Valid = false;
    }

    /// <summary>
    /// Get the production duration in the specified range
    /// 
    /// range is normally before productionanalysisdatetime, so before now
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    TimeSpan GetProductionDurationInRange (IMachine machine,
                                           UtcDateTimeRange range)
    {
      Debug.Assert (range.Upper.HasValue);
      Debug.Assert (range.Upper.Value <= DateTime.UtcNow);

      TimeSpan productionDuration = TimeSpan.FromTicks (0);
      SetActive ();

      if (range.IsEmpty ()) {
        return productionDuration;
      }

      if (Bound.Compare<DateTime> (DateTime.UtcNow, range.Lower) <= 0) { // Shortcut for periods in the future
        GetLogger ().DebugFormat ("GetProductionDurationInRange: " +
                                  "period {0} in the future " +
                                  "=> return 0",
                                  range);
      }
      else {
        IList<IObservationStateSlot> observationStateSlots =
          ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
          .FindOverlapsRange (machine, range);
        SetActive ();
        LowerBound<DateTime> lastBegin = DateTime.MinValue;
        foreach (IObservationStateSlot observationStateSlot in observationStateSlots) {
          SetActive ();
          if (!observationStateSlot.Production.HasValue || !observationStateSlot.Production.Value) {
            continue;
          }
          if (observationStateSlot.DateTimeRange.IsEmpty ()) {
            continue;
          }
          if (Bound.Equals<DateTime> (lastBegin, observationStateSlot.BeginDateTime)) {
            continue; // To avoid duplicated slots (although there should not be any)
          }
          lastBegin = observationStateSlot.BeginDateTime;
          UtcDateTimeRange intersection = new UtcDateTimeRange (observationStateSlot.DateTimeRange.Intersects (range));
          Debug.Assert (!intersection.IsEmpty ()); // Because slots in range
          Debug.Assert (intersection.Lower.HasValue);
          Debug.Assert (intersection.Upper.HasValue);
          Debug.Assert (intersection.Duration.HasValue);
          productionDuration += intersection.Duration.Value;
        }
      }

      GetLogger ().DebugFormat ("GetProductionDurationInRange: " +
                                "production duration in period {0} is {1}",
                                range, productionDuration);
      return productionDuration;
    }

    /// <summary>
    /// Update the average cycle time of the operation slot
    /// </summary>
    public virtual void UpdateAverageCycleTime ()
    {
      if (this.TotalCycles <= 1) {
        GetLogger ().InfoFormat ("UpdateAverageCycleTime: " +
                                 "the number of total cycles {0} " +
                                 "is too low to get an average cycle time",
                                 this.TotalCycles);
        this.AverageCycleTime = null;
      }
      else {
        TimeSpan? periodDuration;

        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          periodDuration = ModelDAOHelper.DAOFactory.OperationCycleDAO
            .GetSlotPeriodDuration (this);
        }

        if (periodDuration.HasValue) {
          UpdateAverageCycleTime (periodDuration.Value);
        }
        else {
          GetLogger ().ErrorFormat ("UpdateAverageCycleTime: " +
                                    "the slot period could not be determined, " +
                                    "which should not occur because the number of total cycles is positive");
          this.AverageCycleTime = null;
        }
      }
    }

    /// <summary>
    /// Update the average cycle time
    /// thanks to the slot period (minimum and maximum cycle end) duration
    /// of this operation slot
    /// </summary>
    /// <param name="periodDuration"></param>
    void UpdateAverageCycleTime (TimeSpan periodDuration)
    {
      if (this.TotalCycles <= 1) {
        GetLogger ().InfoFormat ("UpdateAverageCycleTime: " +
                                 "the number of total cycles {0} " +
                                 "is too low to get an average cycle time",
                                 this.TotalCycles);
        this.AverageCycleTime = null;
      }
      else {
        Debug.Assert (0 <= periodDuration.Ticks);
        Debug.Assert (1 < this.TotalCycles);
        this.AverageCycleTime =
          TimeSpan.FromTicks (periodDuration
                              .Ticks
                              / (this.TotalCycles - 1));
        GetLogger ().DebugFormat ("UpdateAverageCycleTime: " +
                                  "for duration={0} " +
                                  "totalCycles={1} " +
                                  "=> average cycle time={2}",
                                  periodDuration,
                                  this.TotalCycles,
                                  this.AverageCycleTime);
      }
    }

    /// <summary>
    /// Merge the next slot with this when:
    /// <item>it comes right after this</item>
    /// <item>the reference data are the same</item>
    /// </summary>
    /// <param name="nextSlot"></param>
    public override void Merge (ISlot nextSlot)
    {
      Debug.Assert (null != nextSlot);
      Debug.Assert (nextSlot.ReferenceDataEquals (this));
      Debug.Assert (this.EndDateTime.HasValue);
      Debug.Assert (Bound.Equals<DateTime> (this.EndDateTime, nextSlot.BeginDateTime));

      var nextOperationSlot = nextSlot as OperationSlot;
      Debug.Assert (null != nextOperationSlot);

      GetLogger ().DebugFormat ("Merge: " +
                                "merge nextSlot={0} into {1}",
                                nextSlot, this);

      // Try to keep the consolidated values
      var activityRange = new ConsolidationRange ();
      TimeSpan? runTime = null;
      var productionRange = new ConsolidationRange ();
      TimeSpan? productionDuration = null;

      if (AnalysisConfigHelper.OperationSlotRunTime) { // - Activity
        if (this.m_activityConsolidated) {
          Debug.Assert (this.RunTime.HasValue); // m_activityConsolidated => RunTime.HasValue
          activityRange.Range = this.DateTimeRange;
          activityRange.Valid = true;
          runTime = this.RunTime;
        }
        else if (this.m_activityConsolidationRange.Valid) {
          Debug.Assert (this.RunTime.HasValue); // m_activityConsolidationRange.Value => RunTime.HasValue
          activityRange = this.m_activityConsolidationRange;
          runTime = this.RunTime;
        }
        if (nextOperationSlot.m_activityConsolidated) {
          Debug.Assert (nextOperationSlot.RunTime.HasValue); // m_activityConsolidated => RunTime.HasValue
          if (!activityRange.Valid) {
            Debug.Assert (!runTime.HasValue);
            activityRange.Range = nextOperationSlot.DateTimeRange;
            activityRange.Valid = true;
            runTime = nextOperationSlot.RunTime;
          }
          else if (Bound.Equals<DateTime> (activityRange.Range.Upper, nextOperationSlot.BeginDateTime)) { // && activityRange.Valid
            Debug.Assert (runTime.HasValue);
            activityRange.Range = new UtcDateTimeRange (activityRange.Range.Lower, nextOperationSlot.EndDateTime);
            runTime = runTime.Value.Add (nextOperationSlot.RunTime.Value);
          }
        }
        else if (nextOperationSlot.m_activityConsolidationRange.Valid) {
          Debug.Assert (nextOperationSlot.RunTime.HasValue); // m_activityConsolidationRange.Valid => RunTime.HasValue
          if (!activityRange.Valid) {
            Debug.Assert (!runTime.HasValue);
            activityRange = nextOperationSlot.m_activityConsolidationRange;
            runTime = nextOperationSlot.RunTime;
          }
          else if (activityRange.Range.Upper.Equals (nextOperationSlot.m_activityConsolidationRange.Range.Lower)) { // && activityRange.Valid
            Debug.Assert (runTime.HasValue);
            activityRange.Range = new UtcDateTimeRange (activityRange.Range.Lower, nextOperationSlot.m_activityConsolidationRange.Range.Upper);
            runTime = runTime.Value.Add (nextOperationSlot.RunTime.Value);
          }
        }
      }
      if (AnalysisConfigHelper.OperationSlotProductionDuration) { // - Production
        if (this.m_productionConsolidated && this.ProductionDuration.HasValue) {
          productionRange.Range = this.DateTimeRange;
          productionRange.Valid = true;
          productionDuration = this.ProductionDuration;
        }
        else if (this.m_productionConsolidationRange.Valid) {
          Debug.Assert (this.ProductionDuration.HasValue); // m_productionConsolidationRange.Value => ProductionDuration.HasValue
          productionRange = this.m_productionConsolidationRange;
          productionDuration = this.ProductionDuration;
        }
        if (nextOperationSlot.m_productionConsolidated && nextOperationSlot.ProductionDuration.HasValue) {
          if (!productionRange.Valid) {
            Debug.Assert (!productionDuration.HasValue);
            productionRange.Range = nextOperationSlot.DateTimeRange;
            productionRange.Valid = true;
            productionDuration = nextOperationSlot.ProductionDuration;
          }
          else if (Bound.Equals<DateTime> (productionRange.Range.Upper, nextOperationSlot.BeginDateTime)) { // && productionRange.Valid
            Debug.Assert (productionDuration.HasValue);
            productionRange.Range = new UtcDateTimeRange (productionRange.Range.Lower, nextOperationSlot.EndDateTime);
            productionDuration = productionDuration.Value.Add (nextOperationSlot.ProductionDuration.Value);
          }
        }
        else if (nextOperationSlot.m_productionConsolidationRange.Valid) {
          Debug.Assert (nextOperationSlot.ProductionDuration.HasValue); // m_productionConsolidationRange.Valid => ProductionDuration.HasValue
          if (!productionRange.Valid) {
            Debug.Assert (!productionDuration.HasValue);
            productionRange = nextOperationSlot.m_productionConsolidationRange;
            productionDuration = nextOperationSlot.ProductionDuration;
          }
          else if (Bound.Equals<DateTime> (productionRange.Range.Upper, nextOperationSlot.m_productionConsolidationRange.Range.Lower)) { // && productionRange.Valid
            Debug.Assert (productionDuration.HasValue);
            productionRange.Range = new UtcDateTimeRange (productionRange.Range.Lower, nextOperationSlot.m_productionConsolidationRange.Range.Upper);
            productionDuration = productionDuration.Value.Add (nextOperationSlot.ProductionDuration.Value);
          }
        }
      }

      { // Adjust the number of cycles
        m_adjustedCycles += nextOperationSlot.AdjustedCycles;
        m_adjustedQuantity += nextOperationSlot.AdjustedQuantity;
        // Note: the number of partial cycles and of full cycles is managed by the accumulators
      }

      this.EndDateTime = nextSlot.EndDateTime;

      if (AnalysisConfigHelper.OperationSlotRunTime) {
        m_activityConsolidated = false;
        m_activityConsolidationRange.Valid = false;
        if (activityRange.Valid) {
          Debug.Assert (runTime.HasValue);
          this.RunTime = runTime;
          Debug.Assert (activityRange.Range.Lower.HasValue);
          if (activityRange.Range
              .Equals (this.DateTimeRange)) { // Whole period
            m_activityConsolidated = true;
            m_activityConsolidationRange.Valid = false;
          }
          else {
            m_activityConsolidationRange = activityRange;
          }
        }
      }
      if (AnalysisConfigHelper.OperationSlotProductionDuration) {
        m_productionConsolidated = false;
        m_productionConsolidationRange.Valid = false;
        if (productionRange.Valid) {
          Debug.Assert (productionDuration.HasValue);
          this.ProductionDuration = productionDuration;
          Debug.Assert (productionRange.Range.Lower.HasValue);
          if (productionRange.Range
              .Equals (this.DateTimeRange)) { // Whole period
            m_productionConsolidated = true;
            m_productionConsolidationRange.Valid = false;
          }
          else {
            m_productionConsolidationRange = productionRange;
          }
        }
      }
    }
    #endregion // Methods

    /// <summary>
    /// <see cref="Object.ToString" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[OperationSlot {this.Id} {this.Machine?.ToStringIfInitialized ()} Range={this.DateTimeRange}]";
      }
      else {
        return $"[OperationSlot {this.Id}]";
      }
    }

    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals (IOperationSlot other)
    {
      return this.Equals ((object)other);
    }

    /// <summary>
    ///   Determines whether the specified Object
    ///   is equal to the current Object
    /// </summary>
    /// <param name="obj">The object to compare with the current object</param>
    /// <returns>true if the specified Object is equal to the current Object; otherwise, false</returns>
    public override bool Equals (object obj)
    {
      if (object.ReferenceEquals (this, obj)) {
        return true;
      }

      if (obj == null) {
        return false;
      }
      // Note: do not use here this.GetType () != obj.GetType
      //       because a Xxx may be compared with a XxxProxy
      //       which may return false although true might be returned
      IOperationSlot other = obj as OperationSlot;
      if (null == other) {
        return false;
      }
      if (this.Id != 0) {
        return (other.Id == this.Id);
      }
      return false;
    }

    /// <summary>
    ///   Serves as a hash function for a particular type
    /// </summary>
    /// <returns>A hash code for the current Object</returns>
    public override int GetHashCode ()
    {
      if (0 != Id) {
        int hashCode = 0;
        unchecked {
          hashCode += 1000000007 * Id.GetHashCode ();
        }
        return hashCode;
      }
      else {
        return base.GetHashCode ();
      }
    }

  }
}
