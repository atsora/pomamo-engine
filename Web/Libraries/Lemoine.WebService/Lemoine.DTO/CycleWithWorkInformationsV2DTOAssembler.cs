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
  /// Assembler for CycleWithWorkInformationsV2DTO
  /// </summary>
  public class CycleWithWorkInformationsV2DTOAssembler: IGenericDTOAssembler<CycleWithWorkInformationsV2DTO, IOperationCycle>
  {
    /// <summary>
    /// CycleWithWorkInformationsV2DTO assembler
    /// </summary>
    /// <param name="operationCycle"></param>
    /// <returns></returns>
    public CycleWithWorkInformationsV2DTO Assemble(Lemoine.Model.IOperationCycle operationCycle)
    {
      CycleWithWorkInformationsV2DTO dto = new CycleWithWorkInformationsV2DTO();

      dto.CycleId = operationCycle.Id;
      dto.Begin = ConvertDTO.DateTimeUtcToIsoString(operationCycle.Begin);
      dto.End = ConvertDTO.DateTimeUtcToIsoString(operationCycle.End);
      dto.EstimatedBegin = operationCycle.Status.HasFlag (OperationCycleStatus.BeginEstimated);
      dto.EstimatedEnd = operationCycle.Status.HasFlag (OperationCycleStatus.EndEstimated);

      IList<Lemoine.Model.IOperationCycleDeliverablePiece> ocdpList =
        ModelDAOHelper.DAOFactory.OperationCycleDeliverablePieceDAO.FindAllWithOperationCycle(operationCycle);
      
      if (ocdpList.Count == 0) {
        dto.SerialNumber = null;
      } else {
        // TODO: what if there are several deliverable pieces ?
        Lemoine.Model.IOperationCycleDeliverablePiece ocdp = ocdpList[0];
        dto.SerialNumber = ocdp.DeliverablePiece.Code;
      }
      
      dto.WorkInformations = WorkInformationDTOBuilder.BuildFromOperationSlot(operationCycle.OperationSlot);
      return dto;
    }
    
    /// <summary>
    /// CycleWithWorkInformationsV2DTO list assembler
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public IEnumerable<CycleWithWorkInformationsV2DTO> Assemble(IEnumerable<Lemoine.Model.IOperationCycle> list)
    {
      IList<CycleWithWorkInformationsV2DTO> dtoList = new List<CycleWithWorkInformationsV2DTO>();
      foreach (Lemoine.Model.IOperationCycle item in list) {
        dtoList.Add(Assemble(item));
      }
      return dtoList;
    }
  }
}
