// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Extensions.Web.Responses
{
  /// <summary>
  /// ExceptionDTO
  /// </summary>
  public class ExceptionDTO
    : ErrorDTO
  {
    readonly ILog log = LogManager.GetLogger (typeof (ExceptionDTO).FullName);

    #region Getters / Setters
    /// <summary>
    /// Name of the exception
    /// </summary>
    public string ExceptionName { get; set; }

    /// <summary>
    /// Full name of the exception
    /// </summary>
    public string ExceptionFullName { get; set; }

    /// <summary>
    /// Stack trace if configured
    /// </summary>
    public string StackTrace { get; set; }

    /// <summary>
    /// Inner exception if any
    /// </summary>
    public ExceptionDTO InnerException { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public ExceptionDTO (Exception ex, bool addStackTrace = false, bool includeInnerException = false)
      : base (ex.Message, ErrorStatus.UnexpectedError)
    {
      this.ExceptionName = ex.GetType ().Name;
      this.ExceptionFullName = ex.GetType ().FullName;
      if (addStackTrace) {
        this.StackTrace = ex.StackTrace;
      }
      if ( (null != ex.InnerException) && includeInnerException) {
        this.InnerException = new ExceptionDTO (ex.InnerException, addStackTrace: addStackTrace, includeInnerException: true);
      }
    }
    #endregion // Constructors
  }
}
