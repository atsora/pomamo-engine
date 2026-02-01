// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.User;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Pulse.Extensions.Web.User;

namespace Lemoine.Plugin.FordAdfs
{
  public class AccessTokenResponse
  {
    [JsonProperty ("access_token")]
    public string AccessToken { get; set; }

    [JsonProperty ("token_type")]
    public string TokenType { get; set; }

    [JsonProperty ("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonProperty ("resource")]
    public string Resource { get; set; }

    [JsonProperty ("refresh_token")]
    public string RefreshToken { get; set; }

    [JsonProperty ("refresh_token_expires_in")]
    public int RefreshTokenExpiresIn { get; set; }

    [JsonProperty ("id_token")]
    public string IdToken { get; set; }
  }

  public class MeResponse
  {
    [JsonProperty ("name")]
    public string Name { get; set; }

    [JsonProperty ("id")]
    public string Id { get; set; }
  }

  public class ValidateAuthenticationCodeExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IValidateAuthenticationCodeExtension
  {
    const string CLIENT_SECRET_KEY = "Plugin.FordAdfs.Secret";

    readonly ILog log = LogManager.GetLogger (typeof (ValidateAuthenticationCodeExtension).FullName);

    Configuration m_configuration;

    public string Name => "FordAdfs";
      
    public string AuthenticationName => m_configuration.AuthenticationName;

    public bool Initialize ()
    {
      return LoadConfiguration (out m_configuration);
    }

    public async Task<ValidateCodeResponse> ValidateCodeAsync (IServiceProvider serviceProvider, string code, string state = "")
    {
      var httpClient = serviceProvider.GetService<HttpClient> ();
      var clientId = m_configuration.ClientId;
      var clientSecret = m_configuration.ClientSecret;
      if (string.IsNullOrEmpty (clientSecret)) {
        clientSecret = Lemoine.Info.ConfigSet.LoadAndGet<string> (CLIENT_SECRET_KEY, "");
      }
      var contents = new List<KeyValuePair<string, string>> {
        new KeyValuePair<string, string> ("grant_type", "authorization_code"),
        new KeyValuePair<string, string> ("client_id", clientId),
        new KeyValuePair<string, string> ("code", code)
      };
      if (!string.IsNullOrEmpty (m_configuration.AdfsResource)) {
        contents.Add (new KeyValuePair<string, string> ("resource", m_configuration.AdfsResource));
      }
      if (!string.IsNullOrEmpty (m_configuration.AuthenticationRedirect)) {
        contents.Add (new KeyValuePair<string, string> ("redirect_uri", m_configuration.AuthenticationRedirect));
      }
      if (!string.IsNullOrEmpty (clientSecret)) {
        contents.Add (new KeyValuePair<string, string> ("client_secret", clientSecret));
      }
      if (!string.IsNullOrEmpty (state)) {
        contents.Add (new KeyValuePair<string, string> ("state", state));
      }
      var content = new FormUrlEncodedContent (contents);
      var request = new HttpRequestMessage () {
        RequestUri = new Uri ($"https://corp{m_configuration.AdfsEnvironment}.sts.ford.com/adfs/oauth2/token"),
        Method = HttpMethod.Post,
        Content = content,
      };
      request.Headers.Accept.Add (new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue ("application/json"));
      string responseString;
      using (var response = await httpClient.SendAsync (request)) {
        if (response.IsSuccessStatusCode) {
          responseString = await response.Content.ReadAsStringAsync ();
        }
        else {
          log.Error ($"ValidateCodeAsync: invalid status code {response.StatusCode}");
          throw new Exception ("Wrong status code to get the token");
        }
      }
      var accessTokenResponse = JsonConvert.DeserializeObject<AccessTokenResponse> (responseString);
      var token = accessTokenResponse.AccessToken;
      if (string.IsNullOrEmpty (token)) {
        log.Error ($"ValidateCodeAsync: no token retrieved in {responseString}");
        throw new Exception ("No token retrieved");
      }

      var login = ExtractLoginFromToken (token);
      if (string.IsNullOrEmpty (login)) {
        log.Error ($"ValidateCodeAsync: no login could be extracted from {token}");
        throw new Exception ("No login could be extracted from the token");
      }

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginReadOnlyTransaction ("Plugin.FordAdfs.ValidateCode")) {
        var user = await ModelDAOHelper.DAOFactory.UserDAO
          .FindByLoginAsync (login);
        if (user is null) {
          log.Error ($"ValidateCodeAsync: login {login} does not exist in Lemoine database");
        }
        return new ValidateCodeResponse {
          User = user,
          RefreshToken = accessTokenResponse.RefreshToken,
          RefreshTokenExpiresIn = TimeSpan.FromSeconds (accessTokenResponse.RefreshTokenExpiresIn)
        };
      }
    }

    string ExtractLoginFromToken (string tokenString)
    {
      try {
        // In case the WSL standard claims are used
        var tokenHandler = new JwtSecurityTokenHandler ();
        var token = tokenHandler.ReadJwtToken (tokenString);
        var userIdClaim = token.Claims.First (x => x.Type.Equals ("userid"));
        return userIdClaim.Value;
      }
      catch (Exception ex) {
        log.Error ($"ExtractLoginFromToken: exception", ex);
        throw;
      }
    }

    public async Task<ValidateCodeResponse> RenewTokenAsync (IServiceProvider serviceProvider, string refreshToken)
    {
      var httpClient = serviceProvider.GetService<HttpClient> ();
      var clientId = m_configuration.ClientId;
      var clientSecret = m_configuration.ClientSecret;
      if (string.IsNullOrEmpty (clientSecret)) {
        clientSecret = Lemoine.Info.ConfigSet.LoadAndGet<string> (CLIENT_SECRET_KEY, "");
      }
      var contents = new List<KeyValuePair<string, string>> {
        new KeyValuePair<string, string> ("grant_type", "refresh_token"),
        new KeyValuePair<string, string> ("client_id", clientId),
        new KeyValuePair<string, string> ("refresh_token", refreshToken)
      };
      if (!string.IsNullOrEmpty (m_configuration.AdfsResource)) {
        contents.Add (new KeyValuePair<string, string> ("resource", m_configuration.AdfsResource));
      }
      if (!string.IsNullOrEmpty (m_configuration.AuthenticationRedirect)) {
        contents.Add (new KeyValuePair<string, string> ("redirect_uri", m_configuration.AuthenticationRedirect));
      }
      if (!string.IsNullOrEmpty (clientSecret)) {
        contents.Add (new KeyValuePair<string, string> ("client_secret", clientSecret));
      }
      var content = new FormUrlEncodedContent (contents);
      var request = new HttpRequestMessage () {
        RequestUri = new Uri ($"https://corp{m_configuration.AdfsEnvironment}.sts.ford.com/adfs/oauth2/token"),
        Method = HttpMethod.Post,
        Content = content,
      };
      request.Headers.Accept.Add (new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue ("application/json"));
      string responseString;
      using (var response = await httpClient.SendAsync (request)) {
        if (response.IsSuccessStatusCode) {
          responseString = await response.Content.ReadAsStringAsync ();
        }
        else {
          log.Error ($"RenewTokenAsync: invalid status code {response.StatusCode}");
          throw new Exception ("Wrong status code to renew the token");
        }
      }
      var accessTokenResponse = JsonConvert.DeserializeObject<AccessTokenResponse> (responseString);
      var token = accessTokenResponse.AccessToken;
      if (string.IsNullOrEmpty (token)) {
        log.Error ($"RenewTokenAsync: no token retrieved in {responseString}");
        throw new Exception ("No token retrieved");
      }

      var login = ExtractLoginFromToken (token);

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginReadOnlyTransaction ("Plugin.FordAdfs.ValidateCode")) {
        var user = await ModelDAOHelper.DAOFactory.UserDAO
          .FindByLoginAsync (login);
        if (user is null) {
          log.Error ($"RenewTokenAsync: login {login} does not exist in Lemoine database");
        }
        return new ValidateCodeResponse {
          User = user,
          RefreshToken = accessTokenResponse.RefreshToken,
          RefreshTokenExpiresIn = TimeSpan.FromSeconds (accessTokenResponse.RefreshTokenExpiresIn)
        };
      }
    }

  }
}
