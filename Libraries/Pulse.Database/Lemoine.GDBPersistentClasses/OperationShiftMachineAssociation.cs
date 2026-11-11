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
  /// Modification class to easily process and insert any operation/shift pair
  /// in the analysis tables.
  /// 
  /// This is not a persistent class, no database table is associated to it !
  /// </summary>
  internal class OperationShiftMachineAssociation: MachineAssociation
  {
    IWorkOrder m_workOrder = null;
    ILine m_line = null;
    IManufacturingOrder m_manufacturingOrder = null;
    IComponent m_component = null;
    IOperation m_operation;
    DateTime? m_day = null;
    IShift m_shift;
    OperationMachineAssociation m_operationMachineAssociation;
    bool m_partOfDetectionAnalysis;
    bool m_resetFutureWorkOrderManufacturingOrder = false;

    /// <summary>
    /// <see cref="IModification"/>
    /// </summary>
    public override string ModificationType
    {
      get { return "OperationShiftMachineAssociation"; }
    }

    /// <summary>
    /// Reference to the Operation
    /// </summary>
    public virtual IOperation Operation {
      get { return m_operation; }
      set { m_operation = value; }
    }

    /// <summary>
    /// Reference to the shift day
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
    /// Determined work order from the operation
    /// </summary>
    internal protected virtual IWorkOrder WorkOrder {
      get { return m_workOrder; }
      set { m_workOrder = value; }
    }
    
    /// <summary>
    /// Determined line from the operation
    /// </summary>
    internal protected virtual ILine Line {
      get { return m_line; }
      set { m_line = value; }
    }
    
    /// <summary>
    /// Determined manufacturing order from the operation
    /// </summary>
    internal protected virtual IManufacturingOrder ManufacturingOrder {
      get { return m_manufacturingOrder; }
      set { m_manufacturingOrder = value; }
    }
    
    /// <summary>
    /// Determined component from the operation
    /// </summary>
    internal protected virtual IComponent Component {
      get { return m_component; }
      set { m_component = value; }
    }
    
    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="operationMachineAssociation">not null</param>
    /// <param name="partOfDetectionAnalysis"></param>
    internal OperationShiftMachineAssociation (IMachine machine, UtcDateTimeRange range,
                                               OperationMachineAssociation operationMachineAssociation,
                                               bool partOfDetectionAnalysis)
      : base (machine, range, operationMachineAssociation.MainModification)
    {
      Debug.Assert (null != operationMachineAssociation);
      Debug.Assert ((null == operationMachineAssociation.MainModification) || (false == partOfDetectionAnalysis));
      if ((null != operationMachineAssociation.MainModification) && partOfDetectionAnalysis) {
        log.Fatal ($"OperationMachineAssociation: incompatible arguments mainModification={operationMachineAssociation.MainModification} and partOfDetectionAnalysis={partOfDetectionAnalysis}");
      }
      
      base.Option = operationMachineAssociation.Option;
      m_workOrder = operationMachineAssociation.WorkOrder;
      m_component = operationMachineAssociation.Component;
      m_operation = operationMachineAssociation.Operation;
      m_line = operationMachineAssociation.Line;
      m_manufacturingOrder = operationMachineAssociation.ManufacturingOrder;
      
      m_operationMachineAssociation = operationMachineAssociation;
      m_partOfDetectionAnalysis = partOfDetectionAnalysis;
    }
    #endregion // Constructors
    
    /// <summary>
    /// Add an analysis log for this modification
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    protected override void AddAnalysisLog (LogLevel level,
                                            string message)
    {
      if (m_partOfDetectionAnalysis) {
        if (log.IsDebugEnabled) {
          log.Debug ($"AddAnalysisLog: add a detection analysis log with the message {message} and level {level}");
        }
        IDetectionAnalysisLog detectionAnalysisLog = ModelDAOHelper.ModelFactory
          .CreateDetectionAnalysisLog (level, message, this.Machine);
        ModelDAOHelper.DAOFactory.DetectionAnalysisLogDAO
          .MakePersistent (detectionAnalysisLog);
      }
      else {
        base.AddAnalysisLog (level, message);
      }
    }

    /// <summary>
    /// <see cref="PeriodAssociation.ConvertToSlot" />
    /// </summary>
    /// <returns></returns>
    public override TSlot ConvertToSlot<TSlot> ()
    {
      if (typeof (TSlot).Equals (typeof (OperationSlot))) {
        IOperationSlot operationSlot = ModelDAO.ModelDAOHelper.ModelFactory
          .CreateOperationSlot (this.Machine,
                                this.Operation,
                                this.Component,
                                this.WorkOrder,
                                this.Line,
                                this.ManufacturingOrder,
                                this.Day,
                                this.Shift,
                                this.Range);
        return (TSlot) operationSlot;
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
    public override TSlot MergeDataWithOldSlot<TSlot>(TSlot oldSlot,
                                                      UtcDateTimeRange range)
    {
      Debug.Assert (null != oldSlot);
      // disable once CSharpWarnings::CS0183
      Debug.Assert (oldSlot is Slot);
      Debug.Assert (null != (oldSlot as IPartitionedByMachine).Machine);
      Debug.Assert (null != this.Machine);
      Debug.Assert (object.Equals (this.Machine, (oldSlot as IPartitionedByMachine).Machine));
      
      if (oldSlot is OperationSlot) {
        IOperationSlot oldOperationSlot = oldSlot as OperationSlot;
        
        IOperationSlot newOperationSlot = (IOperationSlot) oldOperationSlot.Clone ();
        ((OperationSlot)newOperationSlot).Operation = this.Operation;
        
        if (!object.Equals (newOperationSlot.Day, this.Day)) {
          if (newOperationSlot.Day.HasValue && this.Day.HasValue) { // Log it only if a real value changed
            log.Warn ($"MergeDataWithOldSlot: in OperationShiftMachineAssociation, the day is requested to be changed from {newOperationSlot.Day} to {this.Day}");
            AddAnalysisLog (LogLevel.WARN,
                            string.Format ("OperationShiftMachineAssociation: " +
                                           "day change not valid"));
          }
          ((OperationSlot)newOperationSlot).Day = this.Day;
        }
        if (!object.Equals (newOperationSlot.Shift, this.Shift)) {
          if ( (null != newOperationSlot.Shift) && (null != this.Shift)) { // Log it only if a real value changed
            log.Warn ($"MergeDataWithOldSlot: in OperationShiftMachineAssociation, the shift is requested to be changed from {newOperationSlot.Shift} to {this.Shift}");
            AddAnalysisLog (LogLevel.WARN,
                            string.Format ("OperationShiftMachineAssociation: " +
                                           "shift change not valid"));
          }
          ((OperationSlot)newOperationSlot).Shift = this.Shift;
        }
        
        // If the old work order is compatible with the new operation,
        // keep it ! Else reset it
        Debug.Assert (object.Equals (oldOperationSlot.WorkOrder,
                                     newOperationSlot.WorkOrder));
        bool resetFutureWorkOrder = false;
        if (null != m_workOrder) {
          ((OperationSlot)newOperationSlot).WorkOrder = m_workOrder;
        }
        else if (null != oldOperationSlot.WorkOrder) {
          if (Lemoine.Business.Operation.OperationRelations.IsWorkOrderCompatibleWithOperation (oldOperationSlot.WorkOrder,
                                                                 this.Operation)) {
            Debug.Assert (null != newOperationSlot.WorkOrder);
            if (log.IsDebugEnabled) {
              log.Debug ($"MergeDataWithOldSlot: keep the old work order {oldOperationSlot.WorkOrder} which is compatible with operation {this.Operation}");
            }
          }
          else {
            ((OperationSlot)newOperationSlot).WorkOrder = null;
            AddAnalysisLog (LogLevel.INFO,
                            string.Format ("Reset the work order because " +
                                           "the new operation {0} is not compatible " +
                                           "with the old work order {1}",
                                           this.Operation,
                                           oldOperationSlot.WorkOrder));
            if (this.Option.HasValue && this.Option.Value.HasFlag (AssociationOption.Detected)) { // In case of detection, reset also any future work order
              resetFutureWorkOrder = true;
            }
          }
        }
        
        // If the old component is compatible with the new operation,
        // keep it ! Else reset it
        Debug.Assert (object.Equals (oldOperationSlot.Component,
                                     newOperationSlot.Component));
        if (null != m_component) {
          ((OperationSlot)newOperationSlot).Component = m_component;
        }
        else if (null != oldOperationSlot.Component) {
          if (!Lemoine.Business.Operation.OperationRelations.IsComponentCompatibleWithOperation (oldOperationSlot.Component,
                                                                  this.Operation)) {
            AddAnalysisLog (LogLevel.INFO,
                            string.Format ("Reset the component because " +
                                           "the new operation {0} is not compatible " +
                                           "with the old component {1}",
                                           this.Operation,
                                           oldOperationSlot.Component));
            ((OperationSlot)newOperationSlot).Component = null;
          }
          else if (!Lemoine.Business.Operation.OperationRelations.IsWorkOrderCompatibleWithComponent (newOperationSlot.WorkOrder,
                                                                       oldOperationSlot.Component)) {
            AddAnalysisLog (LogLevel.INFO,
                            string.Format ("Reset the component because " +
                                           "the new work order {0} is not compatible " +
                                           "with the old component {1}",
                                           newOperationSlot.WorkOrder,
                                           oldOperationSlot.Component));
            ((OperationSlot)newOperationSlot).Component = null;
          }
          else { // Compatible with both the work order and the operation
            Debug.Assert (null != newOperationSlot.Component);
            if (log.IsDebugEnabled) {
              log.Debug ($"MergeDataWithOldSlot: keep the old component {oldOperationSlot.Component} which is compatible with both the work order {newOperationSlot.WorkOrder} and the operation {this.Operation}");
            }
          }
        }
        
        // Line
        Debug.Assert (object.Equals (oldOperationSlot.Line,
                                     newOperationSlot.Line));
        if (null != m_line) {
          ((OperationSlot)newOperationSlot).Line = m_line;
        }
        else if (null != oldOperationSlot.Line) {
          if (Lemoine.Business.Operation.OperationRelations.IsLineCompatibleWithMachineOperation (oldOperationSlot.Line,
                                                                   this.Machine,
                                                                   this.Operation)) {
            if (log.IsDebugEnabled) {
              log.Debug ($"MergeDataWithOldSlot: keep the old line {oldOperationSlot.Line} which is compatible with {this.Operation}");
            }
          }
          else {
            ((OperationSlot)newOperationSlot).Line = null;
            AddAnalysisLog (LogLevel.INFO,
                            string.Format ("Reset the line because " +
                                           "the new operation {0} is not compatible " +
                                           "with the old line {1}",
                                           this.Operation,
                                           oldOperationSlot.Line));
          }
        }
        
        // Manufacturing order
        Debug.Assert (object.Equals (oldOperationSlot.ManufacturingOrder,
                                     newOperationSlot.ManufacturingOrder));
        bool resetFutureManufacturingOrder = false;
        if (null != m_manufacturingOrder) {
          ((OperationSlot)newOperationSlot).ManufacturingOrder = m_manufacturingOrder;
        }
        else if (null != oldOperationSlot.ManufacturingOrder) {
          if (Lemoine.Business.Operation.OperationRelations.IsManufacturingOrderCompatibleWithMachineOperation (oldOperationSlot.ManufacturingOrder,
                                                                   this.Machine,
                                                                   this.Operation)) {
            if (log.IsDebugEnabled) {
              log.Debug ($"MergeDataWithOldSlot: keep the old manufacturing order {oldOperationSlot.ManufacturingOrder} which is compatible with {this.Operation}");
            }
          }
          else {
            // Get the next manufacturing order that could match this operation
            ((OperationSlot)newOperationSlot).ManufacturingOrder = null;
            ((OperationSlot)newOperationSlot).AutoManufacturingOrder = null;
            AddAnalysisLog (LogLevel.INFO,
                            string.Format ("Reset the manufacturing order because " +
                                           "the new operation {0} is not compatible " +
                                           "with the old manufacturing order {1}",
                                           this.Operation,
                                           oldOperationSlot.ManufacturingOrder));
            if (this.Option.HasValue && this.Option.Value.HasFlag (AssociationOption.Detected)) { // In case of detection, the future manufacturing orders must be discontinued as well
              resetFutureManufacturingOrder = true;
            }
          }
        }
        
        if (this.Option.HasValue && this.Option.Value.HasFlag (AssociationOption.Detected)
            && (resetFutureManufacturingOrder || resetFutureWorkOrder)
            && this.End.HasValue
            && m_operationMachineAssociation.Range.Upper.HasValue
            && Bound.Equals<DateTime> (this.End, m_operationMachineAssociation.Range.Upper)) {
          // Note 1: Do it only for the last OperationShiftMachineAssociation of OperationMachineAssociation.
          //         For that the end of both modifications was compared
          // Note 2: For the moment, the work order and the manufacturing order must be reset in the same time.
          //         There is no modification to reset only one of them yet.
          //         And it is probably not necessary.
          m_resetFutureWorkOrderManufacturingOrder = true;
          // The process is done after the operations are updated in the Analyze method
        }
        
        return (TSlot) newOperationSlot;
      }
      else {
        System.Diagnostics.Debug.Assert (false);
        log.Fatal ($"MergeData: trying to merge the association with a not supported slot {typeof (TSlot)}");
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
    /// Apply the modification without updating the analysis status
    /// </summary>
    public override void Apply ()
    {
      Analyze ();
    }
    
    /// <summary>
    /// Insert all the slots that may correspond to this modification
    /// </summary>
    public virtual void Analyze ()
    {
      Analyze (null);
    }

    /// <summary>
    /// Insert all the slots that may correspond to this modification
    /// </summary>
    /// <param name="preFetchedOperationSlots">List of pre-fetched operation slots. The list is sorted</param>
    protected internal virtual void Analyze (IList<IOperationSlot> preFetchedOperationSlots)
    {
      // - Insert the operation/shift in OperationSlot
      if (log.IsDebugEnabled) {
        log.Debug ($"Analyze: insert the {this} operation={this.Operation} {this.Begin}-{this.End} into OperationSlot");
      }
      OperationSlotDAO operationSlotDAO = new OperationSlotDAO ();
      operationSlotDAO.Caller = this;
      this.Insert<OperationSlot, IOperationSlot, OperationSlotDAO> (operationSlotDAO,
                                                                    preFetchedOperationSlots);
      
      if (m_resetFutureWorkOrderManufacturingOrder) {
        UtcDateTimeRange resetWorkOrderRange = new UtcDateTimeRange (this.End.Value);
        IWorkOrderMachineAssociation resetWorkOrderAssociation =
          new WorkOrderMachineAssociation (this.Machine, resetWorkOrderRange,
                                           this.MainModification, this.m_partOfDetectionAnalysis);
        resetWorkOrderAssociation.Apply ();
        m_resetFutureWorkOrderManufacturingOrder = false; // Done
      }
    }
    
    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public override void Unproxy ()
    {
      // This is not a persistent class, do nothing
      throw new NotImplementedException ("This is not a persistent class");
    }
  }
}
