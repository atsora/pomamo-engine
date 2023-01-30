// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Info.ConfigReader
{
  /// <summary>
  /// Description of IListStringConfigReader.
  /// </summary>
  public interface IListStringConfigReader
  {
    /// <summary>
    /// Get a list of configurations for a specified key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    IEnumerable<IStringConfigReader> GetStringConfigs (string key);
  }
}
