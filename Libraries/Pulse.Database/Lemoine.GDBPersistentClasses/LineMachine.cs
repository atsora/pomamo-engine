// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Xml;
using System.Xml.Serialization;

using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Association of a machine within a line, having a dedicated operation
  /// </summary>
  public class LineMachine: IVersionable, ILineMachine, ISerializableModel
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    ILine m_line = null;
    IMachine m_machine = null;
    LineMachineStatus m_lineMachineStatus = LineMachineStatus.Dedicated;
    IOperation m_operation = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (LineMachine).FullName);

    #region Getters / Setters
    /// <summary>
    /// ID
    /// </summary>
    [XmlAttribute("Id")]
    public virtual int Id
    {
      get { return m_id; }
    }
    
    /// <summary>
    /// Version
    /// </summary>
    [XmlIgnore]
    public virtual int Version
    {
      get { return m_version; }
    }
    
    /// <summary>
    /// Line (not null)
    /// </summary>
    [XmlAttribute("Line")]
    public virtual ILine Line
    {
      get { return m_line; }
      protected set
      {
        Debug.Assert (null != value);
        if (null == value) {
          log.FatalFormat("Line cannot be null");
          throw new ArgumentNullException("Line");
        }
        m_line = value;
      }
    }
    
    /// <summary>
    /// Machine
    /// </summary>
    [XmlAttribute("Machine")]
    public virtual IMachine Machine
    {
      get { return m_machine; }
      protected set {
        Debug.Assert (null != value);
        if (value == null) {
          log.FatalFormat("Machine cannot be null");
          throw new ArgumentNullException("Machine");
        }
        m_machine = value;
      }
    }
    
    /// <summary>
    /// LineMachineStatus
    /// </summary>
    [XmlAttribute("LineMachineStatus")]
    public virtual LineMachineStatus LineMachineStatus
    {
      get { return m_lineMachineStatus; }
      set { m_lineMachineStatus = value; }
    }
    
    /// <summary>
    /// Operation
    /// </summary>
    [XmlAttribute("Operation")]
    public virtual IOperation Operation
    {
      get { return this.m_operation; }
      set {
        Debug.Assert (null != value);
        if (value == null) {
          log.FatalFormat("Operation cannot be null");
          throw new ArgumentNullException("Operation");
        }
        m_operation = value;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Forbidden default constructor, only reachable by NHibernate
    /// </summary>
    protected internal LineMachine()
    {
    }
    
    /// <summary>
    /// Constructor, no null values are accepted
    /// </summary>
    /// <param name="line">Line comprising the machine</param>
    /// <param name="machine">Machine associated to a line</param>
    /// <param name="operation">Operation associated to the machine within the line</param>
    public LineMachine(ILine line, IMachine machine, IOperation operation)
    {
      Line = line;
      Machine = machine;
      Operation = operation;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public virtual void Unproxy ()
    {
      NHibernateHelper.Unproxy<ILine> (ref m_line);
      NHibernateHelper.Unproxy<IMachine> (ref m_machine);
      NHibernateHelper.Unproxy<IOperation> (ref m_operation);
    }
    #endregion // Methods
  }
}
