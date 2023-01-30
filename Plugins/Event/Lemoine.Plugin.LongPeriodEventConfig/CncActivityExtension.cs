// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Threading;
using Pulse.Extensions.Database;

namespace Lemoine.Plugin.LongPeriodEventConfig
{
  public class CncActivityExtension
    : Lemoine.Extensions.UniqueInstanceConfigurableExtension<Configuration>
    , ICncActivityExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (CncActivityExtension).FullName);

    IMonitoredMachine m_machine;
    double m_priority;
    IEnumerable<IEventLongPeriodConfig> m_eventLongPeriodConfigs;

    public double Priority => m_priority;

    public bool Initialize (IMonitoredMachine machine)
    {
      m_machine = machine;

      var configurations = LoadConfigurations ();
      if (!configurations.Any ()) {
        log.ErrorFormat ("Initialize: no valid configuration, skip this instance");
        return false;
      }
      m_priority = configurations.Min (c => c.Priority);

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        m_eventLongPeriodConfigs = ModelDAOHelper.DAOFactory.EventLongPeriodConfigDAO
          .FindAll ()
          .Where (c => (c.MonitoredMachine is null) || (c.MonitoredMachine.Id == m_machine.Id))
          .ToList ();
      }
      return m_eventLongPeriodConfigs.Any ();
    }

    public bool ProcessAssociation (IChecked checkedThread, UtcDateTimeRange range, IMachineMode machineMode, IMachineStateTemplate machineStateTemplate, IMachineObservationState machineObservationState, IShift shift, IMachineStatus machineStatus, CancellationToken cancellationToken = default)
    {
      Debug.Assert (range.Lower.HasValue);
      Debug.Assert (range.Upper.HasValue);
      Debug.Assert (null != machineMode);

      var configs = m_eventLongPeriodConfigs
        .Where (c => (null == c.MachineObservationState) || (c.MachineObservationState.Id == machineObservationState.Id))
        .Where (c => IsMatchingMachineMode (machineMode, c));
      // configs matching the machine, machine observation state and machine mode
      if (!configs.Any ()) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("ProcessAssociation: skip it since there is no related config");
        }
        return true;
      }
      checkedThread.SetActive ();

      DateTime oldPeriodBegin = range.Lower.Value;
      DateTime oldPeriodEnd = range.Upper.Value;

      if ((null != machineStatus)
          && (machineStatus.CncMachineMode.Equals (machineMode))
          && (machineStatus.MachineObservationState.Equals (machineObservationState))
          && (Bound.Compare<DateTime> (range.Lower, machineStatus.ReasonSlotEnd) <= 0)
          && (Bound.Compare<DateTime> (machineStatus.ReasonSlotEnd, range.Upper) < 0)) {
        // Get the previous ReasonSlots that correspond
        // to this machine association
        // and the corresponding period
        oldPeriodBegin = machineStatus.ReasonSlotEnd;
        oldPeriodEnd = machineStatus.ReasonSlotEnd;
        while (true) {
          checkedThread.SetActive ();
          IReasonSlot reasonSlot = ModelDAOHelper.DAOFactory.ReasonSlotDAO
            .FindWithEnd (m_machine,
                          machineStatus.MachineMode,
                          machineStatus.MachineObservationState,
                          oldPeriodBegin);
          if (null != reasonSlot) {
            Debug.Assert (reasonSlot.BeginDateTime.HasValue);
            Debug.Assert (Bound.Compare<DateTime> (reasonSlot.BeginDateTime, oldPeriodBegin) < 0);
            oldPeriodBegin = reasonSlot.BeginDateTime.Value;
          }
          else {
            break;
          }
        }
      }

      TimeSpan oldPeriodDuration = oldPeriodEnd.Subtract (oldPeriodBegin);
      TimeSpan newPeriodDuration = range.Upper.Value.Subtract (oldPeriodBegin);
      if (log.IsDebugEnabled) {
        log.Debug ($"ProcessAssociation: oldPeriod: {oldPeriodBegin}-{oldPeriodEnd} newPeriod: {range} PeriodDuration: {oldPeriodDuration} vs {newPeriodDuration}");
      }

      foreach (var config in configs.Where (c => oldPeriodDuration < c.TriggerDuration)) {
        checkedThread.SetActive ();
        cancellationToken.ThrowIfCancellationRequested ();
        if ((config.MachineMode is null)
            && (config.MachineObservationState is null)) {
          if ((oldPeriodDuration < config.TriggerDuration)
              && (config.TriggerDuration <= newPeriodDuration)) {
            // Create an event
            IEventLongPeriod eventLongPeriod = ModelDAOHelper.ModelFactory
              .CreateEventLongPeriod (config.Level,
                                      range.Upper.Value,
                                      m_machine,
                                      machineMode,
                                      machineObservationState,
                                      config.TriggerDuration);
            eventLongPeriod.Config = config;
            if (log.IsInfoEnabled) {
              log.Info ($"ProcessAssociation: new event {eventLongPeriod} created");
            }
            ModelDAOHelper.DAOFactory.EventLongPeriodDAO.MakePersistent (eventLongPeriod);
          }
          continue;
        }
        else { // null != config.MachineMode || null != config.MachineObservationState
          // Else see if the period can be extended with the considered config.MachineMode
          // and config.MachineObservationState
          DateTime extendedBegin = oldPeriodBegin;
          int i = 0;
          const int ITERATION_WARNING = 100;
          while (!cancellationToken.IsCancellationRequested) {
            checkedThread.SetActive ();
            ++i;
            IReasonSlot reasonSlot = ModelDAOHelper.DAOFactory.ReasonSlotDAO
              .FindWithEnd (m_machine,
                            extendedBegin);
            if (null == reasonSlot) {
              if (log.IsWarnEnabled) {
                log.Warn ($"ProcessAssociation: stop extending the period at {extendedBegin} because there is no reason before that time");
              }
              break;
            }
            else { // null != reasonSlot
              Debug.Assert (reasonSlot.BeginDateTime.HasValue);
              Debug.Assert (Bound.Compare<DateTime> (reasonSlot.BeginDateTime, extendedBegin) < 0);
              Debug.Assert (null != reasonSlot.MachineMode);
              Debug.Assert (null != reasonSlot.MachineObservationState);
              if ((null != config.MachineObservationState)
                  && (!config.MachineObservationState.Equals (reasonSlot.MachineObservationState))) {
                if (log.IsDebugEnabled) {
                  log.Debug ($"ProcessAssociation: extend the period begin until {extendedBegin} only because the machine observation state changed");
                }
                break;
              }
              if (null != config.MachineMode) {
                if (reasonSlot.MachineMode.Id != config.MachineMode.Id) {
                  // attachedMachineMode is a version of reasonSlot.MachineMode
                  // that is associated to this session.
                  // This is ok because there is no update of machineMode
                  // MachineModeDAO.Lock does not work here because
                  // NonUniqueObjectException may be raised else
                  IMachineMode attachedMachineMode = ModelDAOHelper.DAOFactory.MachineModeDAO
                    .FindById (reasonSlot.MachineMode.Id);
                  ModelDAOHelper.DAOFactory.MachineModeDAO.Lock (attachedMachineMode);
                  if (!attachedMachineMode.IsDescendantOrSelfOf (config.MachineMode)) {
                    if (log.IsDebugEnabled) {
                      log.Debug ($"ProcessAssociation: extend the period begin until {extendedBegin} only because the machine mode {attachedMachineMode.Id} at that time is not a descendant of {config.MachineMode.Id}");
                    }
                    break;
                  }
                } // Else same machine mode, extend...
              }
              if (Bound.Compare<DateTime> (extendedBegin, reasonSlot.BeginDateTime) <= 0) {
                log.FatalFormat ("ProcessAssociation: " +
                                 "reasonSlot before {0} has a begin date/time {1} that is not before it",
                                 extendedBegin, reasonSlot.BeginDateTime);
                throw new InvalidOperationException ();
              }
              extendedBegin = reasonSlot.BeginDateTime.Value; // reasonSlotBeginDateTime < extendedBegin
              oldPeriodDuration = oldPeriodEnd.Subtract (extendedBegin);
              newPeriodDuration = range.Upper.Value.Subtract (extendedBegin);
              if (config.TriggerDuration <= oldPeriodDuration) {
                if (log.IsDebugEnabled) {
                  log.Debug ($"ProcessAssociation: stop extending the period at {extendedBegin} because the old duration is now {oldPeriodDuration} which is longer than the configuration trigger {config.TriggerDuration} (it is useless to continue)");
                }
                break;
              }
              if (log.IsWarnEnabled && (ITERATION_WARNING < i)) {
                log.Warn ($"ProcessAssociation: already {i} iterations while trying to extend the reasonSlot");
              }
            } // else: null != reasonSlot
          } // Loop
          cancellationToken.ThrowIfCancellationRequested ();
          checkedThread.SetActive ();
          if ((oldPeriodDuration < config.TriggerDuration)
              && (config.TriggerDuration <= newPeriodDuration)) {
            IMachineMode eventLongPeriodMachineMode;
            if (config.MachineMode is null) {
              eventLongPeriodMachineMode = machineMode;
            }
            else { // config.MachineMode is not null: get a machine mode that is attached to the session
              eventLongPeriodMachineMode = ModelDAOHelper.DAOFactory.MachineModeDAO
                .FindById (config.MachineMode.Id);
              if (eventLongPeriodMachineMode is null) {
                log.Fatal ($"ProcessAssociation: machine mode with id {config.MachineMode.Id}");
                throw new Exception ($"Machine mode from EventLongPeriodConfig with id={config.MachineMode.Id} does not exist any more");
              }
            }
            // Create an event
            IEventLongPeriod eventLongPeriod = ModelDAOHelper.ModelFactory
              .CreateEventLongPeriod (config.Level,
                                      range.Upper.Value,
                                      m_machine,
                                      eventLongPeriodMachineMode,
                                      machineObservationState,
                                      config.TriggerDuration);
            eventLongPeriod.Config = config;
            if (log.IsInfoEnabled) {
              log.Info ($"ProcessAssociation: new event {eventLongPeriod} created");
            }
            ModelDAOHelper.DAOFactory.EventLongPeriodDAO.MakePersistent (eventLongPeriod);
          }
        } // else: null != config.MachineMode || null != config.MachineObservationState
      } // Loop on configs

      return true;
    }

    bool IsMatchingMachineMode (IMachineMode machineMode, IEventLongPeriodConfig config)
    {
      Debug.Assert (null != machineMode);

      if (null == config.MachineMode) {
        return true;
      }
      if (config.MachineMode.Id == machineMode.Id) {
        return true;
      }
      var isDescendantOrSelfOfRequest = new Lemoine.Business.MachineMode.IsDescendantOrSelfOf (config.MachineMode, machineMode);
      var isDescendantOrSelfOf = Lemoine.Business.ServiceProvider
        .Get (isDescendantOrSelfOfRequest);
      return isDescendantOrSelfOf;
    }
  }
}
