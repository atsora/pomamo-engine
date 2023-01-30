// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lemoine.Extensions.Web.Attributes
{
  /// <summary>
  /// Route attribute in a web service definition
  /// </summary>
  //TODO define attribute scope
  [AttributeUsage (AttributeTargets.Class, AllowMultiple = true)]
  public class RouteAttribute : Attribute
  {
    /// <summary>
    /// Defines a Path for this contract.
    /// Multiple Routes can be added to the same contract but must be of the same Verb
    /// </summary>
    /// <param name="endpoint"></param>
    /// <param name="method"></param>
    public RouteAttribute (string endpoint, string method)
    {
#if DEBUG
      if (string.IsNullOrWhiteSpace (endpoint)) {
        throw new ArgumentException ("message", nameof (endpoint));
      }

      if (string.IsNullOrWhiteSpace (endpoint)) {
        throw new ArgumentException ("message", nameof (method));
      }

      if (endpoint[0] != '/') {
        throw new ArgumentException ("Endpoint needs to begin with /");
      }
      //todo Method is a known verb
#endif
      Endpoint = endpoint;
      Method = method;
    }

    /// <summary>
    /// End point / path of the request
    /// </summary>
    public string Endpoint { get; set; }

    /// <summary>
    /// Request method:
    /// <item>GET</item>
    /// <item>POST</item>
    /// </summary>
    public string Method { get; set; }

    /// <summary>
    /// Summary of the web service
    /// </summary>
    public string Summary { get; set; }

    /// <summary>
    /// Additional notes how to request it
    /// </summary>
    public string Notes { get; set; }
  }
}
