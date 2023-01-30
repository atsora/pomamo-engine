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
  /// Log manager (singleton class) that provides the default log provider
  /// </summary>
  public sealed class LogManager
  {
    readonly ILogProvider m_multiLoggerFactory;
    readonly ILogFactory m_loggerWithStoppingPrefixFactory;
    ILogFactory m_logFactory;
    bool m_stoppingFeatureOn = true;
    bool m_applicationStopping = false;

    /// <summary>
    /// Constructor (add by default the log4net implementation)
    /// </summary>
    LogManager ()
    {
      m_multiLoggerFactory = new MultiLoggerFactory ();
      m_loggerWithStoppingPrefixFactory = new LoggerWithStoppingPrefixFactory (m_multiLoggerFactory);
      m_logFactory = m_loggerWithStoppingPrefixFactory;
    }

    /// <summary>
    /// De-activate the stopping feature
    /// </summary>
    public static void DeactivateStoppingFeature ()
    {
      if (Instance.m_stoppingFeatureOn) {
        Instance.m_stoppingFeatureOn = false;
        Instance.m_logFactory = Instance.m_multiLoggerFactory;
      }
    }

    /// <summary>
    /// Is the application stopping ?
    /// </summary>
    internal static bool ApplicationStopping => Instance.m_applicationStopping;

    /// <summary>
    /// Set the application is stopping
    /// </summary>
    public static void SetApplicationStopping ()
    {
      if (!ApplicationStopping) {
        Instance.m_applicationStopping = true;
      }
    }

    /// <summary>
    /// Main logger factory
    /// </summary>
    public static ILogFactory LoggerFactory
    {
      get {
        return Instance.m_logFactory;
      }
    }

    /// <summary>
    /// Associated <see cref="ILogProvider"/>
    /// </summary>
    public static ILogProvider LogProvider
    {
      get {
        return Instance.m_multiLoggerFactory;
      }
    }

    /// <summary>
    /// Add an additional logger factory
    /// </summary>
    /// <param name="factory"></param>
    public static void AddLoggerFactory (ILogFactory factory)
    {
      Instance.m_multiLoggerFactory.AddLoggerFactory (factory);
    }

    /// <summary>
    /// Clear the logger factories
    /// (to add new ones afterwards)
    /// </summary>
    public static void ClearLoggerFactories ()
    {
      Instance.m_multiLoggerFactory.Clear ();
    }

    /// <summary>
    /// Add log4net with the default configuration file,
    /// made of the application file name followed by ".log4net"
    /// </summary>
    public static void AddLog4net (bool traceEnabled = false)
    {
      AddLog4netDefaultConfigurationFile (traceEnabled: traceEnabled);
    }

    /// <summary>
    /// Add log4net with the configuration file made of:
    /// <item>the application name</item>
    /// <item>.log4net</item>
    /// </summary>
    /// <param name="logFileSuffix"></param>
    /// <param name="traceEnabled"></param>
    public static void AddLog4netDefaultConfigurationFile (string logFileSuffix = "", bool traceEnabled = false)
    {
      Instance.m_multiLoggerFactory.AddLog4netDefaultConfigurationFile (logFileSuffix, traceEnabled: traceEnabled);
    }

    /// <summary>
    /// Add log4net with a specific configuration file
    /// </summary>
    /// <param name="configurationFilePath"></param>
    public static void AddLog4net (string configurationFilePath)
    {
      Instance.m_multiLoggerFactory.AddLog4net (configurationFilePath);
    }

    /// <summary>
    /// <see cref="ILogFactory"/>
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static ILog GetLogger (string name)
    {
      try {
        return Instance.m_logFactory.GetLogger (name);
      }
      catch (OutOfMemoryException) {
        Lemoine.Core.Environment.ForceExit ();
        throw;
      }
    }

    /// <summary>
    /// Get the logger for a specific class
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public static ILog GetLogger (Type t)
    {
      try {
        return Instance.m_logFactory.GetLogger (t);
      }
      catch (OutOfMemoryException) {
        Lemoine.Core.Environment.ForceExit ();
        throw;
      }
    }

    /// <summary>
    /// Get the logger for a specific class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static ILog GetLogger<T> ()
    {
      return GetLogger (typeof (T));
    }

    /// <summary>
    /// Shutdown the loggers
    /// </summary>
    public static void Shutdown ()
    {
      try {
        Instance.m_logFactory.Shutdown ();
      }
      catch (OutOfMemoryException) {
        Lemoine.Core.Environment.ForceExit ();
        throw;
      }
    }

    #region Instance
    static LogManager Instance { get { return Nested.instance; } }
    class Nested
    {
      // Explicit static constructor to tell C# compiler not to mark type as beforefieldinit
      static Nested () { }
      internal static readonly LogManager instance = new LogManager ();
    }
    #endregion
  }
}
