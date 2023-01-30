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
  /// CncValueListener for a specific machine module and a specific reasonId
  /// </summary>
  /// 
  [Serializable]
  public class ReasonSlotMachineModuleListener : IListener
  {
    static readonly string APPLICATION_STATE_KEY_PREFIX = "alert.listener.reasonslot.";   // .module.reason

    #region Members
    int m_reasonId;
    IDictionary<TimeSpan, string> m_thresholds;
    readonly IMachineModule m_machineModule;
    IApplicationState m_reasonSlotMachineModuleListenerState = null;

    #endregion // Members

    ILog log = LogManager.GetLogger (typeof (ReasonSlotMachineModuleListener).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="field"></param>
    /// <param name="machineModule"></param>
    public ReasonSlotMachineModuleListener (int reasonid, IDictionary<TimeSpan, string> thresholds, IMachineModule machineModule)
    {
      Debug.Assert (null != machineModule);

      m_reasonId = reasonid;
      m_thresholds = thresholds;
      m_machineModule = machineModule;

      log = LogManager.GetLogger (string.Format ("{0}.{1}.{2}",
        this.GetType ().FullName, machineModule.MonitoredMachine.Id, machineModule.Id));
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
      if (log.IsDebugEnabled) {
        //log.Debug ($"GetData ----------------------------------------------------------");
      }

      CheckInitialization ();

      XmlElement data = null;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        DateTime dateTime = (DateTime)m_reasonSlotMachineModuleListenerState.Value;
        DateTime dateTimeNow = DateTime.UtcNow;


        if (DateTime.Compare(dateTime, dateTimeNow) >= 0) {
          // empty range
          if (log.IsDebugEnabled) {
            log.Debug ($"GetData: empty range {dateTime}, {dateTimeNow}");
          }
          return null;
        }
        var fetchRange = new UtcDateTimeRange (dateTime, dateTimeNow);
        if (log.IsDebugEnabled) {
          log.Debug ($"GetData: fetchRange= {fetchRange}");
        }

        XmlDocument document = new XmlDocument ();
        XmlNode arrayOfReasons = document.CreateElement ("ArrayOfReasons");
        document.AppendChild (arrayOfReasons);
        XmlAttribute arrayDateTime = document.CreateAttribute ("DateTime");
        arrayDateTime.Value = dateTimeNow.ToString();
        arrayOfReasons.Attributes.Append (arrayDateTime);
        XmlAttribute arrayMachineModule = document.CreateAttribute ("MachineName");
        arrayMachineModule.Value = m_machineModule.MonitoredMachine.Name;
        arrayOfReasons.Attributes.Append (arrayMachineModule);

        bool reasonExists = false;
        
        IList<IReasonSlot> reasonSlots = null;
        reasonSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO.FindAllInUtcRangeWithMachineModeReason (
              m_machineModule.MonitoredMachine, fetchRange).Where (r => r.Reason.Id == m_reasonId).ToList ();

        foreach (var reasonSlot in reasonSlots) {
          for (int i= m_thresholds.Count-1; i >= 0; i--) {
            if (TimeSpan.Compare ((TimeSpan)(reasonSlot.Duration), m_thresholds.ElementAt(i).Key) >=0) {
              if (log.IsDebugEnabled) {
                log.Debug ($"GetData: machineModule={m_machineModule.Name} add reasonslot {reasonSlot} {m_thresholds.ElementAt (i).Value} {reasonSlot.Duration}");
              }
              reasonExists = true;
              XmlNode reason = document.CreateElement ("Reason");
              XmlAttribute beginDateTime = document.CreateAttribute ("DateTime");
              beginDateTime.Value = reasonSlot.BeginDateTime.Value.ToLocalTime().ToString();
              reason.Attributes.Append (beginDateTime);
              XmlAttribute severity = document.CreateAttribute ("Severity");
              severity.Value = m_thresholds.ElementAt (i).Value;
              reason.Attributes.Append (severity);
              XmlAttribute duration = document.CreateAttribute ("Duration");
              duration.Value = reasonSlot.Duration.ToString ();
              reason.Attributes.Append (duration);
              arrayOfReasons.AppendChild (reason);
              break;
            }
          }
        }
        
        if (reasonExists) {
          data = document.DocumentElement;
          // - Update the application state
          using (IDAOTransaction transaction = session.BeginTransaction ()) {
            ModelDAOHelper.DAOFactory.ApplicationStateDAO.Lock (m_reasonSlotMachineModuleListenerState);
            m_reasonSlotMachineModuleListenerState.Value = dateTimeNow.AddMinutes (1);
            ModelDAOHelper.DAOFactory.ApplicationStateDAO
              .MakePersistent (m_reasonSlotMachineModuleListenerState);
            transaction.Commit ();
          }
          if (log.IsDebugEnabled) {
            log.Debug ($"GetData: data={document.InnerXml}");
          }
        }
        return data;
      }
    }

    /// <summary>
    /// Check m_cncValueMachineModuleListenerState is initialized
    /// </summary>
    void CheckInitialization ()
    {
      if (null == m_reasonSlotMachineModuleListenerState) {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          string key = APPLICATION_STATE_KEY_PREFIX + m_machineModule.MonitoredMachine + "." + m_reasonId;
          m_reasonSlotMachineModuleListenerState = ModelDAOHelper.DAOFactory.ApplicationStateDAO
            .GetApplicationState (key);
          if (null == m_reasonSlotMachineModuleListenerState) {
            using (IDAOTransaction transaction = session.BeginTransaction ()) {
              m_reasonSlotMachineModuleListenerState = ModelDAOHelper.ModelFactory.CreateApplicationState (key);
              m_reasonSlotMachineModuleListenerState.Value = DateTime.UtcNow.AddDays(-1);
              ModelDAOHelper.DAOFactory.ApplicationStateDAO.MakePersistent (m_reasonSlotMachineModuleListenerState);
              transaction.Commit ();
            }
          }
        }
      }
    }

    #endregion // Methods
  }
}
