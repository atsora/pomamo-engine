// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Description of MachineCellDTOAssembler.
  /// </summary>
  public class MachineCellDTOAssembler
  {
    /// <summary>
    /// MachineCellDTO assembler
    /// </summary>
    /// <param name="cell"> </param>
    /// <returns></returns>
    public MachineCellDTO Assemble(Lemoine.Model.ICell cell) {
      MachineCellDTO machineCellDTO = new MachineCellDTO();
      machineCellDTO.Id = cell.Id;
      machineCellDTO.Name = cell.Display;
      return machineCellDTO;
    }
    
    /// <summary>
    /// MachineCellDTO list assembler
    /// </summary>
    /// <param name="cellList"> </param>
    /// <returns></returns>
    public IEnumerable<MachineCellDTO> Assemble(IEnumerable<Lemoine.Model.ICell> cellList) {
      IList<MachineCellDTO> machineCellDTOList = new List<MachineCellDTO>();
      foreach (Lemoine.Model.ICell machineCell in cellList) {
        machineCellDTOList.Add(Assemble(machineCell));
      }
      return machineCellDTOList;
    }
  }
}
