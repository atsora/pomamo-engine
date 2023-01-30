// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.Collections;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of the Project table
  /// 
  /// A project represents a coherent set of components
  /// that must be delivered in the same time.
  /// </summary>
  [Serializable]
  public class Project: DataWithDisplayFunction, IProject, IMergeable<IProject>, IEquatable<IProject>, Lemoine.Collections.IDataWithId
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    string m_name;
    string m_code;
    string m_externalCode;
    ICustomer m_customer;
    string m_documentLink;
    DateTime m_creationDateTime = DateTime.UtcNow;
    DateTime m_reactivationDateTime = DateTime.UtcNow;
    DateTime? m_archiveDateTime;
    ICollection <IWorkOrder> m_workOrders = new InitialNullIdSet<IWorkOrder, int>();
    ICollection <IComponent> m_components = new InitialNullIdSet<IComponent, int> ();
    #endregion

    static readonly ILog log = LogManager.GetLogger(typeof (Project).FullName);

    #region Getters / Setters
    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] {"Id", "ExternalCode", "Code", "Name"}; }
    }
    
    /// <summary>
    /// Project id
    /// </summary>
    [XmlAttribute("Id")]
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
    /// Associated Part
    /// </summary>
    [XmlIgnore]
    public virtual IPart Part
    {
      get { return new Part (this); }
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
    /// Project name
    /// </summary>
    [XmlAttribute("Name"), MergeAuto]
    public virtual string Name
    {
      get { return this.m_name; }
      set { this.m_name = value; }
    }
    
    /// <summary>
    /// Project code
    /// </summary>
    [XmlAttribute("Code"), MergeAuto]
    public virtual string Code {
      get { return m_code; }
      set { m_code = value; }
    }
    
    /// <summary>
    /// Project external code
    /// </summary>
    [XmlAttribute("ExternalCode"), MergeAuto]
    public virtual string ExternalCode {
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
    /// Project document link
    /// </summary>
    [XmlAttribute ("DocumentLink"), MergeAuto]
    public virtual string DocumentLink {
      get { return m_documentLink; }
      set { m_documentLink = value; }
    }
    
    /// <summary>
    /// Project creation date/time
    /// </summary>
    [XmlIgnore]
    public virtual DateTime CreationDateTime {
      get { return m_creationDateTime; }
    }
    
    /// <summary>
    /// Creation date/time when the project is created.
    /// Else date/time of the re-activation of the project
    /// if the project has been archived
    /// </summary>
    [XmlIgnore]
    public virtual DateTime ReactivationDateTime {
      get { return m_reactivationDateTime; }
      set { m_reactivationDateTime = value; }
    }
    
    /// <summary>
    /// Unset if the propject is not archived.
    /// Else date/time when the project is archived.
    /// </summary>
    [XmlIgnore]
    public virtual DateTime? ArchiveDateTime {
      get { return m_archiveDateTime; }
      set { m_archiveDateTime = value; }
    }
    
    /// <summary>
    /// Work orders that are associated to the project
    /// </summary>
    [XmlIgnore]
    public virtual ICollection<IWorkOrder> WorkOrders {
      get
      {
        return m_workOrders;
      }
    }
    
    /// <summary>
    /// Components that are associated to the project
    /// </summary>
    [XmlIgnore, MergeChildren("Project")]
    public virtual ICollection <IComponent> Components {
      get
      {
        return m_components;
      }
    }
    #endregion
    
    #region Add methods
    /// <summary>
    /// Add a work order
    /// </summary>
    /// <param name="workOrder"></param>
    public virtual void AddWorkOrder (IWorkOrder workOrder)
    {
      if (!this.WorkOrders.Contains (workOrder)) {
        WorkOrders.Add (workOrder);
        (workOrder as WorkOrder).AddProjectForInternalUse (this);
      }
      else {
        log.Info ($"AddWorkOrder: WorkOrder {workOrder} is already in {this}");
      }
    }
    
    /// <summary>
    /// Remove a work order
    /// </summary>
    /// <param name="workOrder"></param>
    public virtual void RemoveWorkOrder (IWorkOrder workOrder)
    {
      if (!this.WorkOrders.Contains (workOrder)) {
        log.WarnFormat ("RemoveWorkOrder: " +
                        "WorkOrder {0} is not in {1}",
                        workOrder, this);
      }
      else {
        WorkOrders.Remove (workOrder);
        (workOrder as WorkOrder)
          .RemoveProjectForInternalUse (this);
      }
    }
    
    /// <summary>
    /// Add a component
    /// </summary>
    /// <param name="component"></param>
    public virtual void AddComponent (IComponent component)
    {
      component.Project = this; // Everything is done in the setter
    }
    
    
    /// <summary>
    /// Remove a component
    /// </summary>
    /// <param name="component"></param>
    public virtual void RemoveComponent (IComponent component)
    {
      Components.Remove(component);
    }
    #endregion // Add methods
    
    #region Methods
    /// <summary>
    /// Check if the project is undefined
    /// 
    /// A project is considered as undefined if it has no name and no code
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
    /// <see cref="Object.ToString" />
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[Project {this.Id} {this.Code} {this.Name}]";
      }
      else {
        return $"[Project {this.Id}]";
      }
    }
    #endregion // Methods
    
    #region IMergeable implementation
    /// <summary>
    /// <see cref="IMergeable&lt;T&gt;.Merge" />
    /// </summary>
    /// <param name="other"></param>
    /// <param name="conflictResolution"></param>
    public virtual void Merge(IProject other,
                              ConflictResolution conflictResolution)
    {
      // Rename any component from other that may have the same
      // name than a component from this
      foreach (IComponent component in this.Components) {
        foreach (IComponent otherComponent in other.Components) {
          // - Name
          if ( (null != otherComponent.Name)
              && (otherComponent.Name.Equals (component.Name))) {
            log.InfoFormat ("Merge: " +
                            "Rename component {0} from project other {1} " +
                            "because the used name already exists in project {2}",
                            otherComponent, other, this);
            for (int i = 1; true; i++) {
              string newName = string.Format ("{0} ({1})",
                                              otherComponent.Name, i);
              bool ok = true;
              foreach (IComponent component2 in this.Components) {
                if ( (null != component2.Name)
                    && (component2.Name.Equals (newName))) {
                  ok = false;
                  break;
                }
              }
              foreach (IComponent component2 in other.Components) {
                if ( (null != component2.Name)
                    && (component2.Name.Equals (newName))) {
                  ok = false;
                  break;
                }
              }
              if (ok) {
                otherComponent.Name = newName;
                break;
              }
            }
          }
          // - Code
          if ( (null != otherComponent.Code)
              && (otherComponent.Code.Equals (component.Code))) {
            log.InfoFormat ("Merge: " +
                            "Recode component {0} from project other {1} " +
                            "because the used code already exists in project {2}",
                            otherComponent, other, this);
            for (int i = 1; true; i++) {
              string newCode = string.Format ("{0} ({1})",
                                              otherComponent.Code, i);
              bool ok = true;
              foreach (IComponent component2 in this.Components) {
                if ( (null != component2.Code)
                    && (component2.Code.Equals (newCode))) {
                  ok = false;
                  break;
                }
              }
              foreach (IComponent component2 in other.Components) {
                if ( (null != component2.Code)
                    && (component2.Code.Equals (newCode))) {
                  ok = false;
                  break;
                }
              }
              if (ok) {
                otherComponent.Code = newCode;
                break;
              }
            }
          }
        }
      }
      
      Mergeable.MergeAuto (this, other, conflictResolution);
      // There is no need to update the quantity here
      ModifyItems<IWorkOrder>
        (other.WorkOrders,
         new Modifier<IWorkOrder>
         (delegate (IWorkOrder otherWorkOrder)
          {
            this.AddWorkOrder (otherWorkOrder);
          }));
      ModifyItems<IWorkOrder>
        (this.WorkOrders,
         new Modifier<IWorkOrder>
         (delegate (IWorkOrder workOrder)
          {
            other.AddWorkOrder (workOrder);
          }));
      // TODO: Restrictions following DataStructureOption
    }
    #endregion // IMergeable implementation
    
    #region Methods
    /// <summary>
    /// Add a component in the member directly
    /// 
    /// To be used by the Component class only
    /// </summary>
    /// <param name="component"></param>
    protected internal virtual void AddComponentForInternalUse (IComponent component)
    {
      AddToProxyCollection<IComponent> (m_components, component);
    }
    
    /// <summary>
    /// Remove a component in the member directly
    /// 
    /// To be used by the Component class only
    /// </summary>
    /// <param name="component"></param>
    protected internal virtual void RemoveComponentForInternalUse (IComponent component)
    {
      RemoveFromProxyCollection<IComponent> (m_components, component);
    }
    #endregion // Methods
    
    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals(IProject other)
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
      Project other = obj as Project;
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
    
    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public virtual void Unproxy ()
    {
      NHibernateHelper.Unproxy<ICustomer> (ref m_customer);
    }
  }
}
