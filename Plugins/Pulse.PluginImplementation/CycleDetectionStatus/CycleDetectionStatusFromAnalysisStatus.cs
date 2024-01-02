// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2023 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Pulse.Extensions.Database;

namespace Pulse.PluginImplementation.CycleDetectionStatus
{
  /// <summary>
  /// Get the cycle detection status from the machinemoduleanalysisstatus
  /// and machinemoduledetection tables
  /// </summary>
  public sealed class CycleDetectionStatusFromAnalysisStatus
    : ICycleDetectionStatusExtension
  {
    ILog log = LogManager.GetLogger (typeof (CycleDetectionStatusFromAnalysisStatus).FullName);

    IMachine m_machine;
    int m_priority;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="priority"></param>
    public CycleDetectionStatusFromAnalysisStatus (int priority)
    {
      m_priority = priority;
    }

    /// <summary>
    /// ICycleDetectionStatusExtension implementation
    /// </summary>
    public int CycleDetectionStatusPriority
    {
      get { return m_priority; }
    }

    /// <summary>
    /// IExtension implementation
    /// </summary>
    public bool UniqueInstance
    {
      get
      {
        return true;
      }
    }

    /// <summary>
    /// ICycleDetectionStatusExtension implementation
    /// </summary>
    /// <returns></returns>
    public DateTime? GetCycleDetectionDateTime ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("CycleDetectionStatusFromAnalysisStatus.GetCycleDetectionDateTime")) {
          IMonitoredMachine monitoredMachine;
          if (m_machine is IMonitoredMachine) {
            monitoredMachine = (IMonitoredMachine)m_machine;
          }
          else {
            monitoredMachine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
              .FindById (m_machine.Id);
            if (null == monitoredMachine) {
              log.ErrorFormat ("GetCycleDetectionDateTime: not a monitored machine");
              return null;
            }
          }
          var machineModule = monitoredMachine.MainMachineModule;
          if (null == machineModule) {
            log.ErrorFormat ("GetCycleDetectionDateTime: no main machine module");
            return null;
          }
          var analysisStatus = ModelDAOHelper.DAOFactory.MachineModuleAnalysisStatusDAO
            .FindById (machineModule.Id);
          if (null == analysisStatus) {
            log.Error ($"GetCycleDetectionDateTime: no analysis status for machine module id {machineModule.Id}");
            return null;
          }
          if (0 == analysisStatus.LastMachineModuleDetectionId) {
            if (log.IsWarnEnabled) {
              log.Warn ($"GetCycleDetectionDateTime: last machine module detection id is 0 for machine {m_machine.Id} => return null");
            }
            return null;
          }
          var lastMachineModuleDetection = ModelDAOHelper.DAOFactory
            .MachineModuleDetectionDAO
            .FindById (analysisStatus.LastMachineModuleDetectionId, machineModule);
          if (null == lastMachineModuleDetection) {
            log.Error ($"GetCycleDetectionDateTime: no machine module detection id {analysisStatus.LastMachineModuleDetectionId}");
            return null;
          }
          return lastMachineModuleDetection.DateTime;
        }
      }
    }

    /// <summary>
    /// ICycleDetectionStatusExtension implementation
    /// </summary>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    public bool Initialize (IMachine machine)
    {
      Debug.Assert (null != machine);

      m_machine = machine;

      ILog log = LogManager.GetLogger (typeof (CycleDetectionStatusFromAnalysisStatus).FullName + "." + machine.Id);

      return true;
    }
  }
}
