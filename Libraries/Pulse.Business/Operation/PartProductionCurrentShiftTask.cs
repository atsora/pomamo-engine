// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Lemoine.Collections;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Lemoine.Extensions;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Pulse.Extensions.Database;

namespace Lemoine.Business.Operation
{
  /// <summary>
  /// Request class to get the part production machining status of the current shift
  /// for the currently effective operation.
  /// 
  /// The task is not considered here
  /// </summary>
  public sealed class PartProductionCurrentShiftTask
    : IRequest<PartProductionCurrentShiftTaskResponse>
  {
    /// <summary>
    /// Options key for the minimum duration of the current no operation period
    /// </summary>
    static readonly string CURRENT_NO_OPERATION_MIN_DURATION_KEY = "Business.Operation.PartProductionCurrentShiftTask.CurrentNoOperationMinDuration";
    /// <summary>
    /// Options default value for the minimum duration of the current no operation period
    /// </summary>
    static readonly TimeSpan CURRENT_NO_OPERATION_MIN_DURATION_DEFAULT = TimeSpan.FromSeconds (10);

    readonly IMonitoredMachine m_machine;
    readonly DateTime? m_dateTime;

    static readonly ILog log = LogManager.GetLogger (typeof (PartProductionCurrentShiftTask).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    public PartProductionCurrentShiftTask (IMonitoredMachine machine)
    {
      Debug.Assert (null != machine);

      m_machine = machine;
    }


    /// <summary>
    /// Alternative constructor
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="dateTime"></param>
    public PartProductionCurrentShiftTask (IMonitoredMachine machine, DateTime dateTime)
      : this (machine)
    {
      m_dateTime = dateTime;
    }

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public PartProductionCurrentShiftTaskResponse Get ()
    {
      var dateTime = m_dateTime ?? DateTime.UtcNow;

      var response = new PartProductionCurrentShiftTaskResponse ();
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
      response.Task = effectiveOperationCurrentShiftResponse.Task;
      response.WorkOrder = effectiveOperationCurrentShiftResponse.WorkOrder;
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
          GetProductionDurations (out TimeSpan shiftProductionDuration, out TimeSpan globalProductionDuration,
                                  dateTime,
                                  response.Task,
                                  response.Day, response.Shift,
                                  effectiveOperationSlots);
          if (log.IsDebugEnabled) {
            log.Debug ($"Get: shiftProductionDuration={shiftProductionDuration} globalProductionDuration={globalProductionDuration}");
          }
          if (response.Day.HasValue && (null != response.Shift)) {
            response.GoalCurrentShift = shiftProductionDuration.TotalHours * standardProductionTargetPerHour.Value;
            if (!isLongCycle) {
              // Consider the number of pieces by cycle in case it is rounded (not a long cycle)
              if (response.GoalCurrentShift.HasValue) {
                response.GoalCurrentShift = nbPiecesByCycle * (((int)response.GoalCurrentShift.Value) / nbPiecesByCycle);
              }
            }
          }
          if (null != response.Task) {
            response.GoalWholeTask = globalProductionDuration.TotalHours * standardProductionTargetPerHour.Value;
            if (!isLongCycle && response.GoalWholeTask.HasValue) {
              // Consider the number of pieces by cycle in case it is rounded (not a long cycle)
              response.GoalWholeTask = nbPiecesByCycle * (((int)response.GoalWholeTask.Value) / nbPiecesByCycle);
            }
          }

          response.CycleDurationTarget = GetCycleDurationTarget (response.Operation, standardProductionTargetPerHour, nbPiecesByCycle);
        }

        // - Get the number of produced parts
        if (null != response.Task) {
          GetNumberOfProducedParts (out var shiftPieces, out var globalPieces,
                                    response.Task,
                                    response.Day, response.Shift);
          if (response.Day.HasValue
              && (null != response.Shift)) {
            response.NbPiecesCurrentShift = shiftPieces;
            if (!isLongCycle) {
              response.NbPiecesCurrentShift = (int?)response.NbPiecesCurrentShift;
            }
          }
          response.NbPiecesWholeTask = globalPieces;
          if (!isLongCycle) {
            response.NbPiecesWholeTask = (int?)response.NbPiecesWholeTask;
          }
        }
        else { // null == task
          if ((null != effectiveOperationSlots) && effectiveOperationSlots.Any ()) {
            response.NbPiecesCurrentShift =
              GetNumberOfProducedParts (effectiveOperationSlots, nbPiecesByCycle,
                isLongCycle);
            if (!isLongCycle) {
              response.NbPiecesCurrentShift = (int?)response.NbPiecesCurrentShift;
            }
          }
        }
      }
      else { // null == operation
             // - Get an appromative number of produced parts
        if ((null != effectiveOperationSlots) && effectiveOperationSlots.Any ()) {
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
    public async Task<PartProductionCurrentShiftTaskResponse> GetAsync ()
    {
      return await Task.FromResult (Get ());
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      var cacheKey = "Business.Operation.PartProductionCurrentShiftTask." + m_machine.Id;
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
    public TimeSpan GetCacheTimeout (PartProductionCurrentShiftTaskResponse data)
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
    public bool IsCacheValid (CacheValue<PartProductionCurrentShiftTaskResponse> data)
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
        log.Error ($"GetCycleDurationTarget: no standard production target for operation {((IDataWithId)operation).Id}");
        return null;
      }
    }

    /// <summary>
    /// Get the production durations when the task is tracked
    /// </summary>
    /// <param name="shiftProductionDuration"></param>
    /// <param name="globalProductionDuration"></param>
    /// <param name="now"></param>
    /// <param name="task"></param>
    /// <param name="day"></param>
    /// <param name="shift"></param>
    /// <param name="effectiveOperationSlots"></param>
    void GetProductionDurations (out TimeSpan shiftProductionDuration, out TimeSpan globalProductionDuration,
                                 DateTime now,
                                 ITask task,
                                 DateTime? day, IShift shift,
                                 IList<IOperationSlot> effectiveOperationSlots)
    {
      Debug.Assert (null != effectiveOperationSlots);

      // - Get the production duration
      shiftProductionDuration = TimeSpan.FromSeconds (0);
      globalProductionDuration = TimeSpan.FromSeconds (0);

      foreach (IOperationSlot effectiveOperationSlot in effectiveOperationSlots) {
        if (effectiveOperationSlot.ProductionDuration.HasValue) {
          shiftProductionDuration = shiftProductionDuration.Add (effectiveOperationSlot.ProductionDuration.Value);
          globalProductionDuration = shiftProductionDuration.Add (effectiveOperationSlot.ProductionDuration.Value);
        }
      }

      // - Consider the time between the last effective operation slot and now
      UtcDateTimeRange currentNoOperationRange = GetCurrentNoOperationRange (now, effectiveOperationSlots);
      Debug.Assert (currentNoOperationRange.Duration.HasValue);
      TimeSpan currentNoOperationMinDuration = Lemoine.Info.ConfigSet
        .LoadAndGet<TimeSpan> (CURRENT_NO_OPERATION_MIN_DURATION_KEY, CURRENT_NO_OPERATION_MIN_DURATION_DEFAULT);
      if (currentNoOperationRange.Duration.HasValue
          && (currentNoOperationMinDuration <= currentNoOperationRange.Duration.Value)) {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          TimeSpan productionDuration = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
            .GetProductionDuration (m_machine, currentNoOperationRange);
          shiftProductionDuration = shiftProductionDuration.Add (productionDuration);
          globalProductionDuration = globalProductionDuration.Add (productionDuration);
        }
      }

      if (null != task) {
        Debug.Assert (0 < effectiveOperationSlots.Count);
        if (effectiveOperationSlots[0].BeginDateTime.HasValue) {
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
            IList<IOperationSlot> operationSlots = ModelDAOHelper.DAOFactory.OperationSlotDAO
              .FindByTaskStrictlyBefore (m_machine, task, effectiveOperationSlots[0].BeginDateTime.Value);
            foreach (IOperationSlot operationSlot in operationSlots) {
              if (operationSlot.ProductionDuration.HasValue) {
                if (day.HasValue
                    && (null != shift)
                    && object.Equals (day.Value, operationSlot.Day)
                    && (null != operationSlot.Shift)
                    && (shift.Id == operationSlot.Shift.Id)) {
                  shiftProductionDuration = shiftProductionDuration.Add (operationSlot.ProductionDuration.Value);
                }
                globalProductionDuration = globalProductionDuration.Add (operationSlot.ProductionDuration.Value);
              }
            }
          }
        }
      }
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
          currentNoOperationRange = new UtcDateTimeRange (lastOperationSlot.EndDateTime.Value,
                                                          now);
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
    /// Get the number of produced parts when a task is defined
    /// </summary>
    /// <param name="shiftPieces"></param>
    /// <param name="globalPieces"></param>
    /// <param name="task"></param>
    /// <param name="day"></param>
    /// <param name="shift"></param>
    void GetNumberOfProducedParts (out double shiftPieces, out double globalPieces,
                                   ITask task,
                                   DateTime? day, IShift shift)
    {
      var extensionsRequest = new Lemoine.Business.Extension
        .MachineExtensions<ITaskNumberOfPartsExtension> (m_machine, (ext, m) => ext.Initialize (m));
      var extensions = Lemoine.Business.ServiceProvider
        .Get (extensionsRequest);
      foreach (var extension in extensions.OrderByDescending (ext => ext.Priority)) {
        var result = extension.GetNumberOfProducedParts (out shiftPieces, out globalPieces, task, day, shift);
        if (result) {
          return;
        }
      }

      log.Error ("GetNumberOfProducedParts: no ITaskNumberOfPartsExtension is registered");
      shiftPieces = 0;
      globalPieces = 0;
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
      if (log.IsDebugEnabled) {
        log.Debug ($"GetNumberOfProducedParts: part number from full registered cycles is {partNumber}");
      }

      if (longCycle && (0 < effectiveOperationSlots.Count)) {
        // - Add the current cycle progress
        if ((partNumber <= 0.0)
            || effectiveOperationSlots.Any (operationSlot => (0 < operationSlot.PartialCycles))) {
          CycleProgressResponse cycleProgress = Lemoine.Business.ServiceProvider
            .Get (new CycleProgress (m_machine));
          IOperation operation = effectiveOperationSlots.Last ().Operation;
          if (!object.Equals (cycleProgress.Operation, operation)) {
            log.Error ("GetNumberOfProducedParts: the operation in cycle progress does not match the operation the operation in effectiveOperationSlots => skip the current cycle");
          }
          else if (cycleProgress.MachiningCompletion.HasValue) {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetNumberOfProducedParts: completion of cycle is {cycleProgress.MachiningCompletion.Value}");
            }
            partNumber += cycleProgress.MachiningCompletion.Value * nbPiecesByCycle;
          }
          else {
            log.Debug ("GetNumberOfProducedParts: no current cycle completion");
          }
        }

        if (0 < partNumber) {
          // - Completion % at the first effective operation slot begin of the first full cycle
          IOperationSlot firstSlot = effectiveOperationSlots.First ();
          Debug.Assert (null != firstSlot);
          partNumber -= GetPartsOfFirstCycleOutOfOperationSlot (firstSlot.BeginDateTime, nbPiecesByCycle);
        }
      }

      if (log.IsDebugEnabled) {
        log.Debug ($"GetNumberOfProducedParts: number of produced parts is {partNumber}");
      }

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

      var cycleProgressRequest = new CycleProgress (m_machine) {
        At = dateTime.Value
      };
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
  }

  /// <summary>
  /// Response of the business request PartProductionCurrentShiftTask
  /// 
  /// <see cref="PartProductionCurrentShiftTask"/>
  /// </summary>
  public sealed class PartProductionCurrentShiftTaskResponse
    : IPartProductionCurrentShiftResponse
  {
    readonly ILog log = LogManager.GetLogger<PartProductionCurrentShiftTaskResponse> ();

    /// <summary>
    /// Constructor
    /// </summary>
    internal PartProductionCurrentShiftTaskResponse ()
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
    public double? NbPiecesCurrentShift { get; internal set; }

    /// <summary>
    /// Number of parts completed in the current production for the whole task
    /// </summary>
    public double? NbPiecesWholeTask { get; internal set; }

    /// <summary>
    /// Production target in number of parts for the current production of the current shift
    /// </summary>
    public double? GoalCurrentShift { get; internal set; }

    /// <summary>
    /// Production target in number of parts for the current production for the whole task
    /// </summary>
    public double? GoalWholeTask { get; internal set; }

    /// <summary>
    /// Associated task
    /// </summary>
    public ITask Task { get; internal set; }

    /// <summary>
    /// Associated work order
    /// </summary>
    public IWorkOrder WorkOrder { get; internal set; }

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
