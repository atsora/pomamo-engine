// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using Lemoine.Alert;
using Lemoine.Core.Log;
using Lemoine.Extensions.Alert;

namespace Lemoine.Alert.TestListeners
{
  /// <summary>
  /// Listener that returns the same XmlElement every x seconds
  /// </summary>
  [Serializable]
  public class LoopListener: IListener
  {
    #region Members
    DateTime m_lastExecution = DateTime.UtcNow;
    TimeSpan m_frequency = TimeSpan.FromSeconds (2);
    XmlElement m_data;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (LoopListener).FullName);

    #region Getters / Setters
    /// <summary>
    /// Frequency when a new data must be returned
    /// </summary>
    [XmlIgnore]
    public TimeSpan Frequency {
      get { return m_frequency; }
      set { m_frequency = value; }
    }
    
    /// <summary>
    /// Frequency when a new data must be returned for Xml Serialization
    /// </summary>
    [XmlAttribute("Frequency")]
    public string FrequencyXmlSerialization {
      get { return m_frequency.ToString (); }
      set { m_frequency = TimeSpan.Parse (value); }
    }
    
    /// <summary>
    /// Data to return
    /// </summary>
    public string Data {
      get { return m_data.OuterXml; }
      set
      {
        XmlDocument xmlDocument = new XmlDocument ();
        xmlDocument.LoadXml (value);
        m_data = xmlDocument.DocumentElement;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public LoopListener ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Implements <see cref="IListener" />
    /// </summary>
    /// <returns></returns>
    public XmlElement GetData ()
    {
      if (DateTime.UtcNow.Subtract (m_lastExecution) < m_frequency) {
        // Last returned data is not old enough
        return null;
      }
      else {
        m_lastExecution = DateTime.UtcNow;
        return m_data;
      }
    }
    #endregion // Methods
  }
}
