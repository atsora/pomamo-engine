// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IIntermediateWorkPieceTargetDAO">IIntermediateWorkPieceTargetDAO</see>
  /// </summary>
  public class IntermediateWorkPieceTargetDAO
    : VersionableNHibernateDAO<IntermediateWorkPieceTarget, IIntermediateWorkPieceTarget, int>
    , IIntermediateWorkPieceTargetDAO
  {
    /// <summary>
    /// Find all the items that match the line / intermediate work piece / shift
    /// </summary>
    /// <param name="intermediateWorkPiece">not null</param>
    /// <param name="line">not null</param>
    /// <param name="day">not null</param>
    /// <param name="shift">may be null</param>
    /// <returns></returns>
    public IList<IIntermediateWorkPieceTarget> FindByLineIntermediateWorkPieceShift (IIntermediateWorkPiece intermediateWorkPiece,
                                                                                     ILine line,
                                                                                     DateTime day,
                                                                                     IShift shift)
    {
      Debug.Assert (null != intermediateWorkPiece);
      Debug.Assert (null != line);
      
      ICriteria criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<IntermediateWorkPieceTarget> ()
        .Add (Restrictions.Eq ("IntermediateWorkPiece", intermediateWorkPiece))
        .Add (Restrictions.Eq ("Line", line))
        .Add (Restrictions.Eq ("Day", day));
      if (null == shift) {
        criteria = criteria.Add (Restrictions.IsNull ("Shift"));
      }
      else {
        criteria = criteria.Add (Restrictions.Eq ("Shift", shift));
      }
      
      return criteria.List<IIntermediateWorkPieceTarget> ();
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
    public IIntermediateWorkPieceTarget FindByKey (IIntermediateWorkPiece intermediateWorkPiece,
                                                   IComponent component,
                                                   IWorkOrder workOrder,
                                                   ILine line,
                                                   DateTime? day,
                                                   IShift shift)
    {
      Debug.Assert (null != intermediateWorkPiece);
      ICriteria criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<IntermediateWorkPieceTarget> ()
        .Add (Restrictions.Eq ("IntermediateWorkPiece", intermediateWorkPiece));
      if (null == component) {
        criteria = criteria.Add (Restrictions.IsNull ("Component"));
      }
      else {
        criteria = criteria.Add (Restrictions.Eq ("Component", component));
      }
      if (null == workOrder) {
        criteria = criteria.Add (Restrictions.IsNull ("WorkOrder"));
      }
      else {
        criteria = criteria.Add (Restrictions.Eq ("WorkOrder", workOrder));
      }
      if (null == line) {
        criteria = criteria.Add (Restrictions.IsNull ("Line"));
      }
      else {
        criteria = criteria.Add (Restrictions.Eq ("Line", line));
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
        criteria = criteria.Add (Restrictions.Eq ("Shift", shift));
      }
      return criteria
        .UniqueResult<IIntermediateWorkPieceTarget> ();
    }

    /// <summary>
    /// Find all the items that match the specified intermediate work piece
    /// </summary>
    /// <param name="intermediateWorkPiece"></param>
    /// <returns></returns>
    public IList<IIntermediateWorkPieceTarget> FindByIntermediateWorkPiece (IIntermediateWorkPiece intermediateWorkPiece)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<IntermediateWorkPieceTarget> ()
        .Add (Restrictions.Eq ("IntermediateWorkPiece", intermediateWorkPiece))
        .List<IIntermediateWorkPieceTarget> ();
    }

    /// <summary>
    /// Find all the items that match the specified component
    /// </summary>
    /// <param name="component"></param>
    /// <returns></returns>
    public IList<IIntermediateWorkPieceTarget> FindByComponent (IComponent component)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<IntermediateWorkPieceTarget> ()
        .Add (Restrictions.Eq ("Component", component))
        .List<IIntermediateWorkPieceTarget> ();
    }

    /// <summary>
    /// Find all the items that match the specified work order
    /// </summary>
    /// <param name="workOrder">not null</param>
    /// <returns></returns>
    public IList<IIntermediateWorkPieceTarget> FindByWorkOrder (IWorkOrder workOrder)
    {
      Debug.Assert (null != workOrder);
      
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<IntermediateWorkPieceTarget> ()
        .Add (Restrictions.Eq ("WorkOrder", workOrder))
        .List<IIntermediateWorkPieceTarget> ();
    }

    /// <summary>
    /// Find all the items that match the specified line
    /// </summary>
    /// <param name="line">not null</param>
    /// <returns></returns>
    public IList<IIntermediateWorkPieceTarget> FindByLine (ILine line)
    {
      Debug.Assert (null != line);
      
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<IntermediateWorkPieceTarget> ()
        .Add (Restrictions.Eq ("Line", line))
        .List<IIntermediateWorkPieceTarget> ();
    }

    /// <summary>
    /// Find all the items that match the specified intermediate work piece, work order and line
    /// </summary>
    /// <param name="intermediateWorkPiece">not null</param>
    /// <param name="workOrder">not null</param>
    /// <param name="line">nullable</param>
    /// <returns></returns>
    public IList<IIntermediateWorkPieceTarget> FindByIwpWorkOrderLine (IIntermediateWorkPiece intermediateWorkPiece, IWorkOrder workOrder, ILine line)
    {
      Debug.Assert (null != intermediateWorkPiece);
      Debug.Assert (null != workOrder);
      
      ICriteria criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<IntermediateWorkPieceTarget> ()
        .Add (Restrictions.Eq ("IntermediateWorkPiece", intermediateWorkPiece))
        .Add (Restrictions.Eq ("WorkOrder", workOrder));
      if (null == line) {
        criteria = criteria.Add (Restrictions.IsNull ("Line"));
      }
      else {
        criteria = criteria.Add (Restrictions.Eq ("Line", line));
      }
      return criteria.List<IIntermediateWorkPieceTarget> ();
    }
    
    /// <summary>
    /// Find all the items that match the specified work order and line
    /// </summary>
    /// <param name="workOrder">not null</param>
    /// <param name="line">nullable</param>
    /// <returns></returns>
    public IList<IIntermediateWorkPieceTarget> FindByWorkOrderLine (IWorkOrder workOrder, ILine line)
    {
      Debug.Assert (null != workOrder);
      
      ICriteria criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<IntermediateWorkPieceTarget> ()
        .Add (Restrictions.Eq ("WorkOrder", workOrder));
      if (null == line) {
        criteria = criteria.Add (Restrictions.IsNull ("Line"));
      }
      else {
        criteria = criteria.Add (Restrictions.Eq ("Line", line));
      }
      return criteria.List<IIntermediateWorkPieceTarget> ();
    }
    
    /// <summary>
    /// Get all the intermediate work piece targets for the specified time range
    /// </summary>
    /// <param name="begin">date from which values are kept</param>
    /// <param name="end">date after which the values are not kept</param>
    /// <returns></returns>
    public IList<IIntermediateWorkPieceTarget> GetListInRange (DateTime begin, DateTime end)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<IntermediateWorkPieceTarget> ()
        .Add (Restrictions.Conjunction ()
              .Add (Restrictions.Le ("Day", end.Date))
              .Add (Restrictions.Ge ("Day", begin.Date)))
        .AddOrder (Order.Asc ("Day"))
        .List<IIntermediateWorkPieceTarget> ();
    }
    
    /// <summary>
    /// Count all targets that match the line / shift
    /// Targets taken into account are not null and > 0
    /// All targets are attached to a workorder
    /// </summary>
    /// <param name="line">not null</param>
    /// <param name="day">not null</param>
    /// <param name="shift">may be null</param>
    /// <returns></returns>
    public int CountTargetsByLineShift (ILine line, DateTime day, IShift shift)
    {
      Debug.Assert (null != line);
      
      ICriteria criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<IntermediateWorkPieceTarget> ()
        .Add (Restrictions.Eq ("Line", line))
        .Add (Restrictions.Eq ("Day", day))
        .Add (Restrictions.IsNotNull("WorkOrder"))
        .Add (Restrictions.IsNotNull("Number"))
        .Add (Restrictions.Gt ("Number", 0));
      if (null == shift) {
        criteria = criteria.Add (Restrictions.IsNull ("Shift"));
      }
      else {
        criteria = criteria.Add (Restrictions.Eq ("Shift", shift));
      }

      IList<IIntermediateWorkPieceTarget> iwpss = criteria.List<IIntermediateWorkPieceTarget>();
      
      // Count the number of workorders
      IList<int> woId = new List<int>();
      foreach (IIntermediateWorkPieceTarget iwps in iwpss) {
        if (!woId.Contains(((Lemoine.Collections.IDataWithId)iwps.WorkOrder).Id)) {
          woId.Add(((Lemoine.Collections.IDataWithId)iwps.WorkOrder).Id);
        }
      }

      return woId.Count;
    }
  }
}
