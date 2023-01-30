// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;
using Lemoine.Model;

namespace Pulse.Extensions.Database.Impl.CycleDetectionStatus
{
  /// <summary>
  /// Get the cycle detection status from the machinemoduleanalysisstatus
  /// and machinemoduledetection tables
  /// 
  /// Configurable version with a machine filter
  /// </summary>
  public class CycleDetectionStatusFromAnalysisStatusWithMachineFilter<TConfiguration>
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<TConfiguration>
    , ICycleDetectionStatusExtension
    where TConfiguration : Pulse.Extensions.Configuration.IConfigurationWithMachineFilter, ICycleDetectionStatusConfiguration, new ()
  {
    ILog log = LogManager.GetLogger (typeof (CycleDetectionStatusFromAnalysisStatusWithMachineFilter<TConfiguration>).FullName);

    IMachine m_machine;
    TConfiguration m_configuration;
    ICycleDetectionStatusExtension m_cycleDetectionStatus;

    /// <summary>
    /// Initialize for ICycleDetectionStatusExtension
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    public bool Initialize (IMachine machine)
    {
      Debug.Assert (null != machine);
      Debug.Assert (null == m_machine);

      m_machine = machine;

      log = LogManager.GetLogger (string.Format ("{0}.{1}",
                                                 this.GetType ().FullName,
                                                 m_machine.Id));

      if (!LoadConfiguration (out m_configuration)) {
        log.ErrorFormat ("Initialize: bad configuration, skip this instance");
        return false;
      }

      if (!CheckMachineFilter (machine)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"Initialize: machine {machine.Id} does not match machine filter => return false (skip this extension)");
        }
        return false;
      }

      m_cycleDetectionStatus = new CycleDetectionStatusFromAnalysisStatus (m_configuration.CycleDetectionStatusPriority);
      if (!m_cycleDetectionStatus.Initialize (machine)) {
        log.ErrorFormat ("Initialize: initialization of cycle detection status failed");
        return false;
      }

      return true;
    }

    /// <summary>
    /// ICycleDetectionStatusExtension
    /// </summary>
    public int CycleDetectionStatusPriority
    {
      get { return m_configuration.CycleDetectionStatusPriority; }
    }

    /// <summary>
    /// ICycleDetectionStatusExtension
    /// </summary>
    /// <returns></returns>
    public DateTime? GetCycleDetectionDateTime ()
    {
      return m_cycleDetectionStatus.GetCycleDetectionDateTime ();
    }

    bool CheckMachineFilter (IMachine machine)
    {
      IMachineFilter machineFilter = null;
      if (0 == m_configuration.MachineFilterId) { // Machine filter
        return false;
      }
      else { // Machine filter
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("CycleDetectionStatusFromAnalysisStatus.CheckMachineFilter")) {
            int machineFilterId = m_configuration.MachineFilterId;
            machineFilter = ModelDAOHelper.DAOFactory.MachineFilterDAO
              .FindById (machineFilterId);
            if (null == machineFilter) {
              log.ErrorFormat ("Initialize: " +
                               "machine filter id {0} does not exist",
                               machineFilterId);
              return false;
            }
            else {
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
