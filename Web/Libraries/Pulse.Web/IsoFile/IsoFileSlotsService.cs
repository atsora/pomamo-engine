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
using Pulse.Web.CommonResponseDTO;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Collections;
using Lemoine.Web;

namespace Pulse.Web.IsoFile
{
  /// <summary>
  /// Description of IsoFileSlotsService
  /// </summary>
  public class IsoFileSlotsService
    : GenericCachedService<IsoFileSlotsRequestDTO>
  {
    static readonly string VERTICAL_SPLIT_TOTAL_DIVISOR_KEY = "Web.VerticalSplit.TotalDurationDivisor";
    static readonly int VERTICAL_SPLIT_TOTAL_DIVISOR_DEFAULT = 640;
    // 400 corresponds for a full day to 3,6 minutes (on a 1200px width screen, about 3px, on a FullHD screen, about 5 px)
    // 640 corresponds for a full day to 2,5 minutes (on a 1200px width screen, about 2px, on a FullHD screen, about 3 px)
    // 960 corresponds for a full day to 1,5 minutes (on a 1200px width screen, about 1px, on a FullHD screen, about 2 px)

    static readonly ILog log = LogManager.GetLogger(typeof (IsoFileSlotsService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public IsoFileSlotsService ()
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
    protected override TimeSpan GetCacheTimeOut (string url, IsoFileSlotsRequestDTO requestDTO)
    {
      UtcDateTimeRange range;
      if (string.IsNullOrEmpty (requestDTO.Range)) {
        range = new UtcDateTimeRange (DateTime.UtcNow, DateTime.UtcNow, "[]");
      }
      else {
        range = new UtcDateTimeRange (requestDTO.Range);
      }
      
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
    public override object GetWithoutCache(IsoFileSlotsRequestDTO request)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        int machineId = request.MachineId;
        IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (machineId);
        if (null == machine) {
          log.ErrorFormat ("GetWithoutCache: " +
                           "unknown machine with ID {0}",
                           machineId);
          return new ErrorDTO ("No machine with the specified ID",
                               ErrorStatus.WrongRequestParameter);
        }
        
        UtcDateTimeRange range = ParseRange (request.Range);
        
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Web.IsoFileSlots"))
        {
          return GetAutoSplit (machine, range, request.SkipDetails); // Less than a week
        }
      }
    }
    
    IsoFileSlotsResponseDTO GetAutoSplit (IMonitoredMachine machine, UtcDateTimeRange range, bool skipDetails)
    {
      Debug.Assert (range.Duration.HasValue);
      
      int verticalSplitMaxTotalDurationFactor = Lemoine.Info.ConfigSet
        .LoadAndGet<int> (VERTICAL_SPLIT_TOTAL_DIVISOR_KEY,
                          VERTICAL_SPLIT_TOTAL_DIVISOR_DEFAULT);
      TimeSpan minimumSplitDuration =
        TimeSpan.FromTicks(range.Duration.Value.Ticks / verticalSplitMaxTotalDurationFactor);
      
      var result = new IsoFileSlotsResponseDTO ();
      result.Range = range.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
      result.IsoFileSlots = new List<IsoFileSlotBlockDTO> ();

      var slots = ModelDAOHelper.DAOFactory.IsoFileSlotDAO
        .FindOverlapsRange (machine.MainMachineModule, range);
      
      IList<IIsoFileSlot> pending = new List<IIsoFileSlot> ();
      IIsoFileSlot previousSlot = null;
      foreach (var slot in slots) {
        UtcDateTimeRange slotRange = new UtcDateTimeRange (slot.DateTimeRange
                                                           .Intersects (range));
        Debug.Assert (!slotRange.IsEmpty ());
        Debug.Assert (slotRange.Duration.HasValue);
        
        if ( (null != previousSlot)
            && (minimumSplitDuration <= slot.DateTimeRange.Lower.Value.Subtract (previousSlot.DateTimeRange.Upper.Value))) {
          // Large gap
          PushBlockAndAddToPending (result, pending, skipDetails, slot);
        }
        else if (minimumSplitDuration <= slotRange.Duration.Value) { // Long enough
          PushBlockAndAddSlot (result, pending, skipDetails, slot);
        }
        else {
          pending.Add (slot);
        }

        previousSlot = slot;
      }
      PushBlock (result, pending, skipDetails);
      
      return result;
    }

    void PushBlockAndAddToPending (IsoFileSlotsResponseDTO result, IList<IIsoFileSlot> pending,
                                   bool skipDetails, IIsoFileSlot slot)
    {
      PushBlock (result, pending, skipDetails);
      pending.Clear ();
      pending.Add (slot);
    }

    void PushBlockAndAddSlot (IsoFileSlotsResponseDTO result, IList<IIsoFileSlot> pending,
                              bool skipDetails, IIsoFileSlot slot)
    {
      PushBlock (result, pending, skipDetails);
      pending.Clear ();
      pending.Add (slot);
      PushBlock (result, pending, skipDetails);
      pending.Clear ();
    }
    
    void PushBlock (IsoFileSlotsResponseDTO response, IList<IIsoFileSlot> slots, bool skipDetails)
    {
      if (!slots.Any ()) {
        return;
      }
      
      var block = new IsoFileSlotBlockDTO ();
      var range = new UtcDateTimeRange (slots.First ().DateTimeRange.Lower,
                                        slots.Last ().DateTimeRange.Upper,
                                        slots.First ().DateTimeRange.LowerInclusive,
                                        slots.Last ().DateTimeRange.UpperInclusive);
      block.Range = range
        .ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
      if (1 == slots.Count) {
        var slot = slots [0];
        int id = slot.IsoFile.Id;
        block.Id = id;
        block.BgColor = ColorGenerator.GetColor ("IsoFile", id);
        block.FgColor = ColorGenerator.GetContrastColor (block.BgColor);
        block.Display = slot.IsoFile.Display;
      }
      else { // 1 < slots.Count
        Debug.Assert (1 < slots.Count);
        var byDescendingDuration = slots
          .Where (s => null != s.IsoFile)
          .GroupBy (s => ((IDataWithId<int>)s.IsoFile).Id)
          .Select (sk => new KeyValuePair<int, double> (sk.Key,
                                                          sk.Sum (s => s.DateTimeRange.Duration.HasValue
                                                                  ? s.DateTimeRange.Duration.Value.TotalSeconds
                                                                  : TimeSpan.FromDays (365).TotalSeconds)))
          .OrderByDescending (kv => kv.Value)
          .ToList ();
        if (0 == byDescendingDuration.Count) {
          Debug.Assert (null == slots [0].IsoFile);
          block.Id = 0;
          block.BgColor = ColorGenerator.GetColor ("IsoFile", 0);
          block.FgColor = ColorGenerator.GetContrastColor (block.BgColor);
        }
        else if (1 == byDescendingDuration.Count) {
          block.BgColor = ColorGenerator.GetColor ("IsoFile", byDescendingDuration [0].Key);
          block.FgColor = ColorGenerator.GetContrastColor (block.BgColor);
        }
        else { // 2 <= byDecendingDuration.Count
          block.BgColor = ColorGenerator.GetColor ("IsoFile", byDescendingDuration [0].Key);
          block.PatternName = "diagonal-stripe-3";
          block.PatternColor = ColorGenerator.GetColor ("IsoFile", byDescendingDuration [1].Key);
        }
      }
      if (skipDetails) {
        block.Details = null;
      }
      else { // !skipDetails
        block.Details = new List<IsoFileSlotBlockDetailDTO> ();
        foreach (var slot in slots.OrderBy (s => s.DateTimeRange)) {
          var detail = new IsoFileSlotBlockDetailDTO ();
          detail.Range = slot.DateTimeRange.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
          detail.Display = slot.IsoFile.Display;
          block.Details.Add (detail);
        }
      }
      response.IsoFileSlots.Add (block);
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
