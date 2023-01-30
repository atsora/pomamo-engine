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
  /// Persistent class of table MachineFilterCompany
  /// </summary>
  [Serializable]
  public class MachineFilterCompany: MachineFilterItem, IMachineFilterCompany
  {
    #region Members
    ICompany m_company;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (MachineFilterCompany).FullName);

    #region Getters / Setters
    /// <summary>
    /// Associated company (not null)
    /// </summary>
    public virtual ICompany Company
    {
      get { return m_company; }
      protected set
      {
        Debug.Assert (null != value);
        if (null == value) {
          log.FatalFormat ("Company.set: null value");
          throw new ArgumentNullException ("Company.set");
        }
        
        m_company = value;
      }
    }
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// Default constructor for NHibernate
    /// </summary>
    protected MachineFilterCompany ()
    {
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="company">Not null</param>
    /// <param name="rule"></param>
    internal protected MachineFilterCompany (ICompany company,
                                             MachineFilterRule rule)
      : base (rule)
    {
      this.Company = company;
    }
    #endregion // Constructors
    
    /// <summary>
    /// <see cref="IMachineFilterItem.IsMatch"></see>
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    public override bool IsMatch (IMachine machine)
    {
      return this.Company.Equals (machine.Company);
    }
  }
}
