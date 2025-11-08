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

namespace Lemoine.Business.MachineState
{
  /// <summary>
  /// Request class to get ...
  /// </summary>
  public sealed class MachineShiftSlotEnd
    : IRequest<UpperBound<DateTime>?>
  {
    readonly IMachine m_machine;
    readonly DateTime m_at;

    static readonly ILog log = LogManager.GetLogger (typeof (MachineShiftSlotEnd).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="at"></param>
    public MachineShiftSlotEnd (IMachine machine, DateTime at)
    {
      Debug.Assert (null != machine);

      m_machine = machine;
      m_at = at;
    }

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public UpperBound<DateTime>? Get ()
    {
      using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Business.MachineState.MachineShitSlotEnd")) {
          var extendRightOnly = new MachineShiftSlotDAO ()
            .FindAtExtendRightOnly (m_machine, m_at);
          if (null == extendRightOnly) {
            return null;
          }
          else {
            return extendRightOnly.DateTimeRange.Upper;
          }
        }
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<UpperBound<DateTime>?> GetAsync ()
    {
      using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Business.MachineState.MachineShitSlotEnd")) {
          var extendRightOnly = await new MachineShiftSlotDAO ()
            .FindAtExtendRightOnlyAsync (m_machine, m_at);
          if (null == extendRightOnly) {
            return null;
          }
          else {
            return extendRightOnly.DateTimeRange.Upper;
          }
        }
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return $"Business.MachineState.MachineShiftSlotEnd.{m_machine.Id}.{m_at}";
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (UpperBound<DateTime>? data)
    {
      return CacheTimeOut.Config.GetTimeSpan ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<UpperBound<DateTime>?> data)
    {
      return true;
    }
    #endregion // IRequest implementation
  }
}
