// Copyright (c) 2023 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lemoine.Core.Log
{
  /// <summary>
  /// Class with a method that returns a logger <see cref="ILog"/>
  /// </summary>
  public interface ILogged
  {
    /// <summary>
    /// Get the logger
    /// </summary>
    /// <returns></returns>
    ILog GetLogger ();
  }
}
