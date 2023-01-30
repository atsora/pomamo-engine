// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// DTO for sequence state information
  /// </summary>
  public class SequenceStateDTO
  {
    /// <summary>
    /// Sequence Id
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Total number of sequences of the corresponding path
    /// </summary>
    public int Total { get; set; }
    
    /// <summary>
    /// Currently running sequence display
    /// </summary>
    public string Display { get; set; }
    
    /// <summary>
    /// Currently running sequence order in operation
    /// </summary>
    public int Order { get; set; }
    
    /// <summary>
    /// Duration until next sequence in seconds (can be negative), null if no next sequence
    /// </summary>
    public Nullable<int> UntilNext { get; set; }
  }
}
