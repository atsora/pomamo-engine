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
  /// Persistent class of table CncAlarm
  /// </summary>
  [Serializable]
  public class CncAlarm : GenericMachineModuleRangeSlot, ICncAlarm, IVersionable, Lemoine.Collections.IDataWithId
  {
    #region Members
    string m_cncInfo = "";
    string m_cncSubInfo = "";
    string m_type = "";
    string m_number = "";
    string m_message;
    IDictionary<string, object> m_properties = new Dictionary<string, object> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (CncAlarm).FullName);

    #region Getters / Setters
    /// <summary>
    /// Display name that is retrieved with a display function
    /// </summary>
    [XmlAttribute ("Display")]
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
    [XmlElement ("Severity")]
    public virtual CncAlarmSeverity XmlSerializationSeverity
    {
      get { return this.Severity as CncAlarmSeverity; }
      set { throw new InvalidOperationException ("Severity read-only - deserialization not supported"); }
    }

    /// <summary>
    /// Color that is associated to the severity
    /// </summary>
    [XmlIgnore]
    public virtual string Color
    {
      get { return (this.Severity != null) ? this.Severity.Color : null; }
    }

    /// <summary>
    /// Color for XML serialization
    /// </summary>
    [XmlAttribute ("Color")]
    public virtual string XmlSerializationColor
    {
      get { return this.Color; }
      set { throw new InvalidOperationException ("Color read-only - deserialization not supported"); }
    }

    /// <summary>
    /// Alarm Cnc Info
    /// </summary>
    [XmlIgnore]
    public virtual string CncInfo
    {
      get { return m_cncInfo; }
    }

    /// <summary>
    /// Alarm Cnc Info for XML serialization
    /// </summary>
    [XmlAttribute ("CncInfo")]
    public virtual string XmlSerializationCncInfo
    {
      get { return m_cncInfo; }
      set { throw new InvalidOperationException ("CncInfo read-only - deserialization not supported"); }
    }

    /// <summary>
    /// Alarm Cnc Sub Info
    /// </summary>
    [XmlIgnore]
    public virtual string CncSubInfo
    {
      get { return m_cncSubInfo; }
    }

    /// <summary>
    /// Alarm Cnc Sub Info for XML serialization
    /// </summary>
    [XmlAttribute ("CncSubInfo")]
    public virtual string XmlSerializationCncSubInfo
    {
      get { return m_cncSubInfo; }
      set { throw new InvalidOperationException ("CncSubInfo read-only - deserialization not supported"); }
    }

    /// <summary>
    /// Alarm Type
    /// </summary>
    [XmlIgnore]
    public virtual string Type
    {
      get { return m_type; }
    }

    /// <summary>
    /// Alarm Type for XML serialization
    /// </summary>
    [XmlAttribute ("Type")]
    public virtual string XmlSerializationType
    {
      get { return m_type; }
      set { throw new InvalidOperationException ("Type read-only - deserialization not supported"); }
    }

    /// <summary>
    /// Alarm Number
    /// </summary>
    [XmlIgnore]
    public virtual string Number
    {
      get { return m_number; }
    }

    /// <summary>
    /// Alarm Number for XML serialization
    /// </summary>
    [XmlAttribute ("Number")]
    public virtual string XmlSerializationNumber
    {
      get { return m_number; }
      set { throw new InvalidOperationException ("Number read-only - deserialization not supported"); }
    }

    /// <summary>
    /// Alarm Message
    /// </summary>
    [XmlAttribute ("Message")]
    public virtual string Message
    {
      get { return m_message; }
      set { m_message = value; }
    }

    /// <summary>
    /// Alarm properties
    /// </summary>
    [XmlIgnore]
    public virtual IDictionary<string, object> Properties
    {
      get { return m_properties; }
    }

    /// <summary>
    /// Alarm properties for xml serialization
    /// </summary>
    [XmlAttribute ("Properties")]
    public virtual string XmlSerializationProperties
    {
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
    public virtual int Priority
    {
      get {
        if (null == this.Severity) {
          return 1000;
        }
        else {
          return this.Severity.Priority;
        }
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Protected constructor with no arguments
    /// </summary>
    protected CncAlarm () : base (false) { }

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="range"></param>
    /// <param name="cncInfo"></param>
    /// <param name="cncSubInfo"></param>
    /// <param name="type"></param>
    /// <param name="number"></param>
    public CncAlarm (IMachineModule machineModule, UtcDateTimeRange range, string cncInfo, string cncSubInfo,
                    string type, string number) : base (false, machineModule, range)
    {
      m_cncInfo = cncInfo;
      m_cncSubInfo = cncSubInfo;
      m_type = type;
      m_number = number;
    }
    #endregion // Constructors

    /// <summary>
    /// Make the Cnc alarm slot longer, extend it
    /// </summary>
    /// <param name="newUpperBound"></param>
    /// <param name="inclusive"></param>
    public virtual void Extend (UpperBound<DateTime> newUpperBound, bool inclusive)
    {
      Debug.Assert (Bound.Compare<DateTime> (this.DateTimeRange.Upper, newUpperBound) <= 0);

      var newRange = new UtcDateTimeRange (this.DateTimeRange.Lower, newUpperBound, this.DateTimeRange.LowerInclusive, inclusive);
      this.UpdateDateTimeRange (newRange);
    }

    /// <summary>
    /// IComparable implementation
    /// <see cref="IComparable.CompareTo" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override int CompareTo (object obj)
    {
      if (obj is CncAlarm) {
        var other = (ICncAlarm)obj;
        return CompareTo (other);
      }

      GetLogger ().ErrorFormat ("CompareTo: " +
                               "object {0} of invalid type",
                               obj);
      throw new ArgumentException ("object is not a ICncAlarm");
    }

    /// <summary>
    /// IComparable implementation
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual int CompareTo (ICncAlarm other)
    {
      if (other.MachineModule.MonitoredMachine.Id == this.MachineModule.MonitoredMachine.Id) {
        int comparison = this.DateTimeRange.CompareTo (other.DateTimeRange);
        if (0 == comparison) {
          // Sort them based on some on the other keys
          comparison = this.Id.CompareTo (other.Id);
          if (0 == comparison) {
            comparison = this.GetHashCode ().CompareTo (other.GetHashCode ());
            if (0 == comparison) {
              var thisKey = this.CncInfo + this.CncSubInfo + this.Type + this.Number + this.Message;
              var otherKey = other.CncInfo + other.CncSubInfo + other.Type + other.Number + other.Message;
              comparison = string.Compare (thisKey, otherKey, StringComparison.Ordinal);
            }
          }
        }
        return comparison;
      }
      else {
        GetLogger ().ErrorFormat ("CompareTo: " +
                                  "trying to compare cnc alarms " +
                                  "for different machine modules {0} {1}",
                                  this, other);
        throw new ArgumentException ("Comparison of cnc alarms from different machine modules");
      }
    }

    /// <summary>
    /// Slot implementation
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool ReferenceDataEquals (ISlot obj)
    {
      var other = obj as ICncAlarm;
      if (other == null) {
        return false;
      }

      return object.Equals (this.Color, other.Color)
        && object.Equals (this.CncInfo, other.CncInfo)
        && object.Equals (this.CncSubInfo, other.CncSubInfo)
        && object.Equals (this.Type, other.Type)
        && object.Equals (this.Number, other.Number)
        && object.Equals (this.Message, other.Message);
    }

    /// <summary>
    /// <see cref="Slot.IsEmpty" />
    /// </summary>
    /// <returns></returns>
    public override bool IsEmpty ()
    {
      return this.DateTimeRange.IsEmpty ();
    }

    /// <summary>
    /// <see cref="Slot.HandleAddedSlot" />
    /// </summary>
    public override void HandleAddedSlot ()
    {
      // Normally not used, because it is not a real analysis slot
      throw new NotImplementedException ();
    }

    /// <summary>
    /// <see cref="Slot.HandleModifiedSlot" />
    /// </summary>
    /// <param name="oldSlot"></param>
    public override void HandleModifiedSlot (ISlot oldSlot)
    {
      // Normally not used, because it is not a real analysis slot
      throw new NotImplementedException ();
    }

    /// <summary>
    /// <see cref="Slot.HandleRemovedSlot" />
    /// </summary>
    public override void HandleRemovedSlot ()
    {
      // Normally not used, because it is not a real analysis slot
      throw new NotImplementedException ();
    }

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[CncAlarm {this.Id} Message={this.Message}]";
      }
      else {
        return $"[CncAlarm {this.Id}]";
      }
    }

    /// <summary>
    /// Indicates whether the current object
    /// is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals (ICncAlarm other)
    {
      return this.Equals ((object)other);
    }

    /// <summary>
    /// Determines whether the specified Object
    /// is equal to the current Object
    /// </summary>
    /// <param name="obj">The object to compare with the current object</param>
    /// <returns>true if the specified Object is equal to the current Object; otherwise, false</returns>
    public override bool Equals (object obj)
    {
      if (object.ReferenceEquals (this, obj)) {
        return true;
      }

      if (obj == null) {
        return false;
      }
      // Note: do not use here this.GetType () != obj.GetType
      //       because a Xxx may be compared with a XxxProxy
      //       which may return false although true might be returned
      ICncAlarm other = obj as CncAlarm;
      if (null == other) {
        return false;
      }
      if (this.Id != 0) {
        return (other.Id == this.Id);
      }
      return false;
    }

    /// <summary>
    /// Serves as a hash function for a particular type
    /// </summary>
    /// <returns>A hash code for the current Object</returns>
    public override int GetHashCode ()
    {
      if (0 != Id) {
        int hashCode = 0;
        unchecked {
          hashCode += 1000000007 * Id.GetHashCode ();
        }
        return hashCode;
      }
      else {
        int hashCode = 0;
        unchecked {
          hashCode += 1000000007 * base.GetHashCode ();
          hashCode += 1000000009 * (this.CncInfo ?? "").GetHashCode ();
          hashCode += 1000000021 * (this.CncSubInfo ?? "").GetHashCode ();
          hashCode += 1000000033 * (this.Type ?? "").GetHashCode ();
          hashCode += 1000000087 * (this.Number ?? "").GetHashCode ();
          hashCode += 1000000093 * (this.Message ?? "").GetHashCode ();
          hashCode += 1000000097 * Properties.GetHashCode ();
        }
        return hashCode;
      }
    }

    /// <summary>
    /// Unproxy all the properties
    /// </summary>
    public override void Unproxy ()
    {
      if (m_severity != null) {
        NHibernateHelper.Unproxy<ICncAlarmSeverity> (ref m_severity);
      }

      base.Unproxy ();
    }
  }
}
