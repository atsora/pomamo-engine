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

namespace Lemoine.Plugin.AcquisitionErrorEvent
{
  /// <summary>
  /// Description of Configuration.
  /// </summary>
  public class Configuration : Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly TimeSpan MAX_DURATION_DEFAULT = TimeSpan.FromMinutes (15);
    static readonly int MACHINE_MODE_ID_DEFAULT = (int)Model.MachineModeId.AcquisitionError;
    static readonly int EVENT_LEVEL_ID_DEFAULT = 2;

    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters
    /// <summary>
    /// Max duration
    /// </summary>
    [PluginConf ("DurationPicker", "Maximum duration", Description = "maximum duration of the period. Default is 0:15:00", Parameters = "0:00:00", Multiple = false, Optional = false)]
    [DefaultValue ("0:15:00")]
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
    /// Machine mode ID
    /// </summary>
    [PluginConf ("MachineMode", "Machine mode", Description = "machine mode to consider for an acquisition error. Default is AcquisitionError", Multiple = false, Optional = false)]
    [DefaultValue (59)]
    public int MachineModeId
    {
      get; set;
    }

    /// <summary>
    /// Event Level ID
    /// </summary>
    [PluginConf ("EventLevel", "Event level", Description = "Event level. Default is 2", Multiple = false, Optional = false)]
    [DefaultValue (2)]
    public int EventLevelId
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
      this.MachineModeId = MACHINE_MODE_ID_DEFAULT;
      this.EventLevelId = EVENT_LEVEL_ID_DEFAULT;
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
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("AcquisitionErrorEvent.ConfigurationErrors")) {
          if (null == ModelDAOHelper.DAOFactory.MachineModeDAO
              .FindById (this.MachineModeId)) {
            log.ErrorFormat ("GetConfigurationErrors: " +
                             "MachineMode {0} does not exist",
                             this.MachineModeId);
            errorList.Add ("MachineMode with ID "
                        + this.MachineModeId
                        + " does not exist");
          }
          if (null == ModelDAOHelper.DAOFactory.EventLevelDAO
                .FindById (this.EventLevelId)) {
            log.ErrorFormat ("GetConfigurationErrors: " +
                             "Event level {0} does not exist",
                             this.EventLevelId);
            errorList.Add ("EventLevel with ID "
                        + this.EventLevelId
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
