// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Lemoine.Model;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table ProductionStateSummary
  /// 
  /// Analysis table in which is stored a summary of the used reasons by day
  /// 
  /// The associated table is partioned by monitored machine
  /// although here it is set as partioned by machine because of the getter/setter use
  /// </summary>
  [Serializable]
  public class ProductionStateSummary : IProductionStateSummary
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    IMachine m_machine;
    DateTime m_day;
    IMachineObservationState m_machineObservationState;
    IProductionState m_productionState;
    IShift m_shift;
    TimeSpan m_duration;
    #endregion // Members

    ILog log = LogManager.GetLogger (typeof (ProductionStateSummary).FullName);

    #region Constructors
    /// <summary>
    /// The default constructor is forbidden
    /// </summary>
    protected ProductionStateSummary ()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="day"></param>
    /// <param name="shift"></param>
    /// <param name="machineObservationState"></param>
    /// <param name="productionState"></param>
    public ProductionStateSummary (IMachine machine, DateTime day, IShift shift, IMachineObservationState machineObservationState, IProductionState productionState)
    {
      this.m_machine = machine;
      this.m_day = day;
      this.m_shift = shift;
      this.m_machineObservationState = machineObservationState;
      this.m_productionState = productionState;
    }
    #endregion // Constructors

    #region Getters / Setters
    /// <summary>
    /// Summary Id
    /// </summary>
    public virtual int Id
    {
      get { return this.m_id; }
    }

    /// <summary>
    /// Summary Version
    /// </summary>
    public virtual int Version
    {
      get { return this.m_version; }
    }

    /// <summary>
    /// Reference to the Machine
    /// </summary>
    public virtual IMachine Machine
    {
      get { return m_machine; }
      protected set {
        m_machine = value;
        log = LogManager.GetLogger (string.Format ("{0}.{1}",
                                                  this.GetType ().FullName,
                                                  value.Id));
      }
    }

    /// <summary>
    /// Day of the analysis (from cut-off time)
    /// </summary>
    public virtual DateTime Day
    {
      get { return m_day; }
    }

    /// <summary>
    /// Reference to the shift
    /// </summary>
    public virtual IShift Shift
    {
      get { return m_shift; }
    }

    /// <summary>
    /// Reference to the Machine Observation State
    /// </summary>
    public virtual IMachineObservationState MachineObservationState
    {
      get { return m_machineObservationState; }
    }

    /// <summary>
    /// Reference to the ProductionState
    /// </summary>
    public virtual IProductionState ProductionState
    {
      get { return m_productionState; }
    }

    /// <summary>
    /// Total time during the period
    /// </summary>
    public virtual TimeSpan Duration
    {
      get { return m_duration; }
      set { m_duration = value; }
    }
    #endregion // Getters / Setters

    #region Methods
    /// <summary>
    /// <see cref="Object.GetHashCode" />
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode ()
    {
      int hashCode = 0;
      unchecked {
        if (this.Machine != null) {
          hashCode += 1000000007 * this.Machine.GetHashCode ();
        }

        hashCode += 1000000009 * Day.GetHashCode ();
        if (null != this.Shift) {
          hashCode += 1000000011 * this.Shift.GetHashCode ();
        }
        if (this.MachineObservationState != null) {
          hashCode += 1000000021 * this.MachineObservationState.GetHashCode ();
        }

        if (this.ProductionState != null) {
          hashCode += 1000000033 * this.ProductionState.GetHashCode ();
        }
      }
      return hashCode;
    }

    /// <summary>
    /// <see cref="Object.Equals(object)" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals (object obj)
    {
      IProductionStateSummary other = obj as ProductionStateSummary;
      if (other == null) {
        return false;
      }

      return object.Equals (this.Machine, other.Machine)
        && this.Day == other.Day
        && object.Equals (this.Shift, other.Shift)
        && object.Equals (this.MachineObservationState, other.MachineObservationState)
        && object.Equals (this.ProductionState, other.ProductionState);
    }
    #endregion // Methods
  }
}
