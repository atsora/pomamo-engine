// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Database;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Pulse.Extensions.Database;

namespace Lemoine.Plugin.RunningMachineModeIsProduction
{
  public class ProductionStateExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IProductionStateExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (ProductionStateExtension).FullName);

    Configuration m_configuration;

    IMonitoredMachine m_machine;
    double m_score;

    IProductionState m_running;
    string m_defaultRunningProductionStateTranslationKey;
    string m_defaultRunningProductionStateTranslationValue;
    string m_defaultRunningProductionStateColor;

    IProductionState m_notRunning;
    string m_defaultNotRunningProductionStateTranslationKey;
    string m_defaultNotRunningProductionStateTranslationValue;
    string m_defaultNotRunningProductionStateColor;

    public double Score => m_score;

    public bool IsRequired (IReasonSlot oldReasonSlot, IReasonSlot newReasonSlot, IModification modification, ReasonSlotChange reasonSlotChange)
    {
      return reasonSlotChange.HasFlag (ReasonSlotChange.Requested) || reasonSlotChange.HasFlag (ReasonSlotChange.NewActivity) || reasonSlotChange.HasFlag (ReasonSlotChange.MachineMode);
    }

    public bool ConsolidateProductionStateRate (IReasonSlot oldReasonSlot, IReasonSlot newReasonSlot, IModification modification, ReasonSlotChange reasonSlotChange)
    {
      // Note: do not run MakePersistent on newReasonSlot if you are not sure it is already persistent

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var machineMode = newReasonSlot.MachineMode;
        if (!ModelDAOHelper.DAOFactory.IsInitialized (machineMode)) {
          machineMode = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById (machineMode.Id);
          if (machineMode is null) {
            log.Fatal ($"ConsolidateProductionStateRate: machine mode with id {machineMode.Id} does not exist => skip  !");
            return false;
          }
        }
        Debug.Assert (null != machineMode);
        if (machineMode.Running.HasValue) {
          if (machineMode.Running.Value) {
            if (m_configuration.ExcludeManual && machineMode.Manual.HasValue && machineMode.Manual.Value) {
              newReasonSlot.ProductionState = m_notRunning;
              newReasonSlot.ProductionRate = 0.0;
              return true;
            }
            else {
              newReasonSlot.ProductionState = m_running;
              newReasonSlot.ProductionRate = 1.0;
              return true;
            }
          }
          else { // !Running.Value 
            newReasonSlot.ProductionState = m_notRunning;
            newReasonSlot.ProductionRate = 0.0;
            return true;
          }
        }
        else { // !Running.HasValue
          if (m_configuration.UnknownIsNotRunning) {
            newReasonSlot.ProductionState = m_notRunning;
            newReasonSlot.ProductionRate = 0.0;
            return true;
          }
        }
      }

      return false;
    }

    public bool Initialize (IMonitoredMachine machine)
    {
      Debug.Assert (null != machine);

      m_machine = machine;

      if (!LoadConfiguration (out m_configuration)) {
        log.Warn ($"Initialize: the configuration is not valid");
        return false;
      }

      return Initialize (m_configuration);
    }

    /// <summary>
    /// Initialize (configuration part)
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    protected virtual bool Initialize (Configuration configuration)
    {
      m_score = configuration.Score;

      if (!string.IsNullOrEmpty (configuration.DefaultRunningProductionStateTranslationKey)) {
        Debug.Assert (!string.IsNullOrEmpty (configuration.DefaultRunningProductionStateTranslationValue));
        m_defaultRunningProductionStateTranslationKey = configuration.DefaultRunningProductionStateTranslationKey;
        m_defaultRunningProductionStateTranslationValue = configuration.DefaultRunningProductionStateTranslationValue;
        m_defaultRunningProductionStateColor = configuration.DefaultRunningProductionStateColor;
      }

      if (!string.IsNullOrEmpty (configuration.DefaultNotRunningProductionStateTranslationKey)) {
        Debug.Assert (!string.IsNullOrEmpty (configuration.DefaultNotRunningProductionStateTranslationValue));
        m_defaultNotRunningProductionStateTranslationKey = configuration.DefaultNotRunningProductionStateTranslationKey;
        m_defaultNotRunningProductionStateTranslationValue = configuration.DefaultNotRunningProductionStateTranslationValue;
        m_defaultNotRunningProductionStateColor = configuration.DefaultNotRunningProductionStateColor;
      }

      if ((0 == configuration.RunningProductionStateId) && string.IsNullOrEmpty (m_defaultRunningProductionStateTranslationKey)) {
        log.Error ($"Initialize: no running production state {configuration.RunningProductionStateId} or {m_defaultRunningProductionStateTranslationKey} was set => return false");
        return false;
      }

      if ((0 == configuration.NotRunningProductionStateId) && string.IsNullOrEmpty (m_defaultNotRunningProductionStateTranslationKey)) {
        log.Error ($"Initialize: no not running production state {configuration.NotRunningProductionStateId} or {m_defaultNotRunningProductionStateTranslationKey} was set => return false");
        return false;
      }

      if (!configuration.CheckMachineFilter (m_machine)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"Initialize: machine {m_machine} does not match machine filter {configuration.MachineFilterId} => return false");
        }
        return false;
      }

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        int runningId = configuration.RunningProductionStateId;
        if (0 == runningId) {
          Debug.Assert (!string.IsNullOrEmpty (m_defaultRunningProductionStateTranslationKey)); // See above
          m_running = ConfigRequests.AddProductionState (m_defaultRunningProductionStateTranslationKey,
            m_defaultRunningProductionStateTranslationValue, m_defaultRunningProductionStateColor);
        }
        else { // 0 != runningId
          m_running = ModelDAOHelper.DAOFactory.ProductionStateDAO
            .FindById (runningId);
        }
        if (m_running is null) {
          log.Error ($"Initialize: running production state {runningId} or {m_defaultRunningProductionStateTranslationKey} could not be loaded");
          return false;
        }
        else {
          if (log.IsDebugEnabled) {
            log.Debug ($"Initialize: successfully loaded production state {m_defaultRunningProductionStateTranslationKey}: id {m_running.Id}");
          }
        }

        int notRunningId = configuration.NotRunningProductionStateId;
        if (0 == notRunningId) {
          Debug.Assert (!string.IsNullOrEmpty (m_defaultNotRunningProductionStateTranslationKey)); // See above
          m_notRunning = ConfigRequests.AddProductionState (m_defaultNotRunningProductionStateTranslationKey,
            m_defaultNotRunningProductionStateTranslationValue, m_defaultNotRunningProductionStateColor);
        }
        else { // 0 != runningId
          m_notRunning = ModelDAOHelper.DAOFactory.ProductionStateDAO
            .FindById (notRunningId);
        }
        if (m_notRunning is null) {
          log.Error ($"Initialize: not running production state {notRunningId} or {m_defaultNotRunningProductionStateTranslationKey} could not be loaded");
          return false;
        }
        else {
          if (log.IsDebugEnabled) {
            log.Debug ($"Initialize: successfully loaded production state {m_defaultNotRunningProductionStateTranslationKey}: id {m_notRunning.Id}");
          }
        }
      } // session

      return true;
    }


  }
}
