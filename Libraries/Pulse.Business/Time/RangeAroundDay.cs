// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Cache;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System.Threading.Tasks;

namespace Lemoine.Business.Time
{
  /// <summary>
  /// Get a range (date/time and day) around a specified day
  /// </summary>
  public sealed class RangeAroundDay
    : IRequest<RangeAroundDayResponse>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (RangeAroundDay).FullName);

    #region Getters / Setters
    /// <summary>
    /// Reference to the day
    /// </summary>
    public DateTime Day { get; internal set; }
    
    /// <summary>
    /// Range type: day (default) / week / month / quarter / semester / year
    /// </summary>
    public string RangeType { get; set; }
    
    /// <summary>
    /// Range size (default: 1)
    /// </summary>
    public int RangeSize { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="day">not null</param>
    public RangeAroundDay (DateTime day)
    {
      this.Day = day;
      this.RangeType = "day";
      this.RangeSize = 1;
    }
    #endregion // Constructors

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns>RangeAroundDayResponse</returns>
    public RangeAroundDayResponse Get ()
    {
      var response = new RangeAroundDayResponse ();

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ("Business.Time.RangeAroundDay")) // Read-write because of the days
      {
        var dayRange = ConvertRangeType (this.Day, this.RangeType);
        
        if (this.RangeType.Equals ("day", StringComparison.InvariantCultureIgnoreCase)
            && (1 == this.RangeSize)) {
          response.DateTimeRange = ModelDAOHelper.DAOFactory.DaySlotDAO
            .ConvertToUtcDateTimeRange (this.Day);
          response.DayRange = dayRange;
          transaction.Commit ();
          return response;
        }
        
        // First alternatively on the left and on the right
        // else RTD combined view won't work
        int offset = this.RangeSize - 1;
        bool right = true; // Do that on the right too
        DateTime? today = null;
        while (0 < offset) {
          // On the left
          dayRange = new DayRange (AddOffset (dayRange.Lower, this.RangeType, -1), dayRange.Upper);
          --offset;
          // Check today has not been reached yet, else, set right to false
          if (null == today) { // Lazy loading
            today = ModelDAOHelper.DAOFactory.DaySlotDAO
              .GetDay (DateTime.UtcNow);
          }
          Debug.Assert (today.HasValue);
          Debug.Assert (dayRange.Upper.HasValue);
          if (today.Value <= dayRange.Upper.Value) {
            right = false;
          }
          // On the right
          if (right && (0 < offset)) {
            dayRange = new DayRange (dayRange.Lower, AddOffset (dayRange.Upper, this.RangeType, +1));
            --offset;
          }
        }
        
        // dayRange is computed, return the result
        var minDateTime = ModelDAOHelper.DAOFactory.DaySlotDAO
          .GetDayBegin (dayRange.Lower.Value);
        var maxDateTime = ModelDAOHelper.DAOFactory.DaySlotDAO
          .GetDayEnd (dayRange.Upper.Value);
        response.DateTimeRange = new UtcDateTimeRange (minDateTime, maxDateTime);
        response.DayRange = dayRange;
        transaction.Commit ();
        return response;
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async Task<RangeAroundDayResponse> GetAsync ()
    {
      return await Task.FromResult (Get ());
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
          firstDay = firstDay.AddDays (-1);
        }
        return new DayRange (firstDay, firstDay.AddDays (6));
      }
      else if (rangeType.Equals ("month", StringComparison.CurrentCultureIgnoreCase)) {
        var monthStart = new DateTime (day.Year, day.Month, 1);
        var monthEnd = monthStart.AddMonths (1).AddDays (-1);
        return new DayRange (monthStart, monthEnd);
      }
      else if (rangeType.Equals ("quarter", StringComparison.CurrentCultureIgnoreCase)) {
        var quarterStart = new DateTime (day.Year, (day.Month-1)/3*3+1, 1);
        var quarterEnd = quarterStart.AddMonths (3).AddDays (-1);
        return new DayRange (quarterStart, quarterEnd);
      }
      else if (rangeType.Equals ("semester", StringComparison.CurrentCultureIgnoreCase)) {
        var semesterStart = new DateTime (day.Year, (day.Month-1)/6*6+1, 1);
        var semesterEnd = semesterStart.AddMonths (6).AddDays (-1);
        return new DayRange (semesterStart, semesterEnd);
      }
      else if (rangeType.Equals ("year", StringComparison.CurrentCultureIgnoreCase)) {
        var yearStart = new DateTime (day.Year, 1, 1);
        var yearEnd = yearStart.AddYears (1).AddDays (-1);
        return new DayRange (yearStart, yearEnd);
      }
      else {
        log.Fatal ($"ConvertRangeType: range type {rangeType} not supported");
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

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey()
    {
      return "Business.Time.RangeAroundDay." + this.Day + "." + this.RangeType + "." + this.RangeSize;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/>
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<RangeAroundDayResponse> data)
    {
      return true;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (RangeAroundDayResponse data)
    {
      return CacheTimeOut.Config.GetTimeSpan ();
    }
    #endregion // IRequest implementation
  }
  
  /// <summary>
  /// Response to RangeAroundDay request
  /// </summary>
  public class RangeAroundDayResponse
  {
    /// <summary>
    /// Date/time range
    /// </summary>
    public UtcDateTimeRange DateTimeRange { get; set; }
    
    /// <summary>
    /// Day range
    /// </summary>
    public DayRange DayRange { get; set; }
  }
}
