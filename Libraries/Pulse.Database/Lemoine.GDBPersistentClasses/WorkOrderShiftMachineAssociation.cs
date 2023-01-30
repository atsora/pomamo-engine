// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.Core.Log;
using Lemoine.Business.Config;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Modification class to easily process and insert any WorkOrder/Shift pair
  /// in the analysis tables.
  /// 
  /// This is not a persistent class, no database table is associated to it !
  /// </summary>
  public class WorkOrderShiftMachineAssociation: MachineAssociation
  {
    #region Members
    IWorkOrder m_workOrder;
    ILine m_line = null;
    ITask m_task = null;
    bool? m_autoTask;
    bool? m_resetTask;
    IComponent m_component = null;
    IOperation m_operation = null;
    DateTime? m_day = null;
    IShift m_shift;
    #endregion

    #region Getters / Setters
    /// <summary>
    /// <see cref="IModification"/>
    /// </summary>
    public override string ModificationType
    {
      get { return "WorkOrderShiftMachineAssociation"; }
    }

    /// <summary>
    /// Work order to associate to a machine
    /// </summary>
    public virtual IWorkOrder WorkOrder {
      get { return m_workOrder; }
      set { m_workOrder = value; }
    }
    
    /// <summary>
    /// Line to association to a machine with a work order
    /// </summary>
    public virtual ILine Line {
      get { return m_line; }
      set { m_line = value; }
    }

    /// <summary>
    /// Task to associate to a machine with a work order
    /// </summary>
    public virtual ITask Task {
      get { return m_task; }
      set { m_task = value; }
    }
    
    /// <summary>
    /// The task was automatically set
    /// </summary>
    public virtual bool? AutoTask {
      get { return m_autoTask; }
      set { m_autoTask = value; }
    }
    
    /// <summary>
    /// Is the option to reset the task active ?
    /// </summary>
    public virtual bool? ResetTask {
      get { return m_resetTask; }
      set { m_resetTask = value; }
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
    /// Determined component from the line/work order
    /// </summary>
    internal protected virtual IComponent Component {
      get { return m_component; }
      set { m_component = value; }
    }
    
    /// <summary>
    /// Determined operation from the line/work order
    /// </summary>
    internal protected virtual IOperation Operation {
      get { return m_operation; }
      set { m_operation = value; }
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
    internal WorkOrderShiftMachineAssociation (IMachine machine, UtcDateTimeRange range, IModification mainModification)
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
        IOperationSlot operationSlot = ModelDAO.ModelDAOHelper.ModelFactory
          .CreateOperationSlot (this.Machine,
                                m_operation,
                                m_component,
                                this.WorkOrder,
                                this.Line,
                                this.Task,
                                this.Day,
                                this.Shift,
                                this.Range);
        if (m_autoTask.HasValue) {
          operationSlot.AutoTask = m_autoTask;
        }
        return (TSlot) operationSlot;
      }
      else {
        System.Diagnostics.Debug.Assert (false);
        throw new NotImplementedException ("Slot type not implemented");
      }
    }

    bool GetDataStructureOption (DataStructureConfigKey key)
    {
      return Lemoine.Info.ConfigSet
        .Get<bool> (ConfigKeys.GetDataStructureConfigKey (key));
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
      Debug.Assert (this.Machine.Equals ((oldSlot as IPartitionedByMachine).Machine));
      
      if (oldSlot is OperationSlot) {
        IOperationSlot oldOperationSlot = oldSlot as OperationSlot;
        
        IOperationSlot newOperationSlot = (IOperationSlot) oldOperationSlot.Clone ();
        ((OperationSlot)newOperationSlot).WorkOrder = this.WorkOrder;
        ((OperationSlot)newOperationSlot).Line = this.Line;
        
        if (!object.Equals (newOperationSlot.Day, this.Day)) {
          if (newOperationSlot.Day.HasValue && this.Day.HasValue) { // Log it only if a real value changed
            log.WarnFormat ("MergeDataWithOldSlot: " +
                            "in WorkOrderShiftMachineAssociation, the day is requested to be changed " +
                            "from {0} to {1}",
                            newOperationSlot.Day, this.Day);
            AddAnalysisLog (LogLevel.WARN,
                            string.Format ("WorkOrderShiftMachineAssociation: " +
                                           "day change not valid"));
          }
          ((OperationSlot)newOperationSlot).Day = this.Day;
        }
        if (!object.Equals (newOperationSlot.Shift, this.Shift)) {
          if ((null != newOperationSlot.Shift) && (null != this.Shift)) { // Log it only if a read value changed
            log.WarnFormat ("MergeDataWithOldSlot: " +
                            "in WorkOrderShiftMachineAssociation, the shift is requested to be changed " +
                            "from {0} to {1}",
                            newOperationSlot.Shift, this.Shift);
            AddAnalysisLog (LogLevel.WARN,
                            string.Format ("WorkOrderShiftMachineAssociation: " +
                                           "shift change not valid"));
          }
          ((OperationSlot)newOperationSlot).Shift = this.Shift;
        }
        
        // If the task is compatible with the new work order,
        // keep it ! Else reset it
        if (null != this.Task) {
          ((OperationSlot)newOperationSlot).Task = this.Task;
          ((OperationSlot)newOperationSlot).AutoTask = this.AutoTask;
        }
        else if (this.ResetTask.HasValue && this.ResetTask.Value) {
          ((OperationSlot)newOperationSlot).Task = null;
          ((OperationSlot)newOperationSlot).AutoTask = null;
        }
        else if (!Lemoine.Business.Operation.OperationRelations.IsTaskCompatibleWithMachineWorkOrder (oldOperationSlot.Task,
                                                                       this.Machine,
                                                                       this.WorkOrder)) {
          Debug.Assert (null != oldOperationSlot.Task);
          ((OperationSlot)newOperationSlot).Task = null;
          ((OperationSlot)newOperationSlot).AutoTask = null;
          AddAnalysisLog (LogLevel.INFO,
                          string.Format ("Reset the task because " +
                                         "the new work order {0} is not compatible " +
                                         "with the old task {1}",
                                         this.WorkOrder,
                                         oldOperationSlot.Task));
        }
        
        if (null != m_component) {
          ((OperationSlot)newOperationSlot).Component = m_component;
        }
        else {
          if (AnalysisConfigHelper.CreateWorkOrderComponentLink) { // Create automatically a link work order <-> component
            // WorkOrder is associated to a component through the operationSlot's component
            if ((null != oldOperationSlot.Component) && (null != oldOperationSlot.Component.Project)
                && (null != this.WorkOrder)
                && !(this.WorkOrder.Projects.Contains(oldOperationSlot.Component.Project))) {
              this.WorkOrder.Projects.Add(oldOperationSlot.Component.Project);
            }
          }
          else { // Do not create automatically a work order <-> component link
            // If the component is compatible with the new work order,
            // keep it ! Else reset it
            if (!Lemoine.Business.Operation.OperationRelations.IsWorkOrderCompatibleWithComponent (this.WorkOrder,
                                                                    oldOperationSlot.Component)) {
              Debug.Assert (null != this.WorkOrder);
              if (null == oldOperationSlot.Component) {
                Debug.Assert (GetDataStructureOption (DataStructureConfigKey.WorkOrderFromComponentOnly));
                log.WarnFormat ("MergeDataWithOldSlot: " +
                                "why is WorkOrderMachineAssociation used " +
                                "while data structure option WorkOrderFromComponentOnly is set ?");
              }
              else { // null != oldOperationSlot.Component
                Debug.Assert (null != oldOperationSlot.Component);
                ((OperationSlot)newOperationSlot).Component = null;
                AddAnalysisLog (LogLevel.INFO,
                                string.Format ("Reset the component " +
                                               "because the new work order {0} " +
                                               "is not compatible with the old component {1}",
                                               this.WorkOrder, oldOperationSlot.Component));
              }
            }
          }
          
          // If the component is not compatible with the new line
          // reset it
          if (!Lemoine.Business.Operation.OperationRelations.IsLineCompatibleWithComponent (this.Line,
                                                             oldOperationSlot.Component)) {
            Debug.Assert (null != oldOperationSlot.Component);
            ((OperationSlot)newOperationSlot).Component = null;
            AddAnalysisLog (LogLevel.INFO,
                            string.Format ("Reset the component " +
                                           "because the new line {0} " +
                                           "is not compatible with the old component {1}",
                                           this.Line, oldOperationSlot.Component));
          }
        }
        
        // If the old work order is compatible with the new operation,
        // keep it ! Else reset it
        if (null != m_operation) {
          ((OperationSlot)newOperationSlot).Operation = m_operation;
        }
        else if (!Lemoine.Business.Operation.OperationRelations.IsWorkOrderCompatibleWithOperation (this.WorkOrder,
                                                                     oldOperationSlot.Operation)) {
          // If the operation is compatible with the new work order,
          // keep it ! Else reset it
          Debug.Assert (null != this.WorkOrder);
          if (null == oldOperationSlot.Operation) {
            if (!this.Auto) {
              Debug.Assert (GetDataStructureOption (DataStructureConfigKey.WorkOrderFromComponentOnly));
              Debug.Assert (GetDataStructureOption (DataStructureConfigKey.ComponentFromOperationOnly));
              log.WarnFormat ("MergeDataWithOldSlot: " +
                              "why is WorkOrderMachineAssociation used " +
                              "while data structure option WorkOrderFromComponentOnly is set ?");
              AddAnalysisLog (LogLevel.WARN,
                              "WorkOrderMachineAssociation used " +
                              "while data structure option WorkOrderFromComponentOnly " +
                              "is set");
            }
          }
          else {
            Debug.Assert (null != oldOperationSlot.Operation);
            ((OperationSlot)newOperationSlot).Operation = null;
            AddAnalysisLog (LogLevel.INFO,
                            string.Format ("Reset the operation " +
                                           "because the new work order {0} " +
                                           "is not compatible with the old operation {1}",
                                           this.WorkOrder,
                                           oldOperationSlot.Operation));
          }
        }

        return (TSlot) newOperationSlot;
      }
      else {
        System.Diagnostics.Debug.Assert (false);
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
    /// Apply
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
      // - Insert the work order in OperationSlot
      OperationSlotDAO operationSlotDAO = new OperationSlotDAO ();
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
      // Not a persistent class, do nothing
      throw new NotImplementedException ("Not a persistent class");
    }
  }
}
