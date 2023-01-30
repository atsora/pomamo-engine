// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Extensions.Web.User
{
  /// <summary>
  /// Extension to get what URL to use to authenticate with OAuth2
  /// 
  /// TODO: state and login
  /// </summary>
  public interface IOAuth2RedirectionExtension
    : Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Kind of extension
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Configurable authentication name
    /// </summary>
    string AuthenticationName { get; }

    /// <summary>
    /// Restrict this authentication configuration to this application
    /// If empty, any application matches
    /// </summary>
    string ApplicationName { get; }

    /// <summary>
    /// Is a state required ?
    /// </summary>
    bool StateRequired { get; }

    /// <summary>
    /// Is a login required ?
    /// </summary>
    bool LoginRequired { get; }

    /// <summary>
    /// OAuth2 authentication URL redirection
    /// 
    /// The URL may contain {{state}} if the state is required or {{login}} if the login is required
    /// </summary>
    string GetAuthenticationUrl ();

    /// <summary>
    /// Initialize the extension
    /// </summary>
    /// <returns>success</returns>
    bool Initialize ();
  }
}
