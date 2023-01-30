// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.Extensions
{
  /// <summary>
  /// Class describing the context of an extension:
  /// - the environment
  /// </summary>
  public class PluginContext
  {
    static readonly ILog log = LogManager.GetLogger (typeof (PluginContext).FullName);

    #region Getters / Setters
    /// <summary>
    /// Dll path of the plugin (set by the dll loader)
    /// </summary>
    public string DllPath { get; set; }

    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    public PluginContext () { }
    #endregion // Constructors

    #region Methods
    #endregion // Methods
  }
}
