// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Cnc.Data;
using Lemoine.Info;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Core.ExceptionManagement;
using System.Threading;

namespace Lemoine.CncDataImport
{
  /// <summary>
  /// Description of ImportDataMachineMode.
  /// </summary>
  internal sealed class ImportDataMachineMode : IImportData
  {
    static readonly TimeSpan DEFAULT_MAX_FACT_GAP = TimeSpan.FromMinutes (1);

    readonly ILog log;
    readonly IMonitoredMachine m_monitoredMachine;
    IFact m_fact = null;

    /// <summary>
    /// Last datetime when the method "ImportDatas" has been visited
    /// (automatically set by ImportCncValueFromQueue)
    /// </summary>
    public DateTime LastVisitDateTime { get; set; }

    /// <summary>
    /// Maximum duration of a gap
    /// </summary>
    TimeSpan MaxFactGap { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="monitoredMachine">not null</param>
    public ImportDataMachineMode (IMonitoredMachine monitoredMachine)
    {
      Debug.Assert (null != monitoredMachine);

      m_monitoredMachine = monitoredMachine;
      log = LogManager.GetLogger ($"{typeof (ImportDataMachineMode).FullName}.{monitoredMachine.Id}");

      MaxFactGap = ConfigSet.LoadAndGet<TimeSpan> ("MaxFactGap", DEFAULT_MAX_FACT_GAP);
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
        if (log.IsDebugEnabled) {
          log.Debug ($"IsMergeable: the new MachineMode data {data} does not contain the same key/value than {otherData} => not compatible");
        }
        return false;
      }
      if (data.DateTime < otherData.DateTime) {
        log.Fatal ($"IsMergeable: new data {data} is before the last data {otherData} => this should not happen");
        Debug.Assert (false);
        return false;
      }

      if (MaxFactGap < data.DateTime.Subtract (otherData.DateTime)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"IsMergeable: there is a too big gap between the new data {data} and {otherData} => not compatible");
        }
        return false;
      }
      if (log.IsDebugEnabled) {
        log.Debug ($"IsMergeable: MachineMode data {data} is compatible with {otherData}");
      }
      return true;
    }

    /// <summary>
    /// Import data that has been previously merged
    /// </summary>
    /// <param name="data"></param>
    public void ImportDatas (IList<ExchangeData> data, CancellationToken cancellationToken = default)
    {
      var firstData = data[0];
      var lastData = data[data.Count - 1];
      Debug.Assert (firstData.Command.Equals (lastData.Command));
      Debug.Assert (firstData.Key.Equals (lastData.Key));
      Debug.Assert (firstData.DateTime <= lastData.DateTime);
      ImportMachineMode (firstData.Key, firstData.Value, firstData.DateTime, lastData.DateTime, cancellationToken);
    }
    #endregion // IImportData implementation

    void ImportMachineMode (string key, object v, DateTime beginDateTime, DateTime endDateTime, CancellationToken cancellationToken = default)
    {
      log.Debug ($"ImportMachineMode: /B period={beginDateTime}-{endDateTime}");

      try {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          var daoFactory = ModelDAOHelper.DAOFactory;

          // Get the last fact and make some checks
          if (m_fact == null) {
            log.Info ("ImportMachineMode: initialize m_fact");
            using (IDAOTransaction transaction = session.BeginReadOnlyTransaction (
              "CncData.ImportMachineMode.GetLastFact", TransactionLevel.ReadCommitted)) {
              m_fact = daoFactory.FactDAO.GetLast (m_monitoredMachine);
            }

            if (log.IsDebugEnabled) {
              log.Debug ($"ImportMachineMode: initialize m_fact with {m_fact}");
            }
          }
          if (m_fact != null) {
            if (m_fact.End > beginDateTime) {
              log.Fatal ($"ImportMachineMode: recorded machine mode at {beginDateTime} comes before last fact end {m_fact.End} => skip it (this should not happen)");
              return;
            }
            if (m_fact.End < m_fact.Begin) {
              log.Error ($"ImportMachineMode: something strange happened, m_fact {m_fact} with end {m_fact.End} < begin {m_fact.Begin} => reset m_fact");
              m_fact = null;
            }
          }
          cancellationToken.ThrowIfCancellationRequested ();

          // Get the corresponding machine mode
          IMachineMode machineMode = GetMachineMode (key, v, session);
          if (machineMode == null) {
            return;
          }
          cancellationToken.ThrowIfCancellationRequested ();

          // Insert the new machine mode
          using (IDAOTransaction transaction = session.BeginTransaction (
            "CncData.ImportMachineMode.Insert", TransactionLevel.ReadCommitted)) {
            if (null != m_fact && machineMode.Id == m_fact.CncMachineMode.Id) {
              log.Debug ("ImportMachineMode: same machine mode => make the fact longer if the gap is not too big");
              Debug.Assert (m_fact.End <= beginDateTime); // Condition check made above
              if (beginDateTime.Subtract (m_fact.End) <= MaxFactGap) { // => make the fact longer
                if (log.IsDebugEnabled) {
                  log.Debug ($"ImportMachineMode: the gap {m_fact.End}-{beginDateTime} is short => make the fact longer");
                }
                if (0 != m_fact.Id) { // Re-associate the fact
                  daoFactory.FactDAO.UpgradeLock (m_fact);
                }
                m_fact.End = endDateTime;
                if (!m_fact.Begin.Equals (m_fact.End) &&
                    m_fact.Length >= TimeSpan.FromSeconds (1)) { // Only if the length of the fact is long enough to be stored in the database
                  if (log.IsDebugEnabled) {
                    log.Debug ($"ImportMachineMode: the fact {m_fact} {m_fact.Begin}-{m_fact.End} is long enough to be stored");
                  }
                  Debug.Assert (m_fact.Begin < m_fact.End);
                  daoFactory.FactDAO.MakePersistent (m_fact);
                }
              }
              else { // m_maxFactGap < dateTime - m_fact.End => create a gap
                if (log.IsDebugEnabled) {
                  log.Debug ($"ImportMachineMode: dateTime {beginDateTime} long after fact.End {m_fact.End} => create a gap");
                }
                m_fact = ModelDAOHelper.ModelFactory.CreateFact (
                  m_monitoredMachine, beginDateTime,
                  endDateTime, machineMode);

                // record it right now, only if it has a not null length
                if (!m_fact.Begin.Equals (m_fact.End) &&
                    m_fact.Length >= TimeSpan.FromSeconds (1)) { // Only if the length of the fact is long enough to be stored in the database
                  if (log.IsDebugEnabled) {
                    log.Debug ($"ImportMachineMode: the fact {m_fact} {m_fact.Begin}-{m_fact.End} is long enough to be stored");
                  }
                  Debug.Assert (m_fact.Begin < m_fact.End);
                  daoFactory.FactDAO.MakePersistent (m_fact);
                }
              }
            }
            else { // - New machine mode or no previous fact
              // - Make the previous fact longer in case there is no gap
              if (null != m_fact && beginDateTime.Subtract (m_fact.End) <= MaxFactGap) {
                if (log.IsDebugEnabled) {
                  log.Debug ($"ImportMachineMode: new machine mode and the the gap {m_fact.End}-{beginDateTime} is short => make the previous fact longer");
                }
                if (0 != m_fact.Id) { // Re-associate the fact
                  daoFactory.FactDAO.UpgradeLock (m_fact);
                }

                m_fact.End = beginDateTime;
                if (!m_fact.Begin.Equals (m_fact.End) &&
                    TimeSpan.FromSeconds (1) <= m_fact.Length) { // Only if the length of the fact is long enough to be stored in the database
                  daoFactory.FactDAO.MakePersistent (m_fact);
                }
              }

              // - Create a new fact
              m_fact = ModelDAOHelper.ModelFactory.CreateFact (m_monitoredMachine,
                                                               beginDateTime,
                                                               endDateTime,
                                                               machineMode);
              // record it right now, only if it has a not null length
              if (!m_fact.Begin.Equals (m_fact.End) &&
                  TimeSpan.FromSeconds (1) <= m_fact.Length) { // Only if the length of the fact is long enough to be stored in the database
                if (log.IsDebugEnabled) {
                  log.Debug ($"ImportMachineMode: the fact {m_fact} {m_fact.Begin}-{m_fact.End} is long enough to be stored");
                }
                Debug.Assert (m_fact.Begin < m_fact.End);
                daoFactory.FactDAO.MakePersistent (m_fact);
              }
            }

            transaction.Commit ();
          }
        }
      }
      catch (Exception ex) {
        // Reload the last fact
        log.Error ($"ImportMachineMode: exception => try to reload m_fact {m_fact}", ex);
        Debug.Assert (!ModelDAOHelper.DAOFactory.IsSessionActive ());
        if (ModelDAOHelper.DAOFactory.IsSessionActive ()) {
          log.Fatal ($"ImportMachineMode: the session is still active before reloading m_fact");
        }
        ReloadFact ();
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
          log.Error ($"ImportMachineMode: MachineMode id={(int)v} does not exist => skip it");
        }
      }
      else if (key.Equals (ExchangeData.MACHINE_MODE_TRANSLATION_KEY_OR_NAME)) {
        // Find the machine mode by its translationKeyOrName
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction (
          "CncData.ImportMachineMode.MachineModeTranslationKeyOrName", TransactionLevel.ReadCommitted)) {
          machineMode = ModelDAOHelper.DAOFactory.MachineModeDAO.FindByTranslationKeyOrName ((string)v);
        }

        if (machineMode == null) {
          log.Error ($"ImportMachineMode: MachineMode TranslationKeyOrName={(string)v} does not exist => skip it");
        }
      }
      else {
        log.Error ($"ImportMachineMode: key {key} is invalid => skip the record");
      }

      return machineMode;
    }

    void ReloadFact ()
    {
      if (null != m_fact && 0 != m_fact.Id) {
        try {
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
            using (IDAOTransaction transaction = session.BeginReadOnlyTransaction (
              "CncData.ReloadFact", TransactionLevel.ReadCommitted)) {
              m_fact = ModelDAOHelper.DAOFactory.FactDAO.FindById (m_fact.Id, m_fact.Machine);
            } // no commit because read-only
          }
        }
        catch (Exception ex) {
          log.Error ($"ImportMachineMode: reloading fact {m_fact} failed => set it to null", ex);
          m_fact = null;
        }
      }
    }
  }
}
