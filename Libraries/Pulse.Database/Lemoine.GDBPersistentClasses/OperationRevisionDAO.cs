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
  /// OperationRevisionDAO
  /// </summary>
  public class OperationRevisionDAO: IOperationRevisionDAO
  {
    readonly ILog log = LogManager.GetLogger (typeof (OperationRevisionDAO).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public OperationRevisionDAO ()
    {
    }

    public virtual bool IsAttachedToSession (Lemoine.Model.IOperationRevision persistent) => NHibernateHelper.GetCurrentSession ().Contains (persistent.Operation);

    /// <summary>
    /// <see cref="IOperationRevisionDAO"/>
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public IList<IOperationRevision> FindAll ()
    {
      throw new NotImplementedException ();
    }

    /// <summary>
    /// <see cref="IOperationRevisionDAO"/>
    /// </summary>
    public Task<IList<IOperationRevision>> FindAllAsync ()
    {
      throw new NotImplementedException ();
    }

    /// <summary>
    /// <see cref="IOperationRevisionDAO"/>
    /// </summary>
    public IOperationRevision FindById (int id)
    {
      // Note: the operation model id and the operation id are the same for the moment
      var operation = ModelDAOHelper.DAOFactory.OperationDAO
        .FindById (id);
      return operation.ActiveRevision;
    }

    /// <summary>
    /// <see cref="IOperationRevisionDAO"/>
    /// </summary>
    public IOperationRevision FindByIdAndLock (int id)
    {
      throw new NotImplementedException ();
    }

    /// <summary>
    /// <see cref="IOperationRevisionDAO"/>
    /// </summary>
    public Task<IOperationRevision> FindByIdAsync (int id)
    {
      throw new NotImplementedException ();
    }

    /// <summary>
    /// <see cref="IOperationRevisionDAO"/>
    /// </summary>
    public void Lock (IOperationRevision entity)
    {
      throw new NotImplementedException ();
    }

    /// <summary>
    /// <see cref="IOperationRevisionDAO"/>
    /// </summary>
    public System.Threading.Tasks.Task LockAsync (IOperationRevision entity)
    {
      throw new NotImplementedException ();
    }

    /// <summary>
    /// <see cref="IOperationRevisionDAO"/>
    /// </summary>
    public IOperationRevision MakePersistent (IOperationRevision entity)
    {
      throw new NotImplementedException ();
    }

    /// <summary>
    /// <see cref="IOperationRevisionDAO"/>
    /// </summary>
    public Task<IOperationRevision> MakePersistentAsync (IOperationRevision entity)
    {
      throw new NotImplementedException ();
    }

    /// <summary>
    /// <see cref="IOperationRevisionDAO"/>
    /// </summary>
    public void MakeTransient (IOperationRevision entity)
    {
      throw new NotImplementedException ();
    }

    /// <summary>
    /// <see cref="IOperationRevisionDAO"/>
    /// </summary>
    public System.Threading.Tasks.Task MakeTransientAsync (IOperationRevision entity)
    {
      throw new NotImplementedException ();
    }

    /// <summary>
    /// <see cref="IOperationRevisionDAO"/>
    /// </summary>
    public IOperationRevision Reload (IOperationRevision entity)
    {
      throw new NotImplementedException ();
    }

    /// <summary>
    /// <see cref="IOperationRevisionDAO"/>
    /// </summary>
    public void UpgradeLock (IOperationRevision entity)
    {
      throw new NotImplementedException ();
    }
  }
}
