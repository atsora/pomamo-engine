// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model for table Event
  /// </summary>
  public interface IEvent: Lemoine.Collections.IDataWithId, ISerializableModel
  {
    /// <summary>
    /// Event level
    /// </summary>
    IEventLevel Level { get; }
    
    /// <summary>
    /// Date/time of the event
    /// </summary>
    DateTime DateTime { get; }
  }
}
