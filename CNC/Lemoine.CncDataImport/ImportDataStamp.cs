// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2024-2026 Atsora Solutions
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
  /// Description of ImportDataStamp.
  /// </summary>
  internal sealed class ImportDataStamp : IImportData
  {
    static readonly string CREATE_STAMP_KEY = "CncData.ImportStamp.CreateStamp";
    static readonly bool CREATE_STAMP_DEFAULT = false;

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
    public ImportDataStamp (IMachineModule machineModule)
    {
      Debug.Assert (null != machineModule);

      m_machineModule = machineModule;
      log = LogManager.GetLogger ($"{typeof (ImportDataStamp).FullName}.{machineModule.MonitoredMachine.Id}.{machineModule.Id}");
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
      if (!otherData.Key.Equals (data.Key) || !otherData.Value.Equals (data.Value)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"IsMergeable: the new Stamp data {data} does not contain the same key/value than {otherData} => not compatible");
        }
        return false;
      }

      if (log.IsDebugEnabled) {
        log.Debug ($"IsMergeable: exactly the same stamp data {data} as the previous one {otherData} => compatible");
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
      try {
        ImportStamp (firstData.Key, (double)firstData.Value, firstData.DateTime);
      }
      catch (Exception ex) {
        log.Error ($"ImportDatas: exception in ImportStamp", ex);
        throw;
      }
    }
    #endregion // IImportData implementation

    void ImportStamp (string key, double stampIdMilestone, DateTime dateTime)
    {
      var stampId = (int)Math.Floor (stampIdMilestone);
      var sequenceMilestoneMin = 10000 * (stampIdMilestone - stampId);
      if (log.IsDebugEnabled) {
        log.Debug ($"ImportStamp: stampIdMilestone={stampIdMilestone} stampId={stampId} sequenceKilestoneMin={sequenceMilestoneMin}");
      }

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        DateTime effectiveDateTime = NullableDateTime.TruncateToSeconds (dateTime);
        IStamp stamp;
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction (
          "CncData.ImportStamp.CheckStamp", TransactionLevel.ReadCommitted)) {
          stamp = ModelDAOHelper.DAOFactory.StampDAO.FindById (stampId);
        }
        if (stamp is null) {
          if (Lemoine.Info.ConfigSet.LoadAndGet (CREATE_STAMP_KEY, CREATE_STAMP_DEFAULT)) {
            using (IDAOTransaction transaction = session.BeginTransaction (
              "CncData.ImportStamp.CreateStamp", TransactionLevel.ReadCommitted)) {
              var sqlRequest = $"""
                INSERT INTO stamp (stampid)
                VALUES ({stampId});
                """;
              ModelDAOHelper.DAOFactory.ExecuteNonQuery (sqlRequest);
              var sequenceRequest = $"""
                SELECT setval(
                    'stamp_stampid_seq', 
                    GREATEST(15, nextval('stamp_stampid_seq')-1)
                );
                """;
              ModelDAOHelper.DAOFactory.ExecuteNonQuery (sequenceRequest);
              stamp = ModelDAOHelper.DAOFactory.StampDAO.FindById (stampId);
              if (stamp is null) {
                log.Fatal ($"ImportStamp: stamp with Id={stampId} was not created => skip the record with machineModuleId={m_machineModule.Id} key={key} stampId={stampId} dateTime={dateTime}");
                transaction.Rollback ();
                return;
              }
              transaction.Commit ();
            }
          }
          else {
            log.Error ($"ImportStamp: stamp with Id={stampId} does not exist => skip the record with machineModuleId={m_machineModule.Id} key={key} stampId={stampId} dateTime={dateTime}");
            // Read-only transaction: nothing to commit
            return;
          }
        }
        using (IDAOTransaction transaction = session.BeginTransaction (
          "CncData.ImportStamp.Process", TransactionLevel.ReadCommitted)) {
          // - MachineModuleDetection
          IMachineModuleDetection detection = ModelDAOHelper.ModelFactory
            .CreateMachineModuleDetection (m_machineModule, effectiveDateTime); // machine module
          detection.Stamp = stamp;
          if (0 != sequenceMilestoneMin) {
            detection.SequenceMilestone = TimeSpan.FromMinutes (sequenceMilestoneMin);
          }
          detection.Name = key;
          ModelDAOHelper.DAOFactory.MachineModuleDetectionDAO.MakePersistent (detection);

          // - DetectionTimeStamp
          var detectionTimeStamp = ModelDAOHelper.DAOFactory.AcquisitionStateDAO.GetAcquisitionState (m_machineModule, AcquisitionStateKey.Detection);
          if (detectionTimeStamp is null) {
            detectionTimeStamp = ModelDAOHelper.ModelFactory.CreateAcquisitionState (m_machineModule, AcquisitionStateKey.Detection);
          }
          detectionTimeStamp.DateTime = effectiveDateTime;
          ModelDAOHelper.DAOFactory.AcquisitionStateDAO.MakePersistent (detectionTimeStamp);

          transaction.Commit ();
        }
      }
    }
  }
}
