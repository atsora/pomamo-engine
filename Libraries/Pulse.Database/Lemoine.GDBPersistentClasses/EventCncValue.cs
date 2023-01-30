// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table EventCncValue
  /// </summary>
  [Serializable]
  public class EventCncValue: EventUnion, IEventCncValue
  {
    #region Members
    string m_message;
    IMachineModule m_machineModule;
    IField m_field;
    string m_string = null;
    int? m_int = null;
    double? m_double = null;
    TimeSpan m_duration;
    IEventCncValueConfig m_config;
    #endregion // Members

    ILog log = LogManager.GetLogger(typeof (EventCncValue).FullName);

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected EventCncValue ()
    { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="level"></param>
    /// <param name="dateTime"></param>
    /// <param name="message"></param>
    /// <param name="machineModule">not null</param>
    /// <param name="field"></param>
    /// <param name="v"></param>
    /// <param name="duration"></param>
    /// <param name="config"></param>
    internal protected EventCncValue(IEventLevel level, DateTime dateTime, string message, IMachineModule machineModule, IField field, object v, TimeSpan duration, IEventCncValueConfig config)
      : base (level, dateTime)
    {
      this.Message = message;
      this.MachineModule = machineModule;
      this.Field = field;
      this.Value = v;
      this.Duration = duration;
      this.Config = config;
    }
    #endregion // Constructors
    
    #region Getters / Setters
    /// <summary>
    /// Event message
    /// </summary>
    [XmlAttribute("Message")]
    public virtual string Message {
      get { return m_message; }
      set // public because of the XmlSerialization
      {
        if (null == value) {
          log.FatalFormat ("Message can't be null");
          Debug.Assert (null != value);
          throw new ArgumentNullException ("null Message in EventCncValue");        }
        else {
          m_message = value;
        }
      }
    }
    
    /// <summary>
    /// Machine module that is associated to the event
    /// 
    /// not null
    /// </summary>
    [XmlIgnore]
    public virtual IMachineModule MachineModule {
      get { return m_machineModule; }
      protected set
      {
        if (null == value) {
          log.FatalFormat ("MachineModule can't be null");
          Debug.Assert (null != value);
          throw new ArgumentNullException ("null MachineModule in EventCncValue");        }
        else {
          m_machineModule = value;
          log = LogManager.GetLogger(string.Format ("{0}.{1}.{2}",
                                                    this.GetType ().FullName,
                                                    value.MonitoredMachine.Id,
                                                    value.Id));
        }
      }
    }
    
    /// <summary>
    /// Machine module for XML serialization
    /// </summary>
    [XmlElement("MachineModule")]
    public virtual MachineModule XmlSerializationMachineModule {
      get { return this.MachineModule as MachineModule; }
      set { this.MachineModule = value; }
    }
    
    /// <summary>
    /// Monitored machine that is associated to the event
    /// 
    /// not null
    /// </summary>
    [XmlIgnore]
    public virtual IMonitoredMachine MonitoredMachine {
      get { return m_machineModule.MonitoredMachine; }
    }
    
    /// <summary>
    /// Monitored machine for XML serialization
    /// </summary>
    [XmlElement("MonitoredMachine")]
    public virtual MonitoredMachine XmlSerializationMonitoredMachine {
      get { return this.MonitoredMachine as MonitoredMachine; }
      set { throw new InvalidOperationException (); }
    }
    
    /// <summary>
    /// Machine that is associated to the event
    /// 
    /// not null
    /// </summary>
    [XmlIgnore]
    public virtual IMachine Machine {
      get { return this.MonitoredMachine; }
    }

    /// <summary>
    /// Associated field
    /// </summary>
    [XmlIgnore]
    public virtual IField Field {
      get { return m_field; }
      protected set {
        if (null == value) {
          log.FatalFormat ("Field can't be null");
          Debug.Assert (null != value);
          throw new ArgumentNullException ("null Field in EventCncValue");
        } else {
          m_field = value;
        }
      }
    }

    /// <summary>
    /// Field for XML serialization
    /// </summary>
    [XmlElement("Field")]
    public virtual Field XmlSerializationField {
      get { return this.Field as Field; }
      set { this.Field = value; }
    }

    /// <summary>
    /// Value in case the corresponding Field refers to a String
    /// </summary>
    [XmlIgnore]
    public virtual string String {
      get { return m_string; }
    }
    
    /// <summary>
    /// Value in case the corresponding Field refers to an Int32
    /// </summary>
    [XmlIgnore]
    public virtual Nullable<int> Int {
      get { return m_int; }
    }
    
    /// <summary>
    /// Value in case the corresponding Field refers to a Double
    /// </summary>
    [XmlIgnore]
    public virtual Nullable<double> Double {
      get { return m_double; }
    }
    
    /// <summary>
    /// String, Int or Double value according to the Type property of the Field
    /// </summary>
    [XmlElement("Value")]
    public virtual object Value {
      get
      {
        switch (this.Field.Type) {
          case FieldType.Boolean:
            return (this.Int != 0);
          case FieldType.String:
            return this.String;
          case FieldType.Int32:
            return this.Int;
          case FieldType.Double:
            return this.Double;
          default:
            log.ErrorFormat ("Value.get: " +
                             "unknown field type {0}",
                             this.Field.Type);
            throw new Exception ("Unknown field type");
        }
      }
      set
      {
        try {
          switch (this.Field.Type) {
            case FieldType.Boolean:
              m_string = (Convert.ToBoolean (value)).ToString ();
              m_int = (Convert.ToBoolean (value))?1:0;
              m_double = (Convert.ToBoolean (value))?1.0:0.0;
              return;
            case FieldType.String:
              m_string = value.ToString ();
              m_int = null;
              m_double = null;
              return;
            case FieldType.Int32:
            case FieldType.Double:
              m_string = value.ToString ();
              m_int = Convert.ToInt32 (value);
              m_double = Convert.ToDouble (value);
              return;
          }
        }
        catch (Exception ex) {
          log.ErrorFormat ("Value.set: " +
                           "error {0} value {1}",
                           ex, value);
          throw new InvalidCastException ("The value could not be converted to the right type", ex);
        }
      }
    }
    
    /// <summary>
    /// Duration that triggered the event
    /// </summary>
    [XmlIgnore]
    public virtual TimeSpan Duration {
      get { return m_duration; }
      protected set { m_duration = value; }
    }
    
    /// <summary>
    /// Duration that triggered the event for XML serialization
    /// </summary>
    [XmlAttribute ("Duration")]
    public virtual string XmlSerializationDuration {
      get { return this.Duration.ToString (); }
      set { this.Duration = TimeSpan.Parse (value); }
    }

    /// <summary>
    /// Associated config
    /// 
    /// null if the config has been removed
    /// </summary>
    [XmlIgnore]
    public virtual IEventCncValueConfig Config {
      get { return m_config; }
      set { m_config = value; }
    }
    #endregion // Getters / Setters
    
    #region Methods
    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public override void Unproxy ()
    {
      base.Unproxy ();
      NHibernateHelper.Unproxy<IMachineModule> (ref m_machineModule);
      NHibernateHelper.Unproxy<IField> (ref m_field);
    }
    #endregion // Methods
  }
}
