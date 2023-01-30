// Copyright (C) 2009-2023 Lemoine Automation Technologies
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

namespace Lemoine.Cnc.DataImport
{
  /// <summary>
  /// Description of ImportDataStamp.
  /// </summary>
  internal sealed class ImportDataStamp: IImportData
  {
    #region Members
    readonly ILog log;
    readonly IMachineModule m_machineModule;
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
    public ImportDataStamp (IMachineModule machineModule)
    {
      Debug.Assert (null != machineModule);
      
      m_machineModule = machineModule;
      log = LogManager.GetLogger (string.Format ("{0}.{1}.{2}",
                                                 typeof (ImportDataStamp).FullName,
                                                 machineModule.MonitoredMachine.Id,
                                                 machineModule.Id));
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
      if (!otherData.Key.Equals (data.Key) || !otherData.Value.Equals (data.Value)) {
        log.DebugFormat ("IsDataCompatible: " +
                         "the new Stamp data {0} does not contain the same key/value than {1} " +
                         "=> not compatible",
                         data, otherData);
        return false;
      }
      
      log.DebugFormat ("IsDataCompatible: " +
                       "exactly the same stamp data {0} as the previous one {1} " +
                       "=> compatible",
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
      ImportStamp(firstData.Key, (double)firstData.Value, firstData.DateTime);
    }
    #endregion // IImportData implementation
    
    #region Private methods
    void ImportStamp(string key, double stampIdMilestone, DateTime dateTime)
    {
      var stampId = (int)Math.Floor (stampIdMilestone);
      var sequenceMilestoneMin = 10000 * (stampIdMilestone - stampId);

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession())
      {
        DateTime effectiveDateTime = NullableDateTime.TruncateToSeconds(dateTime);
        IStamp stamp;
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction(
          "CncData.ImportStamp.CheckStamp", TransactionLevel.ReadCommitted))
        {
          stamp = ModelDAOHelper.DAOFactory.StampDAO.FindById(stampId);
          if (null == stamp) {
            log.ErrorFormat ("ImportStamp: " +
                             "stamp with Id={2} does not exist " +
                             "=> skip the record with machineModuleId={0} key={1} stampId={2} dateTime={3}",
                             m_machineModule.Id, key, stampId, dateTime);
            // Read-only transaction: nothing to commit
            return;
          }
        }
        using (IDAOTransaction transaction = session.BeginTransaction(
          "CncData.ImportStamp.Process", TransactionLevel.ReadCommitted))
        {
          // - MachineModuleDetection
          IMachineModuleDetection detection = ModelDAOHelper.ModelFactory
            .CreateMachineModuleDetection (m_machineModule, effectiveDateTime); // machine module
          detection.Stamp = stamp;
          if (0 != sequenceMilestoneMin) {
            detection.SequenceMilestone = TimeSpan.FromMinutes (sequenceMilestoneMin);
          }
          detection.Name = key;
          ModelDAOHelper.DAOFactory.MachineModuleDetectionDAO.MakePersistent(detection);

          // - DetectionTimeStamp
          var detectionTimeStamp = ModelDAOHelper.DAOFactory.AcquisitionStateDAO.GetAcquisitionState (m_machineModule, AcquisitionStateKey.Detection);
          if (null == detectionTimeStamp) {
            detectionTimeStamp = ModelDAOHelper.ModelFactory.CreateAcquisitionState(m_machineModule, AcquisitionStateKey.Detection);
          }
          detectionTimeStamp.DateTime = effectiveDateTime;
          ModelDAOHelper.DAOFactory.AcquisitionStateDAO.MakePersistent(detectionTimeStamp);
          
          // - Commit
          transaction.Commit ();
        }
      }
    }
    #endregion // Private methods
  }
}
