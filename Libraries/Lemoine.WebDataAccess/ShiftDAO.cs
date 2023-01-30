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
  /// Description of ShiftDAO.
  /// </summary>
  public class ShiftDAO: Lemoine.ModelDAO.IShiftDAO
  {
    #region IGenericUpdateDAO implementation
    public Lemoine.Model.IShift FindByIdAndLock(int id)
    {
      throw new NotImplementedException();
    }
    #endregion
    #region IGenericDAO implementation
    public Lemoine.Model.IShift FindById(int id)
    {
      return WebServiceHelper.UniqueResult<IShift, Shift> (new RequestUrl ("/Data/Shift/FindById/" + id));
    }
    #endregion
    #region IBaseGenericUpdateDAO implementation
    public void UpgradeLock(Lemoine.Model.IShift entity)
    {
      throw new NotImplementedException();
    }
    public Lemoine.Model.IShift Reload(Lemoine.Model.IShift entity)
    {
      throw new NotImplementedException();
    }
    #endregion
    #region IBaseGenericDAO implementation
    public System.Collections.Generic.IList<Lemoine.Model.IShift> FindAll()
    {
      throw new NotImplementedException();
    }
    public Lemoine.Model.IShift MakePersistent(Lemoine.Model.IShift entity)
    {
      throw new NotImplementedException();
    }
    public void MakeTransient(Lemoine.Model.IShift entity)
    {
      throw new NotImplementedException();
    }
    public void Lock(Lemoine.Model.IShift entity)
    {
      throw new NotImplementedException();
    }

    public Task<IList<IShift>> FindAllAsync ()
    {
      throw new NotImplementedException ();
    }

    public Task<IShift> MakePersistentAsync (IShift entity)
    {
      throw new NotImplementedException ();
    }

    public Task MakeTransientAsync (IShift entity)
    {
      throw new NotImplementedException ();
    }

    public Task LockAsync (IShift entity)
    {
      throw new NotImplementedException ();
    }

    public async Task<IShift> FindByIdAsync (int id)
    {
      return await WebServiceHelper.UniqueResultAsync<IShift, Shift> (new RequestUrl ("/Data/Shift/FindById/" + id));
    }
    #endregion
  }
}
