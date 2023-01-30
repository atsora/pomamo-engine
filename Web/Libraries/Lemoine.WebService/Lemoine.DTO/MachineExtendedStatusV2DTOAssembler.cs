// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Assembler for MachineExtendedStatusV2DTO
  /// </summary>
  public class MachineExtendedStatusV2DTOAssembler: IGenericDTOAssembler<MachineExtendedStatusV2DTO, Lemoine.Model.IReasonSlot>
  {
    /// <summary>
    /// MachineExtendedStatusV2DTO assembler
    /// </summary>
    /// <param name="reasonSlot"></param>
    /// <returns></returns>
    public MachineExtendedStatusV2DTO Assemble(Lemoine.Model.IReasonSlot reasonSlot)
    {
      MachineExtendedStatusV2DTO dto = new MachineExtendedStatusV2DTO();
      dto.ReasonSlot = (new ReasonSlotV2DTOAssembler()).Assemble(reasonSlot);
      dto.MachineObservationState = (new MachineObservationStateDTOAssembler()).Assemble(reasonSlot.MachineObservationState);
      dto.MachineMode = (new MachineModeDTOAssembler()).Assemble(reasonSlot.MachineMode);
      return dto;
    }
    
    /// <summary>
    /// MachineExtendedStatusV2DTO list assembler
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public IEnumerable<MachineExtendedStatusV2DTO> Assemble(IEnumerable<Lemoine.Model.IReasonSlot> list)
    {
      IList<MachineExtendedStatusV2DTO> dtoList = new List<MachineExtendedStatusV2DTO>();
      foreach (Lemoine.Model.IReasonSlot item in list) {
        dtoList.Add(Assemble(item));
      }
      return dtoList;
    }
  }
}
