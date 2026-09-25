// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

namespace Pulse.Web.CncAlarm
{
  /// <summary>
  /// Option to get the severity of the cnc alarms and current cnc alarms
  /// from the cached business request Lemoine.Business.CncAlarm.CncAlarmSeverityFromAttributes
  /// instead of from the costly dynamic column cncalarmseverityid
  /// </summary>
  internal static class CncAlarmSeverityOption
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
