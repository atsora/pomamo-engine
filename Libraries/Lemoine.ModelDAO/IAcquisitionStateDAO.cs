// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Model;
using System.Collections.Generic;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IAcquisitionState.
  /// </summary>
  public interface IAcquisitionStateDAO : IGenericUpdateDAO<IAcquisitionState, int>
  {
    /// <summary>
    /// Get an acquisition state
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="key">not null or empty</param>
    /// <returns>One element or null</returns>
    IAcquisitionState GetAcquisitionState (IMachineModule machineModule, AcquisitionStateKey key);

    /// <summary>
    /// Find by machine module
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <returns></returns>
    IList<IAcquisitionState> FindByMachineModule (IMachineModule machineModule);
  }
}
