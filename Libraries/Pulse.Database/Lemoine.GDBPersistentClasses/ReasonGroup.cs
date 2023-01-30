// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Lemoine.Model;
using Lemoine.Collections;
using Lemoine.Core.Log;
using NHibernate;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table ReasonGroup
  /// 
  /// Group of reasons.
  /// This allows to classify the different reasons
  /// and to make some more compact charts in some reports.
  /// </summary>
  [Serializable]
  public class ReasonGroup
    : DataWithDisplayFunction
    , IReasonGroup
    , IVersionable
    , IDisplayPriorityComparerItem
  {
    static readonly string DEFAULT_COLOR = "#FFFF00";

    #region Members
    int m_id = 0;
    int m_version = 0;
    string m_name; // Can't be empty but can be null
    string m_translationKey; // Can't be empty but can be null
    string m_description; // Can't be empty but can be null
    string m_descriptionTranslationKey; // Can't be empty but can be null
    string m_color = DEFAULT_COLOR;
    string m_reportColor = DEFAULT_COLOR;
    ICollection<IReason> m_reasons = new InitialNullIdSet<IReason, int> ();
    int? m_displayPriority;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (ReasonGroup).FullName);

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
    /// Long display: reason group with its description
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
    /// Description of the reason group
    /// 
    /// Note an empty string is converted to null.
    /// </summary>
    [XmlAttribute("Description")]
    public virtual string Description {
      get { return m_description; }
      set { m_description = value; }
    }
    
    /// <summary>
    /// Reference to a translation key for the description
    /// 
    /// Note an empty string is converted to null.
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
      }
    }
    
    /// <summary>
    /// Color to use when the reason group is displayed in an application
    /// </summary>
    [XmlAttribute("Color")]
    public virtual string Color {
      get { return m_color; }
      set { m_color = value; }
    }
    
    /// <summary>
    /// Color to use when the reason group is displayed in a report
    /// </summary>
    [XmlAttribute("ReportColor")]
    public virtual string ReportColor {
      get { return m_reportColor; }
      set { m_reportColor = value; }
    }
    
    /// <summary>
    /// Set of reasons that are part of this reason group
    /// </summary>
    [XmlIgnore]
    public virtual ICollection<IReason> Reasons {
      get
      {
        return m_reasons;
      }
    }

    /// <summary>
    /// Text to use in a selection dialog
    /// </summary>
    [XmlIgnore]
    public virtual string SelectionText {
      get
      {
        string selectionText =
          string.Format ("{0}: {1}{2}",
                         this.Id,
                         (null == this.Name)?"":this.Name,
                         (null == this.TranslationKey)?"":this.TranslationKey);
        log.DebugFormat ("SelectionText: " +
                         "selection text is {0}",
                         selectionText);
        return selectionText;
      }
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
    /// Default constructor
    /// </summary>
    internal protected ReasonGroup ()
    { }
    
    /// <summary>
    /// Constructor for default values
    /// </summary>
    /// <param name="id"></param>
    /// <param name="translationKey"></param>
    internal protected ReasonGroup (int id, string translationKey)
    {
      m_id = id;
      this.TranslationKey = translationKey;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Add a reason in the member directly
    /// 
    /// To be used by the Reason class only
    /// </summary>
    /// <param name="reason"></param>
    protected internal virtual void AddReasonForInternalUse (IReason reason)
    {
      AddToProxyCollection<IReason> (m_reasons, reason);
    }
    
    /// <summary>
    /// Remove a reason in the member directly
    /// 
    /// To be used by the Reason class only
    /// </summary>
    /// <param name="reason"></param>
    protected internal virtual void RemoveReasonForInternalUse (IReason reason)
    {
      RemoveFromProxyCollection<IReason> (m_reasons, reason);
    }
    #endregion // Methods
    
    #region Equals and GetHashCode implementation
    /// <summary>
    /// <see cref="Object.Equals(object)" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object obj)
    {
      ReasonGroup other = obj as ReasonGroup;
      if (other == null) {
        return false;
      }

      return this.Id.Equals(other.Id)
        && this.Version.Equals(other.Version);
    }
    
    /// <summary>
    /// <see cref="Object.GetHashCode" />
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
      int hashCode = 0;
      unchecked {
        hashCode += 1000000007 * m_id.GetHashCode();
        hashCode += 1000000009 * m_version.GetHashCode();
      }
      return hashCode;
    }

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[ReasonGroup {this.Id} {this.TranslationKey ?? this.Name}]";
      }
      else {
        return $"[ReasonGroup {this.Id}]";
      }
    }
    #endregion


    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public virtual void Unproxy ()
    {
      // Nothing to do here
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

      var other = obj as IReasonGroup;
      if (other != null) {
        return new DisplayPriorityComparer<IReasonGroup, ReasonGroup> ()
          .Compare (this, other);
      }
      else {
        throw new ArgumentException ("other is not a ReasonGroup", "obj");
      }
    }
    #endregion // IComparable implementation
    #region IComparable<IReasonGroup> implementation
    /// <summary>
    /// <see cref="IComparable{T}"/>
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual int CompareTo (IReasonGroup other)
    {
      if (other is null) {
        return int.MinValue;
      }
      return new DisplayPriorityComparer<IReasonGroup, ReasonGroup> ()
        .Compare (this, other);
    }
    /// <summary>
    /// <see cref="IComparer{T}"/>
    /// </summary>
    public class DisplayComparer : IComparer<IReasonGroup>
    {
      /// <summary>
      /// Implementation of DisplayComparer
      /// 
      /// Use the following properties to sort the ReasonGroups:
      /// <item>Display priority</item>
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      /// <returns></returns>
      int IComparer<IReasonGroup>.Compare (IReasonGroup x, IReasonGroup y)
      {
        return (new DisplayPriorityComparer<IReasonGroup, ReasonGroup> ())
          .Compare (x, y);
      }
    }
    #endregion // IComparable<IReasonGroup> implementation
  }
}
