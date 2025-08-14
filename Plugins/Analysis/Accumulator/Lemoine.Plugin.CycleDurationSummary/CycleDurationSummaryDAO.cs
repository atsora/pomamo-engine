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

namespace Lemoine.Plugin.CycleDurationSummary
{
  public class CycleDurationSummaryDAO
    : VersionableByMachineNHibernateDAO<CycleDurationSummary, ICycleDurationSummary, int>
  {
    readonly ILog log = LogManager.GetLogger (typeof (CycleDurationSummaryDAO).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public CycleDurationSummaryDAO ()
      : base ("Machine")
    { }

    internal ICycleDurationSummary FindByKey (IMachine machine,
                                              DateTime day,
                                              IShift shift,
                                              IWorkOrder workOrder,
                                              ILine line,
                                              IManufacturingOrder manufacturingOrder,
                                              IComponent component,
                                              IOperation operation,
                                              int offset)
    {
      Debug.Assert (null != machine);

      ICriteria criteria =
        NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CycleDurationSummary> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (Restrictions.Eq ("Day", day))
        .Add (Restrictions.Eq ("Offset", offset));
      if (null == shift) {
        criteria.Add (Restrictions.IsNull ("Shift"));
      }
      else {
        criteria.Add (Restrictions.Eq ("Shift.Id", shift.Id));
      }
      if (null == operation) {
        criteria.Add (Restrictions.IsNull ("Operation"));
      }
      else {
        criteria.Add (Restrictions.Eq ("Operation.Id", ((IDataWithId)operation).Id));
      }
      if (null == workOrder) {
        criteria.Add (Restrictions.IsNull ("WorkOrder"));
      }
      else {
        criteria.Add (Restrictions.Eq ("WorkOrder.Id", ((IDataWithId)workOrder).Id));
      }
      if (null == component) {
        criteria.Add (Restrictions.IsNull ("Component"));
      }
      else {
        criteria.Add (Restrictions.Eq ("Component.Id", ((IDataWithId)component).Id));
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
      return criteria.UniqueResult<ICycleDurationSummary> ();
    }

    /// <summary>
    /// Find by work order
    /// </summary>
    /// <param name="workOrder">not null</param>
    /// <returns></returns>
    internal IList<ICycleDurationSummary> FindByWorkOrder (IWorkOrder workOrder)
    {
      Debug.Assert (null != workOrder);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CycleDurationSummary> ()
        .Add (Restrictions.Eq ("WorkOrder", workOrder))
        .List<ICycleDurationSummary> ();
    }

    /// <summary>
    /// Find by component
    /// </summary>
    /// <param name="component">not null</param>
    /// <returns></returns>
    internal IList<ICycleDurationSummary> FindByComponent (IComponent component)
    {
      Debug.Assert (null != component);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CycleDurationSummary> ()
        .Add (Restrictions.Eq ("Component", component))
        .List<ICycleDurationSummary> ();
    }

    /// <summary>
    /// Find by operation
    /// </summary>
    /// <param name="operation">not null</param>
    /// <returns></returns>
    internal IList<ICycleDurationSummary> FindByOperation (IOperation operation)
    {
      Debug.Assert (null != operation);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CycleDurationSummary> ()
        .Add (Restrictions.Eq ("Operation", operation))
        .List<ICycleDurationSummary> ();
    }

    /// <summary>
    /// Update the CycleDurationSummary for a given day (add or remove a duration)
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="day"></param>
    /// <param name="shift"></param>
    /// <param name="workOrder"></param>
    /// <param name="line"></param>
    /// <param name="manufacturingOrder"></param>
    /// <param name="component"></param>
    /// <param name="operation"></param>
    /// <param name="offset"></param>
    /// <param name="incrementValue"></param>
    /// <param name="partialIncrementValue"></param>
    internal void UpdateDay (IMachine machine,
                             DateTime day,
                             IShift shift,
                             IWorkOrder workOrder,
                             ILine line,
                             IManufacturingOrder manufacturingOrder,
                             IComponent component,
                             IOperation operation,
                             int offset,
                             int incrementValue,
                             int partialIncrementValue)
    {
      if ((0 == incrementValue) && (0 == partialIncrementValue)) {
        log.DebugFormat ("UpdateDay: " +
                         "incrementValue is 0: " +
                         "=> do nothing");
        return;
      }

      // Find any existing analysis in database
      ICycleDurationSummary cycleDurationSummary =
        FindByKey (machine, day, shift, workOrder, line, manufacturingOrder, component, operation, offset);
      if (null == cycleDurationSummary) {
        // The persistent class does not exist in database, create it
        cycleDurationSummary = new CycleDurationSummary (machine,
                                                         day,
                                                         shift,
                                                         workOrder,
                                                         line,
                                                         manufacturingOrder,
                                                         component,
                                                         operation,
                                                         offset);
        Debug.Assert (0 <= incrementValue);
        if ((0 == incrementValue) && (0 == partialIncrementValue)) {
          log.InfoFormat (".{0} UpdateDay: " +
                          "new summary with no full or partial cycles " +
                          "at day {1} " +
                          "=> skip it",
                          machine.Id,
                          day);
        }
        else if ((incrementValue < 0) || (partialIncrementValue < 0)) {
          log.WarnFormat (".{0} UpdateDay: " +
                          "new summary with a negative number of cycles {1} or partial {2} " +
                          "at day {3} " +
                          "=> skip it",
                          machine.Id,
                          incrementValue, partialIncrementValue,
                          day);
          // UNDONE: log it also in analysislog (but there is no access to the modification here)
        }
        else {
          cycleDurationSummary.Number = incrementValue;
          cycleDurationSummary.Partial = partialIncrementValue;
          Debug.Assert (0.0 <= cycleDurationSummary.Number);
          MakePersistent (cycleDurationSummary);
        }
        return;
      }
      else { // Update the existing analysis
        Debug.Assert (0 <= cycleDurationSummary.Number + incrementValue);
        Debug.Assert (0 <= cycleDurationSummary.Partial + partialIncrementValue);
        if ((cycleDurationSummary.Number + incrementValue <= 0)
            && (cycleDurationSummary.Partial + partialIncrementValue <= 0)) {
          if ((0 == cycleDurationSummary.Number + incrementValue)
              && (0 == cycleDurationSummary.Partial + partialIncrementValue)) {
            log.InfoFormat (".{0} UpdateDay: " +
                            "delete summary " +
                            "because the new number of full cycles would be {1} " +
                            "and partial cycles {2} " +
                            "at day {3}",
                            machine.Id,
                            cycleDurationSummary.Number + incrementValue,
                            cycleDurationSummary.Partial + partialIncrementValue,
                            day);
          }
          else { // < 0
            log.WarnFormat (".{0} UpdateDay: " +
                            "delete summary " +
                            "because the new number would be full={1} partial={2} " +
                            "at day {3}",
                            machine.Id,
                            cycleDurationSummary.Number + incrementValue,
                            cycleDurationSummary.Partial + partialIncrementValue,
                            day);
            // UNDONE: log it also in analysislog (but there is no access to the modification here)
          }
          MakeTransient (cycleDurationSummary);
        }
        else {
          cycleDurationSummary.Number += incrementValue;
          cycleDurationSummary.Partial += partialIncrementValue;
          Debug.Assert ((0 < cycleDurationSummary.Number) || (0 < cycleDurationSummary.Partial));
          if (cycleDurationSummary.Number < 0) {
            log.WarnFormat (".{0} UpdateDay: " +
                            "the new number of full cycles is negative {1} " +
                            "=> correct it to 0",
                            machine.Id,
                            cycleDurationSummary.Number);
            cycleDurationSummary.Number = 0;
          }
          if (cycleDurationSummary.Partial < 0) {
            log.WarnFormat (".{0} UpdateDay: " +
                            "the new number of partial cycles is negative {1} " +
                            "=> correct it to 0",
                            machine.Id,
                            cycleDurationSummary.Partial);
            cycleDurationSummary.Partial = 0;
          }
          Debug.Assert (0 <= cycleDurationSummary.Number);
          Debug.Assert (0 <= cycleDurationSummary.Partial);
          MakePersistent (cycleDurationSummary);
        }
        return;
      }
    }
  }
}
