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
  /// Persistent class of table Updater
  /// </summary>
  [Serializable, XmlInclude(typeof(User)), XmlInclude(typeof(Service))]
  public abstract class Updater: DataWithDisplayFunction, IUpdater
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    ICollection<IRevision> m_revisions = new List<IRevision> ();
    #endregion

    static readonly ILog log = LogManager.GetLogger(typeof (Updater).FullName);

    #region Getters / Setters
    /// <summary>
    /// Updater ID
    /// </summary>
    [XmlIgnore]
    public virtual int Id
    {
      get { return this.m_id; }
      internal protected set { m_id = value; }
    }
    
    /// <summary>
    /// Version
    /// </summary>
    [XmlIgnore]
    public virtual int Version
    {
      get { return this.m_version; }
      internal protected set { m_version = value; }
    }

    /// <summary>
    /// Associated revisions
    /// </summary>
    [XmlIgnore]
    public virtual ICollection<IRevision> Revisions {
      get { return m_revisions; }
      internal protected set { m_revisions = value; }
    }
    #endregion
    
    #region Methods
    /// <summary>
    /// Cast Updater to the underlying class
    /// </summary>
    /// <returns></returns>
    public virtual T As<T>() where T: class, IUpdater
    {
      return this as T;
    }
    #endregion // Methods
    
    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public abstract void Unproxy ();
  }
}
