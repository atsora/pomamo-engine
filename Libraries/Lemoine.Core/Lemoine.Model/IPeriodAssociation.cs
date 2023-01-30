// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Description of IPeriodAssociation.
  /// </summary>
  public interface IPeriodAssociation
  {
    /// <summary>
    /// UTC begin date/time of the association
    /// </summary>
    LowerBound<DateTime> Begin { get; set; }

    /// <summary>
    /// UTC end date/time of the association
    /// </summary>
    UpperBound<DateTime> End { get; set; }
    
    /// <summary>
    /// UTC date/time range
    /// </summary>
    UtcDateTimeRange Range { get; }
  }
}
