// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Info.ConfigReader
{
  /// <summary>
  /// Persistent config writer
  /// </summary>
  public interface IPersistentConfigWriter
  {
    /// <summary>
    /// Set a persistent config
    /// </summary>
    /// <param name="key"></param>
    /// <param name="v"></param>
    /// <param name="overwrite"></param>
    /// <returns>false when the key already existed and overwrite was false</returns>
    bool SetPersistentConfig<T> (string key, T v, bool overwrite = false);

    /// <summary>
    /// Reset a persistent config
    /// </summary>
    /// <param name="key"></param>
    void ResetPersistentConfig (string key);
  }
}
