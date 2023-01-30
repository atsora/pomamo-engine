// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Pulse.Extensions.Database
{
  /// <summary>
  /// Status of an operation cycle detection
  /// </summary>
  public interface IOperationDetectionStatusExtension : IInitializedByMachineExtension
  {
    /// <summary>
    /// Priority
    /// 
    /// Extensions with the same priority are processed together. The maximum value is considered
    /// </summary>
    int OperationDetectionStatusPriority { get; }

    /// <summary>
    /// Get the date/time status of the operation detection
    /// </summary>
    /// <returns></returns>
    DateTime? GetOperationDetectionDateTime ();
  }

  /// <summary>
  /// Interface for a configuration that is related to IOperationDetectionStatusExtension
  /// </summary>
  public interface IOperationDetectionStatusConfiguration
  {
    /// <summary>
    /// Associated priority
    /// </summary>
    int OperationDetectionStatusPriority { get; }
  }
}
