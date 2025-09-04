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
using Lemoine.Web;

namespace Pulse.Web.User
{
  /// <summary>
  /// Service to return the name of a machine or a group
  /// </summary>
  public class UserPermissionsService
    : GenericPostOnlyNoCacheService<UserPermissionsPostRequestDTO, UserPermissionsPostDto>
    , IBodySupport
  {
    static readonly ILog log = LogManager.GetLogger (typeof (UserPermissionsService).FullName);

    /// <summary>
    /// Description of the constructor
    /// </summary>
    public UserPermissionsService () { }

    /// <summary>
    /// Post method
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async override Task<object> PostAsync (UserPermissionsPostDto postDto)
    {
      return await GetPermissionsAsync (postDto.Login, postDto.Password);
    }

    async System.Threading.Tasks.Task<object> GetPermissionsAsync (string login, string password)
    {
      if (string.IsNullOrEmpty (login)) {
        log.Error ($"GetPermissionsAsync: login was not set");
        return new ErrorDTO ("Login not set", ErrorStatus.WrongRequestParameter);
      }

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var user = await ModelDAOHelper.DAOFactory.UserDAO
          .FindByLoginAsync (login);
        if (user is null) {
          log.Error ($"GetPermissionsAsync: unknown user with login {login}");
          return new ErrorDTO ("No user with the specified login", ErrorStatus.WrongRequestParameter);
        }

        if (!Lemoine.Model.Password.IsMatch (password, user.Password)) {
          log.Error ($"GetPermissionsAsync: old password was invalid");
          return new ErrorDTO ("Invalid password", ErrorStatus.WrongRequestParameter);
        }

        var refreshToken = await ModelDAOHelper.DAOFactory.RefreshTokenDAO.GetRefreshTokenAsync (user, clean: true);
        return new UserPermissionsResponseDTO (user, refreshToken);
      }
    }
  }
}
