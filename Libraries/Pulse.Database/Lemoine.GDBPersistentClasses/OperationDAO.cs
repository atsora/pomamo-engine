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
using NHibernate;
using NHibernate.Criterion;
using Lemoine.Extensions.Database;
using Pulse.Extensions.Database;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IOperationDAO">IOperationDAO</see>
  /// </summary>
  public class OperationDAO
    : VersionableNHibernateDAO<Operation, IOperation, int>
    , IOperationDAO
  {
    ILog log = LogManager.GetLogger(typeof (OperationDAO).FullName);
    
    #region IMergeDAO implementation
    /// <summary>
    /// Merge one old operation into a new one
    /// 
    /// This returns the merged operation
    /// </summary>
    /// <param name="oldOperation"></param>
    /// <param name="newOperation"></param>
    /// <param name="conflictResolution"></param>
    /// <returns></returns>
    /// <exception cref="ConflictException">the two data conflict with each other and conflictResolution is Exception</exception>
    public IOperation Merge (IOperation oldOperation,
                             IOperation newOperation,
                             ConflictResolution conflictResolution)
    {
      if (0 == ((Lemoine.Collections.IDataWithId)newOperation).Id) { // newOperation is not persistent, inverse the arguments
        Debug.Assert (0 != ((Lemoine.Collections.IDataWithId)oldOperation).Id);
        ConflictResolution localConflictResolution =
          ConflictResolutionMethods.Inverse (conflictResolution);
        return InternalMerge (newOperation, oldOperation,
                              localConflictResolution);
      }
      else { // 0 != newOperation.Id
        return InternalMerge (oldOperation, newOperation, conflictResolution);
      }
    }

    /// <summary>
    /// Merge one old operation into a new one
    /// 
    /// This returns the merged operation
    /// </summary>
    /// <param name="oldOperation"></param>
    /// <param name="newOperation">persistent item</param>
    /// <param name="conflictResolution"></param>
    /// <returns></returns>
    /// <exception cref="ConflictException">the two data conflict with each other and conflictResolution is Exception</exception>
    IOperation InternalMerge (IOperation oldOperation,
                              IOperation newOperation,
                              ConflictResolution conflictResolution)
    {
      Debug.Assert (0 != ((Lemoine.Collections.IDataWithId)newOperation).Id);
      
      LockForMerge (newOperation);
      if (0 != ((Lemoine.Collections.IDataWithId)oldOperation).Id) {
        LockForMerge (oldOperation);
      }
      
      ((Operation)newOperation).Merge (oldOperation, conflictResolution);
      ModelDAOHelper.DAOFactory.OperationDAO.MakePersistent (newOperation);
      foreach (IIntermediateWorkPiece intermediateWorkPiece in newOperation.IntermediateWorkPieces) {
        ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.MakePersistent (intermediateWorkPiece);
      }
      
      if (0 != ((Lemoine.Collections.IDataWithId)oldOperation).Id) { // Note: only for persistent classes, not transient ones
        // Merge the data in all the impacted tables
        ISession session = NHibernateHelper.GetCurrentSession (); // Temporary
        // - Data
        IList<ICadModel> cadModels = session.CreateCriteria<CadModel> ()
          .Add (Restrictions.Eq ("Operation", oldOperation))
          .List<ICadModel> ();
        foreach (ICadModel cadModel in cadModels) {
          cadModel.Operation = newOperation;
        }
        IList<IStamp> stamps = session.CreateCriteria<Stamp> ()
          .Add (Restrictions.Eq ("Operation", oldOperation))
          .List<IStamp> ();
        foreach (IStamp stamp in stamps) {
          stamp.Operation = newOperation;
        }
        IList<ILineMachine> lineMachines = ModelDAOHelper.DAOFactory.LineMachineDAO
          .FindAllByOperation (oldOperation);
        foreach (ILineMachine lineMachine in lineMachines) {
          log.DebugFormat ("Merge: " +
                           "in LineMachine {0}, replace operation {1} by {2}",
                           lineMachine, oldOperation, newOperation);
          lineMachine.Operation = newOperation;
        }
        IList<ITask> tasks = ModelDAOHelper.DAOFactory.TaskDAO
          .FindAllByOperation (oldOperation);
        foreach (ITask task in tasks) {
          ((Task)task).Operation = newOperation;
        }
        // TODO: Operation Goal
        // - Modification
        IList<IOperationMachineAssociation> operationMachineAssociations =
          session.CreateCriteria<OperationMachineAssociation> ()
          .Add (Restrictions.Eq ("Operation", oldOperation))
          .List<IOperationMachineAssociation> ();
        foreach (IOperationMachineAssociation operationMachineAssociation
                 in operationMachineAssociations) {
          operationMachineAssociation.Operation = newOperation;
        }
        IList<IIntermediateWorkPieceOperationUpdate> newIntermediateWorkPieceOperationUpdates =
          session.CreateCriteria<IntermediateWorkPieceOperationUpdate> ()
          .Add (Restrictions.Eq ("NewOperation", oldOperation))
          .List<IIntermediateWorkPieceOperationUpdate> ();
        foreach (IIntermediateWorkPieceOperationUpdate intermediateWorkPieceOperationUpdate
                 in newIntermediateWorkPieceOperationUpdates) {
          intermediateWorkPieceOperationUpdate.NewOperation = newOperation;
        }
        IList<IIntermediateWorkPieceOperationUpdate> oldIntermediateWorkPieceOperationUpdates =
          session.CreateCriteria<IntermediateWorkPieceOperationUpdate> ()
          .Add (Restrictions.Eq ("OldOperation", oldOperation))
          .List<IIntermediateWorkPieceOperationUpdate> ();
        foreach (IIntermediateWorkPieceOperationUpdate intermediateWorkPieceOperationUpdate
                 in oldIntermediateWorkPieceOperationUpdates) {
          intermediateWorkPieceOperationUpdate.OldOperation = newOperation;
        }
        // - Analysis
        IList<IOperationSlot> operationSlots =
          ModelDAO.ModelDAOHelper.DAOFactory.OperationSlotDAO
          .FindByOperation (oldOperation);
        foreach (IOperationSlot operationSlot in operationSlots) {
          ((OperationSlot)operationSlot).Operation = newOperation;
        }
        
        var extensions = Lemoine.Business.ServiceProvider
          .Get (new Lemoine.Business.Extension.GlobalExtensions<IOperationExtension> ());
        foreach (var extension in extensions) {
          extension.Merge (oldOperation, newOperation);
        }

        // Modifications
        // There is no need to add some Modification
        // OperationSequenceUpdate / IntermediateWorkPieceOperation /
        // StampUpdate rows,
        // because the data is automatically updated in the analysis tables above
        
        // Flush the database before deleting the operation
        // because of some foreign key constraints
        ModelDAOHelper.DAOFactory.FlushData ();
        
        // Delete the old operation
        MakeTransient (oldOperation);
      }
      
      return newOperation;
    }

    /// <summary>
    /// Initialize the associated intermediate workpieces
    /// </summary>
    /// <param name="operation"></param>
    public void InitializeIntermediateWorkPieces (IOperation operation)
    {
      NHibernateUtil.Initialize (operation.IntermediateWorkPieces);
    }

    /// <summary>
    /// Lock an operation for merge
    /// </summary>
    internal protected void LockForMerge (IOperation operation)
    {
      UpgradeLock (operation);
      NHibernateUtil.Initialize (operation.Type);
      NHibernateUtil.Initialize (operation.IntermediateWorkPieces);
    }
    #endregion // IMergeDAO implementation
    
    /// <summary>
    /// Reload an entity (for example after an update operation fails or because it was changed somewhere else)
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override IOperation Reload (IOperation entity)
    {
      IOperation result = base.Reload (entity);
      NHibernateUtil.Initialize (result.Type);
      return result;
    }
    
    /// <summary>
    /// Find all the operations from a given operation code
    /// </summary>
    /// <param name="code">not null</param>
    /// <returns></returns>
    public IList<IOperation> FindByCode (string code)
    {
      Debug.Assert (null != code);
      
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Operation> ()
        .Add (Restrictions.Eq ("Code", code))
        .List<IOperation> ();
    }
    
    /// <summary>
    /// Return the completion of an operation when the specified sequence starts
    /// </summary>
    /// <param name="sequence">not null</param>
    /// <returns></returns>
    public double? GetCompletion (ISequence sequence)
    {
      Debug.Assert (null != sequence);
      
      // - Try first with the cache
      double? completion;
      if (OperationCompletionCache.TryGetValue (sequence, out completion)) {
        return completion;
      }
      
      IOperation operation = sequence.Operation;
      Debug.Assert (null != operation);
      
      // TODO: option to select the right method
      
      // - 1st method: using the machining duration and sequence duration
      if (operation.MachiningDuration.HasValue) {
        if (0 == operation.MachiningDuration.Value.Ticks) {
          log.Warn ($"GetCompletion: no machining duration for {operation} => return null");
          return null;
        }
        IEnumerable<ISequence> sequences = operation.Sequences;
        var sequencesBefore = sequences
          .Where (seq => (seq.Order < sequence.Order) && object.Equals (seq.Path, sequence.Path));
        if (sequencesBefore.Any(seq => !seq.EstimatedTime.HasValue)) {
          log.WarnFormat("GetCompletion: " +
          "there are sequences before {0} for operation {1} that have no estimated time",
            sequence, operation);
        }
        TimeSpan elapsedTime = sequencesBefore
          .Where (seq => seq.EstimatedTime.HasValue)
          .Aggregate (TimeSpan.FromSeconds (0), (total, next) => total.Add (sequence.EstimatedTime.Value));
        completion = elapsedTime.TotalSeconds / operation.MachiningDuration.Value.TotalSeconds;
        OperationCompletionCache.Add (sequence, completion);
        return completion;
      }
      
      // - TODO: 2nd method: using the stamps and the line number in the file
      
      return null;
    }

    /// <summary>
    /// Find the operation with the specified id with an eager fetch of its sequences
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public IOperation FindByIdWithSequences (int id)
    {
      Debug.Assert (0 != id);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Operation> ()
        .Add (Restrictions.IdEq (id))
        .Fetch (SelectMode.Fetch, "Sequences")
        // .SetCacheable (true) // SetCacheable is not behaving well with FetchMode.Eager
        .UniqueResult<IOperation> ();
    }
  }
}
