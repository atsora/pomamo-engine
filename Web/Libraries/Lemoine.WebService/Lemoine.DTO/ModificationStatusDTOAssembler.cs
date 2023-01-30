// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Assembler for ModificationStatusDTO
  /// </summary>
  public class ModificationStatusDTOAssembler : IGenericDTOAssembler<Lemoine.DTO.ModificationStatusDTO, Lemoine.Model.IModification>
  {
    /// <summary>
    /// ModificationStatusDTO assembler
    /// </summary>
    /// <param name="modification"></param>
    /// <returns></returns>
    public ModificationStatusDTO Assemble(Lemoine.Model.IModification modification) {
      ModificationStatusDTO modificationStatusDTO = new ModificationStatusDTO();
      modificationStatusDTO.Id = ((Lemoine.Collections.IDataWithId<long>)modification).Id;
      modificationStatusDTO.Status = modification.AnalysisStatus;
      return modificationStatusDTO;
    }
    
    /// <summary>
    /// ModificationStatusDTO list assembler
    /// </summary>
    /// <param name="modificationList"></param>
    /// <returns></returns>
    public IEnumerable<ModificationStatusDTO> Assemble(IEnumerable<Lemoine.Model.IModification> modificationList) {
      IList<ModificationStatusDTO> modificationStatusDTOList = new List<ModificationStatusDTO>();
      foreach (Lemoine.Model.IModification modification in modificationList) {
        modificationStatusDTOList.Add(Assemble(modification));
      }
      return modificationStatusDTOList;
    }
  }
}
