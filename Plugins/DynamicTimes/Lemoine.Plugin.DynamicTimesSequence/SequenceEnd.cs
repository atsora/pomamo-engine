// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;
using Lemoine.Extensions.Business.DynamicTimes;
using Lemoine.Model;
using Lemoine.Core.Cache;

namespace Lemoine.Plugin.DynamicTimesSequence
{
  /// <summary>
  /// Sequence end dynamic time.
  /// 
  /// The parameter is the machine module id. If not set, the main machine module is considered.
  /// 
  /// The sequence must exist already at the specified time (usually it starts at the specified time).
  /// </summary>
  public class SequenceEnd
    : Lemoine.Extensions.NotConfigurableExtension
    , IDynamicTimeExtension
  {
    ILog log = LogManager.GetLogger (typeof (SequenceEnd).FullName);

    IMachineModule m_machineModule;

    public IMachine Machine
    {
      get; private set;
    }

    public string Name
    {
      get
      {
        return "SequenceEnd";
      }
    }

    public bool IsApplicable ()
    {
      return true;
    }

    public DynamicTimeApplicableStatus IsApplicableAt (DateTime dateTime)
    {
      var operationDetectionDateTime = GetOperationDetectionDateTime ();
      if (!operationDetectionDateTime.HasValue
        || (operationDetectionDateTime.Value < dateTime)) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("IsApplicableAt: operation detection date/time={0} VS dateTime={1} => Pending",
            operationDetectionDateTime, dateTime);
        }
        return DynamicTimeApplicableStatus.Pending;
      }

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("DynamicTimesSequence.SequenceEnd.IsApplicableAt")) {
          var sequenceSlot = ModelDAOHelper.DAOFactory.SequenceSlotDAO
            .FindAtWithSequence (m_machineModule, dateTime);
          if (null != sequenceSlot) {
            return DynamicTimeApplicableStatus.YesAtDateTime;
          }
          else {
            return DynamicTimeApplicableStatus.NoAtDateTime;
          }
        }
      }
    }

    public IDynamicTimeResponse Get (DateTime dateTime, UtcDateTimeRange hint, UtcDateTimeRange limit)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("DynamicTimesSequence.SequenceEnd")) {
          var sequenceSlots = ModelDAOHelper.DAOFactory.SequenceSlotDAO
            .FindAllAtAndAfter (m_machineModule, dateTime, 2);
          if (!sequenceSlots.Any ()) {
            var operationDetectionDateTime = GetOperationDetectionDateTime ();
            if (!operationDetectionDateTime.HasValue
              || (operationDetectionDateTime.Value < dateTime)) {
              if (log.IsDebugEnabled) {
                log.DebugFormat ("Get: no sequence at {0} (and after) and detection date/time {1} older => pending",
                  dateTime, operationDetectionDateTime);
              }
              return this.CreatePending ();
            }
            else {
              if (log.IsErrorEnabled) {
                log.ErrorFormat ("Get: there is no sequence at {0} (and after) with detection date/time {1} => not applicable",
                  dateTime, operationDetectionDateTime);
              }
              return this.CreateNotApplicable ();
            }
          }
          var sequenceAt = sequenceSlots.FirstOrDefault (s => (null != s.Sequence) && s.DateTimeRange.ContainsElement (dateTime));
          if (null != sequenceAt) {
            var nextSequence = sequenceSlots.FirstOrDefault (s => s.Id != sequenceAt.Id);
            if (null != nextSequence) {
              if (log.IsDebugEnabled) {
                log.DebugFormat ("Get: sequence at range is {0} with a next sequence => return {1}", sequenceAt.DateTimeRange, sequenceAt.EndDateTime);
              }
              Debug.Assert (sequenceAt.EndDateTime.HasValue);
              return this.CreateFinal (sequenceAt.EndDateTime.Value);
            }
            else { // null == nextSequence
              var operationDetectionDateTime = GetOperationDetectionDateTime ();
              if (operationDetectionDateTime.HasValue
                && (Bound.Compare<DateTime> (sequenceAt.EndDateTime, operationDetectionDateTime.Value) < 0)) {
                if (log.IsDebugEnabled) {
                  log.DebugFormat ("Get: sequence end {0} is before operation detection date/time, it can be returned", sequenceAt.EndDateTime);
                }
                Debug.Assert (sequenceAt.EndDateTime.HasValue);
                return this.CreateFinal (sequenceAt.EndDateTime.Value);
              }
              else {
                if (log.IsDebugEnabled) {
                  log.DebugFormat ("Get: operation detection date/time {0} before or equal sequence end date/time {1} => pending",
                    operationDetectionDateTime, sequenceAt.EndDateTime);
                }
                return this.CreatePending ();
              }
            }
          }
          else { // null == SequenceAt
            if (log.IsErrorEnabled) {
              log.ErrorFormat ("Get: there is no sequence at {0} (but there are some after) => not applicable",
                dateTime);
            }
            return this.CreateNotApplicable ();
          }
        }
      }
    }

    DateTime? GetOperationDetectionDateTime ()
    {
      var operationDetectionStatusRequest = new Lemoine.Business.Operation
        .OperationDetectionStatus (this.Machine);
      var operationDetectionStatus = Business.ServiceProvider
        .Get<DateTime?> (operationDetectionStatusRequest);
      return operationDetectionStatus;
    }

    public TimeSpan GetCacheTimeout (IDynamicTimeResponse data)
    {
      if (data.Final.HasValue || data.NoData) {
        return CacheTimeOut.Permanent.GetTimeSpan ();
      }
      else {
        return CacheTimeOut.CurrentShort.GetTimeSpan ();
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="parameter">Machine module id</param>
    /// <returns></returns>
    public bool Initialize (IMachine machine, string parameter)
    {
      Debug.Assert (null != machine);

      this.Machine = machine;

      LogManager.GetLogger (typeof (SequenceEnd).FullName + "." + machine.Id);

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        if (string.IsNullOrEmpty (parameter)) { // Consider the main machine module
          var monitoredMachine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindByIdWithMachineModules (machine.Id);
          if (null == monitoredMachine) {
            if (log.IsErrorEnabled) {
              log.ErrorFormat ("Initialize: machine id {0} is not monitored", machine.Id);
            }
            return false;
          }
          m_machineModule = monitoredMachine.MainMachineModule;
          if (null == m_machineModule) {
            if (log.IsErrorEnabled) {
              log.ErrorFormat ("Initialize: no main machine module for machine {0}", machine.Id);
            }
            return false;
          }
        }
        else {
          int machineModuleId;
          if (!int.TryParse (parameter, out machineModuleId)) {
            if (log.IsErrorEnabled) {
              log.ErrorFormat ("Initialize: the parameter {0} is not an integer, so not a machine module id", parameter);
            }
            return false;
          }
          m_machineModule = ModelDAOHelper.DAOFactory.MachineModuleDAO
            .FindById (machineModuleId);
          if (null == m_machineModule) {
            if (log.IsErrorEnabled) {
              log.ErrorFormat ("Initialize: no machine module with id {0}", machineModuleId);
            }
            return false;
          }
        }
      }

      Debug.Assert (null != m_machineModule);

      return true;
    }
  }
}
