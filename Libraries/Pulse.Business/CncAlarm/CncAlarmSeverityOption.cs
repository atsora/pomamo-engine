// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

namespace Lemoine.Business.CncAlarm
{
  /// <summary>
  /// Option to get the severity of the cnc alarms and current cnc alarms
  /// from the cached business request <see cref="CncAlarmSeverityFromAttributes"/>
  /// instead of from the costly dynamic column cncalarmseverityid
  ///
  /// It is used by the web services CncAlarm/At, CncAlarm/Current, CncAlarm/Color
  /// and by the plugin SignalCncAlarmSeverity
  /// </summary>
  public static class CncAlarmSeverityOption
  {
    static readonly string BUSINESS_SEVERITY_KEY = "Web.CncAlarm.BusinessSeverity";
    static readonly bool BUSINESS_SEVERITY_DEFAULT = true;

    /// <summary>
    /// Is the severity computed by the business request (and not by the dynamic column)?
    /// </summary>
    /// <returns></returns>
    public static bool IsBusinessSeverity ()
    {
      return Lemoine.Info.ConfigSet
        .LoadAndGet<bool> (BUSINESS_SEVERITY_KEY, BUSINESS_SEVERITY_DEFAULT);
    }
  }
}
