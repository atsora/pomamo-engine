// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

using Lemoine.Model;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table WorkOrderProject
  /// 
  /// This new table stores the n:n relation 
  /// between the Work Order and the Project.
  /// 
  /// The field Work Order Project Quantity stores 
  /// how many projects must be manufactured in the given work order. 
  /// </summary>
  [Serializable]
  public class WorkOrderProject: BaseData, IWorkOrderProject
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    IWorkOrder m_workOrder;
    IProject m_project;
    int m_quantity = 1;
    #endregion

    static readonly ILog log = LogManager.GetLogger(typeof (WorkOrderProject).FullName);

    #region Constructors
    /// <summary>
    /// The default constructor is forbidden
    /// </summary>
    protected WorkOrderProject ()
    {
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="workOrder"></param>
    /// <param name="project"></param>
    public WorkOrderProject (IWorkOrder workOrder, IProject project)
    {
      m_workOrder = workOrder;
      m_project = project;
    }
    #endregion // Constructors
    
    #region Getters / Setters
    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] {"WorkOrder", "Project"}; }
    }

    /// <summary>
    /// WorkOrderProject ID
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
    /// Reference to the related work order
    /// 
    /// Be careful when set is used ! This is part of a secondary key
    /// </summary>
    [XmlIgnore]
    public virtual IWorkOrder WorkOrder {
      get { return m_workOrder; }
      set { m_workOrder = value; }
    }
    
    /// <summary>
    /// Reference to the related work order for Xml Serialization
    /// 
    /// Be careful when set is used ! This is part of a secondary key
    /// </summary>
    [XmlElement("WorkOrder")]
    public virtual WorkOrder XmlSerializationWorkOrder {
      get { return this.WorkOrder as WorkOrder; }
      set { this.WorkOrder = value; }
    }
    
    /// <summary>
    /// Reference to the related project
    /// 
    /// Be careful when set is used ! This is part of a secondary key
    /// </summary>
    [XmlIgnore]
    public virtual IProject Project {
      get { return m_project; }
      set { m_project = value; }
    }
    
    /// <summary>
    /// Reference to the related project for Xml Serialization
    /// 
    /// Be careful when set is used ! This is part of a secondary key
    /// </summary>
    [XmlElement("Project")]
    public virtual Project XmlSerializationProject {
      get { return this.Project as Project; }
      set { this.Project = value; }
    }

    /// <summary>
    /// Number of project to machien for the given work order
    /// </summary>
    [XmlAttribute("Quantity")]
    public virtual int Quantity {
      get { return m_quantity; }
      set { m_quantity = value; }
    }
    #endregion
  }
}
