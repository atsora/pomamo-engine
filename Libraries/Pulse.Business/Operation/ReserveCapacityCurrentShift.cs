// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Business;
using Lemoine.Collections;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business.Group;
using Lemoine.Model;

namespace Lemoine.Business.Operation
{
  /// <summary>
  /// Response class for the ReserveCapacityCurrentShift business request
  /// </summary>
  public sealed class ReserveCapacityCurrentShiftResponse
    : Lemoine.Business.Operation.PartProductionCurrentShiftOperationResponse
  {
    /// <summary>
    /// Remaining capacity
    /// </summary>
    public double? RemainingCapacity { get; set; }

    /// <summary>
    /// Goal for the whole shift
    /// </summary>
    public double? ShiftGoal { get; set; }

    /// <summary>
    /// Reserved cpaacity
    /// </summary>
    public double? ReserveCapacity { get; set; }
  }

  /// <summary>
  /// Request class to get the reserved capacity for the current shift
  /// </summary>
  public sealed class ReserveCapacityCurrentShift
    : IRequest<ReserveCapacityCurrentShiftResponse>
  {
    #region Members
    readonly IMachine m_machine;
    readonly IGroup m_group;
    readonly string m_groupId;
    #endregion // Members

    readonly ILog log = LogManager.GetLogger (typeof (ReserveCapacityCurrentShift).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="group">not null and</param>
    public ReserveCapacityCurrentShift (IGroup group)
    {
      Debug.Assert (null != group);

      m_machine = null;
      m_group = group;
      m_groupId = group.Id;

      log = LogManager.GetLogger ($"{typeof (ReserveCapacityCurrentShift).FullName}.{m_groupId}");
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="groupId">not null and</param>
    public ReserveCapacityCurrentShift (string groupId)
    {
      Debug.Assert (!string.IsNullOrEmpty (groupId));

      m_machine = null;
      m_group = null;
      m_groupId = groupId;

      log = LogManager.GetLogger ($"{typeof (ReserveCapacityCurrentShift).FullName}.{m_groupId}");
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    public ReserveCapacityCurrentShift (IMachine machine)
    {
      Debug.Assert (null != machine);

      m_machine = machine;
      m_group = null;
      m_groupId = machine.Id.ToString ();

      log = LogManager.GetLogger ($"{typeof (ReserveCapacityCurrentShift).FullName}.{m_groupId}");
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public ReserveCapacityCurrentShiftResponse Get ()
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"Get: ...");
      }

      if (null != m_machine) {
        return GetByMachine (m_machine);
      }

      var group = m_group;
      if (null == group) {
        var groupRequest = new Lemoine.Business.Machine.GroupFromId (m_groupId);
        group = Lemoine.Business.ServiceProvider.Get (groupRequest);
        if (null == group) {
          log.Error ($"Get: invalid group {m_groupId}");
          throw new InvalidOperationException ($"Invalid group {m_groupId}");
        }
      }

      if (group.SingleMachine) {
        return GetByMachine (group.GetMachines ().Single ());
      }
      else {
        return GetByGroup (group);
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<ReserveCapacityCurrentShiftResponse> GetAsync ()
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"GetAsync: ...");
      }

      if (null != m_machine) {
        return await GetByMachineAsync (m_machine);
      }

      var group = m_group;
      if (null == group) {
        var groupRequest = new Lemoine.Business.Machine.GroupFromId (m_groupId);
        group = await Lemoine.Business.ServiceProvider.GetAsync (groupRequest);
        if (null == group) {
          log.Error ($"GetAsync: invalid group {m_groupId}");
          throw new InvalidOperationException ($"Invalid group {m_groupId}");
        }
      }

      if (group.SingleMachine) {
        return await GetByMachineAsync (group.GetMachines ().Single ());
      }
      else {
        return await GetByGroupAsync (group);
      }
    }

    ReserveCapacityCurrentShiftResponse GetByMachine (IMachine machine)
    {
      var partProductionCurrentShiftRequest = new Lemoine.Business.Operation.PartProductionCurrentShiftOperation (machine);
      var partProductionCurrentShift = Lemoine.Business.ServiceProvider
        .Get (partProductionCurrentShiftRequest);
      Debug.Assert (null != partProductionCurrentShift);

      var response = new ReserveCapacityCurrentShiftResponse () {
        DateTime = partProductionCurrentShift.DateTime,
        Operation = partProductionCurrentShift.Operation,
        Component = partProductionCurrentShift.Component,
        Day = partProductionCurrentShift.Day,
        Shift = partProductionCurrentShift.Shift,
        Range = partProductionCurrentShift.Range,
        NbPiecesCurrentShift = partProductionCurrentShift.NbPiecesCurrentShift,
        GoalCurrentShift = partProductionCurrentShift.GoalCurrentShift
      };

      if ((response.Operation is null) || (response.Range is null) || response.Range.IsEmpty () || !response.Day.HasValue || (response.Shift is null)) {
        log.Info ($"GetByMachine: no range, operation, day or shift => return an incomplete result");
        return response;
      }

      var operationShiftRequest = new Lemoine.Business.Shift.OperationShift (machine, response.Day.Value, response.Shift);
      UtcDateTimeRange dateTimeRange;
      try {
        dateTimeRange = Lemoine.Business.ServiceProvider.Get (operationShiftRequest);
      }
      catch (Exception ex) {
        log.Error ($"GetByMachine: OperationShift returned an exception", ex);
        throw;
      }
      if ((dateTimeRange is null) || dateTimeRange.IsEmpty () || !dateTimeRange.Duration.HasValue) {
        log.Error ($"GetByMachine: no valid date/time range for day={response.Day} and shift={response.Shift?.Id} => give up");
        return response;
      }
      var remainingProductionDuration = GetRemainingProductionDuration (machine, dateTimeRange, response.DateTime);
      if (!remainingProductionDuration.HasValue) {
        log.Error ($"GetByMachine: no remaining time => give up");
        return response;
      }
      else if (0 == remainingProductionDuration.Value.Ticks) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetByMachine: remaining time is 0 => return immediately");
        }
        response.RemainingCapacity = 0.0;
        response.ShiftGoal = response.GoalCurrentShift;
        response.ReserveCapacity = 0.0;
        return response;
      }
      else { // remainingTime.HasValue
        // Production capacity and remaining capacity
        var productionCapacityPerHourRequest = new Lemoine.Business.Operation
          .ProductionCapacityPerHour (machine, response.Operation);
        var productionCapacityPerHour = Lemoine.Business.ServiceProvider
          .Get (productionCapacityPerHourRequest);
        if (productionCapacityPerHour.HasValue) {
          response.RemainingCapacity = remainingProductionDuration.Value.TotalHours * productionCapacityPerHour.Value;
          if (log.IsDebugEnabled) {
            log.Debug ($"GetByMachine: remainingCapacity={response.RemainingCapacity} from productionCapacity={productionCapacityPerHour}");
          }
        }

        // Shift goal
        var standardProductionTargetPerHourRequest = new Lemoine.Business.Operation
          .StandardProductionTargetPerHour (machine, response.Operation);
        var standardProductionTargetPerHour = Lemoine.Business.ServiceProvider
          .Get (standardProductionTargetPerHourRequest);
        if (standardProductionTargetPerHour.HasValue) {
          var productionDuration = GetProductionDuration (machine, dateTimeRange);
          response.ShiftGoal = productionDuration.TotalHours * standardProductionTargetPerHour.Value;
          if (log.IsDebugEnabled) {
            log.Debug ($"GetByMachine: shiftGoal={response.ShiftGoal} from standard target={standardProductionTargetPerHour} and duration={dateTimeRange.Duration}");
          }
        }

        // Reserved capacity
        if (response.RemainingCapacity.HasValue && response.ShiftGoal.HasValue && response.NbPiecesCurrentShift.HasValue) {
          response.ReserveCapacity = response.RemainingCapacity.Value - (response.ShiftGoal.Value - response.NbPiecesCurrentShift.Value);
          if (log.IsDebugEnabled) {
            log.Debug ($"GetByMachine: ReserveCapacity={response.ReserveCapacity}");
          }
        }
      }

      return response;
    }

    async Task<ReserveCapacityCurrentShiftResponse> GetByMachineAsync (IMachine machine)
    {
      var partProductionCurrentShiftRequest = new Lemoine.Business.Operation.PartProductionCurrentShiftOperation (machine);
      var partProductionCurrentShift = await Lemoine.Business.ServiceProvider
        .GetAsync (partProductionCurrentShiftRequest);
      Debug.Assert (null != partProductionCurrentShift);

      var response = new ReserveCapacityCurrentShiftResponse () {
        DateTime = partProductionCurrentShift.DateTime,
        Operation = partProductionCurrentShift.Operation,
        Component = partProductionCurrentShift.Component,
        Day = partProductionCurrentShift.Day,
        Shift = partProductionCurrentShift.Shift,
        Range = partProductionCurrentShift.Range,
        NbPiecesCurrentShift = partProductionCurrentShift.NbPiecesCurrentShift,
        GoalCurrentShift = partProductionCurrentShift.GoalCurrentShift
      };

      if ((response.Operation is null) || (response.Range is null) || response.Range.IsEmpty () || !response.Day.HasValue || (response.Shift is null)) {
        log.Info ($"GetByMachineAsync: no range, operation, day or shift => return an incomplete result");
        return response;
      }

      var operationShiftRequest = new Lemoine.Business.Shift.OperationShift (machine, response.Day.Value, response.Shift);
      UtcDateTimeRange dateTimeRange;
      try {
        dateTimeRange = await Lemoine.Business.ServiceProvider.GetAsync (operationShiftRequest);
      }
      catch (Exception ex) {
        log.Error ($"GetByMachineAsync: OperationShift returned an exception", ex);
        throw;
      }
      if ((dateTimeRange is null) || dateTimeRange.IsEmpty () || !dateTimeRange.Duration.HasValue) {
        log.Error ($"GetByMachineAsync: no valid date/time range for day={response.Day} and shift={response.Shift?.Id} => give up");
        return response;
      }
      var remainingProductionDuration = GetRemainingProductionDuration (machine, dateTimeRange, response.DateTime);
      if (!remainingProductionDuration.HasValue) {
        log.Error ($"GetByMachineAsync: no remaining time => give up");
        return response;
      }
      else if (0 == remainingProductionDuration.Value.Ticks) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetByMachineAsync: remaining time is 0 => return immediately");
        }
        response.RemainingCapacity = 0.0;
        response.ShiftGoal = response.GoalCurrentShift;
        response.ReserveCapacity = 0.0;
        return response;
      }
      else { // remainingTime.HasValue
        // Production capacity and remaining capacity
        var productionCapacityPerHourRequest = new Lemoine.Business.Operation
          .ProductionCapacityPerHour (machine, response.Operation);
        var productionCapacityPerHour = await Lemoine.Business.ServiceProvider
          .GetAsync (productionCapacityPerHourRequest);
        if (productionCapacityPerHour.HasValue) {
          response.RemainingCapacity = remainingProductionDuration.Value.TotalHours * productionCapacityPerHour.Value;
          if (log.IsDebugEnabled) {
            log.Debug ($"GetByMachineAsync: remainingCapacity={response.RemainingCapacity} from productionCapacity={productionCapacityPerHour}");
          }
        }

        // Shift goal
        var standardProductionTargetPerHourRequest = new Lemoine.Business.Operation
          .StandardProductionTargetPerHour (machine, response.Operation);
        var standardProductionTargetPerHour = await Lemoine.Business.ServiceProvider
          .GetAsync (standardProductionTargetPerHourRequest);
        if (standardProductionTargetPerHour.HasValue) {
          var productionDuration = GetProductionDuration (machine, dateTimeRange);
          response.ShiftGoal = productionDuration.TotalHours * standardProductionTargetPerHour.Value;
          if (log.IsDebugEnabled) {
            log.Debug ($"GetByMachineAsync: shiftGoal={response.ShiftGoal} from standard target={standardProductionTargetPerHour} and duration={dateTimeRange.Duration}");
          }
        }

        // Reserved capacity
        if (response.RemainingCapacity.HasValue && response.ShiftGoal.HasValue && response.NbPiecesCurrentShift.HasValue) {
          response.ReserveCapacity = response.RemainingCapacity.Value - (response.ShiftGoal.Value - response.NbPiecesCurrentShift.Value);
          if (log.IsDebugEnabled) {
            log.Debug ($"GetByMachineAsync: ReserveCapacity={response.ReserveCapacity}");
          }
        }
      }

      return response;
    }

    TimeSpan? GetRemainingProductionDuration (IMachine machine, UtcDateTimeRange dateTimeRange, DateTime utcNow)
    {
      if (Bound.Compare<DateTime> (dateTimeRange.Lower, utcNow) < 0) {
        if (Bound.Compare<DateTime> (utcNow, dateTimeRange.Upper) < 0) {
          if (dateTimeRange.Upper.HasValue) {
            var range = new UtcDateTimeRange (utcNow, dateTimeRange.Upper);
            var result = GetProductionDuration (machine, range, dateTimeRange);
            if (log.IsDebugEnabled) {
              log.Debug ($"GetRemainingTime: {result} from range={dateTimeRange}");
            }
            return result;
          }
          else { // !Upper.HasValue
            log.Warn ($"GetRemainingTime: range={dateTimeRange} with no upper bound => return null");
            return null;
          }
        }
        else { // upper <= now => in the past 
          return TimeSpan.FromSeconds (0);
        }
      }
      else { // now <= lower => in the future
        var result = GetProductionDuration (machine, dateTimeRange);
        log.Warn ($"GetRemainingTime: range={dateTimeRange} in the future, return {result}");
        return result;
      }
    }

    TimeSpan GetProductionDuration (IMachine machine, UtcDateTimeRange range, UtcDateTimeRange preLoadRange = null)
    {
      return Lemoine.Business.MachineObservationState.Production.GetProductionDuration (machine, range, preLoadRange);
    }

    ReserveCapacityCurrentShiftResponse GetByGroup (IGroup group)
    {
      Debug.Assert (null != group);

      var machines = group.GetMachines ();
      var aggregatingMachines = machines
        .Where (m => IsMachineAggregatingParts (group, m));
      var results = aggregatingMachines
        .Select (m => RequestByMachine (m));
      if (results.Any ()) {
        var result = new ReserveCapacityCurrentShiftResponse () {
          DateTime = results.Min (x => x.DateTime),
          Operation = GetOperation (results),
          Component = GetComponent (results),
          Day = GetDay (results),
          Shift = GetShift (results),
          Range = GetRange (results),
          NbPiecesCurrentShift = Sum (results.Select (x => x.NbPiecesCurrentShift)),
          GoalCurrentShift = Sum (results.Select (x => x.GoalCurrentShift)),
          RemainingCapacity = Sum (results.Select (x => x.RemainingCapacity)),
          ShiftGoal = Sum (results.Select (x => x.ShiftGoal)),
          ReserveCapacity = Sum (results.Select (x => x.ReserveCapacity))
        };
        if (log.IsDebugEnabled) {
          log.Debug ($"GetByGroupAsync: return result for {group.Id}");
        }
        return result;
      }
      else {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetByGroupAsync: no data from {group.Id} => return an empty response");
        }
        return new ReserveCapacityCurrentShiftResponse ();
      }
    }

    async Task<ReserveCapacityCurrentShiftResponse> GetByGroupAsync (IGroup group)
    {
      Debug.Assert (null != group);

      var machines = group.GetMachines ();
      var aggregatingMachines = machines
        .Where (m => IsMachineAggregatingParts (group, m));
      var tasks = aggregatingMachines
        .Select (m => RequestByMachineAsync (m));
      var results = await Task.WhenAll (tasks);
      if (results.Any ()) {
        var result = new ReserveCapacityCurrentShiftResponse () {
          DateTime = results.Min (x => x.DateTime),
          Operation = GetOperation (results),
          Component = GetComponent (results),
          Day = GetDay (results),
          Shift = GetShift (results),
          Range = GetRange (results),
          NbPiecesCurrentShift = Sum (results.Select (x => x.NbPiecesCurrentShift)),
          GoalCurrentShift = Sum (results.Select (x => x.GoalCurrentShift)),
          RemainingCapacity = Sum (results.Select (x => x.RemainingCapacity)),
          ShiftGoal = Sum (results.Select (x => x.ShiftGoal)),
          ReserveCapacity = Sum (results.Select (x => x.ReserveCapacity))
        };
        if (log.IsDebugEnabled) {
          log.Debug ($"GetByGroupAsync: return result for {group.Id}");
        }
        return result;
      }
      else {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetByGroupAsync: no data from {group.Id} => return an empty response");
        }
        return new ReserveCapacityCurrentShiftResponse ();
      }
    }

    bool IsMachineAggregatingParts (IGroup group, IMachine machine)
    {
      Debug.Assert (null != group);

      if (group.IsMachineAggregatingParts is null) {
        return false;
      }
      else {
        return group.IsMachineAggregatingParts (machine);
      }
    }

    ReserveCapacityCurrentShiftResponse RequestByMachine (IMachine machine)
    {
      var request = new ReserveCapacityCurrentShift (machine);
      return Lemoine.Business.ServiceProvider
        .Get (request);
    }

    async Task<ReserveCapacityCurrentShiftResponse> RequestByMachineAsync (IMachine machine)
    {
      var request = new ReserveCapacityCurrentShift (machine);
      return await Lemoine.Business.ServiceProvider
        .GetAsync (request);
    }

    IOperation GetOperation (IEnumerable<ReserveCapacityCurrentShiftResponse> responses)
    {
      return responses.UniqueOrDefault (x => x.Operation, new EqualityComparerDataWithId<IOperation> ());
    }

    IComponent GetComponent (IEnumerable<ReserveCapacityCurrentShiftResponse> responses)
    {
      return responses.UniqueOrDefault (x => x.Component, new EqualityComparerDataWithId<IComponent> ());
    }

    DateTime? GetDay (IEnumerable<ReserveCapacityCurrentShiftResponse> responses)
    {
      return responses.UniqueOrDefault (x => x.Day);
    }

    IShift GetShift (IEnumerable<ReserveCapacityCurrentShiftResponse> responses)
    {
      return responses.UniqueOrDefault (x => x.Shift, new EqualityComparerFromId<IShift, int> (x => x.Id));
    }

    UtcDateTimeRange GetRange (IEnumerable<ReserveCapacityCurrentShiftResponse> responses)
    {
      return responses.UniqueOrDefault (x => x.Range);
    }

    double? Sum (IEnumerable<double?> values)
    {
      if (values.Any (x => !x.HasValue)) {
        return null;
      }
      else {
        return values.Sum (x => x.Value);
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return "Business.Shift.ReserveCapacityCurrentShift." + m_groupId;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (ReserveCapacityCurrentShiftResponse data)
    {
      return CacheTimeOut.CurrentShort.GetTimeSpan ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<ReserveCapacityCurrentShiftResponse> data)
    {
      if (null == data.Value) {
        return true;
      }
      else {
        return true;
      }
    }
    #endregion // IRequest implementation
  }
}
