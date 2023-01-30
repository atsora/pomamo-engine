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
using Lemoine.Extensions.Database;
using System.Threading;
using Pulse.Extensions.Database;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Modification class to easily process and insert the coming machine mode
  /// from the CNC only in the analysis tables.
  /// 
  /// This is not a persistent class, no database table is associated to it !
  /// </summary>
  public class CncActivityMachineAssociation : MachineAssociation, ICloneable
  {
    #region Members
    IMachineMode m_machineMode;
    IMachineStateTemplate m_machineStateTemplate;
    IMachineObservationState m_machineObservationState;
    IShift m_shift;
    #endregion // Members

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected CncActivityMachineAssociation ()
    { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="machineMode"></param>
    /// <param name="machineStateTemplate"></param>
    /// <param name="machineObservationState"></param>
    /// <param name="range"></param>
    public CncActivityMachineAssociation (IMachine machine,
                                          IMachineMode machineMode,
                                          IMachineStateTemplate machineStateTemplate,
                                          IMachineObservationState machineObservationState,
                                          UtcDateTimeRange range)
      : base (machine, range)
    {
      m_machineMode = machineMode;
      m_machineStateTemplate = machineStateTemplate;
      m_machineObservationState = machineObservationState;
    }
    #endregion // Constructors

    #region Getters / Setters
    /// <summary>
    /// <see cref="IModification"/>
    /// </summary>
    public override string ModificationType
    {
      get { return "CncActivityMachineAssociation"; }
    }

    /// <summary>
    /// Reference to the Machine Mode
    /// 
    /// It can't be null
    /// </summary>
    public IMachineMode MachineMode
    {
      get
      {
        Debug.Assert (null != m_machineMode);
        return m_machineMode;
      }
      set
      {
        if (null == value) {
          log.Fatal ("CncActivityMachineAssociation: " +
                     "MachineMode is mandatory");
          Debug.Assert (null != value);
          throw new ArgumentNullException ("MachineMode");
        }
        else {
          m_machineMode = value;
        }
      }
    }

    /// <summary>
    /// Reference to a Machine State Template
    /// 
    /// It may be null
    /// </summary>
    public virtual IMachineStateTemplate MachineStateTemplate
    {
      get { return m_machineStateTemplate; }
      set { m_machineStateTemplate = value; }
    }

    /// <summary>
    /// Reference to the Machine Observation State
    /// 
    /// It can't be null
    /// </summary>
    public virtual IMachineObservationState MachineObservationState
    {
      get
      {
        Debug.Assert (null != m_machineObservationState);
        return m_machineObservationState;
      }
      set
      {
        if (null == value) {
          log.Fatal ("CncActivityMachineAssociation: " +
                     "MachineObservationState is mandatory");
          Debug.Assert (null != value);
          throw new ArgumentNullException ("MachineObservationState");
        }
        else {
          m_machineObservationState = value;
        }
      }
    }

    /// <summary>
    /// Reference to the Shift
    /// 
    /// Can be null
    /// </summary>
    public virtual IShift Shift
    {
      get { return m_shift; }
      set { m_shift = value; }
    }

    /// <summary>
    /// UTC end date/time (mandatory here) of the association
    /// </summary>
    [XmlIgnore]
    public override UpperBound<DateTime> End
    {
      get {
        if (!base.End.HasValue) {
          log.Fatal ("CncActivityMachineAssociation: end should not be +oo");
        }
        return base.End;
      }
      set
      {
        if (!value.HasValue) {
          log.Fatal ("CncActivityMachineAssociation: " +
                     "End is mandatory in this association");
          throw new ArgumentNullException ("End");
        }
        else {
          base.End = value;
        }
      }
    }
    #endregion // Getters / Setters

    #region ICloneable implementation
    /// <summary>
    /// Make a shallow copy
    /// <see cref="ICloneable.Clone" />
    /// </summary>
    /// <returns></returns>
    public object Clone ()
    {
      return this.MemberwiseClone ();
    }
    #endregion // ICloneable implementation

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

      if (slot is IReasonSlot) {
        var reasonSlot = slot as IReasonSlot;
        reasonSlot.MachineMode = this.MachineMode;
        reasonSlot.MachineObservationState = this.MachineObservationState;
        reasonSlot.Shift = this.Shift;
        ((ReasonSlot)reasonSlot).SetNewActivitySlot ();
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
      Debug.Assert (oldSlot is GenericMachineRangeSlot);
      Debug.Assert (this.Machine.Equals ((oldSlot as IPartitionedByMachine).Machine));

      if (oldSlot is IReasonSlot) {
        // This should not happen !
        Debug.Assert (false);
        log.FatalFormat ("MergeDataWithOldSlot: " +
                         "unexpected ReasonSlot");
        throw new ArgumentException ("CncActivity should not overlap an existing ReasonSlot");
      }
      else {
        System.Diagnostics.Debug.Assert (false);
        log.FatalFormat ("MergeDataWithOldSlot: " +
                         "trying to merge the association with a not supported slot {0}",
                         typeof (TSlot));
        throw new ArgumentException ("Not supported machine slot");
      }
    }
    #endregion // MachineAssociation implementation

    #region Modification implementation
    /// <summary>
    /// MakeAnalysis
    /// 
    /// Not valid because this modification is always transient
    /// </summary>
    public override void MakeAnalysis ()
    {
      Debug.Assert (false);
      log.FatalFormat ("MakeAnalysis: not valid");
      throw new NotImplementedException ("ActivityReasonMachineAssociation.MakeAnalysis");
    }

    /// <summary>
    /// Apply the modification while keeping it transient
    /// </summary>
    public override void Apply ()
    {
      Debug.Assert (null != ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (this.Machine.Id));

      IMachineStatus machineStatus =
        ModelDAOHelper.DAOFactory.MachineStatusDAO
        .FindById (this.Machine.Id);
      if (false == ProcessAssociation (machineStatus)) {
        log.ErrorFormat ("MakeAnalysis: " +
                         "only partially applied because the observation state slots may have changed " +
                         "because of the auto machine state template process");
        throw new Exception ("Partially applied");
      }
    }

    /// <summary>
    /// Process the association itself,
    /// without updating any Modification property
    /// </summary>
    /// <param name="machineStatus">null if it does not exist yet</param>
    /// <returns>it was fully processed else it was only partially processed and the observation state slots may have changed</returns>
    public virtual bool ProcessAssociation (IMachineStatus machineStatus)
    {
      if (null != machineStatus) {
        if (false == ProcessAssociationWithExistingCurrentStatus (machineStatus)) {
          log.Info ("ProcessAssociation: it was only partially applied because some observation state slots changed");
          return false;
        }
        if (log.IsDebugEnabled) {
          log.Debug ($"ProcessAssociation: ok for {this.MachineMode} in {this.Range}");
        }
        return true;
      }
      else {
        return ProcessAssociationWithNoCurrentStatus ();
      }
    }

    bool ProcessAssociationWithExistingCurrentStatus (IMachineStatus machineStatus)
    {
      Debug.Assert (this.End.HasValue);
      Debug.Assert (UpperBound.Compare<DateTime> (machineStatus.ReasonSlotEnd, machineStatus.ConsolidationLimit) <= 0);

      SetActive ();

      if (Bound.Compare<DateTime> (this.Begin, machineStatus.ReasonSlotEnd) < 0) {
        // Second ActivityDetection
        Debug.Assert (machineStatus.CncMachineMode.Id.Equals (this.MachineMode.Id));
        Debug.Assert (object.Equals (machineStatus.MachineStateTemplate, this.MachineStateTemplate));
        Debug.Assert (machineStatus.MachineObservationState.Id.Equals (this.MachineObservationState.Id));
        Debug.Assert (((null == machineStatus.Shift) && (null == this.Shift))
                      || object.Equals (machineStatus.Shift.Id, this.Shift.Id));
        if (Bound.Compare<DateTime> (this.End, machineStatus.ReasonSlotEnd) <= 0) { // Already processed
          return true;
        }
        else {
          this.Begin = machineStatus.ReasonSlotEnd;
        }
      }

      Debug.Assert (Bound.Compare<DateTime> (machineStatus.ReasonSlotEnd, this.Begin) <= 0);

      if (Bound.Compare<DateTime> (machineStatus.ReasonSlotEnd, this.Begin) < 0) { // No data period
        Debug.Assert (this.Begin.HasValue);
        AddNoDataPeriod (machineStatus, machineStatus.ReasonSlotEnd, this.Begin.Value);
      }
      SetActive ();

      // Extensions including CheckEventLongPeriod
      if (!ProcessExtensions (machineStatus)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"ProcessAssociationWithExistingCUrrentStatus: ProcessExtensions returned false, give up for now");
        }
        return false;
      }
      SetActive ();

      if (machineStatus.ManualActivity
          && (Bound.Compare<DateTime> (new UpperBound<DateTime> (machineStatus.ManualActivityEnd), this.Begin) <= 0)) {
        // Obsolete manual activity, reset it
        log.Debug ("ProcessAssociationWithExistingCurrentStatus: obsolete manual activity, reset it");
        machineStatus.ManualActivity = false;
        machineStatus.ManualActivityEnd = null;
      }

      if (!machineStatus.CncMachineMode.Equals (this.MachineMode)
          || !machineStatus.MachineObservationState.Equals (this.MachineObservationState)
          || !object.Equals (machineStatus.Shift, this.Shift)) {
        // CncMachineMode or MachineObservationState or Shift changed
        // => reset a manual current activity
        log.Debug ("ProcessAssociationWithExistingCurrentStatus: CncMachineMode or MachineObservationState changed => reset a manual current activity");
        machineStatus.ManualActivity = false;
        machineStatus.ManualActivityEnd = null;
      }

      // Update current MachineMode and MachineObservationState
      machineStatus.CncMachineMode = this.MachineMode;
      bool machineModeOrObservationStateOrShiftUpdate = false;
      if (!machineStatus.ManualActivity) {
        if (!machineStatus.MachineMode.Equals (this.MachineMode)) {
          machineStatus.MachineMode = this.MachineMode;
          machineModeOrObservationStateOrShiftUpdate = true;
        }
      }
      if (!machineStatus.MachineObservationState.Equals (this.MachineObservationState)) {
        machineStatus.MachineObservationState = this.MachineObservationState;
        machineModeOrObservationStateOrShiftUpdate = true;
      }
      machineStatus.MachineStateTemplate = this.MachineStateTemplate;
      if (!NHibernateHelper.EqualsNullable (machineStatus.Shift, this.Shift, (a, b) => a.Id == b.Id)) {
        machineStatus.Shift = this.Shift;
        machineModeOrObservationStateOrShiftUpdate = true;
      }
      SetActive ();

      AssociateActivityReason (machineStatus, machineModeOrObservationStateOrShiftUpdate);
      SetActive ();

      // If the currentStatus is not Processing
      // => invalidate the cache
      if ((int)ReasonId.Processing != machineStatus.Reason.Id) {
        var message = "Cache/ClearDomainByMachine/ReasonAssociation/" + this.Machine.Id + "?Broadcast=true";
        AnalysisAccumulator.PushMessage (message);
        SetActive ();
      }

      return true;
    }

    bool ProcessAssociationWithNoCurrentStatus ()
    {
      if (!ProcessExtensions (null)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"ProcessAssociationWithNoCurrentStatus: ProcessExtensions returned false, give up for now");
        }
        return false;
      }
      SetActive ();

      // Create the corresponding ReasonSlot
      ReasonSlotDAO reasonSlotDAO = new ReasonSlotDAO ();
      reasonSlotDAO.Caller = this;
      Insert<ReasonSlot, IReasonSlot, ReasonSlotDAO> (reasonSlotDAO);
      SetActive ();

      // Get the associated MonitoredMachine
      IMonitoredMachine monitoredMachine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
        .FindById (this.Machine.Id);

      // Create the MachineStatus
      Debug.Assert (this.End.HasValue);
      MachineStatus machineStatus = new MachineStatus (monitoredMachine);
      machineStatus.MachineStateTemplate = this.MachineStateTemplate;
      machineStatus.MachineObservationState = this.MachineObservationState;
      machineStatus.Shift = this.Shift;
      machineStatus.CncMachineMode = this.MachineMode;
      machineStatus.MachineMode = this.MachineMode;
      machineStatus.ManualActivity = false;
      machineStatus.ReasonSlotEnd = this.End.Value;
      machineStatus.SetReasonFromLastReasonSlot ();
      ModelDAOHelper.DAOFactory.MachineStatusDAO.MakePersistent (machineStatus);

      return true;
    }

    bool ProcessExtensions (IMachineStatus machineStatus)
    {
      var monitoredMachine = this.Machine as IMonitoredMachine;
      var extensions = Lemoine.Business.ServiceProvider
        .Get (new Lemoine.Business.Extension.MonitoredMachineExtensions<ICncActivityExtension> (monitoredMachine, (ext, m) => ext.Initialize (m)));
      foreach (var extension in extensions) {
        var result = extension.ProcessAssociation (this, this.Range, this.MachineMode, this.MachineStateTemplate, this.MachineObservationState, this.Shift, machineStatus);
        if (!result) {
          log.Info ($"ProcessExtension: false was returned by {extension}, return false");
          return false;
        }
        SetActive ();
      }
      return true;
    }

    void AddNoDataPeriod (IMachineStatus machineStatus,
                          DateTime begin,
                          DateTime end)
    {
      log.DebugFormat ("AddNoDataPeriod {0}-{1}",
                       begin, end);

      IMachineMode noDataMachineMode =
        ModelDAOHelper.DAOFactory.MachineModeDAO
        .FindById ((int)MachineModeId.NoData);
      Debug.Assert (null != noDataMachineMode);

      IList<IObservationStateSlot> noDataObservationStateSlots =
        ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
        .GetListInRange (this.Machine, begin, end);

      // - Check all the machine observation states were already processed from the template
      bool oneMachineStateTemplateProcessed = false;
      foreach (IObservationStateSlot noDataObservationStateSlot in noDataObservationStateSlots) {
        SetActive ();
        if (null == noDataObservationStateSlot.MachineObservationState) {
          log.DebugFormat ("AddNoDataPeriod {0}-{1}: " +
                           "Process template between {2} and {3}",
                           begin, end,
                           noDataObservationStateSlot.BeginDateTime, end);
          // Note: this should happen very rarely
          // - Process the template between noDataObservationStateSlot.BeginDateTime and end
          // TODO: cancellationToken
          if (false == ((ObservationStateSlot)noDataObservationStateSlot)
              .ProcessTemplate (CancellationToken.None, new UtcDateTimeRange (noDataObservationStateSlot.BeginDateTime, end),
                                null, true, null, null)) {
            Debug.Assert (false); // This should not happen because maxAnalysisDateTime parameter is null
            log.FatalFormat ("AddNoDataPeriod: " +
                             "ProcessTemplate interrupted although maxAnalysisDateTime is not set");
            throw new Lemoine.GDBPersistentClasses.InterruptedAnalysis ("AddNoDataPeriod");
          }
          oneMachineStateTemplateProcessed = true;
        }
      }
      if (oneMachineStateTemplateProcessed) {
        // - Reload noDataObservationStateSlots
        noDataObservationStateSlots =
          ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
          .GetListInRange (this.Machine, begin, end);
      }

      // - Now noDataObservationStateSlots should not have a null MachineObservationState,
      //   process them
      foreach (IObservationStateSlot noDataObservationStateSlot
               in noDataObservationStateSlots) {
        SetActive ();
        Debug.Assert (null != noDataObservationStateSlot.MachineObservationState);
        if (null == noDataObservationStateSlot.MachineObservationState) {
          log.ErrorFormat ("AddNoDataPeriod {0}-{1}: " +
                           "null machine observation state in slot {2}, " +
                           "may be because the transaction was not serializable",
                           begin, end,
                           noDataObservationStateSlot);
          throw new Lemoine.GDBUtils.TransientAnalysisException ("AddNoDataPeriod: null machine observation state in slot");
        }
        UtcDateTimeRange range;
        {
          var rangeLower = LowerBound.GetMaximum<DateTime> (noDataObservationStateSlot.BeginDateTime, begin);
          var rangeEnd = UpperBound.GetMinimum<DateTime> (end, noDataObservationStateSlot.EndDateTime);
          range = new UtcDateTimeRange (rangeLower, rangeEnd);
        }
        CncActivityMachineAssociation association =
          new CncActivityMachineAssociation (this.Machine,
          noDataMachineMode,
          noDataObservationStateSlot.MachineStateTemplate,
          noDataObservationStateSlot.MachineObservationState,
          range);
        association.Caller = this;
        association.Shift = noDataObservationStateSlot.Shift;
        Debug.Assert (association.End.HasValue);
        Debug.Assert (Bound.Compare<DateTime> (association.Begin, association.End.Value) < 0);
        if (false == association.ProcessAssociation (machineStatus)) {
          log.FatalFormat ("AddNoDataPeriod: " +
                           "Machine mode NoData should be not configured with an AutoMachineStateTemplate " +
                           "so true should be always returned");
          Debug.Assert (false);
          throw new Exception ("Unexpected behavior");
        }
      }
      SetActive ();

      log.DebugFormat ("AddNoDataPeriod {0}-{1} ok",
                       begin, end);
    }

    UpperBound<DateTime> GetManualActivityLimit (IMachineStatus machineStatus)
    {
      if (machineStatus.ManualActivity && machineStatus.ManualActivityEnd.HasValue) {
        return UpperBound.GetMinimum<DateTime> (this.End, machineStatus.ManualActivityEnd.Value);
      }
      else {
        return this.End;
      }
    }

    bool TryFastProcess (IMachineStatus machineStatus, bool machineModeOrObservationStateUpdate, out UpperBound<DateTime> effectiveEnd)
    {
      // Check if the fast process is available
      if (machineModeOrObservationStateUpdate
        || (UpperBound.Compare (machineStatus.ConsolidationLimit, this.End) < 0)) {
        Debug.Assert (this.Begin.HasValue, "CncActivityMachineAssociation with Start=-oo");
        effectiveEnd = this.Begin.Value;
        return false;
      }
      else { // Fast process applicable
        // Extend the reason slot until the minimum between this.End and ManualActivityEnd
        effectiveEnd = GetManualActivityLimit (machineStatus);
        Debug.Assert (effectiveEnd.HasValue, "Invalid effectiveEnd for manual activity=+oo");
        Debug.Assert (Bound.Compare<DateTime> (this.Begin, effectiveEnd) < 0);
        var reasonSlot = ModelDAOHelper.DAOFactory.ReasonSlotDAO
          .GetLast (this.Machine);
        Debug.Assert (null != reasonSlot);
        if (null == reasonSlot) {
          effectiveEnd = this.Begin.Value;
          return false;
        }
        // null != reasonSlot
        Debug.Assert (reasonSlot.Reason.Id == machineStatus.Reason.Id, "Incompatible reason between machine status and reason slot");
        Debug.Assert (Bound.Equals (reasonSlot.EndDateTime, machineStatus.ReasonSlotEnd), "Incompatible end between machine status and reason slot");
        Debug.Assert (reasonSlot.ReasonSource.IsSameMainSource (machineStatus.ReasonSource), "Incompatible reason source between machine status and reason slot", string.Format ("{0} VS {1}", reasonSlot.ReasonSource, machineStatus.ReasonSource)); // Not necessarily all the same Unsafe flags
        Debug.Assert (reasonSlot.ReasonScore == machineStatus.ReasonScore, "Incompatible reason source between machine status and reason slot");
        Debug.Assert (reasonSlot.ReasonSource.HasFlag (ReasonSource.UnsafeAutoReasonNumber) || machineStatus.ReasonSource.HasFlag (ReasonSource.UnsafeAutoReasonNumber) || (reasonSlot.AutoReasonNumber == machineStatus.AutoReasonNumber), "Incompatible auto reason # between machine status and reason slot", string.Format ("{0}/{1} VS {2}/{3}", reasonSlot.ReasonSource, reasonSlot.AutoReasonNumber, machineStatus.ReasonSource, machineStatus.AutoReasonNumber));
        using (var slotModificationTracker = new SlotModificationTracker<IReasonSlot> (reasonSlot)) {
          var oldSlot = slotModificationTracker.OldSlot;
          ((ReasonSlot)reasonSlot).SetNewActivitySlot ();
          reasonSlot.EndDateTime = effectiveEnd;
          ((ReasonSlot)reasonSlot).Consolidate (oldSlot, this);
          ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
        }
        machineStatus.ReasonSlotEnd = effectiveEnd.Value;
        SetActive ();
        return true;
      }
    }

    /// <summary>
    /// Associate the corresponding activity reason
    /// </summary>
    /// <param name="machineStatus"></param>
    /// <param name="machineModeOrObservationStateOrShiftUpdate"></param>
    void AssociateActivityReason (IMachineStatus machineStatus, bool machineModeOrObservationStateOrShiftUpdate)
    {
      Debug.Assert (null != machineStatus.Reason);

      UpperBound<DateTime> associationEnd;

      if (!TryFastProcess (machineStatus, machineModeOrObservationStateOrShiftUpdate, out associationEnd)) {
        // Before Manual activity
        // and the consolidated data can't be used
        associationEnd = GetManualActivityLimit (machineStatus);
        ActivityReasonMachineAssociation association =
          new ActivityReasonMachineAssociation (this.Machine, this.Range);
        association.MachineObservationState = machineStatus.MachineObservationState;
        association.Shift = machineStatus.Shift;
        association.MachineMode = machineStatus.MachineMode;
        association.End = associationEnd;
        association.ProcessAssociation ();
      }
      SetActive ();

      if (Bound.Compare<DateTime> (associationEnd, this.End) < 0) {
        // End period: after ManualActivityEnd
        Debug.Assert (associationEnd.HasValue);
        ActivityReasonMachineAssociation association =
          new ActivityReasonMachineAssociation (this.Machine,
                                                new UtcDateTimeRange (associationEnd.Value,
                                                                      this.End));
        association.MachineObservationState = machineStatus.MachineObservationState;
        association.Shift = machineStatus.Shift;
        association.MachineMode = this.MachineMode;
        association.ProcessAssociation ();
        SetActive ();
      }
    }

    /// <summary>
    /// Check if an auto machine state template must be processed
    /// </summary>
    /// <param name="newMachineMode"></param>
    /// <param name="currentMachineStateTemplate"></param>
    /// <returns>an auto machine state template was processed, and the observation state slots were updated</returns>
    bool CheckAutoMachineStateTemplate (IMachineMode newMachineMode,
                                        IMachineStateTemplate currentMachineStateTemplate)
    {
      Debug.Assert (null != newMachineMode);
      IMachineStateTemplate newMachineStateTemplate = null;

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ("CncActivityMachineAssociation.CheckAutoMachineStateTemplate")) {
        if (null != currentMachineStateTemplate) {
          // Check first a AutoMachineStateTemplate that applies only for currentMachineStateTemplate
          IAutoMachineStateTemplate automst = ModelDAOHelper.DAOFactory.AutoMachineStateTemplateDAO
            .Find (newMachineMode, currentMachineStateTemplate);
          if (null != automst) {
            Debug.Assert (object.Equals (automst.MachineMode, newMachineMode));
            Debug.Assert (object.Equals (automst.Current, currentMachineStateTemplate));
            newMachineStateTemplate = automst.New;
          }
        }
        SetActive ();
        if (null == newMachineStateTemplate) {
          // If newMachineStateTemplate is not known yet,
          // try with no currentMachineStateTemplate criteria
          IAutoMachineStateTemplate automst = ModelDAOHelper.DAOFactory.AutoMachineStateTemplateDAO
            .Find (newMachineMode);
          if (null != automst) {
            Debug.Assert (object.Equals (automst.MachineMode, newMachineMode));
            Debug.Assert (null == automst.Current);
            newMachineStateTemplate = automst.New;
          }
        }

        if ((null == newMachineStateTemplate)
            || (object.Equals (newMachineStateTemplate, currentMachineStateTemplate))) {
          transaction.Commit ();
          return false;
        }
        else { // null != newMachineStateTemplate
          // Apply the new machine state template
          IMachineStateTemplateAssociation association = ModelDAOHelper.ModelFactory
            .CreateMachineStateTemplateAssociation (this.Machine, newMachineStateTemplate,
                                                    new UtcDateTimeRange (this.Begin, new UpperBound<DateTime> (null)));
          association.Apply ();
          transaction.Commit ();
          return true;
        }
      }
    }
    #endregion // Modification implementation

    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public override void Unproxy ()
    {
      // This is not a persistent class, do nothing
      throw new NotImplementedException ("Not a persistent class");
    }
  }
}
