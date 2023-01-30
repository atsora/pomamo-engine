// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Xml.Serialization;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Factory and base class for MachineModuleRangeSlots
  /// </summary>
  public abstract class GenericMachineModuleRangeSlot: RangeSlot, IPartitionedByMachineModule, Lemoine.Model.ISerializableModel
  {
    ILog log = LogManager.GetLogger(typeof (GenericMachineModuleRangeSlot).FullName);
    
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
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Reference to the machineModule
    /// </summary>
    [XmlIgnore]
    public virtual IMachineModule MachineModule {
      get { return m_machineModule; }
      protected set
      {
        Debug.Assert (null != value);
        if (value == null) {
          log.FatalFormat ("GenericMachineModuleRangeSlot.set: " +
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

    /// <summary>
    /// Machine module for XML serialization
    /// </summary>
    [XmlElement("MachineModule")]
    public virtual MachineModule XmlSerializationMachineModule {
      get { return this.MachineModule as MachineModule; }
      set { throw new InvalidOperationException ("MachineModule read-only - deserialization not supported"); }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor (forbidden outside this library)
    /// </summary>
    /// <param name="dayColumns"></param>
    protected GenericMachineModuleRangeSlot (bool dayColumns)
      : base (dayColumns)
    {
    }
    
    /// <summary>
    /// Create a new MachineModuleRangeSlot (factory method)
    /// </summary>
    /// <param name="type">Type of the machineModule slot to create</param>
    /// <param name="machineModule">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public static ISlot Create (Type type,
                                IMachineModule machineModule,
                                UtcDateTimeRange range)
    {
      Debug.Assert (null != machineModule);
      
      /* // Alternative when the constructor is not public
      return (GenericMachineModuleRangeSlot)
        type.GetConstructor (System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                             null,
                             new Type[] {typeof(IMachineModule), typeof (UtcDateTimeRange)},
                             null)
        .Invoke (new Object[] {machineModule, range});
       */
      
      return (ISlot)
        type.GetConstructor (new Type[] {typeof (IMachineModule), typeof (UtcDateTimeRange)})
        .Invoke (new Object[] {machineModule, range});
    }

    /// <summary>
    /// Constructor (to override)
    /// </summary>
    /// <param name="dayColumns"></param>
    /// <param name="machineModule">not null</param>
    /// <param name="range"></param>
    protected GenericMachineModuleRangeSlot (bool dayColumns,
                                             IMachineModule machineModule,
                                             UtcDateTimeRange range)
      : base (dayColumns, range)
    {
      Debug.Assert (null != machineModule);
      if (machineModule == null) {
        log.FatalFormat ("GenericMachineModuleRangeSlot.set: " +
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
      var other = obj as GenericMachineModuleRangeSlot;
      if (other == null) {
        return false;
      }

      return Bound.Equals<DateTime> (this.BeginDateTime, other.BeginDateTime)
        && object.Equals(this.MachineModule, other.MachineModule);
    }
    
    /// <summary>
    /// Unproxy all the properties
    /// </summary>
    public virtual void Unproxy()
    {
      NHibernateHelper.Unproxy<IMachineModule>(ref m_machineModule);
    }
    #endregion // Methods
  }
}
