// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using System.Collections.Generic;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for ICurrentCncValue.
  /// </summary>
  public interface ICurrentCncValueDAO: IGenericUpdateDAO<ICurrentCncValue, int>
  {
    /// <summary>
    /// Find the CurrentCncValue item that corresponds to the specified machine module and field
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="field"></param>
    /// <returns></returns>
    ICurrentCncValue Find (IMachineModule machineModule, IField field);

    /// <summary>
    /// Find all the CurrentCncValue item that corresponds to the specified machine module
    /// </summary>
    /// <param name="machineModule"></param>
    /// <returns></returns>
    IList<ICurrentCncValue> FindByMachineModule (IMachineModule machineModule);
  }
}
