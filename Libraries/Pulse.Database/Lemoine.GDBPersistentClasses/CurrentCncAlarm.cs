// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table CurrentCncAlarm
  /// </summary>
  [Serializable]
  public class CurrentCncAlarm: ICurrentCncAlarm, IVersionable, Lemoine.Collections.IDataWithId
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    string m_cncInfo = "";
    string m_cncSubInfo = "";
    string m_type = "";
    string m_number = "";
    IMachineModule m_machineModule;
    DateTime m_dateTime = DateTime.UtcNow;
    string m_message;
    IDictionary<string, object> m_properties = new Dictionary<string, object>();
    #endregion // Members

    ILog log = LogManager.GetLogger(typeof(CurrentCncAlarm).FullName);

    #region Getters / Setters
    /// <summary>
    /// Slot Id
    /// </summary>
    public virtual int Id
    {
      get { return this.m_id; }
    }
    
    /// <summary>
    /// Slot Version
    /// </summary>
    public virtual int Version
    {
      get { return this.m_version; }
    }
    
    /// <summary>
    /// UTC date/time stamp
    /// </summary>
    [XmlAttribute("DateTime")]
    public virtual DateTime DateTime {
      get { return m_dateTime; }
      set {
        switch (value.Kind) {
          case DateTimeKind.Unspecified:
            log.WarnFormat ("DateTime.set: " +
                            "unspecified DateTimeKind => suppose it is a universal time");
            m_dateTime = new DateTime (value.Ticks, DateTimeKind.Utc);
            break;
          case DateTimeKind.Utc:
            m_dateTime = value;
            break;
          case DateTimeKind.Local:
            m_dateTime = value.ToUniversalTime ();
            break;
          default:
            throw new Exception("Invalid value for DateTimeKind");
        }
      }
    }

    /// <summary>
    /// Associated machine module
    /// </summary>
    public virtual IMachineModule MachineModule {
      get { return m_machineModule; }
      protected set
      {
        m_machineModule = value;
        log = LogManager.GetLogger(string.Format(
          "{0}.{1}.{2}", this.GetType().FullName, value.MonitoredMachine.Id, value.Id));
      }
    }
    
    /// <summary>
    /// Display name that is retrieved with a display function
    /// </summary>
    [XmlAttribute("Display")]
    public virtual string Display
    {
      get; set;
    }
    
    /// <summary>
    /// Severity that is retrieved with a function using the severity patterns
    /// </summary>
    [XmlIgnore]
    public virtual ICncAlarmSeverity Severity
    {
      get { return m_severity; }
    }
    ICncAlarmSeverity m_severity = null;

    /// <summary>
    /// Severity for XML serialization
    /// </summary>
    [XmlElement("Severity")]
    public virtual CncAlarmSeverity XmlSerializationSeverity {
      get { return this.Severity as CncAlarmSeverity; }
      set { throw new InvalidOperationException ("Severity read-only - deserialization not supported"); }
    }

    /// <summary>
    /// Color that is associated to the severity
    /// </summary>
    [XmlIgnore]
    public virtual string Color {
      get { return this.Severity?.Color; }
    }
    
    /// <summary>
    /// Color for XML serialization
    /// </summary>
    [XmlAttribute("Color")]
    public virtual string XmlSerializationColor {
      get { return this.Color; }
      set { throw new InvalidOperationException ("Color read-only - deserialization not supported"); }
    }
    
    /// <summary>
    /// Alarm Cnc Info
    /// </summary>
    [XmlIgnore]
    public virtual string CncInfo {
      get { return m_cncInfo; }
    }

    /// <summary>
    /// Alarm Cnc Info for XML serialization
    /// </summary>
    [XmlAttribute("CncInfo")]
    public virtual string XmlSerializationCncInfo {
      get { return m_cncInfo; }
      set { throw new InvalidOperationException ("CncInfo read-only - deserialization not supported"); }
    }
    
    /// <summary>
    /// Alarm Cnc Sub Info
    /// </summary>
    [XmlIgnore]
    public virtual string CncSubInfo {
      get { return m_cncSubInfo; }
    }

    /// <summary>
    /// Alarm Cnc Sub Info for XML serialization
    /// </summary>
    [XmlAttribute("CncSubInfo")]
    public virtual string XmlSerializationCncSubInfo {
      get { return m_cncSubInfo; }
      set { throw new InvalidOperationException ("CncSubInfo read-only - deserialization not supported"); }
    }

    /// <summary>
    /// Alarm Type
    /// </summary>
    [XmlIgnore]
    public virtual string Type {
      get { return m_type; }
    }

    /// <summary>
    /// Alarm Type for XML serialization
    /// </summary>
    [XmlAttribute("Type")]
    public virtual string XmlSerializationType {
      get { return m_type; }
      set { throw new InvalidOperationException ("Type read-only - deserialization not supported"); }
    }

    /// <summary>
    /// Alarm Number
    /// </summary>
    [XmlIgnore]
    public virtual string Number {
      get { return m_number; }
    }
    
    /// <summary>
    /// Alarm Number for XML serialization
    /// </summary>
    [XmlAttribute("Number")]
    public virtual string XmlSerializationNumber {
      get { return m_number; }
      set { throw new InvalidOperationException ("Number read-only - deserialization not supported"); }
    }
    
    /// <summary>
    /// Alarm Message
    /// </summary>
    [XmlAttribute("Message")]
    public virtual string Message {
      get { return m_message; }
      set { m_message = value; }
    }
    
    /// <summary>
    /// Alarm properties
    /// </summary>
    [XmlIgnore]
    public virtual IDictionary<string, object> Properties {
      get { return m_properties; }
    }
    
    /// <summary>
    /// Alarm properties for xml serialization
    /// </summary>
    [XmlAttribute("Properties")]
    public virtual string XmlSerializationProperties {
      get {
        string txt = "";
        foreach (var key in Properties.Keys) {
          if (txt != "") {
            txt += ";";
          }

          txt += key + "=" + Properties[key];
        }
        return txt;
      }
      set { throw new InvalidOperationException ("Properties - deserialization not supported"); }
    }

    /// <summary>
    /// Computed priority, based on the severity if any
    /// 
    /// The lower the priority is, the more critical the alarm is
    /// </summary>
    [XmlIgnore]
    public virtual int Priority {
      get {
        return this.Severity == null ? 1000 : this.Severity.Priority;
      }
    }
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// Protected constructor with no arguments
    /// </summary>
    protected CurrentCncAlarm() {}
    
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="datetime"></param>
    /// <param name="cncInfo"></param>
    /// <param name="cncSubInfo"></param>
    /// <param name="type"></param>
    /// <param name="number"></param>
    public CurrentCncAlarm(IMachineModule machineModule, DateTime datetime, string cncInfo, string cncSubInfo,
                           string type, string number)
    {
      m_cncInfo = cncInfo;
      m_cncSubInfo = cncSubInfo;
      m_type = type;
      m_number = number;
      m_machineModule = machineModule;
      m_dateTime = datetime;
    }
    #endregion // Constructors
    
    #region Methods
    /// <summary>
    /// Unproxy all the properties
    /// </summary>
    public virtual void Unproxy()
    {
      if (m_severity != null) {
        NHibernateHelper.Unproxy<ICncAlarmSeverity>(ref m_severity);
      }
    }
    #endregion // Methods
  }
}
