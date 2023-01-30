// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Xml.Serialization;
using Lemoine.Collections;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table Component
  /// </summary>
  [Serializable]
  public class Component : DataWithDisplayFunction, IComponent, IMergeable<Component>, IEquatable<IComponent>, Lemoine.Collections.IDataWithId
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    string m_name;
    string m_code;
    string m_externalCode;
    string m_documentLink;
    IProject m_project;
    IComponentType m_type;
    IIntermediateWorkPiece m_finalWorkPiece;
    double? m_estimatedHours;
    ICollection<IComponentIntermediateWorkPiece> m_componentIntermediateWorkPieces =
      new List<IComponentIntermediateWorkPiece> ();
    ICollection<IStamp> m_stamps = new InitialNullIdSet<IStamp, int> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (Component).FullName);

    #region Getters / Setters
    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] { "Id", "ExternalCode", "Code", "Name", "Type", "Project" }; }
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
    /// Component ID
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
    /// Full name of the component as used in the shop
    /// </summary>
    [XmlAttribute ("Name"), MergeAuto]
    public virtual string Name
    {
      get { return this.m_name; }
      set { this.m_name = value; }
    }

    /// <summary>
    /// Code given to the component
    /// </summary>
    [XmlAttribute ("Code"), MergeAuto]
    public virtual string Code
    {
      get { return m_code; }
      set { m_code = value; }
    }

    /// <summary>
    /// External code
    /// 
    /// It may help synchronizing PUSLE data with an external database
    /// </summary>
    [XmlAttribute ("ExternalCode"), MergeAuto]
    public virtual string ExternalCode
    {
      get { return m_externalCode; }
      set { m_externalCode = value; }
    }

    /// <summary>
    /// Link to the documentation in the network
    /// </summary>
    [XmlAttribute ("DocumentLink"), MergeAuto]
    public virtual string DocumentLink
    {
      get { return m_documentLink; }
      set { m_documentLink = value; }
    }

    /// <summary>
    /// Reference to the project the component is belong to.
    /// 
    /// This field is mandatory (not null), an orphaned component is not possible.
    /// </summary>
    [MergeParent, XmlIgnore]
    public virtual IProject Project
    {
      get { return m_project; }
      set {
        if (object.Equals (m_project, value)) {
          // nothing to do
          return;
        }
        // Remove the component from the previous project
        if (m_project != null) {
          Project project = m_project as Project;
          project.RemoveComponentForInternalUse (this);
        }
        m_project = value;
        if (m_project != null) {
          // Add the component to the new project
          (m_project as Project).AddComponentForInternalUse (this);
        }
      }
    }

    /// <summary>
    /// Associated project for XML Serialization
    /// </summary>
    [XmlElement ("Project")]
    public virtual Project XmlSerializationProject
    {
      get { return this.Project as Project; }
      set { this.Project = value; }
    }

    /// <summary>
    /// Associated component type
    /// </summary>
    [MergeAuto, XmlIgnore]
    public virtual IComponentType Type
    {
      get { return m_type; }
      set { m_type = value; }
    }

    /// <summary>
    /// Associated component type for XML Serialization
    /// </summary>
    [XmlElement ("Type")]
    public virtual ComponentType XmlSerializationType
    {
      get { return this.Type as ComponentType; }
      set { this.Type = value; }
    }

    /// <summary>
    /// Reference to the final work piece (in IntermediateWorkPiece table)
    /// that corresponds to the finished component
    /// </summary>
    [XmlIgnore, MergeObject]
    public virtual IIntermediateWorkPiece FinalWorkPiece
    {
      get { return m_finalWorkPiece; }
      set { m_finalWorkPiece = value; }
    }

    /// <summary>
    /// Estimated hours
    /// </summary>
    [XmlIgnore, MergeAuto]
    public virtual double? EstimatedHours
    {
      get { return m_estimatedHours; }
      set { m_estimatedHours = value; }
    }

    /// <summary>
    /// Estimated hours as string
    /// 
    /// Takes the value null in case EstimatedHours is null
    /// </summary>
    [XmlAttribute ("EstimatedHours")]
    public virtual string EstimatedHoursAsString
    {
      get {
        return (this.EstimatedHours.HasValue)
          ? this.EstimatedHours.Value.ToString (CultureInfo.InvariantCulture)
          : null;
      }
      set {
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
    public virtual ICollection<IComponentIntermediateWorkPiece> ComponentIntermediateWorkPieces
    {
      get {
        if (null == m_componentIntermediateWorkPieces) {
          m_componentIntermediateWorkPieces = new List<IComponentIntermediateWorkPiece> ();
        }
        return m_componentIntermediateWorkPieces;
      }
    }

    /// <summary>
    /// List of stamps (and then ISO files) that are associated to this component
    /// </summary>
    [XmlIgnore, MergeChildren ("Component")]
    public virtual ICollection<IStamp> Stamps
    {
      get {
        return m_stamps;
      }
    }
    #endregion // Getters / Setters

    #region Contructors
    /// <summary>
    /// The default constructor is forbidden
    /// and is only used by NHibernate
    /// </summary>
    protected Component ()
    { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="project"></param>
    /// <param name="componentType"></param>
    internal protected Component (IProject project, IComponentType componentType)
    {
      m_project = project;
      if (m_project != null) {
        // Add the component to the new project
        (m_project as Project).AddComponentForInternalUse (this);
      }

      m_type = componentType;
    }
    #endregion // Constructors

    #region Add methods
    /// <summary>
    /// Check if the component is undefined
    /// 
    /// A component is considered as undefined if it has no name and no given type
    /// </summary>
    /// <returns></returns>
    public virtual bool IsUndefined ()
    {
      return ((this.Name == null)
              || (0 == this.Name.Length))
        && (1 == ((Lemoine.Collections.IDataWithId)this.Type).Id);
    }

    /// <summary>
    /// Add an intermediate work piece to the component
    /// without precising the code or order
    /// 
    /// If the intermediate work piece is already part of the component,
    /// nothing is done
    /// </summary>
    /// <param name="intermediateWorkPiece">The intermediate work piece to add (can't be null)</param>
    /// <returns></returns>
    public virtual IComponentIntermediateWorkPiece AddIntermediateWorkPiece (IIntermediateWorkPiece intermediateWorkPiece)
    {
      Debug.Assert (null != intermediateWorkPiece);

      foreach (IComponentIntermediateWorkPiece componentIntermediateWorkPiece in ComponentIntermediateWorkPieces) {
        if (componentIntermediateWorkPiece.IntermediateWorkPiece.Equals (intermediateWorkPiece)) {
          log.DebugFormat ("AddIntermediateWorkPiece: " +
                           "{0} is already part of {1}",
                           intermediateWorkPiece, this);
          return componentIntermediateWorkPiece;
        }
      }

      // Does not contain the intermediate work piece
      IComponentIntermediateWorkPiece newComponentIntermediateWorkPiece =
        new ComponentIntermediateWorkPiece (this, intermediateWorkPiece);
      ComponentIntermediateWorkPieces.Add (newComponentIntermediateWorkPiece);
      (intermediateWorkPiece as IntermediateWorkPiece)
        .AddComponentForInternalUse (newComponentIntermediateWorkPiece);
      return newComponentIntermediateWorkPiece;
    }

    /// <summary>
    /// Add an intermediate work piece to the component
    /// precising the code
    /// 
    /// If the intermediate work piece is already part of the component,
    /// nothing is done
    /// </summary>
    /// <param name="intermediateWorkPiece">The intermediate work piece to add (can't be null)</param>
    /// <param name="code"></param>
    /// <returns></returns>
    public virtual IComponentIntermediateWorkPiece AddIntermediateWorkPiece (IIntermediateWorkPiece intermediateWorkPiece,
                                                                             string code)
    {
      Debug.Assert (null != intermediateWorkPiece);

      foreach (IComponentIntermediateWorkPiece componentIntermediateWorkPiece in ComponentIntermediateWorkPieces) {
        if (componentIntermediateWorkPiece.IntermediateWorkPiece.Equals (intermediateWorkPiece)
            && componentIntermediateWorkPiece.Code.Equals (code)) {
          log.DebugFormat ("AddIntermediateWorkPiece: " +
                           "{0} is already part of {1}",
                           intermediateWorkPiece, this);
          return componentIntermediateWorkPiece;
        }
      }

      // Does not contain the intermediate work piece
      IComponentIntermediateWorkPiece newComponentIntermediateWorkPiece =
        new ComponentIntermediateWorkPiece (this, intermediateWorkPiece);
      newComponentIntermediateWorkPiece.Code = code;
      ComponentIntermediateWorkPieces.Add (newComponentIntermediateWorkPiece);
      (intermediateWorkPiece as IntermediateWorkPiece)
        .AddComponentForInternalUse (newComponentIntermediateWorkPiece);
      return newComponentIntermediateWorkPiece;
    }

    /// <summary>
    /// Add an intermediate work piece to the component
    /// precising the order
    /// 
    /// If the intermediate work piece is already part of the component,
    /// nothing is done
    /// </summary>
    /// <param name="intermediateWorkPiece">The intermediate work piece to add (can't be null)</param>
    /// <param name="order"></param>
    /// <returns></returns>
    public virtual IComponentIntermediateWorkPiece AddIntermediateWorkPiece (IIntermediateWorkPiece intermediateWorkPiece,
                                                                             int order)
    {
      Debug.Assert (null != intermediateWorkPiece);

      foreach (IComponentIntermediateWorkPiece componentIntermediateWorkPiece in ComponentIntermediateWorkPieces) {
        if (componentIntermediateWorkPiece.IntermediateWorkPiece.Equals (intermediateWorkPiece)
            && componentIntermediateWorkPiece.Order.Equals (order)) {
          log.DebugFormat ("AddIntermediateWorkPiece: " +
                           "{0} is already part of {1}",
                           intermediateWorkPiece, this);
          return componentIntermediateWorkPiece;
        }
      }

      // Does not contain the intermediate work piece
      IComponentIntermediateWorkPiece newComponentIntermediateWorkPiece =
        new ComponentIntermediateWorkPiece (this, intermediateWorkPiece);
      newComponentIntermediateWorkPiece.Order = order;
      ComponentIntermediateWorkPieces.Add (newComponentIntermediateWorkPiece);
      (intermediateWorkPiece as IntermediateWorkPiece)
        .AddComponentForInternalUse (newComponentIntermediateWorkPiece);
      return newComponentIntermediateWorkPiece;
    }

    /// <summary>
    /// Remove an intermediate work piece from the component
    /// </summary>
    /// <param name="intermediateWorkPiece"></param>
    public virtual IList<IComponentIntermediateWorkPiece> RemoveIntermediateWorkPiece (IIntermediateWorkPiece intermediateWorkPiece)
    {
      IList<IComponentIntermediateWorkPiece> componentIntermediateWorkPieceToRemove =
        new List<IComponentIntermediateWorkPiece> ();
      foreach (IComponentIntermediateWorkPiece componentIntermediateWorkPiece in m_componentIntermediateWorkPieces) {
        if (componentIntermediateWorkPiece.IntermediateWorkPiece.Equals (intermediateWorkPiece)) {
          componentIntermediateWorkPieceToRemove.Add (componentIntermediateWorkPiece);
        }
      }
      foreach (IComponentIntermediateWorkPiece componentIntermediateWorkPiece in componentIntermediateWorkPieceToRemove) {
        RemoveComponentIntermediateWorkPiece (componentIntermediateWorkPiece);
      }
      return componentIntermediateWorkPieceToRemove;
    }

    /// <summary>
    /// Remove a component / intermediate work piece association
    /// </summary>
    /// <param name="componentIntermediateWorkPiece"></param>
    protected virtual void RemoveComponentIntermediateWorkPiece (IComponentIntermediateWorkPiece componentIntermediateWorkPiece)
    {
      ComponentIntermediateWorkPieces.Remove (componentIntermediateWorkPiece);
      (componentIntermediateWorkPiece.IntermediateWorkPiece as IntermediateWorkPiece)
        .RemoveComponentForInternalUse (componentIntermediateWorkPiece);
    }
    #endregion // Add methods

    #region IMergeable implementation
    /// <summary>
    /// <see cref="IMergeable&lt;T&gt;.Merge" />
    /// </summary>
    /// <param name="other"></param>
    /// <param name="conflictResolution"></param>
    public virtual void Merge (Component other,
                              ConflictResolution conflictResolution)
    {
      Mergeable.MergeAuto (this, other, conflictResolution);
      // There is no need to update the quantity here
      ModifyItems<IComponentIntermediateWorkPiece>
        (other.ComponentIntermediateWorkPieces,
         new Modifier<IComponentIntermediateWorkPiece>
         (delegate (IComponentIntermediateWorkPiece componentIntermediateWorkPiece) {
           if (!string.IsNullOrEmpty (componentIntermediateWorkPiece.Code)) {
             IComponentIntermediateWorkPiece newComponentIntermediateWorkPiece =
               this.AddIntermediateWorkPiece (componentIntermediateWorkPiece.IntermediateWorkPiece,
                                              componentIntermediateWorkPiece.Code);
             ModelDAOHelper.DAOFactory.ComponentIntermediateWorkPieceDAO.MakePersistent (newComponentIntermediateWorkPiece);
           }
           else if (componentIntermediateWorkPiece.Order.HasValue) {
             IComponentIntermediateWorkPiece newComponentIntermediateWorkPiece =
               this.AddIntermediateWorkPiece (componentIntermediateWorkPiece.IntermediateWorkPiece,
                                              componentIntermediateWorkPiece.Order.Value);
             ModelDAOHelper.DAOFactory.ComponentIntermediateWorkPieceDAO.MakePersistent (newComponentIntermediateWorkPiece);
           }
           else {
             IComponentIntermediateWorkPiece newComponentIntermediateWorkPiece =
               this.AddIntermediateWorkPiece (componentIntermediateWorkPiece.IntermediateWorkPiece);
             ModelDAOHelper.DAOFactory.ComponentIntermediateWorkPieceDAO.MakePersistent (newComponentIntermediateWorkPiece);
           }
           IComponentIntermediateWorkPieceUpdate update = ModelDAOHelper.ModelFactory
             .CreateComponentIntermediateWorkPieceUpdate (this, componentIntermediateWorkPiece.IntermediateWorkPiece, ComponentIntermediateWorkPieceUpdateModificationType.NEW);
           ModelDAOHelper.DAOFactory.ComponentIntermediateWorkPieceUpdateDAO.MakePersistent (update);

           other.RemoveComponentIntermediateWorkPiece (componentIntermediateWorkPiece);
           update = ModelDAOHelper.ModelFactory
             .CreateComponentIntermediateWorkPieceUpdate (other, componentIntermediateWorkPiece.IntermediateWorkPiece, ComponentIntermediateWorkPieceUpdateModificationType.DELETE);
           ModelDAOHelper.DAOFactory.ComponentIntermediateWorkPieceUpdateDAO.MakePersistent (update);
         }));
      // TODO: Restrictions following DataStructureOption
    }
    #endregion // IMergeable implementation

    #region Methods
    /// <summary>
    /// Add a stamp in the member directly
    /// 
    /// To be used by the Stamp class only
    /// </summary>
    /// <param name="stamp"></param>
    protected internal virtual void AddStampForInternalUse (IStamp stamp)
    {
      AddToProxyCollection<IStamp> (m_stamps, stamp);
    }

    /// <summary>
    /// Remove a stamp in the member directly
    /// 
    /// To be used by the Stamp class only
    /// </summary>
    /// <param name="stamp"></param>
    protected internal virtual void RemoveStampForInternalUse (IStamp stamp)
    {
      RemoveFromProxyCollection<IStamp> (m_stamps, stamp);
    }
    #endregion // Methods

    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public virtual void Unproxy ()
    {
      NHibernateHelper.Unproxy<IProject> (ref m_project);
      NHibernateHelper.Unproxy<IComponentType> (ref m_type);
    }

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[Component {this.Id} Name={this.Name}]";
      }
      else {
        return $"[Component {this.Id}]";
      }
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
      IComponent other = obj as Component;
      if (null == other) {
        return false;
      }
      if (this.Id != 0) {
        return (((Lemoine.Collections.IDataWithId)other).Id == ((Lemoine.Collections.IDataWithId)this).Id);
      }
      return false;
    }

    /// <summary>
    ///   Determines whether the specified Object
    ///   is equal to the current Object
    /// </summary>
    /// <param name="other">The object to compare with the current object</param>
    /// <returns>true if the specified Object is equal to the current Object; otherwise, false</returns>
    public virtual bool Equals (IComponent other)
    {
      return this.Equals ((object)other);
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
