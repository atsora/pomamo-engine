// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model for table autoreasonstate
  /// </summary>
  public interface IAutoReasonState : IVersionable, IPartitionedByMonitoredMachine
  {
    /// <summary>
    /// Application state key
    /// 
    /// Use the '.' separator to structure the key
    /// </summary>
    string Key { get; }

    /// <summary>
    /// Application state value, a serializable object
    /// </summary>
    object Value { get; set; }
  }
}
