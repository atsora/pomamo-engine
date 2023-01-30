// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IUser.
  /// </summary>
  public interface IUserDAO: IGenericUpdateDAO<IUser, int>
  {
    /// <summary>
    /// Find user through its login
    /// </summary>
    /// <param name="userLogin"></param>
    /// <returns></returns>
    IUser FindByLogin(string userLogin);

    /// <summary>
    /// Find user through its login asynchronously
    /// </summary>
    /// <param name="userLogin"></param>
    /// <returns></returns>
    System.Threading.Tasks.Task<IUser> FindByLoginAsync (string userLogin);
  }
}
