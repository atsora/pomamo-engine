// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Response DTO for ComponentDTO
  /// </summary>
  public class ComponentDTO
  {
    /// <summary>
    /// Component id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Component display
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// ComponentType id
    /// </summary>
    public int TypeId { get; set; }
    
    /// <summary>
    /// Project id
    /// </summary>
    public int ProjectId { get; set; }
    
    /// <summary>
    /// Final work piece id
    /// </summary>
    public int? FinalWorkPieceId { get; set; }
    
    /// <summary>
    /// Estimated hours
    /// </summary>
    public double? EstimatedHours { get; set; }

  }
}
