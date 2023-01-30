// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Cache;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Extensions.Web.Services;
using Lemoine.Web.CommonRequestDTO;
using Lemoine.Extensions.Web.Interfaces;
using System.IO;
using System.Threading.Tasks;
using Lemoine.Web;

namespace Pulse.Web.User
{
  /// <summary>
  /// Description of XxxService
  /// </summary>
  public class UserChangePasswordService
    : GenericPostOnlyNoCacheService<UserChangePasswordRequestDTO, UserChangePasswordPostDto>
    , IBodySupport
  {
    static readonly ILog log = LogManager.GetLogger (typeof (UserChangePasswordService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public UserChangePasswordService ()
    {
    }
    #endregion // Constructors

    /// <summary>
    /// Post method
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override async Task<object> PostAsync (UserChangePasswordPostDto postDto)
    {
      return await UpdatePasswordAsync (postDto.Login, postDto.OldPassword, postDto.NewPassword);
    }

    async System.Threading.Tasks.Task<object> UpdatePasswordAsync (string login, string oldPassword, string newPassword)
    {
      if (string.IsNullOrEmpty (login)) {
        log.Error ($"UpdatePasswordAsync: login was not set");
        return new ErrorDTO ("Login not set", ErrorStatus.WrongRequestParameter);
      }

      if (string.IsNullOrEmpty (newPassword)) {
        log.Error ($"UpdatePasswordAsync: NewPassword was not set");
        return new ErrorDTO ("NewPassword not set", ErrorStatus.WrongRequestParameter);
      }

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ("Web.UserChangePassword")) {

        var user = await ModelDAOHelper.DAOFactory.UserDAO
          .FindByLoginAsync (login);
        if (user is null) {
          log.Error ($"UpdatePasswordAsync: unknown user with login {login}");
          transaction.Commit ();
          return new ErrorDTO ("No user with the specified login", ErrorStatus.WrongRequestParameter);
        }

        if (!string.Equals (user.Password, oldPassword)) {
          log.Error ($"UpdatePasswordAsync: old password was invalid");
          transaction.Commit ();
          return new ErrorDTO ("Invalid old password", ErrorStatus.WrongRequestParameter);
        }

        user.Password = newPassword;
        await ModelDAOHelper.DAOFactory.UserDAO
          .MakePersistentAsync (user);
        transaction.Commit ();
        return new OkDTO ("The password was successfully updated");
      }
    }
  }
}
