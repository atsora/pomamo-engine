// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Model;
using System;
using System.Collections.Generic;

namespace Lemoine.Extensions.Analysis
{
  /// <summary>
  /// Extension to call a method after a call to IOperationCycleDetection
  /// </summary>
  public interface IAfterCycleDetectionExtension: IExtension
  {
    /// <summary>
    /// Initialize
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    bool Initialize (IMonitoredMachine machine);

    /// <summary>
    /// Associated machine
    /// </summary>
    IMonitoredMachine Machine { get; }

    /// <summary>
    /// Start an OperationCycle
    /// </summary>
    /// <param name="dateTime"></param>
    void StartCycle (DateTime dateTime);

    /// <summary>
    /// Stop an OperationCycle.
    /// 
    /// Note the operation slot is not extended with this method.
    /// 
    /// The top transaction must be created in the parent function.
    /// 
    /// Most of the time, the quantity parameter should be kept null.
    /// Set it only when you detect you made for a specific cycle a not standard number of parts.
    /// </summary>
    /// <param name="quantity">Quantity of completed parts when not standard</param>
    /// <param name="dateTime"></param>
    void StopCycle (int? quantity, DateTime dateTime);

    /// <summary>
    /// Start and then Stop an OperationCycle
    /// in one step
    /// 
    /// Note the operation slot is not extended with this method
    /// 
    /// The top transaction is defined in the parent function
    /// </summary>
    /// <param name="startDateTime"></param>
    /// <param name="stopDateTime"></param>
    void StartStopCycle (DateTime startDateTime,
                         DateTime stopDateTime);

    /// <summary>
    /// Trigger a detection process error
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="ex"></param>
    void DetectionProcessError (IMachineModule machineModule, Exception ex);
  }
}
