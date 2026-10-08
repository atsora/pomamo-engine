// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.WebClient;

namespace Lemoine.WebDataAccess
{
  /// <summary>
  /// Web implementation of IOperationDAO
  ///
  /// Only FindById is supported
  /// </summary>
  public class OperationDAO : Lemoine.ModelDAO.IOperationDAO
  {
    public IOperation FindById (int id) =>
      WebServiceHelper.UniqueResult<IOperation, Operation> (new RequestUrl ($"/Data/Operation/FindById/{id}"));

    public async Task<IOperation> FindByIdAsync (int id) =>
      await WebServiceHelper.UniqueResultAsync<IOperation, Operation> (new RequestUrl ($"/Data/Operation/FindById/{id}"));

    public IList<IOperation> FindByCode (string code) => throw new NotImplementedException ();

    public double? GetCompletion (ISequence sequence) => throw new NotImplementedException ();

    public IOperation FindByIdWithSequences (int id) => throw new NotImplementedException ();

    public void InitializeIntermediateWorkPieces (IOperation operation) => throw new NotImplementedException ();

    public IOperation Merge (IOperation oldItem, IOperation newItem, ConflictResolution conflictResolution) => throw new NotImplementedException ();

    public IOperation FindByIdAndLock (int id) => throw new NotImplementedException ();

    public void UpgradeLock (IOperation entity) => throw new NotImplementedException ();

    public IOperation Reload (IOperation entity) => throw new NotImplementedException ();

    public virtual bool IsAttachedToSession (IOperation persistent) => true;

    public IList<IOperation> FindAll () => throw new NotImplementedException ();

    public IOperation MakePersistent (IOperation entity) => throw new NotImplementedException ();

    public void MakeTransient (IOperation entity) => throw new NotImplementedException ();

    public void Lock (IOperation entity) => throw new NotImplementedException ();

    public Task<IList<IOperation>> FindAllAsync () => throw new NotImplementedException ();

    public Task<IOperation> MakePersistentAsync (IOperation entity) => throw new NotImplementedException ();

    public Task MakeTransientAsync (IOperation entity) => throw new NotImplementedException ();

    public Task LockAsync (IOperation entity) => throw new NotImplementedException ();
  }
}
