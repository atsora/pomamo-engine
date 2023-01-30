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
  /// Description of ReasonDAO.
  /// </summary>
  public class ReasonDAO: Lemoine.ModelDAO.IReasonDAO
  {
    #region IReasonDAO implementation
    public Lemoine.Model.IReason FindByCode(string code)
    {
      return WebServiceHelper.UniqueResult<IReason, Reason> (new RequestUrl ("/Data/Reason/FindByCode/" + code));
    }
    public System.Collections.Generic.IList<Lemoine.Model.IReason> FindAllWithReasonGroup()
    {
      throw new NotImplementedException();
    }
    public System.Collections.Generic.IList<Lemoine.Model.IReason> FindAllWithReasonGroup(Lemoine.Model.IReasonGroup reasonGroup)
    {
      throw new NotImplementedException();
    }
    #endregion
    #region IGenericUpdateDAO implementation
    public Lemoine.Model.IReason FindByIdAndLock(int id)
    {
      throw new NotImplementedException();
    }
    #endregion
    #region IGenericDAO implementation
    public Lemoine.Model.IReason FindById(int id)
    {
      return WebServiceHelper.UniqueResult<IReason, Reason> (new RequestUrl ("/Data/Reason/FindById/" + id));
    }
    #endregion
    #region IBaseGenericUpdateDAO implementation
    public void UpgradeLock(Lemoine.Model.IReason entity)
    {
      throw new NotImplementedException();
    }
    public Lemoine.Model.IReason Reload(Lemoine.Model.IReason entity)
    {
      throw new NotImplementedException();
    }
    #endregion
    #region IBaseGenericDAO implementation
    public System.Collections.Generic.IList<Lemoine.Model.IReason> FindAll()
    {
      throw new NotImplementedException();
    }
    public Lemoine.Model.IReason MakePersistent(Lemoine.Model.IReason entity)
    {
      throw new NotImplementedException();
    }
    public void MakeTransient(Lemoine.Model.IReason entity)
    {
      throw new NotImplementedException();
    }
    public void Lock(Lemoine.Model.IReason entity)
    {
      throw new NotImplementedException();
    }

    public Task<IList<IReason>> FindAllAsync ()
    {
      throw new NotImplementedException ();
    }

    public Task<IReason> MakePersistentAsync (IReason entity)
    {
      throw new NotImplementedException ();
    }

    public Task MakeTransientAsync (IReason entity)
    {
      throw new NotImplementedException ();
    }

    public Task LockAsync (IReason entity)
    {
      throw new NotImplementedException ();
    }

    public async Task<IReason> FindByIdAsync (int id)
    {
      return await WebServiceHelper.UniqueResultAsync<IReason, Reason> (new RequestUrl ("/Data/Reason/FindById/" + id));
    }
    #endregion
  }
}
