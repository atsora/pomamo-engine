// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Business;
using Lemoine.Conversion;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Lemoine.Business.Shift
{
  /// <summary>
  /// Request class to get date/time range of the shift that is used to count the parts
  /// 
  /// Return null if there is no such shift
  /// </summary>
  public sealed class OperationShift
    : IRequest<UtcDateTimeRange>
  {
    #region Members
    readonly IMachine m_machine;
    readonly DateTime m_day;
    readonly IShift m_shift;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (OperationShift).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="day">Day, DateTineKind=Unspecified</param>
    /// <param name="shift">not null</param>
    public OperationShift (IMachine machine, DateTime day, IShift shift)
    {
      Debug.Assert (null != machine);
      Debug.Assert (day.Kind == DateTimeKind.Unspecified);
      Debug.Assert (null != shift);

      m_machine = machine;
      m_day = day;
      m_shift = shift;
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public UtcDateTimeRange Get ()
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"Get: ...");
      }

      var operationSlotSplitOptionKey = ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.OperationSlotSplitOption);
      var operationSlotSplitOption = (OperationSlotSplitOption)Lemoine.Info.ConfigSet.Get<int> (operationSlotSplitOptionKey);

      if (operationSlotSplitOption.HasFlag (OperationSlotSplitOption.ByGlobalShift)) {
        return GetByGlobalShift ();
      }
      else if (operationSlotSplitOption.HasFlag (OperationSlotSplitOption.ByMachineShift)) {
        return GetByMachineShift ();
      }
      else {
        log.Error ($"Get: not supported for operation slot split option {operationSlotSplitOption}");
        throw new InvalidOperationException ("OperationSlotShift not supported with current operation slot split option");
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<UtcDateTimeRange> GetAsync ()
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"GetAsync: ...");
      }

      var operationSlotSplitOptionKey = ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.OperationSlotSplitOption);
      var operationSlotSplitOption = (OperationSlotSplitOption)Lemoine.Info.ConfigSet.Get<int> (operationSlotSplitOptionKey);

      if (operationSlotSplitOption.HasFlag (OperationSlotSplitOption.ByGlobalShift)) {
        return await GetByGlobalShiftAsync ();
      }
      else if (operationSlotSplitOption.HasFlag (OperationSlotSplitOption.ByMachineShift)) {
        return await GetByMachineShiftAsync ();
      }
      else {
        log.Error ($"GetAsync: not supported for operation slot split option {operationSlotSplitOption}");
        throw new InvalidOperationException ("OperationSlotShift not supported with current operation slot split option");
      }
    }

    UtcDateTimeRange GetByGlobalShift ()
    {
      using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
        var globalShifts = ModelDAO.ModelDAOHelper.DAOFactory.ShiftSlotDAO
          .FindWith (m_day, m_shift);
        if (globalShifts.Any ()) {
          if (1 < globalShifts.Count ()) {
            log.Warn ($"GetByGlobalShift: more than one global shift with {m_day}/{m_shift.Id}");
            return globalShifts
              .Select (s => s.DateTimeRange)
              .Aggregate ((x, y) => new UtcDateTimeRange (x.Union (y)));
          }
          else { // 1 == globalShifts.Count ()
            return globalShifts.First ().DateTimeRange;
          }
        }
        else {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetByGlobalShift: no global shift with {m_day}/{m_shift.Id} => return null");
          }
          return null;
        }
      }
    }

    async Task<UtcDateTimeRange> GetByGlobalShiftAsync ()
    {
      using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
        var globalShifts = await ModelDAO.ModelDAOHelper.DAOFactory.ShiftSlotDAO
          .FindWithAsync (m_day, m_shift);
        if (globalShifts.Any ()) {
          if (1 < globalShifts.Count ()) {
            log.Warn ($"GetByGlobalShiftAsync: more than one global shift with {m_day}/{m_shift.Id}");
            return globalShifts
              .Select (s => s.DateTimeRange)
              .Aggregate ((x, y) => new UtcDateTimeRange (x.Union (y)));
          }
          else { // 1 == globalShifts.Count ()
            return globalShifts.First ().DateTimeRange;
          }
        }
        else {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetByGlobalShiftAsync: no global shift with {m_day}/{m_shift.Id} => return null");
          }
          return null;
        }
      }
    }

    UtcDateTimeRange GetByMachineShift ()
    {
      using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
        var dao = new Lemoine.Business.MachineState.MachineShiftSlotDAO ();
        var machineShift = dao.FindWith (m_machine, m_day, m_shift);
        if (null != machineShift) {
          return machineShift.DateTimeRange;
        }
        else {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetByMachineShift: no machine shift with {m_day}/{m_shift.Id} => return null");
          }
          return null;
        }
      }
    }

    async Task<UtcDateTimeRange> GetByMachineShiftAsync ()
    {
      using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
        var dao = new Lemoine.Business.MachineState.MachineShiftSlotDAO ();
        var machineShift = await dao.FindWithAsync (m_machine, m_day, m_shift);
        if (null != machineShift) {
          return machineShift.DateTimeRange;
        }
        else {
          if (log.IsDebugEnabled) {
            log.Debug ($"GetByMachineShiftAsync: no machine shift with {m_day}/{m_shift.Id} => return null");
          }
          return null;
        }
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return $"Business.Shift.OperationShift.{m_machine.Id}.{m_day.ToString ("s")}.{m_shift.Id}";
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (UtcDateTimeRange data)
    {
      return CacheTimeOut.Config.GetTimeSpan ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<UtcDateTimeRange> data)
    {
      return true;
    }
    #endregion // IRequest implementation
  }
}
