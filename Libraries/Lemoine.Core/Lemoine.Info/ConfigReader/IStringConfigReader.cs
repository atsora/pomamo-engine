// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.Info.ConfigReader
{
  /// <summary>
  /// Interface to get a string configuration from a configuration key
  /// </summary>
  public interface IStringConfigReader
  {
    /// <summary>
    /// Get a string configuration
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    /// <exception cref="ConfigKeyNotFoundException">The configuration key is not found</exception>
    string GetString (string key);
  }
}
