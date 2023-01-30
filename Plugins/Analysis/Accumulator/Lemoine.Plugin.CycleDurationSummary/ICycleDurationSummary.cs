// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Model;
using System;

namespace Lemoine.Plugin.CycleDurationSummary
{
  /// <summary>
  /// Description of CycleDurationSummary.
  /// </summary>
  public interface ICycleDurationSummary : IVersionable, IPartitionedByMachine
  {
    /// <summary>
    /// Associated day
    /// </summary>
    DateTime Day { get; }

    /// <summary>
    /// Shift that is associated to the operation slot
    /// </summary>
    IShift Shift { get; }

    /// <summary>
    /// Associated work order
    /// </summary>
    IWorkOrder WorkOrder { get; }

    /// <summary>
    /// Associated line
    /// </summary>
    ILine Line { get; }

    /// <summary>
    /// Associated task
    /// </summary>
    ITask Task { get; }

    /// <summary>
    /// Associated component
    /// </summary>
    IComponent Component { get; }

    /// <summary>
    /// Associated operation
    /// </summary>
    IOperation Operation { get; }

    /// <summary>
    /// Associated offset (rounded to the closest integer)
    /// </summary>
    int Offset { get; }

    /// <summary>
    /// Number of full cycles
    /// </summary>
    int Number { get; set; }

    /// <summary>
    /// Number of partial cycles
    /// </summary>
    int Partial { get; set; }
  }
}
