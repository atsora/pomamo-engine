// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate.Mapping;
using Pulse.Extensions.Database;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table ScrapReasonReport
  /// </summary>
  [Serializable]
  public class ScrapReasonReport : IScrapReasonReport
  {
    int m_id = 0;
    IMachine m_machine;

    /// <summary>
    /// Default constructor
    /// </summary>
    internal protected ScrapReasonReport () { }

    /// <summary>
    /// Constructor
    /// </summary>
    internal protected ScrapReasonReport (IScrapReport scrapReport, INonConformanceReason reason, int quantity)
    {
      Debug.Assert (null != scrapReport);
      Debug.Assert (null != reason);

      m_machine = scrapReport.Machine;
      this.ScrapReport = scrapReport;
      this.NonConformanceReason = reason;
      this.Quantity = quantity;
    }

    /// <summary>
    /// MachineStateTemplate ID
    /// </summary>
    [XmlAttribute ("Id")]
    public virtual int Id
    {
      get { return this.m_id; }
    }

    /// <summary>
    /// <see cref="IScrapReasonReport"/>
    /// </summary>
    public virtual IMachine Machine => m_machine;

    /// <summary>
    /// <see cref="IScrapReasonReport"/>
    /// </summary>
    public virtual IScrapReport ScrapReport { get; protected set; }

    /// <summary>
    /// <see cref="IScrapReasonReport"/>
    /// </summary>
    public virtual INonConformanceReason NonConformanceReason { get; protected set; }

    /// <summary>
    /// <see cref="IScrapReasonReport"/>
    /// </summary>
    public virtual int Quantity { get; protected set; }
  }

  /// <summary>
  /// Persistent class of table ScrapReport
  /// </summary>
  [Serializable]
  public class ScrapReport : MachineModification, IScrapReport
  {
    DateTime? m_day;
    IShift m_shift;
    IOperation m_operation;
    IComponent m_component;
    IWorkOrder m_workOrder;
    IManufacturingOrder m_manufacturingOrder;
    IScrapReport m_reportUpdate = null;
    ICollection<IScrapReasonReport> m_reasons =
      new List<IScrapReasonReport> ();

    /// <summary>
    /// Default constructor
    /// </summary>
    internal protected ScrapReport ()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    internal protected ScrapReport (IOperationSlot operationSlot, UtcDateTimeRange dateTimeRange)
    {
      Debug.Assert (null != operationSlot);
      Debug.Assert (null != operationSlot.Operation);

      m_day = operationSlot.Day;
      m_shift = operationSlot.Shift;
      m_operation = operationSlot.Operation;
      m_component = operationSlot.Component;
      m_workOrder = operationSlot.WorkOrder;
      m_manufacturingOrder = operationSlot.ManufacturingOrder;
      this.DateTimeRange = dateTimeRange;
    }

    /// <summary>
    /// <see cref="IScrapReport"/>
    /// </summary>
    public virtual UtcDateTimeRange DateTimeRange { get; set; }

    /// <summary>
    /// <see cref="IScrapReport"/>
    /// </summary>
    public virtual DateTime? Day => m_day;

    /// <summary>
    /// <see cref="IScrapReport"/>
    /// </summary>
    public virtual IShift Shift => m_shift;

    /// <summary>
    /// <see cref="IScrapReport"/>
    /// </summary>
    public virtual IOperation Operation => m_operation;

    /// <summary>
    /// <see cref="IScrapReport"/>
    /// </summary>
    public virtual IComponent Component => m_component;

    /// <summary>
    /// <see cref="IScrapReport"/>
    /// </summary>
    public virtual IWorkOrder WorkOrder => m_workOrder;

    /// <summary>
    /// <see cref="IScrapReport"/>
    /// </summary>
    public virtual IManufacturingOrder ManufacturingOrder => m_manufacturingOrder;

    /// <summary>
    /// <see cref="IScrapReport"/>
    /// </summary>
    public virtual int NbCycles { get; set; }

    /// <summary>
    /// <see cref="IScrapReport"/>
    /// </summary>
    public virtual int NbParts { get; set; }

    /// <summary>
    /// <see cref="IScrapReport"/>
    /// </summary>
    public virtual string Details { get; set; }

    /// <summary>
    /// <see cref="IScrapReport"/>
    /// </summary>
    public virtual IScrapReport ReportUpdate
    {
      get => m_reportUpdate;
      set { m_reportUpdate = value; }
    }

    /// <summary>
    /// <see cref="IScrapReport"/>
    /// </summary>
    public virtual ICollection<IScrapReasonReport> Reasons
    {
      get {
        if (m_reasons is null) {
          m_reasons = new List<IScrapReasonReport> ();
        }
        return m_reasons;
      }
    }

    public override string ModificationType => "ScrapReport";

    public override void MakeAnalysis ()
    {
      Debug.Assert (!IsMainModificationTransient ());
      Debug.Assert (!IsAnalysisCompleted ());

      Debug.Assert (null != m_operation);
      Debug.Assert (null != m_machine);

      var monitoredMachine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (this.Machine.Id);

      // TODO: ... update number of parts in operation slot
      // TODO: scrap in operation slot
      throw new NotImplementedException ();

      if (null == monitoredMachine) {
        string message = $"MakeAnalysis: there is no monitored machine with id {m_machine.Id}";
        log.Error ($"MakeAnalysis: {message}");
        AddAnalysisLog (LogLevel.ERROR, message);
        MarkAsError ();
        return;
      }

    }

    /// <summary>
    /// Apply the modification while keeping it transient
    /// 
    /// It should be never called, because there is no transient NonConformanceReport to process.
    /// Use a persistent entity instead and MakeAnalysis.
    /// </summary>
    public override void Apply ()
    {
      Debug.Assert (false);
      log.FatalFormat ("Apply: not implemented/supported");
      throw new NotImplementedException ();
    }

    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public override void Unproxy ()
    {
      base.Unproxy ();
      NHibernateHelper.Unproxy<IShift> (ref m_shift);
      NHibernateHelper.Unproxy<IOperation> (ref m_operation);
      NHibernateHelper.Unproxy<IComponent> (ref m_component);
      NHibernateHelper.Unproxy<IWorkOrder> (ref m_workOrder);
      NHibernateHelper.Unproxy<IManufacturingOrder> (ref m_manufacturingOrder);
      NHibernateHelper.Unproxy<IScrapReport> (ref m_reportUpdate);
    }
  }
}
