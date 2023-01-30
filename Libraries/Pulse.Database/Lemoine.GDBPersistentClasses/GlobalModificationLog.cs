// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Xml.Serialization;

using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table GlobalModificationLog
  /// </summary>
  [Serializable]
  public class GlobalModificationLog : Log, IGlobalModificationLog
  {
    #region Members
    IGlobalModification m_modification;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (GlobalModificationLog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Reference to the modification
    /// </summary>
    [XmlIgnore]
    public virtual IGlobalModification Modification
    {
      get { return m_modification; }
    }

    /// <summary>
    /// Reference to the modification for XML serialization
    /// </summary>
    [XmlElement ("Modification")]
    public virtual GlobalModification XmlSerializationModification
    {
      get { return this.Modification as GlobalModification; }
      set { m_modification = value; }
    }
    #endregion // Getters / Setters

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    /// <param name="modification">Can't be null</param>
    public GlobalModificationLog (LogLevel level,
                                  string message,
                                  IGlobalModification modification)
      : base (level, message)
    {
      Debug.Assert (null != modification);
      Debug.Assert (0 != ((Lemoine.Collections.IDataWithId<long>)modification).Id);

      m_modification = modification;
    }

    /// <summary>
    /// Protected constructor for NHibernate
    /// </summary>
    internal protected GlobalModificationLog ()
    {
    }

    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public override void Unproxy ()
    {
      NHibernateHelper.Unproxy<IGlobalModification> (ref m_modification);
    }
  }
}
