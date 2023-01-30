// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Response DTO for NonConformanceReasonDTO
  /// </summary>
  public class NonConformanceReasonDTO
  {
    /// <summary>
    /// Id of nonconformance reason
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of nonconformance reason
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Description of nonconformance reason
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Are nonconformance reason details required ?
    /// </summary>
    public bool DetailsRequired { get; set; }

  }
}
