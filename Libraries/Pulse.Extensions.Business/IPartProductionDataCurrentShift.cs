// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Model;
using System;
using System.Collections.Generic;

namespace Lemoine.Extensions.Business
{
  /// <summary>
  /// 
  /// </summary>
  public interface IPartProductionDataCurrentShift
  {
    /// <summary>
    /// Date/time of the data
    /// </summary>
    DateTime DateTime { get; }

    /// <summary>
    /// Number of parts completed in the current production of the current shift
    /// </summary>
    double? NbPiecesCurrentShift { get; }

    /// <summary>
    /// Production target in number of parts for the current production of the current shift
    /// </summary>
    double? GoalCurrentShift { get; }

    /// <summary>
    /// Associated component
    /// </summary>
    IComponent Component { get; }

    /// <summary>
    /// Associated operation
    /// </summary>
    IOperation Operation { get; }

    /// <summary>
    /// Associated day
    /// </summary>
    DateTime? Day { get; }

    /// <summary>
    /// Associated shift
    /// </summary>
    IShift Shift { get; }

    /// <summary>
    /// UTC date/time range of the current production (effective operation for the current shift)
    /// </summary>
    UtcDateTimeRange Range { get; }

    /// <summary>
    /// Cycle duration target (machining+loading+adjustment)
    /// </summary>
    TimeSpan? CycleDurationTarget { get; }
  }
}
