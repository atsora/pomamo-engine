// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.Plugin.CycleDurationSummary
{
  /// <summary>
  /// Persistent class of table CycleDurationSummary
  /// </summary>
  [Serializable]
  public class CycleDurationSummary : ICycleDurationSummary, IVersionable
  {
    int m_id = 0;
    int m_version = 0;
    IMachine m_machine;
    DateTime m_day;
    IShift m_shift;
    IWorkOrder m_workOrder;
    ILine m_line;
    IManufacturingOrder m_manufacturingOrder;
    IComponent m_component;
    IOperation m_operation;
    int m_offset;
    int m_number;
    int m_partial = 0;

    static readonly ILog log = LogManager.GetLogger (typeof (CycleDurationSummary).FullName);

    /// <summary>
    /// CycleDurationSummary Id
    /// </summary>
    public virtual int Id
    {
      get { return this.m_id; }
    }

    /// <summary>
    /// CycleDurationSummary Version
    /// </summary>
    public virtual int Version
    {
      get { return this.m_version; }
    }

    /// <summary>
    /// Associated monitored machine
    /// </summary>
    public virtual IMachine Machine
    {
      get { return m_machine; }
    }

    /// <summary>
    /// Associated day
    /// </summary>
    public virtual DateTime Day
    {
      get { return m_day; }
    }

    /// <summary>
    /// Shift that is associated to the operation slot
    /// </summary>
    public virtual IShift Shift
    {
      get { return m_shift; }
    }

    /// <summary>
    /// Associated work order
    /// </summary>
    public virtual IWorkOrder WorkOrder
    {
      get { return m_workOrder; }
    }

    /// <summary>
    /// Associated line
    /// </summary>
    public virtual ILine Line
    {
      get { return m_line; }
    }

    /// <summary>
    /// Associated manufacturing order
    /// </summary>
    public virtual IManufacturingOrder ManufacturingOrder
    {
      get { return m_manufacturingOrder; }
    }

    /// <summary>
    /// Associated component
    /// </summary>
    public virtual IComponent Component
    {
      get { return m_component; }
    }

    /// <summary>
    /// Associated operation
    /// </summary>
    public virtual IOperation Operation
    {
      get { return m_operation; }
    }

    /// <summary>
    /// Associated duration offset (rounded to the closest integer)
    /// </summary>
    public virtual int Offset
    {
      get { return m_offset; }
    }

    /// <summary>
    /// Number of full cycles
    /// </summary>
    public virtual int Number
    {
      get { return m_number; }
      set
      {
        Debug.Assert (0 <= value);
        m_number = value;
      }
    }

    /// <summary>
    /// Number of partial cycles
    /// </summary>
    public virtual int Partial
    {
      get { return m_partial; }
      set
      {
        Debug.Assert (0 <= value);
        m_partial = value;
      }
    }

    /// <summary>
    /// The default constructor is forbidden
    /// </summary>
    protected CycleDurationSummary ()
    { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="day"></param>
    /// <param name="shift"></param>
    /// <param name="workOrder"></param>
    /// <param name="line"></param>
    /// <param name="manufacturingOrder"></param>
    /// <param name="component"></param>
    /// <param name="operation"></param>
    /// <param name="offset"></param>
    public CycleDurationSummary (IMachine machine,
                                 DateTime day,
                                 IShift shift,
                                 IWorkOrder workOrder,
                                 ILine line,
                                 IManufacturingOrder manufacturingOrder,
                                 IComponent component,
                                 IOperation operation,
                                 int offset)
    {
      m_machine = machine;
      m_day = day;
      m_shift = shift;
      m_workOrder = workOrder;
      m_line = line;
      m_manufacturingOrder = manufacturingOrder;
      m_component = component;
      m_operation = operation;
      m_offset = offset;
    }
  }
}
