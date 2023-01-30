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
  /// Description of ImportDataDetectionTimestamp.
  /// </summary>
  internal sealed class ImportDataDetectionTimestamp: IImportData
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
    public ImportDataDetectionTimestamp (IMachineModule machineModule)
    {
      Debug.Assert (null != machineModule);
      
      m_machineModule = machineModule;
      log = LogManager.GetLogger (string.Format ("{0}.{1}.{2}",
                                                 typeof (ImportDataDetectionTimestamp).FullName,
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
      log.DebugFormat ("IsDataCompatible: " +
                       "DetectionTimeStamp several times now={0} previously={1} " +
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
      // Last datetime
      var dateTime = datas[datas.Count - 1].DateTime;
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        DateTime effectiveDateTime = NullableDateTime.TruncateToSeconds(dateTime);
        using (IDAOTransaction transaction = session.BeginTransaction(
          "CncData.ImportDataDetectionTimeStamp", TransactionLevel.ReadCommitted))
        {
          var detectionTimeStamp = ModelDAOHelper.DAOFactory.AcquisitionStateDAO.GetAcquisitionState (m_machineModule, AcquisitionStateKey.Detection);
          if (null == detectionTimeStamp) {
            detectionTimeStamp = ModelDAOHelper.ModelFactory.CreateAcquisitionState (m_machineModule, AcquisitionStateKey.Detection);
          }
          detectionTimeStamp.DateTime = effectiveDateTime;
          ModelDAOHelper.DAOFactory.AcquisitionStateDAO.MakePersistent(detectionTimeStamp);
          transaction.Commit();
        }
      }
    }
    #endregion // IImportData implementation
  }
}
