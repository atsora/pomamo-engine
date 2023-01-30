// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Pulse.Web.CommonResponseDTO
{
  /// <summary>
  /// DTO for MachineModeCategoryDTO.
  /// </summary>
  public class MachineModeCategoryDTO
  {
    /// <summary>
    /// Id
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Display
    /// </summary>
    public string Display { get; set; }
  }
  
  /// <summary>
  /// Assembler for MachineModeCategoryDTO.
  /// </summary>
  public class MachineModeCategoryDTOAssembler : IGenericDTOAssembler<MachineModeCategoryDTO, Lemoine.Model.MachineModeCategoryId> 
  {
    /// <summary>
    /// MachineModeCategoryDTO assembler
    /// </summary>
    /// <param name="machineModeCategoryId"></param>
    /// <returns></returns>
    public MachineModeCategoryDTO Assemble(Lemoine.Model.MachineModeCategoryId machineModeCategoryId)
    {
      MachineModeCategoryDTO machineModeCategoryDTO = new MachineModeCategoryDTO();
      machineModeCategoryDTO.Id = (int)machineModeCategoryId;
      var i18nKey = "MachineModeCategory" + machineModeCategoryId.ToString ();
      machineModeCategoryDTO.Display = Lemoine.I18N.PulseCatalog.GetString (i18nKey, machineModeCategoryId.ToString ());
      return machineModeCategoryDTO;
    }
    
    /// <summary>
    /// MachineModeCategoryDTO list assembler
    /// </summary>
    /// <param name="machineModeCategoryIds"></param>
    /// <returns></returns>
    public IEnumerable<MachineModeCategoryDTO> Assemble(IEnumerable<Lemoine.Model.MachineModeCategoryId> machineModeCategoryIds)
    {
      IList<MachineModeCategoryDTO> machineModeCategoryIdsDTO = new List<MachineModeCategoryDTO>();
      foreach (Lemoine.Model.MachineModeCategoryId machineModeCategory in machineModeCategoryIds) {
        machineModeCategoryIdsDTO.Add(Assemble(machineModeCategory));
      }
      return machineModeCategoryIdsDTO;
    }
  }
}
