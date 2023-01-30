// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Collections;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Assembler for MachineObservationStateListSlotDTO
  /// </summary>
  public class MachineObservationStateSlotListV2DTOAssembler :
    IGenericDTOAssembler<Lemoine.DTO.MachineObservationStateSlotListV2DTO,
                         Tuple<DateTime, DateTime, IList<Model.IObservationStateSlot>>>
  {
    /// <summary>
    /// MachineObservationStateListSlotDTO assembler
    /// </summary>
    /// <param name="beginEndList"></param>
    /// <returns></returns>
    public MachineObservationStateSlotListV2DTO Assemble(Tuple<DateTime, DateTime, IList<Model.IObservationStateSlot>> beginEndList) 
    {
      DateTime begin = beginEndList.Item1;
      DateTime end = beginEndList.Item2;
      IList<Lemoine.Model.IObservationStateSlot> slotList = beginEndList.Item3;
      MachineObservationStateSlotListV2DTO MachineObservationStateSlotListV2DTO = new MachineObservationStateSlotListV2DTO();      
      MachineObservationStateSlotListV2DTO.Begin = ConvertDTO.DateTimeUtcToIsoString(begin);
      MachineObservationStateSlotListV2DTO.End = ConvertDTO.DateTimeUtcToIsoString(end);
      MachineObservationStateSlotListV2DTO.List = (new MachineObservationStateSlotV2DTOAssembler()).Assemble(slotList).Reverse().ToList<MachineObservationStateSlotV2DTO>(); // reverse chronological order
      return MachineObservationStateSlotListV2DTO;
    }
    
    /// <summary>
    /// MachineObservationStateListSlotDTO list assembler
    /// </summary>
    /// <param name="listBeginEndList"></param>
    /// <returns></returns>
    public IEnumerable<MachineObservationStateSlotListV2DTO> Assemble(IEnumerable<Tuple<DateTime, DateTime, IList<Model.IObservationStateSlot>>> listBeginEndList) {
      IList<MachineObservationStateSlotListV2DTO> MachineObservationStateSlotListV2DTO = new List<MachineObservationStateSlotListV2DTO>();
      foreach (Tuple<DateTime, DateTime, IList<Model.IObservationStateSlot>> beginEndList in listBeginEndList) {
        MachineObservationStateSlotListV2DTO.Add((MachineObservationStateSlotListV2DTO)this.Assemble((Tuple<DateTime, DateTime, IList<Model.IObservationStateSlot>>)beginEndList));
      }
      return MachineObservationStateSlotListV2DTO;
    }
  }
}
