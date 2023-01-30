// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Stamping
{
  /// <summary>
  /// Get the time settings for the sequence duration and the milestones
  /// </summary>
  public interface ITimeConfig
  {
    /// <summary>
    /// Get an adjusting time factor for the milestones and the sequence duration
    /// 
    /// No adjusting factor means 1.0
    /// </summary>
    /// <returns></returns>
    double GetTimeFactor ();

    /// <summary>
    /// Get how much often is written a milestone
    /// </summary>
    /// <returns></returns>
    TimeSpan GetMilestoneTriggerFrequency ();
  }
}
