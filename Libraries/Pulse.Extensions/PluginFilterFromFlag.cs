// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.Extensions;
using Lemoine.Extensions.Interfaces;
using Lemoine.Model;
using Pulse.Extensions.Plugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Pulse.Extensions
{
  /// <summary>
  /// Filter the plugin with a <see cref="PluginFlag"/>
  /// </summary>
  public class PluginFilterFromFlag: Lemoine.Extensions.Interfaces.IPluginFilter
  {
    readonly ILog log = LogManager.GetLogger (typeof (PluginFilterFromFlag).FullName);

    readonly PluginFlag m_pluginFlag;

    /// <summary>
    /// Plugin flag
    /// </summary>
    public PluginFlag PluginFlag => m_pluginFlag;

    /// <summary>
    /// Constructor
    /// </summary>
    public PluginFilterFromFlag (PluginFlag pluginFlag)
    {
      m_pluginFlag = pluginFlag;
     }

    /// <summary>
    /// <see cref="IPluginFilter"/>
    /// </summary>
    /// <param name="pluginDll"></param>
    /// <returns></returns>
    public bool IsMatch (IPluginDll pluginDll)
    {
      if (pluginDll is IFlaggedPlugin flaggedPlugin) {
        return flaggedPlugin.Flags.HasFlag (m_pluginFlag);
      }
      else {
        log.Warn ($"IsMatch: plugin which is not an IFlaggedPlugin => return true");
        return true;
      }
    }
  }

  /// <summary>
  /// Extensions to <see cref="IPluginFilter"/>
  /// </summary>
  public static class PluginFilterExtensions
  {
    /// <summary>
    /// Get the associated plugin flag if the the plugin filter is a <see cref="PluginFilterFromFlag"/> else return null
    /// </summary>
    /// <param name="pluginFilter"></param>
    /// <returns></returns>

    public static PluginFlag? GetPluginFlag (this IPluginFilter pluginFilter)
    {
      if (pluginFilter is PluginFilterFromFlag pluginFilterFromFlag) {
        return pluginFilterFromFlag.PluginFlag;
      }
      else {
        return null;
      }
    }
  }
}
