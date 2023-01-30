// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Extensions.Web.Services;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Web;

namespace Pulse.Web.OperationCycle
{
  /// <summary>
  /// Description of OperationCycleSlotsService
  /// </summary>
  public class OperationCycleSlotsService
    : GenericAsyncCachedService<OperationCycleSlotsRequestDTO>
  {
    static readonly string VERTICAL_SPLIT_TOTAL_DIVISOR_KEY = "Web.VerticalSplit.TotalDurationDivisor";
    static readonly int VERTICAL_SPLIT_TOTAL_DIVISOR_DEFAULT = 640;
    // 400 corresponds for a full day to 3,6 minutes (on a 1200px width screen, about 3px, on a FullHD screen, about 5 px)
    // 640 corresponds for a full day to 2,5 minutes (on a 1200px width screen, about 2px, on a FullHD screen, about 3 px)
    // 960 corresponds for a full day to 1,5 minutes (on a 1200px width screen, about 1px, on a FullHD screen, about 2 px)

    static readonly string EVEN_COLOR = "#79B4B7";
    static readonly string ODD_COLOR = "#9D9D9D";

    static readonly ILog log = LogManager.GetLogger (typeof (OperationCycleSlotsService).FullName);

    int m_colorIndex = 0;

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public OperationCycleSlotsService ()
      : base (Lemoine.Core.Cache.CacheTimeOut.CurrentShort)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Get the cache time out
    /// </summary>
    /// <param name="url"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public override TimeSpan GetCacheTimeOut (string url, OperationCycleSlotsRequestDTO request)
    {
      UtcDateTimeRange range = ParseRange (request.Range);

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
      if (log.IsDebugEnabled) {
        log.Debug ($"GetCacheTimeOut: cacheTimeSpan is {cacheTimeSpan} for url={url}");
      }
      return cacheTimeSpan;
    }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override async System.Threading.Tasks.Task<object> Get (OperationCycleSlotsRequestDTO request)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        int machineId = request.MachineId;
        IMachine machine = await ModelDAOHelper.DAOFactory.MachineDAO
          .FindByIdAsync (machineId);
        if (null == machine) {
          log.Error ($"GetWithoutCache: unknown machine with ID {machineId}");
          return new ErrorDTO ("No machine with the specified ID",
                               ErrorStatus.WrongRequestParameter);
        }

        UtcDateTimeRange range = ParseRange (request.Range);

        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Web.OperationCycleSlots")) {
          return await GetAutoSplitAsync (machine, range, request.SkipDetails); // Less than a week
        }
      }
    }

    async System.Threading.Tasks.Task<OperationCycleSlotsResponseDTO> GetAutoSplitAsync (IMachine machine, UtcDateTimeRange range, bool skipDetails)
    {
      Debug.Assert (range.Duration.HasValue);

      int verticalSplitMaxTotalDurationFactor = Lemoine.Info.ConfigSet
        .LoadAndGet<int> (VERTICAL_SPLIT_TOTAL_DIVISOR_KEY,
                          VERTICAL_SPLIT_TOTAL_DIVISOR_DEFAULT);
      TimeSpan minimumSplitDuration =
        TimeSpan.FromTicks (range.Duration.Value.Ticks / verticalSplitMaxTotalDurationFactor);

      var result = new OperationCycleSlotsResponseDTO ();
      result.Range = range.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
      result.Blocks = new List<OperationCycleSlotBlockDTO> ();

      var slots = await ModelDAOHelper.DAOFactory.OperationCycleDAO
        .FindOverlapsRangeAsync (machine, range);

      IList<IOperationCycle> pending = new List<IOperationCycle> ();
      IOperationCycle previousSlot = null;
      UtcDateTimeRange previousDateTimeRange = null;
      var lastSlot = slots.LastOrDefault ();
      foreach (var slot in slots) {
        if (!slot.Begin.HasValue) {
          log.Debug ($"GetAutoSplitAsync: operation cycle with no start => skip it");
          continue;
        }
        if (!slot.End.HasValue && (lastSlot != slot)) {
          log.Debug ($"GetAutoSplitAsync: operation cycle (not last) with no end => skip it");
          continue;
        }
        var dateTimeRange = GetRange (slot);
        UtcDateTimeRange slotRange = new UtcDateTimeRange (dateTimeRange.Intersects (range));
        Debug.Assert (!slotRange.IsEmpty ());
        Debug.Assert (slotRange.Duration.HasValue);

        if ((null != previousSlot)
            && (minimumSplitDuration <= dateTimeRange.Lower.Value.Subtract (previousDateTimeRange.Upper.Value))) {
          // Large gap
          await PushBlockAndAddToPendingAsync (result, pending, skipDetails, slot);
        }
        else if (minimumSplitDuration <= slotRange.Duration.Value) { // Long enough
          await PushBlockAndAddSlotAsync (result, pending, skipDetails, slot);
        }
        else {
          pending.Add (slot);
        }

        previousSlot = slot;
        previousDateTimeRange = dateTimeRange;
      }
      await PushBlockAsync (result, pending, skipDetails);

      return result;
    }

    async System.Threading.Tasks.Task PushBlockAndAddToPendingAsync (OperationCycleSlotsResponseDTO result, IList<IOperationCycle> pending,
                                   bool skipDetails, IOperationCycle slot)
    {
      await PushBlockAsync (result, pending, skipDetails);
      pending.Clear ();
      pending.Add (slot);
    }

    async System.Threading.Tasks.Task PushBlockAndAddSlotAsync (OperationCycleSlotsResponseDTO result, IList<IOperationCycle> pending,
                              bool skipDetails, IOperationCycle slot)
    {
      await PushBlockAsync (result, pending, skipDetails);
      pending.Clear ();
      pending.Add (slot);
      await PushBlockAsync (result, pending, skipDetails);
      pending.Clear ();
    }

    string GetColor ()
    {
      m_colorIndex = 1 - m_colorIndex;
      return (1 == m_colorIndex) ? EVEN_COLOR : ODD_COLOR;
    }

    async System.Threading.Tasks.Task PushBlockAsync (OperationCycleSlotsResponseDTO response, IList<IOperationCycle> slots, bool skipDetails)
    {
      if (!slots.Any ()) {
        return;
      }

      var block = new OperationCycleSlotBlockDTO ();
      var range = new UtcDateTimeRange (slots.First ().Begin.Value,
                                        new UpperBound<DateTime> (slots.Last ().End),
                                        !slots.First ().Status.HasFlag (OperationCycleStatus.BeginEstimated),
                                        !slots.Last ().Status.HasFlag (OperationCycleStatus.EndEstimated));
      block.Range = range
        .ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
      if (1 == slots.Count) {
        var slot = slots[0];
        int id = slot.Id;
        block.Id = id;
        block.BgColor = GetColor ();
        block.FgColor = ColorGenerator.GetContrastColor (block.BgColor);
        block.Display = await GetDisplayAsync (slot);
      }
      else { // 1 < slots.Count
        Debug.Assert (1 < slots.Count);
        var byDescendingDuration = slots
          .Where (s => s.HasRealBegin () && s.HasRealEnd ())
          .Select (s => new KeyValuePair<int, double> (s.Id, s.End.Value.Subtract (s.Begin.Value).TotalSeconds))
          .OrderByDescending (kv => kv.Value)
          .ToList ();
        if (0 == byDescendingDuration.Count) {
          block.Id = 0;
          block.BgColor = GetColor ();
          block.FgColor = ColorGenerator.GetContrastColor (block.BgColor);
        }
        else if (1 == byDescendingDuration.Count) {
          block.BgColor = GetColor ();
          block.FgColor = ColorGenerator.GetContrastColor (block.BgColor);
        }
        else { // 2 <= byDecendingDuration.Count
          block.BgColor = EVEN_COLOR;
          block.PatternName = "diagonal-stripe-3";
          block.PatternColor = ODD_COLOR;
        }
      }
      if (skipDetails) {
        block.Details = null;
      }
      else { // !skipDetails
        block.Details = new List<OperationCycleSlotBlockDetailDTO> ();
        foreach (var slot in slots.OrderBy (s => s.Begin.Value)) {
          var detail = new OperationCycleSlotBlockDetailDTO ();
          detail.Range = GetRange (slot).ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
          detail.Display = await GetDisplayAsync (slot);
          block.Details.Add (detail);
        }
      }
      response.Blocks.Add (block);
    }

    UtcDateTimeRange GetRange (IOperationCycle slot)
    {
      return new UtcDateTimeRange (slot.Begin.Value, new UpperBound<DateTime> (slot.End),
        !slot.Status.HasFlag (OperationCycleStatus.BeginEstimated),
        !slot.Status.HasFlag (OperationCycleStatus.EndEstimated));
    }

    async System.Threading.Tasks.Task<string> GetDisplayAsync (IOperationCycle slot)
    {
      var deliverablePieces = await ModelDAOHelper.DAOFactory.DeliverablePieceDAO
        .FindByOperationCycleAsync (slot);
      return string.Join (" ", deliverablePieces.Select (d => d.Display).ToArray ());
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
