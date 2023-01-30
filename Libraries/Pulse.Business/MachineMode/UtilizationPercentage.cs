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
  /// Request class to get the refrence utilization percentage of a machine
  /// 
  /// This is not really precise for the moment. It only consider the last three months
  /// </summary>
  public sealed class UtilizationPercentage
    : IRequest<double>
  {
    static readonly string CACHE_TIMEOUT_KEY = "Business.MachineMode.UtilizationPercentage.CacheTimeOut";
    static readonly TimeSpan CACHE_TIMEOUT_DEFAULT = TimeSpan.FromHours (3);

    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (UtilizationPercentage).FullName);

    #region Getters / Setters
    /// <summary>
    /// Machine (not null)
    /// </summary>
    IMachine Machine { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    public UtilizationPercentage (IMachine machine)
    {
      Debug.Assert (null != machine);

      this.Machine = machine;
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns>Utilization percentage (double)</returns>
    public double Get ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Business.MachineMode.UtilizationPercentage")) {
          // For the moment, really approximative
          // In the future consider also the week day and shift for example
          var dayRange = new DayRange (DateTime.Today, new UpperBound<DateTime> (null));
          var summarys = ModelDAOHelper.DAOFactory.MachineActivitySummaryDAO
            .FindInDayRangeWithMachineMode (this.Machine, dayRange);
          var totalSeconds = summarys
            .Sum (s => s.Time.TotalSeconds);
          var runningSeconds = summarys
            .Where (s => s.MachineMode.Running.HasValue && s.MachineMode.Running.Value)
            .Sum (s => s.Time.TotalSeconds);
          if (0 < totalSeconds) {
            return runningSeconds / totalSeconds;
          }
          else {
            log.ErrorFormat ("Get: no data in the last three months, return 100%");
            return 1.0;
          }
        }
      }
    }


    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async Task<double> GetAsync ()
    {
      return await Task.FromResult (Get ());
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return "Business.MachineMode.UtilizationPercentage." + Machine.Id;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/>
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<double> data)
    {
      return true;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (double data)
    {
      return Lemoine.Info.ConfigSet.LoadAndGet<TimeSpan> (CACHE_TIMEOUT_KEY,
        CACHE_TIMEOUT_DEFAULT);
    }
    #endregion // IRequest implementation
  }
}
