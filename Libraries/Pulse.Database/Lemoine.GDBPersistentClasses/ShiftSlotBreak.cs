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
  /// Persistent class of table ShiftSlotBreak
  /// </summary>
  public class ShiftSlotBreak: IShiftSlotBreak
  {
    #region Members
    int m_id = 0;
    UtcDateTimeRange m_range;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (ShiftSlotBreak).FullName);

    #region Getters / Setters
    /// <summary>
    /// ShiftSlotBreak Id
    /// </summary>
    public virtual int Id
    {
      get { return this.m_id; }
    }
    
    /// <summary>
    /// UTC Date/time range of the break
    /// </summary>
    public virtual UtcDateTimeRange Range
    {
      get { return m_range; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor for NHibernate
    /// </summary>
    protected ShiftSlotBreak ()
    { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="range">with finite bounds</param>
    internal protected ShiftSlotBreak (UtcDateTimeRange range)
    {
      Debug.Assert (range.Lower.HasValue);
      Debug.Assert (range.Upper.HasValue);
      m_range = range;
    }
    #endregion // Constructors
  }
}
