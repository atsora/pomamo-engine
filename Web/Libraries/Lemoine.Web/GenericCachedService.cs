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
  /// Base case for cached service which has only a "current" cache timeout.
  /// Cache based on URL of request.
  /// </summary>
  public abstract class GenericCachedService<InputDTO> : ICachedHandler
  {
#if NSERVICEKIT
    readonly int MAX_ATTEMPT = 2;
#endif // NSERVICEKIT
    readonly Regex DISABLE_BROWSER_CACHE_URL_FIX_REGEX = new Regex ("&_=[0-9]+$");
    readonly Regex CACHE_PARAMETER_REGEX = new Regex ("&Cache=(No|Clear)", RegexOptions.IgnoreCase);

    readonly ILog log = LogManager.GetLogger (typeof (GenericCachedService<InputDTO>).FullName);

    #region Members
#if NSERVICEKIT
    NServiceKit.CacheAccess.ICacheClient m_cacheClient = null;
#endif // NSERVICEKIT
    #endregion // Members

    #region Getters / Setters
#if NSERVICEKIT
    /// <summary>
    /// Reference to the cache client
    /// 
    /// Not null if called after Get()
    /// </summary>
    protected NServiceKit.CacheAccess.ICacheClient CacheClient
    {
      get { return m_cacheClient; }
    }
#endif // NSERVICEKIT

    CacheTimeOut DefaultCacheTimeOut { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="defaultCacheTimeOut"></param>
    protected GenericCachedService (Lemoine.Core.Cache.CacheTimeOut defaultCacheTimeOut)
    {
      this.DefaultCacheTimeOut = defaultCacheTimeOut;
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
    /// Get implementation
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public virtual Task<object> Get (InputDTO request)
    {
      var result = GetWithoutCache (request);
      return Task.FromResult (result);
    }

    /// <summary>
    /// Build the cache key from the URL
    /// 
    /// Default is the URL itself
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    protected virtual string GetCacheKey (string url)
    {
      return url;
    }

    /// <summary>
    /// <see cref="ICachedHandler"/>
    /// </summary>
    /// <param name="pathQuery"></param>
    /// <param name="requestDTO"></param>
    /// <param name="outputDTO">not considered here</param>
    /// <returns></returns>
    public virtual TimeSpan GetCacheTimeOut (string pathQuery, object requestDTO, object outputDTO)
    {
      if (requestDTO is InputDTO) {
        return GetCacheTimeOut (pathQuery, (InputDTO)requestDTO, outputDTO);
      }
      else {
        log.Fatal ($"GetCacheTimeOut: {requestDTO} is not of type {typeof (InputDTO)}");
        Debug.Assert (false);
        return CacheTimeOut.NoCache.GetTimeSpan ();
      }
    }

    /// <summary>
    /// Get the cache time out (Asp service only)
    /// 
    /// Override it by the implementation if needed
    /// 
    /// By default, call GetCacheTimeout without outputDTO
    /// </summary>
    /// <param name="pathQuery"></param>
    /// <param name="requestDTO"></param>
    /// <param name="outputDTO">not considered here</param>
    /// <returns></returns>
    protected virtual TimeSpan GetCacheTimeOut (string pathQuery, InputDTO requestDTO, object outputDTO)
    {
      return GetCacheTimeOut (pathQuery, (InputDTO)requestDTO);
    }

    /// <summary>
    /// Get the cache time out
    /// 
    /// Default is taken from CurrentCacheTimeOut
    /// </summary>
    /// <param name="url"></param>
    /// <param name="requestDTO"></param>
    /// <returns></returns>
    protected virtual TimeSpan GetCacheTimeOut (string url, InputDTO requestDTO)
    {
      return this.DefaultCacheTimeOut.GetTimeSpan ();
    }

#if NSERVICEKIT
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
        if (log.IsInfoEnabled) {
          log.Info ($"GetRequestAnswer: request={request} attempt={attempt}");
        }
        try {
          return GetWithoutCache (request);
        }
        catch (Exception ex) {
          if (ExceptionTest.IsTransactionSerializationFailure (ex, log)) {
            if (log.IsInfoEnabled) {
              log.Info ("GetRequestAnswer: serialization failure => try again once", ex);
            }
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
            log.Fatal ("GetRequestAnswer: IntegrityConstraintViolation => give up for this request", ex);
            throw;
          }
          else if (ExceptionTest.IsInvalid (ex, log)) {
            log.Error ($"GetRequestAnswer: Invalid request exception {ex.Message} with InnerException {ex.InnerException?.Message} give up just for this request",
              ex);
            throw;
          }
          else if (ExceptionTest.RequiresExit (ex, log)) {
            log.Error ("GetRequestAnswer: exception requires to exit, give up", ex);
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
          else if (ex is NullReferenceException) {
            log.Fatal ("GetRequestAnswer: NullReferenceException", ex);
            throw;
          }
          else {
            log.Error ("GetRequestAnswer: error", ex);
            throw;
          }
        }
        finally {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetRequestAnswer: request={request} is completed");
          }
        }
      }

      Debug.Assert (null != lastException);
      if (null == lastException) {
        log.Fatal ($"GetRequestAnswer: unexpected lastException value (should not be null)");
        throw new InvalidOperationException ("Unexpected lastException value");
      }
      else if ((lastException is ObjectDisposedException) || (lastException is InvalidOperationException)) {
        return new ErrorDTO (lastException.StackTrace, ErrorStatus.TransientProcessError);
      }
      else {
        Debug.Assert (!(lastException is OutOfMemoryException));
        log.Error ($"GetRequestAnswer: second time the exception {lastException.Message} happens");
        throw new Exception ("Second exception", lastException);
      }
    }

    /// <summary>
    /// Request a stop of the service with a specified message and the exception that was the cause of the exit request
    /// </summary>
    /// <param name="ex"></param>
    void Exit (Exception ex)
    {
      Lemoine.Core.Environment.LogAndForceExit (ex, log);
    }

    /// <summary>
    /// Fix the read URL
    /// 
    /// Remove the &amp;_(random number) at the end of the URL that is used to disable the browser cache
    /// </summary>
    /// <param name="url">Not null</param>
    /// <returns></returns>
    string FixUrl (string url)
    {
      Debug.Assert (null != url);

      string result = DISABLE_BROWSER_CACHE_URL_FIX_REGEX.Replace (url, "");
      result = CACHE_PARAMETER_REGEX.Replace (result, "");
      return result;
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
      m_cacheClient = cacheClient;

      using (var logRequest = new LogRequest (httpRequest)) {
        try {
          if ((null != cacheClient)
              && (null != requestContext)
              && (null != httpRequest)) { // Consider the cache only in this condition
            string rawUrl = httpRequest.RawUrl;
            Debug.Assert (null != rawUrl);
            string url = FixUrl (rawUrl);
            string cacheKey = GetCacheKey (url);

            if (null != cacheKey) {
              if (log.IsDebugEnabled) {
                log.Debug ($"Get: use the cache key={cacheKey} fixed URL={url}");
              }
              string cacheAction = GetCacheAction (rawUrl);
              if (cacheAction.Equals ("no", StringComparison.InvariantCultureIgnoreCase)
                  || cacheAction.Equals ("clear", StringComparison.InvariantCultureIgnoreCase)) {
                logRequest.NoCache = true;
                // Clear the cache
                if (!cacheClient.Remove (cacheKey)) {
                  log.Error ($"Get: Removing key {cacheKey} from cache failed");
                }
                if (cacheAction.Equals ("clear", StringComparison.InvariantCultureIgnoreCase)) {
                  logRequest.ClearOnly = true;
                  // Just return an OK DTO
                  string message = $"Cache key {cacheKey} cleared";
                  return new OkDTO (message);
                }
              }
              logRequest.UseCache = true;
              return requestContext.ToOptimizedResultUsingCache<object> (
                cacheClient, // cache client
                cacheKey, // cache key
                GetCacheTimeOut (url, request), // expiration for the given cache key (optional)
                () => GetRequestAnswer (request, logRequest) // function to execute in case of a cache miss or expiration
               );
            }
          }

          if (log.IsDebugEnabled) {
            log.Debug ("Get: do not use the cache");
          }
          return GetRequestAnswer (request);
        }
        catch (Exception ex) {
          if (Lemoine.Core.ExceptionManagement.ExceptionTest.RequiresExit (ex)) {
            try {
              log.Fatal ("Exception requires to exit", ex);
            }
            catch (Exception) { }

            Exit (ex);
          }
          logRequest.ThrownException = ex;
          throw;
        }
      }
    }

    string GetCacheAction (string url)
    {
      var match = CACHE_PARAMETER_REGEX.Match (url);
      if (match.Success) {
        return match.Groups[1].Value.ToLowerInvariant ();
      }

      return "";
    }
#endif // NSERVICEKIT
    #endregion // Methods
  }
}
