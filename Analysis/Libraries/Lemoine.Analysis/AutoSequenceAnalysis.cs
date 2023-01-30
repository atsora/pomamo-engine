// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.ExceptionManagement;
using Lemoine.Info;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System.Threading;

namespace Lemoine.Analysis
{
  /// <summary>
  /// Description of AutoSequenceAnalysis.
  /// </summary>
  internal class AutoSequenceAnalysis : Lemoine.Threading.IChecked
  {
    /// <summary>
    /// Maximum number of auto-sequences that are processed in the same time
    /// </summary>
    static readonly string MAX_NUMBER_OF_AUTO_SEQUENCES_KEY = "Analysis.Activity.AutoSequences.MaxNumber";
    static readonly int MAX_NUMBER_OF_AUTO_SEQUENCES_DEFAULT = 400;

    /// <summary>
    /// Number of attempt to complete a transaction in case of a serialization failure
    /// </summary>
    static readonly string NB_ATTEMPT_SERIALIZATION_FAILURE_KEY = "Analysis.Activity.NbAttemptSerializationFailure";
    static readonly int NB_ATTEMPT_SERIALIZATION_FAILURE_DEFAULT = 2;

    #region Members
    readonly MonitoredMachineActivityAnalysis m_machineActivityAnalysis;
    readonly IDictionary<int, AutoSequenceMachineModuleAnalysis> m_byMachineModuleAnalysis = new Dictionary<int, AutoSequenceMachineModuleAnalysis> ();
    #endregion // Members

    ILog log = LogManager.GetLogger (typeof (AutoSequenceAnalysis).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="machineActivityAnalysis">Not null (including its associated machine)</param>
    public AutoSequenceAnalysis (MonitoredMachineActivityAnalysis machineActivityAnalysis)
    {
      Debug.Assert (null != machineActivityAnalysis);
      Debug.Assert (null != machineActivityAnalysis.MonitoredMachine);

      m_machineActivityAnalysis = machineActivityAnalysis;

      log = LogManager.GetLogger (string.Format ("{0}.{1}",
                                                this.GetType ().FullName,
                                                machineActivityAnalysis.MonitoredMachine.Id));
    }
    #endregion // Constructors

    #region IChecked implementation
    /// <summary>
    /// Lemoine.Threading.IChecked implementation
    /// </summary>
    public void SetActive ()
    {
      m_machineActivityAnalysis.SetActive ();
    }

    /// <summary>
    /// <see cref="Lemoine.Threading.IChecked" />
    /// </summary>
    public void PauseCheck ()
    {
      m_machineActivityAnalysis.PauseCheck ();
    }

    /// <summary>
    /// <see cref="Lemoine.Threading.IChecked" />
    /// </summary>
    public void ResumeCheck ()
    {
      m_machineActivityAnalysis.ResumeCheck ();
    }
    #endregion // IChecked implementation

    #region ManageAutoSequence part
    /// <summary>
    /// Manage the auto-sequence periods
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="facts">Previously retrieved facts to check if they can be re-used</param>
    /// <param name="maxAutoSequenceAnalysisDateTime"></param>
    /// <param name="minTimePerMachineModule"></param>
    internal void ManageAutoSequencePeriods (CancellationToken cancellationToken, IList<IFact> facts, DateTime maxAutoSequenceAnalysisDateTime, TimeSpan minTimePerMachineModule, int? numberOfItems = null)
    {
      var processValidityChecker = new ProcessValidityChecker (m_machineActivityAnalysis, maxAutoSequenceAnalysisDateTime, minTimePerMachineModule, log);

      // This is for an optimization in case the auto-sequence activity is driven by the machine
      IList<IAutoSequencePeriod> proposedMachineAutoSequencePeriods = new List<IAutoSequencePeriod> ();
      if (null != facts) {
        foreach (IFact fact in facts) {
          proposedMachineAutoSequencePeriods.Add (fact);
        }
      }

      // Do not start a session here because the session may be closed
      // in case a transaction is replayed after a serialization failure

      // For each machine module, process the auto-sequence
      // between analysisStatus.AutoSequenceDateTime and DetectionTimeStamp
      foreach (IMachineModule machineModule in m_machineActivityAnalysis.MonitoredMachine.MachineModules) {
        SetActive ();

        processValidityChecker.ResetStartDateTime (); // To give enough time to each machine module

        if (!processValidityChecker.IsValid ()) {
          log.InfoFormat ("ManageAutoSequencePeriods: " +
                          "requested to exit by processValidityChecker");
          return;
        }

        if (cancellationToken.IsCancellationRequested) {
          log.Info ("ManageAutoSequencePeriods: cancellation was requested");
          return;
        }

        ManageAutoSequencePeriodsByMachineModule (machineModule, ref proposedMachineAutoSequencePeriods, processValidityChecker, numberOfItems);
      }
    }

    void ManageAutoSequencePeriodsByMachineModule (IMachineModule machineModule, ref IList<IAutoSequencePeriod> proposedMachineAutoSequencePeriods, ProcessValidityChecker processValidityChecker, int? numberOfItems = null)
    {
      SetActive ();

      // - Get the date/time range to process
      IMachineModuleAnalysisStatus analysisStatus;
      UtcDateTimeRange range;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        IAcquisitionState detectionTimeStamp = null;
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Analysis.AcquisitionStatus",
                                                                               TransactionLevel.ReadCommitted)) {
          detectionTimeStamp = ModelDAOHelper.DAOFactory.AcquisitionStateDAO.GetAcquisitionState (machineModule, AcquisitionStateKey.Detection);
        } // auto commit because read-only
        if (null == detectionTimeStamp) {
          if (log.IsInfoEnabled) {
            log.Info ($"ManageAutoSequencePeriodsByMachineModule: no detectiontimestamp data for machine module {machineModule.Id}");
          }
          return;
        }

        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Analysis.AutoSequencePeriods.MachineModuleStatusInitialization",
                                                                               TransactionLevel.ReadCommitted)) {
          analysisStatus = ModelDAOHelper.DAOFactory.MachineModuleAnalysisStatusDAO
            .FindById (machineModule.Id);
        } // auto commit because read-only

        DateTime begin = analysisStatus.AutoSequenceAnalysisDateTime;
        DateTime end = detectionTimeStamp.DateTime;
        range = new UtcDateTimeRange (begin, end);
      }

      if (range.IsEmpty ()) {
        if (log.IsDebugEnabled) {
          log.DebugFormat ("ManageAutoSequencePeriodsByMachineModule: range is empty for machine module id {0} (nothing to process)",
            machineModule.Id);
        }
        return;
      }

      IEnumerable<IAutoSequencePeriod> autoSequencePeriods;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Analysis.GetAutoSequencePeriods",
                                                                               TransactionLevel.ReadCommitted)) {
          autoSequencePeriods = GetAutoSequencePeriods (proposedMachineAutoSequencePeriods,
                                                        machineModule,
                                                        range,
                                                        numberOfItems);
        } // auto commit because read-only
      }
      if (null == autoSequencePeriods) {
        return;
      }
      else if (!autoSequencePeriods.Any ()) {
        return;
      }
      proposedMachineAutoSequencePeriods = autoSequencePeriods.ToList ();

      // - Pre-load all the IAutoSequence in the autoSequencePeriods range
      //   Note: the IAutoSequence are not updated in the same time because they are updated by the same thread
      Debug.Assert (autoSequencePeriods.Any ());
      UtcDateTimeRange autoSequencesRange = GetRangeWithOnlyAutoSequencePeriods (autoSequencePeriods);
      var autoSequences = GetAutoSequences (machineModule, autoSequencesRange);

      // - To process the auto-sequence periods, you can group them together the auto-sequence periods
      //   that intersect the same IAutoSequence
      var machineModuleAnalysis = GetAnalysisByMachineModule (machineModule);
      int nbAttemptSerializationFailure = (int)ConfigSet.LoadAndGet (NB_ATTEMPT_SERIALIZATION_FAILURE_KEY,
                                                                     NB_ATTEMPT_SERIALIZATION_FAILURE_DEFAULT);
      IEnumerable<IAutoSequencePeriod> currentAutoSequencePeriods;
      IEnumerable<IAutoSequence> matchingAutoSequences;
      while (GetNextAutoSequencePeriods (machineModule, autoSequencesRange,
                                         ref autoSequencePeriods,
                                         ref autoSequences,
                                         out currentAutoSequencePeriods,
                                         out matchingAutoSequences)) {
        SetActive ();

        // - Process the group of auto-sequence periods and store the last date/time
        Debug.Assert (currentAutoSequencePeriods.Any ());
        UtcDateTimeRange currentRange = new UtcDateTimeRange (currentAutoSequencePeriods.First ().Begin,
                                                              currentAutoSequencePeriods.Last ().End);
        bool isAutoSequence = currentAutoSequencePeriods.First ().AutoSequence;
        Debug.Assert (!currentRange.IsEmpty ());
        if (log.IsDebugEnabled) {
          log.DebugFormat ("ManageAutoSequencePeriodsByMachineModule: " +
                           "process auto-sequence period {0} {1}",
                           currentRange, isAutoSequence);
        }
        if (!ApplyAutoSequencePeriodWithAttempts (machineModuleAnalysis, currentRange, isAutoSequence, matchingAutoSequences, analysisStatus, nbAttemptSerializationFailure, processValidityChecker)) {
          if (log.IsDebugEnabled) {
            log.DebugFormat ("ManageAutoSequencePeriodsByMachineModule: ApplyAutoSequencePeriodWithAttempds was interrupted");
          }
          return;
        }

        if (!processValidityChecker.IsValid ()) {
          log.Info ("ManageAutoSequencePeriodsByMachineModule: " +
                    "interrupt the analysis because it was requested by the processValidityChecker");
          return;
        }
      } // while
    }

    /// <summary>
    /// Get the range with only auto-sequence periods
    /// 
    /// An empty range is returned if there is no such period
    /// </summary>
    /// <param name="autoSequencePeriods"></param>
    /// <returns></returns>
    UtcDateTimeRange GetRangeWithOnlyAutoSequencePeriods (IEnumerable<IAutoSequencePeriod> autoSequencePeriods)
    {
      Debug.Assert (autoSequencePeriods.Any ());
      IEnumerable<IAutoSequencePeriod> trimmedAutoSequencePeriods =
        Trim (autoSequencePeriods);
      if (!trimmedAutoSequencePeriods.Any ()) {
        return new UtcDateTimeRange ();
      }
      else {
        return
          new UtcDateTimeRange (trimmedAutoSequencePeriods.First ().Begin,
                               trimmedAutoSequencePeriods.Last ().End);
      }
    }

    IEnumerable<IAutoSequence> GetAutoSequences (IMachineModule machineModule,
      UtcDateTimeRange range)
    {
      if (range.IsEmpty ()) {
        return new List<IAutoSequence> ();
      }
      else {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Analysis.GetAutoSequences",
                                                                                TransactionLevel.ReadCommitted)) {
            return ModelDAOHelper.DAOFactory.AutoSequenceDAO
              .FindAllBetween (machineModule, range);
          } // auto commit because read-only
        }
      }
    }

    bool ApplyAutoSequencePeriodWithAttempts (AutoSequenceMachineModuleAnalysis machineModuleAnalysis, UtcDateTimeRange range, bool isAutoSequence, IEnumerable<IAutoSequence> matchingAutoSequences, IMachineModuleAnalysisStatus analysisStatus, int nbAttemptSerializationFailure, ProcessValidityChecker processValidityChecker)
    {
      for (int attempt = 0; attempt < nbAttemptSerializationFailure; attempt++) { // Limit the number of attempts in case of serializable failure
        SetActive ();

        // - Try not to spend too much time in this method
        if (0 != attempt) {
          if (!processValidityChecker.IsValid ()) {
            log.Info ("ApplyAutoSequencePeriodWithAttempts: " +
                      "interrupt the analysis because it was requested by the processValidityChecker");
            return false; // Interrupted
          }
        }

        bool result = ApplyAutoSequencePeriod (machineModuleAnalysis, range, isAutoSequence, matchingAutoSequences, analysisStatus, attempt);
        if (result) {
          return true;
        }
      } // for

      return true; // Try another machine module then
    }


    bool ApplyAutoSequencePeriod (AutoSequenceMachineModuleAnalysis machineModuleAnalysis, UtcDateTimeRange range, bool isAutoSequence, IEnumerable<IAutoSequence> matchingAutoSequences, IMachineModuleAnalysisStatus analysisStatus, int attempt)
    {
      try {
        // Note: this is done in two transactions
        //       because it is ok to play AddAutoSequencePeriod twice
        // - Process
        if (isAutoSequence) {
          machineModuleAnalysis.AddAutoSequencePeriod (range, matchingAutoSequences);
        }

        // - Record the last processed date/time
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (IDAOTransaction transaction = session.BeginTransaction ("Analysis.AutoSequencePeriods.Reload",
                                                                         TransactionLevel.ReadCommitted)) // Serializable is not necessary here
          {
            transaction.SynchronousCommitOption = SynchronousCommit.Off; // Because this do not drive to any risk of loosing some data

            // - Re-associate analysisStatus to this session
            //   The upgrade lock is not absolutely necessary here. So do not use it.
            ModelDAOHelper.DAOFactory.MachineModuleAnalysisStatusDAO
              .Lock (analysisStatus);

            Debug.Assert (range.Upper.HasValue);
            analysisStatus.AutoSequenceAnalysisDateTime = range.Upper.Value;
            ModelDAOHelper.DAOFactory.MachineModuleAnalysisStatusDAO.MakePersistent (analysisStatus);

            transaction.Commit ();
          }
        }
        return true; // Transaction ok and completed
      }
      catch (Exception ex) {
        if (ExceptionTest.IsStale (ex)) {
          log.Warn ($"ApplyAutoSequencePeriod: stale object state exception with attempt {attempt}", ex);
          return false; // Not completed
        }
        else if (ExceptionTest.IsTemporary (ex)) {
          // For example, TransactionSerializationFailure
          log.Warn ($"ApplyAutoSequencePeriod: temporary failure with attempt {attempt}", ex);
          return false; // Not completed
        }
        else {
          log.Exception (ex, "ApplyAutoSequencePeriod");
          throw;
        }
      } // Try catch
    }

    /// <summary>
    /// Trim the auto-sequence periods from all the not auto-sequence periods
    /// </summary>
    /// <param name="autoSequencePeriods"></param>
    /// <returns></returns>
    internal static IEnumerable<IAutoSequencePeriod> Trim (IEnumerable<IAutoSequencePeriod> autoSequencePeriods)
    {
      return autoSequencePeriods
        .SkipWhile (period => !period.AutoSequence)
        .Reverse ()
        .SkipWhile (period => !period.AutoSequence)
        .Reverse ();
    }

    /// <summary>
    /// Get the range of auto-sequence periods that can be processed simultaneously
    /// 
    /// This method was written to optimize the process
    /// 
    /// Note that the case an auto-sequence is long enough and there is a long break between two auto-sequence periods
    /// is not particularly managed. In this case, the sequence slot will be extended, whichever the break time is.
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="autoSequencesRange"></param>
    /// <param name="remainingAutoSequencePeriods"></param>
    /// <param name="remainingAutoSequences"></param>
    /// <param name = "autoSequencePeriods"></param>
    /// <param name = "autoSequences"></param>
    /// <returns></returns>
    internal bool GetNextAutoSequencePeriods (IMachineModule machineModule, UtcDateTimeRange autoSequencesRange,
                                              ref IEnumerable<IAutoSequencePeriod> remainingAutoSequencePeriods,
                                              ref IEnumerable<IAutoSequence> remainingAutoSequences,
                                              out IEnumerable<IAutoSequencePeriod> autoSequencePeriods,
                                              out IEnumerable<IAutoSequence> autoSequences)
    {
      Debug.Assert (null != machineModule);

      autoSequencePeriods = null;
      autoSequences = null;

      // - Get the first auto-sequence period
      IAutoSequencePeriod firstAutoSequencePeriod = remainingAutoSequencePeriods.FirstOrDefault ();
      if (null == firstAutoSequencePeriod) {
        autoSequencePeriods = remainingAutoSequencePeriods;
        autoSequences = new List<IAutoSequence> ();
        return false;
      }

      // - Process the case when the first auto-sequence period is a not auto-sequence period
      if (!firstAutoSequencePeriod.AutoSequence) {
        autoSequencePeriods = remainingAutoSequencePeriods
          .TakeWhile (period => !period.AutoSequence);
        autoSequences = new List<IAutoSequence> ();
        remainingAutoSequencePeriods = remainingAutoSequencePeriods
          .SkipWhile (period => !period.AutoSequence);
        return true;
      }
      // Now a set of consecutive really auto-sequence periods are processed

      // - Skip in remainingAutoSequences, all the auto-sequences that are left of firstAutoSequencePeriod
      remainingAutoSequences =
        remainingAutoSequences
        .SkipWhile (autoSequence => autoSequence.Range.IsStrictlyLeftOf (firstAutoSequencePeriod.Range));

      // - Get the auto-sequences that overlap the first auto-sequence period
      IEnumerable<IAutoSequence> overlappingAutoSequences =
        remainingAutoSequences
        .TakeWhile (autoSequence => autoSequence.Range.Overlaps (firstAutoSequencePeriod.Range));
      if (overlappingAutoSequences.Any ()) {
        // There are some overlapping auto-sequences
        // => the auto-sequence periods can be extended until the end of the last overlapping auto-sequence
        IAutoSequence lastOverlappingAutoSequence =
          overlappingAutoSequences.Last ();
        autoSequencePeriods = remainingAutoSequencePeriods
          .TakeWhile (period => period.AutoSequence && !period.Range.IsStrictlyRightOf (lastOverlappingAutoSequence.Range));
        remainingAutoSequencePeriods = remainingAutoSequencePeriods
          .SkipWhile (period => period.AutoSequence && !period.Range.IsStrictlyRightOf (lastOverlappingAutoSequence.Range));
        UtcDateTimeRange autoSequencePeriodRange = new UtcDateTimeRange (autoSequencePeriods.First ().Begin,
                                                                         autoSequencePeriods.Last ().End);
        autoSequences = remainingAutoSequences
          .TakeWhile (autoSequence => autoSequence.Range.Overlaps (autoSequencePeriodRange));
        return true;
      }
      else { // No overlapping auto-sequence
        IAutoSequence nextAutoSequence =
          remainingAutoSequences.FirstOrDefault ();
        // Return all the auto-sequence periods that are left of nextAutoSequence
        if (null == nextAutoSequence) {
          Debug.Assert (!autoSequencesRange.IsEmpty ()); // Else firstAutoSequencePeriod.AutoSequence is false and the function would have already return
          Debug.Assert (autoSequencesRange.Upper.HasValue); // Because made of autoSequencePeriods
          using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
            using (var transaction = session.BeginReadOnlyTransaction ("Analysis.AutoSequenceAnalysis.AutoSequenceAfter", TransactionLevel.ReadCommitted)) {
              nextAutoSequence = ModelDAOHelper.DAOFactory.AutoSequenceDAO
                .GetFirstAfter (machineModule, autoSequencesRange.Upper.Value);
            }
          }
          if (null == nextAutoSequence) {
            // return false if because some auto-sequences may be in database later
            if (log.IsDebugEnabled) {
              log.Debug ($"GetNextAutoSequencePeriods: stop the process because there is no auto-sequence in the future, after {autoSequencesRange}");
            }
            return false;
          }
          else {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetNextAutoSequencePeriods: there are auto-sequences after {autoSequencesRange} => continue");
            }
          }
        }

        autoSequencePeriods = remainingAutoSequencePeriods
          .TakeWhile (period => period.AutoSequence && period.Range.IsStrictlyLeftOf (nextAutoSequence.Range));
        remainingAutoSequencePeriods = remainingAutoSequencePeriods
          .SkipWhile (period => period.AutoSequence && period.Range.IsStrictlyLeftOf (nextAutoSequence.Range));
        autoSequences = new List<IAutoSequence> ();
        return true;
      }
    }

    IList<IAutoSequencePeriod> GetAutoSequencePeriods (IList<IAutoSequencePeriod> proposedMachineAutoSequencePeriods,
                                                       IMachineModule machineModule,
                                                       UtcDateTimeRange range,
                                                       int? numberOfItems = null)
    {
      Debug.Assert (!range.IsEmpty ());

      IList<IAutoSequencePeriod> retrievedAutoSequencePeriods = null;
      switch (machineModule.AutoSequenceActivity) {
      case MachineModuleAutoSequenceActivity.None:
        break;
      case MachineModuleAutoSequenceActivity.Machine:
        // Check if you can re-use proposedAutoSequencePeriods (there is an intersection)
        if ((null != proposedMachineAutoSequencePeriods)
            && (1 <= proposedMachineAutoSequencePeriods.Count)) {
          UtcDateTimeRange proposedMachineAutoSequencePeriodsRange =
            new UtcDateTimeRange (proposedMachineAutoSequencePeriods[0].Begin,
                                  proposedMachineAutoSequencePeriods[proposedMachineAutoSequencePeriods.Count - 1].End);
          if (proposedMachineAutoSequencePeriodsRange.ContainsRange (range)) {
            retrievedAutoSequencePeriods = proposedMachineAutoSequencePeriods;
          }
        }
        if (null == retrievedAutoSequencePeriods) { // Retrieve some facts
          var maxNumberOfAutoSequences1 = numberOfItems ?? ConfigSet.LoadAndGet (MAX_NUMBER_OF_AUTO_SEQUENCES_KEY,
                                                                                 MAX_NUMBER_OF_AUTO_SEQUENCES_DEFAULT);
          retrievedAutoSequencePeriods = ModelDAOHelper.DAOFactory.FactDAO
            .FindAllAutoSequencePeriodsBetween (machineModule.MonitoredMachine,
                                                range,
                                                maxNumberOfAutoSequences1);
        }
        break;
      case MachineModuleAutoSequenceActivity.MachineModule:
        var maxNumberOfAutoSequences2 = numberOfItems ?? ConfigSet.LoadAndGet (MAX_NUMBER_OF_AUTO_SEQUENCES_KEY,
                                                                               MAX_NUMBER_OF_AUTO_SEQUENCES_DEFAULT);
        retrievedAutoSequencePeriods = ModelDAOHelper.DAOFactory.MachineModuleActivityDAO
          .FindAllAutoSequencePeriodsBetween (machineModule,
                                              range,
                                              maxNumberOfAutoSequences2);
        break;
      default:
        throw new Exception ("Invalid value for MachineModuleAutoSequenceActivity");
      }

      if (null == retrievedAutoSequencePeriods) {
        // Nothing to compact
        // Return null because nothing was retrieved
        return null;
      }

      // Compact the list
      IList<IAutoSequencePeriod> autoSequencePeriods = new List<IAutoSequencePeriod> ();
      IAutoSequencePeriod lastAutoSequencePeriod = null;
      foreach (IAutoSequencePeriod retrievedAutoSequencePeriod in retrievedAutoSequencePeriods) {
        if (retrievedAutoSequencePeriod.Range.IsStrictlyLeftOf (range)) {
          continue;
        }
        if (retrievedAutoSequencePeriod.Range.IsStrictlyRightOf (range)) {
          break;
        }
        Debug.Assert (retrievedAutoSequencePeriod.Range.Overlaps (range));
        UtcDateTimeRange intersection = new UtcDateTimeRange (retrievedAutoSequencePeriod.Range.Intersects (range));
        if ((null != lastAutoSequencePeriod)
            && (lastAutoSequencePeriod.AutoSequence == retrievedAutoSequencePeriod.AutoSequence)
            && Bound.Equals<DateTime> (lastAutoSequencePeriod.End, intersection.Lower)) {
          // Extend lastAutoSequencePeriod
          Debug.Assert (intersection.Upper.HasValue);
          lastAutoSequencePeriod.End = intersection.Upper.Value;
        }
        else {
          lastAutoSequencePeriod = new AutoSequencePeriod (intersection,
                                                           retrievedAutoSequencePeriod.AutoSequence);
          autoSequencePeriods.Add (lastAutoSequencePeriod);
        }
      }
      return autoSequencePeriods;
    }

    AutoSequenceMachineModuleAnalysis GetAnalysisByMachineModule (IMachineModule machineModule)
    {
      AutoSequenceMachineModuleAnalysis machineModuleAnalysis;
      if (!m_byMachineModuleAnalysis.TryGetValue (machineModule.Id, out machineModuleAnalysis)) {
        machineModuleAnalysis = new AutoSequenceMachineModuleAnalysis (m_machineActivityAnalysis.DetectionAnalysis.OperationDetection, machineModule, m_machineActivityAnalysis.RestrictedTransactionLevel, this);
        m_byMachineModuleAnalysis[machineModule.Id] = machineModuleAnalysis;
      }
      return machineModuleAnalysis;
    }
    #endregion // ManageAutoSequence part

    class AutoSequencePeriod : IAutoSequencePeriod
    {
      DateTime m_begin;
      DateTime m_end;
      readonly bool m_autoSequence;

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="begin"></param>
      /// <param name="end"></param>
      /// <param name="autoSequence"></param>
      public AutoSequencePeriod (DateTime begin, DateTime end, bool autoSequence)
      {
        m_begin = begin;
        m_end = end;
        m_autoSequence = autoSequence;
      }

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="range">with a finite bounds</param>
      /// <param name="autoSequence"></param>
      public AutoSequencePeriod (UtcDateTimeRange range, bool autoSequence)
      {
        Debug.Assert (range.Lower.HasValue);
        Debug.Assert (range.Upper.HasValue);
        m_begin = range.Lower.Value;
        m_end = range.Upper.Value;
        m_autoSequence = autoSequence;
      }

      /// Begin date/time
      public DateTime Begin
      {
        get { return m_begin; }
      }

      /// <summary>
      /// End date/time
      /// </summary>
      // disable once ConvertToAutoProperty
      public DateTime End
      {
        get { return m_end; }
        set { m_end = value; }
      }

      /// <summary>
      /// Range [Begin,End)
      /// </summary>
      public UtcDateTimeRange Range
      {
        get { return new UtcDateTimeRange (this.Begin, this.End); }
      }

      /// <summary>
      /// Auto-sequence ?
      /// </summary>
      public bool AutoSequence
      {
        get { return m_autoSequence; }
      }
    }

  }
}
