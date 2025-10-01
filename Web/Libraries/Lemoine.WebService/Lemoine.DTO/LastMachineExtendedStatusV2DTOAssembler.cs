// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Assembler for LastMachineExtendedStatusV2DTO
  /// </summary>
  public class LastMachineExtendedStatusV2DTOAssembler: IGenericDTOAssembler<LastMachineExtendedStatusV2DTO, Tuple<Lemoine.Model.IReasonSlot, bool, int>>
  {
    /// <summary>
    /// LastMachineExtendedStatusV2DTO assembler
    /// </summary>
    /// <param name="reasonSlotRequirement"></param>
    /// <returns></returns>
    public LastMachineExtendedStatusV2DTO Assemble(Tuple<Lemoine.Model.IReasonSlot, bool, int> reasonSlotRequirement)
    {
      LastMachineExtendedStatusV2DTO dto = new LastMachineExtendedStatusV2DTO();
      dto.MachineStatus = (new MachineExtendedStatusV2DTOAssembler()).Assemble(reasonSlotRequirement.Item1);
      dto.RequiredReason = reasonSlotRequirement.Item2;
      dto.RequiredNumber = reasonSlotRequirement.Item3;
      return dto;
    }
    
    /// <summary>
    /// LastMachineExtendedStatusV2DTO list assembler
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public IEnumerable<LastMachineExtendedStatusV2DTO> Assemble(IEnumerable<Tuple<Lemoine.Model.IReasonSlot, bool, int>> list)
    {
      var dtoList = new List<LastMachineExtendedStatusV2DTO>();
      foreach (var item in list) {
        dtoList.Add(Assemble(item));
      }
      return dtoList;
    }
  }
}
