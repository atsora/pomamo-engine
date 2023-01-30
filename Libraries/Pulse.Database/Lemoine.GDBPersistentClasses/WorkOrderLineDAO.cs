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
using System.Threading.Tasks;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IWorkOrderLineDAO">IWorkOrderLineDAO</see>
  /// </summary>
  public sealed class WorkOrderLineDAO
    : GenericLineSlotDAO<WorkOrderLine, IWorkOrderLine>
    , IWorkOrderLineDAO
  {
    /// <summary>
    /// Constructor
    /// </summary>
    internal WorkOrderLineDAO()
      : base (false)
    {
    }
    
    /// <summary>
    /// Find all WorkOrderLine for a specific workorder
    /// </summary>
    /// <param name="workOrder">not null</param>
    /// <returns></returns>
    public IList<IWorkOrderLine> FindAllByWorkOrder(IWorkOrder workOrder)
    {
      Debug.Assert (null != workOrder);
      
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<WorkOrderLine> ()
        .Add (Restrictions.Eq ("WorkOrder", workOrder))
        .List<IWorkOrderLine> ();
    }
    
    /// <summary>
    /// Find all WorkOrderLine for a specific line sorted by BeginDateTime (ascending order)
    /// </summary>
    /// <param name="line">not null</param>
    /// <returns></returns>
    public IList<IWorkOrderLine> FindAllByLine(ILine line)
    {
      Debug.Assert (null != line);
      
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<WorkOrderLine> ()
        .Add (Restrictions.Eq ("Line", line))
        .AddOrder (Order.Asc ("BeginDateTime"))
        .List<IWorkOrderLine> ();
    }
    
    /// <summary>
    /// Find a specific WorkOrderLine for a specific (line + workorder)
    /// The result may be null if no such workorderline was found
    /// </summary>
    /// <param name="line">not null</param>
    /// <param name="workOrder">not null</param>
    /// <returns></returns>
    public IWorkOrderLine FindByLineAndWorkOrder(ILine line, IWorkOrder workOrder)
    {
      Debug.Assert (null != line);
      Debug.Assert (null != workOrder);
      
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<WorkOrderLine> ()
        .Add (Restrictions.Eq ("Line", line))
        .Add (Restrictions.Eq ("WorkOrder", workOrder))
        .UniqueResult<IWorkOrderLine> (); // Because there is a unique constraint on (line, workOrder)
    }
    
    /// <summary>
    /// Find the first IWorkOrderLine after a given date
    /// </summary>
    /// <param name="line"></param>
    /// <param name="beginAfter"></param>
    /// <returns></returns>
    public IWorkOrderLine FindFirstAfter (ILine line, DateTime beginAfter)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<WorkOrderLine> ()
        .Add (Restrictions.Eq ("Line", line))
        .Add (Restrictions.Ge ("BeginDateTime", (LowerBound<DateTime>)beginAfter))
        .AddOrder (Order.Asc ("BeginDateTime"))
        .SetMaxResults (1)
        .UniqueResult<IWorkOrderLine> ();
    }
    
    /// <summary>
    /// Get all the line slots in progress
    /// </summary>
    /// <returns></returns>
    public IList<IWorkOrderLine> GetWorkOrderLineInProgress () {
      DateTime dateTime = DateTime.UtcNow;
      
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<WorkOrderLine> ()
        .Add (Restrictions.Disjunction ()
              .Add (Restrictions.IsNull ("EndDateTime"))
              .Add (Restrictions.Gt ("EndDateTime",
                                     (UpperBound<DateTime>)dateTime)))
        .Add (Restrictions.Le ("BeginDateTime", (LowerBound<DateTime>)dateTime))
        .Fetch (SelectMode.Fetch, "Line")
        .Fetch (SelectMode.Fetch, "Line.Components")
        .List<IWorkOrderLine> ();
    }
    
    /// <summary>
    /// MakePersistent with a cascade option
    /// </summary>
    /// <param name="entity"></param>
    public override IWorkOrderLine MakePersistent (IWorkOrderLine entity)
    {
      IWorkOrderLine result = base.MakePersistent (entity);
      foreach (IWorkOrderLineQuantity quantity in entity.IntermediateWorkPieceQuantities.Values) {
        NHibernateHelper.GetCurrentSession ()
          .SaveOrUpdate (quantity);
      }
      return result;
    }

    /// <summary>
    /// MakePersistent with a cascade option
    /// </summary>
    /// <param name="entity"></param>
    public override async Task<IWorkOrderLine> MakePersistentAsync (IWorkOrderLine entity)
    {
      IWorkOrderLine result = await base.MakePersistentAsync (entity);
      foreach (IWorkOrderLineQuantity quantity in entity.IntermediateWorkPieceQuantities.Values) {
        await NHibernateHelper.GetCurrentSession ()
          .SaveOrUpdateAsync (quantity);
      }
      return result;
    }

    /// <summary>
    /// MakeTransient with a cascade action
    /// </summary>
    /// <param name="entity"></param>
    public override void MakeTransient(IWorkOrderLine entity)
    {
      foreach (IWorkOrderLineQuantity quantity in entity.IntermediateWorkPieceQuantities.Values) {
        NHibernateHelper.GetCurrentSession ()
          .Delete (quantity);
      }
      // Remove the association to the quantities because of the option
      // cascade="save-update"
      entity.IntermediateWorkPieceQuantities.Clear ();
      base.MakeTransient(entity);
    }

    /// <summary>
    /// MakeTransient with a cascade action
    /// </summary>
    /// <param name="entity"></param>
    public override async System.Threading.Tasks.Task MakeTransientAsync (IWorkOrderLine entity)
    {
      foreach (IWorkOrderLineQuantity quantity in entity.IntermediateWorkPieceQuantities.Values) {
        await NHibernateHelper.GetCurrentSession ()
          .DeleteAsync (quantity);
      }
      // Remove the association to the quantities because of the option
      // cascade="save-update"
      entity.IntermediateWorkPieceQuantities.Clear ();
      await base.MakeTransientAsync (entity);
    }
  }
}
