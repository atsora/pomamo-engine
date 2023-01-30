// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IOperationCycleDeliverablePieceDAO">IOperationCycleDeliverablePieceDAO</see>
  /// </summary>

  public class OperationCycleDeliverablePieceDAO
    : VersionableByMachineNHibernateDAO<OperationCycleDeliverablePiece, IOperationCycleDeliverablePiece, int>
    , IOperationCycleDeliverablePieceDAO

  {
    readonly ILog log = LogManager.GetLogger<OperationCycleDeliverablePieceDAO> ();

    /// <summary>
    /// Constructor
    /// </summary>
    public OperationCycleDeliverablePieceDAO ()
      : base ("Machine")
    { }
    
    /// <summary>
    /// Find all operation cycle/deliverable piece associations
    /// for a given operation cycle
    /// </summary>
    /// <param name="operationCycle">not null</param>
    /// <returns></returns>
    public IList<IOperationCycleDeliverablePiece> FindAllWithOperationCycle(IOperationCycle operationCycle)
    {
      Debug.Assert (null != operationCycle);
      if (!ModelDAOHelper.DAOFactory.IsInitialized (operationCycle)) {
        log.ErrorFormat ("FindAllWithOperationCycle: operationCycle is not initialized. StackTrace={0}",
          System.Environment.StackTrace);
      }
      Debug.Assert (null != operationCycle.Machine);
      
      return
        NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<OperationCycleDeliverablePiece>()
        .Add (Restrictions.Eq ("Machine.Id", operationCycle.Machine.Id)) // To take profit of the partitioning
        .Add (Restrictions.Eq ("OperationCycle.Id", operationCycle.Id))
        .List<IOperationCycleDeliverablePiece> ();
    }

    /// <summary>
    /// Returns unique operation cycle/deliverable piece association
    /// for a given operation cycle/deliverable piece pair
    /// if it exists
    /// </summary>
    /// <param name="operationCycle">not null</param>
    /// <param name="deliverablePiece">not null</param>
    /// <returns></returns>
    public IOperationCycleDeliverablePiece
      FindWithOperationCycleDeliverablePiece(IOperationCycle operationCycle, IDeliverablePiece deliverablePiece)
    {
      Debug.Assert (null != operationCycle);
      if (!ModelDAOHelper.DAOFactory.IsInitialized (operationCycle)) {
        log.ErrorFormat ("FindAllWithOperationCycle: operationCycle is not initialized. StackTrace={0}",
          System.Environment.StackTrace);
      }
      Debug.Assert (null != operationCycle.Machine);
      Debug.Assert (null != deliverablePiece);
      
      return
        NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<OperationCycleDeliverablePiece>()
        .Add (Restrictions.Eq ("Machine.Id", operationCycle.Machine.Id)) // To take profit of the partitioning
        .Add (Restrictions.Eq ("OperationCycle", operationCycle))
        .Add (Restrictions.Eq ("DeliverablePiece", deliverablePiece))
        .UniqueResult<IOperationCycleDeliverablePiece> ();
    }
    
    
    /// <summary>
    /// Find all operation cycle/deliverable piece associations
    /// for a given machine in a datetime range
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="utcFrom"></param>
    /// <param name="utcTo"></param>
    /// <returns></returns>
    public IList<IOperationCycleDeliverablePiece> FindAllInRangeByMachine(IMonitoredMachine machine, DateTime? utcFrom, DateTime? utcTo)
    {
      string queryName;
      if (utcTo.HasValue) {
        queryName = "OperationCycleDeliverablePieceByMachineInUtcRangeWithEnd";
      }
      else {
        queryName = "OperationCycleDeliverablePieceByMachineInUtcRangeWithoutEnd";
      }
      IQuery query = 
        NHibernateHelper.GetCurrentSession ()
        .GetNamedQuery(queryName)
        .SetParameter<IMachine>("machine",machine)
        .SetParameter<DateTime?>("rangeBegin", utcFrom);
      
      if(utcTo.HasValue){
        query.SetParameter<DateTime?>("rangeEnd",utcTo);
      }
  
      return query.List<IOperationCycleDeliverablePiece> ();
    }
    
  }
}
