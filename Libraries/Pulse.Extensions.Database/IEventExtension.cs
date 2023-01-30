// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Pulse.Extensions.Database
{
  /// <summary>
  /// Extension for the event reference values
  /// </summary>
  public interface IEventExtension: Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Used event type name
    /// </summary>
    string Type { get; }
    
    /// <summary>
    /// Used event type text
    /// </summary>
    string TypeText { get; }
  }
}
