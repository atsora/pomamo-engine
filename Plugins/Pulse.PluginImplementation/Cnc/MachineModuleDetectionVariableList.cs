// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Collections;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Pulse.PluginImplementation.Cnc
{
  /// <summary>
  /// To be inherited in a plugin:
  /// create a row in machinemoduledetection with the variable list when new cnc variable values are detected
  /// </summary>
  public abstract class MachineModuleDetectionVariableList<TConfiguration>
    : Lemoine.Extensions.UniqueInstanceConfigurableExtension<TConfiguration>
    , Lemoine.Extensions.Cnc.IImportCncVariablesExtension
    where TConfiguration : Pulse.Extensions.Configuration.IConfigurationWithMachineFilter
  {
    IMachineFilter m_machineFilter = null;
    IMachineModule m_machineModule = null;

    ILog log = LogManager.GetLogger (typeof (MachineModuleDetectionVariableList<TConfiguration>).FullName);

    /// <summary>
    /// <see cref="Lemoine.Extensions.Cnc.IImportCncVariablesExtension"/>
    /// </summary>
    /// <param name="machineModule"></param>
    /// <returns></returns>
    public bool Initialize (IMachineModule machineModule)
    {
      Debug.Assert (null != machineModule);

      m_machineModule = machineModule;

      log = LogManager.GetLogger ($"{this.GetType ().FullName}.{m_machineModule.MonitoredMachine.Id}.{m_machineModule.Id}");

      var configurations = LoadConfigurations ();
      if (!configurations.Any ()) {
        log.Error ("Initialize: no configuration");
        return false;
      }

      foreach (var configuration in configurations) {
        if ((0 == configuration.MachineFilterId) && !IsMachineFilterRequired ()) { // Machine filter
          return true;
        }
        else { // Machine filter
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
            using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Plugin.StampDetection.CncFileRepoExtension.Initialize")) {
              int machineFilterId = configuration.MachineFilterId;
              m_machineFilter = ModelDAOHelper.DAOFactory.MachineFilterDAO
                .FindById (machineFilterId);
              if (null == m_machineFilter) {
                log.Error ($"Initialize: machine filter id {machineFilterId} does not exist");
              }
              else { // null != m_machineFilter
                if (m_machineFilter.IsMatch (machineModule.MonitoredMachine)) {
                  return true;
                }
              }
            }
          }
        }
      }

      return false;
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Cnc.IImportCncVariablesExtension"/>
    /// </summary>
    /// <param name="variableSet"></param>
    /// <param name="startDateTime"></param>
    public void AfterImportCncVariables (IDictionary<string, object> variableSet, DateTime startDateTime)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session
          .BeginTransaction ("MachineModuleDetectionVariableList.AfterImportCncVariables", TransactionLevel.ReadCommitted)) {
          // Read-committed because it is run after several transactions
          transaction.SynchronousCommitOption = SynchronousCommit.Off; // This is ok because it may be replayed

          // - MachineModuleDetection
          IMachineModuleDetection detection = ModelDAOHelper.ModelFactory
            .CreateMachineModuleDetection (m_machineModule, startDateTime); // machine module
          detection.CustomType = "CncVariables";
          detection.CustomValue = variableSet.Keys.ToListString ();
          ModelDAOHelper.DAOFactory.MachineModuleDetectionDAO.MakePersistent (detection);

          // - DetectionTimeStamp
          var detectionTimeStamp = ModelDAOHelper.DAOFactory.AcquisitionStateDAO.GetAcquisitionState (m_machineModule, AcquisitionStateKey.Detection);
          if (null == detectionTimeStamp) {
            detectionTimeStamp = ModelDAOHelper.ModelFactory.CreateAcquisitionState (m_machineModule, AcquisitionStateKey.Detection);
          }
          detectionTimeStamp.DateTime = startDateTime;
          ModelDAOHelper.DAOFactory.AcquisitionStateDAO.MakePersistent (detectionTimeStamp);

          // - Commit
          transaction.Commit ();
        }
      }
    }

    /// <summary>
    /// Is a machine filter required ? (to override)
    /// </summary>
    /// <returns></returns>
    protected abstract bool IsMachineFilterRequired ();
  }
}
