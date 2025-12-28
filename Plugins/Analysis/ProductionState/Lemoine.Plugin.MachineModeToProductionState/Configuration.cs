// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.ModelDAO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace Lemoine.Plugin.MachineModeToProductionState
{
  public sealed class Configuration
    : Pulse.Extensions.Configuration.Implementation.ConfigurationWithMachineFilter
    , Pulse.Extensions.Configuration.IConfigurationWithMachineFilter
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    /// <summary>
    /// Score of the plugin
    /// </summary>
    [PluginConf ("Double", "Score", Description = "Score. Default: 20.0", Parameters = "100000:3", Multiple = false, Optional = false)]
    [DefaultValue (20.0)]
    public double Score { get; set; } = 20.0;

    /// <summary>
    /// Associated running production state id. If 0, consider a default production state
    /// </summary>
    [PluginConf ("ProductionState", "Running production state", Description = "Running production state", Multiple = false, Optional = true)]
    public int ProductionStateId { get; set; }

    /// <summary>
    /// Default associated production state translation key.
    /// 
    /// If null or empty, a default production state translation key is set in the plugin
    /// </summary>
    [PluginConf ("Text", "Default production state translation key", Description = "optionally a default production state translation key in case no specific production state is set", Multiple = false, Optional = true)]
    [DefaultValue ("")]
    public string DefaultProductionStateTranslationKey { get; set; } = "";

    /// <summary>
    /// Default associated production state translation value.
    /// 
    /// If null or empty, a default production state translation value is set in the plugin
    /// </summary>
    [PluginConf ("Text", "Default production state translation value", Description = "optionally a default production state translation value in case no specific production state is set", Multiple = false, Optional = true)]
    [DefaultValue ("Translation value to set")]
    public string DefaultProductionStateTranslationValue { get; set; } = "Translation value to set";

    /// <summary>
    /// Default associated production state color.
    /// 
    /// If null or empty, a default production state color is set in the plugin
    /// </summary>
    [PluginConf ("Text", "Default production state color", Description = "optionally a production state color in case no production state is set. Default: white #FFFFFF", Multiple = false, Optional = true)]
    [DefaultValue ("#FFFFFF")]
    public string DefaultProductionStateColor { get; set; } = "#FFFFFF"; // White

    /// <summary>
    /// Production rate
    /// </summary>
    [PluginConf ("Double", "Production rate", Description = "Production rate", Parameters = "100:3", Multiple = false, Optional = false)]
    public double? ProductionRate { get; set; }

    /// <summary>
    /// Machine mode that is the source of the production state
    /// </summary>
    [PluginConf ("MachineMode", "Machine mode", Description = "Machine mode that is the source of the production state", Multiple = false, Optional = false)]
    public int MachineModeId { get; set; }

    /// <summary>
    /// Max duration
    /// </summary>
    [PluginConf ("TimeSpan", "Max duration", Description = "Max duration", Multiple = false, Optional = true)]
    public TimeSpan? MaxDuration { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    public Configuration ()
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="errors"></param>
    /// <returns></returns>
    public override bool IsValid (out IEnumerable<string> errors)
    {
      IEnumerable<string> baseErrors;
      var result = base.IsValid (out baseErrors);

      var errorList = new List<string> ();

      if (this.Score < 0.0) {
        string message = $"invalid score {this.Score}: negative";
        log.Error ($"IsValid: {message}");
        errorList.Add (message);
      }

      if (!string.IsNullOrEmpty (this.DefaultProductionStateTranslationKey)
        && (string.IsNullOrEmpty (this.DefaultProductionStateTranslationValue)
        || string.IsNullOrEmpty (this.DefaultProductionStateColor))) {
        string message = $"a default production state translation key {DefaultProductionStateTranslationKey} is set while no translation value or color is set";
        log.Error ($"IsValid: {message}");
        errorList.Add (message);
      }

      // TODO: check colors

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyDeferrableTransaction ("Plugin.RunningMachineModeIsProduction.IsValidConfiguration")) {
          if (0 != this.ProductionStateId) { // Else consider the default running production state
            if (this.ProductionStateId <= 0) {
              string message = $"invalid production state id {this.ProductionStateId}: not strictly positive";
              log.Error ($"IsValid: {message}");
              errorList.Add (message);
            }
            else {
              var productionState = ModelDAOHelper.DAOFactory.ProductionStateDAO
                .FindById (this.ProductionStateId);
              if (null == productionState) {
                string message = $"invalid production state id {this.ProductionStateId}: unknown production state";
                log.Error ($"IsValid: {message}");
                errorList.Add (message);
              }
            }
          }
          if (this.MachineModeId <= 0) {
            string message = $"invalid reason id {this.MachineModeId}: not strictly positive";
            log.Error ($"IsValid: {message}");
            errorList.Add (message);
          }
          else {
            var machineMode = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById (this.MachineModeId);
            if (machineMode is null) {
              string message = $"invalid machine mode id {this.MachineModeId}: unknown machine mode";
              log.Error ($"IsValid: {message}");
              errorList.Add (message);
            }
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
  }
}
