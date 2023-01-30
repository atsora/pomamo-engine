// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Model;
using Lemoine.WebClient;
using Lemoine.Core.Log;
using System.Threading.Tasks;
using System.Collections.Generic;
using Lemoine.ModelDAO;

namespace Lemoine.WebDataAccess
{
  /// <summary>
  /// Description of UserDAO.
  /// </summary>
  public class UserDAO: Lemoine.ModelDAO.IUserDAO
  {
    #region IUserDAO implementation
    public Lemoine.Model.IUser FindByLogin(string userLogin)
    {
      throw new NotImplementedException();
    }
    public System.Threading.Tasks.Task<Lemoine.Model.IUser> FindByLoginAsync (string userLogin)
    {
      throw new NotImplementedException ();
    }
    #endregion
    #region IGenericUpdateDAO implementation
    public Lemoine.Model.IUser FindByIdAndLock(int id)
    {
      throw new NotImplementedException();
    }
    #endregion
    #region IGenericDAO implementation
    public Lemoine.Model.IUser FindById(int id)
    {
      return WebServiceHelper.UniqueResult<IUser, User> (new RequestUrl ("/Data/User/FindById/" + id));
    }
    #endregion
    #region IBaseGenericUpdateDAO implementation
    public void UpgradeLock(Lemoine.Model.IUser entity)
    {
      throw new NotImplementedException();
    }
    public Lemoine.Model.IUser Reload(Lemoine.Model.IUser entity)
    {
      throw new NotImplementedException();
    }
    #endregion
    #region IBaseGenericDAO implementation
    public System.Collections.Generic.IList<Lemoine.Model.IUser> FindAll()
    {
      throw new NotImplementedException();
    }
    public Lemoine.Model.IUser MakePersistent(Lemoine.Model.IUser entity)
    {
      throw new NotImplementedException();
    }
    public void MakeTransient(Lemoine.Model.IUser entity)
    {
      throw new NotImplementedException();
    }
    public void Lock(Lemoine.Model.IUser entity)
    {
      throw new NotImplementedException();
    }

    public Task<IList<IUser>> FindAllAsync ()
    {
      throw new NotImplementedException ();
    }

    public Task<IUser> MakePersistentAsync (IUser entity)
    {
      throw new NotImplementedException ();
    }

    public Task MakeTransientAsync (IUser entity)
    {
      throw new NotImplementedException ();
    }

    public Task LockAsync (IUser entity)
    {
      throw new NotImplementedException ();
    }

    public async Task<IUser> FindByIdAsync (int id)
    {
      return await WebServiceHelper.UniqueResultAsync<IUser, User> (new RequestUrl ("/Data/User/FindById/" + id));
    }
    #endregion
  }
}
