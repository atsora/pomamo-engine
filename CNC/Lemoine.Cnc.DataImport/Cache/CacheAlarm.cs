// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.Cnc.DataImport.Cache
{
  /// <summary>
  /// Description of CacheAlarm.
  /// </summary>
  internal sealed class CacheAlarm
  {
    #region Members
    readonly IDictionary<AlarmKey, ICncAlarm> m_cncAlarms = new Dictionary<AlarmKey, ICncAlarm>();
    readonly IMachineModule m_machineModule;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (CacheAlarm).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="machineModule"></param>
    public CacheAlarm(IMachineModule machineModule)
    {
      m_machineModule = machineModule;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Set a cnc alarm, not stored in the database yet
    /// </summary>
    /// <param name="alarmKey"></param>
    /// <param name="cncAlarm"></param>
    public void SetCncAlarm(AlarmKey alarmKey, ICncAlarm cncAlarm)
    {
      Debug.Assert(alarmKey != null);
      m_cncAlarms[alarmKey] = cncAlarm;
    }
    
    /// <summary>
    /// Get a cnc alarm from the cache
    /// </summary>
    /// <param name="alarmKey"></param>
    /// <returns></returns>
    public ICncAlarm GetStoredCncAlarm(AlarmKey alarmKey)
    {
      ICncAlarm cncAlarm = null;
      if (m_cncAlarms.ContainsKey(alarmKey)) {
        cncAlarm = m_cncAlarms[alarmKey];
        Debug.Assert(null != cncAlarm);
        Debug.Assert(cncAlarm.MachineModule.Id == m_machineModule.Id);
        Debug.Assert(cncAlarm.CncInfo == alarmKey.CncInfo);
        Debug.Assert(cncAlarm.CncSubInfo == alarmKey.CncSubInfo);
        Debug.Assert(cncAlarm.Type == alarmKey.Type);
        Debug.Assert(cncAlarm.Number == alarmKey.Number);
      }
      
      log.DebugFormat("GetStoredCncAlarm: " +
                      "return {0} for machineModuleId={1} alarmKey={2}",
                      cncAlarm, m_machineModule.Id, alarmKey);
      
      return cncAlarm;
    }
    
    /// <summary>
    /// Get the already stored Cnc Alarm for the specified machine module and field,
    /// and re-attach it for the current session.
    /// 
    /// This method must be run in a NHibernate session.
    /// </summary>
    /// <param name="alarmKey"></param>
    /// <returns></returns>
    public ICncAlarm GetStoredReattachedCncAlarm(AlarmKey alarmKey)
    {
      ICncAlarm cncAlarm = GetStoredCncAlarm(alarmKey);
      
      // Re-attach the cncValue
      if (cncAlarm != null && cncAlarm.Id != 0) {
        ModelDAOHelper.DAOFactory.CncAlarmDAO.UpgradeLock(cncAlarm);
      }

      return cncAlarm;
    }
    
    /// <summary>
    /// Reset a cnc alarm in the cache
    /// </summary>
    /// <param name="alarmKey"></param>
    public void ResetCncAlarm(AlarmKey alarmKey)
    {
      log.DebugFormat("ResetCncAlarm: " +
                      "MachineModuleId={0} alarmKey={1}",
                      m_machineModule.Id, alarmKey);
      m_cncAlarms.Remove(alarmKey);
    }
    
    /// <summary>
    /// Clear all cnc alarms in the cache
    /// </summary>
    /// <param name="eltKept">elements to keep, can be null</param>
    public void Clear(ICollection<AlarmKey> eltKept)
    {
      if (eltKept == null) {
        m_cncAlarms.Clear();
      }
      else {
        // Check all the elements
        var keys = m_cncAlarms.Keys.ToList();
        for (int i = keys.Count - 1; i >= 0; i--) {
          if (!eltKept.Contains(keys[i])) {
            ResetCncAlarm (keys[i]);
          }
        }
      }
    }
    
    /// <summary>
    /// Reload a cnc alarm from the database
    /// </summary>
    /// <param name="alarmKey"></param>
    public void ReloadCncAlarm(AlarmKey alarmKey)
    {
      if (m_cncAlarms.ContainsKey(alarmKey)) {
        ICncAlarm cncAlarm = m_cncAlarms[alarmKey];
        Debug.Assert (null != cncAlarm);
        if (0 != cncAlarm.Id) {
          try {
            using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession())
            {
              cncAlarm = ModelDAOHelper.DAOFactory.CncAlarmDAO.FindById(cncAlarm.Id, cncAlarm.MachineModule);
            }
            log.DebugFormat ("ReloadCncAlarm: reloaded alarm is {0}", cncAlarm);
            if (cncAlarm != null) {
              m_cncAlarms[alarmKey] = cncAlarm;
            }
            else {
              ResetCncAlarm (alarmKey);
            }
          }
          catch (Exception ex) {
            log.ErrorFormat ("ReloadCncAlarm: reloading cncalarm {0} failed with {1} => reset it",
                             cncAlarm, ex);
            ResetCncAlarm(alarmKey);
          }
        }
      }
    }
    #endregion // Methods
  }
}
