// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2023 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Core.Log;
using Lemoine.Extensions.Interfaces;

namespace Lemoine.Extensions
{
  /// <summary>
  /// Class describing the context of an extension:
  /// - the environment
  /// </summary>
  public class PluginContext: IPluginContext
  {
    static readonly ILog log = LogManager.GetLogger (typeof (PluginContext).FullName);

    /// <summary>
    /// Dll path of the plugin (set by the dll loader)
    /// </summary>
    public string DllPath { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public PluginContext () { }
  }
}
