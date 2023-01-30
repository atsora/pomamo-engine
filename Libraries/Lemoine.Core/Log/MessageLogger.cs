// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using System.IO;

namespace Lemoine.Core.Log
{
  /// <summary>
  /// ILog factory to log messages into a stream
  /// </summary>
  [Serializable]
  public class MessageLoggerFactory : ILogFactory
  {
    readonly TextWriter m_textWriter;
    readonly Level m_level;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="textWriter"></param>
    public MessageLoggerFactory (TextWriter textWriter)
      : this (textWriter, Level.Info)
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="textWriter"></param>
    /// <param name="level"></param>
    public MessageLoggerFactory (TextWriter textWriter, Level level)
    {
      m_textWriter = textWriter;
      m_level = level;
    }

    /// <summary>
    /// <see cref="ILogFactory"/>
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public ILog GetLogger (string name)
    {
      return new MessageLogger (m_textWriter, m_level, name);
    }

    /// <summary>
    /// <see cref="ILogFactory"/>
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public ILog GetLogger (Type type)
    {
      return GetLogger (type.FullName);
    }

    /// <summary>
    /// <see cref="ILogFactory"/>
    /// </summary>
    public void Shutdown ()
    {
      m_textWriter.Close ();
    }
  }

  /// <summary>
  /// MessageLogger: log the message into a stream
  /// 
  /// TODO: how to format the message could be improved later
  /// </summary>
  [Serializable]
  public class MessageLogger : ILog
  {
    readonly TextWriter m_textWriter;
    readonly Level m_level;
    readonly string m_name;

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    public bool IsTraceEnabled
    {
      get {
        switch (m_level) {
        case Level.Trace:
          return true;
        default:
          return false;
        }
      }
    }

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    public bool IsDebugEnabled
    {
      get {
        switch (m_level) {
        case Level.Trace:
        case Level.Debug:
          return true;
        default:
          return false;
        }
      }
    }

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    public bool IsInfoEnabled
    {
      get {
        switch (m_level) {
        case Level.Trace:
        case Level.Debug:
        case Level.Info:
          return true;
        default:
          return false;
        }
      }
    }

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    public bool IsWarnEnabled
    {
      get {
        switch (m_level) {
        case Level.Trace:
        case Level.Debug:
        case Level.Info:
        case Level.Notice:
        case Level.Warn:
          return true;
        default:
          return false;
        }
      }
    }

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    public bool IsErrorEnabled
    {
      get {
        switch (m_level) {
        case Level.Trace:
        case Level.Debug:
        case Level.Info:
        case Level.Notice:
        case Level.Warn:
        case Level.Error:
          return true;
        default:
          return false;
        }
      }
    }

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    public bool IsFatalEnabled
    {
      get {
        switch (m_level) {
        case Level.Trace:
        case Level.Debug:
        case Level.Info:
        case Level.Notice:
        case Level.Warn:
        case Level.Error:
        case Level.Fatal:
          return true;
        default:
          return false;
        }
      }
    }

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    public string Name
    {
      get {
        return m_name;
      }
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
      // TODO: 
      return null;
    }

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="textWriter"></param>
    /// <param name="level"></param>
    /// <param name="name"></param>
    internal MessageLogger (TextWriter textWriter, Level level, string name)
    {
      m_textWriter = textWriter;
      m_level = level;
      m_name = name;
    }
    #endregion // Constructors

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    public void Trace (object message)
    {
      if (IsTraceEnabled) {
        var s = "(T) " + message;
        m_textWriter.WriteLine (s);
      }
    }

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    public void Debug (object message)
    {
      if (IsDebugEnabled) {
        var s = "(D) " + message;
        m_textWriter.WriteLine (s);
      }
    }

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    public void Debug (object message, Exception exception)
    {
      if (IsDebugEnabled) {
        var s = string.Format ("(D) {0}: {1}", message, exception.Message);
        m_textWriter.WriteLine (s);
      }
    }

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    public void DebugFormat (string format, params object[] args)
    {
      if (IsDebugEnabled) {
        var s = string.Format ("(D) " + format, args);
        m_textWriter.WriteLine (s);
      }
    }

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    public void Info (object message)
    {
      if (IsInfoEnabled) {
        var s = "(I) " + message;
        m_textWriter.WriteLine (s);
      }
    }

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    public void Info (object message, Exception exception)
    {
      if (IsInfoEnabled) {
        var s = string.Format ("(I) {0}: {1}", message, exception.Message);
        m_textWriter.WriteLine (s);
      }
    }

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    public void InfoFormat (string format, params object[] args)
    {
      if (IsInfoEnabled) {
        var s = string.Format ("(I) " + format, args);
        m_textWriter.WriteLine (s);
      }
    }

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    public void Warn (object message)
    {
      if (IsWarnEnabled) {
        var s = "(W) " + message;
        m_textWriter.WriteLine (s);
      }
    }

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    public void Warn (object message, Exception exception)
    {
      if (IsWarnEnabled) {
        var s = string.Format ("(W) {0}: {1}", message, exception.Message);
        m_textWriter.WriteLine (s);
      }
    }

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    public void WarnFormat (string format, params object[] args)
    {
      if (IsWarnEnabled) {
        var s = string.Format ("(W) " + format, args);
        m_textWriter.WriteLine (s);
      }
    }

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    public void Error (object message)
    {
      if (IsErrorEnabled) {
        var s = "(E) " + message;
        m_textWriter.WriteLine (s);
      }
    }

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    public void Error (object message, Exception exception)
    {
      if (IsErrorEnabled) {
        var s = string.Format ("(E) {0}: {1}", message, exception.Message);
        m_textWriter.WriteLine (s);
      }
    }

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    public void ErrorFormat (string format, params object[] args)
    {
      if (IsErrorEnabled) {
        var s = string.Format ("(E) " + format, args);
        m_textWriter.WriteLine (s);
      }
    }

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    public void Fatal (object message)
    {
      if (IsFatalEnabled) {
        var s = "(F) " + message;
        m_textWriter.WriteLine (s);
      }
    }

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    public void Fatal (object message, Exception exception)
    {
      if (IsFatalEnabled) {
        var s = string.Format ("(F) {0}: {1}", message, exception.Message);
        m_textWriter.WriteLine (s);
      }
    }

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    public void FatalFormat (string format, params object[] args)
    {
      if (IsFatalEnabled) {
        var s = string.Format ("(F) " + format, args);
        m_textWriter.WriteLine (s);
      }
    }

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    public bool Log (Level level, string message)
    {

      throw new NotImplementedException ();
    }

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    public bool Log (Level level, string message, Exception exception)
    {
      throw new NotImplementedException ();
    }

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    public bool LogFormat (Level level, string messageFormat, params object[] messageArguments)
    {
      throw new NotImplementedException ();
    }
  }
}
