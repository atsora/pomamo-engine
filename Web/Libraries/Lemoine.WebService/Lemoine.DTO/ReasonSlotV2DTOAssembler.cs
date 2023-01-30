// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Assembler for ReasonSlotV2DTO
  /// </summary>
  public class ReasonSlotV2DTOAssembler: IGenericDTOAssembler<ReasonSlotV2DTO, IReasonSlot>
  {
    /// <summary>
    /// ReasonSlotV2DTO assembler
    /// </summary>
    /// <param name="reasonSlot"></param>
    /// <returns></returns>
    public ReasonSlotV2DTO Assemble(Lemoine.Model.IReasonSlot reasonSlot)
    {
      ReasonSlotV2DTO dto = new ReasonSlotV2DTO();
      dto.Id = reasonSlot.Id;
      dto.Begin = Lemoine.DTO.ConvertDTO.DateTimeUtcToIsoString(reasonSlot.BeginDateTime);
      dto.End = Lemoine.DTO.ConvertDTO.DateTimeUtcToIsoString(reasonSlot.EndDateTime);
      dto.Reason = (new ReasonDTOAssembler()).Assemble(reasonSlot.Reason);
      dto.OverwriteRequired = reasonSlot.OverwriteRequired;      
      return dto;
    }
    
    /// <summary>
    /// ReasonSlotV2DTO list assembler
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public IEnumerable<ReasonSlotV2DTO> Assemble(IEnumerable<Lemoine.Model.IReasonSlot> list)
    {
      IList<ReasonSlotV2DTO> dtoList = new List<ReasonSlotV2DTO>();
      foreach (Lemoine.Model.IReasonSlot item in list) {
        dtoList.Add(Assemble(item));
      }
      return dtoList;
    }
  }
}
