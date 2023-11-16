// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Plugin.DefaultReasonMinimalConfig
{
  public sealed class Configuration
    : Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    /// <summary>
    /// Parameter to turn off the plugin
    /// </summary>
    [PluginConf ("Bool", "Turn off", Description = "If this parameter is on on one of the configurations, then the plugin is turned off", Multiple = false, Optional = false)]
    [DefaultValue (false)]
    public bool TurnOff { get; set; } = false;

    /// <summary>
    /// Reason score
    /// </summary>
    [PluginConf ("DoubleAsNumericUpDown", "Reason score", Description = "reason score. Default: 2.0", Parameters = "100000:3", Multiple = false, Optional = false)]
    [DefaultValue (2.0)]
    public double ReasonScore { get; set; } = 2.0;

    /// <summary>
    /// Active reason id. Default: Motion (id=2). If 0, consider a default reason
    /// </summary>
    [PluginConf ("Reason", "Active reason", Description = "Active reason", Multiple = false, Optional = false)]
    [DefaultValue ((int)ReasonId.Motion)]
    public int ActiveReasonId { get; set; } = (int)ReasonId.Motion;

    /// <summary>
    /// Inactive short reason id. Default: Short (id=3). If 0, consider a default reason
    /// </summary>
    [PluginConf ("Reason", "Inactive short reason", Description = "Inactive short reason", Multiple = false, Optional = false)]
    [DefaultValue ((int)ReasonId.Short)]
    public int InactiveShortReasonId { get; set; } = (int)ReasonId.Short;

    /// <summary>
    /// Maximum short duration. Default: 10 minutes
    /// </summary>
    [PluginConf ("DurationPicker", "Maximum short duration", Description = "maximum short duration. Default: 10 min", Parameters = "0:00:00", Multiple = false, Optional = true)]
    public TimeSpan? MaximumShortDuration { get; set; } = TimeSpan.FromMinutes (10);

    /// <summary>
    /// Inactive long reason id. If 0, consider a default reason
    /// </summary>
    [PluginConf ("Reason", "Inactive long Reason", Description = "Inactive long reason", Multiple = false, Optional = true)]
    public int InactiveLongReasonId { get; set; }

    /// <summary>
    /// Inactive long default associated reason translation key.
    /// 
    /// If null or empty, a default reason translation key is set in the plugin
    /// </summary>
    [PluginConf ("Text", "Inactive long default reason translation key", Description = "optionally a default reason translation key in case no reason is set", Multiple = false, Optional = true)]
    public string InactiveLongDefaultReasonTranslationKey { get; set; }

    /// <summary>
    /// Inactive long default associated reason translation value.
    /// 
    /// If null or empty, a default reason translation value is set in the plugin
    /// </summary>
    [PluginConf ("Text", "Inactive long default reason translation value", Description = "optionally a default reason translation value in case no reason is set", Multiple = false, Optional = true)]
    public string InactiveLongDefaultReasonTranslationValue { get; set; }

    /// <summary>
    /// Overwrite required flag for inactive long periods
    /// </summary>
    [PluginConf ("Bool", "Overwrite required flag for inactive long periods", Description = "For inactive long reason, should the flag Overwrite Required be set ?", Multiple = false, Optional = false)]
    [DefaultValue (false)]
    public bool OverwriteRequired { get; set; } = false;

    /// <summary>
    /// Unknown reason id. Default: Unknown (id=7). If 0, consider a default reason
    /// </summary>
    [PluginConf ("Reason", "Unknown reason", Description = "Unknown reason", Multiple = false, Optional = false)]
    [DefaultValue ((int)ReasonId.Unknown)]
    public int UnknownReasonId { get; set; } = (int)ReasonId.Unknown;

    /// <summary>
    /// Consider an unknown period is inactive (not recommended)
    /// </summary>
    [PluginConf ("Bool", "Unknown is inactive", Description = "Consider an unknown period is inactive. This is not recommended to turn this option on", Multiple = false, Optional = false)]
    [DefaultValue (false)]
    public bool UnknownIsInactive { get; set; } = false;

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
    public bool IsValid (out IEnumerable<string> errors)
    {
      var errorList = new List<string> ();
      if (this.ReasonScore <= 0.0) {
        log.Error ($"IsValid: reason score {this.ReasonScore} is not valid");
        errorList.Add ("Invalid reason score: negative");
      }
      if (!string.IsNullOrEmpty (this.InactiveLongDefaultReasonTranslationKey)
        && string.IsNullOrEmpty (this.InactiveLongDefaultReasonTranslationValue)) {
        string message = $"a default reason translation key {this.InactiveLongDefaultReasonTranslationKey} is set while no translation value is set";
        log.Error ($"IsValid: {message}");
        errorList.Add (message);
      }

      if (string.IsNullOrEmpty (this.InactiveLongDefaultReasonTranslationKey)
        && !string.IsNullOrEmpty (this.InactiveLongDefaultReasonTranslationValue)) {
        string message = $"a default reason translation value {this.InactiveLongDefaultReasonTranslationValue} is set while no translation key is set";
        log.Error ($"IsValid: {message}");
        errorList.Add (message);
      }

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Plugin.AutoReason.IsValidConfiguration")) {
          ValidateReason (this.ActiveReasonId, errorList, mandatoryId: true);
          ValidateReason (this.InactiveShortReasonId, errorList, mandatoryId: true);
          ValidateReason (this.InactiveLongReasonId, errorList, mandatoryId: false);
          ValidateReason (this.UnknownReasonId, errorList, mandatoryId: true);
        }
      }

      errors = errorList;
      return !errorList.Any ();
    }

    void ValidateReason (int reasonId, IList<string> errorList, bool mandatoryId = false)
    {
      if (mandatoryId || (0 != reasonId)) { // Else consider the default reason
        if (reasonId <= 0) {
          string message = $"invalid reason id {reasonId}: not strictly positive";
          log.Error ($"IsValid: {message}");
          errorList.Add (message);
        }
        else {
          var reason = ModelDAOHelper.DAOFactory.ReasonDAO
            .FindById (reasonId);
          if (reason is null) {
            string message = $"invalid reason id {reasonId}: unknown reason";
            log.Error ($"ValidateReason: {message}");
            errorList.Add (message);
          }
        }
      }
    }
  }
}
