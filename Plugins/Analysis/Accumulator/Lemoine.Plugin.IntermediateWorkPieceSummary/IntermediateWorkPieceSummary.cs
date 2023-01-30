// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Xml.Serialization;

using Lemoine.GDBMigration;
using Lemoine.Model;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.GDBPersistentClasses;
using Lemoine.Collections;

namespace Lemoine.Plugin.IntermediateWorkPieceSummary
{
  /// <summary>
  /// Persistent class of view IntermediateWorkPieceSummary
  /// 
  /// It contains various information on an intermediate work piece
  /// </summary>
  [Serializable]
  public class IntermediateWorkPieceSummary : BaseData, IIntermediateWorkPieceSummary, IDataWithId
  {
    #region Members
    int m_id = 0;
    IIntermediateWorkPiece m_intermediateWorkPiece;
    IComponent m_component = null;
    IWorkOrder m_workOrder = null;
    ILine m_line = null;
    DateTime? m_day = null;
    IShift m_shift = null;
    int m_counted = 0;
    int m_corrected = 0;
    int m_checked = 0;
    int m_scrapped = 0;
    // Note: if you add some summary values here, do not forget to update the method IsEmpty () accordingly
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (IntermediateWorkPieceSummary).FullName);

    #region Getters / Setters
    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] { "Id", "IntermediateWorkPiece", "Component", "WorkOrder" }; }
    }

    /// <summary>
    /// IntermediateWorkPieceSummary Id
    /// </summary>
    [XmlAttribute ("Id")]
    public virtual int Id
    {
      get { return this.m_id; }
    }

    /// <summary>
    /// Reference to the IntermediateWorkPiece
    /// 
    /// Not null
    /// </summary>
    [XmlIgnore]
    public virtual IIntermediateWorkPiece IntermediateWorkPiece
    {
      get
      {
        Debug.Assert (null != m_intermediateWorkPiece);
        return m_intermediateWorkPiece;
      }
      protected set { m_intermediateWorkPiece = value; }
    }

    /// <summary>
    /// Reference to the related intermediate work piece for Xml Serialization
    /// 
    /// Be careful when set is used ! This is part of a secondary key
    /// </summary>
    [XmlElement ("IntermediateWorkPiece")]
    public virtual IntermediateWorkPiece XmlSerializationIntermediateWorkPiece
    {
      get { return this.IntermediateWorkPiece as IntermediateWorkPiece; }
      set { this.IntermediateWorkPiece = value; }
    }

    /// <summary>
    /// Reference to the associated Component
    /// 
    /// Set to null if unknown
    /// </summary>
    [XmlIgnore]
    public virtual IComponent Component
    {
      get { return m_component; }
      protected set { m_component = value; }
    }

    /// <summary>
    /// Reference to the related component for Xml Serialization
    /// 
    /// Be careful when set is used ! This is part of a secondary key
    /// </summary>
    [XmlElement ("Component")]
    public virtual Component XmlSerializationComponent
    {
      get { return this.Component as Component; }
      set { this.Component = value; }
    }

    /// <summary>
    /// Reference to the Work Order if known
    /// 
    /// Set to null if it could not be identified yet
    /// </summary>
    [XmlIgnore]
    public virtual IWorkOrder WorkOrder
    {
      get { return m_workOrder; }
      protected set { m_workOrder = value; }
    }

    /// <summary>
    /// Reference to the related work order for Xml Serialization
    /// 
    /// Be careful when set is used ! This is part of a secondary key
    /// </summary>
    [XmlElement ("WorkOrder")]
    public virtual WorkOrder XmlSerializationWorkOrder
    {
      get { return this.WorkOrder as WorkOrder; }
      set { this.WorkOrder = value; }
    }

    /// <summary>
    /// Reference to the Line if known
    /// 
    /// Set to null if it could not be identified yet or it if is not applicable
    /// </summary>
    [XmlIgnore]
    public virtual ILine Line
    {
      get { return m_line; }
      protected set { m_line = value; }
    }

    /// <summary>
    /// Reference to the related line for Xml Serialization
    /// 
    /// Be careful when set is used ! This is part of a secondary key
    /// </summary>
    [XmlElement ("Line")]
    public virtual Line XmlSerializationLine
    {
      get { return this.Line as Line; }
      set { this.Line = value; }
    }

    /// <summary>
    /// If the option to split the operation slots by day is set,
    /// reference to the day.
    /// 
    /// null if the option to split the operation slot by day is not set
    /// </summary>
    [XmlIgnore]
    public virtual DateTime? Day
    {
      get { return m_day; }
      internal protected set { m_day = value; }
    }

    /// <summary>
    /// If the corresponding option is selected,
    /// reference to the shift.
    /// 
    /// null if there is no shift
    /// or if the option to split the operation slot by shift is not set
    /// </summary>
    [XmlIgnore]
    public virtual IShift Shift
    {
      get { return m_shift; }
      internal protected set { m_shift = value; }
    }

    /// <summary>
    /// Reference to the related shift for Xml Serialization
    /// 
    /// Be careful when set is used ! This is part of a secondary key
    /// </summary>
    [XmlElement ("Shift")]
    public virtual Shift XmlSerializationShift
    {
      get { return this.Shift as Shift; }
      set { this.Shift = value; }
    }

    /// <summary>
    /// Total number of work pieces as detected from the cycle detection
    /// </summary>
    [XmlAttribute ("Counted")]
    public virtual int Counted
    {
      get { return m_counted; }
      set
      {
        Debug.Assert (0 <= value);
        if (value < 0) {
          log.ErrorFormat ("Counted: " +
                           "new counted value {0} is negative !",
                           value);
        }
        m_counted = value;
      }
    }

    /// <summary>
    /// Corrected number of work pieces that takes also into account the input information
    /// </summary>
    [XmlAttribute ("Corrected")]
    public virtual int Corrected
    {
      get { return m_corrected; }
      set
      {
        if (value < 0) {
          log.WarnFormat ("Corrected: " +
                          "new corrected value {0} is negative !",
                          value);
        }
        m_corrected = value;
      }
    }

    /// <summary>
    /// Number of checked work pieces
    /// </summary>
    [XmlAttribute ("Checked")]
    public virtual int Checked
    {
      get { return m_checked; }
      set { m_checked = value; }
    }

    /// <summary>
    /// Number of scrapped work pieces
    /// </summary>
    [XmlAttribute ("Scrapped")]
    public virtual int Scrapped
    {
      get { return m_scrapped; }
      set { m_scrapped = value; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// The default constructor is forbidden
    /// </summary>
    protected IntermediateWorkPieceSummary ()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="intermediateWorkPiece"></param>
    internal protected IntermediateWorkPieceSummary (IIntermediateWorkPiece intermediateWorkPiece)
    {
      m_intermediateWorkPiece = intermediateWorkPiece;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="intermediateWorkPiece"></param>
    /// <param name="component"></param>
    /// <param name="workOrder"></param>
    /// <param name="line"></param>
    /// <param name="day"></param>
    /// <param name="shift"></param>
    internal protected IntermediateWorkPieceSummary (IIntermediateWorkPiece intermediateWorkPiece,
                                                     IComponent component,
                                                     IWorkOrder workOrder,
                                                     ILine line,
                                                     DateTime? day,
                                                     IShift shift)
    {
      m_intermediateWorkPiece = intermediateWorkPiece;
      m_component = component;
      m_workOrder = workOrder;
      m_line = line;
      m_day = day;
      m_shift = shift;
    }
    #endregion // Getters / Setters

    /// <summary>
    /// Is the data empty ? It means may it be deleted because all the data are null ?
    /// </summary>
    /// <returns></returns>
    public virtual bool IsEmpty ()
    {
      return (0 == this.Counted)
        && (0 == this.Corrected)
        && (0 == this.Checked)
        && (0 == this.Scrapped);
    }
  }
}
