// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for ICncAcquisition.
  /// </summary>
  public interface ICncAcquisitionDAO: IGenericUpdateDAO<ICncAcquisition, int>
  {
    /// <summary>
    /// Find by Id an ICncAcquisition
    /// with an eager fetch of:
    /// <item>MachineModules</item>
    /// <item>MachineModules.MonitoredMachine</item>
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    ICncAcquisition FindByIdWithMonitoredMachine (int id);
    
    /// <summary>
    /// Find all ICncAcquisition, but get also in the same all its children
    /// </summary>
    /// <returns></returns>
    IList<ICncAcquisition> FindAllWithChildren ();

    /// <summary>
    /// Find all ICncAcquisition for a given computer
    /// </summary>
    /// <returns></returns>
    IList<ICncAcquisition> FindAllForComputer (IComputer computer);
  }
}
