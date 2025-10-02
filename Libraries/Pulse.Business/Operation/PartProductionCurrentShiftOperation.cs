// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using Lemoine.Core.Cache;
using Lemoine.Business;
using Lemoine.Core.Log;
using Lemoine.Model;
using System.Collections.Generic;
using System.Linq;
using Lemoine.ModelDAO;
using System.Threading.Tasks;
using Lemoine.Collections;

namespace Lemoine.Business.Operation
{
  /// <summary>
  /// Request class to get the part production machining status of the current shift
  /// for the currently effective operation.
  /// 
  /// The task is not considered here
  /// </summary>
  public sealed class PartProductionCurrentShiftOperation
    : IRequest<PartProductionCurrentShiftOperationResponse>
  {
    /// <summary>
    /// Options key for the minimum duration of the current no operation period
    /// </summary>
    static readonly string CURRENT_NO_OPERATION_MIN_DURATION_KEY = "Business.Operation.PartProductionCurrentShiftOperation..CurrentNoOperationMinDuration";
    /// <summary>
    /// Options default value for the minimum duration of the current no operation period
    /// </summary>
    static readonly TimeSpan CURRENT_NO_OPERATION_MIN_DURATION_DEFAULT = TimeSpan.FromSeconds (10);

    readonly IMachine m_machine;
    readonly DateTime? m_dateTime;

    static readonly ILog log = LogManager.GetLogger (typeof (PartProductionCurrentShiftOperation).FullName);

    /// <summary>
    /// Machine
    /// </summary>
    public IMachine Machine { get { return m_machine; } }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    public PartProductionCurrentShiftOperation (IMachine machine)
    {
      Debug.Assert (null != machine);

      m_machine = machine;
    }


    /// <summary>
    /// Alternative constructor
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="dateTime"></param>
    public PartProductionCurrentShiftOperation (IMachine machine, DateTime dateTime)
      : this (machine)
    {
      m_dateTime = dateTime;
    }

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public PartProductionCurrentShiftOperationResponse Get ()
    {
      var dateTime = m_dateTime ?? DateTime.UtcNow;

      var response = new PartProductionCurrentShiftOperationResponse ();
      response.RequestDateTime = m_dateTime;
      response.DateTime = dateTime;

      // - Get the effective operation slots + associated component/operation
      EffectiveOperationCurrentShift effectiveOperationCurrentShiftRequest;
      if (m_dateTime.HasValue) {
        effectiveOperationCurrentShiftRequest = new Lemoine.Business.Operation
          .EffectiveOperationCurrentShift (m_machine, dateTime);
      }
      else {
        effectiveOperationCurrentShiftRequest = new Lemoine.Business.Operation
          .EffectiveOperationCurrentShift (m_machine);
      }
      var effectiveOperationCurrentShiftResponse = Lemoine.Business.ServiceProvider
        .Get (effectiveOperationCurrentShiftRequest);
      var effectiveOperationSlots = effectiveOperationCurrentShiftResponse
        .OperationSlots;
      var virtualOperationSlot = effectiveOperationCurrentShiftResponse.VirtualOperationSlot;
      response.Component = effectiveOperationCurrentShiftResponse.Component;
      response.Operation = effectiveOperationCurrentShiftResponse.Operation;
      response.Day = effectiveOperationCurrentShiftResponse.Day;
      response.Shift = effectiveOperationCurrentShiftResponse.Shift;

      if (null != response.Operation) {
        // - Get the cycle properties
        var operationCyclePropertiesRequest = new Lemoine.Business.Operation
          .OperationCycleProperties (m_machine, response.Operation);
        var operationCyclePropertiesResponse = Lemoine.Business.ServiceProvider
          .Get (operationCyclePropertiesRequest);
        bool isLongCycle = operationCyclePropertiesResponse.IsLongCycle;
        int nbPiecesByCycle = operationCyclePropertiesResponse.NbPiecesByCycle;

        var standardProductionTargetPerHour = GetStandardProductionTargetPerHour (response.Operation);

        if (standardProductionTargetPerHour.HasValue) {
          // - Get the production durations
          TimeSpan shiftProductionDuration =
            GetProductionDuration (dateTime,
                                   effectiveOperationSlots);

          // - Compute the expected number of parts
          if (response.Day.HasValue
              && (null != response.Shift)) {
            response.GoalCurrentShift = shiftProductionDuration.TotalHours * standardProductionTargetPerHour.Value;
            if (!isLongCycle) {
              // Consider the number of pieces by cycle in case it is rounded (not a long cycle)
              if (response.GoalCurrentShift.HasValue) {
                response.GoalCurrentShift = nbPiecesByCycle * (((int)response.GoalCurrentShift.Value) / nbPiecesByCycle);
              }
            }
          }
        }

        response.CycleDurationTarget = GetCycleDurationTarget (response.Operation, standardProductionTargetPerHour, nbPiecesByCycle);

        // - Get the number of produced parts
        if ( (null != effectiveOperationSlots) && effectiveOperationSlots.Any ()) {
          response.NbPiecesCurrentShift = GetNumberOfProducedParts (effectiveOperationSlots, nbPiecesByCycle, isLongCycle);
          if (!isLongCycle) {
            response.NbPiecesCurrentShift = (int?)response.NbPiecesCurrentShift;
          }
        }
      }
      else { // null == operation
             // - Get an appromative number of produced parts
        if ( (null != effectiveOperationSlots) && effectiveOperationSlots.Any ()) {
          response.NbPiecesCurrentShift = (int?)GetNumberOfProducedParts (effectiveOperationSlots, 1,
            false);
        }
      }

      if (null != virtualOperationSlot) {
        response.Range = virtualOperationSlot.DateTimeRange;
      }
      else {
        response.Range = new UtcDateTimeRange ();
      }

      return response;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async Task<PartProductionCurrentShiftOperationResponse> GetAsync ()
    {
      return await Task.FromResult (Get ());
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      var cacheKey = "Business.Operation.PartProductionCurrentShiftOperation." + m_machine.Id;
      if (m_dateTime.HasValue) {
        Debug.Assert (DateTimeKind.Local != m_dateTime.Value.Kind);
        cacheKey += "." + m_dateTime.Value.ToString ("s") + "Z";
      }
      return cacheKey;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (PartProductionCurrentShiftOperationResponse data)
    {
      if (data.RequestDateTime.HasValue) { // In the past
        if (data.Range.ContainsElement (data.RequestDateTime.Value)) {
          return CacheTimeOut.PastLong.GetTimeSpan ();
        }
        else {
          return CacheTimeOut.CurrentLong.GetTimeSpan ();
        }
      }

      return CacheTimeOut.CurrentLong.GetTimeSpan ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<PartProductionCurrentShiftOperationResponse> data)
    {
      return true;
    }
    #endregion // IRequest implementation

    double? GetStandardProductionTargetPerHour (IOperation operation)
    {
      Debug.Assert (null != operation);

      var standardProductionTargetPerHourRequest = new Lemoine.Business.Operation
        .StandardProductionTargetPerHour (m_machine, operation);
      return Lemoine.Business.ServiceProvider
        .Get (standardProductionTargetPerHourRequest);
    }

    TimeSpan? GetCycleDurationTarget (IOperation operation, double? standardProductionTargetPerHour, int nbPiecesByCycle)
    {
      Debug.Assert (null != operation);

      if (standardProductionTargetPerHour.HasValue) {
        var result = TimeSpan.FromTicks ((long)(TimeSpan.FromHours (1).Ticks / standardProductionTargetPerHour.Value * nbPiecesByCycle));
        if (log.IsDebugEnabled) {
          log.Debug ($"GetCycleDurationTarget: cycle duration target is {result} from standard production target per hour {standardProductionTargetPerHour}");
        }
        return result;
      }
      else {
        log.Error ($"GetCycleDurationTarget: no standard production target for operation {operation} => return null");
        return null;
      }
    }

    /// <summary>
    /// Get the production durations when the task is not tracked
    /// </summary>
    /// <param name="now"></param>
    /// <param name="effectiveOperationSlots"></param>
    /// <returns></returns>
    TimeSpan GetProductionDuration (DateTime now,
                                    IList<IOperationSlot> effectiveOperationSlots)
    {
      // - Get the production duration
      TimeSpan shiftProductionDuration = effectiveOperationSlots
        .Where (slot => slot.ProductionDuration.HasValue)
        .Aggregate (TimeSpan.FromSeconds (0),
                    (total, next) => total.Add (next.ProductionDuration.Value));

      // - Consider the time between the last effective operation slot and now
      UtcDateTimeRange currentNoOperationRange = GetCurrentNoOperationRange (now, effectiveOperationSlots);
      Debug.Assert (currentNoOperationRange.Duration.HasValue);
      TimeSpan currentNoOperationMinDuration = Lemoine.Info.ConfigSet
        .LoadAndGet<TimeSpan> (CURRENT_NO_OPERATION_MIN_DURATION_KEY, CURRENT_NO_OPERATION_MIN_DURATION_DEFAULT);
      if (currentNoOperationRange.Duration.HasValue
          && (currentNoOperationMinDuration <= currentNoOperationRange.Duration.Value)) {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          TimeSpan productionDuration = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
            .GetProductionDuration (m_machine, currentNoOperationRange);
          shiftProductionDuration = shiftProductionDuration.Add (productionDuration);
        }
      }

      return shiftProductionDuration;
    }

    /// <summary>
    /// Get the date/time range between the end of the last operation slot and now,
    /// when the machine has not produced any part
    /// 
    /// An empty range is returned if no operation slots was identified
    /// </summary>
    /// <param name="now"></param>
    /// <param name="effectiveOperationSlots"></param>
    /// <returns></returns>
    UtcDateTimeRange GetCurrentNoOperationRange (DateTime now,
                                                 IList<IOperationSlot> effectiveOperationSlots)
    {
      UtcDateTimeRange currentNoOperationRange;
      if (effectiveOperationSlots.Any ()) {
        IOperationSlot lastOperationSlot = effectiveOperationSlots.Last ();
        if (Bound.Compare<DateTime> (lastOperationSlot.EndDateTime, now) < 0) {
          Debug.Assert (lastOperationSlot.EndDateTime.HasValue);
          currentNoOperationRange = new UtcDateTimeRange (lastOperationSlot.EndDateTime.Value, now);
        }
        else {
          currentNoOperationRange = new UtcDateTimeRange ();
        }
      }
      else {
        currentNoOperationRange = new UtcDateTimeRange ();
      }
      return currentNoOperationRange;
    }

    /// <summary>
    /// Get the number of produced parts when no task is defined
    /// </summary>
    /// <param name="effectiveOperationSlots"></param>
    /// <param name="nbPiecesByCycle"></param>
    /// <param name="longCycle"></param>
    /// 
    /// <returns></returns>
    double GetNumberOfProducedParts (IList<IOperationSlot> effectiveOperationSlots, int nbPiecesByCycle,
      bool longCycle)
    {
      Debug.Assert (null != effectiveOperationSlots);

      // - Full registered cycles
      double partNumber = effectiveOperationSlots
        .Aggregate (0, (total, next) => total
                    + (next.TotalCycles - next.AdjustedCycles) * nbPiecesByCycle
                    + next.AdjustedQuantity);
      log.DebugFormat ("GetNumberOfProducedParts: " +
                       "part number from full registered cycles is {0}",
                       partNumber);

      if (longCycle && (0 < effectiveOperationSlots.Count)) {
        // - Add the current cycle progress
        if ((partNumber <= 0.0)
            || effectiveOperationSlots.Any (operationSlot => (0 < operationSlot.PartialCycles))) {
          var monitoredMachine = GetMonitoredMachine ();
          if (null != monitoredMachine) {
            CycleProgressResponse cycleProgress = Lemoine.Business.ServiceProvider
              .Get (new CycleProgress (monitoredMachine));
            IOperation operation = effectiveOperationSlots.Last ().Operation;
            if (!object.Equals (cycleProgress.Operation, operation)) {
              log.ErrorFormat ("GetNumberOfProducedParts: " +
                               "the operation in cycle progress does not match the operation the operation in effectiveOperationSlots " +
                               "=> skip the current cycle");
            }
            else if (cycleProgress.MachiningCompletion.HasValue) {
              log.DebugFormat ("GetNumberOfProducedParts: " +
                               "completion of cycle is {0}",
                               cycleProgress.MachiningCompletion.Value);
              partNumber += cycleProgress.MachiningCompletion.Value * nbPiecesByCycle;
            }
            else {
              log.DebugFormat ("GetNumberOfProducedParts: " +
                               "no current cycle completion");
            }
          }
        }

        if (0 < partNumber) {
          // - Completion % at the first effective operation slot begin of the first full cycle
          IOperationSlot firstSlot = effectiveOperationSlots.First ();
          Debug.Assert (null != firstSlot);
          partNumber -= GetPartsOfFirstCycleOutOfOperationSlot (firstSlot.BeginDateTime, nbPiecesByCycle);
        }
      }

      log.DebugFormat ("GetNumberOfProducedParts: " +
                       "number of produced parts is {0}",
                       partNumber);

      return partNumber;
    }

    /// <summary>
    /// Get the number of parts of the first full cycle that was made in a previous slot
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="nbPiecesByCycle"></param>
    /// <returns></returns>
    double GetPartsOfFirstCycleOutOfOperationSlot (Bound<DateTime> dateTime, int nbPiecesByCycle)
    {
      if (!dateTime.HasValue) {
        if (log.IsDebugEnabled) {
          log.Debug ("GetPartsOfFirstCycleOutOfOperationSlot: " +
                     "dateTime=-oo");
        }
        return 0;
      }

      var monitoredMachine = GetMonitoredMachine ();
      if (null == monitoredMachine) {
        log.Warn ("GetPartsOffFirstCycleOutOfOperationSlot: not a monitored machine");
        return 0;
      }
      var cycleProgressRequest = new CycleProgress (monitoredMachine);
      cycleProgressRequest.At = dateTime.Value;
      CycleProgressResponse cycleProgress = Lemoine.Business.ServiceProvider
        .Get (cycleProgressRequest);
      if (null == cycleProgress.OperationCycle) {
        if (log.IsDebugEnabled) {
          log.Debug ("GetPartsOfFirstCycleOutOfOperationSlot: " +
                     "no cycle");
        }
        return 0;
      }
      else if (cycleProgress.MachiningCompletion.HasValue) {
        var operationCycle = cycleProgress.OperationCycle;
        Debug.Assert (null != operationCycle); // See above
        int cycleQuantity = operationCycle.Quantity ?? nbPiecesByCycle;
        if (log.IsDebugEnabled) {
          log.DebugFormat ("GetPartsOfFirstCycleOutOfOperationSlot: " +
                           "cycleQuantity={0} machining completion={1}",
                           cycleQuantity, cycleProgress.MachiningCompletion);
        }
        return cycleProgress.MachiningCompletion.Value * cycleQuantity;
      }
      else {
        if (log.IsDebugEnabled) {
          log.Debug ("GetPartsOfFirstCycleOutOfOperationSlot: " +
                     "completion is unknown");
        }
        return 0;
      }
    }

    IMonitoredMachine GetMonitoredMachine ()
    {
      if (m_machine is IMonitoredMachine) {
        return (IMonitoredMachine)m_machine;
      }
      else {
        var monitoredMachineBusinessRequest = new Lemoine.Business.Machine.MonitoredMachineFromId (m_machine.Id);
        return ServiceProvider.Get (monitoredMachineBusinessRequest);
      }
    }
  }

  /// <summary>
  /// Response of the business request PartProductionCurrentShiftOperation
  /// 
  /// <see cref="PartProductionCurrentShiftOperation"/>
  /// </summary>
  public class PartProductionCurrentShiftOperationResponse
    : IPartProductionCurrentShiftResponse
  {
    /// <summary>
    /// Constructor
    /// </summary>
    public PartProductionCurrentShiftOperationResponse ()
    { }

    /// <summary>
    /// Request date/time.
    /// 
    /// If not set, now is considered
    /// </summary>
    internal DateTime? RequestDateTime { get; set; }

    /// <summary>
    /// Date/time of the data
    /// </summary>
    public DateTime DateTime { get; internal set; }

    /// <summary>
    /// Number of parts completed in the current production of the current shift
    /// </summary>
    public double? NbPiecesCurrentShift { get; set; }

    /// <summary>
    /// Production target in number of parts for the current production of the current shift
    /// </summary>
    public double? GoalCurrentShift { get; set; }

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
  }
}
