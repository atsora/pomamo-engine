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
  /// Persistent class of table MachineFilterDepartment
  /// </summary>
  [Serializable]
  public class MachineFilterDepartment: MachineFilterItem, IMachineFilterDepartment
  {
    IDepartment m_department;

    static readonly ILog log = LogManager.GetLogger(typeof (MachineFilterDepartment).FullName);

    /// <summary>
    /// Associated Department (not null)
    /// </summary>
    public virtual IDepartment Department
    {
      get { return m_department; }
      protected set
      {
        Debug.Assert (null != value);
        if (null == value) {
          log.Fatal ("Department.set: null value");
          throw new ArgumentNullException ("Department.set");
        }
        
        m_department = value;
      }
    }
    
    #region Constructors
    /// <summary>
    /// Default constructor for NHibernate
    /// </summary>
    protected MachineFilterDepartment ()
    {
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="Department">Not null</param>
    /// <param name="rule"></param>
    internal protected MachineFilterDepartment (IDepartment Department,
                                             MachineFilterRule rule)
      : base (rule)
    {
      this.Department = Department;
    }
    #endregion // Constructors
    
    /// <summary>
    /// <see cref="IMachineFilterItem.IsMatch"></see>
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    public override bool IsMatch (IMachine machine)
    {
      return this.Department.Equals (machine.Department);
    }
  }
}
