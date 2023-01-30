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
using System.Threading;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table ComponentMachineAssociation
  /// 
  /// In this table is stored a new row each time a component
  /// is associated to a machine.
  /// 
  /// It does not represent the current relation between the component
  /// and a machining resource, but all the manual or automatic associations
  /// that are made between a component and a machining resource.
  /// </summary>
  [Serializable]
  public class ComponentMachineAssociation: MachineAssociation, IComponentMachineAssociation
  {
    #region Members
    IComponent m_component;
    IWorkOrder m_workOrder = null;
    ILine m_line = null;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// <see cref="IModification"/>
    /// </summary>
    [XmlIgnore]
    public override string ModificationType
    {
      get { return "ComponentMachineAssociation"; }
    }

    /// <summary>
    /// Reference to the Component persistent class
    /// </summary>
    [XmlIgnore]
    public virtual IComponent Component {
      get { return m_component; }
      set { m_component = value; }
    }
    
    /// <summary>
    /// Reference to the Component persistent class
    /// for Xml Serialization
    /// </summary>
    [XmlElement("Component")]
    public virtual Component XmlSerializationComponent {
      get { return this.Component as Component; }
      set { this.Component = value; }
    }
    
    /// <summary>
    /// Determined work order from the component
    /// </summary>
    [XmlIgnore]
    internal protected virtual IWorkOrder WorkOrder {
      get { return m_workOrder; }
      set { m_workOrder = value; }
    }
    
    /// <summary>
    /// Determined line from the component
    /// </summary>
    [XmlIgnore]
    internal protected virtual ILine Line {
      get { return m_line; }
      set { m_line = value; }
    }
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// The default constructor is forbidden
    /// </summary>
    protected ComponentMachineAssociation ()
    {
    }
    
    /// <summary>
    /// Constructor for some existing unit tests
    /// </summary>
    /// <returns></returns>
    internal protected ComponentMachineAssociation (IMachine machine, DateTime begin)
      : base (machine, begin)
    {
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    internal protected ComponentMachineAssociation (IMachine machine, UtcDateTimeRange range)
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
    internal protected ComponentMachineAssociation (IMachine machine, UtcDateTimeRange range, IModification mainModification)
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
      var slot =
        GenericMachineRangeSlot.Create (typeof (TSlot), this.Machine, this.Range) as TSlot;
      slot.Consolidated = false;
      
      // Note: OperationSlot is processed by ComponentShiftMachineAssociation

      System.Diagnostics.Debug.Assert (false);
      log.FatalFormat ("ConvertToSlot: " +
                       "Slot of type {0} is not implemented",
                       typeof (TSlot));
      throw new NotImplementedException ("Slot type not implemented");
    }

    /// <summary>
    /// <see cref="PeriodAssociation.MergeDataWithOldSlot" />
    /// </summary>
    /// <param name="oldSlot">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public override TSlot MergeDataWithOldSlot<TSlot>(TSlot oldSlot,
                                                      UtcDateTimeRange range)
    {
      Debug.Assert (null != oldSlot);
      Debug.Assert (oldSlot is GenericMachineRangeSlot);
      Debug.Assert (null != (oldSlot as IPartitionedByMachine).Machine);
      Debug.Assert (null != this.Machine);
      Debug.Assert (object.Equals (this.Machine, (oldSlot as IPartitionedByMachine).Machine));
      
      // Note: the OperationSlot is processed by the ComponentShiftMachineAssociation

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
        AddAnalysisLog(LogLevel.ERROR, message);
        MarkAsError ();
        return;
      }

      if (this.Option.HasValue
          && this.Option.Value.HasFlag (AssociationOption.AssociateToSlotOption)) {
        #region AssociateToSlotOption implementation
        // update end date time according to current slot if it exists
        IOperationSlot modificationDateTimeSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
          .FindAt (this.Machine, this.Begin);
        if (modificationDateTimeSlot != null) {
          SetActive ();
          IOperationSlot lastOperationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
            .GetLastOperationNotNull(this.Machine);
          SetActive ();
          
          LowerBound<DateTime> begin;
          UpperBound<DateTime> end;
          if (lastOperationSlot != null) {
            if (modificationDateTimeSlot.Id != lastOperationSlot.Id) {
              // slot targetted by modification is not current slot
              // apply modification to end of slot
              begin = modificationDateTimeSlot.BeginDateTime;
              end = modificationDateTimeSlot.EndDateTime;
            }
            else {
              // modification targets current slot
              // apply modification to infinity
              begin = modificationDateTimeSlot.BeginDateTime;
              end = new UpperBound<DateTime> (null);
            }
          }
          else {
            // modification targets current slot
            // apply modification to infinity
            begin = modificationDateTimeSlot.BeginDateTime;
            end = new UpperBound<DateTime> (null);
          }
          IComponentMachineAssociation subModification = ModelDAOHelper.ModelFactory
            .CreateComponentMachineAssociation (this.Machine, this.Component, new UtcDateTimeRange (begin, end));
          ModelDAOHelper.DAOFactory.ComponentMachineAssociationDAO.MakePersistent (subModification);
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
      else {
        // Get the adjusted step range
        UtcDateTimeRange range = GetNotAppliedRange ();

        // Analyze ! with the adjusted begin and end
        ComponentMachineAssociation association = new ComponentMachineAssociation (this.Machine,
                                                                                   range,
                                                                                   this);
        association.DateTime = this.DateTime;
        association.Component = this.Component;
        association.Caller = this;
        association.Analyze ();
        
        // Analysis is done
        MarkAsCompleted ("Cache/ClearDomainByMachine/OperationAssociation/" + this.Machine.Id + "?Broadcast=true",
                         (DateTime?) range.Upper); // => InProgress or Done
      }
    }
    
    /// <summary>
    /// Apply the modification
    /// 
    /// The option AssociateToSlot is not implemented
    /// </summary>
    public override void Apply ()
    {
      if (this.Option.HasValue
          && this.Option.Value.HasFlag (AssociationOption.AssociateToSlotOption)) {
        Debug.Assert (false);
        log.FatalFormat ("Apply: " +
                         "the option AssociateToSlot is not implemented");
        throw new NotImplementedException ();
      }
      
      this.Analyze ();
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
      Debug.Assert (null == m_workOrder);
      Debug.Assert (null == m_line);

      // - Check the slots are processed in the range to avoid some side effects
      //   in the preFetchedOperationSlots optimization
      CheckSlotProcessed ();
      
      // - Component => Line
      if ((null != m_line)
          && (null != this.Component)) { // TODO: option to validate this
        IList<ILine> lines = ModelDAOHelper.DAOFactory.LineDAO
          .FindAllByComponent (this.Component);
        if (1 < lines.Count) {
          log.DebugFormat ("Analyze: " +
                           "more than one line is defined for component {0}",
                           this.Component);
        }
        else if (1 == lines.Count) {
          foreach (ILine line in lines) {
            log.DebugFormat ("Analyze: " +
                             "get one line id={1} for component {0}",
                             this.Component, line.Id);
            m_line = line;
          }
        }
      }
      
      // - Component => WorkOrder
      if ((null == m_line)
          && (null == m_workOrder)
          && (null != this.Component)) {
        var uniqueWorkOrderFromProjectOrComponent = Lemoine.Info.ConfigSet
          .Get<bool> (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.UniqueWorkOrderFromProjectOrComponent));
        if (uniqueWorkOrderFromProjectOrComponent) {
          m_workOrder = Lemoine.Business.Operation.OperationRelations.GuessUniqueWorkOrderFromComponent (this.Component);
        }
        else {
          m_workOrder = Lemoine.Business.Operation.OperationRelations.TryToGuessWorkOrderFromComponent (this.Component);
        }
      }
      
      // - Line => WorkOrder
      if ((null != m_line)
          && (null == m_workOrder)) {
        IList<IWorkOrderLine> workOrderLines = ModelDAOHelper.DAOFactory.WorkOrderLineDAO
          .GetListInRange (m_line, this.Range);
        if (1 == workOrderLines.Count) {
          IWorkOrderLine workOrderLine = workOrderLines [0];
          Debug.Assert (null != workOrderLine.WorkOrder);
          if ( (Bound.Compare<DateTime> (workOrderLine.BeginDateTime, this.Begin) <= 0)
              && (NullableDateTime.Compare (this.End, workOrderLine.EndDateTime) <= 0)) { // On the whole period
            log.DebugFormat ("Analyze: " +
                             "got work order id={0} from line id={1}",
                             ((Lemoine.Collections.IDataWithId)workOrderLine.WorkOrder).Id, m_line.Id);
            if (Lemoine.Business.Operation.OperationRelations.IsWorkOrderCompatibleWithComponent (workOrderLine.WorkOrder, this.Component)) {
              m_workOrder = workOrderLine.WorkOrder;
            }
            else {
              log.WarnFormat ("Analyze: " +
                              "got work order id={0} from line id={1} " +
                              "but it is not compatible with component={2}",
                              ((Lemoine.Collections.IDataWithId)workOrderLine.WorkOrder).Id, m_line.Id, this.Component);
            }
          }
        }
        if ( (null == m_workOrder) && (1 <= workOrderLines.Count)) { // Split by date
          log.DebugFormat ("Analyze: " +
                           "more than one work order may correspond to line {0} in period {1}-{2}",
                           m_line.Id, this.Begin, this.End);
          LowerBound<DateTime> begin = this.Begin;
          IList<IOperationSlot> preFetchedOperationSlots = null;
          foreach (IWorkOrderLine workOrderLine in workOrderLines) {
            Debug.Assert (null != workOrderLine.WorkOrder);
            SetActive ();
            if (!Lemoine.Business.Operation.OperationRelations.IsWorkOrderCompatibleWithComponent (workOrderLine.WorkOrder, this.Component)) {
              log.WarnFormat ("Analyze: " +
                              "got work order id={0} from line id={1} " +
                              "but it is not compatible with component={2}",
                              ((Lemoine.Collections.IDataWithId)workOrderLine.WorkOrder).Id, m_line.Id, this.Component);
              continue;
            }
            {
              UtcDateTimeRange range = new UtcDateTimeRange (begin, workOrderLine.BeginDateTime.Value);
              if (!range.IsEmpty ()) {
                log.DebugFormat ("Analyze: " +
                                 "no work order for line {0} in period {1}-{2}",
                                 m_line.Id, begin, workOrderLine.BeginDateTime);
                Debug.Assert (workOrderLine.BeginDateTime.HasValue);
                ComponentMachineAssociation association = new ComponentMachineAssociation (this.Machine,
                                                                                           range,
                                                                                           this.MainModification);
                association.Caller = this;
                association.Component = this.Component;
                association.Line = m_line;
                UpdatePreFetchedOperationSlots (range, ref preFetchedOperationSlots);
                association.InsertCorrespondingSlots (preFetchedOperationSlots);
              }
            }
            {
              begin = (Bound.Compare<DateTime> (this.Begin, workOrderLine.BeginDateTime) <= 0)?workOrderLine.BeginDateTime:this.Begin;
              UpperBound<DateTime> end = (Bound.Compare<DateTime> (workOrderLine.EndDateTime, this.End) <= 0)?workOrderLine.EndDateTime:this.End;
              UtcDateTimeRange range = new UtcDateTimeRange (begin, end);
              Debug.Assert (!range.IsEmpty ());

              log.DebugFormat ("Analyze: " +
                               "work order {0} for line {1} in period {2}",
                               ((Lemoine.Collections.IDataWithId)workOrderLine.WorkOrder).Id, m_line.Id, range);
              ComponentMachineAssociation association = new ComponentMachineAssociation (this.Machine,
                                                                                         range,
                                                                                         this.MainModification);
              association.Caller = this;
              association.Component = this.Component;
              association.WorkOrder = workOrderLine.WorkOrder;
              association.Line = m_line;
              UpdatePreFetchedOperationSlots (range, ref preFetchedOperationSlots);
              association.InsertCorrespondingSlots (preFetchedOperationSlots);
              if (!end.HasValue) {
                break;
              }
              Debug.Assert (end.HasValue);
              begin = end.Value;
            }
          } // end loop
          {
            UtcDateTimeRange range = new UtcDateTimeRange (begin, this.End);
            if (!range.IsEmpty ()) {
              log.DebugFormat ("Analyze: " +
                               "no work order for line {0} in period {1}-{2}",
                               m_line.Id, begin, this.End);
              ComponentMachineAssociation association = new ComponentMachineAssociation (this.Machine,
                                                                                         range,
                                                                                         this.MainModification);
              association.Caller = this;
              association.Component = this.Component;
              association.Line = m_line;
              UpdatePreFetchedOperationSlots (range, ref preFetchedOperationSlots);
              association.InsertCorrespondingSlots (preFetchedOperationSlots);
            }
          }
          return;
        } // split by date
      } // Line => WorkOrder
      
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
        if ( (null != operationSlotSplit)
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
            log.Fatal ($"InsertCorrespondingSlots: Operation slot split active with a new case {AnalysisConfigHelper.OperationSlotSplitOption} that is not implemented yet");
            throw new NotImplementedException ();
          }
          foreach (ISlotWithDayShift slot in slots) {
            SetActive ();
            if (Bound.Compare<DateTime> (begin, slot.BeginDateTime) < 0) { // No shift data gap to process
              Debug.Assert (false);
              Debug.Assert (slot.BeginDateTime.HasValue);
              UtcDateTimeRange range = new UtcDateTimeRange ((LowerBound<DateTime>)begin, slot.BeginDateTime.Value);
              Debug.Assert (!range.IsEmpty ());
              log.FatalFormat ("InsertCorrespondingSlots: " +
                               "there should be no gap in slots any more " +
                               "but process no data gap component={0} period={1}",
                               this.Component, range);
              ComponentShiftMachineAssociation noShiftAssociation = new ComponentShiftMachineAssociation (this.Machine,
                                                                                                          range,
                                                                                                          this.MainModification);
              noShiftAssociation.Caller = this;
              noShiftAssociation.Option = AssociationOption.NoRightMerge.Add (this.Option);
              noShiftAssociation.Component = this.Component;
              noShiftAssociation.WorkOrder = m_workOrder;
              noShiftAssociation.Line = m_line;
              noShiftAssociation.Shift = null;
              UpdatePreFetchedOperationSlots (range, ref preFetchedOperationSlots);
              noShiftAssociation.Analyze (preFetchedOperationSlots);
            }
            UtcDateTimeRange intersection = new UtcDateTimeRange (this.Range.Intersects (slot.DateTimeRange));
            if (intersection.IsEmpty ()) {
              Debug.Assert (false);
              log.FatalFormat ("InsertCorrespondingSlots: " +
                               "empty intersection of {0} with {1}",
                               this, slot);
            }
            else { // !intersection.IsEmpty ()
              log.DebugFormat ("InsertCorrespondingSlots: " +
                               "process component={0} shift={1} period={2}",
                               this.Component, slot.Shift, intersection);
              ComponentShiftMachineAssociation association = new ComponentShiftMachineAssociation (this.Machine,
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
              association.Component = this.Component;
              association.WorkOrder = m_workOrder;
              association.Line = m_line;
              association.Day = slot.Day;
              association.Shift = slot.Shift;
              UpdatePreFetchedOperationSlots (intersection, ref preFetchedOperationSlots);
              association.Analyze (preFetchedOperationSlots);
              begin = intersection.Upper;
            } // Intersection not empty
          }
        }
        if (Bound.Compare<DateTime> (begin, this.End) < 0) {
          UtcDateTimeRange range = new UtcDateTimeRange ((LowerBound<DateTime>) begin, this.End);
          Debug.Assert (!range.IsEmpty ());
          if (splitRange.ContainsRange (this.Range)) {
            Debug.Assert (false);
            log.FatalFormat ("InsertCorrespondingSlots: " +
                             "there should be no gap in shift slots any more " +
                             "but process a last no shift data gap component={0} period={1}-{2}",
                             this.Component, begin, this.End);
          }
          ComponentShiftMachineAssociation association = new ComponentShiftMachineAssociation (this.Machine,
                                                                                               range,
                                                                                               this.MainModification);
          association.Caller = this;
          association.Option = this.Option;
          if (Bound.Compare<DateTime> (this.Begin, begin) < 0) {
            association.Option = AssociationOption.NoLeftMerge.Add (association.Option);
          }
          association.Component = this.Component;
          association.WorkOrder = m_workOrder;
          association.Line = m_line;
          association.Shift = null;
          UpdatePreFetchedOperationSlots (range, ref preFetchedOperationSlots);
          association.Analyze (preFetchedOperationSlots);
        }
      }
      else { // No split option
        ComponentShiftMachineAssociation association = new ComponentShiftMachineAssociation (this.Machine,
                                                                                             this.Range,
                                                                                             this.MainModification);
        association.Caller = this;
        association.Component = this.Component;
        association.WorkOrder = m_workOrder;
        association.Line = m_line;
        association.Day = null;
        association.Shift = null;
        association.Analyze (preFetchedOperationSlots);
      }
    }
    
    void UpdatePreFetchedOperationSlots (UtcDateTimeRange range, ref IList<IOperationSlot> preFetchedOperationSlots)
    {
      if (this.Range.ContainsRange (range)
          && !Bound.Equals<DateTime> (range.Lower, this.Range.Lower)
          && !Bound.Equals<DateTime> (range.Upper, this.Range.Upper)) {
        // Not in the limits: you can try to use preFetchedOperationSlots
        // because the slots won't be cut
        
        // Check the pre-fetched operation slots reached the limit, else there may be some side effects
        if ( (null != preFetchedOperationSlots) && (0 < preFetchedOperationSlots.Count)) {
          IOperationSlot lastPreFetchedOperationSlot = preFetchedOperationSlots [preFetchedOperationSlots.Count - 1];
          if (lastPreFetchedOperationSlot.EndDateTime < range.Upper) {
            preFetchedOperationSlots = null;
          }
          else {
            IOperationSlot firstPreFetchedOperationSlot = preFetchedOperationSlots [0];
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
          preFetchedOperationSlots = (new OperationSlotDAO())
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
        (new DaySlotDAO ()).ProcessInRange (CancellationToken.None, range, this);
      }
    }
    #endregion // Methods
    
    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public override void Unproxy ()
    {
      base.Unproxy ();
      NHibernateHelper.Unproxy<IComponent> (ref m_component);
    }
  }
}
