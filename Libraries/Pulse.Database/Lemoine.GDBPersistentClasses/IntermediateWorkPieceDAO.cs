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
using Lemoine.Collections;
using Pulse.Extensions.Database;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Description of IntermediateWorkPieceDAO.
  /// </summary>
  
  public class IntermediateWorkPieceDAO
    : VersionableNHibernateDAO<IntermediateWorkPiece, IIntermediateWorkPiece, int>
    , IIntermediateWorkPieceDAO
  {
    static readonly ILog log = LogManager.GetLogger(typeof (IntermediateWorkPieceDAO).FullName);

    #region IMergeDAO implementation
    /// <summary>
    /// Merge one old IntermediateWorkPiece into a new one
    /// 
    /// This returns the merged IntermediateWorkPiece
    /// </summary>
    /// <param name="oldIntermediateWorkPiece"></param>
    /// <param name="newIntermediateWorkPiece"></param>
    /// <param name="conflictResolution"></param>
    /// <returns></returns>
    /// <exception cref="ConflictException">the two data conflict with each other and conflictResolution is Exception</exception>
    public IIntermediateWorkPiece Merge (IIntermediateWorkPiece oldIntermediateWorkPiece,
                                         IIntermediateWorkPiece newIntermediateWorkPiece,
                                         ConflictResolution conflictResolution)
    {
      if (0 == ((Lemoine.Collections.IDataWithId)newIntermediateWorkPiece).Id) { // newIntermediateWorkPiece is not persistent, inverse the arguments
        Debug.Assert (0 != ((Lemoine.Collections.IDataWithId)oldIntermediateWorkPiece).Id);
        ConflictResolution localConflictResolution =
          ConflictResolutionMethods.Inverse (conflictResolution);
        return InternalMerge (newIntermediateWorkPiece, oldIntermediateWorkPiece,
                              localConflictResolution);
      }
      else { // 0 != newIntermediateWorkPiece.Id
        return InternalMerge (oldIntermediateWorkPiece, newIntermediateWorkPiece, conflictResolution);
      }
    }

    /// <summary>
    /// Merge one old IntermediateWorkPiece into a new one
    /// 
    /// This returns the merged IntermediateWorkPiece
    /// </summary>
    /// <param name="oldIntermediateWorkPiece"></param>
    /// <param name="newIntermediateWorkPiece">persistent item</param>
    /// <param name="conflictResolution"></param>
    /// <returns></returns>
    /// <exception cref="ConflictException">the two data conflict with each other and conflictResolution is Exception</exception>
    IIntermediateWorkPiece InternalMerge (IIntermediateWorkPiece oldIntermediateWorkPiece,
                                          IIntermediateWorkPiece newIntermediateWorkPiece,
                                          ConflictResolution conflictResolution)
    {
      Debug.Assert (0 != ((Lemoine.Collections.IDataWithId)newIntermediateWorkPiece).Id);
      
      LockForMerge (newIntermediateWorkPiece);
      if (0 != ((Lemoine.Collections.IDataWithId)oldIntermediateWorkPiece).Id) {
        LockForMerge (oldIntermediateWorkPiece);
      }
      
      // To be merged, oldIntermediateWorkPiece and newIntermediateWorkPiece must be made by the same operation
      if (!object.Equals (newIntermediateWorkPiece.Operation, oldIntermediateWorkPiece.Operation)) {
        log.ErrorFormat ("InternalMerge: " +
                         "trying to merge two new Intermediate Work Pieces that are made by two different operations " +
                         "old={0} new={1}",
                         oldIntermediateWorkPiece, newIntermediateWorkPiece);
        throw new InvalidOperationException ("The two intermediate work pieces must be made by the same operation to be merged"); // TODO: i18n
      }
      
      Debug.Assert (newIntermediateWorkPiece is IntermediateWorkPiece);
      (newIntermediateWorkPiece as IntermediateWorkPiece).Merge (oldIntermediateWorkPiece as IntermediateWorkPiece, conflictResolution);
      
      if (0 != ((Lemoine.Collections.IDataWithId)oldIntermediateWorkPiece).Id) { // Note: only for persistent classes, not transient ones
        // Merge the data in all the impacted tables
        ISession session = NHibernateHelper.GetCurrentSession ();
        // - Data
        IList<IComponent> finalWorkPieces =
          session.CreateCriteria<Component> ()
          .Add (Expression.Eq ("FinalWorkPiece", oldIntermediateWorkPiece))
          .List<IComponent> ();
        foreach (IComponent finalWorkPiece in finalWorkPieces) {
          finalWorkPiece.FinalWorkPiece = newIntermediateWorkPiece;
        }
        // - Modification
        IList<IComponentIntermediateWorkPieceUpdate> componentIntermediateWorkPieceUpdates =
          ModelDAOHelper.DAOFactory.ComponentIntermediateWorkPieceUpdateDAO
          .FindAllWithIntermediateWorkPiece (oldIntermediateWorkPiece);
        foreach (IComponentIntermediateWorkPieceUpdate componentIntermediateWorkPieceUpdate
                 in componentIntermediateWorkPieceUpdates) {
          componentIntermediateWorkPieceUpdate.IntermediateWorkPiece = newIntermediateWorkPiece;
          ModelDAOHelper.DAOFactory.ComponentIntermediateWorkPieceUpdateDAO
            .MakePersistent (componentIntermediateWorkPieceUpdate);
        }
        IList<IIntermediateWorkPieceOperationUpdate> intermediateWorkPieceOperationUpdates =
          session.CreateCriteria<IntermediateWorkPieceOperationUpdate> ()
          .Add (Expression.Eq ("IntermediateWorkPiece", oldIntermediateWorkPiece))
          .List<IIntermediateWorkPieceOperationUpdate> ();
        foreach (IIntermediateWorkPieceOperationUpdate intermediateWorkPieceOperationUpdate
                 in intermediateWorkPieceOperationUpdates) {
          intermediateWorkPieceOperationUpdate.IntermediateWorkPiece = newIntermediateWorkPiece;
          ModelDAOHelper.DAOFactory.IntermediateWorkPieceOperationUpdateDAO
            .MakePersistent (intermediateWorkPieceOperationUpdate);
        }
        // TODO: IntermediateWorkPieceInformation
        // - Analysis
        
        { // - Update IntermediateWorkPieceTarget
          IList<IIntermediateWorkPieceTarget> iwpTargets =
            ModelDAOHelper.DAOFactory.IntermediateWorkPieceTargetDAO
            .FindByIntermediateWorkPiece (oldIntermediateWorkPiece);
          foreach (IIntermediateWorkPieceTarget iwpTarget in iwpTargets) {
            ModelDAOHelper.DAOFactory.IntermediateWorkPieceTargetDAO
              .MakeTransient (iwpTarget);
            IIntermediateWorkPieceTarget existing =
              ModelDAOHelper.DAOFactory.IntermediateWorkPieceTargetDAO
              .FindByKey (newIntermediateWorkPiece, iwpTarget.Component, iwpTarget.WorkOrder, iwpTarget.Line, iwpTarget.Day, iwpTarget.Shift);
            if (null != existing) {
              existing.Number += iwpTarget.Number;
              ModelDAOHelper.DAOFactory.IntermediateWorkPieceTargetDAO
                .MakePersistent (existing);
            }
            else {
              IIntermediateWorkPieceTarget created =
                ModelDAOHelper.ModelFactory.CreateIntermediateWorkPieceTarget (newIntermediateWorkPiece,
                                                                                iwpTarget.Component,
                                                                                iwpTarget.WorkOrder,
                                                                                iwpTarget.Line,
                                                                                iwpTarget.Day,
                                                                                iwpTarget.Shift);
              created.Number = iwpTarget.Number;
              ModelDAOHelper.DAOFactory.IntermediateWorkPieceTargetDAO
                .MakePersistent (created);
            }
          }
        }
        
        var extensions = Lemoine.Business.ServiceProvider
          .Get (new Lemoine.Business.Extension.GlobalExtensions<IIntermediateWorkPieceExtension> ());
        foreach (var extension in extensions) {
          extension.Merge (oldIntermediateWorkPiece, newIntermediateWorkPiece);
        }

        // Modifications
        // There is no need to add some Modification
        // IntermediateWorkPieceOperationUpdate,
        // because the data is automatically updated in the analysis tables above

        // Delete the old intermediate work piece
        // Flush the data first because of some foreign keys
        ModelDAOHelper.DAOFactory.FlushData ();
        ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.MakeTransient (oldIntermediateWorkPiece);
      }
      
      return newIntermediateWorkPiece;
    }
    
    /// <summary>
    /// Lock a IntermediateWorkPiece for merge
    /// </summary>
    protected internal void LockForMerge (IIntermediateWorkPiece intermediateWorkPiece)
    {
      UpgradeLock (intermediateWorkPiece);
      NHibernateUtil.Initialize (intermediateWorkPiece.PossibleNextOperations);
      NHibernateUtil.Initialize (intermediateWorkPiece.ComponentIntermediateWorkPieces);
      foreach (IComponentIntermediateWorkPiece componentIntermediateWorkPiece in intermediateWorkPiece.ComponentIntermediateWorkPieces) {
        IComponent component = componentIntermediateWorkPiece.Component;
        ModelDAOHelper.DAOFactory.ComponentDAO.Lock (component);
      }
    }
    #endregion // IMergeDAO implementation

    /// <summary>
    /// Get orphans IntermediateWorkPiece, means IntermediateWorkPiece without link to Component
    /// </summary>
    /// <returns></returns>
    public IList<IIntermediateWorkPiece> GetOrphans()
    {
      return NHibernateHelper.GetCurrentSession().CreateCriteria<IntermediateWorkPiece>()
        .Add(Restrictions.IsEmpty("ComponentIntermediateWorkPieces"))
        .List<IIntermediateWorkPiece>();
    }
    
    /// <summary>
    /// Initialize the possible next operations
    /// </summary>
    /// <param name="intermediateWorkPiece"></param>
    public void InitializePossibleNextOperations (IIntermediateWorkPiece intermediateWorkPiece)
    {
      NHibernateUtil.Initialize (intermediateWorkPiece.PossibleNextOperations);
    }
    
    /// <summary>
    /// Find all ipw by operation
    /// </summary>
    /// <param name="operation">not null</param>
    /// <returns></returns>
    public IList<IIntermediateWorkPiece> FindByOperation(IOperation operation)
    {
      Debug.Assert (null != operation);

      return NHibernateHelper.GetCurrentSession().CreateCriteria<IntermediateWorkPiece>()
        .Add(Restrictions.Eq ("Operation.Id", ((IDataWithId)operation).Id))
        .List<IIntermediateWorkPiece>();
    }

    /// <summary>
    /// <see cref="IIntermediateWorkPieceDAO"/>
    /// </summary>
    /// <param name="component"></param>
    /// <param name="order"></param>
    /// <returns></returns>
    public IEnumerable<IIntermediateWorkPiece> FindWithOrderForComponent (IComponent component, int order)
    {
      Debug.Assert (null != component);

      var componentIntermediateWorkPieces = ModelDAOHelper.DAOFactory.ComponentIntermediateWorkPieceDAO
        .FindWithComponentOrder (component, order);
      return componentIntermediateWorkPieces
        .Select (x => x.IntermediateWorkPiece);
    }
  }
}
