// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading;
using Lemoine.Model;
using Lemoine.Threading;

namespace Pulse.Extensions.Database
{
  /// <summary>
  /// 
  /// </summary>
  public interface ICncActivityExtension
     : Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Initialize the extension
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    bool Initialize (IMonitoredMachine machine);

    /// <summary>
    /// Priority in which the extensions are executed
    /// 
    /// The extensions are run in ascending priority order
    /// </summary>
    double Priority { get; }

    /// <summary>
    /// Additional action to the ProcessAssociation method of CncActivityMachineAssociation class
    /// </summary>
    /// <param name="checkedThread">not null</param>
    /// <param name="range">Lower and Upper have values</param>
    /// <param name="machineMode">not null</param>
    /// <param name="machineStateTemplate">nullable</param>
    /// <param name="machineObservationState">not null</param>
    /// <param name="shift">nullable</param>
    /// <param name="machineStatus">nullable</param>
    /// <param name="cancellationToken"></param>
    /// <returns>If false, interrupt the process, the observation state slots must be reloaded</returns>
    bool ProcessAssociation (IChecked checkedThread, UtcDateTimeRange range, IMachineMode machineMode, IMachineStateTemplate machineStateTemplate, IMachineObservationState machineObservationState, IShift shift, IMachineStatus machineStatus, CancellationToken cancellationToken = default);
  }
}
