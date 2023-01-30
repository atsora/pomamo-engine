// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Assembler for MachineObservationStateDTO.
  /// </summary>
  public class MachineObservationStateDTOAssembler : IGenericDTOAssembler<Lemoine.DTO.MachineObservationStateDTO, Lemoine.Model.IMachineObservationState>
  {
    /// <summary>
    /// MachineObservationStateDTO assembler
    /// </summary>
    /// <param name="machineObservationState"></param>
    /// <returns></returns>
    public MachineObservationStateDTO Assemble(Lemoine.Model.IMachineObservationState machineObservationState) {
      MachineObservationStateDTO machineObservationStateDTO = new MachineObservationStateDTO();
      machineObservationStateDTO.Id = machineObservationState.Id;
      machineObservationStateDTO.Text = machineObservationState.Display;
      return machineObservationStateDTO;
    }
    
    /// <summary>
    /// MachineObservationStateDTO list assembler
    /// </summary>
    /// <param name="machineObservationStateList"></param>
    /// <returns></returns>
    public IEnumerable<MachineObservationStateDTO> Assemble(IEnumerable<Lemoine.Model.IMachineObservationState> machineObservationStateList) {
      IList<MachineObservationStateDTO> machineObservationStateDTOList = new List<MachineObservationStateDTO>();
      foreach (Lemoine.Model.IMachineObservationState machineObservationState in machineObservationStateList) {
        machineObservationStateDTOList.Add(Assemble(machineObservationState));
      }
      return machineObservationStateDTOList;
    }
  }
}