// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Model;
using System;
using System.Collections.Generic;

namespace Lemoine.Extensions.Analysis
{
  /// <summary>
  /// Extension to call a method after a call to IOperationDetection
  /// </summary>
  public interface IAfterOperationDetectionExtension: Detection.IOperationDetection, IExtension
  {
    /// <summary>
    /// Initialize
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    bool Initialize (IMonitoredMachine machine);
  }
}
