// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.DTO;


using Lemoine.Core.Log;
using System.Linq;

namespace Lemoine.WebService
{
  /// <summary>
  /// GetLastMachineStatus service
  /// </summary>
  public class GetLastMachineStatusV2Service: GenericCachedService<Lemoine.DTO.GetLastMachineStatusV2>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (GetLastMachineStatusV2Service).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public GetLastMachineStatusV2Service () : base(Lemoine.Core.Cache.CacheTimeOut.CurrentShort)
    {
    }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache (Lemoine.DTO.GetLastMachineStatusV2 request)
    {
      int machineId = request.Id;
      DateTime dateOfRequest = DateTime.UtcNow;
      
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById(machineId);
        if (machine == null) {
          return ServiceHelper.NoMonitoredMachineWithIdErrorDTO(machineId);
        }
        
        IReasonSlot reasonSlot = ModelDAOHelper.DAOFactory.ReasonSlotDAO.GetLast(machine);
        if (reasonSlot == null) {
          return ServiceHelper.NoReasonSlotErrorDTO(machineId);
        }
        
        DateTime? datetime = Lemoine.DTO.ConvertDTO.IsoStringToDateTimeUtc(request.Begin);
        DateTime beginDate =
          datetime.HasValue ? datetime.Value :
          dateOfRequest.Subtract(ServiceHelper.GetCurrentPeriodDuration(machine));
        
        var reasonSlots =
          ModelDAOHelper.DAOFactory.ReasonSlotDAO
          .FindAllInUtcRangeWithReasonMachineObservationStateMachineMode (machine,
                                                                          new UtcDateTimeRange (beginDate));
        var reasonRequired = reasonSlots.Any (x => x.OverwriteRequired);
        var reasonRequiredNumber = reasonSlots.Count (x => x.OverwriteRequired);
        
        Lemoine.DTO.LastMachineExtendedStatusV2DTO lastMachineExtendedStatusV2DTO =
          (new Lemoine.DTO.LastMachineExtendedStatusV2DTOAssembler()).Assemble(new Tuple<IReasonSlot,bool, int>(reasonSlot, reasonRequired, reasonRequiredNumber));
        
        // last reason slot is set too old if more than 1 minute old
        if ( (reasonSlot.EndDateTime.HasValue)
            && (reasonSlot.EndDateTime.Value.AddMinutes(1) < dateOfRequest)) {
          ICurrentMachineMode currentMachineMode = ModelDAOHelper.DAOFactory.CurrentMachineModeDAO.Find(machine);
          // If currentMachineMode is tool old, return ReasonTooOld
          if (null == currentMachineMode) {
            lastMachineExtendedStatusV2DTO.ReasonTooOld = true;
            return lastMachineExtendedStatusV2DTO;
          }
          else { // null != currentMachineMode
            if (currentMachineMode.DateTime.AddMinutes (1) < dateOfRequest) {
              lastMachineExtendedStatusV2DTO.ReasonTooOld = true;
              return lastMachineExtendedStatusV2DTO;
            }
            else {
              UtcDateTimeRange range = new UtcDateTimeRange (currentMachineMode.Change, currentMachineMode.DateTime);
              // TODO: this is not 100% exact. It should be new UtcDateTimeRange (, , "[]")
              // And then the observation state slot dao should consider the inclusivity (see below)
              // But the whole method will be written much better after a rework
              if (range.IsEmpty ()) {
                return lastMachineExtendedStatusV2DTO;
              }
              IObservationStateSlot observationSlot = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
                .GetLastIntersectWithRange(machine, currentMachineMode.Change, currentMachineMode.DateTime);
              if (null == observationSlot) {
                log.ErrorFormat ("GetWithoutCache: no observation state slot in range {0}-{1} for machine id {2}",
                  currentMachineMode.Change, currentMachineMode.DateTime, machine.Id);
              }
              else { // null != observationStateSlot
                range = new UtcDateTimeRange (range.Intersects (observationSlot.DateTimeRange));
              }
              TimeSpan? duration = range.Duration;
              
              IMachineModeDefaultReason defaultReason;
              if (duration.HasValue && (null != observationSlot) && (null != observationSlot.MachineObservationState)) {
                defaultReason = ModelDAOHelper.DAOFactory.MachineModeDefaultReasonDAO.FindWith(machine, currentMachineMode.MachineMode,
                                                                                               observationSlot.MachineObservationState,
                                                                                               duration.Value);
              }
              else {
                defaultReason = null;
              }
              if (null != defaultReason) {
                log.Info ("GetWithoutCache: compute live the default reason from the current machine mode");
                lastMachineExtendedStatusV2DTO.MachineStatus.MachineMode = (new MachineModeDTOAssembler()).Assemble(defaultReason.MachineMode);
                lastMachineExtendedStatusV2DTO.MachineStatus.MachineObservationState = (new MachineObservationStateDTOAssembler()).Assemble(defaultReason.MachineObservationState);
                lastMachineExtendedStatusV2DTO.MachineStatus.ReasonSlot.Begin = ConvertDTO.DateTimeUtcToIsoString(range.Lower);
                lastMachineExtendedStatusV2DTO.MachineStatus.ReasonSlot.End = ConvertDTO.DateTimeUtcToIsoString(range.Upper);
                lastMachineExtendedStatusV2DTO.MachineStatus.ReasonSlot.Reason = (new ReasonDTOAssembler()).Assemble(defaultReason.Reason);
                lastMachineExtendedStatusV2DTO.MachineStatus.ReasonSlot.OverwriteRequired = defaultReason.OverwriteRequired;
                
                lastMachineExtendedStatusV2DTO.ReasonTooOld = false;
              }
              else { // null == defaultReason
                lastMachineExtendedStatusV2DTO.ReasonTooOld = true;
              }
            }
          }
        }
        else {
          lastMachineExtendedStatusV2DTO.ReasonTooOld = false;
        }
        
        return lastMachineExtendedStatusV2DTO;
      }
      
      
    }
  }
}
