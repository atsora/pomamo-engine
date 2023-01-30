// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Assembler for CyclesWithWorkInformationsInPeriodV2DTO
  /// </summary>
  public class CyclesWithWorkInformationsInPeriodV2DTOAssembler:     
               IGenericDTOAssembler<Lemoine.DTO.CyclesWithWorkInformationsInPeriodV2DTO,
                                    Tuple<DateTime, DateTime, IList<Model.IOperationCycle>>>
  {
    /// <summary>
    /// CyclesWithWorkInformationsInPeriodV2DTO assembler
    /// </summary>
    /// <param name="beginEndList"></param>
    /// <returns></returns>
    public CyclesWithWorkInformationsInPeriodV2DTO Assemble(Tuple<DateTime, DateTime, IList<Model.IOperationCycle>> beginEndList)
    {
      DateTime begin = beginEndList.Item1;
      DateTime end = beginEndList.Item2;
      IList<Lemoine.Model.IOperationCycle> cyclesList = beginEndList.Item3;
      CyclesWithWorkInformationsInPeriodV2DTO dto = new CyclesWithWorkInformationsInPeriodV2DTO();      
      dto.Begin = ConvertDTO.DateTimeUtcToIsoString(begin);
      dto.End = ConvertDTO.DateTimeUtcToIsoString(end);
      dto.List = (new CycleWithWorkInformationsV2DTOAssembler()).Assemble(cyclesList).Reverse().ToList<CycleWithWorkInformationsV2DTO>(); // reverse chronological order
      return dto;
    }
    
    /// <summary>
    /// CyclesWithWorkInformationsInPeriodV2DTO list assembler
    /// </summary>
    /// <param name="listBeginEndList"></param>
    /// <returns></returns>
    public IEnumerable<CyclesWithWorkInformationsInPeriodV2DTO> Assemble(IEnumerable<Tuple<DateTime, DateTime, IList<Model.IOperationCycle>>> listBeginEndList)
    {
      IList<CyclesWithWorkInformationsInPeriodV2DTO> dtoList = new List<CyclesWithWorkInformationsInPeriodV2DTO>();
      foreach (Tuple<DateTime, DateTime, IList<Model.IOperationCycle>> beginEndList in listBeginEndList) {
        dtoList.Add((CyclesWithWorkInformationsInPeriodV2DTO)this.Assemble((Tuple<DateTime, DateTime, IList<Model.IOperationCycle>>)beginEndList));
      }
      return dtoList;
    }
  }
}
