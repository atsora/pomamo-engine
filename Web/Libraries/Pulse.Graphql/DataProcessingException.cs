// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Threading;

namespace Pulse.Graphql
{
  /// <summary>
  /// DataProcessingException
  /// </summary>
  public class DataProcessingException : Exception
  {
    readonly ILog log = LogManager.GetLogger (typeof (DataProcessingException).FullName);

    /// <summary>
    /// Associated error status
    /// </summary>
    public ErrorStatus ErrorStatus { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    public DataProcessingException (string message, ErrorStatus errorStatus)
      : base (message)
    {
      this.ErrorStatus = errorStatus;
    }

    /// <summary>
    /// Alternative constructor
    /// </summary>
    public DataProcessingException (string message, ErrorStatus errorStatus, Exception innerException)
      : base (message, innerException)
    {
      this.ErrorStatus = errorStatus;
    }

  }
}
