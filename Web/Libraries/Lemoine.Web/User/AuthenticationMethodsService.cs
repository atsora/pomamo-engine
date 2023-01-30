// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Extensions.Web.Services;
using Lemoine.Extensions.Web.User;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Web.User
{
  /// <summary>
  /// Description of XxxService
  /// </summary>
  public class AuthenticationMethodsService
    : GenericAsyncCachedService<AuthenticationMethodsRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AuthenticationMethodsService).FullName);

    const string USER_PASSWORD_AUTHENTICATION_KEY = "Web.User.UserPasswordAuthentication";
    const bool USER_PASSWORD_AUTHENTICATION_DEFAULT = true;

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public AuthenticationMethodsService ()
      : base (Lemoine.Core.Cache.CacheTimeOut.Config)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override async System.Threading.Tasks.Task<object> Get (AuthenticationMethodsRequestDTO request)
    {
      try {
        var response = new AuthenticationMethodsResponseDTO ();
        response.UserPasswordAuthentication = Lemoine.Info.ConfigSet
          .LoadAndGet (USER_PASSWORD_AUTHENTICATION_KEY, USER_PASSWORD_AUTHENTICATION_DEFAULT);

        var oauth2ExtensionRequest = new Lemoine.Business.Extension.GlobalExtensions<IOAuth2RedirectionExtension> (ext => ext.Initialize ());
        var oauth2Extensions = await Lemoine.Business.ServiceProvider.GetAsync (oauth2ExtensionRequest);
        foreach (var oauth2Extension in oauth2Extensions.Where (ext => IsApplicationValid (ext, request.ApplicationName))) {
          var oauth2Response = new OAuth2MethodResponseDTO ();
          oauth2Response.AuthenticationKind = oauth2Extension.Name;
          oauth2Response.AuthenticationName = oauth2Extension.AuthenticationName;
          oauth2Response.AuthenticationUrl = oauth2Extension.GetAuthenticationUrl ();
          response.OAuth2Methods.Add (oauth2Response);
        }
        return response;
      }
      catch (Exception ex) {
        log.Error ($"Get: exception", ex);
        throw;
      }
    }

    bool IsApplicationValid (IOAuth2RedirectionExtension ext, string applicationName)
    {
      if (string.IsNullOrEmpty (applicationName)) {
        return true;
      }
      if (string.IsNullOrEmpty (ext.ApplicationName)) {
        return true;
      }
      return string.Equals (ext.ApplicationName, applicationName);
    }
    #endregion // Methods
  }
}
