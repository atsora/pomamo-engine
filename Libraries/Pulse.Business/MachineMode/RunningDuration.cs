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
using System.Threading.Tasks;

namespace Lemoine.Business.MachineMode
{
  /// <summary>
  /// Request class to get the running time in a specified range for a given machine
  ///
  /// It is the duration of <see cref="MachiningDuration"/>, without the date/time it was
  /// counted up to
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
      return ServiceProvider
        .Get (new MachiningDuration (this.Machine, this.Range))
        .Duration;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns>Running duration (TimeSpan)</returns>
    public async Task<TimeSpan> GetAsync ()
    {
      var response = await ServiceProvider
        .GetAsync (new MachiningDuration (this.Machine, this.Range));
      return response.Duration;
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
