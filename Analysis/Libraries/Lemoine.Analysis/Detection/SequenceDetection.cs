// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using System.Linq;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Business;
using Lemoine.Extensions.Analysis;

namespace Lemoine.Analysis.Detection
{
  /// <summary>
  /// Class to insert the right data in the database each time a sequence is detected
  /// </summary>
  public class SequenceDetection : Lemoine.Extensions.Analysis.Detection.ISequenceDetection, Lemoine.Threading.IChecked
  {
    ILog log;

    #region Members
    readonly OperationDetection m_operationDetection;
    readonly SequenceMilestoneDetection m_sequenceMilestoneDetection;
    readonly IMachineModule m_machineModule;
    DateTime? m_previousDateTime;
    readonly Lemoine.Threading.IChecked m_caller;
    readonly IEnumerable<IAfterSequenceDetectionExtension> m_extensions;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Associated machine module
    /// </summary>
    public IMachineModule MachineModule
    {
      get { return m_machineModule; }
    }

    /// <summary>
    /// Restricted transaction level
    /// </summary>
    public TransactionLevel RestrictedTransactionLevel { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor to set the machine module and sequence
    /// </summary>
    /// <param name="operationDetection">not null</param>
    /// <param name="sequenceMilestoneDetection">not null</param>
    /// <param name="machineModule">not null</param>
    /// <param name="caller"></param>
    public SequenceDetection (OperationDetection operationDetection, SequenceMilestoneDetection sequenceMilestoneDetection, IMachineModule machineModule, Lemoine.Threading.IChecked caller)
    {
      Debug.Assert (null != operationDetection);
      Debug.Assert (null != machineModule);

      m_operationDetection = operationDetection;
      m_sequenceMilestoneDetection = sequenceMilestoneDetection;
      m_machineModule = machineModule;
      m_caller = caller;

      m_extensions = ServiceProvider
        .Get<IEnumerable<IAfterSequenceDetectionExtension>> (new Lemoine.Business.Extension.MachineModuleExtensions<IAfterSequenceDetectionExtension> (machineModule, (ext, m) => ext.Initialize (m)));

      log = LogManager.GetLogger (string.Format ("{0}.{1}.{2}",
                                                 this.GetType ().FullName,
                                                 machineModule.MonitoredMachine.Id,
                                                 machineModule.Id));

      this.RestrictedTransactionLevel = TransactionLevel.Serializable;
    }
    #endregion // Constructors

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

    /// <summary>
    /// Start an auto-only operation
    /// 
    /// Update the autosequence or sequenceslot table accordingly
    /// 
    /// It contains a top transaction and must not be run inside a transaction
    /// </summary>
    /// <param name="operation">not null</param>
    /// <param name="dateTime"></param>
    public void StartAutoOnlyOperation (IOperation operation, DateTime dateTime)
    {
      Debug.Assert (null != operation);

      log.DebugFormat ("StartAutoOnlyOperation /B " +
                       "operation={0} dateTime={1}",
                       operation, dateTime);

      // Check the dateTime are coming in chronological order
      if (m_previousDateTime.HasValue) {
        if (dateTime < m_previousDateTime.Value) {
          log.FatalFormat ("StartAutoOnlyOperation: " +
                           "operation {0} is coming at {1} after a previously operation/sequence at {2}, " +
                           "=> skip it",
                           operation, dateTime, m_previousDateTime.Value);
          Debug.Assert (m_previousDateTime <= dateTime);
          return;
        }
      }
      m_previousDateTime = dateTime;

      // Note: the machine module was already re-associated in the parent session in StartStamp
      // Note: this is done in two steps, each step has its own transaction

      // - 1st transaction: remove if needed the sequence milestone
      m_sequenceMilestoneDetection.CancelSequenceMilestone (dateTime);

      // - 2nd transaction: cut different operation after dateTime
      //   because the auto-sequence has an higher priority on future operation slots
      CutDifferentOperationAfter (operation, dateTime);

      // - 3rd transaction: main process
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ("Detection.StartAutoOnlyOperation", this.RestrictedTransactionLevel)) {
        transaction.SynchronousCommitOption = SynchronousCommit.Off; // This is ok because it may be replayed

        // - Cut any existing sequenceslot
        {
          int numberOfImpactedSlots = RemoveAfter (dateTime);
          if (0 < numberOfImpactedSlots) {
            // - Remove any operation after dateTime in the same time
            log.DebugFormat ("StartAutoOnlyOperation: " +
                             "stop operation at {0}",
                             dateTime);
            StopOperation (dateTime);
          }
        }

        // - Cut any existing autosequence or make it longer
        bool processed = false;
        {
          IList<IAutoSequence> autoSequences =
            ModelDAOHelper.DAOFactory.AutoSequenceDAO
            .FindAllAtAndAfter (m_machineModule, dateTime);
          foreach (IAutoSequence autoSequence in autoSequences) {
            Debug.Assert (Bound.Compare<DateTime> (dateTime, autoSequence.End) < 0);

            if ((autoSequence.Begin <= dateTime)
                && (null == autoSequence.Sequence)
                && (null != autoSequence.Operation)
                && (autoSequence.Operation.Equals (operation))) {
              // matching present auto-sequence: set the end date/time to oo
              log.DebugFormat ("StartAutoOnlyOperation: " +
                               "extend {0} to oo",
                               autoSequence);
              autoSequence.End = new UpperBound<DateTime> (null);
              ModelDAOHelper.DAOFactory.AutoSequenceDAO.MakePersistent (autoSequence);
              processed = true;
            }
            else {
              log.DebugFormat ("StartAutoOnlyOperation: " +
                               "truncate or remove {0} at {1}",
                               autoSequence, dateTime);
              TruncateOrRemove (autoSequence, dateTime);
            }
          }
        }

        // - If no existing autosequence was made longer, add a new autosequence
        if (!processed) {
          // - Flush the database to avoid any duplicate key (remove effectively the data first before adding a new one)
          ModelDAOHelper.DAOFactory.Flush ();

          log.DebugFormat ("StartAutoOnlyOperation: " +
                           "create a new auto sequence at {0} for operation {1}",
                           dateTime, operation);
          IAutoSequence newAutoSequence =
            ModelDAOHelper.ModelFactory
            .CreateAutoSequence (m_machineModule, operation, dateTime);
          ModelDAOHelper.DAOFactory.AutoSequenceDAO
            .MakePersistent (newAutoSequence);
        }

        transaction.Commit ();
      }

      try {
        foreach (var extension in m_extensions) {
          extension.StartAutoOnlyOperation (operation, dateTime);
        }
      }
      catch (Exception ex) {
        log.Error ("StartAutoOnlyOperation: extension exception", ex);
        throw;
      }
    }

    /// <summary>
    /// Start a new sequence
    /// 
    /// Update the autosequence or sequenceslot table accordingly
    /// 
    /// This method contains top transactions and must not be run inside a transaction
    /// </summary>
    /// <param name="sequence">not null</param>
    /// <param name="dateTime"></param>
    public void StartSequence (ISequence sequence, DateTime dateTime)
    {
      Debug.Assert (null != sequence);

      log.DebugFormat ("StartSequence /B " +
                       "sequence={0} dateTime={1}",
                       sequence, dateTime);

      // Check the dateTime are coming in chronological order
      if (m_previousDateTime.HasValue) {
        if (dateTime < m_previousDateTime.Value) {
          log.FatalFormat ("StartSequence: " +
                           "sequence {0} is coming at {1} after a previously sequence at {2}, " +
                           "=> skip it",
                           sequence, dateTime, m_previousDateTime.Value);
          Debug.Assert (m_previousDateTime <= dateTime);
          return;
        }
      }
      m_previousDateTime = dateTime;

      // Note: the machine module was already re-associated in the parent session in StartStamp
      // Note: this is done in two steps, each step has its own transaction

      // - 1st transaction
      m_sequenceMilestoneDetection.StartSequence (dateTime, sequence);

      // - 2nd transaction: cut different operation after dateTime
      //   because the auto-sequence has an higher priority on future operation slots
      Debug.Assert (null != sequence.Operation);
      CutDifferentOperationAfter (sequence.Operation, dateTime);

      // - 3rd transaction: process the sequence
      if (sequence.AutoOnly) {
        StartAutoOnlySequence (sequence, dateTime);
      }
      else {
        StartNotAutoSequence (sequence, dateTime);
      }

      try {
        foreach (var extension in m_extensions) {
          extension.StartSequence (sequence, dateTime);
        }
      }
      catch (Exception ex) {
        log.Error ("StartSequence: extension exception", ex);
        throw;
      }
    }

    void StartAutoOnlySequence (ISequence sequence, DateTime dateTime)
    {
      log.DebugFormat ("StartAutoOnlySequence /B " +
                       "sequence={0} dateTime={1}",
                       sequence, dateTime);

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ("Detection.StartAutoOnlySequence", this.RestrictedTransactionLevel)) {
        transaction.SynchronousCommitOption = SynchronousCommit.Off; // This is ok because it may be replayed

        // - Cut any existing sequenceslot
        {
          int numberOfImpactedSlots = RemoveAfter (dateTime);
          if (0 < numberOfImpactedSlots) {
            // - Remove any operation after dateTime in the same time
            log.DebugFormat ("StartAutoOnlySequence: " +
                             "stop operation at {0}",
                             dateTime);
            StopOperation (dateTime);
          }
        }

        // - Cut any existing autosequence or make it longer
        bool processed = false;
        {
          IList<IAutoSequence> autoSequences =
            ModelDAOHelper.DAOFactory.AutoSequenceDAO
            .FindAllAtAndAfter (m_machineModule, dateTime);
          foreach (IAutoSequence autoSequence in autoSequences) {
            Debug.Assert (Bound.Compare<DateTime> (dateTime, autoSequence.End) < 0);

            if ((autoSequence.Begin <= dateTime)
                && (null == autoSequence.Operation)
                && (null != autoSequence.Sequence)
                && (autoSequence.Sequence.Equals (sequence))) {
              // matching present auto-sequence: set the end date/time to oo
              log.DebugFormat ("StartAutoOnlySequence: " +
                               "extend {0} to oo",
                               autoSequence);
              autoSequence.End = new UpperBound<DateTime> (null);
              ModelDAOHelper.DAOFactory.AutoSequenceDAO.MakePersistent (autoSequence);
              processed = true;
            }
            else {
              log.DebugFormat ("StartAutoOnlySequence: " +
                               "truncate or remove {0} at {1}",
                               autoSequence, dateTime);
              TruncateOrRemove (autoSequence, dateTime);
            }
          }
        }

        // - If no existing autosequence was made longer, add a new autosequence
        if (!processed) {
          // - Flush the database to avoid any duplicate key (remove effectively the data first before adding a new one)
          ModelDAOHelper.DAOFactory.Flush ();

          log.DebugFormat ("StartAutoOnlySequence: " +
                           "create a new autosequence at {0} for {1}",
                           dateTime, sequence);
          IAutoSequence newAutoSequence =
            ModelDAOHelper.ModelFactory
            .CreateAutoSequence (m_machineModule, sequence, dateTime);
          ModelDAOHelper.DAOFactory.AutoSequenceDAO
            .MakePersistent (newAutoSequence);
        }

        transaction.Commit ();
      }
    }

    void StartNotAutoSequence (ISequence sequence, DateTime dateTime)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"StartNotAutoSequence /B sequence={sequence?.Id} dateTime={dateTime}");
      }

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ("Detection.StartNotAutoSequence", this.RestrictedTransactionLevel)) {
        transaction.SynchronousCommitOption = SynchronousCommit.Off; // This is ok because it may be replayed

        // - Cut any existing autosequence
        {
          IList<IAutoSequence> autoSequences =
            ModelDAOHelper.DAOFactory.AutoSequenceDAO
            .FindAllAtAndAfter (m_machineModule, dateTime);
          foreach (IAutoSequence autoSequence in autoSequences) {
            if (log.IsDebugEnabled) {
              log.Debug ($"StartNotAutoSequence: truncate {autoSequence} at {dateTime}");
            }
            Debug.Assert (Bound.Compare<DateTime> (dateTime, autoSequence.End) < 0, $"autoSequence with end {autoSequence.End} not after {dateTime}");
            TruncateOrRemove (autoSequence, dateTime);
          }
        }

        // - Cut any existing sequenceslot or make it longer
        bool processed = false;
        ISequenceSlot previousSlot = null;
        {
          IList<ISequenceSlot> sequenceSlots =
            ModelDAOHelper.DAOFactory.SequenceSlotDAO
            .FindAllAtAndAfter (m_machineModule, dateTime);
          // Note: there should be maximum one sequence slot in the present
          //       and no sequence slot only in the future
          if (sequenceSlots.Count <= 1) {
            log.Warn ($"StartNotAutoSequence: {sequenceSlots.Count} slots at and after {dateTime}");
          }
          foreach (ISequenceSlot sequenceSlot in sequenceSlots) {
            Debug.Assert (NullableDateTime.Compare (dateTime, sequenceSlot.EndDateTime) < 0, $"sequenceSlot with end {sequenceSlot.EndDateTime} not after {dateTime}");

            if ((sequenceSlot.BeginDateTime <= dateTime)
                && (object.Equals (sequenceSlot.Sequence, sequence))) {
              // matching present sequence slot: set the end date/time to oo
              if (log.IsDebugEnabled) {
                log.Debug ($"StartNotAutoSequence: extend sequence slot {sequenceSlot} to oo");
              }
              MakeSequenceSlotLonger (sequenceSlot, new UpperBound<DateTime> (null));
              processed = true;
            }
            else {
              log.Error ($"StartNotAutoSequence: sequenceSlot id={sequenceSlot.Id} start={sequenceSlot.BeginDateTime} in the future, dateTime={dateTime}");

              // NextBegin does not need to be considered here because there is no sequence slot only in the future
              if (log.IsDebugEnabled) {
                log.Debug ($"StartNotAutoSequence: truncate sequence slot {sequenceSlot} at {dateTime}");
              }
              bool removeSlot = TruncateOrRemove (sequenceSlot, dateTime);
              // Note: there is no need to cut here the operation slots, because a new operation slot will be created
              //       when the sequence slot is created or made longer
              if (!removeSlot) {
                Debug.Assert (previousSlot is null, "previousSlot is not null although maximum one sequence slot is supposed to be removed");
                previousSlot = sequenceSlot;
              }
            }
          }
        }

        // - If no existing sequenceslot was made longer, add directly a new data in the sequenceslot table"
        if (!processed) {
          if (log.IsDebugEnabled) {
            log.Debug ($"StartNotAutoSequence: create a new sequence slot at {dateTime} for sequence {sequence}");
          }
          UtcDateTimeRange range = new UtcDateTimeRange (dateTime);
          ISequenceSlot sequenceSlot = ModelDAOHelper.ModelFactory.CreateSequenceSlot (m_machineModule,
                                                                                       sequence,
                                                                                       range);
          ModelDAOHelper.DAOFactory.SequenceSlotDAO.MakePersistent (sequenceSlot);
          Debug.Assert (null != sequence.Operation, "sequence with no operation");
          AddOperation (sequence.Operation, range);
          // Re-adjust NextBegin of the previous slot
          if (null == previousSlot) { // 
            previousSlot = ModelDAOHelper.DAOFactory.SequenceSlotDAO
              .FindFirstBefore (m_machineModule, dateTime);
            if (log.IsDebugEnabled) {
              log.Debug ($"StartNotAutoSequence: got the sequence {previousSlot} for the previous slot");
            }
          }
          AdjustNextBegin (previousSlot, sequenceSlot);
        }

        transaction.Commit ();
      }
    }

    /// <summary>
    /// Stop an sequence, e.g. because the end of an ISO file was reached
    /// 
    /// Update the autosequence and sequenceslot tables accordingly
    /// </summary>
    /// <param name="dateTime"></param>
    public void StopSequence (DateTime dateTime)
    {
      // - 1st transaction
      m_sequenceMilestoneDetection.TagSequenceCompleted (dateTime);

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ("Detection.StopSequence", this.RestrictedTransactionLevel)) {
        transaction.SynchronousCommitOption = SynchronousCommit.Off; // This is ok because it may be replayed

        // - Cut any existing sequenceslot
        {
          int numberOfImpactedSlots = RemoveAfter (dateTime);
          if (0 < numberOfImpactedSlots) {
            // - Remove any operation after dateTime in the same time
            StopOperation (dateTime);
          }
        }

        // - Cut any existing autosequence
        {
          IList<IAutoSequence> autoSequences =
            ModelDAOHelper.DAOFactory.AutoSequenceDAO
            .FindAllAtAndAfter (m_machineModule, dateTime);
          foreach (IAutoSequence autoSequence in autoSequences) {
            Debug.Assert (Bound.Compare<DateTime> (dateTime, autoSequence.End) <= 0);
            TruncateOrRemove (autoSequence, dateTime);
          }
        }

        transaction.Commit ();
      }

      try {
        foreach (var extension in m_extensions) {
          extension.StopSequence (dateTime);
        }
      }
      catch (Exception ex) {
        log.Error ("StopSequence: extension exception", ex);
        throw;
      }
    }

    /// <summary>
    /// Adjust the next begin property
    /// <item>in case the next sequence slot comes before the existing nextbegin</item>
    /// <item>in case nextbegin is still unknown</item>
    /// </summary>
    /// <param name="previousSequenceSlot"></param>
    /// <param name="nextSequenceSlot"></param>
    internal void AdjustNextBegin (ISequenceSlot previousSequenceSlot, ISequenceSlot nextSequenceSlot)
    {
      if ((null != previousSequenceSlot) && (null != nextSequenceSlot)
          && (!previousSequenceSlot.NextBegin.HasValue
              || (Bound.Compare<DateTime> (nextSequenceSlot.BeginDateTime, previousSequenceSlot.NextBegin.Value) < 0))) {
        Debug.Assert (previousSequenceSlot.EndDateTime.HasValue);
        Debug.Assert (previousSequenceSlot.EndDateTime.Value <= nextSequenceSlot.BeginDateTime);
        Debug.Assert (nextSequenceSlot.BeginDateTime.HasValue);
        log.DebugFormat ("AdjustNextBegin: " +
                         "adjust the NextBegin of {0} to {1}",
                         previousSequenceSlot, nextSequenceSlot.BeginDateTime);
        previousSequenceSlot.NextBegin = nextSequenceSlot.BeginDateTime.Value;
      }
    }

    /// <summary>
    /// Remove all the sequence slots after the specified date/time
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns>Number of impacted sequence slots</returns>
    int RemoveAfter (DateTime dateTime)
    {
      log.DebugFormat ("RemoveAfter /B " +
                       "dateTime={0}",
                       dateTime);

      IList<ISequenceSlot> sequenceSlots =
        ModelDAOHelper.DAOFactory.SequenceSlotDAO
        .FindAllAtAndAfter (m_machineModule, dateTime);
      foreach (ISequenceSlot sequenceSlot in sequenceSlots) {
        log.DebugFormat ("RemoveAfter: " +
                         "truncate or remove at {0} {1}",
                         dateTime, sequenceSlot);
        TruncateOrRemove (sequenceSlot, dateTime);
      }

      { // Remove the NextBegin property of the last sequence slot if the NextBegin property is after dateTime
        ISequenceSlot lastSequenceSlot = ModelDAOHelper.DAOFactory.SequenceSlotDAO
          .FindLast (m_machineModule);
        if (null != lastSequenceSlot) {
          Debug.Assert (Bound.Compare<DateTime> (lastSequenceSlot.BeginDateTime, dateTime) < 0);
          Debug.Assert (Bound.Compare<DateTime> (lastSequenceSlot.EndDateTime, dateTime) <= 0);
          log.DebugFormat ("RemoveAfter: " +
                           "lastSequenceSlot is {0}",
                           lastSequenceSlot);
          if ((lastSequenceSlot.NextBegin.HasValue)
              && (dateTime <= lastSequenceSlot.NextBegin.Value)) {
            log.DebugFormat ("RemoveAfter: " +
                             "remove NextBegin from {0}",
                             lastSequenceSlot);
            lastSequenceSlot.NextBegin = null;
            ModelDAOHelper.DAOFactory.SequenceSlotDAO.MakePersistent (lastSequenceSlot);
          }
        }
      }

      return sequenceSlots.Count;
    }

    /// <summary>
    /// dateTime must be before sequenceSlot.End
    /// 
    /// Do not process here the NextBegin property
    /// </summary>
    /// <param name="sequenceSlot"></param>
    /// <param name="dateTime"></param>
    /// <returns>the sequence slot was removed</returns>
    bool TruncateOrRemove (ISequenceSlot sequenceSlot, DateTime dateTime)
    {
      Debug.Assert (NullableDateTime.Compare (dateTime, sequenceSlot.EndDateTime) <= 0);

      if ((Bound.Compare<DateTime> (sequenceSlot.BeginDateTime, dateTime) < 0)
          && (!sequenceSlot.BeginDateTime.HasValue
              || (TimeSpan.FromSeconds (1) <= dateTime.Subtract (sequenceSlot.BeginDateTime.Value)))) { // Present: truncate it
        // Note the duration must be more than 1 s, else the unicity of the sequenceslotend won't be checked
        sequenceSlot.EndDateTime = dateTime;
        ModelDAOHelper.DAOFactory.SequenceSlotDAO.MakePersistent (sequenceSlot);
      }
      else { // In the future, or possible null duration: remove it
        if (Bound.Compare<DateTime> (dateTime, sequenceSlot.BeginDateTime) < 0) {
          // Note: in normal cases, there is no reason why it should happen
          //       No exception is returned here though for abnormal cases
          log.ErrorFormat ("TruncateOrRemove: " +
                           "sequence slot {0} in the future. " +
                           "Was the data replayed ?",
                           sequenceSlot);
        }
        // Note: a sequence slot with a null duration may be normal here
        ModelDAOHelper.DAOFactory.SequenceSlotDAO.MakeTransient (sequenceSlot);
        return true;
      }

      return false;
    }

    /// <summary>
    /// dateTime must be before autoSequence.End
    /// </summary>
    /// <param name="autoSequence"></param>
    /// <param name="dateTime"></param>
    void TruncateOrRemove (IAutoSequence autoSequence, DateTime dateTime)
    {
      Debug.Assert (Bound.Compare<DateTime> (dateTime, autoSequence.End) <= 0);

      log.DebugFormat ("TruncateOrRemove autosequence /B " +
                       "dateTime={0} autoSequence={1}",
                       dateTime, autoSequence);

      if ((Bound.Compare<DateTime> (autoSequence.Begin, dateTime) < 0)
          && (TimeSpan.FromSeconds (1) <= dateTime.Subtract (autoSequence.Begin))) { // Present: truncate it
        // Note the duration must be more than 1 s, else the unicity of the sequenceslotend won't be checked
        log.DebugFormat ("TruncateOrRemove autosequence: " +
                         "truncate {0} at {1}",
                         autoSequence, dateTime);
        autoSequence.End = dateTime;
        ModelDAOHelper.DAOFactory.AutoSequenceDAO.MakePersistent (autoSequence);
      }
      else { // In the future, or possible null duration: remove it
        if (Bound.Compare<DateTime> (dateTime, autoSequence.Begin) < 0) {
          // Note: in normal cases, there is no reason why it should happen
          //       No exception is returned here though for abnormal cases
          log.ErrorFormat ("TruncateOrRemove autosequence: " +
                           "{0} in the future. " +
                           "Was the data replayed ?",
                           autoSequence);
        }
        ModelDAOHelper.DAOFactory.AutoSequenceDAO
          .MakeTransient (autoSequence);
      }
    }

    internal ISequenceSlot MakeSequenceSlotLonger (ISequenceSlot sequenceSlot, UpperBound<DateTime> newEnd)
    {
      if (NullableDateTime.Compare (sequenceSlot.EndDateTime, newEnd) == 0) {
        log.WarnFormat ("MakeSequenceSlotLonger: " +
                        "no change to make, the sequence slot {0} already reaches {1}",
                        sequenceSlot, newEnd);
        return sequenceSlot;
      }

      Debug.Assert (NullableDateTime.Compare (sequenceSlot.EndDateTime, newEnd) < 0);
      Debug.Assert (sequenceSlot.EndDateTime.HasValue);

      log.DebugFormat ("MakeSequenceSlotLonger: " +
                       "make the sequence slot {0} longer from {1} to {2}",
                       sequenceSlot, sequenceSlot.EndDateTime, newEnd);

      if (sequenceSlot.NextBegin.HasValue
          && (NullableDateTime.Compare (sequenceSlot.NextBegin, newEnd) < 0)) {
        string message = string.Format ("Extend a sequence slot to {0} after NextBegin {1}",
                                        newEnd, sequenceSlot.NextBegin);
        log.WarnFormat ("MakeSequenceLonger: " +
                        "{0}",
                        message);
        IDetectionAnalysisLog detectionAnalysisLog = ModelDAOHelper.ModelFactory
          .CreateDetectionAnalysisLog (LogLevel.WARN, message, m_machineModule.MonitoredMachine, m_machineModule);
        detectionAnalysisLog.Module = "SequenceDetection";
        ModelDAOHelper.DAOFactory.DetectionAnalysisLogDAO.MakePersistent (detectionAnalysisLog);
      }

      if ((null != sequenceSlot.Sequence) && (null != sequenceSlot.Sequence.Operation)) {
        // Note: disallow the auto-operation process, to make the process faster
        AddOperation (sequenceSlot.Sequence.Operation, new UtcDateTimeRange (sequenceSlot.EndDateTime.Value, newEnd),
                      false);
      }

      if (sequenceSlot.NextBegin.HasValue
          && (NullableDateTime.Compare (sequenceSlot.NextBegin.Value, newEnd) < 0)) { // New end is after nextbegin
        // The only case when this may happen is when the nextbegin comes from a cycle end
        // Reset NextBegin to null, although I am not sure this is the best solution
        log.WarnFormat ("MakeSequenceLonger: " +
                        "new end is after a nextbegin that was set by a cycle end " +
                        "=> reset nextbegin to end");
        sequenceSlot.NextBegin = null;
      }

      sequenceSlot.EndDateTime = newEnd;
      return sequenceSlot;
    }

    /// <summary>
    /// Note: begin &lt; end (without any ms)
    /// </summary>
    /// <param name="sequence"></param>
    /// <param name="range">not empty</param>
    /// <returns></returns>
    internal ISequenceSlot AddNewSequenceSlot (ISequence sequence, UtcDateTimeRange range)
    {
      Debug.Assert (!range.Lower.HasValue || (0 == range.Lower.Value.Millisecond));
      Debug.Assert (!range.Upper.HasValue || (0 == range.Upper.Value.Millisecond));
      Debug.Assert (!range.IsEmpty ());

      log.DebugFormat ("AddNewSequenceSlot: " +
                       "add a new sequence sequence={0} range={1}",
                       sequence, range);

      ISequenceSlot newSequenceSlot =
        ModelDAOHelper.ModelFactory.CreateSequenceSlot (m_machineModule, sequence, range);
      ModelDAOHelper.DAOFactory.SequenceSlotDAO.MakePersistent (newSequenceSlot);
      if ((null != sequence) && (null != sequence.Operation)) {
        AddOperation (sequence.Operation, range);
      }
      return newSequenceSlot;
    }

    /// <summary>
    /// Add an operation
    /// </summary>
    /// <param name="operation">not null</param>
    /// <param name="range"></param>
    internal void AddOperation (IOperation operation, UtcDateTimeRange range)
    {
      AddOperation (operation, range, true);
    }

    /// <summary>
    /// Add an operation
    /// 
    /// Note:
    /// - for multi-machine module machines, the call of AddOperation is not always chronological
    /// - because 'Auto-Only' and 'Not auto-only' sequences are processed separately, the call to AddOperation
    ///   could not be chronological
    /// </summary>
    /// <param name="operation">Not null</param>
    /// <param name="range"></param>
    /// <param name="autoOperation"></param>
    internal void AddOperation (IOperation operation, UtcDateTimeRange range, bool autoOperation)
    {
      Debug.Assert (null != operation);
      Debug.Assert (!range.IsEmpty ());

      log.DebugFormat ("AddOperation: " +
                       "operation={0} in period {1}",
                       operation, range);

      m_operationDetection.AddOperation (operation, range, autoOperation);
    }

    void StopOperation (DateTime dateTime)
    {
      log.DebugFormat ("StopOperation: " +
                       "at {0}",
                       dateTime);

      var association =
        new Lemoine.GDBPersistentClasses.OperationMachineAssociation (m_machineModule.MonitoredMachine,
                                                                       new UtcDateTimeRange (dateTime),
                                                                       null, // Transient !
                                                                       true);
      association.Operation = null;
      association.Caller = m_caller;
      association.Apply ();
    }

    /// <summary>
    /// Cut any operation slot with a different operation after the specified date/time
    /// 
    /// This can be necessary because the auto-sequence has an higher priority on future operation slots
    /// 
    /// It contains a top transaction and must not be run inside a transaction
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="dateTime"></param>
    void CutDifferentOperationAfter (IOperation operation, DateTime dateTime)
    {
      log.DebugFormat ("CutDifferentOperationAfter: " +
                       "operation={0} dateTime={1}",
                       operation, dateTime);

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ("Detection.CutDifferentOperationAfter",
                                                                     this.RestrictedTransactionLevel)) {
        transaction.SynchronousCommitOption = SynchronousCommit.Off; // This is ok because it may be replayed

        IOperationSlot firstDifferentOperationSlot =
          ModelDAOHelper.DAOFactory.OperationSlotDAO
          .GetFirstDifferentOperationAfter (m_machineModule.MonitoredMachine,
                                            dateTime,
                                            operation);
        if (null != firstDifferentOperationSlot) {
          DateTime cutTime = LowerBound.GetMaximum<DateTime> (firstDifferentOperationSlot.BeginDateTime, dateTime).Value;
          string message = string.Format ("Discontinue at {0} the future operation slot at {1} because of the sequence detection",
                                          cutTime, firstDifferentOperationSlot.BeginDateTime);
          log.InfoFormat ("AddOperation: " +
                          "cut future operation slots at {0}, {1}",
                          cutTime, message);
          IDetectionAnalysisLog cutLog = ModelDAOHelper.ModelFactory
            .CreateDetectionAnalysisLog (LogLevel.INFO,
                                         message,
                                         m_machineModule.MonitoredMachine,
                                         m_machineModule);
          ModelDAOHelper.DAOFactory.DetectionAnalysisLogDAO.MakePersistent (cutLog);
          var cutAssociation =
            new Lemoine.GDBPersistentClasses.OperationMachineAssociation (m_machineModule.MonitoredMachine,
                                                                          new UtcDateTimeRange (cutTime),
                                                                          null, // Transient !
                                                                          true);
          cutAssociation.Operation = null;
          cutAssociation.Caller = m_caller;
          cutAssociation.Apply ();
        }

        transaction.Commit ();
      }
    }
  }
}
