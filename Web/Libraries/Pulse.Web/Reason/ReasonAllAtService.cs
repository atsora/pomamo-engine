// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Lemoine.Core.Cache;
using Lemoine.Info;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System.Globalization;
using Pulse.Extensions.Database;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Web;

namespace Pulse.Web.Reason
{
  /// <summary>
  /// Description of ReasonOnlySlotsService
  /// </summary>
  public class ReasonAllAtService
    : GenericCachedService<ReasonAllAtRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ReasonAllAtService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public ReasonAllAtService ()
      : base (Lemoine.Core.Cache.CacheTimeOut.PastShort)
    {
    }
    #endregion // Constructors

    #region Methods
    DateTime ParseDateTime (string s)
    {
      var bound = ConvertDTO.IsoStringToDateTimeUtc (s);
      Debug.Assert (bound.HasValue);
      return bound.Value;
    }

    /// <summary>
    /// Get the cache time out
    /// </summary>
    /// <param name="url"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    protected override TimeSpan GetCacheTimeOut (string url, ReasonAllAtRequestDTO request)
    {
      DateTime at;
      if (string.IsNullOrEmpty (request.At)) {
        at = DateTime.UtcNow;
      }
      else {
        at = ParseDateTime (request.At);
      }

      TimeSpan cacheTimeSpan;
      // Previous day => old
      IDaySlot daySlot = ModelDAOHelper.DAOFactory.DaySlotDAO.FindProcessedAt (DateTime.UtcNow);
      if (Bound.Compare<DateTime> (at, daySlot.DateTimeRange.Lower) < 0) { // Old
        cacheTimeSpan = CacheTimeOut.OldShort.GetTimeSpan ();
      }
      else { // Past
        cacheTimeSpan = CacheTimeOut.PastShort.GetTimeSpan ();
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
    public override object GetWithoutCache (ReasonAllAtRequestDTO request)
    {
      try {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          int machineId = request.MachineId;
          IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (machineId);
          if (null == machine) {
            log.ErrorFormat ("GetWithoutCache: " +
                             "unknown machine with ID {0}",
                             machineId);
            return new ErrorDTO ("No machine with the specified ID",
                                 ErrorStatus.WrongRequestParameter);
          }

          DateTime at;
          if (string.IsNullOrEmpty (request.At)) {
            log.Error ("GetWithoutCache: " +
                       "unknown at parameter");
            return new ErrorDTO ("No date/time specified",
                                 ErrorStatus.WrongRequestParameter);
          }
          else {
            at = ParseDateTime (request.At);
          }

          return Get (machine, at);
        }
      }
      catch (Exception ex) {
        log.Error ("GetWithoutCache: exception", ex);
        throw;
      }
    }

    ReasonAllAtResponseDTO Get (IMonitoredMachine machine, DateTime at)
    {
      var result = new ReasonAllAtResponseDTO ();
      result.At = ConvertDTO.DateTimeUtcToIsoString (at);

      var range = new UtcDateTimeRange (at, at, "[]");
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Web.Reason.AllAt")) {
          var slots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
            .FindOverlapsRange (machine, range);
          Debug.Assert (slots.Count () <= 1);

          if (0 == slots.Count) {
            log.ErrorFormat ("Get: no reason only slot at {0}", at);
            return result;
          }
          else if (1 == slots.Count) {
            var items = GetItemsWithReasonSlot (machine, at, slots[0]);
            result.ReasonAllAtItems = items
              .OrderByDescending (item => item.Score)
              .ToList ();
            return result;
          }
          else { // 1 < slots.Count
            log.FatalFormat ("Get: several slots at range {0}", range);
            throw new Exception ("Several slots at a unique date/time");
          }
        }
      }
    }

    IEnumerable<ReasonAllAtItemDTO> GetItemsWithReasonSlot (IMonitoredMachine machine, DateTime at, IReasonSlot reasonSlot)
    {
      try {
        bool extraReason = false;
        // Check extra auto-reasons
        if (reasonSlot.ReasonSource.HasFlag (ReasonSource.UnsafeAutoReasonNumber)) {
          extraReason = true;
        }
        else {
          if (reasonSlot.ReasonSource.IsAuto ()) {
            extraReason |= (1 < reasonSlot.AutoReasonNumber);
          }
          else {
            extraReason |= (0 < reasonSlot.AutoReasonNumber);
          }
        }
        // Check extra manual reasons
        if (!extraReason && !reasonSlot.ReasonSource.Equals (ReasonSource.Manual)) {
          if (reasonSlot.ReasonSource.HasFlag (ReasonSource.UnsafeManualFlag)) {
            extraReason = true;
          }
          else {
            extraReason |= reasonSlot.ReasonSource.HasFlag (ReasonSource.Manual);
          }
        }

        if (!extraReason) {
          var item = new ReasonAllAtItemDTO (reasonSlot);
          return new List<ReasonAllAtItemDTO> { item };
        }
        else { // extraReason
          IEnumerable<ReasonAllAtItemDTO> result = new List<ReasonAllAtItemDTO> ();
          var possibleReasonExtensions = GetReasonExtensions (machine);
          foreach (var possibleReasonExtension in possibleReasonExtensions) {
            var possibleReasons = possibleReasonExtension.TryGetActiveAt (at, reasonSlot.MachineMode, reasonSlot.MachineObservationState, true);
            var items = possibleReasons
              .Select (r => new ReasonAllAtItemDTO (r));
            result = result
              .Concat (items);
          }
          return result;
        }
      }
      catch (Exception ex) {
        log.Error ("GetItemsWithReasonSlot: exception", ex);
        throw;
      }
    }

    IEnumerable<IReasonExtension> GetReasonExtensions (IMonitoredMachine machine)
    {
      Debug.Assert (null != machine);

      return Lemoine.Business.ServiceProvider
        .Get (new Lemoine.Business.Extension.MonitoredMachineExtensions<IReasonExtension> (machine, (x, m) => x.Initialize (m)));
    }
    #endregion // Methods
  }
}
