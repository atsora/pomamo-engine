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

namespace Lemoine.Plugin.ActivityIsProduction
{
  public class LastProductionEnd
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IDynamicTimeExtension
  {
    static readonly TimeSpan STEP = TimeSpan.FromHours (8);

    ILog log = LogManager.GetLogger (typeof (LastProductionEnd).FullName);

    static readonly string CACHE_TIME_OUT_KEY = "ActivityIsProduction.LastProductionEnd.CacheTimeOut";
    static readonly TimeSpan CACHE_TIME_OUT_DEFAULT = TimeSpan.FromSeconds (10);

    static readonly string TIMEOUT_KEY = "ActivityIsProduction.LastProductionEnd.Timeout";
    static readonly TimeSpan TIMEOUT_DEFAULT = TimeSpan.FromSeconds (30);

    Configuration m_configuration = null;

    public IMachine Machine
    {
      get; private set;
    }

    public string Name
    {
      get
      {
        var suffix = "LastProductionEnd";
        if (null == m_configuration) {
          if (!LoadConfiguration (out m_configuration)) {
            log.Error ("Name.get: LoadConfiguration failed");
            return "";
          }
        }
        if ( (null != m_configuration) && (null != m_configuration.NamePrefix)) {
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
      Debug.Assert (limit.ContainsElement (dateTime));

      var startDateTime = DateTime.UtcNow;
      var timeout = Lemoine.Info.ConfigSet.LoadAndGet (TIMEOUT_KEY, TIMEOUT_DEFAULT);

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("ActivityIsProduction.LastProductionEnd")) {
          LowerBound<DateTime> lower;
          if (m_configuration.Maximum.HasValue) {
            lower = dateTime.Subtract (m_configuration.Maximum.Value);
          }
          else {
            lower = dateTime.Subtract (TimeSpan.FromDays (10 * 365)); // 10 years
          }
          var range = new UtcDateTimeRange (lower, dateTime, "[]");
          range = new UtcDateTimeRange (range.Intersects (limit).Intersects (hint));
          var firstStepLimit = dateTime.Subtract (STEP);
          var facts = ModelDAOHelper.DAOFactory.FactDAO
            .FindOverlapsRangeDescending (this.Machine, range, STEP);
          bool isFactAfter = false;
          IFact first = null;
          foreach (var fact in facts) {
            if (timeout < DateTime.UtcNow.Subtract (startDateTime)) {
              log.Error ($"Get: timeout, start={startDateTime} VS timeout={timeout}");
              return this.CreateTimeout ();
            }

            if (null == first) {
              first = fact;
              isFactAfter = first.DateTimeRange.ContainsElement (dateTime)
                || IsFactAfter (dateTime);
            }
            if (isFactAfter) {
              if (Match (fact)) {
                Debug.Assert (fact.DateTimeRange.Upper.HasValue);
                if (fact.DateTimeRange.ContainsElement (dateTime)) {
                  log.InfoFormat ("Get: the fact at {0} is a production period, return NotApplicable", dateTime);
                  return this.CreateNotApplicable ();
                }
                else {
                  return this.CreateFinal (fact.DateTimeRange.Upper.Value);
                }
              }
            }
            else {
              if (Match (fact)) {
                Debug.Assert (fact.DateTimeRange.Upper.HasValue);
                return this.CreateWithHint (new UtcDateTimeRange (fact.DateTimeRange.Upper.Value));
              }
              if (fact.DateTimeRange.Lower.Value < firstStepLimit) {
                return this.CreatePending ();
              }
            }
          }
          if (null == first) {
            if (IsFactAfter (dateTime)) {
              if (log.IsDebugEnabled) {
                log.Debug ($"Get: no fact in range {range}");
              }
              return this.CreateNoData ();
            }
            else {
              log.Debug ("Get: waiting for some coming facts");
              return this.CreatePending ();
            }
          }
          else { // null != first
            if (isFactAfter) {
              if (log.IsDebugEnabled) {
                log.Debug ($"Get: no activity in range {range}");
              }
              return this.CreateNoData ();
            }
            else {
              log.Debug ("Get: waiting for some coming facts");
              return this.CreatePending ();
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
        return TimeSpan.MaxValue;
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

    public bool Initialize (IMachine machine, string parameter)
    {
      this.Machine = machine;
      log = LogManager.GetLogger ($"{typeof (LastProductionEnd).FullName}.{this.Machine?.Id}");
      if (string.IsNullOrEmpty (this.Name)) { // The configuration is loaded in Name.get
        return false;
      }
      Debug.Assert (null != m_configuration);
      return m_configuration.CheckMachineFilter (machine);
    }
  }
}
