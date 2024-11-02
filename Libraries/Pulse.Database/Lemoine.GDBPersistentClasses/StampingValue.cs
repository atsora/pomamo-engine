// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

using Lemoine.Model;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table StampingValue
  /// 
  /// This table lists the different machining parameters
  /// and various information on a sequence,
  /// that could have been taken by the Stamping.
  /// 
  /// It has been designed to be very flexible and accept almost any value,
  /// as soon as the kind of value is declared in the Field table.
  /// </summary>
  [Serializable]
  public class StampingValue: BaseData, IStampingValue, Lemoine.Collections.IDataWithId
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    ISequence m_sequence;
    IField m_field;
    string m_string;
    int? m_int;
    double? m_double;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (StampingValue).FullName);

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected StampingValue()
    { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="sequence"></param>
    /// <param name="field"></param>
    public StampingValue(ISequence sequence, IField field)
    {
      Debug.Assert (null != sequence);
      Debug.Assert (null != field);

      this.m_sequence = sequence;
      this.m_field = field;
      
      m_sequence.StampingValues.Add (this);
    }
    #endregion // Constructors
    
    #region Getters / Setters
    
    /// <summary>
    /// ID
    /// </summary>
    public virtual int Id
    {
      get { return this.m_id; }
    }
    
    /// <summary>
    /// Version
    /// </summary>
    public virtual int Version
    {
      get { return this.m_version; }
    }
    
    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] {"Sequence", "Field"}; }
    }
    
    /// <summary>
    /// Reference to the Sequence table
    /// </summary>
    [XmlIgnore]
    public virtual ISequence Sequence {
      get { return m_sequence; }
      set { m_sequence = value; }
    }
    
    /// <summary>
    /// Corresponding sequence for Xml Serialization
    /// </summary>
    [XmlElement("Sequence")]
    public virtual OpSequence XmlSerializationSequence {
      get { return this.Sequence as OpSequence; }
      set { this.Sequence = value; }
    }
    
    /// <summary>
    /// Reference to the Field table
    /// </summary>
    [XmlIgnore]
    public virtual IField Field {
      get { return m_field; }
      set { m_field = value; }
    }
    
    /// <summary>
    /// Corresponding field for Xml Serialization
    /// </summary>
    [XmlElement("Field")]
    public virtual Field XmlSerializationField {
      get { return this.Field as Field; }
      set { this.Field = value; }
    }
    
    /// <summary>
    /// String value in case the corresponding Field refers to a String
    /// </summary>
    [XmlAttribute("String")]
    public virtual string String {
      get { return m_string; }
      set
      {
        m_string = value;
        m_int = null;
        m_double = null;
      }
    }
    
    /// <summary>
    /// Int value in case the corresponding Field refers to an Int32
    /// </summary>
    [XmlElement(IsNullable=true)]
    public virtual Nullable<int> Int {
      get { return m_int; }
      set
      {
        m_int = value;
        m_string = value.ToString ();
        m_double = value;
      }
    }
    
    /// <summary>
    /// For Xml (de)Serialization of Int
    /// </summary>
    [XmlAttribute("Int")]
    public virtual int XmlSerializationInt {
      get { return m_int.Value; }
      set {
        m_int = value;
        m_string = value.ToString();
        m_double = value;
      }
    }

    /// <summary>
    /// used to serialize Int only when not null
    /// </summary>
    public virtual bool XmlSerializationIntSpecified{ get { return m_int.HasValue; } }
    
    /// <summary>
    /// Double value in case the corresponding Field refers to a Double
    /// </summary>
    [XmlElement(IsNullable=true)]
    public virtual Nullable<double> Double {
      get { return m_double; }
      set
      {
        m_double = value;
        if (value.HasValue) {
          m_int = (int) value.Value;
        }
        else {
          m_int = null;
        }
        m_string = value.ToString ();
      }
    }
    
    /// <summary>
    /// For Xml (de)Serialization of Double
    /// </summary>
    [XmlAttribute("Double")]
    public virtual double XmlSerializationDouble {
      get { return m_double.Value; }
      
      set {
        m_double = value;
        m_int = (int) value;
        m_string = value.ToString();
      }
    }
    
    /// <summary>
    /// used to serialize Double only when not null
    /// </summary>
    public virtual bool XmlSerializationDoubleSpecified{ get { return m_double.HasValue; } }
    
    #endregion // Getters / Setters
    
    #region Methods
    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals(IStampingValue other)
    {
      return this.Equals ((object) other);
    }
    
    /// <summary>
    ///   Determines whether the specified Object
    ///   is equal to the current Object
    /// </summary>
    /// <param name="obj">The object to compare with the current object</param>
    /// <returns>true if the specified Object is equal to the current Object; otherwise, false</returns>
    public override bool Equals(object obj)
    {
      if (object.ReferenceEquals(this,obj)) {
        return true;
      }

      if (obj == null) {
        return false;
      }
      // Note: do not use here this.GetType () != obj.GetType
      //       because a Xxx may be compared with a XxxProxy
      //       which may return false although true might be returned
      StampingValue other = obj as StampingValue;
      if (null == other) {
        return false;
      }
      if (this.Id != 0) {
        return (other.Id == this.Id);
      }
      return false;
    }
    
    /// <summary>
    ///   Serves as a hash function for a particular type
    /// </summary>
    /// <returns>A hash code for the current Object</returns>
    public override int GetHashCode()
    {
      if (0 != Id) {
        int hashCode = 0;
        unchecked {
          hashCode += 1000000007 * Id.GetHashCode();
        }
        return hashCode;
      }
      else {
        return base.GetHashCode ();
      }
    }
    #endregion // Methods
  }
}
