// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Business.Config;
using System.Threading;

namespace Lemoine.Analysis
{
  /// <summary>
  /// Description of ProductionAnalysis.
  /// </summary>
  internal sealed class ProductionAnalysis : ISingleAnalysis, Lemoine.Threading.IChecked
  {
    #region Members
    readonly IMachine m_machine;
    readonly TransactionLevel m_restrictedTransactionLevel;
    readonly Lemoine.Threading.IChecked m_caller;
    IProductionAnalysisStatus m_productionAnalysisStatus = null;
    #endregion // Members

    readonly ILog log = LogManager.GetLogger (typeof (ProductionAnalysis).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="machineActivityAnalysis">Not null (including its associated machine)</param>
    public ProductionAnalysis (MachineActivityAnalysis machineActivityAnalysis)
    {
      Debug.Assert (null != machineActivityAnalysis);
      Debug.Assert (null != machineActivityAnalysis.Machine);

      m_machine = machineActivityAnalysis.Machine;
      m_restrictedTransactionLevel = machineActivityAnalysis.RestrictedTransactionLevel;
      m_caller = machineActivityAnalysis;

      log = LogManager.GetLogger (string.Format ("{0}.{1}",
                                                this.GetType ().FullName,
                                                m_machine.Id));
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
      // - Initialize m_productionAnalysisStatus with the time of the latest analysis
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Analysis.ProductionAnalysisStatusInitialization",
                                                                               TransactionLevel.ReadCommitted)) {
          m_productionAnalysisStatus =
            ModelDAOHelper.DAOFactory.ProductionAnalysisStatusDAO
            .FindById (m_machine.Id);
        }
        if (null == m_productionAnalysisStatus) {
          m_productionAnalysisStatus = ModelDAOHelper.ModelFactory
            .CreateProductionAnalysisStatus (m_machine);
          m_productionAnalysisStatus.AnalysisDateTime = GetRoundedNow ();
          using (IDAOTransaction transaction = session.BeginTransaction ("Analysis.ProductionAnalysisStatusCreation",
                                                                         TransactionLevel.ReadCommitted)) { // Transaction: Initiate ProductionAnalysisStatus
            ModelDAOHelper.DAOFactory.ProductionAnalysisStatusDAO
              .MakePersistent (m_productionAnalysisStatus);
            transaction.Commit ();
          }
        }
      }
      Debug.Assert (null != m_productionAnalysisStatus);
      if (GetLogger ().IsDebugEnabled) {
        GetLogger ().DebugFormat ("Initialize: production analysis status {0} loaded for monitored machine {1}",
          m_productionAnalysisStatus, m_machine);
      }
    }

    /// <summary>
    /// ISingleAnalysis implementation
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="maxDateTime"></param>
    /// <param name="minTime"></param>
    /// <param name="numberOfItems">not used</param>
    public bool RunOnce (CancellationToken cancellationToken, DateTime maxDateTime, TimeSpan minTime, int? numberOfItems)
    {
      if (!AnalysisConfigHelper.OperationSlotProductionDuration) {
        if (GetLogger ().IsDebugEnabled) {
          GetLogger ().Debug ("RunOnce: return immediately because the option is not active");
        }
        return true;
      }

      // Note: m_productionAnalysisStatus is initialized in the Initialize method
      Debug.Assert (null != m_productionAnalysisStatus);

      DateTime lastProductionAnalysisStatusDateTime = m_productionAnalysisStatus.AnalysisDateTime;
      DateTime now = GetRoundedNow ();

      try {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          try {
            m_productionAnalysisStatus =
              ModelDAOHelper.DAOFactory.ProductionAnalysisStatusDAO
              .FindById (m_machine.Id);
          }
          catch (Exception ex) {
            GetLogger ().Fatal ("RunOnce: Refresh of ProductionAnalysisStatus failed", ex);
            throw;
          }
          if (null == m_productionAnalysisStatus) {
            GetLogger ().Fatal ("RunOnce: unexpected null production analysis status");
            throw new InvalidOperationException ("Unexpected null productionAnalysisStatus");
          }
          SetActive ();
          lastProductionAnalysisStatusDateTime = m_productionAnalysisStatus.AnalysisDateTime;

          UtcDateTimeRange range = new UtcDateTimeRange (m_productionAnalysisStatus.AnalysisDateTime,
                                                         now);
          if (range.IsEmpty ()) { // This should not happen very often
            GetLogger ().WarnFormat ("RunOnce: empty range => nothing to do");
            return true;
          }

          using (IDAOTransaction transaction = session.BeginTransaction ("Analysis.Production",
                                                                         m_restrictedTransactionLevel)) {
            if (m_restrictedTransactionLevel.Equals (TransactionLevel.ReadCommitted)) {
              ModelDAOHelper.DAOFactory.ProductionAnalysisStatusDAO.UpgradeLock (m_productionAnalysisStatus);
            }
            IList<IObservationStateSlot> observationStateSlots = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
              .FindOverlapsRange (m_machine, range);
            foreach (var observationStateSlot in observationStateSlots) {
              SetActive ();
              if (null == observationStateSlot.MachineObservationState) {
                GetLogger ().Warn ($"RunOnce: observation state slot {observationStateSlot} with no MachineObservationState => skip it");
                continue;
              }
              if (!observationStateSlot.Production.HasValue || !observationStateSlot.Production.Value) {
                if (GetLogger ().IsDebugEnabled) {
                  GetLogger ().Debug ($"RunOnce: skip {observationStateSlot} because it is not a production");
                }
                continue;
              }
              UtcDateTimeRange productionRange = new UtcDateTimeRange (observationStateSlot.DateTimeRange
                                                                       .Intersects (range));
              Debug.Assert (!productionRange.IsEmpty ());
              if (GetLogger ().IsDebugEnabled) {
                GetLogger ().DebugFormat ($"RunOnce: consider production range {productionRange}");
              }
              // Loop now on operation slots
              IList<IOperationSlot> operationSlots = ModelDAOHelper.DAOFactory.OperationSlotDAO
                .FindOverlapsRange (m_machine, productionRange);
              foreach (var operationSlot in operationSlots) {
                SetActive ();
                UtcDateTimeRange intersection = new UtcDateTimeRange (productionRange.Intersects (operationSlot.DateTimeRange));
                ProcessOperationSlot (operationSlot, intersection);
                if (maxDateTime < DateTime.UtcNow) {
                  if (GetLogger ().IsWarnEnabled) {
                    GetLogger ().Warn ($"RunOnce: interrupt after operationSlot {operationSlot.Id} since {maxDateTime} is reached, stop the analysis at {intersection.Upper}");
                  }
                  m_productionAnalysisStatus.AnalysisDateTime = UpperBound
                    .GetMinimum<DateTime> (intersection.Upper, now).Value;
                  ModelDAOHelper.DAOFactory.ProductionAnalysisStatusDAO.MakePersistent (m_productionAnalysisStatus);
                  transaction.Commit ();
                  return true;
                }
              }
              SetActive ();

              if (maxDateTime < DateTime.UtcNow) {
                if (GetLogger ().IsWarnEnabled) {
                  GetLogger ().Warn ($"RunOnce: interrupt since {maxDateTime} is reached, stop the analysis at {observationStateSlot.EndDateTime}");
                }
                m_productionAnalysisStatus.AnalysisDateTime = UpperBound
                  .GetMinimum<DateTime> (observationStateSlot.DateTimeRange.Upper, now).Value;
                ModelDAOHelper.DAOFactory.ProductionAnalysisStatusDAO.MakePersistent (m_productionAnalysisStatus);
                transaction.Commit ();
                return true;
              }
            }

            m_productionAnalysisStatus.AnalysisDateTime = now;
            ModelDAOHelper.DAOFactory.ProductionAnalysisStatusDAO.MakePersistent (m_productionAnalysisStatus);

            transaction.Commit ();
          } // end of transaction
        } // end of session
      } // try
      catch (OperationCanceledException ex) {
        GetLogger ().Error ("RunOnce: OperationCanceledException", ex);
        ReloadAfterException (ex, lastProductionAnalysisStatusDateTime);
        throw;
      }
      catch (Lemoine.Threading.AbortException ex) {
        GetLogger ().Error ("RunOnce: AbortException", ex);
        ReloadAfterException (ex, lastProductionAnalysisStatusDateTime);
        throw;
      }
      catch (Exception ex) {
        SetActive ();
        GetLogger ().Exception (ex, "RunOnce");
        ReloadAfterException (ex, lastProductionAnalysisStatusDateTime);
      }

      return true;
    }

    void ProcessOperationSlot (IOperationSlot operationSlot, UtcDateTimeRange intersection)
    {
      Debug.Assert (!intersection.IsEmpty ());
      Debug.Assert (intersection.Duration.HasValue);
      if (GetLogger ().IsDebugEnabled) {
        GetLogger ().Debug ($"ProcessOperationSlot: intersection={intersection}");
      }
      if (operationSlot.ProductionDuration.HasValue) {
        ((Lemoine.GDBPersistentClasses.OperationSlot)operationSlot).ProductionDuration =
          operationSlot.ProductionDuration.Value.Add (intersection.Duration.Value);
        ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent (operationSlot);
      }
      else { // !operationSlot.ProductionDuration.HasValue
        if (Bound.Compare<DateTime> (operationSlot.BeginDateTime,
                                     m_productionAnalysisStatus.AnalysisDateTime) < 0) {
          GetLogger ().Warn ($"ProcessOperationSlot:the previous production duration in {operationSlot.Id} is unknown => reconsolidate the production duration");
          var effectiveOperationSlot = ((Lemoine.GDBPersistentClasses.OperationSlot)operationSlot);
          effectiveOperationSlot.Caller = this;
          effectiveOperationSlot.ConsolidateProduction ();
          ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent (effectiveOperationSlot);
        }
        else {
          ((Lemoine.GDBPersistentClasses.OperationSlot)operationSlot).ProductionDuration =
            intersection.Duration.Value;
          ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent (operationSlot);
        }
      }
    }

    void ReloadAfterException (Exception ex, DateTime lastProductionAnalysisStatusDateTime)
    {
      // Reload m_productionAnalysisStatus that may have been updated
      try {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Analysis.ProductionAnalysisStatusReload")) {
            m_productionAnalysisStatus = ModelDAOHelper.DAOFactory.ProductionAnalysisStatusDAO
              .FindById (m_machine.Id); // Reload may return a Stale exception
          } // auto commit because read-only
        }
        if (null == m_productionAnalysisStatus) {
          GetLogger ().Fatal ("ReloadAfterException: null ProductionAnalysisStatus");
          Debug.Assert (null != m_productionAnalysisStatus);
          throw new Exception ("null ProductionAnalysisStatus");
        }
        else if (!lastProductionAnalysisStatusDateTime.Equals (m_productionAnalysisStatus.AnalysisDateTime)) {
          GetLogger ().FatalFormat ("ReloadAfterException: " +
             "Production Analysis DateTime changed from {0} to {1} " +
             "after the serialization failure, " +
             "reset lastProductionAnalysisStatusDateTime and continue",
             lastProductionAnalysisStatusDateTime, m_productionAnalysisStatus.AnalysisDateTime);
        }
      }
      catch (Exception ex1) {
        GetLogger ().Fatal ($"ReloadAfterException: re-loading productionAnalysisStatus after {ex} failed", ex1);
        throw new Exception ("Reload error of ProductionAnalysisStatus", ex);
      }
    }

    DateTime RoundUtcDateTime (DateTime dateTime)
    {
      DateTime dateValue = dateTime;
      if (dateValue.Kind == DateTimeKind.Local) {
        dateValue = dateValue.ToUniversalTime ();
      }
      return new DateTime (dateValue.Year, dateValue.Month, dateValue.Day,
                          dateValue.Hour, dateValue.Minute, dateValue.Second, DateTimeKind.Utc);
    }

    DateTime GetRoundedNow ()
    {
      return RoundUtcDateTime (DateTime.UtcNow);
    }

    #endregion // ISingleAnalysis implementation
  }
}
