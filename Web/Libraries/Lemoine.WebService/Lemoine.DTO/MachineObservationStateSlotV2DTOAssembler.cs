// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Assembler for MachineObservationStateSlotV2DTO.
  /// </summary>
  public class MachineObservationStateSlotV2DTOAssembler : IGenericDTOAssembler<Lemoine.DTO.MachineObservationStateSlotV2DTO, Lemoine.Model.IObservationStateSlot>
  {
    
    /// <summary>
    /// MachineObservationStateSlotV2DTO assembler
    /// </summary>
    /// <param name="observationSlot"></param>
    /// <returns></returns>
    public MachineObservationStateSlotV2DTO Assemble(Lemoine.Model.IObservationStateSlot observationSlot) {
      MachineObservationStateSlotV2DTO machineObservationStateSlotV2DTO = new MachineObservationStateSlotV2DTO();
      machineObservationStateSlotV2DTO.Id = observationSlot.Id;
      machineObservationStateSlotV2DTO.Begin = Lemoine.DTO.ConvertDTO.DateTimeUtcToIsoString(observationSlot.BeginDateTime);
      machineObservationStateSlotV2DTO.End = Lemoine.DTO.ConvertDTO.DateTimeUtcToIsoString(observationSlot.EndDateTime);
      machineObservationStateSlotV2DTO.MachineObservationState = (new MachineObservationStateDTOAssembler()).Assemble(observationSlot.MachineObservationState);
      return machineObservationStateSlotV2DTO;
    }
    
    /// <summary>
    /// MachineObservationStateSlotV2DTO list assembler
    /// </summary>
    /// <param name="observationSlotList"></param>
    /// <returns></returns>
    public IEnumerable<MachineObservationStateSlotV2DTO> Assemble(IEnumerable<Lemoine.Model.IObservationStateSlot> observationSlotList) {
      IList<MachineObservationStateSlotV2DTO> observationSlotDTOList = new List<MachineObservationStateSlotV2DTO>();
      foreach (Lemoine.Model.IObservationStateSlot observationStateSlot in observationSlotList) {
        observationSlotDTOList.Add(Assemble(observationStateSlot));
      }
      return observationSlotDTOList;
    }
  }
}

