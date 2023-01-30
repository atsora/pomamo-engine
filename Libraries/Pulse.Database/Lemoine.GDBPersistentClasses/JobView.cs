// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

using Lemoine.Collections;
using Lemoine.Model;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of view Job
  /// that is a join between WorkOrder and Project
  /// in case the data structure option WorkOrderProjectIsJob is set
  /// 
  /// Use this class very carefully. Each time it is possible, prefer to use
  /// the Job class instead.
  /// Any modification made in JobView is not propagated to the other classes,
  /// which may cause some problems.
  /// </summary>
  [Serializable]
  public class JobView: BaseData
  {
    #region Members
    int m_projectId = 0;
    int m_workOrderId = 0;
    int m_projectVersion = 0;
    int m_workOrderVersion = 0;
    string m_name;
    string m_code;
    string m_externalCode;
    string m_documentLink;
    DateTime? m_deliveryDate;
    IWorkOrderStatus m_status;
    DateTime m_creationDateTime = DateTime.UtcNow;
    DateTime m_reactivationDateTime = DateTime.UtcNow;
    DateTime? m_archiveDateTime;
    ICollection <IComponent> m_components = new InitialNullIdSet<IComponent, int> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (JobView).FullName);

    #region Getters / Setters
    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] {"ExternalCode", "Name", "Code"}; }
    }
    
    /// <summary>
    /// Job ID that corresponds also to the Project ID
    /// </summary>
    [XmlAttribute("ProjectId")]
    public virtual int ProjectId
    {
      get { return this.m_projectId; }
    }
    
    /// <summary>
    /// Work Order ID
    /// </summary>
    [XmlAttribute("WorkOrderId")]
    public virtual int WorkOrderId {
      get { return m_workOrderId; }
    }
    
    /// <summary>
    /// Project Version
    /// </summary>
    [XmlIgnore]
    public virtual int ProjectVersion {
      get { return m_projectVersion; }
    }

    /// <summary>
    /// WorkOrder Version
    /// </summary>
    [XmlIgnore]
    public virtual int WorkOrderVersion {
      get { return m_workOrderVersion; }
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
    /// Job name that corresponds also to
    /// the project name and the work order name
    /// </summary>
    [XmlAttribute("Name")]
    public virtual string Name
    {
      get { return this.m_name; }
      set { this.m_name = value; }
    }
    
    /// <summary>
    /// Job code that corresponds also to
    /// the project code and the work order code
    /// </summary>
    [XmlAttribute("Code")]
    public virtual string Code {
      get { return m_code; }
      set { m_code = value; }
    }
    
    /// <summary>
    /// Job external code that corresponds to
    /// the project external code and the work order external code
    /// </summary>
    [XmlAttribute("ExternalCode")]
    public virtual string ExternalCode {
      get { return m_externalCode; }
      set { m_externalCode = value; }
    }
    
    /// <summary>
    /// Job document link that corresponds to
    /// the project document link and the work order document link
    /// </summary>
    [XmlAttribute("DocumentLink")]
    public virtual string DocumentLink {
      get { return m_documentLink; }
      set { m_documentLink = value; }
    }
    
    /// <summary>
    /// Work order delivery date
    /// </summary>
    [XmlIgnore]
    public virtual DateTime? DeliveryDate {
      get { return m_deliveryDate; }
      set { m_deliveryDate = value; }
    }
    
    /// <summary>
    /// Work order status
    /// </summary>
    [XmlIgnore]
    public virtual IWorkOrderStatus Status {
      get { return m_status; }
      set { m_status = value; }
    }

    /// <summary>
    /// Work order status for Xml Serialization
    /// </summary>
    [XmlElement("Status")]
    public virtual WorkOrderStatus XmlSerializationStatus {
      get { return this.Status as WorkOrderStatus; }
      set { this.Status = value; }
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
    /// Components that are associated to the project
    /// </summary>
    [XmlIgnore]
    public virtual ICollection <IComponent> Components {
      get
      {
        return m_components;
      }
    }
    #endregion // Getters / Setters
    
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
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[JobView {this.ProjectId} Code={this.Code} Name={this.Name}]";
      }
      else {
        return $"[JobView {this.ProjectId}]";
      }
    }
    #endregion
  }
}
