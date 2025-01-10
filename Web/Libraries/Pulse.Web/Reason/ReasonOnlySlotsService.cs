// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Lemoine.Core.Cache;
using Lemoine.Info;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Web.CommonRequestDTO;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Extensions.Web.Responses;
using System.Threading.Tasks;
using Lemoine.Extensions.Web.Interfaces;
using Lemoine.Web;
using Pulse.Business.Reason;

namespace Pulse.Web.Reason
{
  /// <summary>
  /// Description of ReasonOnlySlotsService
  /// </summary>
  public class ReasonOnlySlotsService
    : GenericCachedService<ReasonOnlySlotsRequestDTO>
    , IBodySupport
  {
    static readonly string CURRENT_MARGIN_KEY = "Web.ReasonOnlySlots.CurrentMargin";
    static readonly TimeSpan CURRENT_MARGIN_DEFAULT = TimeSpan.FromMinutes (1);

    static readonly ILog log = LogManager.GetLogger (typeof (ReasonOnlySlotsService).FullName);

    Stream m_body;

    /// <summary>
    /// 
    /// </summary>
    public ReasonOnlySlotsService ()
      : base (Lemoine.Core.Cache.CacheTimeOut.CurrentShort)
    {
    }

    /// <summary>
    /// Get the cache time out
    /// </summary>
    /// <param name="url"></param>
    /// <param name="requestDTO"></param>
    /// <returns></returns>
    protected override TimeSpan GetCacheTimeOut (string url, ReasonOnlySlotsRequestDTO requestDTO)
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
    public override object GetWithoutCache (ReasonOnlySlotsRequestDTO request)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
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

        UtcDateTimeRange range;
        if (string.IsNullOrEmpty (request.Range)) {
          range = new UtcDateTimeRange (DateTime.UtcNow, DateTime.UtcNow, "[]");
        }
        else {
          range = new UtcDateTimeRange (request.Range);
        }

        UtcDateTimeRange extendLimitrange;
        if (string.IsNullOrEmpty (request.ExtendLimitRange)) {
          extendLimitrange = new UtcDateTimeRange (new LowerBound<DateTime> (), new UpperBound<DateTime> ()); // (-oo,+oo)
        }
        else {
          extendLimitrange = new UtcDateTimeRange (request.ExtendLimitRange);
        }

        return Get (machine, range, request.SelectableOption, request.NoPeriodExtension, extendLimitrange);
      }
    }

    ReasonOnlySlotsResponseDTO Get (IMachine machine, UtcDateTimeRange range, bool selectableOption, bool noPeriodExtension, UtcDateTimeRange extendLimitRange)
    {
      var result = new ReasonOnlySlotsResponseDTO ();
      result.Range = range.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
      result.RangeNumber = 1;
      result.ReasonOnlySlots = new List<ReasonOnlySlotDTO> ();

      bool lowerLimitReached, upperLimitReached;
      if (selectableOption) {
        if (range.IsPoint ()) {
          Debug.Assert (range.Lower.HasValue); // Point
          Debug.Assert (range.Lower.Value.Equals (range.Upper.Value));
          var slot = (new Lemoine.Business.Reason.ReasonSelectionSlotDAO ())
            .FindAtWithReason (machine, range.Lower.Value, extendLimitRange, out lowerLimitReached, out upperLimitReached, out var reasonSlot);
          if (null != slot) {
            var slotDto = new ReasonOnlySlotDTO ();
            slotDto.Id = slot.Reason.Id;
            slotDto.Display = slot.Reason.Display;
            slotDto.LongDisplay = slot.Reason.LongDisplay;
            if (!ReasonData.IsJsonNullOrEmpty (slot.JsonData)) {
              slotDto.Display = ReasonData.OverwriteDisplay (slotDto.Display, slot.JsonData, false);
              slotDto.LongDisplay = ReasonData.OverwriteDisplay (slotDto.LongDisplay, slot.JsonData, true);
            }
            slotDto.Description = slot.Reason.DescriptionOrTranslation;
            slotDto.Range = slot.DateTimeRange.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
            slotDto.BgColor = slot.Reason.Color;
            slotDto.FgColor = ColorGenerator.GetContrastColor (slotDto.BgColor);
            if (null != reasonSlot) {
              slotDto.Score = reasonSlot.ReasonScore;
              slotDto.Source = new ReasonSourceDTO (reasonSlot.ReasonSource);
              slotDto.AutoReasonNumber = reasonSlot.AutoReasonNumber;
            }
            slotDto.Details = slot.ReasonDetails;
            slotDto.OverwriteRequired = slot.OverwriteRequired;
            slotDto.Running = slot.Running;
            slotDto.DefaultReason = slot.DefaultReason;
            TimeSpan currentMargin = ConfigSet.LoadAndGet<TimeSpan> (CURRENT_MARGIN_KEY,
                                                                     CURRENT_MARGIN_DEFAULT);
            if (Bound.Compare<DateTime> (DateTime.UtcNow.Subtract (currentMargin),
                                         slot.DateTimeRange.Upper) < 0) { // May be current ?
              IMachineStatus machineStatus = ModelDAOHelper.DAOFactory.MachineStatusDAO
                .FindById (machine.Id);
              if (null != machineStatus) { // Normally always
                Debug.Assert (slot.DateTimeRange.Upper.HasValue);
                if (machineStatus.ReasonSlotEnd.Equals (slot.DateTimeRange.Upper.Value)) { // This is the current one !
                  slotDto.Current = true;
                }
              }
            }
            slotDto.IsSelectable = slot.SelectableReasons.Any ();
            slotDto.MachineModes = new List<ReasonOnlyMachineModeSubSlotDTO> ();
            foreach (var subSlot in slot.MachineModeSlots) {
              var machineModeDto = new ReasonOnlyMachineModeSubSlotDTO ();
              if (noPeriodExtension && !range.Overlaps (subSlot.DateTimeRange)) {
                continue;
              }
              machineModeDto.Id = subSlot.MachineMode.Id;
              machineModeDto.Display = subSlot.MachineMode.Display;
              machineModeDto.Range = subSlot.DateTimeRange.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
              machineModeDto.BgColor = subSlot.MachineMode.Color;
              machineModeDto.FgColor = ColorGenerator.GetContrastColor (machineModeDto.BgColor);
              machineModeDto.Category = new MachineModeCategoryDTOAssembler ().Assemble (subSlot.MachineMode.MachineModeCategory);
              slotDto.MachineModes.Add (machineModeDto);
            }
            result.ReasonOnlySlots.Add (slotDto);
          }
        }
        else { // Real range
          var slots = (new Lemoine.Business.Reason.ReasonSelectionSlotDAO ())
            .FindOverlapsRangeWithReason (machine, range, extendLimitRange, out lowerLimitReached, out upperLimitReached);
          foreach (var slot in slots) {
            var slotDto = new ReasonOnlySlotDTO ();
            slotDto.Id = slot.Reason.Id;
            slotDto.Display = slot.Reason.Display;
            slotDto.LongDisplay = slot.Reason.LongDisplay;
            if (!ReasonData.IsJsonNullOrEmpty (slot.JsonData)) {
              slotDto.Display = ReasonData.OverwriteDisplay (slotDto.Display, slot.JsonData, false);
              slotDto.LongDisplay = ReasonData.OverwriteDisplay (slotDto.LongDisplay, slot.JsonData, true);
            }
            slotDto.Description = slot.Reason.DescriptionOrTranslation;
            slotDto.Range = slot.DateTimeRange.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
            slotDto.BgColor = slot.Reason.Color;
            slotDto.FgColor = ColorGenerator.GetContrastColor (slotDto.BgColor);
            // Note: reasonScore, reasonSource and autoReasonNumber are not available if the selectableOption is on
            slotDto.Details = slot.ReasonDetails;
            slotDto.OverwriteRequired = slot.OverwriteRequired;
            slotDto.Running = slot.Running;
            slotDto.DefaultReason = slot.DefaultReason;
            TimeSpan currentMargin = ConfigSet.LoadAndGet<TimeSpan> (CURRENT_MARGIN_KEY,
                                                                     CURRENT_MARGIN_DEFAULT);
            if (Bound.Compare<DateTime> (DateTime.UtcNow.Subtract (currentMargin),
                                         slot.DateTimeRange.Upper) < 0) { // May be current ?
              IMachineStatus machineStatus = ModelDAOHelper.DAOFactory.MachineStatusDAO
                .FindById (machine.Id);
              if (null != machineStatus) { // Normally always
                Debug.Assert (slot.DateTimeRange.Upper.HasValue);
                if (machineStatus.ReasonSlotEnd.Equals (slot.DateTimeRange.Upper.Value)) { // This is the current one !
                  slotDto.Current = true;
                }
              }
            }
            slotDto.IsSelectable = slot.SelectableReasons.Any ();
            slotDto.MachineModes = new List<ReasonOnlyMachineModeSubSlotDTO> ();
            foreach (var subSlot in slot.MachineModeSlots) {
              var machineModeDto = new ReasonOnlyMachineModeSubSlotDTO ();
              if (noPeriodExtension && !range.Overlaps (subSlot.DateTimeRange)) {
                continue;
              }
              machineModeDto.Id = subSlot.MachineMode.Id;
              machineModeDto.Display = subSlot.MachineMode.Display;
              machineModeDto.Range = subSlot.DateTimeRange.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
              machineModeDto.BgColor = subSlot.MachineMode.Color;
              machineModeDto.FgColor = ColorGenerator.GetContrastColor (machineModeDto.BgColor);
              machineModeDto.Category = new MachineModeCategoryDTOAssembler ().Assemble (subSlot.MachineMode.MachineModeCategory);
              slotDto.MachineModes.Add (machineModeDto);
            }
            result.ReasonOnlySlots.Add (slotDto);
          }
        }
      }
      else { // !selectableOption
        var slots = (new Lemoine.Business.Reason.ReasonOnlySlotDAO ())
          .FindOverlapsRangeWithReason (machine, range, extendLimitRange, out lowerLimitReached, out upperLimitReached);
        foreach (var slot in slots) {
          var slotDto = new ReasonOnlySlotDTO ();
          slotDto.Id = slot.Reason.Id;
          slotDto.Display = slot.Reason.Display;
          slotDto.LongDisplay = slot.Reason.LongDisplay;
          if (!ReasonData.IsJsonNullOrEmpty (slot.JsonData)) {
            slotDto.Display = ReasonData.OverwriteDisplay (slotDto.Display, slot.JsonData, false);
            slotDto.LongDisplay = ReasonData.OverwriteDisplay (slotDto.LongDisplay, slot.JsonData, true);
          }
          slotDto.Description = slot.Reason.DescriptionOrTranslation;
          slotDto.Range = slot.DateTimeRange.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
          slotDto.BgColor = slot.Reason.Color;
          slotDto.FgColor = ColorGenerator.GetContrastColor (slotDto.BgColor);
          slotDto.Score = slot.ReasonScore;
          slotDto.Source = new ReasonSourceDTO (slot.ReasonSource);
          slotDto.AutoReasonNumber = slot.AutoReasonNumber;
          slotDto.Details = slot.ReasonDetails;
          slotDto.OverwriteRequired = slot.OverwriteRequired;
          slotDto.Running = slot.Running;
          slotDto.DefaultReason = slot.DefaultReason;
          TimeSpan currentMargin = ConfigSet.LoadAndGet<TimeSpan> (CURRENT_MARGIN_KEY,
                                                                   CURRENT_MARGIN_DEFAULT);
          if (Bound.Compare<DateTime> (DateTime.UtcNow.Subtract (currentMargin),
                                       slot.DateTimeRange.Upper) < 0) { // May be current ?
            IMachineStatus machineStatus = ModelDAOHelper.DAOFactory.MachineStatusDAO
              .FindById (machine.Id);
            if (null != machineStatus) { // Normally always
              Debug.Assert (slot.DateTimeRange.Upper.HasValue);
              if (machineStatus.ReasonSlotEnd.Equals (slot.DateTimeRange.Upper.Value)) { // This is the current one !
                slotDto.Current = true;
              }
            }
          }
          slotDto.MachineModes = new List<ReasonOnlyMachineModeSubSlotDTO> ();
          foreach (var subSlot in slot.MachineModeSlots) {
            var machineModeDto = new ReasonOnlyMachineModeSubSlotDTO ();
            if (noPeriodExtension && !range.Overlaps (subSlot.DateTimeRange)) {
              continue;
            }
            machineModeDto.Id = subSlot.MachineMode.Id;
            machineModeDto.Display = subSlot.MachineMode.Display;
            machineModeDto.Range = subSlot.DateTimeRange.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
            machineModeDto.BgColor = subSlot.MachineMode.Color;
            machineModeDto.FgColor = ColorGenerator.GetContrastColor (machineModeDto.BgColor);
            machineModeDto.Category = new MachineModeCategoryDTOAssembler ().Assemble (subSlot.MachineMode.MachineModeCategory);
            slotDto.MachineModes.Add (machineModeDto);
          }
          result.ReasonOnlySlots.Add (slotDto);
        }
      }
      if (lowerLimitReached) {
        result.ReasonOnlySlots.First ().LowerLimitReached = true;
      }
      if (upperLimitReached) {
        result.ReasonOnlySlots.Last ().UpperLimitReached = true;
      }

      return result;
    }

    /// <summary>
    /// Post method
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<object> Post (ReasonOnlySlotsPostRequestDTO request)
    {
      int machineId = request.MachineId;

      log.Debug ("GetReasonSelectionV3 begin");

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Web.ReasonOnlySlots")) {
        IMachine machine = ModelDAOHelper.DAOFactory.MachineDAO
          .FindById (machineId);
        if (null == machine) {
          log.ErrorFormat ("Post: " +
                           "unknown machine with ID {0}",
                           machineId);
          return new ErrorDTO ("No machine with the specified ID",
                               ErrorStatus.WrongRequestParameter);
        }

        // Ranges
        RangesPostDTO deserializedResult = PostDTO.Deserialize<RangesPostDTO> (m_body);
        ReasonOnlySlotsResponseDTO response = null;
        foreach (var range in deserializedResult.ExtractRanges ()) {
          UtcDateTimeRange extendLimitrange;
          if (string.IsNullOrEmpty (request.ExtendLimitRange)) {
            extendLimitrange = new UtcDateTimeRange (new LowerBound<DateTime> (null),
                                                     new UpperBound<DateTime> (null));
          }
          else {
            extendLimitrange = new UtcDateTimeRange (request.ExtendLimitRange);
          }

          if (null == response) {
            // TODO: write an asynchronous method for Get
            response = await Task.Run ( () => Get (machine, range, request.SelectableOption, request.NoPeriodExtension, extendLimitrange));
          }
          else { // Merge
            Merge (response, Get (machine, range, request.SelectableOption, request.NoPeriodExtension, extendLimitrange));
          }
        }

        return response;
      }
    }

    ReasonOnlySlotsResponseDTO Merge (ReasonOnlySlotsResponseDTO response, ReasonOnlySlotsResponseDTO additional)
    {
      Debug.Assert (null != response);
      Debug.Assert (null != additional);

      if (null == additional) {
        return response;
      }
      else {
        response.Range = null;
        ++response.RangeNumber;
        foreach (var element in additional.ReasonOnlySlots) {
          response.ReasonOnlySlots.Add (element);
        }
        return response;
      }
    }

    #region IBodySupport
    /// <summary>
    /// <see cref="IBodySupport"/>
    /// </summary>
    /// <param name="body"></param>
    public void SetBody (Stream body)
    {
      m_body = body;
    }
    #endregion // IBodySupport
  }
}
