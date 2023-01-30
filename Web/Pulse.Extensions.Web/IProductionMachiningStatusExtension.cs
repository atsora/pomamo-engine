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
  /// 
  /// </summary>
  public interface IProductionMachiningStatusExtension
    : Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Initialize the extension. Return true if the extension is active, else false
    /// </summary>
    /// <param name="machine"></param>
    /// <returns>the extension is active</returns>
    bool Initialize (IMonitoredMachine machine);

    /// <summary>
    /// Get active events
    /// </summary>
    /// <param name="partProductionCurrentShiftResponse"></param>
    /// <returns></returns>
    IList<Event> GetActiveEvents (Lemoine.Extensions.Business.IPartProductionDataCurrentShift partProductionCurrentShiftResponse);

    /// <summary>
    /// Get coming events
    /// 
    /// Note: this is called first, before GetActiveEvents
    /// </summary>
    /// <param name="partProductionCurrentShiftResponse"></param>
    /// <returns></returns>
    IList<Event> GetComingEvents (Lemoine.Extensions.Business.IPartProductionDataCurrentShift partProductionCurrentShiftResponse);
  }
}
