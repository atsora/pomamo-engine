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

namespace Lemoine.Plugin.FordAdfs
{
  public class OAuth2RedirectionExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IOAuth2RedirectionExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (OAuth2RedirectionExtension).FullName);

    Configuration m_configuration;

    public string Name => "FordAdfs";

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
      var url = $"https://corp{m_configuration.AdfsEnvironment}.sts.ford.com/adfs/oauth2/authorize?response_type=code&client_id={m_configuration.ClientId}&resource={m_configuration.AdfsResource}";
      if (!string.IsNullOrEmpty (m_configuration.RedirectUri)) {
        var encodedRedirectUri = System.Web.HttpUtility.UrlEncode (m_configuration.RedirectUri);
        url += $"&redirect_uri={encodedRedirectUri}";
      }
      if (this.StateRequired) {
        url += "&state={{state}}";
      }
      return url;
    }
  }
}
