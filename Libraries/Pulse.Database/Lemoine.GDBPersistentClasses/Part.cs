// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Xml.Serialization;

using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Class that corresponds to a Part
  /// that is a join between the Project and the Component tables
  /// in case the data structure option ProjectComponentIsPart is set
  /// </summary>
  [Serializable]
  public class Part: DataWithPatternName, IPart, IEquatable<IPart>
  {
    #region Members
    IComponent m_component;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (Part).FullName);

    #region Constructors
    /// <summary>
    /// Constructor for a new Part
    /// </summary>
    /// <param name="componentType"></param>
    public Part (IComponentType componentType)
    {
      Project project = new Project ();
      m_component = new Component (project, componentType);
    }
    
    /// <summary>
    /// Constructor from a component
    /// </summary>
    /// <param name="component"></param>
    public Part (IComponent component)
    {
      m_component = component;
      Debug.Assert (null != m_component);
    }
    
    /// <summary>
    /// Constructor from a project
    /// </summary>
    /// <param name="project"></param>
    public Part (IProject project)
    {
      m_component = GetFirst<IComponent> (project.Components);
      Debug.Assert (null != m_component);
    }
    #endregion // Constructors
    
    #region Getters / Setters
    /// <summary>
    /// Id
    /// </summary>
    [XmlIgnore]
    public virtual int Id
    {
      get { return ComponentId; }
    }
    
    /// <summary>
    /// Version
    /// </summary>
    [XmlIgnore]
    public virtual int Version
    {
      get { return m_component.Version; }
    }

    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] {"ComponentId", "ExternalCode", "Code", "Name", "Type"}; }
    }
    
    /// <summary>
    /// Component
    /// </summary>
    [XmlIgnore]
    public virtual IComponent Component
    {
      get { return this.m_component; }
    }
    
    /// <summary>
    /// Part ID that corresponds also to the Component ID
    /// </summary>
    [XmlAttribute("ComponentId")]
    public virtual int ComponentId
    {
      get { return ((Lemoine.Collections.IDataWithId)this.m_component).Id; }
    }
    
    /// <summary>
    /// Associated project
    /// </summary>
    [XmlIgnore]
    public virtual IProject Project
    {
      get { return this.m_component.Project; }
    }

    /// <summary>
    /// Project ID
    /// </summary>
    [XmlAttribute("ProjectId")]
    public virtual int ProjectId {
      get { return ((Lemoine.Collections.IDataWithId)m_component.Project).Id; }
    }
    
    /// <summary>
    /// Full name of the part as used in the shop
    /// that corresponds also to the name of the component
    /// or the name of the project
    /// </summary>
    [XmlAttribute("Name")]
    public virtual string Name
    {
      get { return m_component.Name; }
      set
      {
        m_component.Name = value;
        m_component.Project.Name = value;
      }
    }
    
    /// <summary>
    /// Code given to the component
    /// that corresponds also to the code of the component
    /// or the code of the project
    /// </summary>
    [XmlAttribute("Code")]
    public virtual string Code {
      get { return m_component.Code; }
      set
      {
        m_component.Code = value;
        m_component.Project.Code = value;
      }
    }
    
    /// <summary>
    /// External code
    /// 
    /// It may help synchronizing PUSLE data with an external database
    /// 
    /// It corresponds also to the external code of the component
    /// and the external code of the project
    /// </summary>
    [XmlAttribute("ExternalCode")]
    public virtual string ExternalCode {
      get { return m_component.ExternalCode; }
      set
      {
        m_component.ExternalCode = value;
        m_component.Project.ExternalCode = value;
      }
    }
    
    /// <summary>
    /// <see cref="IPart"/>
    /// </summary>
    [XmlElement("Customer")]
    public virtual ICustomer Customer {
      get { return m_component.Project.Customer; }
      set { m_component.Project.Customer = value; }
    }

    /// <summary>
    /// Link to the documentation in the network
    /// 
    /// It corresponds also to the document link of the component
    /// and the document link of the project
    /// </summary>
    [XmlAttribute("DocumentLink")]
    public virtual string DocumentLink {
      get { return m_component.DocumentLink; }
      set
      {
        m_component.DocumentLink = value;
        m_component.Project.DocumentLink = value;
      }
    }
    
    /// <summary>
    /// Project creation date/time
    /// </summary>
    [XmlIgnore]
    public virtual DateTime CreationDateTime {
      get { return m_component.Project.CreationDateTime; }
    }
    
    /// <summary>
    /// Creation date/time when the project is created.
    /// Else date/time of the re-activation of the project
    /// if the project has been archived
    /// </summary>
    [XmlIgnore]
    public virtual DateTime ReactivationDateTime {
      get { return m_component.Project.ReactivationDateTime; }
      set { m_component.Project.ReactivationDateTime = value; }
    }
    
    /// <summary>
    /// Unset if the propject is not archived.
    /// Else date/time when the project is archived.
    /// </summary>
    [XmlIgnore]
    public virtual DateTime? ArchiveDateTime {
      get { return m_component.Project.ArchiveDateTime; }
      set { m_component.Project.ArchiveDateTime = value; }
    }
    
    /// <summary>
    /// Work orders that are associated to the project
    /// </summary>
    [XmlIgnore]
    public virtual ICollection<IWorkOrder> WorkOrders {
      get { return m_component.Project.WorkOrders; }
    }

    /// <summary>
    /// Associated component type
    /// </summary>
    [XmlIgnore]
    public virtual IComponentType Type {
      get { return m_component.Type; }
      set { m_component.Type = value; }
    }
    
    /// <summary>
    /// Associated component type
    /// </summary>
    [XmlElement("Type")]
    public virtual ComponentType XmlSerializationType {
      get { return this.Type as ComponentType; }
      set { this.Type = value; }
    }
    
    /// <summary>
    /// Reference to the final work piece (in IntermediateWorkPiece table)
    /// that corresponds to the finished component
    /// </summary>
    [XmlIgnore]
    public virtual IIntermediateWorkPiece FinalWorkPiece {
      get { return m_component.FinalWorkPiece; }
      set { m_component.FinalWorkPiece = value; }
    }
    
    /// <summary>
    /// Reference to the final work piece (in IntermediateWorkPiece table)
    /// that corresponds to the finished component
    /// for Xml Serialization
    /// </summary>
    [XmlElement("FinalWorkPiece")]
    public virtual IntermediateWorkPiece XmlSerializationFinalWorkPiece {
      get { return this.FinalWorkPiece as IntermediateWorkPiece; }
      set { this.FinalWorkPiece = value; }
    }
    
    /// <summary>
    /// Estimated hours
    /// </summary>
    [XmlIgnore]
    public virtual double? EstimatedHours {
      get { return m_component.EstimatedHours; }
      set { m_component.EstimatedHours = value; }
    }

    
    /// <summary>
    /// Estimated hours as string
    /// </summary>
    [XmlAttribute("EstimatedHours")]
    public virtual string EstimatedHoursAsString {
      get
      {
        return (this.EstimatedHours.HasValue)
          ? this.EstimatedHours.Value.ToString (CultureInfo.InvariantCulture)
          : null;
      }
      set
      {
        this.EstimatedHours =
          string.IsNullOrEmpty (value)
          ? default (double?)
          : double.Parse (value, CultureInfo.InvariantCulture);
      }
    }
    
    /// <summary>
    /// Set of intermediate work pieces that are associated to the component
    /// 
    /// The intermediate work pieces are ordered by ascending order (if available)
    /// </summary>
    [XmlIgnore]
    public virtual ICollection<IComponentIntermediateWorkPiece> ComponentIntermediateWorkPieces {
      get { return m_component.ComponentIntermediateWorkPieces; }
    }
    
    /// <summary>
    /// List of stamps (and then ISO files) that are associated to this part
    /// </summary>
    [XmlIgnore, MergeChildren("Component")]
    public virtual ICollection<IStamp> Stamps {
      get { return m_component.Stamps; }
    }
    #endregion // Getters / Setters
    
    #region Add methods
    /// <summary>
    /// Add a work order
    /// </summary>
    /// <param name="workOrder"></param>
    public virtual void AddWorkOrder (IWorkOrder workOrder)
    {
      m_component.Project.AddWorkOrder (workOrder);
    }
    
    /// <summary>
    /// Remove a work order
    /// </summary>
    /// <param name="workOrder"></param>
    public virtual void RemoveWorkOrder (IWorkOrder workOrder)
    {
      m_component.Project.RemoveWorkOrder (workOrder);
    }
    
    /// <summary>
    /// Add an intermediate work piece to the part
    /// without precising the code or order
    /// </summary>
    /// <param name="intermediateWorkPiece"></param>
    public virtual IComponentIntermediateWorkPiece AddIntermediateWorkPiece (IIntermediateWorkPiece intermediateWorkPiece)
    {
      return m_component.AddIntermediateWorkPiece (intermediateWorkPiece);
    }
    
    /// <summary>
    /// Remove an intermediate work piece from the component
    /// </summary>
    /// <param name="intermediateWorkPiece"></param>
    public virtual IList<IComponentIntermediateWorkPiece> RemoveIntermediateWorkPiece (IIntermediateWorkPiece intermediateWorkPiece)
    {
      return m_component.RemoveIntermediateWorkPiece (intermediateWorkPiece);
    }
    #endregion // Add methods
    
    #region Methods
    /// <summary>
    /// Check if the part is undefined
    /// 
    /// An part is considered as undefined if it has no name and no code
    /// </summary>
    /// <returns></returns>
    public virtual bool IsUndefined ()
    {
      return ( (this.Name == null)
              || (0 == this.Name.Length))
        && ( (this.Code == null)
            || (0 == this.Code.Length));
    }
    #endregion // Methods

    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals (IPart other)
    {
      if (object.ReferenceEquals (this, other)) {
        return true;
      }

      if (other == null) {
        return false;
      }

      if (0 != this.ComponentId) {
        return ((other.ProjectId == this.ProjectId) && (other.ComponentId == this.ComponentId));
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
      IPart other = obj as Part;
      if (null == other) {
        return false;
      }
      if (0 != this.ComponentId) {
        return ((other.ProjectId == this.ProjectId) && (other.ComponentId == this.ComponentId));
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
      return m_component.GetHashCode();
    }
    
  }
}
