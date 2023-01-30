// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IToolPosition.
  /// </summary>
  public interface IToolPositionDAO: IGenericByMachineModuleUpdateDAO<IToolPosition, int>
  {
    /// <summary>
    /// Find by machine module
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <returns></returns>
    IList<IToolPosition> FindByMachineModule(IMachineModule machineModule);
    
    /// <summary>
    /// Get a specific position (machine module, magazine, pot)
    /// </summary>
    /// <param name="machineModule">cannot be null</param>
    /// <param name="magazine">can be null</param>
    /// <param name="pot">can be null</param>
    /// <returns></returns>
    IToolPosition FindByPosition(IMachineModule machineModule, int? magazine, int? pot);
  }
}
