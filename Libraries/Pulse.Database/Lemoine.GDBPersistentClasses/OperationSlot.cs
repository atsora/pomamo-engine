// Copyright (C) 2009-2023 Lemoine Automation Technologies, 2023 Nicolas Relange
// Copyright (C) 2025 Atsora Solutions
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

    IOperation m_operation;
    IComponent m_component;
    IWorkOrder m_workOrder;
    ILine m_line;
    IManufacturingOrder m_manufacturingOrder;
    IShift m_shift;
    DateTime? m_day;
    TimeSpan? m_runTime = null;
    int m_totalCycles = 0;
    int m_adjustedCycles = 0;
    int m_adjustedQuantity = 0;
    int m_partialCycles = 0;
    TimeSpan? m_averageCycleTime; // in s
    bool? m_autoManufacturingOrder;
    TimeSpan? m_productionDuration = null;
    int m_reportedCycles = 0;
    int m_reportedParts = 0;
    int m_validParts = 0;
    int m_setupParts = 0;
    int m_scrapParts = 0;
    int m_fixableParts = 0;

    /// <summary>
    /// Are the scrap counters up to date ?
    ///
    /// They must be recomputed from the scrap reports after a new slot is created,
    /// or after its range or its operation changed.
    /// Not when the slot is only extended to the future, since there is no scrap report there yet
    /// </summary>
    bool m_scrapConsolidated = true;
    /// <summary>
    /// If valid, range for which the scrap counters were consolidated (with the same operation),
    /// so that only the changed periods are considered by the next consolidation
    /// </summary>
    [NonSerialized]
    ConsolidationRange m_scrapConsolidationRange = new ConsolidationRange ();
    bool m_activityConsolidated = true;
    [NonSerialized]
    ConsolidationRange m_activityConsolidationRange = new ConsolidationRange ();
    bool m_productionConsolidated = true; // It does not necessary means ProductionDuration.HasValue
    [NonSerialized]
    ConsolidationRange m_productionConsolidationRange = new ConsolidationRange (); // Only valid if ProductionDuration.HasValue;
    bool m_endDateTimeInitialized = false;

    bool m_deleted = false;
    bool m_changeTrackerActive = false;

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
      m_scrapConsolidated = IsInTheFuture (range);
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
    /// <param name="manufacturingOrder"></param>
    /// <param name="day"></param>
    /// <param name="shift"></param>
    /// <param name="range"></param>
    internal protected OperationSlot (IMachine machine,
                                      IOperation operation,
                                      IComponent component,
                                      IWorkOrder workOrder,
                                      ILine line,
                                      IManufacturingOrder manufacturingOrder,
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
      m_manufacturingOrder = manufacturingOrder;
      m_day = day;
      m_shift = shift;
      m_scrapConsolidated = IsInTheFuture (range);
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
        // The scrap reports of the slot must match its operation: full recompute
        InvalidateScrapFully ();
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
    /// Reference to the manufacturing order if known.
    /// 
    /// null if no manufacturing order was identified
    /// </summary>
    public virtual IManufacturingOrder ManufacturingOrder
    {
      get { return m_manufacturingOrder; }
      set {
        if (object.Equals (m_manufacturingOrder, value)) {
          // Nothing to do
          return;
        }

        using (ChangeTracker changeTracker = new ChangeTracker (this)) {
          m_manufacturingOrder = value;
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
            if (GetLogger ().IsDebugEnabled) {
              GetLogger ().Debug ("EndDateTime.set: do not invalid the Runtime and OperationCycle values because the slot is in the future");
            }
          }
          else { // Slot in present / past
                 // If the activity is consolidated, keep the consolidated range, to avoid too much process in the future
            if (m_activityConsolidated && AnalysisConfigHelper.OperationSlotRunTime) {
              Debug.Assert (this.RunTime.HasValue); // activityConsolidated => RunTime.HasValue
              if (GetLogger ().IsDebugEnabled) {
                GetLogger ().Debug ($"EndDateTime.set: consider the activity consolidation period {this.DateTimeRange} runTime={this.RunTime} for {this}, before updating the end date/time to {value}");
              }
              m_activityConsolidationRange.Range = this.DateTimeRange;
              m_activityConsolidationRange.Valid = true;
              m_activityConsolidated = false;
            }

            // If the production is consolidated, keep the consolidated range, to avoid too much process in the future
            if (m_productionConsolidated && this.ProductionDuration.HasValue && AnalysisConfigHelper.OperationSlotProductionDuration) {
              if (GetLogger ().IsDebugEnabled) {
                GetLogger ().Debug ($"EndDateTime.set: consider the production consolidation period {this.DateTimeRange} productionDuration={this.ProductionDuration} for {this}, before updating the end date/time to {value}");
              }
              m_productionConsolidationRange.Range = this.DateTimeRange;
              m_productionConsolidationRange.Valid = true; // Ok because this.ProductionDuration.HasValue
              m_productionConsolidated = false;
            }
          }
        }
        else {
          m_endDateTimeInitialized = true;
        }

        if (Bound.Compare<DateTime> (value, base.EndDateTime) < 0) {
          // Some scrap reports may not be included in the slot any more.
          // Note: an extension of the end is ignored, since there is no scrap report there yet
          InvalidateScrapForNewRange ();
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
          GetLogger ().Fatal ($"RunTime.set: New run time {value.Value} can't be negative on operation slot id {this.Id} range {this.DateTimeRange}");
          GetLogger ().Fatal ($"RunTime.set: fallback: set 0 for the run time instead of {value.Value} on operation slot id {this.Id} range {this.DateTimeRange}");
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
    /// The manufacturing order was automatically determined
    /// </summary>
    public virtual bool? AutoManufacturingOrder
    {
      get { return m_autoManufacturingOrder; }
      set { m_autoManufacturingOrder = value; }
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
    /// <see cref="IOperationSlot"/>
    /// </summary>
    public virtual int ReportedCycles => m_reportedCycles;

    /// <summary>
    /// <see cref="IOperationSlot"/>
    /// </summary>
    public virtual int ReportedParts => m_reportedParts;

    /// <summary>
    /// <see cref="IOperationSlot"/>
    /// </summary>
    public virtual int ValidParts => m_validParts;

    /// <summary>
    /// <see cref="IOperationSlot"/>
    /// </summary>
    public virtual int SetupParts => m_setupParts;

    /// <summary>
    /// <see cref="IOperationSlot"/>
    /// </summary>
    public virtual int ScrapParts => m_scrapParts;

    /// <summary>
    /// <see cref="IOperationSlot"/>
    /// </summary>
    public virtual int FixableParts => m_fixableParts;

    /// <summary>
    /// <see cref="Slot.Consolidated" />
    ///
    /// Note: the setter does not change the consolidation state of the scrap counters,
    /// which depends only on the changes of range and operation
    /// </summary>
    public override bool Consolidated
    {
      get { return m_activityConsolidated && m_productionConsolidated && m_scrapConsolidated; }
      set {
        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().Debug ($"Consolidated.set: set {value} to Consolidated for {this}");
        }
        m_activityConsolidated = value;
        m_productionConsolidated = value;
      }
    }

    /// <summary>
    /// Is a range in the future only ? There is no scrap report there yet
    /// </summary>
    static bool IsInTheFuture (UtcDateTimeRange range) =>
      Bound.Compare<DateTime> (DateTime.UtcNow, range.Lower) <= 0;

    /// <summary>
    /// Is the full range of a scrap report included in the range of this slot ?
    ///
    /// A scrap report applies to a single operation slot
    /// </summary>
    /// <param name="scrapReport">not null</param>
    /// <returns></returns>
    internal protected virtual bool IsScrapReportIn (IScrapReport scrapReport)
    {
      Debug.Assert (null != scrapReport);

      return this.DateTimeRange.ContainsRange (scrapReport.DateTimeRange);
    }

    /// <summary>
    /// Does a scrap report apply to this slot: is it included in the slot with the same operation ?
    /// </summary>
    /// <param name="scrapReport">not null</param>
    /// <returns></returns>
    internal protected virtual bool IsScrapReportApplicable (IScrapReport scrapReport) =>
      IsScrapReportIn (scrapReport) && object.Equals (scrapReport.Operation, this.Operation);

    /// <summary>
    /// Add (multiplicator = +1) or remove (multiplicator = -1) the counters of a scrap report
    /// </summary>
    /// <param name="scrapReport">not null</param>
    /// <param name="multiplicator">+1 or -1</param>
    internal protected virtual void AddScrapReport (IScrapReport scrapReport, int multiplicator = +1)
    {
      Debug.Assert (null != scrapReport);
      Debug.Assert ((1 == multiplicator) || (-1 == multiplicator));

      m_reportedCycles += multiplicator * scrapReport.NbCycles;
      m_reportedParts += multiplicator * scrapReport.NbParts;
      m_validParts += multiplicator * scrapReport.NbValid;
      m_setupParts += multiplicator * scrapReport.NbSetup;
      m_scrapParts += multiplicator * scrapReport.NbScrap;
      m_fixableParts += multiplicator * scrapReport.NbFixable;
      if (GetLogger ().IsDebugEnabled) {
        GetLogger ().Debug ($"AddScrapReport: scrap report {scrapReport.Id} multiplicator={multiplicator} => reported cycles={m_reportedCycles} parts={m_reportedParts} valid={m_validParts} setup={m_setupParts} scrap={m_scrapParts} fixable={m_fixableParts}");
      }
    }

    void ResetScrapCounters ()
    {
      m_reportedCycles = 0;
      m_reportedParts = 0;
      m_validParts = 0;
      m_setupParts = 0;
      m_scrapParts = 0;
      m_fixableParts = 0;
    }

    /// <summary>
    /// The range of the slot is about to change: keep the range the scrap counters were consolidated for,
    /// so that only the changed periods are considered by the next consolidation
    ///
    /// To call before the range is updated
    /// </summary>
    void InvalidateScrapForNewRange ()
    {
      if (m_scrapConsolidated) {
        m_scrapConsolidationRange = new ConsolidationRange { Range = this.DateTimeRange, Valid = true };
        m_scrapConsolidated = false;
      }
      // else keep the current consolidation range, valid or not
    }

    /// <summary>
    /// The scrap counters must be fully recomputed, for example after a change of operation
    /// </summary>
    void InvalidateScrapFully ()
    {
      m_scrapConsolidated = false;
      m_scrapConsolidationRange = new ConsolidationRange ();
    }

    /// <summary>
    /// Consolidate the scrap counters if needed
    /// </summary>
    internal protected virtual void EnsureScrapConsolidated ()
    {
      if (!m_scrapConsolidated) {
        ConsolidateScrap ();
      }
    }

    /// <summary>
    /// Consolidate the scrap counters
    ///
    /// The scrap counters are the sum of the completed scrap reports (analysis status Done) that are included in the slot,
    /// that are not updated by another completed report and whose operation is the operation of the slot.
    /// A scrap report that is not completed yet is added by its own analysis (<see cref="ScrapReport.MakeAnalysis"/>).
    /// A scrap report that overlaps the slot but that is not included in it (it overlaps several operation slots),
    /// or whose operation is another one, does not apply: it is discarded (with an analysis log, once).
    ///
    /// If the range was only changed (same operation), only the reports in the changed periods are considered:
    /// a report is counted in the new range if it is included in it, and it was counted in the previous range if it was included in it.
    /// Else all the reports of the slot are considered.
    /// Nothing is written in the scrap reports, so this can be run several times
    /// </summary>
    public virtual void ConsolidateScrap ()
    {
      if (IsInTheFuture (this.DateTimeRange)) {
        ResetScrapCounters ();
      }
      else if (m_scrapConsolidationRange.Valid && m_scrapConsolidationRange.Range.Overlaps (this.DateTimeRange)) {
        ConsolidateScrapChangedPeriods (m_scrapConsolidationRange.Range);
      }
      else {
        ResetScrapCounters ();
        var scrapReports = ModelDAOHelper.DAOFactory.ScrapReportDAO
          .FindCompletedNotUpdatedOverlapping (this.Machine, this.DateTimeRange);
        SetActive ();
        foreach (var scrapReport in scrapReports) {
          if (IsScrapReportApplicable (scrapReport)) {
            AddScrapReport (scrapReport);
          }
          else {
            LogScrapReportDiscard (scrapReport);
          }
        }
      }
      m_scrapConsolidated = true;
      m_scrapConsolidationRange = new ConsolidationRange ();
    }

    /// <summary>
    /// Consolidate the scrap counters considering only the periods that changed since the previous range
    /// </summary>
    /// <param name="previousRange">range the scrap counters were consolidated for, with the same operation, that overlaps the current range</param>
    void ConsolidateScrapChangedPeriods (UtcDateTimeRange previousRange)
    {
      var changedPeriods = new List<UtcDateTimeRange> ();
      if (!Bound.Equals<DateTime> (previousRange.Lower, this.DateTimeRange.Lower)) {
        var (min, max) = (Bound.Compare<DateTime> (previousRange.Lower, this.DateTimeRange.Lower) < 0)
          ? (previousRange.Lower, this.DateTimeRange.Lower)
          : (this.DateTimeRange.Lower, previousRange.Lower);
        changedPeriods.Add (new UtcDateTimeRange (min, new UpperBound<DateTime> (max.HasValue ? max.Value : (DateTime?)null)));
      }
      if (!Bound.Equals<DateTime> (previousRange.Upper, this.DateTimeRange.Upper)) {
        var (min, max) = (Bound.Compare<DateTime> (previousRange.Upper, this.DateTimeRange.Upper) < 0)
          ? (previousRange.Upper, this.DateTimeRange.Upper)
          : (this.DateTimeRange.Upper, previousRange.Upper);
        changedPeriods.Add (new UtcDateTimeRange (new LowerBound<DateTime> (min.HasValue ? min.Value : (DateTime?)null), max));
      }

      var scrapReports = changedPeriods
        .Where (p => !p.IsEmpty ())
        .SelectMany (p => ModelDAOHelper.DAOFactory.ScrapReportDAO.FindCompletedNotUpdatedOverlapping (this.Machine, p))
        .GroupBy (r => ((Lemoine.Collections.IDataWithId<long>)r).Id)
        .Select (g => g.First ());
      SetActive ();
      foreach (var scrapReport in scrapReports) {
        var sameOperation = object.Equals (scrapReport.Operation, this.Operation);
        var inPrevious = sameOperation && previousRange.ContainsRange (scrapReport.DateTimeRange);
        var inNew = sameOperation && IsScrapReportIn (scrapReport);
        if (inNew && !inPrevious) {
          AddScrapReport (scrapReport, +1);
        }
        else if (inPrevious && !inNew) {
          AddScrapReport (scrapReport, -1);
        }
        if (!inNew && this.DateTimeRange.Overlaps (scrapReport.DateTimeRange)) {
          LogScrapReportDiscard (scrapReport);
        }
      }
      if (GetLogger ().IsDebugEnabled) {
        GetLogger ().Debug ($"ConsolidateScrapChangedPeriods: previous range {previousRange} => {this.DateTimeRange}, {changedPeriods.Count} changed periods");
      }
    }

    /// <summary>
    /// A scrap report overlaps the slot but does not apply to it: log it (once)
    /// </summary>
    /// <param name="scrapReport">not null</param>
    void LogScrapReportDiscard (IScrapReport scrapReport)
    {
      var discardReason = IsScrapReportIn (scrapReport)
        ? $"the operation of the period is now {((Lemoine.Collections.IDataWithId)this.Operation)?.Id.ToString () ?? "none"} instead of {((Lemoine.Collections.IDataWithId)scrapReport.Operation)?.Id}"
        : $"its range {scrapReport.DateTimeRange} is not included in a single operation slot, for example {this.DateTimeRange}";
      if (scrapReport is ScrapReport persistentScrapReport) {
        persistentScrapReport.LogDiscard (LogLevel.WARN, discardReason);
      }
      else {
        GetLogger ().Fatal ($"LogScrapReportDiscard: unexpected type of scrap report {scrapReport.GetType ()}");
      }
    }

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
        GetLogger ().Fatal ($"GetDisplay: not implemented for variant {variant}");
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

      // Keep the scrap counters with the range they were consolidated for,
      // so that only the changed periods are considered once the range of the clone is set
      if (this.m_scrapConsolidated) {
        clone.m_scrapConsolidationRange = new ConsolidationRange { Range = this.DateTimeRange, Valid = true };
      }
      else if (this.m_scrapConsolidationRange.Valid) {
        clone.m_scrapConsolidationRange = (ConsolidationRange)this.m_scrapConsolidationRange.Clone ();
      }
      else {
        clone.ResetScrapCounters ();
        clone.m_scrapConsolidationRange = new ConsolidationRange ();
      }
      clone.m_scrapConsolidated = false;
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
        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().Debug ("UpdateDateTimeRange: do not invalid the Runtime and OperationCycle values because the slot is in the future");
        }
      }
      else { // Slot in present / past
        if (!newRange.Equals (this.DateTimeRange)) { // New range
                                                     // If the operation cycles or activity or production are consolidated, keep the consolidated range, to avoid too much process in the future
          if (m_activityConsolidated && AnalysisConfigHelper.OperationSlotRunTime) {
            Debug.Assert (this.RunTime.HasValue); // m_activityConsolidated => this.RunTime.HasValue
            if (GetLogger ().IsDebugEnabled) {
              GetLogger ().Debug ($"UpdateDateTimeRange: consider the activity consolidation period {this.DateTimeRange} runTime={this.RunTime} for {this}, before updating the date/time range to {newRange}");
            }
            m_activityConsolidationRange.Range = this.DateTimeRange;
            m_activityConsolidationRange.Valid = true;
            m_activityConsolidated = false;
          }
          if (m_productionConsolidated && this.ProductionDuration.HasValue && AnalysisConfigHelper.OperationSlotProductionDuration) {
            if (GetLogger ().IsDebugEnabled) {
              GetLogger ().Debug ($"UpdateDateTimeRange: consider the production consolidation period {this.DateTimeRange} productionDuration={this.ProductionDuration} for {this}, before updating the date/time range to {newRange}");
            }
            m_productionConsolidationRange.Range = this.DateTimeRange;
            m_productionConsolidationRange.Valid = true; // Ok because this.ProductionDuration.HasValue
            m_productionConsolidated = false;
          }
        } // End if new begin date/time
      } // End Future VS present / past

      // Recompute the scrap counters, except if the slot is only extended to the future,
      // because there is no scrap report there yet
      if (!Bound.Equals<DateTime> (newRange.Lower, this.DateTimeRange.Lower)
          || (Bound.Compare<DateTime> (newRange.Upper, this.DateTimeRange.Upper) < 0)) {
        InvalidateScrapForNewRange ();
      }

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
          GetLogger ().Error ($"CompareTo: trying to compare operation slots for different machines {this} {other}");
          throw new ArgumentException ("Comparison of OperationSlots from different machines");
        }
      }

      GetLogger ().Error ($"CompareTo: object {obj} of invalid type");
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

      GetLogger ().Error ($"CompareTo: trying to compare operation slots for different machines {this} {other}");
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
        && object.Equals (this.ManufacturingOrder, other.ManufacturingOrder)
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
                     && (null == this.ManufacturingOrder))
        || this.DateTimeRange.IsEmpty ();
      if (GetLogger ().IsDebugEnabled) {
        GetLogger ().Debug ($"IsEmpty: return {result} for {this}");
      }
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

        if (!m_scrapConsolidated) {
          GetLogger ().Debug ("Consolidate: consolidate the scrap counters");
          ConsolidateScrap ();
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
        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().Debug ("HandleRemovedSlot: no cycle in the removed slot");
        }
      }
      else { // 0 != this.TotalCycles
        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().Debug ($"HandleRemovedSlot: remove {this.TotalCycles} existing cycles and {this.PartialCycles} partial cycles");
        }
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
        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().Debug ("ConsolidateRunTime: return immediately because the option is not active");
        }
        return;
      }

      TimeSpan runTime;

      // Shortcut for slots in the future
      if (Bound.Compare<DateTime> (DateTime.UtcNow, this.BeginDateTime) <= 0) {
        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().Debug ("ConsolidateRunTime: slot in the future => no possible run time, set it to 0");
        }
        runTime = TimeSpan.FromTicks (0);
      }
      else {
        IMonitoredMachine monitoredMachine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (this.Machine.Id);
        if (null == monitoredMachine) {
          GetLogger ().Info ($"ConsolidateRunTime: machine {this.Machine.Id} is not a monitored one => skip ConsolidateRunTime");
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
          if (GetLogger ().IsDebugEnabled) {
            GetLogger ().Debug ($"ConsolidateRunTime: intersection with activityConsolidationRange {m_activityConsolidationRange.Range}, vs OperationSlot {this} {this.DateTimeRange}");
          }
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
        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().Debug ($"ConsolidateRunTime: update run time from {this.RunTime} to {runTime} for operation slot {this}");
        }
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
        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().Debug ($"GetRunTimeBetween: period {range} in the future => return 0");
        }
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

      if (GetLogger ().IsDebugEnabled) {
        GetLogger ().Debug ($"GetRunTimeBetween: run time in period {range} is {runTime}");
      }
      return runTime;
    }

    /// <summary>
    /// Consolidate the production duration
    /// </summary>
    public virtual void ConsolidateProduction ()
    {
      if (!AnalysisConfigHelper.OperationSlotProductionDuration) { // Option is not active, do nothing
        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().Debug ("ConsolidateProduction: return immediately because the option is not active");
        }
        return;
      }

      TimeSpan productionDuration;

      // Shortcut for slots in the future
      if (Bound.Compare<DateTime> (DateTime.UtcNow, this.BeginDateTime) <= 0) {
        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().Debug ("ConsolidateProduction: slot in the future => no production yet set it to 0");
        }
        productionDuration = TimeSpan.FromTicks (0);
      }
      else {
        // Restrict the range to process to (,ProductionAnalysisDateTime)
        IProductionAnalysisStatus productionAnalysisStatus = ModelDAOHelper.DAOFactory.ProductionAnalysisStatusDAO
          .FindById (this.Machine.Id);
        SetActive ();
        if (null == productionAnalysisStatus) {
          GetLogger ().Error ("ConsolidateProduction: the row in productionanalysisstatus has not been intialized yet => return");
          return;
        }
        if (Bound.Compare<DateTime> (productionAnalysisStatus.AnalysisDateTime, this.BeginDateTime) <= 0) {
          if (GetLogger ().IsDebugEnabled) {
            GetLogger ().Debug ("ConsolidateProduction: slot after the analysis date/time => no production yet, set it to 0");
          }
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
            GetLogger ().Fatal ("ConsolidateProduction: ConsolidationRange ok but productionDuration is null");
            productionDuration = GetProductionDurationInRange (this.Machine, range);
          }
          else { // The new range is an extension of the production in cache
            Debug.Assert (this.ProductionDuration.HasValue); // m_productionConsolidationRange.Valid => ProductionDuration.HasValue
            productionDuration = this.ProductionDuration.Value;
            if (GetLogger ().IsDebugEnabled) {
              GetLogger ().Debug ($"ConsolidateProduction: intersection with productionConsolidationRange {m_productionConsolidationRange.Range}, vs OperationSlot {this} {range}");
            }
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
        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().Debug ($"ConsolidateProduction: update production duration from {this.ProductionDuration} to {productionDuration} for operation slot {this}");
        }
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
        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().Debug ($"GetProductionDurationInRange: period {range} in the future => return 0");
        }
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

      if (GetLogger ().IsDebugEnabled) {
        GetLogger ().Debug ($"GetProductionDurationInRange: production duration in period {range} is {productionDuration}");
      }
      return productionDuration;
    }

    /// <summary>
    /// Update the average cycle time of the operation slot
    /// </summary>
    public virtual void UpdateAverageCycleTime ()
    {
      if (this.TotalCycles <= 1) {
        GetLogger ().Info ($"UpdateAverageCycleTime: the number of total cycles {this.TotalCycles} is too low to get an average cycle time");
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
          GetLogger ().Error ("UpdateAverageCycleTime: the slot period could not be determined, which should not occur because the number of total cycles is positive");
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
        GetLogger ().Info ($"UpdateAverageCycleTime: the number of total cycles {this.TotalCycles} is too low to get an average cycle time");
        this.AverageCycleTime = null;
      }
      else {
        Debug.Assert (0 <= periodDuration.Ticks);
        Debug.Assert (1 < this.TotalCycles);
        this.AverageCycleTime =
          TimeSpan.FromTicks (periodDuration
                              .Ticks
                              / (this.TotalCycles - 1));
        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().Debug ($"UpdateAverageCycleTime: for duration={periodDuration} totalCycles={this.TotalCycles} => average cycle time={this.AverageCycleTime}");
        }
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

      if (GetLogger ().IsDebugEnabled) {
        GetLogger ().Debug ($"Merge: merge nextSlot={nextSlot} into {this}");
      }

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

      // Sum the scrap counters: a scrap report applies only to a single initial operation slot,
      // so the reports of the merged slot are the reports of the two slots
      // (only if both are consolidated, else full recompute)
      bool scrapConsolidated = this.m_scrapConsolidated && nextOperationSlot.m_scrapConsolidated;
      m_reportedCycles += nextOperationSlot.m_reportedCycles;
      m_reportedParts += nextOperationSlot.m_reportedParts;
      m_validParts += nextOperationSlot.m_validParts;
      m_setupParts += nextOperationSlot.m_setupParts;
      m_scrapParts += nextOperationSlot.m_scrapParts;
      m_fixableParts += nextOperationSlot.m_fixableParts;

      this.EndDateTime = nextSlot.EndDateTime;
      if (scrapConsolidated) {
        m_scrapConsolidated = true;
        m_scrapConsolidationRange.Valid = false;
      }
      else {
        InvalidateScrapFully ();
      }

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
