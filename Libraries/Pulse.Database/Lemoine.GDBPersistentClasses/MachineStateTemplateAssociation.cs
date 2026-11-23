// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;

using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Business.DynamicTimes;
using Lemoine.Extensions.Business.DynamicTimes;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table MachineStateTemplateAssociation
  /// </summary>
  [Serializable]
  public class MachineStateTemplateAssociation : MachineAssociation, IMachineStateTemplateAssociation
  {
    /// <summary>
    /// Minimum duration of the association when the 'by period' process applies: key
    /// </summary>
    static readonly string BY_PERIOD_MIN_DURATION_KEY = "Analysis.MachineStateTemplateAssociation.ByPeriodMinDuration";
    /// <summary>
    /// Minimum duration of the association when the 'by period' process applies: default value
    /// </summary>
    static readonly TimeSpan BY_PERIOD_MIN_DURATION_DEFAULT = TimeSpan.FromHours (4);

    /// <summary>
    /// Default priority for the process machine state template
    /// </summary>
    static readonly string PROCESS_MACHINE_STATE_TEMPLATE_PRIORITY_KEY = "Analysis.ProcessMachineStateTemplate.Priority";
    static readonly int PROCESS_MACHINE_STATE_TEMPLATE_PRIORITY_DEFAULT = 1;

    /// <summary>
    /// Create an asynchronous process machine state template, else leave the analysis service manage the uncompleted machine state templates
    /// </summary>
    static readonly string CREATE_ASYNC_PROCESS_MACHINE_STATE_TEMPLATE_KEY = "Analysis.MachineStateTemplateAssociation.CreateAsyncProcessMachineStateTemplate";
    static readonly bool CREATE_ASYNC_PROCESS_MACHINE_STATE_TEMPLATE_DEFAULT = false;

    /// <summary>
    /// Default priority for the dynamic end tracker sub-modifications
    /// </summary>
    static readonly string DYNAMIC_END_TRACKER_PRIORITY_KEY = "MachineStateTemplateAssociation.DynamicEndTracker.Priority";
    static readonly int DYNAMIC_END_TRACKER_PRIORITY_DEFAULT = 10;

    IMachineStateTemplate m_MachineStateTemplate;
    IMachineStateTemplate m_nextMachineStateTemplate;
    IUser m_user;
    IShift m_shift;
    bool m_force = false;

    /// <summary>
    /// Default constructor
    /// </summary>
    protected MachineStateTemplateAssociation ()
    { }

    /// <summary>
    /// Constructor for some existing unit tests
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="machineStateTemplate">not null</param>
    /// <param name="begin"></param>
    internal protected MachineStateTemplateAssociation (IMachine machine,
                                                        IMachineStateTemplate machineStateTemplate,
                                                        DateTime begin)
      : base (machine, begin)
    {
      this.MachineStateTemplate = machineStateTemplate;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="machineStateTemplate">not null</param>
    /// <param name="range"></param>
    internal protected MachineStateTemplateAssociation (IMachine machine,
                                                        IMachineStateTemplate machineStateTemplate,
                                                        UtcDateTimeRange range)
      : base (machine, range)
    {
      this.MachineStateTemplate = machineStateTemplate;
    }

    /// <summary>
    /// Specific constructor when begin may be equal to end
    /// because of the use of dynamic times
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="machineStateTemplate">not null</param>
    /// <param name="dynamic">dynamic times, not null or empty</param>
    internal protected MachineStateTemplateAssociation (IMachine machine,
                                                        IMachineStateTemplate machineStateTemplate,
                                                        UtcDateTimeRange range,
                                                        string dynamic)
      : base (machine, range, dynamic)
    {
      this.MachineStateTemplate = machineStateTemplate;
    }

    /// <summary>
    /// Constructor for abstract modifications, that are kept transient
    /// 
    /// You may add a mainModification reference to be used in the analysis logs
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="machineStateTemplate">not null</param>
    /// <param name="range"></param>
    /// <param name="mainModification"></param>
    internal protected MachineStateTemplateAssociation (IMachine machine,
                                                        IMachineStateTemplate machineStateTemplate,
                                                        UtcDateTimeRange range,
                                                        IModification mainModification)
      : base (machine, range, mainModification)
    {
      this.MachineStateTemplate = machineStateTemplate;
    }

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
    /// Range to use to clone a machine state template association
    /// </summary>
    [XmlIgnore]
    UtcDateTimeRange CloneRange => base.Range;

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
    public override string ModificationType => "MachineStateTemplateAssociation";

    /// <summary>
    /// Reference to the Machine State Template
    /// 
    /// Not null
    /// </summary>
    [XmlIgnore]
    public virtual IMachineStateTemplate MachineStateTemplate
    {
      get { return m_MachineStateTemplate; }
      set {
        Debug.Assert (null != value);
        if (null == value) {
          log.Error ("MachineStateTemplate.set: null value");
          throw new ArgumentNullException ("MachineStateTemplateAssociation.MachineStateTemplate");
        }
        m_MachineStateTemplate = value;
      }
    }

    /// <summary>
    /// Reference to the Machine Observation State for Xml Serialization
    /// </summary>
    [XmlElement ("MachineStateTemplate")]
    public virtual MachineStateTemplate XmlSerializationMachineStateTemplate
    {
      get { return this.MachineStateTemplate as MachineStateTemplate; }
      set { this.MachineStateTemplate = value; }
    }

    /// <summary>
    /// Machine state template to apply once the dynamic end of this association is reached
    ///
    /// Nullable. If set, it has the priority on the NextMachineStateTemplate property
    /// of <see cref="MachineStateTemplate"/>
    /// </summary>
    [XmlIgnore]
    public virtual IMachineStateTemplate NextMachineStateTemplate
    {
      get { return m_nextMachineStateTemplate; }
      set { m_nextMachineStateTemplate = value; }
    }

    /// <summary>
    /// Reference to the next Machine State Template for Xml Serialization
    /// </summary>
    [XmlElement ("NextMachineStateTemplate")]
    public virtual MachineStateTemplate XmlSerializationNextMachineStateTemplate
    {
      get { return this.NextMachineStateTemplate as MachineStateTemplate; }
      set { this.NextMachineStateTemplate = value; }
    }

    /// <summary>
    /// used to serialize NextMachineStateTemplate only when not null
    /// </summary>
    public virtual bool XmlSerializationNextMachineStateTemplateSpecified => null != this.NextMachineStateTemplate;

    /// <summary>
    /// Reference to the User, according the Machine Observation State
    /// </summary>
    [XmlIgnore]
    public virtual IUser User
    {
      get { return m_user; }
      set { m_user = value; }
    }

    /// <summary>
    /// Reference to the User, according the Machine Observation State
    /// for Xml Serialization
    /// </summary>
    [XmlElement ("User")]
    public virtual User XmlSerializationUser
    {
      get { return this.User as User; }
      set { this.User = value; }
    }

    /// <summary>
    /// Optional reference to the Shift
    /// </summary>
    [XmlIgnore]
    public virtual IShift Shift
    {
      get { return m_shift; }
      set { m_shift = value; }
    }

    /// <summary>
    /// Reference to the Shift
    /// for Xml Serialization
    /// </summary>
    [XmlElement ("Shift")]
    public virtual Shift XmlSerializationShift
    {
      get { return this.Shift as Shift; }
      set { this.Shift = value; }
    }

    /// <summary>
    /// Force re-building the machine state template
    /// 
    /// Default is False
    /// </summary>
    [XmlIgnore]
    public virtual bool Force
    {
      get { return m_force; }
      set { m_force = value; }
    }

    /// <summary>
    /// Clone a MachineStateTemplateAssociation with a new date/time range
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public virtual IMachineStateTemplateAssociation Clone (UtcDateTimeRange range)
    {
      var clone = new MachineStateTemplateAssociation (this.Machine, this.MachineStateTemplate, range);
      clone.NextMachineStateTemplate = this.NextMachineStateTemplate;
      clone.Option = this.Option;
      clone.Dynamic = this.Dynamic;
      clone.Priority = this.StatusPriority;
      clone.User = this.User;
      clone.Shift = this.Shift;
      clone.Force = this.Force;
      return clone;
    }

    #region MachineAssociation implementation
    /// <summary>
    /// <see cref="PeriodAssociation.ConvertToSlot" />
    /// </summary>
    /// <returns></returns>
    public override TSlot ConvertToSlot<TSlot> ()
    {
      if (IsStopProcess ()) { // Stop must be processed first
        System.Diagnostics.Debug.Assert (false);
        throw new Exception ("Stop not processed yet");
      }
      else { // Stop already processed
        var slot =
          GenericMachineRangeSlot.Create (typeof (TSlot), this.Machine, new UtcDateTimeRange (this.Begin, this.End)) as TSlot;
        slot.Consolidated = false;

        if (slot is ObservationStateSlot) {
          var observationStateSlot = slot as ObservationStateSlot;
          Debug.Assert (null == observationStateSlot.MachineObservationState);
          observationStateSlot.MachineStateTemplate = this.MachineStateTemplate;
          observationStateSlot.User = this.User;
          observationStateSlot.Shift = this.Shift;
          return slot;
        }
        else {
          System.Diagnostics.Debug.Assert (false);
          throw new NotImplementedException ("Slot type not implemented");
        }
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
      if (IsStopProcess ()) { // Stop must be processed first
        System.Diagnostics.Debug.Assert (false);
        throw new Exception ("Stop not processed yet");
      }
      else { // Stop already processed
        Debug.Assert (null != oldSlot);
        Debug.Assert (this.Machine.Equals ((oldSlot as IPartitionedByMachine).Machine));

        if (oldSlot is ObservationStateSlot) {
          IObservationStateSlot oldObservationStateSlot =
            oldSlot as ObservationStateSlot;
          IObservationStateSlot newObservationStateSlot =
            (IObservationStateSlot)oldObservationStateSlot.Clone ();

          Debug.Assert (object.Equals (newObservationStateSlot.Machine,
                                       this.Machine));

          if (!object.Equals (oldObservationStateSlot.MachineStateTemplate, this.MachineStateTemplate)) {
            // change of MachineStateTemplate
            newObservationStateSlot.MachineStateTemplate = this.MachineStateTemplate;
            newObservationStateSlot.MachineObservationState = null;
            newObservationStateSlot.Production = null;
          }
          else if (this.Force) { // Save MachineStateTemplate, but force re-building the machine observation states
            newObservationStateSlot.MachineObservationState = null;
            newObservationStateSlot.Production = null;
          }
          // Merge user and shift (is it pertinent ?)
          newObservationStateSlot.User = ((null == this.User) && this.MachineStateTemplate.UserRequired)
            ? oldObservationStateSlot.User
            : this.User;
          newObservationStateSlot.Shift = this.Shift;
          return newObservationStateSlot as TSlot;
        }
        else {
          System.Diagnostics.Debug.Assert (false);
          log.Fatal ($"MergeData: trying to merge the association with a not supported slot {typeof (TSlot)}");
          throw new ArgumentException ("Not supported machine slot");
        }
      }
    }
    #endregion // MachineAssociation implementation

    #region Modification implementation
    /// <summary>
    /// Mark the modification as completed (Done) or partially completed (InProgress)
    /// </summary>
    /// <param name="message">Message to send to the web service (nullable)</param>
    /// <param name="effectiveEnd"></param>
    protected override void MarkAsCompleted (string message, DateTime? effectiveEnd)
    {
      if (null == this.Parent) { // Only if that modification is the main one, create or not a processmachinestatetemplate modification
        bool synchronous = this.Option.HasValue && this.Option.Value.HasFlag (AssociationOption.Synchronous);
        if (synchronous
            || Lemoine.Info.ConfigSet.LoadAndGet<bool> (CREATE_ASYNC_PROCESS_MACHINE_STATE_TEMPLATE_KEY,
                                                        CREATE_ASYNC_PROCESS_MACHINE_STATE_TEMPLATE_DEFAULT)) {
          UtcDateTimeRange applicationRange =
            new UtcDateTimeRange (this.Range.Intersects (new UtcDateTimeRange (new LowerBound<DateTime> (null),
                                                                               DateTime.UtcNow)));
          if (!applicationRange.IsEmpty ()) {
            ProcessMachineStateTemplate subModification =
              new ProcessMachineStateTemplate (this.Machine, applicationRange);
            (new ProcessMachineStateTemplateDAO ()).MakePersistent (subModification);
            if (synchronous) {
              subModification.Parent = this.MainModification ?? this;
              subModification.Priority = this.StatusPriority;
            }
            else { // Not really a sub-modification but an independent modification so that it is processed asynchronously
              subModification.Priority = Lemoine.Info.ConfigSet
                .LoadAndGet<int> (PROCESS_MACHINE_STATE_TEMPLATE_PRIORITY_KEY,
                                  PROCESS_MACHINE_STATE_TEMPLATE_PRIORITY_DEFAULT);
              subModification.Auto = true; // so that it is deleted once processed
            }
          }
        }
      }

      base.MarkAsCompleted (message, effectiveEnd);
    }

    void SetModificationInError (string message)
    {
      log.Error ($"SetModificationInError: {message} => finish in error");
      AddAnalysisLog (LogLevel.ERROR, message);
      MarkAsError ();
    }

    /// <summary>
    /// Mark the modification as not applicable because a dynamic time could not be resolved
    ///
    /// Note: on the contrary to <see cref="ReasonMachineAssociation"/>, a machine state template that was
    /// already applied is not reverted here. This is on purpose: there is no trace of the machine state
    /// template that was effective before, so it could not be restored. This is why the consequence is
    /// always recorded in the analysis logs.
    /// The only exception is the dynamic end tracker with the option <see cref="AssociationOption.CancelAfterEnd"/>,
    /// see <see cref="CancelTrackedMachineStateTemplate"/>
    /// </summary>
    /// <param name="message">reason why the dynamic time is not applicable and its consequence, recorded in the analysis logs</param>
    void MarkDynamicTimeNotApplicable (string message)
    {
      AddAnalysisLog (LogLevel.WARN, message);
      base.MarkAsNotApplicable ();
    }

    /// <summary>
    /// Make the analysis
    /// </summary>
    public override void MakeAnalysis ()
    {
      Debug.Assert (!IsMainModificationTransient ());
      Debug.Assert (!IsAnalysisCompleted ());

      Debug.Assert (null != this.MachineStateTemplate);

      if (Bound.Compare<DateTime> (this.End, this.Begin) < 0) { // Empty period: error
        string message = string.Format ("End={0} before Begin={1}",
                                        this.End, this.Begin);
        SetModificationInError (message);
        return;
      }

      if (this.Range.IsEmpty ()) {
        if (log.IsInfoEnabled) {
          log.Info ($"MakeAnalysis: empty range => nothing to do for modification id={this.Id}");
        }
        MarkAsCompleted ("");
        return;
      }

      // If no dynamic end is set in the association while there is no upper bound in the range,
      // consider the dynamic end of the machine state template.
      // Note: the dynamic end of the association has the priority.
      // This is processed before any sub-modification is created, so that they all inherit it
      if ((null == this.Parent) && !this.End.HasValue
        && string.IsNullOrEmpty (this.DynamicEnd)
        && !string.IsNullOrEmpty (this.MachineStateTemplate.DynamicEnd)) {
        this.Dynamic = $"{this.DynamicStart},{this.MachineStateTemplate.DynamicEnd}";
        if (log.IsDebugEnabled) {
          log.Debug ($"MakeAnalysis: no dynamic end in the association => consider {this.Dynamic} from the machine state template {this.MachineStateTemplate.Id}");
        }
      }

      if ((AnalysisStatus.New == this.AnalysisStatus) && (null == this.Parent)
        && (null != this.MachineStateTemplate) && (LinkDirection.None != this.MachineStateTemplate.LinkOperationDirection)) {
        // Create the associated link operation modification if applicable (not on a sub-modification)
        IMonitoredMachine monitoredMachine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (this.Machine.Id);
        if (null == monitoredMachine) {
          log.Warn ($"MakeAnalysis: no monitored machine with ID {this.Machine.Id} => do not process any LinkOperation");
        }
        else {
          if (log.IsDebugEnabled) {
            log.Debug ($"MakeAnalysis: create link operation {this.MachineStateTemplate.LinkOperationDirection}");
          }
          ILinkOperation linkOperation = ModelDAOHelper.ModelFactory
            .CreateLinkOperation (monitoredMachine, this.MachineStateTemplate.LinkOperationDirection,
                                  this.Range);
          ModelDAOHelper.DAOFactory.LinkOperationDAO.MakePersistent (linkOperation);
          linkOperation.Parent = this.MainModification ?? this;
          linkOperation.Priority = this.Priority;
        }
        MarkAsInProgress (null);
        return;
      }

      if (IsStopProcess ()) { // Stop must be processed first
        UtcDateTimeRange range = new UtcDateTimeRange (this.Range.Lower, GetStop ());
        Debug.Assert (this.Range.ContainsRange (range));
        IMachineStateTemplateAssociation association = ModelDAOHelper.ModelFactory
          .CreateMachineStateTemplateAssociation (this.Machine, this.MachineStateTemplate, range);
        association.NextMachineStateTemplate = this.NextMachineStateTemplate;
        association.DateTime = this.DateTime;
        association.Shift = this.Shift;
        association.User = this.User;
        association.Option = AssociationOption.NoStop.Add (this.Option);
        association.Dynamic = this.Dynamic;
        ModelDAOHelper.DAOFactory.MachineStateTemplateAssociationDAO.MakePersistent (association);
        association.Parent = this.MainModification ?? this;
        association.Priority = this.StatusPriority;

        MarkAsCompleted ("", null);
        return;
      }

      // Handle dynamic times
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

      UtcDateTimeRange endHint = new UtcDateTimeRange ("(,)");
      if (!string.IsNullOrEmpty (this.Dynamic)) {
        if (this.Dynamic.StartsWith ("?")) { // Recompute for optimization the dynamic end
          var dynamicEnd = this.DynamicEnd;
          var notProcessedRange = GetNotPocessedRange ();
          if (notProcessedRange.IsEmpty ()) {
            if (log.IsFatalEnabled) {
              log.Fatal ("MakeAnalysis: (unexpected) GetNotPocessedRange returned an empty range => completed");
            }
            Debug.Assert (false);
            ForceAsDone (""); // To skip the process of the sub-modifications
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
            log.Error ($"MakeAnalysis: unknown dynamic end {dynamicEnd}");
            SetModificationInError ("Unknown dynamic end");
            return;
          }
          Debug.Assert (null != dynamicEndResponse);
          if (dynamicEndResponse.Timeout) {
            if (log.IsWarnEnabled) {
              log.Warn ($"MakeAnalysis: Timeout for dynamic end {dynamicEnd} range {this.Range} hint {hint} => not applicable");
            }
            MarkDynamicTimeNotApplicable ($"Timeout for the dynamic end {dynamicEnd} on {this.Range} => the part of {this.Range} that was already applied for the machine state template {this.MachineStateTemplate} is kept, it is not reverted");
            return;
          }
          else if (dynamicEndResponse.NotApplicable) {
            if (log.IsDebugEnabled) {
              log.Debug ($"MakeAnalysis: NotApplicable for dynamic end {dynamicEnd} range {this.Range} hint {hint} => not applicable");
            }
            MarkDynamicTimeNotApplicable ($"The dynamic end {dynamicEnd} is not applicable on {this.Range} => the part of {this.Range} that was already applied for the machine state template {this.MachineStateTemplate} is kept, it is not reverted");
            return;
          }
          else if (dynamicEndResponse.NoData) {
            if (log.IsDebugEnabled) {
              log.Debug ($"MakeAnalysis: no data for dynamic end {dynamicEnd} range {this.Range} hint {hint} => not applicable");
            }
            MarkDynamicTimeNotApplicable ($"No data for the dynamic end {dynamicEnd} on {this.Range} => the part of {this.Range} that was already applied for the machine state template {this.MachineStateTemplate} is kept, it is not reverted");
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

      if (!IsProcessByPeriod ()) { // Apply directly the changes, do not split it by period
        // Get the adjusted step range
        UtcDateTimeRange range = GetNotAppliedRange ();
        UtcDateTimeRange restrictedRange = range;
        if (endHint.Upper.HasValue
          && (Bound.Compare<DateTime> (endHint.Upper, range.Upper) < 0)) {
          restrictedRange = new UtcDateTimeRange (range.Lower, endHint.Upper);
        }
        if (log.IsDebugEnabled) {
          log.Debug ($"MakeAnalysis: consider step {range} restricted={restrictedRange}");
        }
        if (restrictedRange.IsEmpty ()) { // Already completed
          MarkAsCompleted ("");
        }
        else { // restrictedRange not empty
          // Analyze ! with the adjusted begin and end
          MachineStateTemplateAssociation association = new MachineStateTemplateAssociation (this.Machine,
                                                                                             this.MachineStateTemplate,
                                                                                             restrictedRange,
                                                                                             this);
          association.NextMachineStateTemplate = this.NextMachineStateTemplate;
          association.Option = this.Option;
          association.DateTime = this.DateTime;
          association.User = this.User;
          association.Force = this.Force;
          association.Shift = this.Shift;
          association.Dynamic = this.Dynamic;
          association.Caller = this;
          association.Analyze ();

          // Analysis is done
          MarkAsCompleted ("Cache/ClearDomainByMachine/MachineStateTemplateAssociation/" + this.Machine.Id + "?Broadcast=true",
                           (DateTime?)range.Upper); // => InProgress or Done
        }
      }
      else { // Try to split the process by periods where a change is really required
        IEnumerable<IWithRange> noChangeRequiredSlots;
        if ((null != this.Shift) || (null != this.User)) {
          noChangeRequiredSlots = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
            .FindMatchingMachineStateTemplateAssociation (this.Machine, this.Range, this.MachineStateTemplate,
                                                          this.Shift, this.User)
            .Cast<IWithRange> ();
        }
        else { // Optimization if the shift and user is not used: use directly the machinestatetemplateslots
          noChangeRequiredSlots = ModelDAOHelper.DAOFactory.MachineStateTemplateSlotDAO
            .FindOverlapsRangeMatchingMachineStateTemplate (this.Machine, this.Range, this.MachineStateTemplate)
            .Cast<IWithRange> ();
        }
        bool associationCreated = false;
        Bound<DateTime> lower = this.Range.Lower;
        foreach (IWithRange noChangeRequiredSlot in noChangeRequiredSlots) {
          if (Bound.Compare<DateTime> (lower, noChangeRequiredSlot.DateTimeRange.Lower) < 0) {
            Debug.Assert (noChangeRequiredSlot.DateTimeRange.Lower.HasValue);
            Debug.Assert (Bound.Compare<DateTime> (noChangeRequiredSlot.DateTimeRange.Lower, this.Range.Upper) <= 0);
            UtcDateTimeRange range = new UtcDateTimeRange ((LowerBound<DateTime>)lower,
                                                           noChangeRequiredSlot.DateTimeRange.Lower.Value);
            Debug.Assert (this.Range.ContainsRange (range));
            IMachineStateTemplateAssociation association = ModelDAOHelper.ModelFactory
              .CreateMachineStateTemplateAssociation (this.Machine, this.MachineStateTemplate, range);
            association.NextMachineStateTemplate = this.NextMachineStateTemplate;
            association.DateTime = this.DateTime;
            association.Shift = this.Shift;
            association.User = this.User;
            association.Force = this.Force;
            association.Option = AssociationOption.NotByPeriod.Add (this.Option);
            association.Dynamic = this.Dynamic;
            ModelDAOHelper.DAOFactory.MachineStateTemplateAssociationDAO.MakePersistent (association);
            association.Parent = this.MainModification ?? this;
            association.Priority = this.StatusPriority;
            associationCreated = true;
          }
          Debug.Assert (Bound.Compare<DateTime> (this.Range.Lower, noChangeRequiredSlot.DateTimeRange.Upper) < 0);
          Debug.Assert (Bound.Compare<DateTime> (lower, noChangeRequiredSlot.DateTimeRange.Upper) < 0);
          lower = noChangeRequiredSlot.DateTimeRange.Upper;
        }
        if (Bound.Compare<DateTime> (lower, this.Range.Upper) < 0) {
          UtcDateTimeRange range = new UtcDateTimeRange ((LowerBound<DateTime>)lower,
                                                         this.Range.Upper);
          Debug.Assert (this.Range.ContainsRange (range));
          IMachineStateTemplateAssociation association = ModelDAOHelper.ModelFactory
            .CreateMachineStateTemplateAssociation (this.Machine, this.MachineStateTemplate, range);
          association.NextMachineStateTemplate = this.NextMachineStateTemplate;
          association.DateTime = this.DateTime;
          association.Shift = this.Shift;
          association.User = this.User;
          association.Force = this.Force;
          association.Option = AssociationOption.NotByPeriod.Add (this.Option);
          association.Dynamic = this.Dynamic;
          ModelDAOHelper.DAOFactory.MachineStateTemplateAssociationDAO.MakePersistent (association);
          association.Parent = this.MainModification ?? this;
          association.Priority = this.StatusPriority;
          associationCreated = true;
        }

        if (!associationCreated) {
          if (log.IsDebugEnabled) {
            log.Debug ("MakeAnalysis: no change was required");
          }
        }
        MarkAsCompleted ("", null);
      }
    }

    void MakeAnalysisDynamicStart (string dynamicStart)
    {
      if (!this.Begin.HasValue) {
        SetModificationInError ("Incompatible properties Begin==null VS DynamicStart");
        return;
      }
      if (this.Option.HasValue && this.Option.Value.HasFlag (AssociationOption.CancelAfterEnd)) {
        MakeAnalysisDynamicEndTrackerWithLimit (dynamicStart);
        return;
      }
      try {
        IDynamicTimeResponse dynamicStartResponse = Lemoine.Business.DynamicTimes.DynamicTime
         .GetDynamicTime (dynamicStart, this.Machine, this.Range.Lower.Value);
        Debug.Assert (null != dynamicStartResponse);
        if (dynamicStartResponse.Timeout) {
          if (log.IsWarnEnabled) {
            log.Warn ($"MakeAnalysisDynamicStart: Timeout for dynamic start {dynamicStart} at {this.Range.Lower} => mark as pending, try again later");
          }
          MarkAsPending (null); // Not sure what's the best option, try again later,
          return;
        }
        else if (dynamicStartResponse.NotApplicable) {
          if (log.IsWarnEnabled) {
            GetLogger ().Warn ($"MakeAnalysisDynamicStart: not applicable for dynamic start {dynamicStart} => {this.MachineStateTemplate} is not applied");
          }
          MarkDynamicTimeNotApplicable ($"The dynamic start {dynamicStart} is not applicable => the machine state template {this.MachineStateTemplate} is not applied on {this.Range}, so the machine state template that is effective at {this.Range.Lower} remains effective afterwards");
        }
        else if (dynamicStartResponse.NoData) {
          GetLogger ().Error ($"MakeAnalysisDynamicStart: no data for dynamic start {dynamicStart} => {this.MachineStateTemplate} is not applied");
          MarkDynamicTimeNotApplicable ($"No data for the dynamic start {dynamicStart} => the machine state template {this.MachineStateTemplate} is not applied on {this.Range}, so the machine state template that is effective at {this.Range.Lower} remains effective afterwards");
        }
        else if (dynamicStartResponse.Final.HasValue) {
          var start = dynamicStartResponse.Final.Value;
          var range = new UtcDateTimeRange (start, this.End);
          if (!range.IsEmpty ()) {
            var subModification = this.Clone (range);
            subModification.Dynamic = "," + this.DynamicEnd;
            subModification = ModelDAOHelper.DAOFactory.MachineStateTemplateAssociationDAO.MakePersistent (subModification);
            subModification.Parent = this.MainModification ?? this;
            subModification.Priority = this.Priority;
          }
          MarkAsCompleted ("");
        }
        else { // !dynamicStartResponse.Start.HasValue => Pending
          if (GetLogger ().IsDebugEnabled) {
            GetLogger ().Debug ($"MakeAnalysisDynamicStart: the dynamic start {dynamicStart} is not known yet");
          }
          MarkAsPending (null);
        }
      }
      catch (NoDynamicTime) {
        GetLogger ().Error ($"MakeAnalysisDynamicStart: no dynamic start {dynamicStart}");
        SetModificationInError ("Dynamic start " + dynamicStart + " unknown");
      }
    }

    /// <summary>
    /// Dynamic end tracker with the option <see cref="AssociationOption.CancelAfterEnd"/>
    ///
    /// The tracked dynamic end is the dynamic start of the tracker and the end of the range of the tracker is a limit:
    /// <item>dynamic end before or at the limit: apply the next machine state template (of the tracker) from the dynamic end, with no upper bound</item>
    /// <item>dynamic end after the limit, or never reached: cancel the machine state template that was applied aggressively</item>
    /// </summary>
    /// <param name="trackedDynamicEnd">dynamic end that is tracked, not null or empty</param>
    void MakeAnalysisDynamicEndTrackerWithLimit (string trackedDynamicEnd)
    {
      Debug.Assert (this.Begin.HasValue);
      if (!this.End.HasValue) {
        SetModificationInError ("Incompatible properties End=null VS option CancelAfterEnd");
        return;
      }

      IDynamicTimeResponse response;
      try {
        // Note: the upper limit is inclusive, so that a dynamic end at the limit is accepted
        var limit = new UtcDateTimeRange (this.Range.Lower, this.Range.Upper, true, true);
        response = Lemoine.Business.DynamicTimes.DynamicTime
          .GetDynamicTime (trackedDynamicEnd, this.Machine, this.Range.Lower.Value, new UtcDateTimeRange ("(,)"), limit);
      }
      catch (NoDynamicTime) {
        GetLogger ().Error ($"MakeAnalysisDynamicEndTrackerWithLimit: no dynamic time {trackedDynamicEnd}");
        SetModificationInError ("Dynamic end " + trackedDynamicEnd + " unknown");
        return;
      }
      Debug.Assert (null != response);

      if (response.Timeout) {
        if (log.IsWarnEnabled) {
          log.Warn ($"MakeAnalysisDynamicEndTrackerWithLimit: Timeout for the dynamic end {trackedDynamicEnd} at {this.Range.Lower} => try again later");
        }
        MarkAsPending (null);
      }
      else if (response.NotApplicable) {
        CancelTrackedMachineStateTemplate ($"The dynamic end {trackedDynamicEnd} is not applicable at {this.Range.Lower}");
      }
      else if (response.NoData) {
        CancelTrackedMachineStateTemplate ($"The dynamic end {trackedDynamicEnd} is not reached before the end {this.End}");
      }
      else if (response.Final.HasValue) {
        var dynamicEnd = response.Final.Value;
        if (Bound.Compare<DateTime> (this.End, dynamicEnd) < 0) {
          CancelTrackedMachineStateTemplate ($"The dynamic end {trackedDynamicEnd} at {dynamicEnd} is after the end {this.End}");
        }
        else {
          if (log.IsDebugEnabled) {
            log.Debug ($"MakeAnalysisDynamicEndTrackerWithLimit: dynamic end {dynamicEnd} before the end {this.End} => apply {this.MachineStateTemplate} from it");
          }
          var subModification = this.Clone (new UtcDateTimeRange (dynamicEnd));
          subModification.Dynamic = null;
          subModification.Option = subModification.Option?.Remove (AssociationOption.CancelAfterEnd);
          subModification = ModelDAOHelper.DAOFactory.MachineStateTemplateAssociationDAO.MakePersistent (subModification);
          subModification.Parent = this.MainModification ?? this;
          subModification.Priority = this.Priority;
          MarkAsCompleted ("");
        }
      }
      else if (response.Hint.Lower.HasValue && (Bound.Compare<DateTime> (this.End, response.Hint.Lower.Value) < 0)) {
        CancelTrackedMachineStateTemplate ($"The dynamic end {trackedDynamicEnd} is after {response.Hint.Lower}, after the end {this.End}");
      }
      else { // Pending
        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().Debug ($"MakeAnalysisDynamicEndTrackerWithLimit: the dynamic end {trackedDynamicEnd} is not known yet, hint={response.Hint}");
        }
        MarkAsPending (response.Hint.Lower.HasValue ? response.Hint.Lower.Value : (DateTime?)null);
      }
    }

    /// <summary>
    /// Cancel the machine state template that was applied aggressively on the range of this dynamic end tracker
    ///
    /// The machine state template (or the machine observation state if there was no machine state template)
    /// that was effective just before the range is restored, only where the tracked machine state template
    /// is still effective, and only up to the next association that was created afterwards
    /// </summary>
    /// <param name="reason">reason of the cancellation, recorded in the analysis logs</param>
    void CancelTrackedMachineStateTemplate (string reason)
    {
      Debug.Assert (this.Begin.HasValue);
      Debug.Assert (this.End.HasValue);

      var trackedMachineStateTemplate = ((this.Parent as IMachineStateTemplateAssociation)
        ?? (this.MainModification as IMachineStateTemplateAssociation))?.MachineStateTemplate;
      if (trackedMachineStateTemplate is null) {
        log.Error ($"CancelTrackedMachineStateTemplate: no tracked machine state template for the tracker {this.Id}");
        MarkDynamicTimeNotApplicable ($"{reason} => the machine state template should be cancelled on {this.Range}, but the tracked machine state template is unknown");
        return;
      }

      // Restrict the cancellation up to the next association that was created afterwards
      var mainModificationId = (this.MainModification ?? this).Id;
      var restoreRange = new UtcDateTimeRange (this.Begin.Value, this.End.Value);
      var laterAssociations = ModelDAOHelper.DAOFactory.MachineStateTemplateAssociationDAO
        .FindByMachineAndPeriod (this.Machine, this.Begin.Value, this.End.Value)
        .Where (a => mainModificationId < a.Id);
      foreach (var laterAssociation in laterAssociations) {
        var laterStart = Bound.Compare<DateTime> (laterAssociation.Begin, restoreRange.Lower) < 0
          ? restoreRange.Lower.Value
          : laterAssociation.Begin.Value;
        if (Bound.Compare<DateTime> (laterStart, restoreRange.Upper) < 0) {
          restoreRange = new UtcDateTimeRange (restoreRange.Lower, laterStart);
        }
      }
      if (restoreRange.IsEmpty ()) {
        MarkDynamicTimeNotApplicable ($"{reason} => the machine state template {trackedMachineStateTemplate} should be cancelled on {this.Range}, but a more recent association already applies");
        return;
      }

      var previousSlot = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
        .FindWithEnd (this.Machine, this.Begin.Value);
      if (previousSlot is null) {
        MarkDynamicTimeNotApplicable ($"{reason} => the machine state template {trackedMachineStateTemplate} should be cancelled on {restoreRange}, but nothing was effective just before, so it is kept");
        return;
      }
      if (trackedMachineStateTemplate.Equals (previousSlot.MachineStateTemplate)) {
        MarkDynamicTimeNotApplicable ($"{reason} => the machine state template {trackedMachineStateTemplate} was already effective before {this.Begin}, so it is kept");
        return;
      }
      if ((previousSlot.MachineStateTemplate is null) && (previousSlot.MachineObservationState is null)) {
        MarkDynamicTimeNotApplicable ($"{reason} => the machine state template {trackedMachineStateTemplate} should be cancelled on {restoreRange}, but nothing was effective just before, so it is kept");
        return;
      }

      // Only where the tracked machine state template is still effective
      var trackedRanges = ModelDAOHelper.DAOFactory.MachineStateTemplateSlotDAO
        .FindOverlapsRangeMatchingMachineStateTemplate (this.Machine, restoreRange, trackedMachineStateTemplate)
        .Select (s => new UtcDateTimeRange (s.DateTimeRange.Intersects (restoreRange)))
        .Where (r => !r.IsEmpty ())
        .ToList ();
      foreach (var range in trackedRanges) {
        IMachineModification restoreModification;
        if (null != previousSlot.MachineStateTemplate) {
          var association = ModelDAOHelper.ModelFactory
            .CreateMachineStateTemplateAssociation (this.Machine, previousSlot.MachineStateTemplate, range);
          association.DateTime = this.DateTime;
          restoreModification = ModelDAOHelper.DAOFactory.MachineStateTemplateAssociationDAO.MakePersistent (association);
        }
        else {
          var association = ModelDAOHelper.ModelFactory
            .CreateMachineObservationStateAssociation (this.Machine, previousSlot.MachineObservationState, range);
          association.User = previousSlot.User;
          association.Shift = previousSlot.Shift;
          association.DateTime = this.DateTime;
          restoreModification = ModelDAOHelper.DAOFactory.MachineObservationStateAssociationDAO.MakePersistent (association);
        }
        restoreModification.Parent = this.MainModification ?? this;
        restoreModification.Priority = this.StatusPriority;
      }

      var restored = (null != previousSlot.MachineStateTemplate)
        ? $"the machine state template {previousSlot.MachineStateTemplate}"
        : $"the machine observation state {previousSlot.MachineObservationState}";
      var message = $"{reason} => the machine state template {trackedMachineStateTemplate} is cancelled: {restored} that was effective before {this.Begin} is restored on {string.Join (", ", trackedRanges)}";
      if (trackedRanges.Any ()) {
        // Note: not MarkAsNotApplicable here, else the restore sub-modifications would not be processed (ParentInError)
        AddAnalysisLog (LogLevel.WARN, message);
        MarkAsCompleted ("");
      }
      else {
        MarkDynamicTimeNotApplicable (message);
      }
    }

    void MakeAnalysisDynamicEnd (string dynamicEnd)
    {
      if (!this.AnalysisAppliedDateTime.HasValue) { // first time
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
      else { // Aggressive strategy (default)
        MakeAnalysisDynamicEndAggressive (dynamicEnd);
      }
    }

    void MakeAnalysisDynamicEndProgressive (string dynamicEnd)
    {
      var notProcessedRange = GetNotPocessedRange ();
      if (notProcessedRange.IsEmpty ()) {
        if (log.IsDebugEnabled) {
          log.Debug ("MakeAnalysisDynamicEndProgressive: GetNotPocessedRange returned an empty range => completed");
        }
        MarkAsCompleted ("");
        return;
      }

      Debug.Assert (notProcessedRange.Lower.HasValue);
      IDynamicTimeResponse dynamicEndResponse;
      try {
        Debug.Assert (!notProcessedRange.IsEmpty ());
        var limit = new UtcDateTimeRange (notProcessedRange.Lower, notProcessedRange.Upper, true, true);
        dynamicEndResponse = Lemoine.Business.DynamicTimes.DynamicTime
          .GetDynamicTime (dynamicEnd, this.Machine, this.DynamicTimeRange, new UtcDateTimeRange (notProcessedRange.Lower.Value), limit);
      }
      catch (NoDynamicTime) {
        log.Error ($"MakeAnalysisDynamicEndProgressive: unknown dynamic end {dynamicEnd}");
        SetModificationInError ("Dynamic end " + dynamicEnd + " unknown");
        return;
      }
      SetActive ();
      Debug.Assert (null != dynamicEndResponse);

      if (dynamicEndResponse.Timeout) {
        if (log.IsWarnEnabled) {
          log.Warn ($"MakeAnalysisDynamicEndProgressive: Timeout is returned for {dynamicEnd} at {this.DynamicTimeRange} => mark as not applicable");
        }
        MarkDynamicTimeNotApplicable ($"Timeout for the dynamic end {dynamicEnd} at {this.DynamicTimeRange} => the part of {this.Range} that was already applied for the machine state template {this.MachineStateTemplate} is kept, it is not reverted");
        return;
      }
      else if (dynamicEndResponse.NotApplicable) {
        if (log.IsDebugEnabled) {
          log.Debug ($"MakeAnalysisDynamicEndProgressive: NotApplicable is returned for {dynamicEnd} at {this.DynamicTimeRange} => mark as not applicable");
        }
        MarkDynamicTimeNotApplicable ($"The dynamic end {dynamicEnd} is not applicable at {this.DynamicTimeRange} => the part of {this.Range} that was already applied for the machine state template {this.MachineStateTemplate} is kept, it is not reverted");
        return;
      }
      else if (dynamicEndResponse.NoData) {
        if (log.IsDebugEnabled) {
          log.Debug ($"MakeAnalysisDynamicEndProgressive: NoData is returned for {dynamicEnd} at {this.DynamicTimeRange} => apply from start");
        }
        ApplyDynamicEnd (notProcessedRange.Lower.Value);
        return;
      }
      else if (dynamicEndResponse.Final.HasValue) {
        ApplyDynamicEnd (dynamicEndResponse.Final.Value);
        return;
      }
      else {
        if (dynamicEndResponse.Hint.Lower.HasValue) {
          MarkAsPending (dynamicEndResponse.Hint.Lower.Value);
        }
        else {
          MarkAsPending (this.AnalysisAppliedDateTime);
        }
        return;
      }
    }

    void MakeAnalysisDynamicEndAggressive (string dynamicEnd)
    {
      if (log.IsDebugEnabled) {
        log.Debug ("MakeAnalysisDynamicEndAggressive");
      }

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
        log.Error ($"MakeAnalysisDynamicEndAggressive: No dynamic end {dynamicEnd}");
        SetModificationInError ("Dynamic end " + dynamicEnd + " unknown");
        return;
      }
      SetActive ();
      Debug.Assert (null != dynamicEndResponse);
      
      if (dynamicEndResponse.Timeout) {
        if (log.IsWarnEnabled) {
          log.Warn ($"MakeAnalysisDynamicEndAggressive: Timeout is returned for {dynamicEnd} at {this.DynamicTimeRange} => mark as not applicable");
        }
        MarkDynamicTimeNotApplicable ($"Timeout for the dynamic end {dynamicEnd} at {this.DynamicTimeRange} => the machine state template {this.MachineStateTemplate} is not applied on {this.Range}");
        return;
      }
      else if (dynamicEndResponse.NotApplicable) {
        if (log.IsDebugEnabled) {
          log.Debug ($"MakeAnalysisDynamicEndAggressive: NotApplicable is returned for {dynamicEnd} at {this.DynamicTimeRange} => mark as not applicable");
        }
        MarkDynamicTimeNotApplicable ($"The dynamic end {dynamicEnd} is not applicable at {this.DynamicTimeRange} => the machine state template {this.MachineStateTemplate} is not applied on {this.Range}");
        return;
      }
      else if (dynamicEndResponse.NoData) {
        if (log.IsDebugEnabled) {
          log.Debug ($"MakeAnalysisDynamicEndAggressive: NoData is returned for {dynamicEnd} at {this.DynamicTimeRange} => mark as not applicable");
        }
        MarkDynamicTimeNotApplicable ($"No data for the dynamic end {dynamicEnd} at {this.DynamicTimeRange} => the machine state template {this.MachineStateTemplate} is not applied on {this.Range}");
        return;
      }
      else if (dynamicEndResponse.Final.HasValue) {
        // The dynamic end is known at once: no dynamic end tracker is created here,
        // so the next machine state template must be applied directly, so that the result
        // does not depend on the moment the dynamic end is known
        ApplyDynamicEnd (dynamicEndResponse.Final.Value, applyNextMachineStateTemplate: true);
        return;
      }
      else {
        if (!this.Range.Overlaps (dynamicEndResponse.Hint)
          && this.Option.HasValue && this.Option.Value.HasFlag (AssociationOption.DynamicEndBeforeRealEnd)) {
          MarkDynamicTimeNotApplicable ($"The dynamic end {dynamicEnd} is out of {this.Range} while the option DynamicEndBeforeRealEnd is set => the machine state template {this.MachineStateTemplate} is not applied on {this.Range}");
          return;
        }

        // With the option DynamicEndBeforeRealEnd, the machine state template that is applied aggressively
        // must be cancelled if the dynamic end is finally after the end:
        // this is the dynamic end tracker that takes care of it, with the option CancelAfterEnd
        var cancelAfterEnd = this.Option.HasValue && this.Option.Value.HasFlag (AssociationOption.DynamicEndBeforeRealEnd)
          && this.End.HasValue && !this.Dynamic.EndsWith ("+");
        if (cancelAfterEnd && (GetNextMachineStateTemplate (this) is null)) {
          // No dynamic end tracker is possible without a next machine state template:
          // do not apply the machine state template aggressively, wait for the dynamic end instead
          if (log.IsDebugEnabled) {
            log.Debug ($"MakeAnalysisDynamicEndAggressive: option DynamicEndBeforeRealEnd and no next machine state template => wait for the dynamic end, hint={dynamicEndResponse.Hint}");
          }
          MarkAsPending (dynamicEndResponse.Hint.Lower.HasValue ? dynamicEndResponse.Hint.Lower.Value : (DateTime?)null);
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
          
          // Two sub-modifications: one to apply the machine state template aggressively, one to track the dynamic end
          // Apply the machine state template aggressively
          var aggressiveSubModification = ModelDAOHelper.DAOFactory.MachineStateTemplateAssociationDAO
            .InsertSub (this, range, association => MakeAnalysisDynamicEndAggressiveSubChangeAggressive (dynamicEnd, association, removeDynamicPlus), null);
          
          { // One to track the dynamic end
            if (!dynamicEndResponse.Hint.Overlaps (this.Range)) {
              if (log.IsDebugEnabled) {
                log.Debug ("MakeAnalysisDynamicEndAggressive: hint does not overlap range => no DynamicEndTracker");
              }
            }
            else if (GetNextMachineStateTemplate (this) is null) {
              if (log.IsWarnEnabled) {
                log.Warn ($"MakeAnalysisDynamicEndAggressive: no next machine state template for {this.MachineStateTemplate} => no DynamicEndTracker");
              }
            }
            else {
              // With the option CancelAfterEnd, the end of the range of the tracker is the limit
              // after which the machine state template is cancelled
              var trackerRange = this.Dynamic.EndsWith ("+")
                ? new UtcDateTimeRange (range.Upper.Value)
                : (cancelAfterEnd
                  ? new UtcDateTimeRange (range.Lower.Value, this.End.Value)
                  : new UtcDateTimeRange (range.Lower.Value));
              var trackDynamicEndModification = ModelDAOHelper.DAOFactory.MachineStateTemplateAssociationDAO
                .InsertSub (this, trackerRange, association => MakeAnalysisDynamicEndAggressiveSubChangeDynamicEndTracker (dynamicEnd, dynamicEndResponse.Hint, aggressiveSubModification, association, removeDynamicPlus, cancelAfterEnd), aggressiveSubModification);
              if (dynamicEndResponse.Hint.Lower.HasValue) {
                ((MachineStateTemplateAssociation)trackDynamicEndModification).MarkAsInProgress (dynamicEndResponse.Hint.Lower.Value);
              }
            }
          }
        } // !range.IsEmpty ()
        MarkAsCompleted ("");
        return;
      }
    }

    static void MakeAnalysisDynamicEndAggressiveSubChangeAggressive (string dynamicEnd, IMachineStateTemplateAssociation association, bool removeDynamicPlus)
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

    /// <summary>
    /// Get the machine state template to apply once the dynamic end is reached
    ///
    /// The next machine state template of the association has the priority on the one
    /// of the machine state template, then on the default machine state template of the machine
    /// </summary>
    /// <param name="association">not null</param>
    /// <returns>nullable</returns>
    static IMachineStateTemplate GetNextMachineStateTemplate (IMachineStateTemplateAssociation association) =>
      association.NextMachineStateTemplate
      ?? association.MachineStateTemplate?.NextMachineStateTemplate
      ?? association.Machine?.DefaultMachineStateTemplate;

    static void MakeAnalysisDynamicEndAggressiveSubChangeDynamicEndTracker (string dynamicEnd, UtcDateTimeRange hint, IMachineModification parent, IMachineStateTemplateAssociation association, bool removeDynamicPlus, bool cancelAfterEnd)
    {
      if (cancelAfterEnd) {
        association.Option = AssociationOption.CancelAfterEnd.Add (association.Option);
      }
      var nextMachineStateTemplate = GetNextMachineStateTemplate (association);
      association.Dynamic = $"{dynamicEnd},";
      association.MachineStateTemplate = nextMachineStateTemplate;
      // The next machine state template was consumed here, do not apply it once again later
      association.NextMachineStateTemplate = null;
      association.Priority = Lemoine.Info.ConfigSet
        .LoadAndGet (DYNAMIC_END_TRACKER_PRIORITY_KEY, DYNAMIC_END_TRACKER_PRIORITY_DEFAULT);
      // Remove the option TrackSlotChanges
      if (association.Option.HasValue && association.Option.Value.HasFlag (AssociationOption.TrackSlotChanges)) {
        association.Option = association.Option.Value
          .Remove (AssociationOption.TrackSlotChanges);
      }
    }

    /// <summary>
    /// Apply the machine state template up to the dynamic end
    /// </summary>
    /// <param name="dynamicApplicableEnd">effective dynamic end</param>
    /// <param name="applyNextMachineStateTemplate">also apply the next machine state template from the dynamic end.
    /// To set only when no dynamic end tracker takes care of it</param>
    void ApplyDynamicEnd (DateTime dynamicApplicableEnd, bool applyNextMachineStateTemplate = false)
    {
      if ((Bound.Compare<DateTime> (this.End, dynamicApplicableEnd) < 0)
        && this.Option.HasValue && this.Option.Value.HasFlag (AssociationOption.DynamicEndBeforeRealEnd)) {
        // Note: applyNextMachineStateTemplate is set by the aggressive strategy when the dynamic end is known at once:
        // nothing was applied yet then
        var consequence = applyNextMachineStateTemplate
          ? $"the machine state template {this.MachineStateTemplate} is not applied on {this.Range}"
          : $"the part of {this.Range} that was already applied for the machine state template {this.MachineStateTemplate} is kept, it is not reverted";
        MarkDynamicTimeNotApplicable ($"The dynamic end {dynamicApplicableEnd} is after the end {this.End} while the option DynamicEndBeforeRealEnd is set => {consequence}");
        return;
      }

      var range = this.GetNotPocessedRange ();
      if (range.IsEmpty ()) {
        if (log.IsFatalEnabled) {
          log.Fatal ("ApplyDynamicEnd: (unexpected) GetNotProcessedRange is empty => nothing to do");
        }
        Debug.Assert (false);
        ForceAsDone (""); // To skip the process of the sub-modifications
        return;
      }

      UtcDateTimeRange restriction = new UtcDateTimeRange (new LowerBound<DateTime> (null), dynamicApplicableEnd);
      UtcDateTimeRange restrictedRange = new UtcDateTimeRange (range.Intersects (restriction));
      if (restrictedRange.IsEmpty ()) {
        if (log.IsDebugEnabled) {
          log.Debug ("ApplyDynamicEnd: restricted range is empty => nothing to do");
        }
      }
      else {
        log.Info ($"ApplyDynamicEnd: restrict {this} to {dynamicApplicableEnd}");
        ModelDAOHelper.DAOFactory.MachineStateTemplateAssociationDAO
          .InsertSub (this, restrictedRange, ApplyDynamicEndSubChange, null);
      }
      if (applyNextMachineStateTemplate) {
        ApplyNextMachineStateTemplate (dynamicApplicableEnd);
      }
      MarkAsCompleted ("");
    }

    void ApplyDynamicEndSubChange (IMachineStateTemplateAssociation association)
    {
      association.Dynamic = null;
    }

    /// <summary>
    /// Insert a sub-modification to apply the next machine state template from the dynamic end
    ///
    /// This is what the dynamic end tracker does when the dynamic end is known only later:
    /// the result of the analysis does not depend then on the moment the dynamic end is known
    /// </summary>
    /// <param name="dynamicApplicableEnd">effective dynamic end</param>
    void ApplyNextMachineStateTemplate (DateTime dynamicApplicableEnd)
    {
      var nextMachineStateTemplate = GetNextMachineStateTemplate (this);
      if (nextMachineStateTemplate is null) {
        if (log.IsDebugEnabled) {
          log.Debug ($"ApplyNextMachineStateTemplate: no next machine state template for {this.MachineStateTemplate} => nothing to apply from {dynamicApplicableEnd}");
        }
        return;
      }

      if (log.IsDebugEnabled) {
        log.Debug ($"ApplyNextMachineStateTemplate: apply {nextMachineStateTemplate} from {dynamicApplicableEnd}");
      }
      ModelDAOHelper.DAOFactory.MachineStateTemplateAssociationDAO
        .InsertSub (this, new UtcDateTimeRange (dynamicApplicableEnd),
                    association => ApplyNextMachineStateTemplateSubChange (nextMachineStateTemplate, association), null);
    }

    static void ApplyNextMachineStateTemplateSubChange (IMachineStateTemplate nextMachineStateTemplate, IMachineStateTemplateAssociation association)
    {
      association.MachineStateTemplate = nextMachineStateTemplate;
      // The next machine state template was consumed here, do not apply it once again later
      association.NextMachineStateTemplate = null;
      // The dynamic end was already processed
      association.Dynamic = null;
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
        if (this.Dynamic.EndsWith ("+")) {
          return new UtcDateTimeRange (this.AnalysisAppliedDateTime.HasValue
                                       ? this.AnalysisAppliedDateTime.Value
                                       : this.Begin,
                                       new UpperBound<DateTime> (null));
        }
      }

      return base.GetNotPocessedRange ();
    }

    /// <summary>
    /// Apply the modifications
    /// 
    /// Note: the link operation process it not processed here
    /// It is processed only in persistent modifications
    /// </summary>
    public override void Apply ()
    {
      Debug.Assert (string.IsNullOrEmpty (this.Dynamic));
      if (!string.IsNullOrEmpty (this.Dynamic)) {
        log.Fatal ($"Apply: association {this} had a dynamic value {this.Dynamic}, which is unexpected");
      }

      if (IsStopProcess ()) { // Stop must be processed first
        UtcDateTimeRange range = new UtcDateTimeRange (this.Range.Lower, GetStop ());
        Debug.Assert (this.Range.ContainsRange (range));
        var association = new MachineStateTemplateAssociation (this.Machine,
                                                               this.MachineStateTemplate,
                                                               range,
                                                               this.MainModification);
        association.NextMachineStateTemplate = this.NextMachineStateTemplate;
        association.DateTime = this.DateTime;
        association.Shift = this.Shift;
        association.User = this.User;
        association.Force = this.Force;
        association.Option = AssociationOption.NoStop.Add (this.Option);
        association.Caller = this;
        association.Apply ();
        return;
      }

      if (!this.IsProcessByPeriod ()) { // Apply directly the changes, do not split it by period
        this.Analyze ();
      }
      else { // Try to split the process by periods where a change is really required
        // Note: use MachineStateTemplateSlot instead when it will be implemented
        IList<IObservationStateSlot> noChangeRequiredSlots = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
          .FindMatchingMachineStateTemplateAssociation (this.Machine, this.Range, this.MachineStateTemplate,
                                                        this.Shift, this.User);
        Bound<DateTime> lower = this.Range.Lower;
        foreach (var noChangeRequiredSlot in noChangeRequiredSlots) {
          if (Bound.Compare<DateTime> (lower, noChangeRequiredSlot.DateTimeRange.Lower) < 0) {
            Debug.Assert (noChangeRequiredSlot.DateTimeRange.Lower.HasValue);
            Debug.Assert (Bound.Compare<DateTime> (noChangeRequiredSlot.DateTimeRange.Lower, this.Range.Upper) <= 0);
            UtcDateTimeRange range = new UtcDateTimeRange ((LowerBound<DateTime>)lower,
                                                           noChangeRequiredSlot.DateTimeRange.Lower.Value);
            Debug.Assert (this.Range.ContainsRange (range));
            var association = new MachineStateTemplateAssociation (this.Machine,
                                                                   this.MachineStateTemplate,
                                                                   range,
                                                                   this.MainModification);
            association.NextMachineStateTemplate = this.NextMachineStateTemplate;
            association.DateTime = this.DateTime;
            association.Shift = this.Shift;
            association.User = this.User;
            association.Force = this.Force;
            association.Option = AssociationOption.NotByPeriod.Add (this.Option);
            association.Caller = this;
            association.Analyze ();
          }
          Debug.Assert (Bound.Compare<DateTime> (this.Range.Lower, noChangeRequiredSlot.DateTimeRange.Upper) < 0);
          Debug.Assert (Bound.Compare<DateTime> (lower, noChangeRequiredSlot.DateTimeRange.Upper) < 0);
          lower = noChangeRequiredSlot.DateTimeRange.Upper;
        }
        if (Bound.Compare<DateTime> (lower, this.Range.Upper) < 0) {
          UtcDateTimeRange range = new UtcDateTimeRange ((LowerBound<DateTime>)lower,
                                                         this.Range.Upper);
          Debug.Assert (this.Range.ContainsRange (range));
          var association = new MachineStateTemplateAssociation (this.Machine,
                                                                 this.MachineStateTemplate,
                                                                 range,
                                                                 this.MainModification);
          association.NextMachineStateTemplate = this.NextMachineStateTemplate;
          association.DateTime = this.DateTime;
          association.Shift = this.Shift;
          association.User = this.User;
          association.Force = this.Force;
          association.Option = AssociationOption.NotByPeriod.Add (this.Option);
          association.Caller = this;
          association.Analyze ();
        }
      }
    }

    /// <summary>
    /// Process the association itself,
    /// without updating any Modification property
    /// </summary>
    void Analyze ()
    {
      Debug.Assert (!IsStopProcess ());

      {
        ObservationStateSlotDAO observationStateSlotDAO = new ObservationStateSlotDAO ();
        observationStateSlotDAO.Caller = this;
        Insert<ObservationStateSlot, IObservationStateSlot, ObservationStateSlotDAO> (observationStateSlotDAO);
      }
    }

    bool IsStopProcess ()
    {
      return (!this.Option.HasValue || !this.Option.Value.HasFlag (AssociationOption.NoStop))
        && this.MachineStateTemplate.Stops.Any ();
    }

    bool IsProcessByPeriod ()
    {
      if (this.Range.Duration.HasValue
          && (this.Range.Duration.Value < Lemoine.Info.ConfigSet.LoadAndGet<TimeSpan> (BY_PERIOD_MIN_DURATION_KEY,
                                                                                       BY_PERIOD_MIN_DURATION_DEFAULT))) {
        return false;
      }
      return !this.Force && (!this.Option.HasValue || !this.Option.Value.HasFlag (AssociationOption.NotByPeriod));
    }

    UpperBound<DateTime> GetStop ()
    {
      UpperBound<DateTime> end = this.End;

      foreach (IMachineStateTemplateStop stop in this.MachineStateTemplate.Stops) {
        if (!this.Begin.HasValue) {
          // No begin date/time, stops can't be processed
          log.Warn ($"GetStop: defined stop {stop} but no lower bound in {this} => can't be processed");
          AddAnalysisLog (LogLevel.WARN,
                          "MachineStateTemplateStop incompatible with an association with no lower bound");
          break;
        }
        Debug.Assert (this.Begin.HasValue);
        DateTime currentDay = this.Begin.Value.ToLocalTime ().Date;
        UpperBound<DateTime> lastDay = new UpperBound<DateTime> (null);
        if (end.HasValue) {
          lastDay = end.Value.ToLocalTime ().Date;
        }
        int i = 0;
        while (NullableDateTime.Compare (currentDay, lastDay) <= 0) { // Loop on days until one matches
          // Check this is not an infinite loop
          if (8 < ++i) { // At most one week and one day
            Debug.Assert (false);
            log.Fatal ("GetStop: infinite loop is detected");
            break;
          }

          if (stop.WeekDays.HasFlagDayOfWeek (currentDay.DayOfWeek)) { // Day of week is ok
            // Compute the potential stop date/time
            DateTime stopDateTime = currentDay.Add (stop.LocalTime).ToUniversalTime ();
            if (Bound.Compare<DateTime> (this.Begin, stopDateTime) < 0) { // The potential stop is after this.Begin
              // => stop here
              if (log.IsDebugEnabled) {
                log.Debug ($"ProcessAssociation: consider stop {stopDateTime}");
              }
              if (Bound.Compare<DateTime> (stopDateTime, end) < 0) {
                end = stopDateTime;
              }
              break;
            }
          }
          currentDay = currentDay.AddDays (1);
        }
      }

      return end;
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
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public override void Unproxy ()
    {
      base.Unproxy ();
      NHibernateHelper.Unproxy<IMachineStateTemplate> (ref m_MachineStateTemplate);
      NHibernateHelper.Unproxy<IMachineStateTemplate> (ref m_nextMachineStateTemplate);
      NHibernateHelper.Unproxy<IUser> (ref m_user);
      NHibernateHelper.Unproxy<IShift> (ref m_shift);
    }
  }
}
