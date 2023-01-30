// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;
using Lemoine.Extensions.Configuration.GuiBuilder;
using System.Linq;
using System.ComponentModel;

namespace Lemoine.Plugin.AcquisitionDelayEvent
{
  /// <summary>
  /// Description of Configuration.
  /// </summary>
  public class Configuration
    : Pulse.Extensions.Configuration.Implementation.ConfigurationWithMachineFilter
    , Pulse.Extensions.Configuration.IConfigurationWithMachineFilter
  {
    static readonly TimeSpan MAX_DURATION_DEFAULT = TimeSpan.FromMinutes (15);
    static readonly int EVENT_LEVEL_ID_DEFAULT = 3;

    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters
    /// <summary>
    /// Max duration
    /// </summary>
    [PluginConf ("DurationPicker", "Maximum duration", Description = "the maximum duration of the period. Default is 0:15:00")]
    [DefaultValue ("0:15:00")]
    public TimeSpan MaxDuration
    {
      get; set;
    }

    /// <summary>
    /// Event Level ID
    /// </summary>
    [PluginConf ("EventLevel", "Event level", Description = "the associated event level. Default is 3", Optional = false, Multiple = false)]
    [DefaultValue (3)]
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
      this.EventLevelId = EVENT_LEVEL_ID_DEFAULT;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Return true if the configuration is valid
    /// </summary>
    public override bool IsValid (out IEnumerable<string> errors)
    {
      IEnumerable<string> baseErrors;
      var result = base.IsValid (out baseErrors);

      var errorList = new List<string> ();

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("AcquisitionDelayEvent.ConfigurationErrors")) {
          if (null == ModelDAOHelper.DAOFactory.EventLevelDAO
                .FindById (this.EventLevelId)) {
              log.ErrorFormat ("GetConfigurationErrors: " +
                               "Event level {0} does not exist",
                               this.EventLevelId);
              errorList.Add ("EventLevel with ID "
                          + this.EventLevelId
                          + " does not exist");
          }
        }
      }

      errors = baseErrors.Concat (errorList);
      return result && (!errors.Any ());
    }

    protected override bool IsMachineFilterRequired ()
    {
      return false;
    }
    #endregion // Methods
  }
}
