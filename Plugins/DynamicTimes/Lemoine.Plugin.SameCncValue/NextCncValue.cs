// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;
using Lemoine.Model;
using Lemoine.Extensions.Business.DynamicTimes;
using Lemoine.Core.Cache;

namespace Lemoine.Plugin.SameCncValue
{
  public class NextCncValue
    : Lemoine.Extensions.NotConfigurableExtension
    , IDynamicTimeExtension
  {
    static readonly string TIMEOUT_KEY = "SameCncValue.NextCncValue.Timeout";
    static readonly TimeSpan TIMEOUT_DEFAULT = TimeSpan.FromSeconds (30);

    ILog log = LogManager.GetLogger (typeof (NextCncValue).FullName);

    IMachineModule m_machineModule;
    IField m_field;

    public IMachine Machine
    {
      get; private set;
    }

    public string Name
    {
      get
      {
        return "NextCncValue";
      }
    }

    public bool IsApplicable ()
    {
      return true;
    }

    public DynamicTimeApplicableStatus IsApplicableAt (DateTime at)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("DynamicTime.NextCncValue.IsApplicableAt", TransactionLevel.ReadCommitted)) {
          var cncValueAtDateTime = ModelDAOHelper.DAOFactory.CncValueDAO
            .FindAt (m_machineModule, m_field, at);
          if (null == cncValueAtDateTime) {
            var nextCncValue = ModelDAOHelper.DAOFactory.CncValueDAO
              .FindNext (m_machineModule, m_field, at.AddSeconds (1), 1)
              .FirstOrDefault ();
            if (null == nextCncValue) {
              return DynamicTimeApplicableStatus.Pending;
            }
            else {
              if (log.IsWarnEnabled) {
                log.WarnFormat ("IsApplicableAt: gap between {0} and {1} => NotApplicable", at, nextCncValue.Begin);
              }
              return DynamicTimeApplicableStatus.NoAtDateTime;
            }
          }
          else { // null != cncValueAtDateTime
            return DynamicTimeApplicableStatus.YesAtDateTime;
          }
        }
      }

    }

    public IDynamicTimeResponse Get (DateTime dateTime, UtcDateTimeRange hint, UtcDateTimeRange limit)
    {
      var startDateTime = DateTime.UtcNow;
      var timeout = Lemoine.Info.ConfigSet.LoadAndGet (TIMEOUT_KEY, TIMEOUT_DEFAULT);

      Debug.Assert (limit.ContainsElement (dateTime));

      var range = new UtcDateTimeRange (limit
        .Intersects (hint)
        .Intersects (new UtcDateTimeRange (dateTime)));
      if (range.IsEmpty ()) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("Get: dateTime={0} limit={1} hint={2} => No data", dateTime, limit, hint);
        }
        return this.CreateNoData ();
      }
      Debug.Assert (!range.IsEmpty ());
      Debug.Assert (range.Lower.HasValue);

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("DynamicTime.NextCncValue", TransactionLevel.ReadCommitted)) {
          object v = null;
          if (!range.ContainsElement (dateTime)) {
            var cncValueAtDateTime = ModelDAOHelper.DAOFactory.CncValueDAO
              .FindAt (m_machineModule, m_field, dateTime);
            if (null == cncValueAtDateTime) {
              return GetInitialGap (dateTime, limit);
            }
            else { // null != cncValueAtDateTime
              v = cncValueAtDateTime.Value;
            }
          }

          var step = TimeSpan.FromHours (4);
          var cncValues = ModelDAOHelper.DAOFactory.CncValueDAO
            .FindOverlapsRangeAscending (m_machineModule, m_field, range, step);
          DateTime lastCncValueEnd = range.Lower.Value;
          foreach (var cncValue in cncValues) {
            if (timeout < DateTime.UtcNow.Subtract (startDateTime)) {
              log.Error ($"GetValid: timeout, start={startDateTime} VS timeout={timeout}");
              return this.CreateTimeout ();
            }

            if (null == v) {
              if (!cncValue.DateTimeRange.ContainsElement (dateTime)) {
                if (log.IsWarnEnabled) {
                  log.WarnFormat ("Get: no cnc value at {0} (initial gap) => NotApplicable", dateTime);
                }
                return this.CreateNotApplicable ();
              }
              v = cncValue.Value;
            }
            else {
              Debug.Assert (null != v);
              if (lastCncValueEnd < cncValue.Begin) { // Gap
                if (log.IsWarnEnabled) {
                  log.WarnFormat ("Get: gap {0}-{1} => approximative final", lastCncValueEnd, cncValue.Begin);
                }
                return this.CreateFinal (lastCncValueEnd);
              }
              Debug.Assert (lastCncValueEnd.Equals (cncValue.Begin));
              if (!object.Equals (cncValue.Value, v)) {
                if (log.IsDebugEnabled) {
                  log.DebugFormat ("Get: value change at {0}", cncValue.Begin);
                }
                return this.CreateFinal (lastCncValueEnd);
              }
            }
            lastCncValueEnd = cncValue.End;
          } // Loop on cncValues

          if (range.Upper.HasValue) {
            Debug.Assert (limit.Upper.HasValue);
            Debug.Assert (range.Upper.Value.Equals (limit.Upper.Value));
            var after = (null != v)
              ? lastCncValueEnd
              : range.Upper.Value;
            var nextCncValue = GetNextCncValueAfter (after);
            if (null == nextCncValue) {
              if (null == v) { // No cnc value after dateTime at all
                return this.CreateWithHint (hint);
              }
              else { // lastCncValueEnd makes sense, no cnc value after it
                return this.CreateWithHint (new UtcDateTimeRange (hint
                  .Intersects (new UtcDateTimeRange (lastCncValueEnd))));
              }
            }
            else { // null != nextCncValue
              if (null == v) {
                if (log.IsDebugEnabled) {
                  log.Debug ($"Get: no cnc value in {range} (some after) => NotApplicable");
                }
                return this.CreateNotApplicable ();
              }
              else { // null != v, lastCncValueEnd valid
                if (lastCncValueEnd.Equals (nextCncValue.Begin)) {
                  if (!object.Equals (nextCncValue.Value, v)
                    && limit.ContainsElement (lastCncValueEnd)) {
                    if (log.IsDebugEnabled) {
                      log.Debug ($"Get: value change at {lastCncValueEnd}");
                    }
                    return this.CreateFinal (lastCncValueEnd);
                  }
                  else {
                    Debug.Assert (limit.Upper.Value < nextCncValue.End);
                    if (log.IsDebugEnabled) {
                      log.Debug ($"Get: change is not in limit {limit}");
                    }
                    return this.CreateNoData ();
                  }
                }
                else { // Gap
                  if (log.IsDebugEnabled) {
                    log.Debug ($"Get: gap between {lastCncValueEnd} and {nextCncValue.Begin} in limit end {limit}");
                  }
                  if (limit.ContainsElement (lastCncValueEnd)) {
                    return this.CreateFinal (lastCncValueEnd);
                  }
                  else {
                    return this.CreateNoData ();
                  }
                }
              }
            } // Test on nextCncValue
          }
          else {
            if (null == v) { // No cnc value in range at all
              return this.CreateWithHint (hint);
            }
            else { // null != v, lastCncValueEnd makes sense, no cnc value after it
              return this.CreateWithHint (new UtcDateTimeRange (hint
                .Intersects (new UtcDateTimeRange (lastCncValueEnd))));
            }
          }
        }
      }
    }

    IDynamicTimeResponse GetInitialGap (DateTime dateTime, UtcDateTimeRange limit)
    {
      var nextCncValue = ModelDAOHelper.DAOFactory.CncValueDAO
        .FindNext (m_machineModule, m_field, dateTime.AddSeconds (1), 1)
        .FirstOrDefault ();
      if (null == nextCncValue) {
        return this.CreatePending ();
      }
      else {
        if (log.IsWarnEnabled) {
          log.WarnFormat ("GetInitialGap: initial gap between {0} and {1} => NotApplicable", dateTime, nextCncValue.Begin);
        }
        return this.CreateNotApplicable ();
      }
    }

    ICncValue GetNextCncValueAfter (DateTime dateTime)
    {
      return ModelDAOHelper.DAOFactory.CncValueDAO
        .FindNext (m_machineModule, m_field, dateTime.AddSeconds (1), 1)
        .FirstOrDefault ();
    }

    bool IsCncValueAfter (DateTime dateTime)
    {
      var nextCncValue = ModelDAOHelper.DAOFactory.CncValueDAO
        .FindNext (m_machineModule, m_field, dateTime.AddSeconds (1), 1)
        .FirstOrDefault ();
      return (null != nextCncValue);
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

    public bool Initialize (IMachine machine, string parameter)
    {
      Debug.Assert (null != machine);

      this.Machine = machine;

      log = LogManager.GetLogger (typeof (NextCncValue).FullName + "." + machine.Id);

      var parameters = parameter?.Split (new char[] { ',' }) ?? new string[0];
      string fieldIdParameter;
      string machineModuleIdParameter = null;
      switch (parameters.Length) {
      case 1: // Only field
        fieldIdParameter = parameters[0].Trim ();
        break;
      case 2: // machine module and field
        machineModuleIdParameter = parameters[0].Trim ();
        fieldIdParameter = parameters[1].Trim ();
        break;
      default:
        log.ErrorFormat ("Initialize: invalid parameter {0}", parameter);
        return false;
      }

      int fieldId;
      if (!int.TryParse (fieldIdParameter, out fieldId)) {
        log.ErrorFormat ("Initialize: invalid fieldId {0}", fieldIdParameter);
        return false;
      }

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        m_field = ModelDAOHelper.DAOFactory.FieldDAO
          .FindById (fieldId);
        if (null == m_field) {
          log.ErrorFormat ("Initialize: unknown field id {0}", fieldId);
          return false;
        }

        if (null == machineModuleIdParameter) {
          var monitoredMachine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (machine.Id);
          m_machineModule = monitoredMachine.MainMachineModule;
        }
        else { // null != m_machine
          int machineModuleId;
          if (!int.TryParse (machineModuleIdParameter, out machineModuleId)) {
            log.ErrorFormat ("Initialize: invalid machine module id {0}", machineModuleIdParameter);
            return false;
          }
          m_machineModule = ModelDAOHelper.DAOFactory.MachineModuleDAO
            .FindById (machineModuleId);
          if (null == m_machineModule) {
            log.ErrorFormat ("Initialize: unknown machine module id {0}", machineModuleId);
            return false;
          }
        }
      }

      return true;
    }
  }
}
