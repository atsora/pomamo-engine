// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Business.Operation;
using Lemoine.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pulse.Extensions.Web
{
  /// <summary>
  /// Extension to the CycleProgress and OperationProgress web services
  /// that adds active and coming events, computed from the progress of the current cycle or operation
  ///
  /// The same implementation is used by both web services,
  /// since <see cref="IProgressResponse"/> is common to the cycle and operation progress responses.
  /// The events are only requested when the IncludeEvents option is set.
  /// For events that are based on the part production data of the current shift
  /// (ProductionMachiningStatus web service), see <see cref="IProductionMachiningStatusExtension"/>
  /// </summary>
  public interface IProgressExtension
    : Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Initialize the extension for a specific machine.
    ///
    /// The initialized extensions are cached per machine by the web services
    /// </summary>
    /// <param name="machine">monitored machine the events are requested for (not null)</param>
    /// <returns>true if the extension is active for this machine, else false</returns>
    bool Initialize (IMonitoredMachine machine);

    /// <summary>
    /// Get the events that are currently active on the machine (for example: completed cycle or operation, active stop)
    ///
    /// Note: this is called after <see cref="GetComingEvents"/>
    /// </summary>
    /// <param name="progressResponse">progress of the current cycle or operation (completion, estimated end, next stop, progress by machine module...)</param>
    /// <returns>list of active events, empty if none (not null)</returns>
    IList<Event> GetActiveEvents (IProgressResponse progressResponse);

    /// <summary>
    /// Get the events that are expected to occur soon on the machine (for example: cycle end, operation end, sequence end, next stop)
    ///
    /// Note: this is called first, before <see cref="GetActiveEvents"/>.
    /// If there are coming events but no active event, the web services also return
    /// whether the machine is currently running, so that the client can display the coming events accordingly
    /// </summary>
    /// <param name="progressResponse">progress of the current cycle or operation (completion, estimated end, next stop, progress by machine module...)</param>
    /// <returns>list of coming events, empty if none (not null)</returns>
    IList<Event> GetComingEvents (IProgressResponse progressResponse);
  }
}
