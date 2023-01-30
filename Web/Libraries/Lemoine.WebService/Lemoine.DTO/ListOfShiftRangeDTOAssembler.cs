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
  /// Assembler for ListOfShiftRangeDTO
  /// </summary>
  public class ListOfShiftRangeDTOAssembler: IGenericDTOAssembler<ListOfShiftRangeDTO, Tuple<DateTime, DateTime, IList<IShiftSlot>>>
  {
    /// <summary>
    /// ListOfShiftRangeDTO assembler
    /// </summary>
    /// <param name="beginEndList"></param>
    /// <returns></returns>
    public ListOfShiftRangeDTO Assemble(Tuple<DateTime, DateTime, IList<IShiftSlot>> beginEndList)
    {
      DateTime begin = beginEndList.Item1;
      DateTime end = beginEndList.Item2;
      IList<Lemoine.Model.IShiftSlot> slotList = beginEndList.Item3;
      ListOfShiftRangeDTO listOfShiftRangeDTO = new ListOfShiftRangeDTO();
      listOfShiftRangeDTO.Begin = ConvertDTO.DateTimeUtcToIsoString(begin);
      listOfShiftRangeDTO.End = ConvertDTO.DateTimeUtcToIsoString(end);
      listOfShiftRangeDTO.List = (new ShiftRangeDTOAssembler()).Assemble(slotList).ToList<ShiftRangeDTO>();
      return listOfShiftRangeDTO;
    }
    
    /// <summary>
    /// ListOfShiftRangeDTO list assembler
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public IEnumerable<ListOfShiftRangeDTO> Assemble(IEnumerable<Tuple<DateTime, DateTime, IList<IShiftSlot>>> list)
    {
      IList<ListOfShiftRangeDTO> dtoList = new List<ListOfShiftRangeDTO>();
      foreach (Tuple<DateTime, DateTime, IList<IShiftSlot>> item in list) {
        dtoList.Add((ListOfShiftRangeDTO)this.Assemble((Tuple<DateTime, DateTime, IList<IShiftSlot>>)item));
      }
      return dtoList;
    }
  }
}
