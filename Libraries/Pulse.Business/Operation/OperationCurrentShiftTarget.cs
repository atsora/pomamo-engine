// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Business;
using Lemoine.Collections;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business.Group;
using Lemoine.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static log4net.Appender.RollingFileAppender;

namespace Lemoine.Business.Operation
{
  /// <summary>
  /// Response class for the OperationCurrentShiftTarget business request
  /// </summary>
  public sealed class OperationCurrentShiftTargetResponse
  {
    /// <summary>
    /// Date/time of the data
    /// </summary>
    public DateTime DateTime { get; internal set; }

    /// <summary>
    /// Associated component
    /// </summary>
    public IComponent Component { get; internal set; }

    /// <summary>
    /// Associated operation
    /// </summary>
    public IOperation Operation { get; internal set; }

    /// <summary>
    /// Associated day
    /// </summary>
    public DateTime? Day { get; internal set; }

    /// <summary>
    /// Associated shift
    /// </summary>
    public IShift Shift { get; internal set; }

    /// <summary>
    /// UTC date/time range of the current production (effective operation for the current shift)
    /// </summary>
    public UtcDateTimeRange Range { get; internal set; }

    /// <summary>
    /// <see cref="IPartProductionCurrentShiftResponse"/>
    /// </summary>
    public TimeSpan? CycleDurationTarget { get; internal set; }

    /// <summary>
    /// Goal for the whole shift
    /// </summary>
    public double? ShiftGoal { get; set; }
  }

  /// <summary>
  /// Request class to get the production target (at the end of the shift) for the current shift
  /// </summary>
  public sealed class OperationCurrentShiftTarget
    : IRequest<OperationCurrentShiftTargetResponse>
  {
    readonly IMachine m_requestedMachine;
    readonly IGroup m_group;
    readonly string m_groupId;

    readonly ILog log = LogManager.GetLogger (typeof (OperationCurrentShiftTarget).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="group">not null and</param>
    public OperationCurrentShiftTarget (IGroup group)
    {
      Debug.Assert (null != group);

      m_requestedMachine = null;
      m_group = group;
      m_groupId = group.Id;

      log = LogManager.GetLogger ($"{typeof (OperationCurrentShiftTarget).FullName}.{m_groupId}");
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="groupId">not null and</param>
    public OperationCurrentShiftTarget (string groupId)
    {
      Debug.Assert (!string.IsNullOrEmpty (groupId));

      m_requestedMachine = null;
      m_group = null;
      m_groupId = groupId;

      log = LogManager.GetLogger ($"{typeof (OperationCurrentShiftTarget).FullName}.{m_groupId}");
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    public OperationCurrentShiftTarget (IMachine machine)
    {
      Debug.Assert (null != machine);

      m_requestedMachine = machine;
      m_group = null;
      m_groupId = machine.Id.ToString ();

      log = LogManager.GetLogger ($"{typeof (OperationCurrentShiftTarget).FullName}.{m_groupId}");
    }

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public OperationCurrentShiftTargetResponse Get ()
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"Get: ...");
      }

      if (null != m_requestedMachine) {
        return GetByMachine (m_requestedMachine);
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
    public async System.Threading.Tasks.Task<OperationCurrentShiftTargetResponse> GetAsync ()
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"GetAsync: ...");
      }

      if (null != m_requestedMachine) {
        return await GetByMachineAsync (m_requestedMachine);
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

    OperationCurrentShiftTargetResponse GetByMachine (IMachine machine)
    {
      // - Get the effective operation slots + associated component/operation
      var effectiveOperationCurrentShiftRequest = new Lemoine.Business.Operation
          .EffectiveOperationCurrentShift (machine);
      var effectiveOperationCurrentShiftResponse = Lemoine.Business.ServiceProvider
        .Get (effectiveOperationCurrentShiftRequest);
      var effectiveOperationSlots = effectiveOperationCurrentShiftResponse
        .OperationSlots;
      var virtualOperationSlot = effectiveOperationCurrentShiftResponse.VirtualOperationSlot;

      var response = new OperationCurrentShiftTargetResponse () {
        DateTime = DateTime.UtcNow,
        Component = effectiveOperationCurrentShiftResponse.Component,
        Operation = effectiveOperationCurrentShiftResponse.Operation,
        Day = effectiveOperationCurrentShiftResponse.Day,
        Shift = effectiveOperationCurrentShiftResponse.Shift,
        Range = effectiveOperationCurrentShiftResponse.Range
      };

      if ((response.Operation is null) || !response.Day.HasValue || (response.Shift is null) || (response.Range is null) || response.Range.IsEmpty ()) {
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

      response.Range = new UtcDateTimeRange (response.Range.Lower, dateTimeRange.Upper);
      if (log.IsDebugEnabled) {
        log.Debug ($"GetByMachine: operation shift range is {dateTimeRange}");
      }

      var standardProductionTargetPerHourRequest = new Lemoine.Business.Operation
        .StandardProductionTargetPerHour (machine, response.Operation);
      var standardProductionTargetPerHour = Lemoine.Business.ServiceProvider
        .Get (standardProductionTargetPerHourRequest);
      if (standardProductionTargetPerHour.HasValue) {
        var productionDuration = GetProductionDuration (machine, response.Range);
        response.ShiftGoal = productionDuration.TotalHours * standardProductionTargetPerHour.Value;
        if (log.IsDebugEnabled) {
          log.Debug ($"GetByMachine: shiftGoal={response.ShiftGoal} from standard target={standardProductionTargetPerHour} and duration={dateTimeRange.Duration}");
        }
      }
      else { 
        log.Info ($"GetByMachine: no standard production target per hour for operation {response.Operation?.Id} => return an incomplete result");
      }

      return response;
    }

    async Task<OperationCurrentShiftTargetResponse> GetByMachineAsync (IMachine machine)
    {
      // - Get the effective operation slots + associated component/operation
      var effectiveOperationCurrentShiftRequest = new Lemoine.Business.Operation
          .EffectiveOperationCurrentShift (machine);
      var effectiveOperationCurrentShiftResponse = await Lemoine.Business.ServiceProvider
        .GetAsync (effectiveOperationCurrentShiftRequest);
      var effectiveOperationSlots = effectiveOperationCurrentShiftResponse
        .OperationSlots;
      var virtualOperationSlot = effectiveOperationCurrentShiftResponse.VirtualOperationSlot;

      var response = new OperationCurrentShiftTargetResponse () {
        DateTime = DateTime.UtcNow,
        Component = effectiveOperationCurrentShiftResponse.Component,
        Operation = effectiveOperationCurrentShiftResponse.Operation,
        Day = effectiveOperationCurrentShiftResponse.Day,
        Shift = effectiveOperationCurrentShiftResponse.Shift,
        Range = effectiveOperationCurrentShiftResponse.Range
      };

      if ((response.Operation is null) || !response.Day.HasValue || (response.Shift is null) || (response.Range is null) || response.Range.IsEmpty ()) {
        log.Info ($"GetByMachine: no range, operation, day or shift => return an incomplete result");
        return response;
      }

      var operationShiftRequest = new Lemoine.Business.Shift.OperationShift (machine, response.Day.Value, response.Shift);
      UtcDateTimeRange dateTimeRange;
      try {
        dateTimeRange = await Lemoine.Business.ServiceProvider.GetAsync (operationShiftRequest);
      }
      catch (Exception ex) {
        log.Error ($"GetByMachine: OperationShift returned an exception", ex);
        throw;
      }
      if ((dateTimeRange is null) || dateTimeRange.IsEmpty () || !dateTimeRange.Duration.HasValue) {
        log.Error ($"GetByMachine: no valid date/time range for day={response.Day} and shift={response.Shift?.Id} => give up");
        return response;
      }

      response.Range = new UtcDateTimeRange (response.Range.Lower, dateTimeRange.Upper);
      if (log.IsDebugEnabled) {
        log.Debug ($"GetByMachine: operation shift range is {dateTimeRange}");
      }

      var standardProductionTargetPerHourRequest = new Lemoine.Business.Operation
        .StandardProductionTargetPerHour (machine, response.Operation);
      var standardProductionTargetPerHour = await Lemoine.Business.ServiceProvider
        .GetAsync (standardProductionTargetPerHourRequest);
      if (standardProductionTargetPerHour.HasValue) {
        var productionDuration = GetProductionDuration (machine, response.Range);
        response.ShiftGoal = productionDuration.TotalHours * standardProductionTargetPerHour.Value;
        if (log.IsDebugEnabled) {
          log.Debug ($"GetByMachine: shiftGoal={response.ShiftGoal} from standard target={standardProductionTargetPerHour} and duration={dateTimeRange.Duration}");
        }
      }
      else {
        log.Info ($"GetByMachine: no standard production target per hour for operation {response.Operation?.Id} => return an incomplete result");
      }

      return response;
    }

    TimeSpan GetProductionDuration (IMachine machine, UtcDateTimeRange range, UtcDateTimeRange preLoadRange = null)
    {
      return Lemoine.Business.MachineObservationState.Production.GetProductionDuration (machine, range, preLoadRange);
    }

    OperationCurrentShiftTargetResponse GetByGroup (IGroup group)
    {
      Debug.Assert (null != group);

      var machines = group.GetMachines ();
      var aggregatingMachines = machines
        .Where (m => IsMachineAggregatingParts (group, m));
      var results = aggregatingMachines
        .Select (m => RequestByMachine (m));
      if (results.Any ()) {
        var result = new OperationCurrentShiftTargetResponse () {
          DateTime = results.Min (x => x.DateTime),
          Operation = GetOperation (results),
          Component = GetComponent (results),
          Day = GetDay (results),
          Shift = GetShift (results),
          Range = GetRange (results),
          ShiftGoal = Sum (results.Select (x => x.ShiftGoal)),
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
        return new OperationCurrentShiftTargetResponse ();
      }
    }

    async Task<OperationCurrentShiftTargetResponse> GetByGroupAsync (IGroup group)
    {
      Debug.Assert (null != group);

      var machines = group.GetMachines ();
      var aggregatingMachines = machines
        .Where (m => IsMachineAggregatingParts (group, m));
      var tasks = aggregatingMachines
        .Select (m => RequestByMachineAsync (m));
      var results = await Task.WhenAll (tasks);
      if (results.Any ()) {
        var result = new OperationCurrentShiftTargetResponse () {
          DateTime = results.Min (x => x.DateTime),
          Operation = GetOperation (results),
          Component = GetComponent (results),
          Day = GetDay (results),
          Shift = GetShift (results),
          Range = GetRange (results),
          ShiftGoal = Sum (results.Select (x => x.ShiftGoal)),
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
        return new OperationCurrentShiftTargetResponse ();
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
      var cacheKey = "Business.Operation.OperationCurrentShiftTarget." + m_groupId;
      return cacheKey;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (OperationCurrentShiftTargetResponse data)
    {
      return CacheTimeOut.CurrentLong.GetTimeSpan ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<OperationCurrentShiftTargetResponse> data)
    {
      if (null == data.Value) {
        return true;
      }
      else {
        return data.Value.Range?.ContainsElement (DateTime.UtcNow) ?? true;
      }
    }
    #endregion // IRequest implementation
  }
}
