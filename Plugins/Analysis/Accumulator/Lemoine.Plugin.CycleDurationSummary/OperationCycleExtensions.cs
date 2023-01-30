// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.GDBPersistentClasses;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Plugin.CycleDurationSummary
{
  /// <summary>
  /// OperationCycleExtensions
  /// </summary>
  public static class OperationCycleExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (OperationCycleExtensions).FullName);

    /// <summary>
    /// compute OffsetDuration w.r.t. a standard value equal to standardCycleTimeInHours
    /// </summary>
    /// <param name="operationCycle"></param>
    /// <param name="standardCycleTime"></param>
    /// <returns></returns>
    public static double? ComputeOffsetDuration (this IOperationCycle operationCycle, TimeSpan? standardCycleTime)
    {
      try {
        if ((standardCycleTime.HasValue) && (standardCycleTime.Value.TotalSeconds > 0)) {
          TimeSpan estimatedCycleDuration = standardCycleTime.Value;

          // recompute operation cycle's offset actual vs standard
          DateTime? cycleBegin = operationCycle.Begin;
          DateTime? cycleEnd = operationCycle.End;

          if ((cycleBegin.HasValue) && (cycleEnd.HasValue)) {
            TimeSpan cycleDuration = cycleEnd.Value - cycleBegin.Value;
            double offsetDurationInPercentage = (100 * (cycleDuration.TotalSeconds - estimatedCycleDuration.TotalSeconds)) / estimatedCycleDuration.TotalSeconds;
            return offsetDurationInPercentage;
          }
        }
        return null;
      }
      catch (System.ArithmeticException) {
        return null;
      }
    }


    /// <summary>
    /// Re-compute the offset duration
    /// </summary>
    /// <param name="operationCycle"></param>
    public static void RecomputeOffsetDuration (this IOperationCycle operationCycle)
    {
      IOperationSlot operationSlot = operationCycle.OperationSlot;
      if ((operationSlot != null) && (operationSlot.Operation != null)) {
        TimeSpan? estimatedCycleTime = operationSlot.Operation.MachiningDuration;
        ((OperationCycle)operationCycle).OffsetDuration = operationCycle.ComputeOffsetDuration (estimatedCycleTime);
      }
      else {
        ((OperationCycle)operationCycle).OffsetDuration = null;
      }
    }

    /// <summary>
    /// Re-compute the offset duration of the next and previous 'between cycles'
    /// </summary>
    public static void RecomputeBetweenCyclesOffsetDuration (this IOperationCycle operationCycle)
    {
      IMonitoredMachine monitoredMachine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
        .FindById (operationCycle.Machine.Id);
      if ((null != monitoredMachine)
          && (!monitoredMachine.PalletChangingDuration.HasValue
              || (0 == monitoredMachine.PalletChangingDuration.Value.Ticks))) { // Else this is not needed
        if (0 != operationCycle.Id) { // For a transient operationcycle, there is no associated BetweenCycles entities
          IBetweenCycles previousBetweenCycles = ModelDAOHelper.DAOFactory.BetweenCyclesDAO
            .FindWithNextCycle (operationCycle);
          if (null != previousBetweenCycles) {
            (new BetweenCyclesDAO ()).UpdateOffsetDuration (previousBetweenCycles);
          }
          IBetweenCycles nextBetweenCycles = ModelDAOHelper.DAOFactory.BetweenCyclesDAO
            .FindWithPreviousCycle (operationCycle);
          if (null != nextBetweenCycles) {
            (new BetweenCyclesDAO ()).UpdateOffsetDuration (nextBetweenCycles);
          }
        }
      }
    }


  }
}
