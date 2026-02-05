// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Cnc.Data;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System.Linq;
using System.Threading;

namespace Lemoine.CncDataImport
{
  /// <summary>
  /// Description of ImportDataSequenceMilestone.
  /// </summary>
  internal sealed class ImportDataSequenceMilestone : IImportData
  {
    readonly ILog log;
    readonly IMachineModule m_machineModule;

    /// <summary>
    /// Last datetime when the method "ImportDatas" has been visited
    /// (automatically set by ImportCncValueFromQueue)
    /// </summary>
    public DateTime LastVisitDateTime { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineModule">not null</param>
    public ImportDataSequenceMilestone (IMachineModule machineModule)
    {
      Debug.Assert (null != machineModule);

      m_machineModule = machineModule;
      log = LogManager.GetLogger ($"{typeof (ImportDataSequenceMilestone).FullName}.{machineModule.MonitoredMachine.Id}.{machineModule.Id}");
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
      if (log.IsDebugEnabled) {
        log.Debug ($"IsMergeable: always mergeable since only the latest one is kept");
      }
      return true;
    }

    /// <summary>
    /// Import data that has been previously merged
    /// </summary>
    /// <param name="datas"></param>
    public void ImportDatas (IList<ExchangeData> datas, CancellationToken cancellationToken = default)
    {
      var lastData = datas.Last ();
      ImportSequenceMilestone (lastData.Key, (double)lastData.Value, lastData.DateTime);
    }
    #endregion // IImportData implementation

    void ImportSequenceMilestone (string key, double sequenceMilestone, DateTime dateTime)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        DateTime effectiveDateTime = NullableDateTime.TruncateToSeconds (dateTime);
        using (IDAOTransaction transaction = session.BeginTransaction (
          "CncData.ImportStamp.Process", TransactionLevel.ReadCommitted)) {
          // - MachineModuleDetection
          IMachineModuleDetection detection = ModelDAOHelper.ModelFactory
            .CreateMachineModuleDetection (m_machineModule, effectiveDateTime); // machine module
          detection.SequenceMilestone = TimeSpan.FromSeconds (sequenceMilestone);
          detection.Name = key;
          ModelDAOHelper.DAOFactory.MachineModuleDetectionDAO.MakePersistent (detection);

          // - DetectionTimeStamp
          var detectionTimeStamp = ModelDAOHelper.DAOFactory.AcquisitionStateDAO.GetAcquisitionState (m_machineModule, AcquisitionStateKey.Detection);
          if (null == detectionTimeStamp) {
            detectionTimeStamp = ModelDAOHelper.ModelFactory.CreateAcquisitionState (m_machineModule, AcquisitionStateKey.Detection);
          }
          detectionTimeStamp.DateTime = effectiveDateTime;
          ModelDAOHelper.DAOFactory.AcquisitionStateDAO.MakePersistent (detectionTimeStamp);

          // - Commit
          transaction.Commit ();
        }
      }
    }
  }
}
