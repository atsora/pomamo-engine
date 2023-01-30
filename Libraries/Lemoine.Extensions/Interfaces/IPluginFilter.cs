// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Extensions.Interfaces
{
  /// <summary>
  /// Interface to filter plugins to load
  /// </summary>
  public interface IPluginFilter
  {
    /// <summary>
    /// Does the plugin match the filter
    /// </summary>
    /// <param name="pluginDll"></param>
    /// <returns></returns>
    bool IsMatch (IPluginDll pluginDll);
  }
}
