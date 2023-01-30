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
  internal sealed class MultiLoggerFactory : ILogFactory, ILogProvider
  {
    readonly IList<ILogFactory> m_factories = new List<ILogFactory> ();

    /// <summary>
    /// <see cref="ILogProvider"/>
    /// </summary>
    public void Clear ()
    {
      m_factories.Clear ();
    }

    /// <summary>
    /// Add a logger factory
    /// </summary>
    /// <param name="factory"></param>
    public void AddLoggerFactory (ILogFactory factory)
    {
      m_factories.Add (factory);
    }

    /// <summary>
    /// Get the logger
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public ILog GetLogger (string name)
    {
      return new MultiLogger (name, m_factories.Select (f => f.GetLogger (name)));
    }

    /// <summary>
    /// <see cref="ILogFactory"/>
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public ILog GetLogger (Type type)
    {
      return new MultiLogger (type.FullName, m_factories.Select (f => f.GetLogger (type)));
    }

    /// <summary>
    /// <see cref="ILogFactory"/>
    /// </summary>
    public void Shutdown ()
    {
      foreach (var factory in m_factories) {
        factory.Shutdown ();
      }
    }
  }

  /// <summary>
  /// ILog implementation to support multiple loggers
  /// </summary>
  [Serializable]
  internal class MultiLogger : ILog
  {
    readonly string m_name;
    readonly IList<ILog> m_loggers = new List<ILog> ();
    bool m_traceDisabled = false; // To keep it in cache. To activate a trace, you can do it only at the program start

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="name"></param>
    public MultiLogger (string name)
    {
      m_name = name;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="name"></param>
    /// <param name="loggers"></param>
    public MultiLogger (string name, IEnumerable<ILog> loggers)
      : this (name)
    {
      m_loggers = loggers.ToList ();
    }

    /// <summary>
    /// Add a logger
    /// </summary>
    /// <param name="logger"></param>
    public void AddLogger (ILog logger)
    {
      m_loggers.Add (logger);
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
        if (m_traceDisabled) {
          return false;
        }
        var enabled = m_loggers.Any (l => l.IsTraceEnabled);
        m_traceDisabled = !enabled;
        return enabled;
      }
    }

    public bool IsDebugEnabled
    {
      get {
        return m_loggers.Any (l => l.IsDebugEnabled);
      }
    }

    public bool IsErrorEnabled
    {
      get {
        return m_loggers.Any (l => l.IsErrorEnabled);
      }
    }

    public bool IsFatalEnabled
    {
      get {
        return m_loggers.Any (l => l.IsFatalEnabled);
      }
    }

    public bool IsInfoEnabled
    {
      get {
        return m_loggers.Any (l => l.IsInfoEnabled);
      }
    }

    public bool IsWarnEnabled
    {
      get {
        return m_loggers.Any (l => l.IsWarnEnabled);
      }
    }

    public void Trace (object message)
    {
      if (IsTraceEnabled) {
        foreach (var logger in m_loggers) {
          logger.Trace (message);
        }
      }
    }

    public void Debug (object message)
    {
      foreach (var logger in m_loggers) {
        logger.Debug (message);
      }
    }

    public void Debug (object message, Exception exception)
    {
      foreach (var logger in m_loggers) {
        logger.Debug (message, exception);
      }
    }

    public void DebugFormat (string format, params object[] args)
    {
      foreach (var logger in m_loggers.Where (x => x.IsDebugEnabled)) {
        logger.DebugFormat (format, args);
      }
    }

    public void Error (object message)
    {
      foreach (var logger in m_loggers) {
        logger.Error (message);
      }
    }

    public void Error (object message, Exception exception)
    {
      foreach (var logger in m_loggers) {
        logger.Error (message, exception);
      }
    }

    public void ErrorFormat (string format, params object[] args)
    {
      foreach (var logger in m_loggers.Where (x => x.IsErrorEnabled)) {
        logger.ErrorFormat (format, args);
      }
    }

    public void Fatal (object message)
    {
      foreach (var logger in m_loggers) {
        logger.Fatal (message);
      }
    }

    public void Fatal (object message, Exception exception)
    {
      foreach (var logger in m_loggers) {
        logger.Fatal (message, exception);
      }
    }

    public void FatalFormat (string format, params object[] args)
    {
      foreach (var logger in m_loggers) {
        logger.FatalFormat (format, args);
      }
    }

    public void Info (object message)
    {
      foreach (var logger in m_loggers) {
        logger.Info (message);
      }
    }

    public void Info (object message, Exception exception)
    {
      foreach (var logger in m_loggers) {
        logger.Info (message, exception);
      }
    }

    public void InfoFormat (string format, params object[] args)
    {
      foreach (var logger in m_loggers.Where (x => x.IsInfoEnabled)) {
        logger.InfoFormat (format, args);
      }
    }

    public bool Log (Level level, string message)
    {
      bool result = false;
      foreach (var logger in m_loggers) {
        result |= logger.Log (level, message);
      }
      return result;
    }

    public bool Log (Level level, string message, Exception exception)
    {
      bool result = false;
      foreach (var logger in m_loggers) {
        result |= logger.Log (level, message, exception);
      }
      return result;
    }

    public bool LogFormat (Level level, string messageFormat, params object[] messageArguments)
    {
      bool result = false;
      foreach (var logger in m_loggers) {
        result |= logger.LogFormat (level, messageFormat, messageArguments);
      }
      return result;
    }

    public void Warn (object message)
    {
      foreach (var logger in m_loggers) {
        logger.Warn (message);
      }
    }

    public void Warn (object message, Exception exception)
    {
      foreach (var logger in m_loggers) {
        logger.Warn (message, exception);
      }
    }

    public void WarnFormat (string format, params object[] args)
    {
      foreach (var logger in m_loggers.Where (x => x.IsWarnEnabled)) {
        logger.WarnFormat (format, args);
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
      var scope = new MultiLoggerScope ();
      foreach (var logger in m_loggers) {
        var loggerScope = logger.BeginScope<TState> (state);
        if (null != loggerScope) {
          scope.AddScope (loggerScope);
        }
      }
      return scope;
    }
    #endregion // ILog implementation
  }

  /// <summary>
  /// Multi-logger scope
  /// </summary>
  public sealed class MultiLoggerScope : IDisposable
  {
    readonly IList<IDisposable> m_scopes = new List<IDisposable> ();

    /// <summary>
    /// Add a virtual scope for multi-logger
    /// </summary>
    /// <param name="scope"></param>
    public void AddScope (IDisposable scope)
    {
      if (null != scope) {
        m_scopes.Add (scope);
      }
    }

    /// <summary>
    /// <see cref="IDisposable"/>
    /// </summary>
    public void Dispose ()
    {
      // Note: sealed class, so this is not necessary to implement Dispose (bool)
      // see https://docs.microsoft.com/fr-fr/dotnet/standard/garbage-collection/implementing-dispose
      foreach (var scope in m_scopes) {
        try {
          scope.Dispose ();
        }
        catch (Exception) { }
      }
    }
  }
}
