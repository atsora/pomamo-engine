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
using Pulse.Extensions.Database;

namespace Lemoine.Plugin.CycleDurationSummary
{
  /// <summary>
  /// BetweenCyclesExtensions
  /// </summary>
  public class BetweenCyclesOffsetDurationExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , IBetweenCyclesOffsetDurationExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (BetweenCyclesOffsetDurationExtension).FullName);

    public double Priority => 10.0;

    /// <summary>
    /// Update the internal OffsetDuration property
    /// 
    /// This method must be run in a session
    /// and the operation must be correct before calling this method (running for example UpdateOperation)
    /// </summary>
    public double? ComputeOffsetDuration (IBetweenCycles betweenCycles)
    {
      IMonitoredMachine monitoredMachine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
        .FindById (betweenCycles.Machine.Id);
      if (null != monitoredMachine && monitoredMachine.PalletChangingDuration.HasValue
          && (0 < ((IMonitoredMachine)betweenCycles.Machine).PalletChangingDuration.Value.Ticks)) {
        log.Debug ("UpdateOffsetDuration: " +
                   "consider the pallet changing duration");
        TimeSpan duration = betweenCycles.End.Subtract (betweenCycles.Begin);
        TimeSpan standardDuration = ((IMonitoredMachine)betweenCycles.Machine).PalletChangingDuration.Value;
        return (100 * duration.TotalSeconds) / standardDuration.TotalSeconds;
      }
      else {
        TimeSpan duration = betweenCycles.End.Subtract (betweenCycles.Begin);
        TimeSpan standardDuration = TimeSpan.FromSeconds (0);

        // Previous cycle
        Debug.Assert (null != betweenCycles.PreviousCycle);
        if ((null != betweenCycles.PreviousCycle.OperationSlot)
            && (null != betweenCycles.PreviousCycle.OperationSlot.Operation)
            && (betweenCycles.PreviousCycle.OperationSlot.Operation.UnloadingDuration.HasValue)) {
          standardDuration = betweenCycles.PreviousCycle.OperationSlot.Operation.UnloadingDuration.Value;
        }

        // Next cycle
        Debug.Assert (null != betweenCycles.NextCycle);
        if ((null != betweenCycles.NextCycle.OperationSlot)
            && (null != betweenCycles.NextCycle.OperationSlot.Operation)
            && (betweenCycles.NextCycle.OperationSlot.Operation.LoadingDuration.HasValue)) {
          standardDuration = standardDuration.Add (betweenCycles.NextCycle.OperationSlot.Operation.LoadingDuration.Value);
        }

        if (0 < standardDuration.Ticks) {
          return (100 * duration.TotalSeconds) / standardDuration.TotalSeconds;
        }

        return null;
      }
    }
  }
}
