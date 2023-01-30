// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using Lemoine.Core.Cache;
using Lemoine.Business;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.Extensions.Database;
using System.Collections.Generic;
using System.Linq;
using Lemoine.ModelDAO;
using System.Threading.Tasks;
using Lemoine.Collections;
using Pulse.Extensions.Database;

namespace Lemoine.Business.Operation
{
  /// <summary>
  /// Request class to get the cycle detection date/time status
  /// </summary>
  public sealed class CycleDetectionStatus
    : IRequest<DateTime?>
  {
    #region Members
    /// <summary>
    /// Reference machine
    /// </summary>
    IMachine Machine { get; set; }
    #endregion // Members

    readonly ILog log = LogManager.GetLogger (typeof (CycleDetectionStatus).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    public CycleDetectionStatus (IMachine machine)
    {
      Debug.Assert (null != machine);

      this.Machine = machine;

      {
        var category = this.GetType ().FullName + "." + machine.Id;
        log = LogManager.GetLogger (category);
      }
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public DateTime? Get ()
    {
      var cycleDetectionStatusRequest = new Lemoine.Business.Extension
   .MachineExtensions<ICycleDetectionStatusExtension> (this.Machine,
   (ext, m) => ext.Initialize (m));
      var cycleDetectionStatusExtensions = Business.ServiceProvider
        .Get<IEnumerable<ICycleDetectionStatusExtension>> (cycleDetectionStatusRequest);
      if (!cycleDetectionStatusExtensions.Any ()) {
        log.ErrorFormat ("Get: no cycle detection status extension registered");
        return null;
      }
      foreach (var groupByPriority in cycleDetectionStatusExtensions.GroupBy (ext => ext.CycleDetectionStatusPriority)
        .OrderBy (g => g.Key)) {
        var cycleDetectionDateTimes = groupByPriority
          .Select (ext => ext.GetCycleDetectionDateTime ())
          .Where (x => x.HasValue)
          .Select (x => x.Value);
        if (!cycleDetectionDateTimes.Any ()) {
          if (log.IsDebugEnabled) {
            log.DebugFormat ("Get: no extension returned a date/time for priority {0}", groupByPriority.Key);
          }
        }
        else {
          var result = cycleDetectionDateTimes.Max ();
          if (log.IsInfoEnabled) {
            log.InfoFormat ("Get: return {0} from priority {1}", result, groupByPriority.Key);
          }
          return result;
        }
      }

      if (log.IsWarnEnabled) {
        log.WarnFormat ("Get: no extension returned a date/time");
      }
      return null;
    }


    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public async Task<DateTime?> GetAsync ()
    {
      return await Task.FromResult (Get ());
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return "Business.Operation.CycleDetectionStatus." + ((IDataWithId)this.Machine).Id;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/>
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool IsCacheValid (CacheValue<DateTime?> data)
    {
      return true;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public TimeSpan GetCacheTimeout (DateTime? data)
    {
      return CacheTimeOut.CurrentShort.GetTimeSpan ();
    }
    #endregion // IRequest implementation
  }
}
