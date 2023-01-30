// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Extensions.Business.Config
{
  /// <summary>
  /// Extension to create custom config readers
  /// </summary>
  public interface IConfigExtension : IExtension
  {
    /// <summary>
    /// If true is returned, the extension is active
    /// </summary>
    /// <returns>false: extension not active</returns>
    bool Initialize ();

    /// <summary>
    /// Priority (config readers with a higher priority are loaded first)
    /// </summary>
    double Priority { get; }

    /// <summary>
    /// Get a new config key
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <exception cref="Lemoine.Info.ConfigKeyNotFoundException"></exception>
    /// <returns></returns>
    T Get<T> (string key);
  }
}
