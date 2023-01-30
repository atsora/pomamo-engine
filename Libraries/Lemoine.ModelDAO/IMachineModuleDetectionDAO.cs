// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IMachineModuleDetection.
  /// </summary>
  public interface IMachineModuleDetectionDAO: IGenericByMachineModuleDAO<IMachineModuleDetection, int>
  {
    /// <summary>
    /// Get the list of IMachineModuleDetection that were inserted after the specified id
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="lastId"></param>
    /// <param name="maxNumber">Maximum number of rows to retrieve</param>
    /// <returns></returns>
    IList<IMachineModuleDetection> FindAfter (IMachineModule machineModule, int lastId, int maxNumber);
    
    /// <summary>
    /// Remove the items that are older than the specified age and whose id is smaller than the specified ID
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="beforeId"></param>
    /// <param name="maxAge"></param>
    void Clean (IMachineModule machineModule, int beforeId, TimeSpan maxAge);
  }
}
