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
  /// Factory and base class for MachineModuleSlots
  /// </summary>
  public abstract class GenericMachineModuleSlot: BeginEndSlot, ISlot
  {
    ILog log = LogManager.GetLogger(typeof (GenericMachineModuleSlot).FullName);
    
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
    /// Associated machine module
    /// </summary>
    protected IMachineModule m_machineModule;
    #endregion // Membersd

    #region Getters / Setters
    /// <summary>
    /// Reference to the machine module
    /// 
    /// Not null
    /// </summary>
    public virtual IMachineModule MachineModule {
      get { return m_machineModule; }
      protected set
      {
        Debug.Assert (null != value);
        if (value == null) {
          log.ErrorFormat ("GenericMachineModuleSlot.set: " +
                           "null value");
          throw new ArgumentNullException ();
        }
        m_machineModule = value;
        log = LogManager.GetLogger(string.Format ("{0}.{1}.{2}",
                                                  this.GetType ().FullName,
                                                  value.MonitoredMachine.Id,
                                                  value.Id));
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// The default constructor should not be used outside the assembly
    /// 
    /// This constructor must be followed by the Initialize method.
    /// </summary>
    protected GenericMachineModuleSlot ()
    {
    }
    
    /// <summary>
    /// Create a new MachineModuleSlot (factory method)
    /// </summary>
    /// <param name="type">Type of the machineModule slot to create</param>
    /// <param name="machineModule">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public static GenericMachineModuleSlot Create (Type type,
                                                   IMachineModule machineModule,
                                                   UtcDateTimeRange range)
    {
      Debug.Assert (null != machineModule);
      return (GenericMachineModuleSlot)
        type.GetConstructor (new Type[] {typeof (IMachineModule), typeof (UtcDateTimeRange)})
        .Invoke (new Object[] {machineModule, range});
    }

    /// <summary>
    /// Constructor (to override)
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="range"></param>
    protected GenericMachineModuleSlot (IMachineModule machineModule,
                                        UtcDateTimeRange range)
      : base (range)
    {
        Debug.Assert (null != machineModule);
        if (machineModule == null) {
          log.ErrorFormat ("GenericMachineModuleSlot.set: " +
                           "null value");
          throw new ArgumentNullException ();
        }
        m_machineModule = machineModule;
        log = LogManager.GetLogger(string.Format ("{0}.{1}.{2}",
                                                  this.GetType ().FullName,
                                                  machineModule.MonitoredMachine.Id,
                                                  machineModule.Id));
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
        if (this.MachineModule != null) {
          hashCode += 1000000009 * this.MachineModule.GetHashCode();
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
      var other = obj as GenericMachineModuleSlot;
      if (other == null) {
        return false;
      }

      return Bound.Equals<DateTime> (this.BeginDateTime, other.BeginDateTime)
        && object.Equals(this.MachineModule, other.MachineModule);
    }
    #endregion // Methods
  }
}
