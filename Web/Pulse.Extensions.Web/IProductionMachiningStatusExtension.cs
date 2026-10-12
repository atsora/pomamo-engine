// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Business.Operation;
using Lemoine.Model;
using System;
using System.Collections.Generic;

namespace Pulse.Extensions.Web
{
  /// <summary>
  /// Extension to the ProductionMachiningStatus web service
  /// that adds active and coming events, computed from the part production data of the current shift
  ///
  /// The events are only requested when the IncludeEvents option is set,
  /// and for a group, only when it is made of a single machine.
  /// For events that are based on the progress of the current cycle or operation
  /// (CycleProgress and OperationProgress web services), see <see cref="IProgressExtension"/>
  /// </summary>
  public interface IProductionMachiningStatusExtension
    : Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Initialize the extension for a specific machine.
    ///
    /// The initialized extensions are cached per machine by the web service
    /// </summary>
    /// <param name="machine">monitored machine the events are requested for (not null)</param>
    /// <returns>true if the extension is active for this machine, else false</returns>
    bool Initialize (IMonitoredMachine machine);

    /// <summary>
    /// Get the events that are currently active on the machine (for example: no part produced for a too long time)
    ///
    /// Note: this is called after <see cref="GetComingEvents"/>
    /// </summary>
    /// <param name="partProductionCurrentShiftResponse">part production data of the current shift (number of parts, goal, cycle duration target...)</param>
    /// <returns>list of active events, empty if none (not null)</returns>
    IList<Event> GetActiveEvents (Lemoine.Extensions.Business.IPartProductionDataCurrentShift partProductionCurrentShiftResponse);

    /// <summary>
    /// Get the events that are expected to occur soon on the machine
    ///
    /// Note: this is called first, before <see cref="GetActiveEvents"/>.
    /// If there are coming events but no active event, the web service also returns
    /// whether the machine is currently running, so that the client can display the coming events accordingly
    /// </summary>
    /// <param name="partProductionCurrentShiftResponse">part production data of the current shift (number of parts, goal, cycle duration target...)</param>
    /// <returns>list of coming events, empty if none (not null)</returns>
    IList<Event> GetComingEvents (Lemoine.Extensions.Business.IPartProductionDataCurrentShift partProductionCurrentShiftResponse);
  }
}
