// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: GPL-2.0

using Lemoine.Cnc.OpcUaClientService.Configuration;
using Opc.Ua;

namespace Lemoine.Cnc.OpcUaClientService.Services
{
  /// <summary>
  /// Connection to the OPC UA server of one acquisition, so of one machine
  ///
  /// Its configuration, the connection properties as well as the data to read, comes from the
  /// acquisition itself, through the /xml requests. Whenever that configuration changes, for
  /// example because the url of the OPC UA server of that machine changed, the connection is
  /// initialized again.
  ///
  /// It owns one <see cref="Lemoine.Cnc.OpcUaClient"/> instance and it serializes the accesses to it,
  /// since that module is not thread safe. It also caches the values that were read, so that the
  /// different requests of a same acquisition cycle only trigger one read request on the OPC UA server.
  /// </summary>
  public sealed class AcquisitionSession : IAsyncDisposable
  {
    readonly string m_identifier;
    readonly ILogger m_logger;
    readonly SemaphoreSlim m_semaphore = new SemaphoreSlim (1, 1);
    readonly TimeSpan m_cacheDuration;
    readonly TimeSpan m_registrationDelay;

    /// <summary>
    /// Parameters that are registered in the current module instance
    /// </summary>
    readonly HashSet<string> m_registeredParameters = new HashSet<string> (StringComparer.Ordinal);

    /// <summary>
    /// Parameters that were requested by a /get request but that are not registered yet
    /// </summary>
    readonly HashSet<string> m_pendingParameters = new HashSet<string> (StringComparer.Ordinal);

    AcquisitionConfiguration m_configuration;
    Lemoine.Cnc.OpcUaClient? m_client = null;
    DateTime m_pendingSinceUtc = DateTime.MaxValue;
    DateTime m_lastReadUtc = DateTime.MinValue;
    bool m_lastReadSuccess = false;

    /// <summary>
    /// Module instance that is dedicated to the direct reads
    ///
    /// A direct read resets the prepared query of the module it is run on, so it must not be run
    /// on the instance that owns the registered parameters
    /// </summary>
    Lemoine.Cnc.OpcUaClient? m_directClient = null;
    DateTime m_lastDirectConnectionUtc = DateTime.MinValue;
    bool m_lastDirectConnectionSuccess = false;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="identifier">acquisition identifier, not null</param>
    /// <param name="configuration">not null</param>
    /// <param name="defaults">not null</param>
    /// <param name="logger">not null</param>
    public AcquisitionSession (string identifier, AcquisitionConfiguration configuration, OpcUaClientServiceOptions defaults, ILogger logger)
    {
      ArgumentNullException.ThrowIfNull (identifier);
      ArgumentNullException.ThrowIfNull (configuration);
      ArgumentNullException.ThrowIfNull (defaults);
      ArgumentNullException.ThrowIfNull (logger);

      m_identifier = identifier;
      m_configuration = configuration;
      m_logger = logger;
      m_cacheDuration = TimeSpan.FromMilliseconds (defaults.CacheDurationMs);
      m_registrationDelay = TimeSpan.FromMilliseconds (defaults.RegistrationDelayMs);

      AddPendingParameters (configuration);
    }

    /// <summary>
    /// Acquisition identifier
    /// </summary>
    public string Identifier => m_identifier;

    /// <summary>
    /// Current configuration
    /// </summary>
    public AcquisitionConfiguration Configuration => m_configuration;

    /// <summary>
    /// Apply a configuration a /xml request carries, then read and return all its data
    ///
    /// The connection is initialized again when the configuration differs from the current one
    /// </summary>
    /// <param name="configuration">not null</param>
    /// <param name="cancellationToken"></param>
    public async Task<IDictionary<string, object?>> ApplyConfigurationAsync (AcquisitionConfiguration configuration, CancellationToken cancellationToken)
    {
      ArgumentNullException.ThrowIfNull (configuration);

      await m_semaphore.WaitAsync (cancellationToken);
      try {
        if (!m_configuration.Equals (configuration)) {
          m_logger.LogInformation ($"ApplyConfigurationAsync: the configuration of acquisition {m_identifier} changed => initialize the connection again, new configuration is {configuration}");
          m_configuration = configuration;
          await DisposeClientAsync ();
          await DisposeDirectClientAsync ();
          m_registeredParameters.Clear ();
          m_pendingParameters.Clear ();
          m_pendingSinceUtc = DateTime.MaxValue;
          AddPendingParameters (configuration);
        }

        var client = await GetClientAsync (cancellationToken);
        await ReadAsync (client, cancellationToken);
        return ReadDataItems (client);
      }
      finally {
        m_semaphore.Release ();
      }
    }

    /// <summary>
    /// Read all the values of the current configuration
    /// </summary>
    /// <param name="cancellationToken"></param>
    public async Task<IDictionary<string, object?>> GetDataAsync (CancellationToken cancellationToken)
    {
      await m_semaphore.WaitAsync (cancellationToken);
      try {
        var client = await GetClientAsync (cancellationToken);
        await ReadAsync (client, cancellationToken);
        return ReadDataItems (client);
      }
      finally {
        m_semaphore.Release ();
      }
    }

    /// <summary>
    /// Read a value with one of the get methods of the OPC UA client module
    /// </summary>
    /// <param name="method">not null or empty</param>
    /// <param name="parameter">not null</param>
    /// <param name="cancellationToken"></param>
    /// <returns>nullable</returns>
    public async Task<object?> GetAsync (string method, string parameter, CancellationToken cancellationToken)
    {
      await m_semaphore.WaitAsync (cancellationToken);
      try {
        if (MethodDispatcher.IsDirectReadMethod (method)) {
          var directClient = await GetDirectClientAsync (cancellationToken);
          return await MethodDispatcher.InvokeDirectReadAsync (directClient, method, parameter);
        }

        if (!m_registeredParameters.Contains (parameter) && m_pendingParameters.Add (parameter)) {
          if (DateTime.MaxValue == m_pendingSinceUtc) {
            m_pendingSinceUtc = DateTime.UtcNow;
          }
          m_logger.LogInformation ($"GetAsync: parameter {parameter} of acquisition {m_identifier} is new, it will be registered in at most {m_registrationDelay}");
        }

        var client = await GetClientAsync (cancellationToken);
        await ReadAsync (client, cancellationToken);
        return MethodDispatcher.InvokeGet (client, method, parameter);
      }
      finally {
        m_semaphore.Release ();
      }
    }

    /// <summary>
    /// Read a property of the OPC UA client module
    /// </summary>
    /// <param name="property">not null or empty</param>
    /// <param name="cancellationToken"></param>
    /// <returns>nullable</returns>
    public async Task<object?> GetPropertyAsync (string property, CancellationToken cancellationToken)
    {
      await m_semaphore.WaitAsync (cancellationToken);
      try {
        var client = await GetClientAsync (cancellationToken);
        try {
          await ReadAsync (client, cancellationToken);
        }
        catch (Exception ex) {
          // A property such as ConnectionError must remain readable when the connection is down
          m_logger.LogDebug (ex, $"GetPropertyAsync: no valid read for acquisition {m_identifier}, but return the property {property} anyway");
        }
        return MethodDispatcher.GetProperty (client, property);
      }
      finally {
        m_semaphore.Release ();
      }
    }

    /// <summary>
    /// Write a value with the OPC UA client module
    /// </summary>
    /// <param name="method">not null or empty</param>
    /// <param name="parameter">not null</param>
    /// <param name="v">value to write, not null</param>
    /// <param name="cancellationToken"></param>
    public async Task SetAsync (string method, string parameter, object v, CancellationToken cancellationToken)
    {
      if (!MethodDispatcher.IsWriteMethod (method)) {
        throw new UnknownMethodException (method);
      }

      await m_semaphore.WaitAsync (cancellationToken);
      try {
        var client = await GetClientAsync (cancellationToken);
        await ConnectAsync (client, cancellationToken);
        await client.WriteAsync (parameter, new VariableNode { Value = new Variant (v) }, cancellationToken);
      }
      finally {
        m_semaphore.Release ();
      }
    }

    /// <summary>
    /// Read every data item of the current configuration
    ///
    /// The values that could not be read are omitted
    ///
    /// The caller must own the semaphore
    /// </summary>
    IDictionary<string, object?> ReadDataItems (Lemoine.Cnc.OpcUaClient client)
    {
      var data = new Dictionary<string, object?> ();
      foreach (var dataItem in m_configuration.DataItems) {
        try {
          data[dataItem.Key] = dataItem.Property is not null
            ? MethodDispatcher.GetProperty (client, dataItem.Property)
            : MethodDispatcher.InvokeGet (client, dataItem.Method ?? "Get", dataItem.Param ?? "");
        }
        catch (Exception ex) {
          m_logger.LogInformation (ex, $"ReadDataItems: {dataItem.Key} could not be read for acquisition {m_identifier} => skip it");
        }
      }
      return data;
    }

    void AddPendingParameters (AcquisitionConfiguration configuration)
    {
      foreach (var parameter in configuration.Parameters) {
        m_pendingParameters.Add (parameter);
      }
      if (0 < m_pendingParameters.Count) {
        // The whole list is known at once, so there is nothing to wait for
        m_pendingSinceUtc = DateTime.MinValue;
      }
    }

    /// <summary>
    /// Return the module instance to use, creating it again first when the set of parameters changed
    ///
    /// The caller must own the semaphore
    /// </summary>
    async Task<Lemoine.Cnc.OpcUaClient> GetClientAsync (CancellationToken cancellationToken)
    {
      if (m_client is not null && !IsRegistrationRequired ()) {
        return m_client;
      }

      // The module only records the parameters to monitor until its query is prepared:
      // a new instance is required to take new parameters into account
      if (m_client is not null) {
        await DisposeClientAsync ();
      }

      foreach (var parameter in m_pendingParameters) {
        m_registeredParameters.Add (parameter);
      }
      m_pendingParameters.Clear ();
      m_pendingSinceUtc = DateTime.MaxValue;

      var client = CreateClient ();
      foreach (var parameter in m_registeredParameters) {
        try {
          // The first call registers the parameter in the module and always fails: this is expected
          client.Get (parameter);
        }
        catch (Exception ex) {
          m_logger.LogTrace (ex, $"GetClientAsync: registration of {parameter} for acquisition {m_identifier}");
        }
      }
      m_client = client;
      m_lastReadUtc = DateTime.MinValue;
      m_lastReadSuccess = false;
      m_logger.LogInformation ($"GetClientAsync: acquisition {m_identifier} is connecting, {m_configuration}, with {m_registeredParameters.Count} parameter(s)");
      return client;
    }

    /// <summary>
    /// Return the module instance that is dedicated to the direct reads, connecting it if required
    ///
    /// The caller must own the semaphore
    /// </summary>
    async Task<Lemoine.Cnc.OpcUaClient> GetDirectClientAsync (CancellationToken cancellationToken)
    {
      if (m_directClient is null) {
        m_directClient = CreateClient ();
        m_logger.LogInformation ($"GetDirectClientAsync: acquisition {m_identifier} is opening a second session for the direct reads");
      }

      var now = DateTime.UtcNow;
      if (m_lastDirectConnectionUtc.Add (m_cacheDuration) <= now) {
        // No parameter is registered in that instance, so Start () only connects
        m_lastDirectConnectionSuccess = await Task.Run (() => m_directClient.Start (), cancellationToken);
        m_lastDirectConnectionUtc = DateTime.UtcNow;
      }
      if (!m_lastDirectConnectionSuccess) {
        throw new ConnectionNotAvailableException (m_identifier);
      }
      return m_directClient;
    }

    bool IsRegistrationRequired () =>
      0 < m_pendingParameters.Count && m_pendingSinceUtc.Add (m_registrationDelay) <= DateTime.UtcNow;

    /// <summary>
    /// Read the values of all the registered parameters, unless they are still in the cache
    ///
    /// The caller must own the semaphore
    /// </summary>
    async Task ReadAsync (Lemoine.Cnc.OpcUaClient client, CancellationToken cancellationToken)
    {
      var now = DateTime.UtcNow;
      if (now < m_lastReadUtc.Add (m_cacheDuration)) {
        if (m_lastReadSuccess) {
          return;
        }
        throw new ConnectionNotAvailableException (m_identifier);
      }

      // Start () connects if required, prepares the query if required, and reads all the registered nodes
      m_lastReadSuccess = await Task.Run (() => client.Start (), cancellationToken);
      m_lastReadUtc = DateTime.UtcNow;
      if (!m_lastReadSuccess) {
        m_logger.LogWarning ($"ReadAsync: no valid read for acquisition {m_identifier}");
        throw new ConnectionNotAvailableException (m_identifier);
      }
    }

    /// <summary>
    /// Only make sure the session with the OPC UA server is established,
    /// without requiring any of the registered parameters to be valid
    ///
    /// The caller must own the semaphore
    /// </summary>
    async Task ConnectAsync (Lemoine.Cnc.OpcUaClient client, CancellationToken cancellationToken)
    {
      try {
        await ReadAsync (client, cancellationToken);
      }
      catch (ConnectionNotAvailableException) {
        if (client.ConnectionError) {
          throw;
        }
        // The read of the registered parameters failed but the session is valid: this is enough here
        m_logger.LogDebug ($"ConnectAsync: no valid read for acquisition {m_identifier} but the session is established");
      }
    }

    Lemoine.Cnc.OpcUaClient CreateClient ()
    {
      var client = new Lemoine.Cnc.OpcUaClient {
        CncAcquisitionName = m_identifier
      };
      foreach (var connectionProperty in m_configuration.ConnectionProperties) {
        try {
          MethodDispatcher.SetProperty (client, connectionProperty.Key, connectionProperty.Value);
        }
        catch (Exception ex) {
          m_logger.LogError (ex, $"CreateClient: property {connectionProperty.Key} of acquisition {m_identifier} could not be set => ignore it");
        }
      }
      return client;
    }

    async Task DisposeClientAsync ()
    {
      var client = m_client;
      m_client = null;
      m_lastReadSuccess = false;
      m_lastReadUtc = DateTime.MinValue;
      await DisposeAsync (client);
    }

    async Task DisposeDirectClientAsync ()
    {
      var client = m_directClient;
      m_directClient = null;
      m_lastDirectConnectionSuccess = false;
      m_lastDirectConnectionUtc = DateTime.MinValue;
      await DisposeAsync (client);
    }

    async Task DisposeAsync (Lemoine.Cnc.OpcUaClient? client)
    {
      if (client is null) {
        return;
      }
      try {
        await client.DisposeAsync ();
      }
      catch (Exception ex) {
        m_logger.LogError (ex, $"DisposeAsync: disconnection of acquisition {m_identifier} failed");
      }
    }

    /// <summary>
    /// <see cref="IAsyncDisposable.DisposeAsync" />
    /// </summary>
    public async ValueTask DisposeAsync ()
    {
      await m_semaphore.WaitAsync ();
      try {
        await DisposeClientAsync ();
        await DisposeDirectClientAsync ();
      }
      finally {
        m_semaphore.Release ();
      }
      m_semaphore.Dispose ();
    }
  }
}
