// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Info.ConfigReader
{
  /// <summary>
  /// Object that can contains a custom ListConfigReader
  /// </summary>
  public interface IListConfigurable
  {
    /// <summary>
    /// Set a config reader
    /// </summary>
    /// <param name="listConfigReader"></param>
    void SetListConfigReader (IListConfigReader listConfigReader);
  }
}
