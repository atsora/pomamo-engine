// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
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

namespace Lemoine.Plugin.NGoodCyclesIsProduction
{
  /// <summary>
  /// Default implementation of <see cref="IGoodCycleExtension"/> that considers a cycle as good if its machining duration is not too long
  /// It is set just in case no other specific GoodCycleExtension is available
  /// </summary>
  public class GoodCycleExtension
    : MultipleInstanceConfigurableExtension<Configuration>
    , IGoodCycleExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (GoodCycleExtension).FullName);

    IMachine m_machine;
    Configuration m_configuration;

    public double Score => 10.0;

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

      if (m_configuration.DeactivateGoodCycleExtension) {
        if (log.IsDebugEnabled) {
          log.Debug ($"Initialize: the good cycle extension is deactivated");
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
        log.Info ("IsGood: good cycle with no start, return N/A");
        return GoodCycleExtensionResponse.NOT_APPLICABLE;
      }
      if (!cycle.End.HasValue) {
        log.Info ("IsGood: good cycle with no end, return N/A");
        return GoodCycleExtensionResponse.NOT_APPLICABLE;
      }
      if ((null == cycle.OperationSlot) || (null == cycle.OperationSlot.Operation)) {
        log.Info ("IsGood: no operation slot or operation, return N/A");
        return GoodCycleExtensionResponse.NOT_APPLICABLE;
      }
      else { // null != cycle.OperationSlot && null != cycle.OperationSlot.Operation
        var standardDuration = cycle.OperationSlot.Operation.MachiningDuration;
        if (!standardDuration.HasValue) {
          log.Info ("IsGood: no standard duration for the operation, return N/A");
          return GoodCycleExtensionResponse.NOT_APPLICABLE;
        }
        else {
          TimeSpan maxMachiningDuration = TimeSpan.FromSeconds (standardDuration.Value.TotalSeconds
            * maxMachiningDurationMultiplicator);
          var duration = cycle.End.Value.Subtract (cycle.Begin.Value);
          return duration <= maxMachiningDuration
            ? GoodCycleExtensionResponse.OK
            : GoodCycleExtensionResponse.KO;
        }
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
        log.Info ("IsGoodLoadingTime: no operation slot or operation, return N/A");
        return GoodCycleExtensionResponse.NOT_APPLICABLE;
      }
      else { // null != cycle.OperationSlot && null != cycle.OperationSlot.Operation
        var operation = cycle.OperationSlot.Operation;
        Debug.Assert (null != operation);
        TimeSpan? standardBetweenCyclesDuration = operation.GetStandardBetweenCyclesDuration (monitoredMachine);
        if (!standardBetweenCyclesDuration.HasValue) {
          return GoodCycleExtensionResponse.NOT_APPLICABLE;
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
