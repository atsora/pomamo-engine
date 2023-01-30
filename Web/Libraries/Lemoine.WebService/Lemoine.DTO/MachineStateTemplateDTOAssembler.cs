// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.I18N;
using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Assembler for MachineStateTemplateDTO.
  /// </summary>
  public class MachineStateTemplateDTOAssembler : IGenericDTOAssembler<Lemoine.DTO.MachineStateTemplateDTO, Lemoine.Model.IMachineStateTemplate>
  {
    /// <summary>
    /// MachineStateTemplateDTO assembler
    /// </summary>
    /// <param name="machineStateTemplate"></param>
    /// <returns></returns>
    public MachineStateTemplateDTO Assemble(Lemoine.Model.IMachineStateTemplate machineStateTemplate)
    {
      MachineStateTemplateDTO machineStateTemplateDTO = new MachineStateTemplateDTO();
      if (null == machineStateTemplate) {
        machineStateTemplateDTO.Id = 0;
        machineStateTemplateDTO.Text = PulseCatalog.GetString ("NoMachineStateTemplate");
      }
      else { // null != machineStateTemplate
        machineStateTemplateDTO.Id = machineStateTemplate.Id;
        machineStateTemplateDTO.Text = machineStateTemplate.Display;
      }
      return machineStateTemplateDTO;
    }
    
    /// <summary>
    /// MachineStateTemplateDTO list assembler
    /// </summary>
    /// <param name="machineStateTemplateList"></param>
    /// <returns></returns>
    public IEnumerable<MachineStateTemplateDTO> Assemble(IEnumerable<Lemoine.Model.IMachineStateTemplate> machineStateTemplateList) {
      IList<MachineStateTemplateDTO> machineStateTemplateDTOList = new List<MachineStateTemplateDTO>();
      foreach (Lemoine.Model.IMachineStateTemplate machineStateTemplate in machineStateTemplateList) {
        machineStateTemplateDTOList.Add(Assemble(machineStateTemplate));
      }
      return machineStateTemplateDTOList;
    }
  }
}