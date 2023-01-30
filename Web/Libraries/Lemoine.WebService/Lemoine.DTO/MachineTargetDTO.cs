// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Response DTO for Machine Display and utilization target percentage
  /// </summary>
  public class MachineTargetDTO
  {
    /// <summary>
    /// Id of machine
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Display (name) of machine
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Target utilization percentage for machine (between 0.0 and 1.0)
    /// </summary>
    public double TargetPercentage { get; set; }
  }
}
