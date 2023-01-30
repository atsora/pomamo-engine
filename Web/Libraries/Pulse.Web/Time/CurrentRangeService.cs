// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Responses;
using System.Globalization;
using Lemoine.Web;

namespace Pulse.Web.Time
{
  /// <summary>
  /// Current Day service
  /// </summary>
  public class CurrentRangeService
    : GenericCachedService<CurrentRangeRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CurrentRangeService).FullName);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public CurrentRangeService ()
      : base (Lemoine.Core.Cache.CacheTimeOut.CurrentShort)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <returns></returns>
    public override object GetWithoutCache (CurrentRangeRequestDTO request)
    {
      // Time of reference, by default it is "now"
      var utcNow = DateTime.UtcNow;
      if (!string.IsNullOrEmpty (request.CurrentDate)) {
        if (DateTime.TryParse (request.CurrentDate, CultureInfo.InvariantCulture,
                                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out utcNow)) {
          // Find the beginning of the current day
          IDaySlot daySlot;
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
          using (IDAOTransaction transaction = session.BeginTransaction ("Web.Time.DayToDateTimeRangeService")) // Read-write because of the days
          {
            daySlot = ModelDAOHelper.DAOFactory.DaySlotDAO.FindProcessedByDay (utcNow);
            transaction.Commit ();
          }
          if (daySlot != null && daySlot.BeginDateTime.HasValue) {
            utcNow = daySlot.BeginDateTime.Value;
          }
        } else {
          log.ErrorFormat ("Couldn't convert {0} into a date", request.CurrentDate);
        }
      }

      // Split arguments
      var durationDefinition = request.RangeDuration.Split (new char[] { '_', ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
      if (2 != durationDefinition.Length) {
        log.ErrorFormat ("CurrentRangeService: the range duration definition {0} does not contain two elements",
                         request.RangeDuration);
        return new ErrorDTO ("The range duration definition does not contain two elements", ErrorStatus.WrongRequestParameter);
      }

      // Process unit
      var durationUnit = durationDefinition[1];
      if (durationUnit.EndsWith ("s", StringComparison.InvariantCultureIgnoreCase)) {
        durationUnit = durationUnit.Substring (0, durationUnit.Length - 1);
      }

      // Parse the duration number
      int durationNumber;
      if (!int.TryParse (durationDefinition[0], out durationNumber)) {
        log.ErrorFormat ("CurrentRangeService: the range duration definition {0} does not start with a number",
                         request.RangeDuration);
        return new ErrorDTO ("The range definition does not start with a number", ErrorStatus.WrongRequestParameter);
      }

      var response = new CurrentRangeResponseDTO ();
      if (durationUnit.ToLowerInvariant () == "hour") {
        // Compute hours
        var upperLimit = new DateTime (utcNow.Year, utcNow.Month, utcNow.Day, utcNow.Hour, 0, 0, DateTimeKind.Utc);
        upperLimit = upperLimit.Add (TimeSpan.FromHours (1));
        var lowerLimit = upperLimit.Subtract (TimeSpan.FromHours (durationNumber));

        response.UtcDateTimeRange = new UtcDateTimeRange (lowerLimit, upperLimit)
          .ToString (dt => ConvertDTO.DateTimeUtcToIsoString (dt));
        response.LocalDateTimeRange = new UtcDateTimeRange (lowerLimit, upperLimit)
          .ToLocalTime ().ToString (dt => ConvertDTO.DateTimeLocalToIsoString (dt));
        response.DayRange = ""; // not applicable
      } else if (durationUnit.ToLowerInvariant () == "shift") {
        // Compute shifts
        try {
          // Last possible shiftslot possibly comprises the current date and must have a shift defined
          IShiftSlot lastShift, mostRecentShift;
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
            using (IDAOTransaction transaction = session.BeginTransaction ("Web.Time.CurrentRangeService")) { // Read-write because of the slots
              mostRecentShift = ModelDAOHelper.DAOFactory.ShiftSlotDAO.GetFirstSlotBeginningBefore (utcNow);
              while (mostRecentShift.Shift == null) {
                mostRecentShift = ModelDAOHelper.DAOFactory.ShiftSlotDAO.GetFirstSlotBeginningBefore (mostRecentShift.BeginDateTime.Value.AddSeconds (-1));
              }
              durationNumber--;

              // Go in the past
              lastShift = mostRecentShift;
              while (durationNumber > 0) {
                lastShift = ModelDAOHelper.DAOFactory.ShiftSlotDAO.GetFirstSlotBeginningBefore (lastShift.BeginDateTime.Value.AddSeconds (-1));
                while (lastShift.Shift == null) {
                  lastShift = ModelDAOHelper.DAOFactory.ShiftSlotDAO.GetFirstSlotBeginningBefore (lastShift.BeginDateTime.Value.AddSeconds (-1));
                }
                durationNumber--;
              }

              transaction.Commit ();
            }
          }

          // Create the range
          response.UtcDateTimeRange = new UtcDateTimeRange (lastShift.BeginDateTime.Value, mostRecentShift.EndDateTime.Value)
            .ToString (dt => ConvertDTO.DateTimeUtcToIsoString (dt));
          response.LocalDateTimeRange = new UtcDateTimeRange (lastShift.BeginDateTime.Value, mostRecentShift.EndDateTime.Value)
            .ToLocalTime ().ToString (dt => ConvertDTO.DateTimeLocalToIsoString (dt));
          response.DayRange = ""; // not applicable
        }
        catch (Exception e) {
          return new ErrorDTO (String.Format ("Cannot compute the shift range: '{0}'", e.Message), ErrorStatus.UnexpectedError);
        }
      } else {
        IDaySlot lowerDaySlot, upperDaySlot;
        DateTime lowerDay, upperDay;
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
          using (IDAOTransaction transaction = session.BeginTransaction ("Web.Time.CurrentRangeService")) { // Read-write because of the days
          IDaySlot currentDaySlot = ModelDAOHelper.DAOFactory.DaySlotDAO.FindProcessedAt (utcNow);
          if (!currentDaySlot.Day.HasValue) {
            log.Error ("CurrentRangeService: no current day could be determined");
            transaction.Commit ();
            return new ErrorDTO ("no current day was determined", ErrorStatus.UnexpectedError);
          }
          Debug.Assert (currentDaySlot.Day.HasValue);
          var currentDay = currentDaySlot.Day.Value;

          switch (durationUnit.ToLowerInvariant ()) {
          case "day": {
            var timeSpan = TimeSpan.FromDays (durationNumber - 1);
            lowerDay = currentDay.Subtract (timeSpan);
            upperDay = currentDay;
          }
          break;
          case "week": {
            DayOfWeek firstDayOfWeek = Lemoine.Info.ConfigSet
              .LoadAndGet<DayOfWeek> (ConfigKeys.GetCalendarConfigKey (CalendarConfigKey.FirstDayOfWeek),
                                      DayOfWeek.Monday);
            DayOfWeek lastDayOfWeek = (DayOfWeek)(((int)firstDayOfWeek + 6) % 7);
            var offset = ((int)lastDayOfWeek - (int)currentDay.DayOfWeek) % 7;
            if (offset < 0) {
              offset += 7;
            }
            upperDay = currentDay.AddDays (offset);
            lowerDay = upperDay.AddDays (-6 - 7 * (durationNumber - 1));
          }
          break;
          case "month": {
            lowerDay = new DateTime (currentDay.Year, currentDay.Month, 1);
            upperDay = lowerDay.AddMonths (1).AddDays (-1);
            lowerDay = lowerDay.AddMonths (-(durationNumber - 1));
          }
          break;
          case "quarter": {
            lowerDay = new DateTime (currentDay.Year, 3 * ((currentDay.Month - 1) / 3) + 1, 1);
            upperDay = lowerDay.AddMonths (3).AddDays (-1);
            lowerDay = lowerDay.AddMonths (-3 * (durationNumber - 1));
          }
          break;
          case "semester": {
            lowerDay = new DateTime (currentDay.Year, 6 * ((currentDay.Month - 1) / 6) + 1, 1);
            upperDay = lowerDay.AddMonths (6).AddDays (-1);
            lowerDay = lowerDay.AddMonths (-6 * (durationNumber - 1));
          }
          break;
          case "year": {
            lowerDay = new DateTime (currentDay.Year, 1, 1);
            upperDay = lowerDay.AddYears (1).AddDays (-1);
            lowerDay = lowerDay.AddYears (-(durationNumber - 1));
          }
          break;
          default:
            log.ErrorFormat ("CurrentRangeService: duration unit {0} is not valid",
                             durationUnit);
            transaction.Commit ();
            return new ErrorDTO ("The range duration unit is not valid", ErrorStatus.WrongRequestParameter);
          }

          // Adapt limits
          lowerDaySlot = currentDay.Equals (lowerDay) ? currentDaySlot :
            ModelDAOHelper.DAOFactory.DaySlotDAO.FindProcessedByDay (lowerDay);
          upperDaySlot = currentDay.Equals (upperDay) ? currentDaySlot :
            upperDaySlot = ModelDAOHelper.DAOFactory.DaySlotDAO.FindProcessedByDay (upperDay);

          transaction.Commit ();
        }

        response.UtcDateTimeRange = new UtcDateTimeRange (lowerDaySlot.DateTimeRange.Lower, upperDaySlot.DateTimeRange.Upper)
          .ToString (dt => ConvertDTO.DateTimeUtcToIsoString (dt));
        response.LocalDateTimeRange = new UtcDateTimeRange (lowerDaySlot.DateTimeRange.Lower, upperDaySlot.DateTimeRange.Upper)
          .ToLocalTime ().ToString (dt => ConvertDTO.DateTimeLocalToIsoString (dt));
        response.DayRange = new DayRange (lowerDay, upperDay)
          .ToString (day => ConvertDTO.DayToIsoString (day));
      }

      return response;
    }
    #endregion // Methods
  }
}
