// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for ICncAlarmSeverity.
  /// </summary>
  public interface ICncAlarmSeverityDAO: IBaseGenericUpdateDAO<ICncAlarmSeverity, int>
  {
    /// <summary>
    /// Find by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    ICncAlarmSeverity FindById (int id);

    /// <summary>
    /// Find all ICncAlarmSeverity for a specified cnc type
    /// </summary>
    /// <param name="cncInfo"></param>
    /// <param name="withStatusDisabled">true if we also want the disabled severities</param>
    /// <returns></returns>
    IList<ICncAlarmSeverity> FindByCnc(string cncInfo, bool withStatusDisabled);
    
    /// <summary>
    /// Find a ICncAlarmSeverity with the specified cnc type and name
    /// </summary>
    /// <param name="cncInfo"></param>
    /// <param name="severityName"></param>
    /// <returns></returns>
    ICncAlarmSeverity FindByCncName(string cncInfo, string severityName);
    
    /// <summary>
    /// Return true if default values are available for a CNC
    /// </summary>
    /// <param name="cncType"></param>
    bool AreDefaultValuesAvailable(string cncType);
    
    /// <summary>
    /// Restore the default values of a cnc type
    /// </summary>
    /// <param name="cncType">cnc type to update</param>
    void RestoreDefaultValues(string cncType);
  }
}
