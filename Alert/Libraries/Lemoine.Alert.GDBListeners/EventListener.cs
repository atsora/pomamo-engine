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
  /// Description of EventListener.
  /// </summary>
  [Serializable]
  public class EventListener: IListener
  {
    static readonly string APPLICATION_STATE_KEY = "alert.listener.event";

    static readonly string FETCH_MAX_ITEMS_KEY = "alert.listener.event.fetch.max";
    static readonly int FETCH_MAX_ITEMS_DEFAULT = 1000;
    
    IApplicationState m_eventListenerState = null;
    IList<IEvent> m_fetched = new List<IEvent> ();
    readonly XmlSerializer m_xmlSerializer;

    static readonly ILog log = LogManager.GetLogger(typeof (EventListener).FullName);

    /// <summary>
    /// Description of the constructor
    /// </summary>
    public EventListener ()
    {
      var xmlSerializerBuilder = new Lemoine.Database.Xml.XmlSerializerBuilder ();
      Type type = Type.GetType ("Lemoine.GDBPersistentClasses.Event, Pulse.Database");
      if (type is null) {
        log.Fatal ($"EventListener: type Lemoine.GDBPersistentClasses.Event, Pulse.Database doest not exist");
      }
      m_xmlSerializer = xmlSerializerBuilder.GetSerializer (type);
    }

    #region Methods
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
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        {
          int fetchMaxItems = Lemoine.Info.ConfigSet.LoadAndGet<int> (FETCH_MAX_ITEMS_KEY,
            FETCH_MAX_ITEMS_DEFAULT);
          m_fetched = ModelDAOHelper.DAOFactory.EventDAO
            .FindGreaterThan ((int)m_eventListenerState.Value, fetchMaxItems)
            .ToList<IEvent> ();
        }
      }
      
      if (!m_fetched.Any ()) { // No data any more to process
        log.Debug ("GetData: no data");
        return null;
      }
      else { // 0 < m_fetched.Count
        IEvent firstData = m_fetched.First ();
        try {
          XmlElement data;
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
            // - serialization
            StringWriter sw = new StringWriter ();
            m_xmlSerializer.Serialize (sw, firstData);
            XmlDocument document = new XmlDocument ();
            document.LoadXml (sw.ToString ());
            data = document.DocumentElement;
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
          return data;
        }
        catch (Exception ex) {
          log.Error ($"GetData: error in data {firstData}", ex);
          if (ex is InvalidOperationException && ex.ToString ().Contains ("Use the XmlInclude")) {
            // In case of XML deserialization exception because a plugin was not activated
            // Skip the record
            log.Error ($"GetData: skip the record because of a missing XmlInclude, probably because a plugin was not activated. Data {firstData}");
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
    }
    
    /// <summary>
    /// Check m_eventListenerState is initialized
    /// </summary>
    void CheckInitialization ()
    {
      if (null == m_eventListenerState) {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        {
          m_eventListenerState = ModelDAOHelper.DAOFactory.ApplicationStateDAO
            .GetApplicationState (APPLICATION_STATE_KEY);
          if (null == m_eventListenerState) {
            using (IDAOTransaction transaction = session.BeginTransaction ())
            {
              m_eventListenerState = ModelDAOHelper.ModelFactory.CreateApplicationState (APPLICATION_STATE_KEY);
              m_eventListenerState.Value = 0;
              ModelDAOHelper.DAOFactory.ApplicationStateDAO.MakePersistent (m_eventListenerState);
              transaction.Commit ();
            }
          }
        }
      }
    }
    
    
    #endregion // Methods
  }
}
