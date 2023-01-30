// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Cache;
using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Extensions.Web.Responses;
using System.Threading.Tasks;
using Lemoine.Web;

namespace Pulse.Web.ProductionState
{
  /// <summary>
  /// Description of ProductionStateSlotsService
  /// </summary>
  public class ProductionStateSlotsService
    : GenericCachedService<ProductionStateSlotsRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ProductionStateSlotsService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public ProductionStateSlotsService ()
      : base (Lemoine.Core.Cache.CacheTimeOut.CurrentShort)
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
    protected override TimeSpan GetCacheTimeOut (string url, ProductionStateSlotsRequestDTO requestDTO)
    {
      UtcDateTimeRange range;
      if (string.IsNullOrEmpty (requestDTO.Range)) {
        range = new UtcDateTimeRange (DateTime.UtcNow, DateTime.UtcNow, "[]");
      }
      else {
        range = new UtcDateTimeRange (requestDTO.Range);
      }

      TimeSpan cacheTimeSpan;
      if (range.IsStrictlyLeftOf (new UtcDateTimeRange (DateTime.UtcNow))) { // Old / Past
        // Previous day => old
        var dayAtRequest = new Lemoine.Business.Time.DayAt (DateTime.UtcNow);
        var daySlot = Lemoine.Business.ServiceProvider.Get (dayAtRequest);
        if (daySlot is null) {
          cacheTimeSpan = CacheTimeOut.CurrentShort.GetTimeSpan ();
          log.Error ($"GetCacheTimeout: no day at {DateTime.UtcNow}, fallback to {cacheTimeSpan}");
        }
        else if (range.IsStrictlyLeftOf (daySlot.DateTimeRange)) { // Old
          cacheTimeSpan = CacheTimeOut.OldShort.GetTimeSpan ();
        }
        else { // Past
          cacheTimeSpan = CacheTimeOut.PastShort.GetTimeSpan ();
        }
      }
      else { // Current or future
        cacheTimeSpan = CacheTimeOut.CurrentShort.GetTimeSpan ();
      }
      if (log.IsDebugEnabled) {
        log.Debug ($"GetCacheTimeOut: cacheTimeSpan is {cacheTimeSpan} for url={url}");
      }
      return cacheTimeSpan;
    }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override async Task<object> Get (ProductionStateSlotsRequestDTO request)
    {
      UtcDateTimeRange range;
      if (string.IsNullOrEmpty (request.Range)) {
        range = new UtcDateTimeRange (DateTime.UtcNow, DateTime.UtcNow, "[]");
      }
      else {
        range = new UtcDateTimeRange (request.Range);
      }


      if (string.IsNullOrEmpty (request.GroupId)) {
        log.Error ($"GetWithoutCache: no groupId in request");
        return new ErrorDTO ("Missing group", ErrorStatus.WrongRequestParameter);
      }
      var groupId = request.GroupId;
      var groupRequest = new Lemoine.Business.Machine.GroupFromId (groupId);
      var group = await Lemoine.Business.ServiceProvider
        .GetAsync (groupRequest);
      if (null == group) {
        if (log.IsErrorEnabled) {
          log.Error ($"GetByGroup: group with id {request.GroupId} is not valid");
        }
        return new ErrorDTO ("Invalid group", ErrorStatus.WrongRequestParameter);
      }
      if (group.SingleMachine) {
        var machine = group.GetMachines ().Single ();
        return await GetByMachineAsync (machine, range);
      }
      else {
        // TODO: ...
        if (log.IsErrorEnabled) {
          log.Error ($"GetByGroup: group with id {request.GroupId} is not supported (multi-machines)");
        }
        return new ErrorDTO ("Not supported group (multi-machines)", ErrorStatus.WrongRequestParameter);
      }
    }

    async Task<object> GetByMachineAsync (IMachine machine, UtcDateTimeRange range)
    {
      if (null == machine) {
        log.Error ($"GetByMachine: unknown machine");
        return new ErrorDTO ("No machine with the specified ID",
                             ErrorStatus.WrongRequestParameter);
      }

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyDeferrableTransaction ("Web.ProductionState.Slots")) {
          var productionStateSlotDao = new Lemoine.Business.ProductionState.ProductionStateSlotDAOExtendDifferent ();
          var slots = await productionStateSlotDao
            .FindOverlapsRangeAsync (machine, range);

          var result = new ProductionStateSlotsResponseDTO ();
          result.Range = range.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
          result.ProductionStateSlots = new List<ProductionStateSlotDTO> ();
          foreach (var slot in slots.Where (s => null != s.ProductionState)) {
            var slotDto = new ProductionStateSlotDTO ();
            slotDto.Range = slot.DateTimeRange.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
            slotDto.Id = slot.ProductionState.Id;
            slotDto.Display = slot.ProductionState.Display;
            slotDto.BgColor = ColorGenerator.GetColor ("ProductionState", slot.ProductionState.Id);
            slotDto.FgColor = ColorGenerator.GetContrastColor (slotDto.BgColor);
            result.ProductionStateSlots.Add (slotDto);
          }

          return result;
        }
      }
    }
    #endregion // Methods
  }
}
