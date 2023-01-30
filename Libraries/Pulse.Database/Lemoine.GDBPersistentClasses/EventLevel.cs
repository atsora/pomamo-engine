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
  /// Persistent class of table EventLevel
  /// </summary>
  [Serializable]
  public class EventLevel: DataWithTranslation, IEventLevel, IDataGridViewEventLevel
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    int m_priority;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (EventLevel).FullName);

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected EventLevel ()
    { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="priority"></param>
    public EventLevel(int priority)
    {
      this.Priority = priority;
    }

    /// <summary>
    /// Constructor for the default values
    /// </summary>
    /// <param name="id"></param>
    /// <param name="priority"></param>
    /// <param name="translationKey"></param>
    public EventLevel(int id, int priority, string translationKey)
    {
      m_id = id;
      this.Priority = priority;
      this.TranslationKey = translationKey;
    }
    #endregion // Constructors
    
    #region Getters / Setters
    /// <summary>
    /// EventLevel Id
    /// </summary>
    [XmlAttribute("Id")]
    public virtual int Id
    {
      get { return this.m_id; }
    }
    
    /// <summary>
    /// EventLevel Version
    /// </summary>
    [XmlIgnore]
    public virtual int Version
    {
      get { return this.m_version; }
    }

    /// <summary>
    /// Event level priority
    /// </summary>
    [XmlAttribute("Priority")]
    public virtual int Priority {
      get { return m_priority; }
      set { m_priority = value; }
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
    /// Text to use if Priority info is needed
    /// </summary>
    [XmlIgnore]
    public virtual string SelectionTextWithPriority {
      get { return string.Format ("{0}: {1}{2} ({3})",
                                  this.Id, this.Name, this.TranslationKey, this.Priority); }
    }
    
    #endregion // Getters / Setters
    
    #region Equals and GetHashCode implementation
    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="obj">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public override bool Equals(object obj)
    {
      EventLevel other = obj as EventLevel;
      if (other == null) {
        return false;
      }

      return this.Id == other.Id && this.Version == other.Version && this.Priority == other.Priority;
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
        hashCode += 1000000021 * Priority.GetHashCode();
      }
      return hashCode;
    }
    #endregion


    /// <summary>
    /// Unproxy the entity for XML serialization
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
        return $"[EventLevel {this.Id} {this.TranslationKey ?? this.Name}]";
      }
      else {
        return $"[EventLevel {this.Id}]";
      }
    }
    
  }
}
