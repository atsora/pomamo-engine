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
  /// Persistent class of table MachineModificationLog
  /// </summary>
  [Serializable]
  public class MachineModificationLog : Log, IMachineModificationLog
  {
    #region Members
    IMachineModification m_modification;
    IMachine m_machine;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (MachineModificationLog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Reference to the modification
    /// </summary>
    [XmlIgnore]
    public virtual IMachineModification Modification
    {
      get { return m_modification; }
      set { m_modification = value; }
    }

    /// <summary>
    /// Reference to the modification for XML serialization
    /// </summary>
    [XmlElement ("Modification")]
    public virtual MachineModification XmlSerializationModification
    {
      get { return this.Modification as MachineModification; }
      set { this.Modification = value; }
    }

    /// <summary>
    /// Reference to the machine
    /// </summary>
    [XmlIgnore]
    protected internal virtual IMachine Machine
    {
      get { return m_machine; }
    }
    #endregion // Getters / Setters

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    /// <param name="modification">Can't be null</param>
    public MachineModificationLog (LogLevel level,
                                  string message,
                                  IMachineModification modification)
      : base (level, message)
    {
      Debug.Assert (null != modification);
      Debug.Assert (0 != ((Lemoine.Collections.IDataWithId<long>)modification).Id);

      m_modification = modification;
      m_machine = modification.Machine;
    }

    /// <summary>
    /// Protected constructor for NHibernate
    /// </summary>
    internal protected MachineModificationLog ()
    {
    }

    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public override void Unproxy ()
    {
      NHibernateHelper.Unproxy<IMachineModification> (ref m_modification);
      NHibernateHelper.Unproxy<IMachine> (ref m_machine);
    }
  }
}
