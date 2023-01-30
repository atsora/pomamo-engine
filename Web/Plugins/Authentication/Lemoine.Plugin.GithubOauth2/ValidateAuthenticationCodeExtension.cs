// Copyright (C) 2009-2023 Lemoine Automation Technologies
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

namespace Lemoine.Plugin.GithubOauth2
{
  public class AccessTokenResponse
  {
    [JsonProperty ("access_token")]
    public string AccessToken { get; set; }

    [JsonProperty ("scope")]
    public string Scope { get; set; }

    [JsonProperty ("token_type")]
    public string TokenType { get; set; }
  }

  public class UserResponse
  {
    [JsonProperty ("login")]
    public string Login { get; set; }
  }

  public class ValidateAuthenticationCodeExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IValidateAuthenticationCodeExtension
  {
    const string CLIENT_SECRET_KEY = "Plugin.GithubOauth2.Secret";

    readonly ILog log = LogManager.GetLogger (typeof (ValidateAuthenticationCodeExtension).FullName);

    Configuration m_configuration;

    public string Name => "GithubOauth2";

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
        clientSecret = Lemoine.Info.ConfigSet.Get<string> (CLIENT_SECRET_KEY);
      }
      var contents = new List<KeyValuePair<string, string>> {
        new KeyValuePair<string, string> ("client_id", clientId),
        new KeyValuePair<string, string> ("client_secret", clientSecret),
        new KeyValuePair<string, string> ("code", code),
      };
      if (!string.IsNullOrEmpty (state)) {
        contents.Add (new KeyValuePair<string, string> ("state", state));
      }
      var content = new FormUrlEncodedContent (contents);
      var request = new HttpRequestMessage () {
        RequestUri = new Uri ("https://github.com/login/oauth/access_token"),
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
        if (responseString.Contains ("bad_verification_code")) {
          log.Error ($"ValidateCodeAsync: bad_verification_code returned, the code {code} is not valid");
        }
        throw new Exception ("No token retrieved");
      }

      var login = await GetLoginAsync (httpClient, token);
      if (string.IsNullOrEmpty (login)) {
        log.Error ($"ValidateCodeAsync: no login was retrieved");
        throw new Exception ("No login was retrieved from github");
      }

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginReadOnlyTransaction ("Plugin.GithubOauth2.ValidateCode")) {
        var user = ModelDAOHelper.DAOFactory.UserDAO
          .FindByLogin (login);
        if (user is null) {
          log.Error ($"ValidateCodeAsync: login {login} does not exist in Lemoine database");
        }
        return new ValidateCodeResponse {
          User = user
        };
      }
    }

    async Task<string> GetLoginAsync (HttpClient httpClient, string token)
    {
      var request = new HttpRequestMessage () {
        RequestUri = new Uri ("https://api.github.com/user")
      };
      request.Headers.Accept.Add (new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue ("application/vnd.github.v3+json"));
      request.Headers.UserAgent.Add (new System.Net.Http.Headers.ProductInfoHeaderValue ("Pulse", "11.0"));
      request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue ("token", token);
      string responseString;
      using (var response = await httpClient.SendAsync (request)) {
        if (response is null) {
          log.Fatal ($"GetLoginAsync: response is null");
          return null;
        }
        else if (response.IsSuccessStatusCode) {
          responseString = await response.Content.ReadAsStringAsync ();
        }
        else {
          log.Error ($"GetLoginAsync: status code is {response.StatusCode}");
          return null;
        }
      }
      try {
        var userResponse = JsonConvert.DeserializeObject<UserResponse> (responseString);
        if (userResponse is null) {
          log.Error ($"GetLoginAsync: invalid response string {responseString}");
          return null;
        }
        if (log.IsDebugEnabled) {
          log.Debug ($"GetLoginAsync: retrieved login is {userResponse.Login}");
        }
        return userResponse.Login;
      }
      catch (Exception ex) {
        log.Error ($"GetLoginAsync: deserialization of {responseString} failed", ex);
        throw;
      }
    }

    public Task<ValidateCodeResponse> RenewTokenAsync (IServiceProvider serviceProvider, string refreshToken)
    {
      throw new NotImplementedException ();
    }
  }
}
