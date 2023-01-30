// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using Lemoine.Model;
using Lemoine.WebClient;
using Lemoine.Core.Log;
using System.Threading.Tasks;
using System.Collections.Generic;
using Lemoine.ModelDAO;

namespace Lemoine.WebDataAccess
{
  /// <summary>
  /// Description of RevisionDAO.
  /// </summary>
  public class RevisionDAO: Lemoine.ModelDAO.IRevisionDAO
  {
    #region IGenericDAO implementation
    public Lemoine.Model.IRevision FindById(int id)
    {
      throw new NotImplementedException();
    }
    #endregion
    #region IBaseGenericDAO implementation
    public System.Collections.Generic.IList<Lemoine.Model.IRevision> FindAll()
    {
      throw new NotImplementedException();
    }
    public Lemoine.Model.IRevision MakePersistent(Lemoine.Model.IRevision entity)
    {
      // Only Save is valid here
      Debug.Assert (0 == entity.Id);
      
      RequestUrl requestUrl = new RequestUrl ("/Data/Revision/Save");
      if (!string.IsNullOrEmpty (entity.Application)) {
        requestUrl.Add ("Application", entity.Application);
      }
      if (!string.IsNullOrEmpty (entity.IPAddress)) {
        requestUrl.Add ("IPAddress", entity.IPAddress);
      }
      if (!string.IsNullOrEmpty (entity.Comment)) {
        requestUrl.Add ("Comment", entity.Comment);
      }
      if (null != entity.Updater) {
        if (entity.Updater is IUser) {
          requestUrl.Add ("UserId", entity.Updater.Id);
        }
        else if (entity.Updater is IService) {
          requestUrl.Add ("ServiceId", entity.Updater.Id);
        }
      }
      int id = WebServiceHelper.Save (requestUrl);
      ((Revision)entity).Id = id;
      return entity;
    }
    public void MakeTransient(Lemoine.Model.IRevision entity)
    {
      throw new NotImplementedException();
    }
    public void Lock(Lemoine.Model.IRevision entity)
    {
      throw new NotImplementedException();
    }

    public Task<IList<IRevision>> FindAllAsync ()
    {
      throw new NotImplementedException ();
    }

    public Task<IRevision> MakePersistentAsync (IRevision entity)
    {
      throw new NotImplementedException ();
    }

    public Task MakeTransientAsync (IRevision entity)
    {
      throw new NotImplementedException ();
    }

    public Task LockAsync (IRevision entity)
    {
      throw new NotImplementedException ();
    }

    public Task<IRevision> FindByIdAsync (int id)
    {
      throw new NotImplementedException ();
    }
    #endregion
  }
}
