// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Extensions.Web.Responses
{
  /// <summary>
  /// Generic Ok DTO, to be used only with a message
  /// </summary>
  public class OkDTO
  {
    /// <summary>
    /// Message of OkDTO
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="message"></param>
    public OkDTO (string message)
    {
      this.Message = message;
    }
  }
}
