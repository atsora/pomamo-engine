// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System.Linq;
using Lemoine.Extensions.AutoReason.ActionableAutoReasonExtension;

namespace Lemoine.Extensions.AutoReason
{
  /// <summary>
  /// Base class for an auto-reason extension
  /// </summary>
  public abstract class AutoReasonExtensionBase<TConfiguration>
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<TConfiguration>
    , IAutoReasonExtension
    , IActionableAutoReason
    , IDateTimeStateAutoReason
    , IMachineModuleDateTimeStateAutoReason
    , IApplyReasonAutoReason
    , IApplyReasonDynamicEndBeforeRealEndAutoReason
    where TConfiguration : AutoReasonConfiguration, new ()
  {
    static readonly string FIRST_RUN_PERIOD_KEY = "AutoReason.FirstRunPeriod";
    static readonly TimeSpan FIRST_RUN_PERIOD_DEFAULT = TimeSpan.FromDays (3);

    static readonly string USE_REVISION_KEY = "AutoReason.Revision";
    static readonly bool USE_REVISION_DEFAULT = false;

    static readonly string DATETIME_KEY = "DateTime";

    Lemoine.Threading.IChecked m_caller = null;
    IMonitoredMachine m_machine;
    IReason m_reason;
    double m_reasonScore;
    double? m_manualScore;

    string m_defaultReasonTranslationKey;
    string m_defaultReasonTranslationValue;
    readonly string m_pluginKey;

    readonly IList<IStateAction> m_delayedStateActions = new List<IStateAction> ();
    readonly IList<IReasonAction> m_delayedReasonActions = new List<IReasonAction> ();

    DateTime m_dateTime = DateTime.UtcNow;
    IRevision m_revision = null;
    IService m_service = null;
    readonly IDictionary<int, DateTime> m_dateTimeByMachineModule = new Dictionary<int, DateTime> ();

    /// <summary>
    /// <see cref="IAutoReasonExtension"/>
    /// </summary>
    public IEnumerable<IStateAction> DelayedStateActions {
      get { return m_delayedStateActions; }
    }

    /// <summary>
    /// <see cref="IAutoReasonExtension"/>
    /// </summary>
    public IEnumerable<IReasonAction> DelayedReasonActions
    {
      get { return m_delayedReasonActions; }
    }

    /// <summary>
    /// Logger
    /// </summary>
    /// <returns></returns>
    public abstract ILog GetLogger ();

    /// <summary>
    /// Plugin key that corresponds to a prefix in autoreasonstate
    /// </summary>
    public virtual string PluginKey
    {
      get { return m_pluginKey; }
    }

    /// <summary>
    /// Default reason translation key
    /// </summary>
    public virtual string DefaultReasonTranslationKey
    {
      get { return m_defaultReasonTranslationKey; }
    }

    /// <summary>
    /// Associated machine
    /// </summary>
    public IMonitoredMachine Machine
    {
      get { return m_machine; }
    }

    /// <summary>
    /// Associated reason
    /// 
    /// Not null if Initialize returns true
    /// </summary>
    public IReason Reason
    {
      get { return m_reason; }
    }

    /// <summary>
    /// Associated reason score
    /// </summary>
    public double ReasonScore
    {
      get { return m_reasonScore; }
    }

    /// <summary>
    /// Associated manual score
    /// </summary>
    public double? ManualScore
    {
      get { return m_manualScore; }
    }

    /// <summary>
    /// Associated date/time
    /// </summary>
    public DateTime DateTime
    {
      get { return m_dateTime; }
    }

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="defaultReasonTranslationKey"></param>
    /// <param name="pluginKey"></param>
    protected AutoReasonExtensionBase (string defaultReasonTranslationKey, string pluginKey)
    {
      m_defaultReasonTranslationKey = defaultReasonTranslationKey;
      m_pluginKey = pluginKey;
    }

    /// <summary>
    /// Constructor with no default reason
    /// </summary>
    protected AutoReasonExtensionBase (string pluginKey)
    {
      m_pluginKey = pluginKey;
    }
    #endregion // Constructors

    #region Lemoine.Threading.IChecked implementation
    /// <summary>
    /// <see cref="Lemoine.Threading.IChecked"/>
    /// </summary>
    public void SetActive ()
    {
      if (null != m_caller) {
        m_caller.SetActive ();
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Threading.IChecked"/>
    /// </summary>
    public void PauseCheck ()
    {
      if (null != m_caller) {
        m_caller.PauseCheck ();
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Threading.IChecked"/>
    /// </summary>
    public void ResumeCheck ()
    {
      if (null != m_caller) {
        m_caller.ResumeCheck ();
      }
    }
    #endregion // Lemoine.Threading.IChecked implementation

    /// <summary>
    /// Build a key for autoreasonstate
    /// </summary>
    /// <param name="subKey"></param>
    /// <returns></returns>
    public virtual string GetKey (string subKey)
    {
      var key = this.PluginKey;
      if (null != this.ConfigurationContext) {
        key += "." + this.ConfigurationContext.InstanceId;
      }
      key += "." + subKey;
      return key;
    }

    /// <summary>
    /// Build a key for autoreasonstate when it is specific to a machine module
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="subKey"></param>
    /// <returns></returns>
    public string GetKey (IMachineModule machineModule, string subKey)
    {
      Debug.Assert (null != machineModule);

      return GetKey (subKey) + "." + machineModule.Id;
    }

    /// <summary>
    /// Initialize
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="caller"></param>
    /// <returns></returns>
    public virtual bool Initialize (IMonitoredMachine machine, Lemoine.Threading.IChecked caller)
    {
      Debug.Assert (null != machine);

      m_machine = machine;
      m_caller = caller;

      if (!LoadConfiguration (out var configuration)) {
        GetLogger ().Warn ("Initialize: the configuration is not valid");
        return false;
      }

      return Initialize (configuration);
    }

    /// <summary>
    /// Initialize (configuration part)
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    protected virtual bool Initialize (TConfiguration configuration)
    {
      m_reasonScore = configuration.ReasonScore;
      m_manualScore = configuration.ManualScore;
      if (!string.IsNullOrEmpty (configuration.DefaultReasonTranslationKey)) {
        Debug.Assert (!string.IsNullOrEmpty (configuration.DefaultReasonTranslationValue));
        m_defaultReasonTranslationKey = configuration.DefaultReasonTranslationKey;
        m_defaultReasonTranslationValue = configuration.DefaultReasonTranslationValue;
      }

      if ( (0 == configuration.ReasonId) && string.IsNullOrEmpty (m_defaultReasonTranslationKey)) {
        GetLogger ().ErrorFormat ("Initialize: no reason {0} or {1} was set => return false",
          configuration.ReasonId, m_defaultReasonTranslationKey);
        return false;
      }

      if (!configuration.CheckMachineFilter (this.Machine)) {
        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().DebugFormat ("Initialize: machine {0} does not match machine filter {1} => return false",
            this.Machine.Id, configuration.MachineFilterId);
        }
        return false;
      }

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        int reasonId = configuration.ReasonId;
        if (0 == reasonId) {
          Debug.Assert (!string.IsNullOrEmpty (m_defaultReasonTranslationKey)); // See above
          m_reason = ConfigRequests.AddReason (m_defaultReasonTranslationKey,
            m_defaultReasonTranslationValue);
        }
        else { // 0 != reasonId
          m_reason = ModelDAOHelper.DAOFactory.ReasonDAO
            .FindById (reasonId);
        }
        if (null == m_reason) {
          GetLogger ().ErrorFormat ("Initialize: " +
                                    "reason {0} or {1} could not be loaded",
                                    reasonId, m_defaultReasonTranslationKey);
          return false;
        }
        else {
          if (GetLogger ().IsDebugEnabled) {
            GetLogger ().DebugFormat ("Initialize: successfully loaded reason {0}: id {1}", m_defaultReasonTranslationKey, reasonId);
          }
        }

        var dateTimeState = ModelDAOHelper.DAOFactory.AutoReasonStateDAO
          .GetAutoReasonState (this.Machine, GetKey (DATETIME_KEY));
        if (null != dateTimeState) {
          m_dateTime = (DateTime)dateTimeState.Value;
        }
        else {
          var firstRunPeriod = Lemoine.Info.ConfigSet
            .LoadAndGet<TimeSpan> (FIRST_RUN_PERIOD_KEY, FIRST_RUN_PERIOD_DEFAULT);
          m_dateTime = DateTime.UtcNow.Subtract (firstRunPeriod);
          if (GetLogger ().IsInfoEnabled) {
            GetLogger ().InfoFormat ("Initialize: first run, set first date/time to {0}, first run period={1}",
              m_dateTime, firstRunPeriod);
          }
        }

        return InitializeAdditionalConfigurations (configuration);
      } // session
    }

    /// <summary>
    /// Initialize some additional configurations
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    protected virtual bool InitializeAdditionalConfigurations (TConfiguration configuration)
    {
      return true;
    }

    /// <summary>
    /// <see cref="IAutoReasonExtension"/>
    /// </summary>
    /// <param name="reasonSlot"></param>
    /// <returns></returns>
    public virtual bool CanOverride (IReasonSlot reasonSlot)
    {
      // Check the scores
      // Note that by default it may potentially apply on top of a running period
      return reasonSlot.ReasonScore < this.ReasonScore;
    }

    /// <summary>
    /// <see cref="IAutoReasonExtension"/>
    /// </summary>
    /// <param name="reasonSlot"></param>
    /// <returns></returns>
    public abstract bool IsValidExtraAutoReason (IReasonSlot reasonSlot);

    /// <summary>
    /// <see cref="IAutoReasonExtension"/>
    /// </summary>
    /// <param name="machineMode"></param>
    /// <param name="machineObservationState"></param>
    /// <param name="reason"></param>
    /// <param name="score"></param>
    /// <returns></returns>
    public virtual bool IsValidMatch (IMachineMode machineMode, IMachineObservationState machineObservationState, IReason reason, double score)
    {
      Debug.Assert (null != this.Reason);

      if ((null != reason) && (reason.Id != this.Reason.Id)) {
        return false;
      }

      if (score != this.ReasonScore) {
        return false;
      }

      return true;
    }

    /// <summary>
    /// <see cref="IAutoReasonExtension"/>
    /// </summary>
    public virtual void RunOnce ()
    {
      RunAllStepsOnce ();
    }

    /// <summary>
    /// Process all the steps (revision / check / actions) once
    /// </summary>
    protected virtual void RunAllStepsOnce ()
    {
      ClearRevision ();      
      Check ();
      InitializeRevisionIfRequired ();
      this.ProcessPendingActions ();
    }

    /// <summary>
    /// Check the data once
    /// </summary>
    protected abstract void Check ();

    /// <summary>
    /// Reset the delayed actions
    /// </summary>
    public void ResetDelayedActions ()
    {
      m_delayedStateActions.Clear ();
      m_delayedReasonActions.Clear ();
    }

    /// <summary>
    /// Add a delayed state action
    /// </summary>
    /// <param name="action"></param>
    protected void AddDelayedAction (IStateAction action)
    {
      m_delayedStateActions.Add (action);
    }

    /// <summary>
    /// Add a delayed reason action
    /// </summary>
    /// <param name="action"></param>
    protected void AddDelayedAction (IReasonAction action)
    {
      m_delayedReasonActions.Add (action);
    }

    /// <summary>
    /// Add a delayed action to update the date/time
    /// </summary>
    /// <param name="dateTime"></param>
    protected void AddUpdateDateTimeDelayedAction (DateTime dateTime)
    {
      var action = new Action.UpdateDateTimeStateAction (this, dateTime);
      AddDelayedAction (action);
    }

    /// <summary>
    /// Add a delayed action to update the date/time
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="dateTime"></param>
    protected void AddUpdateMachineModuleDateTimeDelayedAction (IMachineModule machineModule, DateTime dateTime)
    {
      Debug.Assert (null != machineModule);

      var action = new Action.UpdateMachineModuleDateTimeStateAction (this, machineModule, dateTime);
      AddDelayedAction (action);
    }

    /// <summary>
    /// Apply the associated reason (and score) to the specified range 
    /// and with the dynamic times
    /// 
    /// It needs to be run in a transaction
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="reason"></param>
    /// <param name="reasonScore"></param>
    /// <param name="range"></param>
    /// <param name="dynamic"></param>
    /// <param name="details"></param>
    /// <param name="overwriteRequired"></param>
    public void ApplyReason (IMachine machine, IReason reason, double reasonScore, UtcDateTimeRange range, string dynamic, string details, bool overwriteRequired)
    {
      if (GetLogger ().IsDebugEnabled) {
        GetLogger ().Debug ($"ApplyReason: reason={reason?.Id} reasonScore={reasonScore} range={range}");
      }

      long modificationId = ModelDAOHelper.DAOFactory.ReasonMachineAssociationDAO.InsertAutoReason (machine, range, reason, reasonScore, details, dynamic, overwriteRequired, null);
      if (null != m_revision) {
        var modification = ModelDAOHelper.DAOFactory.MachineModificationDAO
          .FindById (modificationId, machine);
        Debug.Assert (null != modification);
        m_revision.AddModification (modification);
      }
    }

    /// <summary>
    /// Apply the associated reason (and score) to the specified range 
    /// and with the dynamic times with the aggressive mode and the option
    /// to cancel the modification if the dynamic end is after end
    /// 
    /// It needs to be run in a transaction
    /// </summary>
    /// <param name="machine">alternate machine (not null)</param>
    /// <param name="reason">not null</param>
    /// <param name="reasonScore"></param>
    /// <param name="range"></param>
    /// <param name="dynamic"></param>
    /// <param name="details"></param>
    /// <param name="overwriteRequired"></param>
    public void ApplyReasonDynamicEndBeforeRealEnd (IMachine machine, IReason reason, double reasonScore, UtcDateTimeRange range, string dynamic, string details, bool overwriteRequired)
    {
      var modificationId = ModelDAOHelper.DAOFactory.ReasonMachineAssociationDAO
        .InsertAutoReason (machine, range, reason, reasonScore, details, dynamic, overwriteRequired, AssociationOption.DynamicEndBeforeRealEnd);
      if (null != m_revision) {
        var modification = ModelDAOHelper.DAOFactory.MachineModificationDAO
          .FindById (modificationId, machine);
        Debug.Assert (null != modification);
        m_revision.AddModification (modification);
      }
    }

    /// <summary>
    /// Update the date/time in autoreasonstate
    /// </summary>
    /// <param name="dateTime"></param>
    public void UpdateDateTime (DateTime dateTime)
    {
      m_dateTime = dateTime;
      ModelDAOHelper.DAOFactory.AutoReasonStateDAO
        .Save (m_machine, GetKey (DATETIME_KEY), dateTime);
    }

    /// <summary>
    /// <see cref="IDateTimeStateAutoReason"/>
    /// </summary>
    /// <param name="dateTime"></param>
    public void ResetDateTime (DateTime dateTime)
    {
      m_dateTime = dateTime;
    }

    /// <summary>
    /// Update the date/time in autoreasonstate when it is specific to a machine module
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="dateTime"></param>
    public void UpdateDateTime (IMachineModule machineModule, DateTime dateTime)
    {
      Debug.Assert (null != machineModule);

      m_dateTimeByMachineModule[machineModule.Id] = dateTime;
      ModelDAOHelper.DAOFactory.AutoReasonStateDAO
        .Save (m_machine, GetKey (machineModule, DATETIME_KEY), dateTime);
    }

    /// <summary>
    /// <see cref="IMachineModuleDateTimeStateAutoReason"/>
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="dateTime"></param>
    public void ResetDateTime (IMachineModule machineModule, DateTime dateTime)
    {
      Debug.Assert (null != machineModule);

      m_dateTimeByMachineModule[machineModule.Id] = dateTime;
    }

    /// <summary>
    /// Return the date/time when it is specific to a machine module
    /// </summary>
    /// <param name="machineModule">not null</param>
    public DateTime GetDateTime (IMachineModule machineModule)
    {
      Debug.Assert (null != machineModule);

      if (m_dateTimeByMachineModule.TryGetValue (machineModule.Id, out var dateTime)) {
        return dateTime;
      }
      else {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          var dateTimeState = ModelDAOHelper.DAOFactory.AutoReasonStateDAO
          .GetAutoReasonState (this.Machine, GetKey (machineModule, DATETIME_KEY));
          if (null != dateTimeState) {
            dateTime = (DateTime)dateTimeState.Value;
          }
          else {
            var firstRunPeriod = Lemoine.Info.ConfigSet
              .LoadAndGet<TimeSpan> (FIRST_RUN_PERIOD_KEY, FIRST_RUN_PERIOD_DEFAULT);
            dateTime = DateTime.UtcNow.Subtract (firstRunPeriod);
            GetLogger ().InfoFormat ("GetDateTime: first run, set first date/time to {0} for machine module {2}, first run period={1}",
              m_dateTime, firstRunPeriod, machineModule.Id);
          }
        } // session
        m_dateTimeByMachineModule[machineModule.Id] = dateTime;
        return dateTime;
      }
    }

    /// <summary>
    /// Get a reference to the Auto reason service
    /// </summary>
    /// <returns></returns>
    protected IService GetService ()
    {
      if (null == m_service) {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (var transaction = session.BeginTransaction ("AutoReason.GetService")) {
            var computer = ModelDAOHelper.DAOFactory.ComputerDAO
              .GetOrCreateLocal ();
            if (null == computer) {
              GetLogger ().ErrorFormat ("GetService: no local computer known or detected");
              Debug.Assert (null != computer, "Computer is null");
              transaction.Commit ();
              throw new InvalidProgramException ("computer null");
            }
            var program = Lemoine.Info.ProgramInfo.Name;
            if (null == program) {
              GetLogger ().ErrorFormat ("GetService: unknown program");
              Debug.Assert (null != program, "Program is null");
              transaction.Commit ();
              throw new InvalidProgramException ("program null");
            }
            var services = ModelDAOHelper.DAOFactory.ServiceDAO
              .FindAll ()
              .Where (s => s.Lemoine && (computer.Id == s.Computer.Id) && program.Equals (s.Program));
            if (services.Any ()) {
              if (1 < services.Count ()) {
                GetLogger ().ErrorFormat ("GetService: more than one service matches");
              }
              m_service = services.First ();
            }
            else {
              m_service = ModelDAOHelper.ModelFactory.CreateService (computer, "Lemoine AutoReason", program, true);
              ModelDAOHelper.DAOFactory.ServiceDAO.MakePersistent (m_service);
            }
            transaction.Commit ();
          }
        }
      }
      return m_service;
    }

    /// <summary>
    /// 
    /// </summary>
    protected virtual void InitializeRevisionIfRequired ()
    {
      if (this.DelayedReasonActions.Any ()
        && Lemoine.Info.ConfigSet.LoadAndGet (USE_REVISION_KEY, USE_REVISION_DEFAULT)) {
        InitializeRevision ();
      }
    }

    void InitializeRevision ()
    {
      Debug.Assert (null == m_reason);

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ("AutoReason.InitializeRevision", TransactionLevel.ReadCommitted)) {
          m_revision = CreateRevision ();
          transaction.Commit ();
        }
      }
    }

    /// <summary>
    /// Get a new or current revision
    /// </summary>
    /// <returns></returns>
    protected IRevision GetRevision ()
    {
      if (null == m_revision) {
        m_revision = CreateRevision ();
      }
      return m_revision;
    }

    /// <summary>
    /// Create a new revision
    /// </summary>
    /// <returns></returns>
    IRevision CreateRevision ()
    {
      var revision = ModelDAOHelper.ModelFactory
        .CreateRevision ();
      revision.Updater = GetService ();
      revision.IPAddress = Lemoine.Info.ComputerInfo.GetIPAddresses ()
        .First ();
      revision.Application = Lemoine.Info.ProgramInfo.Name;
      ModelDAOHelper.DAOFactory.RevisionDAO.MakePersistent (revision);
      return revision;
    }

    /// <summary>
    /// Clear the current revision
    /// </summary>
    protected void ClearRevision ()
    {
      m_revision = null;
    }
  }
}
