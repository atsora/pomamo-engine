// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Response DTO for OperationDTO
  /// </summary>
  public class OperationDTO
  {
    
    /// <summary>
    /// Id
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Display (name)
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Estimated machining duration
    /// </summary>
    public int? MachiningDuration { get; set; }
    
    /// <summary>
    /// Estimated setup duration
    /// </summary>
    public int? SetUpDuration { get; set; }
    
    /// <summary>
    /// Estimated tear down duration
    /// </summary>
    public int? TearDownDuration { get; set; }

    /// <summary>
    /// Estimated loading duration
    /// </summary>
    public int? LoadingDuration { get; set; }

    /// <summary>
    /// Estimated unloading duration
    /// </summary>
    public int? UnloadingDuration { get; set; }    
  }
}
