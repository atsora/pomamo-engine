// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Model;
using System;
using System.Collections.Generic;

namespace Lemoine.Extensions.Business
{
  /// <summary>
  /// Response type of some methods of the IGoodCycleExtension interface
  /// </summary>
  public enum GoodCycleExtensionResponse
  {
    /// <summary>
    /// The cycle can be considered as ok
    /// </summary>
    OK,
    /// <summary>
    /// The cycle is not ok
    /// </summary>
    KO,
    /// <summary>
    /// The result must be postponed to a later time
    /// </summary>
    POSTPONE,
  }

  /// <summary>
  /// Extension to define when a cycle is a good cycle
  /// </summary>
  public interface IGoodCycleExtension: IExtension
  {
    /// <summary>
    /// Initialize
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    bool Initialize (IMachine machine);

    /// <summary>
    /// Score of the extension.
    /// 
    /// Extensions with the highest score are considered first
    /// </summary>
    double Score { get; }

    /// <summary>
    /// Is a cycle a good cycle ?
    /// </summary>
    /// <param name="cycle">not null</param>
    /// <param name="maxMachiningDurationMultiplicator"></param>
    /// <returns></returns>
    GoodCycleExtensionResponse IsGood (IOperationCycle cycle, double maxMachiningDurationMultiplicator);

    /// <summary>
    /// Is the loading time of the cycle ok ?
    /// </summary>
    /// <param name="cycle">not null</param>
    /// <param name="monitoredMachine"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="maxLoadingDurationMultiplicator"></param>
    /// <returns></returns>
    GoodCycleExtensionResponse IsGoodLoadingTime (IOperationCycle cycle, IMonitoredMachine monitoredMachine, DateTime start, DateTime end, double maxLoadingDurationMultiplicator);
  }
}
