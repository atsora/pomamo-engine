// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IMachineFilter.
  /// </summary>
  public interface IMachineFilterDAO: IGenericUpdateDAO<IMachineFilter, int>
  {
    /// <summary>
    /// Find all IMachineFilter with Eager Mode for Items (MachineFilterItem) Container.
    /// </summary>
    /// <returns></returns>
    IList<IMachineFilter> FindAllMachineFilterForConfig();
    
    /// <summary>
    /// Use to delete a MachienFilterItem from Item Collection linked to a MachineFilter.
    /// </summary>
    /// <param name="machineFilterItemToDelete"></param>
    void DeleteMachineFilterItem(IMachineFilterItem machineFilterItemToDelete);
  }
}
