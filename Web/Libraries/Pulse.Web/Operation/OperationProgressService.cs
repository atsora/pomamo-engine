// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Business.Operation;
using Lemoine.Core.Cache;
using Lemoine.Info;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Collections;
using Lemoine.Web;
using Pulse.Extensions.Web;

namespace Pulse.Web.Operation
{
  /// <summary>
  /// Cycle progress service
  /// </summary>
  public class OperationProgressService
    : GenericCachedService<OperationProgressRequestDTO>
  {
    static readonly string CURRENT_MACHINE_MODE_VALIDITY_KEY = "Web.Operation.OperationProgress.CurrentMachineModeValidity";
    static readonly TimeSpan CURRENT_MACHINE_MODE_VALIDITY_DEFAULT = TimeSpan.FromSeconds (20);

    static readonly string CACHE_AFTER_LAST_UPDATE_DATE_TIME_KEY = "Web.Operation.OperationProgress.CacheAfterLastUpdateDateTime";
    static readonly TimeSpan CACHE_AFTER_LAST_UPDATE_DATE_TIME_DEFAULT = TimeSpan.FromMinutes (1.8); // Because by default, there is an update of the milestone every two minutes

    static readonly ILog log = LogManager.GetLogger (typeof (OperationProgressService).FullName);

    readonly IDictionary<int, IEnumerable<IProgressExtension>> m_extensions = new Dictionary<int, IEnumerable<IProgressExtension>> ();

    /// <summary>
    /// Request date/time for the unit tests.
    /// 
    /// Default: DateTime.UtcNow
    /// </summary>
    internal DateTime? RequestDateTime { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public OperationProgressService ()
      : base (Lemoine.Core.Cache.CacheTimeOut.CurrentShort)
    {
    }

    #region Methods
    IEnumerable<IProgressExtension> GetExtensions (IMonitoredMachine machine)
    {
      Debug.Assert (null != machine);

      if (m_extensions.ContainsKey (machine.Id)) {
        return m_extensions[machine.Id];
      }
      else {
        IEnumerable<IProgressExtension> extensions;
        lock (m_extensions) {
          extensions = Lemoine.Business.ServiceProvider
            .Get (new Lemoine.Business.Extension
            .MonitoredMachineExtensions<IProgressExtension> (machine, (ext, m) => ext.Initialize (m)));
          m_extensions[machine.Id] = extensions;
        }
        return extensions;
      }
    }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache (OperationProgressRequestDTO request)
    {
      Debug.Assert (null != request);

      if (0 < request.MachineId) {
        return GetByMachine (request, request.MachineId);
      }
      else {
        return GetByGroup (request);
      }
    }

    object GetByGroup (OperationProgressRequestDTO request)
    {
      var groupId = request.GroupId;
      var groupRequest = new Lemoine.Business.Machine.GroupFromId (groupId);
      var group = Lemoine.Business.ServiceProvider
        .Get (groupRequest);

      if (null == group) {
        if (log.IsErrorEnabled) {
          log.Error ($"GetByGroup: group with id {request.GroupId} is not valid");
        }
        return new ErrorDTO ("Invalid group", ErrorStatus.WrongRequestParameter);
      }
      if (!group.SingleMachine) {
        if (log.IsErrorEnabled) {
          log.Error ($"GetByGroup: group with id {request.GroupId} is not supported (multi-machines)");
        }
        return new ErrorDTO ("Not supported group (multi-machines)", ErrorStatus.WrongRequestParameter);
      }

      var machine = group.GetMachines ().First ();
      return GetByMachine (request, machine.Id);
    }

    object GetByMachine (OperationProgressRequestDTO request, int machineId)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        IMonitoredMachine monitoredMachine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (machineId);
        if (null == monitoredMachine) {
          log.Error ($"GetByMachine: unknown monitored machine with ID {machineId}");
          return new ErrorDTO ("No monitored machine with the specified ID",
                               ErrorStatus.WrongRequestParameter);
        }

        var operationProgressRequest = new OperationProgress (monitoredMachine);
        if (this.RequestDateTime.HasValue) {
          operationProgressRequest.At = this.RequestDateTime.Value;
        }
        OperationProgressResponse operationProgressResponse = Lemoine.Business.ServiceProvider
          .Get (operationProgressRequest);

        var responseDTO = new OperationProgressResponseDTO ();
        var dataTime = operationProgressResponse.DataTime;
        responseDTO.DataTime = ConvertDTO.DateTimeUtcToIsoString (dataTime);
        if (operationProgressResponse.Flags.HasFlag (OperationProgressResponseFlags.NoEffectiveOperation)) {
          responseDTO.NoEffectiveOperation = true;
        }
        if (null != operationProgressResponse.Operation) {
          responseDTO.Operation = new OperationDTOAssembler ()
            .AssembleLongWithStandardDurations (operationProgressResponse.Operation);
        }
        if (operationProgressResponse.StartDateTime.HasValue) {
          responseDTO.OperationStartDateTime = ConvertDTO.DateTimeUtcToIsoString (operationProgressResponse.StartDateTime.Value);
        }
        if ((null != responseDTO.Operation) && !responseDTO.Operation.MachiningDuration.HasValue) {
          // Try to guess the machining duration from sequence durations
          var withSequences = operationProgressResponse.MachineModuleOperationProgress.Values
            .Where (mmp => null != mmp.Sequences)
            .Where (mmp => mmp.Sequences.Any (s => s.EstimatedTime.HasValue));
          if (withSequences.Any ()) {
            responseDTO.Operation.MachiningDuration = withSequences
              .Max (mmp => mmp.Sequences
                    .Where (s => s.EstimatedTime.HasValue)
                    .Sum (s => (int)s.EstimatedTime.Value.TotalSeconds));
          }
        }
        if ((null != operationProgressResponse.MachineModuleOperationProgress) && !request.Light) {
          responseDTO.ByMachineModule = new List<OperationProgressByMachineModuleDTO> ();
          foreach (var machineModuleProgress in operationProgressResponse.MachineModuleOperationProgress) {
            var item = GetOperationProgressByMachineModule (responseDTO, machineModuleProgress);
            responseDTO.ByMachineModule.Add (item);
          }
        }

        if (operationProgressResponse.Completion.HasValue) {
          responseDTO.Completion = RoundPercent (operationProgressResponse.Completion.Value); // Round the result to avoid getting a value greater than 1
        }
        else if (operationProgressResponse.EstimatedEndDateTime.HasValue
          && (null != operationProgressResponse.Operation)
          && operationProgressResponse.Operation.MachiningDuration.HasValue
          && (1 <= operationProgressResponse.Operation.MachiningDuration.Value.TotalSeconds)) { // Try to get it from cycle progress
          var remainingTime = operationProgressResponse.EstimatedEndDateTime.Value
            .Subtract (dataTime);
          if (remainingTime.Ticks < 0) {
            log.Error ($"GetByMachine: remaining time {remainingTime} is negative");
          }
          var completion = 1.0 -
            (remainingTime.TotalSeconds / operationProgressResponse.Operation.MachiningDuration.Value.TotalSeconds);
          if (log.IsDebugEnabled) {
            log.Debug ($"GetByMachine: estimate completion {completion} from estimatedCycleEndDateTime");
          }
          responseDTO.Completion = RoundPercent (completion);
        }

        if (operationProgressResponse.EstimatedEndDateTime.HasValue) {
          responseDTO.EstimatedOperationEndDateTime =
            ConvertDTO.DateTimeUtcToIsoString (operationProgressResponse.EstimatedEndDateTime.Value);
          responseDTO.EstimatedCycleEndDateTime = responseDTO.EstimatedOperationEndDateTime;
        }

        // Events
        if (request.IncludeEvents) {
          AddEvents (responseDTO, monitoredMachine, operationProgressResponse);
        }
        else {
          responseDTO.ActiveEvents = new List<EventDTO> ();
          responseDTO.ComingEvents = new List<EventDTO> ();
        }

        if (request.Light) {
          responseDTO.ByMachineModule = null;
        }

        responseDTO.Errors = operationProgressResponse.Errors.ToList ();
        responseDTO.Warnings = operationProgressResponse.Warnings.ToList ();
        responseDTO.Notices = operationProgressResponse.Notices.ToList ();

        // In case there is a coming event to display,
        // (and no active event)
        // return if the machine is currently active or not
        if (!responseDTO.ActiveEvents.Any ()
          && responseDTO.ComingEvents.Any ()) {
          var currentMachineMode = ModelDAOHelper.DAOFactory.CurrentMachineModeDAO
            .FindWithMachineMode (monitoredMachine);
          if (null != currentMachineMode) {
            var currentMachineModeValidity = Lemoine.Info.ConfigSet.LoadAndGet<TimeSpan> (CURRENT_MACHINE_MODE_VALIDITY_KEY,
              CURRENT_MACHINE_MODE_VALIDITY_DEFAULT);
            if (DateTime.UtcNow <= currentMachineMode.DateTime.Add (currentMachineModeValidity)) {
              responseDTO.Running = currentMachineMode.MachineMode.Running;
            }
            else {
              log.Warn ($"GetByMachine: current machine mode at {currentMachineMode.DateTime} is too old");
            }
          }
        }

        return responseDTO;
      }
    }

    OperationProgressByMachineModuleDTO GetOperationProgressByMachineModule (OperationProgressResponseDTO responseDTO, KeyValuePair<IMachineModule, OperationProgressByMachineModuleResponse> machineModuleProgress)
    {
      var item = new OperationProgressByMachineModuleDTO ();
      item.MachineModule = new MachineModuleDTO (machineModuleProgress.Key);
      if (null != machineModuleProgress.Value.CurrentSequence) {
        item.Current = new OperationProgressCurrentSequenceDTO (machineModuleProgress.Value.CurrentSequence);
        Debug.Assert (machineModuleProgress.Value.CurrentSequenceBeginDateTime.HasValue);
        item.Current.Begin = ConvertDTO
          .DateTimeUtcToIsoString (machineModuleProgress.Value.CurrentSequenceBeginDateTime.Value);
        var elapsed = machineModuleProgress.Value.CurrentSequenceElapsed;
        var completed = machineModuleProgress.Value.Completed;
        var currentSequenceDurationSeconds = item.Current.StandardDuration;
        if (completed) {
          item.Current.Completion = 1;
        }
        else if (elapsed.HasValue && currentSequenceDurationSeconds.HasValue && (1 <= currentSequenceDurationSeconds.Value)) {
          item.Current.Completion = RoundPercent (elapsed.Value.TotalSeconds / currentSequenceDurationSeconds.Value);
        }
      }
      item.Sequences = GetSequences (responseDTO, machineModuleProgress);
      return item;
    }

    List<OperationProgressSequenceDTO> GetSequences (OperationProgressResponseDTO responseDTO, KeyValuePair<IMachineModule, OperationProgressByMachineModuleResponse> machineModuleProgress)
    {
      var result = new List<OperationProgressSequenceDTO> ();
      double? positionPercent = 0;
      foreach (var sequence in machineModuleProgress.Value.Sequences) {
        var sequenceDto = new OperationProgressSequenceDTO (sequence);
        if (null != machineModuleProgress.Value.CurrentSequence) {
          sequenceDto.IsCurrent = (((IDataWithId<int>)sequence).Id == ((IDataWithId<int>)machineModuleProgress.Value.CurrentSequence).Id);
          sequenceDto.IsCompleted = (sequence.Order < machineModuleProgress.Value.CurrentSequence.Order);
          if (sequenceDto.IsCurrent && machineModuleProgress.Value.CurrentSequenceCompleted) {
            sequenceDto.IsCompleted = true;
          }
        }
        if (positionPercent.HasValue && (null != responseDTO.Operation) && responseDTO.Operation.MachiningDuration.HasValue
            && (0 < responseDTO.Operation.MachiningDuration.Value)) {
          sequenceDto.BeginPercent = RoundPercent (positionPercent.Value);
          if (sequenceDto.StandardDuration.HasValue) {
            var percent = sequenceDto.StandardDuration.Value / responseDTO.Operation.MachiningDuration.Value;
            positionPercent += percent;
            sequenceDto.EndPercent = RoundPercent (positionPercent.Value);
          }
          else {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetSequences: sequence with no standard duration => consider it as 0s");
            }
            sequenceDto.EndPercent = sequenceDto.BeginPercent;
          }
        }
        result.Add (sequenceDto);
      }
      return result;
    }

    double RoundPercent (double p)
    {
      return Math.Round (p, 3, MidpointRounding.AwayFromZero);
    }

    void AddEvents (OperationProgressResponseDTO responseDto, IMonitoredMachine machine, OperationProgressResponse operationProgressResponse)
    {
      var extensions = GetExtensions (machine);
      { // - coming
        IEnumerable<Event> comingEvents = extensions
          .SelectMany (ext => ext.GetComingEvents (operationProgressResponse));
        responseDto.ComingEvents = comingEvents
          .OrderBy (e => e.Severity.LevelValue)
          .OrderBy (e => e.DateTime)
          .Select (e => new EventDTO (e))
          .ToList ();
      }
      { // - active
        IEnumerable<Event> activeEvents = extensions
          .SelectMany (ext => ext.GetActiveEvents (operationProgressResponse));
        responseDto.ActiveEvents = activeEvents
          .OrderBy (e => e.Severity.LevelValue)
          .Select (e => new EventDTO (e))
          .ToList ();
      }
    }

    /// <summary>
    /// Get the cache time out (Asp service only)
    /// </summary>
    /// <param name="pathQuery"></param>
    /// <param name="requestDTO"></param>
    /// <param name="outputDTO"></param>
    /// <returns></returns>
    protected override TimeSpan GetCacheTimeOut (string pathQuery, OperationProgressRequestDTO requestDTO, object outputDTO)
    {
      var defaultTimeout = GetCacheTimeOut (pathQuery, requestDTO);

      OperationProgressResponseDTO responseDto;
      try {
        responseDto = (OperationProgressResponseDTO)outputDTO;
      }
      catch (Exception ex) {
        log.Error ($"GetCacheTimeout: {outputDTO} is not of type {typeof (OperationProgress)} => return {defaultTimeout}", ex);
        return defaultTimeout;
      }

      var utcNow = DateTime.UtcNow;
      var limit = utcNow.Add (defaultTimeout);
      if (null == responseDto.ComingEvents) {
        log.Error ($"GetCacheTimeout: coming events is null");
      }
      else {
        foreach (var e in responseDto.ComingEvents) {
          UpdateIfResponseDateTimeBefore (e.DateTime, ref limit);
        }
      }
      if (!string.IsNullOrEmpty (responseDto.EstimatedOperationEndDateTime)) {
        UpdateIfResponseDateTimeBefore (responseDto.EstimatedOperationEndDateTime, ref limit);
        responseDto.EstimatedCycleEndDateTime = responseDto.EstimatedOperationEndDateTime;
      }
      if (null != responseDto.ByMachineModule) {
        foreach (var machineModuleData in responseDto.ByMachineModule) {
          var currentSequence = machineModuleData.Current;
          if ((null != currentSequence)
            && currentSequence.StandardDuration.HasValue
            && DateTime.TryParse (currentSequence.Begin, out DateTime currentSequenceStart)) {
            var estimatedEnd = currentSequenceStart.AddSeconds (currentSequence.StandardDuration.Value);
            UpdateIfResponseDateTimeBefore (estimatedEnd, ref limit);
          }
        }
      }

      // Consider the limit to be maximum 2 minutes after the last update date/time if any
      if (!string.IsNullOrEmpty (responseDto.LastUpdateDateTime)) {
        var cacheAfterLastUpdateDateTime = Lemoine.Info.ConfigSet
          .LoadAndGet (CACHE_AFTER_LAST_UPDATE_DATE_TIME_KEY, CACHE_AFTER_LAST_UPDATE_DATE_TIME_DEFAULT);
        UpdateIfResponseDateTimeBefore (responseDto.LastUpdateDateTime, ref limit, cacheAfterLastUpdateDateTime);
      }

      return (utcNow < limit)
        ? limit.Subtract (utcNow)
        : TimeSpan.FromSeconds (0);
    }

    bool UpdateIfResponseDateTimeBefore (DateTime parsedResponseDateTime, ref DateTime dateTime)
    {
      if (parsedResponseDateTime < dateTime) {
        dateTime = parsedResponseDateTime;
        return true;
      }
      else {
        return false;
      }
    }

    bool UpdateIfResponseDateTimeBefore (string responseDateTime, ref DateTime dateTime)
    {
      DateTime parsedResponseDateTime;
      try {
        parsedResponseDateTime = DateTime.Parse (responseDateTime);
      }
      catch (Exception ex) {
        log.Error ($"UpdateIfResponseDateTimeBefore: date/time {responseDateTime} could not be parsed, skip it", ex);
        return false;
      }
      return UpdateIfResponseDateTimeBefore (parsedResponseDateTime, ref dateTime);
    }

    bool UpdateIfResponseDateTimeBefore (string responseDateTime, ref DateTime dateTime, TimeSpan delta)
    {
      DateTime parsedResponseDateTime;
      try {
        parsedResponseDateTime = DateTime.Parse (responseDateTime);
      }
      catch (Exception ex) {
        log.Error ($"UpdateIfResponseDateTimeBefore: date/time {responseDateTime} could not be parsed, skip it", ex);
        return false;
      }
      return UpdateIfResponseDateTimeBefore (parsedResponseDateTime.Add (delta), ref dateTime);
    }
    #endregion // Methods
  }
}
