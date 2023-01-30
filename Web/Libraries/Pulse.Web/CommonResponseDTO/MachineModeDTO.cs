// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Pulse.Web.CommonResponseDTO
{
  /// <summary>
  /// DTO for MachineModeDTO.
  /// </summary>
  public class MachineModeDTO
  {
    /// <summary>
    /// Id
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Display
    /// </summary>
    public string Display { get; set; }
    
    /// <summary>
    /// Running property
    /// </summary>
    public bool? Running { get; set; }

    /// <summary>
    /// Color
    /// </summary>
    public string Color { get; set; }
    
    /// <summary>
    /// Machine mode category
    /// </summary>
    public MachineModeCategoryDTO Category { get; set; }
  }
  
  /// <summary>
  /// Assembler for MachineModeDTO.
  /// </summary>
  public class MachineModeDTOAssembler : IGenericDTOAssembler<MachineModeDTO, Lemoine.Model.IMachineMode> 
  {
    /// <summary>
    /// MachineModeDTO assembler
    /// </summary>
    /// <param name="machineMode"></param>
    /// <returns></returns>
    public MachineModeDTO Assemble(Lemoine.Model.IMachineMode machineMode)
    {
      MachineModeDTO machineModeDTO = new MachineModeDTO();
      machineModeDTO.Id = machineMode.Id;
      machineModeDTO.Display = machineMode.Display;
      machineModeDTO.Running = machineMode.Running;
      machineModeDTO.Color = machineMode.Color;
      machineModeDTO.Category = new MachineModeCategoryDTOAssembler ().Assemble (machineMode.MachineModeCategory);
      return machineModeDTO;
    }
    
    /// <summary>
    /// ReasonDTO list assembler
    /// </summary>
    /// <param name="machineModes"></param>
    /// <returns></returns>
    public IEnumerable<MachineModeDTO> Assemble(IEnumerable<Lemoine.Model.IMachineMode> machineModes)
    {
      IList<MachineModeDTO> machineModesDTO = new List<MachineModeDTO>();
      foreach (Lemoine.Model.IMachineMode machineMode in machineModes) {
        machineModesDTO.Add(Assemble(machineMode));
      }
      return machineModesDTO;
    }
  }
}
