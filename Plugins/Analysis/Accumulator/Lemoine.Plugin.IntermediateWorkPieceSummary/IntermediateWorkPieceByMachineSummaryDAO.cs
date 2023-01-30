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
using Lemoine.GDBPersistentClasses;
using Lemoine.Collections;

namespace Lemoine.Plugin.IntermediateWorkPieceSummary
{
  public class IntermediateWorkPieceByMachineSummaryDAO
    : VersionableByMachineNHibernateDAO<IntermediateWorkPieceByMachineSummary, IIntermediateWorkPieceByMachineSummary, int>
  {
    ILog log = LogManager.GetLogger<IntermediateWorkPieceByMachineSummaryDAO> ();

    static readonly string FIND_BY_TASK_MAX_RESULTS_KEY = "Plugin.IntermediateWorkPieceSummary.FindByTask.MaxResults";
    static readonly int FIND_BY_TASK_MAX_RESULTS_DEFAULT = 50;

    /// <summary>
    /// Constructor
    /// </summary>
    public IntermediateWorkPieceByMachineSummaryDAO ()
      : base ("Machine")
    { }

    /// <summary>
    /// Find all the items that match the line / intermediate work piece / shift
    /// </summary>
    /// <param name="intermediateWorkPiece">not null</param>
    /// <param name="line">not null</param>
    /// <param name="day">not null</param>
    /// <param name="shift"></param>
    /// <returns></returns>
    public IList<IIntermediateWorkPieceByMachineSummary> FindByLineIntermediateWorkPieceShift (IIntermediateWorkPiece intermediateWorkPiece,
                                                                                               ILine line,
                                                                                               DateTime day,
                                                                                               IShift shift)
    {
      Debug.Assert (null != intermediateWorkPiece);
      Debug.Assert (null != line);

      ICriteria criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<IntermediateWorkPieceByMachineSummary> ()
        .Add (Restrictions.Eq ("IntermediateWorkPiece.Id", ((IDataWithId)intermediateWorkPiece).Id))
        .Add (Restrictions.Eq ("Line.Id", line.Id))
        .Add (Restrictions.Eq ("Day", day));
      if (null == shift) {
        criteria = criteria.Add (Restrictions.IsNull ("Shift"));
      }
      else {
        criteria = criteria.Add (Restrictions.Eq ("Shift.Id", shift.Id));
      }

      return criteria.List<IIntermediateWorkPieceByMachineSummary> ();
    }

    /// <summary>
    /// Find the unique item that may match the natural key
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="intermediateWorkPiece">not null</param>
    /// <param name="component">nullable</param>
    /// <param name="workOrder">nullable</param>
    /// <param name="line">nullable</param>
    /// <param name="task"></param>
    /// <param name="day">nullable</param>
    /// <param name="shift">nullable</param>
    /// <returns></returns>
    public IIntermediateWorkPieceByMachineSummary FindByKey (IMachine machine,
                                                             IIntermediateWorkPiece intermediateWorkPiece,
                                                             IComponent component,
                                                             IWorkOrder workOrder,
                                                             ILine line,
                                                             ITask task,
                                                             DateTime? day,
                                                             IShift shift)
    {
      Debug.Assert (null != machine);
      Debug.Assert (null != intermediateWorkPiece);

      ICriteria criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<IntermediateWorkPieceByMachineSummary> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
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
      if (null == task) {
        criteria = criteria.Add (Restrictions.IsNull ("Task"));
      }
      else {
        criteria = criteria.Add (Restrictions.Eq ("Task.Id", ((IDataWithId)task).Id));
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
        .UniqueResult<IIntermediateWorkPieceByMachineSummary> ();
    }

    /// <summary>
    /// Find all the items that match the specified intermediate work piece
    /// </summary>
    /// <param name="intermediateWorkPiece">not null</param>
    /// <returns></returns>
    public IList<IIntermediateWorkPieceByMachineSummary> FindByIntermediateWorkPiece (IIntermediateWorkPiece intermediateWorkPiece)
    {
      if (intermediateWorkPiece is null) {
        log.Fatal ("FindByIntermediateWorkPiece: intermediate work piece is null, which is unexpected");
        Debug.Assert (null != intermediateWorkPiece);
        return new List<IIntermediateWorkPieceByMachineSummary> ();
      }

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<IntermediateWorkPieceByMachineSummary> ()
        .Add (Restrictions.Eq ("IntermediateWorkPiece.Id", intermediateWorkPiece.Id))
        .List<IIntermediateWorkPieceByMachineSummary> ();
    }

    /// <summary>
    /// Find all the items that match the specified component
    /// </summary>
    /// <param name="component"></param>
    /// <returns></returns>
    public IList<IIntermediateWorkPieceByMachineSummary> FindByComponent (IComponent component)
    {
      var criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<IntermediateWorkPieceByMachineSummary> ();
      if (component is null) {
        criteria = criteria.Add (Restrictions.IsNull ("Component"));
      }
      else {
        criteria = criteria.Add (Restrictions.Eq ("Component.Id", component.Id));
      }
      return criteria.List<IIntermediateWorkPieceByMachineSummary> ();
    }

    /// <summary>
    /// Find all the items that match the specified work order
    /// </summary>
    /// <param name="workOrder"></param>
    /// <returns></returns>
    public IList<IIntermediateWorkPieceByMachineSummary> FindByWorkOrder (IWorkOrder workOrder)
    {
      var criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<IntermediateWorkPieceByMachineSummary> ();
      if (workOrder is null) {
        criteria = criteria.Add (Restrictions.IsNull ("WorkOrder"));
      }
      else {
        criteria = criteria.Add (Restrictions.Eq ("WorkOrder.Id", workOrder.Id));
      }
      return criteria.List<IIntermediateWorkPieceByMachineSummary> ();
    }

    /// <summary>
    /// Find all the items that match the specified machine and task
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="task">not null</param>
    /// <returns></returns>
    public IList<IIntermediateWorkPieceByMachineSummary> FindByTask (IMachine machine, ITask task)
    {
      Debug.Assert (null != machine);
      Debug.Assert (null != task);

      var maxResults = Lemoine.Info.ConfigSet.LoadAndGet (FIND_BY_TASK_MAX_RESULTS_KEY, FIND_BY_TASK_MAX_RESULTS_DEFAULT);

      var result = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<IntermediateWorkPieceByMachineSummary> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (Restrictions.Eq ("Task.Id", task.Id))
        .AddOrder (Order.Desc ("Day"))
        .SetMaxResults (maxResults)
        .List<IIntermediateWorkPieceByMachineSummary> ();
      if (maxResults <= result.Count) {
        log.Fatal ($"FindByTask: max results {maxResults} was reached");
      }
      return result;
    }
  }
}
