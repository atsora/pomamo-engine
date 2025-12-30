// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;

using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Modification class to easily process and insert:
  /// <item>a machine mode (from CNC or manual)</item>
  /// <item>a default or manual reason</item>
  /// <item>a MachineObservationState</item>
  /// <item>a Reason</item>
  /// in the analysis tables.
  /// 
  /// This is not a persistent class, no database table is associated to it !
  /// </summary>
  public class ActivityReasonMachineAssociation : MachineAssociation, ICloneable
  {
    #region Members
    IMachineObservationState m_machineObservationState;
    IShift m_shift;
    IMachineMode m_machineMode;
    #endregion // Members

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    public ActivityReasonMachineAssociation (IMachine machine, UtcDateTimeRange range)
      : base (machine, range)
    {
    }

    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected ActivityReasonMachineAssociation ()
    {
      Debug.Assert (false);
    }
    #endregion // Constructors

    #region Getters / Setters
    /// <summary>
    /// <see cref="IModification"/>
    /// </summary>
    public override string ModificationType
    {
      get { return "ActivityReasonMachineAssociation"; }
    }

    /// <summary>
    /// Reference to the Machine Observation State
    /// </summary>
    public virtual IMachineObservationState MachineObservationState
    {
      get { return m_machineObservationState; }
      set
      {
        Debug.Assert (null != value);
        if (value == null) {
          log.ErrorFormat ("MachineObservationState.set: " +
                           "null value");
          throw new ArgumentNullException ();
        }

        m_machineObservationState = value;
      }
    }

    /// <summary>
    /// Reference to the Shift
    /// </summary>
    public virtual IShift Shift
    {
      get { return m_shift; }
      set
      {
        m_shift = value;
      }
    }

    /// <summary>
    /// Reference to the Machine Mode
    /// </summary>
    public IMachineMode MachineMode
    {
      get { return m_machineMode; }
      set
      {
        Debug.Assert (null != value);
        if (value == null) {
          log.ErrorFormat ("MachineMode.set: " +
                           "null value");
          throw new ArgumentNullException ();
        }

        m_machineMode = value;
      }
    }
    #endregion // Getters / Setters

    #region ICloneable implementation
    /// <summary>
    /// Make a shallow copy
    /// <see cref="ICloneable.Clone" />
    /// </summary>
    /// <returns></returns>
    public object Clone ()
    {
      return this.MemberwiseClone ();
    }
    #endregion // ICloneable implementation

    #region MachineAssociation implementation
    /// <summary>
    /// <see cref="PeriodAssociation.ConvertToSlot" />
    /// </summary>
    /// <returns></returns>
    public override TSlot ConvertToSlot<TSlot> ()
    {
      var slot =
        GenericMachineRangeSlot.Create (typeof (TSlot),
                                   this.Machine,
                                   new UtcDateTimeRange (this.Begin, this.End)) as TSlot;

      if (slot is IReasonSlot) {
        var reasonSlot =
          slot as IReasonSlot;
        reasonSlot.MachineMode = this.MachineMode;
        reasonSlot.MachineObservationState = this.MachineObservationState;
        reasonSlot.Shift = this.Shift;
        ((ReasonSlot)reasonSlot).SetNewActivitySlot ();
        return slot;
      }
      else {
        System.Diagnostics.Debug.Assert (false);
        throw new NotImplementedException ("Slot type not implemented");
      }
    }

    /// <summary>
    /// <see cref="PeriodAssociation.MergeDataWithOldSlot" />
    /// </summary>
    /// <param name="oldSlot"></param>
    /// <param name="range">merge period range</param>
    /// <returns></returns>
    public override TSlot MergeDataWithOldSlot<TSlot> (TSlot oldSlot,
                                                       UtcDateTimeRange range)
    {
      Debug.Assert (null != oldSlot);
      Debug.Assert (oldSlot is GenericMachineRangeSlot);
      Debug.Assert (this.Machine.Equals ((oldSlot as IPartitionedByMachine).Machine));

      if (oldSlot is IReasonSlot) {
        IReasonSlot oldReasonSlot = oldSlot as IReasonSlot;
        IReasonSlot newReasonSlot = (IReasonSlot)oldReasonSlot.Clone ();
        ((ReasonSlot)newReasonSlot).SetOldSlotFromModification (oldReasonSlot, this);
        newReasonSlot.MachineObservationState = this.MachineObservationState;
        newReasonSlot.Shift = this.Shift;
        newReasonSlot.MachineMode = this.MachineMode;
        return newReasonSlot as TSlot;
      }
      else {
        System.Diagnostics.Debug.Assert (false);
        log.FatalFormat ("MergeDataWithOldSlot: " +
                         "trying to merge the association with a not supported slot {0}",
                         typeof (TSlot));
        throw new ArgumentException ("Not supported machine slot");
      }
    }
    #endregion // MachineAssociation implementation

    #region Modification implementation
    /// <summary>
    /// MakeAnalysis
    /// 
    /// Not valid because this modification is always transient
    /// </summary>
    public override void MakeAnalysis ()
    {
      Debug.Assert (false);
      log.FatalFormat ("MakeAnalysis: not valid");
      throw new NotImplementedException ("ActivityReasonMachineAssociation.MakeAnalysis");
    }

    /// <summary>
    /// Apply the modification while keeping transient the modification
    /// </summary>
    public override void Apply ()
    {
      this.ProcessAssociation ();
    }

    /// <summary>
    /// Process the association itself,
    /// without updating any Modification property
    /// </summary>
    public virtual void ProcessAssociation ()
    {
      Insert<ReasonSlot, IReasonSlot, ReasonSlotDAO> (new ReasonSlotDAO ());
    }
    #endregion // Modification implementation

    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public override void Unproxy ()
    {
      // This is not a persistent class, do nothing
      throw new NotImplementedException ("Not a persistent class");
    }
  }
}
