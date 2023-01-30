// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Core.Log
{
  /// <summary>
  /// Interface for a log provider (that can output logs into different loggers)
  /// </summary>
  public interface ILogProvider : ILogFactory
  {
    /// <summary>
    /// Clear the logger factories
    /// </summary>
    void Clear ();

    /// <summary>
    /// Add an additional logger factory
    /// </summary>
    /// <param name="factory"></param>
    void AddLoggerFactory (ILogFactory factory);
  }

  /// <summary>
  /// Extensions to interface <see cref="ILogProvider"/>
  /// </summary>
  public static class LogProviderExtensions
  {
    /// <summary>
    /// Add Log4net to a <see cref="ILogProvider"/> with the default configuration file made of:
    /// <item>the application name</item>
    /// <item>.log4net</item>
    /// </summary>
    /// <param name="logProvider"></param>
    /// <param name="logFileSuffix"></param>
    /// <param name="traceEnabled"></param>
    public static void AddLog4netDefaultConfigurationFile (this ILogProvider logProvider, string logFileSuffix = "", bool traceEnabled = false)
    {
      logProvider.AddLoggerFactory (Log4netLoggerFactory.CreateWithDefaultConfigurationFile (logFileSuffix, traceEnabled: traceEnabled));
    }

    /// <summary>
    /// Add Log4net to a <see cref="ILogProvider"/>
    /// </summary>
    /// <param name="logProvider"></param>
    /// <param name="configurationFilePath">Configuration file path</param>
    /// <param name="traceEnabled"></param>
    public static void AddLog4net (this ILogProvider logProvider, string configurationFilePath, bool traceEnabled = false)
    {
      logProvider.AddLoggerFactory (Log4netLoggerFactory.Create (configurationFilePath, traceEnabled: traceEnabled));
    }
  }
}
