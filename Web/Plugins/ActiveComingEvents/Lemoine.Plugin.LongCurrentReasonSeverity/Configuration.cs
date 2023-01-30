// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;

namespace Lemoine.Plugin.LongCurrentReasonSeverity
{
  public sealed class Configuration
    : Pulse.Extensions.Configuration.Implementation.ConfigurationWithMachineFilter
    , Pulse.Extensions.Configuration.IConfigurationWithMachineFilter
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters
    /// <summary>
    /// Include periods where the running status is unknown
    /// </summary>
    [PluginConf ("Bool", "Include unknown", Description = "Include periods where the running status is unknown. Default: true", Multiple = false, Optional = true)]
    [DefaultValue (true)]
    public bool IncludeUnknown
    {
      get; set;
    } = true;

    /// <summary>
    /// Warning delay
    /// </summary>
    [PluginConf ("DurationPicker", "Warning2 delay", Description = "Delay before increasing the severity to Warning2", Multiple = false, Optional = true)]
    public TimeSpan? Warning2Delay
    {
      get; set;
    }

    /// <summary>
    /// Error delay
    /// </summary>
    [PluginConf ("DurationPicker", "Error delay", Description = "Delay before increasing the severity to Error", Multiple = false, Optional = true)]
    public TimeSpan? ErrorDelay
    {
      get; set;
    }
    #endregion // Getters / Setters

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
