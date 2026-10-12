// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Lemoine.Cnc.Data;
using Lemoine.CncDataImport.Cache;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.CncDataImport
{
  /// <summary>
  /// Import the stops of the cnc values in table cncdataperiod:
  /// the current period of the field gets the Stopped property, the next value starts a new period
  /// </summary>
  internal sealed class ImportDataStopCncDataPeriods : IImportData
  {
    readonly ILog log;
    readonly IMachineModule m_machineModule;
    readonly CacheCncDataPeriod m_cache;
    readonly ImportDataCncDataPeriods m_importDataCncDataPeriods;

    /// <summary>
    /// <see cref="IImportData"/>
    /// </summary>
    public DateTime LastVisitDateTime { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="cache">not null, shared with importDataCncDataPeriods</param>
    /// <param name="importDataCncDataPeriods">not null, for the extensions</param>
    public ImportDataStopCncDataPeriods (IMachineModule machineModule, CacheCncDataPeriod cache, ImportDataCncDataPeriods importDataCncDataPeriods)
    {
      Debug.Assert (null != machineModule);

      m_machineModule = machineModule;
      m_cache = cache;
      m_importDataCncDataPeriods = importDataCncDataPeriods;
      log = LogManager.GetLogger ($"{typeof (ImportDataStopCncDataPeriods).FullName}.{machineModule.MonitoredMachine.Id}.{machineModule.Id}");
    }

    /// <summary>
    /// <see cref="IImportData"/>: same key (duplicate)
    /// </summary>
    public bool IsMergeable (ExchangeData data, ExchangeData otherData)
    {
      return data.Key.Equals (otherData.Key);
    }

    /// <summary>
    /// <see cref="IImportData"/>
    /// </summary>
    public void ImportDatas (IList<ExchangeData> datas, CancellationToken cancellationToken = default)
    {
      var key = datas[0].Key;
      var dateTime = datas[0].DateTime;

      IField field = null;
      try {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          field = ModelDAOHelper.DAOFactory.FieldDAO.FindByCode (key);
          if (null == field) {
            log.Error ($"ImportDatas: field code={key} is unknown => skip the record");
            return;
          }
          if (!field.Active || !field.GetCncDataStorage ().HasFlag (CncDataStorage.CncDataPeriod)) {
            return;
          }
          var kind = field.GetPeriodAggregationKind ();
          if (CncDataPeriodAggregationKind.None == kind) {
            return;
          }

          using (var transaction = session.BeginTransaction ("CncData.ImportStopCncDataPeriod", TransactionLevel.ReadCommitted)) {
            if (CncDataPeriodAggregationKind.Custom == kind) {
              m_importDataCncDataPeriods.GetExtension (field)?.Stop (m_machineModule, field, dateTime);
            }
            else {
              var state = m_cache.Get (field);
              var period = state.Period;
              if ((null != period) && !period.Stopped) {
                if (log.IsDebugEnabled) {
                  log.Debug ($"ImportDatas: stop {period} at {dateTime}");
                }
                period.Stopped = true;
                ModelDAOHelper.DAOFactory.CncDataPeriodDAO.MakePersistent (period);
                state.BaseStatistics = period.Statistics;
                state.SegmentBegin = period.End;
                state.SegmentStatistics = null;
              }
            }
            transaction.Commit ();
          }
        }
      }
      catch (Exception ex) {
        log.Error ("ImportDatas: exception => reset the state of the field", ex);
        if (null != field) {
          m_importDataCncDataPeriods.Reset (field.Id);
        }
        throw;
      }
    }
  }
}
