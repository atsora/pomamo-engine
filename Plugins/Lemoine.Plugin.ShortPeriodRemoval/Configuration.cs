// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using Lemoine.Conversion.JavaScript;
using Lemoine.Model;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;
using System.Linq;
using Lemoine.Extensions.Configuration.GuiBuilder;
using System.ComponentModel;

namespace Lemoine.Plugin.ShortPeriodRemoval
{
  /// <summary>
  /// Description of Configuration.
  /// </summary>
  public class Configuration
    : Pulse.Extensions.Configuration.Implementation.ConfigurationWithMachineFilter
    , Pulse.Extensions.Configuration.IConfigurationWithMachineFilter
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    /// <summary>
    /// Max duration
    /// </summary>
    [PluginConf ("DurationPicker", "Max duration", Description = "the maximum duration of the period. Default is 0:00:20", Optional = false, Parameters = "0:00:00")]
    [DefaultValue ("0:00:20")]
    public TimeSpan MaxDuration
    {
      get; set;
    } = TimeSpan.FromSeconds (20);

    /// <summary>
    /// Old machine mode ID
    /// </summary>
    [PluginConf ("MachineMode", "Old machine mode", Description = "the machine mode that must be overriden (old). Default is AutoNoMotion", Optional = false, Multiple = false)]
    [DefaultValue (67)]
    public int OldMachineModeId
    {
      get; set;
    } = (int)MachineModeId.AutoNoMotion;

    /// <summary>
    /// New machine mode ID
    /// </summary>
    [PluginConf ("MachineMode", "New machine mode", Description = "the machine modes that can be considered to override the old one (new). Default is AutoActive", Optional = true, Multiple = false)]
    [DefaultValue (3)]
    public int NewMachineModeId
    {
      get; set;
    } = (int)MachineModeId.AutoActive;

    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Configuration ()
    {
    }

    /// <summary>
    /// Return true if the configuration is valid
    /// </summary>
    public override bool IsValid (out IEnumerable<string> errors)
    {
      IEnumerable<string> baseErrors;
      var result = base.IsValid (out baseErrors);

      var errorList = new List<string> ();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("ShortPeriodRemoval.ConfigurationErrors")) {
          if (0 != this.NewMachineModeId) {
            if (null == ModelDAOHelper.DAOFactory.MachineModeDAO
                .FindById (this.NewMachineModeId)) {
              log.ErrorFormat ("GetConfigurationErrors: " +
                               "NewMachineMode {0} does not exist",
                               this.NewMachineModeId);
              errorList.Add ("New MachineMode with ID "
                          + this.NewMachineModeId
                          + " does not exist");
            }
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
        }
      }

      errors = baseErrors.Concat (errorList);
      return result && (!errors.Any ());
    }

    protected override bool IsMachineFilterRequired () => false;
  }
}
