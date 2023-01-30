// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table AutoSequence.
  /// </summary>
  public interface IAutoSequence: IVersionable, IPartitionedByMachineModule
  {
    /// <summary>
    /// Associated auto-sequence
    /// </summary>
    ISequence Sequence { get; }
    
    /// <summary>
    /// Associated operation
    /// </summary>
    IOperation Operation { get; }
    
    /// <summary>
    /// Raw begin date/time of the auto-sequence period
    /// without considering the activities
    /// </summary>
    DateTime Begin { get; set; }
    
    /// <summary>
    /// Raw end date/time of the auto-sequence period
    /// without considering the activities
    /// </summary>
    UpperBound<DateTime> End { get; set; }
    
    /// <summary>
    /// Range [Begin,End)
    /// </summary>
    UtcDateTimeRange Range { get; }
  }
}
