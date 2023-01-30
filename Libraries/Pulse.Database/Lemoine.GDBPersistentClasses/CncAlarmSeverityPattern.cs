// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Xml.Serialization;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table CncAlarmSeverityPattern
  /// </summary>
  [Serializable]
  public class CncAlarmSeverityPattern : ICncAlarmSeverityPattern, IVersionable
  {
    #region Members
    int m_id = 0;
    int m_version = 0;

    string m_cncInfo = "";
    string m_name = "";
    EditStatus m_status = EditStatus.MANUAL_INPUT;
    CncAlarmSeverityPatternRules m_rules = null;
    ICncAlarmSeverity m_severity = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (CncAlarmSeverityPattern).FullName);

    #region Getters / Setters
    /// <summary>
    /// CncAlarmSeverityPattern Id
    /// </summary>
    [XmlAttribute ("Id")]
    public virtual int Id
    {
      get { return this.m_id; }
    }

    /// <summary>
    /// CncAlarmSeverityPattern Version
    /// </summary>
    [XmlIgnore]
    public virtual int Version
    {
      get { return this.m_version; }
    }

    /// <summary>
    /// Cnc that can have the severity pattern
    /// Cannot be null or empty
    /// </summary>
    [XmlAttribute ("CncInfo")]
    public virtual string CncInfo
    {
      get { return m_cncInfo; }
      set
      {
        if (String.IsNullOrEmpty (value)) {
          const string txt = "CncAlarmSeverity: cannot store a null or empty value for the CncInfo";
          log.Error (txt);
          throw new Exception (txt);
        }
        m_cncInfo = value;
      }
    }

    /// <summary>
    /// CncAlarmSeverityPattern name
    /// Can be null or empty
    /// Used to recognize pattern and update them
    /// </summary>
    [XmlAttribute ("Name")]
    public virtual string Name
    {
      get { return m_name; }
      set { m_name = value; }
    }

    /// <summary>
    /// Status of the alarm severity pattern, useful to know if it can be updated or not
    /// 0: new (this is a new item that won't be updated)
    /// 1: default (update allowed)
    /// 2: edited (update forbidden)
    /// 3: deactivated (update forbidden and severity pattern disabled)
    /// </summary>
    [XmlAttribute ("Status")]
    public virtual EditStatus Status
    {
      get { return m_status; }
      set { m_status = value; }
    }

    /// <summary>
    /// Rules used to detect severities in a CncAlarm
    /// (Stored as json in the database)
    /// </summary>
    [XmlAttribute ("Rules")]
    public virtual CncAlarmSeverityPatternRules Rules
    {
      get { return m_rules; }
      set { m_rules = value; }
    }

    /// <summary>
    /// Severity associated to the pattern and the cncInfo
    /// Cannot be null
    /// </summary>
    [XmlAttribute ("Severity")]
    public virtual ICncAlarmSeverity Severity
    {
      get { return m_severity; }
      set
      {
        if (value == null) {
          const string txt = "CncAlarmSeverity: cannot store a null Severity";
          log.Error (txt);
          throw new Exception (txt);
        }
        m_severity = value;
      }
    }

    /// <summary>
    /// Get a textual description of what is in the pattern
    /// </summary>
    [XmlIgnore]
    public virtual string Description
    {
      get
      {
        if (m_rules == null) {
          return "";
        }

        string txt = "";

        // Add common rules
        if (!string.IsNullOrEmpty (m_rules.CncSubInfo)) {
          txt += (txt != "" ? "\n" : "") + "CncSubInfo like '" + m_rules.CncSubInfo + "'";
        }

        if (!string.IsNullOrEmpty (m_rules.Type)) {
          txt += (txt != "" ? "\n" : "") + "Type like '" + m_rules.Type + "'";
        }

        if (!string.IsNullOrEmpty (m_rules.Number)) {
          txt += (txt != "" ? "\n" : "") + "Number like '" + m_rules.Number + "'";
        }

        if (!string.IsNullOrEmpty (m_rules.Message)) {
          txt += (txt != "" ? "\n" : "") + "Message like '" + m_rules.Message + "'";
        }

        // Add properties
        if (m_rules.Properties != null) {
          foreach (string key in m_rules.Properties.Keys) {
            txt += (txt != "" ? "\n" : "") + "Property '" + key + "' is '" + m_rules.Properties[key] + "'";
          }
        }
        
        if (txt == "") {
          txt = "empty pattern";
        }

        return txt;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Protected constructor with no arguments
    /// </summary>
    protected CncAlarmSeverityPattern () { }

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="cncInfo"></param>
    /// <param name="rules"></param>
    /// <param name="severity"></param>
    public CncAlarmSeverityPattern (string cncInfo, CncAlarmSeverityPatternRules rules, ICncAlarmSeverity severity)
    {
      CncInfo = cncInfo;
      Rules = rules;
      Severity = severity;
    }
    #endregion // Constructors
  }
}
