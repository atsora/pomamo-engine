// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Extensions;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.Extensions.Plugin
{
  /// <summary>
  /// Description of Plugin.
  /// </summary>
  public abstract class PluginNoConfig : GenericPluginDll
  {
    static readonly ILog log = LogManager.GetLogger (typeof (PluginNoConfig).FullName);

    /// <summary>
    /// Multiple configurations
    /// </summary>
    public override bool MultipleConfigurations => false;

    /// <summary>
    /// Check the consistency of the properties for the plugin to run
    /// </summary>
    /// <param name="configurationText"></param>
    /// <returns>Can be null or empty if there are no errors</returns>
    public override IEnumerable<string> GetConfigurationErrors (string configurationText) => null;
  }
}
