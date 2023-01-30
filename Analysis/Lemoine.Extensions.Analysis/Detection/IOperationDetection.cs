// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;

namespace Lemoine.Extensions.Analysis.Detection
{
  /// <summary>
  /// Interface of OperationDetection process class
  /// </summary>
  public interface IOperationDetection
  {
    /// <summary>
    /// Associated machine
    /// </summary>
    IMonitoredMachine Machine { get; }

    /// <summary>
    /// Start an operation
    /// 
    /// Only begin a 1-second operation slot
    /// 
    /// The top transaction must be created by the parent function
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="dateTime"></param>
    void StartOperation (IOperation operation,
                         DateTime dateTime);

    /// <summary>
    /// Extend an operation (the operation if specified) until the specified date/time
    /// 
    /// The top transaction is set in the parent function
    /// </summary>
    /// <param name="operation">may be null</param>
    /// <param name="dateTime"></param>
    void ExtendOperation (IOperation operation,
                          DateTime dateTime);

    /// <summary>
    /// Add an operation
    /// 
    /// The top transaction is set in the parent function
    /// </summary>
    /// <param name="operation">not null</param>
    /// <param name="range"></param>
    void AddOperation (IOperation operation, UtcDateTimeRange range);
  }
}
