// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.WebMiddleware.Handlers
{
  /// <summary>
  /// Interface for a class that returns the list of available handlers
  /// </summary>
  public interface IHandlersResolver
  {
    /// <summary>
    /// Get the available handlers
    /// </summary>
    /// <returns></returns>
    IEnumerable<Type> GetHandlers ();
  }
}
