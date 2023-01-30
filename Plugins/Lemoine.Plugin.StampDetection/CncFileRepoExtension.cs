// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Xml;
using Lemoine.Extensions.Cnc;
using Lemoine.Model;
using System.Diagnostics;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System.Collections.Generic;

namespace Lemoine.Plugin.StampDetection
{
  public class CncFileRepoExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , ICncFileRepoExtension
  {
    ILog log = LogManager.GetLogger (typeof (CncFileRepoExtension).FullName);

    IMachineFilter m_machineFilter = null;
    DetectionMethod m_detectionMethod = DetectionMethod.SequenceStamp | DetectionMethod.StartCycleStamp | DetectionMethod.EndCycleStamp;

    public bool Initialize (ICncAcquisition cncAcquisition)
    {
      Debug.Assert (null != cncAcquisition);

      log = LogManager.GetLogger (string.Format ("{0}.{1}",
                                                 this.GetType ().FullName,
                                                 cncAcquisition.Id));

      Configuration configuration;
      if (!LoadConfiguration (out configuration)) {
        log.ErrorFormat ("Initialize: " +
                         "wrong configuration, skip this instance");
        return false;
      }

      if (configuration.IncludeMilestone) {
        m_detectionMethod |= DetectionMethod.SequenceMilestoneWithStamp;
      }

      if (0 == configuration.MachineFilterId) { // Machine filter
        return true;
      }
      else { // Machine filter
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Plugin.StampDetection.CncFileRepoExtension.Initialize")) {
            foreach (IMachineModule machineModule in cncAcquisition.MachineModules) {
              int machineFilterId = configuration.MachineFilterId;
              m_machineFilter = ModelDAOHelper.DAOFactory.MachineFilterDAO
                .FindById (machineFilterId);
              if (null == m_machineFilter) {
                log.ErrorFormat ("Initialize: " +
                                 "machine filter id {0} does not exist",
                                 machineFilterId);
                return false;
              }
              else { // null != m_machineFilter
                if (m_machineFilter.IsMatch (machineModule.MonitoredMachine)) {
                  return true;
                }
              }
            }
          }
        }
        return false; // No machine module matches
      }
    }

    public IEnumerable<string> GetCncVariableKeys (IMachineModule machineModule)
    {
      return new List<string> ();
    }

    public DetectionMethod? GetDefaultDetectionMethod (IMachineModule machineModule)
    {
      if ((null != m_machineFilter) && !m_machineFilter.IsMatch (machineModule.MonitoredMachine)) {
        return null;
      }
      else {
        return m_detectionMethod;
      }
    }

    public string GetIncludePath (string extensionName)
    {
      return null;
    }
    
    public Tuple<string, Dictionary<string, string>> GetIncludedXmlTemplate (string extensionName)
    {
      return null;
    }
  }
}
