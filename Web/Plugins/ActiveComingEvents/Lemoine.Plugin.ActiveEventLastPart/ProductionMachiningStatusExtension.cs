// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Pulse.Extensions.Web;

namespace Lemoine.Plugin.ActiveEventLastPart
{
  public class ProductionMachiningStatusExtension
     : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IProductionMachiningStatusExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (ProductionMachiningStatusExtension).FullName);

    IMonitoredMachine m_machine;
    Configuration m_configuration;
    string m_message;

    public IList<Event> GetActiveEvents (Lemoine.Extensions.Business.IPartProductionDataCurrentShift partProductionCurrentShiftResponse)
    {
      var events = new List<Event> ();

      IOperationCycle lastCycle;
      DateTime lastCycleEnd;
      EventSeverity eventSeverity;
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Plugin.ActiveEventLastPart")) {
          lastCycle = ModelDAOHelper.DAOFactory.OperationCycleDAO
            .GetLastFullCycle (m_machine);
          if (null == lastCycle) {
            if (log.IsDebugEnabled) {
              log.DebugFormat ("GetActiveEvents: no last full cycle for machine {0}",
                m_machine.Id);
            }
            return events;
          }
          var now = DateTime.UtcNow;
          lastCycleEnd = lastCycle.End ?? lastCycle.DateTime;
          TimeSpan cycleDurationTarget = GetCycleDurationTarget (partProductionCurrentShiftResponse, lastCycle);

          if (now <= lastCycleEnd.Add (cycleDurationTarget)) {
            if (log.IsDebugEnabled) {
              log.DebugFormat ("GetActiveEvents: last cycle end {0} is too recent compared to the cycle duration target {1}",
                lastCycleEnd, cycleDurationTarget);
            }
            return events;
          }

          // Check the production times between lastCycleEnd and now
          var productionDuration = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
            .GetProductionDuration (m_machine, new UtcDateTimeRange (lastCycleEnd, now));
          if (productionDuration <= cycleDurationTarget) {
            if (log.IsDebugEnabled) {
              log.DebugFormat ("GetActiveEvents: last cycle end {0} is too recent compare to the cycle duration target {1} and production duration",
                lastCycleEnd, cycleDurationTarget,
                productionDuration);
            }
            return events;
          }

          // Severity configuration
          var delay = productionDuration.Subtract (cycleDurationTarget);
          eventSeverity = GetSeverity (delay);
        }
      }

      var activeStopEvent = Event.CreateActiveEvent (m_message, lastCycleEnd, eventSeverity);
      events.Add (activeStopEvent);

      return events;
    }

    TimeSpan GetCycleDurationTarget (Lemoine.Extensions.Business.IPartProductionDataCurrentShift partProductionCurrentShiftResponse, IOperationCycle lastCycle)
    {
      if (partProductionCurrentShiftResponse.CycleDurationTarget.HasValue) {
        return partProductionCurrentShiftResponse.CycleDurationTarget.Value;
      }

      if ((null != lastCycle.OperationSlot)
        && (null != lastCycle.OperationSlot.Operation)) {
        var operationCyclePropertiesRequest = new Lemoine.Business.Operation
         .OperationCycleProperties (m_machine, lastCycle.OperationSlot.Operation);
        var operationCyclePropertiesResponse = Lemoine.Business.ServiceProvider
          .Get (operationCyclePropertiesRequest);
        TimeSpan? cycleDuration = operationCyclePropertiesResponse.CycleDuration;
        if (cycleDuration.HasValue) {
          if (log.IsDebugEnabled) {
            log.DebugFormat ("GetCycleDurationTarget: return cycle duration target {0} from OperationCycleProperties business service", cycleDuration);
          }
          return cycleDuration.Value;
        }
        else if (lastCycle.OperationSlot.Operation.MachiningDuration.HasValue) {
          if (log.IsWarnEnabled) {
            log.WarnFormat ("GetCycleDurationTarget: return cycle duration target {0} from machining duration only", lastCycle.OperationSlot.Operation.MachiningDuration);
          }
          return lastCycle.OperationSlot.Operation.MachiningDuration.Value;
        }
      }

      if (log.IsWarnEnabled) {
        log.WarnFormat ("GetCycleDurationTarget: return a default value 0");
      }
      return TimeSpan.FromSeconds (0);
    }

    EventSeverity GetSeverity (TimeSpan delay)
    {
      // Severity configuration
      if (m_configuration.ErrorDelay.HasValue
        && (m_configuration.ErrorDelay.Value <= delay)) {
        return new EventSeverity (EventSeverityLevel.Error);
      }
      else if (m_configuration.Warning2Delay.HasValue
        && (m_configuration.Warning2Delay.Value <= delay)) {
        return new EventSeverity (EventSeverityLevel.Warning2);
      }
      else if (m_configuration.Warning1Delay.HasValue
        && (m_configuration.Warning1Delay.Value <= delay)) {
        return new EventSeverity (EventSeverityLevel.Warning1);
      }
      else {
        return new EventSeverity (EventSeverityLevel.Info);
      }
    }

    public IList<Event> GetComingEvents (Lemoine.Extensions.Business.IPartProductionDataCurrentShift partProductionCurrentShiftResponse)
    {
      return new List<Event> ();
    }

    public bool Initialize (IMonitoredMachine machine)
    {
      m_machine = machine;

      if (!LoadConfiguration (out m_configuration)) {
        log.WarnFormat ("Initialize: " +
                        "the configuration is not valid");
        return false;
      }

      m_message = m_configuration.Message;

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        int machineFilterId = m_configuration.MachineFilterId;
        if (0 != machineFilterId) {
          var machineFilter = ModelDAOHelper.DAOFactory.MachineFilterDAO
            .FindById (machineFilterId);
          if (null == machineFilter) {
            log.ErrorFormat ("Initialize: " +
                             "machine filter id {0} does not exist",
                             machineFilterId);
            return false;
          }
          // Note: machineFilter.IsMatch requires it is done in the same session
          if (!machineFilter.IsMatch (machine)) {
            return false;
          }
        }
      }

      return true;
    }
  }
}
