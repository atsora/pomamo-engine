// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Business;
using Lemoine.Business.Reason;
using Lemoine.Collections;
using Pulse.Extensions.Database;
using Lemoine.Business.Extension;
using Pulse.Extensions.Business.Reason;
using System.Text.Json;
using Pulse.Business.Reason;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table ReasonSlot
  /// 
  /// Analysis table where are stored all
  /// the Machine Mode and Reason periods of a given machine.
  /// </summary>
  [Serializable]
  public class ReasonSlot : GenericMachineRangeSlot, IReasonSlot
  {
    #region Members
    IMachineMode m_machineMode;
    IReason m_reason = null;
    IMachineObservationState m_machineObservationState;
    IShift m_shift;
    bool m_overwriteRequired;
    string m_reasonDetails;
    double m_reasonScore = -1.0;
    ReasonSource m_reasonSource = ReasonSource.Default;
    int m_autoReasonNumber = 0;
    IProductionState m_productionState = null;
    double? m_productionRate = null;

    IReasonSlot m_oldSlotFromModification = null; // Old reason slot in case of Modification.MergeDataWithOldSlot
    IModification m_modification = null;
    ReasonSlotChange m_reasonSlotChange = ReasonSlotChange.None;
    UpperBound<DateTime> m_consolidationLimit = new UpperBound<DateTime> (null);

    IEnumerable<IReasonExtension> m_reasonExtensions;
    IEnumerable<IProductionStateExtension> m_productionStateExtensions;
    #endregion // Members

    ILog log = LogManager.GetLogger (typeof (ReasonSlot).FullName);

    #region Constructors and factory methods
    /// <summary>
    /// The default constructor is forbidden
    /// and is only used by NHibernate
    /// </summary>
    protected ReasonSlot ()
      : base (true)
    {
    }

    /// <summary>
    /// Constructor for new slots
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    public ReasonSlot (IMachine machine,
                       UtcDateTimeRange range)
      : base (true, machine, range)
    {
      if (range.IsEmpty ()) {
        log.ErrorFormat ("ReasonSlot: " +
                         "create a new slot with an empty range. " +
                         "StackTrace: {0}",
                         System.Environment.StackTrace);
      }

      if (range.Duration.HasValue && (0 == range.Duration.Value.Ticks)) {
        log.ErrorFormat ("UpdateDateTimeRange: " +
                         "range {0} with an empty duration {1}. " +
                         "StackTrace: {2}",
                         range, range.Duration,
                         System.Environment.StackTrace);
      }
    }
    #endregion // Constructors and factory methods

    #region Getters / Setters
    /// <summary>
    /// Reason slot change
    /// </summary>
    internal protected virtual bool FlagRemoved { get; set; } = false;

    /// <summary>
    /// Optionally UTC end date/time of the slot
    /// </summary>
    public override UpperBound<DateTime> EndDateTime
    {
      get { return base.EndDateTime; }
      set {
        if (Bound.Compare<DateTime> (value, this.BeginDateTime) <= 0) {
          log.ErrorFormat ("EndDateTime.set: " +
                           "new date/time range is empty {0}-{1}. " +
                           "StackTrace: {2}",
                           this.BeginDateTime, value,
                           System.Environment.StackTrace);
        }

        if (object.Equals (value, base.EndDateTime)) {
          // No change
          return;
        }

        if (log.IsDebugEnabled) {
          log.DebugFormat ("EndDateTime.set: change from {0} to {1} for id={2}",
            this.EndDateTime, value, this.Id);
        }

        base.EndDateTime = value;
        m_reasonSlotChange = m_reasonSlotChange.Add (ReasonSlotChange.Period);
      }
    }

    /// <summary>
    /// Reference to the Machine Mode
    /// </summary>
    public virtual IMachineMode MachineMode
    {
      get { return m_machineMode; }
      set {
        Debug.Assert (null != value);
        if (value == null) {
          log.ErrorFormat ("MachineMode.set: " +
                           "null value");
          throw new ArgumentNullException ();
        }

        if (object.Equals (m_machineMode, value)) {
          return; // No change
        }

        if (null != m_machineMode) {
          m_reasonSlotChange = m_reasonSlotChange.Add (ReasonSlotChange.MachineMode);
        } // Else initial set, for example in unit tests
        m_machineMode = value;
      }
    }

    /// <summary>
    /// Reference to the Reason
    /// 
    /// null corresponds to a default reason
    /// </summary>
    public virtual IReason Reason
    {
      get { return m_reason; }
    }

    /// <summary>
    /// Set the old slot in memory in case ReasonMachineAssociation.MergeDataWithOldSlot is used
    /// 
    /// It can be used by the reason plugins
    /// </summary>
    /// <param name="oldSlot">not null</param>
    /// <param name="modification">not null</param>
    protected internal virtual void SetOldSlotFromModification (IReasonSlot oldSlot, IModification modification)
    {
      Debug.Assert (null != oldSlot);
      Debug.Assert (null != modification);

      if (null != m_oldSlotFromModification) {
        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().DebugFormat ("SetOldSlotFromModification: an old slot was already set previously: {0}/modificationid={1} => {2}/modificationid={3}", m_oldSlotFromModification, ((IDataWithId<long>)m_modification).Id, oldSlot, ((IDataWithId<long>)modification).Id);
        }
      }

      m_oldSlotFromModification = oldSlot;
      m_modification = modification;
    }

    /// <summary>
    /// Merge the next slot with this when:
    /// <item>it comes right after this</item>
    /// <item>the reference data are the same</item>
    /// </summary>
    /// <param name="nextSlot"></param>
    public override void Merge (ISlot nextSlot)
    {
      if (nextSlot is ReasonSlot) {
        ReasonSlot nextReasonSlot = nextSlot as ReasonSlot;
        m_consolidationLimit = nextReasonSlot.m_consolidationLimit;
      }
      base.Merge (nextSlot);
    }

    /// <summary>
    /// <see cref="Slot.MergeSamePeriodAdditionalProperties{TSlot}(TSlot)"/>
    /// </summary>
    /// <typeparam name="TSlot"></typeparam>
    /// <param name="slot"></param>
    public override void MergeSamePeriodAdditionalProperties<TSlot> (TSlot slot)
    {
      if (slot is ReasonSlot) {
        ReasonSlot reasonSlot = slot as ReasonSlot;
        if (Bound.Equals<DateTime> (this.EndDateTime, reasonSlot.EndDateTime)) {
          if (Bound.Compare<DateTime> (reasonSlot.ConsolidationLimit, m_consolidationLimit) < 0) {
            m_consolidationLimit = reasonSlot.ConsolidationLimit;
          }
        }
        MergeAdditionalProperties (reasonSlot);
      }
    }

    /// <summary>
    /// Merge a slot with a next one
    /// </summary>
    /// <typeparam name="TSlot"></typeparam>
    /// <param name="slot"></param>
    public override void MergeWithNextSlot<TSlot> (TSlot slot)
    {
      if (slot is ReasonSlot) {
        ReasonSlot reasonSlot = slot as ReasonSlot;
        m_consolidationLimit = reasonSlot.ConsolidationLimit;
        MergeAdditionalProperties (reasonSlot);
      }
      base.MergeWithNextSlot<TSlot> (slot);
    }

    void MergeAdditionalProperties (ReasonSlot reasonSlot)
    {
      m_reasonSlotChange = m_reasonSlotChange.Add (reasonSlot.m_reasonSlotChange);
      if (!object.Equals (this.OverwriteRequired, reasonSlot.OverwriteRequired)
        || !NHibernateHelper.EqualsNullable (this.Reason, reasonSlot.Reason, (a, b) => a.Id == b.Id)
        || !object.Equals (this.ReasonDetails, reasonSlot.ReasonDetails)
        || (this.ReasonScore != reasonSlot.ReasonScore)
        || !this.ReasonSource.IsSameMainSource (reasonSlot.ReasonSource)) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("MergeWithNextSlot: switch to processing because incompatible reasons");
        }
        this.ReasonSource = this.ReasonSource.Add (reasonSlot.ReasonSource);
        if (0 < reasonSlot.AutoReasonNumber) {
          this.SetUnsafeAutoReasonNumber ();
        }
        if (reasonSlot.ReasonSource.HasFlag (ReasonSource.Manual)) {
          this.SetUnsafeManualFlag ();
        }
        this.SwitchToProcessing ();
        // For the moment, postpone the computation of the production state / rate after the reason is set
        this.ProductionState = null;
        this.ProductionRate = null;
      }
      else { // Same main reason
        if (this.AutoReasonNumber != reasonSlot.AutoReasonNumber) {
          this.SetUnsafeAutoReasonNumber ();
        }
        if (!this.ReasonSource.Equals (reasonSlot.ReasonSource)) {
          this.ReasonSource = this.ReasonSource.Add (reasonSlot.ReasonSource);
        }
        if (!NHibernateHelper.EqualsNullable (this.ProductionState, reasonSlot.ProductionState, (a, b) => a.Id == b.Id)) {
          this.ConsolidateProductionStateRate (null, null);
        }
        else { // The production state can probably be kept. Just re-compute the production rate
          if (!this.ProductionRate.HasValue && !reasonSlot.ProductionRate.HasValue) {
            // Do not change anything => no production rate
          }
          else if (this.ProductionRate.HasValue && reasonSlot.ProductionRate.HasValue) {
            if (!this.DateTimeRange.Overlaps (reasonSlot.DateTimeRange)
              && this.DateTimeRange.Duration.HasValue && reasonSlot.DateTimeRange.Duration.HasValue) {
              var durationSeconds = this.DateTimeRange.Duration.Value.TotalSeconds + reasonSlot.Duration.Value.TotalSeconds;
              if (0 != durationSeconds) {
                this.ProductionRate = (this.ProductionRate.Value * this.DateTimeRange.Duration.Value.TotalSeconds + reasonSlot.ProductionRate.Value * reasonSlot.DateTimeRange.Duration.Value.TotalSeconds) / durationSeconds;
              }
              else {
                log.Warn ($"MergeAdditionalProperties: new duration is 0");
                this.ProductionRate = null;
              }
            }
            else { // Overlaps
              if (this.DateTimeRange.ContainsRange (reasonSlot.DateTimeRange)) {
                // Do nothing
              }
              else if (reasonSlot.DateTimeRange.ContainsRange (this.DateTimeRange)) {
                this.ProductionRate = reasonSlot.ProductionRate;
              }
              else { // Recompute 
                this.ConsolidateProductionStateRate (null, null);
              }
            }
          }
          else {
            this.ConsolidateProductionStateRate (null, null);
          }
        }
      }
    }

    /// <summary>
    /// <see cref="IReasonSlot"/>
    /// </summary>
    /// <param name="reasonSlot">not null</param>
    public virtual void MergeWithNext (IReasonSlot reasonSlot)
    {
      Debug.Assert (null != reasonSlot, "reasonSlot is null");
      Debug.Assert (0 == reasonSlot.ModificationTrackerLevel, "Modification tracker level of reasonslot is not 0");
      Debug.Assert (0 == this.ModificationTrackerLevel, "Modification tracker level of this is not 0");
      Debug.Assert (0 != this.Id, "this.Id is 0");
      Debug.Assert (0 != reasonSlot.Id, "reasonSlot.Id is 0");

      if (0 == this.Id) {
        log.Error ($".{this.Machine.Id} MergeWithNext: id is 0\n{System.Environment.StackTrace}");
      }
      if (0 == reasonSlot.Id) {
        log.Error ($".{reasonSlot.Machine.Id} MergeWithNext: next id is 0\n{System.Environment.StackTrace}");
      }

      using (var modificationTracker1 = new SlotModificationTracker<IReasonSlot> (reasonSlot, false)) {
        using (var modificationTracker2 = new SlotModificationTracker<IReasonSlot> (this, false)) {
          var oldFirst = (IReasonSlot)this.Clone ();
          this.MergeWithNextSlot<ReasonSlot> ((ReasonSlot)reasonSlot);
          AnalysisAccumulator.MergeReasonSlots (oldFirst, reasonSlot, this);
        }
      }
    }

    #region ICloneable implementation
    /// <summary>
    /// Make a shallow copy
    /// <see cref="ICloneable.Clone" />
    /// </summary>
    /// <returns></returns>
    public override object Clone ()
    {
      var clone = (ReasonSlot)base.Clone ();
      clone.ModificationTrackerLevel = 0;
      return clone;
    }
    #endregion // ICloneable implementation

    /// <summary>
    /// Clone the object with a new begin date/time
    /// </summary>
    /// <param name="newRange">new range (not empty)</param>
    /// <returns></returns>
    public override ISlot Clone (UtcDateTimeRange newRange)
    {
      var newSlot = (ReasonSlot)base.Clone ();

      // Check if ConsolidationLimit can be kept with the new range
      if (this.ConsolidationLimit.HasValue
        && (Bound.Compare<DateTime> (this.ConsolidationLimit.Value, newRange.Upper) < 0)) {
        Debug.Assert (false);
        log.Fatal ($"Clone: consolidation limit is moved from {this.ConsolidationLimit} to {newRange.Upper}. This should be managed before");
        newSlot.m_consolidationLimit = newRange.Upper;
      }

      newSlot.UpdateDateTimeRange (newRange);
      return newSlot;
    }

    /// <summary>
    /// Consider this reason slot corresponds to a new activity slot
    /// </summary>
    public virtual void SetNewActivitySlot ()
    {
      m_reasonSlotChange = m_reasonSlotChange.Add (ReasonSlotChange.NewActivity);
    }

    /// <summary>
    /// <see cref="IReasonSlot"/>
    /// </summary>
    public virtual void CancelData ()
    {
      using (var modificationTracker = new SlotModificationTracker<IReasonSlot> (this)) {
        this.ReasonSource = ReasonSource.Default;
        this.ReasonScore = -1.0;
        this.OverwriteRequired = false;
        this.ReasonDetails = null;
        m_autoReasonNumber = 0;
      }
    }

    /// <summary>
    /// Switch the reason slot to a processing status
    /// </summary>
    public virtual void SwitchToProcessing ()
    {
      if ((null != m_reason) && (m_reason.Id == (int)ReasonId.Processing)) {
        return;
      }

      var processing = ModelDAOHelper.DAOFactory.ReasonDAO
        .FindById ((int)ReasonId.Processing);
      using (var modificationTracker = new SlotModificationTracker<IReasonSlot> (this)) {
        m_reason = processing;
        m_reasonDetails = null;
        m_reasonSlotChange = m_reasonSlotChange.Add (ReasonSlotChange.Reason);
      }
    }

    /// <summary>
    /// Reset a manual reason
    /// considering only the auto reason machine associations (not the manual ones)
    /// </summary>
    public virtual void ResetManualReason ()
    {
      if (!this.ReasonSource.HasFlag (ReasonSource.Manual)) {
        log.Error ($"ResetManualReason: no manual reason to reset, the reason source is {this.ReasonSource}");
        return;
      }

      using (var modificationTracker = new SlotModificationTracker<IReasonSlot> (this)) {
        // Data
        if (!string.IsNullOrEmpty (this.JsonData)) {
          var extensionRequest = new GlobalExtensions<IReasonDataExtension> (x => x.Initialize ());
          var extensions = Lemoine.Business.ServiceProvider.Get (extensionRequest);
          if (extensions.Any (x => x.DoReset (this))) {
            var data = Pulse.Business.Reason.ReasonData.Deserialize (this.JsonData, extensions);
            data = data.ToDictionary (x => x.Key, x => x.Value); // Clone it
            foreach (var extension in extensions.Where (ext => ext.DoReset (this))) {
              extension.Reset (data);
            }
            this.JsonData = JsonSerializer.Serialize (data);
          }
        }

        switch (this.ReasonSource) {
          case ReasonSource.Manual:
            m_reasonSource = this.ReasonSource.ResetManual ();
            SwitchToProcessing ();
            m_reasonSlotChange = m_reasonSlotChange.Add (ReasonSlotChange.ResetManual);
            break;
          default:
            m_reasonSource = this.ReasonSource.ResetManual ();
            break;
        }
      }
    }

    /// <summary>
    /// Set a default reason
    /// </summary>
    /// <param name="reason">not null</param>
    /// <param name="score"></param>
    /// <param name="overwriteRequired"></param>
    /// <param name="auto"></param>
    /// <param name="consolidationLimit"></param>
    public virtual void SetDefaultReason (IReason reason, double score, bool overwriteRequired, bool auto, UpperBound<DateTime> consolidationLimit)
    {
      Debug.Assert (null != reason);

      using (var modificationTracker = new SlotModificationTracker<IReasonSlot> (this)) {
        var oldReason = m_reason;
        m_reason = reason;
        m_reasonDetails = null;
        m_reasonScore = score;
        m_overwriteRequired = overwriteRequired;
        if (auto) {
          m_reasonSource = m_reasonSource.SetMainDefaultAuto ();
          ++m_autoReasonNumber;
        }
        else {
          m_reasonSource = m_reasonSource.SetMainDefault ();
        }
        m_consolidationLimit = consolidationLimit;
        SetReasonConsolidated (oldReason);
      }
    }

    /// <summary>
    /// Set a default reason with a default consolidation limit (for the unit tests)
    /// </summary>
    /// <param name="reason">not null</param>
    /// <param name="score"></param>
    /// <param name="overwriteRequired"></param>
    /// <param name="auto"></param>
    protected internal virtual void SetDefaultReason (IReason reason, double score, bool overwriteRequired, bool auto)
    {
      SetDefaultReason (reason, score, overwriteRequired, auto, new UpperBound<DateTime> ());
    }

    /// <summary>
    /// Set a manual reason
    /// </summary>
    /// <param name="reason">not null</param>
    /// <param name="score"></param>
    /// <param name="consolidationLimit"></param>
    /// <param name="reasonDetails">Optional: details</param>
    /// <param name="jsonData">Optional: data in Json format</param>
    protected internal virtual void SetManualReason (IReason reason, double score, UpperBound<DateTime> consolidationLimit, string reasonDetails = null, string jsonData = null)
    {
      Debug.Assert (null != reason);

      using (var modificationTracker = new SlotModificationTracker<IReasonSlot> (this)) {
        var oldReason = m_reason;
        m_reason = reason;
        m_reasonDetails = reasonDetails;
        m_reasonScore = score;
        m_overwriteRequired = false;
        m_reasonSource = ReasonSource.Manual;
        this.JsonData = jsonData;
        AddConsolidationLimit (consolidationLimit);
        SetReasonConsolidated (oldReason);
      }
    }

    /// <summary>
    /// Set a manual reason with a default consolidation limit (for the unit tests)
    /// </summary>
    /// <param name="reason">not null</param>
    /// <param name="score"></param>
    /// <param name="reasonDetails">Optional: reason details</param>
    public virtual void SetManualReason (IReason reason, double score, string reasonDetails = null)
    {
      SetManualReason (reason, score, new UpperBound<DateTime> (), reasonDetails);
    }

    /// <summary>
    /// Set a main auto reason
    /// </summary>
    /// <param name="reason">not null</param>
    /// <param name="score"></param>
    /// <param name="overwriteRequired"></param>
    /// <param name="consolidationLimit"></param>
    /// <param name="reasonDetails">Optional: reason details</param>
    /// <param name="jsonData">Optional: data in Json format</param>
    protected internal virtual void SetMainAutoReason (IReason reason, double score, bool overwriteRequired, UpperBound<DateTime> consolidationLimit, string reasonDetails = null, string jsonData = null)
    {
      Debug.Assert (null != reason);

      using (var modificationTracker = new SlotModificationTracker<IReasonSlot> (this)) {
        var oldReason = m_reason;
        m_reason = reason;
        m_reasonDetails = reasonDetails;
        m_reasonScore = score;
        m_overwriteRequired = overwriteRequired;
        m_reasonSource = m_reasonSource.SetMainAuto ();
        this.JsonData = jsonData;
        ++m_autoReasonNumber;
        AddConsolidationLimit (consolidationLimit);
        SetReasonConsolidated (oldReason);
      }
    }

    /// <summary>
    /// Set a main auto reason with a default consolidation limit (for the unit tests)
    /// </summary>
    /// <param name="reason">not null</param>
    /// <param name="score"></param>
    /// <param name="overwriteRequired"></param>
    /// <param name="reasonDetails">Optional: reason details</param>
    public virtual void SetMainAutoReason (IReason reason, double score, bool overwriteRequired, string reasonDetails = null)
    {
      SetMainAutoReason (reason, score, overwriteRequired, new UpperBound<DateTime> (), reasonDetails);
    }

    /// <summary>
    /// Add an extra auto reason
    /// </summary>
    public virtual void AddExtraAutoReason ()
    {
      ++m_autoReasonNumber;
    }

    /// <summary>
    /// Remove an extra auto reason
    /// </summary>
    public virtual void RemoveExtraAutoReason ()
    {
      --m_autoReasonNumber;
    }

    /// <summary>
    /// Add an extra manual reason
    /// </summary>
    public virtual void AddExtraManualReason ()
    {
      m_overwriteRequired = false;
      m_reasonSource = m_reasonSource.AddExtraManual ();
    }

    /// <summary>
    /// Reference to the MachineObservationState
    /// 
    /// A null MachineObservationState can't be set
    /// </summary>
    public virtual IMachineObservationState MachineObservationState
    {
      get { return m_machineObservationState; }
      set {
        Debug.Assert (null != value);
        if (value is null) {
          log.Error ("MachineObservationState.set: null value");
          throw new ArgumentNullException ();
        }

        if (this.DateTimeRange.IsEmpty ()) {
          log.Error ($"MachineObservationState.set: empty range. StackTrace: {System.Environment.StackTrace}");
        }

        if (object.Equals (m_machineObservationState, value)) {
          return; // No change
        }

        if (null != m_machineObservationState) {
          m_reasonSlotChange = m_reasonSlotChange.Add (ReasonSlotChange.MachineObservationState);
        } // Else initial set, for example in unit tests
        m_machineObservationState = value;
      }
    }

    /// <summary>
    /// Reference to the Shift
    /// 
    /// nullable
    /// </summary>
    public virtual IShift Shift
    {
      get { return m_shift; }
      set {
        if (object.Equals (m_shift, value)) {
          return; // No change
        }

        m_shift = value;
        // Note: a change of shift does not change the default reason
      }
    }

    /// <summary>
    /// Default reason ?
    /// </summary>
    public virtual bool DefaultReason
    {
      get { return m_reasonSource.IsDefault (); }
    }

    /// <summary>
    /// Should the operator overwrite the reason
    /// in this reason slot ?
    /// </summary>
    public virtual bool OverwriteRequired
    {
      get { return m_overwriteRequired; }
      internal protected set { m_overwriteRequired = value; }
    }

    /// <summary>
    /// Reason details
    /// </summary>
    public virtual string ReasonDetails
    {
      get { return m_reasonDetails; }
      set { m_reasonDetails = value; }
    }

    /// <summary>
    /// Reason score
    /// </summary>
    public virtual double ReasonScore
    {
      get { return m_reasonScore; }
      internal protected set { m_reasonScore = value; }
    }

    /// <summary>
    /// Number of auto reasons
    /// </summary>
    public virtual int AutoReasonNumber
    {
      get { return m_autoReasonNumber; }
      internal protected set { m_autoReasonNumber = value; }
    }

    /// <summary>
    /// Flag the number of auto-reasons as unsafe
    /// </summary>
    public virtual void SetUnsafeAutoReasonNumber ()
    {
      m_reasonSource = m_reasonSource.SetUnsafeAutoReasonNumber ();
    }

    /// <summary>
    /// Set the manual flag as unsafe
    /// </summary>
    public virtual void SetUnsafeManualFlag ()
    {
      m_reasonSource = m_reasonSource.SetUnsafeManualFlag ();
    }

    /// <summary>
    /// Reason source
    /// </summary>
    public virtual ReasonSource ReasonSource
    {
      get { return m_reasonSource; }
      internal protected set { m_reasonSource = value; }
    }

    /// <summary>
    /// <see cref="Slot.Consolidated" />
    /// </summary>
    public override bool Consolidated
    {
      get {
        if (null == m_reason) {
          return false;
        }
        else {
          return m_reasonSlotChange.IsEmpty ();
        }
      }
      set {
        if (false == value) {
          m_reasonSlotChange = m_reasonSlotChange.Add (ReasonSlotChange.Requested);
        }
        else {
          m_reasonSlotChange = ReasonSlotChange.None;
        }
      }
    }

    void SetReasonConsolidated (IReason oldReason)
    {
      if (!NHibernateHelper.EqualsNullable (m_reason, oldReason, (x, y) => x.Id == y.Id)) {
        m_reasonSlotChange = m_reasonSlotChange.Add (ReasonSlotChange.Reason);
      }
    }

    /// <summary>
    /// Is the machine module considered running ?
    /// </summary>
    public virtual bool Running
    {
      get { return MachineMode.Running.HasValue && MachineMode.Running.Value; }
    }

    /// <summary>
    /// Is the machine module considered not running ?
    /// </summary>
    public virtual bool NotRunning
    {
      get { return MachineMode.Running.HasValue && !MachineMode.Running.Value; }
    }

    /// <summary>
    /// Consolidation limit date/time
    /// </summary>
    public virtual UpperBound<DateTime> ConsolidationLimit
    {
      get { return m_consolidationLimit; }
    }

    /// <summary>
    /// Combine the existing consolidation limit with a new one
    /// </summary>
    /// <param name="consolidationLimit"></param>
    public virtual void AddConsolidationLimit (UpperBound<DateTime> consolidationLimit)
    {
      // Note: the date/time range may be updated later,
      // so no check on whether consolidationLimit is compatible with the current range is possible
      m_consolidationLimit = UpperBound.GetMinimum (m_consolidationLimit, consolidationLimit);
    }

    /// <summary>
    /// <see cref="IReasonSlot"/>
    /// </summary>
    public virtual IProductionState ProductionState
    {
      get { return m_productionState; }
      set { m_productionState = value; }
    }

    /// <summary>
    /// <see cref="IReasonSlot"/>
    /// </summary>
    public virtual double? ProductionRate
    {
      get { return m_productionRate; }
      set { m_productionRate = value; }
    }

    /// <summary>
    /// Reason data in Json format
    /// </summary>
    public virtual string JsonData { get; set; } = null;
    #endregion // Getters / Setters

    #region IPossibleReason implementation
    /// <summary>
    /// <see cref="IPossibleReason"/>
    /// </summary>
    public virtual IDictionary<string, object> Data => Pulse.Business.Reason.ReasonData.Deserialize (this.JsonData);

    /// <summary>
    /// <see cref="IPossibleReason"/>
    /// </summary>
    public virtual UtcDateTimeRange RestrictedRange
    {
      get { return this.DateTimeRange; }
    }

    /// <summary>
    /// <see cref="IPossibleReason"/>
    /// </summary>
    public virtual IMachineMode RestrictedMachineMode
    {
      get { return this.MachineMode; }
    }

    /// <summary>
    /// <see cref="IPossibleReason"/>
    /// </summary>
    public virtual IMachineObservationState RestrictedMachineObservationState
    {
      get { return this.MachineObservationState; }
    }
    #endregion // IPossibleReason implementation

    /// <summary>
    /// Get the ProductionStateExtensions (lazy initialization)
    /// 
    /// They are ordered by descending score
    /// </summary>
    /// <returns></returns>
    IEnumerable<IProductionStateExtension> GetProductionStateExtensions ()
    {
      if (null == m_productionStateExtensions) { // Initialization
        if (!Lemoine.Extensions.ExtensionManager.IsActive ()) {
          log.WarnFormat ("GetReasonExtensions: " +
                          "the extensions are not active");
        }

        IMonitoredMachine monitoredMachine;
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          monitoredMachine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (this.Machine.Id);
        }
        if (null == monitoredMachine) {
          m_productionStateExtensions = new List<IProductionStateExtension> ();
        }
        else {
          var request = new Lemoine.Business.Extension
            .MonitoredMachineExtensions<IProductionStateExtension> (monitoredMachine,
            (ext, m) => ext.Initialize (m));
          m_productionStateExtensions = Lemoine.Business.ServiceProvider
            .Get (request)
            .OrderByDescending (x => x.Score)
            .ToList ();
        }
      }
      return m_productionStateExtensions;
    }

    /// <summary>
    /// Get the ResetReasonExtensions (lazy initialization)
    /// </summary>
    /// <returns></returns>
    IEnumerable<IReasonExtension> GetReasonExtensions ()
    {
      if (null == m_reasonExtensions) { // Initialization
        if (!Lemoine.Extensions.ExtensionManager.IsActive ()) {
          log.Warn ("GetReasonExtensions: the extensions are not active");
        }

        IMonitoredMachine monitoredMachine;
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          monitoredMachine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (this.Machine.Id);
        }
        if (null == monitoredMachine) {
          m_reasonExtensions = new List<IReasonExtension> ();
        }
        else {
          var request = new Lemoine.Business.Extension
            .MonitoredMachineExtensions<IReasonExtension> (monitoredMachine,
            (ext, m) => ext.Initialize (m));
          m_reasonExtensions = Lemoine.Business.ServiceProvider
            .Get (request);
        }
      }
      return m_reasonExtensions;
    }

    /// <summary>
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
      if (obj is IReasonSlot) {
        IReasonSlot other = (IReasonSlot)obj;
        if (other.Machine.Equals (this.Machine)) {
          return this.BeginDateTime.CompareTo (other.BeginDateTime);
        }
        else {
          log.Error ($"CompareTo: trying to compare reason slots for different machines {this} {other}");
          throw new ArgumentException ("Comparison of ReasonSlot from different machines");
        }
      }

      log.Error ($"CompareTo: object {obj} of invalid type");
      throw new ArgumentException ("object is not a ReasonSlot");
    }

    /// <summary>
    /// IComparable implementation
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual int CompareTo (IReasonSlot other)
    {
      if (other.Machine.Equals (this.Machine)) {
        return this.BeginDateTime.CompareTo (other.BeginDateTime);
      }

      log.Error ($"CompareTo: trying to compare reason slots for different machines {this} {other}");
      throw new ArgumentException ("Comparison of ReasonSlot from different machines");
    }


    /// <summary>
    /// Check if an early consolidate() call is required by SlotDAO
    /// to check the equality of the data reference
    /// </summary>
    /// <returns></returns>
    public override bool IsEarlyConsolidateRequiredForDataReference ()
    {
      return true;
    }

    /// <summary>
    /// <see cref="Slot.ReferenceDataEquals" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool ReferenceDataEquals (ISlot obj)
    {
      IReasonSlot other = obj as IReasonSlot;
      if (other == null) {
        return false;
      }

      Debug.Assert (null != this.Machine);
      Debug.Assert (null != other.Machine);

      if (!NHibernateHelper.EqualsNullable (this.MachineMode, other.MachineMode, (a, b) => a.Id == b.Id)
        || (this.Machine.Id != other.Machine.Id)
        || !NHibernateHelper.EqualsNullable (this.MachineObservationState, other.MachineObservationState, (a, b) => a.Id == b.Id)
        || !NHibernateHelper.EqualsNullable (this.Shift, other.Shift, (a, b) => a.Id == b.Id)
        || !this.ReasonSource.Equals (other.ReasonSource)) {
        return false;
      }

      // Note: they should be consolidated before calling ReferenceDataEquals in the Analysis process
      if (this.Consolidated && other.Consolidated) {
        return object.Equals (this.OverwriteRequired, other.OverwriteRequired)
          && NHibernateHelper.EqualsNullable (this.Reason, other.Reason, (a, b) => a.Id == b.Id)
          && object.Equals (this.ReasonDetails, other.ReasonDetails)
          && Pulse.Business.Reason.ReasonData.AreJsonEqual (this.JsonData, other.JsonData)
          && (this.ReasonScore == other.ReasonScore)
          && (this.ReasonSource.Equals (other.ReasonSource))
          && (this.AutoReasonNumber == other.AutoReasonNumber)
        // Note: the production state and production rate should not be here,
        // since it may prevent reason slots to be merged
        ;
      }
      else { // One of them is not consolidated
        if (log.IsWarnEnabled) {
          log.WarnFormat ("ReferenceDataEquals: one of the slot is not consolidated before calling ReferenceDataEquals");
        }
        return true;
      }
    }

    /// <summary>
    /// <see cref="Slot.IsEmpty" />
    /// </summary>
    /// <returns></returns>
    public override bool IsEmpty ()
    {
      Debug.Assert (null != this.Machine);

      if ((null == this.MachineMode)
          || (null == this.MachineObservationState)) {
        log.DebugFormat ("IsEmpty: ReasonSlot is empty because of " +
                         "MachineMode={0} " +
                         "or MachineObservationState={1}",
                         (null != this.MachineMode) ? this.MachineMode.Id : 0,
                         (null != this.MachineObservationState) ? this.MachineObservationState.Id : 0);
        return true;
      }

      if (this.DateTimeRange.IsEmpty ()) {
        log.ErrorFormat ("IsEmpty: ReasonSlot is empty because of its date/time range");
        return true;
      }

      if (this.DateTimeRange.Duration.HasValue
        && (0 == this.DateTimeRange.Duration.Value.Ticks)) {
        log.Error ($"IsEmpty: range {this.DateTimeRange} with an empty duration {this.DateTimeRange.Duration} {System.Environment.StackTrace}");
        return true;
      }

      return false;
    }

    /// <summary>
    /// Implementation of MachineSlot
    /// <see cref="Slot.Consolidate(ISlot)" />
    /// </summary>
    /// <param name="oldSlot"></param>
    /// <param name="association"></param>
    public override void Consolidate (ISlot oldSlot, IPeriodAssociation association)
    {
      Debug.Assert (null != this.Machine, "Machine null");
      Debug.Assert (null != this.MachineMode, "Machine mode null");
      Debug.Assert (this.EndDateTime.HasValue, "No end"); // Never oo
      Debug.Assert (this.EndDay.HasValue, "No end day");
      Debug.Assert (this.Duration.HasValue, "No duration");

      if (this.DateTimeRange.IsEmpty ()) {
        log.Error ($"Consolidate: empty date/time range {System.Environment.StackTrace}");
      }
      Debug.Assert (!this.DateTimeRange.IsEmpty ());

      if (!this.Consolidated) {
        using (var modificationTracker = new SlotModificationTracker<IReasonSlot> (this)) {
          if ((m_reason is null)
            || (!m_reasonSlotChange.IsEmpty ()
              && !m_reasonSlotChange.HasFlag (ReasonSlotChange.Reason))) {
            ResetReason ((IReasonSlot)oldSlot, association);
          }
          else if (m_reasonSlotChange.HasFlag (ReasonSlotChange.Period)) {
            var resetRequired = GetRequiredResetRequired ((IReasonSlot)oldSlot, association);
            if (resetRequired.HasFlag (RequiredResetKind.ExtraAuto)) { // Invalid the number of auto-reaons
              this.SetUnsafeAutoReasonNumber ();
            }
            if (resetRequired.HasFlag (RequiredResetKind.ExtraManual)) { // Invalid the extra-manual reason flag
              this.SetUnsafeManualFlag ();
            }
          }
          if (!m_reasonSlotChange.IsEmpty ()) {
            ConsolidateProductionStateRate ((IReasonSlot)oldSlot, association);
          }
          this.Consolidated = true;
        }
      }
    }

    public virtual void ConsolidateProductionStateRate (IReasonSlot oldSlot, IPeriodAssociation association)
    {
      if (m_reasonSlotChange.IsEmpty ()) {
        if (log.IsDebugEnabled) {
          log.Debug ($"ConsolidateProductionStateRate: no reason slot change => return");
        }
        return;
      }

      var productionStateExtensions = GetProductionStateExtensions ();
      if (!productionStateExtensions.Any ()) {
        return;
      }
      var oldEffectiveSlot = oldSlot ?? m_oldSlotFromModification;
      foreach (var productionStateExtension in productionStateExtensions) {
        try {
          if (productionStateExtension.ConsolidateProductionStateRate (oldEffectiveSlot, this, m_modification, m_reasonSlotChange)) {
            if (log.IsDebugEnabled) {
              log.Debug ($"ConsolidateProductionStateRate: true was returned");
            }
            return;
          }
        }
        catch (Exception ex) {
          log.Error ($"ConsolidateProductionStateRate: error in extension {productionStateExtension}", ex);
        }
      }
    }

    void ResetReason (IReasonSlot oldSlot, IPeriodAssociation association)
    {
      if ((null != this.Reason) && (this.Reason.Id == (int)ReasonId.Processing)) {
        if (m_reasonSlotChange.HasFlag (ReasonSlotChange.Period)) {
          this.ReasonSource = this.ReasonSource.SetUnsafeAutoReasonNumber ();
          this.ReasonSource = this.ReasonSource.SetUnsafeManualFlag ();
        }
        else if (m_reasonSlotChange.HasFlag (ReasonSlotChange.NewActivity)) {
          // This is normally not required, this code should not be reached
          // but do it though, it is safer
          Debug.Assert (false);
          this.ReasonSource = this.ReasonSource.SetUnsafeAutoReasonNumber ();
        }
        return;
      }

      if (null == this.Reason) {
        using (var modificationTracker = new SlotModificationTracker<IReasonSlot> (this)) {
          this.SetUnsafeManualFlag ();
          this.SetUnsafeAutoReasonNumber ();
          SwitchToProcessing ();
        }
        return;
      }

      var resetRequired = GetRequiredResetRequired (oldSlot, association);

      if (resetRequired.HasFlag (RequiredResetKind.ExtraAuto)) { // Invalid the number of auto-reaons
        this.SetUnsafeAutoReasonNumber ();
      }
      if (resetRequired.HasFlag (RequiredResetKind.ExtraManual)) { // Invalid the extra-manual reason flag
        this.SetUnsafeManualFlag ();
      }

      if (!resetRequired.HasFlag (RequiredResetKind.Main)) {
        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().DebugFormat ("ResetReason: reset is not requested, do not switch to processing");
        }
        return;
      }

      if (association is IReasonMachineAssociation) {
        var reasonMachineAssociation = (IReasonMachineAssociation)association;
        if (reasonMachineAssociation.Option.HasValue
          && reasonMachineAssociation.Option.Value.HasFlag (AssociationOption.FinalProcess)) {
          if (!m_reasonSlotChange.HasFlag (ReasonSlotChange.Period) && GetLogger ().IsErrorEnabled) {
            GetLogger ().Error ($"ResetReason: requested to switch to processing although it is the final process reasonSlotChange={m_reasonSlotChange} => do nothing instead. StackTrace={System.Environment.StackTrace}");
          }
          else {
            GetLogger ().Debug ($"ResetReason: requested to switch to processing (final process), reasonSlotChange={m_reasonSlotChange} => do nothing instead");
          }
          return;
        }
      }

      using (var modificationTracker = new SlotModificationTracker<IReasonSlot> (this)) {
        SwitchToProcessing ();
      }
    }

    RequiredResetKind GetRequiredResetRequired (IReasonSlot oldSlot, IPeriodAssociation association)
    {
      RequiredResetKind resetRequired = RequiredResetKind.None;
      if (m_reasonSlotChange.HasFlag (ReasonSlotChange.Requested)) {
        resetRequired = RequiredResetKind.Full;
      }
      else { // Not Requested
        if (m_reasonSlotChange.HasFlag (ReasonSlotChange.ResetManual)
          || (null == this.Reason)
          || (this.ReasonScore < 0.0)) {
          resetRequired = RequiredResetKind.Main;
        }
      }

      if (resetRequired != RequiredResetKind.Full) {
        // The full reset is not necessarily requested, check it in the extensions
        var resetReasonExtensions = GetReasonExtensions ();
        if (resetReasonExtensions.Any ()) {
          var oldEffectiveSlot = oldSlot ?? m_oldSlotFromModification;
          foreach (var resetReasonExtension in resetReasonExtensions) {
            resetRequired = resetRequired | resetReasonExtension.GetRequiredResetKind (oldEffectiveSlot, this, m_modification, m_reasonSlotChange);
            if (resetRequired == RequiredResetKind.Full) { // No need to check another extension
              break;
            }
          }
        }
      }

      return resetRequired;
    }

    /// <summary>
    /// Try to set a default reason in the reset process,
    /// meaning check it is applicable first with the reason score
    /// </summary>
    /// <param name="reason">not null</param>
    /// <param name="score"></param>
    /// <param name="overwriteRequired"></param>
    /// <param name="auto"></param>
    /// <param name="consolidationLimit"></param>
    /// <returns>the new main reason is the default one</returns>
    public virtual bool TryDefaultReasonInReset (IReason reason, double score, bool overwriteRequired, bool auto, UpperBound<DateTime> consolidationLimit)
    {
      Debug.Assert (null != reason);

      if ((0 == this.ReasonScore) || this.ReasonScore < score) {
        var oldReason = m_reason;
        m_reason = reason;
        m_reasonDetails = null;
        m_reasonScore = score;
        m_overwriteRequired = overwriteRequired;
        if (auto) {
          m_reasonSource = m_reasonSource
            .SetMainDefaultAuto ()
            .Add (ReasonSource.DefaultIsAuto);
          ++m_autoReasonNumber;
        }
        else {
          m_reasonSource = m_reasonSource.SetMainDefault ();
        }
        AddConsolidationLimit (consolidationLimit);
        if (!NHibernateHelper.EqualsNullable (m_reason, oldReason, (x, y) => x.Id == y.Id)) {
          m_reasonSlotChange = m_reasonSlotChange.Add (ReasonSlotChange.Reason);
        }
        return true;
      }
      else if (auto) {
        this.AddExtraAutoReason ();
        AddConsolidationLimit (consolidationLimit);
        this.ReasonSource = this.ReasonSource.Add (ReasonSource.DefaultIsAuto);
        return false;
      }
      else {
        return false;
      }
    }

    /// <summary>
    /// Update if applicable the default reason (which may be not valid any more)
    /// on a neighbor reason slot that is not processing
    /// </summary>
    /// <param name="machineModeDefaultReason"></param>
    /// <param name="consolidationLimit"></param>
    /// <returns>the default reason was applied and it the main one</returns>
    public virtual bool TryUpdateDefaultReason (IMachineModeDefaultReason machineModeDefaultReason, UpperBound<DateTime> consolidationLimit)
    {
      Debug.Assert (null != this.Reason);
      Debug.Assert (this.Reason.Id != (int)ReasonId.Processing);

      if ((this.ReasonScore < machineModeDefaultReason.Score)
        || ((this.ReasonScore == machineModeDefaultReason.Score)
            && (this.ReasonSource.IsDefault ()))) {
        if (this.ReasonSource.HasFlag (ReasonSource.Default)
          && this.ReasonSource.HasFlag (ReasonSource.Auto)) {
          // Previous main reason source was default+auto => it is not valid any more, remove an auto-reason number
          TryRemoveAutoReasonNumber ();
        }
        else if (this.ReasonSource.HasFlag (ReasonSource.DefaultIsAuto)) {
          TryRemoveAutoReasonNumber ();
        }
        this.SetDefaultReason (machineModeDefaultReason.Reason,
          machineModeDefaultReason.Score,
          machineModeDefaultReason.OverwriteRequired,
          machineModeDefaultReason.Auto,
          consolidationLimit);
        return true;
      }
      else if (this.ReasonSource.IsDefault ()) {
        if (this.ReasonSource.HasFlag (ReasonSource.Auto)) { // Cancel one auto-reason
          Debug.Assert (0 < this.AutoReasonNumber);
          Debug.Assert (this.ReasonSource.HasFlag (ReasonSource.DefaultIsAuto));
          if (0 < this.AutoReasonNumber) {
            this.RemoveExtraAutoReason ();
          }
          else {
            log.ErrorFormat ("TryResetDefaultReason: could not remove an extra auto-reason");
          }
        }
        this.SwitchToProcessing ();
        return false;
      }
      else { // Not applied and the previous one was not a default reason
        if (this.ReasonSource.HasFlag (ReasonSource.DefaultIsAuto)) {
          if (!machineModeDefaultReason.Auto) {
            TryRemoveAutoReasonNumber ();
            this.ReasonSource = this.ReasonSource.Remove (ReasonSource.DefaultIsAuto);
          }
        }
        else { // No DefautlIsAuto
          if (machineModeDefaultReason.Auto) {
            this.ReasonSource = this.ReasonSource.Add (ReasonSource.DefaultIsAuto);
            this.AddExtraAutoReason ();
          }
        }
        return false;
      }
    }

    void TryRemoveAutoReasonNumber ()
    {
      Debug.Assert (0 < this.AutoReasonNumber);
      if (0 < this.AutoReasonNumber) {
        --m_autoReasonNumber;
      }
      else {
        log.ErrorFormat ("TryRemoveAutoReasonNumber: could not remove an auto-reason number because its number is {0}", this.AutoReasonNumber);
      }
    }

    /// <summary>
    /// Try to set a manual reason in the reset process,
    /// meaning check it is applicable first with the reason score
    /// </summary>
    /// <param name="reason">not null</param>
    /// <param name="score"></param>
    /// <param name="details"></param>
    /// <param name="consolidationLimit"></param>
    /// <returns>a manual reason was applied</returns>
    public virtual bool TryManualReasonInReset (IReason reason, double score, string details, UpperBound<DateTime> consolidationLimit)
    {
      Debug.Assert (null != reason);

      AddConsolidationLimit (consolidationLimit);
      if (this.ReasonScore <= score) {
        var oldReason = m_reason;
        m_reason = reason;
        m_reasonDetails = details;
        m_reasonScore = score;
        m_overwriteRequired = false;
        m_reasonSource = ReasonSource.Manual;
        if (!NHibernateHelper.EqualsNullable (oldReason, reason, (x, y) => x.Id == y.Id)) {
          m_reasonSlotChange = m_reasonSlotChange.Add (ReasonSlotChange.Reason);
        }
        return true;
      }
      else { // Register it as an additional manual reason
        this.AddExtraManualReason ();
        return false;
      }
    }

    /// <summary>
    /// Try to set an auto reason in the reset process,
    /// meaning check it is applicable first with the reason score
    /// after checking the reason is compatible in reasonSlot
    /// </summary>
    /// <param name="reason">not null</param>
    /// <param name="score"></param>
    /// <param name="details"></param>
    /// <param name="overwriteRequired"></param>
    /// <param name="consolidationLimit"></param>
    /// <param name="compatibilityCheck"></param>
    /// <returns>a reason was applied</returns>
    public virtual bool TryAutoReasonInReset (IReason reason, double score, string details, bool overwriteRequired, UpperBound<DateTime> consolidationLimit, bool compatibilityCheck)
    {
      Debug.Assert (null != reason);

      if (compatibilityCheck) {
        var reasonCompatibilityExtensions = GetReasonExtensions ();
        if (!reasonCompatibilityExtensions.Any (ext => ext.IsCompatible (this.DateTimeRange, this.MachineMode, this.MachineObservationState, reason, score, ReasonSource.Auto))) {
          log.Info ($"TryAutoReasonInReset: auto reason {reason} is not compatible with the reason slot {this} => skip it");
          return false;
        }
      }

      AddConsolidationLimit (consolidationLimit);
      if (this.ReasonScore < score) { // Main for the moment
        var oldReason = m_reason;
        m_reason = reason;
        m_reasonScore = score;
        m_reasonDetails = details;
        m_overwriteRequired = overwriteRequired;
        m_reasonSource = m_reasonSource.SetMainAuto ();
        ++m_autoReasonNumber;
        if (!NHibernateHelper.EqualsNullable (m_reason, oldReason, (x, y) => x.Id == y.Id)) {
          m_reasonSlotChange = m_reasonSlotChange.Add (ReasonSlotChange.Reason);
        }
        return true;
      }
      else { // Register it as an additional auto reason
        this.AddExtraAutoReason ();
        return false;
      }
    }

    /// <summary>
    /// <see cref="IReasonSlot"/>
    /// </summary>
    public virtual void UpdateMachineStatusIfApplicable ()
    {
      Debug.Assert (null != this.Reason);

      var machineStatus = Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.MachineStatusDAO
        .FindById (this.Machine.Id);
      if ((null != machineStatus)
        && (Bound.Compare<DateTime> (machineStatus.ReasonSlotEnd, this.DateTimeRange.Upper) <= 0)) {
        machineStatus.Reason = this.Reason;
        machineStatus.ReasonDetails = this.ReasonDetails;
        machineStatus.ReasonScore = this.ReasonScore;
        machineStatus.ReasonSource = this.ReasonSource;
        machineStatus.MachineMode = this.MachineMode;
        machineStatus.MachineObservationState = this.MachineObservationState;
        machineStatus.AutoReasonNumber = this.AutoReasonNumber;
        machineStatus.OverwriteRequired = this.OverwriteRequired;
        machineStatus.ReasonSlotEnd = this.DateTimeRange.Upper.Value;
        if (this.ConsolidationLimit.HasValue
          && (Bound.Compare<DateTime> (this.ConsolidationLimit, this.DateTimeRange.Upper) < 0)) {
          Debug.Assert (false);
          log.Fatal ("UpdateMachineStatusIfApplicable: not a valid consolidation limit (before DateTimeRange.Upper) => invalid it");
          machineStatus.ConsolidationLimit = this.DateTimeRange.Upper.Value;
        }
        else {
          machineStatus.ConsolidationLimit = this.ConsolidationLimit;
        }
        ModelDAOHelper.DAOFactory.MachineStatusDAO.MakePersistent (machineStatus);
      }
    }

    /// <summary>
    /// Action to take after the slot is inserted
    /// </summary>
    public override void HandleAddedSlot ()
    {
      if (this.DateTimeRange.IsEmpty ()) {
        log.Error ($"HandleAddedSlot: empty range {System.Environment.StackTrace}");
      }
      Debug.Assert (!this.DateTimeRange.IsEmpty ());

      Debug.Assert (this.EndDateTime.HasValue);
      AnalysisAccumulator.AddReasonSlot (this);

      UpdateMachineStatusIfApplicable ();
    }

    /// <summary>
    /// <see cref="Slot.HandleModifiedSlot" />
    /// </summary>
    /// <param name="oldSlot"></param>
    public override void HandleModifiedSlot (ISlot oldSlot)
    {
      if (this.DateTimeRange.IsEmpty () && !this.FlagRemoved) {
        log.Error ($"HandleModifiedSlot: empty range. StackTrace: {System.Environment.StackTrace}");
      }
      Debug.Assert (!this.DateTimeRange.IsEmpty ());

      if (oldSlot is ReasonSlot) {
        ReasonSlot oldReasonSlot = oldSlot as ReasonSlot;
        Debug.Assert (null != oldReasonSlot);

        if (this.FlagRemoved) {
          oldReasonSlot.HandleRemovedSlot ();
          return;
        }

        if (!m_reasonSlotChange.IsEmpty () && (null != this.Reason) && ((int)ReasonId.Processing != this.Reason.Id)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"HandleModifiedSlot: reason slot changes => consolidate the production state and rate");
          }
          ConsolidateProductionStateRate (oldReasonSlot, null);
        }

        if (this.ReferenceDataEquals (oldReasonSlot)
            && object.Equals (oldReasonSlot.Reason, this.Reason)
            && NHibernateHelper.EqualsNullable (oldReasonSlot.ProductionState, this.ProductionState, (x, y) => x.Id == y.Id)
            && object.Equals (oldReasonSlot.ProductionRate, this.ProductionRate)) {
          // only the period of time changed => optimization

          // this:    xx.....
          // old:         xx......
          // process: xx..
          if (Bound.Compare<DateTime> (this.BeginDateTime, oldReasonSlot.BeginDateTime) < 0) {
            UpperBound<DateTime> end = UpperBound.GetMinimum<DateTime> (this.EndDateTime, oldReasonSlot.BeginDateTime.Value);
            // Add this period to the impacted tables
            AnalysisAccumulator.AddReasonSlotPeriod (this, oldReasonSlot,
                                                     new UtcDateTimeRange (this.BeginDateTime, end));
          }

          // this:        xxx....
          // old:     xx......
          // process: xx..
          if (Bound.Compare<DateTime> (oldReasonSlot.BeginDateTime, this.BeginDateTime) < 0) {
            Debug.Assert (this.BeginDateTime.HasValue);
            UpperBound<DateTime> end = UpperBound
              .GetMinimum<DateTime> (oldReasonSlot.EndDateTime,
                                     this.BeginDateTime.Value);
            // Remove this period to the impacted summary tables
            AnalysisAccumulator.RemoveReasonSlotPeriod (oldReasonSlot, oldReasonSlot.BeginDateTime, end);
          }

          // Intersection case
          // this:    xxxx    xx
          // old:      xx    xxxxx
          // process:  xx     xx
          // => nothing to do because the reference data is equal

          // this:    ........xxxx
          // old:         xx
          // process:       ..xxxx
          if (NullableDateTime.Compare (oldReasonSlot.EndDateTime, this.EndDateTime) < 0) {
            Debug.Assert (oldReasonSlot.EndDateTime.HasValue);
            LowerBound<DateTime> begin = LowerBound.GetMaximum<DateTime> (this.BeginDateTime, oldReasonSlot.EndDateTime.Value);
            AnalysisAccumulator.AddReasonSlotPeriod (this, oldReasonSlot,
                                                     new UtcDateTimeRange (begin, this.EndDateTime));
          }

          // this:     xx
          // old:     .....xx
          // process:    ..xx
          if (NullableDateTime.Compare (this.EndDateTime, oldReasonSlot.EndDateTime) < 0) {
            Debug.Assert (this.EndDateTime.HasValue);
            LowerBound<DateTime> begin = LowerBound.GetMaximum<DateTime> (oldReasonSlot.BeginDateTime,
                                                                          this.EndDateTime.Value);
            // Remove this period from the impacted summary tables
            AnalysisAccumulator.RemoveReasonSlotPeriod (oldReasonSlot, begin, oldReasonSlot.EndDateTime);
          }
        }
        else { // The reason or machine observation state changed => no optimization is possible
          AnalysisAccumulator.RemoveReasonSlot (oldReasonSlot);
          AnalysisAccumulator.AddReasonSlot (this);
        }
        UpdateMachineStatusIfApplicable ();
      }
      else {
        Debug.Assert (false);
        GetLogger ().Fatal ($"HandleModifiedSlot: unexpected slot type {oldSlot?.GetType ().ToString () ?? ""}");
        throw new ArgumentException ("Not supported slot");
      }
    }

    /// <summary>
    /// <see cref="Slot.HandleRemovedSlot" />
    /// </summary>
    public override void HandleRemovedSlot ()
    {
      Debug.Assert (this.EndDateTime.HasValue);

      if (0 == this.Id) {
        if (log.IsDebugEnabled) {
          log.Debug ($".{this.Machine.Id} HandleRemovedSlot: id is 0, do nothing");
        }
        return;
      }

      AnalysisAccumulator.RemoveReasonSlot (this);
    }

    /// <summary>
    /// Set a new date/time range
    /// </summary>
    /// <param name="newRange">not empty</param>
    public override void UpdateDateTimeRange (UtcDateTimeRange newRange)
    {
      if (newRange.IsEmpty ()) {
        log.Error ($"UpdateDateTimeRange: empty date/time range. StackTrace: {System.Environment.StackTrace}");
      }
      Debug.Assert (!newRange.IsEmpty ());

      if (newRange.Duration.HasValue && (0 == newRange.Duration.Value.Ticks)) {
        log.ErrorFormat ("UpdateDateTimeRange: " +
                         "range {0} with an empty duration {1}. " +
                         "StackTrace: {2}",
                         newRange,
                         newRange.Duration,
                         System.Environment.StackTrace);
      }

      if (this.ConsolidationLimit.HasValue
        && (Bound.Compare<DateTime> (this.ConsolidationLimit.Value, newRange.Upper) < 0)) {
        Debug.Assert (false);
        log.FatalFormat ("UpdateDateTimeRange: invalid consolidation limit {0} VS new range {1}. StackTrace={2}",
          this.ConsolidationLimit, newRange, System.Environment.StackTrace);
        m_consolidationLimit = newRange.Upper;
      }

      if (!this.DateTimeRange.Equals (newRange)) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("UpdateDateTimeRange: change from {0} to {1} for id={2}",
            this.DateTimeRange, newRange, this.Id);
        }

        base.UpdateDateTimeRange (newRange);

        // A change of the date/time range invalids the reason
        m_reasonSlotChange = m_reasonSlotChange.Add (ReasonSlotChange.Period);
      }
    }

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[ReasonSlot {this.Id} {this.Machine?.ToStringIfInitialized ()} Range={this.DateTimeRange}]";
      }
      else {
        return $"[ReasonSlot {this.Id}]";
      }
    }
  }
}
