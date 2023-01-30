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
using Lemoine.GDBPersistentClasses;
using Lemoine.Collections;
using System.Diagnostics;
using Lemoine.Extensions.Alert;

namespace Lemoine.Alert.GDBListeners
{
  /// <summary>
  /// CncValueListener for a specific reasonId
  /// </summary>
  /// 
  [Serializable]
  public class ReasonSlotListener : IListener
  {
    #region Members
    int m_reasonId;
    string m_triggerTimes = null;
    IDictionary<TimeSpan, string> m_thresholds = new SortedDictionary<TimeSpan, string> ();
    readonly IList<ReasonSlotMachineModuleListener> m_machineModuleListeners = new List<ReasonSlotMachineModuleListener> ();
    int m_machineModuleListenerIndex = 0;
    #endregion // Members

    ILog log = LogManager.GetLogger (typeof (ReasonSlotListener).FullName);

    #region Getters / Setters
    /// <summary>
    /// reason Id
    /// </summary>
    [XmlAttribute ("ReasonId")]
    public int ReasonId
    {
      get { return m_reasonId; }
      set {
        if (log.IsDebugEnabled) {
          log.Debug ($"ReasonId.set: set ReasonId={value}");
        }
        m_reasonId = value;

        IEnumerable<IMachineModule> machineModules;
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          machineModules = ModelDAOHelper.DAOFactory.MachineModuleDAO
            .FindAllWithMonitoredMachine ();
        }

        m_machineModuleListeners.Clear ();
        foreach (var machineModule in machineModules) {
          m_machineModuleListeners.Add (new ReasonSlotMachineModuleListener (m_reasonId, m_thresholds,  machineModule));
        }
        m_machineModuleListenerIndex = 0;

      }
    }

    /// <summary>
    /// List of threshold/message after which a message is raised
    /// 
    /// The syntax is:
    /// 0:00:30=WARNING;0:01:00=ERROR;0:10:00=CRITICAL
    /// 
    /// It must be sorted by ascending threshold time.
    /// </summary>
    [XmlAttribute ("Thresholds")]
    public string Tresholds
    {
      get {
        string thresholdsString = "";
        foreach (KeyValuePair<TimeSpan, string> threshold in m_thresholds) {
          thresholdsString += threshold.Key.ToString () + "=" + threshold.Value;
        }
        return thresholdsString;
      }
      set {
        log.Debug ($"Thresholds.set: {value}");
        string[] thresholds = value.Split (new char[] { ';' });
        foreach (string threshold in thresholds) {
          string[] thresholdMessage = threshold.Split (new char[] { '=' }, 2);
          if (2 <= thresholdMessage.Length) {
            TimeSpan timeSpan;
            if (TimeSpan.TryParse (thresholdMessage[1], out timeSpan)) {
              m_thresholds.Add (timeSpan, thresholdMessage[0]);
            }
            else {
              log.Error ($"Thresholds.set: invalid TimeSpan {thresholdMessage[1]} in {threshold}");
            }
          }
          else {
            log.Error ($"Thresholds.set: invalid threshold=message key {threshold}");
          }
        }
      }
    }

    /// <summary>
    /// Trigger times to get events.
    /// Only at times in the list, event will be processed until this time.
    /// format : ;00:03;00:08;14:00;18:00
    /// If list is empty, events are processed immediately
    /// By default: 20 %
    /// </summary>
    [XmlAttribute ("TriggerTimes")]
    public string TriggerTimes
    {
      get { return m_triggerTimes; }
      set {
        if (log.IsDebugEnabled) {
          log.Debug ($"TriggerTimes.set: set TriggerTimes={value}");
        }
        m_triggerTimes = value;

      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="reasonId"></param>
    /// <param name="machineModule"></param>
    public ReasonSlotListener ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Get in the listener the next data.
    /// 
    /// Returns null when there is no data any more to return
    /// </summary>
    /// <returns>new data or null</returns>
    public XmlElement GetData ()
    {
      if (!m_machineModuleListeners.Any ()) {
        return null;
      }

      if (!CheckTriggerTime ()) {
        if (log.IsDebugEnabled) {
        }
        return null;
      }

      Debug.Assert (m_machineModuleListenerIndex < m_machineModuleListeners.Count);
      ReasonSlotMachineModuleListener machineModuleListener = m_machineModuleListeners[m_machineModuleListenerIndex];
      Debug.Assert (null != machineModuleListener);

      var xml = machineModuleListener.GetData ();
      if (null != xml) {
        return xml;
      }
      else { // Go to the next machine module listener
        ++m_machineModuleListenerIndex;
        if (m_machineModuleListenerIndex == m_machineModuleListeners.Count) { // Begin again from start later
          m_machineModuleListenerIndex = 0;
          return null;
        }
        else {
          return GetData ();
        }
      }
    }

    /// <summary>
    /// Check if current time is in the configured trigger times list
    /// Returns true if match or if list is empty
    /// </summary>
    /// <returns>true or false</returns>
    bool CheckTriggerTime ()
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"CheckTriggerTime:");
      }
      if (string.IsNullOrEmpty (m_triggerTimes)) {
        return true;
      }

      string currentTime = DateTime.UtcNow.ToLocalTime ().ToString ("HH:mm");
      if (!m_triggerTimes.Contains (currentTime)) {
        return false;
      }
      return true;
    }
    #endregion // Methods
  }
}
