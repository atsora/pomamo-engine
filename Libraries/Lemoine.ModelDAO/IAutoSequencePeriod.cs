// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Period that corresponds to a auto-sequence process
  /// </summary>
  public interface IAutoSequencePeriod
  {
    /// <summary>
    /// Begin UTC date/time
    /// </summary>
    DateTime Begin { get; }
    
    /// <summary>
    /// End UTC date/time
    /// </summary>
    DateTime End { get; set; }
    
    /// <summary>
    /// Range [Begin,End)
    /// </summary>
    UtcDateTimeRange Range { get; }
    
    /// <summary>
    /// Is the current mode an auto sequence mode ?
    /// </summary>
    bool AutoSequence { get; }
  }
}
