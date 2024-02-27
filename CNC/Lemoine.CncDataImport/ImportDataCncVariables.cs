// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2023 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Cnc.Data;
using Lemoine.Info;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Extensions.Cnc;
using Lemoine.Extensions;
using System.Threading;
using System.Text.Json;

namespace Lemoine.CncDataImport
{
  /// <summary>
  /// This class import cnc variables in tables: cncvariable and optionally some others from extensions
  /// </summary>
  internal sealed class ImportDataCncVariables : IImportData
  {
    #region Members
    readonly ILog log;
    readonly IMachineModule m_machineModule;
    IEnumerable<IImportCncVariablesExtension> m_extensions;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Last datetime when the method "ImportDatas" has been visited
    /// (automatically set by ImportCncValueFromQueue)
    /// </summary>
    public DateTime LastVisitDateTime { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineModule">not null</param>
    public ImportDataCncVariables (IMachineModule machineModule)
    {
      Debug.Assert (null != machineModule);

      m_machineModule = machineModule;
      log = LogManager.GetLogger (string.Format ("{0}.{1}.{2}",
                                                 typeof (ImportDataCncVariables).FullName,
                                                 machineModule.MonitoredMachine.Id,
                                                 machineModule.Id));

      m_extensions = ExtensionManager
        .GetExtensions<Lemoine.Extensions.Cnc.IImportCncVariablesExtension> ()
        .Where (i => i.Initialize (machineModule))
        .ToList ();
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
      // Note: the key is not used
      // Both must be CncVariableSet
      if ((ExchangeDataCommand.CncVariableSet != otherData.Command)
        || (ExchangeDataCommand.CncVariableSet != data.Command)) {
        log.FatalFormat ("IsMergeable: invalid data key {0} or {1}",
          otherData.Command, data.Command);
        Debug.Assert (false);
        throw new ArgumentException ("IsMergeable: wrong data key");
      }

      var set1 = data.Value as IDictionary<string, object>;
      var set2 = otherData.Value as IDictionary<string, object>;
      return object.Equals (set1, set2);
    }

    /// <summary>
    /// Import data that has been previously merged
    /// </summary>
    /// <param name="datas"></param>
    public void ImportDatas (IList<ExchangeData> datas, CancellationToken cancellationToken = default)
    {
      Debug.Assert (datas.Any ());

      var firstData = datas.First ();
      Debug.Assert (firstData.DateTime <= datas.Last ().DateTime);
      var variableSet = firstData.Value as IDictionary<string, object>;
      ImportCncVariableSet (variableSet, NullableDateTime.TruncateToSeconds (firstData.DateTime));
    }
    #endregion // IImportData implementation

    #region Private methods
    void ImportCncVariableSet (IDictionary<string, object> cncVariableSet, DateTime startDatetime)
    {
      IDictionary<string, object> changedVariables = new Dictionary<string, object> ();
      foreach (var item in cncVariableSet) {
        if (item.Key is null) {
          log.Fatal ($"ImportCncVariableSet: key is null at {startDatetime}");
        }
        else { // Key is not null
          if (item.Value is null) {
            log.Warn ($"ImportCncVariableSet: null data for {item.Key} at {startDatetime}");
            if (TryStopSingleCncVariable (item.Key, startDatetime)) {
              changedVariables.Remove (item.Key);
            }
          }
          else {
            if (log.IsDebugEnabled) {
              log.Debug ($"ImportCncVariableSet: {item.Key}={item.Value} at {startDatetime}");
            }
            object v = item.Value;
            if (v is JsonElement jsonElement) {
              if (jsonElement.ValueKind.Equals (JsonValueKind.Number)) {
                if (jsonElement.TryGetInt64 (out var n)) {
                  v = n;
                }
                else if (jsonElement.TryGetDouble (out var d)) {
                  v = d;
                }
              }
              else if (jsonElement.ValueKind.Equals (JsonValueKind.String)) {
                v = jsonElement.GetString ();
              }
            }
            if (TryImportSingleCncVariable (item.Key, v, startDatetime)) {
              changedVariables[item.Key] = v;
            }
          }
        }
      }
      foreach (var extension in m_extensions) {
        extension.AfterImportCncVariables (changedVariables, startDatetime);
      }
    }

    bool TryImportSingleCncVariable (string key, object v, DateTime startDateTime)
    {
      Debug.Assert (!string.IsNullOrEmpty (key), "key must be not null");
      Debug.Assert (v != null, "v must be not null");

      try {
        return ImportSingleCncVariable (key, v, startDateTime);
      }
      catch (Exception ex) {
        log.Error ("TryImportSingleCncVariable: exception in ImportSingleCncVariable", ex);
        if (ex.Message.ToLowerInvariant ().Contains ("serializ")) {
          // This is unexpected... but not blocking... just skip the data
          log.Fatal ($"TryImportSingleCncVariable: serialization failure of {v} for key {key} at {startDateTime}, but continue", ex);
          return false;
        }
        throw;
      }
    }

    bool ImportSingleCncVariable (string key, object v, DateTime startDateTime)
    {
      Debug.Assert (!string.IsNullOrEmpty (key), "key must be not null");
      Debug.Assert (v != null, "v must be not null");

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginTransaction ("CncData.ImportCncVariables.SingleCncVariable", TransactionLevel.ReadCommitted)) {
          var cncVariable = ModelDAOHelper.DAOFactory.CncVariableDAO.FindAt (m_machineModule, key, startDateTime);
          if (null == cncVariable) {
            cncVariable = ModelDAOHelper.ModelFactory.CreateCncVariable (m_machineModule, new UtcDateTimeRange (startDateTime), key, v);
            ModelDAOHelper.DAOFactory.CncVariableDAO.MakePersistent (cncVariable);
            transaction.Commit ();
            return true;
          }
          else { // null != cncVariable
            if (cncVariable.DateTimeRange.Upper.HasValue) {
              var timeLag = cncVariable.DateTimeRange.Upper.Value.Subtract (startDateTime);
              log.FatalFormat ("ImportCncVariable: cnc variable id={0} has an upper bound, which is unexpected, this is probably due by a time change => remove the future data as well, after {1}, time lag={2}", cncVariable.Id, cncVariable.DateTimeRange.Upper.Value, timeLag);
              var futureRange = new UtcDateTimeRange (cncVariable.DateTimeRange.Upper.Value);
              var futureCncVariables = ModelDAOHelper.DAOFactory.CncVariableDAO
                .FindOverlapsRange (m_machineModule, key, futureRange);
              foreach (var futureCncVariable in futureCncVariables) {
                ModelDAOHelper.DAOFactory.CncVariableDAO.MakeTransient (futureCncVariable);
              }
              var message = string.Format ("ImportCncVariable: time lag of {0} in the past - {1} future data items removed",
                timeLag, futureCncVariables.Count ());
              var cncDataImportLog = ModelDAOHelper.ModelFactory.CreateCncDataImportLog (LogLevel.WARN,
                message,
                m_machineModule.MonitoredMachine,
                m_machineModule);
              ModelDAOHelper.DAOFactory.CncDataImportLogDAO.MakePersistent (cncDataImportLog);
              ModelDAOHelper.DAOFactory.Flush ();
            }
            if (!object.Equals (cncVariable.Value, v)) { // update it
              if (Bound.Equals (cncVariable.DateTimeRange.Lower, startDateTime)) { // It will become empty
                ModelDAOHelper.DAOFactory.CncVariableDAO.MakeTransient (cncVariable);
              }
              else {
                cncVariable.Stop (startDateTime);
                ModelDAOHelper.DAOFactory.CncVariableDAO.MakePersistent (cncVariable);
              }
              // New item
              cncVariable = ModelDAOHelper.ModelFactory.CreateCncVariable (m_machineModule, new UtcDateTimeRange (startDateTime), key, v);
              ModelDAOHelper.DAOFactory.CncVariableDAO.MakePersistent (cncVariable);
              transaction.Commit ();
              return true;
            }
            else if (LowerBound.Equals (cncVariable.DateTimeRange.Lower, startDateTime)) { // Replay of the data
              transaction.Commit ();
              return true;
            }
            else {
              transaction.Commit ();
              return false;
            }
          }
        }
      }
    }

    bool TryStopSingleCncVariable (string key, DateTime startDateTime)
    {
      try {
        return StopSingleCncVariable (key, startDateTime);
      }
      catch (Exception ex) {
        log.Error ("TryStopSingleCncVariable: exception in StopSingleCncVariable", ex);
        if (ex.Message.ToLowerInvariant ().Contains ("serializ")) {
          // This is unexpected... but not blocking... just skip the data
          log.Fatal ($"TryStopSingleCncVariable: serialization failure of key {key} at {startDateTime}, but continue", ex);
          return false;
        }
        throw;
      }
    }

    bool StopSingleCncVariable (string key, DateTime startDateTime)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginTransaction ("CncData.ImportCncVariables.StopSingleCncVariable", TransactionLevel.ReadCommitted)) {
          ICncVariable cncVariable = ModelDAOHelper.DAOFactory.CncVariableDAO.FindAt (m_machineModule, key, startDateTime);
          if (cncVariable is null) {
            if (log.IsDebugEnabled) {
              log.Debug ($"StopSingleCncVariable: nothing to do");
            }
            transaction.Commit ();
            return true;
          }
          else { // null != cncVariable
            if (cncVariable.DateTimeRange.Upper.HasValue) {
              var timeLag = cncVariable.DateTimeRange.Upper.Value.Subtract (startDateTime);
              log.FatalFormat ("StopSingleCncVariable: cnc variable id={0} has an upper bound, which is unexpected, this is probably due by a time change => remove the future data as well, after {1}, time lag={2}", cncVariable.Id, cncVariable.DateTimeRange.Upper.Value, timeLag);
              var futureRange = new UtcDateTimeRange (cncVariable.DateTimeRange.Upper.Value);
              var futureCncVariables = ModelDAOHelper.DAOFactory.CncVariableDAO
                .FindOverlapsRange (m_machineModule, key, futureRange);
              foreach (var futureCncVariable in futureCncVariables) {
                ModelDAOHelper.DAOFactory.CncVariableDAO.MakeTransient (futureCncVariable);
              }
              var message = $"StopSingleCncVariable: time lag of {timeLag} in the past - {futureCncVariables.Count} future data items removed";
              var cncDataImportLog = ModelDAOHelper.ModelFactory.CreateCncDataImportLog (LogLevel.WARN,
                message,
                m_machineModule.MonitoredMachine,
                m_machineModule);
              ModelDAOHelper.DAOFactory.CncDataImportLogDAO.MakePersistent (cncDataImportLog);
            }
            if (Bound.Equals (cncVariable.DateTimeRange.Lower, startDateTime)) { // It will become empty
              ModelDAOHelper.DAOFactory.CncVariableDAO.MakeTransient (cncVariable);
            }
            else {
              cncVariable.Stop (startDateTime);
              ModelDAOHelper.DAOFactory.CncVariableDAO.MakePersistent (cncVariable);
            }
            transaction.Commit ();
            return true;
          }
        }
      }
    }
    #endregion // Private methods
  }
}
