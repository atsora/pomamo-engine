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
  /// 
  /// </summary>
  public abstract class GenericRangeSlot: RangeSlot
  {
    static readonly ILog log = LogManager.GetLogger(typeof (GenericRangeSlot).FullName);
    
    /// <summary>
    /// <see cref="Slot.GetLogger" />
    /// </summary>
    /// <returns></returns>
    protected override ILog GetLogger()
    {
      return log;
    }
    
    #region Members
    #endregion // Members

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor (forbidden outside this library)
    /// </summary>
    /// <param name="dayColumns"></param>
    protected GenericRangeSlot (bool dayColumns)
      : base (dayColumns)
    {
    }
    
    /// <summary>
    /// Create a new Slot (factory method)
    /// </summary>
    /// <param name="type">Type of the User slot to create</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public static ISlot Create (Type type,
                                UtcDateTimeRange range)
    {
      return (ISlot)
        type.GetConstructor (System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                             null,
                             new Type[] {typeof (UtcDateTimeRange)},
                             null)
        .Invoke (new Object[] {range});
    }

    /// <summary>
    /// Constructor (to override)
    /// </summary>
    /// <param name="dayColumns"></param>
    /// <param name="range"></param>
    protected GenericRangeSlot (bool dayColumns, UtcDateTimeRange range)
      : base (dayColumns, range)
    { }
    #endregion // Constructors
    
    #region Methods
    /// <summary>
    /// <see cref="Object.GetHashCode" />
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
      int hashCode = 0;
      unchecked {
        hashCode += 1000000007 * BeginDateTime.GetHashCode();
      }
      return hashCode;
    }
    
    /// <summary>
    /// <see cref="Object.Equals(object)" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object obj)
    {
      GenericRangeSlot other = obj as GenericRangeSlot;
      if (other == null) {
        return false;
      }

      return Bound.Equals<DateTime> (this.BeginDateTime, other.BeginDateTime);
    }
    #endregion // Methods
  }
}
