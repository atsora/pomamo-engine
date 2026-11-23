// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Web;
using Pulse.Web.CommonResponseDTO;

namespace Pulse.Web.Scrap
{
  /// <summary>
  /// Scrap counters of the current shift of a machine
  ///
  /// The current day and shift are the ones of the operation slot at the current time
  /// (or at the operation detection status in case of processing delay).
  /// The counters are the sums of the scrap counters of the operation slots of this day and shift,
  /// which the analysis of the scrap reports updates.
  /// Once a scrap report is analyzed, its machine is cleared from this cache
  /// (domain ScrapReport, see Lemoine.Plugin.Web.DefaultCache)
  /// </summary>
  public class ScrapCurrentShiftService
    : GenericCachedService<ScrapCurrentShiftRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ScrapCurrentShiftService).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public ScrapCurrentShiftService ()
      : base (Lemoine.Core.Cache.CacheTimeOut.CurrentLong)
    {
    }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override async Task<object> Get (ScrapCurrentShiftRequestDTO request)
    {
      try {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          var machine = await ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindByIdAsync (request.MachineId);
          if (machine is null) {
            log.Error ($"Get: unknown machine with ID {request.MachineId}");
            return new ErrorDTO ("No machine with the specified ID",
                                 ErrorStatus.WrongRequestParameter);
          }

          // In case of processing delay, consider the latest time when the operation is known
          var dateTime = DateTime.UtcNow;
          var operationDetectionStatusRequest = new Lemoine.Business.Operation.OperationDetectionStatus (machine);
          var operationDetectionStatus = await Lemoine.Business.ServiceProvider.GetAsync (operationDetectionStatusRequest);
          if (operationDetectionStatus.HasValue) {
            if (operationDetectionStatus.Value < dateTime) {
              dateTime = operationDetectionStatus.Value.AddSeconds (-1);
            }
          }
          else {
            log.Error ($"Get: operation detection status can't be retrieved for machine {machine.Id}");
          }

          var response = new ScrapCurrentShiftResponseDTO {
            DateTime = ConvertDTO.DateTimeUtcToIsoString (dateTime)
          };

          using (var transaction = session.BeginReadOnlyTransaction ("Web.Scrap.ScrapCurrentShift")) {
            var operationSlot = await ModelDAOHelper.DAOFactory.OperationSlotDAO
              .FindAtAsync (machine, dateTime);
            if (operationSlot is null) {
              if (log.IsDebugEnabled) {
                log.Debug ($"Get: no operation slot at {dateTime} for machine {machine.Id} => no scrap");
              }
              return response;
            }
            if (!operationSlot.Day.HasValue) {
              log.Warn ($"Get: the operation slot at {dateTime} of machine {machine.Id} has no day (the operation slots are not split by day ?) => no scrap");
              return response;
            }

            var day = operationSlot.Day.Value;
            var shift = operationSlot.Shift;
            response.Day = ConvertDTO.DayToIsoString (day);
            if (shift is not null) {
              response.Shift = new ShiftDTOAssembler ().Assemble (shift);
            }

            var operationSlots = await ModelDAOHelper.DAOFactory.OperationSlotDAO
              .FindByDayShiftAsync (machine, day, shift);
            response.ReportedCycleCount = operationSlots.Sum (x => x.ReportedCycles);
            response.ReportedCount = operationSlots.Sum (x => x.ReportedParts);
            response.ValidCount = operationSlots.Sum (x => x.ValidParts);
            response.SetupCount = operationSlots.Sum (x => x.SetupParts);
            response.ScrapCount = operationSlots.Sum (x => x.ScrapParts);
            response.FixableCount = operationSlots.Sum (x => x.FixableParts);
            return response;
          }
        }
      }
      catch (Exception ex) {
        log.Error ("Get: exception", ex);
        throw;
      }
    }
  }
}
