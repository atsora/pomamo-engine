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
  public sealed class OperationDetectionStatus
    : IRequest<DateTime?>
  {
    /// <summary>
    /// Reference machine
    /// </summary>
    IMachine Machine { get; set; }

    readonly ILog log = LogManager.GetLogger (typeof (OperationDetectionStatus).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine">not null</param>
    public OperationDetectionStatus (IMachine machine)
    {
      Debug.Assert (null != machine);

      this.Machine = machine;

      {
        var category = this.GetType ().FullName + "." + machine.Id;
        log = LogManager.GetLogger (category);
      }
    }

    #region IRequest implementation
    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public DateTime? Get ()
    {
      var operationDetectionStatusRequest = new Lemoine.Business.Extension
        .MachineExtensions<IOperationDetectionStatusExtension> (this.Machine,
        (ext, m) => ext.Initialize (m));
      var operationDetectionStatusExtensions = Business.ServiceProvider
        .Get<IEnumerable<IOperationDetectionStatusExtension>> (operationDetectionStatusRequest);
      if (!operationDetectionStatusExtensions.Any ()) {
        log.ErrorFormat ("Get: no operation detection status extension registered");
        return null;
      }
      foreach (var groupByPriority in operationDetectionStatusExtensions.GroupBy (ext => ext.OperationDetectionStatusPriority)
        .OrderBy (g => g.Key)) {
        var operationDetectionDateTimes = groupByPriority
          .Select (ext => GetOperationDetectionDateTime (ext))
          .Where (x => x.HasValue)
          .Select (x => x.Value);
        if (!operationDetectionDateTimes.Any ()) {
          if (log.IsDebugEnabled) {
            log.DebugFormat ("Get: no extension returned a date/time for priority {0}", groupByPriority.Key);
          }
        }
        else {
          var result = operationDetectionDateTimes.Max ();
          if (log.IsInfoEnabled) {
            log.InfoFormat ("Get: return {0} from priority {1}", result, groupByPriority.Key);
          }
          return result;
        }
      }

      if (log.IsWarnEnabled) {
        log.WarnFormat ("Get: No extension returned a date/time");
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

    DateTime? GetOperationDetectionDateTime (IOperationDetectionStatusExtension ext)
    {
      var result = ext.GetOperationDetectionDateTime ();
      return result;
    }

    /// <summary>
    /// <see cref="IRequest{T}"/> implementation
    /// </summary>
    /// <returns></returns>
    public string GetCacheKey ()
    {
      return "Business.Operation.OperationDetectionStatus." + ((IDataWithId)this.Machine).Id;
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
