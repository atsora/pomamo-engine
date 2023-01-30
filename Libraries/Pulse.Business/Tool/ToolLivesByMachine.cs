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
using Lemoine.Business.Operation;
using System.Threading.Tasks;
using Lemoine.Collections;

namespace Lemoine.Business.Tool
{
  /// <summary>
  /// Description of CycleProgress.
  /// </summary>
  public sealed class ToolLivesByMachine : IRequest<ToolLivesByMachineResponse>
  {
    /// <summary>
    /// Use the cycle or operation progress to determine the estimated time to the tool expiration
    /// 
    /// To be used if the tool are changed at the end of the cycle
    /// </summary>
    static readonly string USE_PROGRESS_KEY = "Tool.Expiration.Progress";
    static readonly string USE_PROGRESS_DEFAULT = ""; // Or cycle or operation

    /// <summary>
    /// Confirm the tool expiration is really by part and not by cycle if the used unit is NumberOfParts
    /// </summary>
    static readonly string TOOL_EXPIRATION_BY_PART_KEY = "Tool.Expiration.ByPart";
    static readonly bool TOOL_EXPIRATION_BY_PART_DEFAULT = true;

    /// <summary>
    /// Confirm the tool expiration is really by number of times and not by cycle if the used unit is NumberOfTimes
    /// </summary>
    static readonly string TOOL_EXPIRATION_BY_NB_OF_TIMES_KEY = "Tool.Expiration.ByNumberOfTimes";
    static readonly bool TOOL_EXPIRATION_BY_NB_OF_TIMES_DEFAULT = true;

    static readonly string EFFECTIVE_OPERATION_MAX_AGE_KEY = "Tool.Expiration.EffectiveOperationMaxAge";
    static readonly TimeSpan EFFECTIVE_OPERATION_MAX_AGE_DEFAULT = TimeSpan.FromDays (7);

    static readonly string MIN_CYCLE_NUMBER_FOR_CYCLE_PROGRESS_KEY = "Tool.Expiration.MinCycleNumberForCycleProgress";
    static readonly int MIN_CYCLE_NUMBER_FOR_CYCLE_PROGRESS_DEFAULT = 1;

    #region Members
    IProgressResponse m_progress = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (ToolLivesByMachine).FullName);

    #region Getters / Setters
    /// <summary>
    /// Reference to the machine
    /// </summary>
    public IMonitoredMachine Machine { get; internal set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    public ToolLivesByMachine (IMonitoredMachine machine)
    {
      Debug.Assert (null != machine);

      this.Machine = machine;
    }

    /// <summary>
    /// Maximum expiration time
    /// </summary>
    public TimeSpan? MaxExpirationTime
    {
      get; set;
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns>ToolLivesByMachineResponse</returns>
    public ToolLivesByMachineResponse Get ()
    {
      Debug.Assert (null != this.Machine);
      m_progress = null;

      if (!ModelDAOHelper.DAOFactory.IsInitialized (this.Machine)) {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (var transaction = session.BeginReadOnlyTransaction ("Business.ToolLivesByMachine.Machine")) {
            this.Machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (((IDataWithId)this.Machine).Id);
            ModelDAOHelper.DAOFactory.Initialize (this.Machine.MachineModules);
          }
        }
      }
      else if (!ModelDAOHelper.DAOFactory.IsInitialized (this.Machine.MachineModules)) {
        try {
          using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
            using (var transaction = session.BeginReadOnlyTransaction ("Business.ToolLivesByMachine.MachineModules")) {
              ModelDAOHelper.DAOFactory.MonitoredMachineDAO.Lock (this.Machine);
              ModelDAOHelper.DAOFactory.Initialize (this.Machine.MachineModules);
            }
          }
        }
        catch (Exception ex) {
          if (Lemoine.Core.ExceptionManagement.ExceptionTest.IsStale (ex)) {
            log.WarnFormat ("Get: machine {0} is stale, try to reload it", this.Machine.Id);
            using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
              using (var transaction = session.BeginReadOnlyTransaction ("Business.ToolLivesByMachine.MachineModules.Stale")) {
                this.Machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (((IDataWithId)this.Machine).Id);
                ModelDAOHelper.DAOFactory.Initialize (this.Machine.MachineModules);
              }
            }
          }
          else {
            throw;
          }
        }
      }

      var response = new ToolLivesByMachineResponse ();

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        // Find the current operation of the machine
        var effectiveOperationMaxAge = Lemoine.Info.ConfigSet
          .LoadAndGet<TimeSpan> (EFFECTIVE_OPERATION_MAX_AGE_KEY,
                                 EFFECTIVE_OPERATION_MAX_AGE_DEFAULT);
        var lastEffectiveOperationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO
          .GetLastEffective (this.Machine, effectiveOperationMaxAge);
        if ((null != lastEffectiveOperationSlot) && (null != lastEffectiveOperationSlot.Operation)) {
          response.Operation = lastEffectiveOperationSlot.Operation;
        }

        // - Get the raw tool lives
        IEnumerable<IToolLife> toolLifes = ModelDAOHelper.DAOFactory.ToolLifeDAO
          .FindAllByMachine (this.Machine); // Correct spelling toolLives, but more convenient for search/replace
        if (null != response.Operation) { // Restrict the tool lives to the active tools of the active machine
          var operationTools = Lemoine.Business.ServiceProvider
            .Get<IEnumerable<string>> (new OperationTools (response.Operation)); // List of tool# that are associated to the active operation
          if (operationTools.Any ()) { // If no tool was associated to this operation, do not apply this filter
            toolLifes = toolLifes.Where (tl => operationTools.Contains (tl.Position.ToolNumber));
          }
          else {
            log.InfoFormat ("Get: operationId {0} does not reference any tool", ((IDataWithId)response.Operation).Id);
          }
        }

        // - Keep only one tool life per toolid
        toolLifes = toolLifes
          .GroupBy (t => t.Position.ToolId)
          .Select (x => x.First ());

        // - Initialization
        response.Tools = CreateToolLifeResponseList (toolLifes);
        CompleteExpiredWarningStatus (response);
        SetSisterToolProperties (response);

        response.DateTime = DateTime.UtcNow;

        // - Estimate the remaining time before expiration
        if (response.Tools.Any (t => IsToolLifeByCycle (t.ToolLife) && !t.Expired)) {
          SetRemainingTimeIfByCycle (response);
        }
        if ((null != response.Operation) && response.Tools.Any (t => IsToolLifeByDuration (t.ToolLife) && !t.Expired)) {
          SetRemainingTimeIfByDuration (response);
        }

        // - Filter the data
        if (this.MaxExpirationTime.HasValue) {
          FilterWithMaxExpirationTime (response);
        }

        // - Create the groups
        CreateGroups (response);

        // - Complete expirationDateTime if it has not been set yet
        CompleteExpirationDateTime (response);

        // - Summary by machine
        GetSummaryByMachine (response);

        // - Sort the data
        Sort (response);

        return response;
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async Task<ToolLivesByMachineResponse> GetAsync ()
    {
      return await Task.FromResult (Get ());
    }

    List<ToolLifeResponse> CreateToolLifeResponseList (IEnumerable<IToolLife> toolLifes)
    {
      var toolResponseList = new List<ToolLifeResponse> ();
      foreach (var toolLife in toolLifes) {
        var toolLifeResponse = new ToolLifeResponse ();
        toolLifeResponse.ToolLife = toolLife;
        toolLifeResponse.Display = toolLife.Position.Display;
        toolResponseList.Add (toolLifeResponse);
      }
      return toolResponseList;
    }

    void CompleteExpiredWarningStatus (ToolLivesByMachineResponse response)
    {
      foreach (var toolLifeResponse in response.Tools) {
        CompleteExpiredWarningStatus (toolLifeResponse);
      }
    }

    void CompleteExpiredWarningStatus (ToolLifeResponse toolLifeResponse)
    {
      var toolLife = toolLifeResponse.ToolLife;
      Debug.Assert (null != toolLife);
      if (toolLife.IsLimitReached ()) { // Limit reached
        toolLifeResponse.Expired = true;

        // Try to get the date/time of the expiration
        var toolLifeEvent = ModelDAOHelper.DAOFactory.EventToolLifeDAO
          .FindLastDateTimeOfAnEvent (toolLife.MachineModule, EventToolLifeType.ExpirationReached,
                                     toolLife.Position.ToolId);
        if (null != toolLifeEvent) {
          log.DebugFormat ("CompleteExpiredWarningStatus: " +
                           "expiration date/time of {0} was {1}",
                           toolLife.Position.ToolId, toolLifeEvent.DateTime);
          toolLifeResponse.ExpirationDateTime = toolLifeEvent.DateTime;
          toolLifeResponse.ExpirationDateTimeRange = new UtcDateTimeRange (toolLifeEvent.DateTime,
                                                                           toolLifeEvent.DateTime,
                                                                           "[]");
        }
        else {
          log.WarnFormat ("CompleteExpiredWarningStatus: no tool life event ExpirationReached to get the real expiration date/time");
        }
      }
      else { // Limit not reached
        toolLifeResponse.Warning = toolLife.IsWarningReached ();
      }
    }

    void SetSisterToolProperties (ToolLivesByMachineResponse response)
    {
      foreach (var group in response.Tools
        .GroupBy (t => GetGroupIdentifier (t.ToolLife))
        .Where (g => 1 < g.Count ())) {
        var toolResponses = group.OrderBy (t => GetGroupSortCriterium (t.ToolLife));
        foreach (var toolResponse in toolResponses) {
          toolResponse.ActiveSisterTool = false;
        }
        var active = toolResponses.FirstOrDefault (t => !t.Expired && !t.Group);
        if (null != active) {
          active.ActiveSisterTool = true;
          if (active.ValidSisterTools) {
            var next = toolResponses
              .FirstOrDefault (t => !t.Expired && !t.Group && (t != active));
            if (null != next) {
              active.Next = next.Display;
            }
            else {
              log.ErrorFormat ("SetActiveSisterTool: next ist null for tool {0}", active.Display);
            }
          }
        }
        // ValidSisterTools
        if (toolResponses.Any (t => !t.Expired)) {
          var count = toolResponses.Count ();
          foreach (var toolResponse in toolResponses.Take (count - 1)) { // Except the last one
            toolResponse.ValidSisterTools = true;
          }
        }
      }
    }

    void SetRemainingTimeIfByCycle (ToolLivesByMachineResponse response)
    {
      var responseToolsByCycle = response.Tools.Where (t => IsToolLifeByCycle (t.ToolLife) && !t.Expired);

      // - Get the number of cycles first
      var numberOfPiecesByCycle = -1;
      if ((null != response.Operation)
          && responseToolsByCycle.Any (t => IsToolLifeByNumberOfParts (t.ToolLife))) {
        numberOfPiecesByCycle = response.Operation.GetTotalNumberOfIntermediateWorkPieces ();
      }
      foreach (var toolResponse in responseToolsByCycle) {
        double? remainingLifeToLimit = toolResponse.ToolLife.GetRemainingLifeToLimit ();
        if (remainingLifeToLimit.HasValue) {
          SetRemainingCyclesToLimitIfByCycle (response, numberOfPiecesByCycle, toolResponse, remainingLifeToLimit.Value);
        }
      }
      // - Estimate the time from the number of remaining cycles
      if ((null != response.Operation)
          && responseToolsByCycle.Any (t => 0 < t.RemainingCyclesToLimit)) {
        SetRemainingTimeIfByCycleWithRemainingCyclesToLimit (response);
      }
      // - Adjust the remaining time for the groups
      var responseToolsByCycleWithRemainingTime = responseToolsByCycle.Where (t => (null != t.RemainingTimeRange) && !t.RemainingTimeRange.IsEmpty ());
      foreach (var toolGroup in responseToolsByCycleWithRemainingTime.GroupBy (t => GetGroupIdentifier (t.ToolLife))) {
        var tools = toolGroup.OrderBy (t => GetGroupSortCriterium (t.ToolLife));
        var activeTool = tools.First ();
        var remainingTime = activeTool.RemainingTime;
        Debug.Assert (!activeTool.RemainingTimeRange.IsEmpty ());
        var lowerRemainingTime = activeTool.RemainingTimeRange.Lower;
        var upperRemainingTime = activeTool.RemainingTimeRange.Upper;
        foreach (var tool in toolGroup.OrderBy (t => GetGroupSortCriterium (t.ToolLife)).Skip (1)) {
          if (!tool.RemainingTime.HasValue) {
            remainingTime = null;
          }
          else if (remainingTime.HasValue) {
            remainingTime = remainingTime.Value.Add (tool.RemainingTime.Value);
          }
          tool.RemainingTime = remainingTime;
          if (lowerRemainingTime.HasValue && tool.RemainingTimeRange.Lower.HasValue) {
            lowerRemainingTime = lowerRemainingTime.Value.Add (tool.RemainingTimeRange.Lower.Value);
          }
          if (upperRemainingTime.HasValue && tool.RemainingTimeRange.Upper.HasValue) {
            upperRemainingTime = upperRemainingTime.Value.Add (tool.RemainingTimeRange.Upper.Value);
          }
          if (Bound<TimeSpan>.Equals (lowerRemainingTime, upperRemainingTime)) {
            Debug.Assert (lowerRemainingTime.HasValue);
            if (!tool.RemainingTime.HasValue) {
              tool.RemainingTime = lowerRemainingTime.Value;
            }
            tool.RemainingTimeRange = new Range<TimeSpan> (lowerRemainingTime, upperRemainingTime, "[]");
          }
          else {
            tool.RemainingTimeRange = new Range<TimeSpan> (lowerRemainingTime, upperRemainingTime);
          }
        }
      }
    }

    void SetRemainingCyclesToLimitIfByCycle (ToolLivesByMachineResponse response, int numberOfPiecesByCycle, ToolLifeResponse toolResponse, double remainingLifeToLimit)
    {
      if ((null != response.Operation)
          && (-1 != numberOfPiecesByCycle)
          && IsToolLifeByNumberOfParts (toolResponse.ToolLife)) {
        SetRemainingCyclesToLimitIfByNumberOfParts (toolResponse, remainingLifeToLimit, numberOfPiecesByCycle);
      }
      else if ((null != response.Operation)
          && IsToolLifeByNumberOfTimes (toolResponse.ToolLife)) {
        SetRemainingCyclesToLimitIfByNumberOfTimes (toolResponse, response.Operation, remainingLifeToLimit);
      }
      else {
        toolResponse.RemainingCyclesToLimit = (int)remainingLifeToLimit;
      }
    }

    void SetRemainingCyclesToLimitIfByNumberOfParts (ToolLifeResponse toolResponse, double remainingLifeToLimit, int numberOfPiecesByCycle)
    {
      if (0 == numberOfPiecesByCycle) {
        log.ErrorFormat ("SetRemainingCyclesToLimitIfByCycle: " +
                         "the configured operation quantity is 0, please correct it");
        toolResponse.RemainingCyclesToLimit = (int)remainingLifeToLimit;
      }
      else {
        toolResponse.RemainingCyclesToLimit = (int)remainingLifeToLimit / numberOfPiecesByCycle;
      }
    }

    void SetRemainingCyclesToLimitIfByNumberOfTimes (ToolLifeResponse toolResponse, IOperation operation, double remainingLifeToLimit)
    {
      string toolNumber = toolResponse.ToolLife.Position.ToolNumber;
      var sequences = Lemoine.Business.ServiceProvider
        .Get (new Lemoine.Business.Tool.OperationToolSequences (operation, toolNumber));
      var times = sequences.Count ();
      if (0 == times) {
        log.ErrorFormat ("SetRemainingCyclesToLimitIfByNumberOfTimes: " +
                         "the number of sequences with tool# {0} is 0, please correct it", toolNumber);
        // This should not happen, because only the tools that are in one of the sequences are considered here
        toolResponse.RemainingCyclesToLimit = (int)remainingLifeToLimit;
      }
      else { // This is an approximation for the moment... we could be more precise, but not necessary right now
        toolResponse.RemainingCyclesToLimit = (int)remainingLifeToLimit / times;
      }
    }

    void SetRemainingTimeIfByCycleWithRemainingCyclesToLimit (ToolLivesByMachineResponse response)
    {
      var cycleDuration = response.Operation.GetStandardCycleDuration (this.Machine);
      if (cycleDuration.HasValue) {
        var cycleProgress = GetProgress ();
        foreach (var toolResponse in response.Tools.Where (t => 0 == t.RemainingCyclesToLimit)) {
          if (toolResponse.ActiveSisterTool) {
            TimeSpan minRemainingTime = TimeSpan.FromTicks (0);
            TimeSpan maxRemainingTime = TimeSpan.FromTicks (0);
            if (cycleProgress.EstimatedEndDateTime.HasValue) { // Let's suppose now it may expire only at the cycle end (there will be parameter later for that)
              var cycleEndRemainingTime = cycleProgress.EstimatedEndDateTime.Value.Subtract (DateTime.UtcNow);
              if (cycleEndRemainingTime.TotalSeconds < 0) { // This may happen when cycleProgress is in cache
                cycleEndRemainingTime = TimeSpan.FromTicks (0);
              }
              maxRemainingTime = cycleEndRemainingTime;
              if (Bound.Equals (minRemainingTime, maxRemainingTime)) {
                toolResponse.RemainingTime = minRemainingTime;
                toolResponse.RemainingTimeRange =
                  new Range<TimeSpan> (toolResponse.RemainingTime.Value, toolResponse.RemainingTime.Value, "[]");
              }
              else {
                toolResponse.RemainingTimeRange = new Range<TimeSpan> (minRemainingTime, maxRemainingTime);
              }
            }
            else {
              toolResponse.RemainingTime = minRemainingTime;
              toolResponse.RemainingTimeRange =
                new Range<TimeSpan> (minRemainingTime, maxRemainingTime);
            }
            Debug.Assert (toolResponse.RemainingTimeRange.Lower.HasValue);
            if (cycleProgress.MachiningCompletion.HasValue) {
              toolResponse.RemainingCyclesToLimit += 1.0 - cycleProgress.MachiningCompletion.Value;
            }
          }
          else { // Not active sister tool
            toolResponse.RemainingTime = TimeSpan.FromTicks (0);
            toolResponse.RemainingTimeRange =
              new Range<TimeSpan> (toolResponse.RemainingTime.Value, toolResponse.RemainingTime.Value, "[]");
          }
        }
        foreach (var toolResponse in response.Tools.Where (t => 0 < t.RemainingCyclesToLimit)) {
          if (toolResponse.ActiveSisterTool) {
            TimeSpan minRemainingTime =
              TimeSpan.FromTicks (((int)toolResponse.RemainingCyclesToLimit - 1) * cycleDuration.Value.Ticks);
            TimeSpan maxRemainingTime =
              TimeSpan.FromTicks ((int)toolResponse.RemainingCyclesToLimit * cycleDuration.Value.Ticks);
            if (cycleProgress.EstimatedEndDateTime.HasValue) {
              var remainingTime = cycleProgress.EstimatedEndDateTime.Value.Subtract (response.DateTime);
              if (remainingTime.TotalSeconds < 0) { // This may happen when cycleProgress is in cache
                remainingTime = TimeSpan.FromTicks (0);
              }
              toolResponse.RemainingTime = minRemainingTime.Add (remainingTime);
              toolResponse.RemainingTimeRange =
                new Range<TimeSpan> (toolResponse.RemainingTime.Value, toolResponse.RemainingTime.Value, "[]");
            }
            else {
              toolResponse.RemainingTimeRange =
                new Range<TimeSpan> (minRemainingTime, maxRemainingTime);
            }
            Debug.Assert (toolResponse.RemainingTimeRange.Lower.HasValue);
            if (cycleProgress.MachiningCompletion.HasValue) {
              toolResponse.RemainingCyclesToLimit += 1.0 - cycleProgress.MachiningCompletion.Value;
            }
          }
          else { // Not active sister tool
            toolResponse.RemainingTime = TimeSpan.FromTicks ((int)toolResponse.RemainingCyclesToLimit * cycleDuration.Value.Ticks);
            toolResponse.RemainingTimeRange =
              new Range<TimeSpan> (toolResponse.RemainingTime.Value, toolResponse.RemainingTime.Value, "[]");
          }
        }
      }
    }

    void SetRemainingTimeIfByDuration (ToolLivesByMachineResponse response)
    {
      Debug.Assert (null != response);

      var cycleDuration = response.Operation.GetStandardCycleDuration (this.Machine);
      if (!cycleDuration.HasValue) {
        return;
      }
      SetRemainingTimeIfByDuration (response, cycleDuration.Value);
    }

    void SetRemainingTimeIfByDuration (ToolLivesByMachineResponse response, TimeSpan cycleDuration)
    {
      var toolResponsesByDuration = response.Tools.Where (t => IsToolLifeByDuration (t.ToolLife) && !t.Expired);
      foreach (var toolGroup in toolResponsesByDuration.GroupBy (t => GetGroupIdentifier (t.ToolLife))) {
        if (1 < toolGroup.Count ()) { // Real group
          var toolNumber = GetGroupToolNumber (toolGroup.First ().ToolLife);
          TimeSpan totalToolDurationInCycle = GetTotalDurationOperationToolNumber (response.Operation, toolNumber);
          if (0 == totalToolDurationInCycle.Ticks) { // No total duration, visit the next tool
            continue;
          }
          var groupRemainingLifeDurationToLimit = TimeSpan.FromTicks (0);
          foreach (var toolResponse in toolGroup.OrderBy (t => GetGroupSortCriterium (t.ToolLife))) {
            TimeSpan? toolRemainingLifeDurationToLimit = toolResponse.ToolLife.GetRemainingLifeDurationToLimit ();
            if (!toolRemainingLifeDurationToLimit.HasValue) {
              continue;
            }
            groupRemainingLifeDurationToLimit = groupRemainingLifeDurationToLimit.Add (toolRemainingLifeDurationToLimit.Value);
            SetRemainingTimeIfByDuration (response.Operation, cycleDuration, toolResponse, groupRemainingLifeDurationToLimit, totalToolDurationInCycle);
          }
        }
        else { // Individual tool
          var toolResponse = toolGroup.First ();
          Debug.Assert (null != toolResponse.ToolLife);
          SetRemainingTimeIfByDuration (response.Operation, cycleDuration, toolResponse);
        }
      }
    }

    void SetRemainingTimeIfByDuration (IOperation operation, TimeSpan cycleDuration, ToolLifeResponse toolResponse)
    {
      Debug.Assert (null != toolResponse);
      Debug.Assert (null != toolResponse.ToolLife);

      TimeSpan? remainingLifeDurationToLimit = toolResponse.ToolLife.GetRemainingLifeDurationToLimit ();
      if (!remainingLifeDurationToLimit.HasValue) {
        return;
      }
      if (remainingLifeDurationToLimit.Value < TimeSpan.FromTicks (0)) {
        log.ErrorFormat ("SetRemainingTimeIfByDuration: unexpected remaining life to limit {0}",
          remainingLifeDurationToLimit);
        Debug.Assert (TimeSpan.FromTicks (0) <= remainingLifeDurationToLimit.Value); // Because not expired
      }
      else {
        SetRemainingTimeIfByDuration (operation, cycleDuration, toolResponse, remainingLifeDurationToLimit.Value);
      }
    }

    void SetRemainingTimeIfByDuration (IOperation operation, TimeSpan cycleDuration, ToolLifeResponse toolResponse, TimeSpan remainingLifeDurationToLimit)
    {
      Debug.Assert (null != toolResponse);
      Debug.Assert (null != toolResponse.ToolLife);

      string toolNumber = toolResponse.ToolLife.Position.ToolNumber;
      TimeSpan totalToolDurationInCycle = GetTotalDurationOperationToolNumber (operation, toolNumber);
      if (0 == totalToolDurationInCycle.Ticks) { // No total duration, visit the next tool
        return;
      }
      SetRemainingTimeIfByDuration (operation, cycleDuration, toolResponse, remainingLifeDurationToLimit, totalToolDurationInCycle);
    }

    void SetRemainingTimeIfByDuration (IOperation operation, TimeSpan cycleDuration, ToolLifeResponse toolResponse, TimeSpan remainingLifeDurationToLimit, TimeSpan totalToolDurationInCycle)
    {
      Debug.Assert (null != toolResponse);
      Debug.Assert (null != toolResponse.ToolLife);

      long numberFullCycles = remainingLifeDurationToLimit.Ticks / totalToolDurationInCycle.Ticks;
      if ((0 < numberFullCycles)
        && (0 == (remainingLifeDurationToLimit.Ticks % totalToolDurationInCycle.Ticks))) {
        --numberFullCycles;
      }
      toolResponse.RemainingCyclesToLimit = (int)numberFullCycles;
      var lowerRemainingTime = TimeSpan.FromTicks ((int)toolResponse.RemainingCyclesToLimit * cycleDuration.Ticks);
      if (toolResponse.ActiveSisterTool) { // In case the sister tool replacement takes place after a cycle end. Note: this could be improved later
        if (this.MaxExpirationTime.HasValue
          && Bound.Compare<TimeSpan> (this.MaxExpirationTime.Value, lowerRemainingTime) < 0) {
          log.DebugFormat ("SetRemainingTimeIfByDuration: one tool data is filtered out because it is longer than {0}", this.MaxExpirationTime.Value);
        }
        else {
          var minCycleNumberForCycleProgress = Lemoine.Info.ConfigSet
            .LoadAndGet<int> (MIN_CYCLE_NUMBER_FOR_CYCLE_PROGRESS_KEY, MIN_CYCLE_NUMBER_FOR_CYCLE_PROGRESS_DEFAULT);
          if (minCycleNumberForCycleProgress <= toolResponse.RemainingCyclesToLimit) { // The precision is good enough, do not analyze the cycle progress
            var upperRemainingTime = TimeSpan.FromTicks ((int)(toolResponse.RemainingCyclesToLimit + 1) * cycleDuration.Ticks);
            toolResponse.RemainingTimeRange = new Range<TimeSpan> (lowerRemainingTime, upperRemainingTime);
          }
          else {
            var progress = GetProgress ();
            Debug.Assert (null != progress);
            var machineModuleProgress = progress.MachineModuleProgress;
            if (machineModuleProgress is null) { // CycleProgress is deactivated
              var upperRemainingTime = TimeSpan.FromTicks ((int)(toolResponse.RemainingCyclesToLimit + 1) * cycleDuration.Ticks);
              toolResponse.RemainingTimeRange = new Range<TimeSpan> (lowerRemainingTime, upperRemainingTime);
            }
            else { // null != machineModuleProgress
              var toolNumber = toolResponse.ToolLife.Position.ToolNumber;
              if (!machineModuleProgress.Values
                .Any (p => p?.Sequences.Any (s => toolNumber.Equals (s.ToolNumber, StringComparison.InvariantCultureIgnoreCase)) ?? false)) {
                log.Info ($"SetRemainingTimeIfByDuration: No machine module progress references tool number {toolNumber} => no remaining time may be computed because the tool is probably not currently active");
              }
              else {
                // Time for full cycles
                var remainingToolDuration = remainingLifeDurationToLimit;
                var remainingTime = GetRemainingTimeIfDuration (toolNumber, remainingLifeDurationToLimit,
                  progress, cycleDuration, totalToolDurationInCycle);
                toolResponse.RemainingTime = remainingTime;
                toolResponse.RemainingTimeRange = new Range<TimeSpan> (remainingTime, remainingTime, "[]");
              }
            }
          }
        }
      }
      else { // Not an active sister tool (approximative...)
        var upperRemainingTime = TimeSpan.FromTicks ((int)(toolResponse.RemainingCyclesToLimit + 1) * cycleDuration.Ticks);
        toolResponse.RemainingTimeRange = new Range<TimeSpan> (lowerRemainingTime, upperRemainingTime);
      }
    }

    void FilterWithMaxExpirationTime (ToolLivesByMachineResponse response)
    {
      response.Tools = response.Tools
        .Where (t => t.Expired
                || t.Warning
                || (t.RemainingTime.HasValue && (t.RemainingTime.Value <= this.MaxExpirationTime.Value))
                || ((null != t.RemainingTimeRange) && !t.RemainingTimeRange.IsEmpty ()
                    && (Bound.Compare<TimeSpan> (t.RemainingTimeRange.Lower,
                                                 this.MaxExpirationTime.Value) <= 0)))
        .ToList ();
    }

    void CreateGroups (ToolLivesByMachineResponse response)
    {
      foreach (var toolGroup in response.Tools
        .Where (t => !t.Group)
        .GroupBy (t => GetGroupIdentifier (t.ToolLife))
        .Where (g => 1 < g.Count ())) {
        var first = toolGroup.First ();
        var group = new ToolLifeResponse ();
        group.ActiveSisterTool = false;
        group.Group = true;
        group.Display = GetGroupDisplay (first.ToolLife);
        { // Alternative display
          var i = 1;
          foreach (var tool in toolGroup.OrderBy (t => GetGroupSortCriterium (t.ToolLife))) {
            if (string.Equals (tool.Display, group.Display)) {
              tool.Display = GetAlternativeDisplay (tool.ToolLife, group.Display, i);
            }
            ++i;
          }
        }
        group.Expired = toolGroup.All (t => t.Expired);
        group.Warning = !group.Expired && toolGroup.All (t => t.Expired || t.Warning);
        group.RemainingCyclesToLimit = toolGroup
          .Where (t => 0 < t.RemainingCyclesToLimit)
          .Sum (t => t.RemainingCyclesToLimit);
        if (toolGroup.Any (t => t.RemainingTime.HasValue)) {
          group.RemainingTime = toolGroup
            .Where (t => t.RemainingTime.HasValue)
            .Max (t => t.RemainingTime.Value);
        }
        var withRange = toolGroup
            .Where (t => (null != t.RemainingTimeRange) && !t.RemainingTimeRange.IsEmpty ());
        if (withRange.Any ()) {
          var maxRemainingTimeRangeItem = withRange
            .OrderByDescending (t => t.RemainingTimeRange.Lower)
            .First ();
          group.RemainingTimeRange = maxRemainingTimeRangeItem.RemainingTimeRange;
        }
        response.Tools.Add (group);
      }
    }

    void CompleteExpirationDateTime (ToolLivesByMachineResponse response)
    {
      foreach (var toolResponse in response.Tools
               .Where (t => t.RemainingTime.HasValue && !t.ExpirationDateTime.HasValue)) {
        toolResponse.ExpirationDateTime = response.DateTime.Add (toolResponse.RemainingTime.Value);
        toolResponse.ExpirationDateTimeRange = new UtcDateTimeRange (toolResponse.ExpirationDateTime.Value,
                                                                     toolResponse.ExpirationDateTime.Value,
                                                                     "[]");
      }
      foreach (var toolResponse in response.Tools
               .Where (t => (null != t.RemainingTimeRange) && !t.RemainingTimeRange.IsEmpty ()
                       && ((null == t.ExpirationDateTimeRange) || t.ExpirationDateTimeRange.IsEmpty ()))) {
        Debug.Assert (toolResponse.RemainingTimeRange.Lower.HasValue);
        Debug.Assert (toolResponse.RemainingTimeRange.Upper.HasValue);
        if (!toolResponse.RemainingTimeRange.Lower.HasValue) {
          log.FatalFormat ("CompleteExpirationDateTime: " +
                           "RemainingTimeRange.Lower=-oo");
        }
        else if (!toolResponse.RemainingTimeRange.Upper.HasValue) {
          log.FatalFormat ("CompleteExpirationDateTime: " +
                           "RemainingTimeRange.Upper=+oo");
        }
        else { // toolResponse.RemainingTimeRange.Lower.HasValue && toolResponse.RemainingTimeRange.Upper.HasValue
          toolResponse.ExpirationDateTimeRange =
            new UtcDateTimeRange (response.DateTime.Add (toolResponse.RemainingTimeRange.Lower.Value),
                                  response.DateTime.Add (toolResponse.RemainingTimeRange.Upper.Value));
        }
      }
    }

    void GetSummaryByMachine (ToolLivesByMachineResponse response)
    {
      response.Expired = response.Tools.Any (t => t.Expired && (t.Group || !t.ValidSisterTools));
      response.Warning = response.Tools.Any (t => t.Warning);
      var toolsWithValidRemainingTime = response.Tools
        .Where (t => t.Group || (t.ActiveSisterTool && !t.ValidSisterTools));
      if (toolsWithValidRemainingTime.Any (t => t.RemainingTime.HasValue)) {
        response.MinRemainingTime = response.Tools
          .Where (t => t.RemainingTime.HasValue)
          .Min<ToolLifeResponse, TimeSpan> (t => t.RemainingTime.Value);
      }
      else if (toolsWithValidRemainingTime.Any (t => (null != t.RemainingTimeRange)
                 && !t.RemainingTimeRange.IsEmpty ()
                 && t.RemainingTimeRange.Lower.HasValue)) {
        response.MinRemainingTime = toolsWithValidRemainingTime
          .Where (t => (null != t.RemainingTimeRange) && !t.RemainingTimeRange.IsEmpty () && t.RemainingTimeRange.Lower.HasValue)
          .Min<ToolLifeResponse, TimeSpan> (t => t.RemainingTimeRange.Lower.Value);
      }
    }

    void Sort (ToolLivesByMachineResponse response)
    {
      response.Tools = response.Tools
        .OrderBy (t => t, new ToolLifeResponseComparer ())
        .ToList ();
    }


    IProgressResponse GetProgress ()
    {
      if (null == m_progress) {
        var useProgress = Lemoine.Info.ConfigSet
          .LoadAndGet (USE_PROGRESS_KEY, USE_PROGRESS_DEFAULT)
          .ToLowerInvariant ();
        switch (useProgress) {
        case "cycle":
          m_progress = Lemoine.Business.ServiceProvider
            .Get (new CycleProgress (this.Machine));
          break;
        case "operation":
          m_progress = Lemoine.Business.ServiceProvider
            .Get (new OperationProgress (this.Machine));
          break;
        default:
          m_progress = new CycleProgressResponse ();
          break;
        }
      }
      return m_progress;
    }

    // Note: the 5 next methods may need to be customized in the future
    // With a plugin ? Using other tool tables ?

    string GetGroupToolNumber (IToolLife toolLife)
    {
      // Note: this may change in the future
      return toolLife.Position.ToolNumber;
    }

    string GetGroupIdentifier (IToolLife toolLife)
    {
      // Note: This may change in the future
      return GetGroupToolNumber (toolLife);
    }

    string GetGroupDisplay (IToolLife toolLife)
    {
      // This must be compatible with the method above
      return "T" + GetGroupIdentifier (toolLife);
    }

    int GetGroupSortCriterium (IToolLife toolLife)
    {
      // Within a same group, return in which order the tool lives are returned
      // Note: This may change in the future
      return toolLife.Position.Pot ?? 0;
    }

    /// <summary>
    /// Get an alternative display in case the tool group display is the same than the tool display
    /// </summary>
    /// <param name="toolLife"></param>
    /// <param name="toolGroupDisplay"></param>
    /// <param name="position">position in tool group (starting from 1)</param>
    /// <returns></returns>
    string GetAlternativeDisplay (IToolLife toolLife, string toolGroupDisplay, int position)
    {
      if (toolLife.Position.Pot.HasValue) {
        var alternative = "T" + toolLife.Position.Pot.Value.ToString ();
        if (!string.Equals (alternative, toolGroupDisplay)) {
          return alternative;
        }
      }
      return toolGroupDisplay + "#" + position.ToString ();
    }

    TimeSpan GetTotalDurationOperationToolNumber (IOperation operation, string toolNumber)
    {
      var sequences = Lemoine.Business.ServiceProvider
        .Get (new Lemoine.Business.Tool.OperationToolSequences (operation, toolNumber));
      TimeSpan totalDuration = TimeSpan.FromSeconds (0);
      foreach (var sequence in sequences) {
        var sequenceStandardTime = Lemoine.Business.ServiceProvider
          .Get (new Lemoine.Business.Operation.SequenceStandardTime (sequence));
        if (sequenceStandardTime.HasValue) {
          totalDuration = totalDuration.Add (sequenceStandardTime.Value);
        }
      }
      return totalDuration;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="toolNumber"></param>
    /// <param name="totalToolLifeDuration"></param>
    /// <param name="cycleProgress">cycleProgress?.MachineModuleProgress not null</param>
    /// <param name="cycleDuration"></param>
    /// <param name="totalToolDurationInCycle"></param>
    /// <returns></returns>
    TimeSpan GetRemainingTimeIfDuration (string toolNumber, TimeSpan totalToolLifeDuration,
                                         IProgressResponse cycleProgress,
                                         TimeSpan cycleDuration, TimeSpan totalToolDurationInCycle)
    {
      // Check first there is a sequence with the specified tool number
      Debug.Assert (!string.IsNullOrEmpty (toolNumber));

      var remainingToolDuration = totalToolLifeDuration;
      TimeSpan remainingTime = TimeSpan.FromSeconds (0);

      var cycleProgressMachineModuleProgress = cycleProgress?.MachineModuleProgress;
      if (cycleProgressMachineModuleProgress is null) { // Unexpected
        log.Fatal ($"GetRemainingTimeIfDuration: cycleProgress.MachineModuleProgress null, which is unexpected");
        throw new NullReferenceException ("cycleProgress.MachineModuleProgress null (unexpected)");
      }
      var machineModuleProgress = cycleProgressMachineModuleProgress.Values
        .First (p => p.Sequences.Any (s => toolNumber.Equals (s.ToolNumber, StringComparison.InvariantCultureIgnoreCase)));

      // Current sequences
      if ((null != machineModuleProgress.CurrentSequence)
        && toolNumber.Equals (machineModuleProgress.CurrentSequence.ToolNumber, StringComparison.InvariantCultureIgnoreCase)
        && machineModuleProgress.CurrentSequenceElapsed.HasValue
        && machineModuleProgress.CurrentSequenceStandardTime.HasValue) {
        Debug.Assert (machineModuleProgress.CurrentSequenceElapsed.Value <= machineModuleProgress.CurrentSequenceStandardTime.Value);
        TimeSpan remainingCurrentSequenceTime = machineModuleProgress.CurrentSequenceStandardTime.Value
          .Subtract (machineModuleProgress.CurrentSequenceElapsed.Value);
        if (remainingToolDuration <= remainingCurrentSequenceTime) {
          remainingTime = remainingTime.Add (remainingToolDuration);
          return remainingTime;
        }
        else {
          remainingTime = remainingTime.Add (remainingCurrentSequenceTime);
          remainingToolDuration = remainingToolDuration.Subtract (remainingCurrentSequenceTime);
        }
      }

      // Next sequences
      var nextSequences = machineModuleProgress.Sequences
        .Where (s => machineModuleProgress.CurrentSequence.Order < s.Order)
        .OrderBy (s => s.Order);
      foreach (var nextSequence in nextSequences) {
        var sequenceStandardTime = Lemoine.Business.ServiceProvider
          .Get (new SequenceStandardTime (nextSequence));
        if (sequenceStandardTime.HasValue) {
          if (toolNumber.Equals (nextSequence.ToolNumber, StringComparison.InvariantCultureIgnoreCase)) {
            if (remainingToolDuration <= sequenceStandardTime.Value) {
              remainingTime = remainingTime.Add (remainingToolDuration);
              return remainingTime;
            }
            else {
              remainingTime = remainingTime.Add (sequenceStandardTime.Value);
              remainingToolDuration = remainingToolDuration.Subtract (sequenceStandardTime.Value);
            }
          }
          else { // The tool does not match
            remainingTime = remainingTime.Add (sequenceStandardTime.Value);
          }
        }
      }
      Debug.Assert (0 < remainingToolDuration.Ticks); // Else there would be a return statement above

      // Full cycles
      long numberFullCycles = remainingToolDuration.Ticks / totalToolDurationInCycle.Ticks;
      if (0 == (remainingToolDuration.Ticks % totalToolDurationInCycle.Ticks)) {
        --numberFullCycles;
      }
      remainingTime = remainingTime.Add (TimeSpan.FromTicks (numberFullCycles * cycleDuration.Ticks));
      remainingToolDuration = remainingToolDuration.Subtract (TimeSpan.FromTicks (numberFullCycles * totalToolDurationInCycle.Ticks));
      Debug.Assert (0 <= remainingToolDuration.Ticks);
      if (0 == remainingToolDuration.Ticks) {
        return remainingTime;
      }

      // New cycle after the full cycle
      foreach (var sequence in machineModuleProgress.Sequences.OrderBy (s => s.Order)) {
        var sequenceStandardTime = Lemoine.Business.ServiceProvider
          .Get (new SequenceStandardTime (sequence));
        if (sequenceStandardTime.HasValue) {
          if (toolNumber.Equals (sequence.ToolNumber, StringComparison.InvariantCultureIgnoreCase)) {
            if (remainingToolDuration <= sequenceStandardTime.Value) {
              remainingTime = remainingTime.Add (remainingToolDuration);
              return remainingTime;
            }
            else {
              remainingTime = remainingTime.Add (sequenceStandardTime.Value);
              remainingToolDuration = remainingToolDuration.Subtract (sequenceStandardTime.Value);
            }
          }
          else { // The tool does not match
            remainingTime = remainingTime.Add (sequenceStandardTime.Value);
          }
        }
      }

      Debug.Assert (false);
      log.FatalFormat ("GetRemainingTimeIfDuration: the function should have already returned");
      throw new InvalidProgramException ("GetRemainingTimeIfDuration");
    }

    bool IsToolLifeByCycle (IToolLife toolLife)
    {
      return (null != toolLife) && (null != toolLife.Unit)
        && (toolLife.Unit.Id.Equals ((int)UnitId.NumberOfCycles)
            || toolLife.Unit.Id.Equals ((int)UnitId.ToolNumberOfTimes)
            || toolLife.Unit.Id.Equals ((int)UnitId.NumberOfParts));
    }

    bool IsToolLifeByNumberOfParts (IToolLife toolLife)
    {
      return (null != toolLife) && (null != toolLife.Unit)
        && toolLife.Unit.Id.Equals ((int)UnitId.NumberOfParts)
        && Lemoine.Info.ConfigSet.LoadAndGet<bool> (TOOL_EXPIRATION_BY_PART_KEY,
                                                    TOOL_EXPIRATION_BY_PART_DEFAULT);
    }

    bool IsToolLifeByNumberOfTimes (IToolLife toolLife)
    {
      return (null != toolLife) && (null != toolLife.Unit)
        && toolLife.Unit.Id.Equals ((int)UnitId.ToolNumberOfTimes)
        && Lemoine.Info.ConfigSet.LoadAndGet<bool> (TOOL_EXPIRATION_BY_NB_OF_TIMES_KEY,
                                                    TOOL_EXPIRATION_BY_NB_OF_TIMES_DEFAULT);
    }

    bool IsToolLifeByDuration (IToolLife toolLife)
    {
      return (null != toolLife) && (null != toolLife.Unit)
        && (toolLife.Unit.Id.Equals ((int)UnitId.DurationHours)
            || toolLife.Unit.Id.Equals ((int)UnitId.DurationMinutes)
            || toolLife.Unit.Id.Equals ((int)UnitId.DurationSeconds));
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      string cacheKey = "Business.Tool.ToolLivesByMachine."
        + ((IDataWithId<int>)Machine).Id;
      if (this.MaxExpirationTime.HasValue) {
        cacheKey += "?MaxExpirationTime=" + this.MaxExpirationTime.Value.TotalSeconds;
      }
      return cacheKey;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/>
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<ToolLivesByMachineResponse> data)
    {
      return true;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (ToolLivesByMachineResponse data)
    {
      return CacheTimeOut.CurrentLong.GetTimeSpan ();
    }
    #endregion // IRequest implementation
  }

  /// <summary>
  /// Response structure to the ToolLivesByMachine request
  /// </summary>
  public class ToolLivesByMachineResponse
  {
    /// <summary>
    /// Current operation of the machine
    /// </summary>
    public IOperation Operation { get; set; }

    /// <summary>
    /// Details on each tool
    /// 
    /// First expiring tools are returned first
    /// </summary>
    public IList<ToolLifeResponse> Tools { get; set; }

    /// <summary>
    /// UTC Date/time of the response
    /// </summary>
    public DateTime DateTime { get; set; }

    /// <summary>
    /// One of the tool expired
    /// </summary>
    public bool Expired { get; set; }

    /// <summary>
    /// One of the tool is in warning
    /// </summary>
    public bool Warning { get; set; }

    /// <summary>
    /// Minimum remaining time of all the tools
    /// </summary>
    public TimeSpan? MinRemainingTime { get; set; }
  }

  /// <summary>
  /// Tool life response
  /// </summary>
  public class ToolLifeResponse // Not a struct, so that it can be accessed by reference and modified accordingly
  {
    /// <summary>
    /// Constructor
    /// </summary>
    public ToolLifeResponse ()
    {
      this.RemainingCyclesToLimit = -1;
      this.Group = false;
      this.ActiveSisterTool = true;
      this.ValidSisterTools = false;
    }

    /// <summary>
    /// Tool life reference
    /// 
    /// Only for individual tools
    /// null for groups
    /// </summary>
    public IToolLife ToolLife { get; set; }

    /// <summary>
    /// Is it a group ?
    /// </summary>
    public bool Group { get; set; }

    /// <summary>
    /// Tool(group) display
    /// </summary>
    public string Display { get; set; }

    /// <summary>
    /// Next sister tool if applicable (ValidSisterTools and ActiveSisterTool are true),
    /// else null
    /// </summary>
    public string Next { get; set; }

    /// <summary>
    /// Is it the active sister tool
    /// 
    /// (for individual tools only)
    /// </summary>
    public bool ActiveSisterTool { get; set; }

    /// <summary>
    /// Are there still valid sister tools after it ?
    /// 
    /// (for individual tools part of a group only)
    /// 
    /// false by default if not part of a tool group
    /// </summary>
    public bool ValidSisterTools { get; set; }

    /// <summary>
    /// The tool is expired
    /// </summary>
    public bool Expired { get; set; }

    /// <summary>
    /// The tool life is in warning
    /// </summary>
    public bool Warning { get; set; }

    /// <summary>
    /// Number of reamining cycle before expiration
    /// 
    /// -1 is returned if not applicable
    /// </summary>
    public double RemainingCyclesToLimit { get; set; }

    /// <summary>
    /// Remaining time before expiration
    /// </summary>
    public TimeSpan? RemainingTime { get; set; }

    /// <summary>
    /// Remaining time range before expiration
    /// </summary>
    public Range<TimeSpan> RemainingTimeRange { get; set; }

    /// <summary>
    /// Estimated expiration date/time range is not expired yet
    /// 
    /// Else date/time of the expiration
    /// </summary>
    public DateTime? ExpirationDateTime { get; set; }

    /// <summary>
    /// Estimated expiration date/time range is not expired yet
    /// 
    /// Else date/time of the expiration
    /// </summary>
    public UtcDateTimeRange ExpirationDateTimeRange { get; set; }
  }

  /// <summary>
  /// Comparer class for ToolLifeResponse
  /// </summary>
  class ToolLifeResponseComparer : IComparer<ToolLifeResponse>
  {
    /// <summary>
    /// 
    /// </summary>
    public int Compare (ToolLifeResponse a, ToolLifeResponse b)
    {
      int compareExpired = CompareExpired (a, b);
      if (0 != compareExpired) {
        return compareExpired;
      }

      int compareExpirationDateTimeRange = CompareExpirationDateTimeRange (a, b);
      if (0 != compareExpirationDateTimeRange) {
        return compareExpirationDateTimeRange;
      }

      int compareExpirationDateTime = CompareExpirationDateTime (a, b);
      if (0 != compareExpirationDateTime) {
        return compareExpirationDateTime;
      }

      var compareRemainingCycles = CompareRemainingCycles (a, b);
      if (compareRemainingCycles.HasValue) {
        return compareRemainingCycles.Value;
      }

      var compareExpirationDateTimeRange2 = CompareExpirationDateTimeRange2 (a, b);
      if (compareExpirationDateTimeRange2.HasValue) {
        return compareExpirationDateTimeRange2.Value;
      }

      var compareExpirationDateTime2 = CompareExpirationDateTime2 (a, b);
      if (compareExpirationDateTime2.HasValue) {
        return compareExpirationDateTime2.Value;
      }

      var compareRemainingCycles2 = CompareRemainingCycles2 (a, b);
      if (compareRemainingCycles2.HasValue) {
        return compareRemainingCycles2.Value;
      }

      int compareActiveSisterTool = CompareActiveSisterTool (a, b);
      if (0 != compareActiveSisterTool) {
        return compareActiveSisterTool;
      }

      int compareValidSisterTools = CompareValidSisterTools (a, b);
      if (0 != compareValidSisterTools) {
        return compareValidSisterTools;
      }

      int compareWarning = CompareWarning (a, b);
      if (0 != compareWarning) {
        return compareWarning;
      }

      return 0;
    }

    int CompareExpired (ToolLifeResponse a, ToolLifeResponse b)
    {
      if (object.Equals (a.Expired, b.Expired)) {
        return 0;
      }
      else {
        return a.Expired ? -1 : +1;
      }
    }

    int CompareExpirationDateTimeRange (ToolLifeResponse a, ToolLifeResponse b)
    {
      if ((null != a.ExpirationDateTimeRange) && !a.ExpirationDateTimeRange.IsEmpty ()) {
        if ((null != b.ExpirationDateTimeRange) && !b.ExpirationDateTimeRange.IsEmpty ()) {
          return DateTime.Compare (a.ExpirationDateTimeRange.Lower.Value,
                                   b.ExpirationDateTimeRange.Lower.Value);
        }
      }

      return 0;
    }

    int? CompareExpirationDateTimeRange2 (ToolLifeResponse a, ToolLifeResponse b)
    {
      if ((null != a.ExpirationDateTimeRange) && !a.ExpirationDateTimeRange.IsEmpty ()) {
        if ((null != b.ExpirationDateTimeRange) && !b.ExpirationDateTimeRange.IsEmpty ()) {
          return DateTime.Compare (a.ExpirationDateTimeRange.Lower.Value,
                                   b.ExpirationDateTimeRange.Lower.Value);
        }
        else {
          return -1;
        }
      }
      else {
        if ((null != b.ExpirationDateTimeRange) && !b.ExpirationDateTimeRange.IsEmpty ()) {
          return +1;
        }
        else {
          return null;
        }
      }
    }

    int CompareExpirationDateTime (ToolLifeResponse a, ToolLifeResponse b)
    {
      if (a.ExpirationDateTime.HasValue && b.ExpirationDateTime.HasValue) {
        return DateTime.Compare (a.ExpirationDateTime.Value, b.ExpirationDateTime.Value);
      }
      else {
        return 0;
      }
    }

    int? CompareExpirationDateTime2 (ToolLifeResponse a, ToolLifeResponse b)
    {
      if (a.ExpirationDateTime.HasValue) {
        if (b.ExpirationDateTime.HasValue) {
          return DateTime.Compare (a.ExpirationDateTime.Value, b.ExpirationDateTime.Value);
        }
        else {
          return -1;
        }
      }
      else { // !a.ExpirationDateTime.HasValue
        if (b.ExpirationDateTime.HasValue) {
          return +1;
        }
        else {
          return null;
        }
      }
    }

    int? CompareRemainingCycles (ToolLifeResponse a, ToolLifeResponse b)
    {
      if ((0 <= a.RemainingCyclesToLimit) && (0 <= b.RemainingCyclesToLimit)) {
        return a.RemainingCyclesToLimit.CompareTo (b.RemainingCyclesToLimit);
      }
      return null;
    }

    int? CompareRemainingCycles2 (ToolLifeResponse a, ToolLifeResponse b)
    {
      if ((0 <= a.RemainingCyclesToLimit) && (0 <= b.RemainingCyclesToLimit)) {
        return a.RemainingCyclesToLimit.CompareTo (b.RemainingCyclesToLimit);
      }
      if (a.RemainingCyclesToLimit < 0) {
        if (0 <= b.RemainingCyclesToLimit) {
          return +1;
        }
      }
      else if (b.RemainingCyclesToLimit < 0) {
        return -1;
      }
      return null;
    }

    int CompareWarning (ToolLifeResponse a, ToolLifeResponse b)
    {
      if (object.Equals (a.Warning, b.Warning)) {
        return 0;
      }
      else { // Tool with warning before
        return a.Warning ? -1 : +1;
      }
    }

    int CompareActiveSisterTool (ToolLifeResponse a, ToolLifeResponse b)
    {
      if (a.ActiveSisterTool == b.ActiveSisterTool) {
        return 0;
      }
      else { // ActiveSisterTool before
        return a.ActiveSisterTool ? -1 : +1;
      }
    }

    int CompareValidSisterTools (ToolLifeResponse a, ToolLifeResponse b)
    {
      if (b.ValidSisterTools == b.ValidSisterTools) {
        return 0;
      }
      else { // If ValidSisterTools, after
        return a.ValidSisterTools ? +1 : -1;
      }
    }
  }
}
