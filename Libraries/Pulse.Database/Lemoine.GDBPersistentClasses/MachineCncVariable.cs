// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

using Lemoine.Model;

using Lemoine.Core.Log;
using System.Diagnostics;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table MachineCncVariable
  /// </summary>
  public class MachineCncVariable: IMachineCncVariable, IVersionable
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    IMachineFilter m_machineFilter;
    string m_cncVariableKey;
    object m_cncVariableValue;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (MachineCncVariable).FullName);

    #region Getters / Setters
    /// <summary>
    /// MachineCncVariable Id
    /// </summary>
    public virtual int Id
    {
      get { return this.m_id; }
    }
    
    /// <summary>
    /// MachineCncVariable Version
    /// </summary>
    public virtual int Version
    {
      get { return this.m_version; }
    }

    /// <summary>
    /// Machine filter
    /// </summary>
    public virtual IMachineFilter MachineFilter
    {
      get { return m_machineFilter;  }
      set { m_machineFilter = value; }
    }

    /// <summary>
    /// 
    /// </summary>
    public virtual string CncVariableKey
    {
      get { return m_cncVariableKey; }
    }

    /// <summary>
    /// 
    /// </summary>
    public virtual object CncVariableValue
    {
      get { return m_cncVariableValue; }
    }

    /// <summary>
    /// 
    /// </summary>
    public virtual IComponent Component
    {
      get; set;
    }

    /// <summary>
    /// 
    /// </summary>
    public virtual IOperation Operation
    {
      get; set;
    }

    /// <summary>
    /// 
    /// </summary>
    public virtual ISequence Sequence
    {
      get; set;
    }

    /// <summary>
    /// 
    /// </summary>
    public virtual string[] Identifiers
    {
      get
      {
        return new string[] { "Id" };
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    internal protected MachineCncVariable ()
    { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="cncVariableKey"></param>
    /// <param name="cncVariableValue"></param>
    internal protected MachineCncVariable (string cncVariableKey, object cncVariableValue)
    {
      Debug.Assert (!string.IsNullOrEmpty (cncVariableKey));

      m_cncVariableKey = cncVariableKey;
      m_cncVariableValue = cncVariableValue;
    }
    #endregion // Constructors
  }
}
