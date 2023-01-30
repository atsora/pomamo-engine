// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.ExceptionManagement;
using Lemoine.Core.Log;
using Lemoine.WebMiddleware.Response;
using Lemoine.Extensions.Web.Responses;
using Microsoft.AspNetCore.Http;

namespace Lemoine.WebMiddleware
{
  /// <summary>
  /// MultipleAttemptsMiddleware
  /// </summary>
  public class RequestAttemptMiddleware
  {
    readonly string THROW_UNEXPECTED_EXCEPTION_KEY = "WebMiddleware.RequestAttempt.ThrowUnexpectedException";
    readonly bool THROW_UNEXPECTED_EXCEPTION_VALUE = false;

    readonly int MAX_ATTEMPT = 2;

    readonly ILog log = LogManager.GetLogger<RequestAttemptMiddleware> ();

    readonly RequestDelegate m_next;
    readonly ResponseWriter m_responseWriter;

    /// <summary>
    /// Constructor
    /// </summary>
    public RequestAttemptMiddleware (RequestDelegate next, ResponseWriter responseWriter)
    {
      m_next = next;
      m_responseWriter = responseWriter;
    }

    public async Task InvokeAsync (Microsoft.AspNetCore.Http.HttpContext context)
    {
      Exception? lastException = null;
      bool unexpectedError = false;

      // TODO: use a specific error management instead of returning an error dto ?
      // See: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/error-handling?view=aspnetcore-3.1

      try {
        for (int attempt = 0; attempt < MAX_ATTEMPT; attempt++) {
          if (log.IsInfoEnabled) {
            log.Info ($"InvokeAsync: request={context.Request.Path} attempt={attempt}");
          }
          try {
            await m_next.Invoke (context);
            return;
          }
          catch (Exception ex) {
            if (ExceptionTest.IsTransactionSerializationFailure (ex, log)) {
              if (log.IsInfoEnabled) {
                log.Info ("InvokeAsync: serialization failure => try again once", ex);
              }
              lastException = ex;
              continue;
            }
            else if (ExceptionTest.IsTimeoutFailure (ex, log)) {
              log.Warn ("InvokeAsync: timeout failure => try again", ex);
              lastException = ex;
              continue;
            }
            else if (ExceptionTest.IsTemporaryWithDelay (ex, log)) {
              log.Warn ("InvokeAsync: temporary with delay failure => give up right now", ex);
              lastException = ex;
              break;
            }
            else if (ExceptionTest.IsTemporary (ex, log)) {
              log.Warn ("InvokeAsync: temporary failure => try again", ex);
              lastException = ex;
              continue;
            }
            else if (ExceptionTest.IsIntegrityConstraintViolation (ex, log)) {
              log.Fatal ("InvokeAsync: IntegrityConstraintViolation => give up for this request", ex);
              throw;
            }
            else if (ExceptionTest.IsInvalid (ex, log)) {
              log.Error ($"InvokeAsync: Invalid request exception {ex} with InnerException {ex.InnerException} give up just for this request", ex);
              throw;
            }
            else if (ExceptionTest.RequiresExit (ex, log)) {
              log.Error ("InvokeAsync: exception requires to exit, give up", ex);
              Lemoine.Core.Environment.LogAndForceExit (ex, log);
              throw;
            }
            else if (ex is ObjectDisposedException) {
              log.Error ("InvokeAsync: ObjectDisposedException", ex);
              lastException = ex;
              continue;
            }
            else if (ex is InvalidOperationException) {
              log.Error ("InvokeAsync: InvalidOperationException", ex);
              lastException = ex;
              continue;
            }
            else if (ex is NullReferenceException) {
              log.Fatal ("InvokeAsync: NullReferenceException", ex);
              throw;
            }
            else if (ExceptionTest.IsNotError (ex, log)) {
              log.Info ($"InvokeAsync: exception not really an error", ex);
              throw;
            }
            else {
              log.Error ("InvokeAsync: unexpected error", ex);
              throw;
            }
          }
          finally {
            if (log.IsDebugEnabled) {
              log.Debug ($"InvokeAsync: request={context.Request.Path} is completed");
            }
          }
        }
      }
      catch (Exception ex) {
        var throwUnexpectedException = Lemoine.Info.ConfigSet
          .LoadAndGet (THROW_UNEXPECTED_EXCEPTION_KEY, THROW_UNEXPECTED_EXCEPTION_VALUE);
        log.Info ($"InvokeAsync: exception, throw={throwUnexpectedException}", ex);
        if (throwUnexpectedException) {
          throw;
        }
        else {
          unexpectedError = true;
          lastException = ex;
        }
      }

      Debug.Assert (null != lastException);
      if (null == lastException) {
        log.Fatal ("InvokeAsync: unexpected lastException value (should not be null)");
        throw new InvalidOperationException ("Unexpected lastException value");
      }
      else {
        Debug.Assert (!(lastException is OutOfMemoryException));
        log.Info ($"InvokeAsync: {MAX_ATTEMPT} time the exception happens", lastException);
        var outputDto = GetErrorDto (lastException, unexpectedError: unexpectedError);
        // Note: is there any condition when it is better to return ProcessingDelay instead ?
        await m_responseWriter.WriteToBodyAsync (context, outputDto);
        return;
      }
    }

    /// <summary>
    /// Get the error Dto from an exception
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="unexpectedError">return UnexpectedError, else TransientProcessError</param>
    /// <returns></returns>
    ErrorDTO GetErrorDto (Exception ex, bool unexpectedError = false)
    {
      if (ExceptionTest.IsDatabaseConnectionError (ex, log)) {
        log.Error ($"GetErrorDto: ", ex);
        return new ErrorDTO ("Database connection error", ErrorStatus.DatabaseConnectionError, ex.Message);
      }
      else if (ExceptionTest.IsDatabaseException (ex, log, out IDatabaseExceptionDetails details)) {
        log.Error ($"GetErrorDto: database exception, detail={details.Detail}", ex);
        var errorStatus = ExceptionTest.IsTemporaryWithDelay (ex, log)
          ? ErrorStatus.ProcessingDelay
          : ErrorStatus.TransientProcessError;
        return new ErrorDTO ("Database error, retrying soon", errorStatus, ex.Message);
      }
      else if (ExceptionTest.IsTemporaryWithDelay (ex, log)) {
        log.Error ($"GetErrorDto: temporary with delay", ex);
        return new ErrorDTO ("Transient error, retrying", ErrorStatus.ProcessingDelay, ex.Message);
      }
      else if (ExceptionTest.IsTemporary (ex, log)) {
        log.Error ($"GetErrorDto: temporary", ex);
        return new ErrorDTO ("Transient error, retrying", ErrorStatus.TransientProcessError, ex.Message);
      }
      else if ((ex is ObjectDisposedException) || (ex is InvalidOperationException)) {
        log.Error ($"GetErrorDto: disposed or invalid (restarting ?)", ex);
        return new ErrorDTO ("Transient error, retrying", ErrorStatus.TransientProcessError, ex.Message);
      }
      else if (ExceptionTest.IsNotError (ex, log)) {
        log.Info ($"GetErrorDto: not an error", ex);
        return new ErrorDTO ("Not a real error", ErrorStatus.TransientProcessError, ex.Message);
      }
      else if (unexpectedError) {
        log.Fatal ($"GetErrorDto: unexpected error", ex);
        return new ErrorDTO ("Unexpected error", ErrorStatus.UnexpectedError, ex.Message);
      }
      else {
        log.Fatal ($"GetErrorDto: other exception", ex);
        return new ErrorDTO ("Unexpected problem, retrying", ErrorStatus.TransientProcessError, ex.Message);
      }
    }
  }
}
