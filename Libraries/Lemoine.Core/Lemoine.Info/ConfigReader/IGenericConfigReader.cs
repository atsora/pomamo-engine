// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Info.ConfigReader
{
  /// <summary>
  /// Interface to get a generic configuration value from a string key
  /// </summary>
  public interface IGenericConfigReader
  {
    /// <summary>
    /// Get a configuration value of a specified type
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    /// <exception cref="ConfigKeyNotFoundException">The configuration key is not found</exception>
    T Get<T> (string key);
  }
}
