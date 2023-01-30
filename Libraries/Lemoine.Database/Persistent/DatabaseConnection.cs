// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;
using NHibernate;

namespace Lemoine.Database.Persistent
{
  /// <summary>
  /// Extensions to <see cref="IDatabaseConnection"/>
  /// </summary>
  public static class DatabaseConnectionExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (DatabaseConnectionExtensions));

    /// <summary>
    /// Get the connection status
    /// </summary>
    /// <param name="databaseConnection"></param>
    /// <returns></returns>
    /// <exception cref="InvalidProgramException"></exception>
    public static ConnectionStatus GetStatus (this IDatabaseConnection databaseConnection)
    {
      switch (NHibernateHelper.Status) {
      case NHibernateStatus.NotInitialized:
        return ConnectionStatus.Stopped;
      case NHibernateStatus.StartInitializing:
      case NHibernateStatus.NotConfigured:
      case NHibernateStatus.Configured:
        return ConnectionStatus.Starting;
      case NHibernateStatus.Initialized:
        return ConnectionStatus.Started;
      case NHibernateStatus.ConfigurationError:
      case NHibernateStatus.MigrateDefaultError:
      case NHibernateStatus.BuildSessionError:
        return ConnectionStatus.Error;
      default:
        Debug.Assert (false);
        log.Error ($"GetStatus: unexpected NHibernateStatus {NHibernateHelper.Status}");
        throw new InvalidProgramException ("Unexpected NHibernateStatus");
      }
    }

    /// <summary>
    /// Check a very basic connection
    /// 
    /// Should be fast, even if the full connection is not initialized
    /// </summary>
    /// <exception cref="System.Exception">Connection problem</exception>
    public static void CheckBasicRequest (this IDatabaseConnection databaseConnection)
    {
      string connectionString = Lemoine.Info.GDBConnectionParameters
        .GetGDBConnectionString (NHibernateHelper.ApplicationName);
      ;
      Lemoine.GDBUtils.ConnectionTools.GetPostgreSQLVersion (connectionString);
    }

    /// <summary>
    /// Get the current connection ID in the database
    /// </summary>
    /// <returns></returns>
    public static int GetCurrentConnectionId (this IDatabaseConnection databaseConnection)
    {
      string connectionString = Lemoine.Info.GDBConnectionParameters
        .GetGDBConnectionString (NHibernateHelper.ApplicationName);
      return Lemoine.GDBUtils.ConnectionTools.GetCurrentPid (connectionString);
    }

    /// <summary>
    /// Kill an active or idle in transaction connection
    /// </summary>
    /// <param name="connectionId"></param>
    /// <returns>a connection was killed</returns>
    public static bool KillActiveConnection (this IDatabaseConnection databaseConnection, int connectionId)
    {
      string connectionString = Lemoine.Info.GDBConnectionParameters
        .GetGDBConnectionString (NHibernateHelper.ApplicationName);
      return Lemoine.GDBUtils.ConnectionTools.KillActiveConnection (connectionString, connectionId);
    }

    /// <summary>
    /// Get the connection to the database to execute directly some SQL requests
    /// </summary>
    /// <returns></returns>
    public static System.Data.Common.DbConnection GetConnection (this IDatabaseConnection databaseConnection)
    {
      return NHibernateHelper.GetCurrentSession ().Connection;
    }

    /// <summary>
    /// Initialize a proxy object
    /// </summary>
    /// <param name="proxy"></param>
    /// <returns></returns>
    public static void InitializeProxy (this IDatabaseConnection databaseConnection, object proxy)
    {
      NHibernateUtil.Initialize (proxy);
    }

    /// <summary>
    /// Flush the data
    /// </summary>
    public static void FlushData (this IDatabaseConnection databaseConnection)
    {
      NHibernateHelper.GetCurrentSession ()
        .Flush ();
    }

    /// <summary>
    /// Evict a data
    /// </summary>
    /// <param name="databaseConnection"></param>
    /// <param name="o"></param>
    public static void EvictData (this IDatabaseConnection databaseConnection, object o)
    {
      NHibernateHelper.GetCurrentSession ()
        .Evict (o);
    }

    /// <summary>
    /// Is a session active ?
    /// </summary>
    /// <returns></returns>
    public static bool IsDatabaseSessionActive (this IDatabaseConnection databaseConnection)
    {
      try {
        return (null != NHibernateHelper.GetCurrentSession ());
      }
      catch (NHibernate.HibernateException ex) {
        log.Debug ("IsSessionActive: HibernateException => return false", ex); //  Usually: No session bound to the current context
        return false;
      }
      catch (Exception ex) {
        log.Error ("IsSessionActive: not a HibernateException, throw", ex);
        throw;
      }
    }

    /// <summary>
    /// Is a transaction active ?
    /// </summary>
    /// <returns></returns>
    public static bool IsTransactionActive (this IDatabaseConnection databaseConnection)
    {
      try {
        ISession session = NHibernateHelper.GetCurrentSession ();
        return session?.GetCurrentTransaction ()?.IsActive ?? false;
      }
      catch (NHibernate.HibernateException ex) {
        log.Debug ("IsTransactionActive: HibernateException => return false", ex); //  Usually: No session bound to the current context
        return false;
      }
      catch (Exception ex) {
        log.Error ("IsTransactionActive: not a HibernateException, throw", ex);
        throw;
      }
    }

    /// <summary>
    /// Is the transaction read-only ?
    /// 
    /// In case of error, false is returned
    /// </summary>
    /// <returns></returns>
    public static bool IsTransactionReadOnly (this IDatabaseConnection databaseConnection)
    {
      string requestResult;
      var connection = databaseConnection.GetConnection ();
      using (var command = connection.CreateCommand ()) {
        command.CommandText = "show transaction_read_only;";
        try {
          requestResult = (string)command.ExecuteScalar ();
        }
        catch (Exception ex) {
          log.Error ($"IsTransactionReadOnly: request error in SQL query {command.CommandText}", ex);
          return false;
        }
      }

      return object.Equals (requestResult, "on");
    }

    /// <summary>
    /// Get the server version number, for example: 90501
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception">in case the request fails or if the result is not a valid integer</exception>
    public static int GetPostgreSQLVersionNumber (this IDatabaseConnection databaseConnection)
    {
      string requestResult;
      using (var session = databaseConnection.OpenSession ()) {
        var connection = databaseConnection.GetConnection ();
        using (var command = connection.CreateCommand ()) {
          command.CommandText = "show server_version_num;";
          try {
            requestResult = (string)command.ExecuteScalar ();
          }
          catch (Exception ex) {
            log.Error ($"GetServerVersionNum: request error in SQL query {command.CommandText}", ex);
            throw;
          }
        }
      }

      var postgreSQLVersionNum = int.Parse (requestResult);
      return postgreSQLVersionNum;
    }

    /// <summary>
    /// Check if the server version is greater than the specified version
    /// </summary>
    /// <param name="versionNumber"></param>
    /// <returns></returns>
    /// <exception cref="Exception">in case of database connection error for example</exception>
    public static bool IsPostgreSQLVersionGreaterOrEqual (this IDatabaseConnection databaseConnection, int versionNumber)
    {
      int serverVersionNum;
      try {
        serverVersionNum = databaseConnection.GetPostgreSQLVersionNum ();
      }
      catch (Exception ex) {
        log.ErrorFormat ("IsServerVersionGreaterOrEqual: " +
                         "versionNumber={0} " +
                         "exception {1}",
                         versionNumber, ex);
        throw;
      }

      return serverVersionNum >= versionNumber;
    }

    /// <summary>
    /// Return the server version, for example: 9.05.01
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception">in case the SQL request fails</exception>
    public static string GetPostgreSQLVersion (this IDatabaseConnection databaseConnection)
    {
      string requestResult;
      using (var session = databaseConnection.OpenSession ()) {
        var connection = databaseConnection.GetConnection ();
        using (var command = connection.CreateCommand ()) {
          command.CommandText = "show server_version;";
          try {
            requestResult = (string)command.ExecuteScalar ();
          }
          catch (Exception ex) {
            log.Error ($"GetServerVersion: request error in SQL query {command.CommandText}", ex);
            throw;
          }
        }
      }

      return requestResult;
    }

    /// <summary>
    /// Kill orphaned connections for the same client IP address and the same application name
    /// </summary>
    /// <returns>The number of killed connections (-1 in case the PostgreSQL version is not valid)</returns>
    /// <exception cref="Exception">in case the SQL request fails</exception>
    public static long KillOrphanedConnections (this IDatabaseConnection databaseConnection)
    {
      try {
        if (!databaseConnection.IsPostgreSQLVersionGreaterOrEqual (90200)) {
          log.Fatal ("KillOrphanedConnections: PostgreSQL version is < 9.2 => not supported");
          return -1;
        }
      }
      catch (Exception ex) {
        log.Error ("KillOrphanedConnections: could not check the version of PostgreSQL, give up", ex);
        throw;
      }

      long requestResult;
      using (var session = databaseConnection.OpenSession ()) {
        var connection = databaseConnection.GetConnection ();
        using (var command = connection.CreateCommand ()) {
          command.CommandText = @"
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
          try {
            object result = command.ExecuteScalar ();
            requestResult = (long)result;
          }
          catch (Exception ex) {
            log.Error ($"KillOrphanedConnections: request error in SQL query {command.CommandText}", ex);
            throw;
          }
        }
      }

      if (2 < requestResult) {
        log.FatalFormat ("KillOrphanedConnections: {0} orphaned connections were killed", requestResult);
      }
      else if (0 < requestResult) {
        log.InfoFormat ("KillOrphanedConnections: {0} orphaned connections were killed", requestResult);
      }
      else {
        log.Debug ("KillOrphanedConnections: no killed connection");
      }
      return requestResult;
    }

  }
}
