// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate.Criterion;
using System.Diagnostics;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IUserDAO">IUserDAO</see>
  /// </summary>
  public class UserDAO
    : VersionableNHibernateDAO<User, IUser, int>
    , IUserDAO
  {
    static readonly ILog log = LogManager.GetLogger(typeof (UserDAO).FullName);
    
    /// <summary>
    /// Find user through its login
    /// </summary>
    /// <param name="userLogin"></param>
    /// <returns></returns>
    public IUser FindByLogin(string userLogin)
    {
      Debug.Assert (!string.IsNullOrEmpty (userLogin));

      return NHibernateHelper.GetCurrentSession()
        .CreateCriteria<User> ()
        .Add (Restrictions.Eq ("Login", userLogin))
        .UniqueResult<IUser> ();
    }

    /// <summary>
    /// <see cref="IUserDAO"/>
    /// </summary>
    /// <param name="userLogin"></param>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<IUser> FindByLoginAsync (string userLogin)
    {
      Debug.Assert (!string.IsNullOrEmpty (userLogin));

      return await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<User> ()
        .Add (Restrictions.Eq ("Login", userLogin))
        .UniqueResultAsync<IUser> ();
    }
  }
}
