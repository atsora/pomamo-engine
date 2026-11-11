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
  /// Persistent class of table MachineFilterMachineCategory
  /// </summary>
  [Serializable]
  public class MachineFilterMachineCategory: MachineFilterItem, IMachineFilterMachineCategory
  {
    IMachineCategory m_machineCategory;

    static readonly ILog log = LogManager.GetLogger(typeof (MachineFilterMachineCategory).FullName);

    /// <summary>
    /// Associated MachineCategory (not null)
    /// </summary>
    public virtual IMachineCategory MachineCategory
    {
      get { return m_machineCategory; }
      protected set
      {
        Debug.Assert (null != value);
        if (null == value) {
          log.Fatal ("MachineCategory.set: null value");
          throw new ArgumentNullException ("MachineCategory.set");
        }
        
        m_machineCategory = value;
      }
    }
    
    #region Constructors
    /// <summary>
    /// Default constructor for NHibernate
    /// </summary>
    protected MachineFilterMachineCategory ()
    {
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="MachineCategory">Not null</param>
    /// <param name="rule"></param>
    internal protected MachineFilterMachineCategory (IMachineCategory MachineCategory,
                                             MachineFilterRule rule)
      : base (rule)
    {
      this.MachineCategory = MachineCategory;
    }
    #endregion // Constructors
    
    /// <summary>
    /// <see cref="IMachineFilterItem.IsMatch"></see>
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    public override bool IsMatch (IMachine machine)
    {
      return this.MachineCategory.Equals (machine.Category);
    }
  }
}
