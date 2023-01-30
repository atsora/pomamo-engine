// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
using Lemoine.Extensions.Web.Responses;

namespace Lemoine.Web.User
{
  /// <summary>
  /// OAuth2 method
  /// </summary>
  public class OAuth2MethodResponseDTO
  { 
    /// <summary>
    /// Name of the OAuth2 implementation
    /// </summary>
    public string AuthenticationKind { get; set; }

    /// <summary>
    /// Authentication name that corresponds to a specific configuration
    /// </summary>
    public string AuthenticationName { get; set; }

    /// <summary>
    /// Is a state required ?
    /// </summary>
    public bool StateRequired { get; set; }

    /// <summary>
    /// Is login required ?
    /// </summary>
    public bool LoginRequired { get; set; }

    /// <summary>
    /// OAuth2 authentication URL where to redirect
    /// 
    /// This authentication URL may contain the {{state}} or {{login}} keywords if they are required
    /// </summary>
    public string AuthenticationUrl { get; set; }
  }

  /// <summary>
  /// Response DTO
  /// </summary>
  [Api ("AuthenticationMethods Response DTO")]
  public class AuthenticationMethodsResponseDTO
  {
    /// <summary>
    /// Is the user/password authentication allowed ?
    /// </summary>
    public bool UserPasswordAuthentication { get; set; }

    /// <summary>
    /// Available OAuth2 methods
    /// </summary>
    public List<OAuth2MethodResponseDTO> OAuth2Methods { get; set; } = new List<OAuth2MethodResponseDTO> ();
  }
}
