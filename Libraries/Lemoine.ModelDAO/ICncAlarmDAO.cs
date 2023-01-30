// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for ICncAlarm.
  /// </summary>
  public interface ICncAlarmDAO: IGenericByMachineModuleUpdateDAO<ICncAlarm, int>
  {
    /// <summary>
    /// Find all ICncAlarm for a specified machineModule
    /// </summary>
    /// <param name="machineModule"></param>
    /// <returns></returns>
    IList<ICncAlarm> FindByMachineModule(IMachineModule machineModule);
    
    /// <summary>
    /// Find the active cnc alarms at the specified time for the specified machine module
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    IList<ICncAlarm> FindAt (IMachineModule machineModule,
                             DateTime dateTime);

    /// <summary>
    /// Find the active cnc alarms at the specified time for the specified machine module with an early fetch of the severity
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    IList<ICncAlarm> FindAtWithSeverity (IMachineModule machineModule,
                                         DateTime dateTime);
    
    /// <summary>
    /// Find all the slots that overlap the specified range
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<ICncAlarm> FindOverlapsRange (IMachineModule machineModule, UtcDateTimeRange range);

    /// <summary>
    /// Find all the slots that overlap the specified range with an eager fetch of the severity
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<ICncAlarm> FindOverlapsRangeWithSeverity (IMachineModule machineModule, UtcDateTimeRange range);
    
    /// <summary>
    /// Find all the slots that overlap the specified range
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    IEnumerable<ICncAlarm> FindOverlapsRange (IMonitoredMachine machine, UtcDateTimeRange range);

    /// <summary>
    /// Find all the slots that overlap the specified range with an eager fetch of the severity
    /// 
    /// Order them by ascending range
    /// 
    /// Be careful using this method. It looks like it is inefficient.
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    IEnumerable<ICncAlarm> FindOverlapsRangeWithSeverity (IMonitoredMachine machine, UtcDateTimeRange range);

    /// <summary>
    /// Get all cnc types that have been found 
    /// </summary>
    /// <returns></returns>
    IList<string> FindAllCncTypes();

    /// <summary>
    /// Get all alarms of a particular cnc in a specific range (should not be too large)
    /// The severity is NOT loaded
    /// </summary>
    /// <param name="cncType">null or empty is all cnc</param>
    /// <param name="range"></param>
    /// <returns></returns>
    IEnumerable<ICncAlarm> FindByCncRange (string cncType, UtcDateTimeRange range);

    /// <summary>
    /// Get all alarms of a particular cnc in a specific range (should not be too large)
    /// The severity is NOT loaded
    /// </summary>
    /// <param name="machineModule">not null or empty</param>
    /// <param name="cncType">null or empty is all cnc</param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<ICncAlarm> FindByCncRange (IMachineModule machineModule, string cncType, UtcDateTimeRange range);

    /// <summary>
    /// Get all alarms of a particular cnc in a specific range (should not be too large)
    /// The severity is loaded
    /// </summary>
    /// <param name="cncType">null or empty is all cnc</param>
    /// <param name="range"></param>
    /// <returns></returns>
    IEnumerable<ICncAlarm> FindByCncRangeWithSeverity (string cncType, UtcDateTimeRange range);

    /// <summary>
    /// Get all alarms of a particular cnc in a specific range (should not be too large)
    /// The severity is loaded
    /// </summary>
    /// <param name="machineModule">not null or empty</param>
    /// <param name="cncType">null or empty is all cnc</param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<ICncAlarm> FindByCncRangeWithSeverity (IMachineModule machineModule, string cncType, UtcDateTimeRange range);

    /// <summary>
    /// Find the next ICncAlarms for a specified machineModule
    /// whose ID is strictly more than the specified ID
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="id"></param>
    /// <param name="maxNbFetched"></param>
    /// <returns></returns>
    IList<ICncAlarm> FindAfterID(IMachineModule machineModule, int id, int maxNbFetched);
    
    /// <summary>
    /// Find all the slots whose beginning is within the specified range
    /// Order them by ascending range
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<ICncAlarm> FindWithBeginningInRange(IMachineModule machineModule, UtcDateTimeRange range);

    /// <summary>
    /// Find all the slots whose beginning is within the specified range
    /// Order them by ascending range
    /// The severity is loaded
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<ICncAlarm> FindWithBeginningInRangeWithSeverity (IMachineModule machineModule, UtcDateTimeRange range);

    /// <summary>
    /// Find the first alarm occurring after a specific datetime
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="datetime"></param>
    /// <returns>Can be null</returns>
    ICncAlarm FindFirstAfter(IMachineModule machineModule, DateTime datetime);
  }
}
