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
  /// Get the progress of a specific operation
  /// </summary>
  public sealed class OperationProgress : IRequest<OperationProgressResponse>
  {
    class InvalidCycle : Exception
    { }

    static readonly string EFFECTIVE_OPERATION_MAX_AGE_KEY = "Business.OperationProgress.EffectiveOperationMaxAge";
    static readonly TimeSpan EFFECTIVE_OPERATION_MAX_AGE_DEFAULT = TimeSpan.FromDays (7);

    static readonly string ADJUST_WITH_OVERRIDE_KEY = "Business.OperationProgress.AdjustWithOverride";
    static readonly bool ADJUST_WITH_OVERRIDE_DEFAULT = true;

    static readonly string ADJUST_WITH_OVERRIDE_MIN_TIME_KEY = "Business.OperationProgress.AdjustWithOverrideMinTime";
    static readonly TimeSpan ADJUST_WITH_OVERRIDE_MIN_TIME_DEFAULT = TimeSpan.FromSeconds (3);

    static readonly string PROPORTION_MACHINING_KEY = "Business.OperationProgress.ProportionMachining";
    static readonly double PROPORTION_MACHINING_DEFAULT = 0.80;

    static readonly string PROPORTION_RAPID_KEY = "Business.OperationProgress.PropertionRapid";
    static readonly double PROPORTION_RAPID_DEFAULT = 0.20;

    static readonly string OVERRIDE_LESSER_THAN_KEY = "Business.OperationProgress.OverrideLesserThan";
    static readonly double OVERRIDE_LESSER_THAN_DEFAULT = 95.0;

    static readonly string OVERRIDE_GREATER_THAN_KEY = "Business.OperationProgress.OverrideGreaterThan";
    static readonly double OVERRIDE_GREATER_THAN_DEFAULT = 105.0;

    readonly ILog log = LogManager.GetLogger (typeof (OperationProgress).FullName);

    #region Getters / Setters
    /// <summary>
    /// Reference to the machine
    /// </summary>
    public IMonitoredMachine Machine { get; internal set; }

    /// <summary>
    /// Specify if you want to get the operation progress at a specific date/time
    /// </summary>
    public DateTime? At { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="machine">not null</param>
    public OperationProgress (IMonitoredMachine machine)
    {
      Debug.Assert (null != machine);

      this.Machine = machine;

      log = LogManager.GetLogger (string.Format ("{0}.{1}",
        typeof (OperationProgress).FullName, machine.Id));
    }
    #endregion // Constructors

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns>Not null</returns>
    public OperationProgressResponse Get ()
    {
      var response = new OperationProgressResponse ();
      response.MachineModuleOperationProgress = new Dictionary<IMachineModule, OperationProgressByMachineModuleResponse> ();
      response.Warnings = new List<string> ();
      response.Errors = new List<string> ();
      response.Notices = new List<string> ();

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        IOperationSlot operationSlot;
        IOperation operation;
        IEnumerable<ISequence> sequences;
        var dateTime = this.At.HasValue ? this.At.Value : DateTime.UtcNow;
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Business.OperationProgress.Operation")) {
          if (this.At.HasValue) {
            // TODO: get operationSlot and operation
            operationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
              .FindAt (this.Machine, this.At.Value);
          }
          else { // !this.At.HasValue
            var effectiveOperationMaxAge = Lemoine.Info.ConfigSet
              .LoadAndGet<TimeSpan> (EFFECTIVE_OPERATION_MAX_AGE_KEY,
                                     EFFECTIVE_OPERATION_MAX_AGE_DEFAULT);
            operationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
              .GetLastEffective (this.Machine, effectiveOperationMaxAge);
          }
          operation = operationSlot?.Operation;
          if (operation is null) {
            if (log.IsDebugEnabled) {
              log.Debug ($"Get: no effective operation at {this.At}");
            }
            response.Flags = response.Flags.Add (OperationProgressResponseFlags.NoEffectiveOperation);
            var lastOperationSlotBefore = ModelDAOHelper.DAOFactory.OperationSlotDAO
              .GetLastOperationNotNullBefore (this.Machine, dateTime);
            if (null != lastOperationSlotBefore) {
              if (log.IsDebugEnabled) {
                log.Debug ($"Get: last operation end date/time={lastOperationSlotBefore?.EndDateTime}");
              }
              response.CompletionDateTime = lastOperationSlotBefore.EndDateTime.NullableValue;
            }
            return response;
          }
          if (log.IsDebugEnabled) {
            log.Debug ($"Get: effective operation is {operation?.Id}");
          }
          response.Operation = operation;
          if (null != operationSlot) {
            if (!operationSlot.DateTimeRange.Lower.HasValue) {
              log.Error ($"Get: operation slot {operationSlot} with no start date/time");
            }
            else {
              response.StartDateTime = operationSlot.DateTimeRange.Lower.Value;
            }
          }
          Debug.Assert (null != operation);
          sequences = operation.Sequences
            .OrderBy (s => s.Order);
        } // Transaction end
        Debug.Assert (null != operation);
        if (!ModelDAOHelper.DAOFactory.IsInitialized (this.Machine.MachineModules)) {
          this.Machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindByIdWithMachineModules (this.Machine.Id); // Re-associate Machine to this session
        }
        foreach (var machineModule in Machine.MachineModules) {
          var byMachineModule = GetByMachineModulePart (dateTime, operationSlot, operation, sequences, machineModule);
          if (null != byMachineModule) {
            response.MachineModuleOperationProgress[machineModule] = byMachineModule;
          }
        }
        SetEstimatedNextStopDateTime (response);
        SetEstimatedAutoPalletChangeDateTime (response);
        response.Completed = response.MachineModuleOperationProgress.Values.All (x => x.Completed);
        if (response.Completed && !response.CompletionDateTime.HasValue) {
          var machineModuleOperationProgressWithCompletionDateTime = response.MachineModuleOperationProgress.Values
            .Where (x => x.CompletionDateTime.HasValue);
          if (machineModuleOperationProgressWithCompletionDateTime.Any ()) {
            response.CompletionDateTime = machineModuleOperationProgressWithCompletionDateTime
              .Max (x => x.CompletionDateTime.Value);
          }
        }
        {
          var machineModuleOperationProgressWithLastUpdateDateTime = response.MachineModuleOperationProgress.Values
            .Where (x => x.LastUpdateDateTime.HasValue);
          if (machineModuleOperationProgressWithLastUpdateDateTime.Any ()) {
            response.LastUpdateDateTime = machineModuleOperationProgressWithLastUpdateDateTime
              .Min (x => x.LastUpdateDateTime.Value);
          }
        }
        CompleteOperationData (dateTime, response);
      }

      return response;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async Task<OperationProgressResponse> GetAsync ()
    {
      // TODO: make it asynchronous
      return await Task.FromResult (Get ());
    }

    TimeSpan? GetAdjustedDuration (TimeSpan? duration, IMachineModule machineModule, OperationProgressByMachineModuleResponse byMachineModule, ISequence sequence)
    {
      Debug.Assert (null != machineModule);
      Debug.Assert (null != byMachineModule);

      if ((null != sequence) && duration.HasValue && sequence.Kind.Equals (SequenceKind.Machining)) {
        if (byMachineModule.SpeedFactor.HasValue) {
          return TimeSpan.FromSeconds (duration.Value.TotalSeconds * byMachineModule.SpeedFactor.Value);
        }
        else if (Lemoine.Info.ConfigSet.LoadAndGet (ADJUST_WITH_OVERRIDE_KEY, ADJUST_WITH_OVERRIDE_DEFAULT)
          && IsAdjustmentCheckRequired (duration.Value)) {
          var speedFactor = GetSpeedFactor (machineModule, sequence);
          if (1.0 != speedFactor) {
            byMachineModule.SpeedFactor = speedFactor;
            return TimeSpan.FromSeconds (duration.Value.TotalSeconds * speedFactor);
          }
        }
      }
      return duration;
    }

    OperationProgressByMachineModuleResponse GetByMachineModulePart (DateTime dataTime, IOperationSlot operationSlot, IOperation operation, IEnumerable<ISequence> sequences, IMachineModule machineModule)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Business.OperationProgress.MachineModule")) {
          var sequenceSlot = ModelDAOHelper.DAOFactory.SequenceSlotDAO.FindLastBefore (machineModule, dataTime);
          if ((null != sequenceSlot)
              && (null != sequenceSlot.Sequence)
              && object.Equals (sequenceSlot.Sequence.Operation, operation)
              && sequenceSlot.DateTimeRange.Overlaps (operationSlot.DateTimeRange)) {

            // sequenceSlot ok
            ISequence currentSequence = sequenceSlot.Sequence;
            var byMachineModule = new OperationProgressByMachineModuleResponse ();
            byMachineModule.CurrentSequence = currentSequence;
            Debug.Assert (sequenceSlot.BeginDateTime.HasValue);
            byMachineModule.CurrentSequenceBeginDateTime = sequenceSlot.BeginDateTime.Value;
            if (sequenceSlot.EndDateTime.HasValue) {
              byMachineModule.LastUpdateDateTime = sequenceSlot.EndDateTime.Value;
            }
            else {
              byMachineModule.LastUpdateDateTime = sequenceSlot.BeginDateTime.Value;
            }
            // - current sequence
            TimeSpan currentSequenceElapsed = dataTime.Subtract (byMachineModule.CurrentSequenceBeginDateTime.Value);
            TimeSpan? currentSequenceStandardDuration = ServiceProvider
              .Get (new SequenceStandardTime (currentSequence));
            bool currentSequenceCompleted = false;
            // - current sequence milestone
            var sequenceMilestone = ModelDAOHelper.DAOFactory.SequenceMilestoneDAO
              .FindById (machineModule.Id);
            if (sequenceMilestone is null) {
              if (currentSequence.Kind.Equals (SequenceKind.Machining) && IsAdjustmentCheckRequired (currentSequenceElapsed)) {
                var speedFactor = GetSpeedFactor (machineModule, currentSequence);
                byMachineModule.SpeedFactor = speedFactor;
                currentSequenceElapsed = TimeSpan.FromSeconds (currentSequenceElapsed.TotalSeconds / speedFactor);
              }
            }
            else { // sequenceMilestone is not null
              if (((sequenceMilestone.Sequence is null) || (sequenceMilestone.Sequence.Id == currentSequence.Id))
                && (sequenceSlot.BeginDateTime < sequenceMilestone.DateTime)
                && (sequenceMilestone.Completed || (0 < sequenceMilestone.Milestone.Ticks))) {
                if (log.IsDebugEnabled) {
                  log.Debug ($"GetByMachineModulePart: valid sequence milestone at {sequenceMilestone.DateTime} with milestone={sequenceMilestone.Milestone} completed={sequenceMilestone.Completed}");
                }
                byMachineModule.LastUpdateDateTime = sequenceMilestone.DateTime;
                if (sequenceMilestone.Completed) {
                  byMachineModule.CurrentSequenceCompleted = true;
                  byMachineModule.CurrentSequenceCompletionDateTime = sequenceMilestone.DateTime;
                  if (sequenceMilestone.DateTime <= dataTime) {
                    currentSequenceCompleted = true;
                    if (currentSequenceStandardDuration.HasValue) {
                      currentSequenceElapsed = currentSequenceStandardDuration.Value;
                    }
                  }
                  else { // dateTime < sequenceMilestone.DateTime
                    if (currentSequenceStandardDuration.HasValue) {
                      currentSequenceElapsed = currentSequenceStandardDuration.Value.Subtract (sequenceMilestone.DateTime.Subtract (dataTime));
                    }
                    else {
                      log.Warn ($"GetByMachineModulePart: no standard sequence duration and current sequence completed detected");
                    }
                  }
                }
                else { // Not completed
                  if (sequenceMilestone.DateTime <= dataTime) {
                    var durationAfterMilestone = dataTime.Subtract (sequenceMilestone.DateTime);
                    if (currentSequence.Kind.Equals (SequenceKind.Machining) && IsAdjustmentCheckRequired (durationAfterMilestone)) {
                      var speedFactor = GetSpeedFactor (machineModule, currentSequence);
                      byMachineModule.SpeedFactor = speedFactor;
                      durationAfterMilestone = TimeSpan.FromSeconds (durationAfterMilestone.TotalSeconds / speedFactor);
                    }
                    currentSequenceElapsed = sequenceMilestone.Milestone.Add (durationAfterMilestone);
                  }
                  else { // dataTime < sequenceMilestone.DateTime
                    currentSequenceElapsed = sequenceMilestone.Milestone.Subtract (sequenceMilestone.DateTime.Subtract (dataTime));
                  }
                }
              }
              else {
                if (log.IsDebugEnabled) {
                  log.Debug ($"GetByMachineModulePart: sequence milestone at {sequenceMilestone.DateTime} with sequence={sequenceMilestone.Sequence?.Id} is not valid (too strictly after sequence start or for another sequence)");
                }
                if (currentSequence.Kind.Equals (SequenceKind.Machining) && IsAdjustmentCheckRequired (currentSequenceElapsed)) {
                  var speedFactor = GetSpeedFactor (machineModule, currentSequence);
                  byMachineModule.SpeedFactor = speedFactor;
                  currentSequenceElapsed = TimeSpan.FromSeconds (currentSequenceElapsed.TotalSeconds / speedFactor);
                }
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
            var adjustedRemainingTime = GetAdjustedDuration (remainingTime.Value, machineModule, byMachineModule, currentSequence);
            // Other sequences
            var nextSequences = byMachineModule.Sequences
                     .Where (s => object.Equals (s.Path, currentSequence.Path)
                             && (currentSequence.Order < s.Order));
            if (!nextSequences.Any () && currentSequenceCompleted) {
              byMachineModule.Completed = true;
              byMachineModule.CompletionDateTime = byMachineModule.CurrentSequenceCompletionDateTime;
              if (!byMachineModule.CompletionDateTime.HasValue) {
                if (sequenceSlot.EndDateTime.HasValue) {
                  if (log.IsDebugEnabled) {
                    log.Debug ($"GetByMachineModulePart: take the completion date/time from sequence slot range={sequenceSlot.DateTimeRange}");
                  }
                  byMachineModule.CompletionDateTime = sequenceSlot.EndDateTime.Value;
                }
                else { // Non machining sequence
                  if (log.IsDebugEnabled) {
                    log.Debug ($"GetByMachineModulePart: no end in sequence slot, take begin={sequenceSlot.BeginDateTime} instead");
                  }
                  byMachineModule.CompletionDateTime = sequenceSlot.BeginDateTime.Value;
                }
              }
            }
            foreach (var s in nextSequences.OrderBy (s => s.Order)) {
              if ((SequenceKind.Stop == s.Kind)
                  && adjustedRemainingTime.HasValue
                  && !byMachineModule.EstimatedNextStopDateTime.HasValue) {
                byMachineModule.EstimatedNextStopDateTime = dataTime.Add (adjustedRemainingTime.Value);
                byMachineModule.NextStop = s;
              }
              if ((SequenceKind.AutoPalletChange == s.Kind)
                  && adjustedRemainingTime.HasValue
                  && !byMachineModule.EstimatedAutoPalletChangeDateTime.HasValue) {
                byMachineModule.EstimatedAutoPalletChangeDateTime = dataTime.Add (adjustedRemainingTime.Value);
              }
              TimeSpan? sequenceStandardDuration = ServiceProvider
                .Get (new SequenceStandardTime (s));
              if (sequenceStandardDuration.HasValue) {
                remainingTime = (remainingTime ?? TimeSpan.FromTicks (0)).Add (sequenceStandardDuration.Value);
                var adjustedSequenceDuration = GetAdjustedDuration (sequenceStandardDuration.Value, machineModule, byMachineModule, s);
                adjustedRemainingTime = (adjustedRemainingTime ?? TimeSpan.FromTicks (0)).Add (adjustedSequenceDuration.Value);
              }
            }
            if (adjustedRemainingTime.HasValue) {
              byMachineModule.EstimatedEndDateTime = dataTime.Add (adjustedRemainingTime.Value);
            }
            else if (remainingTime.HasValue) { // This should not happen
              log.Fatal ($"GetByMachineModulePart: unexpected code, remainingTime should not be not null if adjustedRemainingTime is null");
              byMachineModule.EstimatedEndDateTime = dataTime.Add (remainingTime.Value);
            }
            // MachiningDuration
            var sequenceDurations = byMachineModule.Sequences
              .Select (s => new Tuple<ISequence, TimeSpan?>(s,ServiceProvider.Get (new SequenceStandardTime (s))))
              .Where (st => st.Item2.HasValue)
              .Select (st => new Tuple<ISequence, TimeSpan> (st.Item1,st.Item2.Value));
            if (sequenceDurations.Any ()) {
              byMachineModule.MachiningDuration = byMachineModule.NotAdjustedMachiningDuration = sequenceDurations
              .Aggregate (TimeSpan.FromTicks (0), (a, st) => a.Add (st.Item2));
              if (byMachineModule.SpeedFactor.HasValue
                && (1 != byMachineModule.SpeedFactor.Value)
                && byMachineModule.MachiningDuration.HasValue
                && (0 < byMachineModule.MachiningDuration.Value.TotalSeconds)) {
                var machiningOnlySequences = sequenceDurations
                  .Where (st => st.Item1.Kind.Equals (SequenceKind.Machining));
                if (machiningOnlySequences.Any ()) {
                  var machiningOnlyDuration = machiningOnlySequences
                    .Aggregate (TimeSpan.FromTicks (0), (a, st) => a.Add (st.Item2));
                  var machiningOnlyProportion = machiningOnlyDuration.TotalSeconds / byMachineModule.MachiningDuration.Value.TotalSeconds;
                  var effectiveFactor = 1 - machiningOnlyProportion * (1 - byMachineModule.SpeedFactor.Value);
                  byMachineModule.MachiningDuration = TimeSpan.FromSeconds (byMachineModule.MachiningDuration.Value.TotalSeconds * effectiveFactor);
                }
              }
            }
            // Completion
            if (remainingTime.HasValue && byMachineModule.MachiningDuration.HasValue) {
              if (byMachineModule.MachiningDuration < remainingTime) {
                log.Fatal ($"GetByMachineModulePart: the remaining time {remainingTime} is greater than the machining duration {byMachineModule.MachiningDuration} => fallback: completion is 0");
              }
              else if (1 <= byMachineModule.MachiningDuration.Value.TotalSeconds) {
                byMachineModule.Completion =
                  (byMachineModule.MachiningDuration.Value.TotalSeconds - remainingTime.Value.TotalSeconds)
                  / byMachineModule.MachiningDuration.Value.TotalSeconds;
              }
            }
            else if (null != byMachineModule.CurrentSequence) {
              if (log.IsDebugEnabled) {
                log.Debug ("GetByMachineModulePart: try to guess the completion time from the sequences");
              }
              var totalSequences = byMachineModule.Sequences.Count ();
              if (0 < totalSequences) {
                var completedSequences = byMachineModule.Sequences
                  .Where (s => s.Order < byMachineModule.CurrentSequence.Order);
                byMachineModule.Completion =
                  (double)completedSequences.Count ()
                  / (double)totalSequences;
              }
            }
            if (byMachineModule.SpeedFactor.HasValue && (byMachineModule.SpeedFactor.Value == 1.0)) {
              byMachineModule.SpeedFactor = null;
              byMachineModule.NotAdjustedMachiningDuration = null;
            }
            return byMachineModule;
          }
        }
      }

      return null;
    }

    void SetEstimatedNextStopDateTime (OperationProgressResponse response)
    {
      var withEstimatedNextStop = response.MachineModuleOperationProgress.Values
        .Where (bmm => bmm.EstimatedNextStopDateTime.HasValue);
      if (withEstimatedNextStop.Any ()) {
        response.EstimatedNextStopDateTime = withEstimatedNextStop
          .Min (bmm => bmm.EstimatedNextStopDateTime.Value);
      }
    }

    void SetEstimatedAutoPalletChangeDateTime (OperationProgressResponse response)
    {
      var withEstimatedAutoPalletChange = response.MachineModuleOperationProgress.Values
        .Where (bmm => bmm.EstimatedAutoPalletChangeDateTime.HasValue);
      if (withEstimatedAutoPalletChange.Any ()) {
        response.EstimatedAutoPalletChangeDateTime = withEstimatedAutoPalletChange
          .Min (bmm => bmm.EstimatedAutoPalletChangeDateTime.Value);
      }
    }

    void CompleteOperationData (DateTime dateTime, OperationProgressResponse response)
    {
      var withEstimatedEndDateTime = response.MachineModuleOperationProgress.Values
        .Where (bmm => bmm.EstimatedEndDateTime.HasValue);
      if (withEstimatedEndDateTime.Any ()) {
        response.EstimatedEndDateTime = withEstimatedEndDateTime
          .Max (bmm => bmm.EstimatedEndDateTime.Value);
        SetCompletionInOperationInProgress (dateTime, response);
      }
      else {
        ApproximateCompletionAndOperationEndFromSequences (dateTime, response);
      }
    }

    void ApproximateCompletionAndOperationEndFromSequences (DateTime dateTime, OperationProgressResponse response)
    {
      Debug.Assert (null != response);
      Debug.Assert (!response.EstimatedEndDateTime.HasValue);

      var byMachineModuleWithCompletion = response.MachineModuleOperationProgress.Values
        .Where (d => d.Completion.HasValue);
      if (byMachineModuleWithCompletion.Any ()) {
        var completion = byMachineModuleWithCompletion
          .Select (d => d.Completion.Value)
          .Average ();
        if (log.IsDebugEnabled) {
          log.DebugFormat ("ApproximateCompletionAndOperationEndFromSequences: get {0} from byMachineModule completion", completion);
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
          log.Debug ($"ApproximateCompletionAndOperationEndFromSequences: get estimated operation end {response.EstimatedEndDateTime} from completion {response.Completion.Value} and machining duration {response.Operation.MachiningDuration.Value}");
        }
        response.Notices.Add ("Approximated cycle end from the the operation duration and the currently active sequence");
      }
    }

    void SetCompletionInOperationInProgress (DateTime dateTime, OperationProgressResponse response)
    {
      Debug.Assert (null != response);
      var operation = response.Operation;
      Debug.Assert (null != operation);

      if (!response.EstimatedEndDateTime.HasValue) {
        ApproximateCompletionAndOperationEndFromSequences (dateTime, response);
      }
      else {
        var remainingTime = response.EstimatedEndDateTime.Value.Subtract (dateTime);

        // operation.MachiningDuration VS MAX(SUM(MachiningDuration))
        TimeSpan? totalRemainingTime = remainingTime; // In case operation.MachiningDuration != MAX(SUM(MachiningDuration))
        TimeSpan? totalDuration = operation.MachiningDuration; // Only considering the operation duration
        TimeSpan? machiningDuration = operation.MachiningDuration; // Considering both the sequence duration and the operation duration
        if (response.MachineModuleOperationProgress.Any ()) {
          var machineModuleProgressWithMachiningDuration = response.MachineModuleOperationProgress.Values
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
                log.Error ($"SetCompletionInOperationInProgress: maxMachiningDurationByMachineModule {maxMachiningDurationByMachineModule} is greater than operation machining duration {operation.MachiningDuration.Value}");
                string message = $"The sum of the sequence durations {maxMachiningDurationByMachineModule} is greater than the operation machining duration {operation.MachiningDuration.Value}";
                response.Errors.Add (message);
                machiningDuration = maxMachiningDurationByMachineModule;
                totalRemainingTime = null; // This is not possible to get a completion based on the operation duration then
              }
              else if (maxMachiningDurationByMachineModule < operation.MachiningDuration.Value) {
                if (1 == response.MachineModuleOperationProgress.Count) { // Unique machine module
                  log.Warn ($"SetCompletionInOperationInProgress: the sum of sequence durations {maxMachiningDurationByMachineModule} is less than operation machining duration {operation.MachiningDuration.Value} (and unique machine module) for operation id {((IDataWithId<int>)operation).Id}");
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
            && (1.0 <= totalDuration.Value.TotalSeconds)) {
          if (totalDuration.Value < totalRemainingTime) {
            log.Fatal ($"SetCompletionInOperationInProgress: the total remaining time {totalRemainingTime.Value} is greater that the total duration {totalDuration.Value} => fallback: completion is 0");
            response.Completion = 0;
          }
          response.Completion = (totalDuration.Value.TotalSeconds - totalRemainingTime.Value.TotalSeconds)
            / totalDuration.Value.TotalSeconds;
        }
        // Machining completion, based on machiningDuration and remainingTime
        if (machiningDuration.HasValue
            && (1.0 <= machiningDuration.Value.TotalSeconds)) {
          if (machiningDuration.Value < remainingTime) {
            log.Fatal ($"SetCompletionInOperationInProgress: the remaining time {remainingTime} is greater that the machining duration {machiningDuration.Value} => fallback: completion is 0");
            response.MachiningCompletion = 0;
          }
          response.MachiningCompletion = (machiningDuration.Value.TotalSeconds - remainingTime.TotalSeconds)
            / machiningDuration.Value.TotalSeconds;
        }
        // Note: Completion and MachiningCompletion differ if operation.MachiningDuration > MAX(SUM(MachiningDuration))
      }
    }

    (double, double) GetProportionMachiningRapid (ISequence currentSequence)
    {
      if (currentSequence is null) {
        log.Error ($"GetProportionMachiningRapid: currentSequence is null => return (0, 0)");
        return (0, 0);
      }

      if (!currentSequence.Kind.Equals (SequenceKind.Machining)) {
        return (0, 0);
      }
      else { // Machining
        var detail = currentSequence.Detail;
        if ( (null != detail) && (detail.MachiningTime.HasValue || detail.RapidTime.HasValue)) {
          var totalTimeSeconds = (detail.MachiningTime ?? 0.0) + (detail.RapidTime ?? 0.0) + (detail.NonMachiningTime ?? 0.0);
          if (0 < totalTimeSeconds) {
            var machining = detail.MachiningTime ?? 0.0 / totalTimeSeconds;
            var rapid = detail.RapidTime ?? 0.0 / totalTimeSeconds;
            if (log.IsDebugEnabled) {
              log.Debug ($"GetProportionMachiningRapid: use detail => ({machining}, {rapid})");
            }
            return (machining, rapid);
          }
        }

        {
          // Basic implementation to start, from configurations
          // Later, use detail of currentSequence
          var machining = Lemoine.Info.ConfigSet
            .LoadAndGet (PROPORTION_MACHINING_KEY, PROPORTION_MACHINING_DEFAULT);
          var rapid = Lemoine.Info.ConfigSet
            .LoadAndGet (PROPORTION_RAPID_KEY, PROPORTION_RAPID_DEFAULT);
          if (log.IsDebugEnabled) {
            log.Debug ($"GetProportionMachiningRapid: use a basic implementation using configurations => ({machining}, {rapid})");
          }
          return (machining, rapid);
        }
      }
    }

    bool IsAdjustmentCheckRequired (TimeSpan duration)
    {
      var adjustMinTime = Lemoine.Info.ConfigSet
        .LoadAndGet (ADJUST_WITH_OVERRIDE_MIN_TIME_KEY, ADJUST_WITH_OVERRIDE_MIN_TIME_DEFAULT);
      if (duration < adjustMinTime) {
        if (log.IsDebugEnabled) {
          log.Debug ($"IsAdjustmentCheckRequired: duration={duration} is lesser than min adjust time={adjustMinTime} => false");
        }
        return false;
      }
      else {
        return true;
      }
    }

    double GetSpeedFactor (IMachineModule machineModule, ISequence currentSequence)
    {
      var (machining, rapid) = GetProportionMachiningRapid (currentSequence);
      if ((0 == machining) && (0 == rapid)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetSpeedFactor: machining and rapid are 0 => 1.0");
        }
        return 1.0;
      }
      else {
        if ((1 < machining) || (machining < 0)) {
          log.Fatal ($"GetSpeedFactor: invalid machining proportion {machining} => 1.0");
          return 1.0;
        }
        if ((1 < rapid) || (rapid < 0)) {
          log.Fatal ($"GetSpeedFactor: invalid rapid proportion {rapid} => 1.0");
          return 1.0;
        }
        if (1 < (rapid + machining)) {
          log.Error ($"GetSpeedFactor: invalid propertion pair machining={machining} rapid={rapid} => 1.0");
          return 1.0;
        }
        var at = this.At ?? DateTime.UtcNow;
        var range = new UtcDateTimeRange (new LowerBound<DateTime> (), at, "(]");
        var lesserThan = Lemoine.Info.ConfigSet
          .LoadAndGet (OVERRIDE_LESSER_THAN_KEY, OVERRIDE_LESSER_THAN_DEFAULT);
        var greaterThan = Lemoine.Info.ConfigSet
          .LoadAndGet (OVERRIDE_GREATER_THAN_KEY, OVERRIDE_GREATER_THAN_DEFAULT);
        var feedrateOverride = 100.0;
        if (0 < machining) {
          var field = ModelDAOHelper.DAOFactory.FieldDAO.FindByCode ("FeedrateOverride");
          var feedrateOverrideCncValues = ModelDAOHelper.DAOFactory.CncValueDAO
            .FindFirstOverlapsRange (machineModule, field, range, 1, true);
          var fo = feedrateOverrideCncValues.FirstOrDefault ()?.Double;
          if (null != fo) {
            if ((fo.Value < lesserThan) || (greaterThan < fo.Value)) {
              feedrateOverride = fo.Value;
              if (log.IsDebugEnabled) {
                log.Debug ($"GetSpeedFactor: feedrate override={feedrateOverride}");
              }
            }
            else if (log.IsDebugEnabled) {
              log.Debug ($"GetSpeedFactor: feedrateOverride={fo.Value} in range=[{lesserThan},{greaterThan}] => do nothing");
            }
          }
          else if (log.IsDebugEnabled) {
            log.Debug ($"GetSpeedFactor: no feedrate override in {range} for machine module  {machineModule.Id}");
          }
        }
        var rapidOverride = 100.0;
        if (0 < rapid) {
          var field = ModelDAOHelper.DAOFactory.FieldDAO.FindByCode ("RapidTraverseOverride");
          var rapidOverrideCncValues = ModelDAOHelper.DAOFactory.CncValueDAO
            .FindFirstOverlapsRange (machineModule, field, range, 1, true);
          var ro = rapidOverrideCncValues.FirstOrDefault ()?.Double;
          if (null != ro) {
            if ((ro.Value < lesserThan) || (greaterThan < ro.Value)) {
              rapidOverride = ro.Value;
              if (log.IsDebugEnabled) {
                log.Debug ($"GetSpeedFactor: rapid override={rapidOverride}");
              }
            }
            else if (log.IsDebugEnabled) {
              log.Debug ($"GetSpeedFactor: rapidOverride={rapidOverride} in range=[{lesserThan},{greaterThan}] => do nothing");
            }
          }
          else if (log.IsDebugEnabled) {
            log.Debug ($"GetSpeedFactor: no rapid override in {range} for machine module  {machineModule.Id}");
          }
        }
        var factor = machining / (feedrateOverride / 100.0) + rapid / (rapidOverride / 100.0) + (1 - machining - rapid);
        if (log.IsDebugEnabled) {
          log.Debug ($"GetSpeedFactor: factor is {factor} with feedrateOverride={feedrateOverride} rapidOverride={rapidOverride}");
        }
        return factor;
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      string cacheKey = "Business.Operation.OperationProgress."
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
    public bool IsCacheValid (CacheValue<OperationProgressResponse> data)
    {
      return true;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (OperationProgressResponse data)
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
      foreach (var machineModuleData in data.MachineModuleOperationProgress.Values) {
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
  public enum OperationProgressResponseFlags
  {
    /// <summary>
    /// No specific flag
    /// </summary>
    None = 0,
    /// <summary>
    /// No effective operation at the requested time
    /// </summary>
    NoEffectiveOperation = 4,
  }

  /// <summary>
  /// Extensions to the reason source
  /// </summary>
  public static class OperationProgressResponseFlagsExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (OperationProgressResponseFlagsExtensions).ToString ());

    /// <summary>
    /// Check if other is included into t
    /// </summary>
    /// <param name="t"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public static bool HasFlag (this OperationProgressResponseFlags t, OperationProgressResponseFlags other)
    {
      return other == (t & other);
    }

    /// <summary>
    /// Add an option to another one (which may be null)
    /// </summary>
    /// <param name="t"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public static OperationProgressResponseFlags Add (this OperationProgressResponseFlags t, OperationProgressResponseFlags other)
    {
      return t | other;
    }

    /// <summary>
    /// Remove an option
    /// </summary>
    /// <param name="t"></param>
    /// <param name="toRemove"></param>
    /// <returns></returns>
    public static OperationProgressResponseFlags Remove (this OperationProgressResponseFlags t, OperationProgressResponseFlags toRemove)
    {
      return t & ~toRemove;
    }
  }

  /// <summary>
  /// Response structure to the OperationProgress request
  /// </summary>
  public class OperationProgressResponse : IProgressResponse
  {
    /// <summary>
    /// Time of the data
    /// </summary>
    public DateTime DataTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last update of the data (from a new sequence slot or a sequence milestone)
    /// </summary>
    public DateTime? LastUpdateDateTime { get; set; } = null;

    /// <summary>
    /// Associated flags
    /// </summary>
    public OperationProgressResponseFlags Flags { get; set; }

    /// <summary>
    /// Effective operation
    /// </summary>
    public IOperation Operation { get; set; }

    /// <summary>
    /// Progress by machine module
    /// 
    /// nullable
    /// </summary>
    public IDictionary<IMachineModule, OperationProgressByMachineModuleResponse> MachineModuleOperationProgress { get; set; } = null;

    /// <summary>
    /// Progress by machine module
    /// 
    /// nullable
    /// </summary>
    public IDictionary<IMachineModule, IProgressByMachineModuleResponse> MachineModuleProgress => this?.MachineModuleOperationProgress?.ToDictionary (x => x.Key, x => (IProgressByMachineModuleResponse)x.Value);

    /// <summary>
    /// Start date/time of the operation, if applicable
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
    /// Estimated end time of the operation (in the future)
    /// </summary>
    public DateTime? EstimatedEndDateTime { get; set; }

    /// <summary>
    /// Completion % (between 0 and 1) of the operation considering in priority the operation duration
    /// </summary>
    public double? Completion { get; set; }

    /// <summary>
    /// Completion % (between 0 and 1) of the operation considering in priority the machining periods
    /// </summary>
    public double? MachiningCompletion { get; set; }

    /// <summary>
    /// End date/time of the last operation if there is currently no effective operation (<see cref="Operation"/> is null)
    /// or max completion date/time of the last sequences
    /// </summary>
    public DateTime? CompletionDateTime { get; set; }

    /// <summary>
    /// The global operation is completed
    /// </summary>
    public bool Completed { get; set; } = false;

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
  public class OperationProgressByMachineModuleResponse : IProgressByMachineModuleResponse
  {
    /// <summary>
    /// Last update of the data (from a new sequence slot or a sequence milestone)
    /// </summary>
    public DateTime? LastUpdateDateTime { get; set; } = null;

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
    /// Is the current sequence completed?
    /// </summary>
    public bool CurrentSequenceCompleted { get; set; } = false;

    /// <summary>
    /// Current sequence completion date/time if completed
    /// </summary>
    public DateTime? CurrentSequenceCompletionDateTime { get; set; } = null;

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
    /// Estimated end time of the current operation (in the future)
    /// </summary>
    public DateTime? EstimatedEndDateTime { get; set; }

    /// <summary>
    /// Machining duration for this machine module considering the sum of the sequence duration
    /// and the speed factor
    /// </summary>
    public TimeSpan? MachiningDuration { get; set; }

    /// <summary>
    /// Machining duration for this machine module considering the sum of the sequence duration
    /// and not considering the speed factor
    /// </summary>
    public TimeSpan? NotAdjustedMachiningDuration { get; set; }

    /// <summary>
    /// Completion % (between 0 and 1) of the cycle
    /// </summary>
    public double? Completion { get; set; }

    /// <summary>
    /// Completed
    /// </summary>
    public bool Completed { get; set; } = false;

    /// <summary>
    /// Completion date/time if Completed is true
    /// </summary>
    public DateTime? CompletionDateTime { get; set; } = null;

    /// <summary>
    /// Speed factor that is based on the detected override for the machining sequences only
    /// </summary>
    public double? SpeedFactor { get; set; } = null;
  }
}
