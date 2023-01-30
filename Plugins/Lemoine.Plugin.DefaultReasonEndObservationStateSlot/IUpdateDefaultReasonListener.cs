// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading;
using Lemoine.Model;

namespace Lemoine.Plugin.DefaultReasonEndObservationStateSlot
{
  /// <summary>
  /// Description of IReasonSlotListener.
  /// </summary>
  public interface IUpdateDefaultReasonListener
  {
    /// <summary>
    /// Reference to the machine
    /// </summary>
    IMachine Machine { get; }
    
    /// <summary>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="slot"></param>
    bool UpdateDefaultReason (CancellationToken cancellationToken, IReasonSlot slot);
  }
}
