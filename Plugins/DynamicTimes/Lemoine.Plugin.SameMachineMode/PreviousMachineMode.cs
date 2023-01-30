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

namespace Lemoine.Plugin.SameMachineMode
{
  public class PreviousMachineMode
    : Lemoine.Extensions.NotConfigurableExtension
    , IDynamicTimeExtension
  {
    ILog log = LogManager.GetLogger (typeof (PreviousMachineMode).FullName);

    static readonly string TIMEOUT_KEY = "SameMachineMode.PreviousMachineMode.Timeout";
    static readonly TimeSpan TIMEOUT_DEFAULT = TimeSpan.FromSeconds (30);

    public IMachine Machine
    {
      get; private set;
    }

    public string Name
    {
      get
      {
        return "PreviousMachineMode";
      }
    }

    public bool IsApplicable ()
    {
      return true;
    }

    public DynamicTimeApplicableStatus IsApplicableAt (DateTime dateTime)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("SameMachineMode.PreviousMachineMode.IsApplicableAt")) {
          var factAt = ModelDAOHelper.DAOFactory.FactDAO
            .FindAt (this.Machine, dateTime);
          if (null != factAt) {
            return DynamicTimeApplicableStatus.YesAtDateTime;
          }
          else {
            if (IsFactAfter (dateTime)) {
              return DynamicTimeApplicableStatus.NoAtDateTime;
            }
            else {
              return DynamicTimeApplicableStatus.Pending;
            }
          }
        }
      }
    }

    public IDynamicTimeResponse Get (DateTime dateTime, UtcDateTimeRange hint, UtcDateTimeRange limit)
    {
      var startDateTime = DateTime.UtcNow;
      var timeout = Lemoine.Info.ConfigSet.LoadAndGet (TIMEOUT_KEY, TIMEOUT_DEFAULT);

      var range = new UtcDateTimeRange (limit
        .Intersects (hint)
        .Intersects (new UtcDateTimeRange (new LowerBound<DateTime> (null), dateTime, "(]")));
      if (range.IsEmpty ()) {
        return this.CreateNoData ();
      }
      Debug.Assert (!range.IsEmpty ());

      var step = TimeSpan.FromHours (8);

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("SameMachineMode.PreviousMachineMode")) {
          int machineModeId = 0;
          var facts = ModelDAOHelper.DAOFactory.FactDAO
            .FindOverlapsRangeDescending (this.Machine, range, step);
          DateTime lastDateTime = dateTime;
          foreach (var fact in facts) {
            if (timeout < DateTime.UtcNow.Subtract (startDateTime)) {
              log.Error ($"Get: timeout, start={startDateTime} VS timeout={timeout}");
              return this.CreateTimeout ();
            }

            if (0 == machineModeId) {
              if (!fact.DateTimeRange.ContainsElement (dateTime)) {
                log.ErrorFormat ("Get: no fact at {0} (initial gap) => not applicable", dateTime);
                if (IsFactAfter (dateTime)) {
                  return this.CreateNotApplicable ();
                }
                else {
                  return this.CreatePending ();
                }
              }
              machineModeId = fact.CncMachineMode.Id;
            }
            else {
              Debug.Assert (0 != machineModeId);
              if (fact.End < lastDateTime) { // Gap
                log.DebugFormat ("Get: gap {0}-{1}", fact.End, lastDateTime);
                return this.CreateFinal (lastDateTime);
              }
              Debug.Assert (lastDateTime.Equals (fact.End));
              if (fact.CncMachineMode.Id != machineModeId) {
                log.DebugFormat ("Get: machine mode change at {0}", fact.End);
                return this.CreateFinal (lastDateTime);
              }
            }
            lastDateTime = fact.Begin;
          }

          if (0 == machineModeId) { // No fact in range
            if (IsFactAfter (dateTime)) {
              if (log.IsDebugEnabled) {
                log.DebugFormat ("Get: no fact in range {0}, but one after => not applicable",
                  range);
              }
              return this.CreateNotApplicable ();
            }
            else {
              if (log.IsDebugEnabled) {
                log.DebugFormat ("Get: no fact in range {0} and after => pending", range);
              }
              return this.CreatePending ();
            }
          }

          int compare = Bound.Compare<DateTime> (range.Lower, lastDateTime);
          if (compare < 0) { // range.Lower < lastDateTime
            if (log.IsDebugEnabled) {
              log.DebugFormat ("Get: gap at the end, return {0}", lastDateTime);
            }
            return this.CreateFinal (lastDateTime);
          }
          else if (0 == compare) { // range.Lower == lastDateTime
            var factWithEnd = ModelDAOHelper.DAOFactory.FactDAO
              .FindAt (this.Machine, lastDateTime.Subtract (TimeSpan.FromSeconds (1)));
            if (null == factWithEnd) {
              return this.CreateFinal (lastDateTime);
            }
            else { // null != factWithEnd
              Debug.Assert (factWithEnd.End.Equals (lastDateTime));
              if (factWithEnd.CncMachineMode.Id == machineModeId) {
                return this.CreateWithHint (new UtcDateTimeRange (new LowerBound<DateTime> (null), factWithEnd.Begin, "[]"));
              }
              else {
                return this.CreateFinal (lastDateTime);
              }
            }
          }
          else { // lastDateTime < range.Lower
            if (log.IsDebugEnabled) {
              log.DebugFormat ("Get: before {0} because last fact before range {1}",
                lastDateTime, range.Lower);
            }
            return this.CreateWithHint (new UtcDateTimeRange (new LowerBound<DateTime> (null), lastDateTime, "[]"));
          }
        }
      }
    }

    bool IsFactAfter (DateTime dateTime)
    {
      var nextFact = ModelDAOHelper.DAOFactory.FactDAO
        .FindFirstFactAfter (this.Machine, dateTime);
      return (null != nextFact);
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

      log = LogManager.GetLogger (typeof (PreviousMachineMode).FullName + "." + machine.Id);

      return true;
    }
  }
}
