// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;

namespace Lemoine.Stamping.StampVariablesGetters
{
  /// <summary>
  /// <see cref="IStampVariablesGetter"/> implementation using the configuration in database using:
  /// <item>the stampingconfigbyname object that is associated to the monitored machine</item>
  /// <item>the stampingconfigname property</item>
  /// <item>the machine module properties</item>
  /// 
  /// It is possible to manually force a specific value
  /// </summary>
  public class StampVariablesGetterMachineConfig
    : IStampVariablesGetter
  {
    readonly ILog log = LogManager.GetLogger (typeof (StampVariablesGetterMachineConfig).FullName);

    readonly StampingData m_stampingData;
    readonly StampVariablesGetter? m_manualStampVariablesGetter;
    bool m_initialized = false;
    string m_sequenceStampVariable = "";
    string m_startCycleStampVariable = "";
    string m_stopCycleStampVariable = "";
    string m_milestoneStampVariable = "";

    /// <summary>
    /// Sequence stamp variable
    /// </summary>
    public string SequenceStampVariable
    {
      get {
        if (!string.IsNullOrEmpty (m_manualStampVariablesGetter?.SequenceStampVariable)) {
          return m_manualStampVariablesGetter.SequenceStampVariable;
        }
        Initialize ();
        return m_sequenceStampVariable;
      }
    }

    /// <summary>
    /// Start cycle stamp variable
    /// </summary>
    public string StartCycleStampVariable
    {
      get {
        if (!string.IsNullOrEmpty (m_manualStampVariablesGetter?.StartCycleStampVariable)) {
          return m_manualStampVariablesGetter.StartCycleStampVariable;
        }
        Initialize ();
        return m_startCycleStampVariable;
      }
    }

    /// <summary>
    /// Stop cycle stamp variable
    /// </summary>
    public string StopCycleStampVariable
    {
      get {
        if (!string.IsNullOrEmpty (m_manualStampVariablesGetter?.StopCycleStampVariable)) {
          return m_manualStampVariablesGetter.StopCycleStampVariable;
        }
        Initialize ();
        return m_stopCycleStampVariable;
      }
    }

    /// <summary>
    /// Milestone stamp variable
    /// </summary>
    public string MilestoneStampVariable
    {
      get {
        if (!string.IsNullOrEmpty (m_manualStampVariablesGetter?.MilestoneStampVariable)) {
          return m_manualStampVariablesGetter.MilestoneStampVariable;
        }
        Initialize ();
        return m_milestoneStampVariable;
      }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public StampVariablesGetterMachineConfig (StampingData stampingData, StampVariablesGetter manualStampVariablesGetter)
    {
      m_stampingData = stampingData;
      m_manualStampVariablesGetter = manualStampVariablesGetter;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public StampVariablesGetterMachineConfig (StampingData stampingData)
    {
      m_stampingData = stampingData;
    }

    void Initialize ()
    {
      if (!m_initialized) {
        var stampingConfigName = m_stampingData.StampingConfigName;
        if (!string.IsNullOrEmpty (stampingConfigName)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"Initialize: stamping config name={stampingConfigName}");
          }
          using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
            var stampingConfigByName = ModelDAOHelper.DAOFactory.StampingConfigByNameDAO
              .FindByName (stampingConfigName);
            if (stampingConfigByName is null) {
              log.Error ($"Initialize: there is no stamping config with name {stampingConfigByName}");
              throw new Exception ($"Unknown stamping config {stampingConfigByName}");
            }
            else { // not null
              var monitoredMachines = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
                .FindByStampingConfig (stampingConfigByName);
              var machineModules = monitoredMachines.SelectMany (x => x.MachineModules);

              var sequenceStampVariables = machineModules
                .Where (x => !string.IsNullOrEmpty (x.SequenceVariable))
                .Select (x => x.SequenceVariable)
                .Distinct ();
              if (!sequenceStampVariables.Any ()) {
                log.Info ($"Initialize: no sequence variable was set for stamping config name {stampingConfigByName}");
              }
              else if (1 < sequenceStampVariables.Count ()) {
                log.Warn ($"Initialize: more than one sequence variable was set for stamping config name {stampingConfigByName} => skip it");
              }
              else {
                m_sequenceStampVariable = sequenceStampVariables.Single ();
              }

              var startCycleStampVariables = machineModules
                .Where (x => !string.IsNullOrEmpty (x.StartCycleVariable))
                .Select (x => x.StartCycleVariable)
                .Distinct ();
              if (!startCycleStampVariables.Any ()) {
                log.Info ($"Initialize: no start cycle variable was set for stamping config name {stampingConfigByName}");
              }
              else if (1 < startCycleStampVariables.Count ()) {
                log.Warn ($"Initialize: more than one start cycle variable was set for stamping config name {stampingConfigByName} => skip it");
              }
              else {
                m_startCycleStampVariable = startCycleStampVariables.Single ();
              }

              var stopCycleStampVariables = machineModules
                .Where (x => !string.IsNullOrEmpty (x.CycleVariable))
                .Select (x => x.CycleVariable)
                .Distinct ();
              if (!stopCycleStampVariables.Any ()) {
                log.Info ($"Initialize: no stop cycle variable was set for stamping config name {stampingConfigByName}");
              }
              else if (1 < stopCycleStampVariables.Count ()) {
                log.Warn ($"Initialize: more than one stop cycle variable was set for stamping config name {stampingConfigByName} => skip it");
              }
              else {
                m_stopCycleStampVariable = stopCycleStampVariables.Single ();
              }

              var milestoneStampVariables = machineModules
                .Where (x => !string.IsNullOrEmpty (x.MilestoneVariable))
                .Select (x => x.MilestoneVariable)
                .Distinct ();
              if (!milestoneStampVariables.Any ()) {
                log.Info ($"Initialize: no milestone variable was set for stamping config name {stampingConfigByName}");
              }
              else if (1 < milestoneStampVariables.Count ()) {
                log.Warn ($"Initialize: more than one milestone variable was set for stamping config name {stampingConfigByName} => skip it");
              }
              else {
                m_milestoneStampVariable = milestoneStampVariables.Single ();
              }
            }
          }
        }
        m_initialized = true;
      }
    }
  }
}
