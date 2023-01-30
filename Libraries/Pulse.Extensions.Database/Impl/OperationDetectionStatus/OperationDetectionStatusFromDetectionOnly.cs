// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Pulse.Extensions.Database.Impl.OperationDetectionStatus
{
  /// <summary>
  /// Get the operation detection status from the machinemoduleanalysisstatus
  /// and the machinemoduledetection tables without considering the auto-sequences
  /// </summary>
  public sealed class OperationDetectionStatusFromDetectionOnly
    : IOperationDetectionStatusExtension
  {
    ILog log = LogManager.GetLogger (typeof (OperationDetectionStatusFromDetectionOnly).FullName);

    IMachine m_machine;
    IMonitoredMachine m_monitoredMachine;
    int m_priority;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="priority"></param>
    public OperationDetectionStatusFromDetectionOnly (int priority)
    {
      m_priority = priority;
    }

    /// <summary>
    /// IOperationDetectionStatusExtension implementation
    /// </summary>
    public int OperationDetectionStatusPriority
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
    /// IOperationDetectionStatusExtension implementation
    /// </summary>
    /// <returns></returns>
    public DateTime? GetOperationDetectionDateTime ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("OperationDetectionStatusFromDetectionOnly.GetOperationDetectionDateTime")) {
          var dateTimes = m_monitoredMachine.MachineModules
            .Select (m => GetOperationDetectionDateTime (m))
            .Where (x => x.HasValue)
            .Select (x => x.Value);
          if (!dateTimes.Any ()) {
            if (log.IsDebugEnabled) {
              log.DebugFormat ("GetOperationDetectionDateTime: no machine module with a date/time");
            }
            return null;
          }
          else {
            return dateTimes.Min ();
          }
        }
      }
    }

    DateTime? GetOperationDetectionDateTime (IMachineModule machineModule)
    {
      var analysisStatus = ModelDAOHelper.DAOFactory.MachineModuleAnalysisStatusDAO
        .FindById (machineModule.Id);
      if (null == analysisStatus) {
        log.ErrorFormat ("GetOperationDetectionDateTime: no analysis status for machine module id {0}", machineModule.Id);
        return null;
      }
      if (0 == analysisStatus.LastMachineModuleDetectionId) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("GetOperationDetectionDateTime: no machine module detection has been processed yet (LastMachineModuleDetectionId = 0) => return null");
        }
        return null;
      }
      var lastMachineModuleDetection = ModelDAOHelper.DAOFactory
        .MachineModuleDetectionDAO
        .FindById (analysisStatus.LastMachineModuleDetectionId, machineModule);
      if (null == lastMachineModuleDetection) {
        log.ErrorFormat ("GetOperationDetectionDateTime: no machine module detection id {0}",
          analysisStatus.LastMachineModuleDetectionId);
        return null;
      }
      return lastMachineModuleDetection.DateTime;
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

      log = LogManager.GetLogger (typeof (OperationDetectionStatusFromDetectionOnly).FullName + "." + machine.Id);

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("OperationDetectionStatusFromDetectionOnly.Initialize")) {
          m_monitoredMachine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindByIdWithMachineModules (machine.Id);
        }
      }
      if (null == m_monitoredMachine) {
        log.ErrorFormat ("Initialize: no monitored machine with id {0}", machine.Id);
        return false;
      }

      return true;
    }
  }
}
