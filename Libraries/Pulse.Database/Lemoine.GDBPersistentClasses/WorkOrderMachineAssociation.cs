// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;

using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Business.Config;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table WorkOrderMachineAssociation
  /// 
  /// In this table is stored a new row each time a work order
  /// is associated to a machine.
  /// 
  /// It does not represent the current relation
  /// between the work order and a machining resource,
  /// but all the manual or automatic associations that are made
  /// between a work order and a machining resource.
  /// </summary>
  [Serializable]
  public class WorkOrderMachineAssociation : MachineAssociation, IWorkOrderMachineAssociation
  // Note: public else it is not serializable for the alert service
  {
    #region Members
    IWorkOrder m_workOrder;
    ILine m_line;
    IManufacturingOrder m_manufacturingOrder;
    bool? m_autoTask;
    bool? m_resetManufacturingOrder;
    IComponent m_component = null;
    IOperation m_operation = null;
    bool m_partOfDetectionAnalysis;
    #endregion

    #region Getters / Setters
    /// <summary>
    /// <see cref="IModification"/>
    /// </summary>
    [XmlIgnore]
    public override string ModificationType
    {
      get { return "WorkOrderMachineAssociation"; }
    }

    /// <summary>
    /// Work order to associate to a machine
    /// </summary>
    [XmlIgnore]
    public virtual IWorkOrder WorkOrder
    {
      get { return m_workOrder; }
      set { m_workOrder = value; }
    }

    /// <summary>
    /// Reference to the related work order for Xml Serialization
    /// </summary>
    [XmlElement ("WorkOrder")]
    public virtual WorkOrder XmlSerializationWorkOrder
    {
      get { return this.WorkOrder as WorkOrder; }
      set { this.WorkOrder = value; }
    }

    /// <summary>
    /// Line to association to a machine with a work order
    /// </summary>
    [XmlIgnore]
    public virtual ILine Line
    {
      get { return m_line; }
      set { m_line = value; }
    }

    /// <summary>
    /// Manufacturing order to associate to a machine with a work order
    /// </summary>
    [XmlIgnore]
    public virtual IManufacturingOrder ManufacturingOrder
    {
      get { return m_manufacturingOrder; }
      set { m_manufacturingOrder = value; }
    }

    /// <summary>
    /// Was the manufacturing order automatically set ?
    /// </summary>
    [XmlIgnore]
    public virtual bool? AutoManufacturingOrder
    {
      get { return m_autoTask; }
      set { m_autoTask = value; }
    }

    /// <summary>
    /// Is the option to reset the manufacturing order active ?
    /// </summary>
    public virtual bool? ResetManufacturingOrder
    {
      get { return m_resetManufacturingOrder; }
      set { m_resetManufacturingOrder = value; }
    }

    /// <summary>
    /// Determined component from the line/work order
    /// </summary>
    [XmlIgnore]
    internal protected virtual IComponent Component
    {
      get { return m_component; }
      set { m_component = value; }
    }

    /// <summary>
    /// Determined operation from the line/work order
    /// </summary>
    [XmlIgnore]
    internal protected virtual IOperation Operation
    {
      get { return m_operation; }
      set { m_operation = value; }
    }

    /// <summary>
    /// Part of the detection analysis so that any analysis problem is logged
    /// in the detectionanalysislog table
    /// </summary>
    [XmlIgnore]
    public virtual bool PartOfDetectionAnalysis
    {
      get { return m_partOfDetectionAnalysis; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// The default constructor is forbidden
    /// </summary>
    protected WorkOrderMachineAssociation ()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <returns></returns>
    internal protected WorkOrderMachineAssociation (IMachine machine, DateTime begin)
      : base (machine, begin)
    {
    }

    /// <summary>
    /// Constructor for the unit tests
    /// </summary>
    /// <returns></returns>
    internal protected WorkOrderMachineAssociation (IMachine machine, UtcDateTimeRange range)
      : base (machine, range)
    {
    }

    /// <summary>
    /// Constructor for abstract modifications, that are kept transient
    /// 
    /// You may add a mainModification reference to be used in the analysis logs
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="mainModification"></param>
    /// <param name="partOfDetectionAnalysis"></param>
    public WorkOrderMachineAssociation (IMachine machine, UtcDateTimeRange range, IModification mainModification,
                                        bool partOfDetectionAnalysis)
      : base (machine, range, mainModification)
    {
      m_partOfDetectionAnalysis = partOfDetectionAnalysis;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// <see cref="PeriodAssociation.ConvertToSlot" />
    /// </summary>
    /// <returns></returns>
    public override TSlot ConvertToSlot<TSlot> ()
    {
      var slot =
        GenericMachineRangeSlot.Create (typeof (TSlot), this.Machine, this.Range) as TSlot;
      slot.Consolidated = false;

      // Note: OperationSlot is processed by WorkOrderShiftMachineAssociation

      System.Diagnostics.Debug.Assert (false);
      throw new NotImplementedException ("Slot type not implemented");
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
      Debug.Assert (null != (oldSlot as IPartitionedByMachine).Machine);
      Debug.Assert (null != this.Machine);
      Debug.Assert (object.Equals (this.Machine, (oldSlot as IPartitionedByMachine).Machine));

      // Note: the OperationSlot is processed by the WorkOrderShiftMachineAssociation

      System.Diagnostics.Debug.Assert (false);
      log.FatalFormat ("MergeData: " +
                       "trying to merge the association with a not supported slot {0}",
                       typeof (TSlot));
      throw new ArgumentException ("Not supported machine slot");
    }

    /// <summary>
    /// Make the analysis
    /// </summary>
    public override void MakeAnalysis ()
    {
      Debug.Assert (!IsMainModificationTransient ());
      Debug.Assert (!IsAnalysisCompleted ());

      if (Bound.Compare<DateTime> (this.End, this.Begin) < 0) { // Empty period: error
        string message = string.Format ("End={0} before Begin={1}",
                                        this.End, this.Begin);
        log.ErrorFormat ("MakeAnalysis: " +
                         "{0} " +
                         "=> finish in error",
                         message);
        AddAnalysisLog (LogLevel.ERROR, message);
        MarkAsError ();
        return;
      }

      if ((this.Option.HasValue) &&
          ((((int)this.Option.Value) & ((int)AssociationOption.AssociateToSlotOption)) != 0)) {
        #region AssociateToSlotOption implementation
        // update end date time according to current slot if it exists
        IOperationSlot modificationDateTimeSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
          .FindAt (this.Machine, this.Begin);
        if (modificationDateTimeSlot != null) {
          SetActive ();
          IOperationSlot lastOperationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
            .GetLastOperationNotNull (this.Machine);
          SetActive ();

          UtcDateTimeRange range;
          if (lastOperationSlot != null) {
            if (modificationDateTimeSlot.Id != lastOperationSlot.Id) {
              // slot targetted by modification is not current slot
              // apply modification to end of slot
              range = new UtcDateTimeRange (modificationDateTimeSlot.BeginDateTime,
                                            modificationDateTimeSlot.EndDateTime);
            }
            else {
              // modification targets current slot
              // apply modification to infinity
              range = new UtcDateTimeRange (modificationDateTimeSlot.BeginDateTime);
            }
          }
          else {
            // modification targets current slot
            // apply modification to infinity
            range = new UtcDateTimeRange (modificationDateTimeSlot.BeginDateTime);
          }

          IWorkOrderMachineAssociation subModification = ModelDAOHelper.ModelFactory
            .CreateWorkOrderMachineAssociation (this.Machine, this.WorkOrder, range);
          subModification.WorkOrder = this.WorkOrder;
          subModification.Line = this.Line;
          subModification.ResetManufacturingOrder = this.ResetManufacturingOrder;
          ModelDAOHelper.DAOFactory.WorkOrderMachineAssociationDAO.MakePersistent (subModification);
          subModification.Parent = this.MainModification ?? this;
          subModification.Priority = this.StatusPriority;
          MarkAsCompleted ("");
        }
        else {
          string message = string.Format ("no operation slot found for {0} at {1}",
                                          this.Machine, this.Begin);
          log.ErrorFormat ("MakeAnalysis: {0}",
                           message);
          AddAnalysisLog (LogLevel.ERROR,
                          message);
          MarkAsError ();
        }
        #endregion // AssociateToSlotOption implementation
        return;
      }
      else { // Not AssociateToSlotOption
        // Get the adjusted step range
        UtcDateTimeRange range = GetNotAppliedRange ();

        // Analyze ! with the adjusted begin and end
        WorkOrderMachineAssociation association = new WorkOrderMachineAssociation (this.Machine,
                                                                                   range,
                                                                                   this.MainModification ?? this,
                                                                                   m_partOfDetectionAnalysis);
        association.DateTime = this.DateTime;
        association.WorkOrder = this.WorkOrder;
        association.Line = this.Line;
        association.Caller = this;
        association.Analyze ();

        // Analysis is done
        MarkAsCompleted ("Cache/ClearDomainByMachine/OperationAssociation/" + this.Machine.Id + "?Broadcast=true",
                         (DateTime?)range.Upper); // => InProgress or Done
      }
    }

    /// <summary>
    /// Apply the modifications
    /// 
    /// The option AssociateToSlot is not implemented
    /// </summary>
    public override void Apply ()
    {
      if ((this.Option.HasValue) &&
          ((((int)this.Option.Value) & ((int)AssociationOption.AssociateToSlotOption)) != 0)) {
        Debug.Assert (false);
        log.FatalFormat ("Apply: " +
                         "the option AssociateToSlot is not implemented");
        throw new NotImplementedException ();
      }

      this.Analyze ();
    }

    bool GetDataStructureOption (DataStructureConfigKey key)
    {
      return Lemoine.Info.ConfigSet
        .Get<bool> (ConfigKeys.GetDataStructureConfigKey (key));
    }

    /// <summary>
    /// Insert all the slots that may correspond to this modification
    /// taking into account the auto-sequence table
    /// 
    /// Note the DateTime property of the modification must be correctly
    /// set to use this
    /// </summary>
    public virtual void Analyze ()
    {
      Debug.Assert (null == m_component);
      Debug.Assert (null == m_operation);

      // - Check the slots are processed in the range to avoid some side effects
      //   in the preFetchedOperationSlots optimization
      CheckSlotProcessed ();

      if (GetDataStructureOption (DataStructureConfigKey.WorkOrderFromComponentOnly) && !this.Auto) {
        log.WarnFormat ("MergeDataWithOldSlot: " +
                        "why is WorkOrderMachineAssociation used " +
                        "while data structure option WorkOrderFromComponentOnly is set ?");
        AddAnalysisLog (LogLevel.WARN,
                        "WorkOrderMachineAssociation used " +
                        "while data structure option WorkOrderFromComponentOnly " +
                        "is set");
      }
      SetActive ();

      if (null != this.ManufacturingOrder) {
        // - Task => WorkOrder
        if (null != this.ManufacturingOrder.WorkOrder) {
          m_workOrder = this.ManufacturingOrder.WorkOrder;
        }
        // - Task => Component
        if (null != this.ManufacturingOrder.Component) {
          m_component = this.ManufacturingOrder.Component;
        }
        // - Task => Operation
        if (null != this.ManufacturingOrder.Operation) {
          m_operation = this.ManufacturingOrder.Operation;
        }
      }

      // - Line => Component
      if (null != this.Line) {
        Debug.Assert (null == m_component);
        // The following line is necessary to initialize this.Line.Components.
        // This is not clear why though
        ModelDAOHelper.DAOFactory.LineDAO.Lock (this.Line);
        ICollection<IComponent> components = this.Line.Components;
        foreach (IComponent component in components) {
          log.DebugFormat ("{0}", component);
        }
        if ((1 < this.Line.Components.Count)
          && GetDataStructureOption (DataStructureConfigKey.UniqueComponentFromLine)) {
          log.WarnFormat ("Analyze: " +
                          "line {0} is associated to more than one component " +
                          "while data structure option UniqueComponentFromLine is set",
                          this.Line);
          AddAnalysisLog (LogLevel.WARN,
                          "more than one component is associated to the line " +
                          "while data structure option UniqueComponentFromLine is set");
        }
        foreach (IComponent component in components) {
          if (Lemoine.Business.Operation.OperationRelations.IsWorkOrderCompatibleWithComponent (this.WorkOrder, component)) {
            log.DebugFormat ("Analyze: " +
                             "component {0} from line is compatible with the work order {1}",
                             component, this.WorkOrder);
            if (null == m_component) { // First component found
              m_component = component;
            }
            else { // Second one
              log.DebugFormat ("Analyze: " +
                               "a second compatible component {0} was found from line {1} " +
                               "=> reset the found component");
              m_component = null;
              break;
            }
          }
        }
      } // Line => Component
      SetActive ();

      // - WorkOrder => Component
      if ((null == this.Line)
          && (null != this.WorkOrder)
          && (null == m_component)
          && GetDataStructureOption (DataStructureConfigKey.UniqueProjectOrPartFromWorkOrder)) {
        // The component may be determined from the work order
        Debug.Assert (this.WorkOrder.Projects.Count <= 1);
        if (1 < this.WorkOrder.Projects.Count) {
          log.WarnFormat ("Analyze: " +
                          "more than one project is associated to the work order {0} " +
                          "although the option Work Order => 1 Project or Part is set",
                          this.WorkOrder);
          AddAnalysisLog (LogLevel.WARN,
                          "more than one project is associated to the work order " +
                          "while data structure option UniqueProjectOrPartFromWorkOrder is set");
        }
        foreach (IProject project in this.WorkOrder.Projects) {
          if (1 == project.Components.Count) {
            foreach (IComponent theComponent in project.Components) {
              log.DebugFormat ("Analyze: " +
                               "because of data structure option UniqueProjectOrPartFromWorkOrder, " +
                               "get the component {0} from work order {1}",
                               theComponent, this.WorkOrder);
              m_component = theComponent;
            }
          }
          break;
        }
      } // WorkOrder => Component
      SetActive ();

      // - Line + Machine => Operation
      if ((null != this.Line) && (null == m_operation)) {
        IList<ILineMachine> lineMachines = ModelDAOHelper.DAOFactory.LineMachineDAO
          .FindAllByLineMachine (this.Line, this.Machine);
        if (1 < lineMachines.Count) {
          log.DebugFormat ("ConvertToSlot: " +
                           "more than operation defined for line {0} and machine {1}",
                           Line, Machine);
        }
        else { // lineMachines.Count <= 1
          Debug.Assert (lineMachines.Count <= 1);
          foreach (ILineMachine lineMachine in lineMachines) {
            if (LineMachineStatus.Dedicated.Equals (lineMachine.LineMachineStatus)) {
              log.DebugFormat ("ConvertToSlot: " +
                               "Machine {0} is dedicated to line {1} for operation {2}",
                               this.Machine, this.Line, lineMachine.Operation);
              Debug.Assert (null != lineMachine.Operation);
              m_operation = lineMachine.Operation;
            }
          }
        }
      } // Line + Machine => Operation
      SetActive ();

      // Insert the slots
      InsertCorrespondingSlots ();
    }

    /// <summary>
    /// Insert all the slots that may correspond to this modification
    /// </summary>
    void InsertCorrespondingSlots ()
    {
      InsertCorrespondingSlots (null);
    }

    /// <summary>
    /// Insert all the slots that may correspond to this modification
    /// <param name="preFetchedOperationSlots"></param>
    /// </summary>
    void InsertCorrespondingSlots (IList<IOperationSlot> preFetchedOperationSlots)
    {
      if (AnalysisConfigHelper.OperationSlotSplitOption.IsActive ()) {
        // - Get a split range that is limited to machineStatus.OperationSlotSplitEnd
        log.DebugFormat ("InsertCorrespondingSlots: " +
                         "one of the split option is active " +
                         "=> determine a splitRange");
        UtcDateTimeRange splitRange = this.Range;
        IOperationSlotSplit operationSlotSplit = ModelDAOHelper.DAOFactory.OperationSlotSplitDAO
          .FindById (this.Machine.Id);
        if ((null != operationSlotSplit)
            && (Bound.Compare<DateTime> (operationSlotSplit.End, splitRange.Upper) < 0)) {
          log.DebugFormat ("InsertCorrespondingSlots: " +
                           "restrict the split range to {0}",
                           operationSlotSplit.End);
          splitRange = new UtcDateTimeRange (splitRange.Lower, operationSlotSplit.End);
        }

        // - Split the operation slot by day and/or shift
        log.DebugFormat ("InsertCorrespondingSlots: " +
                         "Split operation option");
        Bound<DateTime> begin = this.Begin;
        if (!splitRange.IsEmpty ()) {
          Debug.Assert (splitRange.Lower.Equals (this.Begin));
          IEnumerable<ISlotWithDayShift> slots;
          if (AnalysisConfigHelper.OperationSlotSplitOption.HasFlag (OperationSlotSplitOption.ByGlobalShift)) {
            IList<IShiftSlot> shiftSlots = ModelDAOHelper.DAOFactory.ShiftSlotDAO
              .FindOverlapsRange (splitRange);
            slots = shiftSlots.Cast<ISlotWithDayShift> ();
          }
          else if (AnalysisConfigHelper.OperationSlotSplitOption.HasFlag (OperationSlotSplitOption.ByMachineShift)) {
            IList<IObservationStateSlot> observationStateSlots = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
              .FindOverlapsRange (this.Machine, splitRange);
            IList<IDaySlot> daySlots = ModelDAOHelper.DAOFactory.DaySlotDAO
              .FindOverlapsRange (splitRange);
            slots = SlotWithDayShift.Combine (observationStateSlots.Cast<ISlotWithDayShift> (),
                                              daySlots.Cast<ISlotWithDayShift> ());
          }
          else if (AnalysisConfigHelper.OperationSlotSplitOption.Equals (OperationSlotSplitOption.ByDay)) {
            IList<IDaySlot> daySlots = ModelDAOHelper.DAOFactory.DaySlotDAO
              .FindOverlapsRange (splitRange);
            slots = daySlots.Cast<ISlotWithDayShift> ();
          }
          else {
            Debug.Assert (false);
            log.FatalFormat ("InsertCorrespondingSlots: " +
                             "Operation slot split active with a new case {0} that is not implemented yet",
                             AnalysisConfigHelper.OperationSlotSplitOption);
            throw new NotImplementedException ();
          }
          foreach (ISlotWithDayShift slot in slots) {
            SetActive ();
            if (Bound.Compare<DateTime> (begin, slot.BeginDateTime) < 0) { // No shift data gap to process
              Debug.Assert (false);
              Debug.Assert (slot.BeginDateTime.HasValue);
              UtcDateTimeRange range = new UtcDateTimeRange ((LowerBound<DateTime>)begin, slot.BeginDateTime.Value);
              log.FatalFormat ("InsertCorrespondingSlots: " +
                               "there should be no gap in shift slots any more " +
                               "but process no shift data gap workOrder={0} Line={1} period={2}",
                               this.WorkOrder, this.Line, range);
              WorkOrderShiftMachineAssociation noShiftAssociation = new WorkOrderShiftMachineAssociation (this.Machine,
                                                                                                          range,
                                                                                                          this.MainModification);
              noShiftAssociation.Caller = this;
              noShiftAssociation.Option = AssociationOption.NoRightMerge.Add (this.Option);
              noShiftAssociation.WorkOrder = this.WorkOrder;
              noShiftAssociation.Line = this.Line;
              noShiftAssociation.ManufacturingOrder = m_manufacturingOrder;
              noShiftAssociation.AutoManufacturingOrder = m_autoTask;
              noShiftAssociation.ResetManufacturingOrder = m_resetManufacturingOrder;
              noShiftAssociation.Component = m_component;
              noShiftAssociation.Operation = m_operation;
              noShiftAssociation.Day = null;
              noShiftAssociation.Shift = null;
              UpdatePreFetchedOperationSlots (range, ref preFetchedOperationSlots);
              noShiftAssociation.Analyze (preFetchedOperationSlots);
            }
            UtcDateTimeRange intersection = new UtcDateTimeRange (slot.DateTimeRange.Intersects (this.Range));
            if (intersection.IsEmpty ()) {
              Debug.Assert (false);
              log.FatalFormat ("InsertCorrespondingSlots: " +
                               "empty intersection of {0} with {1}",
                               this, slot);
            }
            else { // !intersection.IsEmpty ()
              log.DebugFormat ("Analyze: " +
                               "process workOrder={0} shift={1} period={2} Line={3}",
                               this.WorkOrder, slot.Shift, intersection, this.Line);
              WorkOrderShiftMachineAssociation association = new WorkOrderShiftMachineAssociation (this.Machine,
                                                                                                   intersection,
                                                                                                   this.MainModification);
              association.Caller = this;
              association.Option = this.Option;
              if (Bound.Compare<DateTime> (this.Begin, intersection.Lower) < 0) {
                association.Option = AssociationOption.NoLeftMerge.Add (association.Option);
              }
              if (Bound.Compare<DateTime> (intersection.Upper, this.End) < 0) {
                association.Option = AssociationOption.NoRightMerge.Add (association.Option);
              }
              association.WorkOrder = this.WorkOrder;
              association.Line = this.Line;
              association.ManufacturingOrder = m_manufacturingOrder;
              association.AutoManufacturingOrder = m_autoTask;
              association.ResetManufacturingOrder = m_resetManufacturingOrder;
              association.Component = m_component;
              association.Operation = m_operation;
              association.Day = slot.Day;
              association.Shift = slot.Shift;
              UpdatePreFetchedOperationSlots (intersection, ref preFetchedOperationSlots);
              association.Analyze (preFetchedOperationSlots);
              begin = intersection.Upper;
            }
          }
        }
        if (Bound.Compare<DateTime> (begin, this.End) < 0) {
          UtcDateTimeRange range = new UtcDateTimeRange ((LowerBound<DateTime>)begin,
                                                         this.End);
          Debug.Assert (!range.IsEmpty ());
          if (splitRange.ContainsRange (this.Range)) {
            Debug.Assert (false);
            log.FatalFormat ("InsertCorrespondingSlots: " +
                             "there should be no gap in shift slots any more " +
                             "but process a last no shift data gap workOrder={0} Line={1} period={2}-{3}",
                             this.WorkOrder, this.Line, range);
          }
          WorkOrderShiftMachineAssociation association = new WorkOrderShiftMachineAssociation (this.Machine,
                                                                                               range,
                                                                                               this.MainModification);
          association.Caller = this;
          association.Option = this.Option;
          if (Bound.Compare<DateTime> (this.Begin, begin) < 0) {
            association.Option = AssociationOption.NoLeftMerge.Add (association.Option);
          }
          association.WorkOrder = this.WorkOrder;
          association.Line = this.Line;
          association.ManufacturingOrder = m_manufacturingOrder;
          association.AutoManufacturingOrder = m_autoTask;
          association.ResetManufacturingOrder = m_resetManufacturingOrder;
          association.Component = m_component;
          association.Operation = m_operation;
          association.Day = null;
          association.Shift = null;
          UpdatePreFetchedOperationSlots (range, ref preFetchedOperationSlots);
          association.Analyze (preFetchedOperationSlots);
        }
      }
      else { // No split option
        WorkOrderShiftMachineAssociation association = new WorkOrderShiftMachineAssociation (this.Machine,
                                                                                             this.Range,
                                                                                             this.MainModification);
        association.Caller = this;
        association.WorkOrder = this.WorkOrder;
        association.Line = this.Line;
        association.ManufacturingOrder = m_manufacturingOrder;
        association.AutoManufacturingOrder = m_autoTask;
        association.ResetManufacturingOrder = m_resetManufacturingOrder;
        association.Component = m_component;
        association.Operation = m_operation;
        association.Day = null;
        association.Shift = null;
        association.Analyze (preFetchedOperationSlots);
      }
    }

    void UpdatePreFetchedOperationSlots (UtcDateTimeRange range, ref IList<IOperationSlot> preFetchedOperationSlots)
    {
      if (this.Range.ContainsRange (range)
          && !Bound.Equals<DateTime> (this.Range.Lower, range.Lower)
          && !Bound.Equals<DateTime> (this.Range.Upper, range.Upper)) {
        // Not in the limits: you can try to use preFetchedOperationSlots
        // because the slots won't be cut

        // Check the pre-fetched operation slots reached the limit, else there may be some side effects
        if ((null != preFetchedOperationSlots) && (0 < preFetchedOperationSlots.Count)) {
          IOperationSlot lastPreFetchedOperationSlot = preFetchedOperationSlots[preFetchedOperationSlots.Count - 1];
          if (lastPreFetchedOperationSlot.EndDateTime < range.Upper) {
            preFetchedOperationSlots = null;
          }
          else {
            IOperationSlot firstPreFetchedOperationSlot = preFetchedOperationSlots[0];
            if (range.Lower < firstPreFetchedOperationSlot.BeginDateTime) {
              preFetchedOperationSlots = null;
            }
          }
        }

        if (null == preFetchedOperationSlots) {
          // Initialize preFetchedOperationSlots
          bool leftMerge = !this.Option.HasValue
            || !this.Option.Value.HasFlag (AssociationOption.NoLeftMerge);
          bool rightMerge = !this.Option.HasValue
            || !this.Option.Value.HasFlag (AssociationOption.NoRightMerge);
          preFetchedOperationSlots = (new OperationSlotDAO ())
            .GetImpactedMachineSlotsForAnalysis (this.Machine, new UtcDateTimeRange (range.Lower, this.End), this.DateTime, false, leftMerge, rightMerge);
        }
      }
      else { // In the limits: don't use preFetchedOperationSlots
        preFetchedOperationSlots = null;
      }
    }

    void CheckSlotProcessed ()
    {
      // TODO: cancellationToken
      if (AnalysisConfigHelper.OperationSlotSplitOption.IsSplitByDay ()) {
        DateTime begin;
        if (this.Begin.HasValue) {
          begin = this.Begin.Value;
        }
        else if (this.End.HasValue) {
          begin = this.End.Value;
        }
        else {
          begin = DateTime.UtcNow;
        }
        DateTime end;
        if (this.End.HasValue) {
          end = this.End.Value;
        }
        else if (this.Begin.HasValue) {
          end = this.Begin.Value;
        }
        else {
          end = DateTime.UtcNow;
        }
        UtcDateTimeRange range = new UtcDateTimeRange (begin, end, "[]");
        (new DaySlotDAO ()).ProcessInRange (System.Threading.CancellationToken.None, range, this);
      }
    }
    #endregion // Methods

    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public override void Unproxy ()
    {
      base.Unproxy ();
      NHibernateHelper.Unproxy<ILine> (ref m_line);
      NHibernateHelper.Unproxy<IWorkOrder> (ref m_workOrder);
    }
  }
}
