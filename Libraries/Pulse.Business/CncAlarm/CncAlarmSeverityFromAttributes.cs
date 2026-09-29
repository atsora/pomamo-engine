// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Lemoine.Business.CncAlarm
{
  /// <summary>
  /// Request class to get the severity of a cnc alarm (or current cnc alarm) from its attributes:
  /// cnc info, cnc sub-info, type, number, message and properties.
  ///
  /// This is an alternative to the costly dynamic column cncalarmseverityid of tables cncalarm and currentcncalarm.
  /// The result is cached considering these attributes.
  ///
  /// If several patterns match, the one with the lowest id is considered.
  /// </summary>
  public sealed class CncAlarmSeverityFromAttributes
    : IRequest<ICncAlarmSeverity>
  {
    static readonly char SEPARATOR = '\u001F';

    static readonly ILog log = LogManager.GetLogger (typeof (CncAlarmSeverityFromAttributes).FullName);

    readonly string m_cncInfo;
    readonly string m_cncSubInfo;
    readonly string m_type;
    readonly string m_number;
    readonly string m_message;
    readonly IDictionary<string, object> m_properties;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="cncInfo"></param>
    /// <param name="cncSubInfo"></param>
    /// <param name="type"></param>
    /// <param name="number"></param>
    /// <param name="message"></param>
    /// <param name="properties"></param>
    public CncAlarmSeverityFromAttributes (string cncInfo, string cncSubInfo, string type, string number, string message, IDictionary<string, object> properties)
    {
      m_cncInfo = cncInfo;
      m_cncSubInfo = cncSubInfo;
      m_type = type;
      m_number = number;
      m_message = message;
      m_properties = properties;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="cncAlarm">not null</param>
    public CncAlarmSeverityFromAttributes (ICncAlarm cncAlarm)
      : this (cncAlarm.CncInfo, cncAlarm.CncSubInfo, cncAlarm.Type, cncAlarm.Number, cncAlarm.Message, cncAlarm.Properties)
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="currentCncAlarm">not null</param>
    public CncAlarmSeverityFromAttributes (ICurrentCncAlarm currentCncAlarm)
      : this (currentCncAlarm.CncInfo, currentCncAlarm.CncSubInfo, currentCncAlarm.Type, currentCncAlarm.Number, currentCncAlarm.Message, currentCncAlarm.Properties)
    {
    }

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns>Severity or null if no pattern matches</returns>
    public ICncAlarmSeverity Get ()
    {
      if (string.IsNullOrEmpty (m_cncInfo)) {
        return null;
      }

      var matchers = ServiceProvider.Get (new CncAlarmSeverityPatternMatchers (m_cncInfo));
      var matcher = matchers
        .FirstOrDefault (m => m.IsMatch (m_cncSubInfo, m_type, m_number, m_message, m_properties));
      if (log.IsDebugEnabled) {
        log.Debug ($"Get: pattern id={matcher?.PatternId} for key {GetCacheKey ()}");
      }
      return matcher?.Severity;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async Task<ICncAlarmSeverity> GetAsync ()
    {
      if (string.IsNullOrEmpty (m_cncInfo)) {
        return null;
      }

      var matchers = await ServiceProvider.GetAsync (new CncAlarmSeverityPatternMatchers (m_cncInfo));
      var matcher = matchers
        .FirstOrDefault (m => m.IsMatch (m_cncSubInfo, m_type, m_number, m_message, m_properties));
      return matcher?.Severity;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    ///
    /// cncinfo, cncsubinfo and type are case insensitive (citext).
    /// Only the properties with a string value may match a pattern.
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      var key = new StringBuilder ("Business.CncAlarm.CncAlarmSeverityFromAttributes.");
      key.Append (m_cncInfo?.ToLowerInvariant ()).Append (SEPARATOR);
      key.Append (m_cncSubInfo?.ToLowerInvariant ()).Append (SEPARATOR);
      key.Append (m_type?.ToLowerInvariant ()).Append (SEPARATOR);
      key.Append (m_number).Append (SEPARATOR);
      key.Append (null == m_message ? "\0" : m_message);
      if (null != m_properties) {
        foreach (var property in m_properties
          .Where (p => p.Value is string)
          .OrderBy (p => p.Key, StringComparer.Ordinal)) {
          key.Append (SEPARATOR).Append (property.Key).Append ('=').Append ((string)property.Value);
        }
      }
      return key.ToString ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/>
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<ICncAlarmSeverity> data)
    {
      return true;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (ICncAlarmSeverity data)
    {
      return CacheTimeOut.Config.GetTimeSpan ();
    }
    #endregion // IRequest implementation
  }
}
