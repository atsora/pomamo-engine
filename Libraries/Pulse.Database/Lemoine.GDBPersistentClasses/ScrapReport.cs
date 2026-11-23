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
      : base (operationSlot.Machine)
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
    public virtual int NbValid { get; set; }

    /// <summary>
    /// <see cref="IScrapReport"/>
    /// </summary>
    public virtual int NbSetup { get; set; }

    /// <summary>
    /// <see cref="IScrapReport"/>
    /// </summary>
    public virtual int NbScrap { get; set; }

    /// <summary>
    /// <see cref="IScrapReport"/>
    /// </summary>
    public virtual int NbFixable { get; set; }

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
      set {
        m_reasons = value;
      }
    }

    public override string ModificationType => "ScrapReport";

    /// <summary>
    /// Prefix of the analysis log message when the report does not apply any more
    /// </summary>
    static readonly string DISCARD_MESSAGE_PREFIX = "Discard:";

    /// <summary>
    /// Make the analysis of the scrap report: its values are added directly to its operation slot,
    /// and the values of the report it updates (if any, and if it was counted) are removed from its operation slot
    ///
    /// A report is counted in an operation slot if its full range is included in the slot
    /// and if its operation is the operation of the slot.
    /// The recompute of the operation slots (<see cref="OperationSlot.ConsolidateScrap"/>) considers only
    /// the completed reports, so that a report is not counted twice before its analysis is completed.
    /// The analysis updates only the status of the modification (and the analysis log)
    /// </summary>
    public override void MakeAnalysis ()
    {
      Debug.Assert (!IsMainModificationTransient ());
      Debug.Assert (!IsAnalysisCompleted ());

      Debug.Assert (null != m_operation);
      Debug.Assert (null != m_machine);

      // The updated report is not counted any more, if it was counted
      // Note: this report is not completed yet, so it is not considered as a report that updates m_reportUpdate
      if ((m_reportUpdate is not null)
        && ModelDAOHelper.DAOFactory.ScrapReportDAO.IsCompletedAndNotUpdated (m_reportUpdate)) {
        var previousOperationSlots = FindOperationSlots (m_reportUpdate);
        if ((1 == previousOperationSlots.Count) && previousOperationSlots[0].IsScrapReportApplicable (m_reportUpdate)) {
          previousOperationSlots[0].EnsureScrapConsolidated ();
          previousOperationSlots[0].AddScrapReport (m_reportUpdate, -1);
        }
      }

      var operationSlots = FindOperationSlots (this);
      if (0 == operationSlots.Count) {
        LogDiscard (LogLevel.WARN, "there is no operation slot in its range");
      }
      else if (1 < operationSlots.Count) {
        LogDiscard (LogLevel.ERROR, $"it overlaps {operationSlots.Count} operation slots, while it must apply to a single operation slot");
      }
      else {
        var operationSlot = operationSlots[0];
        if (operationSlot.IsScrapReportApplicable (this)) {
          // Note: this report is not completed yet, so a consolidation does not count it
          operationSlot.EnsureScrapConsolidated ();
          operationSlot.AddScrapReport (this, +1);
        }
        else if (!operationSlot.IsScrapReportIn (this)) {
          LogDiscard (LogLevel.WARN, $"its range {this.DateTimeRange} is not included in the operation slot {operationSlot.DateTimeRange}");
        }
        else {
          LogDiscard (LogLevel.WARN, $"the operation of the period is now {((Lemoine.Collections.IDataWithId)operationSlot.Operation)?.Id.ToString () ?? "none"} instead of {((Lemoine.Collections.IDataWithId)m_operation).Id}");
        }
      }

      MarkAsCompleted ($"Cache/ClearDomainByMachine/ScrapReport/{this.Machine.Id}?Broadcast=true");
    }

    /// <summary>
    /// Find the operation slots that overlap a scrap report
    /// </summary>
    /// <param name="scrapReport">not null</param>
    /// <returns></returns>
    static IList<OperationSlot> FindOperationSlots (IScrapReport scrapReport) =>
      ModelDAOHelper.DAOFactory.OperationSlotDAO
        .FindOverlapsRange (scrapReport.Machine, scrapReport.DateTimeRange)
        .OfType<OperationSlot> ()
        .ToList ();

    /// <summary>
    /// The scrap report does not apply: add it to the analysis log, once
    ///
    /// Note: the report is not modified, the analysis writes only in the analysis log.
    /// The report is not counted while it is not included in a single operation slot of its operation
    /// </summary>
    /// <param name="level">log level</param>
    /// <param name="reason">reason of the discard</param>
    internal protected virtual void LogDiscard (LogLevel level, string reason)
    {
      var alreadyLogged = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineModificationLog> ()
        .Add (NHibernate.Criterion.Restrictions.Eq ("Machine.Id", this.Machine.Id))
        .Add (NHibernate.Criterion.Restrictions.Eq ("Modification.Id", this.Id))
        .Add (NHibernate.Criterion.Restrictions.Like ("Message", DISCARD_MESSAGE_PREFIX, NHibernate.Criterion.MatchMode.Start))
        .SetProjection (NHibernate.Criterion.Projections.RowCount ())
        .UniqueResult<int> ();
      if (0 < alreadyLogged) {
        if (log.IsDebugEnabled) {
          log.Debug ($"LogDiscard: already logged, {reason}");
        }
        return;
      }
      var message = $"{DISCARD_MESSAGE_PREFIX} the scrap report does not apply, {reason}";
      if (LogLevel.ERROR == level) {
        log.Error (message);
      }
      else {
        log.Warn (message);
      }
      AddAnalysisLog (level, message);
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
      log.Fatal ("Apply: not implemented/supported");
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
