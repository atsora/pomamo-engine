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
  /// Assembler for OperationCycleDeliverablePieceWithWorkInformationDTO
  /// </summary>
  public class OperationCycleDeliverablePieceWithWorkInformationDTOAssembler: IGenericDTOAssembler<OperationCycleDeliverablePieceWithWorkInformationDTO, IOperationCycleDeliverablePiece>
  {
    /// <summary>
    /// OperationCycleDeliverablePieceWithWorkInformationDTO assembler
    /// </summary>
    /// <param name="operationCycleDeliverablePiece"></param>
    /// <returns></returns>
    public OperationCycleDeliverablePieceWithWorkInformationDTO Assemble(Lemoine.Model.IOperationCycleDeliverablePiece operationCycleDeliverablePiece)
    {
      OperationCycleDeliverablePieceWithWorkInformationDTO dto = new OperationCycleDeliverablePieceWithWorkInformationDTO();
      //dto.i
      
      // TODO: build a DTO from the persistent class, to complete...
      return dto;
    }
    
    /// <summary>
    /// OperationCycleDeliverablePieceWithWorkInformationDTO list assembler
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public IEnumerable<OperationCycleDeliverablePieceWithWorkInformationDTO> Assemble(IEnumerable<Lemoine.Model.IOperationCycleDeliverablePiece> list)
    {
      IList<OperationCycleDeliverablePieceWithWorkInformationDTO> dtoList = new List<OperationCycleDeliverablePieceWithWorkInformationDTO>();
      foreach (Lemoine.Model.IOperationCycleDeliverablePiece item in list) {
        dtoList.Add(Assemble(item));
      }
      return dtoList;
    }
  }
}
