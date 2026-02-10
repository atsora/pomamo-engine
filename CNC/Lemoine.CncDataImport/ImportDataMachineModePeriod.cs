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
  /// Use:
  /// <item>Key = machine mode id</item>
  /// <item>Value = start time of the machine mode id</item>
  /// </summary>
  internal sealed class ImportDataMachineModePeriod : IImportData
  {
    static readonly string DEFAULT_MACHINE_MODE_KEY = "MachineModeIdPeriod.DefaultMachineMode";
    static readonly string DEFAULT_MACHINE_MODE_DEFAULT = ((int)MachineModeId.Inactive).ToString ();

    readonly ILog log;
    readonly IMonitoredMachine m_monitoredMachine;
    readonly string m_defaultMachineModeKey;
    IFact m_fact = null;

    /// <summary>
    /// Last datetime when the method "ImportDatas" has been visited
    /// (automatically set by ImportCncValueFromQueue)
    /// </summary>
    public DateTime LastVisitDateTime { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="monitoredMachine">not null</param>
    public ImportDataMachineModePeriod (IMonitoredMachine monitoredMachine)
    {
      Debug.Assert (null != monitoredMachine);

      m_monitoredMachine = monitoredMachine;
      log = LogManager.GetLogger ($"{typeof (ImportDataMachineMode).FullName}.{monitoredMachine.Id}");

      m_defaultMachineModeKey = Lemoine.Info.ConfigSet
        .LoadAndGet (DEFAULT_MACHINE_MODE_KEY, DEFAULT_MACHINE_MODE_DEFAULT);
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
      // Key must be the same
      if (!otherData.Key.Equals (data.Key)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"IsMergeable: the new MachineMode data {data} does not contain the same key than {otherData} => not compatible");
        }
        return false;
      }
      if (data.DateTime < otherData.DateTime) {
        log.Fatal ($"IsMergeable: new data {data} is before the last data {otherData} => this should not happen");
        return false;
      }

      // Start of the next data must correspond to the end of the previous data
      if (!((DateTime)otherData.Value).Equals ((DateTime)data.Value)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"IsMergeable: gap between {otherData.Value} and {data.Value}");
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
      ImportMachineMode (firstData.Key, (DateTime)firstData.Value, lastData.DateTime, cancellationToken);
    }
    #endregion // IImportData implementation

    void ImportMachineMode (string machineModeKey, DateTime startDateTime, DateTime endDateTime, CancellationToken cancellationToken = default)
    {
      log.Debug ($"ImportMachineMode: /B period={startDateTime}-{endDateTime} machineMode={machineModeKey}");

      try {
        if (m_fact.End < m_fact.Begin) {
          log.Error ($"ImportMachineMode: something strange happened, m_fact {m_fact} with end {m_fact.End} < begin {m_fact.Begin} => reset m_fact");
          m_fact = null;
        }

        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          var daoFactory = ModelDAOHelper.DAOFactory;

          // Get the last fact and make some checks
          if (m_fact is null) {
            log.Info ("ImportMachineMode: initialize m_fact");
            using (IDAOTransaction transaction = session.BeginReadOnlyTransaction (
              "CncData.ImportMachineMode.GetLastFact", TransactionLevel.ReadCommitted)) {
              m_fact = daoFactory.FactDAO.GetLast (m_monitoredMachine);
            }

            if (log.IsDebugEnabled) {
              log.Debug ($"ImportMachineMode: initialize m_fact with {m_fact}");
            }
          }
          DateTime adjustedStart = startDateTime;
          if (m_fact != null) {
            if (startDateTime < m_fact.End) {
              if (endDateTime <= m_fact.End) {
                log.Error ($"ImportMachineMode: recorded machine mode at {startDateTime}-{endDateTime} comes before last fact end {m_fact.End} => skip it (this should not happen)");
                return;
              }
              else {
                log.Warn ($"ImportMachineMode: adjust start to last fact end {m_fact.End}");
                adjustedStart = m_fact.End;
              }
            }
          }
          cancellationToken.ThrowIfCancellationRequested ();

          // Get the corresponding machine mode
          IMachineMode machineMode = GetMachineMode (machineModeKey, session);
          if (machineMode is null) {
            log.Error ($"ImportMachineMode: no machine mode found for key {machineModeKey} => skip it");
            return;
          }
          cancellationToken.ThrowIfCancellationRequested ();

          // Insert the new machine mode
          using (IDAOTransaction transaction = session.BeginTransaction (
            "CncData.ImportMachineMode.Insert", TransactionLevel.ReadCommitted)) {
            if (null != m_fact) {
              if (!m_fact.End.Equals (adjustedStart)) { // Gap
                if (adjustedStart < m_fact.End) {
                  log.Fatal ($"ImportMachineMode: this case should not happen adjustedStart={adjustedStart} < m_fact.End={m_fact.End}");
                  throw new InvalidOperationException ();
                }
                else if (TimeSpan.FromSeconds (1) <= adjustedStart.Subtract (m_fact.End)) {
                  if (log.IsDebugEnabled) {
                    log.Debug ($"ImportMachineMode: gap between {m_fact.End} and {adjustedStart} => create a gap");
                  }
                  if (!string.IsNullOrEmpty (m_defaultMachineModeKey)) {
                    var gapMachineMode = GetMachineMode (m_defaultMachineModeKey, session);
                    if (gapMachineMode is null) {
                      log.Error ($"ImportMachineMode: no machine mode with key={m_defaultMachineModeKey}");
                    }
                    else { // Valid gapMachineMode, create a fact in the gap
                      var gapFact = ModelDAOHelper.ModelFactory.CreateFact (
                        m_monitoredMachine, m_fact.End, adjustedStart, gapMachineMode);
                      if (log.IsDebugEnabled) {
                        log.Debug ($"ImportMachineMode: create the gap fact {gapFact} {gapFact.Begin}-{gapFact.End}");
                      }
                      Debug.Assert (gapFact.Begin < gapFact.End);
                      daoFactory.FactDAO.MakePersistent (gapFact);
                    }
                  }
                }
              }
              else if (machineMode.Id == m_fact.CncMachineMode.Id) { // And consecutive machine mode
                log.Debug ("ImportMachineMode: same consecutive machine mode");
                if (log.IsDebugEnabled) {
                  log.Debug ($"ImportMachineMode: make the fact longer");
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
                transaction.Commit ();
                return;
              }
            }
            // Create the new fact
            m_fact = ModelDAOHelper.ModelFactory.CreateFact (m_monitoredMachine,
                                                             adjustedStart,
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
    /// Get the machine mode by the id by the translationKeyOrName
    /// </summary>
    /// <param name="machineModeKey"></param>
    /// <param name="session"></param>
    /// <returns></returns>
    IMachineMode GetMachineMode (string machineModeKey, IDAOSession session)
    {
      try {
        if (int.TryParse (machineModeKey, out var machineModeId)) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction (
            "CncData.ImportMachineModePeriod.MachineModeId", TransactionLevel.ReadCommitted)) {
            return ModelDAOHelper.DAOFactory.MachineModeDAO.FindById (machineModeId);
          }
        }
        else {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction (
            "CncData.ImportMachineModePeriod.MachineModeTranslationKeyOrName", TransactionLevel.ReadCommitted)) {
            return ModelDAOHelper.DAOFactory.MachineModeDAO.FindByTranslationKeyOrName (machineModeKey);
          }
        }
      }
      catch (Exception ex) {
        log.Error ($"GetMachineMode: exception", ex);
        throw;
      }
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
