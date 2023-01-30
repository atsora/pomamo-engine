// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Pulse.Extensions.Database
{
  /// <summary>
  /// Extension to dynamically set a production state and production rate
  /// </summary>
  public interface IProductionStateExtension
    : IInitializedByMonitoredMachineExtension
  {
    /// <summary>
    /// Score (Process the extensions with a highest score first)
    /// </summary>
    double Score { get; }

    /// <summary>
    /// Consolidate in the reason slot the production state and the production rate
    /// </summary>
    /// <param name="oldReasonSlot">nullable</param>
    /// <param name="newReasonSlot">not null</param>
    /// <param name="modification">nullable</param>
    /// <param name="reasonSlotChange"></param>
    /// <returns>true if the production state / rate were set and the process can be interrupted</returns>
    bool ConsolidateProductionStateRate (IReasonSlot oldReasonSlot, IReasonSlot newReasonSlot, IModification modification, ReasonSlotChange reasonSlotChange);
  }
}
