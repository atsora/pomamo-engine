// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Collections;

namespace Lemoine.Model
{
  /// <summary>
  /// Refresh token
  /// </summary>
  public interface IRefreshToken
    : IDataWithId
    , IVersionable
    , IPartitionedByUser
  {
    /// <summary>
    /// Token
    /// </summary>
    string Token { get; set; }

    /// <summary>
    /// Expiration date/time
    /// </summary>
    DateTime Expiration { get; set; }

    /// <summary>
    /// Revocation date/time
    /// </summary>
    DateTime? Revoked { get; set; }

    /// <summary>
    /// OAuth2 plugin name in case it was used to generate the refresh token
    /// </summary>
    string OAuth2Name { get; set; }

    /// <summary>
    /// OAuth2 authentication name in case it was used to generate the refresh token
    /// </summary>
    string OAuth2AuthenticationName { get; set; }

    /// <summary>
    /// OAuth2 refresh token
    /// </summary>
    string OAuth2RefreshToken { get; set; }
  }

  public static class RefreshTokenExtensions
  {
    /// <summary>
    /// Is the token valid ?
    /// </summary>
    /// <param name="refreshToken"></param>
    /// <returns></returns>
    public static bool IsValid (this IRefreshToken refreshToken)
    {
      return !refreshToken.Revoked.HasValue
        && !refreshToken.IsExpired ();
    }

    /// <summary>
    /// Is the token expired ?
    /// </summary>
    /// <param name="refreshToken"></param>
    /// <returns></returns>
    public static bool IsExpired (this IRefreshToken refreshToken)
    {
      return refreshToken.Expiration < DateTime.UtcNow;
    }
  }
}
