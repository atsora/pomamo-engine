// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.Extensions.AutoReason.ActionableAutoReasonExtension
{
  /// <summary>
  /// 
  /// </summary>
  public interface IApplyReasonDynamicEndBeforeRealEndAutoReason: IAutoReasonExtension
  {
    /// <summary>
    /// Apply the associated reason (and score) to the specified range 
    /// and with the dynamic times with the aggressive mode and the option
    /// to cancel the modification if the dynamic end is after end
    /// 
    /// It needs to be run in a transaction
    /// </summary>
    /// <param name="machine">alternate machine (not null)</param>
    /// <param name="reason">not null</param>
    /// <param name="reasonScore"></param>
    /// <param name="range"></param>
    /// <param name="dynamic"></param>
    /// <param name="details"></param>
    /// <param name="overwriteRequired"></param>
    void ApplyReasonDynamicEndBeforeRealEnd (IMachine machine, IReason reason, double reasonScore, UtcDateTimeRange range, string dynamic, string details, bool overwriteRequired);
  }
}
