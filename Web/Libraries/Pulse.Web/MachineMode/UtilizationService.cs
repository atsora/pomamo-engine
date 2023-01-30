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
using Lemoine.Business;
using Lemoine.Web;

namespace Pulse.Web.MachineMode
{
  /// <summary>
  /// Utilization service
  /// 
  /// Return the machine % utilization for the specified day range or date/time range
  /// 
  /// To get the utilization target, use the UtilizationTarget service
  /// </summary>
  public class UtilizationService
    : GenericCachedService<UtilizationRequestDTO>
  {
    static readonly string USE_SUMMARY_KEY = "Web.Utilization.UseSummary";
    static readonly bool USE_SUMMARY_DEFAULT = true;

    static readonly string USE_SUMMARY_MIN_DURATION_KEY = "Web.Utilization.UseSummaryMinDuration";
    static readonly TimeSpan USE_SUMMARY_MIN_DURATION_DEFAULT = TimeSpan.FromDays (7);

    static readonly ILog log = LogManager.GetLogger(typeof (UtilizationService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public UtilizationService ()
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
    protected override TimeSpan GetCacheTimeOut (string url, UtilizationRequestDTO requestDTO)
    {
      UtcDateTimeRange range;
      if (!string.IsNullOrEmpty (requestDTO.Range)) {
        range = ParseRange (requestDTO.Range);
      }
      else {
        range = ServiceProvider.Get (new Lemoine.Business.Time.RangeFromDayRange (ParseDayRange (requestDTO.DayRange)));
      }
      
      TimeSpan cacheTimeSpan;
      if (range.IsStrictlyLeftOf (new UtcDateTimeRange (DateTime.UtcNow))) { // Old / Past
        // Previous day => old
        IDaySlot daySlot = ModelDAOHelper.DAOFactory.DaySlotDAO.FindProcessedAt (DateTime.UtcNow);
        if (range.IsStrictlyLeftOf (daySlot.DateTimeRange)) { // Old
          cacheTimeSpan = CacheTimeOut.OldLong.GetTimeSpan ();
        }
        else { // Past
          cacheTimeSpan = CacheTimeOut.PastLong.GetTimeSpan ();
        }
      }
      else { // Current or future
        cacheTimeSpan = CacheTimeOut.CurrentLong.GetTimeSpan ();
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
    public override object GetWithoutCache(UtilizationRequestDTO request)
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
        
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Web.Utilization"))
        {
          DayRange dayRange = ParseDayRange (request.DayRange);
          
          if (!string.IsNullOrEmpty (request.Range)) {
            UtcDateTimeRange range = ParseRange (request.Range);
            // Check if range can't be converted into a dayRange
            dayRange = ServiceProvider.Get (new Lemoine.Business.Time.DayRangeFromRange (range));
            var rangeFromDays = ServiceProvider.Get (new Lemoine.Business.Time.RangeFromDayRange (dayRange));
            if (!rangeFromDays.Equals (range)) { // Conversion is no ok: consider range
              log.DebugFormat ("GetWithoutCache: " +
                               "consider range {0}",
                               range);
              TimeSpan useSummaryMinDuration = Lemoine.Info.ConfigSet
                .LoadAndGet<TimeSpan> (USE_SUMMARY_MIN_DURATION_KEY,
                                       USE_SUMMARY_MIN_DURATION_DEFAULT);
              if (range.Duration.HasValue && (useSummaryMinDuration <= range.Duration.Value)) {
                log.DebugFormat ("GetWithoutCache: " +
                                 "use summary because duration {0} is longer than {1}",
                                 range.Duration.Value, useSummaryMinDuration);
                return GetByDay (machine, dayRange);
              }
              else {
                return GetByRange (machine, range);
              }
            }
          }
          
          return GetByDay (machine, dayRange);
        }
      }
    }
    
    UtilizationResponseDTO GetByDay (IMachine machine, DayRange dayRange)
    {
      bool useSummary = Lemoine.Info.ConfigSet.LoadAndGet<bool> (USE_SUMMARY_KEY,
                                                                 USE_SUMMARY_DEFAULT);
      if (useSummary) {
        return GetByDayWithSummary (machine, dayRange);
      }
      else {
        return GetByDayWithSlot (machine, dayRange);
      }
    }
    
    UtilizationResponseDTO GetByRange (IMachine machine, UtcDateTimeRange range)
    {
      Debug.Assert (range.Duration.HasValue);
      
      var response = new UtilizationResponseDTO ();
      response.Range = range.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));

      var slots = (new Lemoine.Business.MachineMode.RunningSlotDAO ())
        .FindOverlapsRange (machine, range, false);
      
      IList<IRunningSlot> pending = new List<IRunningSlot> ();
      foreach (var slot in slots) {
        UtcDateTimeRange slotRange = new UtcDateTimeRange (slot.DateTimeRange
                                                           .Intersects (range));
        Debug.Assert (!slotRange.IsEmpty ());
        Debug.Assert (slotRange.Duration.HasValue);
        
        if (slot.Running) {
          response.MotionDuration += (int)slotRange.Duration.Value.TotalSeconds;
        }
        if (slot.NotRunning) {
          response.NotRunningDuration += (int)slotRange.Duration.Value.TotalSeconds;
        }
        response.TotalDuration += (int)slotRange.Duration.Value.TotalSeconds;
      }
      
      return response;
    }
    
    UtilizationResponseDTO GetByDayWithSlot (IMachine machine, DayRange dayRange)
    {
      UtcDateTimeRange range = ServiceProvider.Get (new Lemoine.Business.Time.RangeFromDayRange (dayRange));
      
      var response = GetByRange (machine, range);
      response.DayRange = dayRange.ToString (bound => ConvertDTO.DayToIsoString (bound));
      return response;
    }

    UtilizationResponseDTO GetByDayWithSummary (IMachine machine, DayRange dayRange)
    {
      var response = new UtilizationResponseDTO ();
      response.DayRange = dayRange.ToString (bound => ConvertDTO.DayToIsoString (bound));
      
      RunTotalTime? runTotalTime = ModelDAOHelper.DAOFactory.MachineActivitySummaryDAO
        .GetRunTotalTime (machine, dayRange);
      if (!runTotalTime.HasValue) {
        log.WarnFormat ("GetByDayWithSlot: " +
                        "no RunTotalTime was returned");
        response.MotionDuration = 0;
        response.NotRunningDuration = 0;
        response.TotalDuration = 0;
      }
      else { // runTotalTime.HasValue
        response.MotionDuration = (int)runTotalTime.Value.Run.TotalSeconds;
        response.NotRunningDuration = (int)runTotalTime.Value.NotRunning.TotalSeconds;
        response.TotalDuration = (int)runTotalTime.Value.Total.TotalSeconds;
      }
      
      return response;
    }
    
    DayRange ParseDayRange (string range)
    {
      if (string.IsNullOrEmpty (range)) {
        IDaySlot todaySlot = GetTodaySlot ();
        Debug.Assert (todaySlot.Day.HasValue);
        return new DayRange (todaySlot.Day.Value, todaySlot.Day.Value);
      }
      else {
        return new DayRange (range);
      }
    }
    
    UtcDateTimeRange ParseRange (string range)
    {
      Debug.Assert (!string.IsNullOrEmpty (range));
      if (range.Equals ("CurrentShift")) {
        return GetCurrentShift ();
      }
      else {
        return new UtcDateTimeRange (range);
      }
    }

    /// <summary>
    /// Return today slot
    /// </summary>
    /// <returns></returns>
    IDaySlot GetTodaySlot ()
    {
      var dayAtRequest = new Lemoine.Business.Time.DayAt (DateTime.UtcNow);
      var daySlot = Lemoine.Business.ServiceProvider
        .Get (dayAtRequest);
      return daySlot;
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
        var daySlot = GetTodaySlot ();
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
