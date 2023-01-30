// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Serialization;

using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Extensions.Alert;

namespace Lemoine.Alert.GDBListeners
{
  /// <summary>
  /// Listener to check there is no delay in the modifications
  /// </summary>
  [Serializable]
  public class GlobalModificationDelayListener : IListener
  {
    static readonly int DEFAULT_MARGIN = 20; // in %
    static readonly TimeSpan DEFAULT_FLAPPING_THRESHOLD = TimeSpan.FromSeconds (10);

    /// <summary>
    /// Frequency
    /// </summary>
    static readonly string FREQUENCY_KEY = "Alert.Listener.GlobalModificationDelay.Frequency";
    static readonly TimeSpan FREQUENCY_DEFAULT = TimeSpan.FromMinutes (3); // Check it only every 3 minutes by default

    #region Members
    IDictionary<TimeSpan, string> m_thresholds = new SortedDictionary<TimeSpan, string> ();
    int m_margin = DEFAULT_MARGIN; // in %
    TimeSpan m_flappingThreshold = DEFAULT_FLAPPING_THRESHOLD;

    TimeSpan m_previousThreshold;
    DateTime m_previousDateTime;
    TimeSpan m_reachedThreshold;
    TimeSpan m_nextThreshold;
    DateTime m_lastCreateData;
    bool m_beginFlapping = false;
    bool m_flapping = false;

    DateTime m_lastFetch = DateTime.MinValue;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (GlobalModificationDelayListener).FullName);

    #region Getters / Setters
    /// <summary>
    /// Margin to let the status to be UP again and avoid some flapping status.
    /// It must be set in % and it is between 0 and 100
    /// 
    /// By default: 20 %
    /// </summary>
    [XmlAttribute ("Margin")]
    public int Margin
    {
      get { return m_margin; }
      set
      {
        if ((0 <= value) && (value < 100)) { // Valid value
          log.DebugFormat ("Margin.set: " +
                           "set margin={0}",
                           value);
          m_margin = value;
        }
        else {
          log.ErrorFormat ("Margin.set: " +
                           "invalid margin {0}, " +
                           "keep {1}",
                           value, m_margin);
          throw new ArgumentOutOfRangeException ("Margin");
        }
      }
    }

    /// <summary>
    /// Flapping threshold
    /// </summary>
    [XmlAttribute ("FlappingThreshold")]
    public string FlappingThreshold
    {
      get { return m_flappingThreshold.ToString (); }
      set { m_flappingThreshold = TimeSpan.Parse (value); }
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
      get
      {
        string thresholdsString = "";
        foreach (KeyValuePair<TimeSpan, string> threshold in m_thresholds) {
          thresholdsString += threshold.Key.ToString () + "=" + threshold.Value;
        }
        log.DebugFormat ("Thresholds.get: " +
                         "{0}",
                         thresholdsString);
        return thresholdsString;
      }
      set
      {
        string[] thresholds = value.Split (new char[] { ';' });
        foreach (string threshold in thresholds) {
          string[] thresholdMessage = threshold.Split (new char[] { '=' }, 2);
          if (2 <= thresholdMessage.Length) {
            TimeSpan timeSpan;
            if (TimeSpan.TryParse (thresholdMessage[1], out timeSpan)) {
              m_thresholds.Add (timeSpan, thresholdMessage[0]);
            }
            else {
              log.ErrorFormat ("Thresholds.set: " +
                               "invalid TimeSpan {0} in {1}",
                               thresholdMessage[1],
                               threshold);
            }
          }
          else {
            log.ErrorFormat ("Thresholds.set: " +
                             "invalid threshold=message key {0}",
                             threshold);
          }
        }
        ResetNextThreshold ();
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public GlobalModificationDelayListener ()
    {
      m_reachedThreshold = TimeSpan.FromTicks (0);
      ResetNextThreshold ();
      m_lastCreateData = new DateTime ();
      m_flapping = false;
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
      var frequency = Lemoine.Info.ConfigSet
        .LoadAndGet (FREQUENCY_KEY, FREQUENCY_DEFAULT);
      var nextFetch = m_lastFetch.Add (frequency);
      if (DateTime.UtcNow < nextFetch) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetData: {nextFetch} not reached (frequency={frequency}) => return null");
        }
        return null;
      }

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        // Get the first pending modification
        IGlobalModification firstPendingModification = ModelDAOHelper.DAOFactory.GlobalModificationDAO
          .GetFirstPendingModification ();
        m_lastFetch = DateTime.UtcNow;
        if (null == firstPendingModification) {
          log.DebugFormat ("GetData: " +
                           "no pending modification, no alert to raise");
          return null;
        }
        else { // null != firstPendingModification
          Debug.Assert ((AnalysisStatus.New == firstPendingModification.AnalysisStatus)
                        || (AnalysisStatus.Pending == firstPendingModification.AnalysisStatus));

          DateTime lastModificationDateTime;
          if (AnalysisStatus.New == firstPendingModification.AnalysisStatus) {
            lastModificationDateTime = firstPendingModification.DateTime;
          }
          else if (AnalysisStatus.Pending == firstPendingModification.AnalysisStatus) {
            if (firstPendingModification.AnalysisEnd.HasValue) {
              lastModificationDateTime = firstPendingModification.AnalysisEnd.Value;
            }
            else {
              lastModificationDateTime = firstPendingModification.DateTime;
            }
          }
          else {
            log.WarnFormat ("GetData: " +
                            "first pending modification {0} is not pending any more " +
                            "status={1}",
                            firstPendingModification,
                            firstPendingModification.AnalysisStatus);
            return null;
          }

          DateTime now = DateTime.UtcNow;
          if (now < lastModificationDateTime) {
            log.ErrorFormat ("GetData: " +
                             "date/time {0} of {1} is in the future, give up",
                             lastModificationDateTime, firstPendingModification);
            return null;
          }
          TimeSpan age = now.Subtract (lastModificationDateTime);

          // Check we are still in the same section,
          // else create the XML data
          // - Get the previously reached threshold
          TimeSpan previousReachedThreshold = m_reachedThreshold;
          TimeSpan previousReachedThresholdMinusMargin =
            TimeSpan.FromSeconds (previousReachedThreshold.TotalSeconds * (100 - m_margin) / 100);
          if (age < previousReachedThresholdMinusMargin) {
            // - If age is before previousReachedThreshold - margin
            //   => the delay is much better => UP
            log.DebugFormat ("GetData: " +
                             "the delay is better");
            return CreateData (firstPendingModification, age, GlobalModificationDelayDirection.UP);
          }
          else if (age < previousReachedThreshold) {
            // - a little better but not sufficient to raise an UP message
            //   This is done to avoid the data is flapping too much
            //   => no category change for the moment,
            //      visit the next machine
            log.DebugFormat ("GetData: " +
                             "just a little better");
            return null;
          }
          else {
            // - Get the threshold after previousReachedThreshold
            if (age <= m_nextThreshold) { // No category change
              // Visit the next machine
              log.DebugFormat ("GetData: " +
                               "no category change");
              return null;
            }
            else { // The delay is worse => DOWN
              log.DebugFormat ("GetData: " +
                               "the delay is worse");
              return CreateData (firstPendingModification, age, GlobalModificationDelayDirection.DOWN);
            }
          }
        }
      }
    }

    XmlElement CreateData (IGlobalModification modification, TimeSpan age, GlobalModificationDelayDirection direction)
    {
      // Get the thresholds
      TimeSpan ageThreshold = TimeSpan.FromTicks (0);
      TimeSpan ageNextThreshold = TimeSpan.MaxValue;
      string message = "OK";
      bool thresholdReached = false;
      foreach (KeyValuePair<TimeSpan, string> threshold in m_thresholds) {
        if (thresholdReached && (age < threshold.Key)) { // Next
          ageNextThreshold = threshold.Key;
          break;
        }
        ageThreshold = threshold.Key;
        message = threshold.Value;
        thresholdReached = true;
      }
      if (!thresholdReached) {
        log.DebugFormat ("CreateData: " +
                         "age {0} is before the first defined threshold " +
                         "=> use the default values 0:00:00 and OK");
        Debug.Assert (direction.Equals (GlobalModificationDelayDirection.UP));
      }
      bool flapping = false;
      if (m_previousThreshold.Equals (ageThreshold)
          && (DateTime.UtcNow.Subtract (m_previousDateTime) < m_flappingThreshold)) {
        flapping = true;
      }
      // - Store the new thresholds / update machineStatus
      m_previousThreshold = m_reachedThreshold;
      m_previousDateTime = m_lastCreateData;
      m_reachedThreshold = ageThreshold;
      m_nextThreshold = ageNextThreshold;
      m_lastCreateData = DateTime.UtcNow;
      m_beginFlapping = flapping && !(m_flapping);
      m_flapping = flapping;
      // - Create the Xml data
      modification.Unproxy ();
      GlobalModificationDelayData data =
        new GlobalModificationDelayData (modification, direction, message, age, ageThreshold, flapping, m_beginFlapping);
      Type type = data.GetType ();
      XmlSerializer serializer = new XmlSerializer (type);
      StringWriter sw = new StringWriter ();
      serializer.Serialize (sw, data);
      XmlDocument document = new XmlDocument ();
      document.LoadXml (sw.ToString ());
      return document.DocumentElement;
    }

    void ResetNextThreshold ()
    {
      m_nextThreshold = TimeSpan.MaxValue;
      foreach (TimeSpan threshold in m_thresholds.Keys) {
        if (m_reachedThreshold < threshold) {
          m_nextThreshold = threshold;
          break;
        }
      }
    }
    #endregion // Methods
  }
}
