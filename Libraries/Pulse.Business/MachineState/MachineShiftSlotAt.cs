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
  public sealed class MachineShiftSlotAt
    : IRequest<IMachineShiftSlot>
  {
    readonly IMachine m_machine;
    readonly DateTime m_at;

    static readonly ILog log = LogManager.GetLogger (typeof (MachineShiftSlotAt).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="at"></param>
    public MachineShiftSlotAt (IMachine machine, DateTime at)
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
    public IMachineShiftSlot Get ()
    {
      using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Business.MachineState.MachineShitSlotAt")) {
          return new MachineShiftSlotDAO ()
            .FindAt (m_machine, m_at);
        }
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async System.Threading.Tasks.Task<IMachineShiftSlot> GetAsync ()
    {
      using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Business.MachineState.MachineShitSlotAt")) {
          return await new MachineShiftSlotDAO ()
            .FindAtAsync (m_machine, m_at);
        }
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return $"Business.MachineState.MachineShiftSlotAt.{m_machine.Id}.{m_at}";
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (IMachineShiftSlot data)
    {
      return CacheTimeOut.Config.GetTimeSpan ();
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<IMachineShiftSlot> data)
    {
      return true;
    }
    #endregion // IRequest implementation
  }
}
