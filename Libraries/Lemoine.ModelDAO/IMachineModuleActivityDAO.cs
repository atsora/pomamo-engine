// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IMachineModuleActivity.
  /// </summary>
  public interface IMachineModuleActivityDAO: IGenericByMachineModuleUpdateDAO<IMachineModuleActivity, int>
  {
    /// <summary>
    /// Get the last machine module activity for the specified machine module
    /// </summary>
    /// <param name="machineModule"></param>
    /// <returns></returns>
    IMachineModuleActivity GetLast (IMachineModule machineModule);
    
    /// <summary>
    /// Find all machine module activities in a specified UTC date/time range
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<IMachineModuleActivity> FindAllInUtcRange (IMachineModule machineModule,
                                                     UtcDateTimeRange range);

    /// <summary>
    /// Get all the activities in a specified date/time range
    /// 
    /// Order the result
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="range"></param>
    /// <param name="maxNumber">Max number of activities to retrieve</param>
    /// <returns></returns>
    IList<IAutoSequencePeriod> FindAllAutoSequencePeriodsBetween (IMachineModule machineModule,
                                                                  UtcDateTimeRange range,
                                                                  int maxNumber);
  }
}
