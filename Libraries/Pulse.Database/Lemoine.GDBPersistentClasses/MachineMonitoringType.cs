// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of the machinemonitoringtype table
  /// 
  /// This new table lists the different monitoring types:
  /// <item>Monitored machine</item>
  /// <item>Not monitored machine</item>
  /// <item>Outsource</item>
  /// <item>Obsolete</item>
  /// </summary>
  [Serializable]
  public class MachineMonitoringType: DataWithTranslation, IMachineMonitoringType, IVersionable
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    #endregion

    static readonly ILog log = LogManager.GetLogger(typeof (MachineMonitoringType).FullName);

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
    /// Machine Monitoring Type ID
    /// </summary>
    [XmlIgnore]
    public virtual int Id
    {
      get { return this.m_id; }
    }
    
    /// <summary>
    /// Machine Monitoring Type ID for XML serialization
    /// </summary>
    [XmlAttribute("Id")]
    public virtual int XmlSerializationId
    {
      get { return this.Id; }
      set { m_id = value; }
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
    /// Text to use in a selection dialog
    /// </summary>
    [XmlIgnore]
    public virtual string SelectionText {
      get
      {
        string selectionText =
          string.Format ("{0}: {1}",
                         this.Id,
                         this.NameOrTranslation);
        log.DebugFormat ("SelectionText: " +
                         "selection text is {0}",
                         selectionText);
        return selectionText;
      }
    }
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    internal protected MachineMonitoringType ()
    { }
    
    /// <summary>
    /// Constructor for default values
    /// </summary>
    /// <param name="id"></param>
    /// <param name="translationKey"></param>
    internal protected MachineMonitoringType (int id, string translationKey)
    {
      m_id = id;
      this.TranslationKey = translationKey;
    }
    #endregion // Constructors
    
    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public virtual void Unproxy ()
    {
      // Nothing to do here for the moment
    }

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[MachineMonitoringType {this.Id} {this.TranslationKey ?? this.Name}]";
      }
      else {
        return $"[MachineMonitoringType {this.Id}]";
      }
    }
    
    #region Equals and GetHashCode implementation
    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="obj">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public override bool Equals(object obj)
    {
      MachineMonitoringType other = obj as MachineMonitoringType;
      if (other == null) {
        return false;
      }

      return this.Id == other.Id && this.Version == other.Version;
    }
    
    /// <summary>
    ///   Serves as a hash function for a particular type
    /// </summary>
    /// <returns>A hash code for the current Object</returns>
    public override int GetHashCode()
    {
      int hashCode = 0;
      unchecked {
        hashCode += 1000000007 * Id.GetHashCode();
        hashCode += 1000000009 * Version.GetHashCode();
      }
      return hashCode;
    }
    #endregion

    
    #region IReferenceData implementation
    /// <summary>
    /// <see cref="IReferenceData.IsUndefined" />
    /// </summary>
    /// <returns></returns>
    public virtual bool IsUndefined()
    {
      return (1 == this.Id);
    }
    #endregion // IReferenceData implementation
  }
}
