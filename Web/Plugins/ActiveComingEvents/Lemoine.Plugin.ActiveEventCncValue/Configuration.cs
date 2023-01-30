// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.Core.Log;

namespace Lemoine.Plugin.ActiveEventCncValue
{
  public sealed class Configuration
    : Pulse.Extensions.Configuration.Implementation.ConfigurationWithMachineFilter
    , Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    /// <summary>
    /// Category name
    /// </summary>
    [PluginConf ("Text", "Message", Description = "message of the active event", Multiple = false, Optional = false)]
    public string Message { get; set; }

    /// <summary>
    /// Filter on machine module
    /// 
    /// 0 (default): no filter on machine module is applicable
    /// </summary>
    [PluginConf ("MachineModule", "Machine module Cnc Value", Description = "Machine module on which Cnc Value is read", Parameters = "", Multiple = false, Optional = false)]
    public int MachineModuleId { get; set; }

    /// <summary>
    /// Field
    /// </summary>
    [PluginConf ("Field", "Field", Description = "Field to match", Multiple = false, Optional = false)]
    public int FieldId { get; set; }

    /// <summary>
    /// Field
    /// </summary>
    [PluginConf ("Text", "CncValue", Description = "Cnc value to match", Multiple = false, Optional = false)]
    public string CncValue { get; set; }

    /// <summary>
    /// Warning delay
    /// </summary>
    [PluginConf ("DurationPicker", "Minimum duration", Description = "Minimum duration of cnc value condition", Multiple = false, Optional = true)]
    public TimeSpan? MinimumDelay { get; set; }

    /// <summary>
    /// Lambda condition
    /// </summary>
    [PluginConf ("Text", "Lambda condition", Description = "A lambda expression object => bool to define when a cnc value validate a condition", Optional = true)]
    public string LambdaCondition { get; set; }

    #region Constructors
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
      bool result = true;

      IEnumerable<string> baseErrors;
      result &= base.IsValid (out baseErrors);

      var errorList = new List<string> ();

      // Add here additional parameter validation

      result &= (0 == errorList.Count);

      errors = errorList.Concat (baseErrors);
      return result;
    }

    protected override bool IsMachineFilterRequired ()
    {
      return false;
    }
    #endregion // Constructors

  }
}
