// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Collections;
using Lemoine.Web;

namespace Pulse.Web.Operation
{
  /// <summary>
  /// Return the last identified sequence
  /// </summary>
  public class CurrentSequenceService
    : GenericCachedService<CurrentSequenceRequestDTO>
  {
    static readonly string CURRENT_MARGIN_KEY = "Web.Operation.CurrentSequence.CurrentMargin";
    static readonly TimeSpan CURRENT_MARGIN_DEFAULT = TimeSpan.FromMinutes (1);

    static readonly string CURRENT_MARGIN_MACHINING_KEY = "Web.Operation.CurrentSequence.CurrentMarginMachining";
    static readonly TimeSpan CURRENT_MARGIN_MACHINING_DEFAULT = TimeSpan.FromMinutes (5);

    static ILog log = LogManager.GetLogger (typeof (CurrentSequenceService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public CurrentSequenceService ()
      : base (Lemoine.Core.Cache.CacheTimeOut.CurrentShort)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache (CurrentSequenceRequestDTO request)
    {
      log = LogManager.GetLogger ($"{this.GetType ().FullName}.{request.MachineId}");

      var now = DateTime.UtcNow;
      var response = new CurrentSequenceResponseDTO (now);

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        int machineId = request.MachineId;
        IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindByIdWithMachineModules (machineId);
        if (null == machine) {
          log.Error ($"GetWithoutCache: unknown monitored machine with ID {machineId}");
          return new ErrorDTO ("No monitored machine with the specified ID",
                               ErrorStatus.WrongRequestParameter);
        }

        // Check if the detection acquisition data is not too old
        var operationDetectionStatus = Lemoine.Business.ServiceProvider
          .Get (new Lemoine.Business.Operation.OperationDetectionStatus (machine));
        if (!operationDetectionStatus.HasValue) {
          if (log.IsWarnEnabled) {
            log.Warn ($"GetWithoutCache: no operation detection status for machine id {machine.Id}");
          }
        }
        else {
          response.OperationDetectionStatus = ConvertDTO.DateTimeUtcToIsoString (operationDetectionStatus.Value);
          var currentMargin = Lemoine.Info.ConfigSet
            .LoadAndGet (CURRENT_MARGIN_KEY, CURRENT_MARGIN_DEFAULT);
          response.TooOld = operationDetectionStatus.Value.Add (currentMargin) < now;
        }

        foreach (var machineModule in machine.MachineModules) {
          AddMachineModuleData (now, operationDetectionStatus, response, machineModule);
        }
      }

      return response;
    }

    void AddMachineModuleData (DateTime at, DateTime? operationDetectionStatus, CurrentSequenceResponseDTO responseDto, IMachineModule machineModule)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Web.Operation.CurrentSequence.ByMachineModule")) {
          var sequenceSlot = ModelDAOHelper.DAOFactory.SequenceSlotDAO
            .FindLast (machineModule);
          if (null == sequenceSlot) {
            if (log.IsDebugEnabled) {
              log.Debug ($"AddMachineModuleData: no sequence slot for machine module id {machineModule.Id}");
            }
            return;
          }

          if (sequenceSlot.DateTimeRange.ContainsElement (at)) {
            var byMachineModuleDto = new CurrentSequenceByMachineModuleDTO (machineModule, sequenceSlot.DateTimeRange);
            if (null != sequenceSlot.Sequence) {
              if (log.IsDebugEnabled) {
                log.Debug ($"AddMachineModuleData: {sequenceSlot.DateTimeRange} contains {at}, return sequence id {((IDataWithId<int>)sequenceSlot.Sequence).Id}");
              }
              byMachineModuleDto.Sequence = new SequenceDTO (sequenceSlot.Sequence);
            }
            else {
              if (log.IsDebugEnabled) {
                log.Debug ($"AddMachineModuleData: {sequenceSlot.DateTimeRange} contains {at}, return a null sequence");
              }
            }
            responseDto.ByMachineModule.Add (byMachineModuleDto);
            return;
          }
          else if (operationDetectionStatus.HasValue
            && (Bound.Compare<DateTime> (sequenceSlot.DateTimeRange.Upper, operationDetectionStatus.Value) < 0)) {
            Debug.Assert (sequenceSlot.DateTimeRange.Upper.HasValue);
            var range = new UtcDateTimeRange (sequenceSlot.DateTimeRange.Upper.Value, operationDetectionStatus.Value);
            if (log.IsDebugEnabled) {
              log.Debug ($"AddMachineModuleData: no sequence in between last sequence slot upper time and operation detection status, range is {range}");
            }
            var byMachineModuleDto = new CurrentSequenceByMachineModuleDTO (machineModule, range);
            responseDto.ByMachineModule.Add (byMachineModuleDto);
            return;
          }
          else { // no operationDetectionStatus or sequenceSlot.DateTimeRange.Upper after (not strictly) operationDetectionStatus
            var range = sequenceSlot.DateTimeRange;
            var sequence = sequenceSlot.Sequence;
            bool? byMachineModuleTooOld = null;
            if (operationDetectionStatus.HasValue
              && Bound.Compare<DateTime> (range.Lower, operationDetectionStatus.Value) < 0) { // Only keep the sequence slots after operationDetectionStatus, where operationDetectionStatus can be considered. The other case is possible for example for multi machine module machines
              Debug.Assert (Bound.Compare<DateTime> (operationDetectionStatus.Value, sequenceSlot.DateTimeRange.Upper) <= 0); // See above
              var correctedRange = range
                .Intersects (new UtcDateTimeRange (new LowerBound<DateTime> (null), operationDetectionStatus.Value));
              if (log.IsDebugEnabled) {
                log.Debug ($"AddMachineModuleData: corrected range is {correctedRange} VS initial range {range}, operationDetectionStatus={operationDetectionStatus}");
              }
              range = new UtcDateTimeRange (correctedRange);
              Debug.Assert (!range.IsEmpty ());
              if (range.IsEmpty ()) {
                log.FatalFormat ("AddMachineModuleData: corrected range is empty, which is unexpected");
              }
            }
            else { // OperationDetectionStatus unknown (or it can't be considered since before the sequenceSlot). Keep only the machining sequences (else they should have been managed in the case above, where the range contains at)
              if ((sequence is null) || !sequence.Kind.Equals (SequenceKind.Machining)) {
                // In the past and not a machining sequence => invalid the sequence
                if (log.IsDebugEnabled) {
                  log.Debug ($"AddMachineModuleData: range={range}, kind={sequence?.Kind}, in the past and not machining => invalid the sequence");
                }
                sequence = null;
                byMachineModuleTooOld = true;
                if (range.Upper.HasValue) {
                  range = new UtcDateTimeRange (range.Upper.Value, range.Upper.Value.AddSeconds (1));
                  if (log.IsDebugEnabled) {
                    log.Debug ($"AddMachineModuleData: invalid a non machining sequence in the past, use range={range}");
                  }
                }
                else { // !range.Upper.HasValue
                  log.Error ($"AddMachineModuleData: range={range} has no upper value, it should then contain at={at}");
                }
              }
              else { // Machining sequence
                var currentMargin = Lemoine.Info.ConfigSet
                   .LoadAndGet (CURRENT_MARGIN_MACHINING_KEY, CURRENT_MARGIN_MACHINING_DEFAULT);
                if (Bound.Compare<DateTime> (range.Upper, at.Subtract (currentMargin)) < 0) {
                  // Too old...
                  byMachineModuleTooOld = true;
                }
              }
            }
            var byMachineModuleDto = new CurrentSequenceByMachineModuleDTO (machineModule, range);
            if (null != sequence) {
              byMachineModuleDto.Sequence = new SequenceDTO (sequence);
            }
            byMachineModuleDto.TooOld = byMachineModuleTooOld;
            responseDto.ByMachineModule.Add (byMachineModuleDto);
            return;
          }
        }
      }
    }
    #endregion // Methods
  }
}
