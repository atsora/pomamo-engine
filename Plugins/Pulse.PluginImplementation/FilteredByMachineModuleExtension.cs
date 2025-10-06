// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using System.Diagnostics;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System.Collections.Generic;
using Lemoine.Extensions.Configuration;
using Pulse.Extensions.Configuration.Implementation;
using Pulse.Extensions.Configuration;

namespace Pulse.PluginImplementation
{
  /// <summary>
  /// Abstract class to filter extensions by machine module
  /// </summary>
  /// <typeparam name="TConfiguration"></typeparam>
  public abstract class FilteredByMachineModuleExtension<TConfiguration>
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<TConfiguration>
    where TConfiguration : IConfigurationWithMachineFilter, new()
  {
    ILog log = LogManager.GetLogger (typeof (FilteredByMachineModuleExtension<TConfiguration>).FullName);

    IMachineFilter m_machineFilter = null;
    TConfiguration m_configuration;
    IMachineModule m_machineModule = null;

    /// <summary>
    /// Associated machine
    /// </summary>
    public IMonitoredMachine Machine => m_machineModule.MonitoredMachine;

    /// <summary>
    /// Associated machine module
    /// </summary>
    public IMachineModule MachineModule => m_machineModule;

    /// <summary>
    /// Associated configuration
    /// </summary>
    public virtual TConfiguration Configuration => m_configuration;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="configurationLoader"></param>
    protected FilteredByMachineModuleExtension (IConfigurationLoader<TConfiguration> configurationLoader)
      : base (configurationLoader)
    {
    }

    /// <summary>
    /// Constructor with a default configuration loader
    /// </summary>
    public FilteredByMachineModuleExtension ()
      : base ()
    {
    }

    /// <summary>
    /// Initialize the extension
    /// </summary>
    /// <param name="machineModule"></param>
    /// <returns></returns>
    public virtual bool Initialize (IMachineModule machineModule)
    {
      Debug.Assert (null != machineModule);

      log = LogManager.GetLogger ($"{this.GetType ().FullName}.{machineModule.MonitoredMachine.Id}.{machineModule.Id}");

      if (!LoadConfiguration (out m_configuration)) {
        log.Error ("Initialize: configuration error, skip this instance");
        return false;
      }
      m_machineModule = machineModule;

      if (0 == m_configuration.MachineFilterId) { // Machine filter
        return !IsMachineFilterRequired ();
      }
      else { // Machine filter
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Plugin.Implementation.FilteredByMachineModule.Initialize")) {
            int machineFilterId = m_configuration.MachineFilterId;
            m_machineFilter = ModelDAOHelper.DAOFactory.MachineFilterDAO
              .FindById (machineFilterId);
            if (m_machineFilter is null) {
              log.Error ($"Initialize: machine filter id {machineFilterId} does not exist");
              return false;
            }
            else {
              return m_machineFilter.IsMatch (this.Machine);
            }
          }
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