// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using Lemoine.Core.Log;
using Lemoine.Extensions.Configuration;
using Lemoine.ModelDAO;
using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.Model;

namespace Pulse.Extensions.Configuration.Implementation
{
  /// <summary>
  /// Description of Configuration.
  /// </summary>
  public abstract class ConfigurationWithMachineFilter : IConfigurationWithMachineFilter
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ConfigurationWithMachineFilter).FullName);

    #region Getters / Setters
    /// <summary>
    /// Machine filter ID
    /// 
    /// 0 (default): no machine filter is applicable
    /// </summary>
    [PluginConf ("MachineFilter", "Machine filter", Description = "optionally an applicable machine filter", Multiple = false, Optional = true)]
    public int MachineFilterId
    {
      get; set;
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ConfigurationWithMachineFilter ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Return true if the configuration is valid
    /// </summary>
    public virtual bool IsValid (out IEnumerable<string> errors)
    {
      var errorList = new List<string> ();

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("ConfigurationWithMachineFilter.IsValid")) {
          if (0 != this.MachineFilterId) {
            if (null == ModelDAOHelper.DAOFactory.MachineFilterDAO
                .FindById (this.MachineFilterId)) {
              log.ErrorFormat ("IsValid: " +
                               "MachineFilter {0} does not exist",
                               this.MachineFilterId);
              errorList.Add ("MachineFilter with ID "
                          + this.MachineFilterId
                          + " does not exist");
            }
          }
          else if (IsMachineFilterRequired ()) {
            log.ErrorFormat ("IsValid: " +
                             "MachineFilter was not set",
                             this.MachineFilterId);
            errorList.Add ("MachineFilter not set");
          }
        }
      }

      errors = errorList;
      return (0 == errorList.Count);
    }

    /// <summary>
    /// Is the machine filter required ? (to override)
    /// </summary>
    /// <returns></returns>
    protected abstract bool IsMachineFilterRequired ();

    /// <summary>
    /// Check a machine matches the machine filter in configuration
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    public bool CheckMachineFilter (IMachine machine)
    {
      if (0 == MachineFilterId) {
        return !IsMachineFilterRequired ();
      }
      else { // 0 != MachineFilterId
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Extensions.ConfigurationWithMachineFilter")) {
            int machineFilterId = this.MachineFilterId;
            var machineFilter = ModelDAOHelper.DAOFactory.MachineFilterDAO
              .FindById (machineFilterId);
            if (null == machineFilter) {
              log.ErrorFormat ("CheckMachineFilter: " +
                               "machine filter id {0} does not exist",
                               machineFilterId);
              return false;
            }
            else {
              return machineFilter.IsMatch (machine);
            }
          }
        }
      }
    }
    #endregion // Methods
  }
}
