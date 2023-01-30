// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Lemoine.Core.Log
{
  /// <summary>
  /// log4net factory
  /// </summary>
  [Serializable]
  internal sealed class Log4netLoggerFactory : ILogFactory
  {
    const string LOG_DIRECTORY_KEY = "LogDirectory";
    const string LOG_DIRECTORY_DEFAULT = "Logs";

    readonly bool m_traceEnabled = false;

    /// <summary>
    /// Private constructor. Use instead:
    /// <item><see cref="Create(string, string, bool)"/></item>
    /// <item><see cref="CreateWithDefaultConfigurationFile(string, bool)"/></item>
    /// </summary>
    Log4netLoggerFactory (bool traceEnabled = false)
    {
      m_traceEnabled = traceEnabled;
    }

    /// <summary>
    /// Create a log4net logger factory using a specific configuration file
    /// </summary>
    /// <param name="configurationFilePath"></param>
    /// <param name="logFileSuffix"></param>
    /// <param name="traceEnabled"></param>
    /// <returns></returns>
    public static Log4netLoggerFactory Create (string configurationFilePath, string logFileSuffix = "", bool traceEnabled = false)
    {
      SetLogFileSuffix (logFileSuffix);
      SetProperties ();

      ConfigureAndWatch (configurationFilePath);

      return new Log4netLoggerFactory (traceEnabled);
    }

    /// <summary>
    /// Create a log4net logger factory using the default .log4net configuration file
    /// </summary>
    /// <param name="logFileSuffix"></param>
    /// <param name="traceEnabled"></param>
    /// <returns></returns>
    public static Log4netLoggerFactory CreateWithDefaultConfigurationFile (string logFileSuffix = "", bool traceEnabled = false)
    {
      SetLogFileSuffix (logFileSuffix);
      SetProperties ();

      var programAssembly = System.Reflection.Assembly.GetEntryAssembly ();
      if (null != programAssembly) {
        ConfigureAndWatch (programAssembly.Location + ".log4net");
      }

      return new Log4netLoggerFactory (traceEnabled);
    }

    /// <summary>
    /// Set the properties:
    /// <item>PulseApplication</item>
    /// <item>LogDirectory</item>
    /// </summary>
    /// <param name="applicationName"></param>
    static void SetProperties (string applicationName = null)
    {
      if (string.IsNullOrEmpty (applicationName)) {
        var programPath = Lemoine.Info.ProgramInfo.AbsolutePath;
        if (null == programPath) { // Fallback
          SetProperties ("PulseApplication");
        }
        else { // null != programPath
          var fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension (programPath);
          if (string.IsNullOrEmpty (fileNameWithoutExtension)) {
            SetProperties ("PulseApplication");
          }
          else {
            SetProperties (fileNameWithoutExtension);
          }
        }
        return;
      }

      var logDirectory = Lemoine.Info.ConfigSet
        .LoadAndGet (LOG_DIRECTORY_KEY, LOG_DIRECTORY_DEFAULT);
      if (!System.IO.Directory.Exists (logDirectory)) {
        try {
          System.IO.Directory.CreateDirectory (logDirectory);
        }
        catch (Exception) { }
      }
      if (!Path.IsPathRooted (logDirectory)) {
        logDirectory = Path.Combine (Lemoine.Info.PulseInfo.LocalConfigurationDirectory, logDirectory);
      }
      log4net.GlobalContext.Properties["LogDirectory"] = logDirectory;

      Debug.Assert (!string.IsNullOrEmpty (applicationName));
      log4net.GlobalContext.Properties["ApplicationName"] = applicationName;
    }

    static void SetLogFileSuffix (string logFileSuffix)
    {
      log4net.GlobalContext.Properties["LogFileSuffix"] = logFileSuffix;
    }

    static void ConfigureAndWatch (string configurationFilePath)
    {
      var fileInfo = new System.IO
        .FileInfo (configurationFilePath);
      var repository = log4net.LogManager
        .GetRepository (System.Reflection.Assembly.GetCallingAssembly ());
      log4net.Config.XmlConfigurator.ConfigureAndWatch (repository, fileInfo);
    }

    /// <summary>
    /// <see cref="ILogFactory"/>
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public ILog GetLogger (string name)
    {
      try {
        return new Log4netLogger (log4net.LogManager.GetLogger (System.Reflection.Assembly.GetCallingAssembly (), name), m_traceEnabled);
      }
      catch (OutOfMemoryException) {
        Lemoine.Core.Environment.ForceExit ();
        return new DummyLogger (name);
      }
      catch (Exception) {
        return new DummyLogger (name);
      }
    }

    /// <summary>
    /// <see cref="ILogFactory"/>
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public ILog GetLogger (Type type)
    {
      try {
        return new Log4netLogger (log4net.LogManager.GetLogger (type));
      }
      catch (OutOfMemoryException) {
        Lemoine.Core.Environment.ForceExit ();
        return new DummyLogger (type.FullName);
      }
      catch (Exception) {
        return new DummyLogger (type.FullName);
      }
    }

    /// <summary>
    /// <see cref="ILogFactory"/>
    /// </summary>
    public void Shutdown ()
    {
      try {
        log4net.LogManager.Shutdown ();
      }
      catch (OutOfMemoryException) {
        Lemoine.Core.Environment.ForceExit ();
      }
      catch (Exception) { }
    }
  }

  /// <summary>
  /// /// log4net wrapper to implement the ILog interface
  /// </summary>
  [Serializable]
  internal class Log4netLogger : ILog
  {
    readonly log4net.ILog m_logger;
    readonly bool m_traceEnabled = false;

    public Log4netLogger (log4net.ILog logger, bool traceEnabled = false)
    {
      m_logger = logger;
      m_traceEnabled = traceEnabled;
    }

    #region ILog implementation
    public bool IsTraceEnabled
    {
      get {
        return m_traceEnabled && m_logger.IsDebugEnabled;
      }
    }

    public bool IsDebugEnabled
    {
      get {
        return m_logger.IsDebugEnabled;
      }
    }

    public bool IsErrorEnabled
    {
      get {
        return m_logger.IsErrorEnabled;
      }
    }

    public bool IsFatalEnabled
    {
      get {
        return m_logger.IsFatalEnabled;
      }
    }

    public bool IsInfoEnabled
    {
      get {
        return m_logger.IsInfoEnabled;
      }
    }

    public bool IsWarnEnabled
    {
      get {
        return m_logger.IsWarnEnabled;
      }
    }

    public string Name
    {
      get {
        return m_logger.Logger.Name;
      }
    }

    public void Trace (object message)
    {
      if (IsTraceEnabled) {
        m_logger.Debug (message);
      }
    }

    public void Debug (object message)
    {
      m_logger.Debug (message);
    }

    public void Debug (object message, Exception exception)
    {
      m_logger.Debug (message, exception);
    }

    public void DebugFormat (string format, params object[] args)
    {
      m_logger.DebugFormat (format, args);
    }

    public void DebugFormat (IFormatProvider provider, string format, params object[] args)
    {
      m_logger.DebugFormat (provider, format, args);
    }

    public void Error (object message)
    {
      m_logger.Error (message);
    }

    public void Error (object message, Exception exception)
    {
      m_logger.Error (message, exception);
    }

    public void ErrorFormat (string format, params object[] args)
    {
      m_logger.ErrorFormat (format, args);
    }

    public void Fatal (object message)
    {
      m_logger.Fatal (message);
    }

    public void Fatal (object message, Exception exception)
    {
      m_logger.Fatal (message, exception);
    }

    public void FatalFormat (string format, params object[] args)
    {
      m_logger.FatalFormat (format, args);
    }

    public void Info (object message)
    {
      m_logger.Info (message);
    }

    public void Info (object message, Exception exception)
    {
      m_logger.Info (message, exception);
    }

    public void InfoFormat (string format, params object[] args)
    {
      m_logger.InfoFormat (format, args);
    }

    public bool Log (Level level, string message)
    {
      return Log (level, message, null);
    }

    public bool Log (Level level, string message, Exception exception)
    {
      var logger = m_logger.Logger;
      var log4netLevel = ConvertLevel (level);
      if (logger.IsEnabledFor (log4netLevel)) {
        logger.Log (logger.GetType (), log4netLevel, message, exception);
        return true;
      }

      return false;
    }

    public bool LogFormat (Level level, string messageFormat, params object[] messageArguments)
    {
      var logger = m_logger.Logger;
      var log4netLevel = ConvertLevel (level);
      if (logger.IsEnabledFor (log4netLevel)) {
        var message = string.Format (messageFormat, messageArguments);
        logger.Log (logger.GetType (), log4netLevel, message, null);

        return true;
      }

      return false;
    }

    public void Warn (object message)
    {
      m_logger.Warn (message);
    }

    public void Warn (object message, Exception exception)
    {
      m_logger.Warn (message, exception);
    }

    public void WarnFormat (string format, params object[] args)
    {
      m_logger.WarnFormat (format, args);
    }

    /// <summary>
    /// Begins a logical operation scope.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="state">The identifier for the scope.</param>
    /// <returns>
    /// An IDisposable that ends the logical operation scope on dispose.
    /// </returns>
    public IDisposable BeginScope<TState> (TState state)
    {
      // TODO: see https://github.com/huorswords/Microsoft.Extensions.Logging.Log4Net.AspNetCore
      return null;
    }
    #endregion // ILog implementation

    log4net.Core.Level ConvertLevel (Level level)
    {
      switch (level) {
      case Level.Trace:
      case Level.Debug:
        return log4net.Core.Level.Debug;
      case Level.Info:
        return log4net.Core.Level.Info;
      case Level.Notice:
        return log4net.Core.Level.Notice;
      case Level.Warn:
        return log4net.Core.Level.Warn;
      case Level.Error:
        return log4net.Core.Level.Error;
      case Level.Fatal:
        return log4net.Core.Level.Fatal;
      default:
        var allLevels = new log4net.Core.LevelMap ().AllLevels;
        foreach (var l in allLevels) {
          if (l.Name.Equals (level.ToString (), StringComparison.InvariantCultureIgnoreCase)) {
            return l;
          }
        }
        throw new Exception ("log4net exception not found for " + level);
      }
    }
  }
}
