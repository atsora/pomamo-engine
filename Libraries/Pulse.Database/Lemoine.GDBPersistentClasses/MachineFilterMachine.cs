// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table MachineFilterMachine
  /// </summary>
  [Serializable]
  public class MachineFilterMachine: MachineFilterItem, IMachineFilterMachine
  {
    IMachine m_machine;

    static readonly ILog log = LogManager.GetLogger(typeof (MachineFilterMachine).FullName);

    /// <summary>
    /// Associated Machine (not null)
    /// </summary>
    public virtual IMachine Machine
    {
      get { return m_machine; }
      protected set
      {
        Debug.Assert (null != value);
        if (null == value) {
          log.Fatal ("Machine.set: null value");
          throw new ArgumentNullException ("Machine.set");
        }
        
        m_machine = value;
      }
    }
    
    #region Constructors
    /// <summary>
    /// Default constructor for NHibernate
    /// </summary>
    protected MachineFilterMachine ()
    {
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="Machine">Not null</param>
    /// <param name="rule"></param>
    internal protected MachineFilterMachine (IMachine Machine,
                                             MachineFilterRule rule)
      : base (rule)
    {
      this.Machine = Machine;
    }
    #endregion // Constructors
    
    /// <summary>
    /// <see cref="IMachineFilterItem.IsMatch"></see>
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    public override bool IsMatch (IMachine machine)
    {
      return this.Machine.Equals (machine);
    }
  }
}
