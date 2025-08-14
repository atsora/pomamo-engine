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

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IWorkOrderToOperationOnlySlotDAO">IWorkOrderToOperationOnlySlotDAO</see>
  /// </summary>
  public class WorkOrderToOperationOnlySlotDAO
    : IWorkOrderToOperationOnlySlotDAO
  {
    readonly ILog log = LogManager.GetLogger(typeof (WorkOrderToOperationOnlySlotDAO).FullName);
    
    #region IWorkOrderToOperationOnlySlotDAO implementation
    /// <summary>
    /// IWorkOrderToOperationOnlySlotDAO implementation
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="dateTime"></param>
    /// <param name="extend">Try to extend the slot</param>
    /// <returns></returns>
    public IWorkOrderToOperationOnlySlot FindAt(IMachine machine, DateTime dateTime, bool extend)
    {
      Debug.Assert (null != machine);
      
      IWorkOrderToOperationOnlySlot slot = new WorkOrderToOperationOnlySlot (
        ModelDAOHelper.DAOFactory.OperationSlotDAO
        .FindAt (machine, dateTime));
      if (extend && (null != slot)) {
        slot = ExtendLeft (slot);
        slot = ExtendRight (slot);
      }
      return slot;
    }
    
    /// <summary>
    /// IWorkOrderToOperationOnlySlotDAO implementation
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <param name="extend">Try to extend the slot</param>
    /// <returns></returns>
    public IList<IWorkOrderToOperationOnlySlot> FindOverlapsRange (IMachine machine, UtcDateTimeRange range, bool extend)
    {
      Debug.Assert (null != machine);
      
      IList<IOperationSlot> operationSlots = ModelDAOHelper.DAOFactory.OperationSlotDAO
        .FindOverlapsRange (machine, range);
      return Merge (operationSlots, extend);
    }
    
    /// <summary>
    /// IWorkOrderToOperationOnlySlotDAO implementation
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <param name="extend">Try to extend the slot</param>
    /// <returns></returns>
    public IList<IWorkOrderToOperationOnlySlot> FindOverlapsRangeWithEagerFetch (IMachine machine, UtcDateTimeRange range, bool extend)
    {
      Debug.Assert (null != machine);
      
      IList<IOperationSlot> operationSlots = ModelDAOHelper.DAOFactory.OperationSlotDAO
        .FindOverlapsRangeWithEagerFetch (machine, range);
      return Merge (operationSlots, extend);
    }
    
    /// <summary>
    /// Check if two IMergeable object can be effectively be merged
    /// </summary>
    /// <param name="left">not null</param>
    /// <param name="right">not null</param>
    /// <returns></returns>
    bool IsMergeable (IWorkOrderToOperationOnlySlot left, IWorkOrderToOperationOnlySlot right)
    {
      if (left.DateTimeRange.IsEmpty ()) {
        Debug.Assert (!left.DateTimeRange.IsEmpty ());
        log.ErrorFormat ("IsMergeable: " +
                         "left range is empty, which is unexpected " +
                         "=> return true because it can be dismissed");
        return true;
      }
      if (right.DateTimeRange.IsEmpty ()) {
        Debug.Assert (!right.DateTimeRange.IsEmpty ());
        log.ErrorFormat ("IsMergeable: " +
                         "right range is empty, which is unexpected " +
                         "=> return true because it can be dismissed");
        return true;
      }
      
      return left.ReferenceDataEquals (right)
        && left.DateTimeRange.IsAdjacentTo (right.DateTimeRange);
    }
    
    /// <summary>
    /// Merge two mergeable items
    /// </summary>
    /// <param name="left">not null</param>
    /// <param name="right">not null</param>
    /// <returns></returns>
    IWorkOrderToOperationOnlySlot Merge (IWorkOrderToOperationOnlySlot left, IWorkOrderToOperationOnlySlot right)
    {
      Debug.Assert (IsMergeable (left, right));
      Debug.Assert (left.DateTimeRange.IsStrictlyLeftOf (right.DateTimeRange));
      
      // Note: the Union function supports empty ranges
      UtcDateTimeRange newRange =
        new UtcDateTimeRange (left.DateTimeRange.Union (right.DateTimeRange));
      DayRange newDayRange =
        new DayRange (left.DayRange.Union (right.DayRange));
      
      TimeSpan? runTime;
      if (left.RunTime.HasValue) {
        if (right.RunTime.HasValue) {
          runTime = left.RunTime.Value.Add (right.RunTime.Value);
        }
        else {
          runTime = left.RunTime.Value;
        }
      }
      else {
        runTime = right.RunTime;
      }
      TimeSpan? averageCycleTime; // The computation is a little bit approximative, but it is probably good enough
      if (left.AverageCycleTime.HasValue) {
        if (right.AverageCycleTime.HasValue) {
          int totalCycles = left.TotalCycles + right.TotalCycles;
          averageCycleTime = TimeSpan
            .FromSeconds (left.AverageCycleTime.Value.TotalSeconds *  left.TotalCycles / totalCycles
                          + right.AverageCycleTime.Value.TotalSeconds * right.TotalCycles / totalCycles);
        }
        else {
          averageCycleTime = left.AverageCycleTime;
        }
      }
      else {
        averageCycleTime = right.AverageCycleTime;
      }
      TimeSpan? productionDuration;
      if (left.ProductionDuration.HasValue) {
        if (right.ProductionDuration.HasValue) {
          productionDuration = left.ProductionDuration.Value.Add (right.ProductionDuration.Value);
        }
        else {
          productionDuration = left.ProductionDuration.Value;
        }
      }
      else {
        productionDuration = right.ProductionDuration;
      }
      IWorkOrderToOperationOnlySlot result = new WorkOrderToOperationOnlySlot (left.Machine,
                                                                               left.ManufacturingOrder,
                                                                               left.WorkOrder,
                                                                               left.Component,
                                                                               left.Operation,
                                                                               left.Display,
                                                                               runTime,
                                                                               left.TotalCycles + right.TotalCycles,
                                                                               left.AdjustedCycles + right.AdjustedCycles,
                                                                               left.AdjustedQuantity + right.AdjustedQuantity,
                                                                               left.PartialCycles + right.PartialCycles,
                                                                               averageCycleTime, productionDuration,
                                                                               newRange,
                                                                               newDayRange);
      return result;
    }
    
    IList<IWorkOrderToOperationOnlySlot> Merge (IList<IOperationSlot> operationSlots, bool extend)
    {
      IList<IWorkOrderToOperationOnlySlot> result = new List<IWorkOrderToOperationOnlySlot> ();
      foreach (var operationSlot in operationSlots) {
        IWorkOrderToOperationOnlySlot newSlot = new WorkOrderToOperationOnlySlot (operationSlot);
        if ( (1 <= result.Count)
            && IsMergeable (result [result.Count - 1], newSlot)) {
          result [result.Count - 1] =
            Merge (result [result.Count - 1], newSlot);
        }
        else {
          result.Add (newSlot);
        }
      }
      if (extend && (0 < result.Count)) {
        result [0] = ExtendLeft (result [0]);
        result [result.Count - 1] = ExtendRight (result [result.Count - 1]);
      }
      return result;
    }
    
    /// <summary>
    /// Extend a slot to the left
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    IWorkOrderToOperationOnlySlot ExtendLeft (IWorkOrderToOperationOnlySlot slot)
    {
      if (null == slot) {
        return null;
      }
      if (!slot.DateTimeRange.Lower.HasValue) {
        return slot;
      }
      else {
        IOperationSlot leftOperationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
          .FindWithEnd (slot.Machine, slot.DateTimeRange.Lower.Value);
        if (null == leftOperationSlot) {
          return slot;
        }
        else { // null != leftOperationSlot
          if (leftOperationSlot.IsEmpty ()){
            Debug.Assert (false);
            log.ErrorFormat ("ExtendLeft: " +
                             "empty left reason slot {0}",
                             leftOperationSlot);
            return slot;
          }
          IWorkOrderToOperationOnlySlot leftWorkOrderToOperationOnlySlot = new WorkOrderToOperationOnlySlot (leftOperationSlot);
          if (IsMergeable (leftWorkOrderToOperationOnlySlot, slot)) { // Extend it
            IWorkOrderToOperationOnlySlot merged =
              Merge (leftWorkOrderToOperationOnlySlot, slot);
            return this.ExtendLeft (merged);
          }
          else {
            return slot;
          }
        }
      }
    }

    /// <summary>
    /// Extend a slot to the right
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    IWorkOrderToOperationOnlySlot ExtendRight (IWorkOrderToOperationOnlySlot slot)
    {
      if (null == slot) {
        return null;
      }
      if (!slot.DateTimeRange.Upper.HasValue) {
        return slot;
      }
      else {
        IOperationSlot rightOperationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
          .FindAt (slot.Machine, slot.DateTimeRange.Upper.Value);
        if (null == rightOperationSlot) {
          return slot;
        }
        else { // null != rightOperationSlot
          if (rightOperationSlot.IsEmpty ()){
            Debug.Assert (false);
            log.ErrorFormat ("ExtendRight: " +
                             "empty right reason slot {0}",
                             rightOperationSlot);
            return slot;
          }
          IWorkOrderToOperationOnlySlot rightWorkOrderToOperationOnlySlot = new WorkOrderToOperationOnlySlot (rightOperationSlot);
          if (IsMergeable (rightWorkOrderToOperationOnlySlot, slot)) { // Extend it
            IWorkOrderToOperationOnlySlot merged = Merge (slot, rightWorkOrderToOperationOnlySlot);
            return this.ExtendRight (merged);
          }
          else {
            return slot;
          }
        }
      }
    }
    #endregion // IWorkOrderToOperationOnlySlotDAO implementation
  }
}
