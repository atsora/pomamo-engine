// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Cache;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System.Threading.Tasks;
using Lemoine.Collections;

namespace Lemoine.Business.Operation
{
  /// <summary>
  /// Description of CycleProgress.
  /// </summary>
  public sealed class CycleProgress : IRequest<CycleProgressResponse>
  {
    class InvalidCycle : Exception
    { }

    static readonly string EFFECTIVE_OPERATION_MAX_AGE_KEY = "Business.CycleProgress.EffectiveOperationMaxAge";
    static readonly TimeSpan EFFECTIVE_OPERATION_MAX_AGE_DEFAULT = TimeSpan.FromDays (7);

    static readonly string SEQUENCE_SLOT_START_VS_OPERATION_CYCLE_MARGIN_KEY = "Business.CycleProgress.SequenceSlotStartVsOperationCycleMargin";
    static readonly TimeSpan SEQUENCE_SLOT_START_VS_OPERATION_CYCLE_MARGIN_DEFAULT = TimeSpan.FromSeconds (4);

    readonly ILog log = LogManager.GetLogger (typeof (CycleProgress).FullName);

    #region Getters / Setters
    /// <summary>
    /// Reference to the machine
    /// </summary>
    public IMonitoredMachine Machine { get; internal set; }

    /// <summary>
    /// Specify if you want to get the cycle progress at a specific date/time
    /// If this is set, consider only the full cycle at this date/time
    /// 
    /// Default: consider the last cycle and now
    /// </summary>
    public DateTime? At { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="machine">not null</param>
    public CycleProgress (IMonitoredMachine machine)
    {
      Debug.Assert (null != machine);

      this.Machine = machine;

      log = LogManager.GetLogger (string.Format ("{0}.{1}",
        typeof (CycleProgress).FullName, machine.Id));
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns>Not null</returns>
    public CycleProgressResponse Get ()
    {
      var response = new CycleProgressResponse ();
      response.MachineModuleCycleProgress = new Dictionary<IMachineModule, CycleProgressByMachineModuleResponse> ();
      response.Warnings = new List<string> ();
      response.Errors = new List<string> ();
      response.Notices = new List<string> ();

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        IOperationSlot operationSlot;
        IOperationCycle operationCycle;
        IOperation operation;
        IEnumerable<ISequence> sequences;
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Business.CycleProgress.Operation")) {
          if (this.At.HasValue) {
            operationCycle = ModelDAOHelper.DAOFactory.OperationCycleDAO
              .GetFirstBeginBefore (this.Machine, this.At.Value);
            if (null == operationCycle) {
              if (log.IsDebugEnabled) {
                log.DebugFormat ("Get: " +
                                 "no operation cycle before {0}",
                                 this.At.Value);
              }
              response.Flags = response.Flags.Add (CycleProgressResponseFlags.NotApplicable);
              return response;
            }
            if (!operationCycle.Full) {
              if (log.IsDebugEnabled) {
                log.DebugFormat ("Get: " +
                                 "cycle that starts before {0} is not full",
                                 this.At.Value);
              }
              response.Flags = response.Flags.Add (CycleProgressResponseFlags.InvalidCycle);
              return response;
            }
            if (!operationCycle.End.HasValue || (operationCycle.End.Value <= this.At.Value)) {
              if (log.IsDebugEnabled) {
                log.DebugFormat ("Get: " +
                                 "cycle that starts before {0} does not overlap it",
                                 this.At.Value);
              }
              response.Flags = response.Flags.Add (CycleProgressResponseFlags.InvalidCycle);
              return response;
            }
            operationSlot = operationCycle.OperationSlot;
          }
          else { // !this.At.HasValue
            var effectiveOperationMaxAge = Lemoine.Info.ConfigSet
              .LoadAndGet<TimeSpan> (EFFECTIVE_OPERATION_MAX_AGE_KEY,
                                     EFFECTIVE_OPERATION_MAX_AGE_DEFAULT);
            operationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
              .GetLastEffective (this.Machine, effectiveOperationMaxAge);
            if ((null != operationSlot) && (null != operationSlot.Operation)) {
              operationCycle = ModelDAOHelper.DAOFactory.OperationCycleDAO
                .GetLastCycle (operationSlot);
              if (null == operationCycle) {
                if (log.IsDebugEnabled) {
                  log.DebugFormat ("Get: " +
                                   "no last operation cycle");
                }
                var lastCycle = ModelDAOHelper.DAOFactory.OperationCycleDAO
                  .GetLast (this.Machine);
                if (null == lastCycle) {
                  if (log.IsDebugEnabled) {
                    log.DebugFormat ("Get: no cycle on this machine");
                  }
                  response.Flags = response.Flags.Add (CycleProgressResponseFlags.NotApplicable);
                }
              } // but ok it can be null
            }
            else {
              if (log.IsDebugEnabled) {
                log.DebugFormat ("Get: " +
                                 "no effective operation right now " +
                                 "=> return an empty response");
              }
              response.Flags = response.Flags.Add (CycleProgressResponseFlags.NoEffectiveOperation);
              var lastCycle = ModelDAOHelper.DAOFactory.OperationCycleDAO
                .GetLast (this.Machine);
              if (null == lastCycle) {
                if (log.IsDebugEnabled) {
                  log.DebugFormat ("Get: no cycle on this machine");
                }
                response.Flags = response.Flags.Add (CycleProgressResponseFlags.NotApplicable);
              }
              return response; // With a null operation
            }
          }
          if ((null == operationSlot) || (null == operationSlot.Operation)) {
            if (log.IsDebugEnabled) {
              log.DebugFormat ("Get: " +
                               "no effective operation right now " +
                               "=> return an empty response");
            }
            response.Flags = response.Flags.Add (CycleProgressResponseFlags.NoEffectiveOperation);
            var lastCycle = ModelDAOHelper.DAOFactory.OperationCycleDAO
              .GetLast (this.Machine);
            if (null == lastCycle) {
              if (log.IsDebugEnabled) {
                log.DebugFormat ("Get: no cycle on this machine");
              }
              response.Flags = response.Flags.Add (CycleProgressResponseFlags.NotApplicable);
            }
            return response; // With a null operation
          }
          log.DebugFormat ("Get: " +
                           "effective operation is {0}",
                           operationSlot.Operation);
          operation = operationSlot.Operation;
          response.Operation = operation;
          response.OperationCycle = operationCycle;
          if (null != operationCycle) {
            if (!operationCycle.Begin.HasValue) {
              log.Warn ($"Get: operationCycle {operationCycle} with no begin");
            }
            else {
              response.StartDateTime = operationCycle.Begin.Value;
            }
          }
          sequences = operationSlot.Operation.Sequences
            .OrderBy (s => s.Order);
        } // Transaction end
        Debug.Assert (null != operation);
        var dateTime = this.At.HasValue ? this.At.Value : DateTime.UtcNow;
        if (!ModelDAOHelper.DAOFactory.IsInitialized (this.Machine.MachineModules)) {
          this.Machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindByIdWithMachineModules (this.Machine.Id); // Re-associate Machine to this session
        }
        foreach (var machineModule in Machine.MachineModules) {
          try {
            var byMachineModule = GetByMachineModulePart (dateTime, operationSlot, operationCycle, operation, sequences, machineModule);
            if (null != byMachineModule) {
              response.MachineModuleCycleProgress[machineModule] = byMachineModule;
            }
          }
          catch (InvalidCycle ex) {
            if (log.IsDebugEnabled) {
              log.Debug ("Get: invalid cycle", ex);
            }
            var initialFlags = response.Flags;
            response = new CycleProgressResponse ();
            response.Flags = initialFlags.Add (CycleProgressResponseFlags.InvalidCycle);
            response.MachineModuleCycleProgress = new Dictionary<IMachineModule, CycleProgressByMachineModuleResponse> ();
            response.Notices = new List<string> ();
            response.Warnings = new List<string> ();
            response.Errors = new List<string> ();
            return response;
          }
        }
        SetEstimatedNextStopDateTime (response);
        SetEstimatedAutoPalletChangeDateTime (response);
        CompleteCycleData (dateTime, response);
      }

      return response;
    }


    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async Task<CycleProgressResponse> GetAsync ()
    {
      // TODO: make it asynchronous
      return await Task.FromResult (Get ());
    }

    CycleProgressByMachineModuleResponse GetByMachineModulePart (DateTime dateTime, IOperationSlot operationSlot, IOperationCycle operationCycle, IOperation operation, IEnumerable<ISequence> sequences, IMachineModule machineModule)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Business.CycleProgress.MachineModule")) {
          var sequenceSlot = ModelDAOHelper.DAOFactory.SequenceSlotDAO.FindLastBefore (machineModule, dateTime);
          if ((null != sequenceSlot)
              && (null != sequenceSlot.Sequence)
              && object.Equals (sequenceSlot.Sequence.Operation, operation)
              && sequenceSlot.DateTimeRange.Overlaps (operationSlot.DateTimeRange)) {
            // sequenceSlot VS operationCycle
            if (null != operationCycle) {
              Bound<DateTime> sequenceSlotDateTime = sequenceSlot.EndDateTime.HasValue
                ? sequenceSlot.EndDateTime.Value
                : sequenceSlot.BeginDateTime;
              DateTime cycleStartDateTime = operationCycle.Begin.HasValue
                ? operationCycle.Begin.Value
                : operationCycle.End.Value;
              if (Bound.Compare<DateTime> (cycleStartDateTime, sequenceSlotDateTime) <= 0) { // lastOperationCycle <= sequenceSlot
                if (operationCycle.Full
                  && operationCycle.End.HasValue
                  && (Bound.Compare<DateTime> (operationCycle.End.Value, sequenceSlot.BeginDateTime) <= 0)) {
                  // reset the last operation cycle which is before sequenceSlot
                  // There is probably a cycle begin that was not detected since
                  if (log.IsDebugEnabled) {
                    log.Debug ("GetByMachineModulePart: invalid the cycle and return an empty response");
                  }
                  throw new InvalidCycle ();
                }
              }
              else { // sequenceSlot < operationCycle
                     // sequenceSlot is before operationCycle, do not consider it, go to the next machine module
                if (log.IsDebugEnabled) {
                  log.Debug ("GetByMachineModulePart: do not consider this sequenceSlot because there is a cycle after it");
                }
                return null;
              }
            }

            // sequenceSlot ok
            ISequence currentSequence = sequenceSlot.Sequence;
            var byMachineModule = new CycleProgressByMachineModuleResponse ();
            byMachineModule.CurrentSequence = currentSequence;
            Debug.Assert (sequenceSlot.BeginDateTime.HasValue);
            var sequenceSlotStartVsOperationCycleMargin = Lemoine.Info.ConfigSet
              .LoadAndGet<TimeSpan> (SEQUENCE_SLOT_START_VS_OPERATION_CYCLE_MARGIN_KEY,
              SEQUENCE_SLOT_START_VS_OPERATION_CYCLE_MARGIN_DEFAULT);
            if ((null != operationCycle)
              && !operationCycle.Full
              && operationCycle.Begin.HasValue
              && (Bound.Compare<DateTime> (sequenceSlot.BeginDateTime, operationCycle.Begin.Value.Subtract (sequenceSlotStartVsOperationCycleMargin)) < 0)
              && (1 == sequences.Count ())) {
              if (log.IsDebugEnabled) {
                log.DebugFormat ("GetByMachineModulePart: sequence slot starts before the operation cycle, this is probably wrong, replace {0} by {1}",
                  sequenceSlot.BeginDateTime, operationCycle.Begin);
              }
              byMachineModule.CurrentSequenceBeginDateTime = operationCycle.Begin.Value;
            }
            else {
              byMachineModule.CurrentSequenceBeginDateTime = sequenceSlot.BeginDateTime.Value;
            }
            // - current sequence
            TimeSpan currentSequenceElapsed = dateTime.Subtract (byMachineModule.CurrentSequenceBeginDateTime.Value);
            TimeSpan? currentSequenceStandardDuration = ServiceProvider
              .Get (new SequenceStandardTime (currentSequence));
            bool currentSequenceCompleted = false;
            // - current sequence milestone
            var sequenceMilestone = ModelDAOHelper.DAOFactory.SequenceMilestoneDAO
              .FindById (machineModule.Id);
            if (null != sequenceMilestone) {
              if (((sequenceMilestone.Sequence is null) || (sequenceMilestone.Sequence.Id == currentSequence.Id))
                && (sequenceSlot.BeginDateTime < sequenceMilestone.DateTime)) {
                if (log.IsDebugEnabled) {
                  log.Debug ($"GetByMachineModulePart: valid sequence milestone at {sequenceMilestone.DateTime} with milestone={sequenceMilestone.Milestone}");
                }
                if (sequenceMilestone.Completed) {
                  if (sequenceMilestone.DateTime <= dateTime) {
                    currentSequenceCompleted = true;
                    if (currentSequenceStandardDuration.HasValue) {
                      currentSequenceElapsed = currentSequenceStandardDuration.Value;
                    }
                  }
                  else { // dateTime < sequenceMilestone.DateTime
                    if (currentSequenceStandardDuration.HasValue) {
                      currentSequenceElapsed = currentSequenceStandardDuration.Value.Subtract (sequenceMilestone.DateTime.Subtract (dateTime));
                    }
                    else {
                      log.Warn ($"GetByMachineModulePart: no standard sequence duration and current sequence completed detected");
                    }
                  }
                }
                else {
                  if (sequenceMilestone.DateTime <= dateTime) {
                    currentSequenceElapsed = sequenceMilestone.Milestone.Add (dateTime.Subtract (sequenceMilestone.DateTime));
                  }
                  else { // dateTime < sequenceMilestone.DateTime
                    currentSequenceElapsed = sequenceMilestone.Milestone.Subtract (sequenceMilestone.DateTime.Subtract (dateTime));
                  }
                }
              }
              else {
                log.Warn ($"GetByMachineModulePart: sequence milestone at {sequenceMilestone.DateTime} with sequence={sequenceMilestone.Sequence?.Id} is not valid");
              }
            }
            byMachineModule.CurrentSequenceElapsed = currentSequenceElapsed;
            byMachineModule.Sequences = sequences
              .Where (s => object.Equals (s.Path, currentSequence.Path))
              .OrderBy (s => s.Order);
            TimeSpan? remainingTime = null;
            if (currentSequenceStandardDuration.HasValue) {
              byMachineModule.CurrentSequenceStandardTime = currentSequenceStandardDuration;
              if (currentSequenceStandardDuration.Value <= currentSequenceElapsed) {
                remainingTime = TimeSpan.FromTicks (0);
              }
              else {
                remainingTime = currentSequenceStandardDuration.Value.Subtract (currentSequenceElapsed);
              }
            }
            else if (currentSequenceCompleted) {
              remainingTime = TimeSpan.FromTicks (0);
            }
            // Other sequences
            foreach (var s in byMachineModule.Sequences
                     .Where (s => object.Equals (s.Path, currentSequence.Path)
                             && (currentSequence.Order < s.Order))
                     .OrderBy (s => s.Order)) {
              if ((SequenceKind.Stop == s.Kind)
                  && remainingTime.HasValue
                  && !byMachineModule.EstimatedNextStopDateTime.HasValue) {
                byMachineModule.EstimatedNextStopDateTime = dateTime.Add (remainingTime.Value);
                byMachineModule.NextStop = s;
              }
              if ((SequenceKind.AutoPalletChange == s.Kind)
                  && remainingTime.HasValue
                  && !byMachineModule.EstimatedAutoPalletChangeDateTime.HasValue) {
                byMachineModule.EstimatedAutoPalletChangeDateTime = dateTime.Add (remainingTime.Value);
              }
              TimeSpan? sequenceStandardDuration = ServiceProvider
                .Get (new SequenceStandardTime (s));
              if (sequenceStandardDuration.HasValue) {
                if (remainingTime.HasValue) {
                  remainingTime = remainingTime.Value.Add (sequenceStandardDuration.Value);
                }
                else {
                  remainingTime = sequenceStandardDuration.Value;
                }
              }
            }
            if (remainingTime.HasValue) {
              byMachineModule.EstimatedEndDateTime = dateTime.Add (remainingTime.Value);
            }
            // MachiningDuration
            var sequenceDurations = byMachineModule.Sequences
              .Select (s => ServiceProvider.Get (new SequenceStandardTime (s)))
              .Where (t => t.HasValue)
              .Select (t => t.Value);
            if (sequenceDurations.Any ()) {
              byMachineModule.MachiningDuration = sequenceDurations
              .Aggregate (TimeSpan.FromTicks (0), (a, t) => a.Add (t));
            }
            // Completion
            if (remainingTime.HasValue && byMachineModule.MachiningDuration.HasValue) {
              if (byMachineModule.MachiningDuration < remainingTime) {
                log.Error ($"GetByMachineModulePart: the remaining time {remainingTime} is greater than the machining duration {byMachineModule.MachiningDuration} => fallback: completion is 0");
              }
              else {
                byMachineModule.Completion =
                  (byMachineModule.MachiningDuration.Value.TotalSeconds - remainingTime.Value.TotalSeconds)
                  / byMachineModule.MachiningDuration.Value.TotalSeconds;
              }
            }
            else if (null != byMachineModule.CurrentSequence) {
              if (log.IsDebugEnabled) {
                log.DebugFormat ("GetByMachineModulePart: try to guess the completion time from the sequences");
              }
              var totalSequences = byMachineModule.Sequences.Count ();
              var completedSequences = byMachineModule.Sequences
                .Where (s => s.Order < byMachineModule.CurrentSequence.Order);
              byMachineModule.Completion =
                (double)completedSequences.Count ()
                / (double)totalSequences;
            }
            return byMachineModule;
          }
        }
      }

      return null;
    }

    void SetEstimatedNextStopDateTime (CycleProgressResponse response)
    {
      var withEstimatedNextStop = response.MachineModuleCycleProgress.Values
        .Where (bmm => bmm.EstimatedNextStopDateTime.HasValue);
      if (withEstimatedNextStop.Any ()) {
        response.EstimatedNextStopDateTime = withEstimatedNextStop
          .Min (bmm => bmm.EstimatedNextStopDateTime.Value);
      }
    }

    void SetEstimatedAutoPalletChangeDateTime (CycleProgressResponse response)
    {
      var withEstimatedAutoPalletChange = response.MachineModuleCycleProgress.Values
        .Where (bmm => bmm.EstimatedAutoPalletChangeDateTime.HasValue);
      if (withEstimatedAutoPalletChange.Any ()) {
        response.EstimatedAutoPalletChangeDateTime = withEstimatedAutoPalletChange
          .Min (bmm => bmm.EstimatedAutoPalletChangeDateTime.Value);
      }
    }

    void CompleteCycleData (DateTime dateTime, CycleProgressResponse response)
    {
      var operationCycle = response.OperationCycle;
      if (null == operationCycle) {
        CompleteCycleDataInNullCycle (dateTime, response);
      }
      else { // null != operationCycle
        if (!this.At.HasValue
            && operationCycle.Full) {
          CompleteCycleDataInCompletedCycle (response);
        }
        else { // Current cycle in progress
          CompleteCycleDataInCycleInProgress (dateTime, response);
        }
      }
    }

    void CompleteCycleDataInNullCycle (DateTime dateTime, CycleProgressResponse response)
    {
      var message = "No detected cycle";
      response.Notices.Add (message);

      var withEstimatedCycleEnd = response.MachineModuleCycleProgress.Values
        .Where (bmm => bmm.EstimatedEndDateTime.HasValue);
      if (withEstimatedCycleEnd.Any ()) {
        response.EstimatedEndDateTime = withEstimatedCycleEnd
          .Max (bmm => bmm.EstimatedEndDateTime.Value);
        SetCompletionInCycleInProgress (dateTime, response);
      }
      else {
        ApproximateCompletionAndCycleEndFromSequences (dateTime, response);
      }
    }

    void CompleteCycleDataInCompletedCycle (CycleProgressResponse response)
    {
      var operationCycle = response.OperationCycle;
      response.CompletionDateTime = operationCycle.End;
      if (log.IsDebugEnabled) {
        log.Debug ("CompleteDataInCompletedCycle: " +
                           "Completed cycle => reset the remaining time and the completion");
      }
      response.EstimatedEndDateTime = null;
      response.Completion = null;
    }

    void CompleteCycleDataInCycleInProgress (DateTime dateTime, CycleProgressResponse response)
    {
      Debug.Assert (null != response);
      var operationCycle = response.OperationCycle;
      Debug.Assert (null != operationCycle);
      var operation = response.Operation;
      Debug.Assert (null != operation);

      SetEstimatedCycleEndDateTimeInCycleInProgress (dateTime, response);

      if (response.EstimatedEndDateTime.HasValue) {
        SetCompletionInCycleInProgress (dateTime, response);
      }
      else {
        ApproximateCompletionAndCycleEndFromSequences (dateTime, response);
      }
    }

    void ApproximateCompletionAndCycleEndFromSequences (DateTime dateTime, CycleProgressResponse response)
    {
      Debug.Assert (null != response);
      Debug.Assert (!response.EstimatedEndDateTime.HasValue);

      var byMachineModuleWithCompletion = response.MachineModuleCycleProgress.Values
        .Where (d => d.Completion.HasValue);
      if (byMachineModuleWithCompletion.Any ()) {
        var completion = byMachineModuleWithCompletion
          .Select (d => d.Completion.Value)
          .Average ();
        if (log.IsDebugEnabled) {
          log.DebugFormat ("ApproximateCompletionAndCycleEndFromSequences: get {0} from byMachineModule completion", completion);
        }
        response.Completion = completion;
        response.Notices.Add ("Approximated completion from the currently active sequence");
      }

      if (!response.EstimatedEndDateTime.HasValue
        && response.Completion.HasValue
        && (null != response.Operation)
        && response.Operation.MachiningDuration.HasValue) {
        var remainingTime = TimeSpan.FromSeconds (response.Operation.MachiningDuration.Value.TotalSeconds
          * response.Completion.Value);
        response.EstimatedEndDateTime = dateTime.Add (remainingTime);
        if (log.IsDebugEnabled) {
          log.DebugFormat ("ApproximateCompletionAndCycleEndFromSequences: get estimated cycle end {0} from completion {1} and machining duration {2}", response.EstimatedEndDateTime, response.Completion.Value, response.Operation.MachiningDuration.Value);
        }
        response.Notices.Add ("Approximated cycle end from the the operation duration and the currently active sequence");
      }
    }

    void SetCompletionInCycleInProgress (DateTime dateTime, CycleProgressResponse response)
    {
      Debug.Assert (null != response);
      var operation = response.Operation;
      Debug.Assert (null != operation);

      if (!response.EstimatedEndDateTime.HasValue) {
        ApproximateCompletionAndCycleEndFromSequences (dateTime, response);
      }
      else {
        var remainingTime = response.EstimatedEndDateTime.Value.Subtract (dateTime);

        // operation.MachiningDuration VS MAX(SUM(MachiningDuration))
        TimeSpan? totalRemainingTime = remainingTime; // In case operation.MachiningDuration != MAX(SUM(MachiningDuration))
        TimeSpan? totalDuration = operation.MachiningDuration; // Only considering the operation duration
        TimeSpan? machiningDuration = operation.MachiningDuration; // Considering both the sequence duration and the operation duration
        if (response.MachineModuleCycleProgress.Any ()) {
          var machineModuleProgressWithMachiningDuration = response.MachineModuleCycleProgress.Values
            .Where (v => v.MachiningDuration.HasValue);
          if (machineModuleProgressWithMachiningDuration.Any ()) {
            var maxMachiningDurationByMachineModule = machineModuleProgressWithMachiningDuration
              .Max (v => v.MachiningDuration.Value);
            // In case of automatic standard duration (total duration divided by the number of sequences)
            // this number needs to be rounded at the closest second
            maxMachiningDurationByMachineModule = TimeSpan.FromSeconds (Math.Round (maxMachiningDurationByMachineModule.TotalSeconds));
            if (operation.MachiningDuration.HasValue) {
              totalDuration = operation.MachiningDuration.Value; // Takes the priority
              if (operation.MachiningDuration.Value < maxMachiningDurationByMachineModule) {
                log.ErrorFormat ("SetCompletionInCycleInProgress: " +
                                   "maxMachiningDurationByMachineModule {0} is greater than operation machining duration {1}",
                                   maxMachiningDurationByMachineModule, operation.MachiningDuration.Value);
                string message = $"The sum of the sequence durations {maxMachiningDurationByMachineModule} is greater than the operation machining duration {operation.MachiningDuration.Value}";
                response.Errors.Add (message);
                machiningDuration = maxMachiningDurationByMachineModule;
                totalRemainingTime = null; // This is not possible to get a completion based on the operation duration then
              }
              else if (maxMachiningDurationByMachineModule < operation.MachiningDuration.Value) {
                if (1 == response.MachineModuleCycleProgress.Count) { // Unique machine module
                  log.WarnFormat ("SetCompletionInCycleInProgress: " +
                                  "the sum of sequence durations {0} is less than operation machining duration {1} " +
                                  $"(and unique machine module) for operation id {((IDataWithId<int>)operation).Id}",
                                    maxMachiningDurationByMachineModule, operation.MachiningDuration.Value);
                  string message = $"The sum of the sequence durations {maxMachiningDurationByMachineModule} is less than the operation machining duration {operation.MachiningDuration.Value}";
                  response.Warnings.Add (message);
                  machiningDuration = maxMachiningDurationByMachineModule;
                  totalRemainingTime = remainingTime.Add (operation.MachiningDuration.Value.Subtract (maxMachiningDurationByMachineModule));
                }
                else { // Different machine modules
                  machiningDuration = operation.MachiningDuration.Value;
                }
              }
              else { // Equal
                machiningDuration = operation.MachiningDuration.Value;
              }
            }
            else { // !operation.MachiningDuration.HasValue
              totalDuration = maxMachiningDurationByMachineModule;
              machiningDuration = maxMachiningDurationByMachineModule;
            }
          }
        }
        // Completion, based on totalDuration and totalRemainingTime
        if (totalRemainingTime.HasValue
            && totalDuration.HasValue
            && (0.0 < totalDuration.Value.TotalSeconds)) {
          if (totalDuration.Value < totalRemainingTime) {
            log.FatalFormat ("SetCompletionInCycleInProgress: " +
                                 "the total remaining time {0} is greater that the total duration {1} =>" +
                                 "=> fallback: completion is 0",
                                 totalRemainingTime.Value, totalDuration.Value);
            response.Completion = 0;
          }
          response.Completion = (totalDuration.Value.TotalSeconds - totalRemainingTime.Value.TotalSeconds)
            / totalDuration.Value.TotalSeconds;
        }
        // Machining completion, based on machiningDuration and remainingTime
        if (machiningDuration.HasValue
            && (0.0 < machiningDuration.Value.TotalSeconds)) {
          if (machiningDuration.Value < remainingTime) {
            log.FatalFormat ("SetCompletionInCycleInProgress: " +
                                 "the remaining time {0} is greater that the machining duration {1} =>" +
                                 "=> fallback: completion is 0",
                                 remainingTime, machiningDuration.Value);
            response.MachiningCompletion = 0;
          }
          response.MachiningCompletion = (machiningDuration.Value.TotalSeconds - remainingTime.TotalSeconds)
            / machiningDuration.Value.TotalSeconds;
        }
        // Note: Completion and MachiningCompletion differ if operation.MachiningDuration > MAX(SUM(MachiningDuration))
      }
    }

    void SetEstimatedCycleEndDateTimeInCycleInProgress (DateTime dateTime, CycleProgressResponse response)
    {
      Debug.Assert (null != response);
      var operationCycle = response.OperationCycle;
      Debug.Assert (null != operationCycle);

      var withEstimatedCycleEnd = response.MachineModuleCycleProgress.Values
        .Where (bmm => bmm.EstimatedEndDateTime.HasValue);
      if (withEstimatedCycleEnd.Any ()) {
        response.EstimatedEndDateTime = withEstimatedCycleEnd
          .Max (bmm => bmm.EstimatedEndDateTime.Value);
      }
      else { // No estimated cycle end from the sequences => consider the cycle directly
        if ((null != operationCycle)
          && !operationCycle.Full
          && operationCycle.Begin.HasValue
          && (null != operationCycle.OperationSlot)
          && (null != operationCycle.OperationSlot.Operation)
          && operationCycle.OperationSlot.Operation.MachiningDuration.HasValue) {
          if (log.IsDebugEnabled) {
            log.DebugFormat ("SetEstimatedCycleEndDateTimeInCycleInProgress: estimated cycle end, consider the partial cycle on operation id {0} that started at {1}",
              ((IDataWithId)operationCycle.OperationSlot.Operation).Id, operationCycle.Begin.Value);
          }
          response.EstimatedEndDateTime = operationCycle.Begin.Value
            .Add (operationCycle.OperationSlot.Operation.MachiningDuration.Value);
          var message = string.Format ("Estimated cycle end from the active cycle that started at {0}",
            operationCycle.Begin.Value.ToLocalTime ());
          response.Notices.Add (message);
        }
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      string cacheKey = "Business.Operation.CycleProgress."
        + ((IDataWithId<int>)Machine).Id;
      if (this.At.HasValue) {
        cacheKey += "." + this.At.Value;
      }
      return cacheKey;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/>
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<CycleProgressResponse> data)
    {
      return true;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (CycleProgressResponse data)
    {
      if (this.At.HasValue) {
        return CacheTimeOut.PastLong.GetTimeSpan ();
      }

      var utcNow = DateTime.UtcNow;
      var timeOut = CacheTimeOut.CurrentShort.GetTimeSpan ();
      if (data.EstimatedEndDateTime.HasValue) {
        var delta = data.EstimatedEndDateTime.Value.Subtract (utcNow);
        if (delta < timeOut) {
          timeOut = delta;
        }
      }
      if (data.EstimatedNextStopDateTime.HasValue) {
        var delta = data.EstimatedNextStopDateTime.Value.Subtract (utcNow);
        if (delta < timeOut) {
          timeOut = delta;
        }
      }
      if (data.EstimatedNextStopDateTime.HasValue) {
        var delta = data.EstimatedNextStopDateTime.Value.Subtract (utcNow);
        if (delta < timeOut) {
          timeOut = delta;
        }
      }
      foreach (var machineModuleData in data.MachineModuleCycleProgress.Values) {
        if ((null != machineModuleData.CurrentSequence)
          && machineModuleData.CurrentSequenceElapsed.HasValue
          && machineModuleData.CurrentSequenceStandardTime.HasValue) {
          var currentRemainingTime = machineModuleData.CurrentSequenceStandardTime.Value
            .Subtract (machineModuleData.CurrentSequenceElapsed.Value);
          if (currentRemainingTime < timeOut) {
            timeOut = currentRemainingTime;
          }
        }
      }
      if (timeOut < TimeSpan.FromSeconds (0)) {
        return TimeSpan.FromSeconds (0);
      }
      else {
        return timeOut;
      }
    }
    #endregion // IRequest implementation
  }

  /// <summary>
  /// Flags to the cycle progress response
  /// </summary>
  [Flags]
  public enum CycleProgressResponseFlags
  {
    /// <summary>
    /// No specific flag
    /// </summary>
    None = 0,
    /// <summary>
    /// Not applicable on this machine.
    /// 
    /// For example, no cycle is detected on this machine.
    /// </summary>
    NotApplicable = 1,
    /// <summary>
    /// The cycle is invalid at the requested time
    /// </summary>
    InvalidCycle = 2,
    /// <summary>
    /// No effective operation at the requested time
    /// </summary>
    NoEffectiveOperation = 4,
  }

  /// <summary>
  /// Extensions to the reason source
  /// </summary>
  public static class CycleProgressResponseFlagsExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CycleProgressResponseFlagsExtensions).ToString ());

    /// <summary>
    /// Check if other is included into t
    /// </summary>
    /// <param name="t"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public static bool HasFlag (this CycleProgressResponseFlags t, CycleProgressResponseFlags other)
    {
      return other == (t & other);
    }

    /// <summary>
    /// Add an option to another one (which may be null)
    /// </summary>
    /// <param name="t"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public static CycleProgressResponseFlags Add (this CycleProgressResponseFlags t, CycleProgressResponseFlags other)
    {
      return t | other;
    }

    /// <summary>
    /// Remove an option
    /// </summary>
    /// <param name="t"></param>
    /// <param name="toRemove"></param>
    /// <returns></returns>
    public static CycleProgressResponseFlags Remove (this CycleProgressResponseFlags t, CycleProgressResponseFlags toRemove)
    {
      return t & ~toRemove;
    }
  }

  /// <summary>
  /// Response structure to the CycleProgress request
  /// </summary>
  public class CycleProgressResponse: IProgressResponse
  {
    /// <summary>
    /// Associated flags
    /// </summary>
    public CycleProgressResponseFlags Flags { get; set; }

    /// <summary>
    /// Current operation
    /// </summary>
    public IOperation Operation { get; set; }

    /// <summary>
    /// Progress by machine module
    /// 
    /// nullable
    /// </summary>
    public IDictionary<IMachineModule, CycleProgressByMachineModuleResponse> MachineModuleCycleProgress { get; set; } = null;

    /// <summary>
    /// Progress by machine module
    /// 
    /// nullable
    /// </summary>
    public IDictionary<IMachineModule, IProgressByMachineModuleResponse> MachineModuleProgress => this.MachineModuleCycleProgress?.ToDictionary (x => x.Key, x => (IProgressByMachineModuleResponse)x.Value);

    /// <summary>
    /// Start date/time, if applicable
    /// </summary>
    public DateTime? StartDateTime { get; set; }

    /// <summary>
    /// Estimated date/time of the next stop, if any
    /// </summary>
    public DateTime? EstimatedNextStopDateTime { get; set; }

    /// <summary>
    /// Estimated date/time of the next pallet change, if any
    /// </summary>
    public DateTime? EstimatedAutoPalletChangeDateTime { get; set; }

    /// <summary>
    /// Estimated time of the next cycle end (in the future)
    /// </summary>
    public DateTime? EstimatedEndDateTime { get; set; }

    /// <summary>
    /// Completion % (between 0 and 1) of the cycle considering in priority the operation duration
    /// </summary>
    public double? Completion { get; set; }

    /// <summary>
    /// Completion % (between 0 and 1) of the cycle considering in priority the machining periods
    /// </summary>
    public double? MachiningCompletion { get; set; }

    /// <summary>
    /// Date/time of the latest cycle completion if there is currently no active cycle
    /// </summary>
    public DateTime? CompletionDateTime { get; set; }

    /// <summary>
    /// Reference to the operation cycle
    /// </summary>
    public IOperationCycle OperationCycle { get; set; }

    /// <summary>
    /// Warnings
    /// </summary>
    public IList<string> Warnings { get; set; }

    /// <summary>
    /// Errors
    /// </summary>
    public IList<string> Errors { get; set; }

    /// <summary>
    /// Notices on the data
    /// </summary>
    public IList<string> Notices { get; set; }
  }

  /// <summary>
  /// Machine module part of the response
  /// </summary>
  public class CycleProgressByMachineModuleResponse: IProgressByMachineModuleResponse
  {
    /// <summary>
    /// Current sequence
    /// </summary>
    public ISequence CurrentSequence { get; set; }

    /// <summary>
    /// Duration of the current sequence
    /// </summary>
    public TimeSpan? CurrentSequenceStandardTime { get; set; }

    /// <summary>
    /// Begin date/time of the current sequence
    /// </summary>
    public DateTime? CurrentSequenceBeginDateTime { get; set; }

    /// <summary>
    /// Already elapsed time in the current sequence
    /// </summary>
    public TimeSpan? CurrentSequenceElapsed { get; set; }

    /// <summary>
    /// List of sequences to process for this machine module
    /// </summary>
    public IEnumerable<ISequence> Sequences { get; set; }

    /// <summary>
    /// Estimated date/time of the next stop, if any
    /// </summary>
    public DateTime? EstimatedNextStopDateTime { get; set; }

    /// <summary>
    /// Next stop sequence, if any, else null
    /// </summary>
    public ISequence NextStop { get; set; }

    /// <summary>
    /// Estimated date/time of the next pallet change, if any
    /// </summary>
    public DateTime? EstimatedAutoPalletChangeDateTime { get; set; }

    /// <summary>
    /// Estimated time of the next cycle end (in the future)
    /// </summary>
    public DateTime? EstimatedEndDateTime { get; set; }

    /// <summary>
    /// Machining duration for this machine module considering the sum of the sequence duration
    /// </summary>
    public TimeSpan? MachiningDuration { get; set; }

    /// <summary>
    /// Completion % (between 0 and 1) of the cycle
    /// </summary>
    public double? Completion { get; set; }
  }
}
