// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Analysis.Detection;

namespace Lemoine.Analysis
{
  /// <summary>
  /// Description of MachineModuleAutoSequenceAnalysis.
  /// </summary>
  internal sealed class AutoSequenceMachineModuleAnalysis : Lemoine.Threading.IChecked
  {
    #region Members
    readonly IMachineModule m_machineModule;
    readonly TransactionLevel m_restrictedTransactionLevel;
    readonly SequenceDetection m_sequenceDetection;
    readonly SequenceMilestoneDetection m_sequenceMilestoneDetection;
    readonly Lemoine.Threading.IChecked m_caller;
    #endregion // Members

    readonly ILog log = LogManager.GetLogger (typeof (AutoSequenceMachineModuleAnalysis).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="operationDetection">not null</param>
    /// <param name="machineModule">not null</param>
    /// <param name="restrictedTransactionLevel"></param>
    /// <param name="caller"></param>
    public AutoSequenceMachineModuleAnalysis (Lemoine.Analysis.Detection.OperationDetection operationDetection, IMachineModule machineModule, TransactionLevel restrictedTransactionLevel, Lemoine.Threading.IChecked caller)
    {
      Debug.Assert (null != operationDetection);
      Debug.Assert (null != machineModule);
      Debug.Assert (null != machineModule.MonitoredMachine);

      m_machineModule = machineModule;
      log = LogManager.GetLogger (string.Format ("{0}.{1}.{2}",
                                                 typeof (AutoSequenceMachineModuleAnalysis).FullName,
                                                 machineModule.MonitoredMachine.Id,
                                                 machineModule.Id));
      m_restrictedTransactionLevel = restrictedTransactionLevel;
      m_caller = caller;

      m_sequenceMilestoneDetection = new SequenceMilestoneDetection (machineModule, m_caller);
      m_sequenceDetection = new SequenceDetection (operationDetection, m_sequenceMilestoneDetection, machineModule, m_caller);
    }

    /// <summary>
    /// Constructor for the unit tests
    /// </summary>
    /// <param name="operationDetection">not null</param>
    /// <param name="sequenceMilestoneDetection">not null</param>
    /// <param name="machineModule">not null</param>
    /// <param name="restrictedTransactionLevel"></param>
    internal AutoSequenceMachineModuleAnalysis (Lemoine.Analysis.Detection.OperationDetection operationDetection, IMachineModule machineModule, TransactionLevel restrictedTransactionLevel)
      : this (operationDetection, machineModule, restrictedTransactionLevel, null)
    {
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

    #region Methods
    /// <summary>
    /// Not to re-write all the unit tests after the API change
    /// </summary>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    internal void AddAutoSequencePeriod (DateTime begin, DateTime end)
    {
      UtcDateTimeRange range = new UtcDateTimeRange (begin, end);
      IEnumerable<IAutoSequence> autoSequences =
        ModelDAOHelper.DAOFactory.AutoSequenceDAO
        .FindAllBetween (m_machineModule, range);
      AddAutoSequencePeriod (range, autoSequences);
    }

    /// <summary>
    /// Add an auto-sequence period
    /// 
    /// The period must not be empty (begin &lt; end)
    /// 
    /// begin and end must have been rounded to the closest second
    /// </summary>
    /// <param name="range"></param>
    /// <param name="matchingAutoSequences">may be detached</param>
    internal void AddAutoSequencePeriod (UtcDateTimeRange range, IEnumerable<IAutoSequence> matchingAutoSequences)
    {
      Debug.Assert (!range.IsEmpty ());
      Debug.Assert (range.Lower.HasValue);
      Debug.Assert (range.Upper.HasValue);
      Debug.Assert (0 == range.Lower.Value.Millisecond);
      Debug.Assert (0 == range.Upper.Value.Millisecond);

      log.Debug ($"AddAutoSequencePeriod /B {range}");

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginTransaction ("Detection.AddAutoSequencePeriod", m_restrictedTransactionLevel)) { // First transaction: process
          transaction.SynchronousCommitOption = SynchronousCommit.Off; // This is ok because it may be replayed

          // - Get the impacted sequence slots
          IList<ISequenceSlot> sequenceSlots =
            ModelDAOHelper.DAOFactory.SequenceSlotDAO
            .FindAllEndFrom (m_machineModule, range.Lower.Value);
          // If the first sequence slot is after begin,
          // get the previous one too
          if ((0 == sequenceSlots.Count) || (range.Lower.Value <= sequenceSlots[0].BeginDateTime)) {
            ISequenceSlot previousSequenceSlot =
              ModelDAOHelper.DAOFactory.SequenceSlotDAO
              .FindFirstBefore (m_machineModule, range.Lower.Value);
            if (null != previousSequenceSlot) {
              sequenceSlots.Insert (0, previousSequenceSlot);
            }
          }

          // After begin, there is a set of IAutoSequences (from AutoOnly sequences)
          // and ISequenceSlots (from not AutoOnly sequences).
          // There is a disjunction between them
          // Visit all the periods without an IAutoSequence or a ISequenceSlot between begin and end
          {
            DateTime currentBegin = range.Lower.Value;
            IAutoSequence nextAutoSequence = null; // whose end is after currentBegin
            ISequenceSlot nextSequenceSlot = null; // whose end is after currentBegin
            IEnumerable<IAutoSequence> remainingAutoSequences = matchingAutoSequences;
            int nextSequenceSlotIndex = 0;
            DateTime previousCurrentBegin = new DateTime (0); // To check currentBegin changes each time in the loop
            ISequenceSlot previousSequenceSlot = null;
            // Initialize previousSequenceSlot and nextSequenceSlot
            while ((nextSequenceSlotIndex < sequenceSlots.Count)
                   && sequenceSlots[nextSequenceSlotIndex].EndDateTime.HasValue
                   && (sequenceSlots[nextSequenceSlotIndex].EndDateTime.Value <= range.Lower.Value)) {
              previousSequenceSlot = sequenceSlots[nextSequenceSlotIndex];
              ++nextSequenceSlotIndex;
            }
            if (nextSequenceSlotIndex < sequenceSlots.Count) {
              nextSequenceSlot = sequenceSlots[nextSequenceSlotIndex];
            }
            while (currentBegin < range.Upper.Value) { // currentBegin is moving forward each time
              // Check currentBegin moves forward each time
              // else there is a risk of oo loop
              Debug.Assert (previousCurrentBegin < currentBegin);
              previousCurrentBegin = currentBegin;

              // Readjust nextSequenceSlot compared to currentBegin
              if (0 < sequenceSlots.Count) {
                while ((nextSequenceSlotIndex < sequenceSlots.Count)
                       && (NullableDateTime.Compare (sequenceSlots[nextSequenceSlotIndex].EndDateTime,
                                                     currentBegin) <= 0)) {
                  ++nextSequenceSlotIndex;
                  // Do the following lines only if nextSequenceSlotIndex changed
                  // not to overwrite any existing previousSequenceSlot
                  previousSequenceSlot = nextSequenceSlot;
                  if (nextSequenceSlotIndex < sequenceSlots.Count) {
                    nextSequenceSlot = sequenceSlots[nextSequenceSlotIndex];
                    if (log.IsFatalEnabled) {
                      if (nextSequenceSlot.Sequence?.AutoOnly ?? false) {
                        log.Fatal ($"AddAutoSequencePeriod: next auto sequence slot at {nextSequenceSlot.DateTimeRange} is AutoOnly. How is it possible since AddAutoSequencePeriod is currently executed on {range} ?");
                      }
                      if (Bound.Compare<DateTime> (nextSequenceSlot.EndDateTime, currentBegin) <= 0) {
                        log.Fatal ($"AddAutoSequencePeriod: next sequence slot at {nextSequenceSlot.DateTimeRange} is before current begin {currentBegin}, which is unexpected");
                      }
                    }
                  }
                  else {
                    nextSequenceSlot = null;
                  }
                }
              }

              if (log.IsFatalEnabled) {
                if (!(previousSequenceSlot?.EndDateTime.HasValue ?? true)) {
                  log.Fatal ($"AddAutoSequencePeriod: previous sequence slot at {previousSequenceSlot.DateTimeRange} has no end date/time, which is unexpected");
                }
                if ((null != previousSequenceSlot) && (null != nextSequenceSlot)
                  && (Bound.Compare<DateTime> (nextSequenceSlot.BeginDateTime, previousSequenceSlot.EndDateTime) < 0)) {
                  log.Fatal ($"AddAutoSequencePeriod: next sequence slot at {nextSequenceSlot.DateTimeRange} starts before previous sequence slot at {previousSequenceSlot.DateTimeRange}, which is unexpected");
                }
              }
              Debug.Assert ((null == previousSequenceSlot) || previousSequenceSlot.EndDateTime.HasValue);
              Debug.Assert ((null == previousSequenceSlot) || (null == nextSequenceSlot) || (previousSequenceSlot.EndDateTime.Value <= nextSequenceSlot.BeginDateTime));

              // Adjust currentBegin if it intersects nextSequenceSlot
              if ((null != nextSequenceSlot)
                  && (nextSequenceSlot.BeginDateTime <= currentBegin)) {
                AdjustNextBegin (previousSequenceSlot, nextSequenceSlot);
                if (!nextSequenceSlot.EndDateTime.HasValue) {
                  log.Debug ("AddAutoSequencePeriod: next sequence slot has an end oo => new currentBegin would be oo, break");
                  break;
                }
                else {
                  currentBegin = nextSequenceSlot.EndDateTime.Value;
                  continue;
                }
              }

              // Update remainingAutoSequences / re-adjust nextAutoSequence compared to currentBegin
              remainingAutoSequences =
                remainingAutoSequences
                .SkipWhile (autoSequence => Bound.Compare<DateTime> (autoSequence.End, currentBegin) <= 0);
              nextAutoSequence = remainingAutoSequences.FirstOrDefault ();
              Debug.Assert ((null == nextAutoSequence)
                            || (Bound.Compare<DateTime> (currentBegin, nextAutoSequence.End) < 0));

              // Re-attach nextAutoSequence which was detached
              if (null != nextAutoSequence) {
                ModelDAOHelper.DAOFactory.AutoSequenceDAO.Lock (nextAutoSequence);
              }

              // In case currentBegin intersects nextAutoSequence, process it
              if ((null != nextAutoSequence)
                  && (nextAutoSequence.Begin <= currentBegin)) {
                DateTime currentEnd = range.Upper.Value;
                if (nextAutoSequence.End.HasValue
                    && (nextAutoSequence.End.Value < currentEnd)) {
                  currentEnd = nextAutoSequence.End.Value;
                }
                Debug.Assert (currentBegin < currentEnd);
                if (null != nextAutoSequence.Sequence) {
                  ISequenceSlot newSequenceSlot = AddAutoOnlySequenceSlot (previousSequenceSlot, nextAutoSequence, currentBegin, currentEnd);
                  if (newSequenceSlot != previousSequenceSlot) { // A new sequence slot was created (the references are different)
                    Debug.Assert ((null == previousSequenceSlot)
                                  || (null == newSequenceSlot)
                                  || (NullableDateTime.Compare (previousSequenceSlot.EndDateTime, newSequenceSlot.BeginDateTime) <= 0));
                    AdjustNextBegin (previousSequenceSlot, newSequenceSlot);
                    previousSequenceSlot = newSequenceSlot;
                  }
                  Debug.Assert ((null == previousSequenceSlot)
                                || (null == nextSequenceSlot)
                                || (NullableDateTime.Compare (previousSequenceSlot.EndDateTime, nextSequenceSlot.BeginDateTime) <= 0));
                  AdjustNextBegin (previousSequenceSlot, nextSequenceSlot);
                }
                else { // null != nextAutoSequence.Operation
                  Debug.Assert (null != nextAutoSequence.Operation);
                  m_sequenceDetection.AddOperation (nextAutoSequence.Operation, new UtcDateTimeRange (currentBegin, currentEnd));
                }
                currentBegin = currentEnd;
                continue;
              }

              Debug.Assert (currentBegin < range.Upper.Value);
              // No intersection with SequenceSlots or AutoSequence
              // => add a no sequence period
              {
                DateTime currentEnd = range.Upper.Value;
                if ((null != nextAutoSequence)
                    && (nextAutoSequence.Begin < currentEnd)) {
                  currentEnd = nextAutoSequence.Begin;
                }
                if ((null != nextSequenceSlot)
                    && (nextSequenceSlot.BeginDateTime < currentEnd)) {
                  Debug.Assert (nextSequenceSlot.BeginDateTime.HasValue);
                  currentEnd = nextSequenceSlot.BeginDateTime.Value;
                }
                Debug.Assert (currentBegin < currentEnd);
                log.Debug ($"AddAutoSequencePeriod: no intersection => add a no sequence period between {currentBegin} and {currentEnd}");
                ISequenceSlot newNoSequenceSlot = AddNoSequencePeriod (previousSequenceSlot, currentBegin, currentEnd);
                if (previousSequenceSlot != newNoSequenceSlot) { // A new sequence slot was created (the references are different)
                  Debug.Assert ((null == previousSequenceSlot)
                                || (null == newNoSequenceSlot)
                                || (Bound.Compare<DateTime> (previousSequenceSlot.EndDateTime, newNoSequenceSlot.BeginDateTime) <= 0));
                  AdjustNextBegin (previousSequenceSlot, newNoSequenceSlot);
                  previousSequenceSlot = newNoSequenceSlot;
                }
                Debug.Assert ((null == previousSequenceSlot)
                              || (null == nextSequenceSlot)
                              || (Bound.Compare<DateTime> (previousSequenceSlot.EndDateTime, nextSequenceSlot.BeginDateTime) <= 0));
                AdjustNextBegin (previousSequenceSlot, nextSequenceSlot);

                // Process is completed
                // => move currentBegin forward
                currentBegin = currentEnd;
              }
            } // End while
          }

          transaction.Commit ();
        } // End of transaction

        using (IDAOTransaction transaction = session.BeginTransaction ("Detection.CleanAutoSequence",
                                                                       TransactionLevel.ReadCommitted)) // Serializable is not really necessary here
        { // Second transaction: clean-up
          transaction.SynchronousCommitOption = SynchronousCommit.Off;
          ModelDAOHelper.DAOFactory.AutoSequenceDAO.DeleteBefore (m_machineModule,
                                                                  range.Upper.Value);
          transaction.Commit ();
        }
      }
    }

    /// <summary>
    /// Adjust the next begin property
    /// <item>in case the next sequence slot comes before the existing nextbegin</item>
    /// <item>in case nextbegin is still unknown</item>
    /// </summary>
    /// <param name="previousSequenceSlot"></param>
    /// <param name="nextSequenceSlot"></param>
    void AdjustNextBegin (ISequenceSlot previousSequenceSlot, ISequenceSlot nextSequenceSlot)
    {
      m_sequenceDetection.AdjustNextBegin (previousSequenceSlot, nextSequenceSlot);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="previousSlot"></param>
    /// <param name="autoSequence">whose associated sequence is not null</param>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    /// <returns>Sequence slot that corresponds to the new auto-only period</returns>
    ISequenceSlot AddAutoOnlySequenceSlot (ISequenceSlot previousSlot, IAutoSequence autoSequence, DateTime begin, DateTime end)
    {
      Debug.Assert (null != autoSequence);
      Debug.Assert (null != autoSequence.Sequence);
      Debug.Assert (autoSequence.Sequence.AutoOnly);
      Debug.Assert ((null == previousSlot) || (previousSlot.EndDateTime.HasValue));
      Debug.Assert ((null == previousSlot) || (previousSlot.EndDateTime.Value <= begin));

      if ((null != previousSlot)
          && autoSequence.Sequence.Equals (previousSlot.Sequence)) {
        if (previousSlot.EndDateTime.Value.Equals (begin)) {
          log.DebugFormat ("AddAutoOnlySequenceSlot: " +
                           "extend previous slot {0} to {1} because of overlap",
                           previousSlot, end);
          return m_sequenceDetection.MakeSequenceSlotLonger (previousSlot, end);
        }

        // Else previousSlot.End < begin
        Debug.Assert (Bound.Compare<DateTime> (previousSlot.EndDateTime, begin) < 0);

        // - Config values
        TimeSpan maxSequenceGap = Lemoine.Info.ConfigSet
        .Get<TimeSpan> (ConfigKeys.GetAnalysisConfigKey (AnalysisConfigKey.SequenceGapLimit)); 

        // Check if previousSlot may be extended
        // - if the autoSequence begin is not after the lastSequenceSlot end
        // - no big gap
        if ((Bound.Compare<DateTime> (autoSequence.Begin, previousSlot.EndDateTime) < 0)
            && (begin.Subtract (previousSlot.EndDateTime.Value) <= maxSequenceGap)) {
          // Check there is no operation slot between the two sequence slots
          bool existsOperationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
            .ExistsDifferentOperationBetween (m_machineModule.MonitoredMachine,
                                              previousSlot.EndDateTime.Value,
                                              begin,
                                              autoSequence.Sequence.Operation);
          if (!existsOperationSlot) {
            log.DebugFormat ("AddAutoOnlySequenceSlot: " +
                             "extend the existing sequence slot {0} to {1} because of no big gap",
                             previousSlot, end);
            return m_sequenceDetection.MakeSequenceSlotLonger (previousSlot, end);
          }
        }
      } // end if null != previousSlot

      // The previous sequence slot has not been extended, create a new sequence slot instead
      if ((null != previousSlot)
          && (!previousSlot.NextBegin.HasValue
              || (Bound.Compare<DateTime> (begin, previousSlot.NextBegin.Value) < 0))) {
        Debug.Assert (Bound.Compare<DateTime> (previousSlot.BeginDateTime, begin) < 0);
        Debug.Assert (previousSlot.EndDateTime.HasValue);
        Debug.Assert (Bound.Compare<DateTime> (previousSlot.EndDateTime.Value, begin) <= 0);
        previousSlot.NextBegin = begin;
      }
      ISequenceSlot newSequenceSlot = m_sequenceDetection.AddNewSequenceSlot (autoSequence.Sequence, new UtcDateTimeRange (begin, end));
      return newSequenceSlot;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="lastSequenceSlot"></param>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    /// <returns>Sequence slot that corresponds to the [begin, end] period</returns>
    ISequenceSlot AddNoSequencePeriod (ISequenceSlot lastSequenceSlot, DateTime begin, DateTime end)
    {
      log.DebugFormat ("AddNoSequencePeriod: " +
                       "lastSequenceSlot={0} begin-end={1}-{2}",
                       lastSequenceSlot, begin, end);

      if ((null != lastSequenceSlot)
          && (null == lastSequenceSlot.Sequence)
          && (lastSequenceSlot.EndDateTime.Equals (begin))) {
        log.DebugFormat ("AddNoSequencePeriod: " +
                         "make the no sequence slot {0} longer to {1}",
                         lastSequenceSlot, end);
        return m_sequenceDetection.MakeSequenceSlotLonger (lastSequenceSlot, end);
      }
      else {
        log.DebugFormat ("AddNoSequencePeriod: " +
                         "add a new 'no sequence' slot from {0} to {1}",
                         begin, end);
        if ((null != lastSequenceSlot)
            && (!lastSequenceSlot.NextBegin.HasValue
                || (begin < lastSequenceSlot.NextBegin.Value))) {
          Debug.Assert (lastSequenceSlot.BeginDateTime < begin);
          Debug.Assert (lastSequenceSlot.EndDateTime.HasValue);
          Debug.Assert (lastSequenceSlot.EndDateTime.Value <= begin);
          lastSequenceSlot.NextBegin = begin;
        }
        return m_sequenceDetection.AddNewSequenceSlot (null, new UtcDateTimeRange (begin, end));
      }
    }

    #endregion // Methods
  }
}
