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
  /// Import the stops and the cuts of the cnc values in table cncdataperiod:
  /// <item>stop: the current period gets the Stopped property (its end remains the date/time of the last value),
  ///       the next value starts a new period</item>
  /// <item>cut (StopCncValue with the value Cut, see <see cref="ExchangeData.BuildCutCncValueExchangeData"/>):
  ///       the current period ends at the cut and the next value starts a new period at the cut, without any interruption</item>
  /// For a custom aggregation, both are a stop of the extension
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
    /// <see cref="IImportData"/>: same key and both stops or both cuts (duplicate)
    /// </summary>
    public bool IsMergeable (ExchangeData data, ExchangeData otherData)
    {
      return data.Key.Equals (otherData.Key) && (data.IsCut == otherData.IsCut);
    }

    /// <summary>
    /// <see cref="IImportData"/>
    /// </summary>
    public void ImportDatas (IList<ExchangeData> datas, CancellationToken cancellationToken = default)
    {
      var key = datas[0].Key;
      var dateTime = datas[0].PreciseDateTime;
      var cut = datas[0].IsCut;

      IField field = null;
      try {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          field = ModelDAOHelper.DAOFactory.FieldDAO.FindByCode (key);
          if (null == field) {
            log.Error ($"ImportDatas: field code={key} is unknown => skip the record");
            return;
          }
          if (!field.Active || (field.GetCncDataStorage () != CncDataStorage.CncDataPeriod)) {
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
              if (log.IsDebugEnabled) {
                log.Debug ($"ImportDatas: {(cut ? "cut" : "stop")} {state.Period} at {dateTime}");
              }
              if (cut) {
                m_importDataCncDataPeriods.ProcessCut (field, state, dateTime);
              }
              else {
                m_importDataCncDataPeriods.ProcessStop (field, state, dateTime);
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
