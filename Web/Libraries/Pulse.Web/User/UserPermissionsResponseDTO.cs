// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
#endif // NSERVICEKIT
using Lemoine.Model;
using Lemoine.Extensions.Business.Group;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Lemoine.Extensions.Web.Responses;

namespace Pulse.Web.User
{
  /// <summary>
  /// Response DTO
  /// </summary>
  [Api ("/UserPermissions Response DTO")]
  public class UserPermissionsResponseDTO
  {
    static readonly string JWT_SECRET_KEY = "Jwt.Secret";

    static readonly string JWT_EXPIRES_IN_KEY = "Jwt.ExpiresIn";
    static readonly TimeSpan JWT_EXPIRES_IN_DEFAULT = TimeSpan.FromHours (1);


    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="user">not null</param>
    public UserPermissionsResponseDTO (IUser user, IRefreshToken refreshToken)
    {
      Debug.Assert (user != null);

      this.Login = user.Login;
      this.UserName = user.Name;
      this.UserDisplay = user.Display;
      this.CompanyId = (user.Company != null ? user.Company.Id : -1);
      this.Role = (user.Role != null ? user.Role.WebAppKey : "");
      var expiresIn = Lemoine.Info.ConfigSet.LoadAndGet (JWT_EXPIRES_IN_KEY, JWT_EXPIRES_IN_DEFAULT);
      var expiresAt = DateTime.UtcNow.Add (expiresIn);
      this.ExpiresAt = ConvertDTO.DateTimeUtcToIsoString (expiresAt);
      this.AccessToken = GenerateJwtToken (user, expiresAt);
      this.RefreshToken = refreshToken.Token;
      this.RefreshTokenExpiresAt = ConvertDTO.DateTimeUtcToIsoString (refreshToken.Expiration);
    }

    /// <summary>
    /// User login
    /// </summary>
    public string Login { get; set; }

    /// <summary>
    /// Name
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// User display
    /// </summary>
    public string UserDisplay { get; set; }

    /// <summary>
    /// Restriction for a specific company
    /// -1 if there is no restrictions
    /// </summary>
    public int CompanyId { get; set; }

    /// <summary>
    /// Role of the user
    /// empty if none
    /// </summary>
    public string Role { get; set; }

    /// <summary>
    /// JWT access token to use
    /// </summary>
    public string AccessToken { get; set; }

    /// <summary>
    /// Access token expiration date/time
    /// </summary>
    public string ExpiresAt { get; set; }

    /// <summary>
    /// Refresh token
    /// </summary>
    public string RefreshToken { get; set; }

    /// <summary>
    /// Refresh token expiration date/time
    /// </summary>
    public string RefreshTokenExpiresAt { get; set; }

    string GenerateJwtToken (IUser user, DateTime expiresAt)
    {
      var tokenHandler = new JwtSecurityTokenHandler ();
      var keyString = Lemoine.Info.ConfigSet.LoadAndGet (JWT_SECRET_KEY, "");
      if (string.IsNullOrEmpty (keyString)) { // Generate one
        keyString = System.Guid.NewGuid ().ToString ().Replace ("-", "");
        Lemoine.Info.ConfigSet.SetPersistentConfig (JWT_SECRET_KEY, keyString);
      }
      var key = System.Text.Encoding.ASCII.GetBytes (keyString);
      var claims = new List<Claim> { new Claim (ClaimTypes.PrimarySid, user.Id.ToString ()) };
      if (!string.IsNullOrEmpty (user.Login)) {
        claims.Add (new Claim (ClaimTypes.Name, user.Login));
      }
      if (!string.IsNullOrEmpty (user.Name)) {
        claims.Add (new Claim (ClaimTypes.GivenName, user.Name));
      }
      if (!string.IsNullOrEmpty (user.EMail)) {
        claims.Add (new Claim (ClaimTypes.Email, user.EMail));
      }
      if (!string.IsNullOrEmpty (user.Role?.WebAppKey)) {
        claims.Add (new Claim (ClaimTypes.Role, user.Role?.WebAppKey));
      }
      if (null != user.Company) {
        claims.Add (new Claim ("companyid", user.Company.Id.ToString ()));
      }
      var tokenDescriptor = new SecurityTokenDescriptor {
        Issuer = "https://localhost:5001",
        Audience = "urn:pulse",
        Subject = new System.Security.Claims.ClaimsIdentity (claims),
        Expires = expiresAt,
        SigningCredentials = new SigningCredentials (new SymmetricSecurityKey (key), SecurityAlgorithms.HmacSha256Signature)
      };
      var token = tokenHandler.CreateToken (tokenDescriptor);
      return tokenHandler.WriteToken (token);
    }

  }
}
