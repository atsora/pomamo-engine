// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Stamping
{
  /// <summary>
  /// Stamping application builder
  /// </summary>
  public interface IStampingApplicationBuilder
  {
    /// <summary>
    /// Event handler
    /// </summary>
    IStampingEventHandler EventHandler { get;  }

    /// <summary>
    /// Add a new stamping event handler that will be create through dependency injection
    /// </summary>
    /// <returns></returns>
    IStampingApplicationBuilder UseEventHandler<T> ()
      where T : IStampingEventHandler;

    /// <summary>
    /// Add a new stamping event handler that will be create through dependency injection
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    IStampingApplicationBuilder UseEventHandler (Type type);

    /// <summary>
    /// Add a new stamping event handler that will be create through dependency injection
    /// </summary>
    /// <param name="typeName"></param>
    /// <returns></returns>
    IStampingApplicationBuilder UseEventHandler (string typeName);
  }
}
