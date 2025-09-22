// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.DTO;

using Lemoine.Core.Log;

namespace Lemoine.WebService
{
  /// <summary>
  /// GetRangeAround service
  /// 
  /// Deprecated: use Lemoine.Web.Time.RangeAroundService instead
  /// </summary>
  public class GetRangeAroundService : GenericCachedService<Lemoine.DTO.GetRangeAround>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (GetRangeAroundService).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public GetRangeAroundService () : base (Lemoine.Core.Cache.CacheTimeOut.CurrentShort) // Config for Lemoine.Business.Time.RangeAroundDay
    {
    }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache (Lemoine.DTO.GetRangeAround request)
    {
      Lemoine.DTO.RangeDTO rangeOutput = new Lemoine.DTO.RangeDTO ();

      string rangeType = request.RangeType;
      int rangeSize = request.RangeSize.HasValue ? request.RangeSize.Value : 1;
      DateTime? datetime = Lemoine.DTO.ConvertDTO.IsoStringToDateTimeUtc (request.Around);
      DateTime aroundDateTime = datetime.HasValue ? datetime.Value : DateTime.UtcNow;

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ("WebService.GetRangeAroundService")) {
        if ("shift" == rangeType) { // Old GetShiftAroundService - ignore Range Size
          IShiftSlot slot = ModelDAOHelper.DAOFactory.ShiftSlotDAO.FindAt (aroundDateTime);
          if (null != slot) { // SHIFT DEFINED
            rangeOutput.DayRange.Begin = Lemoine.DTO.ConvertDTO.DayToString (
              slot.BeginDay.HasValue ? slot.BeginDay.Value : (DateTime?)null);
            rangeOutput.DayRange.End = Lemoine.DTO.ConvertDTO.DayToString (
              slot.EndDay.HasValue ? slot.EndDay.Value : (DateTime?)null);
            rangeOutput.DateTimeRange.Begin = Lemoine.DTO.ConvertDTO.DateTimeUtcToIsoString (slot.BeginDateTime);
            rangeOutput.DateTimeRange.End = Lemoine.DTO.ConvertDTO.DateTimeUtcToIsoString (slot.EndDateTime);
            rangeOutput.RangeDisplay = (slot.Shift == null) ? "" : slot.Shift.Display;
            transaction.Commit ();
            return rangeOutput;
          }
          else {
            // NO SLOT found, use day
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

        rangeOutput.DayRange.Begin = Lemoine.DTO.ConvertDTO.DayToString (rangeAroundDayResponse.DayRange.Lower.Value);
        rangeOutput.DayRange.End = Lemoine.DTO.ConvertDTO.DayToString (rangeAroundDayResponse.DayRange.Upper.Value);
        rangeOutput.DateTimeRange.Begin = Lemoine.DTO.ConvertDTO.DateTimeUtcToIsoString (rangeAroundDayResponse.DateTimeRange.Lower.Value);
        rangeOutput.DateTimeRange.End = Lemoine.DTO.ConvertDTO.DateTimeUtcToIsoString (rangeAroundDayResponse.DateTimeRange.Upper.Value);
        rangeOutput.RangeDisplay = ""; // Done in .js
        transaction.Commit ();
        return rangeOutput;
      }
    }
  }
}
