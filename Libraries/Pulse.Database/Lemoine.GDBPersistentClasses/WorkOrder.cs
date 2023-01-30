// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Collections;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table WorkOrder
  /// </summary>
  [Serializable]
  public class WorkOrder : DataWithDisplayFunction, IWorkOrder, IMergeable<IWorkOrder>, IEquatable<IWorkOrder>, Lemoine.Collections.IDataWithId
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    string m_name;
    string m_code;
    string m_externalCode;
    ICustomer m_customer;
    string m_documentLink;
    DateTime? m_deliveryDate;
    IWorkOrderStatus m_status;
    ICollection<IProject> m_projects = new InitialNullIdSet<IProject, int> ();
    #endregion

    static readonly ILog log = LogManager.GetLogger (typeof (WorkOrder).FullName);

    #region Getters / Setters
    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] { "Id", "ExternalCode", "Code", "Name" }; }
    }

    /// <summary>
    /// Work order ID
    /// </summary>
    [XmlAttribute ("Id")]
    public virtual int Id
    {
      get { return this.m_id; }
    }

    /// <summary>
    /// Version
    /// </summary>
    [XmlIgnore]
    public virtual int Version
    {
      get { return this.m_version; }
    }

    /// <summary>
    /// Associated Job
    /// </summary>
    [XmlIgnore]
    public virtual IJob Job
    {
      get { return new Job (this); }
    }

    /// <summary>
    /// Work order name
    /// </summary>
    [XmlAttribute ("Name")]
    public virtual string Name
    {
      get { return this.m_name; }
      set { this.m_name = value; }
    }

    /// <summary>
    /// Work order code
    /// </summary>
    [XmlAttribute ("Code")]
    public virtual string Code
    {
      get { return m_code; }
      set { m_code = value; }
    }

    /// <summary>
    /// Work order external code
    /// </summary>
    [XmlAttribute ("ExternalCode")]
    public virtual string ExternalCode
    {
      get { return m_externalCode; }
      set { m_externalCode = value; }
    }

    /// <summary>
    /// <see cref="IWorkOrder"/>
    /// </summary>
    [XmlIgnore]
    public virtual ICustomer Customer
    {
      get { return m_customer; }
      set { m_customer = value; }
    }

    /// <summary>
    /// Customer for Xml Serialization
    /// </summary>
    [XmlElement ("Customer")]
    public virtual Customer XmlSerializationCustomer
    {
      get { return this.Customer as Customer; }
      set { this.Customer = value; }
    }

    /// <summary>
    /// Work order document link
    /// </summary>
    [XmlAttribute ("DocumentLink")]
    public virtual string DocumentLink
    {
      get { return m_documentLink; }
      set { m_documentLink = value; }
    }

    /// <summary>
    /// Work order delivery date
    /// </summary>
    [XmlIgnore]
    public virtual DateTime? DeliveryDate
    {
      get { return m_deliveryDate; }
      set { m_deliveryDate = value; }
    }

    /// <summary>
    /// Work order delivery date in SQL string for XML serialization
    /// </summary>
    [XmlAttribute ("DeliveryDate")]
    public virtual string SqlDeliveryDate
    {
      get {
        if (this.DeliveryDate.HasValue) {
          return this.DeliveryDate.Value.ToString ("yyyy-MM-dd");
        }
        else {
          return "";
        }
      }
      set {
        if (String.IsNullOrEmpty (value)) {
          this.DeliveryDate = null;
        }
        else {
          this.DeliveryDate = System.DateTime.Parse (value);
        }
      }
    }

    /// <summary>
    /// Work order status
    /// </summary>
    [XmlIgnore]
    public virtual IWorkOrderStatus Status
    {
      get { return m_status; }
      set { m_status = value; }
    }

    /// <summary>
    /// Work order status for Xml Serialization
    /// </summary>
    [XmlElement ("Status")]
    public virtual WorkOrderStatus XmlSerializationStatus
    {
      get { return this.Status as WorkOrderStatus; }
      set { this.Status = value; }
    }

    /// <summary>
    /// Projects that are associated to the work order
    /// </summary>
    [XmlIgnore]
    public virtual ICollection<IProject> Projects
    {
      get { return m_projects; }
    }

    /// <summary>
    /// Parts that are associated to the work order
    /// </summary>
    [XmlIgnore]
    public virtual ICollection<IPart> Parts
    {
      get {
        IList<IPart> parts = new List<IPart> ();
        foreach (IProject project in m_projects) {
          IPart part;
          try {
            part = project.Part;
          }
          catch (Exception) {
            log.WarnFormat ("Parts: " +
                            "Skip project {0} because it is not a part",
                            project);
            continue;
          }
          parts.Add (part);
        }
        return parts;
      }
    }
    #endregion

    #region Add methods
    /// <summary>
    /// Add a project locally
    /// </summary>
    /// <param name="project"></param>
    protected internal virtual void AddProjectForInternalUse (IProject project)
    {
      AddToProxyCollection<IProject> (m_projects, project);
    }

    /// <summary>
    /// Remove a project locally
    /// </summary>
    /// <param name="project"></param>
    protected internal virtual void RemoveProjectForInternalUse (IProject project)
    {
      RemoveFromProxyCollection<IProject> (m_projects, project);
    }
    #endregion // Add methods

    #region Methods
    /// <summary>
    /// Check if the work order is undefined
    /// 
    /// A work order is considered as undefined if it has no name and no code
    /// </summary>
    /// <returns></returns>
    public virtual bool IsUndefined ()
    {
      return (String.IsNullOrEmpty (this.Name) || String.IsNullOrEmpty (this.Code));
    }

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[WorkOrder {this.Id} Code={this.Code} Name={this.Name}]";
      }
      else {
        return $"[WorkOrder {this.Id}]";
      }
    }
    #endregion // Methods

    #region IMergeable implementation
    /// <summary>
    /// <see cref="IMergeable&lt;T&gt;.Merge" />
    /// </summary>
    /// <param name="other"></param>
    /// <param name="conflictResolution"></param>
    public virtual void Merge (IWorkOrder other,
                              ConflictResolution conflictResolution)
    {
      Mergeable.MergeAuto (this, other, conflictResolution);
      ModifyItems<IProject>
        (other.Projects,
         new Modifier<IProject>
         (delegate (IProject project) {
           ModelDAOHelper.DAOFactory.ProjectDAO.Lock (project);

           {
             project.AddWorkOrder (this);
             IWorkOrderProjectUpdate update = ModelDAOHelper.ModelFactory
               .CreateWorkOrderProjectUpdate (this, project, WorkOrderProjectUpdateModificationType.NEW);
             ModelDAOHelper.DAOFactory.WorkOrderProjectUpdateDAO.MakePersistent (update);
           }

           {
             project.RemoveWorkOrder (other);
             IWorkOrderProjectUpdate update = ModelDAOHelper.ModelFactory
               .CreateWorkOrderProjectUpdate (other, project, WorkOrderProjectUpdateModificationType.DELETE);
             ModelDAOHelper.DAOFactory.WorkOrderProjectUpdateDAO.MakePersistent (update);
           }
         }));
      // TODO: Restrictions following DataStructureOption
    }
    #endregion // IMergeable implementation

    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public virtual void Unproxy ()
    {
      NHibernateHelper.Unproxy<IWorkOrderStatus> (ref m_status);
      NHibernateHelper.Unproxy<ICustomer> (ref m_customer);
    }

    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals (IWorkOrder other)
    {
      return this.Equals ((object)other);
    }

    /// <summary>
    ///   Determines whether the specified Object
    ///   is equal to the current Object
    /// </summary>
    /// <param name="obj">The object to compare with the current object</param>
    /// <returns>true if the specified Object is equal to the current Object; otherwise, false</returns>
    public override bool Equals (object obj)
    {
      if (object.ReferenceEquals (this, obj)) {
        return true;
      }

      if (obj == null) {
        return false;
      }
      // Note: do not use here this.GetType () != obj.GetType
      //       because a Xxx may be compared with a XxxProxy
      //       which may return false although true might be returned
      WorkOrder other = obj as WorkOrder;
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
    public override int GetHashCode ()
    {
      if (0 != Id) {
        int hashCode = 0;
        unchecked {
          hashCode += 1000000007 * Id.GetHashCode ();
        }
        return hashCode;
      }
      else {
        return base.GetHashCode ();
      }
    }

  }
}
