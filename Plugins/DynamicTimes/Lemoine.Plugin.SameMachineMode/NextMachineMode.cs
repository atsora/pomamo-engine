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

namespace Lemoine.Plugin.SameMachineMode
{
  public class NextMachineMode
    : Lemoine.Extensions.NotConfigurableExtension
    , IDynamicTimeExtension
  {
    ILog log = LogManager.GetLogger (typeof (NextMachineMode).FullName);

    static readonly string TIMEOUT_KEY = "SameMachineMode.NextMachineMode.Timeout";
    static readonly TimeSpan TIMEOUT_DEFAULT = TimeSpan.FromSeconds (30);

    public IMachine Machine
    {
      get; private set;
    }

    public string Name
    {
      get
      {
        return "NextMachineMode";
      }
    }

    public bool IsApplicable ()
    {
      return true;
    }

    public DynamicTimeApplicableStatus IsApplicableAt (DateTime dateTime)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("SameMachineMode.NextMachineMode.IsApplicableAt")) {
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
      if (!limit.ContainsElement (dateTime)) {
        log.Fatal ($"Get: limit={limit} does not contain dateTime={dateTime}");
      }
      Debug.Assert (limit.ContainsElement (dateTime));

      var startDateTime = DateTime.UtcNow;
      var timeout = Lemoine.Info.ConfigSet.LoadAndGet (TIMEOUT_KEY, TIMEOUT_DEFAULT);

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
        using (var transaction = session.BeginReadOnlyTransaction ("DynamicTime.NextMachineMode", TransactionLevel.ReadCommitted, transactionLevelOptional: true)) {
          int machineModeId = 0;
          if (!range.ContainsElement (dateTime)) {
            var factAtDateTime = ModelDAOHelper.DAOFactory.FactDAO
              .FindAt (this.Machine, dateTime);
            if (null == factAtDateTime) {
              return GetInitialGap (dateTime, limit);
            }
            else { // null != factAtDateTime
              machineModeId = factAtDateTime.CncMachineMode.Id;
            }
          }

          var step = TimeSpan.FromHours (4);
          var facts = ModelDAOHelper.DAOFactory.FactDAO
            .FindOverlapsRangeAscending (this.Machine, range, step);
          DateTime lastFactEnd = range.Lower.Value;
          bool firstFact = true;
          foreach (var fact in facts) {
            if (timeout < DateTime.UtcNow.Subtract (startDateTime)) {
              log.Error ($"Get: timeout, start={startDateTime} VS timeout={timeout}");
              return this.CreateTimeout ();
            }

            if (0 == machineModeId) {
              if (!fact.DateTimeRange.ContainsElement (dateTime)) {
                if (log.IsWarnEnabled) {
                  log.WarnFormat ("Get: no fact at {0} (initial gap) => NotApplicable", dateTime);
                }
                return this.CreateNotApplicable ();
              }
              machineModeId = fact.CncMachineMode.Id;
            }
            else { // 0 != machineModeId
              Debug.Assert (0 != machineModeId);
              if (lastFactEnd < fact.Begin) { // Gap
                if (log.IsWarnEnabled) {
                  log.WarnFormat ("Get: gap {0}-{1} => approximative final", lastFactEnd, fact.Begin);
                }
                return this.CreateFinal (lastFactEnd);
              }
              else { // No gap
                if (!firstFact && (fact.Begin < lastFactEnd)) { // If first fact, lastFactEnd = range.Lower
                  log.Fatal ($"Get: lastFactEnd={lastFactEnd} after new fact start {fact.Begin} for range={range} => skip this fact (fact overlap ?)");
                }
                if (fact.CncMachineMode.Id != machineModeId) {
                  if (log.IsDebugEnabled) {
                    log.DebugFormat ("Get: machine mode change at {0}", fact.Begin);
                  }
                  return this.CreateFinal (lastFactEnd);
                }
              }
            }
            lastFactEnd = fact.End;
            firstFact = false;
          }

          if (0 == machineModeId) { // No fact in range
            if (range.Upper.HasValue && IsFactAfter (range.Upper.Value)) {
              if (log.IsWarnEnabled) {
                log.WarnFormat ("Get: first fact after limit {0} (initial gap) => NoData", limit);
              }
              return this.CreateNoData ();
            }
            else { // Nothing new...
              return this.CreateWithHint (hint);
            }
          }
          else {
            if (limit.Upper.HasValue && (
              (Bound.Compare<DateTime> (limit.Upper, lastFactEnd) < 0)
               || (!limit.UpperInclusive && (limit.Upper.Value == lastFactEnd)))) {
              if (log.IsDebugEnabled) {
                log.Debug ($"Get: lastFactEnd {lastFactEnd} after limit {limit} => return NoData");
              }
              return this.CreateNoData ();
            }
            if (limit.Upper.HasValue && range.Upper.HasValue) {
              var nextFact = ModelDAOHelper.DAOFactory.FactDAO
                .FindFirstFactAfter (this.Machine, range.Upper.Value);
              if (null != nextFact) {
                if ((lastFactEnd <= nextFact.Begin) && limit.ContainsElement (lastFactEnd)) {
                  if (nextFact.CncMachineMode.Id != machineModeId) {
                    if (log.IsDebugEnabled) {
                      log.Debug ($"Get: final {lastFactEnd} found at the limit {limit}");
                    }
                    return this.CreateFinal (lastFactEnd);
                  }
                  else if (lastFactEnd < nextFact.Begin) {
                    if (log.IsDebugEnabled) {
                      log.Debug ($"Get: final {lastFactEnd} found at the limit {limit} with gap");
                    }
                    return this.CreateFinal (lastFactEnd);
                  }
                }
                if (log.IsDebugEnabled) {
                  log.Debug ($"Get: not found in {range}, limit is {limit} and there is a fact after {range} => return NoData");
                }
                return this.CreateNoData ();
              }
              else {
                return this.CreateWithHint (new UtcDateTimeRange (lastFactEnd));
              }
            }
            else {
              return this.CreateWithHint (new UtcDateTimeRange (lastFactEnd));
            }
          }
        }
      }
    }

    IDynamicTimeResponse GetInitialGap (DateTime dateTime, UtcDateTimeRange limit)
    {
      if (IsFactAfter (dateTime)) {
        if (log.IsWarnEnabled) {
          log.WarnFormat ("GetInitialGap: initial gap in fact at {0} => NotApplicable", dateTime);
        }
        return this.CreateNotApplicable ();
      }
      else {
        return this.CreatePending ();
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

      log = LogManager.GetLogger (typeof (NextMachineMode).FullName + "." + machine.Id);

      return true;
    }
  }
}
