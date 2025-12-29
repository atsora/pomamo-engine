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

    IMachineStateTemplate m_MachineStateTemplate;
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
          log.ErrorFormat ("MachineStateTemplate.set: " +
                           "null value");
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
          log.FatalFormat ("MergeData: " +
                           "trying to merge the association with a not supported slot {0}",
                           typeof (TSlot));
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
      log.ErrorFormat ($"SetModificationInError: {message} => finish in error");
      AddAnalysisLog (LogLevel.ERROR, message);
      MarkAsError ();
    }

    /// <summary>
    /// Mark the modification as not applicable
    /// </summary>
    void MarkDynamicTimeNotApplicable ()
    {
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
          log.InfoFormat ("MakeAnalysis: empty range => nothing to do for modification id={0}", this.Id);
        }
        MarkAsCompleted ("");
        return;
      }

      if ((AnalysisStatus.New == this.AnalysisStatus) && (null == this.Parent)
        && (null != this.MachineStateTemplate) && (LinkDirection.None != this.MachineStateTemplate.LinkOperationDirection)) {
        // Create the associated link operation modification if applicable (not on a sub-modification)
        IMonitoredMachine monitoredMachine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (this.Machine.Id);
        if (null == monitoredMachine) {
          log.WarnFormat ("MakeAnalysis: " +
                          "no monitored machine with ID {0} " +
                          "=> do not process any LinkOperation",
                          this.Machine.Id);
        }
        else {
          log.DebugFormat ("MakeAnalysis: " +
                           "create link operation {0}",
                           this.MachineStateTemplate.LinkOperationDirection);
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

      if (!IsProcessByPeriod ()) { // Apply directly the changes, do not split it by period
        // Get the adjusted step range
        UtcDateTimeRange range = GetNotAppliedRange ();

        // Analyze ! with the adjusted begin and end
        MachineStateTemplateAssociation association = new MachineStateTemplateAssociation (this.Machine,
                                                                                           this.MachineStateTemplate,
                                                                                           range,
                                                                                           this);
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
          log.DebugFormat ("MakeAnalysis: " +
                           "no change was required");
        }
        MarkAsCompleted ("", null);
      }
    }

    void MakeAnalysisDynamicStart (string dynamicStart)
    {
      log.FatalFormat ("MakeAnalysisDynamicStart: dynamic start is not supported yet");
      SetModificationInError ("Dynamic start not implemented");
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

      var notProcessedRange = GetNotPocessedRange ();
      if (notProcessedRange.IsEmpty ()) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("MakeAnalysisDynamicEnd: GetNotProcessedRange returned an empty range => completed");
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
        log.ErrorFormat ("MakeAnalysisDynamicEnd: unknown dynamic end {0}", dynamicEnd);
        SetModificationInError ("Dynamic end " + dynamicEnd + " unknown");
        return;
      }
      SetActive ();
      Debug.Assert (null != dynamicEndResponse);

      if (dynamicEndResponse.Timeout) {
        if (log.IsWarnEnabled) {
          log.WarnFormat ("MakeAnalysisDynamicEnd: Timeout is returned for {0} at {1} => mark as not applicable", dynamicEnd, this.DynamicTimeRange);
        }
        MarkDynamicTimeNotApplicable ();
        return;
      }
      else if (dynamicEndResponse.NotApplicable) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("MakeAnalysisDynamicEnd: NotApplicable is returned for {0} at {1} => mark as not applicable", dynamicEnd, this.DynamicTimeRange);
        }
        MarkDynamicTimeNotApplicable ();
        return;
      }
      else if (dynamicEndResponse.NoData) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("MakeAnalysisDynamicEnd: NoData is returned for {0} at {1} => mark as not applicable", dynamicEnd, this.DynamicTimeRange);
        }
        MarkDynamicTimeNotApplicable ();
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

    void ApplyDynamicEnd (DateTime dynamicApplicableEnd)
    {
      var range = this.GetNotPocessedRange ();
      if (range.IsEmpty ()) {
        if (log.IsFatalEnabled) {
          log.Fatal ("ApplyDynamicEnd: (unexpected) GetNotProcessedRange is empty => nothing to do");
        }
        Debug.Assert (false);
        MarkAsCompleted ("");
        return;
      }

      UtcDateTimeRange restriction = new UtcDateTimeRange (new LowerBound<DateTime> (null), dynamicApplicableEnd);
      UtcDateTimeRange restrictedRange = new UtcDateTimeRange (range.Intersects (restriction));
      if (restrictedRange.IsEmpty ()) {
        log.DebugFormat ("ApplyDynamicEnd: restricted range is empty => nothing to do");
      }
      else {
        log.InfoFormat ("ApplyDynamicEnd: restrict {0} to {1}", this, dynamicApplicableEnd);
        ModelDAOHelper.DAOFactory.MachineStateTemplateAssociationDAO
          .InsertSub (this, restrictedRange, ApplyDynamicEndSubChange, null);
      }
      MarkAsCompleted ("");
    }

    void ApplyDynamicEndSubChange (IMachineStateTemplateAssociation association)
    {
      association.Dynamic = null;
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
        log.FatalFormat ($"Apply: association {this} had a dynamic value {this.Dynamic}, which is unexpected");
      }

      if (IsStopProcess ()) { // Stop must be processed first
        UtcDateTimeRange range = new UtcDateTimeRange (this.Range.Lower, GetStop ());
        Debug.Assert (this.Range.ContainsRange (range));
        var association = new MachineStateTemplateAssociation (this.Machine,
                                                               this.MachineStateTemplate,
                                                               range,
                                                               this.MainModification);
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
          log.WarnFormat ("Analyze" +
                          "defined stop {0} but no lower bound in {1} " +
                          "=> can't be processed",
                          stop, this);
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
            log.FatalFormat ("ProcessAssociation: " +
                             "infinite loop is detected");
            break;
          }

          if (stop.WeekDays.HasFlagDayOfWeek (currentDay.DayOfWeek)) { // Day of week is ok
            // Compute the potential stop date/time
            DateTime stopDateTime = currentDay;
            if (stop.LocalTime.HasValue) {
              stopDateTime = stopDateTime.Add (stop.LocalTime.Value);
            }
            stopDateTime = stopDateTime.ToUniversalTime ();
            if (Bound.Compare<DateTime> (this.Begin, stopDateTime) < 0) { // The potential stop is after this.Begin
              // => stop here
              log.DebugFormat ("ProcessAssociation: " +
                               "consider stop {0}",
                               stopDateTime);
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
      NHibernateHelper.Unproxy<IUser> (ref m_user);
      NHibernateHelper.Unproxy<IShift> (ref m_shift);
    }
  }
}
