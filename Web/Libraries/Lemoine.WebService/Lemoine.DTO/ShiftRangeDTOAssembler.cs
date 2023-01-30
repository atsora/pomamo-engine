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
  /// Assembler for of ShiftRangeDTO.
  /// </summary>
  public class ShiftRangeDTOAssembler : IGenericDTOAssembler<Lemoine.DTO.ShiftRangeDTO, IShiftSlot>
  {
    /// <summary>
    /// ShiftRangeDTO assembler
    /// </summary>
    /// <param name="shiftSlot">Not null</param>
    /// <returns></returns>
    public ShiftRangeDTO Assemble(IShiftSlot shiftSlot)
    {
      ShiftRangeDTO shiftRangeDTO = new ShiftRangeDTO();
      
      shiftRangeDTO.ShiftBegin = Lemoine.DTO.ConvertDTO.DateTimeUtcToIsoString(shiftSlot.BeginDateTime);
      shiftRangeDTO.ShiftEnd = Lemoine.DTO.ConvertDTO.DateTimeUtcToIsoString(shiftSlot.EndDateTime);
      if (null != shiftSlot.Shift) {
        shiftRangeDTO.ShiftColor = shiftSlot.Shift.Color;
        shiftRangeDTO.ShiftDisplay = shiftSlot.Shift.Display;
      }

      return shiftRangeDTO;
    }
    
    /// <summary>
    /// ShiftRangeDTO list assembler: skip the slots where shift is null
    /// </summary>
    /// <param name="shiftSlotList"></param>
    /// <returns></returns>
    public IEnumerable<ShiftRangeDTO> Assemble(IEnumerable<IShiftSlot> shiftSlotList) {
      IList<ShiftRangeDTO> shiftRangeDTOList = new List<ShiftRangeDTO>();
      foreach (IShiftSlot shiftSlot in shiftSlotList) {
        if (null != shiftSlot.Shift) {
          shiftRangeDTOList.Add(Assemble(shiftSlot));
        }
      }
      return shiftRangeDTOList;
    }
    
  }
}
