// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Microsoft.Extensions.Logging;

namespace Lemoine.Core.Extensions.Logging
{
  /// <summary>
  /// LogExtensions
  /// </summary>
  public static class Extensions
  {
    /// <summary>
    /// Add the logger provider
    /// </summary>
    /// <param name="factory"></param>
    /// <returns></returns>
    public static ILoggerFactory AddLemoineLog (this ILoggerFactory factory)
    {
      factory.AddProvider (new LoggerProvider ());
      return factory;
    }

    /// <summary>
    /// Add the logger provider
    /// 
    /// TODO: not this request the package Microsoft.Extensions.Logging => to move somewhere else ?
    /// </summary>
    /// <param name="loggingBuilder"></param>
    /// <returns></returns>
    public static ILoggingBuilder AddLemoineLog (this ILoggingBuilder loggingBuilder)
    {
      loggingBuilder.AddProvider (new LoggerProvider ());
      return loggingBuilder;
    }
  }
}
