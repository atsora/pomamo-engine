// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Lemoine.Model;
using Lemoine.WebClient;

namespace Lemoine.WebDataAccess
{
  /// <summary>
  /// Web implementation of IOperationMachineAssociationDAO
  ///
  /// Only MakePersistent (save of a new association) is supported
  /// </summary>
  public class OperationMachineAssociationDAO : Lemoine.ModelDAO.IOperationMachineAssociationDAO
  {
    public IOperationMachineAssociation FindById (long id, IMachine machine) => throw new NotImplementedException ();

    public virtual bool IsAttachedToSession (IOperationMachineAssociation persistent) => true;

    public IList<IOperationMachineAssociation> FindAll () => throw new NotImplementedException ();

    public IOperationMachineAssociation MakePersistent (IOperationMachineAssociation entity)
    {
      // Only Save is valid here
      Debug.Assert (0 == ((Lemoine.Collections.IDataWithId<long>)entity).Id);
      Debug.Assert (null != entity.Machine);

      var requestUrl = new RequestUrl ("/Data/OperationMachineAssociation/Save");
      requestUrl.Add ("MachineId", entity.Machine.Id);
      requestUrl.Add ("Range", entity.Range.ToString ());
      if (entity.Revision != null) {
        requestUrl.Add ("RevisionId", entity.Revision.Id);
      }
      if (entity.Operation != null) {
        requestUrl.Add ("OperationId", ((Lemoine.Collections.IDataWithId<int>)entity.Operation).Id);
      }
      long id = WebServiceHelper.Save (requestUrl);
      ((OperationMachineAssociation)entity).Id = id;
      return entity;
    }

    public void MakeTransient (IOperationMachineAssociation entity) => throw new NotImplementedException ();

    public void Lock (IOperationMachineAssociation entity) => throw new NotImplementedException ();

    public Task<IList<IOperationMachineAssociation>> FindAllAsync () => throw new NotImplementedException ();

    public Task<IOperationMachineAssociation> MakePersistentAsync (IOperationMachineAssociation entity) => throw new NotImplementedException ();

    public Task MakeTransientAsync (IOperationMachineAssociation entity) => throw new NotImplementedException ();

    public Task LockAsync (IOperationMachineAssociation entity) => throw new NotImplementedException ();
  }
}
