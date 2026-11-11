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
  /// Persistent class of table MachineFilterMachineSubCategory
  /// </summary>
  [Serializable]
  public class MachineFilterMachineSubCategory: MachineFilterItem, IMachineFilterMachineSubCategory
  {
    IMachineSubCategory m_machineSubCategory;

    static readonly ILog log = LogManager.GetLogger(typeof (MachineFilterMachineSubCategory).FullName);

    /// <summary>
    /// Associated MachineSubCategory (not null)
    /// </summary>
    public virtual IMachineSubCategory MachineSubCategory
    {
      get { return m_machineSubCategory; }
      protected set
      {
        Debug.Assert (null != value);
        if (null == value) {
          log.Fatal ("MachineSubCategory.set: null value");
          throw new ArgumentNullException ("MachineSubCategory.set");
        }
        
        m_machineSubCategory = value;
      }
    }
    
    #region Constructors
    /// <summary>
    /// Default constructor for NHibernate
    /// </summary>
    protected MachineFilterMachineSubCategory ()
    {
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="MachineSubCategory">Not null</param>
    /// <param name="rule"></param>
    internal protected MachineFilterMachineSubCategory (IMachineSubCategory MachineSubCategory,
                                             MachineFilterRule rule)
      : base (rule)
    {
      this.MachineSubCategory = MachineSubCategory;
    }
    #endregion // Constructors
    
    /// <summary>
    /// <see cref="IMachineFilterItem.IsMatch"></see>
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    public override bool IsMatch (IMachine machine)
    {
      return this.MachineSubCategory.Equals (machine.SubCategory);
    }
  }
}
