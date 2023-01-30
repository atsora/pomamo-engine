// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Description of MachineCategoryDTOAssembler.
  /// </summary>
  public class MachineCategoryDTOAssembler
  {
    /// <summary>
    /// MachineCategoryDTO assembler
    /// </summary>
    /// <param name="machineCategory"></param>
    /// <returns></returns>
    public MachineCategoryDTO Assemble(Lemoine.Model.IMachineCategory machineCategory) {
      MachineCategoryDTO machineCategoryDTO = new MachineCategoryDTO();
      machineCategoryDTO.Id = machineCategory.Id;
      machineCategoryDTO.Name = machineCategory.Display;
      return machineCategoryDTO;
    }
    
    /// <summary>
    /// MachineCategoryDTO list assembler
    /// </summary>
    /// <param name="machineCategoryList"></param>
    /// <returns></returns>
    public IEnumerable<MachineCategoryDTO> Assemble(IEnumerable<Lemoine.Model.IMachineCategory> machineCategoryList) {
      IList<MachineCategoryDTO> machineCategoryDTOList = new List<MachineCategoryDTO>();
      foreach (Lemoine.Model.IMachineCategory machineCategory in machineCategoryList) {
        machineCategoryDTOList.Add(Assemble(machineCategory));
      }
      return machineCategoryDTOList;
    }
  }
}
