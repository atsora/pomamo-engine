// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Assembler for MachineModeDTO.
  /// </summary>
  public class MachineModeDTOAssembler : IGenericDTOAssembler<Lemoine.DTO.MachineModeDTO, Lemoine.Model.IMachineMode>
  {
    /// <summary>
    /// MachineModeDTO assembler
    /// </summary>
    /// <param name="machineMode"></param>
    /// <returns></returns>
    public MachineModeDTO Assemble(Lemoine.Model.IMachineMode machineMode) {
      MachineModeDTO machineModeDTO = new MachineModeDTO();
      machineModeDTO.Id = machineMode.Id;
      machineModeDTO.Text = machineMode.Display;
      machineModeDTO.Color = machineMode.Color;
      return machineModeDTO;
    }
    
    /// <summary>
    /// MachineModeDTO list assembler
    /// </summary>
    /// <param name="machineModeList"></param>
    /// <returns></returns>
    public IEnumerable<MachineModeDTO> Assemble(IEnumerable<Lemoine.Model.IMachineMode> machineModeList) {
      IList<MachineModeDTO> machineModeDTOList = new List<MachineModeDTO>();
      foreach (Lemoine.Model.IMachineMode machineMode in machineModeList) {
        machineModeDTOList.Add(Assemble(machineMode));
      }
      return machineModeDTOList;
    }
  }
}
