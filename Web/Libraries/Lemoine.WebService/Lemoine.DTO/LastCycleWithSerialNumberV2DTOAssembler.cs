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
  /// Assembler for LastCycleWithSerialNumberV2DTO
  /// </summary>
  public class LastCycleWithSerialNumberV2DTOAssembler: IGenericDTOAssembler<LastCycleWithSerialNumberV2DTO, Tuple<IMonitoredMachine, DateTime>>
  {
    /// <summary>
    /// LastCycleWithSerialNumberV2DTO assembler
    /// </summary>
    /// <param name="machineDateTimePair"></param>
    /// <returns></returns>
    public LastCycleWithSerialNumberV2DTO Assemble(Tuple<IMonitoredMachine, DateTime> machineDateTimePair)
    {
      LastCycleWithSerialNumberV2DTO lastCycleWithSerialNumberV2DTO = new LastCycleWithSerialNumberV2DTO();
      lastCycleWithSerialNumberV2DTO.DataMissing = false;
      IMonitoredMachine machine = machineDateTimePair.Item1;
      
      IList<IOperationCycle> operationCyclesInRange =
        ModelDAOHelper.DAOFactory.OperationCycleDAO.FindAllInRange(machine,
                                                                   new UtcDateTimeRange (machineDateTimePair.Item2));
      if (operationCyclesInRange.Count == 0) {
        lastCycleWithSerialNumberV2DTO.CycleId = -1;
        lastCycleWithSerialNumberV2DTO.SerialNumber = "-1";
        return lastCycleWithSerialNumberV2DTO;
      }
      
      IOperationCycle lastOperationCycle = operationCyclesInRange[operationCyclesInRange.Count - 1];
      
      lastCycleWithSerialNumberV2DTO.CycleId = lastOperationCycle.Id;
      lastCycleWithSerialNumberV2DTO.Begin = ConvertDTO.DateTimeUtcToIsoString(lastOperationCycle.Begin);
      lastCycleWithSerialNumberV2DTO.End = ConvertDTO.DateTimeUtcToIsoString(lastOperationCycle.End);
      
      lastCycleWithSerialNumberV2DTO.EstimatedBegin = lastOperationCycle.Status.HasFlag (OperationCycleStatus.BeginEstimated);
      lastCycleWithSerialNumberV2DTO.EstimatedEnd = lastOperationCycle.Status.HasFlag (OperationCycleStatus.EndEstimated);

      IList<IOperationCycleDeliverablePiece> deliverablePieceList =
        ModelDAOHelper.DAOFactory.OperationCycleDeliverablePieceDAO.FindAllWithOperationCycle(lastOperationCycle);
      
      if (deliverablePieceList.Count == 0) {
        lastCycleWithSerialNumberV2DTO.SerialNumber = "0";
        lastCycleWithSerialNumberV2DTO.DataMissing = true;
      }
      
      if (deliverablePieceList.Count > 0)  {
        // TODO: should there be a special behavior if count is > 1 ?
        IDeliverablePiece deliverablePiece = deliverablePieceList[0].DeliverablePiece;
        lastCycleWithSerialNumberV2DTO.SerialNumber = deliverablePiece.Code;
        lastCycleWithSerialNumberV2DTO.DataMissing = false; // to be udpated

        for(int i = 0 ; i < operationCyclesInRange.Count - 1 ; i++) {
          IList<IOperationCycleDeliverablePiece> deliverablePieceList2 =
            ModelDAOHelper.DAOFactory.OperationCycleDeliverablePieceDAO.FindAllWithOperationCycle(operationCyclesInRange[i]);
          
          if (deliverablePieceList2.Count == 0) {
            lastCycleWithSerialNumberV2DTO.DataMissing = true;
            break;
          }
        }
      }
            
      return lastCycleWithSerialNumberV2DTO;
    }
    
    /// <summary>
    /// LastCycleWithSerialNumberV2DTO list assembler
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public IEnumerable<LastCycleWithSerialNumberV2DTO> Assemble(IEnumerable<Tuple<IMonitoredMachine, DateTime>> list)
    {
      IList<LastCycleWithSerialNumberV2DTO> dtoList = new List<LastCycleWithSerialNumberV2DTO>();
      foreach (Tuple<IMonitoredMachine, DateTime> item in list) {
        dtoList.Add(Assemble(item));
      }
      return dtoList;
    }
  }
}
