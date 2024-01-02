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
  /// The logger class.
  /// </summary>
  public class Logger : Microsoft.Extensions.Logging.ILogger
  {
    readonly string m_name;
    readonly ILog m_log;

    /// <summary>
    /// Initializes a new instance of the <see cref="Logger"/> class.
    /// </summary>
    /// <param name="name">The logger's name.</param>
    public Logger (string name)
    {
      m_name = name;
      m_log = LogManager.GetLogger (name);
    }

    /// <summary>
    /// Gets the name.
    /// </summary>
    public string Name
      => m_name;

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
      return m_log.BeginScope (state);
    }

    /// <summary>
    /// Determines whether the logging level is enabled.
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <returns>The <see cref="bool"/> value indicating whether the logging level is enabled.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Throws when <paramref name="logLevel"/> is outside allowed range</exception>
    public bool IsEnabled (LogLevel logLevel) => logLevel switch {
      LogLevel.Critical => m_log.IsFatalEnabled,
      LogLevel.Debug => m_log.IsDebugEnabled,
      LogLevel.Trace => m_log.IsTraceEnabled,
      LogLevel.Error => m_log.IsErrorEnabled,
      LogLevel.Information => m_log.IsInfoEnabled,
      LogLevel.Warning => m_log.IsWarnEnabled,
      _ => throw new ArgumentOutOfRangeException (nameof (logLevel))
    };

    /// <summary>
    /// Logs an exception into the log.
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <param name="eventId">The event Id.</param>
    /// <param name="state">The state.</param>
    /// <param name="exception">The exception.</param>
    /// <param name="formatter">The formatter.</param>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <exception cref="ArgumentNullException">Throws when the <paramref name="formatter"/> is null.</exception>
    public void Log<TState> (
      LogLevel logLevel,
      EventId eventId,
      TState state,
      Exception? exception,
      Func<TState, Exception?, string> formatter)
    {
      if (!this.IsEnabled (logLevel)) {
        return;
      }

      if (formatter == null) {
        throw new ArgumentNullException (nameof (formatter));
      }

      string message = formatter (state, exception);
      if (message is null) {
        m_log.Fatal ($"Log message is null at {System.Environment.StackTrace}");
      }
      else { // message is not null
        if (exception is not null) {
          switch (logLevel) {
          case LogLevel.Critical:
            this.m_log.Fatal (message, exception);
            break;

          case LogLevel.Trace:
          case LogLevel.Debug:
            this.m_log.Debug (message, exception);
            break;

          case LogLevel.Error:
            this.m_log.Error (message, exception);
            break;

          case LogLevel.Information:
            this.m_log.Info (message, exception);
            break;

          case LogLevel.Warning:
            this.m_log.Warn (message, exception);
            break;

          default:
            this.m_log.Warn ($"Encountered unknown log level {logLevel}, writing out as Info.");
            this.m_log.Info (message, exception);
            break;
          }
        }
        else { // exception is null
          switch (logLevel) {
          case LogLevel.Critical:
            this.m_log.Fatal (message);
            break;

          case LogLevel.Trace:
            this.m_log.Trace (message);
            break;

          case LogLevel.Debug:
            this.m_log.Debug (message);
            break;

          case LogLevel.Error:
            this.m_log.Error (message);
            break;

          case LogLevel.Information:
            this.m_log.Info (message);
            break;

          case LogLevel.Warning:
            this.m_log.Warn (message);
            break;

          default:
            this.m_log.Warn ($"Encountered unknown log level {logLevel}, writing out as Info.");
            this.m_log.Info (message);
            break;
          }
        }
      }
    }
  }
}
