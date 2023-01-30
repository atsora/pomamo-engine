// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

using Lemoine.Model;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Description of OperationCycleDeliverablePiece.
  /// </summary>
  public class OperationCycleDeliverablePiece : BaseData, IOperationCycleDeliverablePiece
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    IDeliverablePiece m_deliverablePiece;
    IOperationCycle m_operationCycle;
    IMachine m_machine;
    INonConformanceReason m_nonConformanceReason;
    string m_nonConformanceDetails;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (OperationCycleDeliverablePiece).FullName);

    #region Getters / Setters
    
    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] {"DeliverablePiece", "OperationCycle"}; }
    }

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
    /// Reference to the related deliverable piece
    /// 
    /// Be careful when set is used ! This is part of a secondary key
    /// 
    /// Not null
    /// </summary>
    [XmlIgnore]
    public virtual IDeliverablePiece DeliverablePiece {
      get { return m_deliverablePiece; }
      set
      {
        Debug.Assert (null != value);
        if (null == value) {
          log.FatalFormat ("DeliverablePiece.set: " +
                           "null value");
          throw new ArgumentNullException ("OperationCycleDeliverablePiece.DeliverablePiece");
        }
        else {
          m_deliverablePiece = value;
        }
      }
    }
    
    /// <summary>
    /// Reference to the related deliverable piece for Xml Serialization
    /// 
    /// Be careful when set is used ! This is part of a secondary key
    /// </summary>
    [XmlElement("DeliverablePiece")]
    public virtual DeliverablePiece XmlSerializationDeliverablePiece {
      get { return this.DeliverablePiece as DeliverablePiece; }
      set { this.DeliverablePiece = value; }
    }
    
    /// <summary>
    /// Reference to the related operation cycle
    /// 
    /// Be careful when set is used ! This is part of a secondary key
    /// 
    /// Not null
    /// </summary>
    [XmlIgnore]
    public virtual IOperationCycle OperationCycle {
      get { return m_operationCycle; }
      set
      {
        Debug.Assert (null != value);
        if (null == value) {
          log.FatalFormat ("OperationCycle.set: " +
                           "null value");
          throw new ArgumentNullException ("OperationCycleDeliverablePiece.OperationCycle");
        }
        else {
          m_operationCycle = value;
          Debug.Assert (null != value.Machine);
          m_machine = value.Machine;
        }
      }
    }
    
    /// <summary>
    /// Reference to the related operation cycle for Xml Serialization
    /// 
    /// Be careful when set is used ! This is part of a secondary key
    /// </summary>
    [XmlElement("OperationCycle")]
    public virtual OperationCycle XmlSerializationOperationCycle {
      get { return this.OperationCycle as OperationCycle; }
      set { this.OperationCycle = value; }
    }

    /// <summary>
    /// Reference to possible nonconformance reason
    /// </summary>
    [XmlIgnore]
    public virtual INonConformanceReason NonConformanceReason {
      get { return m_nonConformanceReason; }
      set { m_nonConformanceReason = value; }
    }
    
    /// <summary>
    /// Reference to possible NonConformanceReason for Xml Serialization
    /// </summary>
    [XmlElement("NonConformanceReason")]
    public virtual NonConformanceReason XmlSerializationNonConformanceReason {
      get { return this.NonConformanceReason as NonConformanceReason; }
      set { this.NonConformanceReason = value; }
    }
    
    /// <summary>
    /// Reference to possible NonConformanceDetails for Xml Serialization
    /// </summary>
    [XmlElement("NonConformanceDetails")]
    public virtual string XmlSerializationNonConformanceDetails {
      get { return this.NonConformanceDetails as string; }
      set { this.NonConformanceDetails = value; }
    }
    
    /// <summary>
    /// Nonconformance reason details
    /// </summary>
    public virtual string NonConformanceDetails
    {
      get { return this.m_nonConformanceDetails; }
      set { this.m_nonConformanceDetails = value; }
    }
    
    /// <summary>
    /// Reference to the monitored machine
    /// </summary>
    [XmlIgnore]
    public virtual IMachine Machine {
      get { return m_machine; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// The default constructor is forbidden
    /// </summary>
    protected OperationCycleDeliverablePiece ()
    {
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="deliverablePiece"></param>
    /// <param name="operationCycle"></param>
    internal OperationCycleDeliverablePiece (IDeliverablePiece deliverablePiece,
                                             IOperationCycle operationCycle)
    {
      this.DeliverablePiece = deliverablePiece;
      this.OperationCycle = operationCycle; // also assigns this.Machine
    }
    #endregion // Constructors
  }
}
