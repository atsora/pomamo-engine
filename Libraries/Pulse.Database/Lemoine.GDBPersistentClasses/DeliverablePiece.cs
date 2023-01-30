// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Database.Persistent;
using Lemoine.Core.Log;
using System.Xml.Serialization;
using Lemoine.Model;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Description of DeliverablePiece.
  /// </summary>
  public class DeliverablePiece : DataWithDisplayFunction, IDeliverablePiece
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    string m_code;
    IComponent m_component;
    IWorkOrder m_workOrder;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (DeliverablePiece).FullName);
  
    #region Getters / Setters
    
    /// <summary>
    /// identifier
    /// </summary>
    [XmlAttribute("Id")]
    public virtual int Id {
      get { return this.m_id; }
    }
    
    /// <summary>
    /// code (serial number): nullable
    /// </summary>
    [XmlAttribute("Code")]
    public virtual string Code {
      get { return m_code; }
      set { m_code = value; }
    }
    
    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] {"Id", "Code"}; }
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
    /// Component
    /// </summary>
    [XmlIgnore]
    public virtual IComponent Component {
      get { return m_component; }
      set { m_component = value; }
    }

    /// <summary>
    /// Work order
    /// </summary>
    [XmlIgnore]
    public virtual IWorkOrder WorkOrder {
      get { return m_workOrder; }
      set { m_workOrder = value; }
    }

    /// <summary>
    /// Work order for XML serialization
    /// </summary>
    [XmlElement]
    public virtual WorkOrder XmlSerializationWorkOrder {
      get { return this.WorkOrder as WorkOrder; }
      set { this.WorkOrder = value; }
    }
    #endregion // Getters / Setters

    #region Methods

    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public virtual void Unproxy ()
    {
      NHibernateHelper.Unproxy<IComponent> (ref m_component);
      NHibernateHelper.Unproxy<IWorkOrder> (ref m_workOrder);
    }
    
    #endregion // Methods
  }
}
