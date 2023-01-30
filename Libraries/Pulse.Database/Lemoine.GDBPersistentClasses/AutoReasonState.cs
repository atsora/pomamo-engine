// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

using Lemoine.Model;

using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table AutoReasonState
  /// </summary>
  [Serializable]
  public class AutoReasonState : IAutoReasonState
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    IMonitoredMachine m_machine;
    string m_key;
    object m_value;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (AutoReasonState).FullName);

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected internal AutoReasonState () { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="key"></param>
    public AutoReasonState (IMonitoredMachine machine, string key)
    {
      m_machine = machine;
      m_key = key;
    }
    #endregion // Constructors

    #region Getters / Setters
    /// <summary>
    /// AutoReasonState Id
    /// </summary>
    public virtual int Id
    {
      get { return this.m_id; }
    }

    /// <summary>
    /// AutoReasonState Version
    /// </summary>
    public virtual int Version
    {
      get { return this.m_version; }
    }

    /// <summary>
    /// Associated machine
    /// </summary>
    public virtual IMonitoredMachine Machine
    {
      get { return m_machine; }
    }

    /// <summary>
    /// AutoReasonState key
    /// 
    /// Use the '.' separator to structure the key
    /// </summary>
    public virtual string Key
    {
      get { return this.m_key; }
    }

    /// <summary>
    /// AutoReasonState value, a serializable object
    /// </summary>
    public virtual object Value
    {
      get { return this.m_value; }
      set { this.m_value = value; }
    }
    #endregion // Getters / Setters
  }
}
