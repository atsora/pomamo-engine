// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
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

namespace Lemoine.Plugin.CycleCountSummary
{
  public class CycleCountSummaryDAO
    : VersionableByMachineNHibernateDAO<CycleCountSummary, ICycleCountSummary, int>
  {
    readonly ILog log = LogManager.GetLogger (typeof (CycleCountSummaryDAO).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public CycleCountSummaryDAO ()
      : base ("Machine")
    { }

    /// <summary>
    /// Find by key
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="day"></param>
    /// <param name="shift"></param>
    /// <param name="workOrder"></param>
    /// <param name="line"></param>
    /// <param name="manufacturingOrder"></param>
    /// <param name="component"></param>
    /// <param name="operation">not null</param>
    /// <returns></returns>
    public ICycleCountSummary FindByKey (IMachine machine,
                                         DateTime day,
                                         IShift shift,
                                         IWorkOrder workOrder,
                                         ILine line,
                                         IManufacturingOrder manufacturingOrder,
                                         IComponent component,
                                         IOperation operation)
    {
      Debug.Assert (null != machine);

      ICriteria criteria =
        NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CycleCountSummary> ()
        .Add (Restrictions.Eq ("Machine", machine));
      if (null == operation) {
        criteria.Add (Restrictions.IsNull ("Operation"));
      }
      else {
        criteria.Add (Restrictions.Eq ("Operation.Id", ((IDataWithId)operation).Id));
      }
      criteria.Add (Restrictions.Eq ("Day", day));
      if (null == shift) {
        criteria.Add (Restrictions.IsNull ("Shift"));
      }
      else {
        criteria.Add (Restrictions.Eq ("Shift.Id", shift.Id));
      }
      if (null == workOrder) {
        criteria.Add (Restrictions.IsNull ("WorkOrder"));
      }
      else {
        criteria.Add (Restrictions.Eq ("WorkOrder.Id", ((IDataWithId)workOrder).Id));
      }
      if (null == line) {
        criteria.Add (Restrictions.IsNull ("Line"));
      }
      else {
        criteria.Add (Restrictions.Eq ("Line.Id", line.Id));
      }
      if (null == manufacturingOrder) {
        criteria.Add (Restrictions.IsNull ("ManufacturingOrder"));
      }
      else {
        criteria.Add (Restrictions.Eq ("ManufacturingOrder.Id", ((IDataWithId)manufacturingOrder).Id));
      }
      if (null == component) {
        criteria.Add (Restrictions.IsNull ("Component"));
      }
      else {
        criteria.Add (Restrictions.Eq ("Component.Id", ((IDataWithId)component).Id));
      }
      return criteria.UniqueResult<ICycleCountSummary> ();
    }

    /// <summary>
    /// Find by work order
    /// </summary>
    /// <param name="workOrder">not null</param>
    /// <returns></returns>
    internal IList<ICycleCountSummary> FindByWorkOrder (IWorkOrder workOrder)
    {
      Debug.Assert (null != workOrder);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CycleCountSummary> ()
        .Add (Restrictions.Eq ("WorkOrder", workOrder))
        .List<ICycleCountSummary> ();
    }

    /// <summary>
    /// Find by component
    /// </summary>
    /// <param name="component">not null</param>
    /// <returns></returns>
    internal IList<ICycleCountSummary> FindByComponent (IComponent component)
    {
      Debug.Assert (null != component);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CycleCountSummary> ()
        .Add (Restrictions.Eq ("Component", component))
        .List<ICycleCountSummary> ();
    }

    /// <summary>
    /// Find by operation
    /// </summary>
    /// <param name="operation">not null</param>
    /// <returns></returns>
    internal IList<ICycleCountSummary> FindByOperation (IOperation operation)
    {
      Debug.Assert (null != operation);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CycleCountSummary> ()
        .Add (Restrictions.Eq ("Operation", operation))
        .List<ICycleCountSummary> ();
    }

    /// <summary>
    /// Update the ReasonSummary for a given day (add or remove a Count)
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="day"></param>
    /// <param name="shift"></param>
    /// <param name="workOrder"></param>
    /// <param name="line"></param>
    /// <param name="manufacturingOrder"></param>
    /// <param name="component"></param>
    /// <param name="operation"></param>
    /// <param name="incrementValue"></param>
    /// <param name="partialIncrementValue"></param>
    internal void Update (IMachine machine,
                          DateTime day,
                          IShift shift,
                          IWorkOrder workOrder,
                          ILine line,
                          IManufacturingOrder manufacturingOrder,
                          IComponent component,
                          IOperation operation,
                          int incrementValue,
                          int partialIncrementValue)
    {
      if ((0 == incrementValue) && (0 == partialIncrementValue)) {
        log.Debug ("UpdateDay: incrementValue is 0: => do nothing");
        return;
      }

      // Find any existing analysis in database
      ICycleCountSummary cycleCountSummary =
        FindByKey (machine, day, shift, workOrder, line, manufacturingOrder, component, operation);
      if (null == cycleCountSummary) {
        // The persistent class does not exist in database, create it
        cycleCountSummary = new CycleCountSummary (machine,
                                                   day,
                                                   shift,
                                                   workOrder,
                                                   line,
                                                   manufacturingOrder,
                                                   component,
                                                   operation);
        if ((0 == incrementValue) && (0 == partialIncrementValue)) {
          log.InfoFormat (".{0} UpdateDay: " +
                          "new summary with no full or partial cycles " +
                          "at day {1} " +
                          "=> skip it",
                          machine.Id,
                          day);
        }
        else if ((incrementValue < 0) || (partialIncrementValue < 0)) {
          log.ErrorFormat (".{0} UpdateDay: " +
                           "new summary with a negative number of cycles {1} or partial {2} " +
                           "at day {3} " +
                           "=> skip it",
                           machine.Id,
                           incrementValue, partialIncrementValue,
                           day);
          // UNDONE: log it also in analysislog (but there is no access to the modification here)
        }
        else {
          cycleCountSummary.Full = incrementValue;
          cycleCountSummary.Partial = partialIncrementValue;
          Debug.Assert (0.0 <= cycleCountSummary.Full);
          MakePersistent (cycleCountSummary);
        }
        return;
      }
      else { // Update the existing analysis
        if ((cycleCountSummary.Full + incrementValue <= 0)
            && (cycleCountSummary.Partial + partialIncrementValue <= 0)) {
          if ((0 == cycleCountSummary.Full + incrementValue)
              && (0 == cycleCountSummary.Partial + partialIncrementValue)) {
            log.InfoFormat (".{0} UpdateDay: " +
                            "delete summary " +
                            "because the new number of full cycles would be {1} " +
                            "and partial cycles {2} " +
                            "at day {3}",
                            machine.Id,
                            cycleCountSummary.Full + incrementValue,
                            cycleCountSummary.Partial + partialIncrementValue,
                            day);
          }
          else { // < 0
            log.ErrorFormat (".{0} UpdateDay: " +
                             "delete summary " +
                             "because the new number would be full={1} partial={2} " +
                             "at day {3}",
                             machine.Id,
                             cycleCountSummary.Full + incrementValue,
                             cycleCountSummary.Partial + partialIncrementValue,
                             day);
            // TODO: log it also in analysislog
          }
          MakeTransient (cycleCountSummary);
        }
        else {
          cycleCountSummary.Full += incrementValue;
          cycleCountSummary.Partial += partialIncrementValue;
          Debug.Assert ((0 < cycleCountSummary.Full) || (0 < cycleCountSummary.Partial));
          if (cycleCountSummary.Full < 0) {
            log.ErrorFormat (".{0} UpdateDay: " +
                             "the new number of full cycles is negative {1} " +
                             "=> correct it to 0",
                             machine.Id,
                             cycleCountSummary.Full);
            cycleCountSummary.Full = 0;
          }
          if (cycleCountSummary.Partial < 0) {
            log.ErrorFormat (".{0} UpdateDay: " +
                             "the new number of partial cycles is negative {1} " +
                             "=> correct it to 0",
                             machine.Id,
                             cycleCountSummary.Partial);
            cycleCountSummary.Partial = 0;
          }
          Debug.Assert (0 <= cycleCountSummary.Full);
          Debug.Assert (0 <= cycleCountSummary.Partial);
          MakePersistent (cycleCountSummary);
        }
        return;
      }
    }
  }
}
