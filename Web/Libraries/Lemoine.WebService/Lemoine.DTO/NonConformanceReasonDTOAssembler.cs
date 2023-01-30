// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

using Lemoine.Core.Log;
using Lemoine.Model;

namespace Lemoine.DTO
{
  /// <summary>
  /// Assembler for NonConformanceReasonDTO
  /// </summary>
  public class NonConformanceReasonDTOAssembler: IGenericDTOAssembler<NonConformanceReasonDTO, INonConformanceReason>
  {
    /// <summary>
    /// NonConformanceReasonDTO assembler
    /// </summary>
    /// <param name="nonConformanceReason"></param>
    /// <returns></returns>
    public NonConformanceReasonDTO Assemble(Lemoine.Model.INonConformanceReason nonConformanceReason)
    {
      NonConformanceReasonDTO dto = new NonConformanceReasonDTO();
      dto.Id = nonConformanceReason.Id;
      dto.Name = nonConformanceReason.Name;
      dto.Description = nonConformanceReason.Description;
      dto.DetailsRequired = nonConformanceReason.DetailsRequired;
      return dto;
    }
    
    /// <summary>
    /// NonConformanceReasonDTO list assembler
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public IEnumerable<NonConformanceReasonDTO> Assemble(IEnumerable<Lemoine.Model.INonConformanceReason> list)
    {
      IList<NonConformanceReasonDTO> dtoList = new List<NonConformanceReasonDTO>();
      foreach (Lemoine.Model.INonConformanceReason item in list) {
        dtoList.Add(Assemble(item));
      }
      return dtoList;
    }
  }
}
