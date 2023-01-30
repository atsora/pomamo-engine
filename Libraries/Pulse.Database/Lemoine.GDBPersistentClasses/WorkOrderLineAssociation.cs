// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Xml.Serialization;

using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Modification class to easily process and insert the coming workorder
  /// within a line
  /// </summary>
  [Serializable]
  public class WorkOrderLineAssociation: LineAssociation, IWorkOrderLineAssociation
  {
    #region Members
    IWorkOrder m_workOrder;
    DateTime m_deadline;
    int m_quantity = 0;
    IDictionary<int, IWorkOrderLineAssociationQuantity> m_intermediateWorkPieceQuantities = new Dictionary<int, IWorkOrderLineAssociationQuantity> ();
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// <see cref="IModification"/>
    /// </summary>
    [XmlIgnore]
    public override string ModificationType
    {
      get { return "WorkOrderLineAssociation"; }
    }

    /// <summary>
    /// Reference to the WorkOrder
    /// 
    /// Nullable
    /// </summary>
    [XmlIgnore]
    public virtual IWorkOrder WorkOrder {
      get { return m_workOrder; }
      set { m_workOrder = value; }
    }
    
    /// <summary>
    /// Reference to the WorkOrder for Xml Serialization
    /// </summary>
    [XmlElement("WorkOrder")]
    public virtual WorkOrder XmlSerializationWorkOrder {
      get { return m_workOrder as WorkOrder; }
      set { m_workOrder = value; }
    }
    
    /// <summary>
    /// local deadline of the association
    /// used for setting UTC deadline using a local time
    /// </summary>
    [XmlAttribute("LocalDeadline")]
    public virtual string LocalDeadline {
      get {
        return Deadline.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
      }
      
      set {
        if (string.IsNullOrEmpty (value)) {
          log.WarnFormat ("LocalDeadline.set: " +
                          "null or empty value {0} " +
                          "=> do not update the deadline",
                          value);
        }
        else {
          IFormatProvider provider = CultureInfo.InvariantCulture;
          this.Deadline = System.DateTime.Parse (value, provider,
                                                 DateTimeStyles.AssumeLocal
                                                 | DateTimeStyles.AdjustToUniversal);
        }
      }
    }

    /// <summary>
    /// LocalDeadline is never serialized
    /// </summary>
    public virtual bool LocalDeadlineSpecified{ get { return false; } }

    /// <summary>
    /// UTC deadline of the association
    /// </summary>
    [XmlIgnore]
    public virtual DateTime Deadline {
      get { return m_deadline; }
      protected set { m_deadline = value; }
    }

    /// <summary>
    /// UTC deadline in SQL string for XML serialization
    /// </summary>
    [XmlAttribute("Deadline")]
    public virtual string SqlDeadline {
      get
      {
        return this.Deadline.ToString ("yyyy-MM-dd HH:mm:ss");
      }
      set
      {
        IFormatProvider provider = CultureInfo.InvariantCulture;
        this.Deadline = System.DateTime.Parse (value, provider,
                                               DateTimeStyles.AssumeUniversal
                                               | DateTimeStyles.AdjustToUniversal);
      }
    }

    /// <summary>
    /// Component quantity that has to be produced between the start datetime and
    /// the end date time for the line
    /// </summary>
    [XmlAttribute("Quantity")]
    public virtual int Quantity
    {
      get { return m_quantity; }
      set { m_quantity = value; }
    }
    
    /// <summary>
    /// Map Intermediate Work Piece => Quantity
    /// 
    /// The key is the intermediate work piece id
    /// </summary>
    [XmlIgnore]
    public virtual IDictionary<int, IWorkOrderLineAssociationQuantity> IntermediateWorkPieceQuantities {
      get { return m_intermediateWorkPieceQuantities; }
    }
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    protected WorkOrderLineAssociation()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="line"></param>
    /// <param name="begin"></param>
    /// <param name="deadline"></param>
    public WorkOrderLineAssociation(ILine line, DateTime begin, DateTime deadline) : base(line, begin)
    {
      this.Deadline = deadline;
    }
    
    /// <summary>
    /// Constructor for abstract modifications, that are kept transient
    /// 
    /// You may add a mainModification reference to be used in the analysis logs
    /// </summary>
    /// <param name="line"></param>
    /// <param name="begin"></param>
    /// <param name="deadline"></param>
    /// <param name="mainModification"></param>
    public WorkOrderLineAssociation (ILine line, DateTime begin, DateTime deadline, IModification mainModification)
      : base(line, begin, mainModification)
    {
      this.Deadline = deadline;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Make the analysis
    /// </summary>
    public override void MakeAnalysis()
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

      // Guess the end date/time if end date/time is not specified
      if (!this.End.HasValue) {
        IWorkOrderLine next = ModelDAOHelper.DAOFactory.WorkOrderLineDAO
          .FindFirstAfter (this.Line, this.Deadline);
        if (null != next) {
          log.DebugFormat ("ProcessAssociation: " +
                           "adjust end to {0}",
                           next.BeginDateTime);
          Debug.Assert (next.BeginDateTime.HasValue);
          this.End = next.BeginDateTime.Value;
        }
      }
      
      // Associate the work order to all the components that were associated to the line
      // Note: I do not know exactly yet why it is necessary to lock this.Line
      //       Is it because this.Line is in the parent class ?
      ModelDAOHelper.DAOFactory.LineDAO.Lock (this.Line);
      if (null != this.WorkOrder) {
        foreach (IComponent component in this.Line.Components) {
          if (!Lemoine.Business.Operation.OperationRelations.IsWorkOrderCompatibleWithComponent (this.WorkOrder, component)) {
            log.DebugFormat ("ProcessAssociation: " +
                             "associate work order {0} with {1}",
                             this.WorkOrder, component);
            component.Project.AddWorkOrder (this.WorkOrder);
          }
        }
      }
      
      // Update WorkOrderLine
      WorkOrderLineDAO workOrderLineDAO = new WorkOrderLineDAO ();
      workOrderLineDAO.Caller = this;
      Insert<WorkOrderLine, IWorkOrderLine, WorkOrderLineDAO> (workOrderLineDAO, false);
      
      { // For the dedicated machines, set the work order (asynchronously)
        IList<ILineMachine> lineMachines = ModelDAOHelper.DAOFactory.LineMachineDAO
          .FindAllByLine (this.Line);
        foreach (ILineMachine lineMachine in lineMachines) {
          if (lineMachine.LineMachineStatus.Equals (LineMachineStatus.Dedicated)) {
            log.DebugFormat ("ProcessAssociation: " +
                             "associate {0} to machine {1}",
                             this.WorkOrder, lineMachine.Machine);
            IWorkOrderMachineAssociation association = ModelDAOHelper.ModelFactory
              .CreateWorkOrderMachineAssociation (lineMachine.Machine, this.WorkOrder, this.Range);
            association.Line = this.Line;
            association.Parent = this.MainModification ?? this;
            association.Priority = this.StatusPriority;
            ModelDAOHelper.DAOFactory.WorkOrderMachineAssociationDAO.MakePersistent (association);
          }
        }
      }
      
      MarkAsCompleted ("Cache/ClearDomain/WorkOrderLineAssociation?Broadcast=true");
    }
    
    /// <summary>
    /// Apply the modification while keeping it transient
    /// 
    /// It should be never called, because there is no transient modification to process.
    /// Use a persistent entity instead and MakeAnalysis.
    /// </summary>
    public override void Apply ()
    {
      Debug.Assert (false);
      log.FatalFormat ("Apply: not implemented/supported");
      throw new NotImplementedException ();
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
      Debug.Assert (oldSlot is GenericLineSlot);
      Debug.Assert (this.Line.Id == (oldSlot as GenericLineSlot).Line.Id); // Keep it lazy
      
      if (oldSlot is WorkOrderLine) {
        if (null == this.WorkOrder) {
          return null;
        }
        else {
          WorkOrderLine slot = GenericLineSlot.Create (typeof (WorkOrderLine), this.Line, this.Range) as WorkOrderLine;
          slot.Consolidated = false;
          slot.Deadline = this.Deadline;
          slot.WorkOrder = this.WorkOrder;
          slot.Quantity = this.Quantity;
          foreach (IWorkOrderLineAssociationQuantity workOrderLineQuantity in this.m_intermediateWorkPieceQuantities.Values) {
            slot.SetIntermediateWorkPieceQuantity (workOrderLineQuantity.IntermediateWorkPiece, workOrderLineQuantity.Quantity);
          }
          return slot as TSlot;
        }
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
    /// <see cref="PeriodAssociation.ConvertToSlot" />
    /// </summary>
    /// <returns></returns>
    public override TSlot ConvertToSlot<TSlot> ()
    {
      if (m_workOrder == null) {
        return null;
      }
      else {
        Debug.Assert (null != this.Line);
        var slot = GenericLineSlot.Create (typeof (TSlot), this.Line, this.Range) as TSlot;
        Debug.Assert (null != slot);
        slot.Consolidated = false;
        
        // disable once CanBeReplacedWithTryCastAndCheckForNull
        if (slot is IWorkOrderLine) {
          var workOrderLine = slot as IWorkOrderLine;
          ((WorkOrderLine)workOrderLine).WorkOrder = this.WorkOrder;
          workOrderLine.Deadline = this.Deadline;
          workOrderLine.Quantity = this.Quantity;
          foreach (IWorkOrderLineAssociationQuantity workOrderLineQuantity in this.m_intermediateWorkPieceQuantities.Values) {
            workOrderLine.SetIntermediateWorkPieceQuantity (workOrderLineQuantity.IntermediateWorkPiece, workOrderLineQuantity.Quantity);
          }
          return slot;
        }
        else {
          Debug.Assert (false);
          throw new NotImplementedException ("Slot type not implemented");
        }
      }
    }
    
    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public override void Unproxy ()
    {
      base.Unproxy ();
      NHibernateHelper.Unproxy<IWorkOrder> (ref m_workOrder);
    }
    
    /// <summary>
    /// Set a quantity of intermediate work pieces
    /// </summary>
    /// <param name="intermediateWorkPiece">not null</param>
    /// <param name="quantity"></param>
    public virtual void SetIntermediateWorkPieceQuantity (IIntermediateWorkPiece intermediateWorkPiece,
                                                          int quantity)
    {
      Debug.Assert (null != intermediateWorkPiece);
      Debug.Assert (0 != ((Lemoine.Collections.IDataWithId)intermediateWorkPiece).Id);
      
      IWorkOrderLineAssociationQuantity workOrderQuantity;
      if (this.IntermediateWorkPieceQuantities.TryGetValue (((Lemoine.Collections.IDataWithId)intermediateWorkPiece).Id, out workOrderQuantity)) {
        workOrderQuantity.Quantity = quantity;
      }
      else {
        workOrderQuantity = new WorkOrderLineAssociationQuantity (intermediateWorkPiece);
        workOrderQuantity.Quantity = quantity;
        this.IntermediateWorkPieceQuantities [((Lemoine.Collections.IDataWithId)intermediateWorkPiece).Id] = workOrderQuantity;
      }
    }
    #endregion // Methods
  }
}
