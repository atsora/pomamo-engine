// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Event with a single message that is using the generic table event
  /// </summary>
  public interface IEventMessage: IEvent
  {
    /// <summary>
    /// Associated message
    /// </summary>
    string Message { get; }
  }
}
