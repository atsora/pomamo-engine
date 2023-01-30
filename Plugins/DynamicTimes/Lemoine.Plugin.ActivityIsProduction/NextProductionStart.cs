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
  public class NextProductionStart
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IDynamicTimeExtension
  {
    static readonly TimeSpan STEP = TimeSpan.FromHours (8);

    static readonly string CACHE_TIME_OUT_KEY = "ActivityIsProduction.NextProductionStart.CacheTimeOut";
    static readonly TimeSpan CACHE_TIME_OUT_DEFAULT = TimeSpan.FromSeconds (10);

    static readonly string TIMEOUT_KEY = "ActivityIsProduction.NextProductionStart.Timeout";
    static readonly TimeSpan TIMEOUT_DEFAULT = TimeSpan.FromSeconds (30);

    ILog log = LogManager.GetLogger (typeof (NextProductionStart).FullName);

    Configuration m_configuration;

    public bool Initialize (IMachine machine, string parameter)
    {
      this.Machine = machine;
      log = LogManager.GetLogger ($"{typeof (NextProductionStart).FullName}.{this.Machine?.Id}");
      if (string.IsNullOrEmpty (this.Name)) { // The configuration is loaded in Name.get
        return false;
      }
      Debug.Assert (null != m_configuration);
      return m_configuration.CheckMachineFilter (machine);
    }

    public IMachine Machine
    {
      get; private set;
    }

    public string Name
    {
      get
      {
        var suffix = "NextProductionStart";
        if (null == m_configuration) {
          if (!LoadConfiguration (out m_configuration)) {
            log.ErrorFormat ("Name.get: LoadConfiguration failed");
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
        using (var transaction = session.BeginReadOnlyTransaction ("ActivityIsProduction.NextProductionStart.IsApplicableAt")) {
          var factAt = ModelDAOHelper.DAOFactory.FactDAO
            .FindAt (this.Machine, dateTime);
          if (null != factAt) {
            if (Match (factAt)) { // Production at date/time
              return DynamicTimeApplicableStatus.NoAtDateTime;
            }
            else {
              return DynamicTimeApplicableStatus.YesAtDateTime;
            }
          }
          else {
            if (IsFactAfter (dateTime)) {
              log.WarnFormat ("IsApplicableAt: no fact at {0}", dateTime);
              return DynamicTimeApplicableStatus.YesAtDateTime;
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

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("ActivityIsProduction.NextProductionStart")) {
          var range = new UtcDateTimeRange (limit
            .Intersects (hint)
            .Intersects (new UtcDateTimeRange (dateTime)));
          var facts = ModelDAOHelper.DAOFactory.FactDAO
            .FindOverlapsRangeAscending (this.Machine, range, STEP);
          DateTime? afterResponse = null;
          foreach (var fact in facts) {
            if (timeout < DateTime.UtcNow.Subtract (startDateTime)) {
              log.Error ($"Get: timeout, start={startDateTime} VS timeout={timeout}");
              return this.CreateTimeout ();
            }

            if (Match (fact)) {
              if (fact.DateTimeRange.ContainsElement (dateTime)) {
                log.InfoFormat ("Get: the fact at {0} is a production period, return NotApplicable", dateTime);
                return this.CreateNotApplicable ();
              }
              Debug.Assert (fact.DateTimeRange.Lower.HasValue);
              return this.CreateFinal (fact.DateTimeRange.Lower.Value);
            }
            else {
              Debug.Assert (fact.DateTimeRange.Upper.HasValue);
              afterResponse = fact.DateTimeRange.Upper.Value;
            }
          } // Loop on facts

          if (afterResponse.HasValue) {
            if (!limit.ContainsElement (afterResponse.Value)) {
              if (log.IsDebugEnabled) {
                log.Debug ($"Get: afterResponse {afterResponse} not in limit {limit} => NoData");
              }
              return this.CreateNoData ();
            }
            else {
              var newHint = new UtcDateTimeRange (hint
              .Intersects (new UtcDateTimeRange (afterResponse.Value)));
              if (log.IsDebugEnabled) {
                log.Debug ($"Get: no matching fact, afterResponse is {afterResponse} => return Hint ({newHint})");
              }
              return this.CreateWithHint (newHint);
            }
          }
          else { // !afterResonse.HasValue
            var nextFact = ModelDAOHelper.DAOFactory.FactDAO
              .FindFirstFactAfter (this.Machine, dateTime);
            if (null == nextFact) {
              if (log.IsDebugEnabled) {
                log.Debug ($"Get: no fact after {dateTime} => return initial Hint ({hint})");
              }
              return this.CreateWithHint (hint);
            }
            else {
              Debug.Assert (Bound.Compare<DateTime> (range.Upper, nextFact.Begin) <= 0);
              if (Match (nextFact)) {
                if (!limit.ContainsElement (nextFact.Begin)) {
                  return this.CreateNoData ();
                }
                else { // This should not happen because the fact should have been returned sooner
                  Debug.Assert (false);
                  return this.CreateFinal (nextFact.Begin);
                }
              }
              else {
                if (!limit.ContainsElement (nextFact.End)) {
                  return this.CreateNoData ();
                }
                else {
                  return this.CreateWithHint (new UtcDateTimeRange (nextFact.End));
                }
              }
            }
          }
        }
      }
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
