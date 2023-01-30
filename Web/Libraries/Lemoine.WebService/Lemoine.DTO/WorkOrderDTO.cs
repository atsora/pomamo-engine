// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Response DTO for WorkOrderDTO
  /// </summary>
  public class WorkOrderDTO
  {
    /// <summary>
    /// WorkOrder id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// WorkOrder Display (name)
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Delivery Date
    /// </summary>
    public DateTime? DeliveryDate { get; set; }

    /// <summary>
    /// WorkOrder status id
    /// </summary>
    public int StatusId { get; set; }

  }
}
