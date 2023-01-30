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
  /// Persistent class of table OperationMachineAssociation
  /// 
  /// In this new table is stored a new row each time
  /// an operation is manually associated to a machine.
  /// Usually the operation is automatically associated
  /// to the machine thanks to the ISO file identification and stamping.
  /// This table allows to manually set an operation to a machine period.
  /// 
  /// It is also very useful to associate yellow periods to an operation
  /// (where the ISO file identification is not available)
  /// following some predefined rules.
  /// 
  /// It does not represent the current relation between the operation
  /// and a machining resource, but all the manual or automatic associations
  /// that are made between an operation and a machine.
  /// 
  /// To know the current relation between a machining resource
  /// and an operation, the table Operation Slot that is filled in
  /// by the Analyzer must be used.
  /// </summary>
  [Serializable]
  public class OperationMachineAssociation: MachineAssociation, IOperationMachineAssociation
  {
    /// <summary>
    /// Minimum duration of the association when the 'by period' process applies: key
    /// </summary>
    static readonly string BY_PERIOD_MIN_DURATION_KEY = "Analysis.MachineObservationStateAssociation.ByPeriodMinDuration";
    /// <summary>
    /// Minimum duration of the association when the 'by period' process applies: default value
    /// </summary>
    static readonly TimeSpan BY_PERIOD_MIN_DURATION_DEFAULT = TimeSpan.FromHours  (8);

    #region Members
    IOperation m_operation;
    IWorkOrder m_workOrder = null;
    ILine m_line = null;
    ITask m_task = null;
    IComponent m_component = null;
    bool m_partOfDetectionAnalysis;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// <see cref="IModification"/>
    /// </summary>
    [XmlIgnore]
    public override string ModificationType
    {
      get { return "OperationMachineAssociation"; }
    }

    /// <summary>
    /// Reference to the Operation
    /// </summary>
    [XmlIgnore]
    public virtual IOperation Operation {
      get { return m_operation; }
      set { m_operation = value; }
    }
    
    /// <summary>
    /// Reference to the Operation for Xml Serialization
    /// </summary>
    [XmlElement("Operation")]
    public virtual Operation XmlSerializationOperation {
      get { return this.Operation as Operation; }
      set { this.Operation = value; }
    }
    
    /// <summary>
    /// Part of the detection analysis so that any analysis problem is logged
    /// in the detectionanalysislog table
    /// </summary>
    [XmlIgnore]
    public virtual bool PartOfDetectionAnalysis {
      get { return m_partOfDetectionAnalysis; }
    }
    
    /// <summary>
    /// Determined work order from the operation
    /// </summary>
    [XmlIgnore]
    public virtual IWorkOrder WorkOrder {
      get { return m_workOrder; }
      set { m_workOrder = value; }
    }
    
    /// <summary>
    /// Determined line from the operation
    /// </summary>
    [XmlIgnore]
    public virtual ILine Line {
      get { return m_line; }
      set { m_line = value; }
    }
    
    /// <summary>
    /// Task that was determined from the operation
    /// </summary>
    [XmlIgnore]
    public virtual ITask Task {
      get { return m_task; }
      set { m_task = value; }
    }
    
    /// <summary>
    /// Determined component from the operation
    /// </summary>
    [XmlIgnore]
    public virtual IComponent Component {
      get { return m_component; }
      set { m_component = value; }
    }
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// The default constructor is forbidden
    /// </summary>
    protected OperationMachineAssociation ()
    {
    }
    
    /// <summary>
    /// Constructor for some existing unit tests
    /// </summary>
    /// <returns></returns>
    internal protected OperationMachineAssociation (IMachine machine, DateTime begin)
      : base (machine, begin)
    {
    }
    
    /// <summary>
    /// Constructor for abstract modifications, that are kept transient
    /// 
    /// For some existing unit tests
    /// 
    /// You may add a mainModification reference to be used in the analysis logs
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="begin"></param>
    /// <param name="mainModification"></param>
    /// <param name="partOfDetectionAnalysis"></param>
    internal protected OperationMachineAssociation (IMachine machine, DateTime begin,
                                                    IModification mainModification,
                                                    bool partOfDetectionAnalysis)
      : base (machine, begin, mainModification)
    {
      Debug.Assert ((null == mainModification) || (false == partOfDetectionAnalysis));
      if ((null != mainModification) && partOfDetectionAnalysis) {
        log.FatalFormat ("OperationMachineAssociation: " +
                         "incompatible arguments mainModification={0} and partOfDetectionAnalysis={1}",
                         mainModification, partOfDetectionAnalysis);
      }
      m_partOfDetectionAnalysis = partOfDetectionAnalysis;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <returns></returns>
    internal protected OperationMachineAssociation (IMachine machine, UtcDateTimeRange range)
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
    internal protected OperationMachineAssociation (IMachine machine, UtcDateTimeRange range,
                                                    IModification mainModification,
                                                    bool partOfDetectionAnalysis)
      : base (machine, range, mainModification)
    {
      Debug.Assert ((null == mainModification) || (false == partOfDetectionAnalysis));
      if ((null != mainModification) && partOfDetectionAnalysis) {
        log.FatalFormat ("OperationMachineAssociation: " +
                         "incompatible arguments mainModification={0} and partOfDetectionAnalysis={1}",
                         mainModification, partOfDetectionAnalysis);
      }
      m_partOfDetectionAnalysis = partOfDetectionAnalysis;
    }
    #endregion // Constructors
    
    #region Methods
    /// <summary>
    /// Add an analysis log for this modification
    /// </summary>
    /// <param name="level"></param>
    /// <param name="message"></param>
    protected override void AddAnalysisLog (LogLevel level,
                                            string message)
    {
      if (m_partOfDetectionAnalysis) {
        log.DebugFormat ("AddAnalysisLog: " +
                         "add a detection analysis log with the message {0} and level {1}",
                         message, level);
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
      var slot =
        GenericMachineRangeSlot.Create (typeof (TSlot), this.Machine, this.Range) as TSlot;
      slot.Consolidated = false;
      
      // Note: OperationSlot is processed by OperationShiftMachineAssociation
      
      System.Diagnostics.Debug.Assert (false);
      throw new NotImplementedException ("Slot type not implemented");
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
      Debug.Assert (oldSlot is GenericMachineRangeSlot);
      Debug.Assert (null != (oldSlot as IPartitionedByMachine).Machine);
      Debug.Assert (null != this.Machine);
      Debug.Assert (object.Equals (this.Machine, (oldSlot as IPartitionedByMachine).Machine));
      
      // Note: the OperationSlot is processed by the OperationShiftMachineAssociation

      System.Diagnostics.Debug.Assert (false);
      log.FatalFormat ("MergeData: " +
                       "trying to merge the association with a not supported slot {0}",
                       typeof (TSlot));
      throw new ArgumentException ("Not supported machine slot");
    }
    
    /// <summary>
    /// Make the analysis
    /// 
    /// Must not be run in case of detection analysis
    /// </summary>
    public override void MakeAnalysis ()
    {
      Debug.Assert (!IsMainModificationTransient ());
      Debug.Assert (!IsAnalysisCompleted ());

      Debug.Assert (false == m_partOfDetectionAnalysis);
      
      if (Bound.Compare<DateTime> (this.End, this.Begin) < 0) { // Empty period: error
        var message = $"End={this.End} before Begin={this.Begin}";
        log.Error ($"MakeAnalysis: {message} => finish in error");
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
          
          IOperationMachineAssociation subModification = ModelDAOHelper.ModelFactory
            .CreateOperationMachineAssociation (this.Machine, range);
          subModification.Operation = this.Operation;
          ModelDAOHelper.DAOFactory.OperationMachineAssociationDAO
            .MakePersistent (subModification);
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
      if (!this.IsProcessByPeriod ()) { // Apply directly the changes, do not split it by period
        // Get the adjusted step range
        UtcDateTimeRange range = GetNotAppliedRange ();

        // Analyze ! with the adjusted begin and end
        OperationMachineAssociation association = new OperationMachineAssociation (this.Machine,
                                                                                   range,
                                                                                   this,
                                                                                   false);
        association.DateTime = this.DateTime;
        association.Operation = this.Operation;
        association.Task = this.Task;
        association.Option = this.Option;
        association.Caller = this;
        association.Analyze ();

        // Analysis is done
        MarkAsCompleted ("Cache/ClearDomainByMachine/OperationAssociation/" + this.Machine.Id + "?Broadcast=true");
      }
      else { // Try to split the process by periods where a change is really required
        IList<IOperationSlot> noChangeRequiredSlots = ModelDAOHelper.DAOFactory.OperationSlotDAO
          .FindAllInRangeWith (this.Machine, GetNoChangeRequiredRange (), this.Operation);
        bool associationCreated = false;
        Bound<DateTime> lower = this.Range.Lower;
        foreach (var noChangeRequiredSlot in noChangeRequiredSlots) {
          if (Bound.Compare<DateTime> (lower, noChangeRequiredSlot.DateTimeRange.Lower) < 0) {
            Debug.Assert (noChangeRequiredSlot.DateTimeRange.Lower.HasValue);
            Debug.Assert (Bound.Compare<DateTime> (noChangeRequiredSlot.DateTimeRange.Lower, this.Range.Upper) <= 0);
            UtcDateTimeRange range = new UtcDateTimeRange ((LowerBound<DateTime>)lower,
                                                           noChangeRequiredSlot.DateTimeRange.Lower.Value);
            Debug.Assert (this.Range.ContainsRange (range));
            IOperationMachineAssociation association = ModelDAOHelper.ModelFactory
              .CreateOperationMachineAssociation (this.Machine, range);
            association.DateTime = this.DateTime;
            association.Operation = this.Operation;
            association.Option = AssociationOption.NotByPeriod.Add (this.Option);
            ModelDAOHelper.DAOFactory.OperationMachineAssociationDAO.MakePersistent (association);
            association.Parent = this.MainModification ?? this;
            association.Priority = this.StatusPriority;
            associationCreated = true;
          }
          Debug.Assert (Bound.Compare<DateTime> (this.Range.Lower, noChangeRequiredSlot.DateTimeRange.Upper) < 0);
          Debug.Assert (Bound.Compare<DateTime> (lower, noChangeRequiredSlot.DateTimeRange.Upper) < 0);
          lower = noChangeRequiredSlot.DateTimeRange.Upper;
        }
        if (Bound.Compare<DateTime> (lower, this.Range.Upper) < 0) {
          UtcDateTimeRange range = new UtcDateTimeRange ((LowerBound<DateTime>)lower,
                                                         this.Range.Upper);
          Debug.Assert (this.Range.ContainsRange (range));
          IOperationMachineAssociation association = ModelDAOHelper.ModelFactory
            .CreateOperationMachineAssociation (this.Machine, range);
          association.DateTime = this.DateTime;
          association.Operation = this.Operation;
          association.Option = AssociationOption.NotByPeriod.Add (this.Option);
          ModelDAOHelper.DAOFactory.OperationMachineAssociationDAO.MakePersistent (association);
          association.Parent = this.MainModification ?? this;
          association.Priority = this.StatusPriority;
          associationCreated = true;
        }
        
        if (!associationCreated) {
          log.DebugFormat ("MakeAnalysis: " +
                           "no change was required");
        }
        MarkAsCompleted ("", null);
      }
    }
    
    /// <summary>
    /// Apply the modifications
    /// 
    /// The option AssociateToSlot is not implemented
    /// </summary>
    public override void Apply ()
    {
      if (this.Option.HasValue
          && this.Option.Value.HasFlag (AssociationOption.AssociateToSlotOption)) {
        Debug.Assert (false);
        log.Fatal ("Apply: the option AssociateToSlot is not implemented");
        throw new NotImplementedException ();
      }
      
      if (!this.IsProcessByPeriod ()) { // Apply directly the changes, do not split it by period
        this.Analyze ();
      }
      else { // Try to split the process by periods where a change is really required
        IList<IOperationSlot> noChangeRequiredSlots = ModelDAOHelper.DAOFactory.OperationSlotDAO
          .FindAllInRangeWith (this.Machine, GetNoChangeRequiredRange (), this.Operation);
        Bound<DateTime> lower = this.Range.Lower;
        foreach (var noChangeRequiredSlot in noChangeRequiredSlots) {
          if (Bound.Compare<DateTime> (lower, noChangeRequiredSlot.DateTimeRange.Lower) < 0) {
            Debug.Assert (noChangeRequiredSlot.DateTimeRange.Lower.HasValue);
            Debug.Assert (Bound.Compare<DateTime> (noChangeRequiredSlot.DateTimeRange.Lower, this.Range.Upper) <= 0);
            UtcDateTimeRange range = new UtcDateTimeRange ((LowerBound<DateTime>)lower,
                                                           noChangeRequiredSlot.DateTimeRange.Lower.Value);
            Debug.Assert (this.Range.ContainsRange (range));
            OperationMachineAssociation association = (OperationMachineAssociation)ModelDAOHelper.ModelFactory
              .CreateOperationMachineAssociation (this.Machine, range,
                                                  this.MainModification,
                                                  m_partOfDetectionAnalysis);
            association.DateTime = this.DateTime;
            association.Operation = this.Operation;
            association.Option = AssociationOption.NotByPeriod.Add (this.Option);
            association.Caller = this;
            association.Analyze ();
          }
          Debug.Assert (Bound.Compare<DateTime> (this.Range.Lower, noChangeRequiredSlot.DateTimeRange.Upper) < 0);
          Debug.Assert (Bound.Compare<DateTime> (lower, noChangeRequiredSlot.DateTimeRange.Upper) < 0);
          lower = noChangeRequiredSlot.DateTimeRange.Upper;
        }
        if (Bound.Compare<DateTime> (lower, this.Range.Upper) < 0) {
          UtcDateTimeRange range = new UtcDateTimeRange ((LowerBound<DateTime>)lower,
                                                         this.Range.Upper);
          Debug.Assert (this.Range.ContainsRange (range));
          OperationMachineAssociation association = (OperationMachineAssociation)ModelDAOHelper.ModelFactory
            .CreateOperationMachineAssociation (this.Machine, range,
                                                this.MainModification,
                                                m_partOfDetectionAnalysis);
          association.DateTime = this.DateTime;
          association.Operation = this.Operation;
          association.Option = AssociationOption.NotByPeriod.Add (this.Option);
          association.Caller = this;
          association.Analyze ();
        }
      }
    }
    
    bool IsProcessByPeriod ()
    {
      if (this.Range.Duration.HasValue
          && (this.Range.Duration.Value < Lemoine.Info.ConfigSet.LoadAndGet<TimeSpan> (BY_PERIOD_MIN_DURATION_KEY,
                                                                                       BY_PERIOD_MIN_DURATION_DEFAULT))) {
        return false;
      }
      return !this.Option.HasValue || !this.Option.Value.HasFlag (AssociationOption.NotByPeriod);
    }

    UtcDateTimeRange GetNoChangeRequiredRange ()
    {
      if (AnalysisConfigHelper.OperationSlotSplitOption.IsActive ()) {
        IOperationSlotSplit operationSlotSplit = ModelDAOHelper.DAOFactory.OperationSlotSplitDAO
          .FindById (this.Machine.Id);
        if (null != operationSlotSplit) {
          return new UtcDateTimeRange (this.Range
                                       .Intersects (new UtcDateTimeRange (new LowerBound<DateTime> (),
                                                                          operationSlotSplit.End)));
        }
      }
      return this.Range;
    }

    bool GetDataStructureOption (DataStructureConfigKey key)
    {
      return Lemoine.Info.ConfigSet
        .Get<bool> (ConfigKeys.GetDataStructureConfigKey (key));
    }

    /// <summary>
    /// Analyze directly the modification without updating it status.
    /// 
    /// The option is not taken into account here.
    /// 
    /// Insert all the slots that may correspond to this modification
    /// taking into account the auto-sequence table
    /// 
    /// Note the DateTime property of the modification must be correctly
    /// set to use this
    /// </summary>
    void Analyze ()
    {
      // - Check the slots are processed in the range to avoid some side effects
      //   in the preFetchedOperationSlots optimization
      CheckSlotProcessed ();
      
      // Complete WorkOrder / Line / Task / Component if applicable
      
      // - Machine + Operation => Line
      if ( (null == m_line) && (null != this.Operation)
          && AnalysisConfigHelper.LineManagement
          && AnalysisConfigHelper.LineFromMachineOperation) {
        IList<ILineMachine> lineMachines = ModelDAOHelper.DAOFactory.LineMachineDAO
          .FindAllByMachineOperation (this.Machine, this.Operation);
        if (1 < lineMachines.Count) {
          log.DebugFormat ("Analyze: " +
                           "more than one line is defined for operation {0}",
                           this.Operation);
        }
        else if (1 == lineMachines.Count) {
          foreach (ILineMachine lineMachine in lineMachines) {
            log.DebugFormat ("Analyze: " +
                             "get one line id={1} for operation {0}",
                             this.Operation, lineMachine.Line.Id);
            m_line = lineMachine.Line;
          }
        }
      }
      SetActive ();
      
      // - Line => Component
      if ( (null == m_component) && (null != m_line)) {
        Debug.Assert (null == m_component);
        ICollection<IComponent> components = m_line.Components;
        foreach (IComponent component in components) {
          if (Lemoine.Business.Operation.OperationRelations.IsComponentCompatibleWithOperation (component, this.Operation)) {
            log.DebugFormat ("Analyze: " +
                             "component {0} from line is compatible with the operation {1}",
                             component, this.Operation);
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
      }
      SetActive ();
      
      // - Operation => Component
      if ((null == m_line) && (null == m_component) && (null != this.Operation)) {
        if (GetDataStructureOption (DataStructureConfigKey.UniqueComponentFromOperation)) {
          m_component =
            Lemoine.Business.Operation.OperationRelations.GuessUniqueComponentFromOperation (this.Operation);
        }
        else {
          m_component =
            Lemoine.Business.Operation.OperationRelations.TryToGuessComponentFromOperation (this.Operation);
        }
      }
      SetActive ();
      
      // - Operation => WorkOrder
      if ((null == m_line)
          && (null == m_workOrder)
          && (null != this.Operation)) {
        if (null != m_component) { // Component => WorkOrder
          if (GetDataStructureOption (DataStructureConfigKey.UniqueWorkOrderFromProjectOrComponent)) {
            m_workOrder = Lemoine.Business.Operation.OperationRelations.GuessUniqueWorkOrderFromComponent (m_component);
          }
          else {
            m_workOrder = Lemoine.Business.Operation.OperationRelations.TryToGuessWorkOrderFromComponent (m_component);
          }
        }
        else { // null == m_component
          if (GetDataStructureOption (DataStructureConfigKey.UniqueWorkOrderFromProjectOrComponent)
            && GetDataStructureOption (DataStructureConfigKey.UniqueComponentFromOperation)) {
            m_workOrder = Lemoine.Business.Operation.OperationRelations.GuessUniqueWorkOrderFromOperation (this.Operation);
          }
          else {
            m_workOrder = Lemoine.Business.Operation.OperationRelations.TryToGuessWorkOrderFromOperation (this.Operation);
          }
        }
      }
      SetActive ();
      
      {
        // Adapt the existing auto-sequences to this modification
        // in case the auto-sequences occurred before this modification
        //
        // In this case (the date of the modification is after
        // the begin of the auto-sequence),
        // cut the corresponding auto-sequences,
        // and keep only the auto-sequences whose new begin date/time
        // is before the date/time of the modification
        // TODO: to write
      }
      
      {
        // Adapt the association to the auto-sequences events
        // that occured after the modification but that were processed before
        //
        // In this case (the date of the modification is before
        // the begin of the auto-sequence),
        // cut the association to the begin of the auto-sequence
        // 
        // TODO: [DAO] use the AutoSequenceDAO once it exists
        // TODO: there is probably an error in the query below
        /*
        IList<AutoSequence> autoSequences =
          session.CreateQuery (@"from AutoSequence autoSequence
where autoSequence.Begin>=:dateTime and autoSequence.Begin<:end
and autoSequence.MachineModule.MonitoredMachine=:machine
and autoSequence.Sequence.Operation<>:operation")
          .SetParameter ("dateTime", this.DateTime)
          .SetParameter ("end", this.End)
          .SetParameter ("machine", this.Machine)
          .SetParameter ("operation", this.Operation)
          .List<AutoSequence> ();
        foreach (AutoSequence autoSequence in autoSequencees) {
          // TODO: to write, with a corrected EndDateTime
        }
         */
      }
      
      // - Line => WorkOrder
      if ((null != m_line) && (null == m_workOrder)) {
        IList<IWorkOrderLine> workOrderLines = ModelDAOHelper.DAOFactory.WorkOrderLineDAO
          .GetListInRange (m_line, this.Range);
        if (1 == workOrderLines.Count) {
          IWorkOrderLine workOrderLine = workOrderLines [0];
          Debug.Assert (null != workOrderLine.WorkOrder);
          if (workOrderLine.DateTimeRange.ContainsRange (this.Range)) { // On the whole period
            log.DebugFormat ("Analyze: " +
                             "got work order id={0} from line id={1}",
                             ((Lemoine.Collections.IDataWithId)workOrderLine.WorkOrder).Id, m_line.Id);
            if (Lemoine.Business.Operation.OperationRelations.IsWorkOrderCompatibleWithOperation (workOrderLine.WorkOrder, this.Operation)) {
              m_workOrder = workOrderLine.WorkOrder;
            }
            else {
              log.WarnFormat ("Analyze: " +
                              "got work order id={0} from line id={1} " +
                              "but it is not compatible with operation={2}",
                              ((Lemoine.Collections.IDataWithId)workOrderLine.WorkOrder).Id, m_line.Id, this.Operation);
            }
          }
        }
        if ( (null == m_workOrder) && (1 <= workOrderLines.Count)) { // Split by date
          log.DebugFormat ("Analyze: " +
                           "more than one work order may correspond to line {0} in period {1}",
                           m_line.Id, this.Range);
          Bound<DateTime> begin = this.Begin;
          IList<IOperationSlot> preFetchedOperationSlots = null;
          foreach (IWorkOrderLine workOrderLine in workOrderLines) {
            Debug.Assert (null != workOrderLine.WorkOrder);
            SetActive ();
            if (!Lemoine.Business.Operation.OperationRelations.IsWorkOrderCompatibleWithOperation (workOrderLine.WorkOrder, this.Operation)) {
              log.WarnFormat ("Analyze: " +
                              "got work order id={0} from line id={1} " +
                              "but it is not compatible with operation={2}",
                              ((Lemoine.Collections.IDataWithId)workOrderLine.WorkOrder).Id, m_line.Id, this.Operation);
              continue;
            }
            if (Bound.Compare<DateTime> (begin, workOrderLine.BeginDateTime) < 0) {
              Debug.Assert (workOrderLine.BeginDateTime.HasValue);
              UtcDateTimeRange range = new UtcDateTimeRange ((LowerBound<DateTime>) begin,
                                                             workOrderLine.BeginDateTime.Value);
              if (log.IsDebugEnabled) {
                log.Debug ($"Analyze: no work order for line {m_line?.Id} in period {begin}-{workOrderLine.BeginDateTime}");
              }
              OperationMachineAssociation association = new OperationMachineAssociation (this.Machine,
                                                                                         range,
                                                                                         this.MainModification,
                                                                                         this.PartOfDetectionAnalysis);
              association.Caller = this;
              association.Option = this.Option;
              association.Operation = this.Operation;
              association.Line = m_line;
              association.Component = m_component;
              UpdatePreFetchedOperationSlots (range, ref preFetchedOperationSlots);
              association.InsertCorrespondingSlots (preFetchedOperationSlots);
            }
            UtcDateTimeRange intersection =
              new UtcDateTimeRange (this.Range.Intersects (workOrderLine.DateTimeRange));
            if (intersection.IsEmpty ()) {
              Debug.Assert (false);
              log.Fatal ($"Analyze: empty intersection of {this} with {workOrderLine}");
            }
            else { // !intersection.IsEmpty ()
              if (log.IsDebugEnabled) {
                log.Debug ($"Analyze: work order {((Lemoine.Collections.IDataWithId)workOrderLine.WorkOrder)?.Id} for line {m_line?.Id} in period {intersection}");
              }
              OperationMachineAssociation association = new OperationMachineAssociation (this.Machine,
                                                                                         intersection,
                                                                                         this.MainModification,
                                                                                         this.PartOfDetectionAnalysis);
              association.Caller = this;
              association.Option = this.Option;
              association.Operation = this.Operation;
              association.WorkOrder = workOrderLine.WorkOrder;
              association.Line = m_line;
              association.Component = m_component;
              UpdatePreFetchedOperationSlots (intersection, ref preFetchedOperationSlots);
              association.InsertCorrespondingSlots (preFetchedOperationSlots);
              if (!intersection.Upper.HasValue) {
                break;
              }
              Debug.Assert (intersection.Upper.HasValue);
              begin = intersection.Upper;
            }
          } // end loop
          if (Bound.Compare<DateTime> (begin, this.End) < 0) {
            UtcDateTimeRange range = new UtcDateTimeRange ((LowerBound<DateTime>) begin, this.End);
            if (log.IsDebugEnabled) {
              log.Debug ($"Analyze: no work order for line {m_line?.Id} in period {range}");
            }
            OperationMachineAssociation association = new OperationMachineAssociation (this.Machine,
                                                                                       range,
                                                                                       this.MainModification,
                                                                                       this.PartOfDetectionAnalysis);
            association.Caller = this;
            association.Option = this.Option;
            association.Operation = this.Operation;
            association.Line = m_line;
            association.Component = m_component;
            UpdatePreFetchedOperationSlots (range, ref preFetchedOperationSlots);
            association.InsertCorrespondingSlots (preFetchedOperationSlots);
          }
          return;
        } // split by date
      } // Line => WorkOrder
      SetActive ();
      
      // Insert the slots
      InsertCorrespondingSlots ();
    }
    
    /// <summary>
    /// Insert all the slots that may correspond to this modification
    /// </summary>
    protected virtual void InsertCorrespondingSlots ()
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
        log.Debug ("InsertCorrespondingSlots: one of the split option is active => determine a splitRange");
        UtcDateTimeRange splitRange = this.Range;
        IOperationSlotSplit operationSlotSplit = ModelDAOHelper.DAOFactory.OperationSlotSplitDAO
          .FindById (this.Machine.Id);
        if ( (null != operationSlotSplit)
            && (Bound.Compare<DateTime> (operationSlotSplit.End, splitRange.Upper) < 0)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"InsertCorrespondingSlots: restrict the split range to {operationSlotSplit.End}");
          }
          splitRange = new UtcDateTimeRange (splitRange.Lower, operationSlotSplit.End);
        }
        
        // - Split the operation slot by day and/or shift
        log.Debug ("InsertCorrespondingSlots: Split operation option");
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
            Debug.Assert (slot.DateTimeRange.Overlaps (splitRange));
            UtcDateTimeRange slotRange = new UtcDateTimeRange (slot.DateTimeRange.Intersects (splitRange));
            Debug.Assert (!slotRange.IsEmpty ());
            SetActive ();
            if (Bound.Compare<DateTime> (begin, slotRange.Lower) < 0) { // No shift data gap to process
              Debug.Assert (false);
              Debug.Assert (slotRange.Lower.HasValue);
              UtcDateTimeRange range = new UtcDateTimeRange ((LowerBound<DateTime>)begin, slotRange.Lower.Value);
              log.Fatal ($"InsertCorrespondingSlots: there should be no gap in shift slots any more but process no shift data gap operation={this.Operation?.Id} period={range}");
              OperationShiftMachineAssociation noShiftAssociation = new OperationShiftMachineAssociation (this.Machine,
                                                                                                          range,
                                                                                                          this,
                                                                                                          this.PartOfDetectionAnalysis);
              noShiftAssociation.Caller = this;
              noShiftAssociation.Option = this.Option;
              if (Bound.Compare<DateTime> (range.Upper, this.End) < 0) {
                noShiftAssociation.Option = AssociationOption.NoRightMerge.Add (noShiftAssociation.Option);
              }
              noShiftAssociation.Operation = this.Operation;
              noShiftAssociation.WorkOrder = m_workOrder;
              noShiftAssociation.Line = m_line;
              noShiftAssociation.Task = m_task;
              noShiftAssociation.Component = m_component;
              noShiftAssociation.Shift = null;
              UpdatePreFetchedOperationSlots (range, ref preFetchedOperationSlots);
              noShiftAssociation.Analyze (preFetchedOperationSlots);
            }
            var intersection = new UtcDateTimeRange (this.Range.Intersects (slotRange));
            if (intersection.IsEmpty ()) {
              Debug.Assert (false);
              log.Fatal ($"InsertCorrespondingSlots: empty intersection of {this} with {slotRange}");
            }
            else { // !intersection.IsEmpty ()
              if (log.IsDebugEnabled) {
                log.Debug ($"InsertCorrespondingSlots: process operation={this.Operation?.Id} shift={slot.Shift?.Id} period={intersection}");
              }
              var association = new OperationShiftMachineAssociation (this.Machine,
                                                                                                   intersection,
                                                                                                   this,
                                                                                                   this.PartOfDetectionAnalysis);
              association.Caller = this;
              association.Option = this.Option;
              if (Bound.Compare<DateTime> (this.Begin, intersection.Lower) < 0) {
                association.Option = AssociationOption.NoLeftMerge.Add (association.Option);
              }
              if (Bound.Compare<DateTime> (intersection.Upper, this.End) < 0) {
                association.Option = AssociationOption.NoRightMerge.Add (association.Option);
              }
              association.Operation = this.Operation;
              association.WorkOrder = m_workOrder;
              association.Line = m_line;
              association.Task = m_task;
              association.Component = m_component;
              association.Day = slot.Day;
              association.Shift = slot.Shift;
              UpdatePreFetchedOperationSlots (intersection, ref preFetchedOperationSlots);
              association.Analyze (preFetchedOperationSlots);
              begin = intersection.Upper;
            }
          }
        }
        if (Bound.Compare<DateTime> (begin, this.End) < 0) {
          UtcDateTimeRange range = new UtcDateTimeRange ((LowerBound<DateTime>)begin, this.End);
          Debug.Assert (!range.IsEmpty ());
          Debug.Assert (Bound.Compare<DateTime> (this.Begin, range.Lower) <= 0);
          if (splitRange.ContainsRange (this.Range)) {
            Debug.Assert (false);
            log.Fatal ($"InsertCorrespondingSlots: there should be no gap in shift slots any more but process a last no shift data gap operation={this.Operation?.Id} period={range}");
          } // Else this is the part of operationslotsplit
          OperationShiftMachineAssociation association = new OperationShiftMachineAssociation (this.Machine,
                                                                                               range,
                                                                                               this,
                                                                                               this.PartOfDetectionAnalysis);
          association.Caller = this;
          association.Option = this.Option;
          if (Bound.Compare<DateTime> (this.Begin, begin) < 0) {
            association.Option = AssociationOption.NoLeftMerge.Add (association.Option);
          }
          association.Operation = this.Operation;
          association.WorkOrder = m_workOrder;
          association.Line = m_line;
          association.Task = m_task;
          association.Component = m_component;
          association.Shift = null;
          UpdatePreFetchedOperationSlots (range, ref preFetchedOperationSlots);
          association.Analyze (preFetchedOperationSlots);
        }
      }
      else { // No split option
        OperationShiftMachineAssociation association = new OperationShiftMachineAssociation (this.Machine,
                                                                                             this.Range,
                                                                                             this,
                                                                                             this.PartOfDetectionAnalysis);
        association.Caller = this;
        association.Option = this.Option;
        association.Operation = this.Operation;
        association.WorkOrder = m_workOrder;
        association.Line = m_line;
        association.Task = m_task;
        association.Component = m_component;
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
      NHibernateHelper.Unproxy<IOperation> (ref m_operation);
    }
  }
}
