// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Description of MachineSubCategoryDTOAssembler.
  /// </summary>
  public class MachineSubCategoryDTOAssembler
  {
    /// <summary>
    /// MachineSubCategoryDTO assembler
    /// </summary>
    /// <param name="machineSubCategory"></param>
    /// <returns></returns>
    public MachineSubCategoryDTO Assemble(Lemoine.Model.IMachineSubCategory machineSubCategory) {
      MachineSubCategoryDTO machineSubCategoryDTO = new MachineSubCategoryDTO();
      machineSubCategoryDTO.Id = machineSubCategory.Id;
      machineSubCategoryDTO.Name = machineSubCategory.Display;
      return machineSubCategoryDTO;
    }
    
    /// <summary>
    /// MachineSubCategoryDTO list assembler
    /// </summary>
    /// <param name="machineSubCategoryList"></param>
    /// <returns></returns>
    public IEnumerable<MachineSubCategoryDTO> Assemble(IEnumerable<Lemoine.Model.IMachineSubCategory> machineSubCategoryList) {
      IList<MachineSubCategoryDTO> machineSubCategoryDTOList = new List<MachineSubCategoryDTO>();
      foreach (Lemoine.Model.IMachineSubCategory machineSubCategory in machineSubCategoryList) {
        machineSubCategoryDTOList.Add(Assemble(machineSubCategory));
      }
      return machineSubCategoryDTOList;
    }
  }
}
