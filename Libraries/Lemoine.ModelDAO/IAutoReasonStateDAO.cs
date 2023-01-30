// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using System.Diagnostics;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IApplicationState.
  /// </summary>
  public interface IAutoReasonStateDAO : IGenericByMonitoredMachineUpdateDAO<IAutoReasonState, int>
  {
    /// <summary>
    /// Get the auto reason state for the specified machine and key
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    IAutoReasonState GetAutoReasonState (IMonitoredMachine machine, string key);
  }

  /// <summary>
  /// Extensions to the interface IApplicationState
  /// </summary>
  public static class AutoReasonStateDAOExtensions
  {
    /// <summary>
    /// Save a new auto-reason state value directly
    /// </summary>
    /// <param name="t"></param>
    /// <param name="machine"></param>
    /// <param name="key"></param>
    /// <param name="v"></param>
    /// <returns></returns>
    public static IAutoReasonState Save (this IAutoReasonStateDAO t, IMonitoredMachine machine, string key, object v)
    {
      var autoReasonState = ModelDAOHelper.DAOFactory.AutoReasonStateDAO.GetAutoReasonState (machine, key);
      if (null == autoReasonState) {
        autoReasonState = ModelDAOHelper.ModelFactory.CreateAutoReasonState (machine, key);
      }
      Debug.Assert (v != null);
      autoReasonState.Value = v;
      ModelDAOHelper.DAOFactory.AutoReasonStateDAO.MakePersistent (autoReasonState);
      return autoReasonState;
    }

    /// <summary>
    /// Remove a new auto-reason state value directly
    /// </summary>
    /// <param name="t"></param>
    /// <param name="machine"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static void Remove (this IAutoReasonStateDAO t, IMonitoredMachine machine, string key)
    {
      var autoReasonState = ModelDAOHelper.DAOFactory.AutoReasonStateDAO.GetAutoReasonState (machine, key);
      if (null != autoReasonState) {
        ModelDAOHelper.DAOFactory.AutoReasonStateDAO.MakeTransient (autoReasonState);
      }
    }
  }
}
