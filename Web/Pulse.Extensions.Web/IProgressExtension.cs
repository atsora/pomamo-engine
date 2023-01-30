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
  /// </summary>
  public interface IProgressExtension
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
    IList<Event> GetActiveEvents (IProgressResponse progressResponse);

    /// <summary>
    /// Get coming events
    /// 
    /// Note: this is called first, before GetActiveEvents
    /// </summary>
    IList<Event> GetComingEvents (IProgressResponse progressResponse);
  }
}
