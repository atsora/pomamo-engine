// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Stamping
{
  /// <summary>
  /// To implement to provide the stamping event handlers to use
  /// </summary>
  public interface IStampingEventHandlersProvider
  {
    /// <summary>
    /// Get the event handler types
    /// </summary>
    public IEnumerable<Type> EventHandlerTypes { get; }
  }
}
