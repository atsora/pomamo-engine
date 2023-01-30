// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;

using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.Core.Log;
using System.Xml.Serialization;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table CncValue
  /// </summary>
  [Serializable]
  public class CncValue : ICncValue
  {
    #region Members
    long m_id = 0;
    int m_version = 0;
    IMachineModule m_machineModule;
    IField m_field;
    DateTime m_begin;
    DateTime m_end;
    string m_string = null;
    int? m_int = null;
    double? m_double = null;
    double? m_deviation = null;
    bool m_stopped = false;
    #endregion // Members

    static readonly ILog staticLog = LogManager.GetLogger (typeof (CncValue).FullName);
    ILog log = LogManager.GetLogger (typeof (CncValue).FullName);

    #region Getters / Setters
    /// <summary>
    /// CncValue Id
    /// </summary>
    [XmlIgnore]
    public virtual long Id
    {
      get { return this.m_id; }
    }

    /// <summary>
    /// CncValue Version
    /// </summary>
    [XmlIgnore]
    public virtual int Version
    {
      get { return this.m_version; }
    }

    /// <summary>
    /// Associated machine module
    /// </summary>
    [XmlIgnore]
    public virtual IMachineModule MachineModule
    {
      get { return m_machineModule; }
      protected set {
        if (null == value) {
          log.FatalFormat ("MachineModule can't be null");
          Debug.Assert (null != value);
          throw new ArgumentNullException ("null MachineModule in CncValue");
        }
        else {
          m_machineModule = value;
          log = LogManager.GetLogger (string.Format ("{0}.{1}.{2}",
                                                    this.GetType ().FullName,
                                                    value.MonitoredMachine.Id, value.Id));
        }
      }
    }

    /// <summary>
    /// Machine module for XML serialization
    /// </summary>
    [XmlElement ("MachineModule")]
    public virtual MachineModule XmlSerializationMachineModule
    {
      get { return this.MachineModule as MachineModule; }
      set { this.MachineModule = value; }
    }

    /// <summary>
    /// Associated field
    /// </summary>
    [XmlIgnore]
    public virtual IField Field
    {
      get { return m_field; }
      protected set {
        if (null == value) {
          log.FatalFormat ("Field can't be null");
          Debug.Assert (null != value);
          throw new ArgumentNullException ("null Field in CncValue");
        }
        else {
          m_field = value;
        }
      }
    }

    /// <summary>
    /// Field for XML serialization
    /// </summary>
    [XmlElement ("Field")]
    public virtual Field XmlSerializationField
    {
      get { return this.Field as Field; }
      set { this.Field = value; }
    }

    /// <summary>
    /// Begin UTC date/time
    /// </summary>
    [XmlIgnore]
    public virtual DateTime Begin
    {
      get { return m_begin; }
    }

    /// <summary>
    /// Begin UTC Date/time for XML serialization
    /// </summary>
    [XmlAttribute ("Begin")]
    public virtual string SqlBegin
    {
      get {
        return this.Begin.ToString ("yyyy-MM-dd HH:mm:ss");
      }
      set {
        if (String.IsNullOrEmpty (value)) {
          m_begin = System.DateTime.UtcNow;
        }
        else {
          m_begin = System.DateTime.Parse (value);
        }
      }
    }

    /// <summary>
    /// Local date/time for XML serialization
    /// </summary>
    [XmlAttribute ("LocalBeginString")]
    public virtual string LocalBeginString
    {
      get { return this.Begin.ToLocalTime ().ToString (); }
      set { m_begin = DateTime.SpecifyKind (DateTime.Parse (value), DateTimeKind.Local).ToUniversalTime (); }
    }

    /// <summary>
    /// Local begin date/time for XML serialization in G format
    /// 8/18/2015 1:31:17 PM
    /// </summary>
    [XmlAttribute ("LocalBeginDateTimeG")]
    public virtual string XmlSerializationLocalBeginDateTimeG
    {
      get { return this.Begin.ToLocalTime ().ToString ("G"); }
      set { throw new InvalidOperationException ("LocalBeginDateTimeG - deserialization not supported"); }
    }

    /// <summary>
    /// End UTC date/time
    /// </summary>
    [XmlIgnore]
    public virtual DateTime End
    {
      get { return m_end; }
      set {
        switch (value.Kind) {
        case DateTimeKind.Unspecified:
          log.WarnFormat ("End.set: " +
                          "unspecified DateTimeKind => suppose it is a universal time");
          m_end = new DateTime (value.Ticks, DateTimeKind.Utc);
          break;
        case DateTimeKind.Utc:
          m_end = value;
          break;
        case DateTimeKind.Local:
          m_end = value.ToUniversalTime ();
          break;
        default:
          throw new Exception ("Invalid value for DateTimeKind");
        }
      }
    }

    /// <summary>
    /// End UTC Date/time for XML serialization
    /// </summary>
    [XmlAttribute ("End")]
    public virtual string SqlEnd
    {
      get {
        return this.End.ToString ("yyyy-MM-dd HH:mm:ss");
      }
      set {
        if (String.IsNullOrEmpty (value)) {
          m_end = System.DateTime.UtcNow;
        }
        else {
          m_end = System.DateTime.Parse (value);
        }
      }
    }

    /// <summary>
    /// Local date/time for XML serialization
    /// </summary>
    [XmlAttribute ("LocalEndString")]
    public virtual string LocalEndString
    {
      get { return this.End.ToLocalTime ().ToString (); }
      set { m_end = DateTime.SpecifyKind (DateTime.Parse (value), DateTimeKind.Local).ToUniversalTime (); }
    }

    /// <summary>
    /// Local end date/time for XML serialization in G format
    /// 8/18/2015 1:31:17 PM
    /// </summary>
    [XmlAttribute ("LocalEndDateTimeG")]
    public virtual string XmlSerializationLocalEndDateTimeG
    {
      get { return this.End.ToLocalTime ().ToString ("G"); }
      set { throw new InvalidOperationException ("LocalEndDateTimeG - deserialization not supported"); }
    }

    /// <summary>
    /// Utc date/time Range
    /// </summary>
    [XmlIgnore]
    public virtual UtcDateTimeRange DateTimeRange
    {
      get { return new UtcDateTimeRange (m_begin, m_end); }
    }

    /// <summary>
    /// Length
    /// </summary>
    [XmlIgnore]
    public virtual TimeSpan Length
    {
      get { return m_end.Subtract (m_begin); }
    }

    /// <summary>
    /// String value in case the corresponding Field refers to a String
    /// </summary>
    [XmlAttribute ("String")]
    public virtual string String
    {
      get { return m_string; }
      set {
        m_string = value;
        m_int = null;
        m_double = null;
      }
    }

    /// <summary>
    /// Int value in case the corresponding Field refers to an Int32
    /// </summary>
    [XmlIgnore]
    public virtual int? Int
    {
      get { return m_int; }
      set {
        m_int = value;
        m_string = value.ToString ();
        m_double = value;
      }
    }

    /// <summary>
    /// Double or average value in case the corresponding Field refers to a Double
    /// </summary>
    [XmlIgnore]
    public virtual double? Double
    {
      get { return m_double; }
      set {
        m_double = value;
        if (value.HasValue) {
          m_int = (int)value.Value;
        }
        else {
          m_int = null;
        }
        m_string = value.ToString ();
      }
    }

    /// <summary>
    /// String, Int or Double value according to the Type property of the Field
    /// </summary>
    [XmlElement]
    public virtual object Value
    {
      get {
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
      set {
        try {
          switch (this.Field.Type) {
          case FieldType.Boolean:
            m_string = (Convert.ToBoolean (value)).ToString ();
            m_int = (Convert.ToBoolean (value)) ? 1 : 0;
            m_double = (Convert.ToBoolean (value)) ? 1.0 : 0.0;
            return;
          case FieldType.String:
            this.String = value.ToString ();
            return;
          case FieldType.Int32:
            this.Int = Convert.ToInt32 (value);
            return;
          case FieldType.Double:
            this.Double = Convert.ToDouble (value);
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
    /// Standard Deviation in case the aggregation type is Average
    /// </summary>
    [XmlIgnore]
    public virtual Nullable<double> Deviation
    {
      get { return m_deviation; }
      set { m_deviation = value; }
    }

    /// <summary>
    /// Was the CNC value interrupted ?
    /// </summary>
    [XmlAttribute ("Stopped")]
    public virtual bool Stopped
    {
      get { return m_stopped; }
      set { m_stopped = value; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected CncValue ()
    { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="field"></param>
    /// <param name="beginDateTime"></param>
    internal protected CncValue (IMachineModule machineModule, IField field, DateTime beginDateTime)
    {
      Debug.Assert (null != machineModule);
      Debug.Assert (null != field);

      this.MachineModule = machineModule;

      if (null == field) {
        log.Fatal ("CncValue: " +
                   "null field argument");
        throw new ArgumentNullException ("field");
      }
      m_field = field;

      switch (beginDateTime.Kind) {
      case DateTimeKind.Unspecified:
        log.WarnFormat ("CncValue: " +
                        "unspecified DateTimeKind for beginDateTime argument " +
                        "=> suppose it is a universal time");
        m_begin = new DateTime (beginDateTime.Ticks, DateTimeKind.Utc);
        break;
      case DateTimeKind.Utc:
        m_begin = beginDateTime;
        break;
      case DateTimeKind.Local:
        m_begin = beginDateTime.ToUniversalTime ();
        break;
      default:
        throw new Exception ("Invalid value for DateTimeKind");
      }
    }
    #endregion // Constructors

    /// <summary>
    /// <see cref="Slot.IsEmpty" />
    /// </summary>
    /// <returns></returns>
    public virtual bool IsEmpty ()
    {
      return (null == this.Value);
    }

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[CncValue {this.Id} {this.MachineModule?.ToStringIfInitialized ()} {this.Field?.ToStringIfInitialized ()} Begin={this.Begin}]";
      }
      else {
        return $"[CncValue {this.Id}]";
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public virtual void Unproxy ()
    {
      NHibernateHelper.Unproxy<IMachineModule> (ref m_machineModule);
      NHibernateHelper.Unproxy<IField> (ref m_field);
    }
  }
}
