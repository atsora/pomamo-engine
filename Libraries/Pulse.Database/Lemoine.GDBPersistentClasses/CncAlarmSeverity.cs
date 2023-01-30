// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table CncAlarmSeverity
  /// </summary>
  [Serializable]
  public class CncAlarmSeverity : ICncAlarmSeverity, IVersionable, IDisplayable
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    string m_cncInfo = "";
    string m_name = "";
    EditStatus m_status = EditStatus.MANUAL_INPUT;
    string m_description = "";
    CncAlarmStopStatus m_stopStatus = CncAlarmStopStatus.Unknown;
    string m_color = "";
    bool? m_focus = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (CncAlarmSeverity).FullName);

    #region Getters / Setters
    /// <summary>
    /// CncAlarmSeverity Id
    /// </summary>
    [XmlAttribute ("Id")]
    public virtual int Id
    {
      get { return m_id; }
    }

    /// <summary>
    /// CncAlarmSeverity Version
    /// </summary>
    [XmlIgnore]
    public virtual int Version
    {
      get { return this.m_version; }
    }

    /// <summary>
    /// Cnc that can have the severity
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
    /// CncAlarmSeverity name
    /// Cannot be null or empty
    /// </summary>
    [XmlAttribute ("Name")]
    public virtual string Name
    {
      get { return m_name; }
      set
      {
        if (String.IsNullOrEmpty (value)) {
          const string txt = "CncAlarmSeverity: cannot store a null or empty value for the Name";
          log.Error (txt);
          throw new Exception (txt);
        }
        m_name = value;
      }
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
    /// CncAlarmSeverity description (often taken from the manuals)
    /// Can be null or empty
    /// </summary>
    [XmlAttribute ("Description")]
    public virtual string Description
    {
      get { return m_description; }
      set { m_description = value; }
    }

    /// <summary>
    /// Stop status of the cnc alarm (see enum CncAlarmStopStatus)
    /// </summary>
    [XmlIgnore]
    public virtual CncAlarmStopStatus StopStatus
    {
      get { return m_stopStatus; }
      set { m_stopStatus = value; }
    }

    /// <summary>
    /// Stop status of cnc alarm in string
    /// 
    /// An empty string is returned if the stop status is not set
    /// </summary>
    [XmlAttribute ("StopStatus")]
    public virtual string StopStatusString
    {
      get { return this.StopStatus.ToString (); }
      set
      {
        this.StopStatus = string.IsNullOrEmpty (value) ?
          CncAlarmStopStatus.Unknown :
          (CncAlarmStopStatus)Enum.Parse (typeof (CncAlarmStopStatus), value, true);
      }
    }

    /// <summary>
    /// Full description based on Description, ProgramStopped,
    /// and all other properties that might characterized the severity
    /// </summary>
    [XmlIgnore]
    public virtual string FullDescription
    {
      get { return this.Description + "\n" + "Stop status: " + StopStatus; }
    }

    /// <summary>
    /// Color associated to the severity in the format #RRGGBB
    /// Can be null or empty
    /// </summary>
    [XmlAttribute ("Color")]
    public virtual string Color
    {
      get { return m_color; }
      set
      {
        if (!string.IsNullOrEmpty (value) && !Regex.IsMatch (value, "^#[0-9a-fA-F]{6}$")) {
          string txt = "CncAlarmSeverity: cannot store value " + value + " as a Color";
          log.Error (txt);
          throw new Exception (txt);
        }
        m_color = value;
      }
    }

    /// <summary>
    /// True if the severity must be taken into account
    /// False if the severity can be ignored
    /// Null if we don't know or if the answer is sometimes
    /// </summary>
    [XmlIgnore]
    public virtual bool? Focus
    {
      get { return this.m_focus; }
      set { this.m_focus = value; }
    }

    /// <summary>
    /// Focus for xml serialization
    /// </summary>
    [XmlAttribute ("Focus")]
    public virtual string XmlSerializationFocus
    {
      get { return (this.m_focus == null) ? "" : m_focus.Value.ToString (); }
      set { m_focus = String.IsNullOrEmpty (value) ? (bool?)null : bool.Parse (value); }
    }

    /// <summary>
    /// Computed priority
    /// 
    /// The lower the priority is, the more critical the alarm is
    /// 
    /// Number between 0 and 999
    /// </summary>
    [XmlIgnore]
    public virtual int Priority
    {
      get
      {
        // Offset between 0 and 99
        int offset = this.StopStatus.GetPriority ();

        if (!this.Focus.HasValue) { // 5xx range
          return 500 + offset;
        }
        else { // this.Severity.Focus.HasValue
          if (this.Focus.Value) {
            return 100 + offset;
          }
          else {
            return 700 + offset;
          }
        }
      }
    }

    /// <summary>
    /// Display
    /// </summary>
    [XmlIgnore]
    public virtual string Display
    {
      get { return this.Name; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Protected constructor with no arguments
    /// </summary>
    protected CncAlarmSeverity () { }

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="cncInfo"></param>
    /// <param name="name"></param>
    public CncAlarmSeverity (string cncInfo, string name)
    {
      CncInfo = cncInfo;
      Name = name;
    }
    #endregion // Constructors

    /// <summary>
    /// Unproxy all the properties
    /// </summary>
    public virtual void Unproxy () { }
  }

  /// <summary>
  /// Convert a CncAlarmStoppedStatus enum to a citext in database
  /// </summary>
  [Serializable]
  public class EnumCncAlarmSeverityStopStatus : NHibernateTypes.EnumCitextType<CncAlarmStopStatus>
  {
  }
}
