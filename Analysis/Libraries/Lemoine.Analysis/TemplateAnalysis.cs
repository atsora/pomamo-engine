// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using Lemoine.Core.ExceptionManagement;
using Lemoine.GDBPersistentClasses;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Threading;
using Lemoine.Core.Log;
using System.Linq;
using Lemoine.Business.Config;
using System.Threading;

namespace Lemoine.Analysis
{
  /// <summary>
  /// Generic template Analysis
  /// 
  /// T: type of the slot with the template
  /// </summary>
  internal abstract class TemplateAnalysis<T, I> : IChecked
    where T : I, IWithTemplate
  {
    /// <summary>
    /// Number of attempt to complete a transaction in case of a serialization failure
    /// </summary>
    readonly string NB_ATTEMPT_SERIALIZATION_FAILURE_KEY = "Analysis.Activity.NbAttemptSerializationFailure";
    readonly int NB_ATTEMPT_SERIALIZATION_FAILURE_DEFAULT = 2;

    #region Members
    readonly IChecked m_checkedThread = null;
    TransactionLevel m_restrictedTransactionLevel = TransactionLevel.Serializable;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Max number of slots to process by iteration
    /// </summary>
    protected abstract int MaxSlotsByIteration { get; }

    /// <summary>
    /// Max analysis time range to process when all the templates are processed
    /// </summary>
    protected abstract TimeSpan MaxAnalysisTimeRange { get; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected TemplateAnalysis ()
    {
      Debug.Assert (false);
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="restrictedTransactionLevel"></param>
    /// <param name="checkedThread"></param>
    protected TemplateAnalysis (TransactionLevel restrictedTransactionLevel, IChecked checkedThread)
    {
      m_restrictedTransactionLevel = restrictedTransactionLevel;
      m_checkedThread = checkedThread;
    }
    #endregion // Constructors

    /// <summary>
    /// Get a logger
    /// </summary>
    /// <returns></returns>
    protected abstract ILog GetLogger ();

    /// <summary>
    /// SetActive method
    /// </summary>
    public void SetActive ()
    {
      if (null != m_checkedThread) {
        m_checkedThread.SetActive ();
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Threading.IChecked" />
    /// </summary>
    public void PauseCheck ()
    {
      if (null != m_checkedThread) {
        m_checkedThread.PauseCheck ();
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Threading.IChecked" />
    /// </summary>
    public void ResumeCheck ()
    {
      if (null != m_checkedThread) {
        m_checkedThread.ResumeCheck ();
      }
    }

    /// <summary>
    /// Process the data
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="presentUtcBegin"></param>
    /// <param name="until"></param>
    /// <param name="maxAnalysisDateTime"></param>
    /// <returns>completed if true, else unknown</returns>
    public bool Run (CancellationToken cancellationToken, DateTime presentUtcBegin, DateTime until, DateTime maxAnalysisDateTime)
    {
      int nbAttemptSerializationFailure = (int)Lemoine.Info.ConfigSet.LoadAndGet (NB_ATTEMPT_SERIALIZATION_FAILURE_KEY,
                                                                                  NB_ATTEMPT_SERIALIZATION_FAILURE_DEFAULT);
      for (int attempt = 0; attempt < nbAttemptSerializationFailure; attempt++) { // Limit the number of attempts in case of serializable failure
        SetActive ();
        GetLogger ().DebugFormat ("Run: attempt={0}", attempt);
        try {
          // Present first
          while (true) {
            GetLogger ().DebugFormat ("Run: new iteration of RunInRange");
            if (cancellationToken.IsCancellationRequested) {
              GetLogger ().Warn ($"Run: cancellation requested, exit");
              return false;
            }
            if (RunInRange (cancellationToken, new UtcDateTimeRange (presentUtcBegin, until), this.MaxSlotsByIteration, maxAnalysisDateTime) < this.MaxSlotsByIteration) {
              GetLogger ().DebugFormat ("Run: " +
                                        "all the present slots until {0} were processed, exit the first loop",
                                        until);
              break;
            }
            SetActive ();
            if (cancellationToken.IsCancellationRequested) {
              GetLogger ().Warn ($"Run: cancellation requested, exit");
              return false;
            }
            if (maxAnalysisDateTime <= DateTime.UtcNow) {
              GetLogger ().Warn ($"Run: the process time reached {maxAnalysisDateTime}, exit");
              return false;
            }
          } // Present loop

          if (maxAnalysisDateTime <= DateTime.UtcNow) {
            GetLogger ().Warn ($"Run: the process was already {maxAnalysisDateTime} long, exit");
            return false;
          }
          SetActive ();

          // All slots then
          while (true) {
            GetLogger ().DebugFormat ("Run: new iteration of RunAll");
            if (RunAll (cancellationToken, new UtcDateTimeRange (AnalysisConfigHelper.MinTemplateProcessDateTime, until),
                        this.MaxSlotsByIteration, this.MaxAnalysisTimeRange, maxAnalysisDateTime)) {
              GetLogger ().DebugFormat ("Run: " +
                                        "all the slots until {0} were processed, exit",
                                        until);
              return true;
            }
            SetActive ();
            if (cancellationToken.IsCancellationRequested) {
              GetLogger ().Warn ($"Run: cancellation requested, exit");
              return false;
            }
            if (maxAnalysisDateTime <= DateTime.UtcNow) {
              GetLogger ().Warn ($"Run: the process reached {maxAnalysisDateTime}, exit");
              return false;
            }
          } // all slots loop
        }
        catch (OutOfMemoryException ex) {
          GetLogger ().Error ("Run: OutOfMemoryException, give up");
          throw new OutOfMemoryException ("OutOfMemoryException raised by RunAnalysis", ex);
        }
        catch (Lemoine.GDBPersistentClasses.InterruptedAnalysis) {
          GetLogger ().Warn ("Run: analysis was interrupted because it was probably too long");
          SetActive ();
          return false;
        }
        catch (Exception ex) {
          if (ExceptionTest.IsStale (ex, GetLogger ())) {
            GetLogger ().Info ("Run: Stale exception => try again", ex);
          }
          else if (ExceptionTest.IsTransactionSerializationFailure (ex, GetLogger ())) {
            GetLogger ().Info ("Run: transaction serialization failure => try again", ex);
          }
          else if (ExceptionTest.IsTemporaryWithDelay (ex, GetLogger ())) {
            GetLogger ().Warn ("Run: temporary failure with delay => try again", ex);
          }
          else if (ExceptionTest.IsTemporary (ex, GetLogger ())) {
            GetLogger ().Warn ("Run: temporary failure => try again", ex);
          }
          else {
            GetLogger ().Error ("Run: unexpected exception", ex);
            throw;
          }
          if (maxAnalysisDateTime <= DateTime.UtcNow) {
            GetLogger ().Warn ($"Run: the process reached {maxAnalysisDateTime} after the exception catch, exit");
            SetActive ();
            return false;
          }
        }
      }

      GetLogger ().WarnFormat ("Run: " +
                               "the maximum number of attempt was reached " +
                               "=> return false");
      SetActive ();
      return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="range">Upper is not null</param>
    /// <param name="limit"></param>
    /// <param name="maxAnalysisDateTime"></param>
    /// <returns>number of slots that were processed</returns>
    int RunInRange (CancellationToken cancellationToken, UtcDateTimeRange range, int limit, DateTime maxAnalysisDateTime)
    {
      Debug.Assert (range.Upper.HasValue);

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        IEnumerable<I> impactedSlots;
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Analysis.TemplateAnalysis.RunInRange.Get")) {
          impactedSlots = GetNotProcessedTemplate (range, limit);
        }
        int count = 0;
        foreach (I impactedSlot in impactedSlots) {
          SetActive ();
          using (IDAOTransaction transaction = session.BeginTransaction ("Analysis.TemplateAnalysis.RunInRange.Process",
                                                                         m_restrictedTransactionLevel)) {
            var slot = ReloadSlot (impactedSlot);
            if (null == slot) {
              GetLogger ().ErrorFormat ("RunInRange: " +
                                        "ReloadSlot returned null (slot was deleted since) " +
                                        "=> return MaxInt to reload the impacted slots");
              return int.MaxValue;
            }
            if (false == ProcessTemplateForSlot (cancellationToken, slot, range, maxAnalysisDateTime)) {
              GetLogger ().WarnFormat ("RunInRange: " +
                                       "ProcessTemplateForSlot is interrupt because maxAnalysisDateTime {0} was reached",
                                       maxAnalysisDateTime);
              transaction.Commit ();
              throw new Lemoine.GDBPersistentClasses.InterruptedAnalysis ("TemplateAnalysis");
            }
            transaction.Commit ();
          }
          ++count; // one more slot was completed
          SetActive ();
          if (maxAnalysisDateTime <= DateTime.UtcNow) {
            GetLogger ().WarnFormat ("RunInRange: " +
                                     "interrupt the analysis because now is after {0} after procesing slot {1}",
                                     maxAnalysisDateTime, impactedSlot);
            throw new Lemoine.GDBPersistentClasses.InterruptedAnalysis ("TemplateAnalysis");
          }
        } // foreach

        return count;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="applicableRange">Upper is not null</param>
    /// <param name="limit"></param>
    /// <param name="maxAnalysisTimeRange"></param>
    /// <param name="maxAnalysisDateTime"></param>
    /// <returns>completed</returns>
    bool RunAll (CancellationToken cancellationToken, UtcDateTimeRange applicableRange, int limit, TimeSpan maxAnalysisTimeRange, DateTime maxAnalysisDateTime)
    {
      Debug.Assert (applicableRange.Upper.HasValue);

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        IEnumerable<I> impactedSlots;
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Analysis.TemplateAnalysis.RunAll.Get")) {
          impactedSlots = GetNotProcessedTemplate (applicableRange, limit);
        }

        GetLogger ().DebugFormat ("RunAll: got {0} slots VS limit={1}", impactedSlots.Count (), limit);

        if (cancellationToken.IsCancellationRequested) {
          GetLogger ().Warn ("RunAll: cancellation requested");
          return false;
        }

        UtcDateTimeRange correctedRange = applicableRange;
        int count = 0;
        foreach (I impactedSlot in impactedSlots) {
          SetActive ();
          ++count;
          GetLogger ().DebugFormat ("RunAll: analyze slot #{0}", count);

          if (cancellationToken.IsCancellationRequested) {
            GetLogger ().Warn ("RunAll: cancellation requested");
            return false;
          }

          using (IDAOTransaction transaction = session.BeginTransaction ("Analysis.TemplateAnalysis.RunAll.Process",
                                                                         m_restrictedTransactionLevel)) {
            var slot = ReloadSlot (impactedSlot);
            if (null == slot) {
              GetLogger ().ErrorFormat ("RunAll: " +
                                        "slot is null after reload " +
                                        "=> return false to get again the list of impacted slots");
              transaction.Commit ();
              return false;
            }
            Debug.Assert (slot.DateTimeRange.Overlaps (applicableRange));

            if (1 == count) { // First
              if (slot.DateTimeRange.Lower.HasValue
                  && slot.DateTimeRange.Lower.Value.Add (maxAnalysisTimeRange) < correctedRange.Upper.Value) {
                correctedRange = new UtcDateTimeRange (applicableRange.Lower,
                                                       slot.DateTimeRange.Lower.Value.Add (maxAnalysisTimeRange));
                GetLogger ().InfoFormat ("RunAll: " +
                                         "process only until {0} instead of {1} " +
                                         "because of the max analysis time range {2}",
                                         correctedRange.Upper, applicableRange.Upper, maxAnalysisTimeRange);
              }
            }

            if (!correctedRange.Overlaps (slot.DateTimeRange)) {
              GetLogger ().WarnFormat ("RunAll: " +
                                       "slot is after the corrected end process date/time {0} " +
                                       "because of the max analysis time range of {1} " +
                                       "=> exit returning false",
                                       correctedRange.Upper, maxAnalysisTimeRange);
              transaction.Commit ();
              return false;
            }

            bool partiallyApplied = (correctedRange.Upper < applicableRange.Upper)
              && (correctedRange.Upper < slot.DateTimeRange.Upper);
            SetActive ();
            if (false == ProcessTemplateForSlot (cancellationToken, slot, correctedRange, maxAnalysisDateTime)) {
              GetLogger ().WarnFormat ("RunAll: " +
                                       "ProcessTemplate was interrupted because maxAnalysisDateTime {0} was reached",
                                       maxAnalysisDateTime);
              transaction.Commit ();
              throw new Lemoine.GDBPersistentClasses.InterruptedAnalysis ("TemplateAnalysis");
            }
            if (partiallyApplied) {
              GetLogger ().WarnFormat ("RunAll: " +
                                       "interrupt the process after processing {0} until {1} " +
                                       "because of the max analysis time range {2} " +
                                       "=> exit returning false",
                                       slot, correctedRange.Upper, maxAnalysisTimeRange);
              ModelDAOHelper.DAOFactory.Flush (); // For the unit tests
              transaction.Commit ();
              return false;
            }

            SetActive ();
            if (maxAnalysisDateTime <= DateTime.UtcNow) {
              GetLogger ().WarnFormat ("RunAll: " +
                                       "interrupt the analysis because now is after {0} after procesing slot {1}",
                                       maxAnalysisDateTime, slot);
              transaction.Commit ();
              throw new Lemoine.GDBPersistentClasses.InterruptedAnalysis ("TemplateAnalysis");
            }

            transaction.Commit ();
          } // transaction

        } // loop

        return (count < limit); // In this case there is no slot to process any more
      } // session
    }

    /// <summary>
    /// Process the template for the specified slot
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="slot"></param>
    /// <param name="range"></param>
    /// <param name="maxAnalysisDateTime"></param>
    /// <returns></returns>
    protected virtual bool ProcessTemplateForSlot (CancellationToken cancellationToken, IWithTemplate slot, UtcDateTimeRange range, DateTime? maxAnalysisDateTime)
    {
      return slot.ProcessTemplate (cancellationToken, range, null, true, m_checkedThread, maxAnalysisDateTime);
    }

    /// <summary>
    /// Get the analysis rows in the specified range which template has not been processed yet
    /// </summary>
    /// <param name="range">range.Upper is not null</param>
    /// <param name="limit"></param>
    /// <returns></returns>
    protected abstract IEnumerable<I> GetNotProcessedTemplate (UtcDateTimeRange range, int limit);

    /// <summary>
    /// Reload a slot inside a new transaction
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    protected abstract IWithTemplate ReloadSlot (I slot);
  }
}
