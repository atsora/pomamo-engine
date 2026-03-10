// Copyright (C) 2026 Atsora Solutions
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

namespace Pulse.Web.Reason
{
  /// <summary>
  /// Description of ReasonOverwriteRequiredSlotsService
  /// </summary>
  public class ReasonOverwriteRequiredSlotsService
    : GenericCachedService<ReasonOverwriteRequiredSlotsRequestDTO>
  {
    static readonly string CURRENT_MARGIN_KEY = "Web.ReasonOverwriteRequiredSlots.CurrentMargin";
    static readonly TimeSpan CURRENT_MARGIN_DEFAULT = TimeSpan.FromMinutes (1);

    static readonly ILog log = LogManager.GetLogger (typeof (ReasonOverwriteRequiredSlotsService).FullName);

    /// <summary>
    /// 
    /// </summary>
    public ReasonOverwriteRequiredSlotsService ()
      : base (Lemoine.Core.Cache.CacheTimeOut.CurrentShort)
    {
    }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache (ReasonOverwriteRequiredSlotsRequestDTO request)
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

    ReasonOverwriteRequiredSlotsResponseDTO Get (IMachine machine, UtcDateTimeRange range)
    {
      var result = new ReasonOverwriteRequiredSlotsResponseDTO ();
      result.Range = range.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
      result.ReasonOverwriteRequiredSlots = new List<ReasonOverwriteRequiredSlotDTO> ();

      if (range.IsPoint ()) {
        Debug.Assert (range.Lower.HasValue); // Point
        Debug.Assert (range.Lower.Value.Equals (range.Upper.Value));
        var slot = (new Lemoine.Business.Reason.ReasonOverwriteRequiredSlotDAO ())
          .FindAt (machine, range.Lower.Value, true);
        if (null != slot) {
          var slotDto = new ReasonOverwriteRequiredSlotDTO ();
          slotDto.Range = slot.DateTimeRange.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
          slotDto.Color = slot.Color;
          slotDto.BgColor = slot.Color;
          slotDto.FgColor = ColorGenerator.GetContrastColor (slotDto.BgColor);
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
          result.ReasonOverwriteRequiredSlots.Add (slotDto);
        }
      }
      else { // Real range
        var slots = (new Lemoine.Business.Reason.ReasonOverwriteRequiredSlotDAO ())
          .FindOverlapsRange (machine, range, true);
        foreach (var slot in slots) {
          var slotDto = new ReasonOverwriteRequiredSlotDTO ();
          slotDto.Range = slot.DateTimeRange.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
          slotDto.Color = slot.Color;
          slotDto.BgColor = slot.Color;
          slotDto.FgColor = ColorGenerator.GetContrastColor (slotDto.BgColor);
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
          result.ReasonOverwriteRequiredSlots.Add (slotDto);
        }
      }

      return result;
    }
  }
}
