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
  /// Listener to track the modification in error or in timeout
  /// </summary>
  [Serializable]
  public class GlobalModificationInErrorListener : IListener
  {
    static readonly string APPLICATION_STATE_KEY = "alert.listener.globalmodificationinerror";

    /// <summary>
    /// Frequency
    /// </summary>
    static readonly string FREQUENCY_KEY = "Alert.Listener.GlobalModificationInError.Frequency";
    static readonly TimeSpan FREQUENCY_DEFAULT = TimeSpan.FromMinutes (3); // Check it only every 3 minutes by default

    #region Members
    IApplicationState m_modificationInErrorListenerState = null;
    IList<IGlobalModification> m_fetched = new List<IGlobalModification> ();
    DateTime m_lastFetch = DateTime.MinValue;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (GlobalModificationInErrorListener).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public GlobalModificationInErrorListener ()
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
      CheckInitialization ();

      if (0 == m_fetched.Count) { // No data any more process, try to fetch some more
        var frequency = Lemoine.Info.ConfigSet
          .LoadAndGet (FREQUENCY_KEY, FREQUENCY_DEFAULT);
        var nextFetch = m_lastFetch.Add (frequency);
        if (nextFetch <= DateTime.UtcNow) {
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
            m_fetched = ModelDAOHelper.DAOFactory.GlobalModificationDAO
              .GetInErrorStrictlyAfter ((int)m_modificationInErrorListenerState.Value);
          }
        }
        m_lastFetch = DateTime.UtcNow;
      }

      if (0 == m_fetched.Count) { // No data any more to process
        log.DebugFormat ("GetData: " +
                         "no data");
        return null;
      }
      else { // 0 < m_fetched.Count
        IGlobalModification firstData = m_fetched[0];
        XmlElement data;
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          ModelDAOHelper.DAOFactory.GlobalModificationDAO.Lock (firstData);
          firstData.Unproxy ();
          // - serialization
          Type type = Type.GetType ("Lemoine.GDBPersistentClasses.GlobalModification, Pulse.Database");
          if (type is null) {
            log.Fatal ($"GetData: type Lemoine.GDBPersistentClasses.GlobalModification, Pulse.Database does not exist");
          }
          XmlSerializer serializer = new XmlSerializer (type);
          StringWriter sw = new StringWriter ();
          serializer.Serialize (sw, firstData);
          XmlDocument document = new XmlDocument ();
          document.LoadXml (sw.ToString ());
          data = document.DocumentElement;
          // - Update the application state
          using (IDAOTransaction transaction = session.BeginTransaction ()) {
            ModelDAOHelper.DAOFactory.ApplicationStateDAO.Lock (m_modificationInErrorListenerState);
            m_modificationInErrorListenerState.Value = ((Lemoine.Collections.IDataWithId)firstData).Id;
            ModelDAOHelper.DAOFactory.ApplicationStateDAO
              .MakePersistent (m_modificationInErrorListenerState);
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
      if (null == m_modificationInErrorListenerState) {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          m_modificationInErrorListenerState = ModelDAOHelper.DAOFactory.ApplicationStateDAO
            .GetApplicationState (APPLICATION_STATE_KEY);
          if (null == m_modificationInErrorListenerState) {
            using (IDAOTransaction transaction = session.BeginTransaction ()) {
              m_modificationInErrorListenerState = ModelDAOHelper.ModelFactory.CreateApplicationState (APPLICATION_STATE_KEY);
              m_modificationInErrorListenerState.Value = 0;
              ModelDAOHelper.DAOFactory.ApplicationStateDAO.MakePersistent (m_modificationInErrorListenerState);
              transaction.Commit ();
            }
          }
        }
      }
    }


    #endregion // Methods
  }
}
