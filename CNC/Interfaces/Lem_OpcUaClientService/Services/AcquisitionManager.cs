// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: GPL-2.0

using System.Collections.Concurrent;
using Lemoine.Cnc.OpcUaClientService.Configuration;
using Microsoft.Extensions.Options;

namespace Lemoine.Cnc.OpcUaClientService.Services
{
  /// <summary>
  /// Owner of the connections of all the acquisitions
  ///
  /// A session is created by the first /xml request of an acquisition, and it is kept afterwards,
  /// so that the values that were read for a machine are cached independently from the values that
  /// were read for the other machines. The /get, /set and /data requests use the session the /xml
  /// requests of the same acquisition identifier configured.
  /// </summary>
  public sealed class AcquisitionManager : IAsyncDisposable, IDisposable
  {
    readonly OpcUaClientServiceOptions m_options;
    readonly ILoggerFactory m_loggerFactory;
    readonly ILogger<AcquisitionManager> m_logger;
    readonly ConcurrentDictionary<string, AcquisitionSession> m_sessions
      = new ConcurrentDictionary<string, AcquisitionSession> (StringComparer.InvariantCultureIgnoreCase);

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="options">not null</param>
    /// <param name="loggerFactory">not null</param>
    /// <param name="logger">not null</param>
    public AcquisitionManager (IOptions<OpcUaClientServiceOptions> options, ILoggerFactory loggerFactory, ILogger<AcquisitionManager> logger)
    {
      ArgumentNullException.ThrowIfNull (options);
      ArgumentNullException.ThrowIfNull (loggerFactory);
      ArgumentNullException.ThrowIfNull (logger);

      m_options = options.Value;
      m_loggerFactory = loggerFactory;
      m_logger = logger;
    }

    /// <summary>
    /// Identifiers of the acquisitions that are configured
    /// </summary>
    public IEnumerable<string> AcquisitionIdentifiers => m_sessions.Keys;

    /// <summary>
    /// Return the session of an acquisition, creating it if it does not exist yet, and apply
    /// the configuration a /xml request carries to it
    /// </summary>
    /// <param name="acquisitionIdentifier">nullable</param>
    /// <param name="configuration">not null</param>
    public AcquisitionSession GetOrCreateSession (string? acquisitionIdentifier, AcquisitionConfiguration configuration)
    {
      ArgumentNullException.ThrowIfNull (configuration);

      var identifier = acquisitionIdentifier ?? "";
      return m_sessions.GetOrAdd (identifier, k => {
        m_logger.LogInformation ($"GetOrCreateSession: new acquisition {k}, {configuration}");
        return new AcquisitionSession (k, configuration, m_options,
          m_loggerFactory.CreateLogger ($"{typeof (AcquisitionSession).FullName}.{k}"));
      });
    }

    /// <summary>
    /// Return the session of an acquisition, or null when no /xml request configured it yet
    /// </summary>
    /// <param name="acquisitionIdentifier">nullable</param>
    /// <returns>nullable</returns>
    public AcquisitionSession? TryGetSession (string? acquisitionIdentifier)
    {
      var identifier = acquisitionIdentifier ?? "";
      if (!m_sessions.TryGetValue (identifier, out var session)) {
        m_logger.LogError ($"TryGetSession: acquisition {identifier} is not configured yet, no /xml request was received for it");
        return null;
      }
      return session;
    }

    /// <summary>
    /// <see cref="IAsyncDisposable.DisposeAsync" />
    /// </summary>
    public async ValueTask DisposeAsync ()
    {
      foreach (var identifier in m_sessions.Keys) {
        if (!m_sessions.TryRemove (identifier, out var session)) {
          continue;
        }
        try {
          await session.DisposeAsync ();
        }
        catch (Exception ex) {
          m_logger.LogError (ex, $"DisposeAsync: disposing the session of {identifier} failed");
        }
      }
    }

    /// <summary>
    /// <see cref="IDisposable.Dispose" />
    ///
    /// It is required because the dependency injection container disposes the singletons synchronously
    /// </summary>
    public void Dispose ()
    {
      DisposeAsync ().AsTask ().GetAwaiter ().GetResult ();
    }
  }
}
