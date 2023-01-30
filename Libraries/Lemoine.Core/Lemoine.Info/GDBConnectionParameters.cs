// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;

namespace Lemoine.Info
{
  /// <summary>
  /// GDB Connection Parameters retrieved from the configuration parameters
  /// and the environment variable DefaultDSNName
  /// </summary>
  public sealed class GDBConnectionParameters: ConnectionParameters
  {
    /// <summary>
    /// The time to wait (in seconds) while trying to establish a connection
    /// before terminating the attempt and generating an error.
    /// 
    /// Default: 15 (set to -1 to use it)
    /// </summary>
    static readonly string TIMEOUT_KEY = "Database.Npgsql.Timeout";
    static readonly int TIMEOUT_DEFAULT = -1;

    /// <summary>
    /// Npgsql configuration key to use to set a command timeout
    /// 
    /// In the documentation, it is referenced Command Timeout
    /// The code property is named CommandTimeout
    /// 
    /// With Npgsql 4, both work
    /// </summary>
    static readonly string NPGSQL_COMMAND_TIMEOUT_KEY_KEY = "Database.Npgsql.CommandTimeout.Key";
    static readonly string NPGSQL_COMMAND_TIMEOUT_KEY_DEFAULT = "Command Timeout";

    /// <summary>
    /// The time to wait (in seconds) while trying to execute a command
    /// before terminating the attempt and generating an error.
    /// Set to zero for infinity.
    /// 
    /// Default: 30 (set to -1 to use it)
    /// </summary>
    static readonly string COMMAND_TIMEOUT_KEY = "Database.Npgsql.CommandTimeout";
    static readonly int COMMAND_TIMEOUT_DEFAULT = 0;

    /// <summary>
    /// The number of seconds of connection inactivity before Npgsql sends a keepalive query.
    /// 
    /// Default: disabled (set to -1 to use it)
    /// </summary>
    static readonly string KEEPALIVE_KEY = "Database.Npgsql.Keepalive";
    static readonly int KEEPALIVE_DEFAULT = -1;

    /// <summary>
    /// The number of seconds (from Npgsql 5.0, previously milliseconds) of connection inactivity
    /// before a TCP keepalive query is sent.
    /// Use of this option is discouraged, use KeepAlive instead if possible.
    /// (before Npgsql 5.0, supported only on Windows)
    /// 
    /// Default: disabled (set to -1 to use it)
    /// </summary>
    static readonly string TCP_KEEPALIVE_TIME_KEY = "Database.Npgsql.TcpKeepaliveTime";
    static readonly int TCP_KEEPALIVE_TIME_DEFAULT = -1;

    /// <summary>
    /// Determines the size of the internal buffer Npgsql uses when reading.
    /// Increasing may improve performance if transferring large values from the database.
    /// 
    /// Default: 8192 (set to -1 to use it)
    /// </summary>
    static readonly string READ_BUFFER_SIZE_KEY = "Database.Npgsql.ReadBufferSize";
    static readonly int READ_BUFFER_SIZE_DEFAULT = -1;

    /// <summary>
    /// Determines the size of the internal buffer Npgsql uses when writing.
    /// Increasing may improve performance if transferring large values to the database.
    /// 
    /// Default: 8192 (set to -1 to use it)
    /// </summary>
    static readonly string WRITE_BUFFER_SIZE_KEY = "Database.Npgsql.WriteBufferSize";
    static readonly int WRITE_BUFFER_SIZE_DEFAULT = -1;

    /// <summary>
    /// Whether connection pooling should be used.
    /// 
    /// Default: true
    /// </summary>
    static readonly string POOLING_KEY = "Database.Npgsql.Pooling";
    static readonly bool POOLING_DEFAULT = true;

    /// <summary>
    /// The minimum connection pool size.
    /// 
    /// Default: 0 (set to 0 to use it)
    /// </summary>
    static readonly string MIN_POOL_SIZE_KEY = "Database.Npgsql.MinPoolSize";
    static readonly int MIN_POOL_SIZE_DEFAULT = -1; // -1: default value

    /// <summary>
    /// The maximum connection pool size.
    /// 
    /// Default: 100 (set to 0 to use it)
    /// </summary>
    static readonly string MAX_POOL_SIZE_KEY = "Database.Npgsql.MaxPoolSize";
    static readonly int MAX_POOL_SIZE_DEFAULT = 10; // 0: default value

    /// <summary>
    /// The time to wait before closing idle connections in the pool
    /// if the count of all connections exceeds Minimum Pool Size
    /// 
    /// Default: 300s (set to 0 to use it)
    /// </summary>
    static readonly string CONNECTION_IDLE_LIFETIME_KEY = "Database.Npgsql.ConnectionIdleLifetime";
    static readonly TimeSpan CONNECTION_IDLE_LIFETIME_DEFAULT = TimeSpan.FromSeconds (0); // 0s: default value

    static readonly ILog log = LogManager.GetLogger(typeof (GDBConnectionParameters).FullName);

    #region Getters / Setters
    /// <summary>
    /// Connection string
    /// </summary>
    public override string ConnectionString
    {
      get
      {
        if (log.IsDebugEnabled) {
          log.Debug ($"ConnectionString.get: /B");
        }
        var connectionString = base.ConnectionString;
        connectionString = connectionString.TrimEnd (new char[] { ';' });
        connectionString += ";";
        if (log.IsDebugEnabled) {
          log.Debug ($"ConnectionString.get: about to add some parameters");
        }
        var timeout = Lemoine.Info.ConfigSet
          .LoadAndGet (TIMEOUT_KEY, TIMEOUT_DEFAULT);
        if (log.IsDebugEnabled) {
          log.Debug ($"ConnectionString.get: timeout={timeout}");
        }
        if (-1 != timeout) {
          connectionString += $"Timeout={timeout};";
        }
        var commandTimeout = Lemoine.Info.ConfigSet
          .LoadAndGet (COMMAND_TIMEOUT_KEY, COMMAND_TIMEOUT_DEFAULT);
        if (-1 != commandTimeout) {
          var npgsqlCommandTimeoutKey = Lemoine.Info.ConfigSet
            .LoadAndGet (NPGSQL_COMMAND_TIMEOUT_KEY_KEY, NPGSQL_COMMAND_TIMEOUT_KEY_DEFAULT);
          if (log.IsDebugEnabled) {
            log.Debug ($"ConnectionString.get: about to add {npgsqlCommandTimeoutKey}={commandTimeout}");
          }
          connectionString += $"{npgsqlCommandTimeoutKey}={commandTimeout};";
        }
        var keepalive = Lemoine.Info.ConfigSet
          .LoadAndGet (KEEPALIVE_KEY, KEEPALIVE_DEFAULT);
        if (-1 != keepalive) {
          if (log.IsDebugEnabled) {
            log.Debug ($"ConnectionString.get: about to add Keepalive={keepalive}");
          }
          connectionString += $"Keepalive={keepalive};";
        }
        var tcpKeepAliveTime = Lemoine.Info.ConfigSet
          .LoadAndGet (TCP_KEEPALIVE_TIME_KEY, TCP_KEEPALIVE_TIME_DEFAULT);
        if (-1 != tcpKeepAliveTime) {
          if (log.IsDebugEnabled) {
            log.Debug ($"ConnectionString.get: about to add Tcp Keepalive Time={tcpKeepAliveTime}");
          }
          connectionString += $"Tcp Keepalive=true;Tcp Keepalive Time={tcpKeepAliveTime};";
        }
        var readBufferSize = Lemoine.Info.ConfigSet
          .LoadAndGet (READ_BUFFER_SIZE_KEY, READ_BUFFER_SIZE_DEFAULT);
        if (-1 != readBufferSize) {
          if (log.IsDebugEnabled) {
            log.Debug ($"ConnectionString.get: about to add Read Buffer Size={readBufferSize}");
          }
          connectionString += $"Read Buffer Size={readBufferSize};";
        }
        var writeBufferSize = Lemoine.Info.ConfigSet
          .LoadAndGet (WRITE_BUFFER_SIZE_KEY, WRITE_BUFFER_SIZE_DEFAULT);
        if (-1 != writeBufferSize) {
          if (log.IsDebugEnabled) {
            log.Debug ($"ConnectionString.get: about to add Write Buffer Size={writeBufferSize}");
          }
          connectionString += $"Write Buffer Size={writeBufferSize};";
        }
        var pooling = Lemoine.Info.ConfigSet
          .LoadAndGet (POOLING_KEY, POOLING_DEFAULT);
        if (!pooling) {
          if (log.IsDebugEnabled) {
            log.Debug ($"ConnectionString.get: about to set Pooling=false");
          }
          connectionString += $"Pooling=false;";
        }
        var minPoolSize = Lemoine.Info.ConfigSet
          .LoadAndGet (MIN_POOL_SIZE_KEY, MIN_POOL_SIZE_DEFAULT);
        if (-1 < minPoolSize) {
          if (log.IsDebugEnabled) {
            log.Debug ($"ConnectionString.get: about to add Minimum Pool Size={minPoolSize}");
          }
          connectionString += $"Minimum Pool Size={minPoolSize};";
        }
        // See https://support.dalibo.com/view.php?id=11110
        // and https://github.com/npgsql/doc/issues/17#issuecomment-539984762
        // and https://github.com/npgsql/doc/pull/5#issuecomment-426891220
        // both "Maximum Pool Size" an MaxPoolSize can be used
        // Let's use "Maximum Pool Size" from https://www.npgsql.org/doc/connection-string-parameters.html
        var maxPoolSize = Lemoine.Info.ConfigSet
          .LoadAndGet (MAX_POOL_SIZE_KEY, MAX_POOL_SIZE_DEFAULT);
        if (0 < maxPoolSize) {
          if (log.IsDebugEnabled) {
            log.Debug ($"ConnectionString.get: about to add Maximum Pool Size={maxPoolSize}");
          }
          connectionString += $"Maximum Pool Size={maxPoolSize};";
        }
        var connectionIdleLifetime = Lemoine.Info.ConfigSet
          .LoadAndGet (CONNECTION_IDLE_LIFETIME_KEY, CONNECTION_IDLE_LIFETIME_DEFAULT);
        if (0 < connectionIdleLifetime.TotalSeconds) {
          if (log.IsDebugEnabled) {
            log.Debug ($"ConnectionString.get: about to add Connection Idle Lifetime={connectionIdleLifetime}");
          }
          connectionString += $"Connection Idle Lifetime={(int)connectionIdleLifetime.TotalSeconds};";
        }
        if (log.IsDebugEnabled) {
          log.Debug ($"ConnectionString.get: return {connectionString}");
        }
        return connectionString;
      }
    }

    /// <summary>
    /// Connection string (static)
    /// </summary>
    public static string GDBConnectionString
    {
      get { return Instance.ConnectionString;  }
    }
    #endregion

    #region Constructors
    /// <summary>
    /// Private constructor (singleton class !)
    /// </summary>
    private GDBConnectionParameters()
    {
      LoadGDBParameters ();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Get the connection string with a specified application name (static)
    /// </summary>
    /// <param name="applicationName"></param>
    /// <returns></returns>
    public static string GetGDBConnectionString (string applicationName)
    {
      try {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetGDBConnectionString: about to get the connection string for applicationName={applicationName}");
        }
        return Instance.GetConnectionString (applicationName);
      }
      catch (Exception ex) {
        log.Error ($"GetGDBConnectionString failed for applicationName={applicationName}", ex);
        throw;
      }
    }
    #endregion // Methods

    #region Instance
    static GDBConnectionParameters Instance
    {
      get { return Nested.instance; }
    }
    
    class Nested
    {
      // Explicit static constructor to tell C# compiler
      // not to mark type as beforefieldinit
      static Nested()
      {
      }

      internal static readonly GDBConnectionParameters instance = new GDBConnectionParameters ();
    }
    #endregion
  }
}
