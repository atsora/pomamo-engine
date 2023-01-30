// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Collections;
using Lemoine.GDBPersistentClasses;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate;
using NHibernate.Criterion;

namespace Lemoine.Plugin.IntermediateWorkPieceSummary
{
  public class IntermediateWorkPieceSummaryDAO
    : ReadOnlyNHibernateDAO<IntermediateWorkPieceSummary, IIntermediateWorkPieceSummary, int>
  {
    /// <summary>
    /// Find all the items that match the line / intermediate work piece / shift
    /// </summary>
    /// <param name="intermediateWorkPiece">not null</param>
    /// <param name="line">not null</param>
    /// <param name="day">not null</param>
    /// <param name="shift">may be null</param>
    /// <returns></returns>
    public IList<IIntermediateWorkPieceSummary> FindByLineIntermediateWorkPieceShift (IIntermediateWorkPiece intermediateWorkPiece,
                                                                                      ILine line,
                                                                                      DateTime day,
                                                                                      IShift shift)
    {
      Debug.Assert (null != intermediateWorkPiece);
      Debug.Assert (null != line);

      ICriteria criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<IntermediateWorkPieceSummary> ()
        .Add (Restrictions.Eq ("IntermediateWorkPiece.Id", ((IDataWithId)intermediateWorkPiece).Id))
        .Add (Restrictions.Eq ("Line.Id", line.Id))
        .Add (Restrictions.Eq ("Day", day));
      if (null == shift) {
        criteria = criteria.Add (Restrictions.IsNull ("Shift"));
      }
      else {
        criteria = criteria.Add (Restrictions.Eq ("Shift.Id", shift.Id));
      }

      return criteria.List<IIntermediateWorkPieceSummary> ();
    }

    /// <summary>
    /// Find the unique item that may match the natural key
    /// </summary>
    /// <param name="intermediateWorkPiece">not null</param>
    /// <param name="component">nullable</param>
    /// <param name="workOrder">nullable</param>
    /// <param name="line">nullable</param>
    /// <param name="day">nullable</param>
    /// <param name="shift">nullable</param>
    /// <returns></returns>
    public IIntermediateWorkPieceSummary FindByKey (IIntermediateWorkPiece intermediateWorkPiece,
                                                    IComponent component,
                                                    IWorkOrder workOrder,
                                                    ILine line,
                                                    DateTime? day,
                                                    IShift shift)
    {
      Debug.Assert (null != intermediateWorkPiece);
      ICriteria criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<IntermediateWorkPieceSummary> ()
        .Add (Restrictions.Eq ("IntermediateWorkPiece.Id", ((IDataWithId)intermediateWorkPiece).Id));
      if (null == component) {
        criteria = criteria.Add (Restrictions.IsNull ("Component"));
      }
      else {
        criteria = criteria.Add (Restrictions.Eq ("Component.Id", ((IDataWithId)component).Id));
      }
      if (null == workOrder) {
        criteria = criteria.Add (Restrictions.IsNull ("WorkOrder"));
      }
      else {
        criteria = criteria.Add (Restrictions.Eq ("WorkOrder.Id", ((IDataWithId)workOrder).Id));
      }
      if (null == line) {
        criteria = criteria.Add (Restrictions.IsNull ("Line"));
      }
      else {
        criteria = criteria.Add (Restrictions.Eq ("Line.Id", line.Id));
      }
      if (!day.HasValue) {
        criteria = criteria.Add (Restrictions.IsNull ("Day"));
      }
      else {
        criteria = criteria.Add (Restrictions.Eq ("Day", day.Value));
      }
      if (null == shift) {
        criteria = criteria.Add (Restrictions.IsNull ("Shift"));
      }
      else {
        criteria = criteria.Add (Restrictions.Eq ("Shift.Id", shift.Id));
      }
      return criteria
        .UniqueResult<IIntermediateWorkPieceSummary> ();
    }

    /// <summary>
    /// Find all the items that match the specified intermediate work piece
    /// </summary>
    /// <param name="intermediateWorkPiece"></param>
    /// <returns></returns>
    public IList<IIntermediateWorkPieceSummary> FindByIntermediateWorkPiece (IIntermediateWorkPiece intermediateWorkPiece)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<IntermediateWorkPieceSummary> ()
        .Add (Restrictions.Eq ("IntermediateWorkPiece", intermediateWorkPiece))
        .List<IIntermediateWorkPieceSummary> ();
    }

    /// <summary>
    /// Find all the items that match the specified component
    /// </summary>
    /// <param name="component"></param>
    /// <returns></returns>
    public IList<IIntermediateWorkPieceSummary> FindByComponent (IComponent component)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<IntermediateWorkPieceSummary> ()
        .Add (Restrictions.Eq ("Component", component))
        .List<IIntermediateWorkPieceSummary> ();
    }

    /// <summary>
    /// Find all the items that match the specified work order
    /// </summary>
    /// <param name="workOrder">not null</param>
    /// <returns></returns>
    public IList<IIntermediateWorkPieceSummary> FindByWorkOrder (IWorkOrder workOrder)
    {
      Debug.Assert (null != workOrder);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<IntermediateWorkPieceSummary> ()
        .Add (Restrictions.Eq ("WorkOrder", workOrder))
        .List<IIntermediateWorkPieceSummary> ();
    }

    /// <summary>
    /// Find all the items that match the specified line
    /// </summary>
    /// <param name="line">not null</param>
    /// <returns></returns>
    public IList<IIntermediateWorkPieceSummary> FindByLine (ILine line)
    {
      Debug.Assert (null != line);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<IntermediateWorkPieceSummary> ()
        .Add (Restrictions.Eq ("Line", line))
        .List<IIntermediateWorkPieceSummary> ();
    }

    /// <summary>
    /// Find all the items that match the specified intermediate work piece, work order and line
    /// </summary>
    /// <param name="intermediateWorkPiece">not null</param>
    /// <param name="workOrder">not null</param>
    /// <param name="line">nullable</param>
    /// <returns></returns>
    public IList<IIntermediateWorkPieceSummary> FindByIwpWorkOrderLine (IIntermediateWorkPiece intermediateWorkPiece, IWorkOrder workOrder, ILine line)
    {
      Debug.Assert (null != intermediateWorkPiece);
      Debug.Assert (null != workOrder);

      ICriteria criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<IntermediateWorkPieceSummary> ()
        .Add (Restrictions.Eq ("IntermediateWorkPiece", intermediateWorkPiece))
        .Add (Restrictions.Eq ("WorkOrder", workOrder));
      if (null == line) {
        criteria = criteria.Add (Restrictions.IsNull ("Line"));
      }
      else {
        criteria = criteria.Add (Restrictions.Eq ("Line", line));
      }
      return criteria.List<IIntermediateWorkPieceSummary> ();
    }

    /// <summary>
    /// Find all the items that match the specified work order and line
    /// </summary>
    /// <param name="workOrder">not null</param>
    /// <param name="line">nullable</param>
    /// <returns></returns>
    public IList<IIntermediateWorkPieceSummary> FindByWorkOrderLine (IWorkOrder workOrder, ILine line)
    {
      Debug.Assert (null != workOrder);

      ICriteria criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<IntermediateWorkPieceSummary> ()
        .Add (Restrictions.Eq ("WorkOrder", workOrder));
      if (null == line) {
        criteria = criteria.Add (Restrictions.IsNull ("Line"));
      }
      else {
        criteria = criteria.Add (Restrictions.Eq ("Line", line));
      }
      return criteria.List<IIntermediateWorkPieceSummary> ();
    }

    /// <summary>
    /// Get all the intermediate work piece summary for the specified time range
    /// </summary>
    /// <param name="begin">date from which values are kept</param>
    /// <param name="end">date after which the values are not kept</param>
    /// <returns></returns>
    public IList<IIntermediateWorkPieceSummary> GetListInRange (DateTime begin, DateTime end)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<IntermediateWorkPieceSummary> ()
        .Add (Restrictions.Conjunction ()
              .Add (Restrictions.Le ("Day", end.Date))
              .Add (Restrictions.Ge ("Day", begin.Date)))
        .AddOrder (Order.Asc ("Day"))
        .List<IIntermediateWorkPieceSummary> ();
    }
  }
}
