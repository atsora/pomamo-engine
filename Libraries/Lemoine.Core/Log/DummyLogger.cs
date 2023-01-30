// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lemoine.Core.Log
{
  /// <summary>
  /// Dummy Logger (does nothing)
  /// </summary>
  [Serializable]
  internal class DummyLogger: ILog
  {
    readonly string m_name;

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name"></param>
    public DummyLogger (string name)
    {
      m_name = name;
    }
    #endregion // Constructors

    public bool IsTraceEnabled => false;

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    public bool IsDebugEnabled => false;

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    public bool IsInfoEnabled => false;

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    public bool IsWarnEnabled => false;

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    public bool IsErrorEnabled => false;

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    public bool IsFatalEnabled => false;

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    public string Name => m_name;

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <param name="state"></param>
    /// <returns></returns>
    public IDisposable BeginScope<TState> (TState state)
    {
      return null;
    }

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    /// <param name="message"></param>
    public void Trace (object message)
    {
    }

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    /// <param name="message"></param>
    public void Debug (object message)
    {
    }

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    public void Debug (object message, Exception exception)
    {
    }

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    /// <param name="format"></param>
    /// <param name="args"></param>
    public void DebugFormat (string format, params object[] args)
    {
    }

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    /// <param name="message"></param>
    public void Error (object message)
    {
    }

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    public void Error (object message, Exception exception)
    {
    }

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    /// <param name="format"></param>
    /// <param name="args"></param>
    public void ErrorFormat (string format, params object[] args)
    {
    }

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    /// <param name="message"></param>
    public void Fatal (object message)
    {
    }

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    public void Fatal (object message, Exception exception)
    {
    }

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    /// <param name="format"></param>
    /// <param name="args"></param>
    public void FatalFormat (string format, params object[] args)
    {
    }

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    /// <param name="message"></param>
    public void Info (object message)
    {
    }

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    public void Info (object message, Exception exception)
    {
    }

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    /// <param name="format"></param>
    /// <param name="args"></param>
    public void InfoFormat (string format, params object[] args)
    {
    }

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public bool Log (Level level, string message)
    {
      return true;
    }

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    /// <returns></returns>
    public bool Log (Level level, string message, Exception exception)
    {
      return true;
    }

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    /// <param name="level"></param>
    /// <param name="messageFormat"></param>
    /// <param name="messageArguments"></param>
    /// <returns></returns>
    public bool LogFormat (Level level, string messageFormat, params object[] messageArguments)
    {
      return true;
    }

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    /// <param name="message"></param>
    public void Warn (object message)
    {
    }

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exception"></param>
    public void Warn (object message, Exception exception)
    {
    }

    /// <summary>
    /// <see cref="ILog"/>
    /// </summary>
    /// <param name="format"></param>
    /// <param name="args"></param>
    public void WarnFormat (string format, params object[] args)
    {
    }
  }
}
