// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

using Lemoine.Model;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.GDBPersistentClasses;
using System.Reflection;
using System.Linq;
using System.Runtime.CompilerServices;
using Lemoine.Collections;

namespace Lemoine.Plugin.HourlyOperationSummary
{
  /// <summary>
  /// Persistent class of table IntermediateWorkPieceSummary
  /// 
  /// It contains various information on an intermediate work piece
  /// </summary>
  [Serializable]
  public class HourlyOperationSummary : BaseData, IHourlyOperationSummary, IVersionable
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    IMachine m_machine;
    IOperation m_operation;
    IComponent m_component = null;
    IWorkOrder m_workOrder = null;
    ILine m_line = null;
    IManufacturingOrder m_manufacturingOrder = null;
    [NonSerialized]
    DateTime? m_day = null;
    IShift m_shift = null;
    [NonSerialized]
    DateTime m_localDateHour;
    [NonSerialized]
    TimeSpan m_duration;
    int m_totalCycles = 0;
    int m_adjustedCycles = 0;
    int m_adjustedQuantity = 0;
    // Note: if you add some summary values here, do not forget to update the method IsEmpty () accordingly
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (HourlyOperationSummary).FullName);

    #region Getters / Setters
    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] { "Id", "Machine", "Operation", "Component", "WorkOrder", "ManufacturingOrder" }; }
    }

    /// <summary>
    /// IntermediateWorkPieceSummary Id
    /// </summary>
    [XmlAttribute ("Id")]
    public virtual int Id
    {
      get { return this.m_id; }
    }

    /// <summary>
    /// IntermediateWorkPieceSummary Version
    /// </summary>
    [XmlIgnore]
    public virtual int Version
    {
      get { return this.m_version; }
    }

    /// <summary>
    /// Reference to the Machine
    /// 
    /// Not null
    /// </summary>
    [XmlIgnore]
    public virtual IMachine Machine
    {
      get {
        Debug.Assert (null != m_machine);
        return m_machine;
      }
      protected set { m_machine = value; }
    }

    /// <summary>
    /// Reference to the related machine for Xml Serialization
    /// 
    /// Be careful when set is used ! This is part of a secondary key
    /// </summary>
    [XmlElement ("Machine")]
    public virtual Machine XmlSerializationMachine
    {
      get { return this.Machine as Machine; }
      set { this.Machine = value; }
    }

    /// <summary>
    /// Reference to the Operation
    /// 
    /// Set to null if unknown
    /// </summary>
    [XmlIgnore]
    public virtual IOperation Operation
    {
      get {
        return m_operation;
      }
      protected set { m_operation = value; }
    }

    /// <summary>
    /// Reference to the related operation for Xml Serialization
    /// </summary>
    [XmlElement ("Operation")]
    public virtual Operation XmlSerializationOperation
    {
      get { return this.Operation as Operation; }
      set { this.Operation = value; }
    }

    /// <summary>
    /// Reference to the associated Component
    /// 
    /// Set to null if unknown
    /// </summary>
    [XmlIgnore]
    public virtual IComponent Component
    {
      get { return m_component; }
      protected set { m_component = value; }
    }

    /// <summary>
    /// Reference to the related component for Xml Serialization
    /// </summary>
    [XmlElement ("Component")]
    public virtual Component XmlSerializationComponent
    {
      get { return this.Component as Component; }
      set { this.Component = value; }
    }

    /// <summary>
    /// Reference to the Work Order if known
    /// 
    /// Set to null if it could not be identified yet
    /// </summary>
    [XmlIgnore]
    public virtual IWorkOrder WorkOrder
    {
      get { return m_workOrder; }
      protected set { m_workOrder = value; }
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
    /// Reference to the Line if known
    /// 
    /// Set to null if it could not be identified yet or it if is not applicable
    /// </summary>
    [XmlIgnore]
    public virtual ILine Line
    {
      get { return m_line; }
      protected set { m_line = value; }
    }

    /// <summary>
    /// Reference to the related line for Xml Serialization
    /// </summary>
    [XmlElement ("Line")]
    public virtual Line XmlSerializationLine
    {
      get { return this.Line as Line; }
      set { this.Line = value; }
    }

    /// <summary>
    /// Reference to the ManufacturingOrder if known
    /// </summary>
    [XmlIgnore]
    public virtual IManufacturingOrder ManufacturingOrder
    {
      get { return m_manufacturingOrder; }
      protected set { m_manufacturingOrder = value; }
    }

    /// <summary>
    /// If the option to split the operation slots by day is set,
    /// reference to the day.
    /// 
    /// null if the option to split the operation slot by day is not set
    /// </summary>
    [XmlIgnore]
    public virtual DateTime? Day
    {
      get { return m_day; }
      internal protected set { m_day = value; }
    }

    /// <summary>
    /// If the corresponding option is selected,
    /// reference to the shift.
    /// 
    /// null if there is no shift
    /// or if the option to split the operation slot by shift is not set
    /// </summary>
    [XmlIgnore]
    public virtual IShift Shift
    {
      get { return m_shift; }
      internal protected set { m_shift = value; }
    }

    /// <summary>
    /// Reference to the related shift for Xml Serialization
    /// 
    /// Be careful when set is used ! This is part of a secondary key
    /// </summary>
    [XmlElement ("Shift")]
    public virtual Shift XmlSerializationShift
    {
      get { return this.Shift as Shift; }
      set { this.Shift = value; }
    }

    /// <summary>
    /// Reference to the local date+hour
    /// </summary>
    [XmlIgnore]
    public virtual DateTime LocalDateHour
    {
      get { return m_localDateHour; }
      internal protected set { m_localDateHour = value; }
    }

    /// <summary>
    /// Reference to the local date+hour for XML serialization
    /// </summary>
    [XmlElement ("LocalDateHour")]
    public virtual DateTime XmlSerializationLocalDateHour
    {
      get { return this.LocalDateHour; }
      set { this.LocalDateHour = value; }
    }

    /// <summary>
    /// Duration
    /// </summary>
    [XmlIgnore]
    public virtual TimeSpan Duration
    {
      get { return m_duration; }
      set { m_duration = value; }
    }

    /// <summary>
    /// Total number of cycles as detected by from the cycle detection
    /// </summary>
    [XmlAttribute ("TotalCycles")]
    public virtual int TotalCycles
    {
      get { return m_totalCycles; }
      set {
        Debug.Assert (0 <= value);
        if (value < 0) {
          log.Error ($"TotalCycles.set: new value {value} is negative !");
        }
        m_totalCycles = value;
      }
    }

    /// <summary>
    /// Total number of adjusted cycles
    /// </summary>
    [XmlAttribute ("AdjustedCycles")]
    public virtual int AdjustedCycles
    {
      get { return m_adjustedCycles; }
      set {
        Debug.Assert (0 <= value);
        if (value < 0) {
          log.Error ($"AdjustedCycles: new value {value} is negative !");
        }
        m_adjustedCycles = value;
      }
    }

    /// <summary>
    /// Adjusted quantity
    /// </summary>
    [XmlAttribute ("AdjustedQuantity")]
    public virtual int AdjustedQuantity
    {
      get { return m_adjustedQuantity; }
      set {
        Debug.Assert (0 <= value);
        if (value < 0) {
          log.Error ($"AdjustedQuantity.set: new value {value} is negative !");
        }
        m_adjustedQuantity = value;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// The default constructor is forbidden
    /// </summary>
    protected HourlyOperationSummary ()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="operation"></param>
    /// <param name="component"></param>
    /// <param name="workOrder"></param>
    /// <param name="line"></param>
    /// <param name="manufacturingOrder"></param>
    /// <param name="day"></param>
    /// <param name="shift"></param>
    public HourlyOperationSummary (IMachine machine,
                                                        IOperation operation,
                                                        IComponent component,
                                                        IWorkOrder workOrder,
                                                        ILine line,
                                                        IManufacturingOrder manufacturingOrder,
                                                        DateTime? day,
                                                        IShift shift,
                                                        DateTime localDateHour)
    {
      m_machine = machine;
      m_operation = operation;
      m_component = component;
      m_workOrder = workOrder;
      m_line = line;
      m_manufacturingOrder = manufacturingOrder;
      m_day = day;
      m_shift = shift;
      m_localDateHour = localDateHour;
    }
    #endregion // Getters / Setters

    /// <summary>
    /// Is the data empty ? It means may it be deleted because all the data are null ?
    /// </summary>
    /// <returns></returns>
    public virtual bool IsEmpty ()
    {
      return false;
    }

    public virtual bool ReferenceDataEquals (HourlyOperationSummary other)
    {
      if (other is null) {
        return false;
      }

      Debug.Assert (null != this.Machine);
      Debug.Assert (null != other.Machine);

      if ((this.Machine.Id != other.Machine.Id)
        || !EqualsNullable (this.Operation, other.Operation, (a, b) => ((IDataWithId)a).Id == ((IDataWithId)b).Id)
        || !EqualsNullable (this.Component, other.Component, (a, b) => ((IDataWithId)a).Id == ((IDataWithId)b).Id)
        || !EqualsNullable (this.WorkOrder, other.WorkOrder, (a, b) => ((IDataWithId)a).Id == ((IDataWithId)b).Id)
        || !EqualsNullable (this.Line, other.Line, (a, b) => a.Id == b.Id)
        || !EqualsNullable (this.ManufacturingOrder, other.ManufacturingOrder, (a, b) => ((IDataWithId)a).Id == ((IDataWithId)b).Id)
        || !EqualsNullable (this.Day, other.Day, (a, b) => a.Value == b.Value)
        || !EqualsNullable (this.Shift, other.Shift, (a, b) => a.Id == b.Id)
        || (!DateTime.Equals (this.LocalDateHour, other.LocalDateHour))) {
        return false;
      }

      return true;
    }

    bool EqualsNullable<T, U> (T a, U b, Func<T, U, bool> equals)
    {
      return Lemoine.Model.Comparison.EqualsNullable<T, U> (a, b, equals);
    }

    public virtual int GetReferenceDataHashCode ()
    {
      return (this.Machine, this.Operation, this.Component, this.WorkOrder, this.Line, this.ManufacturingOrder, this.Day, this.Shift, this.LocalDateHour)
        .GetHashCode ();
    }
  }

  public class HourlyOperationSummaryReferenceDataComparer
    : IEqualityComparer<HourlyOperationSummary>
  {
    public bool Equals (HourlyOperationSummary x, HourlyOperationSummary y)
    {
      if ((x is null) && (y is null)) {
        return true;
      }
      if (x is null) {
        return false;
      }
      return x.ReferenceDataEquals (y);
    }

    public int GetHashCode (HourlyOperationSummary obj)
    {
      return obj.GetReferenceDataHashCode ();
    }
  }
}
