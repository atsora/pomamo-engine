// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Business.Operation;
using Lemoine.Core.Cache;
using Lemoine.Info;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Web.CommonRequestDTO;
using Lemoine.Extensions.Web.Responses;
using System.Globalization;
using Lemoine.Collections;
using Lemoine.Web;

namespace Pulse.Web.Operation
{
  /// <summary>
  /// /Operation/Import service
  /// </summary>
  public class ImportService
    : GenericSaveService<ImportRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ImportService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public ImportService ()
      : base ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetSync (ImportRequestDTO request)
    {
      if (string.IsNullOrEmpty (request.DateTime)) {
        log.ErrorFormat ("Get: unknown date/time");
        return new ErrorDTO ("unknown date/time",
          ErrorStatus.WrongRequestParameter);
      }
      DateTime dateTime;
      IFormatProvider provider = CultureInfo.InvariantCulture;
      if (!DateTime.TryParse (request.DateTime, provider,
                              DateTimeStyles.AssumeUniversal
                              | DateTimeStyles.AdjustToUniversal,
                              out dateTime)) {
        log.ErrorFormat ("Get: invalid date/time {0}",
          request.DateTime);
        return new ErrorDTO ("invalid date/time", ErrorStatus.WrongRequestParameter);
      }

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        int machineId = request.MachineId;
        var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindById (machineId);
        if (null == machine) {
          log.ErrorFormat ("Get: " +
                           "unknown monitored machine with ID {0}",
                           machineId);
          return new ErrorDTO ("No monitored machine with the specified ID",
                               ErrorStatus.WrongRequestParameter);
        }

        using (IDAOTransaction transaction = session.BeginTransaction ("Web.Operation.Import")) {
          var operationCycle = ModelDAOHelper.DAOFactory.OperationCycleDAO
            .FindAt (machine, dateTime);
          if (null == operationCycle) {
            log.ErrorFormat ("Get: " +
              "no operation cycle at {0}",
              dateTime);
            transaction.Commit ();
            return new ErrorDTO ("No operation cycle at the specified date/time",
              ErrorStatus.NotApplicable);
          }
          if (!operationCycle.End.HasValue || !operationCycle.Begin.HasValue) {
            log.ErrorFormat ("Get: " +
              "the operation cycle has no bounded range");
            transaction.Commit ();
            return new ErrorDTO ("No bounded range for the operation cycle",
              ErrorStatus.NotApplicable);
          }
          var operationCycleRange = new UtcDateTimeRange (operationCycle.Begin.Value,
            operationCycle.End.Value);

          var operationSlot = ModelDAOHelper.DAOFactory.OperationSlotDAO.FindById (operationCycle.OperationSlot.Id,
            machine);
          if ((null == operationSlot)
            || (null == operationSlot.Operation)) {
            log.ErrorFormat ("Get: no detected operation for the selected operation cycle");
            transaction.Commit ();
            return new ErrorDTO ("No operation at the specified date/time",
              ErrorStatus.NotApplicable);
          }


          var operation = operationSlot.Operation; // not null, see above

          if (request.LoadingDuration) {
            if (request.Override || !operation.LoadingDuration.HasValue) {
              var previousBetweenCycle = ModelDAOHelper.DAOFactory.BetweenCyclesDAO
                .FindWithNextCycle (operationCycle);
              if (null == previousBetweenCycle) {
                log.ErrorFormat ("Get: no between cycles before operation cycle {0} at {1}", operationCycle.Id, request.DateTime);
              }
              else { // null != previousBetweenCycle
                var loadingDuration = previousBetweenCycle.End.Subtract (previousBetweenCycle.Begin);
                if (log.IsInfoEnabled) {
                  log.InfoFormat ("Get: set operation loading duration {0} to operation {1}",
                    loadingDuration, operation);
                }
                operation.LoadingDuration = loadingDuration;
                ModelDAOHelper.DAOFactory.OperationDAO.MakePersistent (operation);
              }
            }
          }

          if (request.MachiningDuration) {
            if (request.Override || !operation.MachiningDuration.HasValue) {
              Debug.Assert (operationCycleRange.Duration.HasValue);
              log.InfoFormat ("Get: set operation machining time {0} to operation {1}",
                operationCycleRange.Duration.Value, operation);
              operation.MachiningDuration = operationCycleRange.Duration.Value;
              ModelDAOHelper.DAOFactory.OperationDAO.MakePersistent (operation);
            }
          }

          IField toolNumberField = null;
          if (request.ToolNumber) {
            toolNumberField = ModelDAOHelper.DAOFactory.FieldDAO
              .FindByCode ("ToolNumber");
            if (null == toolNumberField) {
              log.FatalFormat ("Get: no field ToolNumber");
              transaction.Rollback ();
              return new ErrorDTO ("no field ToolNumber", ErrorStatus.UnexpectedError);
            }
          }
          if (request.SequenceDuration || request.ToolNumber) {
            foreach (var machineModule in machine.MachineModules) {
              var sequenceSlots = ModelDAOHelper.DAOFactory.SequenceSlotDAO
                .FindOverlapsRange (machineModule, operationCycleRange)
                .Where (slot => null != slot.Sequence)
                .Where (slot => ((IDataWithId)slot.Sequence.Operation).Id == ((IDataWithId)operation).Id);
              foreach (var sequenceSlot in sequenceSlots) {
                var sequence = sequenceSlot.Sequence;
                if (request.SequenceDuration
                  && (request.Override || !sequence.EstimatedTime.HasValue)) {
                  var sequenceRange = new UtcDateTimeRange (sequenceSlot.DateTimeRange.Intersects (operationCycleRange));
                  if (sequenceRange.IsEmpty ()) {
                    log.ErrorFormat ("Get: sequence range is empty after the intersection with operaitonCycleRange");
                  }
                  else { // !sequenceRange.IsEmpty ()
                    Debug.Assert (sequenceRange.Duration.HasValue); // Because of the intersection with operationCycleRange
                    var sequenceDuration = sequenceRange.Duration.Value;
                    log.InfoFormat ("Get: set sequence time {0} to sequence {1}",
                      sequenceDuration, sequence);
                    sequence.EstimatedTime = sequenceDuration;
                    if (request.SequenceToSequence && sequenceSlot.NextBegin.HasValue) {
                      Debug.Assert (sequenceSlot.BeginDateTime.HasValue);
                      sequenceRange = new UtcDateTimeRange (new UtcDateTimeRange (sequenceSlot.BeginDateTime, sequenceSlot.NextBegin.Value)
                        .Intersects (operationCycleRange));
                      if (sequenceRange.IsEmpty ()) {
                        log.ErrorFormat ("Get: begin to next begin range is empty");
                      }
                      else { // !sequenceRange.IsEmpty
                        Debug.Assert (sequenceRange.Duration.HasValue);
                        sequenceDuration = sequenceRange.Duration.Value;
                        sequence.EstimatedTime = sequenceDuration;
                      }
                    }
                  }
                  ModelDAOHelper.DAOFactory.SequenceDAO.MakePersistent (sequence);
                }
                if (request.ToolNumber
                  && (request.Override || string.IsNullOrEmpty (sequence.ToolNumber))) {
                  Debug.Assert (null != toolNumberField);
                  IEnumerable<ICncValue> toolNumberValues = ModelDAOHelper.DAOFactory.CncValueDAO
                    .FindByMachineFieldDateRange (machineModule, toolNumberField,
                    sequenceSlot.DateTimeRange);
                  if (toolNumberValues.Any ()) {
                    if (1 < toolNumberValues.Count ()) {
                      var lower = sequenceSlot.DateTimeRange.Lower;
                      var upper = sequenceSlot.DateTimeRange.Upper;
                      if (lower.HasValue) {
                        lower = lower.Value.AddSeconds (2);
                      }
                      if (upper.HasValue) {
                        upper = upper.Value.AddSeconds (-2);
                      }
                      var marginDateTimeRange = new UtcDateTimeRange (lower, upper);
                      if (!marginDateTimeRange.IsEmpty ()) {
                        var test = toolNumberValues
                          .Where (v => v.DateTimeRange.Overlaps (marginDateTimeRange));
                        if (test.Any ()) {
                          toolNumberValues = test;
                        }
                      }
                    }
                    var toolNumber = toolNumberValues
                      .OrderByDescending (v => v.DateTimeRange.Duration)
                      .First ()
                      .String.Trim ();
                    log.InfoFormat ("Get: set tool number {0} to sequence {1}",
                      toolNumber, sequence);
                    sequence.ToolNumber = toolNumber;
                    ModelDAOHelper.DAOFactory.SequenceDAO.MakePersistent (sequence);
                  }
                }
              }
            }
          }

          transaction.Commit ();
        }
      }

      return new OkDTO ("Operation import successful");
    }
    #endregion // Methods
  }
}
