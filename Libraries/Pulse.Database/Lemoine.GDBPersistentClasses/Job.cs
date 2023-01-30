// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Class that corresponds to a Job
  /// that is a join between WorkOrder and Project
  /// in case the data structure option WorkOrderProjectIsJob is set
  /// </summary>
  [Serializable]
  public class Job: DataWithPatternName, IJob, IEquatable<IJob>
  {
    #region Members
    IProject m_project;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (Job).FullName);

    #region Constructors
    /// <summary>
    /// Constructor for a new Job
    /// </summary>
    public Job ()
    {
      m_project = new Project ();
      IWorkOrder workOrder = new WorkOrder ();
      m_project.AddWorkOrder (workOrder);
    }
    
    /// <summary>
    /// Constructor from a project
    /// </summary>
    /// <param name="project"></param>
    public Job (IProject project)
    {
      m_project = project;
      Debug.Assert (null != m_project);
    }
    
    /// <summary>
    /// Constructor from a work order
    /// </summary>
    /// <param name="workOrder"></param>
    public Job (IWorkOrder workOrder)
    {
      m_project = GetFirst<IProject> (workOrder.Projects);
      Debug.Assert (null != m_project);
    }
    #endregion // Constructors
    
    #region Getters / Setters
    /// <summary>
    /// Id
    /// </summary>
    [XmlIgnore]
    public virtual int Id
    {
      get { return ProjectId; }
    }
    
    /// <summary>
    /// Version
    /// </summary>
    [XmlIgnore]
    public virtual int Version
    {
      get { return m_project.Version; }
    }

    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] {"ProjectId", "ExternalCode", "Code", "Name"}; }
    }
    
    /// <summary>
    /// Associated project
    /// </summary>
    [XmlIgnore]
    public virtual IProject Project
    {
      get { return m_project; }
    }
    
    /// <summary>
    /// Job ID that corresponds also to the Project ID
    /// </summary>
    [XmlAttribute("ProjectId")]
    public virtual int ProjectId
    {
      get { return ((Lemoine.Collections.IDataWithId)m_project).Id; }
    }
    
    /// <summary>
    /// Associate work order
    /// </summary>
    [XmlIgnore]
    public IWorkOrder WorkOrder
    {
      get { return GetFirst<IWorkOrder> (m_project.WorkOrders); }
    }
    
    /// <summary>
    /// Work Order ID
    /// </summary>
    [XmlAttribute("WorkOrderId")]
    public virtual int WorkOrderId
    {
      get { return ((Lemoine.Collections.IDataWithId)this.WorkOrder).Id; }
    }
    
    /// <summary>
    /// Job name that corresponds also to
    /// the project name and the work order name
    /// </summary>
    [XmlAttribute("Name")]
    public virtual string Name
    {
      get { return m_project.Name; }
      set {
        m_project.Name = value;
        this.WorkOrder.Name = value;
      }
    }
    
    /// <summary>
    /// Job code that corresponds also to
    /// the project code and the work order code
    /// </summary>
    [XmlAttribute("Code")]
    public virtual string Code
    {
      get { return m_project.Code; }
      set {
        m_project.Code = value;
        this.WorkOrder.Code = value;
      }
    }
    
    /// <summary>
    /// Job external code that corresponds to
    /// the project external code and the work order external code
    /// </summary>
    [XmlAttribute("ExternalCode")]
    public virtual string ExternalCode
    {
      get { return m_project.ExternalCode; }
      set {
        m_project.ExternalCode = value;
        this.WorkOrder.ExternalCode = value;
      }
    }
    
    /// <summary>
    /// <see cref="IJob"/>
    /// </summary>
    [XmlElement("Customer")]
    public virtual ICustomer Customer
    {
      get { return m_project.Customer; }
      set { m_project.Customer = value; }
    }

    /// <summary>
    /// Job document link that corresponds to
    /// the project document link and the work order document link
    /// </summary>
    [XmlAttribute("DocumentLink")]
    public virtual string DocumentLink
    {
      get { return m_project.DocumentLink; }
      set {
        m_project.DocumentLink = value;
        this.WorkOrder.DocumentLink = value;
      }
    }
    
    /// <summary>
    /// Work order delivery date
    /// </summary>
    [XmlIgnore]
    public virtual DateTime? DeliveryDate
    {
      get { return this.WorkOrder.DeliveryDate; }
      set { this.WorkOrder.DeliveryDate = value; }
    }
    
    /// <summary>
    /// Work order status
    /// </summary>
    [XmlIgnore]
    public virtual IWorkOrderStatus Status
    {
      get { return this.WorkOrder.Status; }
      set { this.WorkOrder.Status = value; }
    }

    /// <summary>
    /// Work order status for Xml Serialization
    /// </summary>
    [XmlElement("Status")]
    public virtual WorkOrderStatus XmlSerializationStatus
    {
      get { return this.Status as WorkOrderStatus; }
      set { this.Status = value; }
    }
    
    /// <summary>
    /// Project creation date/time
    /// </summary>
    [XmlIgnore]
    public virtual DateTime CreationDateTime
    {
      get { return m_project.CreationDateTime; }
    }
    
    /// <summary>
    /// Creation date/time when the project is created.
    /// Else date/time of the re-activation of the project
    /// if the project has been archived
    /// </summary>
    [XmlIgnore]
    public virtual DateTime ReactivationDateTime
    {
      get { return m_project.ReactivationDateTime; }
      set { m_project.ReactivationDateTime = value; }
    }
    
    /// <summary>
    /// Unset if the propject is not archived.
    /// Else date/time when the project is archived.
    /// </summary>
    [XmlIgnore]
    public virtual DateTime? ArchiveDateTime
    {
      get { return m_project.ArchiveDateTime; }
      set { m_project.ArchiveDateTime = value; }
    }
    
    /// <summary>
    /// Components that are associated to the project
    /// </summary>
    [XmlIgnore]
    public virtual ICollection<IComponent> Components
    {
      get { return m_project.Components; }
    }
    #endregion // Getters / Setters
    
    #region Add methods
    /// <summary>
    /// Add a component
    /// </summary>
    /// <param name="component"></param>
    public virtual void AddComponent (IComponent component)
    {
      component.Project = m_project; // Everything is done in the setter
    }
    #endregion // Add methods
    
    #region Methods
    /// <summary>
    /// Check if the job is undefined
    /// 
    /// A job is considered as undefined if it has no name and no code
    /// </summary>
    /// <returns></returns>
    public virtual bool IsUndefined ()
    {
      return ( (this.Name == null)
              || (0 == this.Name.Length))
        && ( (this.Code == null)
            || (0 == this.Code.Length));
    }

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return $"[Job {this.Id}]";
    }
    #endregion // Methods

    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals(IJob other)
    {
      if (object.ReferenceEquals(this,other)) {
        return true;
      }

      if (other == null) {
        return false;
      }

      if (0 != this.ProjectId) {
        return (other.ProjectId == this.ProjectId);
      }
      else {
        return false;
      }
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
      IJob other = obj as Job;
      if (null == other) {
        return false;
      }

      if (0 != this.ProjectId) {
        return (other.ProjectId == this.ProjectId);
      }
      else {
        return false;
      }
    }

    /// <summary>
    ///   Serves as a hash function for a particular type
    /// </summary>
    /// <returns>A hash code for the current Object</returns>
    public override int GetHashCode()
    {
      return m_project.GetHashCode();
    }
    
    
  }
}
