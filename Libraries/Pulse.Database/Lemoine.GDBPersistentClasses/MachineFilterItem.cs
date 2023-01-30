// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Abstract class of a MachineFilterItem
  /// </summary>
  [Serializable]
  public abstract class MachineFilterItem: IMachineFilterItem, Lemoine.Collections.IDataWithId
  {
    #region Members
    int m_id = 0;
    int m_order = 0;
    MachineFilterRule m_rule;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (MachineFilterItem).FullName);

    #region Getters / Setters
    /// <summary>
    /// MachineFilterItem Id
    /// </summary>
    public virtual int Id
    {
      get { return this.m_id; }
      internal protected set { m_id = value; }
    }
    
    /// <summary>
    /// Order
    /// </summary>
    public virtual int Order
    {
      get { return this.m_order; }
      set { this.m_order = value; }
    }
    
    /// <summary>
    /// Rule (default is Add)
    /// </summary>
    public virtual MachineFilterRule Rule
    {
      get { return this.m_rule; }
      set { this.m_rule = value; }
    }
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// Default constructor for NHibernate
    /// </summary>
    protected MachineFilterItem ()
    {
    }
    
    /// <summary>
    /// Constructor for the children classes
    /// </summary>
    /// <param name="rule"></param>
    protected MachineFilterItem (MachineFilterRule rule)
    {
      m_rule = rule;
    }
    #endregion // Constructors
    
    /// <summary>
    /// <see cref="IMachineFilterItem.IsMatch"></see>
    /// </summary>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    public abstract bool IsMatch (IMachine machine);
  }
}
