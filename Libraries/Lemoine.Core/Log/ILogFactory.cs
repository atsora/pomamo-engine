// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lemoine.Core.Log
{
  /// <summary>
  /// ILog factory interface
  /// </summary>
  public interface ILogFactory
  {
    /// <summary>
    /// Get a logger
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    ILog GetLogger (string name);

    /// <summary>
    /// Get a logger
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    ILog GetLogger (Type type);

    /// <summary>
    /// Shutdown the loggers
    /// </summary>
    void Shutdown ();
  }
}
