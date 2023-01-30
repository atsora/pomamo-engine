// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lemoine.Model;
using System.Diagnostics;
using Lemoine.WebClient;
using System.Threading.Tasks;

namespace Lemoine.WebDataAccess
{
  /// <summary>
  /// Description of ComponentMachineAssociationDAO.
  /// </summary>
  public class ComponentMachineAssociationDAO: Lemoine.ModelDAO.IComponentMachineAssociationDAO
  {
    #region IGenericByMachineDAO implementation
    public Lemoine.Model.IComponentMachineAssociation FindById (long id, IMachine machine)
    {
      throw new NotImplementedException ();
    }
    #endregion
    #region IBaseGenericDAO implementation
    public System.Collections.Generic.IList<Lemoine.Model.IComponentMachineAssociation> FindAll ()
    {
      throw new NotImplementedException ();
    }
    public Lemoine.Model.IComponentMachineAssociation MakePersistent (Lemoine.Model.IComponentMachineAssociation entity)
    {
      // Only Save is valid here
      Debug.Assert (0 == ((Lemoine.Collections.IDataWithId<long>)entity).Id);
      Debug.Assert (null != entity.Machine);

      var requestUrl = new RequestUrl ("/Data/ComponentMachineAssociation/Save");
      requestUrl.Add ("MachineId", entity.Machine.Id);
      requestUrl.Add ("Range", entity.Range.ToString ());
      if (null != entity.Revision) {
        requestUrl.Add ("RevisionId", entity.Revision.Id);
      }
      if (null != entity.Component) {
        requestUrl.Add ("ComponentId", ((Lemoine.Collections.IDataWithId<int>)entity.Component).Id);
      }
      long id = WebServiceHelper.Save (requestUrl);
      ((ComponentMachineAssociation)entity).Id = id;
      return entity;
    }
    public void MakeTransient (Lemoine.Model.IComponentMachineAssociation entity)
    {
      throw new NotImplementedException ();
    }
    public void Lock (Lemoine.Model.IComponentMachineAssociation entity)
    {
      throw new NotImplementedException ();
    }
    public Lemoine.Model.IComponentMachineAssociation FindMatching (Lemoine.Model.IMachine machine, Lemoine.Model.IComponent component, DateTime startDate)
    {
      throw new NotImplementedException ();
    }

    public IList<IComponentMachineAssociation> FindAllWithComponent (IComponent component)
    {
      throw new NotImplementedException ();
    }

    public Task<IList<IComponentMachineAssociation>> FindAllAsync ()
    {
      throw new NotImplementedException ();
    }

    public Task<IComponentMachineAssociation> MakePersistentAsync (IComponentMachineAssociation entity)
    {
      throw new NotImplementedException ();
    }

    public Task MakeTransientAsync (IComponentMachineAssociation entity)
    {
      throw new NotImplementedException ();
    }

    public Task LockAsync (IComponentMachineAssociation entity)
    {
      throw new NotImplementedException ();
    }
    #endregion
  }
}
