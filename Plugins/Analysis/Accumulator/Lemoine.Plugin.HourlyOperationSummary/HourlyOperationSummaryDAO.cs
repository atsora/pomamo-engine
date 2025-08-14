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
using System.Linq;
using Lemoine.Collections;

namespace Lemoine.Plugin.HourlyOperationSummary
{
  public class HourlyOperationSummaryDAO
    : VersionableByMachineNHibernateDAO<HourlyOperationSummary, IHourlyOperationSummary, int>
  {
    readonly ILog log = LogManager.GetLogger<HourlyOperationSummaryDAO> ();

    /// <summary>
    /// Constructor
    /// </summary>
    public HourlyOperationSummaryDAO ()
      : base ("Machine")
    { }

    public override IHourlyOperationSummary MakePersistent (IHourlyOperationSummary entity)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"MakePersistent: id={entity.Id} machineId={entity.Machine.Id} hour={entity.LocalDateHour}");
      }
      if (TimeSpan.FromHours (1) < entity.Duration) {
        log.Warn ($"MakePersistent: duration {entity.Duration} is greater than 1 hour for machine={entity.Machine.Id} hour={entity.LocalDateHour}");
        if (TimeSpan.FromHours (2) < entity.Duration) {
          log.Fatal ($"MakePersistent: invalid duration {entity.Duration} for machine={entity.Machine.Id} hour={entity.LocalDateHour}");
          Debug.Assert (false);
        }
      }
      NHibernateHelper.GetCurrentSession ()
        .SaveOrUpdate ("HourlyOperationSummary", entity);
      return entity;
    }

    /// <summary>
    /// Find the unique item that may match the natural key
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="operation">nullable</param>
    /// <param name="component">nullable</param>
    /// <param name="workOrder">nullable</param>
    /// <param name="line">nullable</param>
    /// <param name="manufacturingOrder"></param>
    /// <param name="day">nullable</param>
    /// <param name="shift">nullable</param>
    /// <param name="localDateHour"></param>
    /// <returns></returns>
    public IHourlyOperationSummary FindByKey (IMachine machine,
                                              IOperation operation,
                                              IComponent component,
                                              IWorkOrder workOrder,
                                              ILine line,
                                              IManufacturingOrder manufacturingOrder,
                                              DateTime? day,
                                              IShift shift,
                                              DateTime localDateHour)
    {
      Debug.Assert (null != machine);
      Debug.Assert (null != operation);

      ICriteria criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<HourlyOperationSummary> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id));
      if (null == operation) {
        criteria = criteria.Add (Restrictions.IsNull ("Operation"));
      }
      else {
        criteria = criteria.Add (Restrictions.Eq ("Operation.Id", ((IDataWithId)operation).Id));
      }
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
      if (null == manufacturingOrder) {
        criteria = criteria.Add (Restrictions.IsNull ("ManufacturingOrder"));
      }
      else {
        criteria = criteria.Add (Restrictions.Eq ("ManufacturingOrder.Id", ((IDataWithId)manufacturingOrder).Id));
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
      criteria = criteria.Add (Restrictions.Eq ("LocalDateHour", localDateHour));
      var result = criteria
        .UniqueResult<IHourlyOperationSummary> ();
      if (log.IsDebugEnabled) {
        if (null == result) {
          log.Debug ($"FindByKey({machine.Id}): key: Operation:{operation} Component:{component} WorkOrder:{workOrder} Line:{line} ManufacturingOrder:{manufacturingOrder} Day:{day} Shift:{shift} LocalDateHour:{localDateHour} / Result: null");
        }
        else {
          log.Debug ($"FindByKey({machine.Id}): key: Operation:{operation} Component:{component} WorkOrder:{workOrder} Line:{line} ManufacturingOrder:{manufacturingOrder} Day:{day} Shift:{shift} / Result: Operation:{result.Operation} Component:{result.Component} WorkOrder:{result.WorkOrder} Line:{result.Line} ManufacturingOrder:{result.ManufacturingOrder} Day:{result.Day} Shift:{result.Shift} LocalDateHour:{result.LocalDateHour}");
        }
      }
      return result;
    }

    /// <summary>
    /// Find all the items that match the specified machine, day and shift
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="day"></param>
    /// <param name="shift"></param>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<IList<IHourlyOperationSummary>> FindByDayShiftAsync (IMonitoredMachine machine, DateTime day, IShift shift)
    {
      Debug.Assert (null != machine);

      var criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<HourlyOperationSummary> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (Restrictions.Eq ("Day", day));
      if (null == shift) {
        criteria.Add (Restrictions.IsNull ("Shift"));
      }
      else {
        criteria.Add (Restrictions.Eq ("Shift.Id", shift.Id));
      }
      return await criteria.ListAsync<IHourlyOperationSummary> ();
    }

    /// <summary>
    /// Find all the items that match the specified machine and hour
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="localDateHour"></param>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<IList<IHourlyOperationSummary>> FindByHourAsync (IMonitoredMachine machine, DateTime localDateHour)
    {
      Debug.Assert (null != machine);

      return await NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<HourlyOperationSummary> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (Restrictions.Eq ("LocalDateHour", localDateHour))
        .ListAsync<IHourlyOperationSummary> ();
    }

    /// <summary>
    /// Find all the items that match the specified machine and hour
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="localDateHourRange">not null</param>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<IList<IHourlyOperationSummary>> FindByHourAsync (IMonitoredMachine machine, LocalDateTimeRange localDateHourRange)
    {
      Debug.Assert (null != machine);

      if (localDateHourRange.IsEmpty ()) {
        if (log.IsDebugEnabled) {
          log.Debug ($"FindByHourAsync: empty range in parameter");
        }
        return new List<IHourlyOperationSummary> { };
      }

      var criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<HourlyOperationSummary> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id));
      if (localDateHourRange.Lower.HasValue) {
        if (localDateHourRange.LowerInclusive) {
          criteria = criteria
            .Add (Restrictions.Ge ("LocalDateHour", localDateHourRange.Lower.Value));
        }
        else { // Not inclusive
          criteria = criteria
            .Add (Restrictions.Gt ("LocalDateHour", localDateHourRange.Lower.Value));
        }
      }
      if (localDateHourRange.Upper.HasValue) {
        if (localDateHourRange.UpperInclusive) {
          criteria = criteria
            .Add (Restrictions.Le ("LocalDateHour", localDateHourRange.Upper.Value));
        }
        else {
          criteria = criteria
            .Add (Restrictions.Lt ("LocalDateHour", localDateHourRange.Upper.Value));
        }
      }
      return await criteria
        .ListAsync<IHourlyOperationSummary> ();
    }

    /// <summary>
    /// Find all the items that match the specified operation
    /// </summary>
    /// <param name="operation">not null</param>
    /// <returns></returns>
    public IList<IHourlyOperationSummary> FindByOperation (IOperation operation)
    {
      Debug.Assert (null != operation);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<HourlyOperationSummary> ()
        .Add (Restrictions.Eq ("Operation.Id", ((IDataWithId)operation).Id))
        .List<IHourlyOperationSummary> ();
    }

    /// <summary>
    /// Find all the items that match the specified component
    /// </summary>
    /// <param name="component">not null</param>
    /// <returns></returns>
    public IList<IHourlyOperationSummary> FindByComponent (IComponent component)
    {
      Debug.Assert (null != component);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<HourlyOperationSummary> ()
        .Add (Restrictions.Eq ("Component.Id", ((IDataWithId)component).Id))
        .List<IHourlyOperationSummary> ();
    }

    /// <summary>
    /// Find all the items that match the specified work order
    /// </summary>
    /// <param name="workOrder">not null</param>
    /// <returns></returns>
    public IList<IHourlyOperationSummary> FindByWorkOrder (IWorkOrder workOrder)
    {
      Debug.Assert (null != workOrder);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<HourlyOperationSummary> ()
        .Add (Restrictions.Eq ("WorkOrder.Id", ((IDataWithId)workOrder).Id))
        .List<IHourlyOperationSummary> ();
    }

    /// <summary>
    /// Find all the items that match the specified machine and manufacturing order
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="manufacturingOrder">not null</param>
    /// <returns></returns>
    public IList<IHourlyOperationSummary> FindByManufacturingOrder (IMachine machine, IManufacturingOrder manufacturingOrder)
    {
      Debug.Assert (null != machine);
      Debug.Assert (null != manufacturingOrder);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<HourlyOperationSummary> ()
        .Add (Restrictions.Eq ("Machine.Id", machine.Id))
        .Add (Restrictions.Eq ("ManufacturingOrder.Id", ((IDataWithId)manufacturingOrder).Id))
        .List<IHourlyOperationSummary> ();
    }

    public void UpdateOffset (IMachine machine, IOperation operation, IComponent component, IWorkOrder workOrder, ILine line, IManufacturingOrder manufacturingOrder, DateTime? day, IShift shift, DateTime localDateHour, TimeSpan durationOffset, int totalCyclesOffset, int adjustedCyclesOffset, int adjustedQuantityOffset)
    {
      var item = FindByKey (machine, operation, component, workOrder, line, manufacturingOrder, day, shift, localDateHour);
      if (null == item) {
        var maxDuration = TimeSpan.FromHours (2);
        if (log.IsWarnEnabled && ((durationOffset.TotalSeconds < 0) || (totalCyclesOffset < 0) || (adjustedCyclesOffset < 0) || (adjustedQuantityOffset < 0) || (maxDuration < durationOffset))) {
          log.Warn ($"UpdateOffset({machine.Id}): invalid duration {durationOffset} or total cycles {totalCyclesOffset} or adjusted cycles {adjustedCyclesOffset} or adjusted quantity {adjustedQuantityOffset} for machine {machine.Id} for a new key => skip it (may happen when the plugin has just been activated)");
        }
        // TODO: correct the negative values
        var duration = durationOffset;
        if (duration.Ticks <= 0) {
          var range = new UtcDateTimeRange (localDateHour, localDateHour.AddHours (1));
          var operationSlots = ModelDAOHelper.DAOFactory.OperationSlotDAO
            .FindOverlapsRange (machine, range)
            .Where (s => Operation.Equals (s.Operation, operation)
              && Component.Equals (s.Component, component)
              && WorkOrder.Equals (s.WorkOrder, workOrder)
              && Line.Equals (s.Line, line)
              && ManufacturingOrder.Equals (s.ManufacturingOrder, manufacturingOrder)
              && object.Equals (s.Day, day)
              && Shift.Equals (s.Shift, shift));
          var intersections = operationSlots
            .Select (s => new UtcDateTimeRange (s.DateTimeRange.Intersects (range)))
            .Where (r => r.Duration.HasValue);
          duration = TimeSpan.FromTicks (intersections.Sum (r => r.Duration.Value.Ticks));
          if (log.IsDebugEnabled) {
            log.Debug ($"UpdateOffset({machine.Id}): computed duration is {duration} for localDateHour {localDateHour} since durationOffset was <= 0");
          }
        }
        else if (maxDuration < duration) {
          log.Warn ($"UpdateOffset({machine.Id}): duration for a new created item was {duration} which is greater than {maxDuration} hours"); // Because of the daylight saving time
          duration = TimeSpan.FromHours (1);
        }
        if (log.IsDebugEnabled) {
          log.Debug ($"UpdateOffset({machine.Id}): new item with duration offset {durationOffset} cycles offset {totalCyclesOffset}, duration={duration}");
        }
        if (duration.Ticks <= 0) {
          if (duration.Ticks < 0) {
            log.Fatal ($"UpdateOffset({machine.Id}): unexpected computed duration {duration} <= 0 for localDateHour={localDateHour}");
          }
          else if (log.IsDebugEnabled) { // = 0s
            log.Debug ($"UpdateOffset({machine.Id}): computed duration {duration} still 0 for localDateHour={localDateHour}");
          }
          if ((totalCyclesOffset <= 0) && (adjustedCyclesOffset <= 0) && (adjustedQuantityOffset <= 0)) { // Empty data => do not store anything
            return;
          }
        }
        item = new HourlyOperationSummary (machine, operation, component, workOrder, line, manufacturingOrder, day, shift, localDateHour);
        item.Duration = duration;
        item.TotalCycles = totalCyclesOffset >= 0 ? totalCyclesOffset : 0;
        item.AdjustedCycles = adjustedCyclesOffset >= 0 ? adjustedCyclesOffset : 0;
        item.AdjustedQuantity = adjustedQuantityOffset >= 0 ? adjustedQuantityOffset : 0;
        MakePersistent (item);
      }
      else { // Existing key
        var newDuration = item.Duration.Add (durationOffset);
        var newTotalCycles = item.TotalCycles + totalCyclesOffset;
        var newAdjustedCycles = item.AdjustedCycles + adjustedCyclesOffset;
        var newAdjustedQuantity = item.AdjustedQuantity + adjustedQuantityOffset;
        if (log.IsWarnEnabled && ((newDuration.TotalSeconds < 0) || (newTotalCycles < 0) || (newAdjustedCycles < 0))) {
          log.Warn ($"UpdateOffset({machine.Id}): invalid new duration offset {durationOffset} or cycles offset {totalCyclesOffset} or pieces offset {adjustedCyclesOffset} for machine {machine.Id} for an existing key => skip it (may happen shortly after the plugin is activated)");
        }
        if (log.IsDebugEnabled) {
          log.Debug ($"UpdateOffset({machine.Id}): duration offset {durationOffset} cycles offset {totalCyclesOffset}");
        }
        if (newDuration.Ticks < 0) {
          log.Warn ($"UpdateOffset({machine.Id}): replace negative newDuration={newDuration} by 0s");
          item.Duration = TimeSpan.FromTicks (0);
        }
        else if (TimeSpan.FromHours (2) < newDuration) {
          log.Warn ($"UpdateOffset({machine.Id}): new duration {newDuration} = {item.Duration} + {durationOffset} is greater than two hours"); // Because of the daylight saving time
          item.Duration = TimeSpan.FromHours (1);
        }
        else {
          item.Duration = newDuration;
        }
        item.TotalCycles = newTotalCycles >= 0 ? newTotalCycles : 0;
        item.AdjustedCycles = newAdjustedCycles >= 0 ? newAdjustedCycles : 0;
        item.AdjustedQuantity = newAdjustedQuantity >= 0 ? newAdjustedQuantity : 0;
        MakePersistent (item);
      }
    }

    /// <summary>
    /// Delete all the items that match the specified machine and hour
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="localDateHourRange">not null</param>
    /// <returns></returns>
    public void Delete (IMonitoredMachine machine, LocalDateTimeRange localDateHourRange)
    {
      Debug.Assert (null != machine);
      Debug.Assert (null != localDateHourRange);

      if (localDateHourRange.IsEmpty ()) {
        if (log.IsDebugEnabled) {
          log.Debug ($"FindByHourAsync: empty range in parameter");
        }
        return;
      }

      var query = $"delete from Lemoine.Plugin.HourlyOperationSummary.HourlyOperationSummary hos where hos.Machine.Id={machine.Id}";
      if (localDateHourRange.Lower.HasValue) {
        var comparison = localDateHourRange.LowerInclusive ? ">=" : ">";
        query += $" and hos.LocalDateHour {comparison} '{localDateHourRange.Lower.Value.ToString ("yyyy-MM-dd HH:mm:ss")}'";
      }
      if (localDateHourRange.Upper.HasValue) {
        var comparison = localDateHourRange.UpperInclusive ? "<=" : "<";
        query += $" and hos.LocalDateHour {comparison} '{localDateHourRange.Upper.Value.ToString ("yyyy-MM-dd HH:mm:ss")}'";
      }
      try {
        var criteria = NHibernateHelper.GetCurrentSession ()
          .CreateQuery (query)
          .ExecuteUpdate ();
      }
      catch (Exception ex) {
        log.Error ($"Delete: HQL={query} failed", ex);
        throw;
      }
    }

    /// <summary>
    /// Delete all the items that match the specified machine and hour
    /// </summary>
    /// <param name="localDateHourRange">not null</param>
    /// <returns></returns>
    public void Delete (LocalDateTimeRange localDateHourRange)
    {
      Debug.Assert (null != localDateHourRange);

      if (localDateHourRange.IsEmpty ()) {
        if (log.IsDebugEnabled) {
          log.Debug ($"FindByHourAsync: empty range in parameter");
        }
        return;
      }

      var where = "where";
      var query = $"delete from Lemoine.Plugin.HourlyOperationSummary.HourlyOperationSummary hos";
      if (localDateHourRange.Lower.HasValue) {
        var comparison = localDateHourRange.LowerInclusive ? ">=" : ">";
        query += $" {where} hos.LocalDateHour {comparison} '{localDateHourRange.Lower.Value.ToString ("yyyy-MM-dd HH:mm:ss")}'";
        where = "and";
      }
      if (localDateHourRange.Upper.HasValue) {
        var comparison = localDateHourRange.UpperInclusive ? "<=" : "<";
        query += $" {where} hos.LocalDateHour {comparison} '{localDateHourRange.Upper.Value.ToString ("yyyy-MM-dd HH:mm:ss")}'";
      }
      try {
        var criteria = NHibernateHelper.GetCurrentSession ()
          .CreateQuery (query)
          .ExecuteUpdate ();
      }
      catch (Exception ex) {
        log.Error ($"Delete: HQL={query} failed", ex);
        throw;
      }
    }


  }
}
