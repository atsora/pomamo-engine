// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.WebMiddleware.Cache;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Lemoine.WebMiddleware.Log
{
  /// <summary>
  /// Disposable class to log a request and its duration
  /// </summary>
  internal sealed class LogRequest : IDisposable, IAsyncDisposable
  {
    static readonly string LOG_CATEGORY_PREFIX = "Lemoine.Web.Request";

    static readonly string LOG_REQUEST_INFO_THRESHOLD_KEY = "Web.LogRequest.InfoThreshold";
    static readonly TimeSpan LOG_REQUEST_INFO_THRESHOLD_DEFAULT = TimeSpan.FromMilliseconds (500);

    static readonly string LOG_REQUEST_WARN_THRESHOLD_KEY = "Web.LogRequest.WarnThreshold";
    static readonly TimeSpan LOG_REQUEST_WARN_THRESHOLD_DEFAULT = TimeSpan.FromSeconds (1);

    static readonly string LOG_REQUEST_ERROR_THRESHOLD_KEY = "Web.LogRequest.ErrorTreshold";
    static readonly TimeSpan LOG_REQUEST_ERROR_THRESHOLD_DEFAULT = TimeSpan.FromSeconds (3);

    readonly DateTime m_timeStamp = DateTime.UtcNow;
    readonly Microsoft.AspNetCore.Http.HttpContext m_httpContext;
    readonly ILogFactory m_loggerFactory;

    /// <summary>
    /// Used cache action
    /// </summary>
    public CacheAction CacheAction { get; set; } = CacheAction.None;

    /// <summary>
    /// Was the cache hit ?
    /// </summary>
    public bool CacheHit { get; set; } = false;

    /// <summary>
    /// Was an exception thrown ?
    /// </summary>
    public Exception? ThrownException { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="httpContext"></param>
    public LogRequest (Microsoft.AspNetCore.Http.HttpContext httpContext)
      : this (LogManager.LoggerFactory, httpContext)
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="loggerFactory"></param>
    /// <param name="httpContext"></param>
    public LogRequest (ILogFactory loggerFactory, Microsoft.AspNetCore.Http.HttpContext httpContext)
    {
      m_loggerFactory = loggerFactory;
      m_httpContext = httpContext;
    }

    /// <summary>
    /// Fetch IP of request
    /// </summary>
    /// <returns></returns>
    string GetRequestRemoteIp ()
    {
      var ipAddress = m_httpContext.Connection.RemoteIpAddress;
      return ipAddress?.ToString () ?? "";
    }

    string GetWebServiceName ()
    {
      var path = m_httpContext.Request.Path;
      if (path.HasValue) {
        return m_httpContext.Request.Path.Value?.Trim ('/') ?? "";
      }
      else {
        return "";
      }
    }

    #region IDisposable implementation
    /// <summary>
    /// IDisposable implementation
    /// </summary>
    public void Dispose ()
    {
      try {
        TimeSpan duration = DateTime.UtcNow.Subtract (m_timeStamp);

        if (!m_httpContext.Request.Path.HasValue) {
          ILog log = m_loggerFactory.GetLogger (LOG_CATEGORY_PREFIX);
          log.ErrorFormat ("No valid URL. " +
                           "Completed in {0}",
                           duration);
          return;
        }
        else { // Valid URL
          string webServiceName = GetWebServiceName ();
          string logger = LOG_CATEGORY_PREFIX;
          if (null != ThrownException) {
            logger += "." + "Exception";
          }
          else if (this.CacheAction.HasFlag (CacheAction.Clear)) {
            logger += "." + "ClearCacheOnly";
          }
          else if (this.CacheAction.HasFlag (CacheAction.InvalidCache)) {
            logger += "." + "ClearCache";
          }
          else if (this.CacheHit) {
            logger += "." + "Cache";
          }
          else {
            logger += "." + "NoCache";
          }
          logger += "." + webServiceName.Replace ('/', '.');
          ILog log = m_loggerFactory.GetLogger (logger);

          string source = GetRequestRemoteIp ();

          if (null != this.ThrownException) {
            log.Error ($"Exception in Source={source} Duration={duration} Service={webServiceName}", this.ThrownException);
          }

          TimeSpan errorThreshold = Lemoine.Info.ConfigSet
            .LoadAndGet<TimeSpan> (LOG_REQUEST_ERROR_THRESHOLD_KEY,
                                   LOG_REQUEST_ERROR_THRESHOLD_DEFAULT);
          if (errorThreshold <= duration) {
            log.Error ($"Source={source} Duration={duration} Service={webServiceName}");
            return;
          }

          TimeSpan warnThreshold = Lemoine.Info.ConfigSet
            .LoadAndGet<TimeSpan> (LOG_REQUEST_WARN_THRESHOLD_KEY,
                                   LOG_REQUEST_WARN_THRESHOLD_DEFAULT);
          if (warnThreshold <= duration) {
            log.Warn ($"Source={source} Duration={duration} Service={webServiceName}");
            return;
          }

          TimeSpan infoThreshold = Lemoine.Info.ConfigSet
            .LoadAndGet<TimeSpan> (LOG_REQUEST_INFO_THRESHOLD_KEY,
                                   LOG_REQUEST_INFO_THRESHOLD_DEFAULT);
          if (infoThreshold <= duration) {
            if (log.IsInfoEnabled) {
              log.Info ($"Source={source} Duration={duration} Service={webServiceName}");
            }
            return;
          }

          if (log.IsDebugEnabled) {
            log.Debug ($"Source={source} Duration={duration} Service={webServiceName}");
          }
        }
      }
      catch (Exception ex) {
        if (Lemoine.Core.ExceptionManagement.ExceptionTest.RequiresExit (ex)) {
          Exit (ex);
        }
        else {
          try {
            ILog log = m_loggerFactory.GetLogger (LOG_CATEGORY_PREFIX);
            log.Fatal ("Unexpected exception", ex);
          }
          catch (Exception) { }
        }
      }
    }
    #endregion // IDisposable implementation

    #region // IAsyncDisposable
    /// <summary>
    /// <see cref="IAsyncDisposable"/>
    /// </summary>
    /// <returns></returns>
    public async ValueTask DisposeAsync ()
    {
      await Task.Run (Dispose);
    }
    #endregion

    /// <summary>
    /// Request a stop of the service with a specified message and the exception that was the cause of the exit request
    /// </summary>
    /// <param name="ex"></param>
    void Exit (Exception ex)
    {
      ILog? log = null;
      try {
        log = m_loggerFactory.GetLogger (LOG_CATEGORY_PREFIX);
      }
      catch (Exception) {
      }

      Lemoine.Core.Environment.LogAndForceExit (ex, log);
    }
  }
}
