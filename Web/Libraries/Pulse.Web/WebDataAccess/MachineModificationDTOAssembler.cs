// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Core.Log;



namespace Pulse.Web.WebDataAccess
{
  /// <summary>
  /// Assembler for MachineModificationDTO.
  /// </summary>
  public class MachineModificationDTOAssembler
    : IGenericDTOAssembler<MachineModificationDTO, Lemoine.Model.IMachineModification>
  {
    /// <summary>
    /// MachineModificationDTO assembler
    /// </summary>
    /// <param name="machineModification"></param>
    /// <returns></returns>
    public MachineModificationDTO Assemble(Lemoine.Model.IMachineModification machineModification)
    {
      MachineModificationDTO machineModificationDTO = new MachineModificationDTO();
      machineModificationDTO.Id = ((Lemoine.Collections.IDataWithId<long>)machineModification).Id;
      machineModificationDTO.MachineId = machineModification.Machine.Id;
      machineModificationDTO.AnalysisStatus = (int)machineModification.AnalysisStatus;
      return machineModificationDTO;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="modifications"></param>
    /// <returns></returns>
    public IEnumerable<MachineModificationDTO> Assemble (IEnumerable<Lemoine.Model.IMachineModification> modifications)
    {
      return modifications.Select (modification => Assemble(modification));
    }
  }
}
