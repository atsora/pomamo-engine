// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;

namespace Lemoine.Core.ExceptionManagement
{
  /// <summary>
  /// Description of IExceptionTest.
  /// </summary>
  public interface IExceptionTest
  {
    /// <summary>
    /// Does the exception require to exit the application
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger">not null</param>
    /// <returns></returns>
    bool RequiresExit (Exception ex, ILog logger);

    /// <summary>
    /// Does the exception require to exit the application,
    /// except if the request is from the database
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger">not null</param>
    /// <returns></returns>
    bool RequiresExitExceptFromDatabase (Exception ex, ILog logger);

    /// <summary>
    /// Does the exception correspond to a stale exception,
    /// which requires to reload the data before continuing ?
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    bool IsStale (Exception ex, ILog logger);
    
    /// <summary>
    /// Does this exception relate to a right problem? Admin required?
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    bool IsUnauthorized (Exception ex, ILog logger);
    
    /// <summary>
    /// Is the exception temporary (and can the request be retried)
    /// 
    /// Returns true in case of a transaction serialization failure
    /// or in case of a timeout failure
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger">not null</param>
    /// <returns></returns>
    bool IsTemporary (Exception ex, ILog logger);
    
    /// <summary>
    /// Check if a request failed because of a serialization failure (and then the request must be retried)
    /// </summary>
    /// <param name="Ex"></param>
    /// <param name="logger">not null</param>
    /// <returns></returns>
    bool IsTransactionSerializationFailure (Exception Ex, ILog logger);

    /// <summary>
    /// Check if a request failed because a transaction aborted (and then the request must be retried)
    /// </summary>
    /// <param name="Ex"></param>
    /// <param name="logger">not null</param>
    /// <returns></returns>
    bool IsTransactionAborted (Exception Ex, ILog logger);

    /// <summary>
    /// Check if a request failed because of a timeout
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    bool IsTimeoutFailure (Exception ex, ILog logger);
    
    /// <summary>
    /// Is the exception temporary but that usually requires some time before the system is back to normal.
    /// For example, the database is being restarted or there is a connection error.
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    bool IsTemporaryWithDelay (Exception ex, ILog logger);
    
    /// <summary>
    /// Check if a request failed because of a database connection error
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    bool IsDatabaseConnectionError (Exception ex, ILog logger);
    
    /// <summary>
    /// Does the exception correspond to an invalid query that must be skipped and flagged in error ?
    /// 
    /// Returns true in case of an integrity constraint violation
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger">not null</param>
    /// <returns></returns>
    bool IsInvalid (Exception ex, ILog logger);
    
    /// <summary>
    /// Does the exception correspond to an integrity constraint violation ?
    /// </summary>
    bool IsIntegrityConstraintViolation (Exception ex, ILog logger);
    
    /// <summary>
    /// Does the exception come from a database exception ?
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger">not null</param>
    /// <param name="details">Database exception details if found</param>
    /// <returns></returns>
    bool IsDatabaseException (Exception ex, ILog logger,
                              out IDatabaseExceptionDetails details);

    /// <summary>
    /// Does this exception corresponds to a real error ?
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    bool IsNotError (Exception ex, ILog logger);
  }
}
