// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Cnc.Data;
using Lemoine.Extensions;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Cnc.DataImport.Cache;
using System.Threading;
// using Lemoine.Conversion;

namespace Lemoine.Cnc.DataImport
{
  /// <summary>
  /// Description of ImportDataCncValues.
  /// </summary>
  internal sealed class ImportDataCncValues : IImportData, Lemoine.Threading.IChecked
  {
    static readonly TimeSpan DEFAULT_SKIP_OLD_SECONDARY_CNC_VALUES_AFTER = TimeSpan.FromHours (2);

    #region Members
    readonly ILog log;
    readonly IMachineModule m_machineModule;
    readonly CacheCncValue m_cache;
    readonly Lemoine.Threading.IChecked m_caller = null;
    readonly IEnumerable<Lemoine.Extensions.Cnc.IImportCncValuesExtension> m_importCncValuesExtensions;
    readonly IEnumerable<Lemoine.Extensions.Cnc.IImportCncValueExtension> m_importCncValueExtensions;
    //readonly IAutoConverter m_converter = new DefaultAutoConverter ();
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Last datetime when the method "ImportDatas" has been visited
    /// (automatically set by ImportCncValueFromQueue)
    /// </summary>
    public DateTime LastVisitDateTime { get; set; }

    /// <summary>
    /// Duration after which a secondary cnc value is considered old and thus skipped
    /// </summary>
    TimeSpan? SkipOldSecondaryCncValuesAfter { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// cncCache must be the same than in ImportDataStopCncValues
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="cncCache"></param>
    /// <param name="caller"></param>
    public ImportDataCncValues (IMachineModule machineModule, CacheCncValue cncCache, Lemoine.Threading.IChecked caller)
    {
      Debug.Assert (null != machineModule);

      m_machineModule = machineModule;
      log = LogManager.GetLogger (string.Format ("{0}.{1}.{2}",
                                                 typeof (ImportDataCncValues).FullName,
                                                 machineModule.MonitoredMachine.Id,
                                                 machineModule.Id));
      m_cache = cncCache;
      m_caller = caller;

      SkipOldSecondaryCncValuesAfter = Info.ConfigSet.LoadAndGet<TimeSpan> (
        "SkipOldSecondaryCncValuesAfter", DEFAULT_SKIP_OLD_SECONDARY_CNC_VALUES_AFTER);
      if (SkipOldSecondaryCncValuesAfter.HasValue && SkipOldSecondaryCncValuesAfter.Value.TotalSeconds < 1) {
        SkipOldSecondaryCncValuesAfter = null;
      }

      // Extension initialization
      m_importCncValuesExtensions = ExtensionManager
        .GetExtensions<Lemoine.Extensions.Cnc.IImportCncValuesExtension> ()
        .Where (extension => extension.Initialize (m_machineModule))
        .ToList ();
      m_importCncValueExtensions = ExtensionManager.GetExtensions<Lemoine.Extensions.Cnc.IImportCncValueExtension> ()
        .Where (extension => extension.Initialize (m_machineModule))
        .ToList ();
    }
    #endregion // Constructors

    #region Lemoine.Threading.IChecked implementation
    /// <summary>
    /// Method to call regularly to keep the thread active
    ///
    /// Implements <see cref="Lemoine.Threading.IChecked" />
    /// </summary>
    public void SetActive ()
    {
      if (null != m_caller) {
        m_caller.SetActive ();
      }
    }

    /// <summary>
    /// Implements <see cref="Lemoine.Threading.IChecked" />
    /// </summary>
    public void PauseCheck ()
    {
      if (null != m_caller) {
        m_caller.PauseCheck ();
      }
    }

    /// <summary>
    /// Implements <see cref="Lemoine.Threading.IChecked" />
    /// </summary>
    public void ResumeCheck ()
    {
      if (null != m_caller) {
        m_caller.ResumeCheck ();
      }
    }
    #endregion // Lemoine.Threading.IChecked implementation

    #region IImportData implementation
    /// <summary>
    /// Return true if otherData can be merged with data
    /// 
    /// otherData comes before data
    /// </summary>
    /// <param name="data"></param>
    /// <param name="otherData"></param>
    /// <returns></returns>
    public bool IsMergeable (ExchangeData data, ExchangeData otherData)
    {
      // Key + Value for the aggregation NewValue, else Key only
      if (!otherData.Key.Equals (data.Key)) {
        log.DebugFormat ("IsDataCompatible: " +
                         "the key differs between the new CncValue data {0} and {1} " +
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

      if (m_cache.IsEndCncValueRequired (data.Key, otherData.DateTime, data.DateTime, false)) {
        log.DebugFormat ("IsDataCompatible: " +
                         "IsResetCncValueRequired returned true (may be because of a gap) " +
                         "=> return false");
        return false;
      }

      log.DebugFormat ("IsDataCompatible: " +
                       "CncValue data {0} is compatible with {1}",
                       data, otherData);
      return true;
    }

    /// <summary>
    /// Import data that has been previously merged
    /// </summary>
    /// <param name="datas"></param>
    public void ImportDatas (IList<ExchangeData> datas, CancellationToken cancellationToken = default)
    {
      ImportCncValue (datas[0].Key, datas);
    }
    #endregion // IImportData implementation

    #region Private methods
    void ImportCncValue (string key, IList<ExchangeData> datas)
    {
      Debug.Assert (0 < datas.Count);
      Debug.Assert (object.Equals (m_machineModule.Id, datas[0].MachineModuleId));
      Debug.Assert (object.Equals (key, datas[0].Key));

      IField field = null;
      try {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;

          field = daoFactory.FieldDAO.FindByCode (key);
          if (null == field) {
            log.ErrorFormat ("ImportCncValue: " +
                             "field code={0} is unknown " +
                             "=> skip the record",
                             key);
            return;
          }

          if (!field.Active) {
            log.InfoFormat ("ImportCncValue: " +
                            "field {0} with code {1} is not active " +
                            "=> skip it",
                            field, key);
            return;
          }

          if (SkipOldSecondaryCncValuesAfter.HasValue) {
            ExchangeData lastData = datas[datas.Count - 1];
            Debug.Assert (lastData.DateTime <= DateTime.UtcNow);
            if (!field.Equals (m_machineModule.MonitoredMachine.PerformanceField) &&
                SkipOldSecondaryCncValuesAfter.Value < DateTime.UtcNow.Subtract (lastData.DateTime)) {
              // caution: this generates a lot of log when parsing old data
              log.InfoFormat ("ImportCncValue: " +
                              "do not import {0} dateTime={1} " +
                              "because it is older than {2}",
                              lastData, lastData.DateTime,
                              SkipOldSecondaryCncValuesAfter.Value);
              return;
            }
          }

          {
            DateTime dateTime = datas[0].DateTime;
            ICncValue cncValue = m_cache.GetStoredCncValue (field, dateTime);
            if (null != cncValue) {
              if (dateTime < cncValue.End) {
                log.FatalFormat ("ImportCncValue: " +
                                 "first recorded CncValue at {0} comes before last CncValue {1} " +
                                 "=> skip it (this should not happen)",
                                 datas[0], cncValue);
                return;
              }
            }
          }

          IDictionary<UtcDateTimeRange, object> newCncValues = new Dictionary<UtcDateTimeRange, object> ();

          using (IDAOTransaction transaction = session.BeginTransaction (
            "CncData.ImportCncValue", TransactionLevel.ReadCommitted)) {
            // Extension point
            foreach (ExchangeData data in datas) {
              foreach (var extension in m_importCncValuesExtensions) {
                SetActive ();
                extension.BeforeCncValueProcessing (m_machineModule, field, data.Value, data.DateTime);
              }
            }
            SetActive ();

            switch (field.CncDataAggregationType) {
              case CncDataAggregationType.None:
                log.WarnFormat ("ImportCncValue: " +
                                "aggregation type is None, " +
                                "=> skip the record");
                transaction.Commit (); // Although there is nothing to do
                return;
              case CncDataAggregationType.NewValue: {
                  DateTime beginDateTime = datas[0].DateTime;
                  DateTime endDateTime = datas[0].DateTime;
                  object v = datas[0].Value;
                  /*
                  object v = null;
                  try {
                    switch (field.Type) {
                    case FieldType.String:
                      v = m_converter.ConvertAuto<String> (datas[0].Value);
                      break;
                    case FieldType.Int32:
                      v = m_converter.ConvertAuto<Int32> (datas[0].Value);
                      break;
                    case FieldType.Double:
                      v = m_converter.ConvertAuto<Double> (datas[0].Value);
                      break;
                    case FieldType.Boolean:
                      v = m_converter.ConvertAuto<Boolean> (datas[0].Value);
                      break;
                    default:
                      log.Debug ($"ProcessCncValueNew: other type, field={field.Id} ");
                      v = datas[0].Value;
                      break;
                    }
                  }
                  catch (Exception ex) {
                    log.Warn ($"ImportCncValue: Convert field: field={field.Id}, exception={ex} ");
                    v = datas[0].Value;
                  }
                  */
                  foreach (ExchangeData data in datas) {
                    SetActive ();
                    if (data.Value.ToString ().Equals (v.ToString ())) {
                      // Increase the period
                      endDateTime = data.DateTime;
                    }
                    else { // Process the previous period, and create a new one
                      try {
                        if (ProcessCncValueNew (field, v, beginDateTime, endDateTime)) {
                          UtcDateTimeRange range = CreateRange (beginDateTime, endDateTime);
                          newCncValues[range] = v;
                        }
                      }
                      catch (InvalidCastException) { // New value can't be converted correctly
                        // => skip it
                        log.ErrorFormat ("ImportCncValue: InvalidCastException, skip it");
                      }
                      beginDateTime = data.DateTime;
                      endDateTime = data.DateTime;
                      v = data.Value;
                    }
                  }
                  try {
                    if (ProcessCncValueNew (field, v, beginDateTime, endDateTime)) {
                      UtcDateTimeRange range = CreateRange (beginDateTime, endDateTime);
                      newCncValues[range] = v;
                    }
                  }
                  catch (InvalidCastException) { // New value can't be converted correctly
                                                 // => skip it
                    log.ErrorFormat ("ImportCncValue: InvalidCastException, skip it");
                  }
                }
                break;
              case CncDataAggregationType.Sum:
                ProcessCncValueSum (field, datas);
                break;
              case CncDataAggregationType.Average:
                foreach (ExchangeData data in datas) {
                  SetActive ();
                  ProcessCncValueAverage (field, data.Value, data.DateTime);
                }
                break;
              case CncDataAggregationType.Max:
                foreach (ExchangeData data in datas) {
                  SetActive ();
                  ProcessCncValueMax (field, data.Value, data.DateTime);
                }
                break;
            }

            // Extension point
            foreach (var extension in m_importCncValuesExtensions) {
              SetActive ();
              extension.AfterCncValueProcessing (
                m_machineModule, field, new UtcDateTimeRange (
                  datas[0].DateTime, datas[datas.Count - 1].DateTime, true, true));
              SetActive ();
              extension.BeforeCncValueCommit (m_machineModule, field);
            }
            SetActive ();

            transaction.Commit ();
          } // BeginTransaction

          foreach (var extension in m_importCncValueExtensions) {
            SetActive ();
            foreach (var newCncValue in newCncValues
              .Where (v => v.Key.Lower.HasValue)
              .OrderBy (v => v.Key.Lower.Value.Ticks)) {
              extension.AfterImportNewCncValue (field, newCncValue.Key, newCncValue.Value);
            }
          }

        } // OpenSession
      }
      catch (Exception ex) {
        log.ErrorFormat ("ImportCncValue: " +
                         "exception {0} " +
                         "=> try to reload m_cncValues",
                         ex);
        /* // This assert does not work with the unit tests. TODO: find an alternative
        Debug.Assert (!ModelDAOHelper.DAOFactory.IsSessionActive ());
        */
        if (ModelDAOHelper.DAOFactory.IsSessionActive ()) {
          log.FatalFormat ("ImportCncValue: " +
                           "the session is still active before reloading m_cncValues");
        }
        if (null != field) {
          m_cache.ReloadCncValue (field.Id);
          foreach (var extension in m_importCncValuesExtensions) {
            SetActive ();
            extension.AfterCncValueRollback (m_machineModule.Id, field.Id);
          }
          SetActive ();
        }
        throw;
      }
    }

    UtcDateTimeRange CreateRange (DateTime lower, DateTime upper)
    {
      if (DateTime.Equals (lower, upper)) {
        return new UtcDateTimeRange (lower, upper, "[]");
      }
      else {
        return new UtcDateTimeRange (lower, upper);
      }
    }

    /// <summary>
    /// Process a NewValue data aggregation on a CncValue
    /// 
    /// It must be run inside a transaction
    /// </summary>
    /// <param name="field">Can't be null</param>
    /// <param name="v"></param>
    /// <param name="beginDateTime"></param>
    /// <param name="endDateTime"></param>
    /// <returns>a new value has been inserted</returns>
    public bool ProcessCncValueNew (IField field, object v, DateTime beginDateTime, DateTime endDateTime)
    {
      Debug.Assert (null != field);

      // Get the previous cnc value
      ICncValue cncValue = m_cache.GetStoredReattachedCncValue (field, beginDateTime);

      Debug.Assert (null == cncValue || cncValue.Field.Id == field.Id);
      Debug.Assert (null == cncValue || cncValue.MachineModule.Id == m_machineModule.Id);
      Debug.Assert (null == cncValue || cncValue.End <= beginDateTime);

      log.DebugFormat ("ProcessCncValueNew: /B " +
                       "new value={0} at {1} for field={2} and machineModule={3}",
                       v, beginDateTime, field, m_machineModule);

      if ((null != cncValue) && !cncValue.Stopped) { // Make the previous CncValue longer
        log.DebugFormat ("ProcessCncValueNew: " +
                         "make the previous CncValue {0} longer to {1}",
                         cncValue, beginDateTime);
        if (cncValue.Value.ToString ().Equals (v.ToString ())) { // Same value
          cncValue.End = endDateTime;
          cncValue.Stopped = false;
          m_cache.SaveOrUpdateCncValue (cncValue);
        }
        else { // New value
          cncValue.End = beginDateTime;
          cncValue.Stopped = false;
          // Note: a NewValue CncValue may have a null length
          m_cache.SaveOrUpdateCncValue (cncValue);
        }
      }

      if (cncValue == null || !cncValue.Value.ToString ().Equals (v.ToString ())) { // New value to record
        var newCncValue = ModelDAOHelper.ModelFactory.CreateCncValue (m_machineModule, field, beginDateTime);
        try {
          newCncValue.Value = v;
        }
        catch (InvalidCastException) {
          log.ErrorFormat ("ProcessCncValueNew: InvalidCastException for field {0} and value {1}", field.Id, v);
          // Invalid the cache
          if ((null != cncValue) && !cncValue.Stopped) {
            cncValue.End = beginDateTime;
            cncValue.Stopped = true;
            m_cache.SaveOrUpdateCncValue (cncValue);
          }
          throw;
        }
        newCncValue.End = endDateTime;
        m_cache.SetCncValue (field, newCncValue);
        m_cache.SaveOrUpdateCncValue (newCncValue);
        return true;
      }
      else {
        return false;
      }
    }

    /// <summary>
    /// Process a Sum data aggregation on a CncValue
    /// 
    /// It must be run inside a transaction
    /// </summary>
    /// <param name="field">Can't be null</param>
    /// <param name="datas"></param>
    public void ProcessCncValueSum (IField field, IList<ExchangeData> datas)
    {
      Debug.Assert (null != field);
      Debug.Assert (0 < datas.Count);

      DateTime beginDateTime = datas[0].DateTime;
      DateTime endDateTime = datas[datas.Count - 1].DateTime;

      // Get the previous Cnc value
      ICncValue cncValue = m_cache.GetStoredReattachedCncValue (field, beginDateTime);

      Debug.Assert (null == cncValue || cncValue.Field.Id == field.Id);
      Debug.Assert (null == cncValue || cncValue.MachineModule.Id == m_machineModule.Id);
      Debug.Assert (null == cncValue || cncValue.End <= beginDateTime);

      log.DebugFormat ("ProcessCncValueSum: /B " +
                      "at {0}-{1} for field={2} and machineModule={3}",
                      beginDateTime, endDateTime, field, m_machineModule);

      bool createNew = (null == cncValue);
      if (null == cncValue) {
        cncValue = ModelDAOHelper.ModelFactory.CreateCncValue (m_machineModule, field, beginDateTime);
      }

      cncValue.End = endDateTime;
      switch (field.Type) {
        case FieldType.Boolean:
          log.ErrorFormat ("ProcessCncValueSum: " +
                           "Field type Boolean is not supported here " +
                           "=> skip the record");
          return;
        case FieldType.String:
          foreach (ExchangeData data in datas) {
            cncValue.String += (string)data.Value;
          }
          break;
        case FieldType.Int32:
          if (!cncValue.Int.HasValue) {
            cncValue.Int = 0;
          }

          foreach (ExchangeData data in datas) {
            cncValue.Int += (int)data.Value;
          }
          break;
        case FieldType.Double:
          if (!cncValue.Double.HasValue) {
            cncValue.Double = 0;
          }

          foreach (ExchangeData data in datas) {
            cncValue.Double += (double)data.Value;
          }
          break;
      }

      if (!createNew) {
        cncValue.Stopped = false;
        // Note: a sum CncValue may have a null length
        m_cache.SaveOrUpdateCncValue (cncValue);
      }
      else {
        // Create a new CncValue
        m_cache.SetCncValue (field, cncValue);
        m_cache.SaveOrUpdateCncValue (cncValue);
      }
    }

    /// <summary>
    /// Process an Average data aggregation on a CncValue
    /// It must be run inside a transaction
    /// </summary>
    /// <param name="field">Can't be null</param>
    /// <param name="v"></param>
    /// <param name="dateTime"></param>
    public void ProcessCncValueAverage (IField field, object v, DateTime dateTime)
    {
      Debug.Assert (null != field);

      // Get the previous Cnc Value
      ICncValue cncValue = m_cache.GetStoredReattachedCncValue (field, dateTime);

      Debug.Assert (cncValue == null || cncValue.Field.Id.Equals (field.Id));
      Debug.Assert (cncValue == null || cncValue.MachineModule.Id.Equals (m_machineModule.Id));
      Debug.Assert (cncValue == null || cncValue.End <= dateTime);

      if (log.IsDebugEnabled) {
        log.DebugFormat ("ProcessCncValueAverage: /B " +
                         "new value={0} at {1} for field={2} and machineModule={3}",
                         v, dateTime, field, m_machineModule);
      }

      // cncValue (-) / previousAverageValue (x) / currentAverageValue (+)
      // -----------------
      // xxxxxxxxxxx++++++

      // Get or create previousAverageValue
      ICncValue previousAverageValue = m_cache.GetPreviousValue (field);
      if (null == previousAverageValue) {
        if (null != cncValue) {
          previousAverageValue =
            ModelDAOHelper.ModelFactory.CreateCncValue (m_machineModule, field, cncValue.Begin);
          previousAverageValue.Stopped = cncValue.Stopped;
          previousAverageValue.End = cncValue.End;
          previousAverageValue.Double = cncValue.Double;
          previousAverageValue.Deviation = cncValue.Deviation;
        }
        else {
          previousAverageValue = ModelDAOHelper.ModelFactory.CreateCncValue (m_machineModule, field, dateTime);
          previousAverageValue.End = dateTime;
          previousAverageValue.Double = 0.0;
          previousAverageValue.Deviation = 0.0;
        }
        m_cache.SetPreviousValue (field, previousAverageValue);
      }
      Debug.Assert (null == cncValue || cncValue.Begin.Second.Equals (previousAverageValue.Begin.Second));
      // Note: the number of ms may not be the same between cncValue and previousAverageValue
      // because cncValue may be drawn from the database which does not keep the number of ms

      // Get or create currentAverageValue and update it
      ICncValue currentAverageValue = m_cache.GetCurrentValue (field);
      if (null == currentAverageValue) { // No current average value, set a new one
        if (null != cncValue) {
          Debug.Assert (cncValue.Begin.Equals (previousAverageValue.Begin));
          Debug.Assert (cncValue.End >= previousAverageValue.End);
          DateTime currentAverageValueBegin;
          if (cncValue.Stopped) {
            currentAverageValueBegin = dateTime;
          }
          else {
            currentAverageValueBegin = cncValue.End;
          }
          currentAverageValue =
            ModelDAOHelper.ModelFactory.CreateCncValue (m_machineModule, field, currentAverageValueBegin);
        }
        else {
          currentAverageValue = ModelDAOHelper.ModelFactory.CreateCncValue (m_machineModule, field, dateTime);
        }
        currentAverageValue.End = dateTime;
        currentAverageValue.Double = Convert.ToDouble (v);
        currentAverageValue.Deviation = 0.0;
        Debug.Assert (!currentAverageValue.Stopped);
        if (log.IsDebugEnabled) {
          log.DebugFormat ("ProcessCncValueAverage: " +
                           "initialize currentAverageValue with " +
                           "{0}-{1} avg={2} dev={3}",
                           currentAverageValue.Begin, currentAverageValue.End,
                           currentAverageValue.Double, currentAverageValue.Deviation);
        }
        m_cache.SetCurrentValue (field, currentAverageValue);

        // Extension point
        foreach (var extension in m_importCncValuesExtensions) {
          SetActive ();
          extension.BeforeAddingNewAverageValue (m_machineModule, field, currentAverageValue.Double,
                                                currentAverageValue.DateTimeRange);
          SetActive ();
        }
      }
      else { // update it
        Debug.Assert (currentAverageValue.End <= dateTime);
        Debug.Assert (null != previousAverageValue);
        Debug.Assert (previousAverageValue.End <= currentAverageValue.Begin);
        Debug.Assert (currentAverageValue.Double.HasValue);
        double newValue = Convert.ToDouble (v);
        var newRange = new UtcDateTimeRange (currentAverageValue.End, dateTime);
        Debug.Assert (newRange.Duration.HasValue);
        // Extension point
        foreach (var extension in m_importCncValuesExtensions) {
          SetActive ();
          extension.BeforeAddingNewAverageValue (m_machineModule, field, newValue, newRange);
        }
        SetActive ();
        double newAverage = CombineAverage (currentAverageValue.Double.Value, currentAverageValue.Length,
                                            newValue, newRange.Duration.Value);
        double newDeviation = CombineDeviation (currentAverageValue.Double.Value, currentAverageValue.Deviation.Value, currentAverageValue.Length,
                                                newValue, 0, newRange.Duration.Value,
                                                newAverage);
        if (log.IsDebugEnabled) {
          log.DebugFormat ("ProcessCncValueAverage: " +
                           "current average is going from avg={0} dev={1} dur={2} " +
                           "to avg={3} dev={4} dur={5}",
                           currentAverageValue.Double, currentAverageValue.Deviation, currentAverageValue.Length,
                           newAverage, newDeviation, dateTime.Subtract (currentAverageValue.Begin));
        }
        currentAverageValue.Double = newAverage;
        currentAverageValue.Deviation = newDeviation;
        currentAverageValue.End = dateTime;
        currentAverageValue.Stopped = false;
      }
      Debug.Assert (null != currentAverageValue);

      // Check if currentAverage should be the start of a new CncValue
      if (field.MinTime < currentAverageValue.Length) { // averageMinTime reached
        // Take the decision if it should be the start of a new CncValue
        // or if the data must be combined
        if (null == cncValue) { // No previous CncValue => this should not happen, else length is 0
          log.FatalFormat ("ProcessCncValueAverage: " +
                           "current average length {0} > 0 and no previous cncValue " +
                           "(this should not happen)",
                           currentAverageValue.Length);
          throw new Exception ("Code error");
        }
        Debug.Assert (null != cncValue);
        if (0 == previousAverageValue.Length.Ticks) { // First time
          // First time the minimum duration has been reached
          // current becomes the previous one
          // => start a new CncValue
          log.InfoFormat ("ProcessCncValueAverage: " +
                          "first time with minimum duration " +
                          "=> current becomes previous");
          Debug.Assert (previousAverageValue.Begin.Equals (currentAverageValue.Begin));
          cncValue.Stopped = false;
          cncValue.End = currentAverageValue.End;
          cncValue.Double = currentAverageValue.Double;
          cncValue.Deviation = currentAverageValue.Deviation;
          m_cache.SaveOrUpdateCncValue (cncValue);
        }
        else if (((previousAverageValue.Double + field.LimitDeviation) < currentAverageValue.Double)
                 || (currentAverageValue.Double < (previousAverageValue.Double - field.LimitDeviation))) {
          // the new average is not in [avg-maxdev, avg+maxdev]
          // => start a new CncValue
          log.InfoFormat ("ProcessCncValueAverage: " +
                          "the new average {0} is not in [{1}-{2}, {1}+{2}] " +
                          "=> start a new CncValue",
                          currentAverageValue.Double, previousAverageValue.Double, field.LimitDeviation);
          cncValue.Stopped = previousAverageValue.Stopped;
          cncValue.End = previousAverageValue.End;
          cncValue.Double = previousAverageValue.Double;
          cncValue.Deviation = previousAverageValue.Deviation;
          m_cache.SaveOrUpdateCncValue (cncValue);

          cncValue = currentAverageValue;
          m_cache.SetCncValue (field, cncValue);
          m_cache.SaveOrUpdateCncValue (cncValue);
        }
        else { // the new average is in [avg-maxdev, avg+maxdev] => combine both of them
          CombineAverageCncValues (ref cncValue, previousAverageValue, currentAverageValue);
          log.InfoFormat ("ProcessCncValueAverage: " +
                          "the new average {0} is in [{1}-{2}, {1}+{2}] " +
                          "=> combine both of them to extend CncValue, " +
                          "new average is {3}",
                          currentAverageValue.Double, previousAverageValue.Double, field.LimitDeviation,
                          cncValue.Double);
          Debug.Assert (!cncValue.Stopped);
          m_cache.SaveOrUpdateCncValue (cncValue);
        }

        previousAverageValue = ModelDAOHelper.ModelFactory.CreateCncValue (m_machineModule, field, cncValue.Begin);
        previousAverageValue.Stopped = cncValue.Stopped;
        previousAverageValue.End = cncValue.End;
        previousAverageValue.Double = cncValue.Double;
        previousAverageValue.Deviation = cncValue.Deviation;
        m_cache.SetPreviousValue (field, previousAverageValue);
        m_cache.SetCurrentValue (field, null);
      }
      else { // Else: averageMinTime is not reached, increase the length of cncValue
        // Update cncValue to be the combination of previousAverageValue and currentAverageValue
        if (null == cncValue) {
          Debug.Assert (0 == previousAverageValue.Length.Ticks);
          cncValue =
            ModelDAOHelper.ModelFactory.CreateCncValue (m_machineModule, field, currentAverageValue.Begin);
          cncValue.End = currentAverageValue.End;
          cncValue.Double = currentAverageValue.Double;
          cncValue.Deviation = currentAverageValue.Deviation;
          Debug.Assert (!cncValue.Stopped);
          m_cache.SaveOrUpdateCncValue (cncValue);
          m_cache.SetCncValue (field, cncValue);
        }
        else {
          CombineAverageCncValues (ref cncValue, previousAverageValue, currentAverageValue);
          cncValue.Stopped = false;
          m_cache.SaveOrUpdateCncValue (cncValue);
        }
      }
    }

    /// <summary>
    /// Process an Max data aggregation on a CncValue
    /// It must be run inside a transaction
    /// </summary>
    /// <param name="field">Can't be null</param>
    /// <param name="v"></param>
    /// <param name="dateTime"></param>
    public void ProcessCncValueMax (IField field, object v, DateTime dateTime)
    {
      Debug.Assert (null != field);

      // Get the previous Cnc Value
      ICncValue cncValue = m_cache.GetStoredReattachedCncValue (field, dateTime);

      Debug.Assert (cncValue == null || cncValue.Field.Id.Equals (field.Id));
      Debug.Assert (cncValue == null || cncValue.MachineModule.Id.Equals (m_machineModule.Id));
      Debug.Assert (cncValue == null || cncValue.End <= dateTime);
      Debug.Assert (cncValue == null || cncValue.Double.HasValue);
      Debug.Assert (cncValue == null || cncValue.Deviation.HasValue);

      if (log.IsDebugEnabled) {
        log.DebugFormat ("ProcessCncValueMax: /B " +
                         "new value={0} at {1} for field={2} and machineModule={3}",
                         v, dateTime, field, m_machineModule);
      }

      double newValue = Convert.ToDouble (v);
      if ((null != cncValue)
          && ((dateTime.Subtract (cncValue.Begin) < field.MinTime)
              || (field.LimitDeviation.HasValue
                  && ((cncValue.Double - field.LimitDeviation.Value) <= newValue)))) { // Extend it
        cncValue.End = dateTime;
        cncValue.Deviation = CombineMaxDeviation (cncValue.Double.Value, cncValue.Deviation.Value,
                                                  newValue, 0);
        cncValue.Double = CombineMax (cncValue.Double.Value, newValue);
      }
      else { // Create a new value
        cncValue = ModelDAOHelper.ModelFactory
          .CreateCncValue (m_machineModule, field, dateTime);
        cncValue.End = dateTime;
        cncValue.Deviation = 0.0;
        cncValue.Double = newValue;
      }
      // Create a new CncValue
      cncValue.Stopped = false;
      m_cache.SetCncValue (field, cncValue);
      m_cache.SaveOrUpdateCncValue (cncValue);
    }
    #endregion // Private methods

    #region Utility methods for Average Cnc Data
    double CombineAverage (double average1, TimeSpan period1, double average2, TimeSpan period2)
    {
      if (0 == (int)(period1.TotalSeconds + period2.TotalSeconds)) {
        // Note: this should happen only if the acquisition frequency is less than 1 second
        log.WarnFormat ("CombineAverage: " +
                        "the two input period durations were 0 " +
                        "(this is rounded to the closest number of seconds, " +
                        "this should happen only if the acquiusition frequency is less than 1 s) " +
                        "=> return the average of {0} and {1} as a fallback",
                        average1, average2);
        return (average1 + average2) / 2.0;
      }

      return ((average1 * period1.TotalSeconds) + (average2 * period2.TotalSeconds)) /
        (period1.TotalSeconds + period2.TotalSeconds);
    }

    double CombineDeviation (double average1, double deviation1, TimeSpan period1,
                             double average2, double deviation2, TimeSpan period2,
                             double average)
    {
      double x1 = Pow2 (deviation1) + Pow2 (average1);
      double x2 = Pow2 (deviation2) + Pow2 (average2);
      double variance;
      if (0 == (int)(period1.TotalSeconds + period2.TotalSeconds)) {
        // Note: this should happen only if the acquisition frequency is less than 1 second
        log.WarnFormat ("CombineDeviation: " +
                        "the two input period durations were 0 " +
                        "(this is rounded to the closest number of seconds, " +
                        "this should happen only if the acquiusition frequency is less than 1 s) " +
                        "=> fallback, consider the two period durations are the same");
        variance = (x1 + x2) / 2.0 - Pow2 (average);
      }
      else {
        variance = ((x1 * period1.TotalSeconds) + (x2 * period2.TotalSeconds))
          / (period1.TotalSeconds + period2.TotalSeconds)
          - Pow2 (average);
      }
      Debug.Assert (!variance.Equals (double.NaN));
      variance = Math.Round (variance, 5); // To avoid negative numbers close to 0
      Debug.Assert (0.0 <= variance);
      return Math.Sqrt (variance);
    }

    static double Pow2 (double v)
    {
      return v * v;
    }

    /// <summary>
    /// Combine v1 and v2 in v
    /// v1 is before v2
    /// v must begin like v1
    /// </summary>
    /// <param name="v"></param>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    void CombineAverageCncValues (ref ICncValue v, ICncValue v1, ICncValue v2)
    {
      Debug.Assert (v1.MachineModule.Id.Equals (v2.MachineModule.Id));
      Debug.Assert (v1.Field.Id.Equals (v2.Field.Id));
      Debug.Assert (v1.End <= v2.Begin);

      if (null == v) {
        v = ModelDAOHelper.ModelFactory.CreateCncValue (v1.MachineModule, v1.Field, v1.Begin);
      }

      Debug.Assert (v.MachineModule.Id.Equals (v1.MachineModule.Id));
      Debug.Assert (v.Field.Id.Equals (v1.Field.Id));
      Debug.Assert (v.Begin.Second.Equals (v1.Begin.Second)); // The number of ms may differ (see above)

      double newAverage = CombineAverage (v1.Double.Value, v1.Length,
                                          v2.Double.Value, v2.Length);
      double newDeviation = CombineDeviation (v1.Double.Value, v1.Deviation.Value, v1.Length,
                                              v2.Double.Value, v2.Deviation.Value, v2.Length,
                                              newAverage);
      v.Double = newAverage;
      v.Deviation = newDeviation;
      v.End = v2.End;
    }
    #endregion // Utility methods for Average Cnc Data

    #region Utility methods for Max Cnc Data
    double CombineMax (double max1, double max2)
    {
      return max1 > max2 ? max1 : max2;
    }

    double CombineMaxDeviation (double max1, double deviation1,
                                double max2, double deviation2)
    {
      double max = CombineMax (max1, max2);
      double min1 = max1 - deviation1;
      double min2 = max2 - deviation2;
      double min = min1 < min2 ? min1 : min2;
      return max - min;
    }
    #endregion // Utility methods for Max Cnc Data
  }
}
