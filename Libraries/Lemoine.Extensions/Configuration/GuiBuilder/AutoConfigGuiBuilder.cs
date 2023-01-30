// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Plugin;
using Lemoine.Core.Log;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Lemoine.Extensions.Interfaces;

namespace Lemoine.Extensions.Configuration.GuiBuilder
{
  /// <summary>
  /// Base class to make a new ConfigurationGuiBuilder
  /// </summary>
  /// <typeparam name="TConfiguration"></typeparam>
  public class AutoConfigGuiBuilder<TConfiguration>
    : AutoGuiBuilderWithConfigurationLoader<TConfiguration, TConfiguration>
    , IConfigurationGuiBuilder
    , IPluginConfigurationControl
    where TConfiguration : Lemoine.Extensions.Configuration.IConfiguration, new()
  {
    /// <summary>
    /// <see cref="IPluginConfigurationControl"/>
    /// </summary>
    /// <param name="configurationText"></param>
    public void LoadProperties (string configurationText)
    {
      var configuration = this.ConfigurationLoader.LoadConfiguration (configurationText);
      LoadProperties (configuration);
    }

    /// <summary>
    /// <see cref="IPluginConfigurationControl"/>
    /// </summary>
    /// <returns></returns>
    public string GetProperties ()
    {
      return this.ConfigurationLoader.GetProperties (GetConfiguration ());
    }
  }
}
