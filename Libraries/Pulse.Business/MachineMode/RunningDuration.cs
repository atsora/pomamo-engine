// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using Lemoine.Core.Cache;
using Lemoine.ModelDAO;
using Lemoine.Business;
using Lemoine.Model;
using Lemoine.Core.Log;
using System.Linq;
using System.Threading.Tasks;

namespace Lemoine.Business.MachineMode
{
  /// <summary>
  /// Request class to get the running time in a specified range for a given machine
  /// </summary>
  public sealed class RunningDuration
    : IRequest<TimeSpan>
  {
    static readonly string CACHE_TIMEOUT_OLD_KEY = "Business.MachineMode.RunningDuration.CacheTimeOut.Old";
    static readonly TimeSpan CACHE_TIMEOUT_OLD_DEFAULT = TimeSpan.FromHours (3);
    static readonly string CACHE_TIMEOUT_PAST_KEY = "Business.MachineMode.RunningDuration.CacheTimeOut.Past";
    static readonly TimeSpan CACHE_TIMEOUT_PAST_DEFAULT = CacheTimeOut.PastShort.GetTimeSpan ();
    static readonly string CACHE_TIMEOUT_CURRENT_KEY = "Business.MachineMode.RunningDuration.CacheTimeOut.Current";
    static readonly TimeSpan CACHE_TIMEOUT_CURRENT_DEFAULT = CacheTimeOut.CurrentShort.GetTimeSpan ();

    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (RunningDuration).FullName);

    #region Getters / Setters
    /// <summary>
    /// Machine (not null)
    /// </summary>
    IMachine Machine { get; set; }

    /// <summary>
    /// Range (not empty)
    /// </summary>
    UtcDateTimeRange Range { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range">not empty</param>
    public RunningDuration (IMachine machine, UtcDateTimeRange range)
    {
      Debug.Assert (null != machine);
      Debug.Assert (!range.IsEmpty ());

      this.Machine = machine;
      this.Range = range;
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns>Running duration (TimeSpan)</returns>
    public TimeSpan Get ()
    {
      if (this.Range.IsEmpty ()) {
        log.WarnFormat ("Get: specified range is empty => return 0s");
        return TimeSpan.FromSeconds (0);
      }

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginTransaction ("Business.MachineMode.RunningDuration")) { // Read-write because of day processing
          UpperBound<DateTime> upperFullDay;
          { // upperDay
            var upperBound = Bound.GetMinimum<DateTime> (DateTime.UtcNow, this.Range.Upper).Value;
            var upperDaySlot = ModelDAOHelper.DAOFactory.DaySlotDAO.FindProcessedAt (upperBound);
            if (null == upperDaySlot) {
              log.ErrorFormat ("Get: no processed day at {0} => fallback, return an approximative value", upperBound);
              Debug.Assert (false);
              upperFullDay = upperBound.Date;
            }
            else if (!upperDaySlot.Day.HasValue) {
              log.ErrorFormat ("Get: day slot has no associated day => fallback, return an approximative value");
              Debug.Assert (false);
              upperFullDay = upperBound.Date;
            }
            else { // null != daySlot && daySlot.Day.HasValue
              if (!Bound<DateTime>.Equals (upperBound, upperDaySlot.DateTimeRange.Lower)) {
                Debug.Assert (upperDaySlot.DateTimeRange.Lower.HasValue);
                Debug.Assert (upperDaySlot.DateTimeRange.Lower.Value < upperBound);

                var result = TimeSpan.FromSeconds (0);

                if (Bound.Compare<DateTime> (this.Range.Lower, upperDaySlot.DateTimeRange.Lower) < 0) { // Past
                  var pastRange = new UtcDateTimeRange (this.Range.Lower, upperDaySlot.DateTimeRange.Lower.Value);
                  Debug.Assert (!pastRange.IsEmpty ());
                  var pastDuration = ServiceProvider.Get<TimeSpan> (new RunningDuration (this.Machine, pastRange));
                  result = result.Add (pastDuration);
                }

                // Current
                var currentRange = new UtcDateTimeRange (upperDaySlot.DateTimeRange.Intersects (this.Range));
                var reasonSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
                  .FindAllInUtcRangeWithMachineMode (this.Machine, currentRange);
                var runningSeconds = reasonSlots
                  .Where (s => s.Running)
                  .Where (s => s.Duration.HasValue)
                  .Sum (s => s.Duration.Value.TotalSeconds);
                result = result.Add (TimeSpan.FromSeconds (runningSeconds));

                transaction.Commit ();
                return result;
              }
              else {
                upperFullDay = upperDaySlot.Day.Value.AddDays (-1);
              }
            }
          }

          LowerBound<DateTime> lowerFullDay;
          TimeSpan runningDuration = TimeSpan.FromSeconds (0);

          if (this.Range.Lower.HasValue) {
            var lowerDaySlot = ModelDAOHelper.DAOFactory.DaySlotDAO.FindProcessedAt (this.Range.Lower.Value);
            if (null == lowerDaySlot) {
              log.ErrorFormat ("Get: no processed day at {0} => fallback, return an approximative value", this.Range.Lower.Value);
              Debug.Assert (false);
              lowerFullDay = this.Range.Lower.Value.Date;
            }
            else if (!lowerDaySlot.Day.HasValue) {
              log.ErrorFormat ("Get: day slot has no associated day => fallback, return an approximative value");
              Debug.Assert (false);
              lowerFullDay = this.Range.Lower.Value.Date;
            }
            else { // null != daySlot && daySlot.Day.HasValue
              if (!Bound<DateTime>.Equals (this.Range.Lower, lowerDaySlot.DateTimeRange.Lower)) {
                // Compute the period [this.Range.Lower,daySlot.DateTimeRange.Upper)
                Debug.Assert (lowerDaySlot.DateTimeRange.Upper.HasValue);
                Debug.Assert (this.Range.Lower.Value < lowerDaySlot.DateTimeRange.Upper.Value);
                var reasonSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
                  .FindAllInUtcRangeWithMachineMode (Machine, new UtcDateTimeRange (this.Range.Lower.Value, lowerDaySlot.DateTimeRange.Upper.Value));
                var runningSeconds = reasonSlots
                  .Where (s => s.Running)
                  .Where (slot => slot.Duration.HasValue)
                  .Sum (slot => slot.Duration.Value.TotalSeconds);
                runningDuration = runningDuration.Add (TimeSpan.FromSeconds (runningSeconds));
                lowerFullDay = lowerDaySlot.Day.Value.AddDays (1);
              }
              else {
                lowerFullDay = lowerDaySlot.Day.Value;
              }
            }
          }
          else {
            lowerFullDay = new LowerBound<DateTime> (null);
          }

          if (lowerFullDay < upperFullDay) {
            var dayRange = new DayRange (lowerFullDay, upperFullDay);
            var summarys = ModelDAOHelper.DAOFactory.MachineActivitySummaryDAO
              .FindInDayRangeWithMachineMode (this.Machine, dayRange);
            var runningSeconds = summarys
              .Where (s => s.MachineMode.Running.HasValue && s.MachineMode.Running.Value)
              .Sum (s => s.Time.TotalSeconds);
            runningDuration = runningDuration.Add (TimeSpan.FromSeconds (runningSeconds));
          }

          transaction.Commit ();
          return runningDuration;
        }
      }
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async Task<TimeSpan> GetAsync ()
    {
      return await Task.FromResult (Get ());
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return "Business.MachineMode.RunningDuration." + Machine.Id + "." + Range.ToString (dt => dt.ToString ("yyyy-MM-ddTHH:mm:ss"));
    }

    /// <summary>
    /// <see cref="IRequest{T}"/>
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<TimeSpan> data)
    {
      return true;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (TimeSpan data)
    {
      TimeSpan cacheTimeSpan;
      if (Range.IsStrictlyLeftOf (new UtcDateTimeRange (DateTime.UtcNow))) { // Old / Past
        // Previous day => old
        IDaySlot daySlot = ModelDAOHelper.DAOFactory.DaySlotDAO.FindProcessedAt (DateTime.UtcNow);
        if (Range.IsStrictlyLeftOf (daySlot.DateTimeRange)) { // Old
          cacheTimeSpan = Lemoine.Info.ConfigSet.LoadAndGet<TimeSpan> (CACHE_TIMEOUT_OLD_KEY,
            CACHE_TIMEOUT_OLD_DEFAULT);
        }
        else { // Past
          cacheTimeSpan = Lemoine.Info.ConfigSet.LoadAndGet<TimeSpan> (CACHE_TIMEOUT_PAST_KEY,
            CACHE_TIMEOUT_PAST_DEFAULT);
          cacheTimeSpan = CacheTimeOut.PastShort.GetTimeSpan ();
        }
      }
      else { // Current or future
        cacheTimeSpan = Lemoine.Info.ConfigSet.LoadAndGet<TimeSpan> (CACHE_TIMEOUT_CURRENT_KEY,
          CACHE_TIMEOUT_CURRENT_DEFAULT);
      }
      log.DebugFormat ("GetCacheTimeOut: " +
                       "cacheTimeSpan is {0}",
                       cacheTimeSpan);
      return cacheTimeSpan;
    }
    #endregion // IRequest implementation
  }
}
