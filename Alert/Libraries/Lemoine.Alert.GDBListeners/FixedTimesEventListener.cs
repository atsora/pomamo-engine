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
using Lemoine.Extensions.Alert;

namespace Lemoine.Alert.GDBListeners
{
  /// <summary>
  /// Description of FixedTimesEventListener.
  /// </summary>
  [Serializable]
  public class FixedTimesEventListener : IListener
  {
    static readonly string APPLICATION_STATE_KEY = "alert.listener.fixedtimesevent";

    static readonly string FETCH_MAX_ITEMS_KEY = "alert.listener.fixedtimesevent.fetch.max";
    static readonly int FETCH_MAX_ITEMS_DEFAULT = 1000;


    #region Members
    IApplicationState m_eventListenerState = null;
    IList<IEvent> m_fetched = new List<IEvent> ();
    IList<Event> m_listEvents = new List<Event> ();
    string m_triggerTimes = null;

    //IList<Lemoine.GDBPersistentClasses.Event> m_fetched = new List<Lemoine.GDBPersistentClasses.Event> ();
    IList<TimeSpan> m_fixedTimes = new List<TimeSpan> ();
    readonly XmlSerializer m_xmlSerializer;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (FixedTimesEventListener).FullName);

    #region Getters / Setters
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
    public FixedTimesEventListener ()
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"FixedTimesEventListener: ctor");
      }
      var xmlSerializerBuilder = new Lemoine.Database.Xml.XmlSerializerBuilder ();

      //Type type = m_listString.GetType ();
      Type type = m_listEvents.GetType ();
      //Type type = Type.GetType ("Lemoine.GDBPersistentClasses.Event, Pulse.Database");
      //Type type = Type.GetType ("System.Collections.Generic.List`1[Lemoine.Model.IEvent]");
      m_xmlSerializer = xmlSerializerBuilder.GetSerializer (type);
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
        log.Debug ($"GetData");
      }

      if (!CheckTriggerTime ()) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetData: CheckTriggerTime false");
        }
        return null;
      }

      CheckInitialization ();
      if (log.IsDebugEnabled) {
        log.Debug ($"GetData: CheckInitialization done");
      }

      if (!m_fetched.Any ()) { // No data any more process, try to fetch some more
        if (log.IsDebugEnabled) {
          log.Debug ($"GetData: no more data to fetch");
        }
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          int fetchMaxItems = Lemoine.Info.ConfigSet.LoadAndGet<int> (FETCH_MAX_ITEMS_KEY,
            FETCH_MAX_ITEMS_DEFAULT);
          if (log.IsDebugEnabled) {
            log.Debug ($"GetData: fetchMaxItems={fetchMaxItems}");
          }
          m_fetched = ModelDAOHelper.DAOFactory.EventDAO
            .FindGreaterThan ((int)m_eventListenerState.Value, fetchMaxItems)
            .ToList<IEvent> ();
        }
      }
      else {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetData: data to fetch");
        }
      }

      if (!m_fetched.Any ()) { // No data any more to process
        if (log.IsDebugEnabled) {
          log.Debug ("GetData: no data");
        }
        return null;
      }
      else { // 0 < m_fetched.Count
        if (log.IsDebugEnabled) {
          log.Debug ($"GetData: data to fetch count={m_fetched.Count}");
        }
        IList<IEvent> eventItems = new List<IEvent> ();
        XmlElement data;

        while (m_fetched.Any ()) {
          IEvent firstData = m_fetched.First ();
          if (log.IsDebugEnabled) {
            log.Debug ($"GetData: firstData Id={firstData.Id}");
          }
          try {
            using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
              // Note: Lock does not work on the line below.
              //       This is not fully clear why yet, but it generates a message when the Unproxy() method is called.
              //       Because the event data is read only, FindById may be used instead of Lock.
              // TODO: understand what exactly happens with Lock and Unproxy
              firstData = ModelDAOHelper.DAOFactory.EventDAO
                .FindById (firstData.Id);
              if (null == firstData) {
                log.Fatal ($"GetData: event with id {firstData.Id} does not exist, this is unexpected");
              }
              firstData.Unproxy ();
              m_listEvents.Add ((Event)firstData);

              // - Update the application state
              if (log.IsDebugEnabled) {
                log.Debug ($"GetData: update application state");
              }
              using (IDAOTransaction transaction = session.BeginTransaction ()) {
                ModelDAOHelper.DAOFactory.ApplicationStateDAO.Lock (m_eventListenerState);
                m_eventListenerState.Value = firstData.Id;
                ModelDAOHelper.DAOFactory.ApplicationStateDAO
                  .MakePersistent (m_eventListenerState);
                transaction.Commit ();
              }
            }
            m_fetched.RemoveAt (0);
          }
          catch (Exception ex) {
            log.Error ($"GetData: error in data {firstData}", ex);
            if (ex is InvalidOperationException && ex.ToString ().Contains ("Use the XmlInclude")) {
              // In case of XML deserialization exception because a plugin was not activated
              // Skip the record
              log.ErrorFormat ("GetData: skip the record because of a missing XmlInclude, probably because a plugin was not activated. Data {0}", firstData);
              using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
                // - Update the application state
                using (IDAOTransaction transaction = session.BeginTransaction ()) {
                  ModelDAOHelper.DAOFactory.ApplicationStateDAO.Lock (m_eventListenerState);
                  m_eventListenerState.Value = firstData.Id;
                  ModelDAOHelper.DAOFactory.ApplicationStateDAO
                    .MakePersistent (m_eventListenerState);
                  transaction.Commit ();
                }
              }
              m_fetched.RemoveAt (0);
              return GetData (); // Try the next one
            }
            else {
              throw;
            }
          }
        }
        // - serialization
        if (log.IsDebugEnabled) {
          log.Debug ($"GetData: serialization");
        }
        StringWriter sw = new StringWriter ();
        m_xmlSerializer.Serialize (sw, m_listEvents);
        if (log.IsDebugEnabled) {
          log.Debug ($"GetData: sw.ToString={sw.ToString ()}");
        }

        XmlDocument document = new XmlDocument ();
        document.LoadXml (sw.ToString ());
        data = document.DocumentElement;

        return data;
      }
    }

    /// <summary>
    /// Check m_eventListenerState is initialized
    /// </summary>
    void CheckInitialization ()
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"CheckInitialization");
      }
      if (null == m_eventListenerState) {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          try {
            m_eventListenerState = ModelDAOHelper.DAOFactory.ApplicationStateDAO
              .GetApplicationState (APPLICATION_STATE_KEY);
            if (null == m_eventListenerState) {
              using (IDAOTransaction transaction = session.BeginTransaction ()) {
                m_eventListenerState = ModelDAOHelper.ModelFactory.CreateApplicationState (APPLICATION_STATE_KEY);
                m_eventListenerState.Value = 0;
                ModelDAOHelper.DAOFactory.ApplicationStateDAO.MakePersistent (m_eventListenerState);
                transaction.Commit ();
              }
            }
          }
          catch (Exception ex) {
            if (log.IsDebugEnabled) {
              log.Debug ($"CheckInitialization: exception {ex}");
            }
          }
        }
      }
      else {
        if (log.IsDebugEnabled) {
          log.Debug ($"CheckInitialization: m_eventListenerState not null");
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
