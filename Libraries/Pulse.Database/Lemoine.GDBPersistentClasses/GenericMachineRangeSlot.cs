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
  /// Factory and base class for MachineRangeSlots
  /// </summary>
  public abstract class GenericMachineRangeSlot: RangeSlot, IPartitionedByMachine
  {
    ILog log = LogManager.GetLogger(typeof (GenericMachineRangeSlot).FullName);
    
    /// <summary>
    /// <see cref="Slot.GetLogger()" />
    /// </summary>
    /// <returns></returns>
    protected override ILog GetLogger()
    {
      return log;
    }
    
    #region Members
    /// <summary>
    /// Associated machine
    /// </summary>
    protected IMachine m_machine;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Reference to the machine
    /// </summary>
    public virtual IMachine Machine {
      get { return m_machine; }
      protected set
      {
        Debug.Assert (null != value);
        if (value == null) {
          log.FatalFormat ("GenericMachineRangeSlot.set: " +
                           "null value");
          throw new ArgumentNullException ();
        }
        m_machine = value;
        log = LogManager.GetLogger(string.Format ("{0}.{1}",
                                                  this.GetType ().FullName,
                                                  value.Id));
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor (forbidden outside this library)
    /// </summary>
    /// <param name="dayColumns"></param>
    protected GenericMachineRangeSlot (bool dayColumns)
      : base (dayColumns)
    {
    }
    
    /// <summary>
    /// Create a new MachineRangeSlot (factory method)
    /// </summary>
    /// <param name="type">Type of the machine slot to create</param>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public static ISlot Create (Type type,
                                IMachine machine,
                                UtcDateTimeRange range)
    {
      Debug.Assert (null != machine);
      
      /* // Alternative when the constructor is not public
      return (GenericMachineRangeSlot)
        type.GetConstructor (System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                             null,
                             new Type[] {typeof(IMachine), typeof (UtcDateTimeRange)},
                             null)
        .Invoke (new Object[] {machine, range});
       */
      
      return (ISlot)
        type.GetConstructor (new Type[] {typeof (IMachine), typeof (UtcDateTimeRange)})
        .Invoke (new Object[] {machine, range});
    }

    /// <summary>
    /// Constructor (to override)
    /// </summary>
    /// <param name="dayColumns"></param>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    protected GenericMachineRangeSlot (bool dayColumns,
                                       IMachine machine,
                                       UtcDateTimeRange range)
      : base (dayColumns, range)
    {
      Debug.Assert (null != machine);
      if (machine == null) {
        log.FatalFormat ("GenericMachineRangeSlot.set: " +
                         "null value");
        throw new ArgumentNullException ();
      }
      m_machine = machine;
      log = LogManager.GetLogger(string.Format ("{0}.{1}",
                                                this.GetType ().FullName,
                                                machine.Id));
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
        if (this.Machine != null) {
          hashCode += 1000000009 * this.Machine.GetHashCode();
        }
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
      var other = obj as GenericMachineRangeSlot;
      if (other == null) {
        return false;
      }

      return Bound.Equals<DateTime> (this.BeginDateTime, other.BeginDateTime)
        && object.Equals(this.Machine, other.Machine);
    }
    #endregion // Methods
  }
}
