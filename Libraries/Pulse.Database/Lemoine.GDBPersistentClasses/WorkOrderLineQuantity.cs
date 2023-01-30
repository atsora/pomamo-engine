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
  public class WorkOrderLineQuantity: IVersionable, IWorkOrderLineQuantity
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    ILine m_line;
    IIntermediateWorkPiece m_intermediateWorkPiece = null;
    int m_quantity = 0;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (WorkOrderLineQuantity).FullName);

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
    /// Line (read-only, not null)
    /// </summary>
    [XmlIgnore]
    public virtual ILine Line
    {
      get { return m_line; }
      protected set {
        Debug.Assert (null != value);
        if (value == null) {
          log.FatalFormat("Line cannot be null");
          throw new ArgumentNullException();
        }
        m_line = value;
      }
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
          throw new ArgumentNullException();
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
    protected WorkOrderLineQuantity()
    {
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="line">line (not null)</param>
    /// <param name="intermediateWorkPiece">IntermediateWorkPiece produced by a WorkOrderLine (not null)</param>
    protected internal WorkOrderLineQuantity(ILine line, IIntermediateWorkPiece intermediateWorkPiece)
    {
      Debug.Assert (null != line);
      
      m_line = line;
      IntermediateWorkPiece = intermediateWorkPiece;
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods
  }
}
