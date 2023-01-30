// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Assembler for of OperationSlotDTO (V2).
  /// </summary>
  public class OperationSlotV2DTOAssembler : IGenericDTOAssembler<Lemoine.DTO.OperationSlotV2DTO, IOperationSlot>
  {
    /// <summary>
    /// OperationSlotDTO assembler
    /// </summary>
    /// <param name="operationSlot">Not null</param>
    /// <returns></returns>
    public OperationSlotV2DTO Assemble(IOperationSlot operationSlot) {
      DateTime currentDate = DateTime.UtcNow;
      OperationSlotV2DTO operationSlotV2DTO = new OperationSlotV2DTO();
      
      operationSlotV2DTO.OperationSlotId = operationSlot.Id;
      operationSlotV2DTO.Begin = Lemoine.DTO.ConvertDTO.DateTimeUtcToIsoString(operationSlot.BeginDateTime);
      operationSlotV2DTO.End = Lemoine.DTO.ConvertDTO.DateTimeUtcToIsoString(operationSlot.EndDateTime);
      operationSlotV2DTO.WorkInformations = WorkInformationDTOBuilder.BuildFromOperationSlot(operationSlot);
      operationSlotV2DTO.Color = "#757575"; // May be changed later

      return operationSlotV2DTO;
    }
    
    /// <summary>
    /// OperationSlotDTO list assembler
    /// </summary>
    /// <param name="operationSlotList"></param>
    /// <returns></returns>
    public IEnumerable<OperationSlotV2DTO> Assemble(IEnumerable<IOperationSlot> operationSlotList) {
      IList<OperationSlotV2DTO> operationSlotV2DTOList = new List<OperationSlotV2DTO>();
      foreach (IOperationSlot operationSlot in operationSlotList) {
        operationSlotV2DTOList.Add(Assemble(operationSlot));
      }
      return operationSlotV2DTOList;
    }
    
  }
}
