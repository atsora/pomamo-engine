// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for ICncAlarmSeverityPattern.
  /// </summary>
  public interface ICncAlarmSeverityPatternDAO: IBaseGenericUpdateDAO<ICncAlarmSeverityPattern, int>
  {
    /// <summary>
    /// Find all ICncAlarmSeverityPattern for a specified severity
    /// </summary>
    /// <param name="severity"></param>
    /// <param name="withStatusDisabled">true if we also want the disabled patterns</param>
    /// <returns></returns>
    IList<ICncAlarmSeverityPattern> FindBySeverity(ICncAlarmSeverity severity, bool withStatusDisabled);
    
    /// <summary>
    /// Find all ICncAlarmSeverityPattern for a specified cnc type
    /// </summary>
    /// <param name="cncInfo"></param>
    /// <param name="withStatusDisabled">true if we also want the disabled severities</param>
    /// <returns></returns>
    IList<ICncAlarmSeverityPattern> FindByCnc(string cncInfo, bool withStatusDisabled);
    
    /// <summary>
    /// Find a ICncAlarmSeverityPattern with the specified cnc type and name
    /// </summary>
    /// <param name="cncInfo"></param>
    /// <param name="severityName"></param>
    /// <returns></returns>
    ICncAlarmSeverityPattern FindByCncName(string cncInfo, string severityName);
    
    /// <summary>
    /// Restore the default values of a cnc type
    /// </summary>
    /// <param name="cncType">cnc type to update</param>
    void RestoreDefaultValues(string cncType);
  }
}
