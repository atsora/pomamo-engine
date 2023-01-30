// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Extensions.Alert;

namespace Lemoine.Alert.GDBListeners
{
  /// <summary>
  /// Description of LogListener.
  /// </summary>
  [Serializable]
  public class LogListener: IListener
  {
    static readonly string APPLICATION_STATE_KEY = "alert.listener.log";
    static readonly string FETCH_MAX_ITEMS_KEY = "log.fetch.max";
    static readonly int FETCH_MAX_ITEMS_DEFAULT = 1000;
    
    #region Members
    IApplicationState m_logListenerState = null;
    IList<IBaseLog> m_fetched = new List<IBaseLog> ();
    int m_fetchMaxItems = FETCH_MAX_ITEMS_DEFAULT;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (LogListener).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public LogListener ()
    {
      string fetchMaxItemsOption = Lemoine.Info.OptionsFile.GetOption (FETCH_MAX_ITEMS_KEY);
      if (!string.IsNullOrEmpty (fetchMaxItemsOption)) {
        if (int.TryParse (fetchMaxItemsOption, out m_fetchMaxItems)) {
          log.DebugFormat ("LogListener: " +
                           "option {0}={1}",
                           FETCH_MAX_ITEMS_KEY, m_fetchMaxItems);
        }
        else {
          log.ErrorFormat ("LogListener: " +
                           "invalid option {0}={1}",
                           FETCH_MAX_ITEMS_KEY, fetchMaxItemsOption);
        }
      }
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
      CheckInitialization ();
      
      if (0 == m_fetched.Count) { // No data any more process, try to fetch some more
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        {
          m_fetched = ModelDAOHelper.DAOFactory.LogDAO
            .FindGreaterThan ((int)m_logListenerState.Value, m_fetchMaxItems);
        }
      }
      
      if (0 == m_fetched.Count) { // No data any more to process
        log.DebugFormat ("GetData: " +
                         "no data");
        return null;
      }
      else { // 0 < m_fetched.Count
        IBaseLog firstData = m_fetched [0];
        XmlElement data;
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        {
          ModelDAOHelper.DAOFactory.LogDAO.Lock (firstData);
          firstData.Unproxy ();
          // - serialization
          Type type = Type.GetType ("Lemoine.GDBPersistentClasses.Log, Pulse.Database");
          if (type is null) {
            log.Fatal ($"GetData: type Lemoine.GDBPersistentClasses.Log, Pulse.Database does not exist");
          }
          XmlSerializer serializer = new XmlSerializer (type);
          StringWriter sw = new StringWriter ();
          serializer.Serialize (sw, firstData);
          XmlDocument document = new XmlDocument ();
          document.LoadXml (sw.ToString ());
          data = document.DocumentElement;
          // - Update the application state
          using (IDAOTransaction transaction = session.BeginTransaction ())
          {
            ModelDAOHelper.DAOFactory.ApplicationStateDAO.Lock (m_logListenerState);
            m_logListenerState.Value = firstData.Id;
            ModelDAOHelper.DAOFactory.ApplicationStateDAO
              .MakePersistent (m_logListenerState);
            transaction.Commit ();
          }
        }
        m_fetched.RemoveAt (0);
        return data;
      }
    }
    
    /// <summary>
    /// Check m_LogListenerState is initialized
    /// </summary>
    void CheckInitialization ()
    {
      if (null == m_logListenerState) {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        {
          m_logListenerState = ModelDAOHelper.DAOFactory.ApplicationStateDAO
            .GetApplicationState (APPLICATION_STATE_KEY);
          if (null == m_logListenerState) {
            using (IDAOTransaction transaction = session.BeginTransaction ())
            {
              m_logListenerState = ModelDAOHelper.ModelFactory.CreateApplicationState (APPLICATION_STATE_KEY);
              m_logListenerState.Value = 0;
              ModelDAOHelper.DAOFactory.ApplicationStateDAO.MakePersistent (m_logListenerState);
              transaction.Commit ();
            }
          }
        }
      }
    }
    
    
    #endregion // Methods
  }
}
