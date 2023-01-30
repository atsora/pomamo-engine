// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.Extensions.Configuration.GuiBuilder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace Lemoine.Plugin.FordAdfs
{
  public sealed class Configuration
    : Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters
    /// <summary>
    /// My parameter
    /// </summary>
    [PluginConf ("Text", "AuthenticationName", Description = "Name of the authentication")]
    [DefaultValue ("Adfs")]
    public string AuthenticationName
    {
      get; set;
    } = "Adfs";

    [PluginConf ("Text", "ApplicationName", Description = "Restrict this authentication to a specific application. If empty, matches all applications")]
    [DefaultValue ("")]
    public string ApplicationName
    {
      get; set;
    } = "";

    /// <summary>
    /// My parameter
    /// </summary>
    [PluginConf ("Text", "ClientId", Description = "Ford ADFS Oauth2 client id")]
    [DefaultValue ("urn:lpmm:clientid:web_lpmmqasite:qa")]
    public string ClientId
    {
      get; set;
    } = "urn:lpmm:clientid:web_lpmmqasite:qa";

    /// <summary>
    /// My parameter
    /// </summary>
    [PluginConf ("Text", "ClientSecret", Description = "Ford ADFS Oauth2 client secret. Optional, this is safer to use a config key to set it")]
    [DefaultValue ("")]
    public string ClientSecret
    {
      get; set;
    } = "";

    [PluginConf ("Text", "AdfsEnvironment", Description = "Ford ADFS Environment")]
    public string AdfsEnvironment
    {
      get; set;
    }

    [PluginConf ("Text", "AdfsResource", Description = "ADFS resource. For example: urn:joc:dev:std:web")]
    [DefaultValue ("urn:lpmm:resource:web_lpmmqasite:qa")]
    public string AdfsResource
    {
      get; set;
    } = "urn:lpmm:resource:web_lpmmqasite:qa";

    /// <summary>
    /// Configured redirect URI
    /// </summary>
    [DefaultValue ("")]
    [PluginConf ("Text", "RedirectUri", Description = "Configured Redirect URI that is sent in the initial authentication request. If empty, try to determine it")]
    public string RedirectUri
    {
      get; set;
    } = "";

    [DefaultValue ("")]
    [PluginConf ("Text", "AuthenticationRedirect", Description = "Authentication URI Redirect that is sent to the token validation or renew request. If empty it is not set")]
    public string AuthenticationRedirect
    {
      get; set;
    } = "";
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public Configuration ()
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="errors"></param>
    /// <returns></returns>
    public bool IsValid (out IEnumerable<string> errors)
    {
      var errorList = new List<string> ();
      // TODO: check the parameters here, for example:
      /* 
      if (string.IsNullOrEmpty (this.MyParameter)) {
        errorList.Add ("ID Prefix is empty");
      }
      */
      errors = errorList;
      return true;
    }
    #endregion // Constructors
  }
}
