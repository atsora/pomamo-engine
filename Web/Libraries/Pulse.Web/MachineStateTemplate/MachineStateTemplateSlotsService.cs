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
using Lemoine.Web;

namespace Pulse.Web.MachineStateTemplate
{
  /// <summary>
  /// Description of MachineStateTemplateSlotsService
  /// </summary>
  public class MachineStateTemplateSlotsService
    : GenericCachedService<MachineStateTemplateSlotsRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MachineStateTemplateSlotsService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public MachineStateTemplateSlotsService ()
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
    protected override TimeSpan GetCacheTimeOut (string url, MachineStateTemplateSlotsRequestDTO requestDTO)
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
        IDaySlot daySlot = ModelDAOHelper.DAOFactory.DaySlotDAO.FindProcessedAt (DateTime.UtcNow);
        if (range.IsStrictlyLeftOf (daySlot.DateTimeRange)) { // Old
          cacheTimeSpan = CacheTimeOut.OldShort.GetTimeSpan ();
        }
        else { // Past
          cacheTimeSpan = CacheTimeOut.PastShort.GetTimeSpan ();
        }
      }
      else { // Current or future
        cacheTimeSpan = CacheTimeOut.CurrentShort.GetTimeSpan ();
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
    public override object GetWithoutCache(MachineStateTemplateSlotsRequestDTO request)
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
        
        UtcDateTimeRange range;
        if (string.IsNullOrEmpty (request.Range)) {
          range = new UtcDateTimeRange (DateTime.UtcNow, DateTime.UtcNow, "[]");
        }
        else {
          range = new UtcDateTimeRange (request.Range);
        }
        
        var slots = ModelDAOHelper.DAOFactory.MachineStateTemplateSlotDAO
          .FindOverlapsRange (machine, range);
        
        var result = new MachineStateTemplateSlotsResponseDTO ();
        result.Range = range.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
        result.MachineStateTemplateSlots = new List<MachineStateTemplateSlotDTO> ();
        foreach (var slot in slots) {
          var slotDto = new MachineStateTemplateSlotDTO ();
          slotDto.Range = slot.DateTimeRange.ToString (bound => ConvertDTO.DateTimeUtcToIsoString (bound));
          if (null != slot.MachineStateTemplate) {
            slotDto.Id = slot.MachineStateTemplate.Id;
            slotDto.Display = slot.MachineStateTemplate.Display;
            slotDto.BgColor = ColorGenerator.GetColor ("MachineStateTemplate", slot.MachineStateTemplate.Id);
            slotDto.FgColor = ColorGenerator.GetContrastColor (slotDto.BgColor);
            if (slot.MachineStateTemplate.Category.HasValue) {
              slotDto.Category = (int)slot.MachineStateTemplate.Category.Value;
            }
          }
          else { // null == slot.MachineStateTemplate
            slotDto.Id = 0;
            slotDto.Display = PulseCatalog.GetString ("NoMachineStateTemplate");
            slotDto.BgColor = ColorGenerator.GetColor ("MachineStateTemplate", 0);
            slotDto.FgColor = ColorGenerator.GetContrastColor (slotDto.BgColor);
          }
          result.MachineStateTemplateSlots.Add (slotDto);
        }
        
        return result;
      }
    }
    #endregion // Methods
  }
}
