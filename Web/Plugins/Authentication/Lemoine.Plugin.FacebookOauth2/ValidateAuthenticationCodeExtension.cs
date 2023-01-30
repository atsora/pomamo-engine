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

namespace Lemoine.Plugin.FacebookOauth2
{
  public class AccessTokenResponse
  {
    [JsonProperty ("access_token")]
    public string AccessToken { get; set; }

    [JsonProperty ("token_type")]
    public string TokenType { get; set; }

    [JsonProperty ("expires_in")]
    public string ExpiresIn { get; set; }
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
    const string CLIENT_SECRET_KEY = "Plugin.FacebookOauth2.Secret";

    readonly ILog log = LogManager.GetLogger (typeof (ValidateAuthenticationCodeExtension).FullName);

    Configuration m_configuration;

    public string Name => "FacebookOauth2";

    public string AuthenticationName => m_configuration.AuthenticationName;

    public bool Initialize ()
    {
      var result = LoadConfiguration (out m_configuration);
      return result;
    }

    public async Task<ValidateCodeResponse> ValidateCodeAsync (IServiceProvider serviceProvider, string code, string state = "")
    {
      var httpClient = serviceProvider.GetService<HttpClient> ();
      var clientId = string.IsNullOrEmpty (m_configuration.ClientId)
        ? "2508083976004030"
        : m_configuration.ClientId;
      var clientSecret = m_configuration.ClientSecret;
      if (string.IsNullOrEmpty (clientSecret)) {
        clientSecret = Lemoine.Info.ConfigSet.LoadAndGet<string> (CLIENT_SECRET_KEY, "c06dc80cdffe9757621347e17dcd8c7c");
      }
      var contents = new List<KeyValuePair<string, string>> {
        new KeyValuePair<string, string> ("client_id", clientId),
        new KeyValuePair<string, string> ("client_secret", clientSecret),
        new KeyValuePair<string, string> ("redirect_uri", m_configuration.RedirectUri),
        new KeyValuePair<string, string> ("code", code),
      };
      if (!string.IsNullOrEmpty (state)) {
        contents.Add (new KeyValuePair<string, string> ("state", state));
      }
      var content = new FormUrlEncodedContent (contents);
      var request = new HttpRequestMessage () {
        RequestUri = new Uri ("https://graph.facebook.com/v10.0/oauth/access_token"),
        Method = HttpMethod.Post,
        Content = content,
      };
      request.Headers.Accept.Add (new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue ("application/json"));
      string responseString;
      using (var response = await httpClient.SendAsync (request)) {
        responseString = await response.Content.ReadAsStringAsync ();
      }
      var accessTokenResponse = JsonConvert.DeserializeObject<AccessTokenResponse> (responseString);
      var token = accessTokenResponse.AccessToken;

      var name = await GetNameAsync (httpClient, token);

      // TODO: OAuth2 refresh token

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginReadOnlyTransaction ("Plugin.FacebookOauth2.ValidateCode")) {
        var allUsers = await ModelDAOHelper.DAOFactory.UserDAO
          .FindAllAsync ();
        var user = allUsers.First (x => x.Name.Equals (name));
        return new ValidateCodeResponse {
          User = user
        };
      }
    }

    async Task<string> GetNameAsync (HttpClient httpClient, string token)
    {
      var request = new HttpRequestMessage () {
        RequestUri = new Uri ("https://graph.facebook.com/me")
      };
      request.Headers.Accept.Add (new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue ("application/json"));
      request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue ("Bearer", token);
      string responseString;
      using (var response = await httpClient.SendAsync (request)) {
        responseString = await response.Content.ReadAsStringAsync ();
      }
      var meResponse = JsonConvert.DeserializeObject<MeResponse> (responseString);
      return meResponse.Name;
    }

    public Task<ValidateCodeResponse> RenewTokenAsync (IServiceProvider serviceProvider, string refreshToken)
    {
      throw new NotImplementedException ();
    }
  }
}
