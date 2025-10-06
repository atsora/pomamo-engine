// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Extensions.Cnc;
using Lemoine.Model;
using System.Diagnostics;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System.Collections.Generic;
using Lemoine.Extensions.Configuration;
using System.Linq;
using Pulse.Extensions.Configuration;

namespace Pulse.PluginImplementation.Cnc
{
  /// <summary>
  /// Abstract class to use when a <see cref="ICncFileRepoExtension"/> depends on the machine
  /// </summary>
  /// <typeparam name="TConfiguration"></typeparam>
  public abstract class FilteredByMachineCncFileRepo<TConfiguration>
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<TConfiguration>
    , ICncFileRepoExtension
    where TConfiguration : IConfigurationWithMachineFilter, new()
  {
    ILog log = LogManager.GetLogger (typeof (FilteredByMachineCncFileRepo<TConfiguration>).FullName);

    IMachineFilter m_machineFilter = null;
    TConfiguration m_configuration;

    /// <summary>
    /// Associated configuration
    /// </summary>
    public virtual TConfiguration Configuration => m_configuration;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="configurationLoader"></param>
    protected FilteredByMachineCncFileRepo (IConfigurationLoader<TConfiguration> configurationLoader)
      : base (configurationLoader)
    {
    }

    /// <summary>
    /// Constructor with a default configuration loader
    /// </summary>
    public FilteredByMachineCncFileRepo ()
      : base ()
    {
    }

    /// <summary>
    /// <see cref="ICncFileRepoExtension"/>
    /// </summary>
    public virtual double XmlExtensionOrder => 100; // 100 as a reference

    /// <summary>
    /// Initialize the extension
    /// </summary>
    /// <param name="cncAcquisition"></param>
    /// <returns></returns>
    public bool Initialize (ICncAcquisition cncAcquisition)
    {
      Debug.Assert (null != cncAcquisition);

      log = LogManager.GetLogger ($"{this.GetType ().FullName}.{cncAcquisition.Id}");

      if (!LoadConfiguration (out m_configuration)) {
        log.Error ("Initialize: configuration error, skip this instance");
        return false;
      }

      if (0 == m_configuration.MachineFilterId) { // Machine filter
        return !IsMachineFilterRequired ();
      }
      else { // Machine filter
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Plugin.FilteredByMachineCncFileRepo.Initialize")) {
            if (!cncAcquisition.MachineModules.Any ()) {
              log.Debug ($"Initialize: no machine module is associated to cnc acquisition {cncAcquisition.Id}");
              return false;
            }
            int machineFilterId = m_configuration.MachineFilterId;
            m_machineFilter = ModelDAOHelper.DAOFactory.MachineFilterDAO
              .FindById (machineFilterId);
            if (m_machineFilter is null) {
              log.Error ($"Initialize: machine filter id {machineFilterId} does not exist");
              return false;
            }
            return cncAcquisition.MachineModules
              .Any (x => m_machineFilter.IsMatch (x.MonitoredMachine));
          }
        }
      }
    }

    /// <summary>
    /// Return the cnc variables that are set in the database
    /// </summary>
    /// <param name="machineModule"></param>
    /// <returns></returns>
    public virtual IEnumerable<string> GetCncVariableKeys (IMachineModule machineModule) => null;

    /// <summary>
    /// No detection method is returned
    /// </summary>
    /// <param name="machineModule"></param>
    /// <returns></returns>
    public virtual DetectionMethod? GetDefaultDetectionMethod (IMachineModule machineModule) => null;

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
    public virtual string GetIncludePath (string extensionName) => null;

    /// <summary>
    /// Get include XM implementation (default: return null, nothing)
    /// </summary>
    /// <param name="extensionName"></param>
    /// <returns></returns>
    public virtual Tuple<string, Dictionary<string, string>> GetIncludedXmlTemplate (string extensionName) => null;

    /// <summary>
    /// <see cref="ICncFileRepoExtension"/>
    /// </summary>
    /// <param name="extensionName"></param>
    /// <returns></returns>
    public virtual string GetExtensionAsXmlString (string extensionName) => null;
  }
}