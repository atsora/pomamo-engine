// Copyright (C) 2009-2023 Lemoine Automation Technologies
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
using System.Threading;

namespace Lemoine.Cnc.DataImport
{
  /// <summary>
  /// Description of ImportDataStopCncValue.
  /// </summary>
  internal sealed class ImportDataStopCncValues: IImportData
  {
    #region Members
    readonly ILog log;
    readonly IMachineModule m_machineModule;
    readonly Cache.CacheCncValue m_cache;
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
    /// cncCache must be the same than in ImportDataCncValues
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="cncCache"></param>
    public ImportDataStopCncValues(IMachineModule machineModule, Cache.CacheCncValue cncCache)
    {
      Debug.Assert (null != machineModule);
      
      m_machineModule = machineModule;
      log = LogManager.GetLogger (string.Format ("{0}.{1}.{2}",
                                                 typeof (ImportDataStopCncValues).FullName,
                                                 machineModule.MonitoredMachine.Id,
                                                 machineModule.Id));
      m_cache = cncCache;
    }
    #endregion // Constructors
    
    #region IImportData implementation
    /// <summary>
    /// Return true if otherData can be merged with data
    /// </summary>
    /// <param name="data"></param>
    /// <param name="otherData"></param>
    /// <returns></returns>
    public bool IsMergeable(ExchangeData data, ExchangeData otherData)
    {
      // Compatible if same key (duplicate!)
      if (!data.Key.Equals(otherData.Key)) {
        log.DebugFormat("IsDataCompatible: " +
                        "the key differs between the new StopCncValue data {0} and {1} " +
                        "=> not compatible",
                        data, otherData);
        return false;
      }
      log.DebugFormat("IsDataCompatible: " +
                      "StopCncValue {0} is compatible with {1}",
                      data, otherData);
      return true;
    }
    
    /// <summary>
    /// Import data that has been previously merged
    /// </summary>
    /// <param name="datas"></param>
    public void ImportDatas(IList<ExchangeData> datas, CancellationToken cancellationToken = default)
    {
      var firstData = datas[0];
      ImportStopCncValue(firstData.Key, firstData.DateTime);
    }
    #endregion // IImportData implementation
    
    #region Private methods
    void ImportStopCncValue (string key, DateTime dateTime)
    {
      log.DebugFormat ("ImportStopCncValue /B " +
                       "machineModuleId={0} key={1} dateTime={2}",
                       m_machineModule.Id, key, dateTime);
      
      IField field = null;
      try {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession())
        {
          IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
          
          field = daoFactory.FieldDAO.FindByCode(key);
          if (field == null) {
            log.ErrorFormat ("ImportStopCncValue: " +
                             "field code={0} is unknown " +
                             "=> skip the record",
                             key);
            return;
          }
          
          if (!field.Active) {
            log.InfoFormat ("ImportStopCncValue: " +
                            "field {0} with code {1} is not active " +
                            "=> skip it",
                            field, key);
            return;
          }
          
          if (m_machineModule == null) {
            log.ErrorFormat ("ImportStopCncValue: " +
                             "machine module is unknown " +
                             "=> skip the record");
            return;
          }
          
          ICncValue cncValue = m_cache.GetStoredCncValue(field, dateTime);
          if (cncValue == null) {
            // Note: if null == cncvalue, we do not take care of it
            log.DebugFormat ("ImportStopCncValue: " +
                             "key={0} " +
                             "stored cncValue is already null " +
                             "=> do nothing",
                             key);
          }
          else { // null != cncValue
            if (dateTime < cncValue.End) {
              log.FatalFormat ("ImportStopCncValue: " +
                               "recorded CncValue at {0} comes before last CncValue end {1} " +
                               "=> skip it " +
                               "(this should not happen)",
                               dateTime, cncValue.End);
              return;
            }
            
            if (cncValue != null && cncValue.Id != 0) { // Re-attach the cncValue
              daoFactory.CncValueDAO.UpgradeLock(cncValue);
            }
            using (IDAOTransaction transaction = session.BeginTransaction(
              "CncData.ImportStopCncValue", TransactionLevel.ReadCommitted))
            {
              log.DebugFormat ("ImportStopCncValue: " +
                               "stop cncValue {0} for key={1}",
                               cncValue, key);
              cncValue.Stopped = true;
              ModelDAOHelper.DAOFactory.CncValueDAO.MakePersistent (cncValue);
              transaction.Commit ();
            }
            ICncValue currentAverageValue = m_cache.GetCurrentValue (field);
            if (null != currentAverageValue) {
              currentAverageValue.Stopped = true;
              m_cache.SetPreviousValue (field, currentAverageValue);
              m_cache.SetCurrentValue (field, null);
            }
            else {
              ICncValue previousAverageValue = m_cache.GetPreviousValue (field);
              if (null != previousAverageValue) {
                previousAverageValue.Stopped = true;
              }
            }
          }
        }
      }
      catch (Exception ex) {
        log.ErrorFormat ("ImportStopCncValue: " +
                         "exception {0} " +
                         "=> try to reload m_cncValues",
                         ex);
        Debug.Assert (!ModelDAOHelper.DAOFactory.IsSessionActive ());
        if (ModelDAOHelper.DAOFactory.IsSessionActive ()) {
          log.FatalFormat ("ImportStopCncValue: " +
                           "the session is still active before reloading m_cncValues");
        }
        if (null != field) {
          m_cache.ReloadCncValue(field.Id);
        }
        throw;
      }
    }
    #endregion // Private methods
  }
}
