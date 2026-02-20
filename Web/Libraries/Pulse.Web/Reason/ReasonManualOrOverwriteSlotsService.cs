// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
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
using Lemoine.Web;
using Pulse.Business.Reason;

namespace Pulse.Web.Reason
{
  /// <summary>
  /// Description of ReasonManualOrOverwriteSlotsService
  /// </summary>
  public class ReasonManualOrOverwriteSlotsService
    : GenericCachedService<ReasonManualOrOverwriteSlotsRequestDTO>
  {
    static readonly string CURRENT_MARGIN_KEY = "Web.ReasonManualOrOverwriteSlots.CurrentMargin";
    static readonly TimeSpan CURRENT_MARGIN_DEFAULT = TimeSpan.FromMinutes (1);

    static readonly ILog log = LogManager.GetLogger (typeof (ReasonManualOrOverwriteSlotsService).FullName);

    /// <summary>
    /// 
    /// </summary>
    public ReasonManualOrOverwriteSlotsService ()
      : base (Lemoine.Core.Cache.CacheTimeOut.CurrentShort)
    {
    }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache (ReasonManualOrOverwriteSlotsRequestDTO request)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        int machineId = request.MachineId;
        IMachine machine = ModelDAOHelper.DAOFactory.MachineDAO
          .FindById (machineId);
        if (null == machine) {
          log.Error ($"GetWithoutCache: unknown machine with ID {machineId}");
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

        return Get (machine, range);
      }
    }

    ReasonManualOrOverwriteSlotsResponseDTO Get (IMachine machine, UtcDateTimeRange range)
    {
      var result = new ReasonManualOrOverwriteSlotsResponseDTO ();
      result.Range = range.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
      result.ReasonManualOrOverwriteSlots = new List<ReasonManualOrOverwriteSlotDTO> ();

      if (range.IsPoint ()) {
        Debug.Assert (range.Lower.HasValue); // Point
        Debug.Assert (range.Lower.Value.Equals (range.Upper.Value));
        var slot = (new Lemoine.Business.Reason.ReasonManualOrOverwriteSlotDAO ())
          .FindAt (machine, range.Lower.Value, true);
        if (null != slot) {
          var slotDto = new ReasonManualOrOverwriteSlotDTO ();
          slotDto.Range = slot.DateTimeRange.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
          slotDto.Display = slot.Reason?.Display ?? string.Empty;
          if (!ReasonData.IsJsonNullOrEmpty (slot.JsonData)) {
            slotDto.Display = ReasonData.OverwriteDisplay (slotDto.Display, slot.JsonData, false);
          }
          slotDto.OverwriteRequired = slot.OverwriteRequired;
          var currentMargin = ConfigSet.LoadAndGet<TimeSpan> (CURRENT_MARGIN_KEY,
                                                              CURRENT_MARGIN_DEFAULT);
          if (Bound.Compare<DateTime> (DateTime.UtcNow.Subtract (currentMargin),
                                       slot.DateTimeRange.Upper) < 0) { // May be current ?
            var machineStatus = ModelDAOHelper.DAOFactory.MachineStatusDAO
              .FindById (machine.Id);
            if (null != machineStatus) { // Normally always
              Debug.Assert (slot.DateTimeRange.Upper.HasValue);
              if (machineStatus.ReasonSlotEnd.Equals (slot.DateTimeRange.Upper.Value)) { // This is the current one !
                slotDto.Current = true;
              }
            }
          }
          result.ReasonManualOrOverwriteSlots.Add (slotDto);
        }
      }
      else { // Real range
        var slots = (new Lemoine.Business.Reason.ReasonManualOrOverwriteSlotDAO ())
          .FindOverlapsRange (machine, range, true);
        foreach (var slot in slots) {
          var slotDto = new ReasonManualOrOverwriteSlotDTO ();
          slotDto.Range = slot.DateTimeRange.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
          slotDto.Display = slot.Reason?.Display ?? string.Empty;
          if (!ReasonData.IsJsonNullOrEmpty (slot.JsonData)) {
            slotDto.Display = ReasonData.OverwriteDisplay (slotDto.Display, slot.JsonData, false);
          }
          slotDto.OverwriteRequired = slot.OverwriteRequired;
          var currentMargin = ConfigSet.LoadAndGet<TimeSpan> (CURRENT_MARGIN_KEY,
                                                              CURRENT_MARGIN_DEFAULT);
          if (Bound.Compare<DateTime> (DateTime.UtcNow.Subtract (currentMargin),
                                       slot.DateTimeRange.Upper) < 0) { // May be current ?
            var machineStatus = ModelDAOHelper.DAOFactory.MachineStatusDAO
              .FindById (machine.Id);
            if (null != machineStatus) { // Normally always
              Debug.Assert (slot.DateTimeRange.Upper.HasValue);
              if (machineStatus.ReasonSlotEnd.Equals (slot.DateTimeRange.Upper.Value)) { // This is the current one !
                slotDto.Current = true;
              }
            }
          }
          result.ReasonManualOrOverwriteSlots.Add (slotDto);
        }
      }

      return result;
    }
  }
}
