// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

using Lemoine.Collections;
using Lemoine.Model;
using Lemoine.ModelDAO;

using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Assembler for MachineModuleCycleProgressDTO
  /// </summary>
  public class MachineModuleCycleProgressDTOAssembler :
    IGenericDTOAssembler<Lemoine.DTO.MachineModuleCycleProgressDTO, Tuple<IMachineModule, IOperationCycle, DateTime>>
  {

    static readonly ILog log = LogManager.GetLogger(typeof (MachineModuleCycleProgressDTOAssembler).FullName);

    /// <summary>
    /// MachineModuleCycleProgressDTO assembler
    /// </summary>
    /// <param name="machineModuleCycleDateTimeTriple"></param>
    /// <returns></returns>
    public MachineModuleCycleProgressDTO Assemble(Tuple<IMachineModule, IOperationCycle, DateTime> machineModuleCycleDateTimeTriple) {
      IMachineModule machineModule = machineModuleCycleDateTimeTriple.Item1;
      IOperationCycle operationCycle = machineModuleCycleDateTimeTriple.Item2;
      DateTime dateOfRequest = machineModuleCycleDateTimeTriple.Item3;
      
      if (!operationCycle.Begin.HasValue)
      {
        return null;
      }
      
      // find all sequence slots on machine module between start of cycle and date of request
      IList<ISequenceSlot> sequenceSlotList = ModelDAOHelper.DAOFactory.SequenceSlotDAO
        .FindOverlapsRange (machineModule, new UtcDateTimeRange (operationCycle.Begin.Value, dateOfRequest));
      
      // operation path selection from first sequence slot having a sequence
      IPath operationPath = null;
      for(int i = 0 ; i < sequenceSlotList.Count ; i++) {
        ISequence currentSequence = sequenceSlotList[0].Sequence;
        if (currentSequence != null) {
          operationPath = currentSequence.Path;
          break;
        }
      }
      
      return AssembleCommon(machineModule, sequenceSlotList, operationPath, dateOfRequest);
    }

    /// <summary>
    /// Also used by MachineModuleCycleProgressV2DTOAssembler.
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="sequenceSlotList"></param>
    /// <param name="operationPath"></param>
    /// <param name="dateOfRequest"></param>
    /// <returns></returns>
    public static MachineModuleCycleProgressDTO AssembleCommon(IMachineModule machineModule,
                                                               IList<ISequenceSlot> sequenceSlotList,
                                                               IPath operationPath,
                                                               DateTime dateOfRequest)
    {
      MachineModuleCycleProgressDTO machineModuleCycleProgressDTO = new MachineModuleCycleProgressDTO();
      machineModuleCycleProgressDTO.Id = machineModule.Id;
      machineModuleCycleProgressDTO.Name = machineModule.Name;

      if (operationPath == null)
      {
        // we don't know yet about the executed path found: return
        return machineModuleCycleProgressDTO;
      }

      int currentSPercent = 0;
      int cycleEDuration = 0;
      
      TimeSpan? operationEDuration = operationPath.Operation.MachiningDuration;
      
      if (operationEDuration.HasValue) {
        cycleEDuration = (int) Math.Round(operationEDuration.Value.TotalSeconds);
      } else {
        foreach(ISequence sequenceOfOperation in operationPath.Sequences) {
          if (sequenceOfOperation.EstimatedTime.HasValue) {
            cycleEDuration += (int) Math.Round(sequenceOfOperation.EstimatedTime.Value.TotalSeconds);
          }
        }
      }
      
      int orderSlot = 0;
      var sequenceInCycleStateDTOList = new List<SequenceInCycleStateDTO>();
      machineModuleCycleProgressDTO.SeqInCycleStateList = sequenceInCycleStateDTOList;
      
      // starting from oldest slot
      IEnumerator<ISequenceSlot> enumSequenceSlots = sequenceSlotList.GetEnumerator();
      
      int totalDuration = 0;
      ISequenceSlot currentSlot = null;
      
      // Get a default sequence time
      TimeSpan? defaultSequenceTime = null;
      if (operationPath.Operation.MachiningDuration.HasValue) {
        // Get the number of sequences for this operation with no estimated sequence duration
        int sequenceWithNoDurationNumber = operationPath
          .Sequences.Count (s => !s.EstimatedTime.HasValue
                            && (s.Kind != SequenceKind.Stop)
                            && (s.Kind != SequenceKind.OptionalStop));
        if (0 < sequenceWithNoDurationNumber) {
          // Get the total time of the sequence defined time
          TimeSpan sequenceTotalTime =
            TimeSpan.FromTicks (operationPath.Sequences
                                .Where (s => s.EstimatedTime.HasValue
                                        && (s.Kind != SequenceKind.Stop)
                                        && (s.Kind != SequenceKind.OptionalStop))
                                .Sum (s => s.EstimatedTime.Value.Ticks));
          // Remaining time for the operation
          TimeSpan remainingOperationTime = operationPath.Operation.MachiningDuration.Value
            .Subtract (sequenceTotalTime);
          if (TimeSpan.FromTicks (0) < remainingOperationTime) {
            defaultSequenceTime = TimeSpan.FromTicks (remainingOperationTime.Ticks / sequenceWithNoDurationNumber);
          }
        }
      }
      
      foreach(ISequence sequence in operationPath.Sequences) {
        // hopefully this enumerates sequence as ordered in operation
        
        TimeSpan? sequenceEstimatedTime = sequence.EstimatedTime.HasValue
          ? sequence.EstimatedTime.Value
          : defaultSequenceTime;
        
        SequenceInCycleStateDTO sequenceInCycleStateDTO = new SequenceInCycleStateDTO();
        sequenceInCycleStateDTOList.Add(sequenceInCycleStateDTO);
        
        sequenceInCycleStateDTO.Display = sequence.Display;
        sequenceInCycleStateDTO.Order = sequence.Order;
        sequenceInCycleStateDTO.Kind = sequence.Kind;
        if (sequenceEstimatedTime.HasValue) {
          sequenceInCycleStateDTO.EDuration = (int) Math.Round(sequenceEstimatedTime.Value.TotalSeconds);
        }
        
        if (cycleEDuration > 0) {
          sequenceInCycleStateDTO.SPercent = (int) Math.Round (((double) totalDuration / cycleEDuration) * 100);
          if (sequenceInCycleStateDTO.SPercent > 100) {
            sequenceInCycleStateDTO.SPercent = 100;
          }
        } else {
          sequenceInCycleStateDTO.SPercent = currentSPercent;
        }
        
        if (sequenceInCycleStateDTO.EDuration.HasValue) {
          totalDuration += sequenceInCycleStateDTO.EDuration.Value;
        }
        
        sequenceInCycleStateDTO.EPercent = currentSPercent;
        if ((sequenceInCycleStateDTO.EDuration.HasValue) && (cycleEDuration > 0)) {
          sequenceInCycleStateDTO.EPercent = (int) Math.Round (((double) totalDuration / cycleEDuration) * 100);
          if (sequenceInCycleStateDTO.EPercent > 100) {
            sequenceInCycleStateDTO.EPercent = 100;
          }
          currentSPercent = sequenceInCycleStateDTO.EPercent;
        }

        sequenceInCycleStateDTO.IsInCycle = false; // to be updated based on slots
        
        // advance to first slot corresponding to sequenceOfOperation or which is after it in operation order
        while((currentSlot == null) ||(currentSlot.Sequence == null) || (currentSlot.Sequence.Order < sequence.Order))
        {
          if (enumSequenceSlots.MoveNext()) {
            currentSlot = enumSequenceSlots.Current;
            orderSlot++;
          } else break;
        }
        
        if ((currentSlot != null) &&
            (currentSlot.Sequence != null) &&
            (currentSlot.Sequence.Order == sequence.Order))
        {
          // matching slot
          sequenceInCycleStateDTO.IsInCycle = true;
          
          if (orderSlot == sequenceSlotList.Count) {
            // last sequenceslot in cycle: maybe the slot is not over yet
            int sequenceSlotDurationInSeconds = (int) Math.Round(dateOfRequest.Subtract(currentSlot.BeginDateTime.Value).TotalSeconds);
            if (sequenceEstimatedTime.HasValue) {
              sequenceInCycleStateDTO.Late = (int) Math.Round(sequenceSlotDurationInSeconds - sequenceEstimatedTime.Value.TotalSeconds);
            }
            if ((sequenceEstimatedTime.HasValue) && (sequenceEstimatedTime.Value.TotalSeconds > 0)) {
              sequenceInCycleStateDTO.CPercent = (int) Math.Round(((sequenceSlotDurationInSeconds / sequenceEstimatedTime.Value.TotalSeconds)* 100));
              if (sequenceInCycleStateDTO.CPercent > 100) {
                sequenceInCycleStateDTO.CPercent = 100;
              }
            }
          } else {
            // not last sequenceslot in cycle: we suppose the slot is finished
            if (currentSlot.EndDateTime.HasValue) {
              int sequenceSlotDurationInSeconds = (int) Math.Round(currentSlot.EndDateTime.Value.Subtract(currentSlot.BeginDateTime.Value).TotalSeconds);
              if (sequenceEstimatedTime.HasValue) {
                sequenceInCycleStateDTO.Late = (int) Math.Round(sequenceSlotDurationInSeconds - sequenceEstimatedTime.Value.TotalSeconds);
              }
              sequenceInCycleStateDTO.CPercent = 100;
            }
          }
        } // else sequence does not appear in slots (sequenceInCycleStateDTO.IsInCycle remains false)
      }
      
      // compute next stop information (using the information about sequences that we just computed)
      SequenceInCycleStateDTO lastSequenceInCycle = null;
      int indexLastSequenceInCycle = -1;
      for(int i = 0 ; i < sequenceInCycleStateDTOList.Count ; i++) {
        SequenceInCycleStateDTO sequenceCycleState = sequenceInCycleStateDTOList[i];
        if (sequenceCycleState.IsInCycle) {
          lastSequenceInCycle = sequenceCycleState;
          indexLastSequenceInCycle = i;
        }
      }
      
      // adjust for estimated remaining time in sequence
      int totalTimeUntilNextStop = 0;
      if ((lastSequenceInCycle != null) && (lastSequenceInCycle.Late.HasValue)) {
        totalTimeUntilNextStop = -lastSequenceInCycle.Late.Value;
        if (totalTimeUntilNextStop < 0) totalTimeUntilNextStop = 0;
      }


      // Add all estimated time until next stop
      
      bool isOptional = false;
      bool foundStop = false;
      for(int i = indexLastSequenceInCycle+1 ; i < sequenceInCycleStateDTOList.Count ; i++) {
        SequenceInCycleStateDTO seqCycleState = sequenceInCycleStateDTOList[i];
        // hack for considering only true Stops
        if ((seqCycleState.Kind == SequenceKind.Stop) /*||(seqCycleState.Kind == SequenceKind.OptionalStop)*/)
        {
          isOptional = (seqCycleState.Kind == SequenceKind.OptionalStop);
          foundStop = true;
          break;
        }
        if (sequenceInCycleStateDTOList[i].EDuration.HasValue) {
          totalTimeUntilNextStop += sequenceInCycleStateDTOList[i].EDuration.Value;
        }
      }
      
      if (foundStop) {
        NextStopDTO nextStopDTO = new NextStopDTO();
        nextStopDTO.IsOptional = isOptional;
        nextStopDTO.UntilNext = totalTimeUntilNextStop;
        machineModuleCycleProgressDTO.NextStop = nextStopDTO;
      }
      
      return machineModuleCycleProgressDTO;
    }

    /// <summary>
    /// MachineModuleCycleProgressDTO list assembler
    /// </summary>
    /// <param name="machineModuleCycleDateTimeTripleList"></param>
    /// <returns></returns>
    public IEnumerable<MachineModuleCycleProgressDTO> Assemble(IEnumerable<Tuple<IMachineModule, IOperationCycle, DateTime>> machineModuleCycleDateTimeTripleList) {
      IList<MachineModuleCycleProgressDTO> list = new List<MachineModuleCycleProgressDTO>();
      foreach (Tuple<IMachineModule, IOperationCycle, DateTime> elt in machineModuleCycleDateTimeTripleList) {
        list.Add((MachineModuleCycleProgressDTO)this.Assemble((Tuple<IMachineModule, IOperationCycle, DateTime>)elt));
      }
      return list;
    }
  }
}