// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2026 Atsora Solutions
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
using System.Diagnostics;
using Lemoine.Extensions.Alert;

namespace Lemoine.Alert.GDBListeners
{
  /// <summary>
  /// CncValueListener for a specific machine module and a specific field
  /// </summary>
  internal class CncValueMachineModuleListener : IListener
  {
    static readonly string APPLICATION_STATE_KEY_PREFIX = "alert.listener.cncvalue.";
    static readonly string FETCH_MAX_ITEMS_KEY = "alert.listener.cncvalue.fetch.max";
    static readonly int FETCH_MAX_ITEMS_DEFAULT = 100;

    readonly IField m_field;
    readonly IMachineModule m_machineModule;
    IApplicationState m_cncValueMachineModuleListenerState = null;
    IList<ICncValueData> m_fetched = new List<ICncValueData> (); // From cncvalue or cncdataperiod
    XmlSerializer m_xmlSerializer;

    ILog log = LogManager.GetLogger (typeof (CncValueMachineModuleListener).FullName);

    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="field"></param>
    /// <param name="machineModule"></param>
    public CncValueMachineModuleListener (IField field, IMachineModule machineModule)
    {
      Debug.Assert (null != field);
      Debug.Assert (null != machineModule);

      m_field = field;
      m_machineModule = machineModule;

      log = LogManager.GetLogger (string.Format ("{0}.{1}.{2}",
        this.GetType ().FullName, machineModule.MonitoredMachine.Id, machineModule.Id));

      var xmlSerializerBuilder = new Lemoine.Database.Xml.XmlSerializerBuilder ();
      Type type = Type.GetType ("Lemoine.GDBPersistentClasses.CncValue, Pulse.Database");
      if (type is null) {
        log.Fatal ($"CncValueMachineModuleListener: type Lemoine.GDBPersistentClasses.CncValue, Pulse.Database does not exist");
      }
      m_xmlSerializer = xmlSerializerBuilder.GetSerializer (type);
    }

    /// <summary>
    /// Get in the listener the next data.
    /// 
    /// Returns null when there is no data any more to return
    /// </summary>
    /// <returns>new data or null</returns>
    public XmlElement GetData ()
    {
      CheckInitialization ();

      if (!m_fetched.Any ()) { // No data any more process, try to fetch some more
        var dateTime = (DateTime)m_cncValueMachineModuleListenerState.Value;
        var maxNbFetched = Lemoine.Info.ConfigSet.LoadAndGet<int> (FETCH_MAX_ITEMS_KEY, FETCH_MAX_ITEMS_DEFAULT);
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          m_fetched = ModelDAOHelper.DAOFactory.CncValueDataDAO
            .FindNext (m_machineModule, m_field, dateTime, maxNbFetched)
            .ToList ();
        }
      }

      if (!m_fetched.Any ()) { // No data any more to process
        log.Debug ("GetData: no data");
        return null;
      }
      else { // 0 < m_fetched.Count
        var firstData = m_fetched.First ();
        XmlElement data;
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          // - serialization, with the same format whatever the table the value comes from
          var cncValue = CncValueDataSerialization.ToSerializableCncValue (firstData);
          if (null == cncValue) {
            log.Error ($"GetData: {firstData} does not exist any more => skip it");
            m_fetched.RemoveAt (0);
            return GetData ();
          }
          StringWriter sw = new StringWriter ();
          m_xmlSerializer.Serialize (sw, cncValue);
          XmlDocument document = new XmlDocument ();
          document.LoadXml (sw.ToString ());
          data = document.DocumentElement;
          // - Update the application state
          using (IDAOTransaction transaction = session.BeginTransaction ()) {
            ModelDAOHelper.DAOFactory.ApplicationStateDAO.Lock (m_cncValueMachineModuleListenerState);
            Debug.Assert (firstData.DateTimeRange.Upper.HasValue);
            if (firstData.Begin.Equals (firstData.End)) {
              m_cncValueMachineModuleListenerState.Value = firstData.End.AddSeconds (1);
            }
            else {
              m_cncValueMachineModuleListenerState.Value = firstData.End;
            }
            ModelDAOHelper.DAOFactory.ApplicationStateDAO
              .MakePersistent (m_cncValueMachineModuleListenerState);
            transaction.Commit ();
          }
        }
        m_fetched.RemoveAt (0);
        return data;
      }
    }

    /// <summary>
    /// Check m_cncValueMachineModuleListenerState is initialized
    /// </summary>
    void CheckInitialization ()
    {
      if (null == m_cncValueMachineModuleListenerState) {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          string key = APPLICATION_STATE_KEY_PREFIX + m_field.Id + "." + m_machineModule.Id;
          m_cncValueMachineModuleListenerState = ModelDAOHelper.DAOFactory.ApplicationStateDAO
            .GetApplicationState (key);
          if (null == m_cncValueMachineModuleListenerState) {
            using (IDAOTransaction transaction = session.BeginTransaction ()) {
              m_cncValueMachineModuleListenerState = ModelDAOHelper.ModelFactory.CreateApplicationState (key);
              m_cncValueMachineModuleListenerState.Value = DateTime.UtcNow.AddDays(-1);
              ModelDAOHelper.DAOFactory.ApplicationStateDAO.MakePersistent (m_cncValueMachineModuleListenerState);
              transaction.Commit ();
            }
          }
        }
      }
    }
  }
}
