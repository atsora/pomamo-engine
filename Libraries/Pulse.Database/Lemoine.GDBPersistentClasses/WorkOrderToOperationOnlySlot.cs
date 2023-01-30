// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Task+WorkOrder+Component+Operation only slot
  /// </summary>
  [Serializable]
  public class WorkOrderToOperationOnlySlot : IWorkOrderToOperationOnlySlot
  {
    ILog log = LogManager.GetLogger (typeof (WorkOrderToOperationOnlySlot).FullName);

    #region Members
    IMachine m_machine;
    ITask m_task;
    IWorkOrder m_workOrder;
    IComponent m_component;
    IOperation m_operation;
    string m_display;
    TimeSpan? m_runTime;
    int m_totalCycles;
    int m_adjustedCycles;
    int m_adjustedQuantity;
    int m_partialCycles;
    TimeSpan? m_averageCycleTime;
    TimeSpan? m_productionDuration;
    UtcDateTimeRange m_dateTimeRange;
    DayRange m_dayRange;
    #endregion // Members

    #region Constructors
    /// <summary>
    /// The default constructor is forbidden
    /// </summary>
    protected WorkOrderToOperationOnlySlot ()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="task"></param>
    /// <param name="workOrder"></param>
    /// <param name="component"></param>
    /// <param name="operation"></param>
    /// <param name="display"></param>
    /// <param name="runTime"></param>
    /// <param name="totalCycles"></param>
    /// <param name="adjustedCycles"></param>
    /// <param name="adjustedQuantity"></param>
    /// <param name="partialCycles"></param>
    /// <param name="averageCycleTime"></param>
    /// <param name="productionDuration"></param>
    /// <param name="range"></param>
    /// <param name="dayRange"></param>
    protected internal WorkOrderToOperationOnlySlot (IMachine machine,
                                                     ITask task,
                                                     IWorkOrder workOrder,
                                                     IComponent component,
                                                     IOperation operation,
                                                     string display,
                                                     TimeSpan? runTime,
                                                     int totalCycles,
                                                     int adjustedCycles,
                                                     int adjustedQuantity,
                                                     int partialCycles,
                                                     TimeSpan? averageCycleTime,
                                                     TimeSpan? productionDuration,
                                                     UtcDateTimeRange range,
                                                     DayRange dayRange)
    {
      Debug.Assert (null != machine);
      if (null == machine) {
        log.FatalFormat ("WorkOrderToOperationOnlySlot: " +
                         "null value");
        throw new ArgumentNullException ();
      }
      m_machine = machine;
      log = LogManager.GetLogger (string.Format ("{0}.{1}",
                                                this.GetType ().FullName,
                                                machine.Id));
      m_task = task;
      m_workOrder = workOrder;
      m_component = component;
      m_operation = operation;
      m_display = display;
      m_runTime = runTime;
      m_totalCycles = totalCycles;
      m_adjustedCycles = adjustedCycles;
      m_adjustedQuantity = adjustedQuantity;
      m_partialCycles = partialCycles;
      m_averageCycleTime = averageCycleTime;
      m_productionDuration = productionDuration;
      m_dateTimeRange = range;
      m_dayRange = dayRange;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="task"></param>
    /// <param name="workOrder"></param>
    /// <param name="component"></param>
    /// <param name="operation"></param>
    /// <param name="display"></param>
    /// <param name="runTime"></param>
    /// <param name="totalCycles"></param>
    /// <param name="adjustedCycles"></param>
    /// <param name="adjustedQuantity"></param>
    /// <param name="partialCycles"></param>
    /// <param name="averageCycleTime"></param>
    /// <param name="productionDuration"></param>
    /// <param name="range"></param>
    protected internal WorkOrderToOperationOnlySlot (IMachine machine,
                                                     ITask task,
                                                     IWorkOrder workOrder,
                                                     IComponent component,
                                                     IOperation operation,
                                                     string display,
                                                     TimeSpan? runTime,
                                                     int totalCycles,
                                                     int adjustedCycles,
                                                     int adjustedQuantity,
                                                     int partialCycles,
                                                     TimeSpan? averageCycleTime,
                                                     TimeSpan? productionDuration,
                                                     UtcDateTimeRange range)
      : this (machine, task, workOrder, component, operation, display,
              runTime, totalCycles, adjustedCycles, adjustedQuantity, partialCycles,
              averageCycleTime, productionDuration,
              range, ModelDAOHelper.DAOFactory.DaySlotDAO.ConvertToDayRange (range))
    {
    }

    /// <summary>
    /// Alternative constructor
    /// </summary>
    /// <param name="operationSlot"></param>
    protected internal WorkOrderToOperationOnlySlot (IOperationSlot operationSlot)
      : this (operationSlot.Machine, operationSlot.Task, operationSlot.WorkOrder, operationSlot.Component, operationSlot.Operation, operationSlot.Display,
              operationSlot.RunTime, operationSlot.TotalCycles, operationSlot.AdjustedCycles, operationSlot.AdjustedQuantity,
              operationSlot.PartialCycles, operationSlot.AverageCycleTime, operationSlot.ProductionDuration,
              operationSlot.DateTimeRange, operationSlot.DayRange)
    {
    }
    #endregion // Constructors

    #region Getters / Setters
    /// <summary>
    /// Reference to the machine
    /// </summary>
    public virtual IMachine Machine
    {
      get { return m_machine; }
    }

    /// <summary>
    /// Reference to the task
    /// </summary>
    public virtual ITask Task
    {
      get { return m_task; }
    }

    /// <summary>
    /// Reference to the work order
    /// </summary>
    public virtual IWorkOrder WorkOrder
    {
      get { return m_workOrder; }
    }

    /// <summary>
    /// Reference to the component
    /// </summary>
    public virtual IComponent Component
    {
      get { return m_component; }
    }

    /// <summary>
    /// Reference to the operation
    /// </summary>
    public virtual IOperation Operation
    {
      get { return m_operation; }
    }

    /// <summary>
    /// OperationSlot Display
    /// </summary>
    public virtual string Display
    {
      get { return m_display; }
    }

    /// <summary>
    /// Run time
    /// </summary>
    public virtual TimeSpan? RunTime
    {
      get { return m_runTime; }
    }

    /// <summary>
    /// Number of run full cycles (from begin to end) during the slot
    /// </summary>
    public virtual int TotalCycles
    {
      get { return m_totalCycles; }
    }

    /// <summary>
    /// Number of full cycles for which there is an adjusted number of intermediate work pieces.
    /// </summary>
    public virtual int AdjustedCycles
    {
      get { return m_adjustedCycles; }
    }

    /// <summary>
    /// Adjusted quantity of intermediate work pieces
    /// </summary>
    public virtual int AdjustedQuantity
    {
      get { return m_adjustedQuantity; }
    }

    /// <summary>
    /// Number of partial cycles (a begin but no end) during the slot
    /// </summary>
    public virtual int PartialCycles
    {
      get { return m_partialCycles; }
    }

    /// <summary>
    /// Average cycle time in seconds of the full cycles during the slot
    /// </summary>
    // disable once ConvertToAutoProperty
    public virtual TimeSpan? AverageCycleTime
    {
      get { return m_averageCycleTime; }
    }

    /// <summary>
    /// Production duration
    /// </summary>
    public virtual TimeSpan? ProductionDuration
    {
      get { return m_productionDuration; }
    }

    /// <summary>
    /// Date/time range of the slot
    /// </summary>
    public virtual UtcDateTimeRange DateTimeRange
    {
      get { return m_dateTimeRange; }
      protected set {
        m_dateTimeRange = value;
        m_dayRange = ModelDAOHelper.DAOFactory.DaySlotDAO.ConvertToDayRange (m_dateTimeRange);
      }
    }

    /// <summary>
    /// Day range of the slot
    /// </summary>
    public virtual DayRange DayRange
    {
      get { return m_dayRange; }
    }

    /// <summary>
    /// Duration of the slot
    /// </summary>
    public virtual TimeSpan? Duration
    {
      get { return m_dateTimeRange.Duration; }
    }
    #endregion // Getters / Setters

    /// <summary>
    /// IMergeable implementation
    /// 
    /// Check the reference data of the other item matches
    /// the data of other without considering the date/time
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual bool ReferenceDataEquals (IWorkOrderToOperationOnlySlot other)
    {
      if (other == null) {
        return false;
      }

      return object.Equals (this.Machine, other.Machine)
        && object.Equals (this.Task, other.Task)
        && object.Equals (this.WorkOrder, other.WorkOrder)
        && object.Equals (this.Component, other.Component)
        && object.Equals (this.Operation, other.Operation)
        && object.Equals (this.Display, other.Display);
    }

    #region IWithRange implementation
    /// <summary>
    /// IComparable implementation
    /// <see cref="IComparable.CompareTo" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public virtual int CompareTo (object obj)
    {
      if (obj is WorkOrderToOperationOnlySlot) {
        IWorkOrderToOperationOnlySlot other = (IWorkOrderToOperationOnlySlot)obj;
        if (other.Machine.Equals (this.Machine)) {
          return this.DateTimeRange.CompareTo (other.DateTimeRange);
        }
        else {
          log.ErrorFormat ("CompareTo: " +
                           "trying to compare slots " +
                           "for different machines {0} {1}",
                           this, other);
          throw new ArgumentException ("Comparison of slots from different machines");
        }
      }

      log.ErrorFormat ("CompareTo: " +
                       "object {0} of invalid type",
                       obj);
      throw new ArgumentException ("object is not the right slot");
    }

    /// <summary>
    /// IComparable implementation
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual int CompareTo (IWorkOrderToOperationOnlySlot other)
    {
      if (other.Machine.Equals (this.Machine)) {
        return this.DateTimeRange.CompareTo (other.DateTimeRange);
      }

      log.ErrorFormat ("CompareTo: " +
                       "trying to compare WorkOrderToOperationOnlySlots " +
                       "for different machines {0} {1}",
                       this, other);
      throw new ArgumentException ("Comparison of WorkOrderToOperationOnlySlots from different machines");
    }

    /// <summary>
    /// <see cref="Slot.IsEmpty" />
    /// </summary>
    /// <returns></returns>
    public virtual bool IsEmpty ()
    {
      return (null == this.WorkOrder) && (null == this.Component) && (null == this.Operation);
    }
    #endregion // IWithRange implementation

    #region IDisplayable implementation
    /// <summary>
    /// IDisplay implementation
    /// </summary>
    /// <param name="variant"></param>
    /// <returns></returns>
    public virtual string GetDisplay (string variant)
    {
      if (string.IsNullOrEmpty (variant)) {
        return this.Display;
      }
      else {
        log.FatalFormat ("GetDisplay: not implemented for variant {0}",
                         variant);
        throw new NotImplementedException ("OperationSlot.GetDisplay with variant " + variant);
      }
    }
    #endregion // IDisplayable implementation

    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals (IWorkOrderToOperationOnlySlot other)
    {
      return this.Equals ((object)other);
    }

    /// <summary>
    ///   Determines whether the specified Object
    ///   is equal to the current Object
    /// </summary>
    /// <param name="obj">The object to compare with the current object</param>
    /// <returns>true if the specified Object is equal to the current Object; otherwise, false</returns>
    public override bool Equals (object obj)
    {
      if (object.ReferenceEquals (this, obj)) {
        return true;
      }

      if (obj == null) {
        return false;
      }
      // Note: do not use here this.GetType () != obj.GetType
      //       because a Xxx may be compared with a XxxProxy
      //       which may return false although true might be returned
      WorkOrderToOperationOnlySlot other = obj as WorkOrderToOperationOnlySlot;
      if (null == other) {
        return false;
      }
      return object.Equals (this.Machine, other.Machine)
        && object.Equals (this.DateTimeRange, other.DateTimeRange);
    }


    /// <summary>
    ///   Serves as a hash function for a particular type
    /// </summary>
    /// <returns>A hash code for the current Object</returns>
    public override int GetHashCode ()
    {
      int hashCode = 0;
      unchecked {
        hashCode += 1000000007 * Machine.GetHashCode ();
        hashCode += 1000000009 * DateTimeRange.GetHashCode ();
      }
      return hashCode;
    }

    /// <summary>
    /// <see cref="Object.ToString" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      return $"[WorkOrderToOperationOnlySlot MachineId={this.Machine?.Id} Range={this.DateTimeRange}]";
    }
  }
}
