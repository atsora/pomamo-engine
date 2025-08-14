// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

using Lemoine.Model;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.GDBPersistentClasses;

namespace Lemoine.Plugin.IntermediateWorkPieceSummary
{
  /// <summary>
  /// Persistent class of table IntermediateWorkPieceSummary
  /// 
  /// It contains various information on an intermediate work piece
  /// </summary>
  [Serializable]
  public class IntermediateWorkPieceByMachineSummary : BaseData, IIntermediateWorkPieceByMachineSummary, IVersionable
  {
    int m_id = 0;
    int m_version = 0;
    IMachine m_machine;
    IIntermediateWorkPiece m_intermediateWorkPiece;
    IComponent m_component = null;
    IWorkOrder m_workOrder = null;
    ILine m_line = null;
    IManufacturingOrder m_manufacturingOrder = null;
    DateTime? m_day = null;
    IShift m_shift = null;
    int m_counted = 0;
    int m_corrected = 0;
    int m_checked = 0;
    int m_scrapped = 0;
    int? m_targeted = null;
    // Note: if you add some summary values here, do not forget to update the method IsEmpty () accordingly

    static readonly ILog log = LogManager.GetLogger (typeof (IntermediateWorkPieceByMachineSummary).FullName);

    /// <summary>
    /// Possible identifiers
    /// </summary>
    [XmlIgnore]
    public override string[] Identifiers
    {
      get { return new string[] { "Id", "Machine", "IntermediateWorkPiece", "Component", "WorkOrder" }; }
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
    /// IntermediateWorkPieceSummary Version
    /// </summary>
    [XmlIgnore]
    public virtual int Version
    {
      get { return this.m_version; }
    }

    /// <summary>
    /// Reference to the Machine
    /// 
    /// Not null
    /// </summary>
    [XmlIgnore]
    public virtual IMachine Machine
    {
      get
      {
        Debug.Assert (null != m_machine);
        return m_machine;
      }
      protected set { m_machine = value; }
    }

    /// <summary>
    /// Reference to the related machine for Xml Serialization
    /// 
    /// Be careful when set is used ! This is part of a secondary key
    /// </summary>
    [XmlElement ("Machine")]
    public virtual Machine XmlSerializationMachine
    {
      get { return this.Machine as Machine; }
      set { this.Machine = value; }
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
    /// Reference to the ManufacturingOrder if known
    /// 
    /// Set to null if it could not be identified yet or it if is not applicable
    /// </summary>
    [XmlIgnore]
    public virtual IManufacturingOrder ManufacturingOrder
    {
      get { return m_manufacturingOrder; }
      protected set { m_manufacturingOrder = value; }
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
                           "new counted value {0} is negative !", value);
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
                          "new corrected value {0} is negative", value);
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

    /// <summary>
    /// Number of targeted work pieces
    /// </summary>
    [XmlIgnore]
    public virtual int? Targeted
    {
      get { return m_targeted; }
      set { m_targeted = value; }
    }

    /// <summary>
    /// Number of targeted work pieces for Xml serialization
    /// </summary>
    [XmlAttribute ("Targeted")]
    public virtual string XmlSerializationTargeted
    {
      get { return m_targeted.ToString (); }
      set
      {
        if (string.IsNullOrEmpty (value)) {
          m_targeted = default (int?);
        }
        else {
          m_targeted = int.Parse (value);
        }
      }
    }

    /// <summary>
    /// The default constructor is forbidden
    /// </summary>
    protected IntermediateWorkPieceByMachineSummary ()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="intermediateWorkPiece"></param>
    public IntermediateWorkPieceByMachineSummary (IMachine machine,
                                                              IIntermediateWorkPiece intermediateWorkPiece)
    {
      m_machine = machine;
      m_intermediateWorkPiece = intermediateWorkPiece;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="intermediateWorkPiece"></param>
    /// <param name="component"></param>
    /// <param name="workOrder"></param>
    /// <param name="line"></param>
    /// <param name="manufacturingOrder"></param>
    /// <param name="day"></param>
    /// <param name="shift"></param>
    public IntermediateWorkPieceByMachineSummary (IMachine machine,
                                                              IIntermediateWorkPiece intermediateWorkPiece,
                                                              IComponent component,
                                                              IWorkOrder workOrder,
                                                              ILine line,
                                                              IManufacturingOrder manufacturingOrder,
                                                              DateTime? day,
                                                              IShift shift)
    {
      m_machine = machine;
      m_intermediateWorkPiece = intermediateWorkPiece;
      m_component = component;
      m_workOrder = workOrder;
      m_line = line;
      m_manufacturingOrder = manufacturingOrder;
      m_day = day;
      m_shift = shift;
    }

    /// <summary>
    /// Is the data empty ? It means may it be deleted because all the data are null ?
    /// </summary>
    /// <returns></returns>
    public virtual bool IsEmpty ()
    {
      return (0 == this.Counted)
        && (0 == this.Corrected)
        && (0 == this.Checked)
        && (0 == this.Scrapped)
        && (!this.Targeted.HasValue || (0 == this.Targeted.Value));
    }
  }
}
