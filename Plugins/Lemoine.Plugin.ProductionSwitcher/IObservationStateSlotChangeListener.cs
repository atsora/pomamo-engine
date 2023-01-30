// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;

namespace Lemoine.Plugin.ProductionSwitcher
{
  /// <summary>
  /// Description of IObservationStateSlotListener.
  /// </summary>
  public interface IObservationStateSlotChangeListener
  {
    /// <summary>
    /// Notify a change
    /// </summary>
    /// <param name="slot">new slot (not null)</param>
    void NotifyObservationStateSlotChange (IObservationStateSlot slot);
  }
}
