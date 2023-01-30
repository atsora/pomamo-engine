// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Xml.Serialization;
using Lemoine.Model;
using Lemoine.Core.Log;
using NHibernate.Type;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table goal
  /// </summary>
  [Serializable]
  public class Goal: IGoal, IVersionable
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    IGoalType m_type = null;
    double m_value = 0;
    IMachineObservationState m_machineObservationState = null;
    ICompany m_company = null;
    IDepartment m_department = null;
    IMachineCategory m_machineCategory = null;
    IMachineSubCategory m_machineSubCategory = null;
    ICell m_cell = null;
    IMachine m_machine = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (Goal).FullName);

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected internal Goal()
    {
    }
    
    /// <summary>
    /// Constructor for default values
    /// </summary>
    /// <param name="type">Can't be null</param>
    public Goal(IGoalType type)
    {
      if (type == null) {
        log.Error("Attempted to create a goal with a null value as type");
        throw new ArgumentNullException();
      }
      m_type = type;
    }
    #endregion // Constructors

    #region Getters / Setters
    /// <summary>
    /// ID
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
    /// Type
    /// </summary>
    [XmlAttribute("Type")]
    public virtual IGoalType Type
    {
      get { return m_type; }
    }
    
    /// <summary>
    /// Value
    /// </summary>
    [XmlAttribute("Value")]
    public virtual double Value {
      get { return m_value; }
      set { m_value = value; }
    }
    
    /// <summary>
    /// MachineObservationState
    /// </summary>
    [XmlAttribute("MachineObservationState")]
    public virtual IMachineObservationState MachineObservationState {
      get { return m_machineObservationState; }
      set { m_machineObservationState = value; }
    }
    
    /// <summary>
    /// Company
    /// </summary>
    [XmlAttribute("Company")]
    public virtual ICompany Company {
      get { return m_company; }
      set { m_company = value; }
    }
    
    /// <summary>
    /// Department
    /// </summary>
    [XmlAttribute("Department")]
    public virtual IDepartment Department {
      get { return m_department; }
      set { m_department = value; }
    }
    
    /// <summary>
    /// Machine category
    /// </summary>
    [XmlAttribute("Category")]
    public virtual IMachineCategory Category {
      get { return m_machineCategory; }
      set { m_machineCategory = value; }
    }
    
    /// <summary>
    /// Machine subcategory
    /// </summary>
    [XmlAttribute("SubCategory")]
    public virtual IMachineSubCategory SubCategory {
      get { return m_machineSubCategory; }
      set { m_machineSubCategory = value; }
    }
    
    /// <summary>
    /// Cell
    /// </summary>
    [XmlAttribute("Cell")]
    public virtual ICell Cell {
      get { return m_cell; }
      set { m_cell = value; }
    }
    
    /// <summary>
    /// Machine
    /// </summary>
    [XmlAttribute("Machine")]
    public virtual IMachine Machine {
      get { return m_machine; }
      set { m_machine = value; }
    }
    #endregion // Getters / Setters
  }
}
