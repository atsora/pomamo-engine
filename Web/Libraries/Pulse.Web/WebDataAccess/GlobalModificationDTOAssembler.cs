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
  /// Assembler for GlobalModificationDTO.
  /// </summary>
  public class GlobalModificationDTOAssembler
    : IGenericDTOAssembler<GlobalModificationDTO, Lemoine.Model.IGlobalModification>
  {
    /// <summary>
    /// GlobalModificationDTO assembler
    /// </summary>
    /// <param name="globalModification"></param>
    /// <returns></returns>
    public GlobalModificationDTO Assemble(Lemoine.Model.IGlobalModification globalModification)
    {
      GlobalModificationDTO globalModificationDTO = new GlobalModificationDTO();
      globalModificationDTO.Id = ((Lemoine.Collections.IDataWithId<long>)globalModification).Id;
      globalModificationDTO.AnalysisStatus = (int)globalModification.AnalysisStatus;
      return globalModificationDTO;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="modifications"></param>
    /// <returns></returns>
    public IEnumerable<GlobalModificationDTO> Assemble (IEnumerable<Lemoine.Model.IGlobalModification> modifications)
    {
      return modifications.Select (modification => Assemble(modification));
    }
  }
}
