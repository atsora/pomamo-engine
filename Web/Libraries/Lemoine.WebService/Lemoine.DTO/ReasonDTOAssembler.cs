// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Assembler for ReasonDTO.
  /// </summary>
  public class ReasonDTOAssembler : IGenericDTOAssembler<Lemoine.DTO.ReasonDTO, Lemoine.Model.IReason> 
  {
    /// <summary>
    /// ReasonDTO assembler
    /// </summary>
    /// <param name="reason"></param>
    /// <returns></returns>
    public ReasonDTO Assemble(Lemoine.Model.IReason reason) {
      ReasonDTO reasonDTO = new ReasonDTO();
      reasonDTO.Id = reason.Id;
      reasonDTO.Text = reason.Display;
      reasonDTO.Color = reason.Color;
      return reasonDTO;
    }
    
    /// <summary>
    /// ReasonDTO list assembler
    /// </summary>
    /// <param name="reasonList"></param>
    /// <returns></returns>
    public IEnumerable<ReasonDTO> Assemble(IEnumerable<Lemoine.Model.IReason> reasonList) {
      IList<ReasonDTO> reasonDTOList = new List<ReasonDTO>();
      foreach (Lemoine.Model.IReason reason in reasonList) {
        reasonDTOList.Add(Assemble(reason));
      }
      return reasonDTOList;
    }
  }
}
