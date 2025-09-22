// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Web;
using Lemoine.Extensions.Web.Responses;

namespace Pulse.Web.Time
{
  /// <summary>
  /// Description of RangeAroundService
  /// </summary>
  public class RangeAroundService
    : GenericCachedService<RangeAroundRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (RangeAroundService).FullName);

    static readonly string SHIFT_DAY_FORMAT_KEY = "Web.RangeAround.ShiftDay.Format";
    static readonly string SHIFT_DAY_FORMAT_DEFAULT = "d";

    /// <summary>
    /// 
    /// </summary>
    public RangeAroundService ()
      : base(Lemoine.Core.Cache.CacheTimeOut.CurrentShort) // Config for Lemoine.Business.RangeAroundDay
    {
    }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache(RangeAroundRequestDTO request)
    {
      string rangeType = string.IsNullOrEmpty (request.RangeType)
        ? "day"
        : request.RangeType;
      int rangeSize = request.RangeSize.HasValue
        ? request.RangeSize.Value
        : 1;
      DateTime aroundDateTime;
      if (string.IsNullOrEmpty (request.Around)) {
        aroundDateTime = DateTime.UtcNow;
      }
      else {
        aroundDateTime = ConvertDTO.IsoStringToDateTimeUtc (request.Around).Value;
      }
      var response = new RangeAroundResponseDTO ();

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ("Web.Time.RangeAroundService")) // Read-write because of the days
      {
        if (rangeType.Equals ("shift", StringComparison.InvariantCultureIgnoreCase)) {
          IShiftSlot slot = ModelDAOHelper.DAOFactory.ShiftSlotDAO
            .FindAt (aroundDateTime);
          if (null != slot) {
            response.DateTimeRange = slot.DateTimeRange.ToString (r => ConvertDTO.DateTimeUtcToIsoString (r));
            response.DayRange = slot.DayRange.ToString (r => ConvertDTO.DayToIsoString (r));
            if (slot.Shift is null) {
              response.Display = "";
            }
            else { // slot.Shift is not null
              if (slot.DateTimeRange.ContainsElement (DateTime.UtcNow)) {
                response.Display = slot.Shift.Display;
              }
              else if (slot.Day.HasValue) {
                var format = Lemoine.Info.ConfigSet
                  .LoadAndGet<string> (SHIFT_DAY_FORMAT_KEY, SHIFT_DAY_FORMAT_DEFAULT);
                response.Display = $"{slot.Shift.Display} {slot.Day.Value.ToString (format)}";
              }
              else {
                log.Warn ($"GetWithoutCache: shift {slot.Shift.Id} but no day");
                response.Display = "";
              }
            }
            transaction.Commit ();
            return response;
          }
          else { // No shift slot was found => fallback to day
            log.Info ($"GetWithoutCache: no shift found at {aroundDateTime} => fallback to day");
            rangeType = "day";
          }
        }
        
        var aroundDay = ModelDAOHelper.DAOFactory.DaySlotDAO
          .GetDay (aroundDateTime);
        var rangeAroundDayRequest = new Lemoine.Business.Time.RangeAroundDay (aroundDay);
        rangeAroundDayRequest.RangeSize = rangeSize;
        rangeAroundDayRequest.RangeType = rangeType;
        var rangeAroundDayResponse = Lemoine.Business.ServiceProvider
          .Get (rangeAroundDayRequest);
        
        response.DateTimeRange = rangeAroundDayResponse.DateTimeRange.ToString (r => ConvertDTO.DateTimeUtcToIsoString (r));
        response.DayRange = rangeAroundDayResponse.DayRange.ToString (r => ConvertDTO.DayToIsoString (r));
        response.Display = ""; // Done in .js
        transaction.Commit ();
        return response;
      }
    }
  }
}
