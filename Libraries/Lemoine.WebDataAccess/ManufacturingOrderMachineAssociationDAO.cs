// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
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
  /// Description of ManufacturingOrderMachineAssociationDAO.
  /// </summary>
  public class ManufacturingOrderMachineAssociationDAO: Lemoine.ModelDAO.IManufacturingOrderMachineAssociationDAO
  {
    #region IGenericByMachineDAO implementation
    public Lemoine.Model.IManufacturingOrderMachineAssociation FindById(long id, IMachine machine)
    {
      throw new NotImplementedException();
    }
    #endregion
    #region IBaseGenericDAO implementation
    public System.Collections.Generic.IList<Lemoine.Model.IManufacturingOrderMachineAssociation> FindAll()
    {
      throw new NotImplementedException();
    }
    public Lemoine.Model.IManufacturingOrderMachineAssociation MakePersistent(Lemoine.Model.IManufacturingOrderMachineAssociation entity)
    {
      // Only Save is valid here
      Debug.Assert (0 == ((Lemoine.Collections.IDataWithId<long>)entity).Id);
      Debug.Assert (null != entity.Machine);
      
      RequestUrl requestUrl = new RequestUrl ("/Data/ManufacturingOrderMachineAssociation/Save");
      requestUrl.Add ("MachineId", entity.Machine.Id);
      requestUrl.Add ("Range", entity.Range.ToString ());
      if (null != entity.Revision) {
        requestUrl.Add ("RevisionId", entity.Revision.Id);
      }
      if (null != entity.ManufacturingOrder) {
        requestUrl.Add ("ManufacturingOrderId", ((Lemoine.Collections.IDataWithId<int>)entity.ManufacturingOrder).Id);
      }
      long id = WebServiceHelper.Save (requestUrl);
      ((ManufacturingOrderMachineAssociation)entity).Id = id;
      return entity;
    }
    public void MakeTransient(Lemoine.Model.IManufacturingOrderMachineAssociation entity)
    {
      throw new NotImplementedException();
    }
    public void Lock(Lemoine.Model.IManufacturingOrderMachineAssociation entity)
    {
      throw new NotImplementedException();
    }
    public Lemoine.Model.IManufacturingOrderMachineAssociation FindMatching(Lemoine.Model.IMachine machine, Lemoine.Model.IManufacturingOrder manufacturingOrder, DateTime startDate)
    {
      throw new NotImplementedException();
    }

    public Task<IList<IManufacturingOrderMachineAssociation>> FindAllAsync ()
    {
      throw new NotImplementedException ();
    }

    public Task<IManufacturingOrderMachineAssociation> MakePersistentAsync (IManufacturingOrderMachineAssociation entity)
    {
      throw new NotImplementedException ();
    }

    public Task MakeTransientAsync (IManufacturingOrderMachineAssociation entity)
    {
      throw new NotImplementedException ();
    }

    public Task LockAsync (IManufacturingOrderMachineAssociation entity)
    {
      throw new NotImplementedException ();
    }

    public bool IsAttachedToSession (IManufacturingOrderMachineAssociation persistent) => true;
    #endregion
  }
}
