// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2023 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Cnc.Data;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System.Threading;

namespace Lemoine.CncDataImport
{
  /// <summary>
  /// Description of ImportDataMachineModule.
  /// </summary>
  internal sealed class ImportDataMachineModule : IImportData
  {
    static readonly TimeSpan DEFAULT_MAX_MACHINE_MODULE_ACTIVITY_GAP = TimeSpan.FromMinutes (1);

    #region Members
    readonly ILog log;
    readonly IMachineModule m_machineModule;
    IMachineModuleActivity m_machineModuleActivity = null;
    #endregion // Members

    /// <summary>
    /// Last datetime when the method "ImportDatas" has been visited
    /// (automatically set by ImportCncValueFromQueue)
    /// </summary>
    public DateTime LastVisitDateTime { get; set; }

    /// <summary>
    /// Maximum duration of a gap
    /// </summary>
    TimeSpan MaxMachineModuleActivityGap { get; set; }

    /// <summary>
    /// Constructors
    /// </summary>
    /// <param name="machineModule">not null</param>
    public ImportDataMachineModule (IMachineModule machineModule)
    {
      Debug.Assert (null != machineModule);

      m_machineModule = machineModule;
      log = LogManager.GetLogger (string.Format ("{0}.{1}.{2}",
                                                 typeof (ImportDataMachineModule).FullName,
                                                 machineModule.MonitoredMachine.Id,
                                                 machineModule.Id));

      MaxMachineModuleActivityGap = Info.ConfigSet.LoadAndGet<TimeSpan> (
        "MaxMachineModuleActivityGap", DEFAULT_MAX_MACHINE_MODULE_ACTIVITY_GAP);
    }

    #region IImportData implementation
    /// <summary>
    /// Return true if otherData can be merged with data
    /// </summary>
    /// <param name="data"></param>
    /// <param name="otherData"></param>
    /// <returns></returns>
    public bool IsMergeable (ExchangeData data, ExchangeData otherData)
    {
      // Key + Value must be the same
      if (!otherData.Key.Equals (data.Key) || !otherData.Value.Equals (data.Value)) {
        log.DebugFormat ("IsDataCompatible: " +
                         "the new MachineMode data {0} does not contain the same key/value than {1} " +
                         "=> not compatible",
                         data, otherData);
        return false;
      }
      if (data.DateTime < otherData.DateTime) {
        log.FatalFormat ("IsDataCompatible: " +
                         "new data {0} is before the last data {1} " +
                         "=> this should not happen",
                         data, otherData);
        Debug.Assert (false);
        return false;
      }

      if (MaxMachineModuleActivityGap < data.DateTime.Subtract (otherData.DateTime)) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("IsDataCompatible: " +
                           "there is a too big gap between the new data {0} and {1} " +
                           "=> not compatible",
                           data, otherData);
        }
        return false;
      }
      if (log.IsDebugEnabled) {
        log.DebugFormat ("IsDataCompatible: " +
                         "MachineMode data {0} is compatible with {1}",
                         data, otherData);
      }
      return true;
    }

    /// <summary>
    /// Import data that has been previously merged
    /// </summary>
    /// <param name="datas"></param>
    public void ImportDatas (IList<ExchangeData> datas, CancellationToken cancellationToken = default)
    {
      var firstData = datas[0];
      var lastData = datas[datas.Count - 1];
      Debug.Assert (firstData.Command.Equals (lastData.Command));
      Debug.Assert (firstData.Key.Equals (lastData.Key));
      Debug.Assert (firstData.DateTime <= lastData.DateTime);
      ImportMachineModuleActivity (firstData.Key, firstData.Value, firstData.DateTime, lastData.DateTime);
    }
    #endregion // IImportData implementation

    #region Private methods
    void ImportMachineModuleActivity (string key, object v, DateTime beginDateTime, DateTime endDateTime)
    {
      log.DebugFormat ("ImportMachineModuleActivity: /B " +
                       "period={0}-{1}",
                       beginDateTime, endDateTime);
      try {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;

          // Get and check last machineModuleActivity
          if (null == m_machineModuleActivity) {
            log.Info ("ImportMachineModuleActivity: " +
                      "initialize m_machineModuleActivity");
            m_machineModuleActivity = daoFactory.MachineModuleActivityDAO.GetLast (m_machineModule);
            if (log.IsDebugEnabled) {
              log.Debug ($"ImportMachineModuleActivity: initialize m_machineModuleActivity with {m_machineModuleActivity}");
            }
          }
          if (null != m_machineModuleActivity) {
            if (beginDateTime < m_machineModuleActivity.End) {
              log.FatalFormat ("ImportMachineModuleActivity: " +
                               "recorded machine mode at {0} comes before last machine module activity end {1} " +
                               "=> skip it " +
                               "(this should not happen)",
                               beginDateTime, m_machineModuleActivity.End);
              return;
            }
            if (m_machineModuleActivity.End < m_machineModuleActivity.Begin) {
              log.ErrorFormat ("ImportMachineModuleActivity: " +
                               "something strange happened, m_machineModuleActivity {0} with end {1} < begin {2} " +
                               "=> reset m_machineModuleActivity",
                               m_machineModuleActivity, m_machineModuleActivity.End, m_machineModuleActivity.Begin);
              m_machineModuleActivity = null;
            }
          }

          IMachineMode machineMode = GetMachineMode (key, v, session);
          if (machineMode == null) {
            return;
          }

          // Insert the new machine mode
          using (IDAOTransaction transaction = session.BeginTransaction (
            "CncData.ImportMachineModuleActivity.Insert", TransactionLevel.ReadCommitted)) {
            if (null != m_machineModuleActivity && machineMode.Id == m_machineModuleActivity.MachineMode.Id) {
              log.Debug ("ImportMachineModuleActivity: same machine mode => make the activity longer if the gap is not too big");
              Debug.Assert (m_machineModuleActivity.End <= beginDateTime); // Condition check made above
              if (beginDateTime.Subtract (m_machineModuleActivity.End) <= MaxMachineModuleActivityGap) { // => make the activity longer
                if (log.IsDebugEnabled) {
                  log.DebugFormat ("ImportMachineModuleActivity: " +
                                   "the gap {0}-{1} is short => make the activity longer",
                                   m_machineModuleActivity.End, beginDateTime);
                }
                if (0 != m_machineModuleActivity.Id) { // Re-associate the activity
                  daoFactory.MachineModuleActivityDAO.UpgradeLock (m_machineModuleActivity); // Ok, beause read-committed transaction
                }
                m_machineModuleActivity.End = endDateTime;
                if (!m_machineModuleActivity.Begin.Equals (m_machineModuleActivity.End) &&
                    TimeSpan.FromSeconds (1) <= m_machineModuleActivity.Length) { // Only if the length of the activity is long enough to be stored in the database
                  if (log.IsDebugEnabled) {
                    log.DebugFormat ("ImportMachineModuleActivity: " +
                                     "the activity {0} {1}-{2} is long enough to be stored",
                                     m_machineModuleActivity, m_machineModuleActivity.Begin, m_machineModuleActivity.End);
                  }
                  Debug.Assert (m_machineModuleActivity.Begin < m_machineModuleActivity.End);
                  daoFactory.MachineModuleActivityDAO.MakePersistent (m_machineModuleActivity);
                }
              }
              else { // m_maxFactGap < dateTime - m_machineModuleActivity.End => create a gap
                if (log.IsDebugEnabled) {
                  log.DebugFormat ("ImportMachineModuleActivity: " +
                                   "dateTime {0} long after activity.End {1} " +
                                   "=> create a gap",
                                   beginDateTime, m_machineModuleActivity.End);
                }
                m_machineModuleActivity = ModelDAOHelper.ModelFactory.CreateMachineModuleActivity (
                  m_machineModule, beginDateTime, endDateTime, machineMode);

                // record it right now, only if it has a not null length
                if (!m_machineModuleActivity.Begin.Equals (m_machineModuleActivity.End) &&
                    TimeSpan.FromSeconds (1) <= m_machineModuleActivity.Length) { // Only if the length of the activity is long enough to be stored in the database
                  if (log.IsDebugEnabled) {
                    log.DebugFormat ("ImportMachineModuleActivity: " +
                                     "the activity {0} {1}-{2} is long enough to be stored",
                                     m_machineModuleActivity, m_machineModuleActivity.Begin, m_machineModuleActivity.End);
                  }
                  Debug.Assert (m_machineModuleActivity.Begin < m_machineModuleActivity.End);
                  daoFactory.MachineModuleActivityDAO.MakePersistent (m_machineModuleActivity);
                }
              }
            }
            else { // - New machine mode
                   // - Make the previous activity longer in case there is no gap
              if (null != m_machineModuleActivity &&
                  beginDateTime.Subtract (m_machineModuleActivity.End) <= MaxMachineModuleActivityGap) {
                if (log.IsDebugEnabled) {
                  log.DebugFormat ("ImportMachineModuleActivity: " +
                                   "new machine mode and the the gap {0}-{1} is short " +
                                   "=> make the previous activity longer",
                                   m_machineModuleActivity.End, beginDateTime);
                }
                if (0 != m_machineModuleActivity.Id) { // Re-associate the fact
                  daoFactory.MachineModuleActivityDAO.UpgradeLock (m_machineModuleActivity); // Ok, because read-committed transaction
                }
                m_machineModuleActivity.End = beginDateTime;
                if (!m_machineModuleActivity.Begin.Equals (m_machineModuleActivity.End) &&
                    TimeSpan.FromSeconds (1) <= m_machineModuleActivity.Length) { // Only if the length of the activity is long enough to be stored in the database
                  Debug.Assert (m_machineModuleActivity.Begin < m_machineModuleActivity.End);
                  daoFactory.MachineModuleActivityDAO.MakePersistent (m_machineModuleActivity);
                }
              }

              // - Create a new activity
              m_machineModuleActivity = ModelDAOHelper.ModelFactory.CreateMachineModuleActivity (
                m_machineModule, beginDateTime, endDateTime, machineMode);

              // record it right now, only if it has a not null length
              if (!m_machineModuleActivity.Begin.Equals (m_machineModuleActivity.End) &&
                  TimeSpan.FromSeconds (1) <= m_machineModuleActivity.Length) { // Only if the length of the activity is long enough to be stored in the database
                if (log.IsDebugEnabled) {
                  log.DebugFormat ("ImportMachineModuleActivity: " +
                                   "the activity {0} {1}-{2} is long enough to be stored",
                                   m_machineModuleActivity, m_machineModuleActivity.Begin, m_machineModuleActivity.End);
                }
                Debug.Assert (m_machineModuleActivity.Begin < m_machineModuleActivity.End);
                daoFactory.MachineModuleActivityDAO.MakePersistent (m_machineModuleActivity);
              }
            }

            transaction.Commit ();
          }
        }
      }
      catch (Exception ex) { // Reload m_machineModuleActivity
        log.Error ($"ImportMachineModuleActivity: exception => try to reload m_machineModuleActivity {m_machineModuleActivity}", ex);
        Debug.Assert (!ModelDAOHelper.DAOFactory.IsSessionActive ());
        if (ModelDAOHelper.DAOFactory.IsSessionActive ()) {
          log.FatalFormat ("ImportMachineModuleActivity: " +
                           "the session is still active before reloading m_machineModuleActivity");
        }
        ReloadMachineModuleActivity ();
        throw;
      }
    }

    /// <summary>
    /// Get the machine mode by the id or the translationKeyOrName
    /// </summary>
    /// <param name="key"></param>
    /// <param name="v"></param>
    /// <param name="session"></param>
    /// <returns></returns>
    IMachineMode GetMachineMode (string key, object v, IDAOSession session)
    {
      IMachineMode machineMode = null;

      if (key.Equals (ExchangeData.MACHINE_MODE_ID)) {
        // Find the machine mode by its id
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction (
          "CncData.ImportMachineMode.MachineModeId", TransactionLevel.ReadCommitted)) {
          machineMode = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)v);
        }

        if (machineMode == null) {
          log.ErrorFormat ("ImportMachineMode: " +
                           "MachineMode id={0} does not exist => skip it",
                           (int)v);
        }
      }
      else if (key.Equals (ExchangeData.MACHINE_MODE_TRANSLATION_KEY_OR_NAME)) {
        // Find the machine mode by its translationKeyOrName
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction (
          "CncData.ImportMachineMode.MachineModeTranslationKeyOrName", TransactionLevel.ReadCommitted)) {
          machineMode = ModelDAOHelper.DAOFactory.MachineModeDAO.FindByTranslationKeyOrName ((string)v);
        }

        if (machineMode == null) {
          log.ErrorFormat ("ImportMachineMode: " +
                           "MachineMode TranslationKeyOrName={0} does not exist => skip it",
                           (string)v);
        }
      }
      else {
        log.ErrorFormat ("ImportMachineMode: key {0} is invalid => skip the record", key);
      }

      return machineMode;
    }

    void ReloadMachineModuleActivity ()
    {
      if (null != m_machineModuleActivity && 0 != m_machineModuleActivity.Id) {
        try {
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
            using (IDAOTransaction transaction = session.BeginReadOnlyTransaction (
              "CncData.ReloadMachineModuleActivity", TransactionLevel.ReadCommitted)) {
              m_machineModuleActivity = ModelDAOHelper.DAOFactory.MachineModuleActivityDAO
                .FindById (m_machineModuleActivity.Id, m_machineModuleActivity.MachineModule);
            } // no commit because read-only
          }
        }
        catch (Exception ex) {
          log.Error ($"ImportMachineModuleActivity: reloading activity {m_machineModuleActivity} failed => set it to null", ex);
          m_machineModuleActivity = null;
        }
      }
    }
    #endregion // Private methods
  }
}
