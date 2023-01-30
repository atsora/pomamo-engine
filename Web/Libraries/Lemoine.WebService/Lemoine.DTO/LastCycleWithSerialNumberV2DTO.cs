// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Response DTO  for last cycle with serial number(V2).
  /// </summary>
  public class LastCycleWithSerialNumberV2DTO
  {
    /// <summary>
    /// Last cycle id if any (-1 if not last cycle)
    /// </summary>
    public int CycleId { get; set; }
    
    /// <summary>
    /// Begin of operation cycle in ISO format
    /// </summary>
    public string Begin { get; set; }
    
    /// <summary>
    /// End of operation cycle in ISO format
    /// </summary>
    public string End { get; set; }

    /// <summary>
    /// Last serial number if any ("-1" if no last cycle, "0" if last cycle but missing number)
    /// </summary>
    public string SerialNumber { get; set; }
    
    /// <summary>
    /// True if there exits no last serial number on machine
    /// </summary>
    public bool DataMissing { get; set; }
    
    /// <summary>
    /// Estimated Begin
    /// </summary>
    public Nullable<bool> EstimatedBegin { get; set; }
    
    /// <summary>
    /// Estimated End
    /// </summary>
    public Nullable<bool> EstimatedEnd { get; set; }

  }
}
