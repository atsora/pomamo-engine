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

namespace Lemoine.Plugin.GoodCycleOnlyEnd
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
          else {
            return GoodCycleExtensionResponse.KO;
          }
        }
      }
    }

    public GoodCycleExtensionResponse IsGoodLoadingTime (IOperationCycle cycle, IMonitoredMachine monitoredMachine, DateTime start, DateTime end, double maxLoadingDurationMultiplicator)
    {
      return GoodCycleExtensionResponse.OK;
    }
  }
}
