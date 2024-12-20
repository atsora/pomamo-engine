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
using Lemoine.Business.Config;
using Pulse.Extensions.Database;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table ActivityManual
  /// 
  /// This new table is designed to add any manual activity change
  /// to a machine.
  /// 
  /// This makes the analysis of the different tables much more convenient.
  /// 
  /// It does not represent the current activity of a machine,
  /// but all the manual activity changes that have been made.
  /// 
  /// To know the current activity of a machine,
  /// you can still use the Fact table though,
  /// because it contains an additional column for overwritten activities.
  /// </summary>
  [Serializable]
  public class ActivityManual : MachineAssociation, IActivityManual
  {
    #region Members
    IMachineMode m_machineMode;

    IEnumerable<IReasonExtension> m_reasonExtensions;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// <see cref="IModification"/>
    /// </summary>
    [XmlIgnore]
    public override string ModificationType
    {
      get { return "ActivityManual"; }
    }

    /// <summary>
    /// Reference to the manual MachineMode
    /// 
    /// Not null
    /// </summary>
    [XmlIgnore]
    public virtual IMachineMode MachineMode
    {
      get { return m_machineMode; }
      protected set {
        Debug.Assert (null != value);
        if (null == value) {
          log.ErrorFormat ("MachineMode.set: " +
                           "null value");
          throw new ArgumentNullException ("ActivityManual.MachineMode");
        }
        m_machineMode = value;
      }
    }

    /// <summary>
    /// Reference to the manual MachineMode for XML serialization
    /// </summary>
    [XmlElement ("MachineMode")]
    public virtual MachineMode XmlSerializationMachineMode
    {
      get { return this.MachineMode as MachineMode; }
      set { this.MachineMode = value; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected ActivityManual ()
    { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="mode"></param>
    /// <param name="range"></param>
    public ActivityManual (IMonitoredMachine machine, IMachineMode mode, UtcDateTimeRange range)
      : base (machine, range)
    {
      this.MachineMode = mode;
    }

    /// <summary>
    /// Constructor for abstract modifications, that are kept transient
    /// 
    /// You may add a mainModification reference to be used in the analysis logs
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="mode"></param>
    /// <param name="range"></param>
    /// <param name="mainModification"></param>
    ActivityManual (IMachine machine, IMachineMode mode, UtcDateTimeRange range, IModification mainModification)
      : base (machine, range, mainModification)
    {
      this.MachineMode = mode;
    }
    #endregion // Constructors

    #region MachineAssociation implementation
    /// <summary>
    /// <see cref="PeriodAssociation.ConvertToSlot" />
    /// </summary>
    /// <returns></returns>
    public override TSlot ConvertToSlot<TSlot> ()
    {
      var slot =
        GenericMachineRangeSlot.Create (typeof (TSlot),
                                        this.Machine,
                                        new UtcDateTimeRange (this.Begin, this.End)) as TSlot;

      if (slot is IReasonSlot) {
        // Activity detection is responsible for the creation
        // of the slot
        var reasonSlot = slot as IReasonSlot;
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

      if (oldSlot is IReasonSlot) {
        IReasonSlot oldReasonSlot = oldSlot as IReasonSlot;
        IReasonSlot newReasonSlot = (IReasonSlot)oldReasonSlot.Clone ();
        ((ReasonSlot)newReasonSlot).SetOldSlotFromModification (oldReasonSlot, this);
        newReasonSlot.MachineMode = this.MachineMode;

        if (oldReasonSlot.ReasonSource.Equals (ReasonSource.Manual)) {
          // Check the new MachineMode is compatible with the
          // MachineObservationState and Reason, else raise a warning
          bool isCompatible = GetReasonExtensions ()
            .Any (ext => ext.IsCompatible (range, newReasonSlot.MachineMode, newReasonSlot.MachineObservationState, oldReasonSlot.Reason, oldReasonSlot.ReasonScore, newReasonSlot.ReasonSource));
          if (!isCompatible) {
            string message = $"The new MachineMode {this.MachineMode} is not compatible with the existing MachineObservationState {oldReasonSlot.MachineObservationState} and Reason {oldReasonSlot.Reason}";
            log.Warn ($"MergeDataWithOldSlot: {message}");
            AddAnalysisLog (LogLevel.WARN, message);
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

      if (Bound.Compare<DateTime> (this.End, this.Begin) < 0) { // Empty period: error
        string message = string.Format ("End={0} before Begin={1}",
                                        this.End, this.Begin);
        log.ErrorFormat ("MakeAnalysis: " +
                         "{0} " +
                         "=> finish in error",
                         message);
        AddAnalysisLog (LogLevel.ERROR, message);
        MarkAsError ();
        return;
      }

      IMachineStatus machineStatus = null;
      if (this.Machine.IsMonitored ()
          && ((AnalysisStatus.New == this.AnalysisStatus)
              || (AnalysisStatus.Pending == this.AnalysisStatus))) {
        // Check MachineStatus
        machineStatus =
          ModelDAOHelper.DAOFactory.MachineStatusDAO
          .FindById (this.Machine.Id);
        if (null == machineStatus) {
          IMonitoredMachine monitoredMachine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (this.Machine.Id);
          if (null == monitoredMachine) {
            string machineMessage = $"no monitored machine with ID {this.Machine.Id} although this machine is flagged as monitored";
            log.Error ($"MakeAnalysis: {machineMessage}");
            AddAnalysisLog (LogLevel.ERROR,
                            machineMessage);
            this.MarkAsError ();
            return;
          }
          string message = $"no MachineStatus found for {this.Machine}";
          log.Error ($"MakeAnalysis: {message}");
          AddAnalysisLog (LogLevel.ERROR,
                          message);
          // Do not mark the modification in error to ease some unit tests
        }

        if (null != machineStatus) {
          if (Bound.Compare<DateTime> (machineStatus.ReasonSlotEnd, this.Begin) < 0) { // Future
            log.DebugFormat ("MakeAnalysis: " +
                             "future MachineMode association");
            this.MarkAsPending (machineStatus.ReasonSlotEnd);
            return;
          }
          else if (Bound.Compare<DateTime> (machineStatus.ReasonSlotEnd, this.End) < 0) { // Current
            // This impacts the current period => update machineStatus
            log.DebugFormat ("MakeAnalysis: " +
                             "current MachineMode association");
            machineStatus.ManualActivity = true;
            machineStatus.MachineMode = this.MachineMode;
            machineStatus.ManualActivityEnd = (DateTime?)this.End;
            // Note: because it is just a manual activity, do not process here AutoMachineStateTemplate
          }
        }
      }

      // Get the adjusted step range
      UtcDateTimeRange range = GetNotAppliedRange ();

      // Analyze ! with the adjusted begin and end
      ActivityManual association = new ActivityManual (this.Machine,
                                                       this.MachineMode,
                                                       range,
                                                       this);
      association.DateTime = this.DateTime;
      association.Option = this.Option;
      association.Caller = this;
      association.ProcessAssociation ();


      if (AnalysisConfigHelper.OperationSlotRunTime) {
        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().Debug ("MakeAnalysis: Operation slot run time update");
        }
        UpdateOperationSlotRunTimes (machineStatus, range);
      }

      // Analysis is done
      MarkAsCompleted ("Cache/ClearDomainByMachine/MachineModeAssociation/" + this.Machine.Id + "?Broadcast=true",
                     (DateTime?)range.Upper); // => InProgress or Done
    }

    void UpdateOperationSlotRunTimes (IMachineStatus machineStatus, UtcDateTimeRange range)
    {
      DateTime? reasonSlotEnd = null;
      if (null != machineStatus) {
        reasonSlotEnd = machineStatus.ReasonSlotEnd;
      }

      SetActive ();
      var operationSlots = ModelDAOHelper.DAOFactory.OperationSlotDAO
        .FindOverlapsRange (this.Machine, range);
      foreach (var operationSlot in operationSlots) {
        SetActive ();
        if (range.ContainsRange (operationSlot.DateTimeRange)) {
          var isRunning = this.MachineMode.Running ?? false;
          if (isRunning) {
            if (!reasonSlotEnd.HasValue) {
              var localMachineStatus = ModelDAOHelper.DAOFactory.MachineStatusDAO
                .FindById (this.Machine.Id);
              if (localMachineStatus is null) {
                GetLogger ().Error ($"UpdateOperationSlotRunTimes: no machine status for machine {this.Machine.Id}");
                reasonSlotEnd = DateTime.UtcNow;
              }
              else {
                reasonSlotEnd = localMachineStatus.ReasonSlotEnd;
              }
            }
            Debug.Assert (reasonSlotEnd.HasValue);
            UtcDateTimeRange runTimeRange = new UtcDateTimeRange (range.Intersects (new UtcDateTimeRange (new LowerBound<DateTime> (), reasonSlotEnd.Value)));
            if (runTimeRange.IsEmpty ()) {
              if (GetLogger ().IsDebugEnabled) {
                GetLogger ().Debug ($"UpdateOperationSlotRunTimes: runTimeRange empty, probably in the future, do nothing");
              }
            }
            else { // !runTimeRange.IsEmpty ()
              Debug.Assert (runTimeRange.Upper.HasValue);
              Debug.Assert (runTimeRange.Duration.HasValue);
              operationSlot.RunTime = runTimeRange.Duration.Value;
            }
          }
          else { // !isRunning
            operationSlot.RunTime = TimeSpan.FromSeconds (0);
          }
        }
        else {
          operationSlot.ConsolidateRunTime ();
        }
      }
      SetActive ();
    }

    /// <summary>
    /// Apply the modification while keeping it transient
    /// 
    /// It should be never called, because there is no transient ActivityManual to process.
    /// Use a persistent entity instead and MakeAnalysis.
    /// </summary>
    public override void Apply ()
    {
      Debug.Assert (false);
      log.FatalFormat ("Apply: not implemented/supported");
      throw new NotImplementedException ();
    }

    /// <summary>
    /// Process the association itself,
    /// without updating any Modification property
    /// </summary>
    protected internal virtual void ProcessAssociation ()
    {
      ReasonSlotDAO reasonSlotDAO = new ReasonSlotDAO ();
      reasonSlotDAO.Caller = this;
      Insert<ReasonSlot, IReasonSlot, ReasonSlotDAO> (reasonSlotDAO);
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
    /// Get the ReasonCompatibilityExtensions (lazy initialization)
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

      log.DebugFormat ("ComputeNewSpan: " +
                       "no better end with the observation state slots, " +
                       "use the base method instead");
      return base.ComputeNewStepSpan (range);
    }

    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public override void Unproxy ()
    {
      base.Unproxy ();
      NHibernateHelper.Unproxy<IMachineMode> (ref m_machineMode);
    }
  }
}
