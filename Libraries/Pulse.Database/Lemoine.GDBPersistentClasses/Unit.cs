// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Xml.Serialization;

using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table Unit
  /// 
  /// This new table lists the used units and their translation. This is mainly referenced by the Field table.
  /// 
  /// You can use either the Unit Name column or the Unit Translation Key column.
  /// The other column is set to null.
  /// </summary>
  [Serializable]
  public class Unit: DataWithTranslation, IDataWithTranslation, IUnit, IVersionable
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    string m_description;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (Unit).FullName);

    #region Getters / Setters
    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] {"Id", "Name", "TranslationKey", "Description"}; }
    }
    
    /// <summary>
    /// Component type ID
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
    /// Description of the unit
    /// </summary>
    [XmlAttribute("Description")]
    public virtual string Description {
      get { return m_description; }
      set { m_description = value; }
    }

    /// <summary>
    /// Text to use in a selection dialog
    /// </summary>
    [XmlIgnore]
    public virtual string SelectionText {
      get { return string.Format ("{0}: {1}{2}",
                                  this.Id, this.Name, this.TranslationKey); }
    }
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// New unit with id, translation key and description
    /// </summary>
    /// <param name="id"></param>
    /// <param name="description"></param>
    /// <param name="translationKey"></param>
    internal protected Unit(UnitId id, string description, string translationKey)
    {
      m_id = (int)id;
      Description = description;
      TranslationKey = translationKey;
    }
    
    /// <summary>
    /// Constructor with no arguments
    /// </summary>
    internal protected Unit() {}
    #endregion // Constructors
    
    #region Equals and GetHashCode implementation
    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="obj">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public override bool Equals(object obj)
    {
      Unit other = obj as Unit;
      if (other == null) {
        return false;
      }

      return this.Id == other.Id 
        && this.Version == other.Version 
        && Object.Equals(this.Description,other.Description);
    }
    
    /// <summary>
    ///   Serves as a hash function for a particular type
    /// </summary>
    /// <returns>A hash code for the current Object</returns>
    public override int GetHashCode()
    {
      int hashCode = 0;
      unchecked {
        hashCode += 1000000007 * m_id.GetHashCode();
        hashCode += 1000000009 * m_version.GetHashCode();
        if (m_description != null) {
          hashCode += 1000000021 * m_description.GetHashCode();
        }
      }
      return hashCode;
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
  }
}
