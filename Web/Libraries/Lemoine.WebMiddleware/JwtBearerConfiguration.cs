// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

using Lemoine.Conversion;
using Lemoine.Core.Log;
using System.Threading.Tasks;
using System.Security.Claims;

namespace Lemoine.WebMiddleware
{
  /// <summary>
  /// JwtBearerConfiguration
  /// </summary>
  public static class JwtBearerConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (JwtBearerConfiguration).FullName);

    static readonly string JWT_SECRET_KEY = "Jwt.Secret";

    public static AuthenticationBuilder AddJwtBearerConfiguration (this AuthenticationBuilder builder, string scheme = JwtBearerDefaults.AuthenticationScheme, string issuer = "", string audience = "")
    {
      var keyString = Lemoine.Info.ConfigSet.LoadAndGet (JWT_SECRET_KEY, "");
      if (string.IsNullOrEmpty (keyString)) { // Generate one
        keyString = System.Guid.NewGuid ().ToString ().Replace ("-", "");
        Lemoine.Info.ConfigSet.SetPersistentConfig (JWT_SECRET_KEY, keyString);
      }
      var key = System.Text.Encoding.ASCII.GetBytes (keyString);
      return builder.AddJwtBearer (scheme, options => {
        //        options.RequireHttpsMetadata = false;
        options.SaveToken = true; // TODO: probably not required
        if (!string.IsNullOrEmpty (issuer)) {
          options.Authority = issuer;
        }
        options.Audience = audience;
        options.IncludeErrorDetails = true; // TODO: really ?
        options.TokenValidationParameters = new TokenValidationParameters () {
          ValidateIssuer = !string.IsNullOrEmpty (issuer),
          ValidateAudience = true,
          ValidateLifetime = true,
          ValidateIssuerSigningKey = true,
          IssuerSigningKey = new SymmetricSecurityKey (key)
        };
        options.Events = new JwtBearerEvents () {
          OnAuthenticationFailed = context => {
            if (context.Exception is SecurityTokenExpiredException) {
              log.Error ($"Authentication failed: access token expired", context.Exception);
            }
            else {
              log.Error ("Authentication failed", context.Exception);
            }
            return Task.CompletedTask;
          },
          OnTokenValidated = context => {
            var accessToken = context.SecurityToken as System.IdentityModel.Tokens.Jwt.JwtSecurityToken;
            if (accessToken is not null) {
              var identity = context.Principal?.Identity as ClaimsIdentity;
              if (identity is not null) {
                identity.AddClaim (new Claim ("valid_to", accessToken.ValidTo.ToIsoString ()));
              }
            }
            log.Debug ($"Authentication successful");
            return Task.CompletedTask;
          }
        };
      });
    }

  }
}
