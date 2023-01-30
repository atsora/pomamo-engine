// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lemoine.Extensions.Interfaces;

namespace Lemoine.Extensions.Configuration.GuiBuilder
{
  /// <summary>
  /// Interface to implement for the PluginConfigurationBuilder
  /// </summary>
  public interface IConfigurationGuiBuilder
    : IPluginConfigurationControl
  {
    /// <summary>
    /// Set the associated config control
    /// </summary>
    /// <param name="pluginConfig"></param>
    void SetConfigControl (IPluginConfig pluginConfig);
  }
}
