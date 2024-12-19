// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;
using Lemoine.Business;
using Lemoine.Business.Reason;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;
using System.Globalization;
using Lemoine.Extensions.Database;
using Lemoine.Extensions.Business.DynamicTimes;
using Lemoine.Business.DynamicTimes;
using Lemoine.Collections;
using Pulse.Extensions.Database;
using Pulse.Business.Reason;
using System.Text.Json;
using Lemoine.Business.Extension;
using Pulse.Extensions.Business.Reason;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table ReasonMachineAssociation
  /// 
  /// This new table is designed to add any reason change to a machine.
  /// This is required since the new Fact table now records
  /// only the raw data from the CNC.
  /// 
  /// It does not represent the current reasons of a machine,
  /// but all the manual or automatic reason changes that have been made.
  /// 
  /// To know the current reasons of a machine, the table Reason Slot
  /// that is filled in by the Analyzer must be used.
  /// </summary>
  [Serializable]
  public class ReasonMachineAssociation : MachineAssociation, IReasonMachineAssociation
  {
    /// <summary>
    /// Default manual reason score
    /// </summary>
    static readonly string DEFAULT_MANUAL_SCORE_KEY = "Reason.DefaultManualScore";
    static readonly double DEFAULT_MANUAL_SCORE_DEFAULT = 100.0;

    /// <summary>
    /// Cancel only the data in case of TrackDynamicEnd
    /// </summary>
    static readonly string CANCEL_DATA_TRACK_DYNAMIC_END_ONLY_KEY = "ReasonMachineAssociation.CancelDataTrackDynamicEndOnly";
    static readonly bool CANCEL_DATA_TRACK_DYNAMIC_END_ONLY_DEFAULT = true;

    static readonly string CANCEL_RESET_REASON_SLOTS_IN_SUB_MODIFICATION_KEY = "ReasonMachineAssociation.CancelResetReasonSlotsInSubModification";
    static readonly bool CANCEL_RESET_REASON_SLOTS_IN_SUB_MODIFICATION_DEFAULT = true;

    static readonly string CANCEL_REASON_SLOTS_STEP_KEY = "ReasonMachineAssociation.CancelReasonSlots.Step";
    static readonly TimeSpan CANCEL_REASON_SLOTS_STEP_DEFAULT = TimeSpan.FromHours (8);

    static readonly string DYNAMIC_END_TRACKER_PRIORITY_KEY =
      "ReasonMachineAssociation.DynamicEndTracker.Priority";
    static readonly int DYNAMIC_END_TRACKER_PRIORITY_DEFAULT = 10;

    IReason m_reason = null;
    string m_reasonDetails = null;
    double? m_optionalReasonScore = null;
    string m_jsonData = null;
    ReasonMachineAssociationKind m_kind = ReasonMachineAssociationKind.Consolidate;
    bool m_dataCancelled = false;

    IEnumerable<IReasonExtension> m_reasonExtensions;
    IEnumerable<IReasonSelectionExtension> m_reasonSelectionExtensions;

    /// <summary>
    /// UTC date/time range of the association
    /// </summary>
    [XmlIgnore]
    public override UtcDateTimeRange Range
    {
      get {
        if (!string.IsNullOrEmpty (this.Dynamic) && this.Dynamic.EndsWith ("+")) {
          return new UtcDateTimeRange (this.Begin);
        }
        else {
          return base.Range;
        }
      }
    }

    /// <summary>
    /// Range to use to clone a reason machine association
    /// </summary>
    [XmlIgnore]
    UtcDateTimeRange CloneRange
    {
      get {
        return base.Range;
      }
    }

    /// <summary>
    /// Range to use for the dynamic time computation
    /// </summary>
    [XmlIgnore]
    UtcDateTimeRange DynamicTimeRange
    {
      get {
        if (!string.IsNullOrEmpty (this.Dynamic) && this.Dynamic.EndsWith ("+")) {
          return new UtcDateTimeRange (this.Begin, this.End, "[]");
        }
        else {
          return base.Range;
        }
      }
    }

    /// <summary>
    /// <see cref="IModification"/>
    /// </summary>
    [XmlIgnore]
    public override string ModificationType
    {
      get { return "ReasonMachineAssociation"; }
    }

    /// <summary>
    /// Reference to the Reason
    /// </summary>
    [XmlIgnore]
    public virtual IReason Reason
    {
      get { return m_reason; }
    }

    /// <summary>
    /// Data in Json format
    /// </summary>
    [XmlIgnore]
    public virtual string JsonData => m_jsonData;

    /// <summary>
    /// <see cref="IReasonMachineAssociation"/>
    /// </summary>
    [XmlIgnore]
    public virtual IDictionary<string, object> Data
    {
      get => Pulse.Business.Reason.ReasonData.Deserialize (m_jsonData);
      set {
        m_jsonData = JsonSerializer.Serialize (value);
      }
    }

    /// <summary>
    /// Set a manual reason
    /// </summary>
    /// <param name="reason">not null</param>
    /// <param name="reasonScore"></param>
    /// <param name="details"></param>
    public virtual void SetManualReason (IReason reason, double? reasonScore, string details = null, string jsonData = null)
    {
      double score;
      if (reasonScore.HasValue) {
        score = reasonScore.Value;
      }
      else {
        score = Lemoine.Info.ConfigSet.LoadAndGet<double> (DEFAULT_MANUAL_SCORE_KEY, DEFAULT_MANUAL_SCORE_DEFAULT);
      }
      SetReason (reason, ReasonMachineAssociationKind.Manual, score, details, jsonData);
    }

    /// <summary>
    /// Set a manual reason
    /// </summary>
    /// <param name="reason">not null</param>
    /// <param name="details"></param>
    public virtual void SetManualReason (IReason reason, string details, string jsonData = null)
    {
      double score = Lemoine.Info.ConfigSet.LoadAndGet<double> (DEFAULT_MANUAL_SCORE_KEY, DEFAULT_MANUAL_SCORE_DEFAULT);
      SetReason (reason, ReasonMachineAssociationKind.Manual, score, details, jsonData);
    }

    /// <summary>
    /// Reset a manual reason
    /// </summary>
    public virtual void ResetManualReason ()
    {
      m_reason = null;
      m_kind = ReasonMachineAssociationKind.Manual;
      double score = Lemoine.Info.ConfigSet.LoadAndGet<double> (DEFAULT_MANUAL_SCORE_KEY, DEFAULT_MANUAL_SCORE_DEFAULT);
      m_optionalReasonScore = score;
      m_reasonDetails = null;
      // TODO: jsonData : check with the extension
    }

    /// <summary>
    /// Set an auto reason
    /// </summary>
    /// <param name="reason">not null</param>
    /// <param name="score"></param>
    /// <param name="overwriteRequired"></param>
    /// <param name="details"></param>
    public virtual void SetAutoReason (IReason reason, double score, bool overwriteRequired, string details = null, string jsonData = null)
    {
      SetReason (reason,
        overwriteRequired ? ReasonMachineAssociationKind.AutoWithOverwriteRequired : ReasonMachineAssociationKind.Auto,
        score, details, jsonData);
    }

    /// <summary>
    /// Set a specific reason
    /// </summary>
    /// <param name="reason">not null</param>
    /// <param name="kind">not Consolidate</param>
    /// <param name="score"></param>
    /// <param name="details"></param>
    internal protected virtual void SetReason (IReason reason, ReasonMachineAssociationKind kind, double score, string details, string jsonData = null)
    {
      Debug.Assert (null != reason);
      Debug.Assert (ReasonMachineAssociationKind.Consolidate != kind);

      m_reason = reason;
      m_kind = kind;
      m_optionalReasonScore = score;
      m_reasonDetails = details;
      m_jsonData = jsonData;
    }

    /// <summary>
    /// Consider this reason machine association is of kind TrackDynamicEnd
    /// </summary>
    /// <param name="dynamicEnd"></param>
    /// <param name="hint">not empty</param>
    public virtual void SetDynamicEndTracker (string dynamicEnd, UtcDateTimeRange hint)
    {
      Debug.Assert (!hint.IsEmpty ());
      m_kind = this.Kind.Add (ReasonMachineAssociationKind.TrackDynamicEnd);
      this.Dynamic = "," + dynamicEnd;
    }

    /// <summary>
    /// Reference to the Reason for Xml Serialization
    /// </summary>
    [XmlElement ("Reason")]
    public virtual Reason XmlSerializationReason
    {
      get { return this.Reason as Reason; }
      set { m_reason = value; }
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
    /// Possible reason score
    /// </summary>
    [XmlIgnore]
    public virtual double? OptionalReasonScore
    {
      get { return m_optionalReasonScore; }
    }

    /// <summary>
    /// Reason score
    /// </summary>
    [XmlIgnore]
    public virtual double ReasonScore
    {
      get { return m_optionalReasonScore ?? -1.0; }
    }

    /// <summary>
    /// Reason score for XML serialization
    /// </summary>
    [XmlAttribute ("ReasonScore")]
    public virtual string XmlSerializationReasonScore
    {
      get { return m_optionalReasonScore.HasValue ? m_optionalReasonScore.Value.ToString (CultureInfo.InvariantCulture) : ""; }
      set {
        if (string.IsNullOrEmpty (value)) {
          m_optionalReasonScore = null;
        }
        else {
          m_optionalReasonScore = double.Parse (value, CultureInfo.InvariantCulture);
        }
      }
    }

    /// <summary>
    /// Reason machine association kind
    /// </summary>
    [XmlIgnore]
    public virtual ReasonMachineAssociationKind Kind
    {
      get { return m_kind; }
      set { m_kind = value; }
    }

    /// <summary>
    /// IPossibleReason implementation
    /// </summary>
    [XmlIgnore]
    public virtual bool OverwriteRequired
    {
      get {
        return m_kind.IsOvewriteRequired ();
      }
    }

    /// <summary>
    /// IPossibleReason implementation
    /// </summary>
    [XmlIgnore]
    public virtual ReasonSource ReasonSource
    {
      get {
        return m_kind.ConvertToReasonSource ();
      }
    }

    /// <summary>
    /// <see cref="IReasonProposal"/>
    /// </summary>
    [XmlIgnore]
    public virtual UtcDateTimeRange RestrictedRange
    {
      get { return this.Range; }
    }

    /// <summary>
    /// <see cref="IReasonProposal"/>
    /// </summary>
    [XmlIgnore]
    public virtual IMachineMode RestrictedMachineMode
    {
      get { return null; }
    }

    /// <summary>
    /// <see cref="IReasonProposal"/>
    /// </summary>
    [XmlIgnore]
    public virtual IMachineObservationState RestrictedMachineObservationState
    {
      get { return null; }
    }

    #region Constructors
    /// <summary>
    /// Constructor (used in test)
    /// </summary>
    /// <returns></returns>
    public ReasonMachineAssociation () : base () { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range">not empty</param>
    internal protected ReasonMachineAssociation (IMachine machine, UtcDateTimeRange range)
      : base (machine, range)
    {
      log = LogManager.GetLogger (string.Format ("{0}.{1}",
                                                this.GetType ().FullName,
                                                machine.Id));
    }

    /// <summary>
    /// Specific constructor when begin may be equal to end
    /// because of the use of dynamic times
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    /// <param name="dynamic">dynamic times, not null or empty</param>
    internal protected ReasonMachineAssociation (IMachine machine, LowerBound<DateTime> begin, UpperBound<DateTime> end, string dynamic)
      : base (machine, begin, end, dynamic)
    {
      log = LogManager.GetLogger (string.Format ("{0}.{1}",
                                                this.GetType ().FullName,
                                                machine.Id));
    }

    /// <summary>
    /// Constructor for abstract modifications, that are kept transient
    /// 
    /// You may add a mainModification reference to be used in the analysis logs
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="mainModification"></param>
    ReasonMachineAssociation (IMachine machine, UtcDateTimeRange range, IModification mainModification)
      : base (machine, range, mainModification)
    {
      log = LogManager.GetLogger (string.Format ("{0}.{1}",
                                                this.GetType ().FullName,
                                                machine.Id));
    }
    #endregion // Constructors

    #region MachineAssociation implementation
    /// <summary>
    /// <see cref="MachineAssociation"/>
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    protected override bool IsStepActive (UtcDateTimeRange range)
    {
      return base.IsStepActive (range);
    }

    /// <summary>
    /// <see cref="PeriodAssociation.ConvertToSlot" />
    /// </summary>
    /// <returns></returns>
    public override TSlot ConvertToSlot<TSlot> ()
    {
      var slot =
        GenericMachineRangeSlot.Create (typeof (TSlot), this.Machine, this.Range) as TSlot;
      slot.Consolidated = false;

      if (slot is IReasonSlot) {
        // Activity detection is responsible for the creation
        // of the slot
        var reasonSlot =
          slot as IReasonSlot;
        Debug.Assert (reasonSlot.IsEmpty ());
        return slot;
      }
      else {
        System.Diagnostics.Debug.Assert (false);
        throw new NotImplementedException ("Slot type not implemented");
      }
    }

    /// <summary>
    /// <see cref="PeriodAssociation.MergeDataWithOldSlot" />
    /// </summary>
    /// <param name="oldSlot"></param>
    /// <param name="range">merge period range</param>
    /// <returns></returns>
    public override TSlot MergeDataWithOldSlot<TSlot> (TSlot oldSlot,
                                                      UtcDateTimeRange range)
    {
      Debug.Assert (null != oldSlot);
      // disable once CSharpWarnings::CS0183
      Debug.Assert (oldSlot is Slot);
      Debug.Assert (this.Machine.Equals ((oldSlot as IPartitionedByMachine).Machine));

      if (!(oldSlot is IReasonSlot)) {
        System.Diagnostics.Debug.Assert (false);
        GetLogger ().Fatal ($"MergeData: trying to merge the association with a not supported slot {typeof (TSlot)}");
        Debug.Assert (false);
        throw new ArgumentException ("Not supported machine slot");
      }

      IReasonSlot oldReasonSlot = oldSlot as IReasonSlot;
      ReasonSlot newReasonSlot = (ReasonSlot)oldReasonSlot.Clone ();
      newReasonSlot.SetOldSlotFromModification (oldReasonSlot, this);
      if (this.Reason is null) { // Reset the reason
        switch (this.Kind) {
          case ReasonMachineAssociationKind.Consolidate: // Will be deprecated soon...
            newReasonSlot.SetUnsafeAutoReasonNumber ();
            newReasonSlot.SetUnsafeManualFlag ();
            newReasonSlot.SwitchToProcessing ();
            return newReasonSlot as TSlot;
          case ReasonMachineAssociationKind.Manual:
            ((ReasonSlot)newReasonSlot).ResetManualReason ();
            return newReasonSlot as TSlot;
          default:
            GetLogger ().Fatal ($"MergeDataWithOldSlot: reason null with an invalid kind {this.Kind}");
            Debug.Assert (false);
            return newReasonSlot as TSlot;
        }
      }

      if (!this.Option.HasValue
        || !this.Option.Value.HasFlag (AssociationOption.NoCompatibilityCheck)) {
        // null != this.Reason
        // A real reason was entered
        // Check the new reason is compatible with the
        // MachineObservationState, else raise an error and skip the reason
        var reasonExtensions = GetReasonExtensions ();
        var isCompatible = reasonExtensions
          .Any (ext => ext.IsCompatible (range, oldReasonSlot.MachineMode, oldReasonSlot.MachineObservationState, this.Reason, this.ReasonScore, this.ReasonSource));
        if (!isCompatible) {
          string message = $"The new reason {this.Reason?.Id} is not compatible with the MachineObservationState {oldReasonSlot.MachineObservationState?.Id} and MachineMode {oldReasonSlot.MachineMode?.Id} considering {reasonExtensions.Count ()} reason extensions";
          if (this.ReasonSource.HasFlag (ReasonSource.Manual)) {
            // For the reason slots in the future, this is a warning,
            // else this is an error
            Debug.Assert (oldReasonSlot.EndDateTime.HasValue);
            if (oldReasonSlot.EndDateTime.HasValue
                && (Bound.Compare<DateTime> (this.DateTime, oldReasonSlot.EndDateTime.Value) < 0)) {
              GetLogger ().Warn ($"MergeDataWithOldSlot: {message}");
              AddAnalysisLog (LogLevel.WARN, message);
            }
            else {
              GetLogger ().Error ($"MergeDataWithOldSlot: {message}");
              AddAnalysisLog (LogLevel.ERROR, message);
            }
          }
          return newReasonSlot as TSlot;
        }
      }

      if (oldReasonSlot.Id == (int)ReasonId.Processing) { // Don't change it right now if not PostProcessing
        if (!this.Option.HasValue
           || !this.Option.Value.HasFlag (AssociationOption.FinalProcess)) {
          // keep the Processing reason
          // but update the some manual / auto flags if required
          if (this.Kind.IsAuto ()) {
            ((ReasonSlot)newReasonSlot).AddExtraAutoReason ();
          }
          else if (this.Kind.IsManual ()) { // manual
            if (!newReasonSlot.ReasonSource.HasFlag (ReasonSource.Manual)) {
              ((ReasonSlot)newReasonSlot).AddExtraManualReason ();
            }
          }
        ((ReasonSlot)newReasonSlot).AddConsolidationLimit (this.End);
          return newReasonSlot as TSlot;
        }
      }

      bool apply = (oldReasonSlot.ReasonScore < this.OptionalReasonScore)
        || (oldReasonSlot.ReasonScore.Equals (this.OptionalReasonScore) && this.Kind.IsManual ());
      if (!apply) { // Not a higher score, skip it
        if (oldReasonSlot.ReasonSource.Equals (ReasonSource.Default)) {
          var message = $"association with a score {this.OptionalReasonScore} lower than the current default reason {oldReasonSlot.ReasonScore}";
          AddAnalysisLog (LogLevel.ERROR, message);
        }
        if (this.Kind.IsAuto ()) {
          ((ReasonSlot)newReasonSlot).AddExtraAutoReason ();
        }
        else if (this.Kind.IsManual ()) { // manual
          ((ReasonSlot)newReasonSlot).AddExtraManualReason ();
        }
        ((ReasonSlot)newReasonSlot).AddConsolidationLimit (this.End);
        return newReasonSlot as TSlot;
      }

      // This has a higher score
      double score;
      Debug.Assert (this.OptionalReasonScore.HasValue);
      if (this.OptionalReasonScore.HasValue) {
        score = this.OptionalReasonScore.Value;
      }
      else {
        score = Lemoine.Info.ConfigSet.LoadAndGet<double> (DEFAULT_MANUAL_SCORE_KEY, DEFAULT_MANUAL_SCORE_DEFAULT);
        GetLogger ().ErrorFormat ("ReasonMachineAssociation: association with a null reason score while reason id is {0} fallback to {1}", this.Reason.Id, score);
      }

      // Data
      string newJsonData;
      if (!string.IsNullOrEmpty (this.JsonData)) {
        var extensionRequest = new GlobalExtensions<IReasonDataExtension> (x => x.Initialize ());
        var extensions = Lemoine.Business.ServiceProvider.Get (extensionRequest);
        if (extensions.Any (x => x.DoMerge (oldReasonSlot, this) || !x.Keep (oldReasonSlot, this))) {
          var data = Pulse.Business.Reason.ReasonData.Deserialize (oldReasonSlot.JsonData, extensions);
          data = data.ToDictionary (x => x.Key, x => x.Value); // Clone it
          var newData = Pulse.Business.Reason.ReasonData.Deserialize (this.JsonData, extensions);
          foreach (var extension in extensions.Where (ext => ext.DoMerge (oldReasonSlot, this) || !ext.Keep (oldReasonSlot, this))) {
            var name = extension.Name;
            if (newData.TryGetValue (name, out var o)) {
              extension.Merge (data, o, this);
            }
            else if (!extension.Keep (oldReasonSlot, this)) { // Do not keep the old value
              data.Remove (name);
            }
          }
          newJsonData = JsonSerializer.Serialize (data);
          if (log.IsDebugEnabled) {
            log.Debug ($"MergeDataWithOldSlot: merged data is {newJsonData}");
          }
        }
        else { // No data to merge
          if (log.IsDebugEnabled) {
            log.Debug ($"MergeDataWithOldSlot: no IReasonDataExtension with a Merge => keep the data from ReasonMachineAssociation");
          }
          newJsonData = this.JsonData;
        }
      }
      else { // this.JsonData is null or empty => remove it?
        var extensionRequest = new GlobalExtensions<IReasonDataExtension> (x => x.Initialize ());
        var extensions = Lemoine.Business.ServiceProvider.Get (extensionRequest);
        if (extensions.Any (x => !x.Keep (oldReasonSlot, this))) {
          var data = Pulse.Business.Reason.ReasonData.Deserialize (oldReasonSlot.JsonData, extensions);
          data = data.ToDictionary (x => x.Key, x => x.Value); // Clone it
          bool change = false;
          foreach (var extension in extensions.Where (ext => !ext.Keep (oldReasonSlot, this))) {
            var name = extension.Name;
            if (data.ContainsKey (name)) {
              data.Remove (name);
              change = true;
            }
          }
          if (change) {
            newJsonData = JsonSerializer.Serialize (data);
            if (log.IsDebugEnabled) {
              log.Debug ($"MergeDataWithOldSlot: data is {newJsonData} after purging the data not to keep");
            }
          }
          else {
            newJsonData = oldReasonSlot.JsonData;
          }
        }
        else { // Keep all the data
          newJsonData = oldReasonSlot.JsonData;
        }
      }

      if (this.Kind.IsAuto ()) {
        ((ReasonSlot)newReasonSlot).SetMainAutoReason (this.Reason, score, this.Kind.Equals (ReasonMachineAssociationKind.AutoWithOverwriteRequired), this.End, this.ReasonDetails, newJsonData);
      }
      else { // manual
        ((ReasonSlot)newReasonSlot).SetManualReason (this.Reason, score, this.End, this.ReasonDetails, newJsonData);
      }

      return newReasonSlot as TSlot;
    }
    #endregion // MachineAssociation implementation

    #region Modification implementation
    void SetModificationInError (string message)
    {
      GetLogger ().ErrorFormat ("SetModificationInError: " +
        "{0} => finish in error",
        message);
      AddAnalysisLog (LogLevel.ERROR, message);
      MarkAsError ();
    }

    /// <summary>
    /// <see cref="Modification.Cancel"/>
    /// </summary>
    public override void Cancel ()
    {
      var cancelDataTrackDynamicEndOnly = Lemoine.Info.ConfigSet
        .LoadAndGet (CANCEL_DATA_TRACK_DYNAMIC_END_ONLY_KEY,
          CANCEL_DATA_TRACK_DYNAMIC_END_ONLY_DEFAULT);
      if (!cancelDataTrackDynamicEndOnly || this.Kind.HasFlag (ReasonMachineAssociationKind.TrackDynamicEnd)) {
        if (!m_dataCancelled) {
          CancelData ();
        }
      }
      base.Cancel ();
    }

    /// <summary>
    /// In case the analysis status is Timeout, should the modification switch to the status TimeoutCancelled ?
    /// 
    /// If TrackDynamicEnd, do not cancel it since it can cause some problems with the data
    /// </summary>
    /// <returns></returns>
    public override bool CancelAfterTimeout ()
    {
      return !this.Kind.HasFlag (ReasonMachineAssociationKind.TrackDynamicEnd);
    }

    /// <summary>
    /// Make the analysis
    /// </summary>
    public override void MakeAnalysis ()
    {
      Debug.Assert (!IsMainModificationTransient ());
      Debug.Assert (!IsAnalysisCompleted ());

      if (this.Kind.HasFlag (ReasonMachineAssociationKind.Reset)) {
        MakeAnalysisReset ();
        return;
      }

      if (this.Kind.HasFlag (ReasonMachineAssociationKind.TrackDynamicEnd)) {
        TrackDynamicEnd ();
        return;
      }

      if (Bound.Compare<DateTime> (this.End, this.Begin) < 0) { // Empty period: error
        string message = string.Format ("End={0} before Begin={1}",
                                        this.End, this.Begin);
        SetModificationInError (message);
        return;
      }

      if (this.Range.IsEmpty ()) {
        if (GetLogger ().IsInfoEnabled) {
          GetLogger ().InfoFormat ("MakeAnalysis: empty range => nothing to do for modification id={0}",
            this.Id);
        }
        MarkAsCompleted ("");
        return;
      }

      if ((AnalysisStatus.New == this.AnalysisStatus) && (null == this.Parent)) { // First call
        if ((null != this.Reason) && (LinkDirection.None != this.Reason.LinkOperationDirection)) {
          CreateLinkOperation ();
          return;
        }
      }

      if (CheckMachineStatus ()) {
        return;
      }

      if (!string.IsNullOrEmpty (this.Dynamic)
        && !this.Dynamic.Equals (",")
        && !this.Dynamic.Equals (":")) {
        var dynamicStart = this.DynamicStart;
        if (!string.IsNullOrEmpty (dynamicStart)) {
          MakeAnalysisDynamicStart (dynamicStart);
          return;
        }
        if (!this.Dynamic.StartsWith ("?")) {
          var dynamicEnd = this.DynamicEnd;
          if (!string.IsNullOrEmpty (dynamicEnd)) {
            MakeAnalysisDynamicEnd (dynamicEnd);
            return;
          }
        }
      }

      if (this.Kind.Equals (ReasonMachineAssociationKind.Consolidate)) {
        // In case of 'Consolidate', no step process
        this.Analyze ();
        MarkAsCompleted ("Cache/ClearDomainByMachine/ReasonAssociation/" + this.Machine.Id + "?Broadcast=true",
                         (DateTime?)this.Range.Upper); // => InProgress or Done
        return;
      }

      if (this.Option.HasValue
          && this.Option.Value.HasFlag (AssociationOption.TrackSlotChanges)
          && (null != this.Reason)
          && !this.End.HasValue) { // Current: check if the end date/time must be discontinued
        Debug.Assert (!this.Kind.Equals (ReasonMachineAssociationKind.Consolidate));
        Debug.Assert (null == this.Parent);
        if (null != this.Parent) {
          log.Error ($"MakeAnalysis: TrackSlotChanges option with a parent for modification id {this.Id}");
        }
        MakeAnalysisDynamicEndAggressive ("NextMachineMode");
        return;
      }

      UtcDateTimeRange endHint = new UtcDateTimeRange ("(,)");
      if (!string.IsNullOrEmpty (this.Dynamic)) {
        if (this.Dynamic.StartsWith ("?")) { // Recompute for optimization the dynamic end
          var dynamicEnd = this.DynamicEnd;
          var notProcessedRange = GetNotPocessedRange ();
          if (notProcessedRange.IsEmpty ()) {
            if (GetLogger ().IsFatalEnabled) {
              GetLogger ().Fatal ("MakeAnalysis: (unexpected) GetNotProcessedRange returned an empty range => completed");
            }
            Debug.Assert (false);
            ForceAsDone (""); // To skip the process of the sub-modifications TrackDynamicEnd
            return;
          }
          var hint = new UtcDateTimeRange (notProcessedRange.Lower, notProcessedRange.Upper, true, true);
          IDynamicTimeResponse dynamicEndResponse;
          try {
            Debug.Assert (!this.Range.IsEmpty ());
            // Note: make the upper limit inclusive
            var limit = new UtcDateTimeRange (this.Range.Lower, this.Range.Upper, true, true);
            dynamicEndResponse = Lemoine.Business.DynamicTimes.DynamicTime
              .GetDynamicTime (this.DynamicEnd, this.Machine, this.DynamicTimeRange, hint, limit);
          }
          catch (NoDynamicTime) {
            GetLogger ().Error ($"MakeAnalysis: unknown dynamic end {dynamicEnd}");
            CheckNoReasonProposal ();
            SetModificationInError ("Unknown dynamic end");
            return;
          }
          Debug.Assert (null != dynamicEndResponse);
          if (dynamicEndResponse.Timeout) {
            if (GetLogger ().IsWarnEnabled) {
              GetLogger ().Warn ($"MakeAnalysis: Timeout for dynamic end {dynamicEnd} range {this.Range} hint {hint} => completed");
            }
            CancelAndMarkDynamicTimeNotApplicable ();
            return;
          }
          else if (dynamicEndResponse.NotApplicable) {
            if (GetLogger ().IsDebugEnabled) {
              GetLogger ().DebugFormat ("MakeAnalysis: NotApplicable for dynamic end {0} range {1} hint {2} => completed",
                dynamicEnd, this.Range, hint);
            }
            CancelAndMarkDynamicTimeNotApplicable ();
            return;
          }
          else if (dynamicEndResponse.NoData) {
            if (GetLogger ().IsDebugEnabled) {
              GetLogger ().DebugFormat ("MakeAnalysis: no data for dynamic end {0} range {1} hint {2} => completed",
                dynamicEnd, this.Range, hint);
            }
            CancelAndMarkDynamicTimeNotApplicable ();
            return;
          }
          else if (dynamicEndResponse.Final.HasValue) {
            ApplyDynamicEnd (dynamicEndResponse.Final.Value);
            return;
          }
          else {
            endHint = dynamicEndResponse.Hint;
          }
        }
      }

      Debug.Assert (!this.Kind.HasFlag (ReasonMachineAssociationKind.TrackDynamicEnd));
      if (this.Kind.IsManual ()) {
        // Note: this is not sufficient to cancel any former manual reason proposal
        // only once when the status is New, since it may be done by step
        // and in the mean time, new reason proposals may be created
        CancelFormerManualReasonProposals ();
      }

      { // Get the adjusted step range
        var range = GetNotAppliedRange ();
        UtcDateTimeRange restrictedRange = range;
        if (endHint.Upper.HasValue
          && (Bound.Compare<DateTime> (endHint.Upper, range.Upper) < 0)) {
          restrictedRange = new UtcDateTimeRange (range.Lower, endHint.Upper);
        }
        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().Debug ($"MakeAnalysis: consider step {range} restricted={restrictedRange}");
        }
        if (restrictedRange.IsEmpty ()) { // Already completed
          MarkAsCompleted ("");
        }
        else { // restrictedRange not empty
          // Analyze ! with the adjusted begin and end
          ReasonMachineAssociation association = new ReasonMachineAssociation (this.Machine,
                                                                               restrictedRange,
                                                                               this);
          association.DateTime = this.DateTime;
          association.m_kind = this.Kind;
          association.m_reason = this.Reason;
          association.m_optionalReasonScore = this.OptionalReasonScore;
          association.ReasonDetails = this.ReasonDetails;
          association.Option = this.Option;
          association.Caller = this;
          association.Analyze ();

          // Analysis is done
          MarkAsCompleted ("Cache/ClearDomainByMachine/ReasonAssociation/" + this.Machine.Id + "?Broadcast=true",
                           (DateTime?)range.Upper); // => InProgress or Done
          if (null != this.Reason) {
            AdjustReasonProposalEnd (restrictedRange.Upper, false, false, false);
          }
        }
      }
    }

    void MakeAnalysisReset ()
    {
      var range = GetNotAppliedRange ();
      if (GetLogger ().IsDebugEnabled) {
        GetLogger ().DebugFormat ("MakeAnalysis: " +
                                  "consider step {0}",
                                  range);
      }
      if (range.IsEmpty ()) { // Already completed
        MarkAsCompleted ("");
      }
      else {
        CancelReasonSlots (range);

        // Analysis is done
        MarkAsCompleted ("Cache/ClearDomainByMachine/ReasonAssociation/" + this.Machine.Id + "?Broadcast=true",
                         (DateTime?)range.Upper); // => InProgress or Done
      }
    }

    void CancelFormerManualReasonProposals ()
    {
      var parent = GetParentReasonMachineAssociation ();
      if (null == parent) {
        var manualReasonProposals = ModelDAOHelper.DAOFactory.ReasonProposalDAO
          .FindManualOverlapsRange (this.Machine, this.Range)
          .Where (rp => rp.ModificationId != this.Id);
        foreach (var manualReasonProposal in manualReasonProposals) {
          SetActive ();
          if (Bound.Compare<DateTime> (manualReasonProposal.DateTimeRange.Lower, this.Range.Lower) < 0) {
            Debug.Assert (this.Range.Lower.HasValue);
            if (GetLogger ().IsDebugEnabled) {
              GetLogger ().DebugFormat ("MakeAnalysis: shorten reason proposal id={0} to {1}",
                manualReasonProposal.Id, this.Range.Lower);
            }
            manualReasonProposal.DateTimeRange =
              new UtcDateTimeRange (manualReasonProposal.DateTimeRange.Lower, this.Range.Lower.Value);
            ModelDAOHelper.DAOFactory.ReasonProposalDAO.MakePersistent (manualReasonProposal);
            // Because there is a unique constraint on modificationid,
            // the upper part can't be created, if it exists
          }
          else if (Bound.Compare<DateTime> (this.Range.Upper, manualReasonProposal.DateTimeRange.Upper) < 0) {
            Debug.Assert (this.Range.Upper.HasValue);
            if (GetLogger ().IsDebugEnabled) {
              GetLogger ().DebugFormat ("MakeAnalysis: new start of reason proposal id={0} at {1}",
                manualReasonProposal.Id, this.Range.Upper);
            }
            manualReasonProposal.DateTimeRange =
              new UtcDateTimeRange (this.Range.Upper.Value,
                manualReasonProposal.DateTimeRange.Upper,
                !this.Range.UpperInclusive,
                manualReasonProposal.DateTimeRange.UpperInclusive);
            ModelDAOHelper.DAOFactory.ReasonProposalDAO.MakePersistent (manualReasonProposal);
          }
          else {
            ModelDAOHelper.DAOFactory.ReasonProposalDAO.MakeTransient (manualReasonProposal);
          }
        } // foreach
      } // null == parent
      else if (null != parent) {
        SetActive ();
        if (parent is IReasonMachineAssociation) {
          var parentReasonMachineAssociation = (ReasonMachineAssociation)parent;
          parentReasonMachineAssociation.CancelFormerManualReasonProposals ();
        }
        else {
          GetLogger ().ErrorFormat ("CancelFormerManualReasonProposals: parent {0} is not a IReasonMachineAssociation, type is {1}",
            parent, parent.GetType ());
        }
      }
    }

    void CancelData ()
    {
      var parent = GetParentReasonMachineAssociation ();
      if (null == parent) {
        var reasonProposal = ModelDAOHelper.DAOFactory.ReasonProposalDAO
          .Get (this);
        if (null != reasonProposal) {
          if (GetLogger ().IsDebugEnabled) {
            GetLogger ().DebugFormat ("CancelData: cancel data in range {0}", reasonProposal.DateTimeRange);
          }
          Debug.Assert (!reasonProposal.DateTimeRange.IsEmpty ());
          ModelDAOHelper.DAOFactory.ReasonProposalDAO.MakeTransient (reasonProposal);
          var cancelResetReasonSlotsInSubModification = Lemoine.Info.ConfigSet
            .LoadAndGet (CANCEL_RESET_REASON_SLOTS_IN_SUB_MODIFICATION_KEY,
              CANCEL_RESET_REASON_SLOTS_IN_SUB_MODIFICATION_DEFAULT);
          if (cancelResetReasonSlotsInSubModification) {
            ModelDAOHelper.DAOFactory.ReasonMachineAssociationDAO
              .InsertSub (this, reasonProposal.DateTimeRange, CancelDataSubChange, null);
          }
          else {
            CancelReasonSlots (reasonProposal.DateTimeRange);
          }
        }
      }
      else if (parent is IReasonMachineAssociation) {
        SetActive ();
        var parentReasonMachineAssociation = (ReasonMachineAssociation)parent;
        parentReasonMachineAssociation.CancelData ();
      }
      else {
        if (GetLogger ().IsErrorEnabled) {
          GetLogger ().ErrorFormat ("CancelData: parent {0} is not a IReasonMachineAssociation, type is {1}",
            parent, parent.GetType ());
        }
      }

      m_dataCancelled = true;
    }

    void CancelDataSubChange (IReasonMachineAssociation association)
    {
      association.Dynamic = "";
      ((ReasonMachineAssociation)association).Kind = this.Kind
        .Add (ReasonMachineAssociationKind.Reset)
        .Remove (ReasonMachineAssociationKind.TrackDynamicEnd);
    }

    /// <summary>
    /// Check no reason proposal is associated to this. Log it if it is the case.
    /// 
    /// This method can be removed later if no such log is found
    /// </summary>
    void CheckNoReasonProposal ()
    {
      var parent = GetParentReasonMachineAssociation ();
      if (null == parent) {
        var reasonProposal = ModelDAOHelper.DAOFactory.ReasonProposalDAO
          .Get (this);
        if (null != reasonProposal) {
          GetLogger ().FatalFormat ("CheckNoReasonProposal: a reason proposal was found for parent modification id {0}",
            ((IDataWithId<long>)this).Id);
          ModelDAOHelper.DAOFactory.ReasonProposalDAO.MakeTransient (reasonProposal);
          Debug.Assert (false);
        }
      }
      else if (parent is IReasonMachineAssociation) {
        SetActive ();
        var parentReasonMachineAssociation = (ReasonMachineAssociation)parent;
        parentReasonMachineAssociation.CheckNoReasonProposal ();
      }
      else {
        GetLogger ().ErrorFormat ("CheckNoReasonProposal: parent {0} is not a IReasonMachineAssociation, type is {1}",
          parent, parent.GetType ());
      }
    }

    void AdjustReasonProposalEnd (UpperBound<DateTime> end, bool cancelIfRequired, bool shortenOnly, bool ifExistsOnly)
    {
      var parent = GetParentReasonMachineAssociation ();
      if (null != parent) {
        SetActive ();
        if (parent is IReasonMachineAssociation) {
          var parentReasonMachineAssociation = (ReasonMachineAssociation)parent;
          parentReasonMachineAssociation.AdjustReasonProposalEnd (end, cancelIfRequired, shortenOnly, ifExistsOnly);
        }
        else {
          GetLogger ().ErrorFormat ("AdjustReasonProposalEnd: parent {0} is not a IReasonMachineAssociation, type is {1}",
            parent, parent.GetType ());
        }
      }
      else { // null == parent
        var reasonProposal = ModelDAOHelper.DAOFactory.ReasonProposalDAO
          .Get (this);
        if (null != reasonProposal) {
          var range = new UtcDateTimeRange (reasonProposal.DateTimeRange.Lower, end);
          if (range.IsEmpty ()) {
            if (GetLogger ().IsDebugEnabled) {
              GetLogger ().DebugFormat ("AdjustReasonProposalEnd: new emty range for existing ReasonProposal => remove it");
            }
            if (cancelIfRequired) {
              CancelReasonSlots (reasonProposal.DateTimeRange);
            }
            else {
              GetLogger ().FatalFormat ("AdjustReasonProposalEnd: about to remove a reason proposal for range {0} while the cancelIfRequired option is off", reasonProposal.DateTimeRange);
              Debug.Assert (false);
            }
            ModelDAOHelper.DAOFactory.ReasonProposalDAO.MakeTransient (reasonProposal);
          }
          else { // !range.IsEmpty ()
            var compare = Bound.Compare<DateTime> (end, reasonProposal.DateTimeRange.Upper);
            if (0 == compare) {
              if (GetLogger ().IsDebugEnabled) {
                GetLogger ().DebugFormat ("AdjustReasonProposalEnd: no change");
              }
              return; // No change
            }
            else if (compare < 0) { // new reason proposal shorter
              if (cancelIfRequired) {
                var cancelRange = new UtcDateTimeRange (end.Value, reasonProposal.DateTimeRange.Upper);
                CancelReasonSlots (cancelRange);
              }
              else {
                GetLogger ().FatalFormat ("AdjustReasonProposalEnd: new end {0} is before existing end {1} while the cancelIfRequired option is off", end, reasonProposal.DateTimeRange.Upper);
                Debug.Assert (false);
              }
              if (GetLogger ().IsDebugEnabled) {
                GetLogger ().DebugFormat ("AdjustReasonProposalEnd: shorten reason proposal from {0} to{1}", reasonProposal.DateTimeRange, range);
              }
            }
            else { // 0 < compare: new reason proposal longer
              Debug.Assert (0 < compare);
              Debug.Assert (reasonProposal.DateTimeRange.Upper.HasValue);
              if (GetLogger ().IsDebugEnabled) {
                GetLogger ().DebugFormat ("AdjustReasonProposalEnd: longer reason proposal: extend {0} to{1}", reasonProposal.DateTimeRange, range);
              }
              if (shortenOnly) {
                if (GetLogger ().IsDebugEnabled) {
                  GetLogger ().DebugFormat ("AdjustReasonProposalEnd: shortenonly option on => do nothing");
                }
                return;
              }
            }
            reasonProposal.DateTimeRange = range;
            ModelDAOHelper.DAOFactory.ReasonProposalDAO.MakePersistent (reasonProposal);
          }
        }
        else if (!ifExistsOnly) { // && (null == reasonProposal)
          var range = new UtcDateTimeRange (this.Begin, end);
          if (range.IsEmpty ()) {
            if (GetLogger ().IsDebugEnabled) {
              GetLogger ().DebugFormat ("AdjustReasonProposalEnd: new ReasonProposal with an empty range");
            }
          }
          else { // !range.IsEmpty ()
            ModelDAOHelper.DAOFactory.ReasonProposalDAO.Insert (this, range);
          }
        }
      }
    }

    /// <summary>
    /// Mark the modification as not applicable
    /// </summary>
    void MarkDynamicTimeNotApplicable ()
    {
      var parent = GetParentMachine ();
      if (null == parent) {
        base.MarkAsNotApplicable ();
      }
      else {
        base.MarkAsAncestorNotApplicable ();
      }
    }

    void TrackDynamicEnd ()
    {
      if (string.IsNullOrEmpty (this.DynamicEnd)) {
        Debug.Assert (false, "TrackDynamicEnd with an empty or null DynamicEnd");
        GetLogger ().Fatal ($"TrackDynamicEnd: invalid dynamic end {this.DynamicEnd}");
        AddAnalysisLog (LogLevel.CRIT, "invalid dynamic end with kind=TrackDynamicEnd");
        SetModificationInError ("Invalid dynamic end");
        return;
      }

      // Note: parent if not type MachineModificationProxy, it remains lazy
      // and then the properties of the reason machine association can't be checked
      // (but this is not necessary to do it)
      /*
      if (null != parent) {
        if (!(parent is IReasonMachineAssociation)) {
          GetLogger ().ErrorFormat ("TrackDynamicEnd: parent {0} id={1} type={2} is not a IReasonMachineAssociation", parent, ((IDataWithId<long>)parent).Id, parent.GetType ());
          AddAnalysisLog (LogLevel.ERROR, "Invalid parent for a reasonmachineassociation with Kind=TrackDynamicEnd");
        }
        else { // This is a IReasonMachineAssociation
          var parentReasonMachineAssociation = (IReasonMachineAssociation)parent;
          Debug.Assert (!string.IsNullOrEmpty (parentReasonMachineAssociation.DynamicEnd));
          Debug.Assert (!parentReasonMachineAssociation.Option.HasValue
            || !parentReasonMachineAssociation.Option.Value.HasFlag (AssociationOption.ProgressiveStrategy));
        }
      }
      */

      UtcDateTimeRange hint;
      UtcDateTimeRange limit;
      if (this.DynamicEnd.EndsWith ("+")) {
        Debug.Assert (this.End.HasValue);
        var appliedDateTime = this.AnalysisAppliedDateTime.HasValue
          ? this.AnalysisAppliedDateTime.Value
          : this.End.Value;
        hint = new UtcDateTimeRange (appliedDateTime);
        limit = new UtcDateTimeRange (this.End.Value);
      }
      else { // !this.DynamicEnd.EndsWith ("+")
        Debug.Assert (this.Begin.HasValue);
        var appliedDateTime = this.AnalysisAppliedDateTime.HasValue
          ? this.AnalysisAppliedDateTime.Value
          : this.Begin.Value;
        Debug.Assert (Bound.Compare<DateTime> (appliedDateTime, this.End) <= 0);
        hint = new UtcDateTimeRange (appliedDateTime);
        limit = new UtcDateTimeRange (this.Begin, this.End, "[]");
      }
      Debug.Assert (!limit.IsEmpty ());
      IDynamicTimeResponse dynamicEndResponse;
      try {
        dynamicEndResponse = Lemoine.Business.DynamicTimes.DynamicTime
          .GetDynamicTime (this.DynamicEnd, this.Machine, this.DynamicTimeRange, hint, limit);
      }
      catch (NoDynamicTime ex) {
        GetLogger ().Error ($"TrackDynamicEnd: unknown dynamic end {this.DynamicEnd}", ex);
        SetModificationInError ("Unknown dynamic end");
        return;
      }
      SetActive ();
      Debug.Assert (null != dynamicEndResponse);
      if (dynamicEndResponse.Timeout) {
        var cancelRange = this.Range;
        CancelAndMarkDynamicTimeNotApplicable (cancelRange);
        // Message sent by ProcessingReasonSlotsAnalysis
      }
      else if (dynamicEndResponse.NotApplicable) {
        var cancelRange = this.Range;
        CancelAndMarkDynamicTimeNotApplicable (cancelRange);
        // Message sent by ProcessingReasonSlotsAnalysis
      }
      else if (dynamicEndResponse.NoData) {
        var cancelRange = this.Range;
        CancelAndMarkDynamicTimeNotApplicable (cancelRange);
        // Message sent by ProcessingReasonSlotsAnalysis
      }
      else if (dynamicEndResponse.Final.HasValue) {
        DateTime dynamicApplicableEnd;
        if (this.DynamicEnd.EndsWith ("+")) { // Do not consider the option DynamicEndBeforeRealEnd
          dynamicApplicableEnd = dynamicEndResponse.Final.Value;
        }
        else if (Bound.Compare<DateTime> (this.End, dynamicEndResponse.Final.Value) < 0) {
          Debug.Assert (this.End.HasValue);
          if (this.Option.HasValue && this.Option.Value.HasFlag (AssociationOption.DynamicEndBeforeRealEnd)) {
            Debug.Assert (this.Begin.HasValue);
            dynamicApplicableEnd = this.Begin.Value;
          }
          else {
            dynamicApplicableEnd = this.End.Value;
          }
        }
        else {
          dynamicApplicableEnd = dynamicEndResponse.Final.Value;
        }

        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().Debug ($"TrackDynamicEnd: dynamic end is {dynamicEndResponse.Final.Value} applicable={dynamicApplicableEnd}");
        }
        AdjustReasonProposalEnd (dynamicApplicableEnd, true, true, true);
        MarkAsCompleted ("");
        // Message sent by ProcessingReasonSlotsAnalysis
      } // dynamicEndResponse.Final.HasValue
      else if (this.DynamicEnd.EndsWith ("+")) { // Do not consider the option DynamicEndBeforeRealEnd
        if (dynamicEndResponse.Hint.Lower.HasValue) {
          MarkAsPending (dynamicEndResponse.Hint.Lower.Value);
        }
        else {
          MarkAsPending (this.AnalysisAppliedDateTime);
        }
      }
      else { // Not the "+" option
        if (dynamicEndResponse.Hint.Lower.HasValue) {
          if (GetLogger ().IsDebugEnabled) {
            GetLogger ().Debug ($"TrackDynamicEnd: after is {dynamicEndResponse.Hint.Lower.Value}");
          }
          if (Bound.Compare<DateTime> (this.End, dynamicEndResponse.Hint.Lower.Value) < 0) {
            Debug.Assert (this.End.HasValue);
            if (this.Option.HasValue && this.Option.Value.HasFlag (AssociationOption.DynamicEndBeforeRealEnd)) { // Cancel it
              CancelData ();
            }
            MarkAsCompleted (""); // Message sent by ProcessingReasonSlotsAnalysis
          }
          else if (Bound.Equals (this.End, dynamicEndResponse.Hint.Lower.Value)) {
            if (this.Option.HasValue && this.Option.Value.HasFlag (AssociationOption.DynamicEndBeforeRealEnd)) {
              MarkAsPending (dynamicEndResponse.Hint.Lower.Value);
            }
            else {
              MarkAsCompleted (""); // Message sent by ProcessingReasonSlotsAnalysis
            }
          }
          else {
            MarkAsPending (dynamicEndResponse.Hint.Lower.Value);
          }
        }
        else { // Pending
          if (GetLogger ().IsDebugEnabled) {
            GetLogger ().Debug ("TrackDynamicEnd: no end and no after");
          }
          MarkAsPending (this.AnalysisAppliedDateTime);
        }
      }
    }

    IMachineModification GetParentMachine ()
    {
      if (null == this.ParentMachine) {
        return null;
      }
      else if (ModelDAOHelper.DAOFactory.IsInitialized (this.ParentMachine)) {
        var parent = this.ParentMachine;
        return parent;
      }
      else {
        var parent = ModelDAOHelper.DAOFactory.MachineModificationDAO
          .FindById (((IDataWithId<long>)this.ParentMachine).Id, this.Machine);
        // Note: this is supposed to be faster because the foreign key (machine) is used
        // and Initialize drove to some problems,
        // it could not be converted to a IReasonMachineAssociation
        Debug.Assert (null != parent); // Because by design, a Kind=TrackDynamicEnd reason machine association is a sub-modification of a reason machine association with a dynamic end
        if (null == parent) {
          GetLogger ().FatalFormat ("GetParentMachine: no parent id={0} for modification id={0}", ((IDataWithId<long>)this.Parent).Id, this.Id);
          AddAnalysisLog (LogLevel.CRIT, "Missing parent for a reasonmachineassociation with Kind=TrackDynamicEnd");
          Debug.Assert (false);
          return null;
        }
        return parent;
      }
    }

    IReasonMachineAssociation GetParentReasonMachineAssociation ()
    {
      if (null == this.ParentMachine) {
        return null;
      }
      else if (ModelDAOHelper.DAOFactory.IsInitialized (this.ParentMachine)) {
        string modificationType = this.ParentMachine.ModificationType;
        if (string.Equals (modificationType, "ReasonMachineAssociation", StringComparison.InvariantCultureIgnoreCase)) {
          if (this.ParentMachine is IReasonMachineAssociation) {
            return (IReasonMachineAssociation)this.ParentMachine;
          }
          else {
            GetLogger ().FatalFormat ("GetParentReasonMachineAssociation: ReasonMachineAssociation parent could not be converted to IReasonMachineAssociation, its type is {0}",
              this.ParentMachine.GetType ());
            Debug.Assert (false);
            return null;
          }
        }
        else {
          // Note: for the moment error, because it should never happen...
          GetLogger ().ErrorFormat ("GetParentReasonMachineAssociation: parent is not a reason machine association, its modification type is {0}", this.ParentMachine.ModificationType);
          return null;
        }
      }
      else {
        var parent = ModelDAOHelper.DAOFactory.ReasonMachineAssociationDAO
          .FindById (((IDataWithId<long>)this.ParentMachine).Id, this.Machine);
        // Note: this is supposed to be faster because the foreign key (machine) is used
        // and Initialize drove to some problems,
        // it could not be converted to a IReasonMachineAssociation
        Debug.Assert (null != parent); // Because by design, a Kind=TrackDynamicEnd reason machine association is a sub-modification of a reason machine association with a dynamic end
        if (null == parent) {
          GetLogger ().FatalFormat ("GetParentReasonMachineAssociation: no parent id={0} for modification id={0}", ((IDataWithId<long>)this.Parent).Id, this.Id);
          AddAnalysisLog (LogLevel.CRIT, "Missing parent for a reasonmachineassociation with Kind=TrackDynamicEnd");
          Debug.Assert (false);
          return null;
        }
        return parent;
      }
    }

    /// <summary>
    /// Create the associated link operation modification if applicable (not on a sub-modification)
    /// </summary>
    void CreateLinkOperation ()
    {
      GetLogger ().DebugFormat ("CreateLinkOperation: " +
        "create link operation {0}",
        this.Reason.LinkOperationDirection);
      ILinkOperation linkOperation = ModelDAOHelper.ModelFactory
        .CreateLinkOperation (this.Machine, this.Reason.LinkOperationDirection, this.Range);
      ModelDAOHelper.DAOFactory.LinkOperationDAO.MakePersistent (linkOperation);
      linkOperation.Parent = this.MainModification ?? this;
      linkOperation.Priority = this.Priority;

      MarkAsInProgress (null);
    }

    /// <summary>
    /// Check the machinestatus table
    /// </summary>
    /// <returns>true if the data was processed</returns>
    bool CheckMachineStatus ()
    {
      GetLogger ().DebugFormat ("CheckMachineStatus: " +
                       "about to get machine status for machine id {0}",
                       this.Machine.Id);
      IMachineStatus machineStatus = ModelDAOHelper.DAOFactory.MachineStatusDAO.FindById (this.Machine.Id);
      if (null == machineStatus) {
        string message = string.Format ("no MachineStatus found for {0}, create one",
                                        this.Machine);
        GetLogger ().WarnFormat ("CheckMachineStatus: {0}",
                         message);
        AddAnalysisLog (LogLevel.WARN,
                        message);
        var monitoredMachine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (this.Machine.Id);
        machineStatus = ModelDAOHelper.ModelFactory.CreateMachineStatus (monitoredMachine);
        ((MachineStatus)machineStatus).SetReasonFromLastReasonSlot ();
      }
      Debug.Assert (null != machineStatus);
      GetLogger ().DebugFormat ("CheckMachineStatus: " +
                       "with reason {0} period {1}-{2}",
                       this.Reason, this.Begin, this.End);
      if (Bound.Compare<DateTime> (machineStatus.ReasonSlotEnd, this.Begin) < 0) { // Future
        GetLogger ().Debug ("CheckMachineStatus: " +
                   "future reason machine association " +
                   "=> do it later (status pending)");
        MarkAsPending (machineStatus.ReasonSlotEnd);
        return true;
      }

      return false;
    }

    void MakeAnalysisDynamicStart (string dynamicStart)
    {
      // Note: for the moment, all the dynamic starts finish in error
      SetModificationInError ("Dynamic start => error");
      return;
      // Reconnect the code below later if needed
      /*
            if (this.Kind.Equals (ReasonMachineAssociationKind.Consolidate)) {
              SetModificationInError ("Incompatible properties Kind=Consolidate VS DynamicStart");
              return;
            }
            if (null == this.Reason) {
              SetModificationInError ("Incompatible properties Reason=null VS DynamicStart");
              return;
            }
            if (!this.Begin.HasValue) {
              SetModificationInError ("Incompatible properties Begin==null VS DynamicStart");
              return;
            }
            try {
              IDynamicTimeResponse dynamicStartResponse = Lemoine.Business.DynamicTimes.DynamicTime
               .GetDynamicTime (dynamicStart, this.Machine, this.Range.Lower.Value);
              Debug.Assert (null != dynamicStartResponse);
              if (dynamicStartResponse.NotApplicable) {
                GetLogger ().InfoFormat ("MakeAnalysisDynamicStart: no data for dynamic start {0}", dynamicStart);
                MarkDynamicTimeNotApplicable ();
              }
              else if (dynamicStartResponse.NoData) {
                GetLogger ().ErrorFormat ("MakeAnalysisDynamicStart: no data for dynamic start {0}", dynamicStart);
                MarkDynamicTimeNotApplicable ();
              }
              else if (dynamicStartResponse.Final.HasValue) {
                var start = dynamicStartResponse.Final.Value;
                var range = new UtcDateTimeRange (start, this.End);
                if (!range.IsEmpty ()) {
                  IReasonMachineAssociation subModification = this.Clone (range);
                  subModification.Dynamic = "," + this.DynamicEnd;
                  subModification = ModelDAOHelper.DAOFactory.ReasonMachineAssociationDAO.MakePersistent (subModification);
                  subModification.Parent = this.MainModification ?? this;
                  subModification.Priority = this.Priority;
                }
                MarkAsCompleted ("");
              }
              else { // !dynamicStartResponse.Start.HasValue => Pending
                GetLogger ().DebugFormat ("MakeAnalysisDynamicStart: the dynamic start {0} is not known yet", dynamicStart);
                MarkAsPending (null);
              }
            }
            catch (NoDynamicTime) {
              GetLogger ().ErrorFormat ("MakeAnalysisDynamicStart: no dynamic start {0}", dynamicStart);
              SetModificationInError ("Dynamic start " + dynamicStart + " unknown");
            }
      */
    }

    void MakeAnalysisDynamicEnd (string dynamicEnd)
    {
      if (!this.AnalysisAppliedDateTime.HasValue) { // first time
        if (this.Kind.Equals (ReasonMachineAssociationKind.Consolidate)) {
          SetModificationInError ("Incompatible properties Kind=Consolidate VS DynamicEnd");
          return;
        }
        if (null == this.Reason) {
          SetModificationInError ("Incompatible properties Reason=null VS DynamicEnd");
          return;
        }
        if (!this.Begin.HasValue) {
          SetModificationInError ("Incompatible properties Begin=null VS DynamicEnd");
          return;
        }
        if (dynamicEnd.EndsWith ("+") && !this.End.HasValue) {
          SetModificationInError ("Incompatible properties End=null VS DynamicEnd+");
          return;
        }
      }
      if (this.Option.HasValue && this.Option.Value.HasFlag (AssociationOption.ProgressiveStrategy)) {
        MakeAnalysisDynamicEndProgressive (dynamicEnd);
      }
      else { // Aggressive strategy
        MakeAnalysisDynamicEndAggressive (dynamicEnd);
      }
    }

    void MakeAnalysisDynamicEndProgressive (string dynamicEnd)
    {
      GetLogger ().Debug ("MakeAnalysisDynamicEndProgressive");

      var notProcessedRange = GetNotPocessedRange ();
      if (notProcessedRange.IsEmpty ()) {
        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().DebugFormat ("MakeAnalysisDynamicEndProgressive: GetNotProcessedRange returned an empty range => completed");
        }
        MarkAsCompleted ("");
        return;
      }
      Debug.Assert (notProcessedRange.Lower.HasValue);
      IDynamicTimeResponse dynamicEndResponse;
      try {
        Debug.Assert (!notProcessedRange.IsEmpty ());
        // Note: make the upper limit inclusive
        var limit = new UtcDateTimeRange (notProcessedRange.Lower, notProcessedRange.Upper, true, true);
        dynamicEndResponse = Lemoine.Business.DynamicTimes.DynamicTime
          .GetDynamicTime (dynamicEnd, this.Machine, this.DynamicTimeRange, new UtcDateTimeRange (notProcessedRange.Lower.Value), limit);
      }
      catch (NoDynamicTime) {
        GetLogger ().ErrorFormat ("MakeAnalysisDynamicEndProgress: unknown dynamic end {0}", dynamicEnd);
        SetModificationInError ("Dynamic end " + dynamicEnd + " unknown");
        return;
      }
      SetActive ();
      Debug.Assert (null != dynamicEndResponse);
      if (dynamicEndResponse.Timeout) {
        CancelAndMarkDynamicTimeNotApplicable ();
        return;
      }
      else if (dynamicEndResponse.NotApplicable) {
        CancelAndMarkDynamicTimeNotApplicable ();
        return;
      }
      else if (dynamicEndResponse.NoData) {
        ApplyDynamicEnd (notProcessedRange.Lower.Value);
        return;
      }
      else if (dynamicEndResponse.Final.HasValue) {
        ApplyDynamicEnd (dynamicEndResponse.Final.Value);
        return;
      }
      else if (!dynamicEndResponse.Hint.IsEmpty () && dynamicEndResponse.Hint.Lower.HasValue) {
        if ((Bound.Compare<DateTime> (this.End, dynamicEndResponse.Hint.Lower.Value) < 0)
          && this.Option.HasValue && this.Option.Value.HasFlag (AssociationOption.DynamicEndBeforeRealEnd)) {
          CancelAndMarkDynamicTimeNotApplicable ();
          return;
        }
        // Before After
        UtcDateTimeRange restriction = new UtcDateTimeRange (new LowerBound<DateTime> (null),
                                                             dynamicEndResponse.Hint.Lower.Value);
        UtcDateTimeRange restrictedRange = new UtcDateTimeRange (notProcessedRange.Intersects (restriction));
        if (restrictedRange.IsEmpty ()) {
          GetLogger ().DebugFormat ("MakeAnalysisDynamicEndProgressive: " +
                           "after / before: restricted range is empty => nothing to do");
          MarkAsPending (dynamicEndResponse.Hint.Lower.Value);
          return;
        }
        else { // range ok: two sub-modifications
          { // Before After
            GetLogger ().InfoFormat ("MakeAnalysisDynamicEndProgressive: " +
                            "after / before: restrict {0} to {1}",
                            this, dynamicEndResponse.Hint.Lower.Value);
            ModelDAOHelper.DAOFactory.ReasonMachineAssociationDAO
              .InsertSub (this, restrictedRange, (x) => x.Dynamic = null, null);
          }
          { // After After
            UtcDateTimeRange afterRestrictedRange = new UtcDateTimeRange (notProcessedRange.Intersects (dynamicEndResponse.Hint));
            if (afterRestrictedRange.IsEmpty ()) {
              GetLogger ().DebugFormat ("MakeAnalysisDynamicEndProgressive: " +
                               "after / after: restricted range is empty => nothing to do");
            }
            else { // range ok
              GetLogger ().InfoFormat ("MakeAnalysisDynamicEndProgressive: " +
                              "after / after: restrict {0} to {1}",
                              this, dynamicEndResponse.Hint);
              if (this.Dynamic.EndsWith ("+")) {
                Debug.Assert (this.End <= afterRestrictedRange.Lower);
                ModelDAOHelper.DAOFactory.ReasonMachineAssociationDAO
                  .InsertSub (this, afterRestrictedRange, (x) => x.Dynamic = this.Dynamic.Substring (0, this.Dynamic.Length - 1), null);
              }
              else {
                ModelDAOHelper.DAOFactory.ReasonMachineAssociationDAO
                  .InsertSub (this, afterRestrictedRange, delegate (IReasonMachineAssociation x) { }, null);
              }
            }
          }
          MarkAsCompleted ("");
          return;
        }
      }
      else {
        MarkAsPending (null);
        return;
      }
    }

    void MakeAnalysisDynamicEndAggressive (string dynamicEnd)
    {
      GetLogger ().Debug ("MakeAnalysisDynamicEndAggressive");

      // Note: because of CheckMachineStatus, the status can be pending with an AnalysisAppliedDateTime not null

      Debug.Assert (this.DynamicTimeRange.Lower.HasValue, "Dynamic time with no lower bound range");
      IDynamicTimeResponse dynamicEndResponse;
      try {
        Debug.Assert (!this.Range.IsEmpty ());
        // Note: make the upper limit inclusive
        var limit = new UtcDateTimeRange (this.Range.Lower, this.Range.Upper, true, true);
        dynamicEndResponse = Lemoine.Business.DynamicTimes.DynamicTime
          .GetDynamicTime (dynamicEnd, this.Machine, this.DynamicTimeRange, new UtcDateTimeRange ("(,)"), limit);
      }
      catch (NoDynamicTime) {
        GetLogger ().ErrorFormat ("MakeAnalysisDynamicEndAggressive: No dynamic end {0}", dynamicEnd);
        SetModificationInError ("Dynamic end " + dynamicEnd + " unknown");
        CheckNoReasonProposal ();
        return;
      }
      SetActive ();
      Debug.Assert (null != dynamicEndResponse);
      if (dynamicEndResponse.Timeout) {
        if (GetLogger ().IsWarnEnabled) {
          GetLogger ().Warn ($"MakeAnalysisDynamicEndAggressive: Timeout is returned for {dynamicEnd} at {this.DynamicTimeRange} => cancel and mark as completed");
        }
        CancelAndMarkDynamicTimeNotApplicable ();
        return;
      }
      else if (dynamicEndResponse.NotApplicable) {
        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().DebugFormat ("MakeAnalysisDynamicEndAggressive: notApplicable is returned for {0} at {1} => cancel and mark as completed",
            dynamicEnd, this.DynamicTimeRange);
        }
        CancelAndMarkDynamicTimeNotApplicable ();
        return;
      }
      else if (dynamicEndResponse.NoData) {
        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().DebugFormat ("MakeAnalysisDynamicEndAggressive: no data is returned for {0} at {1} => cancel and mark as completed",
            dynamicEnd, this.DynamicTimeRange);
        }
        CancelAndMarkDynamicTimeNotApplicable ();
        return;
      }
      else if (dynamicEndResponse.Final.HasValue) {
        ApplyDynamicEnd (dynamicEndResponse.Final.Value);
        return;
      }
      else {
        if (!this.Range.Overlaps (dynamicEndResponse.Hint)
          && this.Option.HasValue && this.Option.Value.HasFlag (AssociationOption.DynamicEndBeforeRealEnd)) {
          CancelAndMarkDynamicTimeNotApplicable ();
          return;
        }
        if (!this.Range.IsEmpty ()) {
          var range = this.CloneRange;
          bool removeDynamicPlus = false;
          if (range.IsEmpty ()) {
            Debug.Assert (!string.IsNullOrEmpty (this.Dynamic) && this.Dynamic.EndsWith ("+"));
            if (string.IsNullOrEmpty (this.Dynamic) || !this.Dynamic.EndsWith ("+")) {
              log.Fatal ($"MakeAnalysisDynamicEndAggressive: unexpected value range={this.Range} dynamic={this.Dynamic}");
              MarkAsCompleted ("");
              return;
            }
            range = this.Range;
            removeDynamicPlus = true;
            Debug.Assert (!range.Upper.HasValue);
            // Note: in pre-change, range=[a,a) dynamic+ is converted into range=[a,) dynamic
          }
          Debug.Assert (!range.IsEmpty ());
          // Two sub-modifications: one to apply the reason aggressively, one to track the dynamic end
          // Apply the reason aggressively
          var aggressiveSubModification = ModelDAOHelper.DAOFactory.ReasonMachineAssociationDAO
            .InsertSub (this, range, association => MakeAnalysisDynamicEndAggressiveSubChangeAggressive (dynamicEnd, association, removeDynamicPlus), null);
          CheckNoReasonProposal (); // Not yet
          { // One to track the dynamic end and cancel the request if necessary
            if (!dynamicEndResponse.Hint.Overlaps (this.Range)) {
              if (GetLogger ().IsDebugEnabled) {
                GetLogger ().DebugFormat ("MakeAnalysisDynamicEndAggressive: hint does not overlap range => no DynamicEndTracker");
              }
            }
            else {
              var trackDynamicEndModification = ModelDAOHelper.DAOFactory.ReasonMachineAssociationDAO
                .InsertSub (this, range, association => MakeAnalysisDynamicEndAggressiveSubChangeDynamicEndTracker (dynamicEnd, dynamicEndResponse.Hint, aggressiveSubModification, association, removeDynamicPlus), aggressiveSubModification);
              if (dynamicEndResponse.Hint.Lower.HasValue) {
                ((ReasonMachineAssociation)trackDynamicEndModification).MarkAsInProgress (dynamicEndResponse.Hint.Lower.Value);
              }
            }
          }
        } // !range.IsEmpty ()
        MarkAsCompleted ("");
        return;
      }
    }

    static void MakeAnalysisDynamicEndAggressiveSubChangeAggressive (string dynamicEnd, IReasonMachineAssociation association, bool removeDynamicPlus)
    {
      association.Dynamic = "?" + dynamicEnd;
      if (removeDynamicPlus && association.Dynamic.EndsWith ("+")) {
        association.Dynamic = association.Dynamic.Substring (0, association.Dynamic.Length - 1);
      }
      // Remove the option TrackSlotChanges
      if (association.Option.HasValue && association.Option.Value.HasFlag (AssociationOption.TrackSlotChanges)) {
        association.Option = association.Option.Value
          .Remove (AssociationOption.TrackSlotChanges);
      }
    }

    static void MakeAnalysisDynamicEndAggressiveSubChangeDynamicEndTracker (string dynamicEnd, UtcDateTimeRange hint, IMachineModification parent, IReasonMachineAssociation association, bool removeDynamicPlus)
    {
      association.SetDynamicEndTracker (dynamicEnd, hint);
      if (removeDynamicPlus && association.Dynamic.EndsWith ("+")) {
        association.Dynamic = association.Dynamic.Substring (0, association.Dynamic.Length - 1);
      }
      association.Priority = Lemoine.Info.ConfigSet
        .LoadAndGet (DYNAMIC_END_TRACKER_PRIORITY_KEY, DYNAMIC_END_TRACKER_PRIORITY_DEFAULT);
      // Remove the option TrackSlotChanges
      if (association.Option.HasValue && association.Option.Value.HasFlag (AssociationOption.TrackSlotChanges)) {
        association.Option = association.Option.Value
          .Remove (AssociationOption.TrackSlotChanges);
      }
    }

    /// <summary>
    /// Get the UTC date/time range that has not been processed yet
    /// </summary>
    /// <returns></returns>
    protected override UtcDateTimeRange GetNotPocessedRange ()
    {
      if (!string.IsNullOrEmpty (this.Dynamic)) {
        if (this.Dynamic.EndsWith ("+")) { // In this case, this.End does not apply as a limit
          return new UtcDateTimeRange (this.AnalysisAppliedDateTime.HasValue
                                       ? this.AnalysisAppliedDateTime.Value
                                       : this.Begin,
                                       new UpperBound<DateTime> (null));
        }
      }

      return base.GetNotPocessedRange ();
    }

    DateTime GetDynamicStartDateTime ()
    {
      Debug.Assert (!string.IsNullOrEmpty (this.Dynamic));
      if (this.Dynamic.EndsWith ("+")) {
        Debug.Assert (this.End.HasValue);
        return this.End.Value;
      }
      else {
        Debug.Assert (this.Begin.HasValue);
        return this.Begin.Value;
      }
    }

    void ApplyDynamicEnd (DateTime dynamicApplicableEnd)
    {
      if ((Bound.Compare<DateTime> (this.End, dynamicApplicableEnd) < 0)
        && this.Option.HasValue && this.Option.Value.HasFlag (AssociationOption.DynamicEndBeforeRealEnd)) {
        CancelAndMarkDynamicTimeNotApplicable ();
        return;
      }

      var range = this.GetNotPocessedRange ();
      if (range.IsEmpty ()) {
        if (GetLogger ().IsFatalEnabled) {
          GetLogger ().FatalFormat ("ApplyDynamicEnd: (unexpected) GetNotProcessedRange is empty => nothing to do");
        }
        Debug.Assert (false);
        ForceAsDone (""); // To skip the process of the sub-modifications TrackDynamicEnd
        return;
      }
      UtcDateTimeRange restriction = new UtcDateTimeRange (new LowerBound<DateTime> (null),
                                                           dynamicApplicableEnd);
      UtcDateTimeRange restrictedRange = new UtcDateTimeRange (range.Intersects (restriction));
      if (restrictedRange.IsEmpty ()) {
        GetLogger ().DebugFormat ("ApplyDynamicEnd: restricted range is empty => nothing to do");
      }
      else { // range ok
        GetLogger ().InfoFormat ("ApplyDynamicEnd: restrict {0} to {1}",
                        this, dynamicApplicableEnd);
        ModelDAOHelper.DAOFactory.ReasonMachineAssociationDAO
          .InsertSub (this, restrictedRange, ApplyDynamicEndSubChange, null);
      }
      MarkAsCompleted ("");
      return;
    }

    void ApplyDynamicEndSubChange (IReasonMachineAssociation association)
    {
      association.Dynamic = null;
    }

    void CancelAndMarkDynamicTimeNotApplicable ()
    {
      Debug.Assert (this.Begin.HasValue);

      if (GetLogger ().IsDebugEnabled) {
        GetLogger ().Debug ("CancelAndMarkDynamicTimeNotApplicable");
      }

      CancelData ();
      MarkDynamicTimeNotApplicable ();
    }

    void CancelAndMarkDynamicTimeNotApplicable (UtcDateTimeRange range)
    {
      if (GetLogger ().IsDebugEnabled) {
        GetLogger ().DebugFormat ("CancelAndMarkDynamicTimeNotApplicable: range={0}", range);
      }

      CancelData ();
      MarkDynamicTimeNotApplicable ();
    }

    void CancelReasonSlots (UtcDateTimeRange range)
    {
      if (GetLogger ().IsDebugEnabled) {
        GetLogger ().DebugFormat ("CancelReasonSlots: range={0}", range);
      }
      // TODO: the rules may change in the future. For example, check first how many reason slots have a reason that match

      var step = Lemoine.Info.ConfigSet
        .LoadAndGet (CANCEL_REASON_SLOTS_STEP_KEY, CANCEL_REASON_SLOTS_STEP_DEFAULT);
      var reasonSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
        .FindOverlapsRangeDescending (this.Machine, range, step);
      foreach (var reasonSlot in reasonSlots) {
        SetActive ();
        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().DebugFormat ("CancelReasonSlots: process reason slot with range {0}", reasonSlot.DateTimeRange);
        }
        if (null == this.Reason) {
          if (GetLogger ().IsErrorEnabled) {
            GetLogger ().ErrorFormat ("CancelReasonSlots: the associated reason is null, which is not expected");
          }
          // - Set the flags as unsafe... just in case
          if (this.Kind.HasFlag (ReasonMachineAssociationKind.Manual)) {
            reasonSlot.SetUnsafeManualFlag ();
          }
          if (this.Kind.HasFlag (ReasonMachineAssociationKind.Auto)
            || this.Kind.HasFlag (ReasonMachineAssociationKind.TrackDynamicEnd)) {
            reasonSlot.SetUnsafeAutoReasonNumber ();
          }
          if (!this.Kind.HasFlag (ReasonMachineAssociationKind.Manual)
            && !this.Kind.HasFlag (ReasonMachineAssociationKind.Auto)) {
            if (GetLogger ().IsErrorEnabled) {
              GetLogger ().ErrorFormat ("CancelReasonSlots: nor Manual or Auto is set while reason is null, consider both");
            }
            reasonSlot.SetUnsafeManualFlag ();
            reasonSlot.SetUnsafeAutoReasonNumber ();
          }
          reasonSlot.SwitchToProcessing ();
          ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
        }
        else { // null != this.Reason
          if (reasonSlot.Reason.Id != this.Reason.Id) { // Different reason
            if (this.Kind.HasFlag (ReasonMachineAssociationKind.Manual)
              && reasonSlot.ReasonSource.HasFlag (ReasonSource.Manual)) {
              reasonSlot.SetUnsafeManualFlag ();
            }
            if (this.Kind.HasFlag (ReasonMachineAssociationKind.Auto)
              && (0 < reasonSlot.AutoReasonNumber)) {
              reasonSlot.SetUnsafeAutoReasonNumber ();
            }
            if (!this.Kind.HasFlag (ReasonMachineAssociationKind.Manual)
              && !this.Kind.HasFlag (ReasonMachineAssociationKind.Auto)) {
              if (GetLogger ().IsErrorEnabled) {
                GetLogger ().ErrorFormat ("CancelReasonSlots: nor Manual or Auto is set with different reasons");
              }
              if (reasonSlot.ReasonSource.HasFlag (ReasonSource.Manual)) {
                reasonSlot.SetUnsafeManualFlag ();
              }
              if (0 < reasonSlot.AutoReasonNumber) {
                reasonSlot.SetUnsafeAutoReasonNumber ();
              }
            }
            ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
          }
          else { // Same reason
            if (this.Kind.HasFlag (ReasonMachineAssociationKind.Manual)
              && reasonSlot.ReasonSource.HasFlag (ReasonSource.Manual)) {
              reasonSlot.SetUnsafeManualFlag ();
            }
            if (this.Kind.HasFlag (ReasonMachineAssociationKind.Auto)) {
              if (reasonSlot.ReasonSource.IsAuto ()) {
                if (reasonSlot.ReasonSource.IsDefault ()) {
                  if (GetLogger ().IsWarnEnabled) {
                    GetLogger ().WarnFormat ("CancelReasonSlots: cancel reason {0} in range {1} which corresponds to a default auto reason",
                      this.Reason.Id, range);
                  }
                  reasonSlot.SetUnsafeAutoReasonNumber ();
                }
                else { // Not the default one
                  if (range.ContainsRange (reasonSlot.DateTimeRange)) {
                    ((ReasonSlot)reasonSlot).RemoveExtraAutoReason ();
                  }
                  else {
                    reasonSlot.SetUnsafeAutoReasonNumber ();
                  }
                }
              }
              else if (0 < reasonSlot.AutoReasonNumber) {
                reasonSlot.SetUnsafeAutoReasonNumber ();
              }
            }
            if (!this.Kind.HasFlag (ReasonMachineAssociationKind.Manual)
              && !this.Kind.HasFlag (ReasonMachineAssociationKind.Auto)) {
              if (GetLogger ().IsErrorEnabled) {
                GetLogger ().ErrorFormat ("CancelReasonSlots: nor Manual or Auto is set with the same reason {0}",
                  this.Reason.Id);
              }
              if (reasonSlot.ReasonSource.HasFlag (ReasonSource.Manual)) {
                reasonSlot.SetUnsafeManualFlag ();
              }
              if (reasonSlot.ReasonSource.IsAuto ()) {
                if (reasonSlot.ReasonSource.IsDefault ()) {
                  if (GetLogger ().IsWarnEnabled) {
                    GetLogger ().WarnFormat ("CancelReasonSlots: cancel reason {0} in range {1} which corresponds to a default auto reason",
                      this.Reason.Id, range);
                  }
                  reasonSlot.SetUnsafeAutoReasonNumber ();
                }
                else { // Not the default one
                  if (range.ContainsRange (reasonSlot.DateTimeRange)) {
                    ((ReasonSlot)reasonSlot).RemoveExtraAutoReason ();
                  }
                  else {
                    reasonSlot.SetUnsafeAutoReasonNumber ();
                  }
                }
              }
              else if (0 < reasonSlot.AutoReasonNumber) {
                reasonSlot.SetUnsafeAutoReasonNumber ();
              }
            }
            reasonSlot.SwitchToProcessing ();
            ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
          }
        }
        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().DebugFormat ("CancelReasonSlots: process of reason slot id={0} completed with new reason id {1}",
            reasonSlot.Id, reasonSlot.Reason.Id);
        }
      }
    }

    /// <summary>
    /// Clone a ReasonMachineAssociation with a new date/time range
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public virtual IReasonMachineAssociation Clone (UtcDateTimeRange range)
    {
      var clone = new ReasonMachineAssociation (this.Machine, range);
      clone.Option = this.Option;
      clone.Dynamic = this.Dynamic;
      clone.Priority = this.StatusPriority;
      if (null != this.Reason) {
        Debug.Assert (!this.Kind.Equals (ReasonMachineAssociationKind.Consolidate));
        Debug.Assert (this.OptionalReasonScore.HasValue);
        ((ReasonMachineAssociation)clone).SetReason (this.Reason, this.Kind, this.OptionalReasonScore.Value, this.ReasonDetails, this.JsonData);
      }
      else { // null == this.Reason
        Debug.Assert (this.Kind.Equals (ReasonMachineAssociationKind.Manual));
        clone.m_jsonData = this.JsonData;
        clone.ResetManualReason ();
      }
      return clone;
    }

    /// <summary>
    /// Is the reason machine association applicable on top of the specified reason slot ?
    /// </summary>
    /// <param name="reasonSlot"></param>
    /// <returns></returns>
    public virtual bool IsApplicable (IReasonSlot reasonSlot)
    {
      if (null == this.Reason) { // There is no need to adjust the applicable date/time range in case of a reset
        return false;
      }
      else {
        return GetReasonExtensions ()
          .Any (ext => ext.IsCompatible (reasonSlot.DateTimeRange, reasonSlot.MachineMode, reasonSlot.MachineObservationState, this.Reason, this.ReasonScore, this.Kind.ConvertToReasonSource ()));
      }
    }

    /// <summary>
    /// Apply the modification while keeping it transient
    /// 
    /// Note that it is applicable only if there is no dynamic time
    /// </summary>
    public override void Apply ()
    {
      Debug.Assert (string.IsNullOrEmpty (this.Dynamic));
      if (!string.IsNullOrEmpty (this.Dynamic)) {
        log.FatalFormat ("Apply: association {0} was a dynamic value {1}, which is unexpected", this, this.Dynamic);
      }
      this.Analyze ();
    }

    /// <summary>
    /// Process the association itself,
    /// without updating any Modification property
    /// </summary>
    void Analyze ()
    {
      ReasonSlotDAO reasonSlotDAO = new ReasonSlotDAO ();
      reasonSlotDAO.Caller = this;
      Insert<ReasonSlot, IReasonSlot, ReasonSlotDAO> (reasonSlotDAO, true);
    }

    /// <summary>
    /// Process the association itself,
    /// without updating any Modification property
    /// and considering a list of active slots
    /// </summary>
    void Analyze (IList<IReasonSlot> currentSlots)
    {
      ReasonSlotDAO reasonSlotDAO = new ReasonSlotDAO ();
      reasonSlotDAO.Caller = this;
      Insert<ReasonSlot, IReasonSlot, ReasonSlotDAO> (reasonSlotDAO, true, currentSlots);
    }

    /// <summary>
    /// Get the impacted activity analysis
    /// so that the activity analysis makes a pause
    /// </summary>
    /// <returns></returns>
    public override IList<IMachine> GetImpactedActivityAnalysis ()
    {
      IList<IMachine> list = new List<IMachine> ();
      list.Add (this.Machine);
      return list;
    }
    #endregion // Modification implementation

    /// <summary>
    /// Compute a new step span after a step timeout
    /// 
    /// Consider the observation state slots
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    protected override TimeSpan? ComputeNewStepSpan (UtcDateTimeRange range)
    {
      if (range.Lower.HasValue) {
        // Get the observation state slot at begin
        IObservationStateSlot observationStateSlot =
          ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
          .FindAt (this.Machine, range.Lower.Value);
        if (Bound.Compare<DateTime> (observationStateSlot.EndDateTime, range.Upper) < 0) {
          Debug.Assert (observationStateSlot.EndDateTime.HasValue);
          return observationStateSlot.EndDateTime.Value.Subtract (range.Lower.Value);
        }
      }

      GetLogger ().Debug ("ComputeNewSpan: no better end with the observation state slots, use the base method instead");
      return base.ComputeNewStepSpan (range);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public override void Unproxy ()
    {
      base.Unproxy ();
      NHibernateHelper.Unproxy<IReason> (ref m_reason);
    }


    /// <summary>
    /// Get the ReasonExtensions (lazy initialization)
    /// </summary>
    /// <returns></returns>
    IEnumerable<IReasonExtension> GetReasonExtensions ()
    {
      if (null == m_reasonExtensions) { // Initialization
        if (!Lemoine.Extensions.ExtensionManager.IsActive ()) {
          GetLogger ().Warn ("GetReasonExtensions: the extensions are not active");
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
          SetActive ();
        }
      }
      return m_reasonExtensions;
    }

    /// <summary>
    /// Get the ReasonSelectionExtensions (lazy initialization)
    /// </summary>
    /// <returns></returns>
    IEnumerable<IReasonSelectionExtension> GetReasonSelectionExtensions ()
    {
      if (null == m_reasonSelectionExtensions) { // Initialization
        if (!Lemoine.Extensions.ExtensionManager.IsActive ()) {
          GetLogger ().Warn ("GetReasonSelectionExtensions: the extensions are not active");
        }

        IMonitoredMachine monitoredMachine;
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          monitoredMachine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (this.Machine.Id);
        }
        if (null == monitoredMachine) {
          m_reasonSelectionExtensions = new List<IReasonSelectionExtension> ();
        }
        else {
          var request = new Lemoine.Business.Extension
            .MonitoredMachineExtensions<IReasonSelectionExtension> (monitoredMachine,
            (ext, m) => ext.Initialize (m));
          m_reasonSelectionExtensions = Lemoine.Business.ServiceProvider
            .Get (request);
          SetActive ();
        }
      }
      return m_reasonSelectionExtensions;
    }
  }
}
