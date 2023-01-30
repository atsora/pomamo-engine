// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Response DTO for OperationCycleDTO
  /// </summary>
  public class OperationCycleDTO
  {
    
    /// <summary>
    /// Id of OperationCycle
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Begin of OperationCycle
    /// </summary>
    public string Begin { get; set; }
    
    /// <summary>
    /// End of OperationCycle
    /// </summary>
    public string End { get; set; }
    
    /// <summary>
    /// Offset duration of OperationCycle
    /// </summary>
    public Nullable<Double> OffsetDuration { get; set; }

    /// <summary>
    /// Status of OperationCycle
    /// </summary>
    public string Status { get; set; }

      
  }
}
