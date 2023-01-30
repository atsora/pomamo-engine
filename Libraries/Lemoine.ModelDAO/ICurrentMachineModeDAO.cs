// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for ICurrentMachineMode.
  /// </summary>
  public interface ICurrentMachineModeDAO: IGenericUpdateDAO<ICurrentMachineMode, int>
  {
    /// <summary>
    /// Find the CurrentMachineMode item that corresponds to the specified machine
    /// </summary>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    ICurrentMachineMode Find (IMachine machine);
    
    /// <summary>
    /// Find the ICurrentMachineMode for the specified monitoredMachine
    /// with an early fetch of the machine mode
    /// </summary>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    ICurrentMachineMode FindWithMachineMode (IMachine machine);
  }
}
