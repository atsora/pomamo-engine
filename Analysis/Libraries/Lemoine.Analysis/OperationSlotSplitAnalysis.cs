// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Info;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Business.Config;
using System.Threading;

namespace Lemoine.Analysis
{
  /// <summary>
  /// Description of OperationSlotSplitAnalysis.
  /// </summary>
  internal sealed class OperationSlotSplitAnalysis : ISingleAnalysis, Lemoine.Threading.IChecked
  {
    /// <summary>
    /// Time from now where the operation slot is split
    /// </summary>
    static readonly string OPERATION_SLOT_SPLIT_PERIOD_KEY = "Analysis.OperationSlotSplit.Period";
    static readonly TimeSpan OPERATION_SLOT_SPLIT_PERIOD_DEFAULT = TimeSpan.FromMinutes (2);

    /// <summary>
    /// Machine modification priority for the shift association
    /// </summary>
    static readonly string SHIFT_ASSOCIATION_PRIORITY_KEY = "Analysis.OperationSlotSplit.ShiftAssociation.Priority";

    #region Members
    readonly IMachine m_machine;
    readonly TransactionLevel m_restrictedTransactionLevel;
    readonly Lemoine.Threading.IChecked m_caller;
    #endregion // Members

    readonly ILog log = LogManager.GetLogger (typeof (OperationSlotSplitAnalysis).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="machineActivityAnalysis">Not null (including its associated machine)</param>
    public OperationSlotSplitAnalysis (MachineActivityAnalysis machineActivityAnalysis)
    {
      Debug.Assert (null != machineActivityAnalysis);
      Debug.Assert (null != machineActivityAnalysis.Machine);

      m_machine = machineActivityAnalysis.Machine;
      m_restrictedTransactionLevel = machineActivityAnalysis.RestrictedTransactionLevel;
      m_caller = machineActivityAnalysis;

      log = LogManager.GetLogger (string.Format ("{0}.{1}",
                                                this.GetType ().FullName,
                                                machineActivityAnalysis.Machine.Id));
    }
    #endregion // Constructors

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    ILog GetLogger ()
    {
      return log;
    }

    #region IChecked implementation
    /// <summary>
    /// Lemoine.Threading.IChecked implementation
    /// </summary>
    public void SetActive ()
    {
      if (null != m_caller) {
        m_caller.SetActive ();
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Threading.IChecked" />
    /// </summary>
    public void PauseCheck ()
    {
      if (null != m_caller) {
        m_caller.PauseCheck ();
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Threading.IChecked" />
    /// </summary>
    public void ResumeCheck ()
    {
      if (null != m_caller) {
        m_caller.ResumeCheck ();
      }
    }
    #endregion // IChecked implementation

    #region ISingleAnalysis implementation
    /// <summary>
    /// ISingleAnalysis implementation
    /// </summary>
    public void Initialize ()
    {
      // Do nothing
    }

    /// <summary>
    /// ISingleAnalysis implementation
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="dateTime"></param>
    /// <param name="minTime"></param>
    /// <param name="numberOfItems">Here number of seconds for the period to process. If null consider a configuration key</param>
    public bool RunOnce (CancellationToken cancellationToken, DateTime dateTime, TimeSpan minTime, int? numberOfItems = null)
    {
      if (!AnalysisConfigHelper.OperationSlotSplitOption.IsActive ()) {
        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().Debug ("RunOnce: nothing to do because the options to split the operation slots are not set");
        }
        return true;
      }

      TimeSpan splitPeriod = numberOfItems.HasValue
        ? TimeSpan.FromSeconds (numberOfItems.Value)
        : ConfigSet.LoadAndGet<TimeSpan> (OPERATION_SLOT_SPLIT_PERIOD_KEY,
                                          OPERATION_SLOT_SPLIT_PERIOD_DEFAULT);
      var minProcessDateTime = DateTime.UtcNow.Add (splitPeriod);
      var priority = Lemoine.Info.ConfigSet
        .LoadAndGet (SHIFT_ASSOCIATION_PRIORITY_KEY, AnalysisConfigHelper.AutoModificationPriority);

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        IOperationSlotSplit operationSlotSplit;
        // - First transaction: check if the operation slot split period really needs to be extended
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Analysis.OperationSlotSplit.MachineStatus")) {
          operationSlotSplit = ModelDAOHelper.DAOFactory.OperationSlotSplitDAO
            .FindById (m_machine.Id);
          SetActive ();
          if ((null != operationSlotSplit) && (minProcessDateTime <= operationSlotSplit.End)) {
            if (GetLogger ().IsDebugEnabled) {
              GetLogger ().Debug ($"RunOnce: nothing to do because min process time is {minProcessDateTime} while operation slot split end is {operationSlotSplit.End}");
            }
            return true;
          }
        }
        SetActive ();

        if (null == operationSlotSplit) {
          using (IDAOTransaction transaction = session.BeginTransaction ("Analysis.OperationSlotSplit.CreateMachineStatus", m_restrictedTransactionLevel)) {
            operationSlotSplit = ModelDAOHelper.ModelFactory.CreateOperationSlotSplit (m_machine);
            ModelDAOHelper.DAOFactory.OperationSlotSplitDAO.MakePersistent (operationSlotSplit);
            transaction.Commit ();
          }
        }
        SetActive ();

        // - Second transaction: extend the period if required
        using (IDAOTransaction transaction = session.BeginTransaction ("Analysis.OperationSlotSplit.Extend", m_restrictedTransactionLevel)) {
          if (m_restrictedTransactionLevel.Equals (TransactionLevel.ReadCommitted)) {
            ModelDAOHelper.DAOFactory.OperationSlotSplitDAO.UpgradeLock (operationSlotSplit);
          }
          if (AnalysisConfigHelper.OperationSlotSplitOption.Equals (OperationSlotSplitOption.ByDay)) {
            var daySlots = ModelDAOHelper.DAOFactory.DaySlotDAO
              .FindProcessedInRange (new UtcDateTimeRange (operationSlotSplit.End, minProcessDateTime));
            foreach (var daySlot in daySlots) {
              SetActive ();
              Debug.Assert (Bound.Compare<DateTime> (operationSlotSplit.End, daySlot.EndDateTime) < 0);
              var association =
                new Lemoine.GDBPersistentClasses.ShiftMachineAssociation (m_machine, daySlot.Day.Value, null, daySlot.DateTimeRange);
              association.Auto = true;
              association.Priority = priority;
              (new Lemoine.GDBPersistentClasses.ShiftMachineAssociationDAO ()).MakePersistent (association);

              operationSlotSplit.End = daySlot.EndDateTime.Value;
              ModelDAOHelper.DAOFactory.OperationSlotSplitDAO.MakePersistent (operationSlotSplit);
            }
          }
          else if (AnalysisConfigHelper.OperationSlotSplitOption.HasFlag (OperationSlotSplitOption.ByGlobalShift)) {
            var shiftSlots = ModelDAOHelper.DAOFactory.ShiftSlotDAO
              .FindOverlapsRange (new UtcDateTimeRange (operationSlotSplit.End, minProcessDateTime));
            foreach (var shiftSlot in shiftSlots) {
              SetActive ();
              Debug.Assert (Bound.Compare<DateTime> (operationSlotSplit.End, shiftSlot.EndDateTime) < 0);
              if (!shiftSlot.TemplateProcessed || !shiftSlot.Day.HasValue) {
                if (GetLogger ().IsDebugEnabled) {
                  GetLogger ().Debug ($"RunOnce: shift slot {shiftSlot} is not processed yet, do not go further now until it is processed");
                }
                break;
              }
              Debug.Assert (shiftSlot.EndDateTime.HasValue);
              var association =
                new Lemoine.GDBPersistentClasses.ShiftMachineAssociation (m_machine, shiftSlot.Day.Value, shiftSlot.Shift, shiftSlot.DateTimeRange);
              association.Auto = true;
              association.Priority = priority;
              (new Lemoine.GDBPersistentClasses.ShiftMachineAssociationDAO ()).MakePersistent (association);

              operationSlotSplit.End = shiftSlot.EndDateTime.Value;
              ModelDAOHelper.DAOFactory.OperationSlotSplitDAO.MakePersistent (operationSlotSplit);
            }
          }
          else if (AnalysisConfigHelper.OperationSlotSplitOption.HasFlag (OperationSlotSplitOption.ByMachineShift)) {
            var range = new UtcDateTimeRange (operationSlotSplit.End, minProcessDateTime);
            var firstProcessedObservationStateSlots = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
              .FindOverlapsRange (m_machine, range)
              .TakeWhile (observationStateSlot => (null != observationStateSlot.MachineObservationState));
            if (firstProcessedObservationStateSlots.Any ()) {
              var daySlots = ModelDAOHelper.DAOFactory.DaySlotDAO
                .FindOverlapsRange (range)
                .TakeWhile (daySlot => (null != daySlot.Day));
              var slots = Lemoine.GDBPersistentClasses.SlotWithDayShift
                .Combine (firstProcessedObservationStateSlots.Cast<ISlotWithDayShift> (),
                          daySlots.Cast<ISlotWithDayShift> ());
              foreach (var slot in slots) {
                SetActive ();
                Debug.Assert (Bound.Compare<DateTime> (operationSlotSplit.End, slot.EndDateTime) < 0);
                Debug.Assert (slot.Day.HasValue);
                Debug.Assert (slot.EndDateTime.HasValue);
                var association =
                  new Lemoine.GDBPersistentClasses.ShiftMachineAssociation (m_machine, slot.Day.Value, slot.Shift, slot.DateTimeRange);
                association.Auto = true;
                association.Priority = priority;
                (new Lemoine.GDBPersistentClasses.ShiftMachineAssociationDAO ()).MakePersistent (association);

                operationSlotSplit.End = slot.EndDateTime.Value;
                ModelDAOHelper.DAOFactory.OperationSlotSplitDAO.MakePersistent (operationSlotSplit);
              }
            } // firstProcessedObservationStateSlots.Any ()
          }
          else {
            Debug.Assert (false);
            log.Fatal ($"RunOnce: operation slot split is active but option {AnalysisConfigHelper.OperationSlotSplitOption} is not implemented");
            throw new NotImplementedException ();
          }
          transaction.Commit ();
        }
      }

      return true;
    }
    #endregion // ISingleAnalysis implementation
  }
}
