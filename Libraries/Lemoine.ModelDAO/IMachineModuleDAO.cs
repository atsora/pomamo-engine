// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IMachineModule.
  /// </summary>
  public interface IMachineModuleDAO: IGenericUpdateDAO<IMachineModule, int>
  {
    /// <summary>
    /// Get the machine module from its id, with an eager fetch of the monitored machine and all its machine modules
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    IMachineModule FindByIdWithMonitoredMachine (int id);
    
    /// <summary>
    /// Find all IMachineModule to configure them.
    /// 
    /// That means get also:
    /// <item>its Cnc Acquisition</item>
    /// <item>its Monitored Machine</item>
    /// </summary>
    /// <returns></returns>
    IList<IMachineModule> FindAllForConfig ();

    /// <summary>
    /// Find all IMachineModule with an eager fetch of the associated monitored machine
    /// </summary>
    /// <returns></returns>
    IList<IMachineModule> FindAllWithMonitoredMachine ();
  }
}
