// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Interfaces;
using Lemoine.Extensions.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lemoine.Extensions.Configuration.GuiBuilder
{
  /// <summary>
  /// Interface to implement for CustomActionGuiBuilder
  /// </summary>
  public interface ICustomActionGuiBuilder
    : IPluginCustomActionControl
  {
    /// <summary>
    /// Set the associated config control
    /// </summary>
    /// <param name="pluginConfig"></param>
    void SetConfigControl (IPluginConfig pluginConfig);
  }
}
