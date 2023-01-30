// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;
using Lemoine.Model;
using System.Threading;
using Lemoine.Threading;

namespace Lemoine.Business.Config
{
  /// <summary>
  /// Check there is no config update
  /// </summary>
  public class ConfigUpdateChecker : IConfigUpdateChecker
  {
    readonly ILog log = LogManager.GetLogger (typeof (ConfigUpdateChecker).FullName);

    readonly TimeSpan m_delay;
    volatile bool m_initialized = false;
    DateTime m_configUpdate;
    SemaphoreSlim m_semaphoreSlim = new SemaphoreSlim (1, 1);

    /// <summary>
    /// Constructor
    /// </summary>
    public ConfigUpdateChecker ()
      : this (TimeSpan.FromTicks (0))
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public ConfigUpdateChecker (TimeSpan delay)
    {
      m_delay = delay;
    }

    /// <summary>
    /// Initialize the date/time of the latest config update
    /// </summary>
    public void Initialize ()
    {
      if (m_initialized) {
        return;
      }

      DateTime configUpdate;
      if (log.IsDebugEnabled) {
        log.Debug ($"Initialize");
      }
      try {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("ActivityAnalysis.ConfigUpdate.Init")) {
          var applicationState = ModelDAOHelper.DAOFactory.ApplicationStateDAO
            .GetNoCache (ApplicationStateKey.ConfigUpdate.ToKey ());
          if (null == applicationState) {
            log.Error ($"Initialize: no application state defined for {ApplicationStateKey.ConfigUpdate.ToKey ()}");
            configUpdate = DateTime.UtcNow;
          }
          else {
            configUpdate = (DateTime)applicationState.Value;
          }
        }
      }
      catch (Exception ex) {
        log.Error ("Initialize: exception, skip it", ex);
        return;
      }
      if (log.IsDebugEnabled) {
        log.Debug ($"Initialize: configUpdate={configUpdate}");
      }

      if (m_initialized) {
        log.Warn ("Initialize: it was initialized in the mean time by another thread");
      }
      else { // !m_initialized
        using (var semaphoreSlimHolder = SemaphoreSlimHolder.Create (m_semaphoreSlim)) // make it thread safe
        {
          if (!m_initialized) {
            m_configUpdate = configUpdate;
            m_initialized = true;
          }
        }
      }
      if (log.IsDebugEnabled) {
        log.Debug ($"Initialize: completed");
      }
    }

    /// <summary>
    /// <see cref="IConfigUpdateChecker"/>
    /// </summary>
    /// <returns></returns>
    public bool CheckNoConfigUpdate ()
    {
      return CheckNoConfigUpdate (out var lastConfigUpdate);
    }

    /// <summary>
    /// Check there was no update of the config
    /// 
    /// true is returned in case the condition could not be checked
    /// </summary>
    /// <param name="lastConfigUpdate">if false is returned, date/time of the last config update</param>
    /// <returns></returns>
    bool CheckNoConfigUpdate (out DateTime lastConfigUpdate)
    {
      Initialize ();
      if (!m_initialized) {
        log.Error ("CheckNoConfigUpdate: not initialized, give up, return true");
        lastConfigUpdate = m_configUpdate;
        return true;
      }

      try {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("ActivityAnalysis.ConfigUpdate.Check")) {
          var applicationState = ModelDAOHelper.DAOFactory.ApplicationStateDAO
            .GetNoCache (ApplicationStateKey.ConfigUpdate.ToKey ());
          if (null == applicationState) {
            log.Error ($"CheckNoConfigUpdate: no application state defined for {ApplicationStateKey.ConfigUpdate.ToKey ()} => skip the check");
            lastConfigUpdate = m_configUpdate;
            return true;
          }
          else {
            var currentValue = (DateTime)applicationState.Value;
            if (m_configUpdate < currentValue) {
              log.Error ($"CheckNoConfigUpdate: the configuration was updated");
              lastConfigUpdate = currentValue;
              return false;
            }
            else {
              lastConfigUpdate = m_configUpdate;
              return true;
            }
          }
        }
      }
      catch (Exception ex) {
        log.Error ("Check: exception => return true", ex);
        lastConfigUpdate = m_configUpdate;
        return true;
      }
    }

    /// <summary>
    /// Check there was no update of the config
    /// 
    /// true is returned in case the condition could not be checked
    /// </summary>
    /// <returns></returns>
    public bool Check ()
    {
      if (!CheckNoConfigUpdate (out var lastConfigUpdate)) {
        return CheckTime (lastConfigUpdate);
      }
      else {
        return true;
      }
    }

    bool CheckTime (DateTime lastConfigUpdate)
    {
      if ((0 == m_delay.Ticks) || (lastConfigUpdate.Add (m_delay) < DateTime.UtcNow)) {
        log.Error ($"CheckTime: the configuration was updated and delay {m_delay} was reached => return false");
        return false;
      }
      else {
        log.Info ($"CheckTime: {m_delay} has not been reached yet => return true");
        return true;
      }
    }
  }
}
