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
  /// Persistent class of table MachineFilterCell
  /// </summary>
  [Serializable]
  public class MachineFilterCell: MachineFilterItem, IMachineFilterCell
  {
    ICell m_cell;

    static readonly ILog log = LogManager.GetLogger(typeof (MachineFilterCell).FullName);

    /// <summary>
    /// Associated Cell (not null)
    /// </summary>
    public virtual ICell Cell
    {
      get { return m_cell; }
      protected set
      {
        Debug.Assert (null != value);
        if (null == value) {
          log.Fatal ("Cell.set: null value");
          throw new ArgumentNullException ("Cell.set");
        }
        
        m_cell = value;
      }
    }
    
    #region Constructors
    /// <summary>
    /// Default constructor for NHibernate
    /// </summary>
    protected MachineFilterCell ()
    {
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="Cell">Not null</param>
    /// <param name="rule"></param>
    internal protected MachineFilterCell (ICell Cell,
                                             MachineFilterRule rule)
      : base (rule)
    {
      this.Cell = Cell;
    }
    #endregion // Constructors
    
    /// <summary>
    /// <see cref="IMachineFilterItem.IsMatch"></see>
    /// </summary>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    public override bool IsMatch (IMachine machine)
    {
      return this.Cell.Equals (machine.Cell);
    }
  }
}
