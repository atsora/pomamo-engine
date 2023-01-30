// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table MaintenanceLog
  /// </summary>
  [Serializable]
  public class MaintenanceLog : Log, IMaintenanceLog
  {
    #region Members
    string m_state; // do not use here the enum, just because any new state string may come from a SQL script
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (MaintenanceLog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Associated state
    /// </summary>
    [XmlAttribute ("State")]
    public virtual string State
    {
      get { return m_state; }
      set
      {
        // only XML serialization is supported: do nothing
      }
    }
    #endregion // Getters / Setters

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    /// <param name="state"></param>
    internal MaintenanceLog (LogLevel level,
                             string message,
                             MaintenanceState state)
      : base (level, message)
    {
      m_state = state.ToString ();
    }

    /// <summary>
    /// Protected constructor for NHibernate
    /// </summary>
    internal protected MaintenanceLog ()
    {
    }

    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public override void Unproxy ()
    {
    }
  }

  /// <summary>
  /// Convert a MaintenanceState enum to a string in database
  /// </summary>
  [Serializable]
  public class EnumMaintenanceState : NHibernate.Type.EnumStringType
  {
    /// <summary>
    /// Constructor
    /// </summary>
    public EnumMaintenanceState () : base (typeof (MaintenanceState))
    {
    }
  }
}
