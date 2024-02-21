// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using NHibernate;
using Lemoine.Core.Log;

namespace Lemoine.GDBUtils
{
  /// <summary>
  /// Utility class to parse the exceptions, from both Npgsql and NHibernate
  /// 
  /// The method here are not recursive
  /// </summary>
  public class DatabaseException : Lemoine.Core.ExceptionManagement.IExceptionTest
  {
    readonly ILog log = LogManager.GetLogger (typeof (DatabaseException).FullName);
    static readonly ILog slog = LogManager.GetLogger (typeof (DatabaseException).FullName);

    #region IExceptionTest implementation
    /// <summary>
    /// IExceptionTest implementation
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public bool RequiresExit (Exception ex, ILog logger)
    {
      if (IsOutOfMemory (ex, logger)) {
        return true;
      }

      if (IsDatabaseInsufficientResources (ex, logger)) {
        return true;
      }

      if (ex is NHibernate.TransactionException) { // If not TransactionFailure or
        if (Lemoine.Core.ExceptionManagement.ExceptionTest.IsTemporary (ex, logger)) {
          // TransactionSerializationFailure or a TimeoutFailure
          if ((logger != null) && logger.IsDebugEnabled) {
            logger.Debug ("RequiresExit: temporary exception => return false");
          }
          return false;
        }
        else if (Lemoine.Core.ExceptionManagement.ExceptionTest.IsTemporaryWithDelay (ex, logger)) {
          if ((logger != null) && logger.IsDebugEnabled) {
            logger.Debug ("RequiresExit: temporary with delay exception  => return false");
          }
          return false;
        }
        else { // Not a temporary or connection error
          if ((logger != null) && logger.IsFatalEnabled) {
            logger.FatalFormat ("RequiresExit: " +
                              "Transaction exception {0} with inner exception {1} " +
                              "=> return true",
                              ex, ex.InnerException);
          }
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// IExceptionTest implementation
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public bool RequiresExitExceptFromDatabase (Exception ex, ILog logger)
    {
      return false;
    }

    /// <summary>
    /// IExceptionTest implementation
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public bool IsStale (Exception ex, ILog logger)
    {
      if (ex is NHibernate.StaleObjectStateException) {
        if ((logger != null) && logger.IsInfoEnabled) {
          logger.Info ("IsStale: StaleObjectStateException", ex);
        }
        return true;
      }

      if (ex is NHibernate.StaleStateException) {
        if ((logger != null) && logger.IsDebugEnabled) {
          logger.Info ("IsStale: StaleStateException", ex);
        }
        return true;
      }

      return false;
    }

    /// <summary>
    /// IExceptionTest implementation
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public bool IsTemporary (Exception ex, ILog logger)
    {
      if (IsTransactionSerializationFailure (ex, logger)) {
        return true;
      }
      if (IsTimeoutFailure (ex, logger)) {
        return true;
      }
      if (IsTransientNpgsqlException (ex, logger)) {
        return true;
      }
      if (IsTransactionAborted (ex, logger)) {
        return true;
      }
      if ((ex is NHibernate.Exceptions.GenericADOException) && ex.Message.Contains ("could not lock")) {
        return true;
      }

      return false;
    }

    bool IsTransientNpgsqlException (Exception ex, ILog logger)
    {
      if (ex is Npgsql.NpgsqlException) {
        Npgsql.NpgsqlException npgsqlException = ex as Npgsql.NpgsqlException;
        if (npgsqlException.IsTransient) {
          if ((null != logger) && logger.IsInfoEnabled) {
            logger.Info ($"IsTransientNpgsqlException: transient exception => return true", ex);
          }
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// IExceptionTest implementation
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public bool IsTemporaryWithDelay (Exception ex, ILog logger)
    {
      if (IsDatabaseConnectionError (ex, logger)) {
        return true;
      }

      if (ex is Npgsql.PostgresException) {
        Npgsql.PostgresException postgresException = ex as Npgsql.PostgresException;
        if (postgresException.IsTransient) {
          if ((null != logger) && logger.IsWarnEnabled) {
            logger.Warn ($"IsTemporaryWithDelay: transient PostgresException with error code {postgresException.SqlState} {postgresException}, the transaction might succeed if retried", postgresException);
          }
          return true;
        }
        else if ((null != logger) && logger.IsDebugEnabled) {
          logger.Debug ($"IsTemporaryWithDelay: not a transient PostgresException", postgresException);
        }
      }
      else if (ex is Npgsql.NpgsqlException) {
        Npgsql.NpgsqlException npgsqlException = ex as Npgsql.NpgsqlException;
        if (npgsqlException.Message.ToLowerInvariant ().Contains ("The connection pool has been exhausted".ToLowerInvariant ())) {
          if ((logger != null) && logger.IsWarnEnabled) {
            logger.Warn ("IsTemporaryWithDelay: connection pool exhausted in NpgsqlException", npgsqlException);
          }
          return true;
        }
        else if ((logger != null) && logger.IsDebugEnabled) {
          logger.Debug ("IsTemporaryWithDelay: other NpgsqlException", npgsqlException);
        }
      }

      return false;
    }

    /// <summary>
    /// IExceptionTest implementation
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public bool IsInvalid (Exception ex, ILog logger)
    {
      if (IsIntegrityConstraintViolation (ex, logger)) {
        return true;
      }
      if (ex is NHibernate.ADOException) {
        if (Lemoine.Core.ExceptionManagement.ExceptionTest.IsTemporary (ex, logger)) {
          if ((logger != null) && logger.IsDebugEnabled) {
            logger.Debug ("IsInvalid: ADOException but it is only a temporary error => return false");
          }
          return false;
        }
        else if (Lemoine.Core.ExceptionManagement.ExceptionTest.IsTemporaryWithDelay (ex, logger)) {
          if ((logger != null) && logger.IsDebugEnabled) {
            logger.Debug ("IsInvalid: ADOException but it is only a temporary with delay error => return false");
          }
          return false;
        }
        else {
          // TODO: give up just for this request ? To check...
          if ((logger != null) && logger.IsErrorEnabled) {
            logger.Error ($"IsInvalid: ADOException with InnerException {ex.InnerException} => return true", ex);
            if (null != ex.InnerException) {
              logger.Error ($"IsInvalid: inner exception of ADO exception", ex.InnerException);
            }
          }
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// IExceptionTest implementation
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger"></param>
    /// <param name="details"></param>
    /// <returns></returns>
    public bool IsDatabaseException (Exception ex, ILog logger, out Lemoine.Core.ExceptionManagement.IDatabaseExceptionDetails details)
    {
      if (ex is Npgsql.PostgresException) {
        Npgsql.PostgresException postgresException = ex as Npgsql.PostgresException;
        details = new DatabaseExceptionDetails (postgresException);
        return true;
      }

      if (ex is Npgsql.NpgsqlException) {
        Npgsql.NpgsqlException npgsqlException = ex as Npgsql.NpgsqlException;
        details = new DatabaseExceptionDetails (npgsqlException);
        return true;
      }

      if (ex is NHibernate.ADOException) {
        if ((ex.InnerException is null) || !IsDatabaseException (ex.InnerException, logger, out var innerDetails)) {
          NHibernate.ADOException adoException = ex as NHibernate.ADOException;
          log.Warn ($"IsDatabaseException: NHibernate.ADOException with message {adoException.Message} on SQL {adoException.SqlString} while inner exception {ex.InnerException} is not a database exception", adoException);
          details = new DatabaseExceptionDetails (adoException);
          return true;
        }
      }

      details = null;
      return false;
    }

    /// <summary>
    /// <see cref="IExceptionTest" />
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public bool IsNotError (Exception ex, ILog logger)
    {
      return false;
    }

    /// <summary>
    /// Does this exception relate to a right problem? Admin required?
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public bool IsUnauthorized (Exception ex, ILog logger)
    {
      // Not related to the database today
      return false;
    }
    #endregion

    /// <summary>
    /// Check if a request failed because of an Integrity constraint violation
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public bool IsIntegrityConstraintViolation (Exception ex)
    {
      return IsIntegrityConstraintViolation (ex, log);
    }

    /// <summary>
    /// Check if a request failed because of an Integrity constraint violation
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public bool IsIntegrityConstraintViolation (Exception ex, ILog logger)
    {
      if (ex is Npgsql.PostgresException) {
        Npgsql.PostgresException postgresException = ex as Npgsql.PostgresException;
        if (postgresException.SqlState.StartsWith ("23", StringComparison.InvariantCultureIgnoreCase)) { // Integrity constraint violation class
          if ((logger != null) && logger.IsWarnEnabled) {
            logger.Warn ($"IsIntegrityConstraintViolation: error code {postgresException.SqlState} of class 23: Integrity constraint violation in PostgresException, the transaction might succeed if retried", postgresException);
          }
          return true;
        }
        else {
          if ((logger != null) && logger.IsInfoEnabled) {
            logger.Info ("IsIntegrityConstraintViolation: not an error of class 23: Integrity constraint violation PostgresException", postgresException);
          }
        }
      }

      if (ex is Npgsql.NpgsqlException) {
        if ((null != logger) && logger.IsInfoEnabled) {
          Npgsql.NpgsqlException npgsqlException = ex as Npgsql.NpgsqlException;
          logger.Info ("IsIntegrityConstraintViolation: NpgsqlException", npgsqlException);
        }
      }

      if (null != ex.InnerException) {
        if ((logger != null) && logger.IsDebugEnabled) {
          logger.Debug ("IsIntegrityConstraintViolation: inspect inner exception", ex.InnerException);
        }
        return IsIntegrityConstraintViolation (ex.InnerException, logger);
      }

      return false;
    }

    /// <summary>
    /// Check if a request failed because of a serialization failure (and then the request must be retried)
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public bool IsTransactionSerializationFailure (Exception ex)
    {
      return IsTransactionSerializationFailure (ex, log);
    }

    /// <summary>
    /// Check if a request failed because of a serialization failure (and then the request must be retried)
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public bool IsTransactionSerializationFailure (Exception ex, ILog logger)
    {
      if (ex is Npgsql.PostgresException) {
        Npgsql.PostgresException postgresException = ex as Npgsql.PostgresException;
        if (postgresException.SqlState.Equals ("40001")) { // serialization_failure
          if ((logger != null) && logger.IsInfoEnabled) {
            logger.Info ("IsSerializationFailure: serialization_failure in PostgresException, the transaction might succeed if retried", postgresException);
          }
          return true;
        }
        else if ((null != postgresException.Hint)
          && postgresException.Hint.Contains ("The transaction might succeed if retried.")) {
          if ((logger != null) && logger.IsInfoEnabled) {
            logger.Info ("IsSerializationFailure: other minor in PostgresException, the transaction might succeed if retried", postgresException);
          }
          return true;
        }
        else {
          if ((logger != null) && logger.IsInfoEnabled) {
            logger.InfoFormat ("IsSerializationFailure: not a serialization_failure PostgresException", postgresException);
          }
        }
      }

      if (ex is Npgsql.NpgsqlException) {
        if ((null != logger) && logger.IsInfoEnabled) {
          Npgsql.NpgsqlException npgsqlException = ex as Npgsql.NpgsqlException;
          logger.Info ("IsSerializationFailure: not a serialization_failure NpgsqlException", npgsqlException);
        }
      }

      if (null != ex.InnerException) {
        if ((logger != null) && logger.IsDebugEnabled) {
          logger.Debug ("IsTransactionSerializationFailure: inspect inner exception", ex.InnerException);
        }
        return IsTransactionSerializationFailure (ex.InnerException, logger);
      }

      return false;
    }

    /// <summary>
    /// Check if a request failed because of a timeout
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public bool IsTimeoutFailure (Exception ex)
    {
      return IsTimeoutFailure (ex, log);
    }

    /// <summary>
    /// Check if a request failed because of a timeout
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public bool IsTimeoutFailure (Exception ex, ILog logger)
    {
      if (ex is Npgsql.PostgresException) {
        Npgsql.PostgresException postgresException = ex as Npgsql.PostgresException;
        if (postgresException.Message.Contains ("timeout")
            && (postgresException.StackTrace.Contains ("ExecuteNonQuery"))) {
          if ((logger != null) && logger.IsWarnEnabled) {
            logger.Warn ("IsTimeoutFailure: timeout in PostgresException", postgresException);
          }
          return true;
        }
        else {
          if ((logger != null) && logger.IsInfoEnabled) {
            logger.Info ("IsTimeoutFailure: no timeout in PostgresException", postgresException);
          }
        }
      }

      if (ex is Npgsql.NpgsqlException) {
        Npgsql.NpgsqlException npgsqlException = ex as Npgsql.NpgsqlException;
        if (npgsqlException.Message.Contains ("timeout")
            && (npgsqlException.StackTrace.Contains ("Npgsql.NpgsqlCommand.ExecuteNonQuery"))) {
          if ((logger != null) && logger.IsWarnEnabled) {
            logger.Warn ("IsTimeoutFailure: timeout in NpgsqlException", npgsqlException);
          }
          return true;
        }
        else if (npgsqlException.InnerException is TimeoutException) { // New in Npgsql 5.0
          if ((logger != null) && logger.IsWarnEnabled) {
            logger.Warn ("IsTimeoutFailure: timeout in NpgsqlException", npgsqlException);
          }
          return true;
        }
        else {
          if ((logger != null) && logger.IsInfoEnabled) {
            logger.Info ("IsTimeoutFailure: no timeout in NpgsqlException", npgsqlException);
          }
        }
      }

      if (null != ex.InnerException) {
        if ((logger != null) && logger.IsDebugEnabled) {
          logger.Debug ("IsTimeoutFailure: inspect inner exception", ex.InnerException);
        }
        return IsTimeoutFailure (ex.InnerException, logger);
      }

      return false;
    }

    /// <summary>
    /// Check if a request failed because of the current transaction is aborted
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public bool IsTransactionAborted (Exception ex)
    {
      return IsTransactionAborted (ex, slog);
    }

    /// <summary>
    /// Check if a request failed because of the current transaction is aborted
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public bool IsTransactionAborted (Exception ex, ILog logger)
    {
      if (ex is Npgsql.PostgresException) {
        Npgsql.PostgresException postgresException = ex as Npgsql.PostgresException;
        if (postgresException.SqlState.Equals ("25P02")) { // in_failed_sql_transaction
          if ((logger != null) && logger.IsInfoEnabled) {
            logger.Info ("IsTransactionAborted: in_failed_sql_transaction in NpgsqlException", postgresException);
          }
          return true;
        }
        else {
          if ((logger != null) && logger.IsDebugEnabled) {
            logger.Debug ("IsTransactionAborted: not a in_failed_sql_transaction NpgsqlException", postgresException);
          }
        }
      }

      if (ex is Npgsql.NpgsqlException) {
        if ((logger != null) && logger.IsDebugEnabled) {
          Npgsql.NpgsqlException npgsqlException = ex as Npgsql.NpgsqlException;
          logger.Debug ("IsTransactionAborted: not a in_failed_sql_transaction NpgsqlException", npgsqlException);
        }
      }

      if (null != ex.InnerException) {
        if ((logger != null) && logger.IsDebugEnabled) {
          logger.Debug ("IsTransactionAborted: inspect inner exception", ex.InnerException);
        }
        return IsTransactionAborted (ex.InnerException, logger);
      }

      return false;
    }

    /// <summary>
    /// Check if a request failed because of a connection error:
    /// one of the error code that starts with 57P:
    /// <item>57P01 	admin_shutdown</item>
    /// <item>57P02 	crash_shutdown</item>
    /// <item>57P03 	cannot_connect_now</item>
    /// <item>57P04 	database_dropped</item>
    /// or Npgsql exception is 'Failed to establish a connection'
    /// or the exception message is 'Timeout while getting a connection from pool' at Npgsql.NpgsqlConnectorPool.RequestPooledConnector
    /// or the exception message is 'The Connection is broken'
    /// or the exception message is 'Exception while connecting'
    /// </summary>
    /// <param name="ex">not null</param>
    /// <param name="logger">nullable</param>
    /// <returns></returns>
    public bool IsDatabaseConnectionError (Exception ex, ILog logger)
    {
      var message = ex?.Message ?? "";
      if (ex.StackTrace is null) {
        // According to the documentation of .Net 5, it is not supposed to be null
        log.Fatal ("IsDatabaseConnectionError: stack trace is null", ex);
      }
      if (ex is Npgsql.PostgresException) {
        Npgsql.PostgresException postgresException = ex as Npgsql.PostgresException;
        if (postgresException is null) {
          logger?.Fatal ("IsDatabaseConnectionError: not a valid PostgresException", ex);
        }
        else { // postgresException is not null
          if (postgresException.SqlState.StartsWith ("57P", StringComparison.InvariantCultureIgnoreCase)) {
            if ((logger != null) && logger.IsInfoEnabled) {
              logger.Info ($"IsDatabaseConnectionError: error code {postgresException.SqlState} in PostgresException", postgresException);
            }
            return true;
          }
          else if (postgresException.SqlState.Equals ("28000", StringComparison.InvariantCultureIgnoreCase)) { // invalid_authorization_specification
            // Raised for example if pg_hba is not valid. This is used during the maintenance
            // Example: 28000: no pg_hba.conf entry for host "192.168.x.y"...
            if ((logger != null) && logger.IsInfoEnabled) {
              logger.Info ($"IsDatabaseConnectionError: error code {postgresException.SqlState} in PostgresException", postgresException);
            }
            return true;
          }
          else if (postgresException.SqlState.StartsWith ("800", StringComparison.InvariantCultureIgnoreCase)) { // Class 08 - Connection Exception
            if ((logger != null) && logger.IsInfoEnabled) {
              logger.Info ($"IsDatabaseConnectionError: error code {postgresException.SqlState} in PostgresException", postgresException);
            }
            return true;
          }
          else if (message.ToLowerInvariant ().Contains ("Failed to establish a connection".ToLowerInvariant ())) {
            if ((logger != null) && logger.IsInfoEnabled) {
              logger.Info ("IsDatabaseConnectionError: Failed to establish a connection in PostgresException", postgresException);
            }
            return true;
          }
          else if (message.ToLowerInvariant ().Contains ("The Connection is broken".ToLowerInvariant ())) {
            if ((logger != null) && logger.IsInfoEnabled) {
              logger.Info ("IsDatabaseConnectionError: The connection is broken in PostgresException", postgresException);
            }
            return true;
          }
          else if (IsNullReferenceExceptionBugInClearPoolAndCreateException (postgresException)) {
            if ((logger != null) && logger.IsWarnEnabled) {
              logger.Warn ("IsDatabaseConnectionError: bug in Npgsql, connection broken should be returned instead");
            }
            return true;
          }
          else {
            if ((logger != null) && logger.IsDebugEnabled) {
              logger.Debug ($"IsDatabaseConnectionError: not a connection error. Error code is {postgresException.SqlState}", postgresException);
            }
          }
        }
      }
      else if (ex is Npgsql.NpgsqlException) {
        Npgsql.NpgsqlException npgsqlException = ex as Npgsql.NpgsqlException;
        if (npgsqlException is null) {
          logger?.Fatal ("IsDatabaseConnectionError: not a valid NpgsqlException", ex);
        }
        else { // npgsqlException is not null
          if (message.ToLowerInvariant ().Contains ("Failed to establish a connection".ToLowerInvariant ())) {
            if ((logger != null) && logger.IsInfoEnabled) {
              logger.Info ("IsDatabaseConnectionError: Failed to establish a connection in NpgsqlException", npgsqlException);
            }
            return true;
          }
          else if (message.ToLowerInvariant ().Contains ("The Connection is broken".ToLowerInvariant ())) {
            if ((logger != null) && logger.IsInfoEnabled) {
              logger.Info ("IsDatabaseConnectionError: The connection is broken in NpgsqlException", npgsqlException);
            }
            return true;
          }
          else if (message.ToLowerInvariant ().Contains ("Exception while connecting".ToLowerInvariant ())) {
            if ((logger != null) && logger.IsInfoEnabled) {
              logger.Info ("IsDatabaseConnectionError: Exception while connecting in NpgsqlException", npgsqlException);
            }
            return true;
          }
          else if (message.ToLowerInvariant ().Contains ("Exception while reading from stream".ToLowerInvariant ())) {
            if ((logger != null) && logger.IsInfoEnabled) {
              logger.Info ("IsDatabaseConnectionError: Exception while reading from stream in NpgsqlException", npgsqlException);
            }
            return true;
          }
          else if (IsNullReferenceExceptionBugInClearPoolAndCreateException (npgsqlException)) {
            if ((logger != null) && logger.IsWarnEnabled) {
              logger.Warn ("IsDatabaseConnectionError: bug in Npgsql, connection broken should be returned instead");
            }
            return true;
          }
          else {
            if ((logger != null) && logger.IsDebugEnabled) {
              logger.Debug ("IsDatabaseConnectionError: not a connection error. NpgsqlException", npgsqlException);
            }
          }
        }
      }

      if ((null != ex.StackTrace) && ex.StackTrace.Contains ("Npgsql")) {
        if (ex.StackTrace.Contains ("Npgsql.NpgsqlConnectorPool.RequestPooledConnector")
          && message.ToLowerInvariant ().Contains ("Timeout while getting a connection from pool".ToLowerInvariant ())) {
          // Note: the following exception looks to be a database connection error:
          /*
System.Exception: Timeout while getting a connection from pool. at Npgsql.NpgsqlConnectorPool.RequestPooledConnector(NpgsqlConnection Connection) at Npgsql.NpgsqlConnectorPool.RequestConnector(NpgsqlConnection Connection) at Npgsql.NpgsqlConnection.Open() at NHibernate.Connection.DriverConnectionProvider.GetConnection() at NHibernate.AdoNet.ConnectionManager.GetConnection() at NHibernate.AdoNet.AbstractBatcher.Prepare(IDbCommand cmd) at NHibernate.AdoNet.AbstractBatcher.ExecuteReader(IDbCommand cmd) at NHibernate.Loader.Loader.GetResultSet(IDbCommand st, Boolean autoDiscoverTypes, Boolean callable, RowSelection selection, ISessionImplementor session) at NHibernate.Loader.Loader.DoQuery(ISessionImplementor session, QueryParameters queryParameters, Boolean returnProxies) at NHibernate.Loader.Loader.DoQueryAndInitializeNonLazyCollections(ISessionImplementor session, QueryParameters queryParameters, Boolean returnProxies) at NHibernate.Loader.Loader.DoList(ISessionImplementor session, QueryParameters queryParameters) --- End of inner exception stack trace --- at NHibernate.Loader.Loader.DoList(ISessionImplementor session, QueryParameters queryParameters) at NHibernate.Loader.Loader.ListIgnoreQueryCache(ISessionImplementor session, QueryParameters queryParameters) at NHibernate.Loader.Loader.List(ISessionImplementor session, QueryParameters queryParameters, ISet1 querySpaces, IType[] resultTypes)
at NHibernate.Impl.SessionImpl.List(CriteriaImpl criteria, IList results)
          */
          // Or does it correspond to an OutOfMemory ?
          if ((logger != null) && logger.IsInfoEnabled) {
            logger.Info ("IsDatabaseConnectionError: timeout while getting a connection from pool", ex);
          }
          return true;
        }
        if (ex is System.Net.Sockets.SocketException) {
          var socketException = ex as System.Net.Sockets.SocketException;
          if (socketException is null) {
            logger?.Fatal ("IsDatabaseConnectionError: not a valid SocketException", ex);
          }
          else { // socketException is not null
            if (socketException.ErrorCode == (int)System.Net.Sockets.SocketError.ConnectionReset) {
              // WSAECONNRESET 10054 Connection reset by peer
              // socketException.Message.ToLowerInvariant ().Contains ("An existing connection was forcibly closed by the remote host".ToLowerInvariant ())
              if ((logger != null) && logger.IsInfoEnabled) {
                logger.Info ("IsDatabaseConnectionError: An existing connection was forcily closed by the remote host in SocketException", socketException);
              }
              return true;
            }
            else {
              if ((logger != null) && logger.IsDebugEnabled) {
                logger.Debug ("IsDatabaseConnectionError: not a connection error. SocketException", socketException);
              }
            }
          }
        }
        if (ex is System.IO.IOException) {
          var ioException = ex as System.IO.IOException;
          if (ioException is null) {
            logger?.Fatal ("IsDatabaseConnectionError: not a valid IOException", ex);
          }
          else { // postgresException is not null
            if (message.ToLowerInvariant ().Contains ("Unable to read data from the transport connection".ToLowerInvariant ())) {
              if ((logger != null) && logger.IsInfoEnabled) {
                logger.Info ("IsDatabaseConnectionError: Unable to read data from the transport connection in IOException", ioException);
              }
              return true;
            }
            else {
              if ((logger != null) && logger.IsDebugEnabled) {
                logger.DebugFormat ("IsDatabaseConnectionError: not a connection error. IOException", ioException);
              }
            }
          }
        }
      }

      if (message.Contains ("NullReferenceException")
        && (null != ex.StackTrace)
        && ex.StackTrace.Contains ("ClearPoolAndCreateException")) {
        // Note: the following exception looks to be a database connection error:
        /*
   System.NullReferenceException: Object reference not set to an instance of an object.
   at Npgsql.NpgsqlCommand.ClearPoolAndCreateException(Exception e)
   at Npgsql.ForwardsOnlyDataReader.NextResult()
   at Npgsql.ForwardsOnlyDataReader..ctor(IEnumerable`1 dataEnumeration, CommandBehavior behavior, NpgsqlCommand command, NotificationThreadBlock threadBlock, Boolean synchOnReadError)
   at Npgsql.NpgsqlCommand.GetReader(CommandBehavior cb)
   at Npgsql.NpgsqlCommand.ExecuteBlind()
   at Npgsql.NpgsqlTransaction.Rollback()
   at NHibernate.Transaction.AdoTransaction.Rollback()
         */
        if ((logger != null) && logger.IsWarnEnabled) {
          logger.Warn ("IsDatabaseConnectionError: NullReferenceException in Npgsql.NpgsqlCommand.ClearPoolAndCreateException, probably a database connection error", ex);
        }
        return true;
      }

      if (null != ex.InnerException) {
        if ((logger != null) && logger.IsDebugEnabled) {
          logger.Debug ("IsDatabaseConnectionError: inspect inner exception", ex.InnerException);
        }
        return IsDatabaseConnectionError (ex.InnerException, logger);
      }

      return false;
    }

    bool IsNullReferenceExceptionBugInClearPoolAndCreateException (Exception ex)
    {
      // https://stackoverflow.com/questions/27951362/nullreference-when-disposing-session-with-transaction
      // To remove after the upgrade to Npgsql >= 4.x
      if (ex is NullReferenceException) {
        if (ex.StackTrace.Contains ("NpgsqlCommand.ClearPoolAndCreateException")) {
          return true;
        }
      }
      if (null != ex.InnerException) {
        return IsNullReferenceExceptionBugInClearPoolAndCreateException (ex.InnerException);
      }
      return false;
    }

    /// <summary>
    /// Check if a request failed because of insufficient resources:
    /// one of the error code that starts with 53:
    /// <item>53000 	insufficient_resources</item>
    /// <item>53100 	disk_full</item>
    /// <item>53200 	out_of_memory</item>
    /// <item>53300 	too_many_connections</item>
    /// <item>53400   configuration_limit_exceeded</item>
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public bool IsDatabaseInsufficientResources (Exception ex, ILog logger)
    {
      if (ex is Npgsql.PostgresException) {
        Npgsql.PostgresException postgresException = ex as Npgsql.PostgresException;
        if (postgresException.SqlState.StartsWith ("53", StringComparison.InvariantCultureIgnoreCase)) {
          logger?.FatalFormat ("IsDatabaseInsufficientResources: " +
                               "error code {0} in PostgresException {1}",
                               postgresException.SqlState, postgresException);
          return true;
        }
      }

      if (null != ex.InnerException) {
        if ((logger != null) && logger.IsDebugEnabled) {
          logger.Debug ("IsDatabaseInsufficientResources: inspect inner exception", ex.InnerException);
        }
        return IsDatabaseInsufficientResources (ex.InnerException, logger);
      }

      return false;
    }

    /// <summary>
    /// Check if a request failed because of an OutOfMemory exception
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public bool IsOutOfMemory (Exception ex)
    {
      return IsOutOfMemory (ex, log);
    }

    /// <summary>
    /// Check if a request failed because of an OutOfMemory exception
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public bool IsOutOfMemory (Exception ex, ILog logger)
    {
      if (ex is Npgsql.PostgresException) {
        Npgsql.PostgresException postgresException = ex as Npgsql.PostgresException;
        if (postgresException.SqlState.Equals ("53200")) { // out_of_memory
          logger?.Error ("IsOutOfMemory: out_of_memory in PostgresException", postgresException);

          return true;
        }
        else {
          logger?.Info ("IsOutOfMemory: not a PostgreSQL out_of_memory exception, PostgresException", postgresException);
        }
      }

      return false;
    }

    /// <summary>
    /// Check recursively if an exception is of a given exception type
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="exceptionType"></param>
    /// <returns></returns>
    public static bool IsOfExceptionType (Exception ex, Type exceptionType)
    {
      return IsOfExceptionType (ex, exceptionType, slog);
    }

    /// <summary>
    /// Check recursively if an exception is of a given exception type
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="exceptionType"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public static bool IsOfExceptionType (Exception ex, Type exceptionType, ILog logger)
    {
      if (exceptionType.IsInstanceOfType (ex)) {
        if ((logger != null) && logger.IsErrorEnabled) {
          logger.ErrorFormat ("IsOfExceptionType: " +
                              "{0} exception detected",
                              exceptionType);
        }
        return true;
      }

      if (null != ex.InnerException) {
        if ((logger != null) && logger.IsDebugEnabled) {
          logger.Debug ("IsOfExceptionType: inspect inner exception", ex.InnerException);
        }
        return IsOfExceptionType (ex.InnerException, exceptionType, logger);
      }

      return false;
    }
  }
}
