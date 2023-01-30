// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;

namespace Lemoine.Core.Log
{
  /// <summary>
  /// ILogFactory implementation to support multiple loggers
  /// </summary>
  [Serializable]
  internal sealed class LoggerWithStoppingPrefixFactory : ILogFactory
  {
    readonly ILogFactory m_logFactory;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="logFactory">Effective ILogFactory to use</param>
    public LoggerWithStoppingPrefixFactory (ILogFactory logFactory)
    {
      m_logFactory = logFactory;
    }

    /// <summary>
    /// Get the logger
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public ILog GetLogger (string name)
    {
      return new LoggerWithStoppingPrefix (name, m_logFactory.GetLogger (name));
    }

    /// <summary>
    /// <see cref="ILogFactory"/>
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public ILog GetLogger (Type type)
    {
      return new LoggerWithStoppingPrefix (type.FullName, m_logFactory.GetLogger (type));
    }

    /// <summary>
    /// <see cref="ILogFactory"/>
    /// </summary>
    public void Shutdown ()
    {
      m_logFactory.Shutdown ();
    }
  }

  /// <summary>
  /// ILog implementation to support a stopping instruction
  /// 
  /// After the stopping instruction is run,
  /// all the messages are prefixed by (stopping)
  /// </summary>
  [Serializable]
  internal class LoggerWithStoppingPrefix : ILog
  {
    readonly string m_name;
    readonly ILog m_logger;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name"></param>
    /// <param name="logger"></param>
    public LoggerWithStoppingPrefix (string name, ILog logger)
    {
      m_name = name;
      m_logger = logger;
    }

    object FormatObject (object message)
    {
      if (LogManager.ApplicationStopping) {
        return $"(stopping) {message}";
      }
      else {
        return message;
      }
    }

    string FormatString (string message)
    {
      if (LogManager.ApplicationStopping) {
        return $"(stopping) {message}";
      }
      else {
        return message;
      }
    }

    #region ILog implementation
    public string Name
    {
      get {
        return m_name;
      }
    }

    public bool IsTraceEnabled
    {
      get {
        return m_logger.IsTraceEnabled;
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

    public void Trace (object message)
    {
      if (IsTraceEnabled) {
        m_logger.Trace (FormatObject (message));
      }
    }

    public void Debug (object message)
    {
      m_logger.Debug (FormatObject (message));
    }

    public void Debug (object message, Exception exception)
    {
      m_logger.Debug (FormatObject (message), exception);
    }

    public void DebugFormat (string format, params object[] args)
    {
      m_logger.DebugFormat (FormatString (format), args);
    }

    public void Error (object message)
    {
      m_logger.Error (FormatObject (message));
    }

    public void Error (object message, Exception exception)
    {
      m_logger.Error (FormatObject (message), exception);
    }

    public void ErrorFormat (string format, params object[] args)
    {
      m_logger.ErrorFormat (FormatString (format), args);
    }

    public void Fatal (object message)
    {
      m_logger.Fatal (FormatObject (message));
    }

    public void Fatal (object message, Exception exception)
    {
      m_logger.Fatal (FormatObject (message), exception);
    }

    public void FatalFormat (string format, params object[] args)
    {
      m_logger.FatalFormat (FormatString (format), args);
    }

    public void Info (object message)
    {
      m_logger.Info (FormatObject (message));
    }

    public void Info (object message, Exception exception)
    {
      m_logger.Info (FormatObject (message), exception);
    }

    public void InfoFormat (string format, params object[] args)
    {
      m_logger.InfoFormat (FormatString (format), args);
    }

    public bool Log (Level level, string message)
    {
      return m_logger.Log (level, FormatString (message));
    }

    public bool Log (Level level, string message, Exception exception)
    {
      return m_logger.Log (level, FormatString (message), exception);
    }

    public bool LogFormat (Level level, string messageFormat, params object[] messageArguments)
    {
      return m_logger.LogFormat (level, FormatString (messageFormat), messageArguments);
    }

    public void Warn (object message)
    {
      m_logger.Warn (FormatObject (message));
    }

    public void Warn (object message, Exception exception)
    {
      m_logger.Warn (FormatObject (message), exception);
    }

    public void WarnFormat (string format, params object[] args)
    {
      m_logger.WarnFormat (FormatString (format), args);
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
      return m_logger.BeginScope<TState> (state);
    }
    #endregion // ILog implementation
  }
}
