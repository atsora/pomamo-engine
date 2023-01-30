// Copyright (C) 2009-2023 Lemoine Automation Technologies
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

namespace Pulse.Web.Operation
{
  /// <summary>
  /// MissingWorkInformation service: returns if there is a missing work information (work order or component) in the specified period
  /// </summary>
  public class MissingWorkInformationService
    : GenericCachedService<MissingWorkInformationRequestDTO>
  {
    static readonly string RECENT_TIMESPAN_KEY = "Web.MissingWorkInformation.Recent";
    static readonly TimeSpan RECENT_TIMESPAN_DEFAULT = TimeSpan.FromMinutes (1);
    
    static readonly string STEP_KEY = "Web.MissingWorkInformation.Step";
    static readonly TimeSpan STEP_DEFAULT = TimeSpan.FromDays (1);
    
    static readonly ILog log = LogManager.GetLogger(typeof (MissingWorkInformationService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public MissingWorkInformationService ()
      : base(Lemoine.Core.Cache.CacheTimeOut.CurrentShort)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache(MissingWorkInformationRequestDTO request)
    {
      MissingWorkInformationResponseDTO response = new MissingWorkInformationResponseDTO ();
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        int machineId = request.MachineId;
        IMachine machine;
        bool operationFromCnc;
        IMonitoredMachine monitoredMachine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (machineId);
        if (null == monitoredMachine) {
          machine = ModelDAOHelper.DAOFactory.MachineDAO
            .FindById (machineId);
          if (null == machine) {
            log.ErrorFormat ("GetWithoutCache: " +
                             "unknown machine with ID {0}",
                             machineId);
            return new ErrorDTO ("No machine with the specified ID",
                                 ErrorStatus.WrongRequestParameter);
          }
          operationFromCnc = false;
        }
        else { // null != monitoredMachine
          machine = monitoredMachine;
          operationFromCnc = monitoredMachine.OperationFromCnc;
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

        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Web.MissingWorkInformation"))
        {
          UtcDateTimeRange applicableRange = range;
          
          if (range.ContainsElement (DateTime.UtcNow)) {
            var effectiveOperationSlots = ModelDAOHelper.DAOFactory.OperationSlotDAO
              .GetEffective (machine, range.Lower);
            foreach (var effectiveOperationSlot in effectiveOperationSlots) {
              if (IsMissingInformation (effectiveOperationSlot, operationFromCnc)
                  && effectiveOperationSlot.DateTimeRange.Overlaps (range)
                  && !IsOperationSlotTooRecent (effectiveOperationSlot)) {
                response.IsMissingWorkInformation = true;
                return response;
              }
            }
            if (effectiveOperationSlots.Any ()) {
              var firstEffectiveOperationSlot = effectiveOperationSlots.First ();
              if (firstEffectiveOperationSlot.DateTimeRange.Lower.HasValue) {
                applicableRange = new UtcDateTimeRange (range.Lower, firstEffectiveOperationSlot.DateTimeRange.Lower.Value);
              }
              else { // Lower = -oo
                log.DebugFormat ("GetWithoutCache: effective operation slots from -oo");
                response.IsMissingWorkInformation = false;
                return response;
              }
            }
            else {
              applicableRange = new UtcDateTimeRange (range.Lower, DateTime.UtcNow);
            }
            
            if (applicableRange.IsEmpty ()) {
              log.DebugFormat ("GetWithoutCache: empty applicable range");
              response.IsMissingWorkInformation = false;
              return response;
            }
          }
          
          var step = Lemoine.Info.ConfigSet.LoadAndGet<TimeSpan> (STEP_KEY,
                                                                  STEP_DEFAULT);
          var operationSlots = ModelDAOHelper.DAOFactory.OperationSlotDAO
            .FindOverlapsRangeDescending (machine,
                                          range,
                                          step);
          bool recent = true;
          foreach (var operationSlot in operationSlots) {
            if (IsMissingInformation (operationSlot, operationFromCnc)) {
              if (recent) {
                recent = IsOperationSlotTooRecent (operationSlot);
              }
              if (!recent) {
                response.IsMissingWorkInformation = true;
                return response;
              }
            }
          }
        } // transaction
      } // session

      response.IsMissingWorkInformation = false;
      return response;
    }
    
    bool IsOperationSlotTooRecent (IOperationSlot operationSlot)
    {
      if (operationSlot.DateTimeRange.Lower.HasValue) {
        var recentTimeSpan = Lemoine.Info.ConfigSet.LoadAndGet<TimeSpan> (RECENT_TIMESPAN_KEY,
                                                                          RECENT_TIMESPAN_DEFAULT);
        return DateTime.UtcNow <= operationSlot.DateTimeRange.Lower.Value.Add (recentTimeSpan);
      }
      else {
        return false;
      }
    }
    
    bool IsMissingInformation (IOperationSlot operationSlot, bool operationFromCnc)
    {
      if (!operationFromCnc
          && (null == operationSlot.Operation)) {
        log.DebugFormat ("IsMissingInformation: the operation must be manually set (not from cnc)");
        return true;
      }
      
      if ((operationSlot.WorkOrder == null)
          || (operationSlot.Component == null)) {
        log.DebugFormat ("IsMissingInformation: Missing work order or component");
        return true;
      }
      
      Debug.Assert((operationSlot.WorkOrder != null) && (operationSlot.Component != null));
      
      return false;
    }
    #endregion // Methods
  }
}
