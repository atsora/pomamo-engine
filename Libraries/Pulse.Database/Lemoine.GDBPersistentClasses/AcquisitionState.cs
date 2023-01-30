// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using Lemoine.Model;
using Lemoine.Core.Log;
using NHibernate.Type;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table AcquisitionState
  /// </summary>
  public class AcquisitionState : IAcquisitionState
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    IMachineModule m_machineModule;
    AcquisitionStateKey m_key;
    DateTime m_dateTime;
    #endregion // Members

    static ILog log = LogManager.GetLogger (typeof (AcquisitionState).FullName);

    #region Getters / Setters
    /// <summary>
    /// AcquisitionStatus Id
    /// </summary>
    public virtual int Id
    {
      get { return this.m_id; }
    }

    /// <summary>
    /// Version
    /// </summary>
    public virtual int Version
    {
      get { return m_version; }
    }

    /// <summary>
    /// Machine module
    /// </summary>
    public virtual IMachineModule MachineModule
    {
      get { return m_machineModule; }
      protected set
      {
        if (value == null) {
          log.Fatal ("MachineModule cannot be null");
          throw new ArgumentNullException ("MachineModule");
        }
        m_machineModule = value;
        log = LogManager.GetLogger (string.Format ("{0}.{1}", this.GetType ().FullName, value.Id));
      }
    }

    /// <summary>
    /// AcquisitionState key
    /// </summary>
    public virtual AcquisitionStateKey Key
    {
      get { return this.m_key; }
      protected set { this.m_key = value; } // For NHibernate
    }

    /// <summary>
    /// Acquisition datetime
    /// </summary>
    public virtual DateTime DateTime
    {
      get { return this.m_dateTime; }
      set { this.m_dateTime = value; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor only accessible for NHibernate
    /// </summary>
    protected internal AcquisitionState () {}

    /// <summary>
    /// Constructor for the model factory
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="key"></param>
    protected internal AcquisitionState (IMachineModule machineModule, AcquisitionStateKey key)
    {
      Debug.Assert (machineModule != null);

      MachineModule = machineModule;
      m_key = key;
      DateTime = DateTime.UtcNow;
    }
    #endregion // Constructors
  }

  /// <summary>
  /// Convert a AcquisitionStateKey enum to a string in database
  /// </summary>
  [Serializable]
  public class AcquisitionStateKeyType : EnumStringType
  {
    /// <summary>
    /// Constructor
    /// </summary>
    public AcquisitionStateKeyType () : base (typeof (AcquisitionStateKey))
    {
    }
  }
}
