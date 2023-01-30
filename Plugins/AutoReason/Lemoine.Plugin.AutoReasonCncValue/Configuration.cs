// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Lemoine.Plugin.AutoReasonCncValue
{
  public sealed class Configuration
    : Lemoine.Extensions.AutoReason.AutoReasonConfiguration
    , Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters
    /// <summary>
    /// Lambda condition
    /// </summary>
    [PluginConf ("Text", "Lambda condition", Description = "A lambda expression object => bool to define when a cnc value triggers an auto-reason", Optional = false)]
    public string LambdaCondition { get; set; }

    /// <summary>
    /// Dynamic end
    /// </summary>
    [PluginConf ("Text", "Dynamic start", Description = "Optional dynamic start definition. Any FieldId and MachineModuleId strings are replaced by their value", Optional = true)]
    [DefaultValue ("")]
    public string DynamicStart { get; set; } = "";

    /// <summary>
    /// Dynamic end
    /// </summary>
    [PluginConf ("Text", "Dynamic end", Description = "Dynamic end definition. Any FieldId and MachineModuleId strings are replaced by their value", Optional = false)]
    public string DynamicEnd { get; set; }

    /// <summary>
    /// Field
    /// </summary>
    [PluginConf ("Field", "Field", Description = "Field to match", Multiple = false, Optional = false)]
    public int FieldId { get; set; }

    /// <summary>
    /// Regex to filter the machine module based on the prefix
    /// </summary>
    [PluginConf ("Text", "Machine module prefix regex", Description = "(optional) Regex to filter the machine modules based on the config prefix", Optional = true)]
    [DefaultValue ("")]
    public string MachineModulePrefixRegex { get; set; } = "";
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public Configuration ()
    {
    }
    #endregion // Constructors

    #region IConfiguration implementation
    /// <summary>
    /// <see cref="Lemoine.Extensions.Configuration.IConfiguration"/>
    /// </summary>
    /// <param name="errors"></param>
    /// <returns></returns>
    public override bool IsValid (out IEnumerable<string> errors)
    {
      var baseResult = base.IsValid (out errors);
      var errorList = new List<string> ();

      if (string.IsNullOrEmpty (this.LambdaCondition)) {
        log.ErrorFormat ("IsValid: no lambda condition parameter");
        errorList.Add ("No lambda condition");
      }
      if (string.IsNullOrEmpty (this.DynamicEnd)) {
        log.ErrorFormat ("IsValid: no dynamic end parameter");
        errorList.Add ("No dynamic end");
      }
      if (!string.IsNullOrEmpty (this.MachineModulePrefixRegex)) {
        try {
          var regex = new Regex (this.MachineModulePrefixRegex);
        }
        catch (ArgumentException ex) {
          log.Error ($"IsValid: invalid regex {this.MachineModulePrefixRegex}", ex);
          errorList.Add ("Invalid regex for machine mdoule prefix");
        }
      }

      errors = errors.Concat (errorList);
      return baseResult && !errors.Any ();
    }
    #endregion // IConfiguration implementation
  }
}
