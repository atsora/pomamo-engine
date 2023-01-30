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
using System.Diagnostics;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table ProductionState
  /// </summary>
  [Serializable]
  public class ProductionState
    : DataWithDisplayFunction
    , IProductionState
    , IVersionable
    , IDisplayPriorityCodeNameComparerItem
    , IComparable, IComparable<IProductionState>
    , IEquatable<IProductionState>
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    string m_name; // Can't be empty but can be null
    string m_translationKey; // Can't be empty but can be null
    string m_description; // Can't be empty but can be null
    string m_descriptionTranslationKey; // Can't be empty but can be null
    string m_color;
    int? m_displayPriority;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (ProductionState).FullName);

    #region Getters / Setters
    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] { "Id", "Name", "TranslationKey" }; }
    }

    /// <summary>
    /// ProductionState ID
    /// </summary>
    [XmlAttribute ("Id")]
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
    public virtual string Name
    {
      get { return m_name; }
      set { m_name = value; }
    }

    /// <summary>
    /// Code
    /// </summary>
    [XmlIgnore]
    public virtual string Code
    {
      get { return ""; }
      set { throw new NotImplementedException (); }
    }

    /// <summary>
    /// Translation key
    /// 
    /// Note an empty string is converted to null.
    /// </summary>
    [XmlAttribute ("TranslationKey")]
    public virtual string TranslationKey
    {
      get { return m_translationKey; }
      set { m_translationKey = value; }
    }

    /// <summary>
    /// Translated name of the object (if applicable, else the name of the object)
    /// </summary>
    [XmlIgnore]
    public virtual string NameOrTranslation
    {
      get {
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
    [XmlAttribute ("Description")]
    public virtual string Description
    {
      get { return m_description; }
      set { m_description = value; }
    }

    /// <summary>
    /// Reference to a translation key for the description
    /// 
    /// Note an empty string is converted to null.
    /// </summary>
    [XmlAttribute ("DescriptionTranslationKey")]
    public virtual string DescriptionTranslationKey
    {
      get { return m_descriptionTranslationKey; }
      set { m_descriptionTranslationKey = value; }
    }

    /// <summary>
    /// Description display that is deduced from the translation table
    /// </summary>
    [XmlIgnore]
    public virtual string DescriptionOrTranslation
    {
      get {
        return DataWithTranslation
          .GetTranslationFromNameTranslationKey (this.Description,
                                                 this.DescriptionTranslationKey,
                                                 optional: true);
      }
    }

    /// <summary>
    /// Color to use when the reason group is displayed in an application
    /// 
    /// Not null
    /// </summary>
    [XmlAttribute ("Color")]
    public virtual string Color
    {
      get { return m_color; }
      set { m_color = value; }
    }

    /// <summary>
    /// Text to use in a selection dialog
    /// </summary>
    [XmlIgnore]
    public virtual string SelectionText
    {
      get {
        string selectionText = $"{this.Id}: {this.Name ?? ""}{this.TranslationKey ?? ""}";
        if (log.IsDebugEnabled) {
          log.Debug ($"SelectionText: selection text is {selectionText}");
        }
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
    internal protected ProductionState ()
    { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="color">not null or empty</param>
    internal protected ProductionState (string color)
    {
      Debug.Assert (!string.IsNullOrEmpty (color));

      m_color = color;
    }

    /// <summary>
    /// Constructor for default values
    /// </summary>
    /// <param name="id"></param>
    /// <param name="translationKey"></param>
    internal protected ProductionState (int id, string translationKey)
    {
      m_id = id;
      this.TranslationKey = translationKey;
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods

    #region Equals and GetHashCode implementation
    /// <summary>
    /// <see cref="IEquatable{T}"/>
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual bool Equals (IProductionState other)
    {
      return Equals ((object)other);
    }

    /// <summary>
    /// <see cref="Object.Equals(object)" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals (object obj)
    {
      if (object.ReferenceEquals (this, obj)) {
        return true;
      }
      if (obj is null) {
        return false;
      }

      IProductionState other = obj as ProductionState;
      if (other == null) {
        return false;
      }

      if (this.Id != 0) {
        return this.Id.Equals (other.Id)
          && this.Version.Equals (other.Version);
      }
      return false;
    }

    /// <summary>
    /// <see cref="Object.GetHashCode" />
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode ()
    {
      if (0 != Id) {
        int hashCode = 0;
        unchecked {
          hashCode += 1000000007 * Id.GetHashCode ();
          hashCode += 1000000009 * Version.GetHashCode ();
        }
        return hashCode;
      }
      else {
        int hashCode = 0;
        unchecked {
          hashCode += 1000000007 * base.GetHashCode ();
          hashCode += 1000000009 * (this.Name?.GetHashCode () ?? 0);
          hashCode += 1000000011 * (this.TranslationKey?.GetHashCode () ?? 0);
        }
        return hashCode;
      }
    }

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[ProductionState {this.Id} {this.TranslationKey ?? this.Name}]";
      }
      else {
        return $"[ProductionState {this.Id}]";
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

      var other = obj as IProductionState;
      if (other != null) {
        return new DisplayPriorityComparer<IProductionState, ProductionState> ()
          .Compare (this, other);
      }
      else {
        throw new ArgumentException ("other is not a ProductionState", "obj");
      }
    }
    #endregion // IComparable implementation
    #region IComparable<IProductionState> implementation
    /// <summary>
    /// <see cref="IComparable{T}"/>
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual int CompareTo (IProductionState other)
    {
      if (other is null) {
        return int.MinValue;
      }
      return new DisplayPriorityCodeNameComparer<IProductionState, ProductionState> ()
        .Compare (this, other);
    }

    /// <summary>
    /// <see cref="IComparer{T}"/>
    /// </summary>
    public class DisplayComparer : IComparer<IProductionState>
    {
      /// <summary>
      /// Implementation of DisplayComparer
      /// 
      /// Use the following properties to sort the ProductionStates:
      /// <item>Display priority</item>
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      /// <returns></returns>
      int IComparer<IProductionState>.Compare (IProductionState x, IProductionState y)
      {
        return (new DisplayPriorityCodeNameComparer<IProductionState, ProductionState> ())
          .Compare (x, y);
      }
    }
    #endregion // IComparable<IProductionState> implementation
  }
}
