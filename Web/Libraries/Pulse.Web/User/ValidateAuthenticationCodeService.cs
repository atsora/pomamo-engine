// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Interfaces;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Extensions.Web.Services;
using Lemoine.Extensions.Web.User;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Web;
using Lemoine.Web.User;
using Pulse.Extensions.Web.User;

namespace Pulse.Web.User
{
  /// <summary>
  /// Service to validate an authentication code, for example an OAuth2 code from the first step
  /// </summary>
  public class ValidateAuthenticationCodeService
    : GenericPostOnlyNoCacheService<ValidateAuthenticationCodeRequestDTO, ValidateAuthenticationCodePostDTO>
    , IBodySupport
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ValidateAuthenticationCodeService).FullName);

    static readonly string REFRESH_TOKEN_EXPIRES_IN_KEY = "RefreshToken.ExpiresIn";
    static readonly TimeSpan REFRESH_TOKEN_EXPIRES_IN_DEFAULT = TimeSpan.FromDays (1);

    readonly IServiceProvider m_serviceProvider;

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public ValidateAuthenticationCodeService (IServiceProvider serviceProvider)
    {
      m_serviceProvider = serviceProvider;
    }
    #endregion // Constructors

    /// <summary>
    /// Post data
    /// </summary>
    /// <param name="postDto"></param>
    /// <returns></returns>
    public override async System.Threading.Tasks.Task<object> PostAsync (ValidateAuthenticationCodePostDTO postDto)
    {
      try {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          var extensionRequest = new Lemoine.Business.Extension.NameExtensions<IValidateAuthenticationCodeExtension> (postDto.AuthenticationKind, ext => ext.Initialize ());
          var extensions = await Lemoine.Business.ServiceProvider.GetAsync (extensionRequest);
          var extension = extensions.Single (x => ValidateAuthenticationName (x, postDto));
          var validateCodeResponse = await extension.ValidateCodeAsync (m_serviceProvider, postDto.Code, postDto.State);
          if (validateCodeResponse.User is null) {
            log.Error ($"PostAsync: user null for extension {extension.Name}");
            if (log.IsDebugEnabled) {
              log.Debug ($"PostAsync: no user returned for code {postDto.Code} and state {postDto.State}");
            }
            return new ErrorDTO ("Invalid user", ErrorStatus.AuthorizationError);
          }
          await ModelDAOHelper.DAOFactory.RefreshTokenDAO.CleanAsync (validateCodeResponse.User);
          IRefreshToken refreshToken;
          if (string.IsNullOrEmpty (validateCodeResponse.RefreshToken)) {
            var expiresIn = validateCodeResponse.User.DisconnectionTime
              ?? Lemoine.Info.ConfigSet.LoadAndGet (REFRESH_TOKEN_EXPIRES_IN_KEY, REFRESH_TOKEN_EXPIRES_IN_DEFAULT);
            refreshToken = ModelDAOHelper.ModelFactory.CreateRefreshToken (validateCodeResponse.User, expiresIn);
          }
          else { // validateCoreResponse.RefreshToken not empty
            refreshToken = ModelDAOHelper.ModelFactory.CreateRefreshToken (validateCodeResponse.User, validateCodeResponse.RefreshTokenExpiresIn);
            refreshToken.OAuth2Name = extension.Name;
            refreshToken.OAuth2AuthenticationName = extension.AuthenticationName;
            refreshToken.OAuth2RefreshToken = validateCodeResponse.RefreshToken;
          }
          return new UserPermissionsResponseDTO (validateCodeResponse.User, refreshToken);
        }
      }
      catch (Exception ex) {
        log.Error ($"PostAsync: exception", ex);
        throw;
      }
    }

    bool ValidateAuthenticationName (IValidateAuthenticationCodeExtension ext, ValidateAuthenticationCodePostDTO postDto)
    {
      return string.IsNullOrEmpty (postDto.AuthenticationName)
        || string.Equals (ext.AuthenticationName, postDto.AuthenticationName, StringComparison.InvariantCultureIgnoreCase);
    }
  }
}
