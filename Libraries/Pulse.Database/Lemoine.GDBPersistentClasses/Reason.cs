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
  /// Persistent class of table Reason
  /// 
  /// Reason that is associated to a period.
  /// The reason is very close to the machine mode.
  /// For each machine mode, a default reason is defined.
  /// </summary>
  [Serializable]
  public class Reason
    : DataWithDisplayFunction
    , IReason
    , IDataWithTranslation, IVersionable, Lemoine.Collections.IDataWithId
    , IDisplayPriorityComparerItem
  {
    static readonly string DEFAULT_COLOR = "#FFFF00";
    
    #region Members
    int m_id = 0;
    int m_version = 0;
    string m_name; // Can't be empty but can be null
    string m_translationKey; // Can't be empty but can be null
    string m_code;
    string m_description; // Can't be empty but can be null
    string m_descriptionTranslationKey; // Can't be empty but can be null
    string m_color = DEFAULT_COLOR;
    string m_reportColor = DEFAULT_COLOR;
    string m_customColor = null;
    string m_customReportColor = null;
    LinkDirection m_linkOperationDirection = LinkDirection.None;
    IReasonGroup m_reasonGroup;
    int? m_displayPriority;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (Reason).FullName);

    #region Getters / Setters
    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] {"Id", "Name", "TranslationKey"}; }
    }
    
    /// <summary>
    /// ReasonGroup ID
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
    /// Long display: reason with its description
    /// </summary>
    [XmlIgnore]
    public virtual string LongDisplay
    {
      get; set;
    }

    /// <summary>
    /// Name
    /// 
    /// Note an empty string is converted to null.
    /// </summary>
    [XmlAttribute ("Name")]
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
    /// Code of the reason
    /// </summary>
    [XmlAttribute("Code")]
    public virtual string Code {
      get { return m_code; }
      set { m_code = value; }
    }

    /// <summary>
    /// Description of the reason
    /// </summary>
    [XmlAttribute("Description")]
    public virtual string Description {
      get { return m_description; }
      set { m_description = value; }
    }
    
    /// <summary>
    /// Reference to a translation key for the description
    /// </summary>
    [XmlAttribute("DescriptionTranslationKey")]
    public virtual string DescriptionTranslationKey {
      get { return m_descriptionTranslationKey; }
      set { m_descriptionTranslationKey = value; }
    }
    
    /// <summary>
    /// Description display that is deduced from the translation table
    /// </summary>
    [XmlIgnore]
    public virtual string DescriptionOrTranslation {
      get
      {
        return DataWithTranslation
          .GetTranslationFromNameTranslationKey (this.Description,
                                                 this.DescriptionTranslationKey,
                                                 optional: true);
        ;
      }
    }
    
    /// <summary>
    /// Color to use when the reason is displayed in a chart or in an application
    /// </summary>
    [XmlAttribute("Color")]
    public virtual string Color {
      get { return m_color; }
      set { m_color = value; } // Not private because of NHibernate + XML Serialization
    }
    
    /// <summary>
    /// Color to use when the reason is displayed in a report
    /// </summary>
    [XmlAttribute("ReportColor")]
    public virtual string ReportColor {
      get { return m_reportColor; }
      set { m_reportColor = value; } // Not private because of NHibernate + XML Serialization
    }
    
    /// <summary>
    /// Custom color to override the reason group color
    /// </summary>
    [XmlAttribute("CustomColor")]
    public virtual string CustomColor {
      get { return m_customColor; }
      set { m_customColor = value; }
    }
    
    /// <summary>
    /// Custom report color to override the reason group report color
    /// </summary>
    [XmlAttribute("CustomReportColor")]
    public virtual string CustomReportColor {
      get { return m_customReportColor; }
      set { m_customReportColor = value; }
    }
    
    /// <summary>
    /// Does this reason imply an operation should be automatically set before or after it
    /// </summary>
    [XmlIgnore]
    public virtual LinkDirection LinkOperationDirection {
      get { return m_linkOperationDirection; }
      set { m_linkOperationDirection = value; }
    }
    
    /// <summary>
    /// Reference to its group of reason
    /// </summary>
    [XmlIgnore]
    public virtual IReasonGroup ReasonGroup {
      get { return m_reasonGroup; }
      set
      {
        if (object.Equals (m_reasonGroup, value)) {
          // Nothing to do
          return;
        }
        if ( (null != m_reasonGroup)
            && (m_reasonGroup is ReasonGroup)) {
          (m_reasonGroup as ReasonGroup)
            .RemoveReasonForInternalUse (this);
        }
        m_reasonGroup = value;
        if ( (null != m_reasonGroup)
            && (m_reasonGroup is ReasonGroup)) {
          (m_reasonGroup as ReasonGroup)
            .AddReasonForInternalUse (this);
        }
      }
    }
    
    /// <summary>
    /// Reference to its group of reason for Xml Serialization
    /// </summary>
    [XmlElement("ReasonGroup")]
    public virtual ReasonGroup XmlSerializationReasonGroup {
      get { return this.ReasonGroup as ReasonGroup; }
      set { this.ReasonGroup = value; }
    }
    
    /// <summary>
    /// Text to use in a selection dialog
    /// </summary>
    [XmlIgnore]
    public virtual string SelectionText {
      get { return string.Format ("{0}: {1}{2}",
                                  this.Id, this.Name, this.TranslationKey); }
    }

    /// <summary>
    /// Priority to use to display it in the reports or in the applications
    /// </summary>
    [XmlIgnore]
    public virtual int? DisplayPriority
    {
      get { return m_displayPriority; }
      set { m_displayPriority = value; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// The default constructor is forbidden
    /// and is only used by NHibernate
    /// </summary>
    protected Reason ()
    { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="reasonGroup"></param>
    internal protected Reason (IReasonGroup reasonGroup)
    {
      m_reasonGroup = reasonGroup;
    }
    
    /// <summary>
    /// Constructor for default values
    /// </summary>
    /// <param name="id"></param>
    /// <param name="translationKey"></param>
    /// <param name="reasonGroup"></param>
    internal protected Reason (int id, string translationKey, IReasonGroup reasonGroup)
    {
      m_id = id;
      this.TranslationKey = translationKey;
      m_reasonGroup = reasonGroup;
    }
    #endregion // Constructors    #endregion // Constructors
    
    #region Members
    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals(IReason other)
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
      Reason other = obj as Reason;
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
    
    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[Reason {this.Id} {this.TranslationKey ?? this.Name}]";
      }
      else {
        return $"[Reason {this.Id}]";
      }
    }
    #endregion // Members
    
    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public virtual void Unproxy ()
    {
      NHibernateHelper.Unproxy<IReasonGroup> (ref m_reasonGroup);
    }

    #region IComparable implementation
    /// <summary>
    /// <see cref="IComparable"/>
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public virtual int CompareTo (object obj)
    {
      if (obj == null) {
        return int.MinValue;
      }

      var other = obj as IReason;
      if (other != null) {
        return new DisplayPriorityComparer<IReason, Reason> ()
          .Compare (this, other);
      }
      else {
        throw new ArgumentException ("other is not a Reason", "obj");
      }
    }
    #endregion // IComparable implementation
    #region IComparable<IReason> implementation
    /// <summary>
    /// <see cref="IComparable{T}"/>
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual int CompareTo (IReason other)
    {
      if (other is null) {
        return int.MinValue;
      }
      return new DisplayPriorityComparer<IReason, Reason> ()
        .Compare (this, other);
    }
    /// <summary>
    /// <see cref="IComparer{T}"/>
    /// </summary>
    public class DisplayComparer : IComparer<IReason>
    {
      /// <summary>
      /// Implementation of DisplayComparer
      /// 
      /// Use the following properties to sort the reasons:
      /// <item>Display priority</item>
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      /// <returns></returns>
      int IComparer<IReason>.Compare (IReason x, IReason y)
      {
        return (new DisplayPriorityComparer<IReason, Reason> ())
          .Compare (x, y);
      }
    }
    #endregion // IComparable<IReason> implementation
  }
}
