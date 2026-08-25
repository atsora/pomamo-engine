// Copyright (C) 2026 Atsora Solutions

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.AutoReason.ActionableAutoReasonExtension;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Extensions.AutoReason
{
  /// <summary>
  /// Base class for an auto machine state template extension
  ///
  /// It follows the same principles as <see cref="AutoReasonExtensionBase{TConfiguration}"/>:
  /// a date/time state is stored in autoreasonstate, the detection is done in <see cref="Check"/>
  /// and the modifications are applied afterwards with some delayed actions
  /// </summary>
  public abstract class AutoStateTemplateExtensionBase<TConfiguration>
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<TConfiguration>
    , IAutoStateTemplateExtension
    , IActionableAutoStateTemplate
    , IDateTimeStateAutoExtension
    , IApplyMachineStateTemplateAutoExtension
    where TConfiguration : AutoStateTemplateConfiguration, new ()
  {
    static readonly string FIRST_RUN_PERIOD_KEY = "AutoStateTemplate.FirstRunPeriod";
    static readonly TimeSpan FIRST_RUN_PERIOD_DEFAULT = TimeSpan.FromDays (3);

    static readonly string USE_REVISION_KEY = "AutoStateTemplate.Revision";
    static readonly bool USE_REVISION_DEFAULT = false;

    static readonly string DATETIME_KEY = "DateTime";

    Lemoine.Threading.IChecked m_caller = null;
    IMonitoredMachine m_machine;
    IMachineStateTemplate m_machineStateTemplate;
    IMachineStateTemplate m_nextMachineStateTemplate;

    readonly string m_pluginKey;
    readonly IList<IAutoReasonAction> m_delayedActions = new List<IAutoReasonAction> ();

    DateTime m_dateTime = DateTime.UtcNow;
    IRevision m_revision = null;
    IService m_service = null;

    /// <summary>
    /// <see cref="IActionableAutoExtension"/>
    /// </summary>
    public IEnumerable<IAutoReasonAction> DelayedActions => m_delayedActions;

    /// <summary>
    /// <see cref="IActionableAutoExtension"/>
    /// </summary>
    /// <returns></returns>
    public abstract ILog GetLogger ();

    /// <summary>
    /// Plugin key that corresponds to a prefix in autoreasonstate
    /// </summary>
    public virtual string PluginKey => m_pluginKey;

    /// <summary>
    /// <see cref="IAutoStateTemplateExtension"/>
    /// </summary>
    public IMonitoredMachine Machine => m_machine;

    /// <summary>
    /// <see cref="IAutoStateTemplateExtension"/>
    /// </summary>
    public IMachineStateTemplate MachineStateTemplate => m_machineStateTemplate;

    /// <summary>
    /// <see cref="IAutoStateTemplateExtension"/>
    /// </summary>
    public IMachineStateTemplate NextMachineStateTemplate => m_nextMachineStateTemplate;

    /// <summary>
    /// <see cref="IDateTimeStateAutoExtension"/>
    /// </summary>
    public DateTime DateTime => m_dateTime;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="pluginKey">prefix of the keys in autoreasonstate</param>
    protected AutoStateTemplateExtensionBase (string pluginKey)
    {
      m_pluginKey = pluginKey;
    }

    /// <summary>
    /// <see cref="Lemoine.Threading.IChecked"/>
    /// </summary>
    public void SetActive ()
    {
      m_caller?.SetActive ();
    }

    /// <summary>
    /// <see cref="Lemoine.Threading.IChecked"/>
    /// </summary>
    public void PauseCheck ()
    {
      m_caller?.PauseCheck ();
    }

    /// <summary>
    /// <see cref="Lemoine.Threading.IChecked"/>
    /// </summary>
    public void ResumeCheck ()
    {
      m_caller?.ResumeCheck ();
    }

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
    /// <see cref="IAutoStateTemplateExtension"/>
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
      if (!configuration.CheckMachineFilter (this.Machine)) {
        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().Debug ($"Initialize: machine {this.Machine.Id} does not match machine filter {configuration.MachineFilterId} => return false");
        }
        return false;
      }

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        m_machineStateTemplate = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
          .FindById (configuration.MachineStateTemplateId);
        if (null == m_machineStateTemplate) {
          GetLogger ().Error ($"Initialize: machine state template {configuration.MachineStateTemplateId} could not be loaded");
          return false;
        }

        if (0 != configuration.NextMachineStateTemplateId) {
          m_nextMachineStateTemplate = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
            .FindById (configuration.NextMachineStateTemplateId);
          if (null == m_nextMachineStateTemplate) {
            GetLogger ().Error ($"Initialize: next machine state template {configuration.NextMachineStateTemplateId} could not be loaded");
            return false;
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
            GetLogger ().Info ($"Initialize: first run, set first date/time to {m_dateTime}, first run period={firstRunPeriod}");
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
    protected virtual bool InitializeAdditionalConfigurations (TConfiguration configuration) => true;

    /// <summary>
    /// <see cref="IAutoStateTemplateExtension"/>
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
    /// <see cref="IActionableAutoExtension"/>
    /// </summary>
    public void ResetDelayedActions ()
    {
      m_delayedActions.Clear ();
    }

    /// <summary>
    /// Add a delayed action
    /// </summary>
    /// <param name="action"></param>
    protected void AddDelayedAction (IAutoReasonAction action)
    {
      m_delayedActions.Add (action);
    }

    /// <summary>
    /// Add a delayed action to update the date/time
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="commitNumber"></param>
    protected void AddUpdateDateTimeDelayedAction (DateTime dateTime, int commitNumber = 0)
    {
      var action = new Action.UpdateDateTimeStateAction (this, dateTime, commitNumber: commitNumber);
      AddDelayedAction (action);
    }

    /// <summary>
    /// Add a delayed action to apply the associated machine state template
    /// </summary>
    /// <param name="range"></param>
    /// <param name="dynamic">dynamic times (start/end) description: start,end</param>
    /// <param name="option"></param>
    /// <param name="commitNumber"></param>
    protected void AddApplyMachineStateTemplateDelayedAction (UtcDateTimeRange range, string dynamic = "", AssociationOption? option = null, int commitNumber = 0)
    {
      var action = new Action.ApplyMachineStateTemplateAction (this, range, dynamic, option, commitNumber: commitNumber);
      AddDelayedAction (action);
    }

    /// <summary>
    /// <see cref="IApplyMachineStateTemplateAutoExtension"/>
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="machineStateTemplate">not null</param>
    /// <param name="nextMachineStateTemplate">nullable</param>
    /// <param name="range"></param>
    /// <param name="dynamic"></param>
    /// <param name="option"></param>
    public void ApplyMachineStateTemplate (IMachine machine, IMachineStateTemplate machineStateTemplate, IMachineStateTemplate nextMachineStateTemplate, UtcDateTimeRange range, string dynamic, AssociationOption? option)
    {
      if (GetLogger ().IsDebugEnabled) {
        GetLogger ().Debug ($"ApplyMachineStateTemplate: machineStateTemplate={machineStateTemplate?.Id} range={range} dynamic={dynamic}");
      }

      var modificationId = ModelDAOHelper.DAOFactory.MachineStateTemplateAssociationDAO
        .Insert (machine, range, machineStateTemplate, nextMachineStateTemplate, dynamic, option);
      if (null != m_revision) {
        var modification = ModelDAOHelper.DAOFactory.MachineModificationDAO
          .FindById (modificationId, machine);
        Debug.Assert (null != modification);
        m_revision.AddModification (modification);
      }
    }

    /// <summary>
    /// <see cref="IDateTimeStateAutoExtension"/>
    /// </summary>
    /// <param name="dateTime"></param>
    public void UpdateDateTime (DateTime dateTime)
    {
      m_dateTime = dateTime;
      ModelDAOHelper.DAOFactory.AutoReasonStateDAO
        .Save (m_machine, GetKey (DATETIME_KEY), dateTime);
    }

    /// <summary>
    /// <see cref="IDateTimeStateAutoExtension"/>
    /// </summary>
    /// <param name="dateTime"></param>
    public void ResetDateTime (DateTime dateTime)
    {
      m_dateTime = dateTime;
    }

    /// <summary>
    /// Get a reference to the service that runs this extension
    /// </summary>
    /// <returns></returns>
    protected IService GetService ()
    {
      if (null == m_service) {
        m_service = ServiceRequests.GetService (GetLogger ());
      }
      return m_service;
    }

    /// <summary>
    /// Initialize a revision if some machine state template actions are pending
    /// and if the revisions are enabled
    /// </summary>
    protected virtual void InitializeRevisionIfRequired ()
    {
      if (this.DelayedActions.Any (x => x is IMachineStateTemplateAction)
        && Lemoine.Info.ConfigSet.LoadAndGet (USE_REVISION_KEY, USE_REVISION_DEFAULT)) {
        InitializeRevision ();
      }
    }

    void InitializeRevision ()
    {
      Debug.Assert (null == m_revision);

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ("AutoStateTemplate.InitializeRevision", TransactionLevel.ReadCommitted)) {
          m_revision = ServiceRequests.CreateRevision (GetService ());
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
        m_revision = ServiceRequests.CreateRevision (GetService ());
      }
      return m_revision;
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
