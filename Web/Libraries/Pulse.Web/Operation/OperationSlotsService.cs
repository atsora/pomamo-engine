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
  /// Description of OperationSlotsService
  /// </summary>
  public class OperationSlotsService
    : GenericCachedService<OperationSlotsRequestDTO>
  {
    static readonly string VERTICAL_SPLIT_TOTAL_DIVISOR_KEY = "Web.VerticalSplit.TotalDurationDivisor";
    static readonly int VERTICAL_SPLIT_TOTAL_DIVISOR_DEFAULT = 640;
    // 400 corresponds for a full day to 3,6 minutes (on a 1200px width screen, about 3px, on a FullHD screen, about 5 px)
    // 640 corresponds for a full day to 2,5 minutes (on a 1200px width screen, about 2px, on a FullHD screen, about 3 px)
    // 960 corresponds for a full day to 1,5 minutes (on a 1200px width screen, about 1px, on a FullHD screen, about 2 px)

    static readonly ILog log = LogManager.GetLogger(typeof (OperationSlotsService).FullName);
    
    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public OperationSlotsService ()
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
    protected override TimeSpan GetCacheTimeOut (string url, OperationSlotsRequestDTO requestDTO)
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
    public override object GetWithoutCache(OperationSlotsRequestDTO request)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        int machineId = request.MachineId;
        IMachine machine = ModelDAOHelper.DAOFactory.MachineDAO
          .FindById (machineId);
        if (null == machine) {
          log.ErrorFormat ("GetWithoutCache: " +
                           "unknown machine with ID {0}",
                           machineId);
          return new ErrorDTO ("No machine with the specified ID",
                               ErrorStatus.WrongRequestParameter);
        }
        
        UtcDateTimeRange range = ParseRange (request.Range);
        
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Web.OperationSlots"))
        {
          return GetAutoSplit (machine, range, request.SkipDetails); // Less than a week
        }
      }
    }
    
    OperationSlotsResponseDTO GetAutoSplit (IMachine machine, UtcDateTimeRange range, bool skipDetails)
    {
      Debug.Assert (range.Duration.HasValue);
      
      int verticalSplitMaxTotalDurationFactor = Lemoine.Info.ConfigSet
        .LoadAndGet<int> (VERTICAL_SPLIT_TOTAL_DIVISOR_KEY,
                          VERTICAL_SPLIT_TOTAL_DIVISOR_DEFAULT);
      TimeSpan minimumSplitDuration =
        TimeSpan.FromTicks(range.Duration.Value.Ticks / verticalSplitMaxTotalDurationFactor);
      
      var result = new OperationSlotsResponseDTO ();
      result.Range = range.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
      result.Blocks = new List<OperationSlotBlockDTO> ();

      var slots = ModelDAOHelper.DAOFactory.OperationSlotDAO
        .FindOverlapsRange (machine, range);
      
      // Remove some items at the end if they are not really effective
      int numberToRemove = 0;
      {
        DateTime now = DateTime.UtcNow;
        IOperationSlot firstNotInFuture = null;
        int pendingCounter = 0;
        foreach (var slot in slots.Reverse ()) {
          if (Bound.Compare<DateTime> (now, slot.BeginDateTime) <= 0) { // In the future => remove
            ++numberToRemove;
          }
          else { // Not completely in the future
            if (null != slot.Operation) { // An operation ! Keep them now
              if (0 < pendingCounter) {
                // Is there an operation between the slot and now ?
                if (!ModelDAOHelper.DAOFactory.OperationSlotDAO
                    .ExistsDifferentOperationBetween (machine, slot.EndDateTime.Value, now, null)) {
                  numberToRemove += pendingCounter;
                }
              }
              break;
            }
            if (null == firstNotInFuture) {
              firstNotInFuture = slot;
            }
            Debug.Assert (null != firstNotInFuture);
            Debug.Assert (null == slot.Operation);
            if (firstNotInFuture.EndDateTime.HasValue
                && !firstNotInFuture.DateTimeRange.ContainsElement (now)) {
              ++pendingCounter;
            }
            else {
              ++numberToRemove;
            }
          }
        }
      }
      
      IList<IOperationSlot> pending = new List<IOperationSlot> ();
      IOperationSlot previousSlot = null;
      foreach (var slot in slots.Take (slots.Count-numberToRemove)) {
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

    void PushBlockAndAddToPending (OperationSlotsResponseDTO result, IList<IOperationSlot> pending,
                                   bool skipDetails, IOperationSlot slot)
    {
      PushBlock (result, pending, skipDetails);
      pending.Clear ();
      pending.Add (slot);
    }

    void PushBlockAndAddSlot (OperationSlotsResponseDTO result, IList<IOperationSlot> pending,
                              bool skipDetails, IOperationSlot slot)
    {
      PushBlock (result, pending, skipDetails);
      pending.Clear ();
      pending.Add (slot);
      PushBlock (result, pending, skipDetails);
      pending.Clear ();
    }
    
    void PushBlock (OperationSlotsResponseDTO response, IList<IOperationSlot> slots, bool skipDetails)
    {
      if (!slots.Any ()) {
        return;
      }
      
      var block = new OperationSlotBlockDTO ();
      var range = new UtcDateTimeRange (slots.First ().DateTimeRange.Lower,
                                        slots.Last ().DateTimeRange.Upper,
                                        slots.First ().DateTimeRange.LowerInclusive,
                                        slots.Last ().DateTimeRange.UpperInclusive);
      block.Range = range
        .ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
      if (1 == slots.Count) {
        var slot = slots [0];
        int id = GetOperationId (slot);
        block.Id = id;
        block.BgColor = ColorGenerator.GetColor ("Operation", id);
        block.FgColor = ColorGenerator.GetContrastColor (block.BgColor);
        block.Display = slot.Display;
      }
      else { // 1 < slots.Count
        Debug.Assert (1 < slots.Count);
        var byDescendingDuration = slots
          .Where (s => null != s.Operation)
          .GroupBy (s => ((IDataWithId<int>)s.Operation).Id)
          .Select (sk => new KeyValuePair<int, double> (sk.Key,
                                                          sk.Sum (s => s.DateTimeRange.Duration.HasValue
                                                                  ? s.DateTimeRange.Duration.Value.TotalSeconds
                                                                  : TimeSpan.FromDays (365).TotalSeconds)))
          .OrderByDescending (kv => kv.Value)
          .ToList ();
        if (0 == byDescendingDuration.Count) {
          Debug.Assert (null == slots [0].Operation);
          block.Id = 0;
          block.BgColor = ColorGenerator.GetColor ("Operation", 0);
          block.FgColor = ColorGenerator.GetContrastColor (block.BgColor);
        }
        else if (1 == byDescendingDuration.Count) {
          block.BgColor = ColorGenerator.GetColor ("Operation", byDescendingDuration [0].Key);
          block.FgColor = ColorGenerator.GetContrastColor (block.BgColor);
        }
        else { // 2 <= byDecendingDuration.Count
          block.BgColor = ColorGenerator.GetColor ("Operation", byDescendingDuration [0].Key);
          block.PatternName = "diagonal-stripe-3";
          block.PatternColor = ColorGenerator.GetColor ("Operation", byDescendingDuration [1].Key);
        }
      }
      if (skipDetails) {
        block.Details = null;
      }
      else { // !skipDetails
        block.Details = new List<OperationSlotBlockDetailDTO> ();
        foreach (var slot in slots.OrderBy (s => s.DateTimeRange)) {
          var detail = new OperationSlotBlockDetailDTO ();
          detail.Range = slot.DateTimeRange.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
          detail.Display = slot.Display;
          block.Details.Add (detail);
        }
      }
      response.Blocks.Add (block);
    }
    
    /// <summary>
    /// Return the operation id or 0 if there is no operation
    /// </summary>
    /// <param name="operationSlot"></param>
    /// <returns></returns>
    int GetOperationId (IOperationSlot operationSlot)
    {
      Debug.Assert (null != operationSlot);
      
      if (null == operationSlot.Operation) {
        return 0;
      }
      else {
        return ((IDataWithId<int>)operationSlot.Operation).Id;
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
