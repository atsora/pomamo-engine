// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Xml.Serialization;

using Lemoine.Model;
using Lemoine.Core.Log;
using NHibernate.Type;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table Field
  /// 
  /// This is a new table.
  /// 
  /// A field represents a type of data that is either drawn from the CNC
  /// or from the ISO file.
  /// It allows a much more flexibility that hard-coding the different
  /// types of data in the different tables.
  /// </summary>
  [Serializable]
  public class Field: DataWithDisplayFunction, IDataWithTranslation, IField, IVersionable
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    string m_name; // Can't be empty but can be null
    string m_translationKey; // Can't be empty but can be null
    string m_code;
    string m_description;
    FieldType m_type;
    IUnit m_unit;
    StampingDataType? m_stampingDataType;
    CncDataAggregationType? m_cncDataAggregationType;
    string m_associatedClass = null;
    string m_associatedProperty = null;
    TimeSpan? m_minTime = null;
    double? m_limitDeviation = null;
    bool m_custom = true;
    bool m_active = true;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (Field).FullName);

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    internal protected Field ()
    {  }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="code"></param>
    internal protected Field (string code)
    {
      this.Code = code;
    }
    
    /// <summary>
    /// Constructor for default values
    /// </summary>
    /// <param name="id"></param>
    /// <param name="code"></param>
    internal protected Field (int id, string code)
    {
      m_id = id;
      this.Code = code;
      m_custom = false;
    }
    #endregion // Constructors
    
    #region Getters / Setters
    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] {"Id", "Code", "Name", "TranslationKey"}; }
    }
    
    /// <summary>
    /// Field ID
    /// </summary>
    [XmlAttribute("Id")]
    public virtual int Id
    {
      get { return this.m_id; }
    }
    
    /// <summary>
    /// Version
    /// </summary>
    [XmlIgnore]
    public virtual int Version
    {
      get { return this.m_version; }
    }

    /// <summary>
    /// Display for serialization
    /// </summary>
    [XmlAttribute("Display")]
    public override string Display {
      get
      {
        return base.Display;
      }
      set
      {
        base.Display = value;
      }
    }
    
    /// <summary>
    /// Name
    /// 
    /// Note an empty string is converted to null.
    /// </summary>
    [XmlAttribute("Name")]
    public virtual string Name {
      get { return m_name; }
      set { m_name = value; }
    }
    
    /// <summary>
    /// Translation key
    /// 
    /// Note an empty string is converted to null.
    /// </summary>
    [XmlAttribute("TranslationKey")]
    public virtual string TranslationKey {
      get { return m_translationKey; }
      set { m_translationKey = value; }
    }
    
    /// <summary>
    /// Translated name of the object (if applicable, else the name of the object)
    /// </summary>
    [XmlIgnore]
    public virtual string NameOrTranslation {
      get
      {
        return DataWithTranslation
          .GetTranslationFromNameTranslationKey (this.Name,
                                                 this.TranslationKey);
      }
    }

    /// <summary>
    /// Code that identifies the field (unique)
    /// </summary>
    [XmlAttribute("Code")]
    public virtual string Code {
      get { return m_code; }
      set { m_code = value; }
    }
    
    /// <summary>
    /// Description of the field (optional)
    /// </summary>
    [XmlAttribute("Description")]
    public virtual string Description {
      get { return m_description; }
      set { m_description = value; }
    }
    
    /// <summary>
    /// Type of the field value: String, Double, Int32, Boolean, ...
    /// </summary>
    //[XmlIgnore]
    [XmlAttribute("Type")]
    public virtual FieldType Type {
      get { return m_type; }
      set { m_type = value; }
    }
    
    /// <summary>
    /// Reference to the Unit table
    /// 
    /// (nullable)
    /// </summary>
    [XmlIgnore]
    public virtual IUnit Unit {
      get { return m_unit; }
      set { m_unit = value; }
    }
    
    /// <summary>
    /// In case this field is identified during the stamping phase,
    /// type of data to associate 0x1:DATA 0x2:DBFIELD
    /// </summary>
    [XmlIgnore]
    public virtual StampingDataType? StampingDataType {
      get { return m_stampingDataType; }
      set { m_stampingDataType = value; }
    }
    
    /// <summary>
    /// In case the field is drawn from the CNC,
    /// how do you store it
    /// </summary>
    [XmlIgnore]
    public virtual CncDataAggregationType? CncDataAggregationType {
      get { return m_cncDataAggregationType; }
      set { m_cncDataAggregationType = value; }
    }
    
    /// <summary>
    /// In case this field is a DBFIELD, associated persistent class
    /// </summary>
    [XmlIgnore]
    public virtual string AssociatedClass {
      get { return m_associatedClass; }
      set { m_associatedClass = value; }
    }
    
    /// <summary>
    /// In case this field is a DBFIELD, associated property of the persistent class
    /// </summary>
    [XmlIgnore]
    public virtual string AssociatedProperty {
      get { return m_associatedProperty; }
      set { m_associatedProperty = value; }
    }
    
    /// <summary>
    /// Minimum time to compute an average or a max that makes sense  (in case the CncDataAggregationType is Average or Max)
    /// </summary>
    [XmlIgnore]
    public virtual TimeSpan? MinTime {
      get { return m_minTime; }
      set { m_minTime = value; }
    }
    
    /// <summary>
    /// Limit deviation
    /// 
    /// For an average aggregation type, try to keep this maximum standard deviation in the period
    /// 
    /// For a max aggregation type, keep a deviation that is less than this deviation
    /// </summary>
    [XmlIgnore]
    public virtual double? LimitDeviation {
      get { return m_limitDeviation; }
      set { m_limitDeviation = value; }
    }
    
    /// <summary>
    /// If false, the field can't be deleted
    /// </summary>
    [XmlIgnore]
    public virtual bool Custom {
      get { return m_custom; }
    }
    
    /// <summary>
    /// Should the field be considered as active ?
    /// </summary>
    [XmlIgnore]
    public virtual bool Active {
      get { return m_active; }
      set { m_active = value; }
    }
    
    /// <summary>
    /// Text to use in a selection dialog
    /// </summary>
    [XmlIgnore]
    public virtual string SelectionText {
      get { return string.Format ("{0}: {1}",
                                  this.Id, this.Code); }
    }
    #endregion // Getters / Setters

    #region Methods
    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[Field {this.Id} Code={this.Code}]";
      }
      else {
        return $"[Field {this.Id}]";
      }
    }
    #endregion
    
    #region Lemoine.Model.ISerializableModel implementation
    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public virtual void Unproxy ()
    {
      // Do nothing here for the moment
    }
    #endregion // Lemoine.Model.ISerializableModel implementation
    
    /// <summary>
    /// <see cref="Object.GetHashCode" />
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// <see cref="Object.Equals(object)" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
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
      Field other = obj as Field;
      if (null == other) {
        return false;
      }
      if (this.Id != 0) {
        return (other.Id == this.Id);
      }
      return false;
    }
  }
  
  /// <summary>
  /// Convert a FieldType enum to a string in database
  /// </summary>
  [Serializable]
  public class EnumFieldType: EnumStringType
  {
    /// <summary>
    /// Constructor
    /// </summary>
    public EnumFieldType(): base (typeof (FieldType))
    {
    }
  }
  
}
