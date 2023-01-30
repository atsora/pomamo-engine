// Copyright (C) 2009-2023 Lemoine Automation Technologies
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
  /// NoPluginFilter: accept all the plugins, no filter
  /// </summary>
  public class NoPluginFilter: IPluginFilter
  {
    readonly ILog log = LogManager.GetLogger (typeof (NoPluginFilter).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public NoPluginFilter ()
    {
    }

    public bool IsMatch (IPluginDll pluginDll)
    {
      return true;
    }
  }
}
