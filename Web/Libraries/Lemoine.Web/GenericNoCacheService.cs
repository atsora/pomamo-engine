// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Lemoine.Core.Cache;
using Lemoine.Core.ExceptionManagement;
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#endif // NSERVICEKIT

using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
using Lemoine.Extensions.Web.Responses;

namespace Lemoine.Web
{
  /// <summary>
  /// Base case for no cache service.
  /// </summary>
  public abstract class GenericNoCacheService<InputDTO> : IHandler
  {
#if NSERVICEKIT
    readonly int MAX_ATTEMPT = 2;
#endif // NSERVICEKIT

    readonly ILog log = LogManager.GetLogger (typeof (GenericNoCacheService<InputDTO>).FullName);

#if NSERVICEKIT
    NServiceKit.CacheAccess.ICacheClient m_nserviceKitCacheClient = null;
#endif // NSERVICEKIT

    #region Getters / Setters
#if NSERVICEKIT
    /// <summary>
    /// Reference to the cache client
    /// 
    /// Not null if called after Get()
    /// </summary>
    public NServiceKit.CacheAccess.ICacheClient NServiceKitCacheClient
    {
      get { return m_nserviceKitCacheClient; }
    }
#endif // NSERVICEKIT
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    protected GenericNoCacheService ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Get without cache
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public virtual object GetWithoutCache (InputDTO request)
    {
      throw new NotImplementedException ();
    }

    /// <summary>
    /// Get sync for NServiceKit
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object GetSync (InputDTO request)
    {
      return GetWithoutCache (request);
    }

    /// <summary>
    /// Get implementation
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public virtual Task<object> Get (InputDTO request)
    {
      var result = GetWithoutCache (request);
      return Task.FromResult (result);
    }

#if NSERVICEKIT
    /// <summary>
    /// Request a stop of the service with a specified message and the exception that was the cause of the exit request
    /// </summary>
    /// <param name="ex"></param>
    void Exit (Exception ex)
    {
      Lemoine.Core.Environment.LogAndForceExit (ex, log);
    }

    /// <summary>
    /// Get without cache, catching a few exceptions
    /// </summary>
    /// <param name="request"></param>
    /// <param name="logRequest"></param>
    /// <returns></returns>
    object GetRequestAnswer (InputDTO request, LogRequest logRequest)
    {
      logRequest.UseCache = false;
      return GetRequestAnswer (request);
    }

    /// <summary>
    /// Get without cache, catching a few exceptions
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    object GetRequestAnswer (InputDTO request)
    {
      Exception lastException = null;

      for (int attempt = 0; attempt < MAX_ATTEMPT; attempt++) {
        log.InfoFormat ("GetRequestAnswer: " +
                        "request={0} attempt={1}",
                        request, attempt);
        try {
          return GetWithoutCache (request);
        }
        catch (Lemoine.FileRepository.MissingFileException ex) {
          if (log.IsInfoEnabled) {
            log.Info ("GetRequestAnswer: MissingFileException", ex);
          }
          throw;
        }
        catch (Lemoine.Web.FileRepo.FileRepoWebException ex) {
          if (ex.InnerException is Lemoine.FileRepository.MissingFileException) {
            if (log.IsInfoEnabled) {
              log.Info ("GetRequestAnswer: MissingFileException", ex);
            }
          }
          else {
            if (log.IsErrorEnabled) {
              log.Error ("GetRequestAnswer: FileRepoWebException", ex);
            }
          }
          throw;
        }
        catch (Exception ex) {
          if (ExceptionTest.IsTransactionSerializationFailure (ex, log)) {
            log.Info ("GetRequestAnswer: serialization failure => try again once", ex);
            lastException = ex;
            continue;
          }
          else if (ExceptionTest.IsTimeoutFailure (ex, log)) {
            log.Warn ("GetRequestAnswer: timeout failure => try again", ex);
            lastException = ex;
            continue;
          }
          else if (ExceptionTest.IsTemporary (ex, log)) {
            log.Warn ("GetRequestAnswer: temporary failure => try again", ex);
            lastException = ex;
            continue;
          }
          else if (ExceptionTest.IsIntegrityConstraintViolation (ex, log)) {
            log.Fatal ("GetRequestAnswer: IntegrityConstraintViolation => give up for this request");
            throw;
          }
          else if (ExceptionTest.IsInvalid (ex, log)) {
            log.Error ($"GetRequestAnswer: Invalid request exception with InnerException {ex.InnerException} give up just for this request", ex);
            throw;
          }
          else if (ExceptionTest.RequiresExit (ex, log)) {
            try {
              log.Error ("GetRequestAnswer: exception requires to exit, give up", ex);
            }
            catch (Exception) { }
            Exit (ex);
          }
          else if (ex is ObjectDisposedException) {
            log.Error ("GetRequestAnswer: ObjectDisposedException", ex);
            lastException = ex;
            continue;
          }
          else if (ex is InvalidOperationException) {
            log.Error ("GetRequestAnswer: InvalidOperationException", ex);
            lastException = ex;
            continue;
          }
          else {
            log.Fatal ("GetRequestAnswer: unexpected error", ex);
            throw;
          }
        }
        finally {
          log.Debug ($"GetRequestAnswer: request={request} is completed");
        }
      }

      Debug.Assert (null != lastException);
      if (null == lastException) {
        log.Fatal ("GetRequestAnswer: unexpected lastException value (should not be null)");
        throw new InvalidOperationException ("Unexpected lastException value");
      }
      else if ((lastException is ObjectDisposedException) || (lastException is InvalidOperationException)) {
        return new ErrorDTO (lastException.StackTrace, ErrorStatus.TransientProcessError);
      }
      else {
        Debug.Assert (!(lastException is OutOfMemoryException));
        log.Error ("GetRequestAnswer: second time the exception happens", lastException);
        throw new Exception ("Second exception", lastException);
      }
    }

    /// <summary>
    /// Get
    /// 
    /// This method must not be run inside a transaction or a session
    /// </summary>
    /// <param name="cacheClient"></param>
    /// <param name="requestContext"></param>
    /// <param name="httpRequest"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (IRequestContext requestContext,
                      IHttpRequest httpRequest,
                      InputDTO request)
    {
      using (var logRequest = new LogRequest (httpRequest)) {
        try {
          return GetRequestAnswer (request);
        }
        catch (Exception ex) {
          if (Lemoine.Core.ExceptionManagement.ExceptionTest.RequiresExit (ex)) {
            try {
              log.FatalFormat ("Exception {0} requires to exit", ex);
            }
            catch (Exception) { }

            Exit (ex);
          }
          logRequest.ThrownException = ex;
          throw;
        }
      }
    }

    /// <summary>
    /// Get (potentially in cache)
    /// 
    /// This method must not be run inside a transaction or a session
    /// </summary>
    /// <param name="cacheClient"></param>
    /// <param name="requestContext"></param>
    /// <param name="httpRequest"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (NServiceKit.CacheAccess.ICacheClient cacheClient,
                      IRequestContext requestContext,
                      IHttpRequest httpRequest,
                      InputDTO request)
    {
      m_nserviceKitCacheClient = cacheClient;

      using (var logRequest = new LogRequest (httpRequest)) {
        try {
          return GetRequestAnswer (request);
        }
        catch (Exception ex) {
          if (Lemoine.Core.ExceptionManagement.ExceptionTest.RequiresExit (ex)) {
            try {
              log.FatalFormat ("Exception {0} requires to exit", ex);
            }
            catch (Exception) { }

            Exit (ex);
          }
          logRequest.ThrownException = ex;
          throw;
        }
      }
    }
#endif // NSERVICEKIT
    #endregion // Methods
  }
}
