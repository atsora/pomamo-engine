// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Extensions.Web.Responses;

namespace Pulse.Extensions.Web.Graphql
{
  /// <summary>
  /// Exception a GraphQL resolver throws to return an <see cref="ErrorStatus"/> to the client
  ///
  /// The web service turns it into a GraphQL error whose extensions carry the status, the
  /// same way the REST services return it in their error response. Any other exception is
  /// returned as an unexpected error, with no status
  /// </summary>
  public class DataProcessingException : Exception
  {
    /// <summary>
    /// Status returned to the client
    /// </summary>
    public ErrorStatus ErrorStatus { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="message">message returned to the client</param>
    /// <param name="errorStatus"></param>
    public DataProcessingException (string message, ErrorStatus errorStatus)
      : base (message)
    {
      this.ErrorStatus = errorStatus;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="message">message returned to the client</param>
    /// <param name="errorStatus"></param>
    /// <param name="innerException"></param>
    public DataProcessingException (string message, ErrorStatus errorStatus, Exception innerException)
      : base (message, innerException)
    {
      this.ErrorStatus = errorStatus;
    }
  }
}
