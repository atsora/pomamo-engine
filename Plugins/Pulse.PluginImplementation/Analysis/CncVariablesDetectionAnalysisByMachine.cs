// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Analysis;
using System;
using System.Linq;
using Lemoine.Extensions.Analysis.Detection;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System.Diagnostics;
using Pulse.Extensions.Database;
using Pulse.PluginImplementation.CycleDetectionStatus;

namespace Pulse.PluginImplementation.Analysis
{
  /// <summary>
  /// Base class to process new values of the start and end cycle variables
  /// </summary>
  /// <typeparam name="TConfiguration"></typeparam>
  public abstract class CncVariablesDetectionAnalysisByMachine<TConfiguration>
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<TConfiguration>
    , ICycleDetectionStatusExtension
    , IDetectionAnalysisExtension // TODO: To replace one day by the next line
                                  //    , IDetectionAnalysisByMachineExtension
    where TConfiguration : Pulse.Extensions.Configuration.IConfigurationWithMachineFilter, ICycleDetectionStatusConfiguration, new()
  {
    ILog log = LogManager.GetLogger (typeof (CncVariablesDetectionAnalysisByMachine<TConfiguration>).FullName);

    IMonitoredMachine m_monitoredMachine;
    IOperationDetection m_operationDetection;
    IOperationCycleDetection m_operationCycleDetection;

    ICycleDetectionStatusExtension m_cycleDetectionStatus;
    TConfiguration m_configuration;

    /// <summary>
    /// Associated cycle detection status extension.
    /// </summary>
    protected ICycleDetectionStatusExtension CycleDetectionStatus
    {
      get { return m_cycleDetectionStatus; }
    }

    /// <summary>
    /// Associated configuration
    /// </summary>
    protected TConfiguration Configuration
    {
      get { return m_configuration; }
    }

    /// <summary>
    /// Associated machine
    /// </summary>
    protected IMonitoredMachine Machine
    {
      get { return m_monitoredMachine; }
    }

    /// <summary>
    /// Start cycle variable key
    /// </summary>
    protected virtual string StartCycleVariableKey
    {
      get; set;
    }

    /// <summary>
    /// End cycle variable key
    /// </summary>
    protected virtual string EndCycleVariableKey
    {
      get; set;
    }

    /// <summary>
    /// Associated operation detection
    /// </summary>
    protected IOperationDetection OperationDetection
    {
      get { return m_operationDetection; }
    }

    /// <summary>
    /// Associated operation cycle detection
    /// </summary>
    protected IOperationCycleDetection OperationCycleDetection
    {
      get { return m_operationCycleDetection; }
    }

    /// <summary>
    /// Restricted transaction level
    /// </summary>
    public TransactionLevel RestrictedTransactionLevel
    {
      get; set;
    }

    /// <summary>
    /// Initialize implementation
    /// </summary>
    /// <param name="monitoredMachine"></param>
    /// <param name="operationDetection"></param>
    /// <param name="operationCycleDetection"></param>
    /// <returns></returns>
    public virtual bool Initialize (IMonitoredMachine monitoredMachine, IOperationDetection operationDetection, IOperationCycleDetection operationCycleDetection)
    {
      Debug.Assert (null == m_monitoredMachine);

      m_monitoredMachine = monitoredMachine;
      m_operationDetection = operationDetection;
      m_operationCycleDetection = operationCycleDetection;

      if (!Initialize (m_monitoredMachine)) {
        if (log.IsDebugEnabled) {
          log.Debug ("Initialize: initialize with IMachine failed => return false (skip)");
        }
        return false;
      }

      InitializeCycleVariableKeys ();
      return true;
    }

    /// <summary>
    /// Initialize the cycle variable keys
    /// </summary>
    protected virtual void InitializeCycleVariableKeys ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("CncVariablesDetectionAnalysis.InitializeCycleVariableKeys")) {
          var mainMachineModule = this.Machine.MainMachineModule;
          StartCycleVariableKey = mainMachineModule.StartCycleVariable;
          EndCycleVariableKey = mainMachineModule.CycleVariable;
        }
      }
    }

    /// <summary>
    /// ProcessDetection implementation
    /// </summary>
    /// <param name="detection"></param>
    public virtual void ProcessDetection (IMachineModuleDetection detection)
    {
      // Note: if no plugin returns true for IDetectionAnalysisExtension.UseUniqueSerializableTransaction,
      // then this is not run in a transaction
      // because we suppose this is ok to process a machine module detection twice
      Debug.Assert (null != detection);

      if ((null != detection.CustomType)
        && (detection.CustomType.Equals ("CncVariables", StringComparison.InvariantCultureIgnoreCase))) {
        var cncVariableKeys = Lemoine.Collections.EnumerableString.ParseListString<string> (detection.CustomValue);
        if (!cncVariableKeys.Contains (StartCycleVariableKey) && !cncVariableKeys.Contains (EndCycleVariableKey)) {
          return; // Nothing to do
        }
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          // Process first End cycle
          if (cncVariableKeys.Contains (EndCycleVariableKey)) {
            var newCncVariable = ModelDAOHelper.DAOFactory.CncVariableDAO
              .FindAt (detection.MachineModule, EndCycleVariableKey, detection.DateTime);
            if (newCncVariable is null) {
              log.Error ($"ProcessDetection: no cnc variable {EndCycleVariableKey} at {detection.DateTime}");
            }
            else if (TriggerEndCycleVariableAction (newCncVariable.Value)) {
              var oldCncVariable = ModelDAOHelper.DAOFactory.CncVariableDAO
                .FindAt (detection.MachineModule, EndCycleVariableKey, detection.DateTime.AddSeconds (-1));
              if ((null != oldCncVariable) && object.Equals (newCncVariable.Value, oldCncVariable.Value)) {
                log.Debug ("ProcessDetection: no cnc variable change");
              }
              else {
                try {
                  if (null == oldCncVariable) {
                    RunEndCycleVariableAction (newCncVariable.Value, detection.DateTime);
                  }
                  else { // null != oldCncVariable
                    RunEndCycleVariableAction (oldCncVariable.Value, newCncVariable.Value, detection.DateTime);
                  }
                }
                catch (Exception ex) {
                  log.Error ("ProcessDetection: exception in RunEndCycleVariableAction", ex);
                  throw;
                }
              }
            }
          }
          // Then Start cycle
          if (cncVariableKeys.Contains (StartCycleVariableKey)) {
            var newCncVariable = ModelDAOHelper.DAOFactory.CncVariableDAO
              .FindAt (detection.MachineModule, StartCycleVariableKey, detection.DateTime);
            if (newCncVariable is null) {
              log.Error ($"ProcessDetection: no cnc variable {StartCycleVariableKey} at {detection.DateTime}");
            }
            else if (TriggerStartCycleVariableAction (newCncVariable.Value)) {
              var oldCncVariable = ModelDAOHelper.DAOFactory.CncVariableDAO
                .FindAt (detection.MachineModule, StartCycleVariableKey, detection.DateTime.AddSeconds (-1));
              if ((null != oldCncVariable) && object.Equals (newCncVariable.Value, oldCncVariable.Value)) {
                log.Debug ("ProcessDetection: no cnc variable change");
              }
              else {
                try {
                  RunStartCycleVariableAction (newCncVariable.Value, detection.DateTime);
                }
                catch (Exception ex) {
                  log.Error ("ProcessDetection: exception in RunStartCycleVariableAction", ex);
                  throw;
                }
              }
            }
          }
        }
      }
    }

    /// <summary>
    /// To override: condition to trigger the action considering the end cycle variable
    /// </summary>
    /// <param name="newEndCycleVariableValue"></param>
    /// <returns></returns>
    protected abstract bool TriggerEndCycleVariableAction (object newEndCycleVariableValue);

    /// <summary>
    /// Action of the end cycle variable
    /// 
    /// This is called when the old end cycle variable is known
    /// </summary>
    /// <param name="oldEndCycleVariableValue"></param>
    /// <param name="newEndCycleVariableValue"></param>
    /// <param name="dateTime"></param>
    protected virtual void RunEndCycleVariableAction (object oldEndCycleVariableValue, object newEndCycleVariableValue, DateTime dateTime)
    {
      RunEndCycleVariableAction (newEndCycleVariableValue, dateTime);
    }

    /// <summary>
    /// Action of the end cycle variable
    /// 
    /// This is called when the old end cycle variable is not known
    /// </summary>
    /// <param name="newEndCycleVariableValue"></param>
    /// <param name="dateTime"></param>
    protected virtual void RunEndCycleVariableAction (object newEndCycleVariableValue, DateTime dateTime)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ("Detection.RunEndCycleVariableAction.StopCycle", this.RestrictedTransactionLevel)) {
          transaction.SynchronousCommitOption = SynchronousCommit.Off;
          this.OperationCycleDetection.StopCycle (null, dateTime);
          transaction.Commit ();
        }
      }
    }

    /// <summary>
    /// To override: condition to trigger the action considering the start cycle variable
    /// </summary>
    /// <param name="newStartCycleVariableValue"></param>
    /// <returns></returns>
    protected abstract bool TriggerStartCycleVariableAction (object newStartCycleVariableValue);

    /// <summary>
    /// Action of the start cycle variable
    /// </summary>
    /// <param name="newStartCncVariableValue"></param>
    /// <param name="dateTime"></param>
    protected virtual void RunStartCycleVariableAction (object newStartCncVariableValue, DateTime dateTime)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ("Detection.RunStartCycleVariableAction.StartCycle", this.RestrictedTransactionLevel)) {
          transaction.SynchronousCommitOption = SynchronousCommit.Off;
          this.OperationCycleDetection.StartCycle (dateTime);
          transaction.Commit ();
        }
      }
    }

    /// <summary>
    /// Use a unique serializable transaction here
    /// </summary>
    /// <param name="detection"></param>
    /// <returns></returns>
    public virtual bool UseUniqueSerializableTransaction (IMachineModuleDetection detection) => false;

    #region ICycleDetectionStatusExtension
    /// <summary>
    /// Initialize for ICycleDetectionStatusExtension
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    public virtual bool Initialize (IMachine machine)
    {
      Debug.Assert (null != machine);

      log = LogManager.GetLogger ($"{this.GetType ().FullName}.{machine.Id}");

      TConfiguration configuration;
      if (!base.LoadConfiguration (out configuration)) {
        log.Error ("Initialize: bad configuration, skip this instance");
        return false;
      }

      if (!CheckMachineFilter (configuration, machine)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"Initialize: machine {machine.Id} does not match machine filter => return false (skip this extension)");
        }
        return false;
      }

      if (!InitializeProperties (machine, configuration)) {
        if (log.IsErrorEnabled) {
          log.Error ("Initialize: InitializeProperties returned false");
        }
        return false;
      }
      // Note: Initialize of CycleDetectionStatus is done in InitializeProperties

      return true;
    }

    /// <summary>
    /// Initialize some mandatory properties, including the CycleDetectionStatus.
    /// 
    /// It must be called at the end of <see cref="Initialize(IMachine)"/>
    /// in case base.Initialize is not called
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    protected bool InitializeProperties (IMachine machine, TConfiguration configuration)
    {
      m_configuration = configuration;

      m_cycleDetectionStatus = new CycleDetectionStatusFromAnalysisStatus (configuration.CycleDetectionStatusPriority);
      if (!m_cycleDetectionStatus.Initialize (machine)) {
        if (log.IsErrorEnabled) {
          log.Error ("InitializeProperties: initialization of cycle detection status failed");
        }
        return false;
      }

      return true;
    }

    /// <summary>
    /// ICycleDetectionStatusExtension
    /// </summary>
    public int CycleDetectionStatusPriority => this.CycleDetectionStatus.CycleDetectionStatusPriority;

    /// <summary>
    /// ICycleDetectionStatusExtension
    /// </summary>
    /// <returns></returns>
    public virtual DateTime? GetCycleDetectionDateTime ()
    {
      return this.CycleDetectionStatus.GetCycleDetectionDateTime ();
    }
    #endregion // ICycleDetectionStatusExtension

    /// <summary>
    /// Check a machine matches the machine filter in configuration
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="machine"></param>
    /// <returns></returns>
    protected virtual bool CheckMachineFilter (TConfiguration configuration, IMachine machine)
    {
      IMachineFilter machineFilter = null;
      if (0 == configuration.MachineFilterId) { // Machine filter
        return true;
      }
      else { // Machine filter
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (var transaction = session.BeginReadOnlyTransaction ("CncVariablesDetectionAnalysis.InitializeConfiguration.MachineFilter")) {
            int machineFilterId = configuration.MachineFilterId;
            machineFilter = ModelDAOHelper.DAOFactory.MachineFilterDAO
              .FindById (machineFilterId);
            if (machineFilter is null) {
              log.Error ($"Initialize: machine filter id {machineFilterId} does not exist");
              return false;
            }
            else { // machineFilter is not null
              if (!machineFilter.IsMatch (machine)) {
                return false;
              }
            }
          }
        }
        return true;
      }
    }
  }
}
