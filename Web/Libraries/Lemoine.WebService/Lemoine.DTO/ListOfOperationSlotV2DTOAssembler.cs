// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Collections;
using Lemoine.ModelDAO;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Assembler for ListOfOperationSlotV2DTO
  /// </summary>
  public class ListOfOperationSlotV2DTOAssembler: IGenericDTOAssembler<ListOfOperationSlotV2DTO, Tuple<DateTime, DateTime, Tuple<IMonitoredMachine, IList<IOperationSlot>>>>
  {
    /// <summary>
    /// ListOfOperationSlotV2DTO assembler
    /// </summary>
    /// <param name="beginEndList"></param>
    /// <returns></returns>
    public ListOfOperationSlotV2DTO Assemble(Tuple<DateTime, DateTime, Tuple<IMonitoredMachine, IList<IOperationSlot>>> beginEndList)
    {
      DateTime begin = beginEndList.Item1;
      DateTime end = beginEndList.Item2;
      IMonitoredMachine monitoredMachine = beginEndList.Item3.Item1;
      IList<Lemoine.Model.IOperationSlot> slotList = beginEndList.Item3.Item2;
      ListOfOperationSlotV2DTO listOfOperationSlotV2DTO = new ListOfOperationSlotV2DTO();
      listOfOperationSlotV2DTO.Begin = ConvertDTO.DateTimeUtcToIsoString(begin);
      listOfOperationSlotV2DTO.End = ConvertDTO.DateTimeUtcToIsoString(end);
      listOfOperationSlotV2DTO.List = (new OperationSlotV2DTOAssembler()).Assemble(slotList).Reverse().ToList<OperationSlotV2DTO>(); // reverse chronological order
      listOfOperationSlotV2DTO.Config = (new WorkInformationConfigDTOAssembler()).Assemble(monitoredMachine);
      return listOfOperationSlotV2DTO;
    }
    
    /// <summary>
    /// ListOfOperationSlotV2DTO list assembler
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public IEnumerable<ListOfOperationSlotV2DTO> Assemble(IEnumerable<Tuple<DateTime, DateTime, Tuple<IMonitoredMachine, IList<IOperationSlot>>>> list)
    {
      IList<ListOfOperationSlotV2DTO> dtoList = new List<ListOfOperationSlotV2DTO>();
      foreach (Tuple<DateTime, DateTime, Tuple<IMonitoredMachine, IList<IOperationSlot>>> item in list) {
        dtoList.Add((ListOfOperationSlotV2DTO)this.Assemble((Tuple<DateTime, DateTime, Tuple<IMonitoredMachine, IList<IOperationSlot>>>)item));
      }
      return dtoList;
    }
  }
}
