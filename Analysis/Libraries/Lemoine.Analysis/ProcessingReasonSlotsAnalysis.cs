// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Lemoine.Core.Log;
using Lemoine.Threading;
using Lemoine.Model;
using Lemoine.Core.ExceptionManagement;
using Lemoine.ModelDAO;
using Lemoine.Database.Persistent;
using Pulse.Extensions.Database;

namespace Lemoine.Analysis
{
  /// <summary>
  /// Analyze the Processing ReasonSlots
  /// </summary>
  public sealed class ProcessingReasonSlotsAnalysis
    : ThreadClass
    , IThreadClass
    , IChecked
  {
    static readonly string STEP_NUMBER_KEY = "Analysis.ProcessingReasonSlots.StepNumber";
    static readonly int STEP_NUMBER_DEFAULT = 50;

    static readonly string MAX_LOOP_NUMBER_KEY = "Analysis.ProcessingReasonSlots.MaxLoopNumber";
    static readonly int MAX_LOOP_NUMBER_DEFAULT = 50;

    static readonly string TEMPORARY_WITH_DELAY_EXCEPTION_SLEEP_TIME_KEY = "Analysis.ProcessingReasonSlots.TemporaryWithDelayException.Sleep";
    static readonly TimeSpan TEMPORARY_WITH_DELAY_EXCEPTION_SLEEP_TIME_DEFAULT = TimeSpan.FromSeconds (2);

    readonly ILog log = LogManager.GetLogger (typeof (ProcessingReasonSlotsAnalysis).FullName);

    readonly IMonitoredMachine m_machine;
    readonly IEnumerable<IReasonExtension> m_reasonExtensions;
    readonly IChecked m_checkedParent;

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="checkedParent"></param>
    public ProcessingReasonSlotsAnalysis (IMonitoredMachine machine, IChecked checkedParent)
    {
      Debug.Assert (null != machine);

      m_machine = machine;
      log = LogManager.GetLogger ($"{this.GetType ().FullName}.{machine.Id}");
      m_checkedParent = checkedParent;

      if (!Lemoine.Extensions.ExtensionManager.IsActive ()) {
        log.WarnFormat ("ProcessingReasonSlotsAnalysis: the extensions are not active");
      }
      SetActive ();
      {
        var request = new Lemoine.Business.Extension
          .MonitoredMachineExtensions<IReasonExtension> (machine,
          (ext, m) => ext.Initialize (m));
        m_reasonExtensions = Lemoine.Business.ServiceProvider
          .Get (request);
        if (!m_reasonExtensions.Any ()) {
          log.Error ($"ProcessingReasonSlotsAnalysis: no reason extension");
        }
      }
      SetActive ();
    }
    #endregion // Constructors

    #region IThreadClass implementation
    /// <summary>
    /// <see cref="IThreadClass"/>
    /// </summary>
    /// <returns></returns>
    public override ILog GetLogger ()
    {
      return log;
    }

    /// <summary>
    /// <see cref="IThreadClass"/>
    /// </summary>
    protected override void Run (CancellationToken cancellationToken)
    {
      while (!ExitRequested && !cancellationToken.IsCancellationRequested) {
        try {
          if (log.IsDebugEnabled) {
            log.Debug ($"Run: about to execute RunOnce");
          }
          RunOnce (cancellationToken, null);

          // - Sleep if needed
          TimeSpan every = Lemoine.Info.ConfigSet
            .LoadAndGet<TimeSpan> (AnalysisConfigKey.Every.ToString (), TimeSpan.FromSeconds (1));
          if (log.IsDebugEnabled) {
            log.Debug ($"Run: about to sleep {every}");
          }
          this.Sleep (every, cancellationToken);
          SetActive ();
        }
        catch (OperationCanceledException ex) {
          GetLogger ().Error ("Run: OperationCanceledException", ex);
          throw;
        }
        catch (Lemoine.Threading.AbortException ex) {
          GetLogger ().Error ("Run: AbortException", ex);
          throw;
        }
        catch (Exception ex) {
          SetActive ();
          if (ExceptionTest.IsStale (ex)) {
            log.Info ("Run: StaleObjectStateException => try again", ex);
          }
          else if (ExceptionTest.IsTransactionSerializationFailure (ex, log)) {
            log.Info ("Run: serialization failure => try again", ex);
          }
          else if (ExceptionTest.IsTimeoutFailure (ex, log)) {
            log.Warn ("Run: timeout failure => try again", ex);
          }
          else if (ExceptionTest.IsTemporaryWithDelay (ex, log)) {
            var temporaryWithDelayExceptionSleepTime = Lemoine.Info.ConfigSet
              .LoadAndGet (TEMPORARY_WITH_DELAY_EXCEPTION_SLEEP_TIME_KEY, TEMPORARY_WITH_DELAY_EXCEPTION_SLEEP_TIME_DEFAULT);
            log.Warn ($"Run: temporary with delay exception inner {ex.InnerException}, try again after sleeping {temporaryWithDelayExceptionSleepTime}", ex);
            this.Sleep (temporaryWithDelayExceptionSleepTime, cancellationToken);
          }
          else if (ExceptionTest.IsTemporary (ex, log)) {
            log.Warn ("Run: temporary failure => try again", ex);
          }
          else if (ExceptionTest.RequiresExit (ex, log)) {
            log.Error ("Run: requires to exit, give up", ex);
            log.Error ("Run: requires to exit, give up (inner)", ex.InnerException);
            SetExitRequested ();
            throw new Lemoine.Threading.AbortException ("Exception requires to exit.", ex);
          }
          else {
            log.Error ("Run: exception but try again", ex);
          }
        }
      }
    }

    /// <summary>
    /// Run the analysis once
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="maxDateTime"></param>
    /// <param name="range">if null (or not set), consider (-oo,+oo)</param>
    /// <param name="maxLoopNumber">Max loop number or consider a configuration key</param>
    public void RunOnce (CancellationToken cancellationToken, DateTime? maxDateTime, UtcDateTimeRange range = null, int? maxLoopNumber = null)
    {
      try {
        SetActive ();
        bool completed = false;
        int i = 1;
        var effectiveMaxLoopNumber = maxLoopNumber ?? Lemoine.Info.ConfigSet.LoadAndGet (MAX_LOOP_NUMBER_KEY, MAX_LOOP_NUMBER_DEFAULT);
        while (!completed && !cancellationToken.IsCancellationRequested) {
          if (log.IsDebugEnabled) {
            log.DebugFormat ("RunOnce: step #{0}", i);
          }
          completed = MakeAnalysis (cancellationToken, maxDateTime, range);
          ++i;
          if (maxDateTime.HasValue && (maxDateTime.Value < DateTime.UtcNow)) {
            log.Warn ($"RunOnce: the max date/time was reached after step #{i} => exit");
            return;
          }
          if (effectiveMaxLoopNumber < i) {
            log.Warn ($"RunOnce: max loop number {effectiveMaxLoopNumber} was reached => exit");
            return;
          }
          SetActive ();
        }
      }
      catch (OperationCanceledException ex) {
        GetLogger ().Error ("RunOnce: OperationCanceledException", ex);
        throw;
      }
      catch (Lemoine.Threading.AbortException ex) {
        GetLogger ().Error ("RunOnce: AbortException", ex);
        throw;
      }
      catch (Exception ex) {
        SetActive ();
        if (ExceptionTest.IsStale (ex, log)) {
          log.Info ("RunOnce: StaleObjectStateException", ex);
          throw;
        }
        else if (ExceptionTest.IsTransactionSerializationFailure (ex, log)) {
          log.Info ("RunOnce: serialization failure", ex);
          throw;
        }
        else if (ExceptionTest.IsTimeoutFailure (ex, log)) {
          log.Warn ("RunOnce: timeout failure", ex);
          throw;
        }
        else if (ExceptionTest.IsTemporaryWithDelay (ex, log)) {
          log.Warn ("RunOnce: temporary with delay failure", ex);
          throw;
        }
        else if (ExceptionTest.IsTemporary (ex, log)) {
          log.Warn ("RunOnce: temporary failure", ex);
          throw;
        }
        else if (ExceptionTest.RequiresExit (ex, log)) {
          log.Error ("RunOnce: requires to exit", ex);
          log.Error ("RunOnce: requires to exit (inner)", ex.InnerException);
          throw;
        }
        else {
          log.Exception (ex, "RunOnce");
          throw;
        }
      }
    }
    #endregion // IThreadClass implementation

    #region IChecked implementation
    /// <summary>
    /// <see cref="IChecked"/>
    /// </summary>
    public override void SetActive ()
    {
      if (null != m_checkedParent) {
        m_checkedParent.SetActive ();
      }
      else {
        base.SetActive ();
      }
    }
    #endregion // IChecked implementation

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="maxDateTime"></param>
    /// <param name="range">if null (or not set), consider (-oo,+oo)</param>
    /// <returns>Completed</returns>
    bool MakeAnalysis (CancellationToken cancellationToken, DateTime? maxDateTime, UtcDateTimeRange range = null)
    {
      int processingStepNumber = Lemoine.Info.ConfigSet
        .LoadAndGet<int> (STEP_NUMBER_KEY, STEP_NUMBER_DEFAULT);
      SetActive ();
      try {
        foreach (var reasonExtension in m_reasonExtensions) {
          reasonExtension.StartBatch ();
        }
        return MakeAnalysis (cancellationToken, maxDateTime, processingStepNumber, range);
      }
      finally {
        try {
          foreach (var reasonExtension in m_reasonExtensions) {
            try {
              reasonExtension.EndBatch ();
            }
            catch (OperationCanceledException) {
              throw;
            }
            catch (Lemoine.Threading.AbortException) {
              throw;
            }
            catch (Exception ex1) {
              log.Error ("MakeAnalysis: exception in EndBatch", ex1);
            }
          }
        }
        catch (Exception ex) {
          log.Error ("MakeAnalysis: exception in EndBatch on all extensions", ex);
        }
      }
    }

    void PreLoad (UtcDateTimeRange range)
    {
      foreach (var reasonExtension in m_reasonExtensions) {
        reasonExtension.PreLoad (range);
        SetActive ();
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="maxDateTime"></param>
    /// <param name="processingStepNumber"></param>
    /// <param name="range">if null (or not set), consider (-oo,+oo)</param>
    /// <returns></returns>
    bool MakeAnalysis (CancellationToken cancellationToken, DateTime? maxDateTime, int processingStepNumber, UtcDateTimeRange range = null)
    {
      var notNullRange = range ?? new UtcDateTimeRange ("(,)");
      IEnumerable<IReasonSlot> reasonSlots;
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ("Analysis.ProcessingReasonSlots.InitialGet")) { // read-write because the PreLoad action may update a few modifications
          reasonSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
            .FindProcessingDescending (m_machine, notNullRange, processingStepNumber, PreLoad);
          transaction.Commit ();
        }

        SetActive ();
        bool completed = true;
        bool isLast = true;
        IReasonSlot previous = null;
        try {
          foreach (var reasonSlot in reasonSlots) {
            SetActive ();
            if (cancellationToken.IsCancellationRequested) {
              if (log.IsWarnEnabled) {
                log.Warn ("MakeAnalysis: cancellation was requested");
              }
              return false;
            }

            Debug.Assert (null != reasonSlot);
            Debug.Assert (reasonSlot.Reason.Id == (int)ReasonId.Processing);
            if (log.IsDebugEnabled) {
              log.Debug ($"MakeAnalysis: about to process slot at {reasonSlot.DateTimeRange}");
            }

            completed &= Analyze (reasonSlot, isLast, out var updatedReasonSlot);
            isLast = false;

            previous = TryMerge (previous, updatedReasonSlot);

            if (maxDateTime.HasValue && (maxDateTime.Value < DateTime.UtcNow)) {
              if (log.IsWarnEnabled) {
                log.Warn ($"MakeAnalysis: the max date/time {maxDateTime} was reached => interrupt the process");
              }
              SetActive ();
              return false;
            }
            if (cancellationToken.IsCancellationRequested) {
              if (log.IsWarnEnabled) {
                log.Warn ("MakeAnalysis: cancellation was requested");
              }
              return false;
            }
          }
        }
        finally {
          if (!isLast) { // At least one was processed
            if (null != previous) {
              TryMerge (previous);
            }

            // Note: for the moment, send only the message after the set of reason slots was processed,
            //       not after each individual reason slot process
            SetActive ();
            if (log.IsDebugEnabled) {
              log.Debug ("MakeAnalysis: send the message asynchronously");
            }
            string message = "Cache/ClearDomainByMachine/ReasonAssociation/" + m_machine.Id + "?Broadcast=true";
            //Debug.Assert (!ModelDAOHelper.DAOFactory.IsTransactionActive ()); // Not active because of the unit tests
            _ = Lemoine.GDBPersistentClasses.AnalysisAccumulator
              .SendMessageAsync (message); // Because after the transaction is committed
            // Note: asynchronously, not .Wait ()
          }
        }
        SetActive ();
        return completed;
      }
    }

    /// <summary>
    /// Analyze a specified reason slot
    /// </summary>
    /// <param name="initialReasonSlot"></param>
    /// <param name="isLast">is it the last reason slot ?</param>
    /// <param name="reasonSlot">updated reason slot</param>
    /// <returns>Fully processed, else partially processed</returns>
    bool Analyze (IReasonSlot initialReasonSlot, bool isLast, out IReasonSlot reasonSlot)
    {
      Debug.Assert (null != initialReasonSlot);
      Debug.Assert (null != initialReasonSlot.Reason);
      Debug.Assert ((int)ReasonId.Processing == initialReasonSlot.Reason.Id);

      if (log.IsDebugEnabled) {
        log.Debug ($"Analyze: analyze range={initialReasonSlot.DateTimeRange} bool={isLast}");
      }

      SetActive ();

      try {
        var initialRange = initialReasonSlot.DateTimeRange;
        var initialSource = initialReasonSlot.ReasonSource;
        var initialScore = initialReasonSlot.ReasonScore;
        int initialAutoReasonNumber = initialReasonSlot.AutoReasonNumber;
        reasonSlot = initialReasonSlot;

        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (var transaction = session.BeginTransaction ($"Analysis.ProcessingReasonSlots.Analyze.{initialReasonSlot.Machine.Id}.{initialReasonSlot.DateTimeRange}")) {
            bool fullyProcessed;

            var applicableReasonExtensions = m_reasonExtensions
              .Where (ext => ext.IsResetApplicable (initialSource, initialScore, initialAutoReasonNumber))
              .OrderByDescending (ext => GetSortCriterionForResetReasonExtension (initialReasonSlot, ext))
              .ToList (); // ToList: so that the iterator is not impacted by initialReasonSlot, since it may be updated below (same reference than reasonSlot)

            using (var modificationTracker = new SlotModificationTracker<IReasonSlot> (initialReasonSlot)) {
              initialReasonSlot.CancelData ();

              bool reasonDefined = false;
              foreach (var reasonExtension in applicableReasonExtensions) {
                SetActive ();
                if ((0 < reasonSlot.ReasonScore)
                  && !reasonExtension.MayApplyAutoReasons (reasonSlot)
                  && !reasonExtension.MayApplyManualReasons (reasonSlot)) { // No impact on reasonsource => this is possible not to consider it checking the maximum score
                  double? maximumScore = reasonExtension.GetMaximumScore (reasonSlot);
                  if (maximumScore.HasValue && (maximumScore.Value < reasonSlot.ReasonScore)) {
                    if (log.IsDebugEnabled) {
                      log.Debug ($"Analyze: skip extension {reasonExtension} since its maximum score {maximumScore} is lower than the current score {reasonSlot.ReasonScore}");
                    }
                    continue;
                  }
                }
                reasonExtension.TryResetReason (ref reasonSlot);
                if (log.IsDebugEnabled) {
                  log.Debug ($"Analyze: after {reasonExtension} reasonid={reasonSlot.Reason?.Id} score={reasonSlot.ReasonScore}");
                }
                if (log.IsErrorEnabled) {
                  if ((0 < reasonSlot.ReasonScore) && ((int)ReasonId.Processing == (reasonSlot.Reason?.Id ?? (int)ReasonId.Processing))) {
                    log.Error ($"Analyze: reason {reasonSlot.Reason?.Id} processing while reason score = {reasonSlot.ReasonScore}, unexpected");
                  }
                  if ((int)ReasonId.Processing != reasonSlot.Reason?.Id) {
                    reasonDefined = true;
                  }
                  else if (reasonDefined) { // Processing again: unexpected
                    log.Error ($"Analyze: after extension {reasonExtension} the reason is again {reasonSlot.Reason?.Id} which is unexpected");
                  }
                }
                ModelDAOHelper.DAOFactory.FlushData ();
              } // foreach extension
              SetActive ();
              // Note: in some cases, this is better to consolidate the reason slot already in the plugin, because you could use
              // the processAssociation parameter, but doing it now is also great because you can do it only once
              reasonSlot.Consolidate (modificationTracker.OldSlot, null);

              if (reasonSlot.DateTimeRange.ContainsRange (initialRange)) {
                fullyProcessed = true;
                switch (reasonSlot.Reason.Id) {
                case (int)ReasonId.Processing: {
                  if (0 < reasonSlot.ReasonScore) {
                    log.Fatal ($"Analyze: processing reason with score {reasonSlot.ReasonScore}, unexpected => cancel the data");
                    reasonSlot.CancelData ();
                  }
                  log.Fatal ($"Analyze: no plugin reset the reason slot id={reasonSlot.Id} => switch it to undefined");
                  var undefinedReason = ModelDAOHelper.DAOFactory.ReasonDAO
                    .FindById ((int)ReasonId.Undefined);
                  reasonSlot.TryDefaultReasonInReset (undefinedReason, 0.0, true, false, new UpperBound<DateTime> ());
                }
                break;
                case (int)ReasonId.Undefined:
                  log.Error ($"Analyze: no plugin set a valid reason to reason slot id={reasonSlot.Id} (Reason=Undefined)");
                  break;
                default:
                  break;
                }
              }
              else { // New range is shorter
                fullyProcessed = false;
                // Note: if the reason slot is not fully processed (the range is shorter),
                // it can remain in a processing state
                if (log.IsDebugEnabled) {
                  log.Debug ($"Analyze: initial range {initialRange} does not contain new range {reasonSlot.DateTimeRange}");
                }
              }
              SetActive ();

              if (isLast && reasonSlot.DateTimeRange.Upper.Equals (initialRange.Upper)) {
                reasonSlot.UpdateMachineStatusIfApplicable ();
              }
            } // modificationTracker

            transaction.Commit ();
            return fullyProcessed;
          } // transaction
        } // session
      }
      catch (Exception ex) {
        log.Error ($"Analyze: exception processing {initialReasonSlot?.DateTimeRange}", ex);
        throw;
      }
    }

    double GetSortCriterionForResetReasonExtension (IReasonSlot reasonSlot, IReasonExtension reasonExtension)
    {
      if (reasonExtension.MayApplyAutoReasons (reasonSlot)
          || reasonExtension.MayApplyManualReasons (reasonSlot)) {
        // Apply them first because they won't be filtered by the maximum score value
        return double.MaxValue;
      }
      else {
        return reasonExtension.GetMaximumScore (reasonSlot) ?? 0.0;
      }
    }

    IReasonSlot TryMerge (IReasonSlot previous)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        IReasonSlot current;
        using (var transaction = session.BeginReadOnlyTransaction ("Analysis.ProcessingReasonSlots.TryMerge.Current")) {
          Debug.Assert (previous.BeginDateTime.HasValue);
          current = ModelDAOHelper.DAOFactory.ReasonSlotDAO
            .FindWithEnd (previous.Machine, previous.BeginDateTime.Value);
        }
        if (current is null) {
          return null;
        }
        return TryMerge (previous, current);
      }
    }

    IReasonSlot TryMerge (IReasonSlot previousSlot, IReasonSlot current)
    {
      Debug.Assert (null != current);

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        IReasonSlot previous;
        if ((null != previousSlot) && Bound<DateTime>.Equals (previousSlot.DateTimeRange.Lower, current.DateTimeRange.Upper)) {
          previous = previousSlot;
        }
        else {
          using (var transaction = session.BeginReadOnlyTransaction ("Analysis.ProcessingReasonSlots.TryMerge.Previous")) {
            Debug.Assert (current.EndDateTime.HasValue);
            previous = ModelDAOHelper.DAOFactory.ReasonSlotDAO
              .FindWithBegin (current.Machine, current.EndDateTime.Value);
          }
          if (previous is null) {
            if (log.IsDebugEnabled) {
              log.Debug ($"TryMerge: no reason slot at {current.EndDateTime}");
            }
            return current;
          }
        }

        if (!Bound<DateTime>.Equals (previous.DateTimeRange.Lower, current.DateTimeRange.Upper)) {
          log.Fatal ("TryMerge: not consecutive reason slots");
          Debug.Assert (false);
          return current;
        }

        if (!previous.ReferenceDataEquals (current)) {
          if (log.IsDebugEnabled) {
            log.Debug ("TryMerge: reference data equals returned false");
          }
          return current;
        }
        if (log.IsDebugEnabled) {
          log.Debug ($"TryMerge: about to merge {previous.DateTimeRange}  with {current.DateTimeRange}");
        }
        using (var transaction = session.BeginTransaction ($"Analysis.ProcessingReasonSlots.Merge.{current.Machine.Id}.{current.DateTimeRange}.{previous.DateTimeRange.Upper}")) {
          current.MergeWithNext (previous);
          ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakeTransient (previous);
          ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (current);
          transaction.Commit ();
        }
      }
      return current;
    }
  }
}
