// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.WebClient;
using Lemoine.Core.Log;
using System.Threading.Tasks;
using Lemoine.ModelDAO;

namespace Lemoine.WebDataAccess
{
  /// <summary>
  /// Description of ComputerDAO.
  /// </summary>
  public class ComputerDAO: Lemoine.ModelDAO.IComputerDAO
  {
    #region IComputerDAO implementation
    public Lemoine.Model.IComputer GetLctr()
    {
      return WebServiceHelper.UniqueResult<IComputer, Computer> (new RequestUrl ("/Data/Computer/GetLctr"));
    }
    public async System.Threading.Tasks.Task<Lemoine.Model.IComputer> GetLctrAsync ()
    {
      return await WebServiceHelper.UniqueResultAsync<IComputer, Computer> (new RequestUrl ("/Data/Computer/GetLctr"));
    }
    public Lemoine.Model.IComputer GetLocal()
    {
      throw new NotImplementedException();
    }
    public Lemoine.Model.IComputer GetOrCreateLocal()
    {
      throw new NotImplementedException();
    }
    public IList<Lemoine.Model.IComputer> GetLposts()
    {
      return WebServiceHelper.List<IComputer, Computer> (new RequestUrl ("/Data/Computer/GetLposts"));
    }
    public async System.Threading.Tasks.Task<IList<Lemoine.Model.IComputer>> GetLpostsAsync ()
    {
      return await WebServiceHelper.ListAsync<IComputer, Computer> (new RequestUrl ("/Data/Computer/GetLposts"));
    }
    public IList<Lemoine.Model.IComputer> GetCnc()
    {
      return WebServiceHelper.List<IComputer, Computer> (new RequestUrl ("/Data/Computer/GetCnc"));
    }
    public async System.Threading.Tasks.Task<IList<Lemoine.Model.IComputer>> GetCncAsync ()
    {
      return await WebServiceHelper.ListAsync<IComputer, Computer> (new RequestUrl ("/Data/Computer/GetCnc"));
    }
    public IList<Lemoine.Model.IComputer> GetWeb()
    {
      return WebServiceHelper.List<IComputer, Computer> (new RequestUrl ("/Data/Computer/GetWeb"));
    }
    public async System.Threading.Tasks.Task<IList<Lemoine.Model.IComputer>> GetWebAsync ()
    {
      return await WebServiceHelper.ListAsync<IComputer, Computer> (new RequestUrl ("/Data/Computer/GetWeb"));
    }
    public Lemoine.Model.IComputer GetAutoReason ()
    {
      return WebServiceHelper.UniqueResult<IComputer, Computer> (new RequestUrl ("/Data/Computer/GetAutoReason"));
    }
    public async System.Threading.Tasks.Task<Lemoine.Model.IComputer> GetAutoReasonAsync ()
    {
      return await WebServiceHelper.UniqueResultAsync<IComputer, Computer> (new RequestUrl ("/Data/Computer/GetAutoReason"));
    }
    public Lemoine.Model.IComputer GetAlert ()
    {
      return WebServiceHelper.UniqueResult<IComputer, Computer> (new RequestUrl ("/Data/Computer/GetAlert"));
    }
    public async System.Threading.Tasks.Task<Lemoine.Model.IComputer> GetAlertAsync ()
    {
      return await WebServiceHelper.UniqueResultAsync<IComputer, Computer> (new RequestUrl ("/Data/Computer/GetAlert"));
    }
    public Lemoine.Model.IComputer GetSynchronization ()
    {
      return WebServiceHelper.UniqueResult<IComputer, Computer> (new RequestUrl ("/Data/Computer/GetSynchronization"));
    }
    public async System.Threading.Tasks.Task<Lemoine.Model.IComputer> GetSynchronizationAsync ()
    {
      return await WebServiceHelper.UniqueResultAsync<IComputer, Computer> (new RequestUrl ("/Data/Computer/GetSynchronization"));
    }
    #endregion
    #region IGenericUpdateDAO implementation
    public Lemoine.Model.IComputer FindByIdAndLock(int id)
    {
      throw new NotImplementedException();
    }
    #endregion
    #region IGenericDAO implementation
    public Lemoine.Model.IComputer FindById(int id)
    {
      throw new NotImplementedException();
    }
    #endregion
    #region IBaseGenericUpdateDAO implementation
    public void UpgradeLock(Lemoine.Model.IComputer entity)
    {
      throw new NotImplementedException();
    }
    public Lemoine.Model.IComputer Reload(Lemoine.Model.IComputer entity)
    {
      throw new NotImplementedException();
    }
    #endregion
    #region IBaseGenericDAO implementation
    public System.Collections.Generic.IList<Lemoine.Model.IComputer> FindAll()
    {
      throw new NotImplementedException();
    }
    public Lemoine.Model.IComputer MakePersistent(Lemoine.Model.IComputer entity)
    {
      throw new NotImplementedException();
    }
    public void MakeTransient(Lemoine.Model.IComputer entity)
    {
      throw new NotImplementedException();
    }
    public void Lock(Lemoine.Model.IComputer entity)
    {
      throw new NotImplementedException();
    }

    public Task<IList<IComputer>> FindAllAsync ()
    {
      throw new NotImplementedException ();
    }

    public Task<IComputer> MakePersistentAsync (IComputer entity)
    {
      throw new NotImplementedException ();
    }

    public Task MakeTransientAsync (IComputer entity)
    {
      throw new NotImplementedException ();
    }

    public Task LockAsync (IComputer entity)
    {
      throw new NotImplementedException ();
    }

    public Task<IComputer> FindByIdAsync (int id)
    {
      throw new NotImplementedException ();
    }
    #endregion
  }
}
