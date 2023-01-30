// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Lemoine.Model;
using Lemoine.Collections;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table Revision
  /// </summary>
  [Serializable]
  public class Revision: IRevision
  {
    #region Members
    int m_id = 0;
    IUpdater m_updater;
    DateTime m_dateTime = DateTime.UtcNow;
    string m_comment;
    string m_ipAddress;
    string m_application;
    ICollection<IGlobalModification> m_globalModifications = new InitialNullIdSet<IGlobalModification, long> ();
    ICollection<IMachineModification> m_machineModifications = new InitialNullIdSet<IMachineModification, long> ();
    #endregion

    static readonly ILog log = LogManager.GetLogger(typeof (Revision).FullName);

    #region Getters / Setters
    /// <summary>
    /// Revision ID
    /// </summary>
    [XmlAttribute("Id")]
    public virtual int Id
    {
      get { return this.m_id; }
    }
    
    /// <summary>
    /// Associated updater if known
    /// </summary>
    [XmlIgnore]
    public virtual IUpdater Updater {
      get { return m_updater; }
      set { m_updater = value; }
    }
    
    /// <summary>
    /// Associated updater if known for Xml Serialization
    /// </summary>
    [XmlElement("Updater")]
    public virtual Updater XmlSerializationUpdater {
      get { return this.Updater as Updater; }
      set { this.Updater = value; }
    }
    
    /// <summary>
    /// Date/time of the revision
    /// </summary>
    [XmlIgnore]
    public virtual DateTime DateTime {
      get { return m_dateTime; }
      set { m_dateTime = value; }
    }
    
    /// <summary>
    /// Date/time of the revision in SQL string for XML serialization
    /// </summary>
    [XmlAttribute("DateTime")]
    public virtual string SqlDateTime {
      get
      {
          return this.DateTime.ToString ("yyyy-MM-dd HH:mm:ss");
      }
      set
      {
        if ( (null == value) || (0 == value.Length)) {
          this.DateTime = DateTime.UtcNow;
        }
        else {
          this.DateTime = System.DateTime.Parse (value);
        }
      }
    }
    
    /// <summary>
    /// Comment
    /// </summary>
    [XmlAttribute("Comment")]
    public virtual string Comment {
      get { return m_comment; }
      set { m_comment = value; }
    }

    /// <summary>
    /// IP Address (source of the revision)
    /// </summary>
    [XmlAttribute("IPAddress")]
    public virtual string IPAddress {
      get { return m_ipAddress; }
      set { m_ipAddress = value; }
    }
    
    /// <summary>
    /// Application name (source of the revision)
    /// </summary>
    [XmlAttribute("Application")]
    public virtual string Application {
      get { return m_application; }
      set { m_application = value; }
    }
    
    /// <summary>
    /// Associated global modifications
    /// </summary>
    [XmlIgnore]
    public virtual ICollection<IGlobalModification> GlobalModifications {
      get
      {
        return m_globalModifications;
      }
    }

    /// <summary>
    /// Associated machine modifications
    /// </summary>
    [XmlIgnore]
    public virtual ICollection<IMachineModification> MachineModifications {
      get
      {
        return m_machineModifications;
      }
    }
    
    /// <summary>
    /// Associated modifications (global and machine)
    /// </summary>
    [XmlIgnore]
    public virtual IEnumerable<IModification> Modifications {
      get
      {
        return this.GlobalModifications.Cast<IModification> ()
          .Concat (this.MachineModifications.Cast<IModification> ());
      }
    }
    #endregion // Getters / Setters
    
    #region Add methods
    /// <summary>
    /// Add a modification to the revision
    /// </summary>
    public virtual void AddModification (IModification modification)
    {
      modification.Revision = this; // Everything is done in the setter
    }
    #endregion // Add methods
  }
}
