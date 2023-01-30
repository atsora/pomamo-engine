// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NSERVICEKIT
using System;

using System.Diagnostics;
using System.Linq;
using NServiceKit.ServiceHost;
using Lemoine.Core.Log;

namespace Lemoine.WebService
{
  /// <summary>
  /// Disposable class to log a request and its duration
  /// </summary>
  internal sealed class LogRequest: IDisposable
  {
    static readonly string LOG_CATEGORY_PREFIX = "Lemoine.Web.Request";

    static readonly string LOG_REQUEST_INFO_THRESHOLD_KEY = "Web.LogRequest.InfoThreshold";
    static readonly TimeSpan LOG_REQUEST_INFO_THRESHOLD_DEFAULT = TimeSpan.FromMilliseconds (500);
    
    static readonly string LOG_REQUEST_WARN_THRESHOLD_KEY = "Web.LogRequest.WarnThreshold";
    static readonly TimeSpan LOG_REQUEST_WARN_THRESHOLD_DEFAULT = TimeSpan.FromSeconds (1);
    
    static readonly string LOG_REQUEST_ERROR_THRESHOLD_KEY = "Web.LogRequest.ErrorTreshold";
    static readonly TimeSpan LOG_REQUEST_ERROR_THRESHOLD_DEFAULT = TimeSpan.FromSeconds (3);
    
    readonly DateTime m_timeStamp = DateTime.UtcNow;
    readonly IHttpRequest m_httpRequest;
    
    /// <summary>
    /// Try to use the cache
    /// </summary>
    public bool UseCache { get; set; }
    
    /// <summary>
    /// Do not use the cache explicitely (clear it first)
    /// </summary>
    public bool NoCache { get; set; }
    
    /// <summary>
    /// Clear only the cache
    /// </summary>
    public bool ClearOnly { get; set; }
    
    /// <summary>
    /// Was an exception thrown ?
    /// </summary>
    public Exception ThrownException { get; set; }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="httpRequest"></param>
    public LogRequest (IHttpRequest httpRequest)
    {
      m_httpRequest = httpRequest;
      UseCache = false;
    }

    /// <summary>
    /// Fetch IP of request
    /// </summary>
    /// <returns>"" in case of error</returns>
    string GetRequestRemoteIp()
    {
      Debug.Assert (null != m_httpRequest);
      if (null == m_httpRequest) { // Fallback: return an empty string
        return "";
      }
      
      // string remoteIP = base.RequestContext.IpAddress; // does not work
      if (m_httpRequest != null) {
        System.Net.HttpListenerRequest httpListenerRequest = (System.Net.HttpListenerRequest) m_httpRequest.OriginalRequest;
        if (httpListenerRequest != null) {
          string ipAddress = httpListenerRequest.RemoteEndPoint.ToString();
          // address is of the form ipv4:port
          // or ipv6:port : so remove everything from last ':' onward
          int lastColonIndex = ipAddress.LastIndexOf(':');
          if (lastColonIndex != -1) {
            return ipAddress.Remove(lastColonIndex, ipAddress.Length - lastColonIndex);
          }
        }
      }
      return "";
    }
    
    string GetWebServiceName (string url)
    {
      string withoutParameters = url.Split (new char[] {'?'}, 1) [0];
      string[] split = withoutParameters.Split (new char[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
      if (1 == split.Length) {
        return split [0];
      }      
      else if (1 < split.Length) {
        return string.Join ("/", split);
      }
      else { // 0 == split.Length
        return "";
      }
    }
    
    #region IDisposable implementation
    /// <summary>
    /// IDisposable implementation
    /// </summary>
    public void Dispose()
    {
      try {
        TimeSpan duration = DateTime.UtcNow.Subtract (m_timeStamp);
        
        if ( (null == m_httpRequest)
            || (null == m_httpRequest.RawUrl)) {
          ILog log = LogManager.GetLogger (LOG_CATEGORY_PREFIX);
          log.ErrorFormat ("No valid URL. " +
                           "Completed in {0}",
                           duration);
          return;
        }
        else { // Valid URL
          string url = m_httpRequest.RawUrl;
          string webServiceName = GetWebServiceName (url);
          string logger = LOG_CATEGORY_PREFIX;
          if (null != ThrownException) {
            logger += "." + "Exception";
          }
          else if (ClearOnly) {
            logger += "." + "ClearCacheOnly";
          }
          else if (NoCache) {
            logger += "." + "ClearCache";
          }
          else if (UseCache) {
            logger += "." + "Cache";
          }
          else {
            logger += "." + "NoCache";
          }
          logger += "." + webServiceName.Replace ('/', '.');
          ILog log = LogManager.GetLogger (logger);
          
          string source = GetRequestRemoteIp ();
          
          if (null != this.ThrownException) {
            log.ErrorFormat ("Exception {0} in Source={1} Duration={2} URL={3} StackTrace={4}",
                             this.ThrownException.Message, source, duration, url, this.ThrownException.StackTrace);
          }
          
          TimeSpan errorThreshold = Lemoine.Info.ConfigSet
            .LoadAndGet<TimeSpan> (LOG_REQUEST_ERROR_THRESHOLD_KEY,
                                   LOG_REQUEST_ERROR_THRESHOLD_DEFAULT);
          if (errorThreshold <= duration) {
            log.ErrorFormat ("Source={0} Duration={1} URL={2}",
                             source, duration, url);
            return;
          }
          
          TimeSpan warnThreshold = Lemoine.Info.ConfigSet
            .LoadAndGet<TimeSpan> (LOG_REQUEST_WARN_THRESHOLD_KEY,
                                   LOG_REQUEST_WARN_THRESHOLD_DEFAULT);
          if (warnThreshold <= duration) {
            log.WarnFormat ("Source={0} Duration={1} URL={2}",
                            source, duration, url);
            return;
          }
          
          TimeSpan infoThreshold = Lemoine.Info.ConfigSet
            .LoadAndGet<TimeSpan> (LOG_REQUEST_INFO_THRESHOLD_KEY,
                                   LOG_REQUEST_INFO_THRESHOLD_DEFAULT);
          if (infoThreshold <= duration) {
            log.InfoFormat ("Source={0} Duration={1} URL={2}",
                            source, duration, url);
            return;
          }
          
          log.DebugFormat ("Source={0} Duration={1} URL={2}",
                           source, duration, url);
        }
      }
      catch (Exception ex) {
        ILog log = LogManager.GetLogger (LOG_CATEGORY_PREFIX);
        log.FatalFormat ("Unexpected exception {0}",
                         ex);
      }
    }
    #endregion // IDisposable implementation
  }
}
#endif // NSERVICEKIT