// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Interfaces;

namespace Lemoine.Extensions.Plugin
{
  /// <summary>
  /// PluginSynchronizationTimeoutProviderFromConfigSet
  /// </summary>
  public class PluginSynchronizationTimeoutProviderFromConfigSet: IPluginSynchronizationTimeoutProvider
  {
    readonly ILog log = LogManager.GetLogger (typeof (PluginSynchronizationTimeoutProviderFromConfigSet).FullName);

    const string PLUGIN_SYNCHRONIZATION_TIMEOUT_KEY = "Plugin.Synchronization.Timeout";

    readonly TimeSpan? m_pluginSynchronizationTimeoutDefault;

    /// <summary>
    /// <see cref="IPluginSynchronizationTimeoutProvider"/>
    /// </summary>
    public TimeSpan? PluginSynchronizationTimeout => Lemoine.Info.ConfigSet.LoadAndGet (PLUGIN_SYNCHRONIZATION_TIMEOUT_KEY, m_pluginSynchronizationTimeoutDefault);

    /// <summary>
    /// Constructor
    /// </summary>
    public PluginSynchronizationTimeoutProviderFromConfigSet (TimeSpan? pluginSynchronizationTimeoutDefault = null)
    {
      m_pluginSynchronizationTimeoutDefault = pluginSynchronizationTimeoutDefault;
    }

  }
}
