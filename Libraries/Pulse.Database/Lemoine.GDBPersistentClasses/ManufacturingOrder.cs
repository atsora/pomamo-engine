// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

using System.Xml.Serialization;
using Lemoine.Database.Persistent;
using Lemoine.Model;

using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table ManufOrder
  /// </summary>
  public class ManufacturingOrder: DataWithDisplayFunction, IManufacturingOrder, IMergeable<IManufacturingOrder>, IEquatable<IManufacturingOrder>, Lemoine.Collections.IDataWithId
  {
    int m_id = 0;
    int m_version = 0;
    string m_externalCode = null;
    IOperation m_operation = null;
    IComponent m_component = null;
    IWorkOrder m_workOrder = null;
    ManufacturingOrderStatus m_manufacturingOrderStatus = ManufacturingOrderStatus.New;
    int? m_quantity = null;
    TimeSpan? m_setupDuration = null;
    TimeSpan? m_cycleDuration = null;
    DateTime? m_dueDateTime = null;
    double? m_order = null;
    IMachine m_machine = null;

    static readonly ILog log = LogManager.GetLogger(typeof (ManufacturingOrder).FullName);

    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] {"Id", "ExternalCode", "Operation", "Component", "WorkOrder", "Machine"}; }
    }
    
    /// <summary>
    /// ManufacturingOrder Id
    /// </summary>
    [XmlAttribute("Id")]
    public virtual int Id
    {
      get { return this.m_id; }
    }

    /// <summary>
    /// ManufacturingOrder Version
    /// </summary>
    [XmlIgnore]
    public virtual int Version
    {
      get { return this.m_version; }
    }
    
    /// <summary>
    /// External code
    /// 
    /// Nullable
    /// </summary>
    [XmlAttribute("ExternalCode"), MergeAuto]
    public virtual string ExternalCode {
      get { return m_externalCode; }
      set { m_externalCode = value; }
    }

    /// <summary>
    /// Associated operation
    /// 
    /// Nullable
    /// </summary>
    [XmlIgnore, MergeAuto]
    public virtual IOperation Operation {
      get { return m_operation; }
      set { m_operation = value; }
    }
    
    /// <summary>
    /// Associated operation for Xml Serialization
    /// </summary>
    [XmlElement("Operation")]
    public virtual Operation XmlSerializationOperation {
      get { return this.Operation as Operation; }
      set { this.Operation = value; }
    }

    /// <summary>
    /// Associated component (optional)
    /// 
    /// Nullable
    /// </summary>
    [XmlIgnore, MergeAuto]
    public virtual IComponent Component {
      get { return m_component; }
      set { m_component = value; }
    }

    /// <summary>
    /// Associated component for Xml Serialization
    /// </summary>
    [XmlElement("Component")]
    public virtual Component XmlSerializationComponent {
      get { return this.Component as Component; }
      set { this.Component = value; }
    }
    
    /// <summary>
    /// Associated work order (optional)
    /// 
    /// Nullable
    /// </summary>
    [XmlIgnore, MergeAuto]
    public virtual IWorkOrder WorkOrder {
      get { return m_workOrder; }
      set { m_workOrder = value; }
    }
    
    /// <summary>
    /// Associated work order for Xml Serialization
    /// </summary>
    [XmlElement("WorkOrder")]
    public virtual WorkOrder XmlSerializationWorkOrder {
      get { return this.WorkOrder as WorkOrder; }
      set { this.WorkOrder = value; }
    }
    
    /// <summary>
    /// Manufacturing order status
    /// </summary>
    [XmlAttribute("ManufacturingOrderStatus"), MergeAuto]
    public virtual ManufacturingOrderStatus ManufacturingOrderStatus {
      get { return m_manufacturingOrderStatus; }
      set { m_manufacturingOrderStatus = value; }
    }
    
    /// <summary>
    /// Quantity (optional)
    /// </summary>
    [XmlIgnore, MergeAuto]
    public virtual int? Quantity {
      get { return m_quantity; }
      set { m_quantity = value; }
    }
    
    /// <summary>
    /// Estimated quantity as string
    /// </summary>
    [XmlAttribute("Quantity")]
    public virtual string QuantityAsString {
      get
      {
        return (this.Quantity.HasValue)
          ? this.Quantity.Value.ToString ()
          : null;
      }
      set
      {
        if (string.IsNullOrEmpty (value)) {
          this.Quantity = null;
        }
        else {
          this.Quantity = int.Parse (value);
        }
      }
    }

    /// <summary>
    /// Set-up duration
    /// 
    /// Optional: if not set, the set-up duration is taken from the operation properties
    /// </summary>
    [XmlIgnore, MergeAuto]
    public virtual TimeSpan? SetupDuration {
      get { return m_setupDuration; }
      set { m_setupDuration = value; }
    }
    
    /// <summary>
    /// Set-up duration as string
    /// </summary>
    [XmlAttribute("SetupDuration")]
    public virtual string SetupDurationAsString {
      get {
        return (this.SetupDuration.HasValue)
          ? this.SetupDuration.Value.ToString ()
          : null;
      }
      set {
        this.SetupDuration = ConvertToNullableTimeSpan (value);
      }
    }
    
    /// <summary>
    /// Estimated set up hours
    /// </summary>
    [XmlAttribute("SetupHours")]
    public virtual double SetupHours {
      get
      {
        return (this.SetupDuration.HasValue)
          ? this.SetupDuration.Value.TotalHours
          : 0;
      }
      set
      {
        if (0 < value) {
          this.SetupDuration = TimeSpan.FromHours (value);
        }
        else {
          this.SetupDuration = null;
        }
      }
    }

    /// <summary>
    /// Cycle duration
    /// 
    /// Optional: if not set, the cycle duration is determined from the operation properties
    /// </summary>
    [XmlIgnore, MergeAuto]
    public virtual TimeSpan? CycleDuration {
      get { return m_cycleDuration; }
      set { m_cycleDuration = value; }
    }
    
    /// <summary>
    /// Cycle duration as string
    /// </summary>
    [XmlAttribute("CycleDuration")]
    public virtual string CycleDurationAsString {
      get {
        return (this.CycleDuration.HasValue)
          ? this.CycleDuration.Value.ToString ()
          : null;
      }
      set {
        this.CycleDuration = ConvertToNullableTimeSpan (value);
      }
    }
    
    /// <summary>
    /// Cycle hours
    /// </summary>
    [XmlAttribute("CycleHours")]
    public virtual double CycleHours {
      get
      {
        return (this.CycleDuration.HasValue)
          ? this.CycleDuration.Value.TotalHours
          : 0;
      }
      set
      {
        if (0 < value) {
          this.CycleDuration = TimeSpan.FromHours (value);
        }
        else {
          this.CycleDuration = null;
        }
      }
    }

    /// <summary>
    /// Due date/time
    /// </summary>
    [XmlIgnore, MergeAuto]
    public virtual DateTime? DueDateTime {
      get { return m_dueDateTime; }
      set { m_dueDateTime = value; }
    }
    
    /// <summary>
    /// Due date/time in SQL string for XML serialization
    /// </summary>
    [XmlAttribute("Due")]
    public virtual string SqlDueDateTime {
      get
      {
        if (this.DueDateTime.HasValue) {
          return this.DueDateTime.Value.ToString ("yyyy-MM-dd HH:mm:ss");
        }
        else {
          return "";
        }
      }
      set
      {
        if ( String.IsNullOrEmpty(value)) {
          this.DueDateTime = null;
        }
        else {
          this.DueDateTime = System.DateTime.Parse (value);
        }
      }
    }

    /// <summary>
    /// Order in which the manufacturing orders are scheduled
    /// </summary>
    [XmlIgnore, MergeAuto]
    public virtual double? Order {
      get { return m_order; }
      set { m_order = value; }
    }
    
    /// <summary>
    /// Order as string
    /// </summary>
    [XmlAttribute("Order")]
    public virtual string OrderAsString {
      get
      {
        return (this.Order.HasValue)
          ? this.Order.Value.ToString ()
          : null;
      }
      set
      {
        if (string.IsNullOrEmpty (value)) {
          this.Order = null;
        }
        else {
          System.Globalization.CultureInfo cultureInfo = System.Globalization.CultureInfo.InvariantCulture;
          this.Order = double.Parse (value, cultureInfo);
        }
      }
    }

    /// <summary>
    /// Associated machine
    /// 
    /// May be null at start when a manufacturing order is not fully scheduled yet
    /// </summary>
    [XmlIgnore, MergeAuto]
    public virtual IMachine Machine {
      get { return m_machine; }
      set { m_machine = value; }
    }    

    /// <summary>
    /// Associated machine for Xml Serialization
    /// </summary>
    [XmlElement("Machine")]
    public virtual Machine XmlSerializationMachine {
      get { return this.Machine as Machine; }
      set { this.Machine = value; }
    }
    
    #region Contructors
    /// <summary>
    /// Constructor
    /// </summary>
    internal protected ManufacturingOrder ()
    { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="manufacturingOrderId"></param>
    internal protected ManufacturingOrder (int manufacturingOrderId)
    {
      m_id = manufacturingOrderId;
    }

    /// <summary>
    /// Alternative constructor with an operation
    /// </summary>
    /// <param name="operation"></param>
    internal protected ManufacturingOrder (IOperation operation)
    {
      m_operation = operation;
    }
    #endregion // Constructors

    #region IMergeable implementation
    /// <summary>
    /// <see cref="IMergeable&lt;T&gt;.Merge" />
    /// </summary>
    /// <param name="other"></param>
    /// <param name="conflictResolution"></param>
    public virtual void Merge (IManufacturingOrder other,
                               Lemoine.ModelDAO.ConflictResolution conflictResolution)
    {
      Mergeable.MergeAuto (this, other, conflictResolution);
    }
    #endregion // IMergeable implementation
    
    #region Lemoine.Model.ISerializableModel implementation
    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public virtual void Unproxy ()
    {
      NHibernateHelper.Unproxy<IOperation> (ref m_operation);
      NHibernateHelper.Unproxy<IComponent> (ref m_component);
      NHibernateHelper.Unproxy<IWorkOrder> (ref m_workOrder);
      NHibernateHelper.Unproxy<IMachine> (ref m_machine);
    }
    #endregion // Lemoine.Model.ISerializableModel implementation
    
    /// <summary>
    /// Convert a string to a nullable TimeSpan
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    TimeSpan? ConvertToNullableTimeSpan (string v)
    {
      if (string.IsNullOrEmpty (v)) {
        return default (TimeSpan?);
      }
      else {
        TimeSpan result;
        if (TimeSpan.TryParse (v, out result)) {
          return result;
        }
        else { // Not a d.hh:mm:ss string, consider this is may be a number of seconds
          return TimeSpan.FromSeconds (double.Parse (v, System.Globalization.CultureInfo.InvariantCulture));
        }
      }
    }
    
    /// <summary>
    /// <see cref="Object.ToString" />
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return $"[ManufacturingOrder Id={this.Id}]";
    }
    
    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals(IManufacturingOrder other)
    {
      return this.Equals ((object) other);
    }
    
    /// <summary>
    ///   Determines whether the specified Object
    ///   is equal to the current Object
    /// </summary>
    /// <param name="obj">The object to compare with the current object</param>
    /// <returns>true if the specified Object is equal to the current Object; otherwise, false</returns>
    public override bool Equals(object obj)
    {
      if (object.ReferenceEquals(this,obj)) {
        return true;
      }

      if (obj == null) {
        return false;
      }
      // Note: do not use here this.GetType () != obj.GetType
      //       because a Xxx may be compared with a XxxProxy
      //       which may return false although true might be returned
      ManufacturingOrder other = obj as ManufacturingOrder;
      if (null == other) {
        return false;
      }
      if (this.Id != 0) {
        return (other.Id == this.Id);
      }
      return false;
    }

    /// <summary>
    ///   Serves as a hash function for a particular type
    /// </summary>
    /// <returns>A hash code for the current Object</returns>
    public override int GetHashCode()
    {
      if (0 != Id) {
        int hashCode = 0;
        unchecked {
          hashCode += 1000000007 * Id.GetHashCode();
        }
        return hashCode;
      }
      else {
        return base.GetHashCode ();
      }
    }
  }
}
