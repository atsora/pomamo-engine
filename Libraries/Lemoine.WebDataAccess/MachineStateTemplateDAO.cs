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
  /// Description of MachineStateTemplateDAO.
  /// </summary>
  public class MachineStateTemplateDAO: Lemoine.ModelDAO.IMachineStateTemplateDAO
  {
    #region IMachineStateTemplateDAO implementation
    public System.Collections.Generic.IList<IMachineStateTemplate> FindAllForConfig()
    {
      throw new NotImplementedException();
    }

    public System.Collections.Generic.IList<IMachineStateTemplate> FindByCategory(MachineStateTemplateCategory category)
    {
      return WebServiceHelper.List<IMachineStateTemplate, MachineStateTemplate> (new RequestUrl ("/Data/MachineStateTemplate/FindByCategory/" + (int)category));
    }

    #endregion
    #region IGenericUpdateDAO implementation
    public IMachineStateTemplate FindByIdAndLock(int id)
    {
      throw new NotImplementedException();
    }
    #endregion
    #region IGenericDAO implementation
    public IMachineStateTemplate FindById(int id)
    {
      return WebServiceHelper.UniqueResult<IMachineStateTemplate, MachineStateTemplate> (new RequestUrl ("/Data/MachineStateTemplate/FindById/" + id));
    }
    #endregion
    #region IBaseGenericUpdateDAO implementation
    public void UpgradeLock(IMachineStateTemplate entity)
    {
      throw new NotImplementedException();
    }
    public IMachineStateTemplate Reload(IMachineStateTemplate entity)
    {
      throw new NotImplementedException();
    }
    #endregion
    #region IBaseGenericDAO implementation
    public System.Collections.Generic.IList<IMachineStateTemplate> FindAll()
    {
      throw new NotImplementedException();
    }
    public IMachineStateTemplate MakePersistent(IMachineStateTemplate entity)
    {
      throw new NotImplementedException();
    }
    public void MakeTransient(IMachineStateTemplate entity)
    {
      throw new NotImplementedException();
    }
    public void Lock(IMachineStateTemplate entity)
    {
      throw new NotImplementedException();
    }

    public Task<IList<IMachineStateTemplate>> FindAllAsync ()
    {
      throw new NotImplementedException ();
    }

    public Task<IMachineStateTemplate> MakePersistentAsync (IMachineStateTemplate entity)
    {
      throw new NotImplementedException ();
    }

    public Task MakeTransientAsync (IMachineStateTemplate entity)
    {
      throw new NotImplementedException ();
    }

    public Task LockAsync (IMachineStateTemplate entity)
    {
      throw new NotImplementedException ();
    }

    public async Task<IMachineStateTemplate> FindByIdAsync (int id)
    {
      return await WebServiceHelper.UniqueResultAsync<IMachineStateTemplate, MachineStateTemplate> (new RequestUrl ("/Data/MachineStateTemplate/FindById/" + id));
    }
    #endregion
  }
}
