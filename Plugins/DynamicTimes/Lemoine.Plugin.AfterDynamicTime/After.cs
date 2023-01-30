// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions;
using Lemoine.Extensions.Business.DynamicTimes;
using Lemoine.Model;
using Lemoine.Core.Cache;

namespace Lemoine.Plugin.AfterDynamicTime
{
  public class After
    : MultipleInstanceConfigurableExtension<Configuration>
    , IDynamicTimeExtension
  {
    ILog log = LogManager.GetLogger (typeof (After).FullName);

    Configuration m_configuration;
    string m_redirectName;
    string m_afterName;
    IEnumerable<IDynamicTimeExtension> m_redirectExtensions = null; // cache
    IEnumerable<IDynamicTimeExtension> m_afterExtensions = null; // cache

    public IMachine Machine { get; private set; }

    public string Name
    {
      get
      {
        if (null == m_configuration) {
          if (!LoadConfiguration (out m_configuration)) {
            log.ErrorFormat ("Name.get: LoadConfiguration failed");
            return "";
          }
        }
        Debug.Assert (null != m_configuration); // Load successful
        return m_configuration.Name;
      }
    }

    public IDynamicTimeResponse Get (DateTime dateTime, UtcDateTimeRange hint, UtcDateTimeRange limit)
    {
      // Note: until after dynamic time, hint is not modified

      Debug.Assert (limit.ContainsElement (dateTime));

      if (!hint.ContainsElement (dateTime)) {
        // hint is set, consider redirect directly, skip the after process
        return Lemoine.Business.DynamicTimes.DynamicTime
          .GetDynamicTime (m_redirectName, this.Machine, dateTime, hint, limit);
      }

      var afterResponse = Lemoine.Business.DynamicTimes.DynamicTime.GetDynamicTime (m_afterName, this.Machine, dateTime, hint, limit);
      if (afterResponse.Timeout) {
        if (log.IsDebugEnabled) {
          log.Debug ($"Get: after dynamic time {m_afterName} return Timeout");
        }
        return afterResponse;
      }
      else if (afterResponse.NotApplicable) {
        if (m_configuration.AfterNotApplicableOk) {
          if (log.IsDebugEnabled) {
            log.Debug ($"Get: after dynamic time {m_afterName} return NotApplicable with the option AfterNotApplicableOk");
          }
          return Lemoine.Business.DynamicTimes.DynamicTime
            .GetDynamicTime (m_redirectName, this.Machine, dateTime, hint, limit);
        }
        else {
          if (log.IsDebugEnabled) {
            log.Debug ($"Get: after dynamic time {m_afterName} return NotApplicable without the option AfterNotApplicableOk");
          }
          return afterResponse;
        }
      }
      else if (afterResponse.NoData) {
        if (log.IsDebugEnabled) {
          log.Debug ($"Get: after dynamic time {m_afterName} return NoData");
        }
        return afterResponse;
      }
      else if (afterResponse.Final.HasValue) {
        if (log.IsDebugEnabled) {
          log.Debug ($"Get: after dynamic time {m_afterName} return NotApplicable with the option AfterNotApplicableOk");
        }
        if (m_configuration.AfterMaxDuration.HasValue) {
          if (m_configuration.AfterMaxDuration.Value < afterResponse.Final.Value.Subtract (dateTime)) {
            if (log.IsInfoEnabled) {
              log.Info ($"Get: after dynamic time {m_afterName} returned {afterResponse.Final} which is after max duration {m_configuration.AfterMaxDuration} => return NotApplicable");
            }
            return this.CreateNotApplicable ();
          }
        }
        return Lemoine.Business.DynamicTimes.DynamicTime
          .GetDynamicTime (m_redirectName, this.Machine, afterResponse.Final.Value, hint, limit);
      }
      else {
        if (m_configuration.AfterMaxDuration.HasValue
          && afterResponse.Hint.Lower.HasValue
          && (m_configuration.AfterMaxDuration.Value < afterResponse.Hint.Lower.Value.Subtract (dateTime))) {
          if (log.IsInfoEnabled) {
            log.Info ($"Get: after dynamic time {m_afterName} returned hint {afterResponse.Hint} which is after max duration {m_configuration.AfterMaxDuration} => return NotApplicable");
          }
          return this.CreateNotApplicable ();
        }
        if (log.IsDebugEnabled) {
          log.Debug ($"Get: after dynamic time {m_afterName} returned {afterResponse} => return Pending");
        }
        return this.CreatePending ();
      }
    }

    public TimeSpan GetCacheTimeout (IDynamicTimeResponse data)
    {
      if (data.Final.HasValue || data.NoData) { // This is known...
        return TimeSpan.MaxValue;
      }

      var redirectExtensions = GetRedirectExtensions ();
      if (!redirectExtensions.Any ()) {
        log.Error ($"GetCacheTimeOut: no extension {m_redirectName} is registered");
        return CacheTimeOut.Config.GetTimeSpan ();
      }
      var afterExtensions = GetAfterExtensions ();
      if (!redirectExtensions.Any ()) {
        log.Error ($"GetCacheTimeOut: no extension {m_redirectName} is registered");
        return CacheTimeOut.Config.GetTimeSpan ();
      }
      var extensions = redirectExtensions.Concat (afterExtensions);
      return extensions.Min (ext => ext.GetCacheTimeout (data));
    }

    public bool Initialize (IMachine machine, string parameter)
    {
      this.Machine = machine;

      if (string.IsNullOrEmpty (this.Name)) {
        // The configuration is loaded in Name.get
        return false;
      }

      Debug.Assert (null != m_configuration);

      m_redirectName = m_configuration.RedirectName;
      m_afterName = m_configuration.AfterName;

      return !string.IsNullOrEmpty (this.Name)
        && !string.IsNullOrEmpty (m_redirectName)
        && !string.IsNullOrEmpty (m_afterName);
    }

    IEnumerable<IDynamicTimeExtension> GetRedirectExtensions ()
    {
      if (null == m_redirectExtensions) {
        m_redirectExtensions = Lemoine.Business.DynamicTimes.DynamicTime.GetExtensions (m_redirectName, this.Machine);
      }
      return m_redirectExtensions;
    }

    IEnumerable<IDynamicTimeExtension> GetAfterExtensions ()
    {
      if (null == m_afterExtensions) {
        m_afterExtensions = Lemoine.Business.DynamicTimes.DynamicTime.GetExtensions (m_afterName, this.Machine);
      }
      return m_redirectExtensions;
    }

    public bool IsApplicable ()
    {
      return GetRedirectExtensions ().Any (ext => Lemoine.Business.DynamicTimes.DynamicTime.IsApplicable (ext))
         && GetAfterExtensions ().Any (ext => Lemoine.Business.DynamicTimes.DynamicTime.IsApplicable (ext));
    }

    public DynamicTimeApplicableStatus IsApplicableAt (DateTime dateTime)
    {
      var redirectExtensions = GetRedirectExtensions ();
      if (!redirectExtensions.Any ()) {
        log.Error ($"IsApplicableAt: no redirect extension");
        return DynamicTimeApplicableStatus.Never;
      }
      var afterExtensions = GetAfterExtensions ();
      if (!afterExtensions.Any ()) {
        log.Error ($"IsApplicableAt: no after extension");
        return DynamicTimeApplicableStatus.Never;
      }
      if (!m_configuration.AfterNotApplicableOk) {
        switch (Lemoine.Business.DynamicTimes.DynamicTime.IsApplicableAt (afterExtensions, this.Machine, dateTime)) {
        case DynamicTimeApplicableStatus.NoAtDateTime:
          return DynamicTimeApplicableStatus.NoAtDateTime;
        case DynamicTimeApplicableStatus.Never:
          return DynamicTimeApplicableStatus.Never;
        default:
          break;
        }
      }
      return DynamicTimeApplicableStatus.Pending;
    }

  }
}
