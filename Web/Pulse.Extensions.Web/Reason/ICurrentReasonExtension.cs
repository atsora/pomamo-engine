// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Pulse.Extensions.Web.Reason
{
  /// <summary>
  /// Extension to the CurrentReason web service
  /// </summary>
  public interface ICurrentReasonExtension
    : Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Initialize the extension. Return true if the extension is active, else false
    /// </summary>
    /// <param name="machine"></param>
    /// <returns>the extension is active</returns>
    bool Initialize (IMonitoredMachine machine);

    /// <summary>
    /// Get a severity that is associated to a current reason data
    /// 
    /// nullable
    /// </summary>
    /// <param name="currentReasonData"></param>
    /// <returns></returns>
    EventSeverity GetSeverity (Lemoine.Extensions.Business.Reason.ICurrentReasonData currentReasonData);
  }
}
