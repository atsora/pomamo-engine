// Copyright (C) 2009-2023 Lemoine Automation Technologies
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
using Lemoine.Web.CommonRequestDTO;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Collections;
using System.Threading;
using Lemoine.Web;
using Pulse.Extensions.Web;

namespace Pulse.Web.Operation
{
  /// <summary>
  /// Cycle progress service
  /// </summary>
  public class CycleProgressService
    : GenericCachedService<CycleProgressRequestDTO>
  {
    static readonly string CURRENT_MACHINE_MODE_VALIDITY_KEY = "Web.Operation.CycleProgress.CurrentMachineModeValidity";
    static readonly TimeSpan CURRENT_MACHINE_MODE_VALIDITY_DEFAULT = TimeSpan.FromSeconds (20);

    static readonly ILog log = LogManager.GetLogger (typeof (CycleProgressService).FullName);

    readonly IDictionary<int, IEnumerable<IProgressExtension>> m_extensions = new Dictionary<int, IEnumerable<IProgressExtension>> ();
    readonly SemaphoreSlim m_semaphore = new SemaphoreSlim (1, 1);

    /// <summary>
    /// Request date/time for the unit tests.
    /// 
    /// Default: DateTime.UtcNow
    /// </summary>
    internal DateTime? RequestDateTime { get; set; }

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public CycleProgressService ()
      : base (Lemoine.Core.Cache.CacheTimeOut.CurrentShort)
    {
    }
    #endregion // Constructors

    #region Methods
    IEnumerable<IProgressExtension> GetExtensions (IMonitoredMachine machine)
    {
      Debug.Assert (null != machine);

      if (m_extensions.ContainsKey (machine.Id)) {
        return m_extensions[machine.Id];
      }
      else {
        IEnumerable<IProgressExtension> extensions;
        using (var semaphoreHolder = Lemoine.Threading.SemaphoreSlimHolder.Create (m_semaphore)) {
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
    public override object GetWithoutCache (CycleProgressRequestDTO request)
    {
      Debug.Assert (null != request);

      if (0 < request.MachineId) {
        return GetByMachine (request, request.MachineId);
      }
      else {
        return GetByGroup (request);
      }
    }

    object GetByGroup (CycleProgressRequestDTO request)
    {
      var groupId = request.GroupId;
      var groupRequest = new Lemoine.Business.Machine.GroupFromId (groupId);
      var group = Lemoine.Business.ServiceProvider
        .Get (groupRequest);

      if (null == group) {
        if (log.IsErrorEnabled) {
          log.ErrorFormat ("GetWithoutCache: group with id {0} is not valid",
            request.GroupId);
        }
        return new ErrorDTO ("Invalid group", ErrorStatus.WrongRequestParameter);
      }
      if (!group.SingleMachine) {
        if (log.IsErrorEnabled) {
          log.ErrorFormat ("GetWithoutCache: group with id {0} is not supported (multi-machines)",
            request.GroupId);
        }
        return new ErrorDTO ("Not supported group (multi-machines)", ErrorStatus.WrongRequestParameter);
      }

      var machine = group.GetMachines ().First ();
      return GetByMachine (request, machine.Id);
    }

    object GetByMachine (CycleProgressRequestDTO request, int machineId)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        IMonitoredMachine monitoredMachine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (machineId);
        if (null == monitoredMachine) {
          log.ErrorFormat ("GetWithoutCache: " +
                           "unknown monitored machine with ID {0}",
                           machineId);
          return new ErrorDTO ("No monitored machine with the specified ID",
                               ErrorStatus.WrongRequestParameter);
        }

        var cycleProgressRequest = new CycleProgress (monitoredMachine);
        if (this.RequestDateTime.HasValue) {
          cycleProgressRequest.At = this.RequestDateTime.Value;
        }
        CycleProgressResponse cycleProgressResponse = Lemoine.Business.ServiceProvider
          .Get (cycleProgressRequest);
        if (cycleProgressResponse.Flags.HasFlag (CycleProgressResponseFlags.NotApplicable)) {
          if (log.IsInfoEnabled) {
            log.InfoFormat ("GetByMachine: not applicable for machine {0}", machineId);
          }
          return new ErrorDTO ("Not applicable (no cycle is detected on this machine)", ErrorStatus.NotApplicable);
        }

        var responseDTO = new CycleProgressResponseDTO ();
        var now = RequestDateTime ?? DateTime.UtcNow;
        responseDTO.DataTime = ConvertDTO.DateTimeUtcToIsoString (now);
        if (cycleProgressResponse.Flags.HasFlag (CycleProgressResponseFlags.NoEffectiveOperation)) {
          responseDTO.NoEffectiveOperation = true;
        }
        if (cycleProgressResponse.Flags.HasFlag (CycleProgressResponseFlags.InvalidCycle)) {
          responseDTO.InvalidCycle = true;
        }
        if (null != cycleProgressResponse.Operation) {
          responseDTO.Operation = new OperationDTOAssembler ()
            .AssembleLongWithStandardDurations (cycleProgressResponse.Operation);
        }
        if (cycleProgressResponse.StartDateTime.HasValue) {
          responseDTO.CycleStartDateTime = ConvertDTO.DateTimeUtcToIsoString (cycleProgressResponse.StartDateTime.Value);
        }
        if ((null != responseDTO.Operation) && !responseDTO.Operation.MachiningDuration.HasValue) {
          // Try to guess the machining duration from sequence durations
          var withSequences = cycleProgressResponse.MachineModuleCycleProgress.Values
            .Where (mmp => null != mmp.Sequences)
            .Where (mmp => mmp.Sequences.Any (s => s.EstimatedTime.HasValue));
          if (withSequences.Any ()) {
            responseDTO.Operation.MachiningDuration = withSequences
              .Max (mmp => mmp.Sequences
                    .Where (s => s.EstimatedTime.HasValue)
                    .Sum (s => (int)s.EstimatedTime.Value.TotalSeconds));
          }
        }
        if ((null != cycleProgressResponse.MachineModuleCycleProgress) && !request.Light) {
          responseDTO.ByMachineModule = new List<CycleProgressByMachineModuleDTO> ();
          foreach (var machineModuleProgress in cycleProgressResponse.MachineModuleCycleProgress) {
            var item = GetCycleProgressByMachineModule (now, responseDTO, machineModuleProgress);
            responseDTO.ByMachineModule.Add (item);
          }
        }

        if (cycleProgressResponse.Completion.HasValue) {
          responseDTO.Completion = RoundPercent (cycleProgressResponse.Completion.Value); // Round the result to avoid getting a value greater than 1
        }
        else if (cycleProgressResponse.EstimatedEndDateTime.HasValue
          && (null != cycleProgressResponse.Operation)
          && cycleProgressResponse.Operation.MachiningDuration.HasValue
          && (0 < cycleProgressResponse.Operation.MachiningDuration.Value.TotalSeconds)
          && (null != cycleProgressResponse.OperationCycle)) { // Try to get it from cycle progress
          var remainingTime = cycleProgressResponse.EstimatedEndDateTime.Value
            .Subtract (now);
          if (remainingTime.Ticks < 0) {
            log.Error ($"GetWithoutCache: remaining time {remainingTime} is negative");
          }
          var completion = 1.0 -
            (remainingTime.TotalSeconds / cycleProgressResponse.Operation.MachiningDuration.Value.TotalSeconds);
          if (log.IsDebugEnabled) {
            log.Debug ($"GetWithoutCache: estimate completion {completion} from estimatedCycleEndDateTime");
          }
          responseDTO.Completion = RoundPercent (completion);
        }

        if (cycleProgressResponse.EstimatedEndDateTime.HasValue) {
          responseDTO.EstimatedCycleEndDateTime =
            ConvertDTO.DateTimeUtcToIsoString (cycleProgressResponse.EstimatedEndDateTime.Value);
        }

        // Events
        if (request.IncludeEvents) {
          AddEvents (responseDTO, monitoredMachine, cycleProgressResponse);
        }
        else {
          responseDTO.ActiveEvents = new List<EventDTO> ();
          responseDTO.ComingEvents = new List<EventDTO> ();
        }

        if (request.Light) {
          responseDTO.ByMachineModule = null;
        }

        responseDTO.Errors = cycleProgressResponse.Errors.ToList ();
        responseDTO.Warnings = cycleProgressResponse.Warnings.ToList ();
        responseDTO.Notices = cycleProgressResponse.Notices.ToList ();

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
              log.WarnFormat ("GetWithoutCache: current machine mode at {0} is too old", currentMachineMode.DateTime);
            }
          }
        }

        return responseDTO;
      }
    }

    CycleProgressByMachineModuleDTO GetCycleProgressByMachineModule (DateTime now, CycleProgressResponseDTO responseDTO, KeyValuePair<IMachineModule, CycleProgressByMachineModuleResponse> machineModuleProgress)
    {
      var item = new CycleProgressByMachineModuleDTO ();
      item.MachineModule = new MachineModuleDTO (machineModuleProgress.Key);
      if (null != machineModuleProgress.Value.CurrentSequence) {
        item.Current = new CycleProgressCurrentSequenceDTO (machineModuleProgress.Value.CurrentSequence);
        Debug.Assert (machineModuleProgress.Value.CurrentSequenceBeginDateTime.HasValue);
        item.Current.Begin = ConvertDTO
          .DateTimeUtcToIsoString (machineModuleProgress.Value.CurrentSequenceBeginDateTime.Value);
        var currentSequenceDurationSeconds = item.Current.StandardDuration;
        if (currentSequenceDurationSeconds.HasValue && (0 < currentSequenceDurationSeconds.Value)) {
          var elapsed = now.Subtract (machineModuleProgress.Value.CurrentSequenceBeginDateTime.Value);
          item.Current.Completion = RoundPercent (elapsed.TotalSeconds / currentSequenceDurationSeconds.Value);
        }
      }
      item.Sequences = GetSequences (responseDTO, machineModuleProgress);
      return item;
    }

    List<CycleProgressSequenceDTO> GetSequences (CycleProgressResponseDTO responseDTO, KeyValuePair<IMachineModule, CycleProgressByMachineModuleResponse> machineModuleProgress)
    {
      var result = new List<CycleProgressSequenceDTO> ();
      double? positionPercent = 0;
      foreach (var sequence in machineModuleProgress.Value.Sequences) {
        var sequenceDto = new CycleProgressSequenceDTO (sequence);
        if (null != machineModuleProgress.Value.CurrentSequence) {
          sequenceDto.IsCurrent = (((IDataWithId<int>)sequence).Id == ((IDataWithId<int>)machineModuleProgress.Value.CurrentSequence).Id);
          sequenceDto.IsCompleted = (sequence.Order < machineModuleProgress.Value.CurrentSequence.Order);
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
              log.Debug ($"GetSequences: sequence with no standard duration => consider it as 0s better than giving up");
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

    void AddEvents (CycleProgressResponseDTO responseDto, IMonitoredMachine machine, CycleProgressResponse cycleProgressResponse)
    {
      var extensions = GetExtensions (machine);
      { // - coming
        IEnumerable<Event> comingEvents = extensions
          .SelectMany (ext => ext.GetComingEvents (cycleProgressResponse));
        responseDto.ComingEvents = comingEvents
          .OrderBy (e => e.Severity.LevelValue)
          .OrderBy (e => e.DateTime)
          .Select (e => new EventDTO (e))
          .ToList ();
      }
      { // - active
        IEnumerable<Event> activeEvents = extensions
          .SelectMany (ext => ext.GetActiveEvents (cycleProgressResponse));
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
    protected override TimeSpan GetCacheTimeOut (string pathQuery, CycleProgressRequestDTO requestDTO, object outputDTO)
    {
      var defaultTimeout = GetCacheTimeOut (pathQuery, requestDTO);

      CycleProgressResponseDTO responseDto;
      try {
        responseDto = (CycleProgressResponseDTO)outputDTO;
      }
      catch (Exception ex) {
        log.Error ($"GetCacheTimeout: {outputDTO} is not of type {typeof (CycleProgress)} => return {defaultTimeout}", ex);
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
      if (!string.IsNullOrEmpty (responseDto.EstimatedCycleEndDateTime)) {
        UpdateIfResponseDateTimeBefore (responseDto.EstimatedCycleEndDateTime, ref limit);
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
        log.Error ($"IsStringDateTimeBefore: date/time {responseDateTime} could not be parsed, skip it", ex);
        return false;
      }
      return UpdateIfResponseDateTimeBefore (parsedResponseDateTime, ref dateTime);
    }
    #endregion // Methods
  }
}
