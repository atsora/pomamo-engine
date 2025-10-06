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
  /// Abstract class to filter extensions by machine
  /// </summary>
  /// <typeparam name="TConfiguration"></typeparam>
  public abstract class FilteredByMachineExtension<TConfiguration>
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<TConfiguration>
    where TConfiguration : IConfigurationWithMachineFilter, new()
  {
    ILog log = LogManager.GetLogger (typeof (FilteredByMachineExtension<TConfiguration>).FullName);

    IMachineFilter m_machineFilter = null;
    TConfiguration m_configuration;
    IMachine m_machine = null;

    /// <summary>
    /// Associated machine
    /// </summary>
    public IMachine Machine => m_machine;

    /// <summary>
    /// Associated configuration
    /// </summary>
    public virtual TConfiguration Configuration => m_configuration;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="configurationLoader"></param>
    protected FilteredByMachineExtension (IConfigurationLoader<TConfiguration> configurationLoader)
      : base (configurationLoader)
    {
    }

    /// <summary>
    /// Constructor with a default configuration loader
    /// </summary>
    public FilteredByMachineExtension ()
      : base ()
    {
    }

    /// <summary>
    /// Initialize the extension
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    public virtual bool Initialize (IMachine machine)
    {
      Debug.Assert (null != machine);

      log = LogManager.GetLogger ($"{this.GetType ().FullName}.{machine.Id}");

      if (!LoadConfiguration (out m_configuration)) {
        log.Error ("Initialize: configuration error, skip this instance");
        return false;
      }
      m_machine = machine;

      if (0 == m_configuration.MachineFilterId) { // Machine filter
        return !IsMachineFilterRequired ();
      }
      else { // Machine filter
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Plugin.Implementation.FilteredByMachineExtension.Initialize")) {
            int machineFilterId = m_configuration.MachineFilterId;
            m_machineFilter = ModelDAOHelper.DAOFactory.MachineFilterDAO
              .FindById (machineFilterId);
            if (m_machineFilter is null) {
              log.Error ($"Initialize: machine filter id {machineFilterId} does not exist");
              return false;
            }
            else {
              return m_machineFilter.IsMatch (machine);
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