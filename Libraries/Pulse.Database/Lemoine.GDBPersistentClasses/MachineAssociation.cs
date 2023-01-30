// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Xml.Serialization;

using Lemoine.Info;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Business.Config;
using Lemoine.Database.Persistent;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Abstract base class for all the machine association modification tables
  /// </summary>
  [Serializable,
   XmlInclude (typeof (ActivityManual)),
   XmlInclude (typeof (ComponentMachineAssociation)),
   XmlInclude (typeof (MachineObservationStateAssociation)),
   XmlInclude (typeof (MachineStateTemplateAssociation)),
   XmlInclude (typeof (OperationMachineAssociation)),
   XmlInclude (typeof (ReasonMachineAssociation)),
   XmlInclude (typeof (ShiftMachineAssociation)),
   XmlInclude (typeof (WorkOrderMachineAssociation))]
  public abstract class MachineAssociation : MachineModification, IPeriodAssociationInsert
  {
    static readonly string DEFAULT_MINIMUM_STEP_SPAN_KEY = "Analysis.StepSpan.DefaultMinimum";
    static readonly TimeSpan DEFAULT_MINIMUM_STEP_SPAN_DEFAULT = TimeSpan.FromSeconds (30);

    static readonly string DEFAULT_STEP_SPAN_NO_END_KEY = "Analysis.StepSpan.NoEnd";
    static readonly TimeSpan DEFAULT_STEP_SPAN_NO_END_DEFAULT = TimeSpan.FromDays (1);

    #region Members
    LowerBound<DateTime> m_begin;
    UpperBound<DateTime> m_end;
    bool m_lowerStepSpanPossible = true;
    AssociationOption? m_associationOption;
    string m_dynamic;
    PeriodAssociationInsertImplementation m_insertImplementation;
    #endregion

    #region Getters / Setters
    /// <summary>
    /// UTC date/time range of the association
    /// </summary>
    [XmlIgnore]
    public virtual UtcDateTimeRange Range
    {
      get { return new UtcDateTimeRange (m_begin, m_end); }
    }

    /// <summary>
    /// local begin date/time of the association
    /// used for setting UTC begin date/time using a local time
    /// </summary>
    [XmlAttribute ("LocalBegin")]
    public virtual string LocalBegin
    {
      get
      {
        if (!this.Begin.HasValue) {
          return null;
        }
        else {
          return Begin.Value.ToLocalTime ().ToString ("yyyy-MM-dd HH:mm:ss");
        }
      }

      set
      {
        if (string.IsNullOrEmpty (value)) {
          this.Begin = new LowerBound<DateTime> (null);
        }
        else {
          IFormatProvider provider = CultureInfo.InvariantCulture;
          this.Begin = System.DateTime.Parse (value, provider,
                                              DateTimeStyles.AssumeLocal
                                              | DateTimeStyles.AdjustToUniversal);
        }
      }
    }

    /// <summary>
    /// LocalBegin is never serialized
    /// </summary>
    public virtual bool LocalBeginSpecified { get { return false; } }

    /// <summary>
    /// UTC begin date/time of the association
    /// </summary>
    [XmlIgnore]
    public virtual LowerBound<DateTime> Begin
    {
      get { return m_begin; }
      set { m_begin = value; }
    }

    /// <summary>
    /// UTC begin date/time in SQL string for XML serialization
    /// </summary>
    [XmlAttribute ("Begin")]
    public virtual string SqlBegin
    {
      get
      {
        if (!this.Begin.HasValue) {
          return null;
        }
        else {
          return this.Begin.Value.ToString ("yyyy-MM-dd HH:mm:ss");
        }
      }
      set
      {
        if (string.IsNullOrEmpty (value)) {
          this.Begin = new LowerBound<DateTime> (null);
        }
        else {
          IFormatProvider provider = CultureInfo.InvariantCulture;
          this.Begin = new LowerBound<DateTime> (System.DateTime.Parse (value, provider,
                                                                        DateTimeStyles.AssumeUniversal
                                                                        | DateTimeStyles.AdjustToUniversal));
        }
      }
    }

    /// <summary>
    /// local end date/time of the association
    /// used for setting UTC end date/time using a local time
    /// </summary>
    [XmlAttribute ("LocalEnd")]
    public virtual string LocalEnd
    {
      get
      {
        if (End.HasValue) {
          return End.Value.ToLocalTime ().ToString ("yyyy-MM-dd HH:mm:ss");
        }
        else {
          return null;
        }
      }

      set
      {
        if (string.IsNullOrEmpty (value)) {
          GetLogger ().DebugFormat ("LocalEnd.set: " +
                                    "null or empty value {0}",
                                    value);
          this.End = new UpperBound<DateTime> (null);
        }
        else {
          IFormatProvider provider = CultureInfo.InvariantCulture;
          this.End =
            new UpperBound<DateTime> (System.DateTime.Parse (value, provider,
                                                             DateTimeStyles.AssumeLocal
                                                             | DateTimeStyles.AdjustToUniversal));
        }
      }
    }

    /// <summary>
    /// LocalEnd is never serialized
    /// </summary>
    public virtual bool LocalEndSpecified { get { return false; } }

    /// <summary>
    /// UTC end date/time (optional) of the association
    /// </summary>
    [XmlIgnore]
    public virtual UpperBound<DateTime> End
    {
      get { return m_end; }
      set { m_end = value; }
    }

    /// <summary>
    /// UTC end date/time in SQL string for XML serialization
    /// </summary>
    [XmlAttribute ("End")]
    public virtual string SqlEnd
    {
      get
      {
        if (!this.End.HasValue) {
          return null;
        }
        else {
          return ((DateTime)this.End.Value).ToString ("yyyy-MM-dd HH:mm:ss");
        }
      }
      set
      {
        if (null == value) {
          this.End = new UpperBound<DateTime> (null);
        }
        else {
          IFormatProvider provider = CultureInfo.InvariantCulture;
          this.End = new UpperBound<DateTime> (System.DateTime.Parse (value, provider,
                                                                      DateTimeStyles.AssumeUniversal
                                                                      | DateTimeStyles.AdjustToUniversal));
        }
      }
    }

    /// <summary>
    /// Association option
    /// </summary>
    public virtual AssociationOption? Option
    {
      get { return m_associationOption; }
      set { m_associationOption = value; }
    }

    /// <summary>
    /// Dynamic description
    /// </summary>
    public virtual string Dynamic
    {
      get { return m_dynamic; }
      set { m_dynamic = value; }
    }

    /// <summary>
    /// Dynamic start
    /// </summary>
    public virtual string DynamicStart
    {
      get
      {
        if (string.IsNullOrEmpty (this.Dynamic)) {
          return null;
        }
        var startEnd = this.Dynamic
          .Split (new char[] { ',', ':' }, 2, StringSplitOptions.None);
        if (1 == startEnd.Length) {
          if (GetLogger ().IsDebugEnabled) {
            GetLogger ().DebugFormat ("DynamicStart.get: no start is defined in {0}", this.Dynamic);
          }
          return "";
        }
        else {
          Debug.Assert (2 == startEnd.Length);
          return startEnd[0];
        }
      }
    }

    /// <summary>
    /// Dynamic end
    /// </summary>
    public virtual string DynamicEnd
    {
      get
      {
        if (string.IsNullOrEmpty (this.Dynamic)) {
          return null;
        }
        if (this.Dynamic.StartsWith ("?")) { // Directly apply
          return this.Dynamic.Substring (1);
        }
        var startEnd = this.Dynamic
          .Split (new char[] { ',', ':' }, 2, StringSplitOptions.None);
        if (1 == startEnd.Length) {
          return startEnd[0];
        }
        else {
          Debug.Assert (2 == startEnd.Length);
          return startEnd[1];
        }
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    protected MachineAssociation ()
      : base ()
    {
      this.LoadConfig ();
      m_insertImplementation = new PeriodAssociationInsertImplementation (this);
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="begin"></param>
    protected MachineAssociation (IMachine machine, DateTime begin)
      : base (machine)
    {
      m_begin = begin;

      this.LoadConfig ();
      m_insertImplementation = new PeriodAssociationInsertImplementation (this);
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range">not empty</param>
    protected MachineAssociation (IMachine machine, UtcDateTimeRange range)
      : base (machine)
    {
      Debug.Assert (!range.IsEmpty ());

      if (range.IsEmpty ()) {
        var dateTime = DateTime.UtcNow;
        log.FatalFormat ("MachineAssociation: empty range, fallback use begin=end={0} StackTrace={1}", dateTime, System.Environment.StackTrace);
        m_begin = dateTime;
        m_end = dateTime;
      }
      else {
        m_begin = range.Lower;
        m_end = range.Upper;
      }

      this.LoadConfig ();
      m_insertImplementation = new PeriodAssociationInsertImplementation (this);
    }

    /// <summary>
    /// Specific constructor when begin may be equal to end
    /// because of the use of dynamic times
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    /// <param name="dynamic">dynamic times, not null or empty</param>
    protected MachineAssociation (IMachine machine, LowerBound<DateTime> begin, UpperBound<DateTime> end, string dynamic)
      : base (machine)
    {
      Debug.Assert (!string.IsNullOrEmpty (dynamic));

      if (Bound.Compare<DateTime> (end, begin) < 0) {
        log.FatalFormat ("MachineAssociation: end {0} strictly before begin {1}", end, begin);
      }

      m_begin = begin;
      m_end = end;
      m_dynamic = dynamic;

      this.LoadConfig ();
      m_insertImplementation = new PeriodAssociationInsertImplementation (this);
    }

    /// <summary>
    /// Constructor for abstract modifications, that are kept transient
    /// 
    /// You may add a mainModification reference to be used in the analysis logs
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="begin"></param>
    /// <param name="mainModification"></param>
    protected MachineAssociation (IMachine machine, DateTime begin, IModification mainModification)
      : base (machine, mainModification)
    {
      m_begin = begin;

      this.LoadConfig ();
      m_insertImplementation = new PeriodAssociationInsertImplementation (this);
    }

    /// <summary>
    /// Constructor for abstract modifications, that are kept transient
    /// 
    /// You may add a mainModification reference to be used in the analysis logs
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="mainModification"></param>
    protected MachineAssociation (IMachine machine, UtcDateTimeRange range, IModification mainModification)
      : base (machine, mainModification)
    {
      m_begin = range.Lower;
      m_end = range.Upper;

      this.LoadConfig ();
      m_insertImplementation = new PeriodAssociationInsertImplementation (this);
    }

    void LoadConfig ()
    {
      ConfigSet.Load (DEFAULT_MINIMUM_STEP_SPAN_KEY,
                      DEFAULT_MINIMUM_STEP_SPAN_DEFAULT);
      ConfigSet.Load (DEFAULT_STEP_SPAN_NO_END_KEY,
                      DEFAULT_STEP_SPAN_NO_END_DEFAULT);
    }
    #endregion // Constructors

    /// <summary>
    /// Convert the Association to the given Slot
    /// 
    /// If no slot can be created from the association,
    /// null is returned
    /// </summary>
    /// <returns></returns>
    public abstract TSlot ConvertToSlot<TSlot> ()
      where TSlot : Slot;

    /// <summary>
    /// Merge the data of the current association
    /// with the data of an old slot.
    /// 
    /// The slot and the association must reference the same reference data.
    /// 
    /// oldSlot can't be null.
    /// 
    /// The returned slot has no specific period set and is never null
    /// 
    /// The merge period is given in parameter and can't be used for advanced process
    /// like updating some summary analysis tables
    /// </summary>
    /// <param name="oldSlot">It can't be null and must reference the same machine</param>
    /// <param name="range">merge period range</param>
    /// <returns></returns>
    public abstract TSlot MergeDataWithOldSlot<TSlot> (TSlot oldSlot,
                                                      UtcDateTimeRange range)
      where TSlot : Slot;

    /// <summary>
    /// Insert a period association in database
    /// considering all the existing slots,
    /// cutting them or joining them if necessary
    /// </summary>
    /// <param name="slotDAO">DAO to use to update the slot</param>
    protected internal virtual void Insert<TSlot, I, TSlotDAO> (TSlotDAO slotDAO)
      where TSlot : Slot, I
      where I : ISlot
      where TSlotDAO : SlotDAO<TSlot, I>
    {
      Insert<TSlot, I, TSlotDAO> (slotDAO, null);
    }

    /// <summary>
    /// Insert a period association in database
    /// considering all the existing slots,
    /// cutting them or joining them if necessary
    /// </summary>
    /// <param name="slotDAO">DAO to use to update the slot</param>
    /// <param name="preFetchedImpactedSlots">Pre-fetched list of impacted slots (to be filter). The list is sorted</param>
    protected internal virtual void Insert<TSlot, I, TSlotDAO> (TSlotDAO slotDAO,
                                                                IList<I> preFetchedImpactedSlots)
      where TSlot : Slot, I
      where I : ISlot
      where TSlotDAO : SlotDAO<TSlot, I>
    {
      Insert<TSlot, I, TSlotDAO> (slotDAO, false, preFetchedImpactedSlots);
    }

    /// <summary>
    /// Insert a period association in database
    /// considering all the existing slots,
    /// cutting them or joining them if necessary
    /// </summary>
    /// <param name="slotDAO">DAO to use to update the slot</param>
    /// <param name="pastOnly">Apply the association on the existing past slots only</param>
    protected internal virtual void Insert<TSlot, I, TSlotDAO> (TSlotDAO slotDAO,
                                                                bool pastOnly)
      where TSlot : Slot, I
      where I : ISlot
      where TSlotDAO : SlotDAO<TSlot, I>
    {
      Insert<TSlot, I, TSlotDAO> (slotDAO, pastOnly, null);
    }

    /// <summary>
    /// Insert a period association in database
    /// considering all the existing slots,
    /// cutting them or joining them if necessary
    /// </summary>
    /// <param name="slotDAO">DAO to use to update the slot</param>
    /// <param name="pastOnly">Apply the association on the existing past slots only</param>
    /// <param name="preFetchedImpactedSlots">Pre-fetched list of impacted slots (to be filter). The list is sorted</param>
    protected internal virtual void Insert<TSlot, I, TSlotDAO> (TSlotDAO slotDAO,
                                                                bool pastOnly,
                                                                IList<I> preFetchedImpactedSlots)
      where TSlot : Slot, I
      where I : ISlot
      where TSlotDAO : SlotDAO<TSlot, I>
    {
      m_insertImplementation.Insert<TSlot, I, TSlotDAO> (slotDAO, pastOnly, preFetchedImpactedSlots);
    }

    /// <summary>
    /// Get the impacted slots without considering any pre-fetched slot
    /// </summary>
    /// <param name="slotDAO">DAO to use to update the slot</param>
    /// <param name="pastOnly">Apply the association on the existing past slots only</param>
    public virtual IList<I> GetImpactedSlots<TSlot, I, TSlotDAO> (TSlotDAO slotDAO,
                                                                  bool pastOnly)
      where TSlot : Slot, I
      where I : ISlot
      where TSlotDAO : SlotDAO<TSlot, I>
    {
      bool leftMerge = !this.Option.HasValue
        || !this.Option.Value.HasFlag (AssociationOption.NoLeftMerge);
      bool rightMerge = !this.Option.HasValue
        || !this.Option.Value.HasFlag (AssociationOption.NoRightMerge);

      IList<I> impactedSlots = slotDAO
        .GetImpactedMachineSlotsForAnalysis (this.Machine,
                                             this.Range,
                                             this.DateTime,
                                             pastOnly,
                                             leftMerge,
                                             rightMerge);
      return impactedSlots;
    }

    #region Step utility methods
    /// <summary>
    /// Is a lower step span possible ?
    /// </summary>
    protected virtual bool IsLowerStepSpanPossible ()
    {
      return m_lowerStepSpanPossible;
    }

    /// <summary>
    /// Check if the step timeout is reached.
    /// 
    /// If it is, raise the StepTimeoutException
    /// 
    /// It must be called by the main modification
    /// </summary>
    /// <returns></returns>
    public override void CheckStepTimeout ()
    {
      if (IsStepActive (this.Range)
          && IsLowerStepSpanPossible ()
          && (this.LastAnalysisBegin.Add (AnalysisConfigHelper.ModificationStepTimeout) < DateTime.UtcNow)) {
        TimeSpan minimumStepSpan = this.GetMinimumStepSpan ();
        if (this.AnalysisStepSpan.HasValue && (this.AnalysisStepSpan.Value <= minimumStepSpan)) {
          // Second check here just in case. The call to IsLowerStepSpanPossible is normally sufficient
          if (GetLogger ().IsDebugEnabled) {
            GetLogger ().DebugFormat ("CheckStepTimeout: " +
                             "modification step timeout {0} is reached for modification id={1}, " +
                             "but the analysis step span is already less than the minimum {2} " +
                             "=> do nothing",
                             AnalysisConfigHelper.ModificationStepTimeout,
                             this.Id, minimumStepSpan);
          }
        }
        else {
          if (GetLogger ().IsErrorEnabled) {
            GetLogger ().ErrorFormat ("CheckStepTimeout: " +
                             "elapsed time is {0} for modification id={1} while modification step time out is {2}",
                             DateTime.UtcNow.Subtract (this.LastAnalysisBegin),
                             this.Id, AnalysisConfigHelper.ModificationStepTimeout);
          }
          throw new StepTimeoutException ();
        }
      }
    }

    /// <summary>
    /// Get the minimum step span
    /// </summary>
    /// <returns></returns>
    protected virtual TimeSpan GetMinimumStepSpan ()
    {
      return ConfigSet.LoadAndGet<TimeSpan> (DEFAULT_MINIMUM_STEP_SPAN_KEY,
                                             DEFAULT_MINIMUM_STEP_SPAN_DEFAULT); // By default: 30s
    }

    /// <summary>
    /// Check if the step process should be active or not for the specified range
    /// 
    /// By default, it is not active in the future or when the main modification is transient
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    protected virtual bool IsStepActive (UtcDateTimeRange range)
    {
      return !IsMainModificationTransient ()
        && (Bound.Compare<DateTime> (range.Lower, DateTime.UtcNow) < 0);
    }

    /// <summary>
    /// Compute a new step span after a step timeout
    /// 
    /// By default, the current step span is divided by 2
    /// or one day is considered in case end is null.
    /// 
    /// In case it needs to be overriden, override
    /// ComputeNewStepSpan(DateTime begin, DateTime? end)
    /// instead
    /// </summary>
    /// <returns></returns>
    protected override TimeSpan? ComputeNewStepSpan ()
    {
      UtcDateTimeRange range = GetStepDefaultRange ();
      return ComputeNewStepSpan (range);
    }

    /// <summary>
    /// Compute a new step span after a step timeout
    /// 
    /// By default, the current step span is divided by 2
    /// or one day is considered in case end is null.
    /// 
    /// But this can be overridden by the child classes
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    protected virtual TimeSpan? ComputeNewStepSpan (UtcDateTimeRange range)
    {
      if (this.AnalysisStepSpan.HasValue) {
        return TimeSpan.FromSeconds (TimeSpan.FromTicks (this.AnalysisStepSpan.Value.Ticks / 2).TotalSeconds);
      }
      else if (range.Duration.HasValue) {
        return TimeSpan.FromSeconds (TimeSpan.FromTicks (range.Duration.Value.Ticks / 2).TotalSeconds);
      }
      else { // No AnalysisStepSpan and no end
        return ConfigSet.LoadAndGet<TimeSpan> (DEFAULT_STEP_SPAN_NO_END_KEY,
                                               DEFAULT_STEP_SPAN_NO_END_DEFAULT); // 1 day by default
      }
    }

    /// <summary>
    /// Get the UTC date/time range that has not been processed yet
    /// </summary>
    /// <returns></returns>
    protected virtual UtcDateTimeRange GetNotPocessedRange ()
    {
      return new UtcDateTimeRange (this.AnalysisAppliedDateTime.HasValue
                                   ? this.AnalysisAppliedDateTime.Value
                                   : this.Begin,
                                   this.End);
    }

    /// <summary>
    /// Get the default step range after a step time out but before adjusting it with the step span
    /// </summary>
    /// <returns></returns>
    protected virtual UtcDateTimeRange GetStepDefaultRange ()
    {
      return GetNotPocessedRange ();
    }

    /// <summary>
    /// Get the adjusted step range from the already applied date/time, the analysis status and the step span
    /// </summary>
    /// <returns></returns>
    protected virtual UtcDateTimeRange GetNotAppliedRange ()
    {
      // - Default applicable range
      UtcDateTimeRange range = GetStepDefaultRange ();
      m_lowerStepSpanPossible = true;

      // - Get a new step span in case of Step timeout
      if (AnalysisStatus.StepTimeout == this.AnalysisStatus) {
        TimeSpan? newStepSpan = ComputeNewStepSpan (range);
        if (newStepSpan.HasValue
            && (!this.AnalysisStepSpan.HasValue || (newStepSpan < this.AnalysisStepSpan.Value))) {
          if (GetLogger ().IsDebugEnabled) {
            GetLogger ().DebugFormat ("GetNotAppliedRange: " +
                             "step span is going from {0} to {1}",
                             this.AnalysisStepSpan, newStepSpan);
          }
          this.AnalysisStepSpan = newStepSpan.Value;
          ModelDAOHelper.DAOFactory.ModificationDAO.MakePersistent (this);
        }
        else {
          if (GetLogger ().IsDebugEnabled) {
            GetLogger ().DebugFormat ("GetNotAppliedRange: " +
                             "no better step span could be computed");
          }
          m_lowerStepSpanPossible = false;
        }
        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().DebugFormat ("GetNotAppliedRange: " +
                           "the new step span is {0}",
                           this.AnalysisStepSpan);
        }
      }

      // - Check the minimum step span has not been already reached
      //   This is necessary to check it here because the analysis status may be InProgress
      if (m_lowerStepSpanPossible && this.AnalysisStepSpan.HasValue) {
        TimeSpan minimumStepSpan = GetMinimumStepSpan ();
        if (this.AnalysisStepSpan.Value <= minimumStepSpan) {
          if (GetLogger ().IsInfoEnabled) {
            GetLogger ().InfoFormat ("GetNotAppliedRange: " +
                            "minimum step range {0} reached",
                            minimumStepSpan);
          }
          if (this.AnalysisStepSpan.Value < minimumStepSpan) {
            this.AnalysisStepSpan = minimumStepSpan;
            ModelDAOHelper.DAOFactory.ModificationDAO.MakePersistent (this);
          }
          m_lowerStepSpanPossible = false;
        }
      }

      // - adjust the new end from the step span
      if (range.Lower.HasValue && IsStepActive (range) && this.AnalysisStepSpan.HasValue) {
        DateTime stepEnd = range.Lower.Value.Add (this.AnalysisStepSpan.Value);
        if (Bound.Compare<DateTime> (stepEnd, range.Upper) < 0) {
          range = new UtcDateTimeRange (range.Lower, stepEnd);
          if (GetLogger ().IsDebugEnabled) {
            GetLogger ().DebugFormat ("GetNotAppliedRange: " +
                             "the adjusted end from step is {0}",
                             stepEnd);
          }
        }
      }

      return range;
    }

    /// <summary>
    /// Mark the modification as completed (Done) or partially completed (InProgress)
    /// </summary>
    /// <param name="message">message to send to the web service (nullable)</param>
    /// <param name="effectiveEnd"></param>
    protected override void MarkAsCompleted (string message, DateTime? effectiveEnd)
    {
      if (effectiveEnd.HasValue && (Bound.Compare<DateTime> (effectiveEnd.Value, this.End) < 0)) { // Partially done... continue later
        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().DebugFormat ("MakeAsCompleted: " +
                           "step completed, but the period {0}-{1} still needs to be processed",
                           effectiveEnd, this.End);
        }
        base.MarkAsInProgress (effectiveEnd);
      }
      else { // Completed
        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().DebugFormat ("MakeAsCompleted: " +
                           "completed !");
        }
        base.MarkAsCompleted (message);
      }
    }
    #endregion // Step utility methods
  }
}
