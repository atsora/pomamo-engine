// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// Stamping configuration
  /// </summary>
  public interface IStampingConfigByName : IVersionable, Lemoine.Collections.IDataWithId
  {
    /// <summary>
    /// Name of the configuration (case insensitive)
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Stamping config
    /// </summary>
    StampingConfig Config { get; set; }
  }
}
