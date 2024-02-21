// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.ExceptionManagement;
using Lemoine.Core.Log;

namespace Lemoine.Cnc.SQLiteQueue
{
  /// <summary>
  /// Utility class to parse the exceptions, from SQLite driver
  /// 
  /// The method here are not recursive
  /// </summary>
  public class SQLiteExceptionTest
    : Lemoine.Core.ExceptionManagement.IExceptionTest
  {
    readonly ILog log = LogManager.GetLogger (typeof (SQLiteExceptionTest).FullName);
    static readonly ILog slog = LogManager.GetLogger (typeof (SQLiteExceptionTest).FullName);

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
      if (ex is System.Data.SQLite.SQLiteException) {
        System.Data.SQLite.SQLiteException sqliteException = ex as System.Data.SQLite.SQLiteException;
        if (sqliteException.ErrorCode.Equals (SQLiteErrorCode.ReadOnly)) {
          logger?.Fatal ("IsUnauthorized: exception with error code ReadOnly in SQLite", sqliteException);
          return true;
        }
        else {
          logger?.Info ("IsUnauthorized: not a SQLite ReadOnly exception", sqliteException);
        }
      }

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
      return false;
    }

    /// <summary>
    /// Check if a request failed because a transaction aborted
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public bool IsTransactionAborted (Exception ex)
    {
      return IsTransactionAborted (ex, log);
    }

    /// <summary>
    /// Check if a request failed because a transaction aborted
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public bool IsTransactionAborted (Exception ex, ILog logger)
    {
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
    /// <param name="ex"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public bool IsDatabaseConnectionError (Exception ex, ILog logger)
    {
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
      if (ex is System.Data.SQLite.SQLiteException) {
        System.Data.SQLite.SQLiteException sqliteException = ex as System.Data.SQLite.SQLiteException;
        if (sqliteException.ResultCode.Equals (SQLiteErrorCode.NoMem)) { 
          logger?.Fatal ("IsOutOfMemory: exception with code NoMem in SQLite", sqliteException);
          return true;
        }
        else {
          logger?.Info ("IsOutOfMemory: not a SQLite NoMem exception", sqliteException);
        }
      }

      if (ex?.Message?.Contains ("out of memory") ?? false) {
        logger?.Fatal ("IsOutOfMemory: out of memory  message in SQLite exception ", ex);
        return true;
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
