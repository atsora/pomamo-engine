// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Microsoft.Extensions.Logging;

namespace Lemoine.Core.Extensions.Logging
{
  /// <summary>
  /// The log4net provider class.
  /// </summary>
  /// <seealso cref="ILoggerProvider" />
  public partial class LoggerProvider : ILoggerProvider
  {
    /// <summary>
    /// The loggers collection.
    /// </summary>
    private readonly ConcurrentDictionary<string, Logger> loggers = new ConcurrentDictionary<string, Logger> ();

    /// <summary>
    /// Prevents to dispose the object more than single time.
    /// </summary>
    private bool m_disposedValue = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggerProvider"/> class.
    /// </summary>
    public LoggerProvider ()
    {
    }

    /// <summary>
    /// Finalizes the instance of the <see cref="LoggerProvider"/> object.
    /// </summary>
    ~LoggerProvider ()
    {
      Dispose (false);
    }

    /// <summary>
    /// Creates the logger.
    /// </summary>
    /// <param name="categoryName">The category name.</param>
    /// <returns>An instance of the <see cref="Microsoft.Extensions.Logging.ILogger"/>.</returns>
    public Microsoft.Extensions.Logging.ILogger CreateLogger (string categoryName)
        => this.loggers.GetOrAdd (categoryName, this.CreateLoggerImplementation);

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose ()
    {
      this.Dispose (true);
      GC.SuppressFinalize (this);
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose (bool disposing)
    {
      if (!m_disposedValue) {
        if (disposing) {
          // TODO: shutdown ?
          this.loggers.Clear ();
        }

        m_disposedValue = true;
      }
    }

    /// <summary>
    /// Creates the logger implementation.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns>The <see cref="Logger"/> instance.</returns>
    private Logger CreateLoggerImplementation (string name)
    {
      return new Logger (name);
    }
  }
}
