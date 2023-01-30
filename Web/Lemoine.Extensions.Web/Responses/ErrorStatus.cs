// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Text;

namespace Lemoine.Extensions.Web.Responses
{
  /// <summary>
  /// Define different status about Error
  /// </summary>
  public enum ErrorStatus
  {
    /// <summary>
    /// Wrong request parameter (for example, invalid machine id)
    /// </summary>
    WrongRequestParameter,
    /// <summary>
    /// Missing configuration to make the web service work
    /// </summary>
    MissingConfiguration,
    /// <summary>
    /// Unexpected error
    /// </summary>
    UnexpectedError,
    /// <summary>
    /// The service is not applicable for the input data
    /// </summary>
    NotApplicable,
    /// <summary>
    /// There are some processing delays
    /// </summary>
    ProcessingDelay,
    /// <summary>
    /// Transient process error (may be retried right now)
    /// </summary>
    TransientProcessError,
    /// <summary>
    /// Stale object (during an update): reload the data and update it again
    /// </summary>
    Stale,
    /// <summary>
    /// A maintenance is in progress
    /// </summary>
    PulseMaintenance,
    /// <summary>
    /// Database connection error
    /// 
    /// A message can be displayed at the top of the application
    /// </summary>
    DatabaseConnectionError,
    /// <summary>
    /// Authorization error
    /// </summary>
    AuthorizationError,
  };

}
