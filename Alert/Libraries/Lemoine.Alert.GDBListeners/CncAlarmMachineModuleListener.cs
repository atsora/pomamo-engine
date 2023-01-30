// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Extensions.Alert;

namespace Lemoine.Alert.GDBListeners
{
  /// <summary>
  /// CncAlarmListener for a specific machine module and a specific field
  /// </summary>
  internal class CncAlarmMachineModuleListener : IListener
  {
    static readonly string APPLICATION_STATE_KEY_PREFIX = "alert.listener.alarm.";
    static readonly string FETCH_MAX_ITEMS_KEY = "alert.listener.alarm.fetch.max";
    static readonly int FETCH_MAX_ITEMS_DEFAULT = 100;

    #region Members
    readonly IMachineModule m_machineModule;
    IApplicationState m_cncAlarmMachineModuleListenerState = null;
    IList<ICncAlarm> m_fetched = new List<ICncAlarm>();
    XmlSerializer m_xmlSerializer;
    #endregion // Members

    ILog log = LogManager.GetLogger(typeof(CncAlarmMachineModuleListener).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="machineModule"></param>
    public CncAlarmMachineModuleListener(IMachineModule machineModule)
    {
      m_machineModule = machineModule;

      log = LogManager.GetLogger (string.Format ("{0}.{1}.{2}",
        this.GetType ().FullName, machineModule.MonitoredMachine.Id, machineModule.Id));

      // Create a serializer for the class CncAlarm
      var xmlSerializerBuilder = new Lemoine.Database.Xml.XmlSerializerBuilder();
      Type type = Type.GetType("Lemoine.GDBPersistentClasses.CncAlarm, Pulse.Database");
      if (type is null) {
        log.Fatal ("CncAlarmMachineModuleListener: type Lemoine.GDBPersistentClasses.CncAlarm, Pulse.Database does not exist");
      }
      m_xmlSerializer = xmlSerializerBuilder.GetSerializer(type);
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Get in the listener the next data.
    /// Returns null when there is no data anymore to return
    /// </summary>
    /// <returns>new data or null</returns>
    public XmlElement GetData()
    {
      CheckInitialization();

      // If no data to process, try to fetch some more
      if (m_fetched.Count == 0) {
        var minId = (int)m_cncAlarmMachineModuleListenerState.Value;
        var maxNbFetched = Lemoine.Info.ConfigSet.LoadAndGet<int>(FETCH_MAX_ITEMS_KEY, FETCH_MAX_ITEMS_DEFAULT);
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
          m_fetched = ModelDAOHelper.DAOFactory.CncAlarmDAO.FindAfterID(m_machineModule, minId, maxNbFetched);
        }
      }

      // If no data anymore to process
      if (m_fetched.Count == 0) {
        log.DebugFormat("GetData: no data");
        return null;
      }
      
      // Get the first data in the list
      var firstData = m_fetched[0];
      XmlElement data;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        // Note: Lock does not work on the line below.
        //       This is not fully clear why yet, but it generates a message when the Unproxy() method is called.
        //       Because the event data is read only, FindById may be used instead of Lock.
        // TODO: understand what exactly happens with Lock and Unproxy
        firstData = ModelDAOHelper.DAOFactory.CncAlarmDAO.FindById(firstData.Id, firstData.MachineModule);
        firstData.Unproxy();
        
        // - serialization
        var sw = new StringWriter();
        m_xmlSerializer.Serialize(sw, firstData);
        var document = new XmlDocument();
        document.LoadXml(sw.ToString());
        data = document.DocumentElement;
        
        // - Update the application state
        using (IDAOTransaction transaction = session.BeginTransaction()) {
          ModelDAOHelper.DAOFactory.ApplicationStateDAO.Lock(m_cncAlarmMachineModuleListenerState);
          m_cncAlarmMachineModuleListenerState.Value = firstData.Id;
          ModelDAOHelper.DAOFactory.ApplicationStateDAO.MakePersistent(m_cncAlarmMachineModuleListenerState);
          transaction.Commit();
        }
      }
      
      m_fetched.RemoveAt(0);
      return data;
    }

    /// <summary>
    /// Check m_cncAlarmMachineModuleListenerState is initialized
    /// </summary>
    void CheckInitialization()
    {
      if (m_cncAlarmMachineModuleListenerState == null) {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
          string key = APPLICATION_STATE_KEY_PREFIX + m_machineModule.Id;
          m_cncAlarmMachineModuleListenerState = ModelDAOHelper.DAOFactory.ApplicationStateDAO.GetApplicationState(key);
          if (m_cncAlarmMachineModuleListenerState == null) {
            using (IDAOTransaction transaction = session.BeginTransaction()) {
              m_cncAlarmMachineModuleListenerState = ModelDAOHelper.ModelFactory.CreateApplicationState(key);
              m_cncAlarmMachineModuleListenerState.Value = 0;
              ModelDAOHelper.DAOFactory.ApplicationStateDAO.MakePersistent(m_cncAlarmMachineModuleListenerState);
              transaction.Commit();
            }
          }
        }
      }
    }
    #endregion // Methods
  }
}
