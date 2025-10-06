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
using Lemoine.ModelDAO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lemoine.Business.Operation
{
  /// <summary>
  /// Request class to get the effective operation (and task if applicable) for the current shift
  /// </summary>
  public sealed class EffectiveOperationCurrentShift
    : IRequest<EffectiveOperationCurrentShiftResponse>
  {
    readonly IMachine m_machine;
    readonly DateTime? m_dateTime;

    static readonly ILog log = LogManager.GetLogger (typeof (EffectiveOperationCurrentShift).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="dateTime"></param>
    public EffectiveOperationCurrentShift (IMachine machine, DateTime? dateTime = null)
    {
      Debug.Assert (null != machine);

      if (machine is null) {
        log.Fatal ($"Ctr: machine is null at {System.Environment.StackTrace}");
        throw new ArgumentNullException ("machine");
      }

      m_machine = machine;
      m_dateTime = dateTime;
    }

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public EffectiveOperationCurrentShiftResponse Get ()
    {
      var dateTime = m_dateTime ?? DateTime.UtcNow;

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Business.Operation.EffectiveOperationCurrentShift")) {
          IOperationSlot virtualOperationSlot;
          IList<IOperationSlot> effectiveOperationSlots = ModelDAOHelper.DAOFactory.OperationSlotDAO
            .GetEffectiveCurrentShift (m_machine, out virtualOperationSlot, dateTime);
          if (null != virtualOperationSlot) {
            // Initialize all the proxies for the cache
            if (null != virtualOperationSlot.Task) {
              Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.Initialize (virtualOperationSlot.Task);
            }
            if (null != virtualOperationSlot.WorkOrder) {
              Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.Initialize (virtualOperationSlot.WorkOrder);
            }
            if (null != virtualOperationSlot.Component) {
              Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.Initialize (virtualOperationSlot.Component);
            }
            if (null != virtualOperationSlot.Operation) {
              Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.Initialize (virtualOperationSlot.Operation);
            }
            return new EffectiveOperationCurrentShiftResponse (dateTime, virtualOperationSlot, effectiveOperationSlots);
          }
          else {
            DateTime? day;
            IShift shift;
            var range = GetDayShift (dateTime, out day, out shift);
            // Note: range may be null
            return new EffectiveOperationCurrentShiftResponse (dateTime, day, shift, range);
          }
        }
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async Task<EffectiveOperationCurrentShiftResponse> GetAsync ()
    {
      return await Task.FromResult (Get ());
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      var cacheKey = "Business.Operation.EffectiveOperationCurrentShift." + m_machine.Id;
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
    public TimeSpan GetCacheTimeout (EffectiveOperationCurrentShiftResponse data)
    {
      return CacheTimeOut.CurrentLong.GetTimeSpan ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<EffectiveOperationCurrentShiftResponse> data)
    {
      if (null == data.Value) {
        return false;
      }

      if (data.Value.Range is null) {
        if (log.IsDebugEnabled) {
          log.Debug ($"IsCacheValid: range is null in {data.Value} (no period for day {data.Value.Day} and shift {data.Value.Shift}) => return false");
        }
        return false;
      }

      // data.Value.Range is not null
      if (data.Value.Range.IsEmpty ()) {
        return false;
      }

      var dateTime = m_dateTime ?? DateTime.UtcNow;
      return data.Value.Range.ContainsElement (dateTime);
    }
    #endregion // IRequest implementation

    /// <summary>
    /// Get the day and shift
    /// 
    /// Warning: null is returned if the operation slots are not split by shift
    /// or if no period corresponds to the specified day and shift
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="day"></param>
    /// <param name="shift"></param>
    UtcDateTimeRange GetDayShift (DateTime dateTime, out DateTime? day, out IShift shift)
    {
      day = null;
      shift = null;

      // - Get the current shift
      string splitOptionKey = string.Join (".",
                                           new string[] {
                                             ConfigPrefix.Analysis.ToString (),
                                             AnalysisConfigKey.OperationSlotSplitOption.ToString ()
                                           });
      OperationSlotSplitOption splitOption = (OperationSlotSplitOption)
        Lemoine.Info.ConfigSet.Get<int> (splitOptionKey);
      if (splitOption.HasFlag (OperationSlotSplitOption.ByGlobalShift)) {
        IShiftSlot shiftSlot = ModelDAOHelper.DAOFactory.ShiftSlotDAO
          .FindAt (dateTime);
        if ((null != shiftSlot) && shiftSlot.Day.HasValue && (null != shiftSlot.Shift)) {
          day = shiftSlot.Day.Value;
          shift = shiftSlot.Shift;
          return shiftSlot.DateTimeRange;
        }
      }
      else if (splitOption.HasFlag (OperationSlotSplitOption.ByMachineShift)) {
        var observationStateSlot = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
          .FindAt (m_machine, dateTime);
        if ((null != observationStateSlot) && (null != observationStateSlot.Shift)) {
          var daySlot = ModelDAOHelper.DAOFactory.DaySlotDAO
            .FindProcessedAt (dateTime);
          if ((null != daySlot) && (daySlot.Day.HasValue)) {
            day = daySlot.Day.Value;
            shift = observationStateSlot.Shift;
            return new UtcDateTimeRange (daySlot.DateTimeRange.Intersects (observationStateSlot.DateTimeRange));
          }
        }
      }
      else if (splitOption.Equals (OperationSlotSplitOption.ByDay)) {
        var daySlot = ModelDAOHelper.DAOFactory.DaySlotDAO
          .FindProcessedAt (dateTime);
        if ((null != daySlot) && (daySlot.Day.HasValue)) {
          day = daySlot.Day.Value;
          shift = null;
          return new UtcDateTimeRange (daySlot.DateTimeRange);
        }
      }

      return null;
    }
  }

  /// <summary>
  /// Response to the business request EffectiveOperationCurrentShift
  /// </summary>
  public sealed class EffectiveOperationCurrentShiftResponse
  {
    readonly ILog log = LogManager.GetLogger<EffectiveOperationCurrentShiftResponse> ();

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="virtualOperationSlot">not null</param>
    /// <param name="operationSlots"></param>
    internal EffectiveOperationCurrentShiftResponse (DateTime dateTime, IOperationSlot virtualOperationSlot, IList<IOperationSlot> operationSlots)
    {
      Debug.Assert (null != virtualOperationSlot);

      if (virtualOperationSlot.DateTimeRange is null) {
        log.Fatal ($"EffectiveOperationCurrentShiftResponse: null date/time range in virtual operation slot {virtualOperationSlot} at {System.Environment.StackTrace}");
      }

      this.DateTime = dateTime;
      this.VirtualOperationSlot = virtualOperationSlot;
      this.Task = virtualOperationSlot.Task;
      this.WorkOrder = virtualOperationSlot.WorkOrder;
      this.Component = virtualOperationSlot.Component;
      this.Operation = virtualOperationSlot.Operation;
      this.Day = virtualOperationSlot.Day;
      this.Shift = virtualOperationSlot.Shift;
      this.Range = virtualOperationSlot.DateTimeRange;
      this.OperationSlots = operationSlots;
    }

    /// <summary>
    /// Alternative constructor when there is no active operation
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="day"></param>
    /// <param name="shift"></param>
    /// <param name="range">may be null if the operation slot is not split by shift or if no period corresponds to the day and shift</param>
    internal EffectiveOperationCurrentShiftResponse (DateTime dateTime, DateTime? day, IShift shift, UtcDateTimeRange range)
    {
      if (range is null) {
        if (log.IsDebugEnabled) {
          log.Debug ($"EffectiveOperationCurrentShiftResponse: null date/time range in parameter");
        }
      }

      this.DateTime = dateTime;
      this.Day = day;
      this.Shift = shift;
      this.Range = range;
    }

    /// <summary>
    /// Date/time of the request
    /// </summary>
    public DateTime DateTime { get; private set; }

    /// <summary>
    /// Virtual operation slot
    /// </summary>
    internal IOperationSlot VirtualOperationSlot { get; private set; }

    /// <summary>
    /// Associated task
    /// </summary>
    public ITask Task { get; private set; }

    /// <summary>
    /// Associated work order when a task is active
    /// </summary>
    public IWorkOrder WorkOrder { get; private set; }

    /// <summary>
    /// Associated component
    /// </summary>
    public IComponent Component { get; private set; }

    /// <summary>
    /// Associated operation
    /// </summary>
    public IOperation Operation { get; private set; }

    /// <summary>
    /// Associated day
    /// </summary>
    public DateTime? Day { get; private set; }

    /// <summary>
    /// Associated shift
    /// </summary>
    public IShift Shift { get; private set; }

    /// <summary>
    /// Associated operation slots
    /// </summary>
    public IList<IOperationSlot> OperationSlots { get; private set; }

    /// <summary>
    /// Associated range
    /// 
    /// May be null if no period could be associated to the day and shift
    /// </summary>
    internal UtcDateTimeRange Range { get; private set; }
  }
}
