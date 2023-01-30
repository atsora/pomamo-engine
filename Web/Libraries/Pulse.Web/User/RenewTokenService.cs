// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Model;
using Lemoine.Extensions.Web.Interfaces;
using System.IO;
using Lemoine.Web.CommonRequestDTO;
using System.Threading.Tasks;
using System.Linq;
using Lemoine.Web;
using Pulse.Extensions.Web.User;
using Lemoine.Web.User;

namespace Pulse.Web.User
{
  /// <summary>
  /// Service to return the name of a machine or a group
  /// </summary>
  public class RenewTokenService
    : GenericPostOnlyNoCacheService<RenewTokenRequestDTO, RenewTokenPostDto>
    , IBodySupport
  {
    static readonly ILog log = LogManager.GetLogger (typeof (RenewTokenService).FullName);

    static readonly string REFRESH_TOKEN_EXPIRES_IN_KEY = "RefreshToken.ExpiresIn";
    static readonly TimeSpan REFRESH_TOKEN_EXPIRES_IN_DEFAULT = TimeSpan.FromDays (1);

    readonly IServiceProvider m_serviceProvider;

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public RenewTokenService (IServiceProvider serviceProvider)
    {
      m_serviceProvider = serviceProvider;
    }
    #endregion // Constructors

    public override async Task<object> PostAsync (RenewTokenPostDto postDto)
    {
      var login = postDto.Login;
      var refreshTokenString = postDto.RefreshToken;

      if (string.IsNullOrEmpty (login)) {
        log.Error ($"PostAsync: login was not set");
        return new ErrorDTO ("Login not set", ErrorStatus.WrongRequestParameter);
      }

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ("Web.UserPermissions")) {

        IUser user = null;
        if (!string.IsNullOrEmpty (login)) {
          user = await ModelDAOHelper.DAOFactory.UserDAO
            .FindByLoginAsync (login);
          if (null == user) {
            log.Error ($"PostAsync: unknown user with login {login}");
            transaction.Rollback ();
            return new ErrorDTO ("No user with the specified login", ErrorStatus.WrongRequestParameter);
          }
        }

        if (null != user) {
          if (log.IsDebugEnabled) {
            log.Debug ($"PostAsync: clean tokens");
          }
          await ModelDAOHelper.DAOFactory.RefreshTokenDAO.CleanAsync (user);
        }

        var refreshToken = await ModelDAOHelper.DAOFactory.RefreshTokenDAO.FindByTokenAsync (refreshTokenString, user);
        if (refreshToken is null) {
          log.Error ($"PostAsync: invalid or expired refresh token {refreshTokenString}");
          transaction.Commit ();
          return new ErrorDTO ("Invalid or expired refresh token", ErrorStatus.AuthorizationError);
        }

        refreshToken.Revoked = DateTime.UtcNow;
        await ModelDAOHelper.DAOFactory.RefreshTokenDAO.MakePersistentAsync (refreshToken);

        if (!string.IsNullOrEmpty (refreshToken.OAuth2RefreshToken) && !string.IsNullOrEmpty (refreshToken.OAuth2Name)) {
          var extensionRequest = new Lemoine.Business.Extension.NameExtensions<IValidateAuthenticationCodeExtension> (refreshToken.OAuth2Name, ext => ext.Initialize ());
          var extensions = await Lemoine.Business.ServiceProvider.GetAsync (extensionRequest);
          var extension = extensions.Single (x => ValidateAuthenticationName (x, refreshToken));
          var validateCodeResponse = await extension.RenewTokenAsync (m_serviceProvider, refreshToken.OAuth2RefreshToken);
          if (validateCodeResponse.User is null) {
            log.Error ($"PostAsync: user null for extension {extension.Name}");
            transaction.Commit ();
            return new ErrorDTO ("OAuth2 refresh: renewing the OAuth2 token failed", ErrorStatus.AuthorizationError);
          }
          if (validateCodeResponse.User.Id != refreshToken.User.Id) {
            log.Error ($"PostAsync: user changed");
            transaction.Commit ();
            return new ErrorDTO ($"OAuth2 refresh: user changed", ErrorStatus.AuthorizationError);
          }
          refreshToken = ModelDAOHelper.ModelFactory.CreateRefreshToken (user, validateCodeResponse.RefreshTokenExpiresIn);
          refreshToken.OAuth2Name = extension.Name;
          refreshToken.OAuth2AuthenticationName = extension.AuthenticationName;
          refreshToken.OAuth2RefreshToken = validateCodeResponse.RefreshToken;
          await ModelDAOHelper.DAOFactory.RefreshTokenDAO.MakePersistentAsync (refreshToken);
        }
        else {
          var expiresIn = user.DisconnectionTime
            ?? Lemoine.Info.ConfigSet.LoadAndGet (REFRESH_TOKEN_EXPIRES_IN_KEY, REFRESH_TOKEN_EXPIRES_IN_DEFAULT);
          refreshToken = ModelDAOHelper.ModelFactory.CreateRefreshToken (user, expiresIn);
          await ModelDAOHelper.DAOFactory.RefreshTokenDAO.MakePersistentAsync (refreshToken);
        }

        transaction.Commit ();
        return new UserPermissionsResponseDTO (user, refreshToken);
      }
    }

    bool ValidateAuthenticationName (IValidateAuthenticationCodeExtension ext, IRefreshToken refreshToken)
    {
      return string.IsNullOrEmpty (refreshToken.OAuth2AuthenticationName)
        || string.Equals (ext.AuthenticationName, refreshToken.OAuth2AuthenticationName, StringComparison.InvariantCultureIgnoreCase);
    }
  }
}
