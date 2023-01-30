// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;
using Lemoine.Extensions.Configuration.GuiBuilder;
using System.ComponentModel;

namespace Lemoine.Plugin.ShortPeriodSwitcher
{
  /// <summary>
  /// Description of Configuration.
  /// </summary>
  public class Configuration : Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly TimeSpan MAX_DURATION_DEFAULT = TimeSpan.FromSeconds (20);

    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters
    /// <summary>
    /// Max duration
    /// </summary>
    [PluginConf ("TimeSpan", "Maximum duration", Description = "maximum duration of the period. Default is 0:00:20", Multiple = false, Optional = false)]
    [DefaultValue ("0:00:20")]
    public TimeSpan MaxDuration
    {
      get; set;
    }

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

    /// <summary>
    /// Old machine mode ID
    /// </summary>
    [PluginConf ("MachineMode", "Machine mode to change", Description = "machine mode to match and to replace", Multiple = false, Optional = false)]
    public int OldMachineModeId
    {
      get; set;
    }

    /// <summary>
    /// New machine mode ID
    /// </summary>
    [PluginConf ("MachineMode", "New machine mode", Description = "machine mode to switch to", Multiple = false, Optional = false)]
    public int NewMachineModeId
    {
      get; set;
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Configuration ()
    {
      this.MaxDuration = MAX_DURATION_DEFAULT;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Return true if the configuration is valid
    /// </summary>
    public bool IsValid (out IEnumerable<string> errors)
    {
      var errorList = new List<string> ();

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("ShortPeriodSwitcher.ConfigurationErrors")) {
          if (null == ModelDAOHelper.DAOFactory.MachineModeDAO
              .FindById (this.NewMachineModeId)) {
            log.ErrorFormat ("GetConfigurationErrors: " +
                             "NewMachineMode {0} does not exist",
                             this.NewMachineModeId);
            errorList.Add ("New MachineMode with ID "
                        + this.NewMachineModeId
                        + " does not exist");
          }
          if (null == ModelDAOHelper.DAOFactory.MachineModeDAO
              .FindById (this.OldMachineModeId)) {
            log.ErrorFormat ("GetConfigurationErrors: " +
                             "OldMachineMode {0} does not exist",
                             this.OldMachineModeId);
            errorList.Add ("Old MachineMode with ID "
                        + this.OldMachineModeId
                        + " does not exist");
          }
          if (0 != this.MachineFilterId) {
            if (null == ModelDAOHelper.DAOFactory.MachineFilterDAO
                .FindById (this.MachineFilterId)) {
              log.ErrorFormat ("GetConfigurationErrors: " +
                               "MachineFilter {0} does not exist",
                               this.MachineFilterId);
              errorList.Add ("MachineFilter with ID "
                          + this.MachineFilterId
                          + " does not exist");
            }
          }
        }
      }

      errors = errorList;
      return (0 == errorList.Count);
    }
    #endregion // Methods
  }
}
