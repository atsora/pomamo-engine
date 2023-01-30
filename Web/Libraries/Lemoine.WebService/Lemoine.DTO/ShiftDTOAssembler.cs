// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Assembler for of ShiftDTO.
  /// </summary>
  public class ShiftDTOAssembler : IGenericDTOAssembler<Lemoine.DTO.ShiftDTO, IShift>
  {
    /// <summary>
    /// ShiftDTO assembler
    /// </summary>
    /// <param name="shift">Not null</param>
    /// <returns></returns>
    public ShiftDTO Assemble(IShift shift)
    {
      Debug.Assert (null != shift);
      
      ShiftDTO shiftDTO = new ShiftDTO();
      shiftDTO.Id = shift.Id;
      shiftDTO.Display = shift.Display;
      shiftDTO.Color = shift.Color;

      return shiftDTO;
    }
    
    /// <summary>
    /// ShiftDTO list assembler
    /// </summary>
    /// <param name="shifts"></param>
    /// <returns></returns>
    public IEnumerable<ShiftDTO> Assemble(IEnumerable<IShift> shifts)
    {
      IList<ShiftDTO> shiftDTOs = new List<ShiftDTO>();
      foreach (IShift shift in shifts) {
        shiftDTOs.Add(Assemble(shift));
      }
      return shiftDTOs;
    }
    
  }
}
