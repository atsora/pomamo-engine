// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IAutoMachineStateTemplate.
  /// </summary>
  public interface IAutoMachineStateTemplateDAO: IGenericUpdateDAO<IAutoMachineStateTemplate, int>
  {
    /// <summary>
    /// Find the unique IAutoMachineStateTemplate that matches exactly the specified machine mode
    /// and machine state template
    /// 
    /// Note: The request is not cacheable because there might be (to confirm)
    /// problems with a lazy loading of MachineStateTemplate with the cache
    /// </summary>
    /// <param name="machineMode">not null</param>
    /// <param name="currentMachineStateTemplate">not null</param>
    /// <returns></returns>
    IAutoMachineStateTemplate Find (IMachineMode machineMode,
                                    IMachineStateTemplate currentMachineStateTemplate);
    
    /// <summary>
    /// Find the unique IAutoMachineStateTemplate that matches exactly the specified machine mode
    /// with a 'any' machine state template criteria
    /// 
    /// The request is cacheable
    /// </summary>
    /// <param name="machineMode">not null</param>
    /// <returns></returns>
    IAutoMachineStateTemplate Find (IMachineMode machineMode);
  }
}
