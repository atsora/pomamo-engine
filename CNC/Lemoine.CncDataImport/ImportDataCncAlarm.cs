// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2023 Atsora Solutions
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
using Lemoine.CncDataImport.Cache;
using System.Threading;
using Lemoine.Cnc;

namespace Lemoine.CncDataImport
{
  /// <summary>
  /// This class translates cnc alarms coming from machines to slots in the database
  /// Example of reception:
  ///  t0        t1        t2        t3
  ///  -         alarm A   alarm A   alarm A   (scenario 1)
  ///                                alarm A/B (scenario 2)
  ///                                alarm B   (scenario 3)
  /// Result:
  ///            t0        t1        t2        t3
  ///  at t0     -
  ///  at t1     -         [A]
  ///  at t2     -         [A-------]
  ///  at t3     -         [A-----------------]           (scenario 1)
  ///            -         [A-----------------([B]        (scenario 2)
  ///            -         [A-------]          [B]        (scenario 3)
  /// </summary>
  internal sealed class ImportDataCncAlarm : IImportData
  {
    static readonly TimeSpan DEFAULT_MAX_ALARM_GAP = TimeSpan.FromMinutes (1);

    #region Members
    readonly ILog log;
    readonly IMachineModule m_machineModule;
    readonly CacheAlarm m_cache;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Last datetime when the method "ImportDatas" has been visited
    /// (automatically set by ImportCncValueFromQueue)
    /// </summary>
    public DateTime LastVisitDateTime { get; set; }

    /// <summary>
    /// Maximum duration of a gap
    /// </summary>
    TimeSpan MaxAlarmGap { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineModule">not null</param>
    public ImportDataCncAlarm (IMachineModule machineModule)
    {
      Debug.Assert (null != machineModule);

      m_machineModule = machineModule;
      log = LogManager.GetLogger (string.Format ("{0}.{1}.{2}",
                                               typeof (ImportDataCncAlarm).FullName,
                                               machineModule.MonitoredMachine.Id,
                                               machineModule.Id));
      m_cache = new CacheAlarm (machineModule);
      MaxAlarmGap = ConfigSet.LoadAndGet<TimeSpan> ("MaxAlarmGap", DEFAULT_MAX_ALARM_GAP);
    }
    #endregion // Constructors

    #region IImportData implementation
    /// <summary>
    /// Return true if otherData can be merged with data
    /// </summary>
    /// <param name="data"></param>
    /// <param name="otherData"></param>
    /// <returns></returns>
    public bool IsMergeable (ExchangeData data, ExchangeData otherData)
    {
      if (data is null) {
        log.Error ("ImportDataCncAlarm.IsMergeable: data is null");
        return false;
      }
      if (otherData is null) {
        log.Error ("ImportDataCncAlarm.IsMergeable: otherData is null");
        return false;
      }

      // Note: the key is not used
      // Both must be CncAlarm
      if (otherData.Command != ExchangeDataCommand.CncAlarm ||
          data.Command != ExchangeDataCommand.CncAlarm) {
        log.Error ("ImportDataCncAlarm.IsMergeable: command of both data and otherData must be CncAlarm");
        return false;
      }

      // Both values must be ICollection
      var alarmList1 = GetAlarmList (data.Value);
      var alarmList2 = GetAlarmList (otherData.Value);

      if ((alarmList1 is null) && (alarmList2 is null)) {
        return true;
      }

      if ((alarmList1 is null) || (alarmList2 is null)) {
        return false;
      }

      // Both lists must have the same length
      if (alarmList1.Count != alarmList2.Count) {
        return false;
      }

      // Alarms must match (the "Equals" method being overriden) in the same order
      // Same cncinfo, subcncinfo, type, number, message and properties
      for (int i = 0; i < alarmList1.Count; i++) {
        if (!alarmList1[i].Equals (alarmList2[i])) {
          return false;
        }
      }

      if (MaxAlarmGap < data.DateTime.Subtract (otherData.DateTime)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"IsMergeable: there is a too big gap between the new data {data} and {otherData} => not compatible");
        }
        return false;
      }

      return true;
    }

    /// <summary>
    /// Import data that has been previously merged
    /// All elements in datas contain the same alarms but at a different time
    /// </summary>
    /// <param name="datas"></param>
    public void ImportDatas (IList<ExchangeData> datas, CancellationToken cancellationToken = default)
    {
      if (datas == null) {
        log.Error ("ImportDataCncAlarm.ImportDatas: datas is null");
        return;
      }

      if (datas.Count == 0) {
        log.Error ("ImportDataCncAlarm.ImportDatas: datas is empty");
        return;
      }

      // Start and end datetimes of the data elements
      DateTime startDatetime = datas[0].DateTime;
      DateTime endDatetime = datas[0].DateTime;
      for (int i = 1; i < datas.Count; i++) {
        DateTime newDate = datas[i].DateTime;
        if (newDate < startDatetime) {
          startDatetime = newDate;
        }

        if (newDate > endDatetime) {
          endDatetime = newDate;
        }
      }

      // Get the list of alarms 
      var list = GetAlarmList (datas[0].Value);
      if (list is null || list.Count == 0) {
        // Currently no alarms, the cache is cleared
        m_cache.Clear (null);
      }
      else {
        // Import alarms
        ImportAlarms (list, startDatetime, endDatetime);
      }

      // Update the alarm acquisition state
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginTransaction ("CncData.UpdateAlarmAcquisitionState", TransactionLevel.ReadCommitted)) {
          var acquisitionState = ModelDAOHelper.DAOFactory.AcquisitionStateDAO.GetAcquisitionState (m_machineModule, AcquisitionStateKey.Alarms) ??
  ModelDAOHelper.ModelFactory.CreateAcquisitionState (m_machineModule, AcquisitionStateKey.Alarms);
          acquisitionState.DateTime = endDatetime;
          ModelDAOHelper.DAOFactory.AcquisitionStateDAO.MakePersistent (acquisitionState);

          transaction.Commit ();
        }
      }
    }
    #endregion // IImportData implementation

    #region Private methods
    /// <summary>
    /// Transform an object into a list of cncalarm, if possible
    /// Can return null
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    List<CncAlarm> GetAlarmList (object v)
    {
      if (v is null) {
        log.Error ("ImportDataCncAlarm.GetAlarmList: v is null");
        return null;
      }

      var collection = v as System.Collections.ICollection;
      var list = new List<CncAlarm> ();

      // Already a collection?
      if (collection is null) {
        // Is it a cnc alarm?
        var alarm = v as CncAlarm;
        if (alarm is null) {
          log.Error ("ImportDataCncAlarm.GetAlarmList: v was not a collection and not a cncalarm");
        }
        else {
          list.Add (alarm);
        }
      }
      else {
        if (collection.Count > 0) {
          foreach (object obj in collection) {
            if (obj is CncAlarm alarm) {
              list.Add (alarm);
            }
            else {
              log.Error ($"ImportDataCncAlarm.GetAlarmList: a value of the collection was not a cncalarm ({obj})");
            }
          }
        }
      }

      return (list.Count > 0) ? list : null;
    }

    void ImportAlarms (IList<CncAlarm> cncAlarms, DateTime startDatetime, DateTime endDatetime)
    {
      // Prepare the cache and the alarms to store
      var keyAlarms = new Dictionary<AlarmKey, CncAlarm> ();
      foreach (var cncAlarm in cncAlarms) {
        var alarmKey = new AlarmKey (cncAlarm);

        // Get an existing alarm stored in the cache
        ICncAlarm cachedCncAlarm = m_cache.GetStoredCncAlarm (alarmKey);
        if (cachedCncAlarm != null) {
          if (cachedCncAlarm.DateTimeRange.Upper.Value > startDatetime) {
            // In that case, we ignore
            log.FatalFormat ("ImportCncAlarm: " +
                            "the end of the last alarm in the cache comes after the beginning of the first alarm to process ({0} > {1}). " +
                            "This should not happen => skip it.",
                            cachedCncAlarm.DateTimeRange.Upper.Value, startDatetime);
            continue;
          }
          else {
            // Check the gap and possibly clear the cache for this alarm
            if (startDatetime.Subtract (cachedCncAlarm.DateTimeRange.Upper.Value) > MaxAlarmGap) {
              if (log.IsDebugEnabled) {
                log.DebugFormat ("ImportCncAlarm: there is a gap {0}-{1}, " +
                                 "discontinue the CncAlarm {2}",
                                 cachedCncAlarm.DateTimeRange.Upper.Value, startDatetime,
                                 cachedCncAlarm);
              }
              m_cache.ResetCncAlarm (alarmKey);
            }
          }
        }

        // This alarm will be processed
        keyAlarms[alarmKey] = cncAlarm;
      }

      // Remove all elements in the cache that match none of the current alarms
      m_cache.Clear (keyAlarms.Keys);

      AlarmKey currentAlarmKey = null;
      try {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (IDAOTransaction transaction = session.BeginTransaction (
            "CncData.ImportCncAlarm", TransactionLevel.ReadCommitted)) {
            foreach (var keyAlarm in keyAlarms) {
              ProcessCncAlarm (keyAlarm.Key, keyAlarm.Value, startDatetime, endDatetime);
            }
            transaction.Commit ();
          }
        }
      }
      catch (Exception ex) {
        // Report
        log.Error ($"ImportAlarms: exception => try to reload m_cncAlarms", ex);
        Debug.Assert (!ModelDAOHelper.DAOFactory.IsSessionActive ());
        if (ModelDAOHelper.DAOFactory.IsSessionActive ()) {
          log.Fatal ("ImportAlarms: the session is still active before reloading m_cncAlarms", ex);
        }

        // Reload the cache (new values have been removed)
        m_cache.ReloadCncAlarm (currentAlarmKey);
        throw;
      }
    }

    void ProcessCncAlarm (AlarmKey alarmKey, CncAlarm cncAlarm, DateTime startDatetime, DateTime endDatetime)
    {
      Debug.Assert (cncAlarm != null);

      // Get the previous cnc value
      ICncAlarm cachedCncAlarm = m_cache.GetStoredReattachedCncAlarm (alarmKey);

      log.DebugFormat ("ProcessCncAlarm: /B new alarm={0} at {1} for key={2} and machineModule={3}",
                      cncAlarm, startDatetime, alarmKey, m_machineModule);

      if (cachedCncAlarm != null) {
        Debug.Assert (cachedCncAlarm.CncInfo == cncAlarm.CncInfo);
        Debug.Assert (cachedCncAlarm.CncSubInfo == cncAlarm.CncSubInfo);
        Debug.Assert (cachedCncAlarm.Type == cncAlarm.Type);
        Debug.Assert (cachedCncAlarm.Number == cncAlarm.Number);
        Debug.Assert (cachedCncAlarm.MachineModule.Id == m_machineModule.Id);
        Debug.Assert (cachedCncAlarm.DateTimeRange.Upper.Value <= startDatetime);

        // Make the previous CncAlarm longer
        log.DebugFormat ("ProcessCncAlarm: make the previous CncAlarm {0} longer to {1}",
                        cachedCncAlarm, endDatetime);
        cachedCncAlarm.Extend (new UpperBound<DateTime> (endDatetime), true);
      }
      else {
        // New value created
        var range = new UtcDateTimeRange (startDatetime, endDatetime, true, true);
        cachedCncAlarm = ModelDAOHelper.ModelFactory
          .CreateCncAlarm (m_machineModule, range, alarmKey.CncInfo, alarmKey.CncSubInfo, alarmKey.Type, alarmKey.Number);
        cachedCncAlarm.Message = cncAlarm.Message;
        foreach (var key in cncAlarm.Properties.Keys) {
          cachedCncAlarm.Properties[key] = cncAlarm.Properties[key];
        }

        m_cache.SetCncAlarm (alarmKey, cachedCncAlarm);
      }

      // Check not null constraint and save
      if (String.IsNullOrEmpty (cachedCncAlarm.Type)) {
        log.ErrorFormat ("Type of the alarm {0} is null or empty", cachedCncAlarm);
      }
      else {
        ModelDAOHelper.DAOFactory.CncAlarmDAO.MakePersistent (cachedCncAlarm);
      }
    }
    #endregion // Private methods
  }
}
