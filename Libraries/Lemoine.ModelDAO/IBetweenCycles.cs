// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table BetweenCycles.
  /// </summary>
  public interface IBetweenCycles: IVersionable, IPartitionedByMachine
  {
    /// <summary>
    /// Begin date/time of the between cycles period
    /// </summary>
    DateTime Begin { get; }
    
    /// <summary>
    /// End date/time of the between cycles period
    /// </summary>
    DateTime End { get; }

    /// <summary>
    /// Reference to the previous operation cycle
    /// 
    /// This field can't be null
    /// </summary>
    IOperationCycle PreviousCycle { get; }
    
    /// <summary>
    /// Reference to the next operation cycle
    /// 
    /// This field can't be null
    /// </summary>
    IOperationCycle NextCycle { get; }    

    /// <summary>
    /// Offset (in percentage) between standard and actual cycle duration
    /// </summary>
    Double? OffsetDuration { get; }
  }
}