// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

namespace Lemoine.Extensions.AutoReason
{
  /// <summary>
  /// Abstract class for an auto-reason configuration
  /// </summary>
  public abstract class AutoReasonConfiguration
    : Pulse.Extensions.Configuration.Implementation.ConfigurationWithMachineFilter
    , Pulse.Extensions.Configuration.IConfigurationWithMachineFilter
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AutoReasonConfiguration).FullName);

    /// <summary>
    /// Reason score
    /// </summary>
    [PluginConf ("DoubleAsNumericUpDown", "Reason score", Description = "reason score. Default: 90.0", Parameters = "100000:3", Multiple = false, Optional = false)]
    [DefaultValue (90.0)]
    public double ReasonScore { get; set; } = 90.0;

    /// <summary>
    /// Manual score if applicable
    /// </summary>
    [PluginConf ("DoubleAsNumericUpDown", "Manual score", Description = "optionally a manual score", Parameters = "100000:3", Multiple = false, Optional = true)]
    public double? ManualScore { get; set; }

    /// <summary>
    /// Associated reason id. If 0, consider a default reason
    /// </summary>
    [PluginConf ("Reason", "Reason", Description = "reason", Multiple = false, Optional = true)]
    public int ReasonId { get; set; }

    /// <summary>
    /// Default associated reason translation key.
    /// 
    /// If null or empty, a default reason translation key is set in the plugin
    /// </summary>
    [PluginConf ("Text", "Default reason translation key", Description = "optionally a default reason translation key in case no reason is set", Multiple = false, Optional = true)]
    public string DefaultReasonTranslationKey { get; set; }

    /// <summary>
    /// Default associated reason translation value.
    /// 
    /// If null or empty, a default reason translation value is set in the plugin
    /// </summary>
    [PluginConf ("Text", "Default reason translation value", Description = "optionally a default reason translation value in case no reason is set", Multiple = false, Optional = true)]
    public string DefaultReasonTranslationValue { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    public AutoReasonConfiguration ()
    {
    }

    #region ConfigurationWithMachineFilter implementation
    /// <summary>
    /// By default, the machine filter parameter is not required
    /// </summary>
    /// <returns></returns>
    protected override bool IsMachineFilterRequired ()
    {
      return false;
    }
    #endregion // ConfigurationWithMachineFilter implementation

    #region IConfiguration implementation
    /// <summary>
    /// <see cref="Lemoine.Extensions.Configuration.IConfiguration"/>
    /// </summary>
    /// <param name="errors"></param>
    /// <returns></returns>
    public override bool IsValid (out IEnumerable<string> errors)
    {
      bool result = true;

      IEnumerable<string> baseErrors;
      result &= base.IsValid (out baseErrors);

      var errorList = new List<string> ();

      if (this.ReasonScore < 0.0) {
        string message = string.Format ("invalid reason score {0}: negative", this.ReasonScore);
        log.ErrorFormat ("IsValid: {0}", message);
        errorList.Add (message);
      }
      if (this.ManualScore.HasValue && this.ManualScore.Value < 0.0) {
        string message = string.Format ("invalid manual score {0}: negative", this.ReasonScore);
        log.ErrorFormat ("IsValid: {0}", message);
        errorList.Add (message);
      }

      if (!string.IsNullOrEmpty (this.DefaultReasonTranslationKey)
        && string.IsNullOrEmpty (this.DefaultReasonTranslationValue)) {
        string message = string.Format ("a default reason translation key {0} is set while no translation value is set", this.DefaultReasonTranslationKey);
        log.ErrorFormat ("IsValid: {0}", message);
        errorList.Add (message);
      }

      if (string.IsNullOrEmpty (this.DefaultReasonTranslationKey)
        && !string.IsNullOrEmpty (this.DefaultReasonTranslationValue)) {
        string message = string.Format ("a default reason translation value {0} is set while no translation key is set", this.DefaultReasonTranslationValue);
        log.ErrorFormat ("IsValid: {0}", message);
        errorList.Add (message);
      }

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Plugin.AutoReason.IsValidConfiguration")) {
          if (0 != this.ReasonId) { // Else consider the default reason
            if (this.ReasonId <= 0) {
              string message = string.Format ("invalid reason id {0}: not strictly positive", this.ReasonId);
              log.ErrorFormat ("IsValid: {0}", message);
              errorList.Add (message);
            }
            else {
              var reason = ModelDAOHelper.DAOFactory.ReasonDAO
                .FindById (this.ReasonId);
              if (null == reason) {
                string message = string.Format ("invalid reason id {0}: unknown reason", this.ReasonId);
                log.ErrorFormat ("IsValid: {0}", message);
                errorList.Add (message);
              }
            }
          }
        }
      }
      result &= (0 == errorList.Count);

      errors = errorList.Concat (baseErrors);
      return result;
    }
    #endregion // IConfiguration implementation
  }
}
