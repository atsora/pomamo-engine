// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.Core.ExceptionManagement
{
  /// <summary>
  /// Default exception tests
  /// </summary>
  internal sealed class DefaultExceptionTest : IExceptionTest
  {
    #region IExceptionTest implementation
    /// <summary>
    /// IExceptionTest implementation
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public bool RequiresExit (Exception ex, ILog logger)
    {
      if (ex is OutOfMemoryException) {
        logger?.Error ("RequiresExit: out of memory exception", ex);
        return true;
      }

      if (ex is System.Threading.ThreadStateException) {
        logger?.Error ("RequiresExit: thread state exception", ex);
        return true;
      }

      if (ex is Lemoine.Threading.AbortException) {
        logger?.Error ("RequiresExit: Lemoine.Threading.AbortException", ex);
        return true;
      }

      if (ex?.Message?.ToLowerInvariant ()?.Contains ("out of memory") ?? false) {
        logger?.Error ("RequiresExit: exception with message out of memory", ex);
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
      return RequiresExit (ex, logger);
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
    /// Does this exception relate to a right problem? Admin required?
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public bool IsUnauthorized (Exception ex, ILog logger)
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
    /// <param name="Ex"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public bool IsTransactionSerializationFailure (Exception Ex, ILog logger)
    {
      return false;
    }

    /// <summary>
    /// IExceptionTest implementation
    /// </summary>
    /// <param name="Ex"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public bool IsTransactionAborted (Exception Ex, ILog logger)
    {
      return false;
    }

    /// <summary>
    /// IExceptionTest implementation
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public bool IsTimeoutFailure (Exception ex, ILog logger)
    {
      if (ex is System.Net.Sockets.SocketException) {
        var socketException = ex as System.Net.Sockets.SocketException;
        // WSAETIMEDOUT 10060: Connection timed out.
        var errorCode = socketException.ErrorCode;
        if (errorCode == (int)System.Net.Sockets.SocketError.TimedOut) {
          logger?.Warn ("IsTimeoutFailure: socket timeout", socketException);
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
      return false;
    }

    /// <summary>
    /// IExceptionTest implementation
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public bool IsDatabaseConnectionError (Exception ex, ILog logger)
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
    /// <returns></returns>
    public bool IsIntegrityConstraintViolation (Exception ex, ILog logger)
    {
      return false;
    }

    /// <summary>
    /// Is the exception a database exception ?
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger">not null</param>
    /// <param name="details">Database exception details if found</param>
    /// <returns></returns>
    public bool IsDatabaseException (Exception ex, ILog logger,
                                     out IDatabaseExceptionDetails details)
    {
      details = null;
      return false;
    }

    /// <summary>
    /// IExceptionTest implementation
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public bool IsNotError (Exception ex, ILog logger)
    {
      if (ex is Lemoine.FileRepository.MissingFileException) {
        if ((null != logger) && logger.IsDebugEnabled) {
          logger.Debug ($"IsNotError: Lemoine.FileRepository.MissingFileException");
        }
        return true;
      }

      return false;
    }
    #endregion // IExceptionTest implementation
  }
}
