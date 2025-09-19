// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// OperationModelDAO
  /// </summary>
  public class OperationModelDAO: IOperationModelDAO
  {
    readonly ILog log = LogManager.GetLogger (typeof (OperationModelDAO).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public OperationModelDAO ()
    {
    }

    public virtual bool IsAttachedToSession (Lemoine.Model.IOperationModel persistent) => NHibernateHelper.GetCurrentSession ().Contains (persistent.Operation);

    /// <summary>
    /// <see cref="IOperationModelDAO"/>
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public IList<IOperationModel> FindAll ()
    {
      throw new NotImplementedException ();
    }

    /// <summary>
    /// <see cref="IOperationModelDAO"/>
    /// </summary>
    public Task<IList<IOperationModel>> FindAllAsync ()
    {
      throw new NotImplementedException ();
    }

    /// <summary>
    /// <see cref="IOperationModelDAO"/>
    /// </summary>
    public IOperationModel FindById (int id)
    {
      // Note: for the moment, the id of the operation model and of the operation are the same
      var operation = ModelDAOHelper.DAOFactory.OperationDAO
        .FindById (id);
      return new OperationModel (operation);
    }

    /// <summary>
    /// <see cref="IOperationModelDAO"/>
    /// </summary>
    public IOperationModel FindByIdAndLock (int id)
    {
      throw new NotImplementedException ();
    }

    /// <summary>
    /// <see cref="IOperationModelDAO"/>
    /// </summary>
    public Task<IOperationModel> FindByIdAsync (int id)
    {
      throw new NotImplementedException ();
    }

    /// <summary>
    /// <see cref="IOperationModelDAO"/>
    /// </summary>
    public void Lock (IOperationModel entity)
    {
      throw new NotImplementedException ();
    }

    /// <summary>
    /// <see cref="IOperationModelDAO"/>
    /// </summary>
    public System.Threading.Tasks.Task LockAsync (IOperationModel entity)
    {
      throw new NotImplementedException ();
    }

    /// <summary>
    /// <see cref="IOperationModelDAO"/>
    /// </summary>
    public IOperationModel MakePersistent (IOperationModel entity)
    {
      throw new NotImplementedException ();
    }

    /// <summary>
    /// <see cref="IOperationModelDAO"/>
    /// </summary>
    public Task<IOperationModel> MakePersistentAsync (IOperationModel entity)
    {
      throw new NotImplementedException ();
    }

    /// <summary>
    /// <see cref="IOperationModelDAO"/>
    /// </summary>
    public void MakeTransient (IOperationModel entity)
    {
      throw new NotImplementedException ();
    }

    /// <summary>
    /// <see cref="IOperationModelDAO"/>
    /// </summary>
    public System.Threading.Tasks.Task MakeTransientAsync (IOperationModel entity)
    {
      throw new NotImplementedException ();
    }

    /// <summary>
    /// <see cref="IOperationModelDAO"/>
    /// </summary>
    public IOperationModel Reload (IOperationModel entity)
    {
      throw new NotImplementedException ();
    }

    /// <summary>
    /// <see cref="IOperationModelDAO"/>
    /// </summary>
    public void UpgradeLock (IOperationModel entity)
    {
      throw new NotImplementedException ();
    }
  }
}
