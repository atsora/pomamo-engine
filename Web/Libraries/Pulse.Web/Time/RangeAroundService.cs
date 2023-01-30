// Copyright (C) 2009-2023 Lemoine Automation Technologies
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

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public RangeAroundService ()
      : base(Lemoine.Core.Cache.CacheTimeOut.CurrentShort) // Config for Lemoine.Business.RangeAroundDay
    {
    }
    #endregion // Constructors

    #region Methods
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
            response.Display = (null == slot.Shift) ? "" : slot.Shift.Display;
            transaction.Commit ();
            return response;
          }
          else { // No shift slot was found => fallback to day
            log.InfoFormat ("GetWithoutCache: no shift found at {0} " +
                            "=> fallback to day",
                            aroundDateTime);
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
    
    DayRange ConvertRangeType (DateTime day, string rangeType)
    {
      if (rangeType.Equals ("day", StringComparison.CurrentCultureIgnoreCase)) {
        return new DayRange (day, day);
      }
      else if (rangeType.Equals ("week", StringComparison.CurrentCultureIgnoreCase)) {
        DayOfWeek firstDayOfWeek = Lemoine.Info.ConfigSet
          .LoadAndGet<DayOfWeek> (ConfigKeys.GetCalendarConfigKey (CalendarConfigKey.FirstDayOfWeek),
                                  DayOfWeek.Monday);
        DateTime firstDay = day;
        while (!firstDay.DayOfWeek.Equals (firstDayOfWeek)) {
          firstDay.AddDays (-1);
        }
        return new DayRange (firstDay, firstDay.AddDays (6));
      }
      else if (rangeType.Equals ("month", StringComparison.CurrentCultureIgnoreCase)) {
        var monthStart = new DateTime (day.Year, day.Month, 1);
        var monthEnd = monthStart.AddMonths (1).AddDays (-1);
        return new DayRange (monthStart, monthEnd);
      }
      else if (rangeType.Equals ("quarter", StringComparison.CurrentCultureIgnoreCase)) {
        var quarterStart = new DateTime (day.Year, (day.Month-1)%3+1, 1);
        var quarterEnd = quarterStart.AddMonths (3).AddDays (-1);
        return new DayRange (quarterStart, quarterEnd);
      }
      else if (rangeType.Equals ("semester", StringComparison.CurrentCultureIgnoreCase)) {
        var semesterStart = new DateTime (day.Year, (day.Month-1)%6+1, 1);
        var semesterEnd = semesterStart.AddMonths (6).AddDays (-1);
        return new DayRange (semesterStart, semesterEnd);
      }
      else if (rangeType.Equals ("year", StringComparison.CurrentCultureIgnoreCase)) {
        var yearStart = new DateTime (day.Year, 1, 1);
        var yearEnd = yearStart.AddYears (1).AddDays (-1);
        return new DayRange (yearStart, yearEnd);
      }
      else {
        log.FatalFormat ("ConvertRangeType: " +
                         "range type {0} not supported",
                         rangeType);
        throw new NotImplementedException ("Range type " + rangeType);
      }
    }
    
    DateTime AddOffset (Bound<DateTime> day, string rangeType, int offset)
    {
      Debug.Assert (day.HasValue);
      
      if (rangeType.Equals ("day", StringComparison.CurrentCultureIgnoreCase)) {
        return day.Value.AddDays (offset);
      }
      else if (rangeType.Equals ("week", StringComparison.CurrentCultureIgnoreCase)) {
        return day.Value.AddDays (7*offset);
      }
      else if (rangeType.Equals ("month", StringComparison.CurrentCultureIgnoreCase)) {
        return day.Value.AddMonths (offset);
      }
      else if (rangeType.Equals ("quarter", StringComparison.CurrentCultureIgnoreCase)) {
        return day.Value.AddMonths (3*offset);
      }
      else if (rangeType.Equals ("semester", StringComparison.CurrentCultureIgnoreCase)) {
        return day.Value.AddMonths (6*offset);
      }
      else if (rangeType.Equals ("year", StringComparison.CurrentCultureIgnoreCase)) {
        return day.Value.AddYears (offset);
      }
      else {
        log.FatalFormat ("AddOffset: " +
                         "range type {0} not supported",
                         rangeType);
        throw new NotImplementedException ("Range type " + rangeType);
      }
    }
    #endregion // Methods
  }
}
