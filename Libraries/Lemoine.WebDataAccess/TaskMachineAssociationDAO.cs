// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using Lemoine.Core.Log;
using Lemoine.WebClient;
using Lemoine.Model;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Lemoine.WebDataAccess
{
  /// <summary>
  /// Description of TaskMachineAssociationDAO.
  /// </summary>
  public class TaskMachineAssociationDAO: Lemoine.ModelDAO.ITaskMachineAssociationDAO
  {
    #region IGenericByMachineDAO implementation
    public Lemoine.Model.ITaskMachineAssociation FindById(long id, IMachine machine)
    {
      throw new NotImplementedException();
    }
    #endregion
    #region IBaseGenericDAO implementation
    public System.Collections.Generic.IList<Lemoine.Model.ITaskMachineAssociation> FindAll()
    {
      throw new NotImplementedException();
    }
    public Lemoine.Model.ITaskMachineAssociation MakePersistent(Lemoine.Model.ITaskMachineAssociation entity)
    {
      // Only Save is valid here
      Debug.Assert (0 == ((Lemoine.Collections.IDataWithId<long>)entity).Id);
      Debug.Assert (null != entity.Machine);
      
      RequestUrl requestUrl = new RequestUrl ("/Data/TaskMachineAssociation/Save");
      requestUrl.Add ("MachineId", entity.Machine.Id);
      requestUrl.Add ("Range", entity.Range.ToString ());
      if (null != entity.Revision) {
        requestUrl.Add ("RevisionId", entity.Revision.Id);
      }
      if (null != entity.Task) {
        requestUrl.Add ("TaskId", ((Lemoine.Collections.IDataWithId<int>)entity.Task).Id);
      }
      long id = WebServiceHelper.Save (requestUrl);
      ((TaskMachineAssociation)entity).Id = id;
      return entity;
    }
    public void MakeTransient(Lemoine.Model.ITaskMachineAssociation entity)
    {
      throw new NotImplementedException();
    }
    public void Lock(Lemoine.Model.ITaskMachineAssociation entity)
    {
      throw new NotImplementedException();
    }
    public Lemoine.Model.ITaskMachineAssociation FindMatching(Lemoine.Model.IMachine machine, Lemoine.Model.ITask task, DateTime startDate)
    {
      throw new NotImplementedException();
    }

    public Task<IList<ITaskMachineAssociation>> FindAllAsync ()
    {
      throw new NotImplementedException ();
    }

    public Task<ITaskMachineAssociation> MakePersistentAsync (ITaskMachineAssociation entity)
    {
      throw new NotImplementedException ();
    }

    public Task MakeTransientAsync (ITaskMachineAssociation entity)
    {
      throw new NotImplementedException ();
    }

    public Task LockAsync (ITaskMachineAssociation entity)
    {
      throw new NotImplementedException ();
    }
    #endregion
  }
}
