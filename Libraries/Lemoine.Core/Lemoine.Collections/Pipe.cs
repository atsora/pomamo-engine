// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Collections
{
  /// <summary>
  /// Pipe
  /// </summary>
  public static class PipeExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (PipeExtensions).FullName);

    /// <summary>
    /// Pipe a result to a lambda
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    /// <param name="x"></param>
    /// <param name="lambda"></param>
    /// <returns></returns>
    public static U Pipe<T, U> (this T x, Func<T, U> lambda)
    {
      return lambda (x);
    }

  }
}
