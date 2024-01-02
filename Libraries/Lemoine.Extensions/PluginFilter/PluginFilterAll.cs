// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Interfaces;

namespace Lemoine.Extensions.PluginFilter
{
  /// <summary>
  /// PluginFilterAll: accept no plugin, filter them all
  /// </summary>
  public class PluginFilterAll : IPluginFilter
  {
    readonly ILog log = LogManager.GetLogger (typeof (PluginFilterAll).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public PluginFilterAll ()
    {
    }

    public bool IsMatch (IPluginDll pluginDll)
    {
      return false;
    }
  }
}
