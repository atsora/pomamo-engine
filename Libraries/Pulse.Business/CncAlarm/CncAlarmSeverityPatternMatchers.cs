// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Business.CncAlarm
{
  /// <summary>
  /// Matcher of a cnc alarm severity pattern
  ///
  /// It reproduces in memory the SQL function cncalarmseverityid(cncalarm/currentcncalarm):
  /// <item>cncsubinfo and type are citext columns: the regular expression is case insensitive</item>
  /// <item>number and message are text columns: the regular expression is case sensitive</item>
  /// <item>the properties of the pattern must be contained in the properties of the alarm (jsonb @&gt;)</item>
  /// </summary>
  public sealed class CncAlarmSeverityPatternMatcher
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CncAlarmSeverityPatternMatcher).FullName);

    readonly Regex m_cncSubInfo;
    readonly Regex m_type;
    readonly Regex m_number;
    readonly Regex m_message;
    readonly IDictionary<string, string> m_properties;
    readonly bool m_valid = true;

    /// <summary>
    /// Pattern id
    /// </summary>
    public int PatternId { get; }

    /// <summary>
    /// Associated severity (initialized, not null)
    /// </summary>
    public ICncAlarmSeverity Severity { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="pattern">not null, with an initialized severity</param>
    internal CncAlarmSeverityPatternMatcher (ICncAlarmSeverityPattern pattern)
    {
      Debug.Assert (null != pattern);
      Debug.Assert (null != pattern.Severity);

      this.PatternId = pattern.Id;
      this.Severity = pattern.Severity;

      var rules = pattern.Rules ?? new CncAlarmSeverityPatternRules ();
      try {
        m_cncSubInfo = CreateRegex (rules.CncSubInfo, true);
        m_type = CreateRegex (rules.Type, true);
        m_number = CreateRegex (rules.Number, false);
        m_message = CreateRegex (rules.Message, false);
      }
      catch (ArgumentException ex) {
        log.Error ($"CncAlarmSeverityPatternMatcher: invalid regular expression in pattern id={pattern.Id} => the pattern is ignored", ex);
        m_valid = false;
      }
      m_properties = rules.Properties ?? new Dictionary<string, string> ();
    }

    static Regex CreateRegex (string pattern, bool ignoreCase)
    {
      if (string.IsNullOrEmpty (pattern)) {
        return null;
      }
      var options = RegexOptions.CultureInvariant | RegexOptions.Compiled;
      if (ignoreCase) {
        options |= RegexOptions.IgnoreCase;
      }
      return new Regex (pattern, options);
    }

    /// <summary>
    /// Check if the alarm attributes match the pattern
    ///
    /// The cnc info is supposed to have already been checked
    /// </summary>
    /// <param name="cncSubInfo"></param>
    /// <param name="type"></param>
    /// <param name="number"></param>
    /// <param name="message"></param>
    /// <param name="properties"></param>
    /// <returns></returns>
    public bool IsMatch (string cncSubInfo, string type, string number, string message, IDictionary<string, object> properties)
    {
      return m_valid
        && IsMatch (m_cncSubInfo, cncSubInfo)
        && IsMatch (m_type, type)
        && IsMatch (m_number, number)
        && IsMatch (m_message, message)
        && ArePropertiesContained (properties);
    }

    static bool IsMatch (Regex regex, string v)
    {
      if (null == regex) {
        return true;
      }
      // In SQL, NULL ~ 'pattern' is not true
      return (null != v) && regex.IsMatch (v);
    }

    bool ArePropertiesContained (IDictionary<string, object> properties)
    {
      foreach (var property in m_properties) {
        if ((null == properties) || !properties.TryGetValue (property.Key, out var v)) {
          return false;
        }
        // jsonb containment: a json string in the pattern only matches a json string with the same value
        if (!(v is string s) || !string.Equals (s, property.Value, StringComparison.Ordinal)) {
          return false;
        }
      }
      return true;
    }
  }

  /// <summary>
  /// Request class to get the active cnc alarm severity pattern matchers of a cnc
  ///
  /// Both the pattern and its severity must not be deactivated
  /// </summary>
  public sealed class CncAlarmSeverityPatternMatchers
    : IRequest<IList<CncAlarmSeverityPatternMatcher>>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CncAlarmSeverityPatternMatchers).FullName);

    /// <summary>
    /// Cnc info
    /// </summary>
    string CncInfo { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="cncInfo">not null or empty</param>
    public CncAlarmSeverityPatternMatchers (string cncInfo)
    {
      Debug.Assert (!string.IsNullOrEmpty (cncInfo));

      this.CncInfo = cncInfo;
    }

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns>not null, ordered by pattern id</returns>
    public IList<CncAlarmSeverityPatternMatcher> Get ()
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"Get: cnc info {this.CncInfo}");
      }

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Business.CncAlarm.CncAlarmSeverityPatternMatchers")) {
          var patterns = ModelDAOHelper.DAOFactory.CncAlarmSeverityPatternDAO
            .FindByCnc (this.CncInfo, false);
          var result = new List<CncAlarmSeverityPatternMatcher> ();
          foreach (var pattern in patterns.OrderBy (p => p.Id)) {
            var severity = pattern.Severity;
            if (null == severity) {
              log.Error ($"Get: no severity for pattern id={pattern.Id}");
              continue;
            }
            ModelDAOHelper.DAOFactory.Initialize (severity);
            if (EditStatus.DEFAULT_VALUE_DELETED == severity.Status) {
              continue;
            }
            result.Add (new CncAlarmSeverityPatternMatcher (pattern));
          }
          return result;
        }
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public Task<IList<CncAlarmSeverityPatternMatcher>> GetAsync ()
    {
      return Task.FromResult (Get ());
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return "Business.CncAlarm.CncAlarmSeverityPatternMatchers." + this.CncInfo.ToLowerInvariant ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/>
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<IList<CncAlarmSeverityPatternMatcher>> data)
    {
      return true;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (IList<CncAlarmSeverityPatternMatcher> data)
    {
      return CacheTimeOut.Config.GetTimeSpan ();
    }
    #endregion // IRequest implementation
  }
}
