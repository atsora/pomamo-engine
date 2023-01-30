// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Model;

namespace Lemoine.Extensions.Business.Operation
{
  /// <summary>
  /// Counter of a specific operation
  /// </summary>
  public struct CycleCounterValue
  {
    /// <summary>
    /// Associated operation (nullable)
    /// </summary>
    public IOperation Operation { get; private set; }

    /// <summary>
    /// Associated task (nullable)
    /// </summary>
    public ITask Task { get; private set; }

    /// <summary>
    /// Duration the associated operation/task was active during the requested range
    /// </summary>
    public TimeSpan Duration { get; private set; }

    /// <summary>
    /// Number of cycles
    /// </summary>
    public int TotalCycles { get; private set; }

    /// <summary>
    /// Number of adjusted cycles
    /// </summary>
    public int AdjustedCycles { get; private set; }

    /// <summary>
    /// Adjusted quantity
    /// </summary>
    public int AdjustedQuantity { get; private set; }

    /// <summary>
    /// Is this intermediate work piece currently being processed ?
    /// </summary>
    public bool InProgress { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="operation">nullable</param>
    /// <param name="task">nullable</param>
    /// <param name="duration"></param>
    /// <param name="totalCycles"></param>
    /// <param name="adjustedCycles"></param>
    /// <param name="adjustedQuantity"></param>
    /// <param name="inProgress"></param>
    public CycleCounterValue (IOperation operation, ITask task, TimeSpan duration, int totalCycles, int adjustedCycles, int adjustedQuantity, bool inProgress = false)
    {
      this.Operation = operation;
      this.Task = task;
      this.Duration = duration;
      this.TotalCycles = totalCycles;
      this.AdjustedCycles = adjustedCycles;
      this.AdjustedQuantity = adjustedQuantity;
      this.InProgress = inProgress;
    }
  }

  /// <summary>
  /// 
  /// </summary>
  public interface ICycleCounterExtension: IExtension
  {
    /// <summary>
    /// Initialize the plugin. If false is returned, do not consider the plugin
    /// </summary>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    bool Initialize (IMonitoredMachine machine);

    /// <summary>
    /// Score of the extension.
    /// 
    /// Extensions with the highest score are considered first
    /// </summary>
    double Score { get; }

    /// <summary>
    /// Get the number of cycles asynchronously
    /// 
    /// If it can't be computed, an exception may be returned
    /// </summary>
    /// <param name="range"></param>
    /// <param name="preLoadRange">Pre-load range (optional)</param>
    /// <returns></returns>
    System.Threading.Tasks.Task<IEnumerable<CycleCounterValue>> GetNumberOfCyclesAsync (UtcDateTimeRange range, UtcDateTimeRange preLoadRange = null);
  }
}
