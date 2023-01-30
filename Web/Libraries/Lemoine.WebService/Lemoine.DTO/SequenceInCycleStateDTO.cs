// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Description of SequenceInCycleStateDTO.
  /// </summary>
  public class SequenceInCycleStateDTO
  {
    /// <summary>
    /// Sequence display
    /// </summary>
    public string Display { get; set; }
    
    /// <summary>
    /// Sequence kdind
    /// </summary>
    public Lemoine.Model.SequenceKind Kind { get; set; }
    
    /// <summary>
    /// Sequence order
    /// </summary>
    public int Order { get; set; }
    
    /// <summary>
    /// Estimated duration in seconds
    /// </summary>
    public int? EDuration { get; set; }
    
    /// <summary>
    /// Percentage of cycle progress when reaching start of sequence
    /// </summary>
    public int SPercent { get; set; }

    /// <summary>
    /// Percentage of cycle progress when reaching end of sequence
    /// </summary>
    public int EPercent { get; set; }
    
    /// <summary>
    /// Whether or not sequence of operation appears in cycle
    /// </summary>
    public bool IsInCycle { get; set; }
    
    /// <summary>
    /// Percentage of progress of sequence in cycle (0: start of sequence; 100: end)
    /// Null if not(IsInCycle).
    /// </summary>
    public Nullable<int> CPercent { get; set; }
    
    /// <summary>
    /// Difference between estimated duration and real duration of sequence (in seconds).
    /// Null if not(IsInCycle).
    /// </summary>
    public Nullable<int> Late { get; set; }
  }
}
