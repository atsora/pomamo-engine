// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Extensions;
using Lemoine.Model;

namespace Pulse.Extensions.Web.User
{
  /// <summary>
  /// Answer to ValidateCodeAsync
  /// </summary>
  public struct ValidateCodeResponse
  {
    /// <summary>
    /// User
    /// </summary>
    public IUser User { get; set; }

    /// <summary>
    /// Oauth2 Refresh token
    /// </summary>
    public string RefreshToken { get; set; }

    /// <summary>
    /// OAuth2 refresh token life time
    /// </summary>
    public TimeSpan RefreshTokenExpiresIn { get; set; }
  }

  /// <summary>
  /// Extension to authenticate a user from a code
  /// </summary>
  public interface IValidateAuthenticationCodeExtension
    : IExtension, Lemoine.Extensions.Extension.Categorized.INamed
  {
    /// <summary>
    /// Configurable authentication name
    /// </summary>
    string AuthenticationName { get; }

    /// <summary>
    /// Initialize the extension
    /// </summary>
    /// <returns>success</returns>
    bool Initialize ();

    /// <summary>
    /// Validate the authentication code and return the user to use
    /// </summary>
    /// <returns></returns>
    System.Threading.Tasks.Task<ValidateCodeResponse> ValidateCodeAsync (IServiceProvider serviceProvider, string code, string state = "");

    /// <summary>
    /// Renew a token using a refresh token
    /// </summary>
    /// <returns></returns>
    System.Threading.Tasks.Task<ValidateCodeResponse> RenewTokenAsync (IServiceProvider serviceProvider, string refreshToken);
  }
}
