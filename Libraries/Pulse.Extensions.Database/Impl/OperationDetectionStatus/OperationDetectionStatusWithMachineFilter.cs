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

namespace Pulse.Extensions.Database.Impl.OperationDetectionStatus
{
  /// <summary>
  /// Get the cycle detection status from the machinemoduleanalysisstatus
  /// and machinemoduledetection tables
  /// 
  /// Configurable version with a machine filter
  /// </summary>
  public class OperationDetectionStatusWithMachineFilter<TConfiguration>
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<TConfiguration>
    , IOperationDetectionStatusExtension
    where TConfiguration : Pulse.Extensions.Configuration.IConfigurationWithMachineFilter, IOperationDetectionStatusConfiguration, new ()
  {
    ILog log = LogManager.GetLogger (typeof (OperationDetectionStatusWithMachineFilter<TConfiguration>).FullName);

    IMachine m_machine;
    TConfiguration m_configuration;
    IOperationDetectionStatusExtension m_operationDetectionStatus;
    readonly Func<int, IOperationDetectionStatusExtension> m_extensionConstructor;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="extensionConstructor"></param>
    public OperationDetectionStatusWithMachineFilter (Func<int, IOperationDetectionStatusExtension> extensionConstructor)
    {
      m_extensionConstructor = extensionConstructor;
    }

    /// <summary>
    /// Initialize for IOperationDetectionStatusExtension
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

      m_operationDetectionStatus = m_extensionConstructor (m_configuration.OperationDetectionStatusPriority);
      if (!m_operationDetectionStatus.Initialize (machine)) {
        log.ErrorFormat ("Initialize: initialization of cycle detection status failed");
        return false;
      }

      return true;
    }

    /// <summary>
    /// IOperationDetectionStatusExtension
    /// </summary>
    public int OperationDetectionStatusPriority
    {
      get { return m_configuration.OperationDetectionStatusPriority; }
    }

    /// <summary>
    /// IOperationDetectionStatusExtension
    /// </summary>
    /// <returns></returns>
    public DateTime? GetOperationDetectionDateTime ()
    {
      return m_operationDetectionStatus.GetOperationDetectionDateTime ();
    }

    bool CheckMachineFilter (IMachine machine)
    {
      IMachineFilter machineFilter = null;
      if (0 == m_configuration.MachineFilterId) { // Machine filter
        return true;
      }
      else { // Machine filter
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("OperationDetectionStatusFromAnalysisStatus.CheckMachineFilter")) {
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
