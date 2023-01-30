// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Pulse.Extensions.Plugin
{
  /// <summary>
  /// Extensions to the reason source
  /// </summary>
  public static class PluginFlagExtensions
  {
    static ILog log = LogManager.GetLogger (typeof (PluginFlagExtensions).ToString ());

    /// <summary>
    /// Check if other is included into t
    /// </summary>
    /// <param name="t"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public static bool HasFlag (this PluginFlag t, PluginFlag other)
    {
      return other == (t & other);
    }
  }

  /// <summary>
  /// 
  /// </summary>
  public interface IFlaggedPlugin
  {
    /// <summary>
    /// Flags characterizing a plugin
    /// </summary>
    PluginFlag Flags { get; }
  }
}
