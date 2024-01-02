// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Extensions.Interfaces
{
  /// <summary>
  /// Interface to implement to provide a plugin synchronization timeout
  /// </summary>
  public interface IPluginSynchronizationTimeoutProvider
  {
    /// <summary>
    /// Plugin synchronization timeout
    /// </summary>
    TimeSpan? PluginSynchronizationTimeout { get; }
  }
}
