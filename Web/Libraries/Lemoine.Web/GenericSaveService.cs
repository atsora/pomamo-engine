// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Threading.Tasks;
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
  /// Base save service (not cached)
  /// </summary>
  public abstract class GenericSaveService<InputDTO>: IHandler, IRemoteIpSupport
  {
#if NSERVICEKIT
    readonly int MAX_ATTEMPT = 2;

    IHttpRequest m_httpRequest = null;
#endif // NSERVICEKIT

    readonly ILog log = LogManager.GetLogger (typeof (GenericSaveService<InputDTO>).FullName);

    #region Getters / Setters
    /// <summary>
    /// Remote Ip
    /// </summary>
    public string RemoteIp { get; set; }
    #endregion // Getters / Setters

#region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    protected GenericSaveService ()
    {
    }
#endregion // Constructors

#region Methods
    /// <summary>
    /// Get
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public abstract object GetSync (InputDTO request);

    /// <summary>
    /// Get implementation
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public Task<object> Get (InputDTO request)
    {
      var result = GetSync (request);
      return Task.FromResult (result);
    }

#if NSERVICEKIT
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
          return GetSync (request);
        }
        catch (Exception ex) {
          if (ExceptionTest.IsTransactionSerializationFailure (ex, log)) {
            log.InfoFormat ("GetRequestAnswer: " +
                            "serialization failure => try again once" +
                            "Exception is {0}",
                            ex);
            lastException = ex;
            continue;
          }
          else if (ExceptionTest.IsTimeoutFailure (ex, log)) {
            log.WarnFormat ("GetRequestAnswer: " +
                            "timeout failure => try again " +
                            "Exception is {0}",
                            ex);
            lastException = ex;
            continue;
          }
          else if (ExceptionTest.IsTemporary (ex, log)) {
            log.WarnFormat ("GetRequestAnswer: " +
                            "temporary failure => try again " +
                            "Exception is {0}",
                            ex);
            lastException = ex;
            continue;
          }
          else if (ExceptionTest.IsIntegrityConstraintViolation (ex, log)) {
            log.FatalFormat ("GetRequestAnswer: " +
                             "IntegrityConstraintViolation " +
                             "=> give up for this request");
            throw;
          }
          else if (ExceptionTest.IsInvalid (ex, log)) {
            log.ErrorFormat ("GetRequestAnswer: " +
                             "Invalid request exception {0} with InnerException {1}" +
                             "give up just for this request",
                             ex, ex.InnerException);
            throw;
          }
          else if (ExceptionTest.RequiresExit (ex, log)) {
            log.ErrorFormat ("GetRequestAnswer: " +
                             "exception {0} requires to exit, give up",
                             ex);
            Exit (ex);
            throw;
          }
          else if (ex is ObjectDisposedException) {
            log.ErrorFormat ("GetRequestAnswer: " +
                             "ObjectDisposedException {0} " +
                             "stack trace: {1}",
                             ex, ex.StackTrace);
            lastException = ex;
            continue;
          }
          else if (ex is InvalidOperationException) {
            log.ErrorFormat ("GetRequestAnswer: " +
                             "InvalidOperationException {0} " +
                             "stack trace: {1}",
                             ex, ex.StackTrace);
            lastException = ex;
            continue;
          }
          else {
            log.FatalFormat ("GetRequestAnswer: " +
                             "unexpected error {0} " +
                             "stack trace: {1}",
                             ex, ex.StackTrace);
            throw;
          }
        }
        finally {
          log.DebugFormat ("GetRequestAnswer: " +
                           "request={0} is completed",
                           request);
        }
      }

      Debug.Assert (null != lastException);
      if (null == lastException) {
        log.FatalFormat ("GetRequestAnswer: " +
                         "unexpected lastException value (should not be null)");
        throw new InvalidOperationException ("Unexpected lastException value");
      }
      else if ((lastException is ObjectDisposedException) || (lastException is InvalidOperationException)) {
        return new ErrorDTO (lastException.StackTrace, ErrorStatus.TransientProcessError);
      }
      else {
        Debug.Assert (!(lastException is OutOfMemoryException));
        log.ErrorFormat ("GetRequestAnswer: " +
                         "second time the exception {0} happens",
                         lastException);
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
    /// Get
    /// 
    /// This method must not be run inside a transaction or a session
    /// </summary>
    /// <param name="requestContext"></param>
    /// <param name="httpRequest"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (IRequestContext requestContext,
                      IHttpRequest httpRequest,
                      InputDTO request)
    {
      using (var logRequest = new LogRequest (httpRequest)) {
        m_httpRequest = httpRequest;
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

    /// <summary>
    /// Fetch IP of request
    /// </summary>
    /// <returns></returns>
    public string GetRequestRemoteIp ()
    {
#if NSERVICEKIT
      // Debug.Assert (null != m_httpRequest); // No because of the unit tests
      if (null == m_httpRequest) {
        log.ErrorFormat ("GetRequestRemoteIp: " +
                         "null http request " +
                         "=> return an empty IP address");
        return null;
      }

      // string remoteIP = base.RequestContext.IpAddress; // does not work
      if (m_httpRequest != null) {
        System.Net.HttpListenerRequest httpListenerRequest = (System.Net.HttpListenerRequest)m_httpRequest.OriginalRequest;
        if (httpListenerRequest != null) {
          string ipAddress = httpListenerRequest.RemoteEndPoint.ToString ();
          // address is of the form ipv4:port
          // or ipv6:port : so remove everything from last ':' onward
          int lastColonIndex = ipAddress.LastIndexOf (':');
          if (lastColonIndex != -1) {
            return ipAddress.Remove (lastColonIndex, ipAddress.Length - lastColonIndex);
          }
        }
      }
      return null;
#else // !NSERVICEKIT
      return this.RemoteIp;
#endif // NSERVICEKIT
    }

    /// <summary>
    /// <see cref="IRemoteIpSupport"/>
    /// </summary>
    /// <param name="remoteIp"></param>
    /// <returns></returns>
    public void SetRemoteIp (string remoteIp)
    {
      this.RemoteIp = remoteIp;
    }
#endregion // Methods
  }
}
