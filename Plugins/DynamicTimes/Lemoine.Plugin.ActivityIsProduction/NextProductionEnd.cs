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

namespace Lemoine.Plugin.ActivityIsProduction
{
  public class NextProductionEnd
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IDynamicTimeExtension
  {
    static readonly TimeSpan STEP = TimeSpan.FromHours (8);

    static readonly string CACHE_TIME_OUT_KEY = "ActivityIsProduction.NextProductionEnd.CacheTimeOut";
    static readonly TimeSpan CACHE_TIME_OUT_DEFAULT = TimeSpan.FromSeconds (10);

    static readonly string TIMEOUT_KEY = "ActivityIsProduction.NextProductionEnd.Timeout";
    static readonly TimeSpan TIMEOUT_DEFAULT = TimeSpan.FromSeconds (30);

    ILog log = LogManager.GetLogger (typeof (NextProductionEnd).FullName);

    Configuration m_configuration;
    IEnumerable<IMachineMode> m_ignoreShortMachineModes = new List<IMachineMode> ();

    IMachineMode m_noData = null;
    DateTime? m_lastActivity = null;
    DateTime? m_shortPeriodStart = null;

    public bool Initialize (IMachine machine, string parameter)
    {
      this.Machine = machine;
      log = LogManager.GetLogger ($"{typeof (NextProductionEnd).FullName}.{this.Machine?.Id}");
      if (string.IsNullOrEmpty (this.Name)) { // The configuration is loaded in Name.get
        return false;
      }
      Debug.Assert (null != m_configuration);
      if (!m_configuration.CheckMachineFilter (machine)) {
        return false;
      }

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("ActivityIsProduction.NextProductionEnd.Initialize")) {
          this.MonitoredMachine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (this.Machine.Id);
          if (null == this.MonitoredMachine) {
            log.Fatal ($"Initialize: no monitored machine for id {this.Machine.Id}");
            return false;
          }
          if (m_configuration.IsIgnoreShortActive ()) {
            m_ignoreShortMachineModes = m_configuration.IgnoreShortMachineModeIds
              .Select (i => ModelDAOHelper.DAOFactory.MachineModeDAO.FindById (i))
              .Where (x => (null != x))
              .ToList ();
          }

          m_noData = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.NoData);
          if (null == m_noData) {
            log.Fatal ($"Initialize: machine mode no data id={MachineModeId.NoData} does not exist");
            return false;
          }
        }
      }

      return true;
    }

    public IMachine Machine
    {
      get; private set;
    }

    public IMonitoredMachine MonitoredMachine
    {
      get; private set;
    }

    public string Name
    {
      get
      {
        var suffix = "NextProductionEnd";
        if (null == m_configuration) {
          if (!LoadConfiguration (out m_configuration)) {
            log.Error ("Name.get: LoadConfiguration failed");
            return "";
          }
        }
        if ((null != m_configuration) && (null != m_configuration.NamePrefix)) {
          return m_configuration.NamePrefix + suffix;
        }
        else {
          return suffix;
        }
      }
    }

    public bool IsApplicable ()
    {
      return true;
    }

    public DynamicTimeApplicableStatus IsApplicableAt (DateTime dateTime)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("ActivityIsProduction.LastProductionEnd.IsApplicableAt")) {
          var factAt = ModelDAOHelper.DAOFactory.FactDAO
            .FindAt (this.Machine, dateTime);
          if (null != factAt) {
            if (!Match (factAt)) { // No production at date/time
              return DynamicTimeApplicableStatus.NoAtDateTime;
            }
            else {
              return DynamicTimeApplicableStatus.YesAtDateTime;
            }
          }
          else {
            if (IsFactAfter (dateTime)) {
              log.WarnFormat ("IsApplicableAt: no fact at {0}", dateTime);
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
      Debug.Assert (limit.ContainsElement (dateTime));

      var startDateTime = DateTime.UtcNow;
      var timeout = Lemoine.Info.ConfigSet.LoadAndGet (TIMEOUT_KEY, TIMEOUT_DEFAULT);
      m_shortPeriodStart = null;
      m_lastActivity = Bound.Compare<DateTime> (dateTime, hint.Lower) < 0
        ? (DateTime?)hint.Lower.Value
        : null;

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("ActivityIsProduction.NextProductionEnd")) {
          var range = new UtcDateTimeRange (limit
            .Intersects (hint)
            .Intersects (new UtcDateTimeRange (dateTime)));
          var facts = FindOverlapsRangeAscending (dateTime, hint, range);
          foreach (var fact in facts) {
            if (timeout < DateTime.UtcNow.Subtract (startDateTime)) {
              log.Error ($"Get: timeout, start={startDateTime} VS timeout={timeout}");
              return this.CreateTimeout ();
            }

            if (Match (fact)) {
              Debug.Assert (fact.DateTimeRange.Upper.HasValue);
              m_lastActivity = fact.DateTimeRange.Upper.Value;
              m_shortPeriodStart = null;
            }
            else { // Not activity
              // Is it a short period ?
              if (IsIgnoreShort (dateTime, fact)) {
                if (log.IsDebugEnabled) {
                  log.Debug ($"Get: Ignore short period {m_shortPeriodStart}-{fact.DateTimeRange.Upper}");
                }
                continue;
              }
              else if (m_shortPeriodStart.HasValue && m_shortPeriodStart.Value.Equals (dateTime)) {
                if (log.IsWarnEnabled) {
                  log.Warn ($"Get: period at {dateTime} with initial period that is not short any more => return NotApplicable");
                }
                return this.CreateNotApplicable ();
              }
              else {
                m_shortPeriodStart = null;
              }

              if (fact.Range.ContainsElement (dateTime)) { // First data
                if (log.IsWarnEnabled) {
                  log.Warn ($"Get: period at {dateTime} with machinemode {fact.CncMachineMode.Id} does not correspond to an activity and then a production => return NotApplicable");
                }
                return this.CreateNotApplicable ();
              }
              Debug.Assert (range.Lower.HasValue);
              if (m_lastActivity.HasValue) {
                return this.CreateFinal (m_lastActivity.Value);
              }
            }
          }
          if (!limit.Upper.HasValue) {
            if (m_lastActivity.HasValue) {
              return this.CreateWithHint (new UtcDateTimeRange (m_lastActivity.Value));
            }
            else {
              return this.CreatePending ();
            }
          }
          else { // limit.Upper.HasValue
            if (m_lastActivity.HasValue) {
              if ((limit.Upper.Value < m_lastActivity.Value)
                || (!limit.UpperInclusive && (limit.Upper.Value == m_lastActivity.Value))) {
                if (log.IsDebugEnabled) {
                  log.Debug ($"Get: lastActivity {m_lastActivity} while limit {limit} => return NoData");
                }
                return this.CreateNoData ();
              }
              else {
                var nextFact = ModelDAOHelper.DAOFactory.FactDAO
                  .FindFirstFactAfter (this.Machine, m_lastActivity.Value);
                if (null == nextFact) {
                  return this.CreatePending ();
                }
                else { // null != nextFact
                  if (Match (nextFact) && (nextFact.Begin == m_lastActivity.Value)) {
                    if (log.IsDebugEnabled) {
                      log.Debug ($"Get: next fact after limit is active => return NoData");
                    }
                    return this.CreateNoData ();
                  }
                  else {
                    if (limit.ContainsElement (m_lastActivity.Value)) {
                      if (log.IsDebugEnabled) {
                        log.Debug ($"Get: activity ends at {m_lastActivity} because there is a gap between it and limit {limit}");
                      }
                      return this.CreateFinal (m_lastActivity.Value);
                    }
                    else {
                      if (log.IsDebugEnabled) {
                        log.Debug ($"Get: final {m_lastActivity.Value} found but not in limit {limit}");
                      }
                      return this.CreateNoData ();
                    }
                  }
                }
              }
            }
            else { // !lastActivity.HasValue
              if (m_shortPeriodStart.HasValue) {
                if (log.IsDebugEnabled) {
                  log.Debug ($"Get: unique detected period was a short ignored one => NoData");
                }
                return this.CreateNoData ();
              }
              Debug.Assert (range.Lower.HasValue);
              var nextFact = ModelDAOHelper.DAOFactory.FactDAO
                .FindFirstFactAfter (this.Machine, range.Lower.Value);
              if (null == nextFact) {
                return this.CreateWithHint (hint);
              }
              else { // null != nextFact
                if (range.Lower.Value == nextFact.Begin) {  // No gap between range.Lower and nextFact
                  Debug.Assert (range.Lower.Value != dateTime); // Else it would be retrieved in the first request
                  Debug.Assert (range.Lower.Value == range.Upper.Value);
                  Debug.Assert (hint.Lower.HasValue);
                  Debug.Assert (limit.Upper.HasValue);
                  Debug.Assert (hint.Lower.Value == limit.Upper.Value);
                  if (Match (nextFact)) {
                    if (log.IsDebugEnabled) {
                      log.Debug ($"Get: limit {limit} before {hint} and next fact matches => NoData");
                    }
                    return this.CreateNoData ();
                  }
                  else {
                    if (log.IsDebugEnabled) {
                      log.Debug ($"Get: limit {limit} before {hint} and next fact does not match => Final is {range.Lower.Value}");
                    }
                    return this.CreateFinal (range.Lower.Value);
                  }
                }
                else { // Gap between range.Lower and nextFact
                  if (range.Lower.Value == dateTime) { // Initial gap
                    if (log.IsWarnEnabled) {
                      log.WarnFormat ("Get: initial gap {0}-{1} => return NotApplicable", dateTime, nextFact.Begin);
                    }
                    return this.CreateNotApplicable ();
                  }
                  else { // Not an initial gap => NoData
                    if (log.IsDebugEnabled) {
                      log.Debug ($"Get: gap {range.Lower.Value}-{nextFact.Begin} for dateTime {dateTime} => NoData");
                    }
                    return this.CreateNoData ();
                  }
                }
              }
            }
          }
        }
      }
    }

    IEnumerable<IFact> FindOverlapsRangeAscending (DateTime dateTime, UtcDateTimeRange hint, UtcDateTimeRange range)
    {
      DateTime? current = null;
      if (hint.ContainsElement (dateTime)) {
        current = dateTime; // Initial request
      }

      var facts = ModelDAOHelper.DAOFactory.FactDAO
        .FindOverlapsRangeAscending (this.Machine, range, STEP);
      foreach (var fact in facts) {
        if (current.HasValue
          && (Bound.Compare<DateTime> (current.Value, fact.DateTimeRange.Lower) < 0)) {
          // Gap
          if (log.IsInfoEnabled) {
            log.InfoFormat ("Get: gap {0}-{1}", current, fact.DateTimeRange.Lower);
          }
          var gapFact = ModelDAOHelper.ModelFactory
            .CreateFact (this.MonitoredMachine, current.Value, fact.Begin, m_noData);
          current = fact.Begin;
          yield return gapFact;
        }
        current = fact.End;
        yield return fact;
      }
    }

    bool IsIgnoreShort (DateTime dateTime, IFact fact)
    {
      if (!m_ignoreShortMachineModes.Any () || !m_configuration.IgnoreShortMaximumDuration.HasValue) {
        return false;
      }

      if (fact.IsShort (m_ignoreShortMachineModes, m_configuration.IgnoreShortMaximumDuration.Value)) {
        if (!m_shortPeriodStart.HasValue) {
          if (m_lastActivity.HasValue) {
            m_shortPeriodStart = m_lastActivity;
          }
          else {
            m_shortPeriodStart = dateTime;
          }
        }
        Debug.Assert (m_shortPeriodStart.HasValue);
        if (Bound.Compare<DateTime> (fact.End, m_shortPeriodStart.Value.Add (m_configuration.IgnoreShortMaximumDuration.Value)) < 0) {
          return true;
        }
      }

      return false;
    }

    bool IsFactAfter (DateTime dateTime)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var factAfter = ModelDAOHelper.DAOFactory.FactDAO
          .FindFirstFactAfter (this.Machine, dateTime);
        return (null != factAfter);
      }
    }

    bool Match (IFact fact)
    {
      return fact.IsActivity (m_configuration);
    }

    public TimeSpan GetCacheTimeout (IDynamicTimeResponse data)
    {
      if (data.Final.HasValue || data.NoData) { // This is known...
        return CacheTimeOut.Permanent.GetTimeSpan ();
      }

      if (m_configuration.CacheTimeOut.HasValue) {
        return m_configuration.CacheTimeOut.Value;
      }
      else {
        return Lemoine.Info.ConfigSet
          .LoadAndGet<TimeSpan> (CACHE_TIME_OUT_KEY,
          CACHE_TIME_OUT_DEFAULT);
      }
    }
  }
}
