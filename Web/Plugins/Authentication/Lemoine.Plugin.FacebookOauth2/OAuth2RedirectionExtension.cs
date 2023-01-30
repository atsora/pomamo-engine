// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.User;
using Lemoine.ModelDAO;

namespace Lemoine.Plugin.FacebookOauth2
{
  public class OAuth2RedirectionExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IOAuth2RedirectionExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (OAuth2RedirectionExtension).FullName);

    Configuration m_configuration;

    public string Name => "FacebookOauth2";

    public string AuthenticationName => m_configuration.AuthenticationName;

    public string ApplicationName => m_configuration.ApplicationName;

    public bool StateRequired => false;

    public bool LoginRequired => false;

    public bool Initialize ()
    {
      return LoadConfiguration (out m_configuration);
    }

    public string GetAuthenticationUrl ()
    {
      var clientId = string.IsNullOrEmpty (m_configuration.ClientId)
        ? "2508083976004030"
        : m_configuration.ClientId;
      var encodedRedirectUri = System.Web.HttpUtility.UrlEncode (m_configuration.RedirectUri);
      return $"https://www.facebook.com/v10.0/dialog/oauth?client_id={clientId}&redirect_uri={encodedRedirectUri}";
    }
  }
}
