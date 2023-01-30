// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Model;
using System;

namespace Lemoine.Plugin.CycleCountSummary
{
  /// <summary>
  /// Model of table CycleCountSummary
  /// </summary>
  public interface ICycleCountSummary : IVersionable, IPartitionedByMachine
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
    /// Number of full cycles
    /// </summary>
    int Full { get; set; }

    /// <summary>
    /// Number of partial cycles
    /// </summary>
    int Partial { get; set; }
  }
}
