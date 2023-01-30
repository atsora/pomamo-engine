// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.WebClient;
using System.Collections.Generic;
using Lemoine.Model;
using System.Threading.Tasks;
using Lemoine.ModelDAO;

namespace Lemoine.WebDataAccess
{
  /// <summary>
  /// Description of MachineDAO.
  /// </summary>
  public class MachineDAO: Lemoine.ModelDAO.IMachineDAO
  {
    #region IMachineDAO implementation
    public IList<IMachine> FindAll(bool useCache)
    {
      throw new NotImplementedException();
    }
    public IList<IMachine> FindAllForXmlSerialization()
    {
      throw new NotImplementedException();
    }
    public IList<IMachine> FindAllWithChildren()
    {
      throw new NotImplementedException();
    }
    public IMachine FindByName(string machineName)
    {
      throw new NotImplementedException();
    }
    public IList<IMachine> FindAllOrderByName()
    {
      throw new NotImplementedException();
    }
    public IList<IMachine> FindAllNotObsolete()
    {
      throw new NotImplementedException();
    }
    public IList<IMachine> FindAllInCompany (int companyId)
    {
      throw new NotImplementedException();
    }
    public IList<IMachine> FindAllInDepartment(int departmentId)
    {
      throw new NotImplementedException();
    }
    public IList<IMachine> FindAllInMachineCategory(int machineCategoryId)
    {
      throw new NotImplementedException();
    }
    public IList<IMachine> FindAllInMachineSubCategory(int machineSubCategoryId)
    {
      throw new NotImplementedException();
    }
    public IList<IMachine> FindAllInCell(int cellId)
    {
      throw new NotImplementedException();
    }
    #endregion
    #region IGenericUpdateDAO implementation
    public IMachine FindByIdAndLock(int id)
    {
      throw new NotImplementedException();
    }
    #endregion
    #region IGenericDAO implementation
    public IMachine FindById(int id)
    {
      return WebServiceHelper.UniqueResult<IMachine, Machine> (new RequestUrl ("/Data/Machine/FindById/" + id));
    }
    #endregion
    #region IBaseGenericUpdateDAO implementation
    public void UpgradeLock(IMachine entity)
    {
      throw new NotImplementedException();
    }
    public IMachine Reload(IMachine entity)
    {
      throw new NotImplementedException();
    }
    #endregion
    #region IBaseGenericDAO implementation
    public IList<IMachine> FindAll()
    {
      throw new NotImplementedException();
    }
    public IMachine MakePersistent(IMachine entity)
    {
      throw new NotImplementedException();
    }
    public void MakeTransient(IMachine entity)
    {
      throw new NotImplementedException();
    }
    public void Lock(IMachine entity)
    {
      throw new NotImplementedException();
    }

    public Task<IList<IMachine>> FindAllAsync ()
    {
      throw new NotImplementedException ();
    }

    public Task<IMachine> MakePersistentAsync (IMachine entity)
    {
      throw new NotImplementedException ();
    }

    public Task MakeTransientAsync (IMachine entity)
    {
      throw new NotImplementedException ();
    }

    public Task LockAsync (IMachine entity)
    {
      throw new NotImplementedException ();
    }

    public async Task<IMachine> FindByIdAsync (int id)
    {
      return await WebServiceHelper.UniqueResultAsync<IMachine, Machine> (new RequestUrl ("/Data/Machine/FindById/" + id));
    }
    #endregion
  }
}
