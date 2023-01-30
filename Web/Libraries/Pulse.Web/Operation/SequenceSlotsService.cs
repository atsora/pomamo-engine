// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Cache;
using Lemoine.Info;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Web.CommonRequestDTO;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Collections;
using Lemoine.Web;

namespace Pulse.Web.Operation
{
  /// <summary>
  /// Description of SequenceSlotsService
  /// </summary>
  public class SequenceSlotsService
    : GenericCachedService<SequenceSlotsRequestDTO>
  {
    static readonly string VERTICAL_SPLIT_TOTAL_DIVISOR_KEY = "Web.VerticalSplit.TotalDurationDivisor";
    static readonly int VERTICAL_SPLIT_TOTAL_DIVISOR_DEFAULT = 640;
    // 400 corresponds for a full day to 3,6 minutes (on a 1200px width screen, about 3px, on a FullHD screen, about 5 px)
    // 640 corresponds for a full day to 2,5 minutes (on a 1200px width screen, about 2px, on a FullHD screen, about 3 px)
    // 960 corresponds for a full day to 1,5 minutes (on a 1200px width screen, about 1px, on a FullHD screen, about 2 px)

    static readonly ILog log = LogManager.GetLogger(typeof (SequenceSlotsService).FullName);
    
    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public SequenceSlotsService ()
      : base(Lemoine.Core.Cache.CacheTimeOut.CurrentShort)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Get the cache time out
    /// </summary>
    /// <param name="url"></param>
    /// <param name="requestDTO"></param>
    /// <returns></returns>
    protected override TimeSpan GetCacheTimeOut (string url, SequenceSlotsRequestDTO requestDTO)
    {
      UtcDateTimeRange range = ParseRange (requestDTO.Range);
      
      TimeSpan cacheTimeSpan;
      if (range.IsStrictlyLeftOf (new UtcDateTimeRange (DateTime.UtcNow))) { // Old / Past
        // Previous day => old
        IDaySlot daySlot = ModelDAOHelper.DAOFactory.DaySlotDAO.FindProcessedAt (DateTime.UtcNow);
        if (range.IsStrictlyLeftOf (daySlot.DateTimeRange)) { // Old
          cacheTimeSpan = CacheTimeOut.OldShort.GetTimeSpan ();
        }
        else { // Past
          cacheTimeSpan = CacheTimeOut.PastShort.GetTimeSpan ();
        }
      }
      else { // Current or future
        cacheTimeSpan = CacheTimeOut.CurrentShort.GetTimeSpan ();
      }
      log.DebugFormat ("GetCacheTimeOut: " +
                       "cacheTimeSpan is {0} for url={1}",
                       cacheTimeSpan, url);
      return cacheTimeSpan;
    }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache(SequenceSlotsRequestDTO request)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        int machineId = request.MachineId;
        IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindByIdWithMainMachineModulePerformanceFieldUnit (machineId);
        if (null == machine) {
          log.ErrorFormat ("GetWithoutCache: " +
                           "unknown monitored machine with ID {0}",
                           machineId);
          return new ErrorDTO ("No monitored machine with the specified ID",
                               ErrorStatus.WrongRequestParameter);
        }
        
        var mainMachineModule = machine.MainMachineModule;
        if (null == mainMachineModule) {
          log.InfoFormat ("GetWithoutCache: " +
                          "no main machine module for monitored machine with ID {0}",
                          machineId);
        }
        
        IEnumerable<IMachineModule> machineModules = machine.MachineModules
          .OrderBy (machineModule => IsMainMachineModule (machineModule, mainMachineModule) ? 0 : 1);
        
        UtcDateTimeRange range = ParseRange (request.Range);
        
        return GetAutoSplit (machineModules, mainMachineModule, range, request.SkipDetails);
      }
    }

    bool IsMainMachineModule (IMachineModule machineModule, IMachineModule mainMachineModule)
    {
      Debug.Assert (null != machineModule);
      
      return (null != mainMachineModule) && machineModule.Id.Equals (mainMachineModule.Id);
    }
    
    SequenceSlotsResponseDTO GetAutoSplit (IEnumerable<IMachineModule> machineModules, IMachineModule mainMachineModule,
                                           UtcDateTimeRange range, bool skipDetails)
    {
      Debug.Assert (range.Duration.HasValue);
      
      int verticalSplitMaxTotalDurationFactor = Lemoine.Info.ConfigSet
        .LoadAndGet<int> (VERTICAL_SPLIT_TOTAL_DIVISOR_KEY,
                          VERTICAL_SPLIT_TOTAL_DIVISOR_DEFAULT);
      TimeSpan minimumSplitDuration =
        TimeSpan.FromTicks(range.Duration.Value.Ticks / verticalSplitMaxTotalDurationFactor);
      
      var result = new SequenceSlotsResponseDTO ();
      result.Range = range.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
      result.ByMachineModule = new List<SequenceSlotsByMachineModuleDTO> ();
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        foreach (var machineModule in machineModules) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Web.SequenceSlots"))
          {
            var slots = ModelDAOHelper.DAOFactory.SequenceSlotDAO
              .FindOverlapsRange (machineModule, range);
            if (!slots.Any ()) {
              continue;
            }

            result.ByMachineModule = new List<SequenceSlotsByMachineModuleDTO> ();
            var byMachineModule = new SequenceSlotsByMachineModuleDTO (machineModule,
                                                                       IsMainMachineModule (machineModule, mainMachineModule));
            result.ByMachineModule.Add (byMachineModule);
            byMachineModule.Blocks = new List<SequenceSlotBlockDTO> ();
            
            IList<ISequenceSlot> pending = new List<ISequenceSlot> ();
            ISequenceSlot previousSlot = null;
            foreach (var slot in slots) {
              UtcDateTimeRange slotRange = new UtcDateTimeRange (slot.DateTimeRange
                                                                 .Intersects (range));
              Debug.Assert (!slotRange.IsEmpty ());
              Debug.Assert (slotRange.Duration.HasValue);
              
              if ( (null != previousSlot)
                  && (minimumSplitDuration <= slot.DateTimeRange.Lower.Value.Subtract (previousSlot.DateTimeRange.Upper.Value))) {
                // Large gap
                PushBlockAndAddToPending (byMachineModule, pending, skipDetails, slot);
              }
              else if (minimumSplitDuration <= slotRange.Duration.Value) { // Long enough
                PushBlockAndAddSlot (byMachineModule, pending, skipDetails, slot);
              }
              else {
                pending.Add (slot);
              }

              previousSlot = slot;
            }
            PushBlock (byMachineModule, pending, skipDetails);
          }
        }
      }
      
      return result;
    }

    void PushBlockAndAddToPending (SequenceSlotsByMachineModuleDTO byMachineModule, IList<ISequenceSlot> pending,
                              bool skipDetails, ISequenceSlot slot)
    {
      PushBlock (byMachineModule, pending, skipDetails);
      pending.Clear ();
      pending.Add (slot);
    }

    void PushBlockAndAddSlot (SequenceSlotsByMachineModuleDTO byMachineModule, IList<ISequenceSlot> pending,
                              bool skipDetails, ISequenceSlot slot)
    {
      PushBlock (byMachineModule, pending, skipDetails);
      pending.Clear ();
      pending.Add (slot);
      PushBlock (byMachineModule, pending, skipDetails);
      pending.Clear ();
    }
    
    void PushBlock (SequenceSlotsByMachineModuleDTO byMachineModule, IList<ISequenceSlot> slots, bool skipDetails)
    {
      if (!slots.Any ()) {
        return;
      }
      
      var block = new SequenceSlotBlockDTO ();
      var range = new UtcDateTimeRange (slots.First ().DateTimeRange.Lower,
                                        slots.Last ().DateTimeRange.Upper,
                                        slots.First ().DateTimeRange.LowerInclusive,
                                        slots.Last ().DateTimeRange.UpperInclusive);
      block.Range = range
        .ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
      if (1 == slots.Count) {
        var slot = slots [0];
        int id = GetSequenceId (slot);
        block.Id = id;
        block.BgColor = ColorGenerator.GetColor ("Sequence", id);
        block.FgColor = ColorGenerator.GetContrastColor (block.BgColor);
        block.Display = slot.Display;
      }
      else { // 1 < slots.Count
        Debug.Assert (1 < slots.Count);
        var byDescendingDuration = slots
          .OrderByDescending (s => s.DateTimeRange.Duration.HasValue
                              ? s.DateTimeRange.Duration.Value
                              : TimeSpan.MaxValue)
          .ToList ();
        var firstSlot = slots [0];
        int firstId = GetSequenceId (firstSlot);
        block.BgColor = ColorGenerator.GetColor ("Sequence", firstId);
        var secondSlot = slots [1];
        int secondId = GetSequenceId (secondSlot);
        block.PatternName = "diagonal-stripe-3";
        block.PatternColor = ColorGenerator.GetColor ("Sequence", secondId);
      }
      if (skipDetails) {
        block.Details = null;
      }
      else { // !skipDetails
        block.Details = new List<SequenceSlotBlockDetailDTO> ();
        foreach (var slot in slots.OrderBy (s => s.DateTimeRange)) {
          var detail = new SequenceSlotBlockDetailDTO ();
          detail.Range = slot.DateTimeRange.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
          detail.Display = slot.Display;
          if (null != slot.Sequence) {
            detail.ToolNumber = slot.Sequence.ToolNumber;
          }
          block.Details.Add (detail);
        }
      }
      if (skipDetails) {
        block.Details = null;
      }
      byMachineModule.Blocks.Add (block);
    }
    
    /// <summary>
    /// Return the sequence id or 0 if there is no sequence
    /// </summary>
    /// <param name="sequenceSlot"></param>
    /// <returns></returns>
    int GetSequenceId (ISequenceSlot sequenceSlot)
    {
      Debug.Assert (null != sequenceSlot);
      
      if (null == sequenceSlot.Sequence) {
        return 0;
      }
      else {
        return ((IDataWithId<int>)sequenceSlot.Sequence).Id;
      }
    }
    
    UtcDateTimeRange ParseRange (string range)
    {
      if (string.IsNullOrEmpty (range)) {
        return GetDefaultRange ();
      }
      else if (range.Equals ("CurrentShift")) {
        return GetCurrentShift ();
      }
      else {
        return new UtcDateTimeRange (range);
      }
    }
    
    /// <summary>
    /// Return the default range: the current day
    /// </summary>
    /// <returns></returns>
    UtcDateTimeRange GetDefaultRange ()
    {
      var dayAtRequest = new Lemoine.Business.Time.DayAt (DateTime.UtcNow);
      var daySlot = Lemoine.Business.ServiceProvider
        .Get (dayAtRequest);
      if (daySlot is null) {
        log.Fatal ($"GetDefaultRange: no day slot now");
        return new UtcDateTimeRange ();
      }
      return daySlot.DateTimeRange;
    }
    
    /// <summary>
    /// Get the current shift range
    /// </summary>
    /// <returns></returns>
    UtcDateTimeRange GetCurrentShift ()
    {
      var currentShiftRequest = new Lemoine.Business.Shift.CurrentShift ();
      var currentShift = Lemoine.Business.ServiceProvider
        .Get (currentShiftRequest);
      if (currentShift is null) {
        var dayAtRequest = new Lemoine.Business.Time.DayAt (DateTime.UtcNow);
        var daySlot = Lemoine.Business.ServiceProvider
          .Get (dayAtRequest);
        if (daySlot is null) {
          log.Fatal ($"GetCurrentShift: no day slot now");
          return new UtcDateTimeRange ();
        }
        return daySlot.DateTimeRange;
      }
      else {
        return currentShift.DateTimeRange;
      }
    }
    #endregion // Methods
  }
}
