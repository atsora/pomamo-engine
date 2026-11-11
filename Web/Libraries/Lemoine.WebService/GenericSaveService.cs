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

namespace Lemoine.WebService
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

    /// <summary>
    /// Remote Ip
    /// </summary>
    public string RemoteIp { get; set; }

#region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    protected GenericSaveService ()
    {
    }
#endregion // Constructors

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
        log.Info ($"GetRequestAnswer: request={request} attempt={attempt}");
        try {
          return GetSync (request);
        }
        catch (Exception ex) {
          if (ExceptionTest.IsTransactionSerializationFailure (ex, log)) {
            log.Info ($"GetRequestAnswer: serialization failure => try again onceException is {ex}");
            lastException = ex;
            continue;
          }
          else if (ExceptionTest.IsTimeoutFailure (ex, log)) {
            log.Warn ($"GetRequestAnswer: timeout failure => try again Exception is {ex}");
            lastException = ex;
            continue;
          }
          else if (ExceptionTest.IsTemporary (ex, log)) {
            log.Warn ($"GetRequestAnswer: temporary failure => try again Exception is {ex}");
            lastException = ex;
            continue;
          }
          else if (ExceptionTest.IsIntegrityConstraintViolation (ex, log)) {
            log.Fatal ("GetRequestAnswer: IntegrityConstraintViolation => give up for this request");
            throw;
          }
          else if (ExceptionTest.IsInvalid (ex, log)) {
            log.Error ($"GetRequestAnswer: Invalid request exception {ex} with InnerException {ex.InnerException}give up just for this request");
            throw;
          }
          else if (ExceptionTest.RequiresExit (ex, log)) {
            log.Error ($"GetRequestAnswer: exception {ex} requires to exit, give up");
            Exit (ex);
            throw;
          }
          else if (ex is ObjectDisposedException) {
            log.Error ($"GetRequestAnswer: ObjectDisposedException {ex} stack trace: {ex.StackTrace}");
            lastException = ex;
            continue;
          }
          else if (ex is InvalidOperationException) {
            log.Error ($"GetRequestAnswer: InvalidOperationException {ex} stack trace: {ex.StackTrace}");
            lastException = ex;
            continue;
          }
          else {
            log.Fatal ($"GetRequestAnswer: unexpected error {ex} stack trace: {ex.StackTrace}");
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
        log.Fatal ("GetRequestAnswer: unexpected lastException value (should not be null)");
        throw new InvalidOperationException ("Unexpected lastException value");
      }
      else if ((lastException is ObjectDisposedException) || (lastException is InvalidOperationException)) {
        return new ErrorDTO (lastException.StackTrace, ErrorStatus.TransientProcessError);
      }
      else {
        Debug.Assert (!(lastException is OutOfMemoryException));
        log.Error ($"GetRequestAnswer: second time the exception {lastException} happens");
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
              log.Fatal ($"Exception {ex} requires to exit");
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
        log.Error ("GetRequestRemoteIp: null http request => return an empty IP address");
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
  }
}
