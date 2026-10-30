// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IEventCncValue.
  /// </summary>
  public interface IEventCncValueDAO: IGenericDAO<IEventCncValue, int>
  {
    /// <summary>
    /// Find all the EventCncValue corresponding to a specified config
    /// </summary>
    /// <param name="config"></param>
    /// <returns></returns>
    IList<IEventCncValue> FindWithConfig (IEventCncValueConfig config);

    /// <summary>
    /// Find the events of some machine modules in a range, the oldest first
    /// </summary>
    /// <param name="machineModuleIds">not null</param>
    /// <param name="range">not null</param>
    /// <returns>not null</returns>
    System.Threading.Tasks.Task<IList<IEventCncValue>> FindInRangeAsync (int[] machineModuleIds, UtcDateTimeRange range);
  }
}
