// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Lemoine.Collections;
using Lemoine.GDBPersistentClasses;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System.Diagnostics;
using Lemoine.Extensions.Alert;

namespace Lemoine.Alert.GDBListeners
{
  /// <summary>
  /// CncAlarmListener for a specific machine module and a specific field
  /// </summary>
  internal class CncAlarmStackLightMachineModuleListener : IListener
  {
    static readonly string APPLICATION_STATE_KEY_PREFIX = "alert.listener.alarmstacklight.";
    static readonly string FETCH_MAX_ITEMS_KEY = "alert.listener.alarmstacklight.fetch.max";
    static readonly int FETCH_MAX_ITEMS_DEFAULT = 100;
    static readonly int MARGIN_SECONDS = 3;

    #region Members
    readonly IField m_field;
    readonly IMachineModule m_machineModule;
    IApplicationState m_cncAlarmMachineModuleListenerState = null;
    // Stack light values from cncvalue or cncdataperiod and their alarms, converted into CncValue to be serialized
    List<(ICncValueData CncValue, List<CncAlarm> Alarms)> m_fetched = new List<(ICncValueData CncValue, List<CncAlarm> Alarms)> ();
    XmlSerializer m_xmlSerializer;
    #endregion // Members

    ILog log = LogManager.GetLogger (typeof (CncAlarmStackLightMachineModuleListener).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="field"></param>
    /// <param name="machineModule"></param>
    public CncAlarmStackLightMachineModuleListener (IField field, IMachineModule machineModule)
    {
      Debug.Assert (null != field);
      Debug.Assert (null != machineModule);

      m_field = field;
      m_machineModule = machineModule;

      log = LogManager.GetLogger (string.Format ("{0}.{1}.{2}",
        this.GetType ().FullName, machineModule.MonitoredMachine.Id, machineModule.Id));

      // Create a serializer for the class CncAlarm
      var xmlSerializerBuilder = new Lemoine.Database.Xml.XmlSerializerBuilder ();
      Type type = Type.GetType ("Lemoine.GDBPersistentClasses.CncAlarm, Pulse.Database");
      if (type is null) {
        log.Fatal ($"CncAlarmStackLightMachineModuleListener: type Lemoine.GDBPersistentClasses.CncAlarm, Pulse.Database does not exist");
      }
      m_xmlSerializer = xmlSerializerBuilder.GetSerializer (typeof (SerializableTuple<CncValue, List<CncAlarm>>));
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Get in the listener the next data.
    /// Returns null when there is no data anymore to return
    /// </summary>
    /// <returns>new data or null</returns>
    public XmlElement GetData ()
    {
      CheckInitialization ();

      // If no data to process, try to fetch some more
      DateTime? lastTime = null;
      if (m_fetched.Count == 0) {
        var dateTime = (DateTime)m_cncAlarmMachineModuleListenerState.Value;
        var maxNbFetched = Lemoine.Info.ConfigSet.LoadAndGet<int> (FETCH_MAX_ITEMS_KEY, FETCH_MAX_ITEMS_DEFAULT);
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          var cncValues = ModelDAOHelper.DAOFactory.CncValueDataDAO
            .FindNext (m_machineModule, m_field, dateTime, FETCH_MAX_ITEMS_DEFAULT);

          // Keep a track on the periods processed
          DateTime? oldEnd = null;

          foreach (var cncValue in cncValues) {
            if (cncValue != null && cncValue.Value != null) {
              // Green, yellow or red?
              var light = (StackLight)cncValue.Value;
              bool isGreen = light.IsOnOrFlashingIfAcquired (StackLightColor.Green);
              bool isYellow = light.IsOnOrFlashingIfAcquired (StackLightColor.Yellow);
              bool isRed = light.IsOnOrFlashingIfAcquired (StackLightColor.Red);
              lastTime = cncValue.Begin;
              if (isYellow || isRed) {
                // Try to find alarms starting around the beginning of this stack light value
                // (and not of the first value of the batch)
                var startPeriod = cncValue.Begin.AddSeconds (-MARGIN_SECONDS);
                if (oldEnd.HasValue && startPeriod < oldEnd.Value) {
                  startPeriod = oldEnd.Value;
                }

                var endPeriod = cncValue.Begin.AddSeconds (MARGIN_SECONDS);
                oldEnd = endPeriod;

                // Start may be postponed so we check that it is still before the end
                if (startPeriod < endPeriod) {
                  var iAlarms = ModelDAOHelper.DAOFactory.CncAlarmDAO.FindWithBeginningInRange (
                    m_machineModule,
                    new UtcDateTimeRange (new LowerBound<DateTime> (startPeriod), new UpperBound<DateTime> (endPeriod)));
                  if (iAlarms != null && iAlarms.Count > 0) {
                    // We store the alarms
                    var alarms = new List<CncAlarm> ();
                    foreach (var alarm in iAlarms) {
                      alarms.Add (alarm as CncAlarm);
                    }

                    m_fetched.Add ((cncValue, alarms));
                  }
                }
              }
            }
          }
        }
      }

      // If no data anymore to process
      if (m_fetched.Count == 0) {
        log.DebugFormat ("GetData: no data");
        if (lastTime.HasValue) {
          // Update the datetime so that we can go on
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
            ICncAlarm nextAlarm = ModelDAOHelper.DAOFactory.CncAlarmDAO.FindFirstAfter (m_machineModule, lastTime.Value);
            if (nextAlarm != null) {
              using (IDAOTransaction transaction = session.BeginTransaction ()) {
                ModelDAOHelper.DAOFactory.ApplicationStateDAO.Lock (m_cncAlarmMachineModuleListenerState);
                m_cncAlarmMachineModuleListenerState.Value = nextAlarm.DateTimeRange.Lower.Value;
                ModelDAOHelper.DAOFactory.ApplicationStateDAO.MakePersistent (m_cncAlarmMachineModuleListenerState);
                transaction.Commit ();
              }
            }
          }
        }

        return null;
      }

      // Get the first data in the list
      var (fetchedCncValue, fetchedAlarms) = m_fetched[0];
      XmlElement data;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        // Same format whatever the table the stack light value comes from
        var cncValue = CncValueDataSerialization.ToSerializableCncValue (fetchedCncValue);
        if (null == cncValue) {
          log.Error ($"GetData: {fetchedCncValue} does not exist any more => skip it");
          m_fetched.RemoveAt (0);
          return GetData ();
        }
        // Note: Lock does not work on the line below.
        //       This is not fully clear why yet, but it generates a message when the Unproxy() method is called.
        //       Because the event data is read only, FindById may be used instead of Lock.
        // TODO: understand what exactly happens with Lock and Unproxy
        var alarms = new List<CncAlarm> ();
        foreach (var fetchedAlarm in fetchedAlarms) {
          var alarm = ModelDAOHelper.DAOFactory.CncAlarmDAO.FindById (fetchedAlarm.Id, m_machineModule) as CncAlarm;
          alarm.Unproxy ();
          alarms.Add (alarm);
        }
        var firstData = new SerializableTuple<CncValue, List<CncAlarm>> (cncValue, alarms);

        // - serialization
        var sw = new StringWriter ();
        m_xmlSerializer.Serialize (sw, firstData);
        var document = new XmlDocument ();
        document.LoadXml (sw.ToString ());
        data = document.DocumentElement;

        // - Update the application state
        using (IDAOTransaction transaction = session.BeginTransaction ()) {
          ModelDAOHelper.DAOFactory.ApplicationStateDAO.Lock (m_cncAlarmMachineModuleListenerState);
          m_cncAlarmMachineModuleListenerState.Value = (firstData.Item1.Begin == firstData.Item1.End) ?
            firstData.Item1.End.AddSeconds (1) : firstData.Item1.End;
          ModelDAOHelper.DAOFactory.ApplicationStateDAO.MakePersistent (m_cncAlarmMachineModuleListenerState);
          transaction.Commit ();
        }
      }

      m_fetched.RemoveAt (0);
      return data;
    }

    /// <summary>
    /// Check m_cncAlarmMachineModuleListenerState is initialized
    /// </summary>
    void CheckInitialization ()
    {
      if (m_cncAlarmMachineModuleListenerState == null) {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          string key = APPLICATION_STATE_KEY_PREFIX + m_machineModule.Id;
          m_cncAlarmMachineModuleListenerState = ModelDAOHelper.DAOFactory.ApplicationStateDAO.GetApplicationState (key);
          if (m_cncAlarmMachineModuleListenerState == null) {
            using (IDAOTransaction transaction = session.BeginTransaction ()) {
              m_cncAlarmMachineModuleListenerState = ModelDAOHelper.ModelFactory.CreateApplicationState (key);
              m_cncAlarmMachineModuleListenerState.Value = DateTime.UtcNow.AddDays (-1);
              ModelDAOHelper.DAOFactory.ApplicationStateDAO.MakePersistent (m_cncAlarmMachineModuleListenerState);
              transaction.Commit ();
            }
          }
          else if (m_cncAlarmMachineModuleListenerState.Value == null ||
                   !(m_cncAlarmMachineModuleListenerState.Value is DateTime)) {
            using (IDAOTransaction transaction = session.BeginTransaction ()) {
              m_cncAlarmMachineModuleListenerState.Value = DateTime.UtcNow.AddDays (-1);
              ModelDAOHelper.DAOFactory.ApplicationStateDAO.MakePersistent (m_cncAlarmMachineModuleListenerState);
              transaction.Commit ();
            }
          }
        }
      }
    }
    #endregion // Methods
  }
}
