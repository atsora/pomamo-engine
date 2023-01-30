// Copyright (C) 2009-2023 Lemoine Automation Technologies
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
using Pulse.Extensions.Database;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table MachineObservationStateAssociation
  /// 
  /// This new table is designed to add any Machine Observation State / Machine
  /// association, with the reference to a User when applicable.
  /// 
  /// It does not represent the current observation state of a machine,
  /// but all the manual or automatic reason changes that have been made.
  /// 
  /// To know the current observation states of a machine,
  /// the table Observation State Slot that is filled in by the Analyzer must be used.
  /// </summary>
  [Serializable]
  public class MachineObservationStateAssociation : MachineAssociation, IMachineObservationStateAssociation
  {
    /// <summary>
    /// Minimum duration of the association when the 'by period' process applies: key
    /// </summary>
    static readonly string BY_PERIOD_MIN_DURATION_KEY = "Analysis.MachineObservationStateAssociation.ByPeriodMinDuration";
    /// <summary>
    /// Minimum duration of the association when the 'by period' process applies: default value
    /// </summary>
    static readonly TimeSpan BY_PERIOD_MIN_DURATION_DEFAULT = TimeSpan.FromHours (4);

    #region Members
    IMachineObservationState m_machineObservationState;
    IUser m_user;
    IShift m_shift;
    IMachineStateTemplate m_machineStateTemplate = null;
    bool m_partOfDetectionAnalysis = false;

    IEnumerable<IReasonExtension> m_reasonExtensions;
    #endregion // Members

    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    protected MachineObservationStateAssociation ()
    { }

    /// <summary>
    /// Constructor
    ///
    /// For some existing unit tests
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="begin"></param>
    internal protected MachineObservationStateAssociation (IMachine machine,
                                                           DateTime begin)
      : base (machine, begin)
    { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    internal protected MachineObservationStateAssociation (IMachine machine,
                                                           UtcDateTimeRange range)
      : base (machine, range)
    { }

    /// <summary>
    /// Constructor for abstract modifications, that are kept transient
    /// 
    /// You may add a mainModification reference to be used in the analysis logs
    /// or precise if this is part of a detection analysis
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="mainModification"></param>
    /// <param name="partOfDetectionAnalysis"></param>
    internal protected MachineObservationStateAssociation (IMachine machine, UtcDateTimeRange range,
                                                           IModification mainModification,
                                                           bool partOfDetectionAnalysis)
      : base (machine, range, mainModification)
    {
      Debug.Assert ((null == mainModification) || (false == partOfDetectionAnalysis));
      if ((null != mainModification) && partOfDetectionAnalysis) {
        log.FatalFormat ("MachineObservationStateAssociation: " +
                         "incompatible arguments mainModification={0} and partOfDetectionAnalysis={1}",
                         mainModification, partOfDetectionAnalysis);
      }
      m_partOfDetectionAnalysis = partOfDetectionAnalysis;
    }
    #endregion // Constructors

    #region Getters / Setters
    /// <summary>
    /// <see cref="IModification"/>
    /// </summary>
    [XmlIgnore]
    public override string ModificationType
    {
      get { return "MachineObservationStateAssociation"; }
    }

    /// <summary>
    /// Reference to the Machine Observation State
    /// 
    /// Nullable only for a transient modification.
    /// In this case, do not use the setter
    /// </summary>
    [XmlIgnore]
    public virtual IMachineObservationState MachineObservationState
    {
      get { return m_machineObservationState; }
      set
      {
        Debug.Assert (null != value);
        if (null == value) {
          log.ErrorFormat ("MachineObservationState.set: " +
                           "null value");
          throw new ArgumentNullException ("value");
        }
        m_machineObservationState = value;
      }
    }

    /// <summary>
    /// Reference to the Machine Observation State for Xml Serialization
    /// </summary>
    [XmlElement ("MachineObservationState")]
    public virtual MachineObservationState XmlSerializationMachineObservationState
    {
      get { return this.MachineObservationState as MachineObservationState; }
      set { this.MachineObservationState = value; }
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
    /// Reference to a machine state template
    /// for a virtual transient modification only
    /// 
    /// Nullable
    /// </summary>
    [XmlIgnore]
    public virtual IMachineStateTemplate MachineStateTemplate
    {
      get { return m_machineStateTemplate; }
      set { m_machineStateTemplate = value; }
    }

    /// <summary>
    /// Part of the detection analysis so that any analysis problem is logged
    /// in the detectionanalysislog table
    /// </summary>
    [XmlIgnore]
    public virtual bool PartOfDetectionAnalysis
    {
      get { return m_partOfDetectionAnalysis; }
    }
    #endregion // Getters / Setters

    /// <summary>
    /// Get the ResetReasonExtensions and load them if needed
    /// </summary>
    /// <returns></returns>
    IEnumerable<IReasonExtension> GetReasonExtensions ()
    {
      if (null == m_reasonExtensions) { // Initialization
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
    /// Add an analysis log for this modification
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    protected override void AddAnalysisLog (LogLevel level,
                                            string message)
    {
      if (m_partOfDetectionAnalysis) {
        log.DebugFormat ("AddAnalysisLog: " +
                         "add a detection analysis log with the message {0} and level {1}",
                         message, level);
        IDetectionAnalysisLog detectionAnalysisLog = ModelDAOHelper.ModelFactory
          .CreateDetectionAnalysisLog (level, message, this.Machine);
        ModelDAOHelper.DAOFactory.DetectionAnalysisLogDAO
          .MakePersistent (detectionAnalysisLog);
      }
      else {
        base.AddAnalysisLog (level, message);
      }
    }

    #region MachineAssociation implementation
    /// <summary>
    /// <see cref="PeriodAssociation.ConvertToSlot" />
    /// </summary>
    /// <returns></returns>
    public override TSlot ConvertToSlot<TSlot> ()
    {
      var slot =
        GenericMachineRangeSlot.Create (typeof (TSlot), this.Machine, this.Range) as TSlot;
      slot.Consolidated = false;

      if (slot is IObservationStateSlot) {
        var observationStateSlot = slot as IObservationStateSlot;
        observationStateSlot.MachineObservationState = this.MachineObservationState;
        observationStateSlot.User = this.User;
        observationStateSlot.Shift = this.Shift;
        observationStateSlot.MachineStateTemplate = this.MachineStateTemplate;
        if (null != this.MachineObservationState) {
          observationStateSlot.Production = this.MachineObservationState.IsProduction;
        }
        return slot;
      }
      else if (slot is IReasonSlot) {
        IReasonSlot reasonSlot =
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
      Debug.Assert (this.Machine.Equals ((oldSlot as IPartitionedByMachine).Machine));

      if (oldSlot is ObservationStateSlot) {
        IObservationStateSlot oldObservationStateSlot =
          oldSlot as ObservationStateSlot;
        IObservationStateSlot newObservationStateSlot =
          (IObservationStateSlot)oldObservationStateSlot.Clone ();

        Debug.Assert (object.Equals (newObservationStateSlot.Machine,
                                     this.Machine));

        newObservationStateSlot.MachineObservationState =
          this.MachineObservationState;
        newObservationStateSlot.User = this.User;
        newObservationStateSlot.Shift = this.Shift;
        newObservationStateSlot.MachineStateTemplate = this.MachineStateTemplate;
        if (null != this.MachineObservationState) {
          newObservationStateSlot.Production = this.MachineObservationState.IsProduction;
        }
        else {
          newObservationStateSlot.Production = null;
        }
        return newObservationStateSlot as TSlot;
      }
      else if (oldSlot is IReasonSlot) {
        IReasonSlot oldReasonSlot = oldSlot as IReasonSlot;
        IReasonSlot newReasonSlot = (IReasonSlot)oldReasonSlot.Clone ();
        ((ReasonSlot)newReasonSlot).SetOldSlotFromModification (oldReasonSlot, this);

        if (null != this.MachineObservationState) { // Only if MachineObservationState is known
          // else only the MachineStateTemplate is known and the process is delayed
          newReasonSlot.Shift = this.Shift;

          if (!object.Equals (newReasonSlot.MachineObservationState, this.MachineObservationState)) {
            // Consider the reasons only if there is a change of machine observation state
            newReasonSlot.MachineObservationState = this.MachineObservationState;
            if (newReasonSlot.ReasonSource.Equals (ReasonSource.Manual)) {
              // Check the old reason is compatible with the new
              // MachineObservationState if the machine observation state changed,
              // else raise a warning
              CheckReasonCompatible (range, oldReasonSlot.MachineMode, this.MachineObservationState, oldReasonSlot.Reason, oldReasonSlot.ReasonScore, oldReasonSlot.ReasonSource);
            }
          }
        }

        return newReasonSlot as TSlot;
      }
      else {
        System.Diagnostics.Debug.Assert (false);
        log.FatalFormat ("MergeData: " +
                         "trying to merge the association with a not supported slot {0}",
                         typeof (TSlot));
        throw new ArgumentException ("Not supported machine slot");
      }
    }
    #endregion // MachineAssociation implementation

    #region Modification implementation
    /// <summary>
    /// Make the analysis
    /// </summary>
    public override void MakeAnalysis ()
    {
      Debug.Assert (!IsMainModificationTransient ());
      Debug.Assert (!IsAnalysisCompleted ());

      Debug.Assert (false == m_partOfDetectionAnalysis);
      Debug.Assert (null != this.MachineObservationState);

      if (Bound.Compare<DateTime> (this.End, this.Begin) < 0) { // Empty period: error
        string message = string.Format ("End={0} before Begin={1}",
                                        this.End, this.Begin);
        log.Error ($"MakeAnalysis: {message} => finish in error");
        AddAnalysisLog (LogLevel.ERROR, message);
        MarkAsError ();
        return;
      }

      if ((AnalysisStatus.New == this.AnalysisStatus) && (null == this.Parent)
        && (null != this.MachineObservationState) && (LinkDirection.None != this.MachineObservationState.LinkOperationDirection)) {
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
                           this.MachineObservationState.LinkOperationDirection);
          ILinkOperation linkOperation = ModelDAOHelper.ModelFactory
            .CreateLinkOperation (monitoredMachine, this.MachineObservationState.LinkOperationDirection,
                                  this.Range);
          ModelDAOHelper.DAOFactory.LinkOperationDAO.MakePersistent (linkOperation);
          linkOperation.Parent = this.MainModification ?? this;
          linkOperation.Priority = this.Priority;
        }
        MarkAsInProgress (null);
        return;
      }

      if (!this.IsProcessByPeriod ()) { // Apply directly the changes, do not split it by period
        // Get the adjusted step range
        UtcDateTimeRange range = GetNotAppliedRange ();

        // Analyze ! with the adjusted begin and end
        MachineObservationStateAssociation association = new MachineObservationStateAssociation (this.Machine,
                                                                                                 range,
                                                                                                 this,
                                                                                                 m_partOfDetectionAnalysis);
        association.Option = this.Option;
        association.DateTime = this.DateTime;
        association.MachineObservationState = this.MachineObservationState;
        association.MachineStateTemplate = this.MachineStateTemplate;
        association.User = this.User;
        association.Shift = this.Shift;
        association.Caller = this;
        association.Analyze ();

        // Analysis is done
        MarkAsCompleted ("Cache/ClearDomainByMachine/MachineObservationStateAssociation/" + this.Machine.Id + "?Broadcast=true",
                         (DateTime?)range.Upper); // => InProgress or Done
      }
      else { // Try to split the process by periods where a change is really required
        IList<IObservationStateSlot> noChangeRequiredSlots = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
          .FindAllInUtcRangeWith (this.Machine, this.Range, this.MachineStateTemplate,
                                  this.MachineObservationState, this.Shift, this.User);
        bool associationCreated = false;
        Bound<DateTime> lower = this.Range.Lower;
        foreach (var noChangeRequiredSlot in noChangeRequiredSlots) {
          if (Bound.Compare<DateTime> (lower, noChangeRequiredSlot.DateTimeRange.Lower) < 0) {
            Debug.Assert (noChangeRequiredSlot.DateTimeRange.Lower.HasValue);
            Debug.Assert (Bound.Compare<DateTime> (noChangeRequiredSlot.DateTimeRange.Lower, this.Range.Upper) <= 0);
            UtcDateTimeRange range = new UtcDateTimeRange ((LowerBound<DateTime>)lower,
                                                           noChangeRequiredSlot.DateTimeRange.Lower.Value);
            Debug.Assert (this.Range.ContainsRange (range));
            IMachineObservationStateAssociation association = ModelDAOHelper.ModelFactory
              .CreateMachineObservationStateAssociation (this.Machine, this.MachineObservationState, range);
            association.DateTime = this.DateTime;
            association.MachineStateTemplate = this.MachineStateTemplate;
            association.Shift = this.Shift;
            association.User = this.User;
            association.Option = AssociationOption.NotByPeriod.Add (this.Option);
            ModelDAOHelper.DAOFactory.MachineObservationStateAssociationDAO.MakePersistent (association);
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
          IMachineObservationStateAssociation association = ModelDAOHelper.ModelFactory
            .CreateMachineObservationStateAssociation (this.Machine, this.MachineObservationState, range);
          association.DateTime = this.DateTime;
          association.MachineStateTemplate = this.MachineStateTemplate;
          association.Shift = this.Shift;
          association.User = this.User;
          association.Option = AssociationOption.NotByPeriod.Add (this.Option);
          ModelDAOHelper.DAOFactory.MachineObservationStateAssociationDAO.MakePersistent (association);
          association.Parent = this.MainModification ?? this;
          association.Priority = this.StatusPriority;
          associationCreated = true;
        }

        if (!associationCreated) {
          log.DebugFormat ("MakeAnalysis: " +
                           "no change was required");
        }
        MarkAsCompleted ("Cache/ClearDomainByMachine/MachineObservationStateAssociation/" + this.Machine.Id + "?Broadcast=true",
                         null);
      }
    }

    /// <summary>
    /// Apply the modifications
    /// 
    /// Note: the link operation process it not processed here
    /// It is processed only in persistent modifications
    /// </summary>
    public override void Apply ()
    {
      if (!this.IsProcessByPeriod ()) { // Apply directly the changes, do not split it by period
        this.Analyze ();
      }
      else { // Try to split the process by periods where a change is really required
        IList<IObservationStateSlot> noChangeRequiredSlots = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
          .FindAllInUtcRangeWith (this.Machine, this.Range, this.MachineStateTemplate,
                                  this.MachineObservationState, this.Shift, this.User);
        Bound<DateTime> lower = this.Range.Lower;
        foreach (var noChangeRequiredSlot in noChangeRequiredSlots) {
          if (Bound.Compare<DateTime> (lower, noChangeRequiredSlot.DateTimeRange.Lower) < 0) {
            Debug.Assert (noChangeRequiredSlot.DateTimeRange.Lower.HasValue);
            Debug.Assert (Bound.Compare<DateTime> (noChangeRequiredSlot.DateTimeRange.Lower, this.Range.Upper) <= 0);
            UtcDateTimeRange range = new UtcDateTimeRange ((LowerBound<DateTime>)lower,
                                                           noChangeRequiredSlot.DateTimeRange.Lower.Value);
            Debug.Assert (this.Range.ContainsRange (range));
            MachineObservationStateAssociation association = new MachineObservationStateAssociation (this.Machine,
                                                                                                     range,
                                                                                                     this.MainModification,
                                                                                                     m_partOfDetectionAnalysis);
            association.DateTime = this.DateTime;
            association.MachineObservationState = this.MachineObservationState;
            association.MachineStateTemplate = this.MachineStateTemplate;
            association.Shift = this.Shift;
            association.User = this.User;
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
          MachineObservationStateAssociation association = new MachineObservationStateAssociation (this.Machine,
                                                                                                   range,
                                                                                                   this.MainModification,
                                                                                                   m_partOfDetectionAnalysis);
          association.DateTime = this.DateTime;
          association.MachineObservationState = this.MachineObservationState;
          association.MachineStateTemplate = this.MachineStateTemplate;
          association.Shift = this.Shift;
          association.User = this.User;
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
      { // Update observation state slot
        ObservationStateSlotDAO observationStateSlotDAO = new ObservationStateSlotDAO ();
        observationStateSlotDAO.Caller = this;
        Insert<ObservationStateSlot, IObservationStateSlot, ObservationStateSlotDAO> (observationStateSlotDAO);
      }

      // Update the reason slots
      if (null != this.MachineObservationState) {
        IMonitoredMachine monitoredMachine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (this.Machine.Id);
        if (null != monitoredMachine) {
          UpdateReasonSlots (monitoredMachine);

          // Check MachineStatus
          IMachineStatus machineStatus = ModelDAOHelper.DAOFactory.MachineStatusDAO
            .FindById (this.Machine.Id);
          if (null == machineStatus) {
            string message = string.Format ("no MachineStatus found for {0}",
                                            this.Machine);
            log.WarnFormat ("MakeAnalysis: {0}",
                            message);
            AddAnalysisLog (LogLevel.WARN,
                            message);
          }
          else { // null != machineStatus
            if (this.Range.ContainsElement (machineStatus.ReasonSlotEnd)) { // Current
              // This impacts the current period => update machineStatus
              log.DebugFormat ("MakeAnalysis: " +
                               "current MachineObservationState association");
              machineStatus.MachineObservationState = this.MachineObservationState;
              machineStatus.Shift = this.Shift;
            }
          }
        }
      }
    }

    bool IsProcessByPeriod ()
    {
      if (this.Range.Duration.HasValue
          && (this.Range.Duration.Value < Lemoine.Info.ConfigSet.LoadAndGet<TimeSpan> (BY_PERIOD_MIN_DURATION_KEY,
                                                                                       BY_PERIOD_MIN_DURATION_DEFAULT))) {
        return false;
      }
      return !this.Option.HasValue || !this.Option.Value.HasFlag (AssociationOption.NotByPeriod);
    }

    void UpdateReasonSlots (IMonitoredMachine machine)
    {
      Debug.Assert (null != this.MachineObservationState);

      // The simple code, but also much too slow is:
      // Insert<ReasonSlot> (NHibernateHelper.GetCurrentSession ());
      // The aim of this is to allow the process to be very much faster
      // and limit the number of updates

      // Get all the impacted reason slots
      // and split them in three parts
      // - the one on the left that may be split in two
      // - the one on the right that may be split in two
      // - the other reason slots that will remain unsplit (middle period)
      LowerBound<DateTime> middlePeriodBegin = this.Begin;
      UpperBound<DateTime> middlePeriodEnd = this.End;
      ReasonSlotDAO reasonSlotDAO = new ReasonSlotDAO ();
      reasonSlotDAO.Caller = this;
      // - left reason slot
      {
        IReasonSlot leftReasonSlot =
          ModelDAOHelper.DAOFactory.ReasonSlotDAO.FindAt (machine, this.Begin);
        if ((null != leftReasonSlot)
            && (Bound.Compare<DateTime> (leftReasonSlot.BeginDateTime, this.Begin) < 0)) {
          Debug.Assert (Bound.Compare<DateTime> (this.Begin, leftReasonSlot.EndDateTime) < 0);
          log.DebugFormat ("UpdateReasonSlots: " +
                           "left reason slot {0}-{1}",
                           leftReasonSlot.BeginDateTime, leftReasonSlot.EndDateTime);
          middlePeriodBegin = (LowerBound<DateTime>)leftReasonSlot.EndDateTime; // adjust the middle period
          MachineObservationStateAssociation leftAssociation =
            (MachineObservationStateAssociation)this.MemberwiseClone ();
          Debug.Assert (Bound.Equals<DateTime> (leftAssociation.End, this.End));
          if (Bound.Compare<DateTime> (leftReasonSlot.EndDateTime, this.End) < 0) {
            leftAssociation.End = leftReasonSlot.EndDateTime;
          }
          leftAssociation.Insert<ReasonSlot, IReasonSlot, ReasonSlotDAO> (reasonSlotDAO);
          if (Bound.Compare<DateTime> (this.End, leftAssociation.End) <= 0) {
            // Everything has been processed, return
            return;
          }
        }
      }
      SetActive ();
      // - right reason slot
      if (this.End.HasValue) {
        IReasonSlot rightReasonSlot =
          ModelDAOHelper.DAOFactory.ReasonSlotDAO.FindAt (machine, this.End);
        if ((null != rightReasonSlot)
            && (Bound.Compare<DateTime> (rightReasonSlot.BeginDateTime, this.End.Value) < 0)) {
          Debug.Assert (NullableDateTime.Compare (this.End, rightReasonSlot.EndDateTime) < 0);
          log.DebugFormat ("UpdateReasonSlots: " +
                           "right reason slot {0}-{1}",
                           rightReasonSlot.BeginDateTime, rightReasonSlot.EndDateTime);
          middlePeriodEnd = (UpperBound<DateTime>)rightReasonSlot.BeginDateTime; // adjust the middle period
          MachineObservationStateAssociation rightAssociation =
            (MachineObservationStateAssociation)this.MemberwiseClone ();
          Debug.Assert (Bound.Compare<DateTime> (this.Begin, rightReasonSlot.BeginDateTime) <= 0);
          rightAssociation.Begin = rightReasonSlot.BeginDateTime;
          rightAssociation.Insert<ReasonSlot, IReasonSlot, ReasonSlotDAO> (reasonSlotDAO);
        }
      }
      SetActive ();
      // - middle reason slots that won't be split + check if some reason slots may be merged
      {
        UtcDateTimeRange middlePeriod = new UtcDateTimeRange (middlePeriodBegin, middlePeriodEnd);
        if (!middlePeriod.IsEmpty ()) {
          log.DebugFormat ("UpdateReasonSlots: " +
                           "middle period {0}",
                           middlePeriod);
          // Get all the reason slots in this period, ordered by:
          // - machine mode
          // - machine observation state
          // - day
          // - duration
          IReasonSlot previousReasonSlot = null;
          IList<IReasonSlot> reasonSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
            .FindAllInUtcRangeWithLimit (machine, middlePeriod);
          IDictionary<IMachineMode, IEnumerable<IMachineModeDefaultReason>> machineModeDefaultReasons =
            new Dictionary<IMachineMode, IEnumerable<IMachineModeDefaultReason>> (); // Default reasons for a machine mode and machine observation state
          IDictionary<IMachineMode, IEnumerable<IReasonSelection>> machineModeReasonSelections =
            new Dictionary<IMachineMode, IEnumerable<IReasonSelection>> (); // Reason selections for a machine mode and machine observation state
          foreach (IReasonSlot reasonSlot in reasonSlots) {
            Debug.Assert (null != reasonSlot.MachineObservationState);
            Debug.Assert (null != reasonSlot.MachineMode);
            Debug.Assert (null != reasonSlot.Reason);
            SetActive ();

            // Test first if:
            // - the change has already been made
            // - or if the reason slot is not in the [begin, end] period
            if ((reasonSlot.MachineObservationState.Equals (this.MachineObservationState)
                  && object.Equals (reasonSlot.Shift, this.Shift))
                || (Bound.Compare<DateTime> (middlePeriodEnd, reasonSlot.BeginDateTime) <= 0)
                || (Bound.Compare<DateTime> (reasonSlot.EndDateTime, middlePeriodBegin) <= 0)) {
              log.DebugFormat ("UpdateReasonSlots: " +
                               "skip reason slot {0} because it does not need to be changed " +
                               "or because it is not in the date/time range",
                               reasonSlot);
            }
            else {
              Debug.Assert (Bound.Compare<DateTime> (middlePeriodBegin, reasonSlot.BeginDateTime) <= 0);
              Debug.Assert (Bound.Compare<DateTime> (reasonSlot.EndDateTime, middlePeriodEnd) <= 0);
              // Note: else they were in the left or right limits and their machine observation and shift
              //       have already been updated

              log.DebugFormat ("UpdateReasonSlots: " +
                               "update the machine observation state or shift of reason slot {0}",
                               reasonSlot);

              using (var modificationTracker = new SlotModificationTracker<IReasonSlot> (reasonSlot)) {

                // In case there a machine observation state change (and not only a shift change)
                if (!object.Equals (reasonSlot.MachineObservationState, this.MachineObservationState)) {

                  if (reasonSlot.ReasonSource.Equals (ReasonSource.Manual)) {
                    // Machine observation state change on a manual reason,
                    // check it is still compatible
                    CheckReasonCompatible (middlePeriod,
                                           reasonSlot.MachineMode,
                                           this.MachineObservationState,
                                           reasonSlot.Reason,
                                           reasonSlot.ReasonScore,
                                           reasonSlot.ReasonSource);
                  }

                  var oldReasonSlot = (IReasonSlot)reasonSlot.Clone ();
                  reasonSlot.MachineObservationState = this.MachineObservationState;
                  ((ReasonSlot)reasonSlot).Consolidate (oldReasonSlot, this);
                } // Change of machine observation state

                reasonSlot.Shift = this.Shift;
              }

              // Save the reason slot (optional because the object is already persistent,
              // but may allow to detect some problems earlier)
              ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
            }

            // Test if two consecutive reason slots may be merged
            IReasonSlot currentReasonSlot = reasonSlot;
            if (null != previousReasonSlot) {
              Debug.Assert (previousReasonSlot.EndDateTime.HasValue);
              Debug.Assert (Bound.Equals<DateTime> (previousReasonSlot.EndDateTime, reasonSlot.BeginDateTime));
              if (previousReasonSlot.ReferenceDataEquals (reasonSlot)) {
                log.DebugFormat ("UpdateReasonSlots: " +
                                 "merge {0} and {1}",
                                 previousReasonSlot, reasonSlot);
                currentReasonSlot = Merge (previousReasonSlot, reasonSlot);
              }
            }
            previousReasonSlot = currentReasonSlot;
          }
        }
      }
    }

    IReasonSlot Merge (IReasonSlot previous, IReasonSlot next)
    {
      Debug.Assert (!previous.DateTimeRange.IsEmpty ());
      Debug.Assert (!next.DateTimeRange.IsEmpty ());
      Debug.Assert (previous.EndDateTime.HasValue);
      Debug.Assert (Bound.Equals<DateTime> (previous.EndDateTime.Value, next.BeginDateTime));
      Debug.Assert (previous.ReferenceDataEquals (next));

      using (var modificationTracker1 = new SlotModificationTracker<IReasonSlot> (previous, false)) {
        using (var modificationTracker2 = new SlotModificationTracker<IReasonSlot> (next, false)) {
          var oldPrevious = (IReasonSlot)previous.Clone ();
          ((ReasonSlot)previous).UpdateDateTimeRange (new UtcDateTimeRange (previous.BeginDateTime, next.EndDateTime));
          ((ReasonSlot)previous).Consolidate (null, this);
          ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (previous);
          ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakeTransient (next);
          AnalysisAccumulator.MergeReasonSlots (oldPrevious, next, previous);
        }
      }
      return previous;
    }

    bool IsReasonCompatible (UtcDateTimeRange range, IMachineMode machineMode, IMachineObservationState machineObservationState, IReason reason, double reasonScore, ReasonSource reasonSource)
    {
      return GetReasonExtensions ()
        .Any (ext => ext.IsCompatible (range, machineMode, machineObservationState, reason, reasonScore, reasonSource));
    }

    /// <summary>
    /// Check the old reason is compatible with one of the provided reason selections,
    /// else raise a warning
    /// 
    /// UNDONE: to move in Lemoine.Business, so that it can be used elsewhere ?
    /// </summary>
    /// <param name="range"></param>
    /// <param name="machineMode">not null</param>
    /// <param name="machineObservationState">not null</param>
    /// <param name="reason">not null</param>
    /// <param name="reasonScore"></param>
    /// <param name="reasonSource"></param>
    void CheckReasonCompatible (UtcDateTimeRange range,
                                IMachineMode machineMode,
                                IMachineObservationState machineObservationState,
                                IReason reason,
                                double reasonScore,
                                ReasonSource reasonSource)
    {
      Debug.Assert (null != machineMode);
      Debug.Assert (null != machineObservationState);
      Debug.Assert (null != reason);

      if (!IsReasonCompatible (range, machineMode, machineObservationState, reason, reasonScore, reasonSource)) {
        string message =
          string.Format ("The existing reason {0} is not compatible " +
                         "with the new MachineObservationState {1} and MachineMode {2}",
                         reason, machineObservationState, machineMode);
        log.WarnFormat ("CheckReasonCompatible: {0}",
                        message);
        AddAnalysisLog (LogLevel.WARN, message);
      }
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
      NHibernateHelper.Unproxy<IMachineObservationState> (ref m_machineObservationState);
      NHibernateHelper.Unproxy<IUser> (ref m_user);
      NHibernateHelper.Unproxy<IShift> (ref m_shift);
    }
  }
}
