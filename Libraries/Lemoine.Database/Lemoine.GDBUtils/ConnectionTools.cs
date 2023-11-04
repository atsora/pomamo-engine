// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using Lemoine.Core.Log;

namespace Lemoine.GDBUtils
{
  /// <summary>
  /// Tools to check a connection or some status on the server
  /// </summary>
  public static class ConnectionTools
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ConnectionTools).FullName);

    /// <summary>
    /// Execute a request that returns a scalar
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    /// <exception cref="Exception">in case the request or the connection fails</exception>
    static object ExecuteScalar (string connectionString, string query)
    {
      object requestResult;
      using (var connection = new Npgsql.NpgsqlConnection (connectionString)) {
        connection.Open ();
        using (var command = connection.CreateCommand ()) {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
          command.CommandText = query;
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
          try {
            requestResult = command.ExecuteScalar ();
          }
          catch (Exception ex) {
            log.Error ($"ExecuteScalar: request error in SQL query {command.CommandText}", ex);
            throw;
          }
        }
      } // Dispose normally closes the connection ( "using{...}" avoid connection.Close(); )

      return requestResult;
    }

    /// <summary>
    /// Execute a request that returns a scalar
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    /// <exception cref="Exception">in case the request or the connection fails</exception>
    static async System.Threading.Tasks.Task<object> ExecuteScalarAsync (string connectionString, string query)
    {
      object requestResult;
      using (var connection = new Npgsql.NpgsqlConnection (connectionString)) {
        connection.Open ();
        using (var command = connection.CreateCommand ()) {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
          command.CommandText = query;
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
          try {
            requestResult = await command.ExecuteScalarAsync ();
          }
          catch (Exception ex) {
            log.Error ($"ExecuteScalar: request error in SQL query {command.CommandText}", ex);
            throw;
          }
        }
      } // Dispose normally closes the connection ( "using{...}" avoid connection.Close(); )

      return requestResult;
    }

    /// <summary>
    /// Execute a request that returns a scalar (generic version)
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    /// <exception cref="Exception">in case the request or the connection fails</exception>
    public static T ExecuteScalar<T> (string connectionString, string query)
    {
      return (T)ExecuteScalar (connectionString, query);
    }

    /// <summary>
    /// Execute a request that returns a scalar (generic version)
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="query"></param>
    /// <returns></returns>
    /// <exception cref="Exception">in case the request or the connection fails</exception>
    static async System.Threading.Tasks.Task<T> ExecuteScalarAsync<T> (string connectionString, string query)
    {
      return (T) await ExecuteScalarAsync (connectionString, query);
    }

    /// <summary>
    /// Get the server version number, for example: 90501
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception">in case the request or connection fails or if the result is not a valid integer</exception>
    public static int GetPostgreSQLVersionNum (string connectionString)
    {
      var requestResult = ExecuteScalar<string> (connectionString, "show server_version_num;");
      int postgreSQLVersionNum = int.Parse (requestResult);
      return postgreSQLVersionNum;
    }

    /// <summary>
    /// Get the server version number, for example: 90501
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception">in case the request or connection fails or if the result is not a valid integer</exception>
    public static async System.Threading.Tasks.Task<int> GetPostgreSQLVersionNumAsync (string connectionString)
    {
      var requestResult = await ExecuteScalarAsync<string> (connectionString, "show server_version_num;");
      int postgreSQLVersionNum = int.Parse (requestResult);
      return postgreSQLVersionNum;
    }

    /// <summary>
    /// Return the server version, for example: 9.05.01
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception">in case the request or connection fails</exception>
    public static string GetPostgreSQLVersion (string connectionString)
    {
      var requestResult = ExecuteScalar<string> (connectionString, "show server_version;");
      return requestResult;
    }

    /// <summary>
    /// Check if the server version is greater than the specified version
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="versionNumber"></param>
    /// <returns></returns>
    /// <exception cref="Exception">for example in case of database connection error</exception>
    public static bool IsPostgreSQLVersionGreaterOrEqual (string connectionString, int versionNumber)
    {
      int serverVersionNum;
      try {
        serverVersionNum = GetPostgreSQLVersionNum (connectionString);
      }
      catch (Exception ex) {
        log.Error ($"IsServerVersionGreaterOrEqual: versionNumber={versionNumber}", ex);
        throw;
      }

      return serverVersionNum >= versionNumber;
    }

    /// <summary>
    /// Check if the server version is greater than the specified version
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="versionNumber"></param>
    /// <returns></returns>
    /// <exception cref="Exception">for example in case of database connection error</exception>
    public static async System.Threading.Tasks.Task<bool> IsPostgreSQLVersionGreaterOrEqualAsync (string connectionString, int versionNumber)
    {
      int serverVersionNum;
      try {
        serverVersionNum = await GetPostgreSQLVersionNumAsync (connectionString);
      }
      catch (Exception ex) {
        log.Error ($"IsServerVersionGreaterOrEqual: versionNumber={versionNumber}", ex);
        throw;
      }

      return serverVersionNum >= versionNumber;
    }

    /// <summary>
    /// Get the current PID
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception">in case the request or connection fails</exception>
    public static int GetCurrentPid (string connectionString)
    {
      var requestResult = ExecuteScalar<int> (connectionString, "select pg_backend_pid();");
      return requestResult;
    }

    /// <summary>
    /// Kill an active or idle in transaction connection
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="pid"></param>
    /// <returns>a connection was killed</returns>
    public static bool KillActiveConnection (string connectionString, int pid)
    {
      Debug.Assert (!string.IsNullOrEmpty (connectionString));

      try {
        if (!IsPostgreSQLVersionGreaterOrEqual (connectionString, 90200)) {
          log.FatalFormat ("KillActiveConnection: PostgreSQL version is < 9.2 => not supported");
          throw new NotImplementedException ();
        }
      }
      catch (Exception ex) {
        log.Error ("KillActiveConnection: could not check the version of PostgreSQL, give up", ex);
        throw;
      }

      string query = string.Format (@"
select pg_terminate_backend(a.pid)
from pg_stat_activity a
where a.pid = {0}
  and a.state in ('active', 'idle in transaction')
;
", pid);
      var requestResult = ExecuteScalar<bool?> (connectionString, query);

      if (requestResult.HasValue) {
        if (requestResult.Value) {
          if (log.IsDebugEnabled) {
            log.DebugFormat ("KillActiveConnection: pid={0} terminated", pid);
          }
          return true;
        }
        else { // !requestResult.Value
          if (log.IsErrorEnabled) {
            log.ErrorFormat ("KillActiveConnection: active or idle in transaction pid={0} not terminated", pid);
          }
          return false;
        }
      }
      else { // No value
        if (log.IsDebugEnabled) {
          log.DebugFormat ("KillActiveConnection: pid={0} not active or idle in transaction", pid);
        }
        return false;
      }
    }

    /// <summary>
    /// Kill orphaned connections for the same client IP address and the same application name
    /// </summary>
    /// <param name="connectionString"></param>
    /// <returns>The number of killed connections (-1 in case the PostgreSQL version is not valid)</returns>
    /// <exception cref="Exception">in case the SQL request fails</exception>
    public static long KillOrphanedConnections (string connectionString)
    {
      try {
        if (!IsPostgreSQLVersionGreaterOrEqual (connectionString, 90200)) {
          log.FatalFormat ("KillOrphanedConnections: PostgreSQL version is < 9.2 => not supported");
          return -1;
        }
      }
      catch (Exception ex) {
        log.Error ("KillOrphanedConnections: could not check the version of PostgreSQL, give up", ex);
        throw;
      }

      string query = @"
with current_connection as (
  select *
  from pg_stat_activity
  where pid=pg_backend_pid()
)
, terminated as (
  select pg_terminate_backend(a.pid)
  from pg_stat_activity a, current_connection
  where a.client_addr = current_connection.client_addr
    and a.datname = current_connection.datname
    and a.application_name = current_connection.application_name
    and a.pid <> current_connection.pid
)
select count(*) from terminated
";
      long requestResult = ExecuteScalar<long> (connectionString, query);

      if (0 < requestResult) {
        log.Fatal ($"KillOrphanedConnections: {requestResult} orphaned connections were killed");
      }
      else {
        log.Debug ("KillOrphanedConnections: no killed connection");
      }
      return requestResult;
    }

    /// <summary>
    /// Kill orphaned connections for the same client IP address and the same application name
    /// </summary>
    /// <param name="connectionString"></param>
    /// <returns>The number of killed connections (-1 in case the PostgreSQL version is not valid)</returns>
    /// <exception cref="Exception">in case the SQL request fails</exception>
    public static async System.Threading.Tasks.Task<long> KillOrphanedConnectionsAsync (string connectionString)
    {
      try {
        if (!await IsPostgreSQLVersionGreaterOrEqualAsync (connectionString, 90200)) {
          log.FatalFormat ("KillOrphanedConnectionsAsync: PostgreSQL version is < 9.2 => not supported");
          return -1;
        }
      }
      catch (Exception ex) {
        log.Error ("KillOrphanedConnectionsAsync: could not check the version of PostgreSQL, give up", ex);
        throw;
      }

      string query = @"
with current_connection as (
  select *
  from pg_stat_activity
  where pid=pg_backend_pid()
)
, terminated as (
  select pg_terminate_backend(a.pid)
  from pg_stat_activity a, current_connection
  where a.client_addr = current_connection.client_addr
    and a.datname = current_connection.datname
    and a.application_name = current_connection.application_name
    and a.pid <> current_connection.pid
)
select count(*) from terminated
";
      long requestResult = await ExecuteScalarAsync<long> (connectionString, query);

      if (0 < requestResult) {
        log.FatalFormat ("KillOrphanedConnectionsAsync: {0} orphaned connections were killed", requestResult);
      }
      else {
        log.Debug ("KillOrphanedConnectionsAsync: no killed connection");
      }
      return requestResult;
    }

    /// <summary>
    /// Check there is no active data reader
    /// </summary>
    /// <param name="connection"></param>
    public static void CheckNoActiveDataReader (System.Data.IDbConnection connection)
    {
      Npgsql.NpgsqlConnection npgsqlConnection = connection as Npgsql.NpgsqlConnection;
      if (null != npgsqlConnection) {
        var fullConnectionState = npgsqlConnection.FullState;
        if ((fullConnectionState & System.Data.ConnectionState.Fetching)
            == System.Data.ConnectionState.Fetching) { // Fetching flag is active
          log.Fatal ("CheckNoActiveDataReader: there is an active data reader");
          Debug.Assert (false);
        }
      }
    }
  }
}
