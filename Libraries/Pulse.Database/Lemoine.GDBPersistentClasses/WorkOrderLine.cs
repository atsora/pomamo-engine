// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using System.Xml.Serialization;

using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.Core.Log;
using Lemoine.Collections;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Description of WorkOrderLine.
  /// </summary>
  public class WorkOrderLine: GenericLineSlot, IVersionable, IWorkOrderLine
  {
    #region Members
    IWorkOrder m_workOrder = null;
    DateTime m_deadline;
    int m_quantity = 0;
    IDictionary<int, IWorkOrderLineQuantity> m_intermediateWorkPieceQuantities = new Dictionary<int, IWorkOrderLineQuantity> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (WorkOrderLine).FullName);

    #region Getters / Setters
    /// <summary>
    /// WorkOrder (not null)
    /// </summary>
    [XmlIgnore]
    public virtual IWorkOrder WorkOrder
    {
      get { return m_workOrder; }
      protected internal set {
        Debug.Assert (null != value);
        if (value == null) {
          log.FatalFormat("WorkOrder cannot be null");
          throw new ArgumentNullException("WorkOrder");
        }
        m_workOrder = value;
      }
    }
    
    /// <summary>
    /// Production deadline of a line for a specific workorder
    /// </summary>
    [XmlIgnore]
    public virtual DateTime Deadline
    {
      get { return m_deadline; }
      set { m_deadline = value; }
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
    public virtual IDictionary<int, IWorkOrderLineQuantity> IntermediateWorkPieceQuantities {
      get { return m_intermediateWorkPieceQuantities; }
    }
    #endregion // Getters / Setters

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
      
      if (0 == quantity) {
        this.IntermediateWorkPieceQuantities.Remove (((Lemoine.Collections.IDataWithId)intermediateWorkPiece).Id);
      }
      else {
        IWorkOrderLineQuantity workOrderQuantity;
        if (this.IntermediateWorkPieceQuantities.TryGetValue (((Lemoine.Collections.IDataWithId)intermediateWorkPiece).Id, out workOrderQuantity)) {
          workOrderQuantity.Quantity = quantity;
        }
        else {
          workOrderQuantity = new WorkOrderLineQuantity (this.Line, intermediateWorkPiece);
          workOrderQuantity.Quantity = quantity;
          this.IntermediateWorkPieceQuantities [((Lemoine.Collections.IDataWithId)intermediateWorkPiece).Id] = workOrderQuantity;
        }
      }
    }

    /// <summary>
    /// Add a quantity of intermediate work pieces
    /// </summary>
    /// <param name="intermediateWorkPiece">not null</param>
    /// <param name="quantity"></param>
    public virtual void AddIntermediateWorkPieceQuantity (IIntermediateWorkPiece intermediateWorkPiece,
                                                          int quantity)
    {
      Debug.Assert (null != intermediateWorkPiece);
      Debug.Assert (0 != ((Lemoine.Collections.IDataWithId)intermediateWorkPiece).Id);
      
      if (0 != quantity) {
        IWorkOrderLineQuantity workOrderQuantity;
        if (this.IntermediateWorkPieceQuantities.TryGetValue (((Lemoine.Collections.IDataWithId)intermediateWorkPiece).Id, out workOrderQuantity)) {
          workOrderQuantity.Quantity += quantity;
        }
        else {
          workOrderQuantity = new WorkOrderLineQuantity (this.Line, intermediateWorkPiece);
          workOrderQuantity.Quantity = quantity;
          this.IntermediateWorkPieceQuantities [((Lemoine.Collections.IDataWithId)intermediateWorkPiece).Id] = workOrderQuantity;
        }
      }
    }
    
    #region Constructors
    /// <summary>
    /// Forbidden default constructor, only reachable by NHibernate
    /// </summary>
    protected WorkOrderLine()
    {
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="line"></param>
    /// <param name="range"></param>
    public WorkOrderLine (ILine line,
                          UtcDateTimeRange range)
      : base (line, range)
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="line"></param>
    /// <param name="range"></param>
    /// <param name="workOrder"></param>
    internal protected WorkOrderLine (ILine line,
                                      UtcDateTimeRange range,
                                      IWorkOrder workOrder)
      : base (line, range)
    {
      this.WorkOrder = workOrder;
    }
    #endregion // Constructors
    
    #region Slot implementation
    /// <summary>
    /// IComparable implementation
    /// <see cref="IComparable.CompareTo" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override int CompareTo(object obj)
    {
      if (obj is WorkOrderLine) {
        IWorkOrderLine other = (IWorkOrderLine) obj;
        if (other.Line.Equals (this.Line)) {
          return this.BeginDateTime.CompareTo (other.BeginDateTime);
        }
        else {
          log.ErrorFormat ("CompareTo: " +
                           "trying to compare line slots " +
                           "for different lines {0} {1}",
                           this, other);
          throw new ArgumentException ("Comparison of WorkOrderLine from different lines");
        }
      }
      
      log.ErrorFormat ("CompareTo: " +
                       "object {0} of invalid type",
                       obj);
      throw new ArgumentException ("object is not a WorkOrderLine");
    }

    /// <summary>
    /// IComparable implementation
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual int CompareTo(IWorkOrderLine other)
    {
      if (other.Line.Equals (this.Line)) {
        return this.BeginDateTime.CompareTo (other.BeginDateTime);
      }

      log.ErrorFormat ("CompareTo: " +
                       "trying to compare line slots " +
                       "for different line {0} {1}",
                       this, other);
      throw new ArgumentException ("Comparison of WorkOrderLine from different lines");
    }
    
    /// <summary>
    /// <see cref="Object.GetHashCode" />
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
      int hashCode = 0;
      unchecked {
        hashCode += 1000000007 * BeginDateTime.GetHashCode();
        if (this.Line != null) {
          hashCode += 1000000009 * this.Line.GetHashCode();
        }

        if (this.WorkOrder != null) {
          hashCode += 1000000011 * this.WorkOrder.GetHashCode();
        }
      }
      return hashCode;
    }
    
    /// <summary>
    /// <see cref="Object.Equals(object)" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object obj)
    {
      WorkOrderLine other = obj as WorkOrderLine;
      if (other == null) {
        return false;
      }

      return Bound.Equals<DateTime> (this.BeginDateTime, other.BeginDateTime)
        && object.Equals(this.Line, other.Line)
        && object.Equals(this.WorkOrder, other.WorkOrder);
    }
    
    /// <summary>
    /// <see cref="Slot.Consolidate()" />
    /// </summary>
    protected override void Consolidate()
    {
      // If the deadline is after the new end date/time, change the deadline after raising a log
      if (Bound.Compare<DateTime> (this.EndDateTime, this.Deadline) < 0) {
        Debug.Assert (this.EndDateTime.HasValue);
        log.WarnFormat ("Consolidate: " +
                        "move the deadline from {0} to {1} because it was after the end date/time",
                        this.Deadline, this.EndDateTime.Value);
        // TODO: add a log in analysislog
        //       but what is currently missing is a reference to the associated modification
        this.Deadline = this.EndDateTime.Value;
      }
    }    
    
    /// <summary>
    /// <see cref="Slot.HandleAddedSlot" />
    /// </summary>
    public override void HandleAddedSlot ()
    {
      // There is nothing to do for this slot
    }
    
    /// <summary>
    /// <see cref="Slot.HandleRemovedSlot" />
    /// </summary>
    public override void HandleRemovedSlot ()
    {
      // There is nothing to do for this slot
    }
    
    /// <summary>
    /// <see cref="Slot.HandleModifiedSlot" />
    /// </summary>
    /// <param name="oldSlot"></param>
    public override void HandleModifiedSlot (ISlot oldSlot)
    {
      // There is nothing to do for this slot
    }

    /// <summary>
    /// <see cref="Slot.IsEmpty" />
    /// </summary>
    /// <returns></returns>
    public override bool IsEmpty()
    {
      return (null == this.WorkOrder);
    }
    
    /// <summary>
    /// <see cref="Slot.ReferenceDataEquals" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool ReferenceDataEquals(ISlot obj)
    {
      IWorkOrderLine other = obj as IWorkOrderLine;
      if (other == null) {
        return false;
      }

      return NHibernateHelper.EqualsNullable (this.Line, other.Line, (a, b) => a.Id == b.Id)
        && NHibernateHelper.EqualsNullable (this.WorkOrder, other.WorkOrder, (a, b) => ((IDataWithId)a).Id == ((IDataWithId)b).Id);
    }
    #endregion // Slot implementation
    
    #region ICloneable implementation
    /// <summary>
    /// Make a shallow copy
    /// <see cref="ICloneable.Clone" />
    /// </summary>
    /// <returns></returns>
    public override object Clone()
    {
      WorkOrderLine clone = base.Clone() as WorkOrderLine;
      clone.m_intermediateWorkPieceQuantities.Clear ();
      foreach (var iwpQuantity in this.IntermediateWorkPieceQuantities.Values) {
        clone.AddIntermediateWorkPieceQuantity (iwpQuantity.IntermediateWorkPiece, iwpQuantity.Quantity);
      }
      return clone;
    }
    #endregion // ICloneable implementation

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[WorkOrderLine {this.Id} {this.Line?.Id} Range={this.DateTimeRange}]";
      }
      else {
        return $"[WorkOrderLine {this.Id}]";
      }
    }
  }
}
