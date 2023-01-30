// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lemoine.Extensions.Web.Responses
{
  /// <summary>
  /// DTO for errors (in case of ill-built request or db "problem" occurs when building response)
  /// </summary>
  public class ErrorDTO
  {
    /// <summary>
    /// Error Message of ErrorDTO
    /// 
    /// Short message
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// Give status of the error
    /// </summary>
    public ErrorStatus Status { get; set; }

    /// <summary>
    /// Details
    /// </summary>
    public string Details { get; set; }

    /// <summary>
    /// Default constructor (for XML serialization)
    /// </summary>
    public ErrorDTO ()
    { }

    /// <summary>
    /// Constructor of ErrorDTO
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="status"></param>
    /// <param name="details"></param>
    public ErrorDTO (string msg, ErrorStatus status, string details = "")
    {
      ErrorMessage = msg;
      Status = status;
      Details = details;
    }

  }
}
