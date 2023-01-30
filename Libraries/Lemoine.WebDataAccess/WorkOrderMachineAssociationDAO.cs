// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using Lemoine.WebClient;
using Lemoine.Core.Log;
using Lemoine.Model;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Lemoine.WebDataAccess
{
  /// <summary>
  /// Description of WorkOrderMachineAssociationDAO.
  /// </summary>
  public class WorkOrderMachineAssociationDAO: Lemoine.ModelDAO.IWorkOrderMachineAssociationDAO
  {
    #region IGenericByMachineDAO implementation
    public Lemoine.Model.IWorkOrderMachineAssociation FindById(long id, IMachine machine)
    {
      throw new NotImplementedException();
    }
    #endregion
    #region IBaseGenericDAO implementation
    public System.Collections.Generic.IList<Lemoine.Model.IWorkOrderMachineAssociation> FindAll()
    {
      throw new NotImplementedException();
    }
    public Lemoine.Model.IWorkOrderMachineAssociation MakePersistent(Lemoine.Model.IWorkOrderMachineAssociation entity)
    {
      // Only Save is valid here
      Debug.Assert (0 == ((Lemoine.Collections.IDataWithId<long>)entity).Id);
      Debug.Assert (null != entity.Machine);
      
      RequestUrl requestUrl = new RequestUrl ("/Data/WorkOrderMachineAssociation/Save");
      requestUrl.Add ("MachineId", entity.Machine.Id);
      requestUrl.Add ("Range", entity.Range.ToString ());
      if (null != entity.Revision) {
        requestUrl.Add ("RevisionId", entity.Revision.Id);
      }
      if (null != entity.WorkOrder) {
        requestUrl.Add ("WorkOrderId", ((Lemoine.Collections.IDataWithId<int>)entity.WorkOrder).Id);
      }
      if (entity.ResetTask.HasValue) {
        requestUrl.Add ("ResetTask", entity.ResetTask.Value);
      }
      long id = WebServiceHelper.Save (requestUrl);
      ((WorkOrderMachineAssociation)entity).Id = id;
      return entity;
    }
    public void MakeTransient(Lemoine.Model.IWorkOrderMachineAssociation entity)
    {
      throw new NotImplementedException();
    }
    public void Lock(Lemoine.Model.IWorkOrderMachineAssociation entity)
    {
      throw new NotImplementedException();
    }
    public Lemoine.Model.IWorkOrderMachineAssociation FindMatching(Lemoine.Model.IMachine machine, Lemoine.Model.IWorkOrder workOrder, DateTime startDate)
    {
      throw new NotImplementedException();
    }

    public Task<IList<IWorkOrderMachineAssociation>> FindAllAsync ()
    {
      throw new NotImplementedException ();
    }

    public Task<IWorkOrderMachineAssociation> MakePersistentAsync (IWorkOrderMachineAssociation entity)
    {
      throw new NotImplementedException ();
    }

    public Task MakeTransientAsync (IWorkOrderMachineAssociation entity)
    {
      throw new NotImplementedException ();
    }

    public Task LockAsync (IWorkOrderMachineAssociation entity)
    {
      throw new NotImplementedException ();
    }
    #endregion
  }
}
