// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Serialization;

using Lemoine.Model;
using Lemoine.Collections;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of view Part
  /// that is a join between the Project and the Component tables
  /// in case the data structure option ProjectComponentIsPart is set
  /// 
  /// Use this class very carefully. Each time it is possible, prefer to use
  /// the Part class instead.
  /// Any modification made in PartView is not propagated to the other classes,
  /// which may cause some problems.
  /// </summary>
  [Serializable]
  public class PartView: BaseData, IPartView
  {
    #region Members
    int m_componentId = 0;
    int m_projectId = 0;
    int m_componentVersion = 0;
    int m_projectVersion = 0;
    string m_name;
    string m_code;
    string m_externalCode;
    string m_documentLink;
    DateTime m_creationDateTime = DateTime.UtcNow;
    DateTime m_reactivationDateTime = DateTime.UtcNow;
    DateTime? m_archiveDateTime;
    ICollection <IWorkOrder> m_workOrders = new InitialNullIdSet<IWorkOrder, int>();
    IComponentType m_type;
    IIntermediateWorkPiece m_finalWorkPiece;
    double? m_estimatedHours;
    ICollection<IIntermediateWorkPiece> m_intermediateWorkPieces =
      new List<IIntermediateWorkPiece> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (PartView).FullName);

    #region Getters / Setters
    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] {"ExternalCode", "Code", "Name", "Type"}; }
    }
    
    /// <summary>
    /// Part ID that corresponds also to the Component ID
    /// </summary>
    [XmlAttribute("ComponentId")]
    public virtual int ComponentId
    {
      get { return this.m_componentId; }
    }
    
    /// <summary>
    /// Project ID
    /// </summary>
    [XmlAttribute("ProjectId")]
    public virtual int ProjectId {
      get { return m_projectId; }
    }
    
    /// <summary>
    /// Project Version
    /// </summary>
    [XmlIgnore]
    public virtual int ProjectVersion {
      get { return m_projectVersion; }
    }

    /// <summary>
    /// Component Version
    /// </summary>
    [XmlIgnore]
    public virtual int ComponentVersion {
      get { return m_componentVersion; }
    }
    
    /// <summary>
    /// IDisplay implementation
    /// </summary>
    [XmlIgnore]
    public virtual string Display
    {
      get; set;
    }

    /// <summary>
    /// Full name of the part as used in the shop
    /// that corresponds also to the name of the component
    /// or the name of the project
    /// </summary>
    [XmlAttribute("Name")]
    public virtual string Name
    {
      get { return this.m_name; }
      set { this.m_name = value; }
    }
    
    /// <summary>
    /// Code given to the component
    /// that corresponds also to the code of the component
    /// or the code of the project
    /// </summary>
    [XmlAttribute("Code")]
    public virtual string Code {
      get { return m_code; }
      set { m_code = value; }
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
      get { return m_externalCode; }
      set { m_externalCode = value; }
    }
    
    /// <summary>
    /// Link to the documentation in the network
    /// 
    /// It corresponds also to the document link of the component
    /// and the document link of the project
    /// </summary>
    [XmlAttribute("DocumentLink")]
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
    /// Associated component type
    /// </summary>
    [XmlIgnore]
    public virtual IComponentType Type {
      get { return m_type; }
      set { m_type = value; }
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
      get { return m_finalWorkPiece; }
      set { m_finalWorkPiece = value; }
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
      get { return m_estimatedHours; }
      set { m_estimatedHours = value; }
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
    public virtual ICollection<IIntermediateWorkPiece> IntermediateWorkPieces {
      get { return m_intermediateWorkPieces; }
    }
    #endregion // Getters / Setters
    
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
  }
}
