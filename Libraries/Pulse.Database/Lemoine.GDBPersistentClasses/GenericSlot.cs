// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Lemoine.Model;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Description of UserSlot.
  /// </summary>
  public abstract class GenericSlot: BeginEndSlot, ISlot
  {
    ILog log = LogManager.GetLogger(typeof (GenericSlot).FullName);
    
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
    /// The default constructor should not be used outside the assembly
    /// 
    /// This constructor must be followed by the Initialize method.
    /// </summary>
    protected GenericSlot ()
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
        type.GetConstructor (new Type[] {typeof (UtcDateTimeRange)})
        .Invoke (new Object[] {range});
    }

    /// <summary>
    /// Constructor (to override)
    /// </summary>
    /// <param name="range"></param>
    protected GenericSlot (UtcDateTimeRange range)
      : base (range)
    {
    }
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
      GenericSlot other = obj as GenericSlot;
      if (other == null) {
        return false;
      }

      return Bound.Equals<DateTime> (this.BeginDateTime, other.BeginDateTime);
    }
    #endregion // Methods
  }
}
