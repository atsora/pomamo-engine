// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;

namespace Pulse.Extensions.Database
{
  /// <summary>
  /// Extension to determine if an operation cycle can be considered full
  /// </summary>
  public interface IOperationCycleFullExtension: Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Can this operation cycle can be considered full ?
    /// </summary>
    /// <param name="operationCycle"></param>
    /// <returns>null if this extension does not determine is full status</returns>
    bool? IsFull (IOperationCycle operationCycle);
  }
}
