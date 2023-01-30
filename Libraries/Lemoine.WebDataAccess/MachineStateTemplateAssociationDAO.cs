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
  /// Description of MachineStateTemplateMachineAssociationDAO.
  /// </summary>
  public class MachineStateTemplateAssociationDAO: Lemoine.ModelDAO.IMachineStateTemplateAssociationDAO
  {
    #region IGenericByMachineDAO implementation
    public Lemoine.Model.IMachineStateTemplateAssociation FindById(long id, IMachine machine)
    {
      throw new NotImplementedException();
    }
    #endregion
    #region IBaseGenericDAO implementation
    public System.Collections.Generic.IList<Lemoine.Model.IMachineStateTemplateAssociation> FindAll()
    {
      throw new NotImplementedException();
    }
    public Lemoine.Model.IMachineStateTemplateAssociation MakePersistent(Lemoine.Model.IMachineStateTemplateAssociation entity)
    {
      // Only Save is valid here
      Debug.Assert (0 == ((Lemoine.Collections.IDataWithId<long>)entity).Id);
      Debug.Assert (null != entity.Machine);
      Debug.Assert (null != entity.MachineStateTemplate);
      
      RequestUrl requestUrl = new RequestUrl ("/Data/MachineStateTemplateMachineAssociation/Save");
      requestUrl.Add ("MachineId", entity.Machine.Id);
      requestUrl.Add ("Range", entity.Range.ToString ());
      if (null != entity.Revision) {
        requestUrl.Add ("RevisionId", entity.Revision.Id);
      }
      requestUrl.Add ("MachineStateTemplateId", entity.MachineStateTemplate.Id);
      if (null != entity.User) {
        requestUrl.Add ("UserId", entity.User.Id);
      }
      if (null != entity.Shift) {
        requestUrl.Add ("ShiftId", entity.Shift.Id);
      }
      requestUrl.Add ("Force", entity.Force);
      long id = WebServiceHelper.Save (requestUrl);
      ((MachineStateTemplateAssociation)entity).Id = id;
      return entity;
    }
    public void MakeTransient(Lemoine.Model.IMachineStateTemplateAssociation entity)
    {
      throw new NotImplementedException();
    }
    public void Lock(Lemoine.Model.IMachineStateTemplateAssociation entity)
    {
      throw new NotImplementedException();
    }
    public System.Collections.Generic.IList<Lemoine.Model.IMachineStateTemplateAssociation> FindByMachineAndPeriod(Lemoine.Model.IMachine machine, DateTime start, DateTime end)
    {
      throw new NotImplementedException();
    }

    public Task<IList<IMachineStateTemplateAssociation>> FindAllAsync ()
    {
      throw new NotImplementedException ();
    }

    public Task<IMachineStateTemplateAssociation> MakePersistentAsync (IMachineStateTemplateAssociation entity)
    {
      throw new NotImplementedException ();
    }

    public Task MakeTransientAsync (IMachineStateTemplateAssociation entity)
    {
      throw new NotImplementedException ();
    }

    public Task LockAsync (IMachineStateTemplateAssociation entity)
    {
      throw new NotImplementedException ();
    }
    #endregion
  }
}
