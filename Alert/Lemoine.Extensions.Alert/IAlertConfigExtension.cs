// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Extensions.Alert
{
  /// <summary>
  /// Extension to add an alert configuration
  /// </summary>
  public interface IAlertConfigExtension: IExtension
  {
    /// <summary>
    /// Initialize the plugin
    /// </summary>
    /// <returns></returns>
    bool Initialize ();

    /// <summary>
    /// Listeners
    /// </summary>
    IEnumerable<IListener> Listeners { get; }

    /// <summary>
    /// Trigger actions
    /// </summary>
    IEnumerable<TriggeredAction> TriggeredActions { get; }
  }
}
