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

namespace Lemoine.Plugin.RunningMachineModeIsProduction
{
  public sealed class Configuration
    : Pulse.Extensions.Configuration.Implementation.ConfigurationWithMachineFilter
    , Pulse.Extensions.Configuration.IConfigurationWithMachineFilter
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters
    /// <summary>
    /// Score of the plugin
    /// </summary>
    [PluginConf ("Double", "Score", Description = "Score. Default: 10.0", Parameters = "100000:3", Multiple = false, Optional = false)]
    [DefaultValue (10.0)]
    public double Score { get; set; } = 10.0;

    /// <summary>
    /// Associated running production state id. If 0, consider a default production state
    /// </summary>
    [PluginConf ("ProductionState", "Running production state", Description = "Running production state", Multiple = false, Optional = true)]
    public int RunningProductionStateId { get; set; }

    /// <summary>
    /// Default associated running production state translation key.
    /// 
    /// If null or empty, a default production state translation key is set in the plugin
    /// </summary>
    [PluginConf ("Text", "Default running production state translation key", Description = "optionally a default running production state translation key in case no running production state is set", Multiple = false, Optional = true)]
    [DefaultValue ("ProductionStateProduction")]
    public string DefaultRunningProductionStateTranslationKey { get; set; } = "ProductionStateProduction";

    /// <summary>
    /// Default associated running production state translation value.
    /// 
    /// If null or empty, a default running production state translation value is set in the plugin
    /// </summary>
    [PluginConf ("Text", "Default running production state translation value", Description = "optionally a default running production state translation value in case no running production state is set", Multiple = false, Optional = true)]
    [DefaultValue ("Production")]
    public string DefaultRunningProductionStateTranslationValue { get; set; } = "Production";

    /// <summary>
    /// Default associated running production state color.
    /// 
    /// If null or empty, a default running production state color is set in the plugin
    /// </summary>
    [PluginConf ("Text", "Default running production state color", Description = "optionally a default running production state color in case no running production state is set. Default: blue #0080FF", Multiple = false, Optional = true)]
    [DefaultValue ("#0080FF")]
    public string DefaultRunningProductionStateColor { get; set; } = "#3498DB"; // Blue. Previously #0080FF

    /// <summary>
    /// Associated not running production state id. If 0, consider a default production state
    /// </summary>
    [PluginConf ("ProductionState", "Not running production state", Description = "Not running production state", Multiple = false, Optional = true)]
    public int NotRunningProductionStateId { get; set; }

    /// <summary>
    /// Default associated not running production state translation key.
    /// 
    /// If null or empty, a default production state translation key is set in the plugin
    /// </summary>
    [PluginConf ("Text", "Default not running production state translation key", Description = "optionally a default not running production state translation key in case no not running production state is set", Multiple = false, Optional = true)]
    [DefaultValue ("ProductionStateNoProduction")]
    public string DefaultNotRunningProductionStateTranslationKey { get; set; } = "ProductionStateNoProduction";

    /// <summary>
    /// Default associated not running production state translation value.
    /// 
    /// If null or empty, a default production state translation value is set in the plugin
    /// </summary>
    [PluginConf ("Text", "Default not running production state translation value", Description = "optionally a default not running production state translation value in case no running production state is set", Multiple = false, Optional = true)]
    [DefaultValue ("No production")]
    public string DefaultNotRunningProductionStateTranslationValue { get; set; } = "No production";

    /// <summary>
    /// Default associated not running production state color.
    /// 
    /// If null or empty, a default not running production state color is set in the plugin
    /// </summary>
    [PluginConf ("Text", "Default not running production state color", Description = "optionally a default not running production state color in case no running production state is set. Default: red #AA0909", Multiple = false, Optional = true)]
    [DefaultValue ("#AA0909")]
    public string DefaultNotRunningProductionStateColor { get; set; } = "#AA0909"; // Red

    /// <summary>
    /// Exclude manual
    /// </summary>
    [PluginConf ("Bool", "Exclude manual", Description = "Exclude from production the manual machine modes. Default: false")]
    [DefaultValue (false)]
    public bool ExcludeManual {
      get; set;
    } = false;

    /// <summary>
    /// Unknown
    /// </summary>
    [PluginConf ("Bool", "Unknown is not running", Description = "Consider the machine modes with an unknown running status are not running. Default: false")]
    [DefaultValue (false)]
    public bool UnknownIsNotRunning
    {
      get; set;
    } = false;
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
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

      if (this.Score < 0.0) {
        string message = $"invalid score {this.Score}: negative";
        log.Error ($"IsValid: {message}");
        errorList.Add (message);
      }

      if (!string.IsNullOrEmpty (this.DefaultRunningProductionStateTranslationKey)
        && (string.IsNullOrEmpty (this.DefaultRunningProductionStateTranslationValue)
        || string.IsNullOrEmpty (this.DefaultRunningProductionStateColor))) {
        string message = $"a default running production state translation key {DefaultRunningProductionStateTranslationKey} is set while no translation value or color is set";
        log.Error ($"IsValid: {message}");
        errorList.Add (message);
      }

      if (!string.IsNullOrEmpty (this.DefaultNotRunningProductionStateTranslationKey)
        && (string.IsNullOrEmpty (this.DefaultNotRunningProductionStateTranslationValue)
        || string.IsNullOrEmpty (this.DefaultNotRunningProductionStateColor))) {
        string message = $"a default not running production state translation key {DefaultRunningProductionStateTranslationKey} is set while no translation value or color is set";
        log.Error ($"IsValid: {message}");
        errorList.Add (message);
      }

      // TODO: check colors

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Plugin.RunningMachineModeIsProduction.IsValidConfiguration")) {
          if (0 != this.RunningProductionStateId) { // Else consider the default running production state
            if (this.RunningProductionStateId <= 0) {
              string message = $"invalid running production state id {this.RunningProductionStateId}: not strictly positive";
              log.Error ($"IsValid: {message}");
              errorList.Add (message);
            }
            else {
              var productionState = ModelDAOHelper.DAOFactory.ProductionStateDAO
                .FindById (this.RunningProductionStateId);
              if (null == productionState) {
                string message = $"invalid running production state id {this.RunningProductionStateId}: unknown production state";
                log.Error ($"IsValid: {message}");
                errorList.Add (message);
              }
            }
          }
          if (0 != this.NotRunningProductionStateId) { // Else consider the default not running production state
            if (this.NotRunningProductionStateId <= 0) {
              string message = $"invalid not running production state id {this.NotRunningProductionStateId}: not strictly positive";
              log.Error ($"IsValid: {message}");
              errorList.Add (message);
            }
            else {
              var productionState = ModelDAOHelper.DAOFactory.ProductionStateDAO
                .FindById (this.NotRunningProductionStateId);
              if (null == productionState) {
                string message = $"invalid not running production state id {this.NotRunningProductionStateId}: unknown production state";
                log.Error ($"IsValid: {message}");
                errorList.Add (message);
              }
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
    #endregion // Constructors
  }
}
