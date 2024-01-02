// Copyright (C) 2023 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Extensions.Interfaces
{
  /// <summary>
  /// Class describing the context of an extension:
  /// - the environment
  /// </summary>
  public interface IPluginContext
  {
    /// <summary>
    /// Dll path of the plugin (set by the dll loader)
    /// </summary>
    string DllPath { get; set; }
  }
}
