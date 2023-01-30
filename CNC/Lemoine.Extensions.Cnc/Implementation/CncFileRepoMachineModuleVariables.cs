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
using Lemoine.Extensions.Configuration;
using Pulse.Extensions.Configuration.Implementation;

namespace Lemoine.Extensions.Cnc.Implementation
{
  /// <summary>
  /// Abstract class to use when the CNC variables keys are drawn from the machinemodule table
  /// </summary>
  /// <typeparam name="TConfiguration"></typeparam>
  public abstract class CncFileRepoMachineModuleVariables<TConfiguration>
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<TConfiguration>
    , ICncFileRepoExtension
    where TConfiguration : ConfigurationWithMachineFilter, new ()
  {
    ILog log = LogManager.GetLogger (typeof (CncFileRepoMachineModuleVariables<TConfiguration>).FullName);

    IMachineFilter m_machineFilter = null;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="configurationLoader"></param>
    protected CncFileRepoMachineModuleVariables (IConfigurationLoader<TConfiguration> configurationLoader)
      : base (configurationLoader)
    {
    }

    /// <summary>
    /// Constructor with a default configuration loader
    /// </summary>
    public CncFileRepoMachineModuleVariables ()
      : base ()
    {
    }


    /// <summary>
    /// Initialize the extension
    /// </summary>
    /// <param name="cncAcquisition"></param>
    /// <returns></returns>
    public bool Initialize (ICncAcquisition cncAcquisition)
    {
      Debug.Assert (null != cncAcquisition);

      log = LogManager.GetLogger (string.Format ("{0}.{1}",
                                                 this.GetType ().FullName,
                                                 cncAcquisition.Id));

      if (!LoadConfiguration (out var configuration)) {
        log.ErrorFormat ("Initialize: " +
                         "configuration error, skip this instance");
        return false;
      }

      if (0 == configuration.MachineFilterId) { // Machine filter
        return !IsMachineFilterRequired ();
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

    /// <summary>
    /// Return the cnc variables that are set in the database
    /// </summary>
    /// <param name="machineModule"></param>
    /// <returns></returns>
    public virtual IEnumerable<string> GetCncVariableKeys (IMachineModule machineModule)
    {
      var result = new List<string> ();
      if ((null == m_machineFilter) && IsMachineFilterRequired ()) {
        return result;
      }
      if ((null != m_machineFilter) && !m_machineFilter.IsMatch (machineModule.MonitoredMachine)) { // Machine filter does not match
        return result;
      }
      if (!string.IsNullOrEmpty (machineModule.CycleVariable)) {
        result.Add (machineModule.CycleVariable);
      }
      if (!string.IsNullOrEmpty (machineModule.StartCycleVariable)) {
        result.Add (machineModule.StartCycleVariable);
      }
      if (!string.IsNullOrEmpty (machineModule.SequenceVariable)) {
        result.Add (machineModule.SequenceVariable);
      }
      if (!string.IsNullOrEmpty (machineModule.MilestoneVariable)) {
        result.Add (machineModule.MilestoneVariable);
      }
      return result;
    }

    /// <summary>
    /// No detection method is returned
    /// </summary>
    /// <param name="machineModule"></param>
    /// <returns></returns>
    public virtual DetectionMethod? GetDefaultDetectionMethod (IMachineModule machineModule)
    {
      return null;
    }

    /// <summary>
    /// Is a machine filter required ? (to override)
    /// </summary>
    /// <returns></returns>
    protected abstract bool IsMachineFilterRequired ();

    /// <summary>
    /// Get include path implementation (default: return null, nothing)
    /// </summary>
    /// <param name="extensionName"></param>
    /// <returns></returns>
    public string GetIncludePath (string extensionName)
    {
      return null;
    }

    /// <summary>
    /// Get include XM implementation (default: return null, nothing)
    /// </summary>
    /// <param name="extensionName"></param>
    /// <returns></returns>
    public Tuple<string, Dictionary<string, string>> GetIncludedXmlTemplate (string extensionName)
    {
      return null;
    }
  }
}

