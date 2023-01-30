// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions;
using Lemoine.Extensions.Business;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Plugin.GoodCycleWithoutStopSequences
{
  public class GoodCycleExtension
    : MultipleInstanceConfigurableExtension<Configuration>
    , IGoodCycleExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (GoodCycleExtension).FullName);

    IMachine m_machine;
    Configuration m_configuration;

    public double Score
    {
      get { return m_configuration.Score; }
    }

    public bool Initialize (IMachine machine)
    {
      if (null == machine) {
        return false;
      }
      m_machine = machine;

      if (!LoadConfiguration (out m_configuration)) {
        if (log.IsErrorEnabled) {
          log.ErrorFormat ("Initialize: LoadConfiguration returned false");
        }
        return false;
      }

      return m_configuration.CheckMachineFilter (machine);
    }

    public GoodCycleExtensionResponse IsGood (IOperationCycle cycle, double maxMachiningDurationMultiplicator)
    {
      if (!cycle.Full) {
        return GoodCycleExtensionResponse.KO;
      }
      if (!cycle.Begin.HasValue) {
        log.Info ("IsGood: full cycle with no start, return true");
        return GoodCycleExtensionResponse.OK;
      }
      if (!cycle.End.HasValue) {
        log.Info ("IsGood: full cycle with no end, return true");
        return GoodCycleExtensionResponse.OK;
      }

      // Postpone ? Because the operation may not be accurate ?
      var operationDetectionStatusRequest = new Lemoine.Business.Operation
        .OperationDetectionStatus (m_machine);
      var operationDetectionStatus = Lemoine.Business.ServiceProvider
        .Get (operationDetectionStatusRequest);
      if (operationDetectionStatus.HasValue
        && operationDetectionStatus.Value < cycle.DateTime) {
        return GoodCycleExtensionResponse.POSTPONE;
      }

      if ((null == cycle.OperationSlot) || (null == cycle.OperationSlot.Operation)) {
        log.Info ("IsGood: no operation slot or operation, return true");
        return GoodCycleExtensionResponse.OK;
      }
      else { // null != cycle.OperationSlot && null != cycle.OperationSlot.Operation
        var standardDuration = cycle.OperationSlot.Operation.MachiningDuration;
        if (!standardDuration.HasValue) {
          log.Info ("IsGood: no standard duration for the operation, return true");
          return GoodCycleExtensionResponse.OK;
        }
        else {
          var range = new UtcDateTimeRange (cycle.Begin.Value, cycle.End.Value);
          TimeSpan maxMachiningDuration = TimeSpan.FromSeconds (standardDuration.Value.TotalSeconds
            * maxMachiningDurationMultiplicator);
          var duration = range.Duration.Value;
          if (duration <= maxMachiningDuration) {
            return GoodCycleExtensionResponse.OK;
          }

          using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
            using (var transaction = session.BeginReadOnlyTransaction ("Plugin.GoodCycleWithoutStopSequences.IsGood")) {
              var monitoredMachine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
                .FindByIdWithMachineModules (m_machine.Id);
              if (null == monitoredMachine) {
                if (log.IsDebugEnabled) {
                  log.DebugFormat ("IsGood: machine {0} is not monitored => return true",
                    m_machine.Id);
                }
                return GoodCycleExtensionResponse.OK;
              }

              if (operationDetectionStatus.HasValue
                && operationDetectionStatus.Value < cycle.End.Value) {
                return GoodCycleExtensionResponse.POSTPONE;
              }

              foreach (var machineModule in monitoredMachine.MachineModules) {
                var sequenceSlots = ModelDAOHelper.DAOFactory.SequenceSlotDAO
                  .FindOverlapsRange (machineModule, range);
                var stopSequenceSlots = sequenceSlots
                  .Where (s => null != s.Sequence)
                  .Where (s => IsStopSequence (s.Sequence))
                  .Where (s => s.Duration.HasValue);
                var stopSequencesDuration = stopSequenceSlots
                  .Aggregate (TimeSpan.FromTicks (0), (a, s) => a.Add (s.Duration.Value));
                var durationWithoutStopSequences = duration
                  .Subtract (stopSequencesDuration);
                if (maxMachiningDuration < durationWithoutStopSequences) {
                  if (log.IsDebugEnabled) {
                    log.DebugFormat ("IsGood: duration without stop sequences {0} is greater than {1} for machine module id {2} => return false", durationWithoutStopSequences, maxMachiningDuration, machineModule.Id);
                  }
                  return GoodCycleExtensionResponse.KO;
                }
              }
              return GoodCycleExtensionResponse.OK;
            }
          }
        }
      }
    }

    bool IsStopSequence (ISequence sequence)
    {
      switch (sequence.Kind) {
      case SequenceKind.AutoPalletChange:
      case SequenceKind.Stop:
      case SequenceKind.OptionalStop:
        return true;
      default:
        return false;
      }
    }

    public GoodCycleExtensionResponse IsGoodLoadingTime (IOperationCycle cycle, IMonitoredMachine monitoredMachine, DateTime start, DateTime end, double maxLoadingDurationMultiplicator)
    {
      if ((null != monitoredMachine) && monitoredMachine.PalletChangingDuration.HasValue) {
        return end.Subtract (start) <= monitoredMachine.PalletChangingDuration.Value
          ? GoodCycleExtensionResponse.OK
          : GoodCycleExtensionResponse.KO;
      }

      if ((null == cycle.OperationSlot) || (null == cycle.OperationSlot.Operation)) {
        log.Info ("IsGoodLoadingTime: no operation slot or operation, return true");
        return GoodCycleExtensionResponse.OK;
      }
      else { // null != cycle.OperationSlot && null != cycle.OperationSlot.Operation
        var operation = cycle.OperationSlot.Operation;
        Debug.Assert (null != operation);
        TimeSpan? standardBetweenCyclesDuration = operation.GetStandardBetweenCyclesDuration (monitoredMachine);
        if (!standardBetweenCyclesDuration.HasValue) {
          return GoodCycleExtensionResponse.OK;
        }
        else {
          var maxDuration = TimeSpan.FromSeconds (standardBetweenCyclesDuration.Value.TotalSeconds * maxLoadingDurationMultiplicator);
          var duration = end.Subtract (start);
          return duration <= maxDuration
            ? GoodCycleExtensionResponse.OK
            : GoodCycleExtensionResponse.KO;
        }
      }
    }
  }
}
