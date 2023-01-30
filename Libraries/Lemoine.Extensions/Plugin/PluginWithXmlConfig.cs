// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Extensions;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Extensions.Configuration;

namespace Lemoine.Extensions.Plugin
{
  /// <summary>
  /// Description of Plugin.
  /// </summary>
  public abstract class PluginWithXmlConfig<TConfiguration> : PluginWithConfig<TConfiguration>
    where TConfiguration : Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (PluginWithXmlConfig<TConfiguration>).FullName);

    /// <summary>
    /// Constructor: the configuration loader is mandatory
    /// </summary>
    /// <param name="configurationLoader"></param>
    public PluginWithXmlConfig (IConfigurationLoader<TConfiguration> configurationLoader)
      : base (configurationLoader)
    {
    }
  }
}
