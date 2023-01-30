// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Xml;
using System.Xml.Serialization;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Quantity of IntermediateWorkPiece to produce within a WorkOrderLine
  /// </summary>
  public class WorkOrderLineAssociationQuantity: IVersionable, IWorkOrderLineAssociationQuantity
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    IIntermediateWorkPiece m_intermediateWorkPiece = null;
    int m_quantity = 0;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (WorkOrderLineAssociationQuantity).FullName);

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
    /// IntermediateWorkPiece
    /// </summary>
    [XmlIgnore]
    public virtual IIntermediateWorkPiece IntermediateWorkPiece
    {
      get { return m_intermediateWorkPiece; }
      protected set {
        Debug.Assert (null != value);
        if (value == null) {
          log.FatalFormat("IntermediateWorkPiece cannot be null");
          throw new ArgumentNullException("IntermediateWorkPiece");
        }
        m_intermediateWorkPiece = value;
      }
    }
    
    /// <summary>
    /// Quantity of IntermediateWorkPiece to produce within the WorkOrderLine
    /// </summary>
    [XmlAttribute("Quantity")]
    public virtual int Quantity
    {
      get { return m_quantity; }
      set { m_quantity = value; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Forbidden default constructor, only reachable by NHibernate
    /// </summary>
    protected WorkOrderLineAssociationQuantity()
    {
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="intermediateWorkPiece">IntermediateWorkPiece produced by a WorkOrderLine (not null)</param>
    protected internal WorkOrderLineAssociationQuantity(IIntermediateWorkPiece intermediateWorkPiece)
    {
      IntermediateWorkPiece = intermediateWorkPiece;
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods
  }
}
