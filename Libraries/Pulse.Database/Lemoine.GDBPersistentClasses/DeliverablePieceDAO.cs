// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using NHibernate;
using NHibernate.Criterion;
using Lemoine.Collections;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Description of DeliverablePieceDAO.
  /// </summary>
  public class DeliverablePieceDAO
    : VersionableNHibernateDAO<DeliverablePiece, IDeliverablePiece, int>
    , IDeliverablePieceDAO
  {    
    static readonly ILog log = LogManager.GetLogger(typeof (DeliverablePieceDAO).FullName);
    
    /// <summary>
    /// Find a Deliverablepiece using its serial number(code) and component
    /// </summary>
    /// <param name="serialNumber">not null or empty</param>
    /// <param name="component">not null</param>
    public IDeliverablePiece FindByCodeAndComponent(string serialNumber, IComponent component)
    {
      Debug.Assert (!string.IsNullOrEmpty (serialNumber));
      Debug.Assert (null != component);
      
      return NHibernateHelper.GetCurrentSession()
        .CreateCriteria<DeliverablePiece> ()
        .Add (Restrictions.Eq ("Code", serialNumber))
        .Add (Restrictions.Eq ("Component.Id", ((IDataWithId<int>)component).Id))
        .UniqueResult<IDeliverablePiece> ();
    }

    /// <summary>
    /// Find all  Deliverablepieces matching a serial number code
    /// </summary>
    /// <param name="serialNumber">not null or empty</param>
    public IList<IDeliverablePiece> FindByCode(string serialNumber)
    {
      Debug.Assert (!string.IsNullOrEmpty (serialNumber));
      
      return NHibernateHelper.GetCurrentSession()
        .CreateCriteria<DeliverablePiece> ()
        .Add (Restrictions.Eq ("Code", serialNumber))
        .List<IDeliverablePiece> ();      
    }

    /// <summary>
    /// Find all the deliverable pieces that are associated to an operation cycle
    /// </summary>
    /// <param name="operationCycle"></param>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<IEnumerable<IDeliverablePiece>> FindByOperationCycleAsync (IOperationCycle operationCycle)
    {
      Debug.Assert (null != operationCycle);

      var operationCycleDeliverablePieces = await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<OperationCycleDeliverablePiece> ()
        .Add (Restrictions.Eq ("OperationCycle.Id", operationCycle.Id))
        .Add (Restrictions.Eq ("Machine.Id", operationCycle.Machine.Id))
        .Fetch (SelectMode.Fetch, "DeliverablePiece")
        .ListAsync<OperationCycleDeliverablePiece> ();
      return operationCycleDeliverablePieces
        .Select (x => x.DeliverablePiece);
    }
  }
}

