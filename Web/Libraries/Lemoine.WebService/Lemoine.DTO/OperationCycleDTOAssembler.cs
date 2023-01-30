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
  /// Assembler for OperationCycleDTO
  /// </summary>
  public class OperationCycleDTOAssembler: IGenericDTOAssembler<OperationCycleDTO, IOperationCycle>
  {
    /// <summary>
    /// OperationCycleDTO assembler
    /// </summary>
    /// <param name="operationCycle"></param>
    /// <returns></returns>
    public OperationCycleDTO Assemble(Lemoine.Model.IOperationCycle operationCycle)
    {
      OperationCycleDTO dto = new OperationCycleDTO();
      dto.Id = operationCycle.Id;
      dto.Begin = ConvertDTO.DateTimeUtcToIsoString(operationCycle.Begin);
      dto.End = ConvertDTO.DateTimeUtcToIsoString(operationCycle.End);
      dto.OffsetDuration = operationCycle.OffsetDuration;
      dto.Status = operationCycle.Status.ToString();
      return dto;
    }
    
    /// <summary>
    /// OperationCycleDTO list assembler
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public IEnumerable<OperationCycleDTO> Assemble(IEnumerable<Lemoine.Model.IOperationCycle> list)
    {
      IList<OperationCycleDTO> dtoList = new List<OperationCycleDTO>();
      foreach (Lemoine.Model.IOperationCycle item in list) {
        dtoList.Add(Assemble(item));
      }
      return dtoList;
    }
  }
}
