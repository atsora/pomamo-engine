// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business;
using Lemoine.Model;

namespace Lemoine.Plugin.NGoodCyclesIsProduction
{
  /// <summary>
  /// Extension to an operation cycle
  /// </summary>
  internal static class OperationCycleExtension
  {
    static readonly ILog log = LogManager.GetLogger (typeof (OperationCycleExtension).FullName);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cycle"></param>
    /// <param name="maxMachiningDurationMultiplicator"></param>
    /// <returns></returns>
    internal static GoodCycleExtensionResponse IsGood (this IOperationCycle cycle, double maxMachiningDurationMultiplicator)
    {
      var request = new Lemoine.Business.Operation.GoodCycleMachiningTime (cycle, maxMachiningDurationMultiplicator);
      return Lemoine.Business.ServiceProvider.Get (request);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cycle"></param>
    /// <param name="start"></param>
    /// <param name="maxLoadingDurationMultiplicator"></param>
    /// <returns></returns>
    internal static GoodCycleExtensionResponse IsGoodLoadingTime (this IOperationCycle cycle, DateTime start, double maxLoadingDurationMultiplicator)
    {
      var request = new Lemoine.Business.Operation.GoodCycleLoadingTime (cycle, start, maxLoadingDurationMultiplicator);
      return Lemoine.Business.ServiceProvider.Get (request);
    }
  }
}
