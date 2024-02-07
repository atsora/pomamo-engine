// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;

namespace Lemoine.Extensions.Analysis
{
  /// <summary>
  /// Interface to add features when an operation is detected
  /// </summary>
  public interface IOperationDetectionExtension: IDetectionExtension
  {
    /// <summary>
    /// This method must return true so that
    /// the previous operation slot is retrieved and may be used in AddOperation method
    /// </summary>
    /// <returns></returns>
    bool IsPreviousOperationSlotRequired ();
    
    /// <summary>
    /// Add an operation
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="operation">not null</param>
    /// <param name="range"></param>
    /// <param name="effectiveBegin">Effective begin. May be before range.Lower in case of an auto-operation</param>
    /// <param name="previousOperationSlot">nullable</param>
    void AddOperation (IMonitoredMachine machine, IOperation operation, UtcDateTimeRange range,
                       LowerBound<DateTime> effectiveBegin,
                       IOperationSlot previousOperationSlot);

    /// <summary>
    /// Stop an operation
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="dateTime"></param>
    void StopOperation (IMonitoredMachine machine, DateTime dateTime);
  }
}
