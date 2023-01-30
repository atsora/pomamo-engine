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
  /// Assembler for OperationCycleDeliverablePieceDTO
  /// </summary>
  public class OperationCycleDeliverablePieceDTOAssembler: IGenericDTOAssembler<OperationCycleDeliverablePieceDTO, IOperationCycleDeliverablePiece>
  {
    /// <summary>
    /// OperationCycleDeliverablePieceDTO assembler
    /// </summary>
    /// <param name="operationcycledeliverablepiece"></param>
    /// <returns></returns>
    public OperationCycleDeliverablePieceDTO Assemble(Lemoine.Model.IOperationCycleDeliverablePiece operationcycledeliverablepiece)
    {
      OperationCycleDeliverablePieceDTO dto = new OperationCycleDeliverablePieceDTO();
      dto.Id = operationcycledeliverablepiece.Id;
      
      dto.MachineId = operationcycledeliverablepiece.Machine.Id;
      if(null != operationcycledeliverablepiece.NonConformanceReason){
        dto.NonConformanceReasonId = operationcycledeliverablepiece.NonConformanceReason.Id;
        dto.NonConformanceReasonDetails = operationcycledeliverablepiece.NonConformanceDetails;
      }
 
      dto.OperationCycleId = operationcycledeliverablepiece.OperationCycle.Id;
      if(operationcycledeliverablepiece.OperationCycle.Begin.HasValue){
        dto.OperationCycleBegin = ConvertDTO.DateTimeUtcToIsoString(operationcycledeliverablepiece.OperationCycle.Begin);
      }
      if(operationcycledeliverablepiece.OperationCycle.End.HasValue){
        dto.OperationCycleEnd = ConvertDTO.DateTimeUtcToIsoString(operationcycledeliverablepiece.OperationCycle.End);
      }
      if(null != operationcycledeliverablepiece.OperationCycle.OperationSlot){
        if(null != operationcycledeliverablepiece.OperationCycle.OperationSlot.WorkOrder){
          dto.WorkOrderId = ((Lemoine.Collections.IDataWithId)operationcycledeliverablepiece.OperationCycle.OperationSlot.WorkOrder).Id;
        }
        if(null != operationcycledeliverablepiece.OperationCycle.OperationSlot.Operation){
          dto.OperationId = ((Lemoine.Collections.IDataWithId)operationcycledeliverablepiece.OperationCycle.OperationSlot.Operation).Id;
        }
      }
      
      dto.DeliverablePieceId = operationcycledeliverablepiece.DeliverablePiece.Id;
      dto.SerialNumber = operationcycledeliverablepiece.DeliverablePiece.Code;
      dto.ComponentId = ((Lemoine.Collections.IDataWithId)operationcycledeliverablepiece.DeliverablePiece.Component).Id;
      if(null != dto.OperationId){
        foreach (IComponentIntermediateWorkPiece ciwp in operationcycledeliverablepiece.DeliverablePiece.Component.ComponentIntermediateWorkPieces) {
          if(((Lemoine.Collections.IDataWithId)ciwp.IntermediateWorkPiece.Operation).Id == dto.OperationId){
            dto.IntermediateWorkPieceId = ((Lemoine.Collections.IDataWithId)ciwp.IntermediateWorkPiece).Id;
            break;
          }
        }      
      }

      return dto;
    }
    
    /// <summary>
    /// OperationCycleDeliverablePieceDTO list assembler
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public IEnumerable<OperationCycleDeliverablePieceDTO> Assemble(IEnumerable<Lemoine.Model.IOperationCycleDeliverablePiece> list)
    {
      IList<OperationCycleDeliverablePieceDTO> dtoList = new List<OperationCycleDeliverablePieceDTO>();
      foreach (Lemoine.Model.IOperationCycleDeliverablePiece item in list) {
        dtoList.Add(Assemble(item));
      }
      return dtoList;
    }
  }
}
