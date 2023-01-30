// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Stamping
{
  /// <summary>
  /// Line creator to set milestones
  /// </summary>
  public interface IMilestoneStampLineCreator
  {
    /// <summary>
    /// Maximum number of fractional digits to write into the program
    /// </summary>
    int FractionalDigits
    {
      get; set;
    }

    /// <summary>
    /// Create a line that contains the milestone update
    /// 
    /// Usually used with a variable
    /// </summary>
    /// <param name="timeSpan"></param>
    /// <param name="sequenceStamp"></param>
    /// <returns>null if no line must be created</returns>
    string? CreateMilestoneStampLine (TimeSpan timeSpan, double? sequenceStamp = null);

    /// <summary>
    /// Create a line that reset any milestone
    /// </summary>
    /// <param name="sequenceStamp"></param>
    /// <returns>null if no line must be created</returns>
    string? CreateResetMilestoneLine (double? sequenceStamp = null);
  }
}
