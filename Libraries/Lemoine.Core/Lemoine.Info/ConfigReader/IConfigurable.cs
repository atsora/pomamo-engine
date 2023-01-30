// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Info.ConfigReader
{
  /// <summary>
  /// Object that can contains a custom config reader
  /// </summary>
  public interface IConfigurable
  {
    /// <summary>
    /// Set a config reader
    /// </summary>
    /// <param name="configReader"></param>
    void SetConfigReader (IGenericConfigReader configReader);
  }
}
