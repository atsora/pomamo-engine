// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using System.Linq;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Web;

namespace Pulse.Web.Reason
{
  /// <summary>
  /// ReasonUnanswered service: returns if there is an unanswered reason for the specific period
  /// </summary>
  public class ReasonUnansweredService
    : GenericCachedService<ReasonUnansweredRequestDTO>
  {
    static readonly string RECENT_TIMESPAN_KEY = "Web.ReasonUnanswered.Recent";
    static readonly TimeSpan RECENT_TIMESPAN_DEFAULT = TimeSpan.FromMinutes (1);
    
    static readonly string STEP_KEY = "Web.ReasonUnanswered.Step";
    static readonly TimeSpan STEP_DEFAULT = TimeSpan.FromHours (4);
    
    static readonly ILog log = LogManager.GetLogger(typeof (ReasonUnansweredService).FullName);

    /// <summary>
    /// 
    /// </summary>
    public ReasonUnansweredService ()
      : base(Lemoine.Core.Cache.CacheTimeOut.CurrentShort)
    {
    }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache(ReasonUnansweredRequestDTO request)
    {
      ReasonUnansweredResponseDTO response = new ReasonUnansweredResponseDTO ();
      
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
          log.ErrorFormat ("GetWithoutCache: " +
                           "unknown range {0}",
                           request.Range);
          return new ErrorDTO ("No specified range",
                               ErrorStatus.WrongRequestParameter);
        }
        else {
          range = new UtcDateTimeRange (request.Range);
        }
        if (range.IsEmpty ()) {
          log.ErrorFormat ("GetWithoutCache: " +
                           "empty range");
          return new ErrorDTO ("Empty range",
                               ErrorStatus.WrongRequestParameter);
        }

        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Web.ReasonUnanswered"))
        {
          if (range.ContainsElement (DateTime.UtcNow)) {
            var lastReasonSlot = ModelDAOHelper.DAOFactory.ReasonSlotDAO.GetLast (machine);
            if ( (null != lastReasonSlot)
                && lastReasonSlot.OverwriteRequired
                && lastReasonSlot.DateTimeRange.Overlaps (range)
                && !IsReasonSlotTooRecent (lastReasonSlot)) {
              response.IsUnansweredPeriod = true;
              return response;
            }
          }
          
          var step = Lemoine.Info.ConfigSet.LoadAndGet<TimeSpan> (STEP_KEY,
                                                                  STEP_DEFAULT);
          var reasonSlots = ModelDAOHelper.DAOFactory.ReasonSlotDAO
            .FindOverlapsRangeDescending (machine,
                                          range,
                                          step);
          bool recent = true;
          foreach (var reasonSlot in reasonSlots) {
            if (reasonSlot.OverwriteRequired) {
              if (recent) {
                recent = IsReasonSlotTooRecent (reasonSlot);
              }
              if (!recent) {
                response.IsUnansweredPeriod = true;
                return response;
              }
            }
          }
        } // transaction        
      } // session

      response.IsUnansweredPeriod = false;
      return response;
    }
    
    bool IsReasonSlotTooRecent (IReasonSlot reasonSlot)
    {
      if (reasonSlot.DateTimeRange.Lower.HasValue) {
        var recentTimeSpan = Lemoine.Info.ConfigSet.LoadAndGet<TimeSpan> (RECENT_TIMESPAN_KEY,
                                                                          RECENT_TIMESPAN_DEFAULT);
        return DateTime.UtcNow <= reasonSlot.DateTimeRange.Lower.Value.Add (recentTimeSpan);
      }
      else {
        return false;
      }
    }
  }
}
