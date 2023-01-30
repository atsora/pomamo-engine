// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Core.ExceptionManagement
{
  /// <summary>
  /// Singleton class that can be extended and allows to characterize exceptions
  /// 
  /// The static methods here are recursive
  /// </summary>
  public sealed class ExceptionTest
  {
    #region Members
    readonly IList<IExceptionTest> m_tests = new List<IExceptionTest> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (ExceptionTest).FullName);

    #region Getters / Setters
    /// <summary>
    /// List of tests
    /// </summary>
    static IEnumerable<IExceptionTest> Tests {
      get
      {
        lock (Instance.m_tests)
        {
          return Instance.m_tests;
        }
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Private constructor (singleton class !)
    /// </summary>
    private ExceptionTest()
    {
      m_tests.Add (new DefaultExceptionTest ());
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Add a test
    /// </summary>
    /// <param name="test"></param>
    public static void AddTest (IExceptionTest test)
    {
      Instance.m_tests.Add (test);
    }
    
    /// <summary>
    /// Does the exception require to exit the application
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public static bool RequiresExit (Exception ex)
    {
      return RequiresExit (ex, log);
    }
    
    /// <summary>
    /// Does the exception require to exit the application
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger">not null</param>
    /// <returns></returns>
    public static bool RequiresExit (Exception ex, ILog logger)
    {
      Debug.Assert (null != logger);

      if (Tests.Any (test => test.RequiresExit (ex, logger))) {
        return true;
      }
      
      if (null != ex.InnerException) {
        if (logger.IsDebugEnabled) {
          logger.Debug ("RequiresExit: inspect inner exception", ex.InnerException);
        }
        return RequiresExit (ex.InnerException, logger);
      }
      
      return false;
    }

    /// <summary>
    /// Does the exception require to exit the application,
    /// except if the request is from the database
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public static bool RequiresExitExceptFromDatabase (Exception ex)
    {
      return RequiresExitExceptFromDatabase (ex, log);
    }

    /// <summary>
    /// Does the exception require to exit the application,
    /// except if the request is from the database
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger">not null</param>
    /// <returns></returns>
    public static bool RequiresExitExceptFromDatabase (Exception ex, ILog logger)
    {
      Debug.Assert (null != logger);

      if (Tests.Any (test => test.RequiresExitExceptFromDatabase (ex, logger))) {
        return true;
      }

      if (null != ex.InnerException) {
        if (logger.IsDebugEnabled) {
          logger.Debug ("RequiresExitExceptFromDatabase: inspect inner exception", ex.InnerException);
        }
        return RequiresExitExceptFromDatabase (ex.InnerException, logger);
      }

      return false;
    }

    /// <summary>
    /// Does the exception correspond to a stale exception,
    /// which requires to reload the data before continuing ?
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public static bool IsStale (Exception ex)
    {
      return IsStale (ex, log);
    }
    
    /// <summary>
    /// Does the exception correspond to a stale exception,
    /// which requires to reload the data before continuing ?
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger">not null</param>
    /// <returns></returns>
    public static bool IsStale (Exception ex, ILog logger)
    {
      Debug.Assert (null != logger);

      if (Tests.Any (test => test.IsStale (ex, logger))) {
        return true;
      }
      
      if (null != ex.InnerException) {
        if (logger.IsDebugEnabled) {
          logger.Debug ("IsStale: inspect inner exception", ex.InnerException);
        }
        return IsStale (ex.InnerException, logger);
      }
      
      return false;
    }

    /// <summary>
    /// Does this exception relate to a right problem? Admin required?
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public static bool IsUnauthorized (Exception ex)
    {
      return IsUnauthorized (ex, log);
    }

    /// <summary>
    /// Does this exception relate to a right problem? Admin required?
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger">not null</param>
    /// <returns></returns>
    public static bool IsUnauthorized (Exception ex, ILog logger)
    {
      Debug.Assert (null != logger);

      if (Tests.Any (test => test.IsUnauthorized (ex, logger))) {
        return true;
      }
      
      if (null != ex.InnerException) {
        if (logger.IsDebugEnabled) {
          logger.Debug ($"IsUnauthorized: inspect inner exception", ex.InnerException);
        }
        return IsUnauthorized (ex.InnerException, logger);
      }
      
      return false;
    }
    
    /// <summary>
    /// Is the exception temporary (and can the request be retried)
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public static bool IsTemporary (Exception ex)
    {
      return IsTemporary (ex, log);
    }
    
    /// <summary>
    /// Is the exception temporary (and can the request be retried)
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger">not null</param>
    /// <returns></returns>
    public static bool IsTemporary (Exception ex, ILog logger)
    {
      Debug.Assert (null != logger);

      if (Tests.Any (test => test.IsTemporary (ex, logger))) {
        return true;
      }
      
      if (null != ex.InnerException) {
        if (logger.IsDebugEnabled) {
          logger.Debug ($"IsTemporary: inspect inner exception", ex.InnerException);
        }
        return IsTemporary (ex.InnerException, logger);
      }

      if (IsTemporaryWithDelay (ex, logger)) {
        if (logger.IsDebugEnabled) {
          logger.Debug ($"IsTemporary: temporary with delay");
        }
        return true;
      }

      return false;
    }
    
    /// <summary>
    /// Check if a request failed because of a serialization failure (and then the request must be retried)
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public static bool IsTransactionSerializationFailure (Exception ex)
    {
      return IsTransactionSerializationFailure(ex, log);
    }
    
    /// <summary>
    /// Check if a request failed because of a serialization failure (and then the request must be retried)
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public static bool IsTransactionSerializationFailure (Exception ex, ILog logger)
    {
      Debug.Assert (null != logger);

      if (Tests.Any (test => test.IsTransactionSerializationFailure (ex, logger))) {
        return true;
      }

      if (null != ex.InnerException) {
        if (logger.IsDebugEnabled) {
          logger.Debug ("IsTransactionSerializationFailure: inspect inner exception", ex.InnerException);
        }
        return IsTransactionSerializationFailure (ex.InnerException, logger);
      }
      
      return false;
    }

    /// <summary>
    /// Check if a request failed because a transaction was aborted (and then the request must be retried)
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public static bool IsTransactionAborted (Exception ex)
    {
      return IsTransactionAborted (ex, log);
    }

    /// <summary>
    /// Check if a request failed because a transaction was aborted (and then the request must be retried)
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public static bool IsTransactionAborted (Exception ex, ILog logger)
    {
      Debug.Assert (null != logger);

      if (Tests.Any (test => test.IsTransactionAborted (ex, logger))) {
        return true;
      }

      if (null != ex.InnerException) {
        if (logger.IsDebugEnabled) {
          logger.Debug ("IsTransactionAborted: inspect inner exception", ex.InnerException);
        }
        return IsTransactionAborted (ex.InnerException, logger);
      }

      return false;
    }

    /// <summary>
    /// Check if a request failed because of a timeout
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public static bool IsTimeoutFailure (Exception ex)
    {
      return IsTimeoutFailure(ex, log);
    }
    
    /// <summary>
    /// Check if a request failed because of a timeout
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public static bool IsTimeoutFailure (Exception ex, ILog logger)
    {
      Debug.Assert (null != logger);

      if (Tests.Any (test => test.IsTimeoutFailure (ex, logger))) {
        return true;
      }
      
      if (null != ex.InnerException) {
        if (logger.IsDebugEnabled) {
          logger.Debug ("IsTimeoutFailure: inspect inner exception", ex.InnerException);
        }
        return IsTimeoutFailure (ex.InnerException, logger);
      }
      
      return false;
    }

    /// <summary>
    /// Is the exception temporary but that usually requires some time before the system is back to normal.
    /// For example, the database is being restarted or there is a connection error
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public static bool IsTemporaryWithDelay (Exception ex)
    {
      return IsTemporaryWithDelay (ex, log);
    }

    /// <summary>
    /// Is the exception temporary but that usually requires some time before the system is back to normal.
    /// For example, the database is being restarted or there is a connection error
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger">not null</param>
    /// <returns></returns>
    public static bool IsTemporaryWithDelay (Exception ex, ILog logger)
    {
      Debug.Assert (null != logger);

      if (Tests.Any (test => test.IsTemporaryWithDelay (ex, logger))) {
        return true;
      }
      
      if (null != ex.InnerException) {
        if (logger.IsDebugEnabled) {
          logger.Debug ("IsTemporaryWithDelay: inspect inner exception", ex.InnerException);
        }
        return IsTemporaryWithDelay (ex.InnerException, logger);
      }
      
      return false;
    }

    /// <summary>
    /// Does the exception correspond to an invalid query that must be skipped and flagged in error ?
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public static bool IsInvalid (Exception ex)
    {
      return IsInvalid (ex, log);
    }
    
    /// <summary>
    /// Does the exception correspond to an invalid query that must be skipped and flagged in error ?
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger">not null</param>
    /// <returns></returns>
    public static bool IsInvalid (Exception ex, ILog logger)
    {
      Debug.Assert (null != logger);

      if (Tests.Any (test => test.IsInvalid (ex, logger))) {
        return true;
      }
      
      if (null != ex.InnerException) {
        if (logger.IsDebugEnabled) {
          logger.Debug ("IsInvalid: inspect inner exception", ex.InnerException);
        }
        return IsInvalid (ex.InnerException, logger);
      }
      
      return false;
    }
    
    /// <summary>
    /// Check if a request failed because of an integrity constraint violation
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public static bool IsIntegrityConstraintViolation (Exception ex)
    {
      return IsTimeoutFailure(ex, log);
    }
    
    /// <summary>
    /// Check if a request failed because of an integrity constraint violation
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    public static bool IsIntegrityConstraintViolation (Exception ex, ILog logger)
    {
      Debug.Assert (null != logger);

      if (Tests.Any (test => test.IsIntegrityConstraintViolation (ex, logger))) {
        return true;
      }
      
      if (null != ex.InnerException) {
        if (logger.IsDebugEnabled) {
          logger.Debug ("IsIntegrityConstraintViolation: inspect inner exception", ex.InnerException);
        }
        return IsIntegrityConstraintViolation (ex.InnerException, logger);
      }
      
      return false;
    }

    /// <summary>
    /// Is the exception a database exception ?
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="details">Database exception details if found</param>
    /// <returns></returns>
    public static bool IsDatabaseException (Exception ex, out IDatabaseExceptionDetails details)
    {
      return IsDatabaseException (ex, log, out details);
    }
    
    /// <summary>
    /// Is the exception a database exception ?
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger">not null</param>
    /// <param name="details">Database exception details if found</param>
    /// <returns></returns>
    public static bool IsDatabaseException (Exception ex, ILog logger, out IDatabaseExceptionDetails details)
    {
      Debug.Assert (null != logger);

      foreach (var test in Tests) {
        if (test.IsDatabaseException (ex, logger, out details)) {
          return true;
        }
      }
      
      if (null != ex.InnerException) {
        if (logger.IsDebugEnabled) {
          logger.Debug ("IsDatabaseException: inspect inner exception", ex.InnerException);
        }
        return IsDatabaseException (ex.InnerException, logger, out details);
      }
      
      details = null;
      return false;
    }

    /// <summary>
    /// Does the exception correspond to a database connection error ?
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public static bool IsDatabaseConnectionError (Exception ex)
    {
      return IsDatabaseConnectionError (ex, log);
    }

    /// <summary>
    /// Does the exception correspond to a database connection error ?
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger">not null</param>
    /// <returns></returns>
    public static bool IsDatabaseConnectionError (Exception ex, ILog logger)
    {
      Debug.Assert (null != logger);

      foreach (var test in Tests) {
        if (test.IsDatabaseConnectionError (ex, logger)) {
          return true;
        }
      }

      if (null != ex.InnerException) {
        if (logger.IsDebugEnabled) {
          logger.Debug ("IsDatabaseConnectionError: inspect inner exception", ex.InnerException);
        }
        return IsDatabaseConnectionError (ex.InnerException, logger);
      }

      return false;
    }

    /// <summary>
    /// Does this exception corresponds to a real error ?
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public static bool IsNotError (Exception ex)
    {
      return IsNotError (ex, log);
    }

    /// <summary>
    /// Does this exception corresponds to a real error ?
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="logger">not null</param>
    /// <returns></returns>
    public static bool IsNotError (Exception ex, ILog logger)
    {
      Debug.Assert (null != logger);

      if (Tests.Any (test => test.IsNotError (ex, logger))) {
        return true;
      }

      if (null != ex.InnerException) {
        if (logger.IsDebugEnabled) {
          logger.Debug ("IsNotError: inspect inner exception", ex.InnerException);
        }
        return IsNotError (ex.InnerException, logger);
      }

      return false;
    }
    #endregion // Methods

    #region Instance
    static ExceptionTest Instance
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

      internal static readonly ExceptionTest instance = new ExceptionTest ();
    }
    #endregion // Instance
  }
}
