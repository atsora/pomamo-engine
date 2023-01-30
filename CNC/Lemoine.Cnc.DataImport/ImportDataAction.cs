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
  /// Description of ImportDataAction.
  /// </summary>
  internal sealed class ImportDataAction: IImportData
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
    public ImportDataAction (IMachineModule machineModule)
    {
      Debug.Assert (null != machineModule);
      
      m_machineModule = machineModule;
      log = LogManager.GetLogger (string.Format ("{0}.{1}.{2}",
                                                 typeof (ImportDataAction).FullName,
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
                       "Action datas are never compatible (occur once)");
      return false;
    }
    
    /// <summary>
    /// Import data that has been previously merged
    /// </summary>
    /// <param name="datas"></param>
    public void ImportDatas(IList<ExchangeData> datas, CancellationToken cancellationToken = default)
    {
      Debug.Assert (1 == datas.Count);
      var firstData = datas[0];
      ImportAction (firstData.Key, firstData.Value, firstData.DateTime);
    }
    #endregion // IImportData implementation
    
    #region Private methods
    void ImportAction(string actionKey, object v, DateTime dateTime)
    {
      log.DebugFormat ("ImportAction: " +
                       "machineModuleId={0} action={1}",
                       m_machineModule.Id, actionKey);
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        DateTime effectiveDateTime = NullableDateTime.TruncateToSeconds (dateTime);
        using (IDAOTransaction transaction = session.BeginTransaction(
          "CncData.ImportAction", TransactionLevel.ReadCommitted))
        {
          // - MachineModuleDetection
          IMachineModuleDetection detection = ModelDAOHelper.ModelFactory
            .CreateMachineModuleDetection (m_machineModule, effectiveDateTime); // machine module
          if (actionKey.Equals (ExchangeData.ACTION_STOP_ISO_FILE)) {
            detection.StopNcProgram = true;
          }
          else if (actionKey.Equals (ExchangeData.ACTION_START_CYCLE)) {
            detection.StartCycle = true;
            if (null != v) {
              detection.OperationCode = v.ToString ();
            }
          }
          else if (actionKey.Equals (ExchangeData.ACTION_STOP_CYCLE)) {
            detection.StopCycle = true;
            if (null != v) {
              detection.OperationCode = v.ToString ();
            }
          }
          else if (actionKey.Equals (ExchangeData.ACTION_QUANTITY)) {
            detection.StopCycle = true;
            detection.Quantity = (int) v;
          }
          else if (actionKey.Equals (ExchangeData.ACTION_OPERATIONCODE_QUANTITY)) {
            detection.StopCycle = true;
            if (null != v) {
              // Last character or number is the quantity
              if (v is int) {
                detection.OperationCode = (((int) v) / 10).ToString ();
                detection.Quantity = ((int) v) % 10;
              }
              else { // v is string
                string s = v.ToString ();
                Debug.Assert (null != s);
                if (1 <= s.Length) {
                  string q = s.Substring (s.Length - 1, 1);
                  int intq;
                  if (int.TryParse (q, out intq)) {
                    detection.Quantity = intq;
                  }
                  if (2 <= s.Length) {
                    detection.OperationCode = s.Substring (0, s.Length - 1);
                  }
                }
              }
            }
          }
          else {
            log.ErrorFormat ("ImportAction: " +
                             "key {0} is invalid " +
                             "=> skip the record",
                             actionKey);
            return;
          }
          ModelDAOHelper.DAOFactory.MachineModuleDetectionDAO.MakePersistent(detection);

          // - DetectionTimeStamp
          var detectionTimeStamp = ModelDAOHelper.DAOFactory.AcquisitionStateDAO.GetAcquisitionState (m_machineModule, AcquisitionStateKey.Detection);
          if (null == detectionTimeStamp) {
            detectionTimeStamp = ModelDAOHelper.ModelFactory.CreateAcquisitionState (m_machineModule, AcquisitionStateKey.Detection);
          }
          detectionTimeStamp.DateTime = effectiveDateTime;
          ModelDAOHelper.DAOFactory.AcquisitionStateDAO.MakePersistent(detectionTimeStamp);
          
          // - Commit
          transaction.Commit();
        }
      }
    }
    #endregion // Private methods
  }
}
