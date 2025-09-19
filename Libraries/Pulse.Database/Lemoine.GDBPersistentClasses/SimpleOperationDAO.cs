// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System.Threading.Tasks;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Description of SimpleOperationDAO.
  /// </summary>
  public class SimpleOperationDAO
    : ISimpleOperationDAO
  {
    static readonly ILog log = LogManager.GetLogger(typeof (SimpleOperationDAO).FullName);

    public virtual bool IsAttachedToSession (Lemoine.Model.ISimpleOperation persistent) => NHibernateHelper.GetCurrentSession ().Contains (persistent.Operation);

    #region IMergeDAO implementation
    /// <summary>
    /// Merge one old SimpleOperation into a new one
    /// 
    /// This returns the merged SimpleOperation
    /// </summary>
    /// <param name="oldSimpleOperation"></param>
    /// <param name="newSimpleOperation"></param>
    /// <param name="conflictResolution"></param>
    /// <returns></returns>
    /// <exception cref="ConflictException">the two data conflict with each other and conflictResolution is Exception</exception>
    public ISimpleOperation Merge (ISimpleOperation oldSimpleOperation,
                                   ISimpleOperation newSimpleOperation,
                                   ConflictResolution conflictResolution)
    {
      if (0 == newSimpleOperation.OperationId) { // newSimpleOperation is not persistent, inverse the arguments
        Debug.Assert (0 != oldSimpleOperation.OperationId);
        ConflictResolution localConflictResolution =
          ConflictResolutionMethods.Inverse (conflictResolution);
        return InternalMerge (newSimpleOperation, oldSimpleOperation,
                              localConflictResolution);
      }
      else { // 0 != newSimpleOperation.OperationId
        return InternalMerge (oldSimpleOperation, newSimpleOperation, conflictResolution);
      }
    }

    /// <summary>
    /// Merge one old SimpleOperation into a new one
    /// 
    /// This returns the merged SimpleOperation
    /// </summary>
    /// <param name="oldSimpleOperation"></param>
    /// <param name="newSimpleOperation">persistent item</param>
    /// <param name="conflictResolution"></param>
    /// <returns></returns>
    /// <exception cref="ConflictException">the two data conflict with each other and conflictResolution is Exception</exception>
    ISimpleOperation InternalMerge (ISimpleOperation oldSimpleOperation,
                                    ISimpleOperation newSimpleOperation,
                                    ConflictResolution conflictResolution)
    {
      Debug.Assert (0 != newSimpleOperation.OperationId);

      IOperation oldOperation = oldSimpleOperation.Operation;
      IOperation newOperation = newSimpleOperation.Operation;
      IIntermediateWorkPiece oldIntermediateWorkPiece = oldSimpleOperation.IntermediateWorkPiece;
      IIntermediateWorkPiece newIntermediateWorkPiece = newSimpleOperation.IntermediateWorkPiece;
      
      // - Merge the operations
      IOperation mergedOperation = ModelDAOHelper.DAOFactory.OperationDAO
        .Merge (oldOperation, newOperation, conflictResolution);
      Debug.Assert (2 == mergedOperation.IntermediateWorkPieces.Count);

      ModelDAOHelper.DAOFactory.Flush ();
      NHibernateHelper.GetCurrentSession ().Clear ();
      
      // Note: this is not fully clear yet, why some of the lines below are necessary
      //       I thought this should have been done automatically, but there must be a reference problem somewhere
      foreach (IIntermediateWorkPiece intermediateWorkPiece in mergedOperation.IntermediateWorkPieces) {
        if (!intermediateWorkPiece.Equals (newIntermediateWorkPiece)) {
          oldIntermediateWorkPiece = intermediateWorkPiece;
          break;
        }
      }
      
      // - Merge the intermediate work pieces
      Debug.Assert (oldIntermediateWorkPiece.Operation == mergedOperation);
      Debug.Assert (newIntermediateWorkPiece.Operation == mergedOperation);
      IIntermediateWorkPiece mergedIntermediateWorkPiece = ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO
        .Merge (oldIntermediateWorkPiece, newIntermediateWorkPiece, conflictResolution);
      
      Debug.Assert (mergedIntermediateWorkPiece.Operation == mergedOperation);
      return new SimpleOperation (mergedIntermediateWorkPiece);
    }
    #endregion // IMergeDAO implementation

    /// <summary>
    /// FindById implementation
    /// </summary>
    /// <param name="intermediateWorkPieceId"></param>
    /// <returns></returns>
    public ISimpleOperation FindById (int intermediateWorkPieceId)
    {
      return FindByIntermediateWorkPieceId (intermediateWorkPieceId);
    }

    /// <summary>
    /// FindById implementation
    /// </summary>
    /// <param name="intermediateWorkPieceId"></param>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<ISimpleOperation> FindByIdAsync (int intermediateWorkPieceId)
    {
      return await FindByIntermediateWorkPieceIdAsync (intermediateWorkPieceId);
    }

    /// <summary>
    /// FindByOperationId implementation
    /// </summary>
    /// <param name="operationId"></param>
    /// <returns></returns>
    public ISimpleOperation FindByOperationId (int operationId)
    {
      IOperation operation = ModelDAOHelper.DAOFactory.OperationDAO
        .FindById (operationId);
      if (null == operation) {
        return null;
      }
      else {
        return operation.SimpleOperation;
      }
    }

    /// <summary>
    /// FindByOperationId implementation
    /// </summary>
    /// <param name="operationId"></param>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<ISimpleOperation> FindByOperationIdAsync (int operationId)
    {
      IOperation operation = await ModelDAOHelper.DAOFactory.OperationDAO
        .FindByIdAsync (operationId);
      if (null == operation) {
        return null;
      }
      else {
        return operation.SimpleOperation;
      }
    }

    /// <summary>
    /// FindByIntermediateWorkPieceId implementation
    /// </summary>
    /// <param name="intermediateWorkPieceId"></param>
    /// <returns></returns>
    public ISimpleOperation FindByIntermediateWorkPieceId (int intermediateWorkPieceId)
    {
      IIntermediateWorkPiece intermediateWorkPiece = ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO
        .FindById (intermediateWorkPieceId);
      if (null == intermediateWorkPiece) {
        return null;
      }
      else {
        return intermediateWorkPiece.SimpleOperation;
      }
    }

    /// <summary>
    /// FindByIntermediateWorkPieceId implementation
    /// </summary>
    /// <param name="intermediateWorkPieceId"></param>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<ISimpleOperation> FindByIntermediateWorkPieceIdAsync (int intermediateWorkPieceId)
    {
      IIntermediateWorkPiece intermediateWorkPiece = await ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO
        .FindByIdAsync (intermediateWorkPieceId);
      if (null == intermediateWorkPiece) {
        return null;
      }
      else {
        return intermediateWorkPiece.SimpleOperation;
      }
    }

    /// <summary>
    /// <see cref="ISimpleOperationDAO"/>
    /// </summary>
    /// <param name="component"></param>
    /// <param name="order"></param>
    /// <returns></returns>
    public IEnumerable<ISimpleOperation> FindWithOrderForComponent (IComponent component, int order)
    {
      Debug.Assert (null != component);

      var intermediateWorkPieces = ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO
        .FindWithOrderForComponent (component, order);
      if (!intermediateWorkPieces.Any ()) {
        return new List<ISimpleOperation> ();
      }
      else {
        return intermediateWorkPieces.Select (x => new SimpleOperation (x));
      }
    }

    /// <summary>
    /// FindAll implementation
    /// </summary>
    /// <returns></returns>
    public IList<ISimpleOperation> FindAll ()
    {
      IList<ISimpleOperation> simpleOperations = new List<ISimpleOperation> ();
      IList<IIntermediateWorkPiece> intermediateWorkPieces = ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.FindAll ();
      foreach (IIntermediateWorkPiece intermediateWorkPiece in intermediateWorkPieces) {
        simpleOperations.Add (intermediateWorkPiece.SimpleOperation);
      }
      return simpleOperations;
    }

    /// <summary>
    /// FindAll implementation
    /// </summary>
    /// <returns></returns>
    public async Task<IList<ISimpleOperation>> FindAllAsync ()
    {
      IList<ISimpleOperation> simpleOperations = new List<ISimpleOperation> ();
      IList<IIntermediateWorkPiece> intermediateWorkPieces = await ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.FindAllAsync ();
      foreach (IIntermediateWorkPiece intermediateWorkPiece in intermediateWorkPieces) {
        simpleOperations.Add (intermediateWorkPiece.SimpleOperation);
      }
      return simpleOperations;
    }

    /// <summary>
    /// MakeTransient implementation
    /// </summary>
    /// <param name="simpleOperation">not null</param>
    public void MakeTransient (ISimpleOperation simpleOperation)
    {
      Debug.Assert (null != simpleOperation);
      
      IIntermediateWorkPiece intermediateWorkPiece = ((SimpleOperation)simpleOperation).IntermediateWorkPiece;
      IOperation operation = intermediateWorkPiece.Operation;
      intermediateWorkPiece.Operation = null;
      ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.MakeTransient (intermediateWorkPiece);
      ModelDAOHelper.DAOFactory.OperationDAO.MakeTransient (operation);
    }

    /// <summary>
    /// MakeTransient implementation
    /// </summary>
    /// <param name="simpleOperation">not null</param>
    public async System.Threading.Tasks.Task MakeTransientAsync (ISimpleOperation simpleOperation)
    {
      Debug.Assert (null != simpleOperation);

      IIntermediateWorkPiece intermediateWorkPiece = ((SimpleOperation)simpleOperation).IntermediateWorkPiece;
      IOperation operation = intermediateWorkPiece.Operation;
      intermediateWorkPiece.Operation = null;
      await ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.MakeTransientAsync (intermediateWorkPiece);
      await ModelDAOHelper.DAOFactory.OperationDAO.MakeTransientAsync (operation);
    }

    /// <summary>
    /// Re-attach the object to the session
    /// </summary>
    /// <param name="simpleOperation">not null</param>
    public void Lock (ISimpleOperation simpleOperation)
    {
      Debug.Assert (null != simpleOperation);
      
      IIntermediateWorkPiece intermediateWorkPiece = simpleOperation.IntermediateWorkPiece;
      ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.Lock (intermediateWorkPiece);
      IOperation operation = simpleOperation.Operation;
      ModelDAOHelper.DAOFactory.OperationDAO.Lock (operation);
    }

    /// <summary>
    /// Re-attach the object to the session
    /// </summary>
    /// <param name="simpleOperation">not null</param>
    public async System.Threading.Tasks.Task LockAsync (ISimpleOperation simpleOperation)
    {
      Debug.Assert (null != simpleOperation);

      IIntermediateWorkPiece intermediateWorkPiece = simpleOperation.IntermediateWorkPiece;
      await ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.LockAsync (intermediateWorkPiece);
      IOperation operation = simpleOperation.Operation;
      await ModelDAOHelper.DAOFactory.OperationDAO.LockAsync (operation);
    }

    /// <summary>
    /// FindByIdAndLock implementation
    /// </summary>
    /// <param name="intermediateWorkPieceId"></param>
    /// <returns></returns>
    public ISimpleOperation FindByIdAndLock (int intermediateWorkPieceId)
    {
      IIntermediateWorkPiece intermediateWorkPiece = ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO
        .FindByIdAndLock (intermediateWorkPieceId);
      if (null == intermediateWorkPiece) {
        return null;
      }
      else {
        return intermediateWorkPiece.SimpleOperation;
      }
    }
    
    /// <summary>
    /// MakePersistent implementation
    /// </summary>
    /// <param name="simpleOperation">not null</param>
    /// <returns></returns>
    public ISimpleOperation MakePersistent (ISimpleOperation simpleOperation)
    {
      Debug.Assert (null != simpleOperation);
      
      ModelDAOHelper.DAOFactory.OperationDAO.MakePersistent (((SimpleOperation)simpleOperation).IntermediateWorkPiece.Operation);
      ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.MakePersistent (((SimpleOperation)simpleOperation).IntermediateWorkPiece);
      return simpleOperation;
    }

    /// <summary>
    /// MakePersistent implementation
    /// </summary>
    /// <param name="simpleOperation">not null</param>
    /// <returns></returns>
    public async Task<ISimpleOperation> MakePersistentAsync (ISimpleOperation simpleOperation)
    {
      Debug.Assert (null != simpleOperation);

      await ModelDAOHelper.DAOFactory.OperationDAO.MakePersistentAsync (((SimpleOperation)simpleOperation).IntermediateWorkPiece.Operation);
      await ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.MakePersistentAsync (((SimpleOperation)simpleOperation).IntermediateWorkPiece);
      return simpleOperation;
    }

    /// <summary>
    /// Re-attach the object to the session with an upgrade lock
    /// </summary>
    /// <param name="simpleOperation">not null</param>
    public void UpgradeLock (ISimpleOperation simpleOperation)
    {
      Debug.Assert (null != simpleOperation);
      
      IIntermediateWorkPiece intermediateWorkPiece = simpleOperation.IntermediateWorkPiece;
      ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.UpgradeLock (intermediateWorkPiece);
      IOperation operation = simpleOperation.Operation;
      ModelDAOHelper.DAOFactory.OperationDAO.UpgradeLock (operation);
    }
    
    /// <summary>
    /// Reload an entity (for example after an update operation fails)
    /// </summary>
    /// <param name="simpleOperation">not null</param>
    /// <returns></returns>
    public ISimpleOperation Reload (ISimpleOperation simpleOperation)
    {
      Debug.Assert (null != simpleOperation);
      
      IIntermediateWorkPiece intermediateWorkPiece = simpleOperation.IntermediateWorkPiece;
      intermediateWorkPiece = ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.Reload (intermediateWorkPiece);
      return intermediateWorkPiece.SimpleOperation;
    }
  }
}
