// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Modification class to easily process and insert any component/shift pair
  /// in the analysis tables.
  /// 
  /// This is not a persistent class, no database table is associated to it !
  /// </summary>
  internal class ComponentShiftMachineAssociation: MachineAssociation
  {
    #region Members
    IComponent m_component;
    ILine m_line = null;
    IWorkOrder m_workOrder = null;
    DateTime? m_day;
    IShift m_shift;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// <see cref="IModification"/>
    /// </summary>
    public override string ModificationType
    {
      get { return "ComponentShiftMachineAssociation"; }
    }

    /// <summary>
    /// Reference to the Component persistent class
    /// </summary>
    public virtual IComponent Component {
      get { return m_component; }
      set { m_component = value; }
    }

    /// <summary>
    /// Reference to the day if applicable
    /// </summary>
    public virtual DateTime? Day {
      get { return m_day; }
      set { m_day = value; }
    }
    
    /// <summary>
    /// Reference to the Shift
    /// </summary>
    public virtual IShift Shift {
      get { return m_shift; }
      set { m_shift = value; }
    }
    
    /// <summary>
    /// Determined work order from the component
    /// </summary>
    internal protected virtual IWorkOrder WorkOrder {
      get { return m_workOrder; }
      set { m_workOrder = value; }
    }
    
    /// <summary>
    /// Determined line from the component
    /// </summary>
    internal protected virtual ILine Line {
      get { return m_line; }
      set { m_line = value; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// 
    /// You may add a mainModification reference to be used in the analysis logs
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="mainModification"></param>
    internal ComponentShiftMachineAssociation (IMachine machine, UtcDateTimeRange range, IModification mainModification)
      : base (machine, range, mainModification)
    {
    }
    #endregion // Constructors
    
    #region Methods
    /// <summary>
    /// <see cref="PeriodAssociation.ConvertToSlot" />
    /// </summary>
    /// <returns></returns>
    public override TSlot ConvertToSlot<TSlot> ()
    {
      if (typeof (TSlot).Equals (typeof (OperationSlot))) {
        IOperationSlot operationSlot =
          ModelDAOHelper.ModelFactory.CreateOperationSlot (this.Machine,
                                                           null,
                                                           this.Component,
                                                           m_workOrder,
                                                           m_line,
                                                           null,
                                                           this.Day,
                                                           this.Shift,
                                                           this.Range);
        return (TSlot) operationSlot;
      }
      else {
        System.Diagnostics.Debug.Assert (false);
        log.FatalFormat ("ConvertToSlot: " +
                         "Slot of type {0} is not implemented",
                         typeof (TSlot));
        throw new NotImplementedException ("Slot type not implemented");
      }
    }

    /// <summary>
    /// <see cref="PeriodAssociation.MergeDataWithOldSlot" />
    /// </summary>
    /// <param name="oldSlot">not null</param>
    /// <param name="range">merge period range</param>
    /// <returns></returns>
    public override TSlot MergeDataWithOldSlot<TSlot>(TSlot oldSlot,
                                                      UtcDateTimeRange range)
    {
      Debug.Assert (null != oldSlot, "Old slot is null");
      // disable once CSharpWarnings::CS0183
      Debug.Assert (oldSlot is Slot, "Old slot is not a slot");
      Debug.Assert (null != (oldSlot as IPartitionedByMachine).Machine, "Old slot is not a partitioned machine");
      Debug.Assert (null != this.Machine, "Old slot has no machine");
      Debug.Assert (this.Machine.Equals ((oldSlot as IPartitionedByMachine).Machine), "Incompatible machine in old slot");
      
      if (oldSlot is OperationSlot) {
        IOperationSlot oldOperationSlot = oldSlot as OperationSlot;

        IOperationSlot newOperationSlot = (IOperationSlot) oldOperationSlot.Clone ();
        ((OperationSlot)newOperationSlot).Component = this.Component;
        
        if (!object.Equals (newOperationSlot.Day, this.Day)) {
          if (newOperationSlot.Day.HasValue && this.Day.HasValue) { // Log it only if a real value changed
            log.WarnFormat ("MergeDataWithOldSlot: " +
                            "in ComponentShiftMachineAssociation, the day is requested to be changed " +
                            "from {0} to {1}",
                            newOperationSlot.Day, this.Day);
            AddAnalysisLog (LogLevel.WARN,
                            string.Format ("ComponentShiftMachineAssociation: " +
                                           "day change not valid"));
          }
          ((OperationSlot)newOperationSlot).Day = this.Day;
        }
        if (!object.Equals (newOperationSlot.Shift, this.Shift)) {
          if ( (null != newOperationSlot.Shift) && (null != this.Shift)) { // Log it only if a real value changed
            log.WarnFormat ("MergeDataWithOldSlot: " +
                            "in ComponentShiftMachineAssociation, the shift is requested to be changed " +
                            "from {0} to {1}",
                            newOperationSlot.Shift, this.Shift);
            AddAnalysisLog (LogLevel.WARN,
                            string.Format ("ComponentShiftMachineAssociation: " +
                                           "shift change not valid"));
          }
          ((OperationSlot)newOperationSlot).Shift = this.Shift;
        }
        
        // If the old work order is compatible with the new component,
        // keep it ! Else reset it
        if (null != m_workOrder) {
          ((OperationSlot)newOperationSlot).WorkOrder = m_workOrder;
        }
        else if ( (null != newOperationSlot.WorkOrder)
          && !Lemoine.Business.Operation.OperationRelations.IsWorkOrderCompatibleWithComponent (oldOperationSlot.WorkOrder,
            this.Component)) {
          Debug.Assert (null != oldOperationSlot.WorkOrder, "Unexpected null work order");
          ((OperationSlot)newOperationSlot).WorkOrder = null;
          AddAnalysisLog (LogLevel.INFO,
                          string.Format ("Reset the work order " +
                                         "because the new component {0} " +
                                         "is not compatible with the old work order {1}",
                                         this.Component,
                                         oldOperationSlot.WorkOrder));
        }
        
        // If the line is compatible with the new component,
        // keep it ! Else reset it
        if (null != m_line) {
          ((OperationSlot)newOperationSlot).Line = m_line;
        }
        else if ( (null != newOperationSlot.Line)
          && !Lemoine.Business.Operation.OperationRelations.IsLineCompatibleWithComponent (oldOperationSlot.Line,
            this.Component)) {
          Debug.Assert (null != oldOperationSlot.Line, "Unexpected null line");
          ((OperationSlot)newOperationSlot).Line = null;
          AddAnalysisLog (LogLevel.INFO,
                          string.Format ("Reset the line because " +
                                         "the new component {0} is not compatible " +
                                         "with the old line {1}",
                                         this.Component,
                                         oldOperationSlot.Line));
        }
        
        // If the manufacturing order is compatible with the new component,
        // keep it ! Else reset it
        if ( (null != newOperationSlot.ManufacturingOrder)
          && !Lemoine.Business.Operation.OperationRelations.IsManufacturingOrderCompatibleWithMachineComponent (oldOperationSlot.ManufacturingOrder, 
            this.Machine, this.Component)) {
          Debug.Assert (null != oldOperationSlot.ManufacturingOrder, "Unexpected null manufacturing order");
          ((OperationSlot)newOperationSlot).ManufacturingOrder = null;
          ((OperationSlot)newOperationSlot).AutoManufacturingOrder = null;
          AddAnalysisLog (LogLevel.INFO,
                          string.Format ("Reset the manufacturing order because " +
                                         "the new component {0} is not compatible " +
                                         "with the old manufacturing order {1}",
                                         this.Component,
                                         oldOperationSlot.ManufacturingOrder));
        }
        
        // If the operation is compatible with the new component,
        // keep it ! Else reset it
        if ( (null != newOperationSlot.Operation)
          && !Lemoine.Business.Operation.OperationRelations.IsComponentCompatibleWithOperation (this.Component,
                                                                                                oldOperationSlot.Operation)) {
          Debug.Assert (null != oldOperationSlot.Operation, "Unexpected null operation");
          ((OperationSlot)newOperationSlot).Operation = null;
          AddAnalysisLog (LogLevel.INFO,
                          string.Format ("Reset the operation " +
                                         "because the new component {0} " +
                                         "is not compatible with the old operation {1}",
                                         this.Component, oldOperationSlot.Operation));
        }
        
        return (TSlot) newOperationSlot;
      }
      else {
        System.Diagnostics.Debug.Assert (false, "Not a supported slot", oldSlot.GetType ().ToString ());
        log.FatalFormat ("MergeData: " +
                         "trying to merge the association with a not supported slot {0}",
                         typeof (TSlot));
        throw new ArgumentException ("Not supported machine slot");
      }
    }

    /// <summary>
    /// Make the analysis
    /// </summary>
    public override void MakeAnalysis ()
    {
      Debug.Assert (!IsAnalysisCompleted ());
      
      Analyze ();
      
      // Analysis is done
      MarkAsCompleted ("Cache/ClearDomainByMachine/OperationAssociation/" + this.Machine.Id + "?Broadcast=true");
    }

    /// <summary>
    /// Apply the modification
    /// </summary>
    public override void Apply ()
    {
      Analyze ();
    }
    
    /// <summary>
    /// Insert all the slots that may correspond to this modification
    /// </summary>
    void Analyze ()
    {
      Analyze (null);
    }

    /// <summary>
    /// Insert all the slots that may correspond to this modification
    /// </summary>
    /// <param name="preFetchedOperationSlots">List of pre-fetched operation slots. The list is sorted</param>
    protected internal virtual void Analyze (IList<IOperationSlot> preFetchedOperationSlots)
    {
      // - Insert the component/shift in OperationSlot
      log.Debug ("InsertCorrespondingSlots: => OperationSlot");
      OperationSlotDAO operationSlotDAO  = new OperationSlotDAO ();
      operationSlotDAO.Caller = this;
      this.Insert<OperationSlot, IOperationSlot, OperationSlotDAO> (operationSlotDAO,
                                                                    preFetchedOperationSlots);
    }
    #endregion // Methods
    
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
